using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Recycler : StorageContainer, IPowergridEntity
{
	public enum RecyclerState
	{
		Unpowered,
		PoweredInactive,
		PoweredActive
	}

	public const Flags Flag_InSafeZone = Flags.Reserved9;

	public const Flags Flag_ReceivingPowergridPower = Flags.Reserved11;

	private static readonly int Param_On = Animator.StringToHash("on");

	[Header("Recycler")]
	public RecyclerConfig.RecyclerType recyclerType;

	public Animator Animator;

	public MeshRenderer lightRenderer;

	public Renderer[] recyclerRenderers;

	public Light lightComponent;

	public Color lightGreenColor;

	public Color lightRedColor;

	public string recyclerGreenMaterialAssetPath;

	public string recyclerYellowMaterialAssetPath;

	public string recyclerRedMaterialAssetPath;

	public string lightRedMaterialAssetPath;

	public string lightGreenMaterialAssetPath;

	public string lightOffMaterialAssetPath;

	public SoundDefinition grindingLoopDef;

	public SoundDefinition grindingLoopDef_Slow;

	public GameObjectRef startSound;

	public GameObjectRef stopSound;

	public GameObjectRef errorSound;

	private Action _actionRecycleThink;

	public float scrapRemainder;

	private float lastFetchedEfficiency;

	private int __sync_CurrentReceivedPowergridStage;

	private int __sync_RecyclerTypeSyncVar;

	[Sync(Autosave = true)]
	public int CurrentReceivedPowergridStage
	{
		[CompilerGenerated]
		get
		{
			return __sync_CurrentReceivedPowergridStage;
		}
		[CompilerGenerated]
		private set
		{
			if (!IsSyncVarEqual(__sync_CurrentReceivedPowergridStage, value))
			{
				__sync_CurrentReceivedPowergridStage = value;
				byte nameID = __GetWeaverID("CurrentReceivedPowergridStage");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public int RecyclerTypeSyncVar
	{
		[CompilerGenerated]
		get
		{
			return __sync_RecyclerTypeSyncVar;
		}
		[CompilerGenerated]
		private set
		{
			if (!IsSyncVarEqual(__sync_RecyclerTypeSyncVar, value))
			{
				__sync_RecyclerTypeSyncVar = value;
				byte nameID = __GetWeaverID("RecyclerTypeSyncVar");
				QueueSyncVar(nameID);
			}
		}
	}

	private Action actionRecycleThink => RecycleThink;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Recycler.OnRpcMessage"))
		{
			if (rpc == 4167839872u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SVSwitch"));
				}
				using (TimeWarning.New("SVSwitch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4167839872u, "SVSwitch", this, player, 3f))
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
							SVSwitch(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SVSwitch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	private bool CanBeRecycled(Item item)
	{
		object obj = Interface.CallHook("CanBeRecycled", item, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (item != null)
		{
			return (Object)(object)item.info.Blueprint != (Object)null;
		}
		return false;
	}

	bool IPowergridEntity.Server_ShouldConnectToPowergrid()
	{
		return true;
	}

	void IPowergridEntity.Server_OnPowergridStageChanged(int newStage)
	{
		if (base.isServer)
		{
			CurrentReceivedPowergridStage = newStage;
			Server_RefreshPowergridState();
		}
	}

	public void Server_RefreshPowergridState()
	{
		if (!base.isServer)
		{
			return;
		}
		int num = 0;
		RecyclerConfig instance = RecyclerConfig.instance;
		if (instance != null && instance.TryGetConfigForType(GetRecyclerType(), out var zoneConfig))
		{
			num = zoneConfig.requiredPowergridStageToUse;
		}
		bool b = num <= 0 || GetEffectivePowergridStage() >= num;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Reserved11, b);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (base.isServer)
		{
			ItemContainer itemContainer = base.inventory;
			itemContainer.canAcceptItem = (Func<BasePlayer, Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<BasePlayer, Item, int, bool>(RecyclerItemFilter));
			ItemContainer itemContainer2 = base.inventory;
			itemContainer2.onItemAddedRemoved = (Action<Item, bool>)Delegate.Combine(itemContainer2.onItemAddedRemoved, new Action<Item, bool>(OnItemAddedRemoved));
			if (!Application.isLoadingSave)
			{
				RecyclerTypeSyncVar = (int)recyclerType;
			}
			UpdateInSafeZone();
			Server_RefreshPowergridState();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Server_RefreshPowergridState();
	}

	private void OnItemAddedRemoved(Item item, bool added)
	{
		if (added && IsOn() && (Object)(object)base.LastLootedByPlayer != (Object)null)
		{
			item.CollectedForCrafting(base.LastLootedByPlayer);
		}
	}

	public bool RecyclerItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		int num = Mathf.CeilToInt((float)base.inventory.capacity * 0.5f);
		if (targetSlot == -1)
		{
			bool flag = false;
			for (int i = 0; i < num; i++)
			{
				if (!base.inventory.SlotTaken(item, i))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		if (targetSlot < num)
		{
			return CanBeRecycled(item);
		}
		return true;
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void SVSwitch(RPCMessage msg)
	{
		bool flag = msg.read.Bit();
		if (flag == IsOn() || (Object)(object)msg.player == (Object)null || Interface.CallHook("OnRecyclerToggle", this, msg.player) != null || (onlyOneUser && (Object)(object)msg.player.inventory.loot.entitySource != (Object)(object)this) || (flag && GetRecyclerState() == RecyclerState.Unpowered) || (flag && !HasRecyclable()))
		{
			return;
		}
		if (flag)
		{
			foreach (Item item in base.inventory.itemList)
			{
				item.CollectedForCrafting(msg.player);
			}
			StartRecycling();
		}
		else
		{
			StopRecycling();
		}
	}

	public bool MoveItemToOutput(Item newItem)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		int num = -1;
		for (int i = 6; i < 12; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot == null)
			{
				num = i;
				break;
			}
			if (slot.CanStack(newItem))
			{
				if (slot.amount + newItem.amount <= slot.MaxStackable())
				{
					num = i;
					break;
				}
				int num2 = Mathf.Min(slot.MaxStackable() - slot.amount, newItem.amount);
				newItem.UseItem(num2);
				slot.amount += num2;
				slot.MarkDirty();
				newItem.MarkDirty();
			}
			if (newItem.amount <= 0)
			{
				return true;
			}
		}
		if (num != -1 && newItem.MoveToContainer(base.inventory, num))
		{
			return true;
		}
		newItem.Drop(((Component)this).transform.position + new Vector3(0f, 2f, 0f), GetInheritedDropVelocity() + ((Component)this).transform.forward * 2f);
		return false;
	}

	public bool HasRecyclable()
	{
		for (int i = 0; i < 6; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot != null)
			{
				object obj = Interface.CallHook("CanRecycle", this, slot);
				if (obj is bool)
				{
					return (bool)obj;
				}
				if ((Object)(object)slot.info.Blueprint != (Object)null)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void RecycleThink()
	{
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		float num = lastFetchedEfficiency;
		int num2 = 0;
		while (true)
		{
			if (num2 < 6)
			{
				Item slot = base.inventory.GetSlot(num2);
				if (!CanBeRecycled(slot))
				{
					num2++;
					continue;
				}
				if (Interface.CallHook("OnItemRecycle", slot, this) != null)
				{
					if (!HasRecyclable())
					{
						StopRecycling();
					}
					break;
				}
				if (slot.hasCondition)
				{
					num = Mathf.Clamp01(num * Mathf.Clamp(slot.conditionNormalized * slot.maxConditionNormalized, 0.1f, 1f));
				}
				int num3 = 1;
				if (slot.amount > 1)
				{
					num3 = Mathf.CeilToInt(Mathf.Min((float)slot.amount, (float)slot.MaxStackable() * 0.1f));
				}
				object obj = Interface.CallHook("OnItemRecycleAmount", slot, num3, this);
				if (obj is int)
				{
					num3 = (int)obj;
				}
				if (slot.info.Blueprint.scrapFromRecycle > 0)
				{
					float num4 = slot.info.Blueprint.scrapFromRecycle * num3;
					if (slot.MaxStackable() == 1 && slot.hasCondition)
					{
						num4 *= slot.conditionNormalized;
					}
					float num5 = num / 0.5f;
					num4 *= num5;
					int num6 = Mathf.FloorToInt(num4);
					float num7 = num4 - (float)num6;
					scrapRemainder += num7;
					if (scrapRemainder >= 1f)
					{
						int num8 = Mathf.FloorToInt(scrapRemainder);
						scrapRemainder -= num8;
						num6 += num8;
					}
					if (num6 >= 1)
					{
						Item item = ItemManager.CreateByName("scrap", num6, 0uL);
						if ((Object)(object)base.LastLootedByPlayer != (Object)null)
						{
							item.SetItemOwnership(base.LastLootedByPlayer, ItemOwnershipPhrases.Recycler);
						}
						Facepunch.Rust.Analytics.Azure.OnRecyclerItemProduced(item.info.shortname, item.amount, this, slot);
						MoveItemToOutput(item);
					}
				}
				if (!string.IsNullOrEmpty(slot.info.Blueprint.RecycleStat))
				{
					List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
					Vis.Entities(((Component)this).transform.position, 3f, list, 131072, (QueryTriggerInteraction)2);
					foreach (BasePlayer item3 in list)
					{
						if (item3.IsAlive() && !item3.IsSleeping() && (Object)(object)item3.inventory.loot.entitySource == (Object)(object)this)
						{
							item3.stats.Add(slot.info.Blueprint.RecycleStat, num3, (Stats)5);
							item3.stats.Save();
						}
					}
					Pool.FreeUnmanaged<BasePlayer>(ref list);
				}
				Facepunch.Rust.Analytics.Azure.OnItemRecycled(slot.info.shortname, num3, this);
				slot.UseItem(num3);
				foreach (ItemAmount ingredient in slot.info.Blueprint.GetIngredients())
				{
					if (ingredient.itemDef.shortname == "scrap")
					{
						continue;
					}
					float num9 = ingredient.amount / (float)slot.info.Blueprint.amountToCreate * num * (float)num3;
					int num10 = Mathf.FloorToInt(num9);
					float num11 = num9 - (float)num10;
					if (num11 > float.Epsilon && Random.Range(0f, 1f) <= num11)
					{
						num10++;
					}
					if (num10 <= 0)
					{
						continue;
					}
					int num12 = Mathf.CeilToInt((float)num10 / (float)ingredient.itemDef.stackable);
					for (int i = 0; i < num12; i++)
					{
						if (ingredient.itemDef.IsAllowed((EraRestriction)16))
						{
							int num13 = ((num10 > ingredient.itemDef.stackable) ? ingredient.itemDef.stackable : num10);
							Item item2 = ItemManager.Create(ingredient.itemDef, num13, 0uL, isServerSide: true, 0uL);
							if ((Object)(object)base.LastLootedByPlayer != (Object)null)
							{
								item2.SetItemOwnership(base.LastLootedByPlayer, ItemOwnershipPhrases.Recycler);
							}
							Facepunch.Rust.Analytics.Azure.OnRecyclerItemProduced(item2.info.shortname, item2.amount, this, slot);
							if (!MoveItemToOutput(item2))
							{
								flag = true;
							}
							num10 -= num13;
							if (num10 <= 0)
							{
								break;
							}
						}
					}
				}
			}
			if (flag || !HasRecyclable())
			{
				StopRecycling();
			}
			break;
		}
	}

	public void StartRecycling()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (!base.isServer || IsOn())
		{
			return;
		}
		GetRecyclerStats(out var efficiency, out var duration);
		lastFetchedEfficiency = efficiency;
		InvokeRepeating(actionRecycleThink, duration, duration);
		Effect.server.Run(startSound.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.On, b: true);
	}

	public void StopRecycling()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (!base.isServer)
		{
			return;
		}
		if (IsInvoking(actionRecycleThink))
		{
			CancelInvoke(actionRecycleThink);
		}
		if (!IsOn())
		{
			return;
		}
		Effect.server.Run(stopSound.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.On, b: false);
	}

	public void UpdateInSafeZone()
	{
		if (base.isServer)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags))
			{
				flagsUpdateScope.Set(Flags.Reserved9, IsInSafeZone());
			}
			Server_RefreshPowergridState();
		}
	}

	private bool IsInSafeZone()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (BaseGameMode.TryGetActiveGameMode(base.isServer, out var gameMode) && !gameMode.safeZone)
		{
			return false;
		}
		bool result = false;
		List<TriggerBase> list = Pool.Get<List<TriggerBase>>();
		GamePhysics.OverlapSphere<TriggerBase>(((Component)this).transform.position, 1f, list, 262144, (QueryTriggerInteraction)2);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			TriggerBase triggerBase = list[i];
			if (!((Object)(object)triggerBase == (Object)null))
			{
				if (triggerBase is TriggerSafeZone)
				{
					result = true;
				}
				else if (triggerBase is TriggerSafeZoneOverride { IsCombatActive: not false })
				{
					result = false;
					break;
				}
			}
		}
		Pool.FreeUnmanaged<TriggerBase>(ref list);
		return result;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (base.isServer)
		{
			bool num = (old & Flags.Reserved11) == Flags.Reserved11;
			bool flag = (next & Flags.Reserved11) == Flags.Reserved11;
			if (num != flag && !flag)
			{
				StopRecycling();
			}
		}
	}

	public bool IsSafezoneRecycler()
	{
		return HasFlag(Flags.Reserved9);
	}

	public RecyclerState GetRecyclerState()
	{
		if (RequiresPowergrid() && Powergrid.enabled && !IsReceivingPowergridPower())
		{
			return RecyclerState.Unpowered;
		}
		if (!IsOn())
		{
			return RecyclerState.PoweredInactive;
		}
		return RecyclerState.PoweredActive;
	}

	public bool IsReceivingPowergridPower()
	{
		return HasFlag(Flags.Reserved11);
	}

	public int GetEffectivePowergridStage()
	{
		if (!Powergrid.enabled)
		{
			return 0;
		}
		return CurrentReceivedPowergridStage;
	}

	public void GetRecyclerStats(out float efficiency, out float duration)
	{
		efficiency = 0f;
		duration = 0f;
		RecyclerConfig.RecyclerType recyclerType = GetRecyclerType();
		if (RecyclerConfig.instance.TryGetConfigForType(recyclerType, out var zoneConfig))
		{
			bool enabled = Powergrid.enabled;
			int effectivePowergridStage = GetEffectivePowergridStage();
			bool flag = false;
			flag = ((recyclerType != RecyclerConfig.RecyclerType.Green || Powergrid.greenRecyclerFullEfficiencyStage < 0) ? (enabled && effectivePowergridStage >= zoneConfig.efficiencyBuffPowergridStage && zoneConfig.efficiencyBuffPowergridStage > 0) : (effectivePowergridStage >= Powergrid.greenRecyclerFullEfficiencyStage && zoneConfig.efficiencyBuffPowergridStage > 0));
			bool flag2 = enabled && effectivePowergridStage >= zoneConfig.durationBuffPowergridStage && zoneConfig.durationBuffPowergridStage > 0;
			efficiency = (flag ? zoneConfig.powergridEfficiency : zoneConfig.efficiency);
			duration = (flag2 ? zoneConfig.powergridDuration : zoneConfig.duration);
		}
	}

	public RecyclerConfig.RecyclerType GetRecyclerType()
	{
		RecyclerConfig.RecyclerType recyclerTypeSyncVar = (RecyclerConfig.RecyclerType)RecyclerTypeSyncVar;
		if (!IsSafezoneRecycler() && recyclerTypeSyncVar == RecyclerConfig.RecyclerType.Yellow)
		{
			return RecyclerConfig.RecyclerType.Green;
		}
		return recyclerTypeSyncVar;
	}

	public bool RequiresPowergrid()
	{
		if (!RecyclerConfig.instance.TryGetConfigForType(GetRecyclerType(), out var zoneConfig))
		{
			return false;
		}
		return zoneConfig.requiredPowergridStageToUse > 0;
	}

	[UnityEvent]
	public void PlayAnim()
	{
	}

	[UnityEvent]
	public void StopAnim()
	{
	}

	private void ToggleAnim(bool toggle)
	{
	}

	private void OnSyncVar_RecyclerTypeSyncVar(int? oldValue, int newValue)
	{
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
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
				Debug.Log((object)("SyncVar Writing: CurrentReceivedPowergridStage for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_CurrentReceivedPowergridStage);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: RecyclerTypeSyncVar for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_RecyclerTypeSyncVar);
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
				_ = __sync_CurrentReceivedPowergridStage;
				int _sync_CurrentReceivedPowergridStage = reader.Int32();
				__sync_CurrentReceivedPowergridStage = _sync_CurrentReceivedPowergridStage;
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 1:
			try
			{
				int? oldValue = __sync_RecyclerTypeSyncVar;
				int newValue = (__sync_RecyclerTypeSyncVar = reader.Int32());
				if (fromAutoSave)
				{
					oldValue = null;
				}
				OnSyncVar_RecyclerTypeSyncVar(oldValue, newValue);
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
		if (!(propertyName == "CurrentReceivedPowergridStage"))
		{
			if (propertyName == "RecyclerTypeSyncVar")
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
		var (src, num) = netWrite.GetBuffer();
		if (_autosaveBuffer == null)
		{
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		if (_autosaveBuffer.Length < num)
		{
			BaseEntity._autosaveBufferPool.Return(_autosaveBuffer);
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		Buffer.BlockCopy(src, 0, _autosaveBuffer, 0, num);
		save.msg.baseEntity.syncVars = _autosaveBuffer;
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
		__sync_CurrentReceivedPowergridStage = 0;
		__sync_RecyclerTypeSyncVar = 0;
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

	public Recycler()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		lightGreenColor = new Color(2f / 3f, 1f, 0.08627451f);
		lightRedColor = new Color(1f, 0.04849673f, 0.01568627f);
		base._002Ector();
	}
}
