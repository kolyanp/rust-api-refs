using System;
using System.Collections.Generic;
using ConVar;
using Development.Attributes;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Item : IPooled
{
	[Flags]
	public enum Flag
	{
		None = 0,
		Placeholder = 1,
		IsOn = 2,
		OnFire = 4,
		IsLocked = 8,
		Cooking = 0x10,
		Radioactive = 0x20,
		Refrigerated = 0x40
	}

	private const string DefaultArmourBreakEffectPath = "assets/bundled/prefabs/fx/armor_break.prefab";

	public float _condition;

	public float _maxCondition = 100f;

	public ItemDefinition info;

	public ItemId uid;

	public bool dirty;

	public int amount = 1;

	public int position;

	public float removeTime;

	public float fuel;

	public bool isServer;

	public InstanceData instanceData;

	public ulong skin;

	public ulong attachment;

	public string name;

	public string streamerName;

	public string text;

	public float cookTimeLeft;

	public float radioactivity;

	public List<ItemOwnershipShare> ownershipShares;

	public uint iconImageId;

	public Flag flags;

	public ItemContainer contents;

	public ItemContainer parent;

	private EntityRef worldEnt;

	private EntityRef heldEntity;

	public float condition
	{
		get
		{
			return _condition;
		}
		set
		{
			float num = _condition;
			_condition = Mathf.Clamp(value, 0f, maxCondition);
			if (isServer && Mathf.Ceil(value) != Mathf.Ceil(num))
			{
				MarkDirty();
			}
		}
	}

	public float maxCondition
	{
		get
		{
			return _maxCondition;
		}
		set
		{
			_maxCondition = Mathf.Clamp(value, 0f, info.condition.max);
			if (isServer)
			{
				MarkDirty();
			}
		}
	}

	public float maxConditionNormalized => _maxCondition / info.condition.max;

	public float conditionNormalized
	{
		get
		{
			if (!hasCondition)
			{
				return 1f;
			}
			return condition / maxCondition;
		}
		set
		{
			if (hasCondition)
			{
				condition = value * maxCondition;
			}
		}
	}

	public bool hasCondition
	{
		get
		{
			if ((Object)(object)info != (Object)null && info.condition.enabled)
			{
				return info.condition.max > 0f;
			}
			return false;
		}
	}

	public bool isBroken
	{
		get
		{
			if (hasCondition)
			{
				return condition <= 0f;
			}
			return false;
		}
	}

	public int despawnMultiplier
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Expected I4, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			Rarity val = info.despawnRarity;
			if ((Object)(object)info.isRedirectOf != (Object)null)
			{
				val = info.isRedirectOf.despawnRarity;
			}
			if ((int)val == 0)
			{
				val = info.rarity;
				if ((Object)(object)info.isRedirectOf != (Object)null)
				{
					val = info.isRedirectOf.rarity;
				}
			}
			if (!((Object)(object)info != (Object)null))
			{
				return 1;
			}
			return Mathf.Clamp((val - 1) * 4, 1, 100);
		}
	}

	public ItemDefinition blueprintTargetDef
	{
		get
		{
			if (!IsBlueprint())
			{
				return null;
			}
			return ItemManager.FindItemDefinition(blueprintTarget);
		}
	}

	public int blueprintTarget
	{
		get
		{
			if (instanceData == null)
			{
				return 0;
			}
			return instanceData.blueprintTarget;
		}
		set
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			if (instanceData == null)
			{
				instanceData = new InstanceData();
			}
			instanceData.ShouldPool = false;
			instanceData.blueprintTarget = value;
		}
	}

	public int blueprintAmount
	{
		get
		{
			return amount;
		}
		set
		{
			amount = value;
		}
	}

	public Item parentItem
	{
		get
		{
			if (parent == null)
			{
				return null;
			}
			return parent.parent;
		}
	}

	public float temperature
	{
		get
		{
			if (parent != null)
			{
				return parent.GetTemperature(position);
			}
			return 15f;
		}
	}

	public bool HasOnCycle => onCycle != null;

	public BaseEntity.TraitFlag Traits => info.Traits;

	public event Action<Item> OnDirty;

	public event Action<Item, float> onCycle;

	public void LoseCondition(float amount)
	{
		if (hasCondition && !Debugging.disablecondition && Interface.CallHook("IOnLoseCondition", this, amount) == null)
		{
			float num = condition;
			condition -= amount;
			if (Global.developer > 0)
			{
				Debug.Log((object)(info.shortname + " was damaged by: " + amount + "cond is: " + condition + "/" + maxCondition));
			}
			if (condition <= 0f && condition < num)
			{
				OnBroken();
			}
		}
	}

	public void RepairCondition(float amount)
	{
		if (hasCondition)
		{
			condition += amount;
		}
	}

	public void DoRepair(float maxLossFraction)
	{
		if (!hasCondition)
		{
			return;
		}
		if (info.condition.maintainMaxCondition)
		{
			maxLossFraction = 0f;
		}
		float num = 1f - condition / maxCondition;
		maxLossFraction = Mathf.Clamp(maxLossFraction, 0f, info.condition.max);
		maxCondition *= 1f - maxLossFraction * num;
		condition = maxCondition;
		BaseEntity baseEntity = GetHeldEntity();
		if ((Object)(object)baseEntity != (Object)null)
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Broken, b: false);
		}
		if (Global.developer > 0)
		{
			Debug.Log((object)(info.shortname + " was repaired! new cond is: " + condition + "/" + maxCondition));
		}
	}

	public ItemContainer GetRootContainer()
	{
		ItemContainer itemContainer = parent;
		int num = 0;
		while (itemContainer != null && num <= 8 && itemContainer.parent != null && itemContainer.parent.parent != null)
		{
			itemContainer = itemContainer.parent.parent;
			num++;
		}
		if (num == 8)
		{
			Debug.LogWarning((object)"GetRootContainer failed with 8 iterations");
		}
		return itemContainer;
	}

	public virtual void OnBroken()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		if (!hasCondition)
		{
			return;
		}
		BaseEntity baseEntity = GetHeldEntity();
		if ((Object)(object)baseEntity != (Object)null)
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Broken, b: true);
		}
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (Object.op_Implicit((Object)(object)ownerPlayer))
		{
			if (ownerPlayer.GetActiveItem() == this)
			{
				Effect.server.Run("assets/bundled/prefabs/fx/item_break.prefab", ownerPlayer, 0u, Vector3.zero, Vector3.zero);
				ownerPlayer.ShowToast(GameTip.Styles.Error, PlayerInventoryErrors.ActiveItemBroken, false);
			}
			ItemModWearable itemModWearable = default(ItemModWearable);
			if (((Component)info).TryGetComponent<ItemModWearable>(ref itemModWearable) && ownerPlayer.inventory.containerWear.itemList.Contains(this))
			{
				if (itemModWearable.breakEffect.isValid)
				{
					Effect.server.Run(itemModWearable.breakEffect.resourcePath, ownerPlayer, 0u, Vector3.zero, Vector3.zero);
				}
				else
				{
					Effect.server.Run("assets/bundled/prefabs/fx/armor_break.prefab", ownerPlayer, 0u, Vector3.zero, Vector3.zero);
				}
			}
		}
		if (info.condition.breakEffect != null && info.condition.breakEffect.isValid)
		{
			BasePlayer playerOwner = GetRootContainer().playerOwner;
			if ((Object)(object)playerOwner != (Object)null)
			{
				Effect.server.Run(info.condition.breakEffect.resourcePath, playerOwner, 0u, Vector3.zero, Vector3.zero);
			}
		}
		if ((!info.condition.repairable && !Object.op_Implicit((Object)(object)((Component)info).GetComponent<ItemModRepair>())) || maxCondition <= 5f)
		{
			UnloadAmmo();
			Remove();
		}
		else if (parent != null && parent.HasFlag(ItemContainer.Flag.NoBrokenItems))
		{
			ItemContainer rootContainer = GetRootContainer();
			if (rootContainer.HasFlag(ItemContainer.Flag.NoBrokenItems))
			{
				Remove();
			}
			else
			{
				BasePlayer playerOwner2 = rootContainer.playerOwner;
				if ((Object)(object)playerOwner2 != (Object)null && !MoveToContainer(playerOwner2.inventory.containerMain))
				{
					Drop(((Component)playerOwner2).transform.position, playerOwner2.eyes.BodyForward() * 1.5f);
				}
			}
		}
		ItemModEntity itemModEntity = default(ItemModEntity);
		if (((Component)info).TryGetComponent<ItemModEntity>(ref itemModEntity) && itemModEntity.destroyEntityWhenBroken)
		{
			baseEntity.Kill(BaseNetworkable.DestroyMode.Gib);
		}
		MarkDirty();
	}

	void IPooled.EnterPool()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		info = null;
		uid = default(ItemId);
		dirty = false;
		amount = 1;
		position = 0;
		removeTime = 0f;
		fuel = 0f;
		isServer = false;
		instanceData = null;
		skin = 0uL;
		name = null;
		streamerName = null;
		text = null;
		cookTimeLeft = 0f;
		radioactivity = 0f;
		flags = Flag.None;
		if (contents != null)
		{
			Pool.Free<ItemContainer>(ref contents);
		}
		parent = null;
		heldEntity = default(EntityRef);
		worldEnt = default(EntityRef);
		onCycle = null;
		OnDirty = null;
		_condition = 0f;
		_maxCondition = 100f;
		if (ownershipShares != null)
		{
			Pool.FreeUnmanaged<ItemOwnershipShare>(ref ownershipShares);
		}
		ownershipShares = null;
		iconImageId = 0u;
	}

	void IPooled.LeavePool()
	{
	}

	public string GetName(bool? streamerModeOverride = null)
	{
		if (streamerModeOverride.HasValue)
		{
			if (!streamerModeOverride.Value)
			{
				return name;
			}
			return streamerName ?? name;
		}
		return name;
	}

	public bool IsBlueprint()
	{
		return blueprintTarget != 0;
	}

	public bool HasFlag(Flag f)
	{
		return (flags & f) == f;
	}

	public void SetFlag(Flag f, bool b)
	{
		if (b)
		{
			flags |= f;
		}
		else
		{
			flags &= ~f;
		}
	}

	public bool IsOn()
	{
		return HasFlag(Flag.IsOn);
	}

	public bool IsOnFire()
	{
		return HasFlag(Flag.OnFire);
	}

	public bool IsCooking()
	{
		return HasFlag(Flag.Cooking);
	}

	public bool IsLocked()
	{
		if (!HasFlag(Flag.IsLocked))
		{
			if (parent != null)
			{
				return parent.IsLocked();
			}
			return false;
		}
		return true;
	}

	public bool IsRadioactive()
	{
		return HasFlag(Flag.Radioactive);
	}

	public bool IsRefrigerated()
	{
		return HasFlag(Flag.Refrigerated);
	}

	public void MarkDirty()
	{
		OnChanged();
		dirty = true;
		if (parent != null)
		{
			parent.MarkDirty();
		}
		if (OnDirty != null)
		{
			OnDirty(this);
		}
	}

	public void OnChanged()
	{
		ItemMod[] itemMods = info.itemMods;
		for (int i = 0; i < itemMods.Length; i++)
		{
			itemMods[i].OnChanged(this);
		}
		if (contents != null)
		{
			contents.OnChanged();
		}
	}

	public void CollectedForCrafting(BasePlayer crafter)
	{
		ItemMod[] itemMods = info.itemMods;
		for (int i = 0; i < itemMods.Length; i++)
		{
			itemMods[i].CollectedForCrafting(this, crafter);
		}
	}

	public void ReturnedFromCancelledCraft(BasePlayer crafter)
	{
		ItemMod[] itemMods = info.itemMods;
		for (int i = 0; i < itemMods.Length; i++)
		{
			itemMods[i].ReturnedFromCancelledCraft(this, crafter);
		}
	}

	public void Initialize(ItemDefinition template)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		uid = new ItemId(Net.sv.TakeUID());
		float num = (maxCondition = info.condition.max);
		condition = num;
		SetRadioactivity(template);
		InitializeItemOwnership();
		OnItemCreated();
	}

	public void OnItemCreated()
	{
		onCycle = null;
		ItemMod[] itemMods = info.itemMods;
		for (int i = 0; i < itemMods.Length; i++)
		{
			itemMods[i].OnItemCreated(this);
		}
	}

	public void OnVirginSpawn(BasePlayer creatingPlayer = null)
	{
		ItemMod[] itemMods = info.itemMods;
		for (int i = 0; i < itemMods.Length; i++)
		{
			itemMods[i].OnVirginItem(this, creatingPlayer);
		}
	}

	public float GetDespawnDuration()
	{
		if (info.quickDespawn)
		{
			return ConVar.Server.itemdespawn_quick;
		}
		int num = 0;
		if (contents != null && contents.itemList != null)
		{
			foreach (Item item in contents.itemList)
			{
				num += item.despawnMultiplier;
			}
		}
		return (float)Mathf.Min(Mathf.Max(despawnMultiplier, num), ConVar.Server.itemdespawn_container_max_multiplier) * ConVar.Server.itemdespawn;
	}

	public void RemoveFromWorld()
	{
		BaseEntity worldEntity = GetWorldEntity();
		if ((Object)(object)worldEntity == (Object)null)
		{
			return;
		}
		SetWorldEntity(null);
		OnRemovedFromWorld();
		if (contents != null)
		{
			contents.OnRemovedFromWorld();
		}
		if (worldEntity.IsValid())
		{
			if (worldEntity is WorldItem worldItem)
			{
				worldItem.RemoveItem();
			}
			worldEntity.Kill();
		}
	}

	public void OnRemovedFromWorld()
	{
		ItemMod[] itemMods = info.itemMods;
		for (int i = 0; i < itemMods.Length; i++)
		{
			itemMods[i].OnRemovedFromWorld(this);
		}
	}

	public void RemoveFromContainer()
	{
		if (parent != null)
		{
			SetParent(null);
		}
	}

	public bool DoItemSlotsConflict(Item other)
	{
		return (info.occupySlots & other.info.occupySlots) != 0;
	}

	public void AddItemOwnership(string username, string reason, int shareAmount)
	{
		if (ownershipShares == null)
		{
			return;
		}
		if (ownershipShares.Count > 0)
		{
			for (int i = 0; i < ownershipShares.Count; i++)
			{
				ItemOwnershipShare value = ownershipShares[i];
				if (value.username == username && value.reason == reason)
				{
					value.amount += shareAmount;
					ownershipShares[i] = value;
					shareAmount = 0;
					break;
				}
			}
		}
		if (shareAmount > 0)
		{
			ItemOwnershipShare item = new ItemOwnershipShare
			{
				username = username,
				reason = reason,
				amount = shareAmount
			};
			ownershipShares.Add(item);
		}
		MarkDirty();
	}

	public void MigrateItemOwnership(Item targetItem, int amount)
	{
		if (ownershipShares != null && targetItem.ownershipShares != null)
		{
			TransferOwnership(targetItem, amount);
		}
	}

	public ItemOwnershipShare TakeOwnershipShare()
	{
		if (!HasItemOwnership())
		{
			return default(ItemOwnershipShare);
		}
		ItemOwnershipShare itemOwnershipShare = ownershipShares[0];
		itemOwnershipShare.amount--;
		ownershipShares[0] = itemOwnershipShare;
		if (itemOwnershipShare.amount <= 0)
		{
			ownershipShares.RemoveAt(0);
		}
		return itemOwnershipShare;
	}

	public void ReduceItemOwnership(int amount)
	{
		if (ownershipShares != null)
		{
			TransferOwnership(null, amount);
		}
	}

	private void TransferOwnership(Item targetItem, int amount)
	{
		for (int num = ownershipShares.Count - 1; num >= 0; num--)
		{
			if (amount <= 0)
			{
				return;
			}
			ItemOwnershipShare value = ownershipShares[num];
			int num2 = Mathf.Min(value.amount, amount);
			targetItem?.AddItemOwnership(value.username, value.reason, num2);
			amount -= num2;
			value.amount -= num2;
			if (value.amount == 0)
			{
				ownershipShares.RemoveAt(num);
			}
			else
			{
				ownershipShares[num] = value;
			}
		}
		MarkDirty();
	}

	public bool HasItemOwnership()
	{
		if (ownershipShares != null)
		{
			return ownershipShares.Count > 0;
		}
		return false;
	}

	public void AddItemOwnership(BasePlayer player, Phrase reason)
	{
		AddItemOwnership(((Object)(object)player != (Object)null && !player.IsDestroyed) ? player.displayName : "", reason.token, amount);
	}

	public Item SetItemOwnership(BasePlayer player, Phrase reason)
	{
		return SetItemOwnership(((Object)(object)player != (Object)null && !player.IsDestroyed) ? player.displayName : "", reason.token);
	}

	public Item SetItemOwnership(ItemOwnershipShare ownership)
	{
		return SetItemOwnership(ownership.username, ownership.reason);
	}

	public Item SetItemOwnership(BasePlayer player, string reason)
	{
		return SetItemOwnership(((Object)(object)player != (Object)null && !player.IsDestroyed) ? player.displayName : "", reason);
	}

	public Item SetItemOwnership(string username, Phrase reason)
	{
		return SetItemOwnership(username, reason?.token ?? "");
	}

	public Item SetItemOwnership(string username, string reason)
	{
		if (ownershipShares == null)
		{
			return this;
		}
		ownershipShares?.Clear();
		AddItemOwnership(username, reason, amount);
		return this;
	}

	public void InitializeItemOwnership()
	{
		if (!((Object)(object)info == (Object)null) && info.SupportsItemOwnership() && ownershipShares == null)
		{
			ownershipShares = Pool.Get<List<ItemOwnershipShare>>();
		}
	}

	public void SetParent(ItemContainer target)
	{
		if (target == parent)
		{
			return;
		}
		if (parent != null)
		{
			parent.Remove(this);
			parent = null;
		}
		if (target == null)
		{
			position = 0;
		}
		else
		{
			parent = target;
			if (!parent.Insert(this))
			{
				Remove();
				Debug.LogError((object)"Item.SetParent caused remove - this shouldn't ever happen");
			}
		}
		MarkDirty();
		RecalulateParentEntity(children: false);
		if (parent != null)
		{
			ItemContainer itemContainer = parent;
			bool flag = false;
			do
			{
				BaseEntity entityOwner = itemContainer.entityOwner;
				if ((Object)(object)entityOwner != (Object)null && !entityOwner.enableSaving)
				{
					flag = true;
					break;
				}
				entityOwner = itemContainer.playerOwner;
				if ((Object)(object)entityOwner != (Object)null && !entityOwner.enableSaving)
				{
					flag = true;
					break;
				}
				itemContainer = itemContainer?.parent?.parent;
			}
			while (itemContainer != null);
			if (flag)
			{
				ForbidHeldEntitySaving();
			}
			else
			{
				RestoreHeldEntitySaving();
			}
		}
		else
		{
			RestoreHeldEntitySaving();
		}
	}

	public void RecalulateParentEntity(bool children)
	{
		ItemMod[] itemMods = info.itemMods;
		for (int i = 0; i < itemMods.Length; i++)
		{
			itemMods[i].OnParentChanged(this);
		}
		if (!children || contents == null)
		{
			return;
		}
		foreach (Item item in contents.itemList)
		{
			item.RecalulateParentEntity(children: false);
		}
	}

	private void ForbidHeldEntitySaving()
	{
		if (heldEntity.IsValid(serverside: true))
		{
			heldEntity.Get(serverside: true).EnableSaving(wants: false);
		}
		if (contents == null)
		{
			return;
		}
		foreach (Item item in contents.itemList)
		{
			item.ForbidHeldEntitySaving();
		}
	}

	private void RestoreHeldEntitySaving()
	{
		if (heldEntity.IsValid(serverside: true))
		{
			heldEntity.Get(serverside: true).RestoreCanSave();
		}
		if (contents == null)
		{
			return;
		}
		foreach (Item item in contents.itemList)
		{
			item.RestoreHeldEntitySaving();
		}
	}

	public void OnAttacked(HitInfo hitInfo)
	{
		ItemMod[] itemMods = info.itemMods;
		for (int i = 0; i < itemMods.Length; i++)
		{
			itemMods[i].OnAttacked(this, hitInfo);
		}
	}

	public BaseEntity GetEntityOwner()
	{
		return parent?.GetEntityOwner();
	}

	public bool IsChildContainer(ItemContainer c)
	{
		if (contents == null)
		{
			return false;
		}
		if (contents == c)
		{
			return true;
		}
		foreach (Item item in contents.itemList)
		{
			if (item.IsChildContainer(c))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanMoveTo(BasePlayer player, ItemContainer newcontainer, int iTargetPos = -1)
	{
		if (IsChildContainer(newcontainer))
		{
			return false;
		}
		if (newcontainer.CanAcceptItem(player, this, iTargetPos) != ItemContainer.CanAcceptResult.CanAccept)
		{
			return false;
		}
		if (iTargetPos >= newcontainer.capacity)
		{
			return false;
		}
		if (parent != null && newcontainer == parent && iTargetPos == position)
		{
			return false;
		}
		return true;
	}

	public bool MoveToContainer(ItemContainer newcontainer, int iTargetPos = -1, bool allowStack = true, bool ignoreStackLimit = false, BasePlayer sourcePlayer = null, bool allowSwap = true)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_058a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0590: Unknown result type (might be due to invalid IL or missing references)
		//IL_0597: Unknown result type (might be due to invalid IL or missing references)
		//IL_059d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("MoveToContainer"))
		{
			BasePlayer playerOwner = newcontainer.playerOwner;
			if ((Object)(object)playerOwner != (Object)null && playerOwner.IsDead() && parent != null)
			{
				return false;
			}
			bool flag = iTargetPos == -1;
			ItemContainer itemContainer = parent;
			if (iTargetPos == -1)
			{
				if (allowStack && info.stackable > 1)
				{
					BufferList<Item> val = Pool.Get<BufferList<Item>>();
					newcontainer.FindItemsByItemID(info.itemid, val);
					Enumerator<Item> enumerator = val.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							Item current = enumerator.Current;
							if (current.position > iTargetPos && current.CanStack(this) && (ignoreStackLimit || current.amount < current.MaxStackable()))
							{
								iTargetPos = current.position;
							}
						}
					}
					finally
					{
						((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
					}
					Pool.Free<Item>(ref val, false);
				}
				if (iTargetPos == -1 && newcontainer.GetEntityOwner(returnHeldEntity: true) is IIdealSlotEntity idealSlotEntity)
				{
					iTargetPos = idealSlotEntity.GetIdealSlot(sourcePlayer, newcontainer, this);
					if (iTargetPos == int.MinValue)
					{
						return false;
					}
				}
				if (iTargetPos == -1)
				{
					if (newcontainer == parent)
					{
						return false;
					}
					bool flag2 = newcontainer.HasFlag(ItemContainer.Flag.Clothing) && info.isWearable;
					ItemModWearable itemModWearable = info.ItemModWearable;
					for (int i = 0; i < newcontainer.capacity; i++)
					{
						Item slot = newcontainer.GetSlot(i);
						if (slot == null)
						{
							if (CanMoveTo(sourcePlayer, newcontainer, i))
							{
								iTargetPos = i;
								break;
							}
							continue;
						}
						if (flag2 && slot != null && !slot.info.ItemModWearable.CanExistWith(itemModWearable))
						{
							iTargetPos = i;
							break;
						}
						if (newcontainer.HasAvailableSlotsDefined && DoItemSlotsConflict(slot))
						{
							iTargetPos = i;
							break;
						}
					}
					if (flag2 && iTargetPos == -1)
					{
						iTargetPos = newcontainer.capacity - 1;
					}
				}
			}
			if (iTargetPos == -1)
			{
				return false;
			}
			if (!CanMoveTo(sourcePlayer, newcontainer, iTargetPos))
			{
				return false;
			}
			if (iTargetPos >= 0 && newcontainer.SlotTaken(this, iTargetPos))
			{
				Item slot2 = newcontainer.GetSlot(iTargetPos);
				if (slot2 == this)
				{
					return false;
				}
				if (allowStack && slot2 != null)
				{
					int num = slot2.MaxStackable();
					if (slot2.CanStack(this))
					{
						if (ignoreStackLimit)
						{
							num = int.MaxValue;
						}
						if (slot2.amount >= num)
						{
							return false;
						}
						int num2 = Mathf.Min(num - slot2.amount, amount);
						slot2.amount += num2;
						if (slot2.instanceData != null && instanceData != null && Object.op_Implicit((Object)(object)((Component)info).GetComponent<ItemModFoodSpoiling>()))
						{
							slot2.instanceData.dataFloat = Mathf.Min(slot2.instanceData.dataFloat, instanceData.dataFloat);
						}
						newcontainer.onItemAddedToStack?.Invoke(slot2, num2);
						amount -= num2;
						slot2.MarkDirty();
						Interface.CallHook("OnItemStacked", slot2, this, newcontainer, num2);
						MarkDirty();
						MigrateItemOwnership(slot2, num2);
						if (amount <= 0)
						{
							RemoveFromWorld();
							RemoveFromContainer();
							Remove();
							return true;
						}
						if (flag)
						{
							return MoveToContainer(newcontainer, -1, allowStack, ignoreStackLimit, sourcePlayer);
						}
						return false;
					}
				}
				if (((parent != null) & allowSwap) && slot2 != null)
				{
					ItemContainer itemContainer2 = parent;
					int iTargetPos2 = position;
					ItemContainer newcontainer2 = slot2.parent;
					int num3 = slot2.position;
					if (!slot2.CanMoveTo(sourcePlayer, itemContainer2, iTargetPos2))
					{
						return false;
					}
					if (itemContainer2.maxStackSize > 0 && slot2.amount > itemContainer2.maxStackSize)
					{
						Item item = slot2.SplitItem(slot2.amount - itemContainer2.maxStackSize);
						if (item == null || !item.MoveToContainer(newcontainer2, -1, allowStack: false, ignoreStackLimit: false, sourcePlayer, allowSwap: false))
						{
							slot2.amount += item.amount;
							item.MigrateItemOwnership(slot2, item.amount);
							item.Remove();
							return false;
						}
					}
					BaseEntity entityOwner = GetEntityOwner();
					BaseEntity entityOwner2 = slot2.GetEntityOwner();
					RemoveFromContainer();
					slot2.RemoveFromContainer();
					RemoveConflictingSlots(newcontainer, entityOwner, sourcePlayer);
					slot2.RemoveConflictingSlots(itemContainer2, entityOwner2, sourcePlayer);
					if (!slot2.MoveToContainer(itemContainer2, iTargetPos2, allowStack: true, ignoreStackLimit: false, sourcePlayer) || !MoveToContainer(newcontainer, iTargetPos, allowStack: true, ignoreStackLimit: false, sourcePlayer))
					{
						RemoveFromContainer();
						slot2.RemoveFromContainer();
						SetParent(itemContainer2);
						position = iTargetPos2;
						slot2.SetParent(newcontainer2);
						slot2.position = num3;
						return true;
					}
					return true;
				}
				return false;
			}
			if (parent == newcontainer)
			{
				if (iTargetPos >= 0 && iTargetPos != position && !parent.SlotTaken(this, iTargetPos))
				{
					newcontainer.onItemPositionChanged?.Invoke(this, position, iTargetPos);
					position = iTargetPos;
					MarkDirty();
					return true;
				}
				return false;
			}
			if (newcontainer.maxStackSize > 0 && newcontainer.maxStackSize < amount)
			{
				Item item2 = SplitItem(newcontainer.maxStackSize);
				if (item2 != null && !item2.MoveToContainer(newcontainer, iTargetPos, allowStack: false, ignoreStackLimit: false, sourcePlayer) && (itemContainer == null || !item2.MoveToContainer(itemContainer, -1, allowStack: true, ignoreStackLimit: false, sourcePlayer)))
				{
					DroppedItem droppedItem = item2.Drop(newcontainer.dropPosition, newcontainer.dropVelocity) as DroppedItem;
					if ((Object)(object)droppedItem != (Object)null)
					{
						droppedItem.DroppedBy = sourcePlayer?.userID ?? ((EncryptedValue<ulong>)0uL);
					}
				}
				Interface.CallHook("OnItemStacked", item2, this, newcontainer);
				return true;
			}
			if (!newcontainer.CanAccept(this))
			{
				return false;
			}
			BaseEntity entityOwner3 = GetEntityOwner();
			RemoveFromContainer();
			RemoveFromWorld();
			RemoveConflictingSlots(newcontainer, entityOwner3, sourcePlayer);
			position = iTargetPos;
			SetParent(newcontainer);
			return true;
		}
	}

	private void RemoveConflictingSlots(ItemContainer container, BaseEntity entityOwner, BasePlayer sourcePlayer)
	{
		if (!isServer || !container.HasAvailableSlotsDefined)
		{
			return;
		}
		List<Item> list = Pool.Get<List<Item>>();
		list.AddRange(container.itemList);
		foreach (Item item in list)
		{
			if (item.DoItemSlotsConflict(this))
			{
				item.RemoveFromContainer();
				if (entityOwner is BasePlayer basePlayer)
				{
					basePlayer.GiveItem(item);
				}
				else if (entityOwner is IItemContainerEntity itemContainerEntity)
				{
					item.MoveToContainer(itemContainerEntity.inventory, -1, allowStack: true, ignoreStackLimit: false, sourcePlayer);
				}
			}
		}
		Pool.Free<Item>(ref list, false);
	}

	public BaseEntity CreateWorldObject(Vector3 pos, Quaternion rotation = default(Quaternion), BaseEntity parentEnt = null, uint parentBone = 0u)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity worldEntity = GetWorldEntity();
		if ((Object)(object)worldEntity != (Object)null)
		{
			return worldEntity;
		}
		worldEntity = GameManager.server.CreateEntity("assets/prefabs/misc/burlap sack/generic_world.prefab", pos, rotation);
		if ((Object)(object)worldEntity == (Object)null)
		{
			Debug.LogWarning((object)"Couldn't create world object for prefab: items/generic_world");
			return null;
		}
		WorldItem worldItem = worldEntity as WorldItem;
		if ((Object)(object)worldItem != (Object)null)
		{
			worldItem.InitializeItem(this);
		}
		if ((Object)(object)parentEnt != (Object)null)
		{
			worldEntity.SetParent(parentEnt, parentBone);
		}
		worldEntity.Spawn();
		SetWorldEntity(worldEntity);
		return GetWorldEntity();
	}

	public BaseEntity Drop(Vector3 vPos, Vector3 vVelocity, Quaternion rotation = default(Quaternion))
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		ulong droppedBy = GetRootContainer()?.playerOwner?.userID ?? ((EncryptedValue<ulong>)0uL);
		RemoveFromWorld();
		if (info.AlignWorldModelOnDrop)
		{
			Quaternion val = Quaternion.LookRotation(((Vector3)(ref vVelocity)).normalized, Vector3.up);
			rotation = Quaternion.Euler(0f, ((Quaternion)(ref val)).eulerAngles.y, 0f);
			rotation = Quaternion.Euler(info.WorldModelDropOffset) * rotation;
		}
		BaseEntity baseEntity = null;
		if (vPos != Vector3.zero && !info.HasFlag(ItemDefinition.Flag.NoDropping))
		{
			baseEntity = CreateWorldObject(vPos, rotation);
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				baseEntity.SetVelocity(vVelocity);
			}
			if (baseEntity is DroppedItem droppedItem)
			{
				droppedItem.DroppedBy = droppedBy;
				if (info.AdjustCenterOfMassOnDrop)
				{
					droppedItem.Rigidbody.centerOfMass = info.DropCenterOfMass;
				}
			}
		}
		else
		{
			Remove();
		}
		Interface.CallHook("OnItemDropped", this, baseEntity);
		RemoveFromContainer();
		return baseEntity;
	}

	public BaseEntity DropAndTossUpwards(Vector3 vPos, float force = 2f)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.value * MathF.PI * 2f;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(Mathf.Sin(num), 1f, Mathf.Cos(num));
		return Drop(vPos + Vector3.up * 0.1f, val * force);
	}

	public bool IsRemoved()
	{
		return removeTime > 0f;
	}

	public void Remove(float fTime = 0f)
	{
		if (removeTime > 0f || Interface.CallHook("OnItemRemove", this) != null)
		{
			return;
		}
		if (isServer)
		{
			ItemMod[] itemMods = info.itemMods;
			for (int i = 0; i < itemMods.Length; i++)
			{
				itemMods[i].OnRemove(this);
			}
		}
		removeTime = Time.time + fTime;
		OnDirty = null;
		position = -1;
		amount = 0;
		ItemManager.RemoveItem(this, fTime);
	}

	public void DoRemove()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		OnDirty = null;
		if (isServer && ((ItemId)(ref uid)).IsValid && Net.sv != null)
		{
			Net.sv.ReturnUID(uid.Value);
			uid = default(ItemId);
		}
		if (contents != null)
		{
			Pool.Free<ItemContainer>(ref contents);
		}
		if (isServer)
		{
			RemoveFromWorld();
			RemoveFromContainer();
			onCycle = null;
		}
		BaseEntity baseEntity = GetHeldEntity();
		if (baseEntity.IsValid())
		{
			Debug.LogWarning((object)("Item's Held Entity not removed!" + info.displayName.english + " -> " + (object)baseEntity), (Object)(object)baseEntity);
		}
	}

	public void SwitchOnOff(bool bNewState)
	{
		if (HasFlag(Flag.IsOn) != bNewState)
		{
			SetFlag(Flag.IsOn, bNewState);
			MarkDirty();
		}
	}

	public void LockUnlock(bool bNewState)
	{
		if (HasFlag(Flag.IsLocked) != bNewState && (!bNewState || Interface.CallHook("OnItemLock", this) == null) && (bNewState || Interface.CallHook("OnItemUnlock", this) == null))
		{
			SetFlag(Flag.IsLocked, bNewState);
			MarkDirty();
		}
	}

	public BasePlayer GetOwnerPlayer()
	{
		if (parent == null)
		{
			return null;
		}
		return parent.GetOwnerPlayer();
	}

	public bool IsBackpack()
	{
		if ((Object)(object)info != (Object)null)
		{
			return (info.flags & ItemDefinition.Flag.Backpack) != 0;
		}
		return false;
	}

	public int GetChildItemCount()
	{
		return (contents?.itemList?.Count).GetValueOrDefault();
	}

	public int GetItemVolume()
	{
		if (IsBackpack() && (contents?.itemList?.Count).GetValueOrDefault() > 0)
		{
			ItemModBackpack component = ((Component)info).GetComponent<ItemModBackpack>();
			if ((Object)(object)component != (Object)null)
			{
				return component.containerVolumeWhenFilled;
			}
		}
		return info.volume;
	}

	public Item SplitItem(int split_Amount)
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Expected O, but got Unknown
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		Assert.IsTrue(split_Amount > 0, "split_Amount <= 0");
		if (split_Amount <= 0)
		{
			return null;
		}
		if (split_Amount >= amount)
		{
			return null;
		}
		object obj = Interface.CallHook("OnItemSplit", this, split_Amount);
		if (obj is Item)
		{
			return (Item)obj;
		}
		amount -= split_Amount;
		Item item = ItemManager.CreateByItemID(info.itemid, split_Amount, skin, 0uL);
		MigrateItemOwnership(item, split_Amount);
		if (IsBlueprint())
		{
			item.blueprintTarget = blueprintTarget;
		}
		item.iconImageId = iconImageId;
		item.name = name;
		if (info.amountType == ItemDefinition.AmountType.Genetics && instanceData != null && instanceData.dataInt != 0)
		{
			item.instanceData = new InstanceData();
			item.instanceData.dataInt = instanceData.dataInt;
			item.instanceData.ShouldPool = false;
		}
		if (instanceData != null && instanceData.dataInt > 0 && (Object)(object)info != (Object)null && (Object)(object)info.Blueprint != (Object)null && info.Blueprint.GetWorkbenchLevel() == 3)
		{
			item.instanceData = new InstanceData();
			item.instanceData.dataInt = instanceData.dataInt;
			item.instanceData.ShouldPool = false;
			item.SetFlag(Flag.IsOn, IsOn());
		}
		if (instanceData != null && Object.op_Implicit((Object)(object)((Component)info).GetComponent<ItemModFoodSpoiling>()))
		{
			item.instanceData = new InstanceData();
			item.instanceData.dataFloat = instanceData.dataFloat;
			item.instanceData.ShouldPool = false;
		}
		MarkDirty();
		return item;
	}

	public void UnloadAmmo()
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		BaseProjectile baseProjectile = GetHeldEntity() as BaseProjectile;
		if ((Object)(object)baseProjectile == (Object)null)
		{
			return;
		}
		while (baseProjectile.primaryMagazine.contents > 0)
		{
			int num = Mathf.Min(baseProjectile.primaryMagazine.contents, baseProjectile.primaryMagazine.ammoType.stackable);
			baseProjectile.primaryMagazine.contents -= num;
			Item item = ItemManager.Create(baseProjectile.primaryMagazine.ammoType, num, 0uL, isServerSide: true, 0uL);
			BasePlayer basePlayer = GetRootContainer()?.playerOwner;
			if ((Object)(object)basePlayer != (Object)null)
			{
				basePlayer.GiveItem(item);
			}
			else if (!item.MoveToContainer(parent) && (Object)(object)item.Drop(parent.dropPosition, parent.dropVelocity) == (Object)null)
			{
				item.Remove();
			}
		}
	}

	public bool CanBeHeld()
	{
		if (isBroken)
		{
			return false;
		}
		if ((Object)(object)((Component)info).GetComponent<ItemModShield>() != (Object)null)
		{
			return false;
		}
		return true;
	}

	public bool CanStack(Item item)
	{
		object obj = Interface.CallHook("CanStackItem", this, item);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (item == this)
		{
			return false;
		}
		if (MaxStackable() <= 1)
		{
			return false;
		}
		if (item.info.stackable <= 1)
		{
			return false;
		}
		if (item.info.itemid != info.itemid)
		{
			return false;
		}
		if (hasCondition && condition != item.info.condition.max)
		{
			return false;
		}
		if (item.hasCondition && item.condition != item.info.condition.max)
		{
			return false;
		}
		if (!IsValid())
		{
			return false;
		}
		if (IsBlueprint() && blueprintTarget != item.blueprintTarget)
		{
			return false;
		}
		if (item.skin != skin)
		{
			return false;
		}
		if ((!string.IsNullOrEmpty(item.name) || !string.IsNullOrEmpty(name)) && item.name != name)
		{
			return false;
		}
		if (item.iconImageId != iconImageId)
		{
			return false;
		}
		if (item.info.amountType == ItemDefinition.AmountType.Genetics || info.amountType == ItemDefinition.AmountType.Genetics)
		{
			int num = ((item.instanceData != null) ? item.instanceData.dataInt : (-1));
			int num2 = ((instanceData != null) ? instanceData.dataInt : (-1));
			if (num != num2)
			{
				return false;
			}
		}
		if (item.instanceData != null && instanceData != null && (item.IsOn() != IsOn() || (item.instanceData.dataInt != instanceData.dataInt && (Object)(object)item.info.Blueprint != (Object)null && item.info.Blueprint.GetWorkbenchLevel() == 3)))
		{
			return false;
		}
		if (instanceData != null && ((NetworkableId)(ref instanceData.subEntity)).IsValid && Object.op_Implicit((Object)(object)((Component)info).GetComponent<ItemModSign>()))
		{
			return false;
		}
		if (item.instanceData != null && ((NetworkableId)(ref item.instanceData.subEntity)).IsValid && Object.op_Implicit((Object)(object)((Component)item.info).GetComponent<ItemModSign>()))
		{
			return false;
		}
		if (BlockStackFoodItem(item, this))
		{
			return false;
		}
		return true;
	}

	public static bool BlockStackFoodItem(Item a, Item b)
	{
		ItemModFoodSpoiling itemModFoodSpoiling = default(ItemModFoodSpoiling);
		if (a.instanceData != null && b.instanceData != null && ((Component)a.info).TryGetComponent<ItemModFoodSpoiling>(ref itemModFoodSpoiling))
		{
			bool flag = false;
			float dataFloat = a.instanceData.dataFloat;
			float dataFloat2 = b.instanceData.dataFloat;
			if (Mathf.Abs(dataFloat - dataFloat2) < ConVar.Server.maxFoodSpoilTimeDiffForItemStack)
			{
				flag = true;
			}
			if (!flag)
			{
				float num = itemModFoodSpoiling.TotalSpoilTimeHours * 60f * 60f;
				float num2 = a.instanceData.dataFloat / num;
				float num3 = b.instanceData.dataFloat / num;
				if (num2 > ConVar.Server.normalisedFoodSpoilTimeStackThreshold && num3 > ConVar.Server.normalisedFoodSpoilTimeStackThreshold)
				{
					flag = true;
				}
			}
			if (!flag && b.parent != null && (Object)(object)b.parent.entityOwner != (Object)null && b.parent.entityOwner is CookingWorkbench)
			{
				flag = true;
			}
			if (!flag)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsValid()
	{
		if (removeTime > 0f)
		{
			return false;
		}
		return true;
	}

	public bool IsDroppedInWorld(bool serverside)
	{
		return worldEnt.IsValid(serverside);
	}

	public void SetWorldEntity(BaseEntity ent)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (!ent.IsValid())
		{
			worldEnt.Set(null);
			MarkDirty();
		}
		else if (!(worldEnt.uid == ent.net.ID))
		{
			worldEnt.Set(ent);
			MarkDirty();
			OnMovedToWorld();
			if (contents != null)
			{
				contents.OnMovedToWorld();
			}
		}
	}

	public void OnMovedToWorld()
	{
		ItemMod[] itemMods = info.itemMods;
		for (int i = 0; i < itemMods.Length; i++)
		{
			itemMods[i].OnMovedToWorld(this);
		}
	}

	public BaseEntity GetWorldEntity()
	{
		return worldEnt.Get(isServer);
	}

	public void SetHeldEntity(BaseEntity ent)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (!ent.IsValid())
		{
			this.heldEntity.Set(null);
			MarkDirty();
		}
		else
		{
			if (this.heldEntity.uid == ent.net.ID)
			{
				return;
			}
			this.heldEntity.Set(ent);
			MarkDirty();
			if (ent.IsValid())
			{
				HeldEntity heldEntity = ent as HeldEntity;
				if ((Object)(object)heldEntity != (Object)null)
				{
					heldEntity.SetupHeldEntity(this);
				}
			}
		}
	}

	public BaseEntity GetHeldEntity()
	{
		return heldEntity.Get(isServer);
	}

	public void OnCycle(float delta)
	{
		if (onCycle != null)
		{
			onCycle(this, delta);
		}
	}

	public void ServerCommand(string command, BasePlayer player)
	{
		HeldEntity heldEntity = GetHeldEntity() as HeldEntity;
		if ((Object)(object)heldEntity != (Object)null)
		{
			heldEntity.ServerCommand(this, command, player);
		}
		ItemMod[] itemMods = info.itemMods;
		for (int i = 0; i < itemMods.Length; i++)
		{
			itemMods[i].ServerCommand(this, command, player);
		}
	}

	public void UseItem(int amountToConsume = 1)
	{
		if (amountToConsume > 0)
		{
			object obj = Interface.CallHook("OnItemUse", this, amountToConsume);
			if (obj is int)
			{
				amountToConsume = (int)obj;
			}
			amount -= amountToConsume;
			ReduceItemOwnership(amountToConsume);
			if (amount <= 0)
			{
				amount = 0;
				Remove();
			}
			else
			{
				MarkDirty();
			}
		}
	}

	public bool HasAmmo(AmmoTypes ammoType)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		ItemModProjectile itemModProjectile = default(ItemModProjectile);
		if (((Component)info).TryGetComponent<ItemModProjectile>(ref itemModProjectile) && itemModProjectile.IsAmmo(ammoType))
		{
			return true;
		}
		if (contents != null)
		{
			ItemModContainer itemModContainer = default(ItemModContainer);
			if ((Object)(object)info != (Object)null && ((Component)info).TryGetComponent<ItemModContainer>(ref itemModContainer) && itemModContainer.blockAmmoSource)
			{
				return false;
			}
			return contents.HasAmmo(ammoType);
		}
		return false;
	}

	public Item FindAmmo(AmmoTypes ammoType)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		ItemModProjectile itemModProjectile = default(ItemModProjectile);
		if (((Component)info).TryGetComponent<ItemModProjectile>(ref itemModProjectile) && itemModProjectile.IsAmmo(ammoType))
		{
			return this;
		}
		if (contents != null)
		{
			return contents.FindAmmo(ammoType);
		}
		return null;
	}

	[PoolAnalyzerNonCaching]
	public void FindAmmo(List<Item> list, AmmoTypes ammoType)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		ItemModProjectile itemModProjectile = default(ItemModProjectile);
		if (((Component)info).TryGetComponent<ItemModProjectile>(ref itemModProjectile) && itemModProjectile.IsAmmo(ammoType))
		{
			list.Add(this);
		}
		else if (contents != null)
		{
			contents.FindAmmo(list, ammoType);
		}
	}

	public int GetAmmoAmount(AmmoTypes ammoType)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		ItemModProjectile itemModProjectile = default(ItemModProjectile);
		if (((Component)info).TryGetComponent<ItemModProjectile>(ref itemModProjectile) && itemModProjectile.IsAmmo(ammoType))
		{
			num += amount;
		}
		if (contents != null)
		{
			num += contents.GetAmmoAmount(ammoType);
		}
		return num;
	}

	public int GetAmmoAmount(List<AmmoTypes> ammoTypes)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		ItemModProjectile itemModProjectile = default(ItemModProjectile);
		if (((Component)info).TryGetComponent<ItemModProjectile>(ref itemModProjectile))
		{
			foreach (AmmoTypes ammoType in ammoTypes)
			{
				if (itemModProjectile.IsAmmo(ammoType))
				{
					num += amount;
				}
			}
		}
		if (contents != null)
		{
			foreach (AmmoTypes ammoType2 in ammoTypes)
			{
				num += contents.GetAmmoAmount(ammoType2);
			}
		}
		return num;
	}

	public unsafe override string ToString()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		string[] obj = new string[6]
		{
			"Item.",
			info.shortname,
			"x",
			amount.ToString(),
			".",
			null
		};
		ItemId val = uid;
		obj[5] = ((object)(*(ItemId*)(&val))/*cast due to constrained. prefix*/).ToString();
		return string.Concat(obj);
	}

	public Item FindItem(ItemId iUID)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (uid == iUID)
		{
			return this;
		}
		if (contents == null)
		{
			return null;
		}
		return contents.FindItemByUID(iUID);
	}

	public int MaxStackable()
	{
		int num = info.stackable;
		if (parent != null && parent.maxStackSize > 0)
		{
			num = ((!parent.allowItemsToIncreaseToMaxStackSize) ? Mathf.Min(parent.maxStackSize, num) : Mathf.Max(parent.maxStackSize, num));
		}
		object obj = Interface.CallHook("OnMaxStackable", this);
		if (obj is int)
		{
			return (int)obj;
		}
		return num;
	}

	private void SetRadioactivity(ItemDefinition template)
	{
		if (!((Object)(object)template == (Object)null))
		{
			radioactivity = template.baseRadioactivity;
			if (radioactivity > 0f)
			{
				SetFlag(Flag.Radioactive, b: true);
			}
		}
	}

	public GameObjectRef GetWorldModel()
	{
		return info.GetWorldModel(amount);
	}

	public virtual Item Save(bool bIncludeContainer = false, bool bIncludeOwners = true)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		dirty = false;
		Item val = Pool.Get<Item>();
		val.UID = uid;
		val.itemid = info.itemid;
		val.slot = position;
		val.amount = amount;
		val.flags = (int)flags;
		val.removetime = removeTime;
		val.locktime = 0f;
		val.instanceData = instanceData;
		val.worldEntity = worldEnt.uid;
		val.heldEntity = heldEntity.uid;
		val.skinid = skin;
		val.name = name;
		val.streamerName = streamerName;
		val.text = text;
		val.cooktime = cookTimeLeft;
		val.iconImageId = iconImageId;
		val.attachmentid = attachment;
		if (ownershipShares != null)
		{
			val.ownership = Pool.Get<List<ItemOwnershipAmount>>();
			foreach (ItemOwnershipShare ownershipShare in ownershipShares)
			{
				ItemOwnershipAmount val2 = Pool.Get<ItemOwnershipAmount>();
				val2.username = ownershipShare.username;
				val2.reason = ownershipShare.reason;
				val2.amount = ownershipShare.amount;
				val.ownership.Add(val2);
			}
		}
		val.ammoCount = 0;
		NetworkableId val3 = heldEntity.uid;
		if (((NetworkableId)(ref val3)).IsValid)
		{
			BaseEntity baseEntity = GetHeldEntity();
			if (baseEntity is BaseProjectile baseProjectile)
			{
				val.ammoCount = baseProjectile.primaryMagazine.contents + 1;
			}
			else if (baseEntity is Chainsaw chainsaw)
			{
				val.ammoCount = chainsaw.ammo + 1;
			}
			else if (baseEntity is FlameThrower flameThrower)
			{
				val.ammoCount = flameThrower.ammo + 1;
			}
		}
		if (hasCondition)
		{
			val.conditionData = Pool.Get<ConditionData>();
			val.conditionData.maxCondition = _maxCondition;
			val.conditionData.condition = _condition;
		}
		if ((contents != null) & bIncludeContainer)
		{
			val.contents = contents.Save(bIncludeContainer);
		}
		return val;
	}

	public virtual void Load(Item load)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)info == (Object)null || info.itemid != load.itemid)
		{
			info = ItemManager.FindItemDefinition(load.itemid);
		}
		if ((Object)(object)info == (Object)null)
		{
			Debug.LogError((object)$"Load invalid item id {load.itemid} from item {load.UID} (no ItemDefinition found)");
			return;
		}
		uid = load.UID;
		name = load.name;
		streamerName = load.streamerName;
		text = load.text;
		cookTimeLeft = load.cooktime;
		amount = load.amount;
		position = load.slot;
		removeTime = load.removetime;
		flags = (Flag)load.flags;
		worldEnt.uid = load.worldEntity;
		heldEntity.uid = load.heldEntity;
		iconImageId = load.iconImageId;
		attachment = load.attachmentid;
		if (load.ownership != null && load.ownership.Count > 0)
		{
			if (ownershipShares == null)
			{
				ownershipShares = Pool.Get<List<ItemOwnershipShare>>();
			}
			else
			{
				ownershipShares.Clear();
			}
			foreach (ItemOwnershipAmount item2 in load.ownership)
			{
				ItemOwnershipShare item = new ItemOwnershipShare
				{
					username = item2.username,
					reason = item2.reason,
					amount = item2.amount
				};
				ownershipShares.Add(item);
			}
		}
		else if (ownershipShares != null)
		{
			Pool.FreeUnmanaged<ItemOwnershipShare>(ref ownershipShares);
		}
		SetRadioactivity(info);
		InitializeItemOwnership();
		if (isServer)
		{
			Net.sv.RegisterUID(uid.Value);
		}
		if (instanceData != null && load.instanceData != instanceData)
		{
			instanceData.ShouldPool = true;
			instanceData.ResetToPool();
			instanceData = null;
		}
		instanceData = load.instanceData;
		if (instanceData != null)
		{
			instanceData.ShouldPool = false;
		}
		skin = load.skinid;
		_condition = 0f;
		_maxCondition = 0f;
		if (load.conditionData != null)
		{
			_condition = load.conditionData.condition;
			_maxCondition = load.conditionData.maxCondition;
		}
		else if (info.condition.enabled)
		{
			_condition = info.condition.max;
			_maxCondition = info.condition.max;
		}
		if (load.contents != null)
		{
			if (contents == null)
			{
				contents = Pool.Get<ItemContainer>();
				if (isServer)
				{
					contents.ServerInitialize(this, load.contents.slots);
				}
			}
			contents.Load(load.contents);
		}
		if (isServer)
		{
			removeTime = 0f;
			OnItemCreated();
		}
	}
}
