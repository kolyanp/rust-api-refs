using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;

public class Marketplace : BaseEntity
{
	[Header("Marketplace")]
	public GameObjectRef terminalPrefab;

	public Transform[] terminalPoints;

	public Transform droneLaunchPoint;

	public GameObjectRef deliveryDronePrefab;

	public static readonly List<Marketplace> serverMarketplaces = new List<Marketplace>();

	[NonSerialized]
	public EntityRef<MarketTerminal>[] terminalEntities;

	private float currentCharge;

	private float lastChargeTickTime;

	private bool acceptingOrders;

	private bool hasAppliedAcceptingOrders;

	private bool hasRestoredCharge;

	private Action _actionChargeTick;

	private float __sync_ChargeFraction;

	[Sync(Autosave = true)]
	public float ChargeFraction
	{
		[CompilerGenerated]
		get
		{
			return __sync_ChargeFraction;
		}
		[CompilerGenerated]
		private set
		{
			if (!IsSyncVarEqual(__sync_ChargeFraction, value))
			{
				__sync_ChargeFraction = value;
				byte nameID = __GetWeaverID("ChargeFraction");
				QueueSyncVar(nameID);
			}
		}
	}

	private Action actionChargeTick => ServerChargeTick;

	public override void ServerInit()
	{
		base.ServerInit();
		serverMarketplaces.Add(this);
		lastChargeTickTime = Time.time;
		InvokeRandomized(actionChargeTick, 1f, 1f, 0.1f);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		serverMarketplaces.Remove(this);
		CancelInvoke(actionChargeTick);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Server_RestoreCharge();
	}

	public bool Server_CanAcceptOrder()
	{
		if (Powergrid.enabled)
		{
			return acceptingOrders;
		}
		return true;
	}

	public float Server_GetCurrentCharge()
	{
		return currentCharge;
	}

	private static float GetChargeCapacity()
	{
		return Mathf.Max(Powergrid.marketplaceChargeCapacity, 1f);
	}

	private void Server_RestoreCharge()
	{
		if (!hasRestoredCharge)
		{
			currentCharge = Mathf.Clamp01(ChargeFraction) * GetChargeCapacity();
			hasRestoredCharge = true;
			Server_RefreshChargeState();
		}
	}

	private void ServerChargeTick()
	{
		using (TimeWarning.New("Marketplace.ServerChargeTick"))
		{
			float time = Time.time;
			float num = time - lastChargeTickTime;
			lastChargeTickTime = time;
			if (Application.isServerStarted && !Application.isLoadingSave)
			{
				Server_RestoreCharge();
				if (!Powergrid.enabled)
				{
					Server_SetAcceptingOrders(accepting: true);
					return;
				}
				currentCharge = Mathf.Clamp(currentCharge + Server_GetChargeRate() * num, 0f, GetChargeCapacity());
				Server_RefreshChargeState();
			}
		}
	}

	public float Server_GetChargeRate()
	{
		int num = (((Object)(object)PointEntity<PowergridManager>.ServerInstance != (Object)null) ? PointEntity<PowergridManager>.ServerInstance.Server_GetPowerPlantInsertedFuses() : 0);
		if (num <= 0)
		{
			return 0f - Mathf.Max(Powergrid.marketplaceDrainRate, 0f);
		}
		if (num < Powergrid.marketplaceMinimumFusesToCharge)
		{
			return 0f;
		}
		return (float)num * Mathf.Max(Powergrid.marketplaceChargePerFuse, 0f);
	}

	private void Server_RefreshChargeState()
	{
		if (!Powergrid.enabled)
		{
			Server_SetAcceptingOrders(accepting: true);
			return;
		}
		float num = GetChargeCapacity() * Mathf.Clamp01(Powergrid.marketplaceRequiredChargeFraction);
		Server_SetAcceptingOrders(currentCharge >= num);
		float num2 = Mathf.Round(Mathf.Clamp01(currentCharge / GetChargeCapacity()) * 100f) / 100f;
		if (!Mathf.Approximately(num2, ChargeFraction))
		{
			ChargeFraction = num2;
		}
	}

	private void Server_SetAcceptingOrders(bool accepting)
	{
		if (!hasAppliedAcceptingOrders || acceptingOrders != accepting)
		{
			acceptingOrders = accepting;
			hasAppliedAcceptingOrders = true;
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags))
			{
				flagsUpdateScope.Set(Flags.On, accepting);
			}
			Server_RefreshTerminalPowerFlags();
		}
	}

	private void Server_RefreshTerminalPowerFlags()
	{
		if (terminalEntities == null)
		{
			return;
		}
		bool hasPower = Server_CanAcceptOrder();
		for (int i = 0; i < terminalEntities.Length; i++)
		{
			if (terminalEntities[i].TryGet(serverside: true, out var entity))
			{
				entity.Server_SetMarketplaceHasPower(hasPower);
			}
		}
	}

	public NetworkableId SendDrone(BasePlayer player, MarketTerminal sourceTerminal, VendingMachine vendingMachine)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)sourceTerminal == (Object)null || (Object)(object)vendingMachine == (Object)null)
		{
			return default(NetworkableId);
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(deliveryDronePrefab?.resourcePath, droneLaunchPoint.position, droneLaunchPoint.rotation);
		if (!(baseEntity is DeliveryDrone deliveryDrone))
		{
			baseEntity.Kill();
			return default(NetworkableId);
		}
		deliveryDrone.OwnerID = player.userID;
		deliveryDrone.Spawn();
		deliveryDrone.Setup(this, sourceTerminal, vendingMachine);
		return deliveryDrone.net.ID;
	}

	public void ReturnDrone(DeliveryDrone deliveryDrone)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (deliveryDrone.sourceTerminal.TryGet(serverside: true, out var entity))
		{
			entity.CompleteOrder(deliveryDrone.targetVendingMachine.uid);
		}
		deliveryDrone.Kill();
	}

	public override void Spawn()
	{
		base.Spawn();
		if (!Application.isLoadingSave)
		{
			SpawnSubEntities();
		}
	}

	private void SpawnSubEntities()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (!base.isServer)
		{
			return;
		}
		if (terminalEntities != null && terminalEntities.Length > terminalPoints.Length)
		{
			for (int i = terminalPoints.Length; i < terminalEntities.Length; i++)
			{
				if (terminalEntities[i].TryGet(serverside: true, out var entity))
				{
					entity.Kill();
				}
			}
		}
		Array.Resize(ref terminalEntities, terminalPoints.Length);
		for (int j = 0; j < terminalPoints.Length; j++)
		{
			Transform val = terminalPoints[j];
			if (!terminalEntities[j].TryGet(serverside: true, out var _))
			{
				BaseEntity baseEntity = GameManager.server.CreateEntity(terminalPrefab?.resourcePath, val.position, val.rotation);
				baseEntity.SetParent(this, worldPositionStays: true);
				baseEntity.Spawn();
				if (!(baseEntity is MarketTerminal marketTerminal))
				{
					Debug.LogError((object)("Marketplace.terminalPrefab did not spawn a MarketTerminal (it spawned " + ((object)baseEntity).GetType().FullName + ")"));
					baseEntity.Kill();
				}
				else
				{
					marketTerminal.Setup(this);
					terminalEntities[j].Set(marketTerminal);
				}
			}
		}
		Server_RefreshTerminalPowerFlags();
	}

	public override void Load(LoadInfo info)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.subEntityList != null)
		{
			List<NetworkableId> subEntityIds = info.msg.subEntityList.subEntityIds;
			Array.Resize(ref terminalEntities, subEntityIds.Count);
			for (int i = 0; i < subEntityIds.Count; i++)
			{
				terminalEntities[i] = new EntityRef<MarketTerminal>(subEntityIds[i]);
			}
		}
		SpawnSubEntities();
	}

	public override void Save(SaveInfo info)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.subEntityList = Pool.Get<SubEntityList>();
		info.msg.subEntityList.subEntityIds = Pool.Get<List<NetworkableId>>();
		if (terminalEntities != null)
		{
			for (int i = 0; i < terminalEntities.Length; i++)
			{
				info.msg.subEntityList.subEntityIds.Add(terminalEntities[i].uid);
			}
		}
	}

	private void OnSyncVar_ChargeFraction(float? oldValue, float newValue)
	{
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: ChargeFraction for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_ChargeFraction);
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
				float? oldValue = __sync_ChargeFraction;
				float newValue = (__sync_ChargeFraction = reader.Float());
				if (fromAutoSave)
				{
					oldValue = null;
				}
				OnSyncVar_ChargeFraction(oldValue, newValue);
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
		if (propertyName == "ChargeFraction")
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
		__sync_ChargeFraction = 0f;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}
}
