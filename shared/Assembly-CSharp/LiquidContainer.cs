using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class LiquidContainer : ContainerIOEntity
{
	public const Flags Flag_OpenTap = Flags.Reserved5;

	public ItemDefinition defaultLiquid;

	public int startingAmount;

	public bool autofillOutputs;

	public float autofillTickRate = 2f;

	public int autofillTickAmount = 2;

	public int maxOutputFlow = 6;

	public bool prioritizeDrinkMenuOption;

	public ItemDefinition[] ValidItems;

	[Header("Water Level Mesh")]
	[Tooltip("Replicate how full this container is. Turn it on for containers that show a water level mesh, and leave it off everywhere else: water moves through a barrel every tick and the updates are only worth paying for when something is showing the level.")]
	public bool replicateWaterLevel;

	[Tooltip("Mesh shown while there is liquid in here, posed between the empty and full values below to match how full the container is.")]
	public Transform waterLevelMesh;

	public Vector3 waterLevelMeshEmptyPosition;

	public Vector3 waterLevelMeshFullPosition;

	public Vector3 waterLevelMeshEmptyScale = Vector3.one;

	public Vector3 waterLevelMeshFullScale = Vector3.one;

	private int currentDrainAmount;

	protected HashSet<IOEntity> connectedList = new HashSet<IOEntity>();

	protected HashSet<ContainerIOEntity> pushTargets = new HashSet<ContainerIOEntity>();

	private const int maxPushTargets = 12;

	private IOEntity considerConnectedTo;

	private Action updateDrainAmountAction;

	private Action updatePushLiquidTargetsAction;

	private Action pushLiquidAction;

	private Action deductFuelAction;

	private TimeUntil waterTransferStartTime;

	private float lastOutputDrainUpdate;

	private static HashSet<IOEntity> pushLiquidCheckEntityHash = new HashSet<IOEntity>();

	private byte __sync_WaterLevel;

	public override bool IsGravitySource => true;

	[Sync(Autosave = true)]
	public byte WaterLevel
	{
		[CompilerGenerated]
		get
		{
			return __sync_WaterLevel;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_WaterLevel, value))
			{
				__sync_WaterLevel = value;
				byte nameID = __GetWeaverID("WaterLevel");
				QueueSyncVar(nameID);
			}
		}
	}

	public float WaterLevelFraction => (float)(int)WaterLevel / 255f;

	protected override bool DisregardGravityRestrictionsOnLiquid
	{
		get
		{
			if (!HasFlag(Flags.Reserved8))
			{
				return base.DisregardGravityRestrictionsOnLiquid;
			}
			return true;
		}
	}

	public override bool BlockFluidDraining => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("LiquidContainer.OnRpcMessage"))
		{
			if (rpc == 2002733690 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SVDrink"));
				}
				using (TimeWarning.New("SVDrink"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2002733690u, "SVDrink", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SVDrink(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SVDrink");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool AllowWireConnections()
	{
		if (HasParent() && (Object)(object)parentEntity.Get(isServer) != (Object)null && parentEntity.Get(isServer) is VehicleModuleStorage)
		{
			return true;
		}
		return base.AllowWireConnections();
	}

	public override bool IsRootEntity()
	{
		return true;
	}

	private bool CanAcceptItem(BasePlayer player, Item item, int count)
	{
		if (ValidItems == null || ValidItems.Length == 0)
		{
			return true;
		}
		ItemDefinition[] validItems = ValidItems;
		for (int i = 0; i < validItems.Length; i++)
		{
			if ((Object)(object)validItems[i] == (Object)(object)item.info)
			{
				return true;
			}
		}
		return false;
	}

	public override void ServerInit()
	{
		updateDrainAmountAction = UpdateDrainAmount;
		pushLiquidAction = PushLiquidThroughOutputs;
		deductFuelAction = DeductFuel;
		updatePushLiquidTargetsAction = UpdatePushLiquidTargets;
		base.ServerInit();
		if (startingAmount > 0)
		{
			inventory.AddItem(defaultLiquid, startingAmount, 0uL);
		}
		if (autofillOutputs && HasLiquidItem())
		{
			UpdatePushLiquidTargets();
		}
		ItemContainer itemContainer = inventory;
		itemContainer.canAcceptItem = (Func<BasePlayer, Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<BasePlayer, Item, int, bool>(CanAcceptItem));
		RefreshWaterLevel();
	}

	protected override void OnInventoryDirty()
	{
		base.OnInventoryDirty();
		RefreshWaterLevel();
	}

	private void RefreshWaterLevel()
	{
		if (replicateWaterLevel && maxStackSize > 0)
		{
			int liquidCount = GetLiquidCount();
			WaterLevel = (byte)((liquidCount > 0) ? ((byte)Mathf.Clamp(Mathf.CeilToInt((float)liquidCount / (float)maxStackSize * 255f), 1, 255)) : 0);
		}
	}

	public override void OnCircuitChanged(bool forceUpdate)
	{
		base.OnCircuitChanged(forceUpdate);
		RefreshDrainsAndPushTargets();
	}

	protected override void OnOutputDisconnected(int index)
	{
		base.OnOutputDisconnected(index);
		RefreshDrainsAndPushTargets();
	}

	private void RefreshDrainsAndPushTargets()
	{
		ClearDrains();
		Invoke(updateDrainAmountAction, 0.1f);
		if (autofillOutputs && HasLiquidItem())
		{
			Invoke(updatePushLiquidTargetsAction, 0.1f);
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool added, BasePlayer sourcePlayer)
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		base.OnItemAddedOrRemoved(item, added, sourcePlayer);
		UpdateOnFlag();
		MarkDirtyForceUpdateOutputs();
		Invoke(updateDrainAmountAction, 0.1f);
		if (connectedList.Count > 0)
		{
			List<IOEntity> list = Pool.Get<List<IOEntity>>();
			foreach (IOEntity connected in connectedList)
			{
				if ((Object)(object)connected != (Object)null)
				{
					list.Add(connected);
				}
			}
			foreach (IOEntity item2 in list)
			{
				item2.SendChangedToRoot(forceUpdate: true);
			}
			Pool.FreeUnmanaged<IOEntity>(ref list);
		}
		if (HasLiquidItem() && autofillOutputs)
		{
			Invoke(updatePushLiquidTargetsAction, 0.1f);
		}
		if (added)
		{
			waterTransferStartTime = TimeUntil.op_Implicit(10f);
		}
	}

	private void ClearDrains()
	{
		foreach (IOEntity connected in connectedList)
		{
			if ((Object)(object)connected != (Object)null)
			{
				connected.SetFuelType(null, null);
			}
		}
		connectedList.Clear();
	}

	public override int GetCurrentEnergy()
	{
		return Mathf.Clamp(GetLiquidCount(), 0, maxOutputFlow);
	}

	public override int CalculateCurrentEnergy(int inputAmount, int inputSlot)
	{
		if (!HasLiquidItem())
		{
			return base.CalculateCurrentEnergy(inputAmount, inputSlot);
		}
		return GetCurrentEnergy();
	}

	private void UpdateDrainAmount()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		int amount = 0;
		Item liquidItem = GetLiquidItem();
		if (liquidItem != null)
		{
			IOSlot[] array = outputs;
			foreach (IOSlot iOSlot in array)
			{
				if ((Object)(object)iOSlot.connectedTo.Get() != (Object)null)
				{
					CalculateDrain(iOSlot.connectedTo.Get(), ((Component)this).transform.TransformPoint(iOSlot.handlePosition), IOEntity.backtracking * 2, ref amount, this, liquidItem?.info);
				}
			}
		}
		currentDrainAmount = Mathf.Clamp(amount, 0, maxOutputFlow);
		if (currentDrainAmount <= 0 && IsInvoking(deductFuelAction))
		{
			CancelInvoke(deductFuelAction);
		}
		else if (currentDrainAmount > 0 && !IsInvoking(deductFuelAction))
		{
			InvokeRepeating(deductFuelAction, 0f, 1f);
		}
	}

	private void CalculateDrain(IOEntity ent, Vector3 fromSlotWorld, int depth, ref int amount, IOEntity lastEntity, ItemDefinition waterType)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ent == (Object)(object)this || depth <= 0 || (Object)(object)ent == (Object)null || (Object)(object)lastEntity == (Object)null || ent is LiquidContainer)
		{
			return;
		}
		if (!ent.BlockFluidDraining && ent.HasFlag(Flags.On))
		{
			int num = ent.DesiredPower();
			amount += num;
			ent.SetFuelType(waterType, this);
			connectedList.Add(ent);
		}
		if (!ent.AllowLiquidPassthrough(lastEntity, fromSlotWorld))
		{
			return;
		}
		IOSlot[] array = ent.outputs;
		foreach (IOSlot iOSlot in array)
		{
			if ((Object)(object)iOSlot.connectedTo.Get() != (Object)null && (Object)(object)iOSlot.connectedTo.Get() != (Object)(object)ent)
			{
				CalculateDrain(iOSlot.connectedTo.Get(), ((Component)ent).transform.TransformPoint(iOSlot.handlePosition), depth - 1, ref amount, ent, waterType);
			}
		}
	}

	public override void UpdateOutputs()
	{
		base.UpdateOutputs();
		if (!(Time.realtimeSinceStartup - lastOutputDrainUpdate < 0.2f))
		{
			lastOutputDrainUpdate = Time.realtimeSinceStartup;
			ClearDrains();
			Invoke(updateDrainAmountAction, 0.1f);
		}
	}

	private void DeductFuel()
	{
		if (HasLiquidItem())
		{
			Item liquidItem = GetLiquidItem();
			liquidItem.amount -= currentDrainAmount;
			liquidItem.MarkDirty();
			if (liquidItem.amount <= 0)
			{
				liquidItem.Remove();
			}
		}
	}

	protected void UpdateOnFlag()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, inventory.itemList.Count > 0 && inventory.itemList[0].amount > 0);
	}

	public virtual void OpenTap(float duration)
	{
		if (!HasFlag(Flags.Reserved5))
		{
			SetFlagLocal(Flags.Reserved5, b: true);
			Invoke(ShutTap, duration);
			SendNetworkUpdateImmediate();
		}
	}

	public virtual void ShutTap()
	{
		SetFlagLocal(Flags.Reserved5, b: false);
		SendNetworkUpdateImmediate();
	}

	public bool HasLiquidItem()
	{
		return GetLiquidItem() != null;
	}

	public Item GetLiquidItem()
	{
		if (inventory == null || inventory.itemList.Count == 0)
		{
			return null;
		}
		return inventory.itemList[0];
	}

	public int GetLiquidCount()
	{
		if (!HasLiquidItem())
		{
			return 0;
		}
		return GetLiquidItem().amount;
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void SVDrink(RPCMessage rpc)
	{
		if (!rpc.player.metabolism.CanConsume() || Interface.CallHook("OnPlayerDrink", rpc.player, this) != null)
		{
			return;
		}
		foreach (Item item in inventory.itemList)
		{
			ItemModConsume component = ((Component)item.info).GetComponent<ItemModConsume>();
			if (!((Object)(object)component == (Object)null) && component.CanDoAction(item, rpc.player))
			{
				component.DoAction(item, rpc.player);
				break;
			}
		}
	}

	private void UpdatePushLiquidTargets()
	{
		pushTargets.Clear();
		if (!HasLiquidItem() || IsConnectedTo(this, IOEntity.backtracking * 2))
		{
			return;
		}
		Item liquidItem = GetLiquidItem();
		pushLiquidCheckEntityHash.Clear();
		using (TimeWarning.New("UpdatePushTargets"))
		{
			IOSlot[] array = outputs;
			foreach (IOSlot iOSlot in array)
			{
				if (iOSlot.type == IOType.Fluidic)
				{
					IOEntity iOEntity = iOSlot.connectedTo.Get();
					if ((Object)(object)iOEntity != (Object)null)
					{
						CheckPushLiquid(iOEntity, liquidItem, this, IOEntity.backtracking * 4, pushLiquidCheckEntityHash);
					}
				}
			}
		}
		if (pushTargets.Count > 0)
		{
			InvokeRandomized(pushLiquidAction, 0f, autofillTickRate, autofillTickRate * 0.2f);
		}
	}

	private void PushLiquidThroughOutputs()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (TimeUntil.op_Implicit(waterTransferStartTime) > 0f)
		{
			return;
		}
		if (!HasLiquidItem())
		{
			CancelInvoke(pushLiquidAction);
			return;
		}
		Item liquidItem = GetLiquidItem();
		int num = 0;
		if (pushTargets.Count > 0)
		{
			int num2 = Mathf.Clamp(autofillTickAmount, 0, liquidItem.amount) / pushTargets.Count;
			if (num2 == 0 && liquidItem.amount > 0)
			{
				num2 = liquidItem.amount;
			}
			if (ConVar.Server.waterContainersLeaveWaterBehind && num2 == liquidItem.amount)
			{
				num2--;
			}
			if (num2 == 0)
			{
				return;
			}
			foreach (ContainerIOEntity pushTarget in pushTargets)
			{
				if (pushTarget.inventory == null)
				{
					if (!IsInvoking(updatePushLiquidTargetsAction))
					{
						Invoke(updatePushLiquidTargetsAction, 0.1f);
					}
				}
				else if (pushTarget.inventory.CanAcceptItem(null, liquidItem, 0) == ItemContainer.CanAcceptResult.CanAccept && (pushTarget.inventory.CanAccept(liquidItem) || pushTarget.inventory.FindItemByItemID(liquidItem.info.itemid) != null))
				{
					int num3 = Mathf.Clamp(num2, 0, pushTarget.inventory.GetMaxTransferAmount(liquidItem.info));
					pushTarget.inventory.AddItem(liquidItem.info, num3, 0uL);
					if (liquidItem.IsLocked())
					{
						pushTarget.inventory.GetSlot(0)?.LockUnlock(bNewState: true);
					}
					num += num3;
					liquidItem.amount -= num3;
					liquidItem.MarkDirty();
					if (liquidItem.amount <= 0)
					{
						break;
					}
				}
			}
		}
		if (num > 0)
		{
			OnWaterMoved(num);
		}
		if (liquidItem.amount <= 0 || pushTargets.Count == 0)
		{
			if (liquidItem.amount <= 0)
			{
				liquidItem.Remove();
			}
			CancelInvoke(pushLiquidAction);
		}
	}

	protected virtual void OnWaterMoved(int amount)
	{
	}

	private void CheckPushLiquid(IOEntity connected, Item ourFuel, IOEntity fromSource, int depth, HashSet<IOEntity> checkEntities)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if (depth <= 0 || ourFuel.amount <= 0)
		{
			return;
		}
		Vector3 worldHandlePosition = Vector3.zero;
		IOEntity iOEntity = connected.FindGravitySource(ref worldHandlePosition, IOEntity.backtracking * 2, ignoreSelf: true);
		if (((Object)(object)iOEntity != (Object)null && !connected.AllowLiquidPassthrough(iOEntity, worldHandlePosition)) || (Object)(object)connected == (Object)(object)this || ConsiderConnectedTo(connected))
		{
			return;
		}
		if (connected is ContainerIOEntity containerIOEntity && !pushTargets.Contains(containerIOEntity) && containerIOEntity.inventory.CanAcceptItem(null, ourFuel, 0) == ItemContainer.CanAcceptResult.CanAccept)
		{
			pushTargets.Add(containerIOEntity);
			return;
		}
		IOSlot[] array = connected.outputs;
		foreach (IOSlot iOSlot in array)
		{
			IOEntity iOEntity2 = iOSlot.connectedTo.Get();
			if (!((Object)(object)iOEntity2 != (Object)null) || !((Object)(object)iOEntity2 != (Object)(object)fromSource) || !checkEntities.Add(iOEntity2))
			{
				continue;
			}
			Vector3 sourceWorldPosition = ((Component)connected).transform.TransformPoint(iOSlot.handlePosition);
			if (iOEntity2.AllowLiquidPassthrough(fromSource, sourceWorldPosition))
			{
				CheckPushLiquid(iOEntity2, ourFuel, fromSource, depth - 1, checkEntities);
				if (pushTargets.Count >= 12)
				{
					break;
				}
			}
		}
	}

	public void SetConnectedTo(IOEntity entity)
	{
		considerConnectedTo = entity;
	}

	protected override bool ConsiderConnectedTo(IOEntity entity)
	{
		return (Object)(object)entity == (Object)(object)considerConnectedTo;
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: WaterLevel for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_WaterLevel);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		if (id == 0)
		{
			try
			{
				_ = __sync_WaterLevel;
				byte _sync_WaterLevel = reader.UInt8();
				__sync_WaterLevel = _sync_WaterLevel;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		}
		return base.OnSyncVar(id, reader, fromAutoSave);
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (propertyName == "WaterLevel")
		{
			return 0;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
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
		__sync_WaterLevel = 0;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}

	public LiquidContainer()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
	}
}
