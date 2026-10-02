using System;
using System.Runtime.CompilerServices;
using System.Threading;
using ConVar;
using Facepunch;
using GameMenu;
using Network;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class BiofuelGenerator : ContainerIOEntity
{
	private const int OutputSlot = 3;

	public const Flags Flag_Stirred = Flags.Reserved1;

	public const Flags Flag_Finished = Flags.Reserved5;

	public const Flags Flag_Cooking = Flags.Reserved3;

	public const Flags Flag_NeedsStir = Flags.Reserved4;

	public const float StirTime = 10f;

	public int powerConsumption = 2;

	[Min(0.1f)]
	public float processInterval = 36f;

	[Min(1f)]
	public float caloriesPerFuel = 40f;

	[Min(1f)]
	public int fuelPerStir = 150;

	public ItemDefinition[] wasteItems;

	public GameObjectRef stirMountPrefab;

	public Transform stirMountPoint;

	public Animator stirAnimator;

	public Transform leftHandGrip;

	public Transform rightHandGrip;

	[Header("Stir Audio")]
	public SoundDefinition stirLoopSoundDef;

	public SoundDefinition stirStartSoundDef;

	public SoundDefinition stirStopSoundDef;

	private static ItemDefinition lowGradeFuel;

	private float lastProcessTime;

	private float stirStartTime;

	private float __sync_StirCharge;

	private float __sync_ProcessedFuel;

	public BaseMountable StirMount { get; private set; }

	private static ItemDefinition LowGradeFuel => lowGradeFuel ?? (lowGradeFuel = ItemManager.FindItemDefinition("lowgradefuel"));

	[Sync(Autosave = true)]
	public float StirCharge
	{
		[CompilerGenerated]
		get
		{
			return __sync_StirCharge;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_StirCharge, value))
			{
				__sync_StirCharge = value;
				byte nameID = __GetWeaverID("StirCharge");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public float ProcessedFuel
	{
		[CompilerGenerated]
		get
		{
			return __sync_ProcessedFuel;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_ProcessedFuel, value))
			{
				__sync_ProcessedFuel = value;
				byte nameID = __GetWeaverID("ProcessedFuel");
				QueueSyncVar(nameID);
			}
		}
	}

	public float StirProgress => Mathf.Clamp01(StirCharge / (float)Mathf.Max(1, fuelPerStir));

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BiofuelGenerator.OnRpcMessage"))
		{
			if (rpc == 1481300760 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Stir"));
				}
				using (TimeWarning.New("RPC_Stir"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1481300760u, "RPC_Stir", this, player, 3f))
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
							RPC_Stir(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_Stir");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private static float GetCalories(Item item)
	{
		return item.info.ItemModConsumable?.GetBiofuelCalories() ?? 0f;
	}

	private static bool HasCalorieOverride(Item item)
	{
		return item.info.ItemModConsumable?.overrideBiofuelCalories ?? false;
	}

	private bool IsWaste(Item item)
	{
		return Array.IndexOf(wasteItems, item.info) >= 0;
	}

	public float GetFuelYield(Item item)
	{
		if (IsWaste(item))
		{
			return 1f;
		}
		float calories = GetCalories(item);
		if (calories <= 0f)
		{
			return 0f;
		}
		float num = calories / Mathf.Max(1f, caloriesPerFuel);
		if (HasCalorieOverride(item))
		{
			return num;
		}
		return Mathf.Max(1f, num);
	}

	public float GetProcessDuration(Item item)
	{
		float num = Mathf.Max(1f, GetFuelYield(item));
		return Mathf.Max(0.1f, processInterval * num);
	}

	private static int OutputSpace(ItemContainer container)
	{
		Item slot = container.GetSlot(3);
		if (slot == null)
		{
			return LowGradeFuel.stackable;
		}
		if (!((Object)(object)slot.info == (Object)(object)LowGradeFuel))
		{
			return 0;
		}
		return Mathf.Max(0, slot.MaxStackable() - slot.amount);
	}

	public bool CanProcessFood(ItemContainer container)
	{
		if (IsPowered() && IsStirred() && ProcessedFuel < 0.9999f)
		{
			return OutputSpace(container) > 0;
		}
		return false;
	}

	public bool IsStirring()
	{
		if (!IsStirred() && (Object)(object)StirMount != (Object)null)
		{
			return StirMount.IsMounted();
		}
		return false;
	}

	public override int ConsumptionAmount()
	{
		return powerConsumption;
	}

	public bool IsStirred()
	{
		return HasFlag(Flags.Reserved1);
	}

	public bool CanStir(BasePlayer player)
	{
		if (!IsStirred() && (Object)(object)StirMount != (Object)null && !StirMount.IsMounted() && StirMount.GetDistanceFromMountAnchor(player) <= StirMount.maxMountDistance)
		{
			return HasStirFloor();
		}
		return false;
	}

	private bool HasStirFloor()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit hitInfo;
		return GamePhysics.Trace(new Ray(StirMount.mountAnchor.position + Vector3.up * 0.5f, Vector3.down), 0.1f, out hitInfo, 1f, 1503764737, (QueryTriggerInteraction)1, this);
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (child is BaseMountable stirMount)
		{
			StirMount = stirMount;
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (outputSlot != 0 || !IsPowered() || !HasFlag(Flags.Reserved4))
		{
			return 0;
		}
		return 1;
	}

	public override void ServerInit()
	{
		lastProcessTime = Time.time;
		base.ServerInit();
		inventory.canAcceptItem = CanAcceptItem;
		ItemContainer itemContainer = inventory;
		itemContainer.onItemAddedToStack = (Action<Item, int, BasePlayer>)Delegate.Combine(itemContainer.onItemAddedToStack, new Action<Item, int, BasePlayer>(OnFoodAdded));
		InvokeRandomized(ProcessFood, 1f, 1f, 0.5f);
		if (!Application.isLoadingSave)
		{
			SpawnStirMount();
		}
	}

	protected override bool HasAttachments()
	{
		return children.Count > (((Object)(object)StirMount != (Object)null) ? 1 : 0);
	}

	private void SpawnStirMount()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = GameManager.server.CreateEntity(stirMountPrefab.resourcePath, stirMountPoint.localPosition, stirMountPoint.localRotation);
		baseEntity.SetParent(this);
		baseEntity.Spawn();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (!IsStirred() || StirCharge <= 0f)
		{
			SetStirred(stirred: false);
		}
		UpdateProcessingProgress();
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		UpdateProcessingProgress();
		return base.PlayerOpenLoot(player, panelToOpen, doPositionChecks);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added, BasePlayer sourcePlayer)
	{
		base.OnItemAddedOrRemoved(item, added, sourcePlayer);
		if (added && item.position < 3 && IsOrganicMatter(item))
		{
			if (!Application.isLoadingSave)
			{
				item.cookTimeLeft = GetProcessDuration(item);
			}
			OnFoodAdded(item, item.amount, sourcePlayer);
			item.MarkDirty();
		}
		else if (!added && item.HasFlag(Item.Flag.Cooking))
		{
			item.SetFlag(Item.Flag.Cooking, b: false);
			item.MarkDirty();
		}
		UpdateProcessingProgress();
	}

	private void OnFoodAdded(Item item, int amount, BasePlayer sourcePlayer)
	{
		if (!Application.isLoadingSave && amount > 0 && item.position >= 0 && item.position < 3 && IsOrganicMatter(item))
		{
			SetStirred(stirred: false);
			if ((Object)(object)StirMount != (Object)null && StirMount.IsMounted())
			{
				StirMount.DismountAllPlayers();
			}
		}
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		UpdateProcessingProgress();
	}

	public void UpdateProcessingProgress()
	{
		if (inventory == null)
		{
			return;
		}
		bool flag = CanProcessFood(inventory);
		bool flag2 = false;
		bool flag3 = HasFlag(Flags.Reserved4);
		for (int i = 0; i < 3; i++)
		{
			Item slot = inventory.GetSlot(i);
			if (slot != null)
			{
				bool flag4 = slot.amount > 0 && IsOrganicMatter(slot);
				flag2 |= flag4;
				bool flag5 = flag & flag4;
				if (slot.HasFlag(Item.Flag.Cooking) != flag5)
				{
					slot.SetFlag(Item.Flag.Cooking, flag5);
					slot.MarkDirty();
				}
			}
		}
		Item slot2 = inventory.GetSlot(3);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved5, !flag2 && slot2 != null && (Object)(object)slot2.info == (Object)(object)LowGradeFuel && slot2.amount > 0);
			flagsUpdateScope.Set(Flags.Reserved3, flag2 & flag);
			flagsUpdateScope.Set(Flags.Reserved4, flag2 && !IsStirred());
		}
		if (flag3 != HasFlag(Flags.Reserved4))
		{
			MarkDirty();
		}
	}

	private bool IsOrganicMatter(Item item)
	{
		if (!item.IsBlueprint())
		{
			if (!IsWaste(item))
			{
				if (item.info.category == ItemCategory.Food || item.info.ItemModCompostable != null)
				{
					return GetCalories(item) > 0f;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private bool CanAcceptItem(BasePlayer player, Item item, int targetSlot)
	{
		if (targetSlot == 3)
		{
			if (!item.IsBlueprint())
			{
				return (Object)(object)item.info == (Object)(object)LowGradeFuel;
			}
			return false;
		}
		if (targetSlot < 3)
		{
			return IsOrganicMatter(item);
		}
		return false;
	}

	public void ProcessFood()
	{
		float elapsed = Time.time - lastProcessTime;
		lastProcessTime = Time.time;
		ProcessFood(elapsed);
	}

	public void ProcessFood(float elapsed)
	{
		using (TimeWarning.New("BiofuelGenerator.ProcessFood"))
		{
			if (inventory == null)
			{
				return;
			}
			OutputProcessedFuel();
			for (int i = 0; i < 3; i++)
			{
				Item slot = inventory.GetSlot(i);
				if (slot == null || !IsOrganicMatter(slot))
				{
					continue;
				}
				float num = elapsed;
				while (num > 0f && slot.amount > 0 && slot.parent == inventory && CanProcessFood(inventory))
				{
					float cookTimeLeft = slot.cookTimeLeft;
					float num2 = Mathf.Min(num, Mathf.Max(0f, cookTimeLeft));
					slot.cookTimeLeft -= num2;
					num -= num2;
					if (slot.cookTimeLeft > 0.0001f)
					{
						if (Mathf.FloorToInt(cookTimeLeft / 5f) != Mathf.FloorToInt(slot.cookTimeLeft / 5f))
						{
							slot.MarkDirty();
						}
						break;
					}
					ProcessedFuel += GetFuelYield(slot);
					slot.cookTimeLeft = GetProcessDuration(slot);
					slot.UseItem();
					OutputProcessedFuel();
				}
			}
			UpdateProcessingProgress();
		}
	}

	private void OutputProcessedFuel()
	{
		if (!IsPowered() || !IsStirred())
		{
			return;
		}
		int num = Mathf.Min(new int[3]
		{
			Mathf.FloorToInt(ProcessedFuel + 0.0001f),
			Mathf.FloorToInt(StirCharge),
			OutputSpace(inventory)
		});
		if (num <= 0)
		{
			return;
		}
		Item item = ItemManager.Create(LowGradeFuel, num, 0uL, isServerSide: true, 0uL);
		if (item == null)
		{
			return;
		}
		if (!item.MoveToContainer(inventory, 3, allowStack: true, ignoreStackLimit: false, null, allowSwap: false))
		{
			item.Remove();
			return;
		}
		ProcessedFuel = Mathf.Max(0f, ProcessedFuel - (float)num);
		StirCharge = Mathf.Max(0f, StirCharge - (float)num);
		if (StirCharge <= 0f)
		{
			SetStirred(stirred: false);
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void RPC_Stir(RPCMessage msg)
	{
		Stir(msg.player, (RPCProgressBarState)msg.read.Int32());
	}

	public void Stir(BasePlayer player, RPCProgressBarState state)
	{
		if (state != RPCProgressBarState.Start && Debugging.biofuel_stir_lock)
		{
			return;
		}
		switch (state)
		{
		case RPCProgressBarState.Start:
			if (player.CanInteract() && CanStir(player))
			{
				stirStartTime = Time.realtimeSinceStartup;
				StirMount.AttemptMount(player);
				if ((Object)(object)StirMount.GetMounted() == (Object)(object)player)
				{
					InvokeRepeating(UpdateStirCharge, 0f, 0.1f);
				}
			}
			break;
		case RPCProgressBarState.Complete:
			if (!((Object)(object)StirMount == (Object)null) && !((Object)(object)StirMount.GetMounted() != (Object)(object)player) && !(Mathf.Abs(stirStartTime + 10f - Time.realtimeSinceStartup) > ConVar.AntiHack.rpc_timer_forgiveness))
			{
				SetStirred(stirred: true);
				StirMount.DismountPlayer(player);
			}
			break;
		case RPCProgressBarState.Cancel:
			if ((Object)(object)StirMount != (Object)null && (Object)(object)StirMount.GetMounted() == (Object)(object)player)
			{
				SetStirred(stirred: false);
				StirMount.DismountPlayer(player);
			}
			break;
		}
	}

	public void SetStirred(bool stirred)
	{
		CancelInvoke(UpdateStirCharge);
		StirCharge = (stirred ? ((float)Mathf.Max(1, fuelPerStir)) : 0f);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved1, stirred);
		}
		UpdateProcessingProgress();
	}

	private void UpdateStirCharge()
	{
		if (!IsStirring())
		{
			SetStirred(stirred: false);
		}
		else
		{
			StirCharge = Mathf.Clamp01((Time.realtimeSinceStartup - stirStartTime) / 10f) * (float)Mathf.Max(1, fuelPerStir);
		}
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: StirCharge for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_StirCharge);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: ProcessedFuel for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_ProcessedFuel);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		switch (id)
		{
		case 0:
			try
			{
				_ = __sync_StirCharge;
				float _sync_StirCharge = reader.Float();
				__sync_StirCharge = _sync_StirCharge;
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_ProcessedFuel;
				float _sync_ProcessedFuel = reader.Float();
				__sync_ProcessedFuel = _sync_ProcessedFuel;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		default:
			return base.OnSyncVar(id, reader, fromAutoSave);
		}
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (!(propertyName == "StirCharge"))
		{
			if (propertyName == "ProcessedFuel")
			{
				return 1;
			}
			return byte.MaxValue;
		}
		return 0;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
		WriteSyncVar(1, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
		OnSyncVar(1, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		WriteAutoSaveSyncVars(netWrite);
		(byte[] Buffer, int Length) buffer = netWrite.GetBuffer();
		byte[] item = buffer.Buffer;
		int item2 = buffer.Length;
		byte[] array = _autosaveBuffer;
		if (array == null || array.Length < item2)
		{
			byte[] array2 = BaseEntity._autosaveBufferPool.Rent(item2);
			while (array == null || array.Length < item2)
			{
				byte[] array3 = Interlocked.CompareExchange(ref _autosaveBuffer, array2, array);
				if (array3 == array)
				{
					if (array3 != null)
					{
						BaseEntity._autosaveBufferPool.Return(array3);
					}
					array = array2;
					break;
				}
				array = array3;
			}
			if (array != array2)
			{
				BaseEntity._autosaveBufferPool.Return(array2);
			}
		}
		Buffer.BlockCopy(item, 0, array, 0, item2);
		save.msg.baseEntity.syncVars = array;
		Pool.Free<NetWrite>(ref netWrite);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead netRead = Pool.Get<NetRead>();
			netRead.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(netRead);
			Pool.Free<NetRead>(ref netRead);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_StirCharge = 0f;
		__sync_ProcessedFuel = 0f;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}
}
