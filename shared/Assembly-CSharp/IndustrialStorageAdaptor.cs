using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class IndustrialStorageAdaptor : IndustrialEntity, IIndustrialStorage
{
	public struct SortSettings
	{
		public enum SortMode
		{
			Name = 0,
			Count = 1,
			Condition = 2,
			Category = 3,
			Custom = 4,
			LAST = Custom
		}

		public bool enabled;

		public SortMode mode;

		public bool reverse;

		public bool stack;

		public string translateLanguage;

		public List<string> customList;

		public void SaveTo(SortSettings settings)
		{
			settings.enabled = enabled;
			settings.sortMode = (int)mode;
			settings.reverse = reverse;
			settings.stack = stack;
			settings.translateLanguage = translateLanguage;
			if (customList == null)
			{
				return;
			}
			settings.customList = Pool.Get<List<string>>();
			foreach (string custom in customList)
			{
				settings.customList.Add(custom);
				if (settings.customList.Count >= 64)
				{
					break;
				}
			}
		}

		private string GetStringMaxSize(string s, int maxSize)
		{
			if (s == null)
			{
				return string.Empty;
			}
			return s.Substring(0, Mathf.Min(s.Length, maxSize));
		}

		public void LoadFrom(SortSettings settings)
		{
			enabled = settings.enabled;
			mode = (SortMode)settings.sortMode;
			reverse = settings.reverse;
			stack = settings.stack;
			translateLanguage = GetStringMaxSize(settings.translateLanguage, 5);
			if (settings.customList != null)
			{
				if (customList == null)
				{
					customList = new List<string>();
				}
				else
				{
					customList.Clear();
				}
				{
					foreach (string custom in settings.customList)
					{
						customList.Add(GetStringMaxSize(custom, 64));
						if (customList.Count >= 64)
						{
							break;
						}
					}
					return;
				}
			}
			customList = null;
		}
	}

	public class NameComparer : IComparer<Item>
	{
		public string language = string.Empty;

		public int Compare(Item x, Item y)
		{
			if (x == y)
			{
				return 0;
			}
			if (y == null)
			{
				return 1;
			}
			if (x == null)
			{
				return -1;
			}
			return string.Compare(GetStringForItem(x), GetStringForItem(y), StringComparison.InvariantCultureIgnoreCase);
			string GetStringForItem(Item i)
			{
				if (i.IsBlueprint())
				{
					return Translate.GetServerTranslation(i.blueprintTargetDef.displayName.token, language);
				}
				return Translate.GetServerTranslation(i.info.displayName.token, language);
			}
		}
	}

	public class SlotComparer : IComparer<Item>
	{
		public int Compare(Item x, Item y)
		{
			return CompareStatic(x, y);
		}

		public static int CompareStatic(Item x, Item y)
		{
			if (x == y)
			{
				return 0;
			}
			if (y == null)
			{
				return 1;
			}
			return x?.position.CompareTo(y.position) ?? (-1);
		}
	}

	public class CountComparer : IComparer<Item>
	{
		public int Compare(Item x, Item y)
		{
			if (x == y)
			{
				return 0;
			}
			if (y == null)
			{
				return 1;
			}
			if (x == null)
			{
				return -1;
			}
			if (x.amount > y.amount)
			{
				return 1;
			}
			if (x.amount < y.amount)
			{
				return -1;
			}
			return SlotComparer.CompareStatic(x, y);
		}
	}

	public class ConditionComparer : IComparer<Item>
	{
		public int Compare(Item x, Item y)
		{
			if (x == y)
			{
				return 0;
			}
			if (y == null)
			{
				return 1;
			}
			if (x == null)
			{
				return -1;
			}
			if (!x.hasCondition || !y.hasCondition)
			{
				if (x.hasCondition == y.hasCondition)
				{
					return SlotComparer.CompareStatic(x, y);
				}
				if (!x.hasCondition)
				{
					return 1;
				}
				return -1;
			}
			if (x.condition > y.condition)
			{
				return 1;
			}
			if (x.condition < y.condition)
			{
				return -1;
			}
			return SlotComparer.CompareStatic(x, y);
		}
	}

	public class CategoryComparer : IComparer<Item>
	{
		public int Compare(Item x, Item y)
		{
			if (x == y)
			{
				return 0;
			}
			if (y == null)
			{
				return 1;
			}
			if (x == null)
			{
				return -1;
			}
			ItemCategory category = x.info.category;
			ItemCategory category2 = y.info.category;
			if (category > category2)
			{
				return 1;
			}
			if (category < category2)
			{
				return -1;
			}
			return SlotComparer.CompareStatic(x, y);
		}
	}

	public class CustomListComparer : IComparer<Item>
	{
		public List<string> compareList;

		public int Compare(Item x, Item y)
		{
			if (x == y)
			{
				return 0;
			}
			if (y == null)
			{
				return 1;
			}
			if (x == null)
			{
				return -1;
			}
			int num = compareList.IndexOf(x.IsBlueprint() ? ("bp" + x.blueprintTargetDef.shortname) : x.info.shortname);
			int num2 = compareList.IndexOf(y.IsBlueprint() ? ("bp" + y.blueprintTargetDef.shortname) : y.info.shortname);
			if (num == -1 || num2 == -1)
			{
				if (num == num2)
				{
					return 0;
				}
				if (num <= num2)
				{
					return 1;
				}
				return -1;
			}
			if (num > num2)
			{
				return 1;
			}
			if (num < num2)
			{
				return -1;
			}
			return 0;
		}
	}

	public class SortingQueue : ObjectWorkQueue<IndustrialStorageAdaptor>
	{
		protected override void RunJob(IndustrialStorageAdaptor entity)
		{
			entity.ApplySorting();
		}
	}

	public SortSettings currentSortSettings;

	private static NameComparer nameComparer = new NameComparer();

	private static CountComparer countComparer = new CountComparer();

	private static ConditionComparer conditionComparer = new ConditionComparer();

	private static CategoryComparer categoryComparer = new CategoryComparer();

	private static CustomListComparer customListComparer = new CustomListComparer();

	private static SlotComparer slotComparer = new SlotComparer();

	public static SortingQueue SortQueue = new SortingQueue();

	private Action addToQueueAction;

	private bool ignoreRequest;

	public GameObject GreenLight;

	public GameObject RedLight;

	public BaseEntity _cachedParent;

	public ItemContainer cachedContainer;

	private int cachedPassthroughPower;

	public bool sortingEnabled => currentSortSettings.enabled;

	public BaseEntity cachedParent
	{
		get
		{
			if ((Object)(object)_cachedParent == (Object)null)
			{
				_cachedParent = GetParentEntity();
			}
			return _cachedParent;
		}
	}

	public ItemContainer Container
	{
		get
		{
			if (cachedContainer == null)
			{
				cachedContainer = (cachedParent as StorageContainer)?.inventory;
				if (cachedContainer == null)
				{
					cachedContainer = (cachedParent as ContainerIOEntity)?.inventory;
				}
			}
			return cachedContainer;
		}
	}

	public BaseEntity IndustrialEntity => this;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("IndustrialStorageAdaptor.OnRpcMessage"))
		{
			if (rpc == 3920035167u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - UpdatedStorageSettings"));
				}
				using (TimeWarning.New("UpdatedStorageSettings"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3920035167u, "UpdatedStorageSettings", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							UpdatedStorageSettings(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in UpdatedStorageSettings");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private void SaveSorting(SaveInfo info)
	{
		if (sortingEnabled)
		{
			info.msg.storageAdaptor = Pool.Get<StorageAdaptor>();
			SortSettings val = Pool.Get<SortSettings>();
			currentSortSettings.SaveTo(val);
			info.msg.storageAdaptor.sortingSettings = val;
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		return GetCurrentEnergy();
	}

	public void RequestSort()
	{
		using (TimeWarning.New("RequestSort"))
		{
			if (!ignoreRequest && IsPowered())
			{
				if (addToQueueAction == null)
				{
					addToQueueAction = AddToQueue;
				}
				if (IsInvoking(addToQueueAction))
				{
					CancelInvoke(addToQueueAction);
				}
				Invoke(addToQueueAction, 1f);
			}
		}
	}

	private void AddToQueue()
	{
		((ObjectWorkQueue<IndustrialStorageAdaptor>)SortQueue).Add(this);
	}

	private bool ApplySorting()
	{
		ItemContainer container = Container;
		if (!sortingEnabled)
		{
			return false;
		}
		if (!ConVar.Server.allowSorting)
		{
			return false;
		}
		bool flag = (Object)(object)cachedParent != (Object)null;
		bool flag2;
		if (flag)
		{
			BaseEntity baseEntity = cachedParent;
			if (baseEntity is StorageContainer storageContainer)
			{
				if (storageContainer.allowSorting)
				{
					goto IL_0060;
				}
			}
			else if (baseEntity is ContainerIOEntity { allowSorting: not false })
			{
				goto IL_0060;
			}
			flag2 = false;
			goto IL_0068;
		}
		goto IL_006b;
		IL_006b:
		if (!flag)
		{
			return false;
		}
		if (container == null || container.itemList == null || container.itemList.Count == 0)
		{
			return false;
		}
		List<Item> list = Pool.Get<List<Item>>();
		if (currentSortSettings.stack)
		{
			foreach (Item item2 in container.itemList)
			{
				if (item2.amount < item2.info.stackable)
				{
					list.Add(item2);
				}
			}
			for (int i = 0; i < container.itemList.Count; i++)
			{
				Item item = container.itemList[i];
				if (item.amount <= 0 || item.amount >= item.info.stackable)
				{
					continue;
				}
				int num = -1;
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].CanStack(item))
					{
						num = j;
						break;
					}
				}
				if (num != -1)
				{
					int num2 = Mathf.Clamp(item.amount, 0, list[num].info.stackable - list[num].amount);
					list[num].amount += num2;
					item.UseItem(num2);
				}
			}
			list.Clear();
		}
		foreach (Item item3 in container.itemList)
		{
			list.Add(item3);
		}
		switch (currentSortSettings.mode)
		{
		case SortSettings.SortMode.Name:
			using (TimeWarning.New("NameComparer"))
			{
				nameComparer.language = currentSortSettings.translateLanguage;
				list.Sort(nameComparer);
			}
			break;
		case SortSettings.SortMode.Count:
			using (TimeWarning.New("CountComparer"))
			{
				list.Sort(countComparer);
			}
			break;
		case SortSettings.SortMode.Condition:
			using (TimeWarning.New("ConditionComparer"))
			{
				list.Sort(conditionComparer);
			}
			break;
		case SortSettings.SortMode.Category:
			using (TimeWarning.New("CategoryComparer"))
			{
				list.Sort(categoryComparer);
			}
			break;
		case SortSettings.SortMode.Custom:
			using (TimeWarning.New("CustomListComparer"))
			{
				list.Sort(slotComparer);
				customListComparer.compareList = currentSortSettings.customList;
				list.Sort(customListComparer);
				customListComparer.compareList = null;
			}
			break;
		}
		if (currentSortSettings.reverse)
		{
			using (TimeWarning.New("Reverse"))
			{
				list.Reverse();
			}
		}
		bool result = false;
		for (int k = 0; k < list.Count; k++)
		{
			if (list[k].position != k)
			{
				list[k].position = k;
				result = true;
			}
		}
		Pool.FreeUnmanaged<Item>(ref list);
		try
		{
			ignoreRequest = true;
			container.MarkDirty();
			return result;
		}
		finally
		{
			ignoreRequest = false;
		}
		IL_0060:
		flag2 = true;
		goto IL_0068;
		IL_0068:
		flag = flag2;
		goto IL_006b;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void UpdatedStorageSettings(RPCMessage msg)
	{
		SortSettings val = msg.read.Proto<SortSettings>((SortSettings)null);
		try
		{
			if (val == null)
			{
				currentSortSettings = default(SortSettings);
			}
			else
			{
				currentSortSettings.LoadFrom(val);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		Container.MarkDirty();
		SendNetworkUpdate();
	}

	private void LoadSorting(LoadInfo info)
	{
		if (info.msg.storageAdaptor != null && info.msg.storageAdaptor.sortingSettings != null)
		{
			if (currentSortSettings.enabled)
			{
				currentSortSettings = default(SortSettings);
			}
			currentSortSettings.LoadFrom(info.msg.storageAdaptor.sortingSettings);
		}
		else
		{
			currentSortSettings = default(SortSettings);
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		_cachedParent = null;
		cachedContainer = null;
	}

	public Vector2i InputSlotRange(int slotIndex)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)cachedParent != (Object)null)
		{
			if (cachedParent is IIndustrialStorage industrialStorage)
			{
				return industrialStorage.InputSlotRange(slotIndex);
			}
			if (cachedParent is Locker locker)
			{
				Vector3 localPosition = ((Component)this).transform.localPosition;
				return locker.GetIndustrialSlotRange(localPosition);
			}
		}
		if (Container != null)
		{
			return new Vector2i(0, Container.capacity - 1);
		}
		return new Vector2i(0, 0);
	}

	public Vector2i OutputSlotRange(int slotIndex)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)cachedParent != (Object)null)
		{
			if (cachedParent is DropBox && Container != null)
			{
				return new Vector2i(0, Container.capacity - 1);
			}
			if (cachedParent is IIndustrialStorage industrialStorage)
			{
				return industrialStorage.OutputSlotRange(slotIndex);
			}
			if (cachedParent is Locker locker)
			{
				Vector3 localPosition = ((Component)this).transform.localPosition;
				return locker.GetIndustrialSlotRange(localPosition);
			}
		}
		if (Container != null)
		{
			return new Vector2i(0, Container.capacity - 1);
		}
		return new Vector2i(0, 0);
	}

	public void OnStorageItemTransferBegin()
	{
		if ((Object)(object)cachedParent != (Object)null && cachedParent is IIndustrialStorageCallbackReceiver industrialStorageCallbackReceiver)
		{
			using (TimeWarning.New("IIndustrialStorageCallbackReceiver::OnIndustrialItemTransferBegins"))
			{
				industrialStorageCallbackReceiver.OnIndustrialItemTransferBegins();
			}
		}
	}

	public void OnStorageItemTransferEnd()
	{
		if ((Object)(object)cachedParent != (Object)null && cachedParent is IIndustrialStorageCallbackReceiver industrialStorageCallbackReceiver)
		{
			industrialStorageCallbackReceiver.OnIndustrialItemTransferEnd();
		}
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		if ((Object)(object)newParent != (Object)null)
		{
			cachedContainer = null;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		SaveSorting(info);
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		if (inputSlot == 1)
		{
			cachedPassthroughPower = inputAmount;
			UpdateHasPower(inputAmount, inputSlot);
			MarkDirtyForceUpdateOutputs();
		}
	}

	public override int GetCurrentEnergy()
	{
		return Mathf.Clamp(cachedPassthroughPower - ConsumptionAmount(), 0, cachedPassthroughPower);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		LoadSorting(info);
	}

	public override int ConsumptionAmount()
	{
		return 1;
	}

	public void ClientNotifyItemAddRemoved(bool add)
	{
		if (add)
		{
			GreenLight.SetActive(false);
			GreenLight.SetActive(true);
		}
		else
		{
			RedLight.SetActive(false);
			RedLight.SetActive(true);
		}
	}
}
