using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;

public class PowergridManager : PointEntity<PowergridManager>
{
	public struct PowergridEntityEntry
	{
		public IPowergridEntity Entity;

		public float SqrDistanceToPowerPlant;
	}

	private struct InsertedFuseEntry
	{
		public PowergridFuseBox FuseBox;

		public Item Fuse;
	}

	private static Vector3 powerPlantPosition;

	private static bool hasCachedPowerPlantPosition;

	private const string powerlineAccessPointPrefabPath = "assets/prefabs/io/electric/generators/powergrid_powerline_io.static.prefab";

	private static readonly List<PowergridEntityEntry> powergridEntities = new List<PowergridEntityEntry>();

	public static readonly PowergridStageChangeWorkQueue stageChangeWorkQueue = new PowergridStageChangeWorkQueue(powergridEntities);

	private static ListHashSet<PowergridFuseBox> fuseBoxes = new ListHashSet<PowergridFuseBox>();

	private static ListHashSet<PowerlinePowergridAccessPointSpawn> powerlineAccessPointSpawns = new ListHashSet<PowerlinePowergridAccessPointSpawn>();

	private static ListHashSet<PowergridIOAccessPoint> spawnedPowergridAccessPoints = new ListHashSet<PowergridIOAccessPoint>();

	private static int noOfAccessPoints;

	private static List<InsertedFuseEntry> insertedFuses = new List<InsertedFuseEntry>();

	private static int currentInsertedFusesCount;

	private static int fuseSocketsCount;

	private List<ItemId> loadedFuseInsertionOrder;

	private int previousInsertedFusesCount;

	private bool hasTickedOnce;

	private float lastFuseDeteriorationTickTime;

	private Action _actionServerTick;

	private Action _actionServerFuseDeteriorationTick;

	private int __sync_CurrentStage;

	private float __sync_LastProcessedStateChangeSqrDistance;

	[Sync(Autosave = true)]
	public int CurrentStage
	{
		[CompilerGenerated]
		get
		{
			return __sync_CurrentStage;
		}
		[CompilerGenerated]
		private set
		{
			if (!IsSyncVarEqual(__sync_CurrentStage, value))
			{
				__sync_CurrentStage = value;
				byte nameID = __GetWeaverID("CurrentStage");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync]
	public float LastProcessedStateChangeSqrDistance
	{
		[CompilerGenerated]
		get
		{
			return __sync_LastProcessedStateChangeSqrDistance;
		}
		[CompilerGenerated]
		private set
		{
			if (!IsSyncVarEqual(__sync_LastProcessedStateChangeSqrDistance, value))
			{
				__sync_LastProcessedStateChangeSqrDistance = value;
				byte nameID = __GetWeaverID("LastProcessedStateChangeSqrDistance");
				QueueSyncVar(nameID);
			}
		}
	}

	private Action actionServerTick => ServerTick;

	private Action actionServerFuseDeteriorationTick => ServerFuseDeteriorationTick;

	public static int GetCurrentStage(bool isServer)
	{
		if (isServer)
		{
			if ((Object)(object)PointEntity<PowergridManager>.ServerInstance == (Object)null)
			{
				return 0;
			}
			return PointEntity<PowergridManager>.ServerInstance.CurrentStage;
		}
		return 0;
	}

	public static Vector3 GetPowerPlantPosition()
	{
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if (!hasCachedPowerPlantPosition)
		{
			if ((Object)(object)TerrainMeta.Path == (Object)null || TerrainMeta.Path.Monuments == null)
			{
				return Vector3.zero;
			}
			PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
			try
			{
				List<MonumentInfo> monuments = TerrainMeta.Path.Monuments;
				int i = 0;
				for (int count = monuments.Count; i < count; i++)
				{
					MonumentInfo monumentInfo = monuments[i];
					if (monumentInfo.IsPowerPlant())
					{
						((List<Vector3>)(object)val).Add(((Component)monumentInfo).transform.position);
					}
				}
				int count2 = ((List<Vector3>)(object)val).Count;
				if (count2 > 0)
				{
					if (count2 == 1)
					{
						powerPlantPosition = ((List<Vector3>)(object)val)[0];
					}
					else
					{
						Vector3 val2 = Vector3.zero;
						for (int j = 0; j < count2; j++)
						{
							val2 += ((List<Vector3>)(object)val)[j];
						}
						powerPlantPosition = val2 / (float)count2;
					}
				}
				else
				{
					powerPlantPosition = Vector3.zero;
				}
				hasCachedPowerPlantPosition = true;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		return powerPlantPosition;
	}

	public static void Server_AddPowergridEntity(IPowergridEntity powergridEntity)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (powergridEntity.Server_ShouldConnectToPowergrid())
		{
			float num = Vector3.SqrMagnitude(((Component)powergridEntity.GetEntity()).transform.position - GetPowerPlantPosition());
			int index = FindSortedInsertIndex(num);
			powergridEntities.Insert(index, new PowergridEntityEntry
			{
				Entity = powergridEntity,
				SqrDistanceToPowerPlant = num
			});
			stageChangeWorkQueue.OnEntityInserted(index, powergridEntity);
			if (Application.isServerStarted && (Object)(object)PointEntity<PowergridManager>.ServerInstance != (Object)null)
			{
				powergridEntity.Server_OnPowergridStageChanged(PointEntity<PowergridManager>.ServerInstance.CurrentStage);
			}
		}
	}

	public static void Server_RemovePowergridEntity(IPowergridEntity powergridEntity)
	{
		int i = 0;
		for (int count = powergridEntities.Count; i < count; i++)
		{
			if (powergridEntities[i].Entity == powergridEntity)
			{
				powergridEntities.RemoveAt(i);
				stageChangeWorkQueue.OnEntityRemoved(i, powergridEntity);
				break;
			}
		}
	}

	private static int FindSortedInsertIndex(float sqrDistance)
	{
		int num = 0;
		int num2 = powergridEntities.Count;
		while (num < num2)
		{
			int num3 = num + num2 >> 1;
			if (powergridEntities[num3].SqrDistanceToPowerPlant <= sqrDistance)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3;
			}
		}
		return num;
	}

	public static void Server_AddPowergridFuseBox(PowergridFuseBox fuseBox)
	{
		if (fuseBoxes.TryAdd(fuseBox))
		{
			fuseSocketsCount += fuseBox.GetMaxNoOfFuses();
		}
	}

	public static void Server_RemovePowergridFuseBox(PowergridFuseBox fuseBox)
	{
		if (fuseBoxes.Remove(fuseBox))
		{
			fuseSocketsCount -= fuseBox.GetMaxNoOfFuses();
			if (fuseSocketsCount < 0)
			{
				fuseSocketsCount = 0;
			}
		}
		for (int num = insertedFuses.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)insertedFuses[num].FuseBox == (Object)(object)fuseBox)
			{
				insertedFuses.RemoveAt(num);
			}
		}
	}

	public static void Server_OnFuseInsertedIntoFuseBox(PowergridFuseBox fuseBox, Item fuse, BasePlayer byPlayer)
	{
		InsertedFuseEntry item = new InsertedFuseEntry
		{
			FuseBox = fuseBox,
			Fuse = fuse
		};
		insertedFuses.Add(item);
		currentInsertedFusesCount++;
		if (Application.isServerStarted)
		{
			Facepunch.Rust.Analytics.Azure.OnPowerGridFuseInserted(byPlayer, fuse, currentInsertedFusesCount);
		}
	}

	public static void Server_OnFuseRemovedFromFuseBox(Item fuse)
	{
		int i = 0;
		for (int count = insertedFuses.Count; i < count; i++)
		{
			if (insertedFuses[i].Fuse == fuse)
			{
				insertedFuses.RemoveAt(i);
				currentInsertedFusesCount--;
				break;
			}
		}
	}

	public static void Server_AddPowerlineAccessPointSpawn(PowerlinePowergridAccessPointSpawn accessPointSpawn)
	{
		powerlineAccessPointSpawns.TryAdd(accessPointSpawn);
	}

	public static void Server_RemovePowerlineAccessPointSpawn(PowerlinePowergridAccessPointSpawn accessPointSpawn)
	{
		powerlineAccessPointSpawns.Remove(accessPointSpawn);
	}

	public static void Server_AddPowergridAccessPoint(PowergridIOAccessPoint accessPoint)
	{
		spawnedPowergridAccessPoints.TryAdd(accessPoint);
		noOfAccessPoints++;
	}

	public static void Server_RemovePowergridAccessPoint(PowergridIOAccessPoint accessPoint)
	{
		spawnedPowergridAccessPoints.Remove(accessPoint);
		noOfAccessPoints--;
	}

	public static void SpawnPowerlineAccessPoints()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		Vector3 pos = default(Vector3);
		Quaternion rot = default(Quaternion);
		for (int count = powerlineAccessPointSpawns.Count; i < count; i++)
		{
			((Component)powerlineAccessPointSpawns[i]).transform.GetPositionAndRotation(ref pos, ref rot);
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/io/electric/generators/powergrid_powerline_io.static.prefab", pos, rot);
			if ((Object)(object)baseEntity == (Object)null)
			{
				Debug.LogError((object)"Failed to spawn entity from assets/prefabs/io/electric/generators/powergrid_powerline_io.static.prefab");
			}
			else
			{
				baseEntity.Spawn();
			}
		}
	}

	public static int GetNoOfPowergridAccessPoints()
	{
		return noOfAccessPoints;
	}

	public static int GetNoOfPowergridEntities()
	{
		return powergridEntities.Count;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!World.LoadedFromSave)
		{
			SpawnPowerlineAccessPoints();
		}
		InvokeRepeating(actionServerTick, 0f, 0f);
		lastFuseDeteriorationTickTime = Time.time;
		InvokeRandomized(actionServerFuseDeteriorationTick, 1f, 1f, 0.015f);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		CancelInvoke(actionServerTick);
		CancelInvoke(actionServerFuseDeteriorationTick);
		stageChangeWorkQueue.StopWorkQueue();
	}

	public override void Save(SaveInfo info)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.powergridManager = Pool.Get<PowergridManager>();
			info.msg.powergridManager.fuseItemIds = Pool.Get<List<ItemId>>();
			int i = 0;
			for (int count = insertedFuses.Count; i < count; i++)
			{
				info.msg.powergridManager.fuseItemIds.Add(insertedFuses[i].Fuse.uid);
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && info.msg.powergridManager?.fuseItemIds != null)
		{
			if (loadedFuseInsertionOrder == null)
			{
				loadedFuseInsertionOrder = new List<ItemId>();
			}
			loadedFuseInsertionOrder.AddRange(info.msg.powergridManager.fuseItemIds);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Server_RestoreFuseInsertionOrder();
	}

	private void Server_RestoreFuseInsertionOrder()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (loadedFuseInsertionOrder == null || loadedFuseInsertionOrder.Count == 0)
		{
			return;
		}
		PooledList<InsertedFuseEntry> val = Pool.Get<PooledList<InsertedFuseEntry>>();
		try
		{
			int i = 0;
			for (int count = loadedFuseInsertionOrder.Count; i < count; i++)
			{
				ItemId val2 = loadedFuseInsertionOrder[i];
				for (int j = 0; j < insertedFuses.Count; j++)
				{
					if (insertedFuses[j].Fuse.uid == val2)
					{
						((List<InsertedFuseEntry>)(object)val).Add(insertedFuses[j]);
						insertedFuses.RemoveAt(j);
						break;
					}
				}
			}
			int k = 0;
			for (int count2 = insertedFuses.Count; k < count2; k++)
			{
				((List<InsertedFuseEntry>)(object)val).Add(insertedFuses[k]);
			}
			insertedFuses.Clear();
			insertedFuses.AddRange((IEnumerable<InsertedFuseEntry>)val);
			loadedFuseInsertionOrder = null;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public int Server_GetFuseSocketsCount()
	{
		return fuseSocketsCount;
	}

	public int Server_GetPowerPlantInsertedFuses()
	{
		int num = currentInsertedFusesCount + Powergrid.simulatePowerPlantFuses;
		if (num < 0)
		{
			num = 0;
		}
		return num;
	}

	public int CalculateCurrentStage()
	{
		if (!Powergrid.enabled)
		{
			return 0;
		}
		return PowergridStageConfig.instance.GetStageForFuseCount(Server_GetPowerPlantInsertedFuses());
	}

	private void ServerTick()
	{
		using (TimeWarning.New("PowergridManager.ServerTick"))
		{
			if (!Powergrid.enabled || !Application.isServerStarted)
			{
				return;
			}
			int num = Server_GetPowerPlantInsertedFuses();
			if (num != previousInsertedFusesCount || !hasTickedOnce)
			{
				previousInsertedFusesCount = num;
				int num2 = CalculateCurrentStage();
				if (CurrentStage != num2 || !hasTickedOnce)
				{
					if (Interface.CallHook("OnPowergridStageChange", this, num2) != null)
					{
						return;
					}
					Facepunch.Rust.Analytics.Azure.OnPowerGridStageChanged(CurrentStage, num2);
					CurrentStage = num2;
					bool b = num2 > 0;
					using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags))
					{
						flagsUpdateScope.Set(Flags.On, b);
					}
					Interface.CallHook("OnPowergridStageChanged", this, num2);
					stageChangeWorkQueue.RestartWorkQueue();
					int i = 0;
					for (int count = fuseBoxes.Count; i < count; i++)
					{
						fuseBoxes[i].Server_OnPowergridStageChanged();
					}
				}
			}
			hasTickedOnce = true;
		}
	}

	private static bool Server_IsActiveFuse(InsertedFuseEntry entry)
	{
		if ((Object)(object)entry.FuseBox != (Object)null && !entry.FuseBox.IsDestroyed && entry.Fuse != null)
		{
			return entry.Fuse.hasCondition;
		}
		return false;
	}

	public static void Server_GatherInsertedFuses(List<Item> fuses)
	{
		int i = 0;
		for (int count = insertedFuses.Count; i < count; i++)
		{
			InsertedFuseEntry entry = insertedFuses[i];
			if (Server_IsActiveFuse(entry))
			{
				fuses.Add(entry.Fuse);
			}
		}
	}

	public static void Server_GatherFullDecayFuses(List<Item> fullDecayFuses)
	{
		int fuseFullDecayCount = Powergrid.fuseFullDecayCount;
		if (fuseFullDecayCount <= 0)
		{
			return;
		}
		int i = 0;
		for (int count = insertedFuses.Count; i < count; i++)
		{
			InsertedFuseEntry entry = insertedFuses[i];
			if (!Server_IsActiveFuse(entry))
			{
				continue;
			}
			float condition = entry.Fuse.condition;
			int num = fullDecayFuses.Count;
			for (int j = 0; j < fullDecayFuses.Count; j++)
			{
				if (condition < fullDecayFuses[j].condition)
				{
					num = j;
					break;
				}
			}
			if (num < fuseFullDecayCount)
			{
				fullDecayFuses.Insert(num, entry.Fuse);
				if (fullDecayFuses.Count > fuseFullDecayCount)
				{
					fullDecayFuses.RemoveAt(fullDecayFuses.Count - 1);
				}
			}
		}
	}

	public static float Server_GetSlowDecayRateScale(Item fuse)
	{
		float num = Mathf.Max(Powergrid.fuseSlowDecayFractionMin, 0f);
		float num2 = Mathf.Max(Powergrid.fuseSlowDecayFractionMax, num);
		if (num2 <= 0f)
		{
			return 0f;
		}
		if (num >= num2)
		{
			return num;
		}
		ulong value = fuse.uid.Value;
		uint num3 = (uint)(value ^ (value >> 32));
		return Mathf.Lerp(num, num2, SeedRandom.Wanghash01(ref num3));
	}

	private void ServerFuseDeteriorationTick()
	{
		using (TimeWarning.New("PowergridManager.ServerFuseDeteriorationTick"))
		{
			float time = Time.time;
			float deltaTime = time - lastFuseDeteriorationTickTime;
			lastFuseDeteriorationTickTime = time;
			if (!Powergrid.enabled || Powergrid.fuseLifespanSeconds <= 0f || !Application.isServerStarted)
			{
				return;
			}
			PooledList<Item> val = Pool.Get<PooledList<Item>>();
			try
			{
				Server_GatherFullDecayFuses((List<Item>)(object)val);
				for (int num = insertedFuses.Count - 1; num >= 0; num--)
				{
					if (num < insertedFuses.Count)
					{
						InsertedFuseEntry entry = insertedFuses[num];
						if (Server_IsActiveFuse(entry))
						{
							float decayRateScale = (((List<Item>)(object)val).Contains(entry.Fuse) ? 1f : Server_GetSlowDecayRateScale(entry.Fuse));
							entry.FuseBox.Server_DeteriorateFuse(entry.Fuse, deltaTime, decayRateScale);
						}
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
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
				Debug.Log((object)("SyncVar Writing: CurrentStage for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_CurrentStage);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: LastProcessedStateChangeSqrDistance for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_LastProcessedStateChangeSqrDistance);
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
				_ = __sync_CurrentStage;
				int _sync_CurrentStage = reader.Int32();
				__sync_CurrentStage = _sync_CurrentStage;
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_LastProcessedStateChangeSqrDistance;
				float _sync_LastProcessedStateChangeSqrDistance = reader.Float();
				__sync_LastProcessedStateChangeSqrDistance = _sync_LastProcessedStateChangeSqrDistance;
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
		if (!(propertyName == "CurrentStage"))
		{
			if (propertyName == "LastProcessedStateChangeSqrDistance")
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
		__sync_CurrentStage = 0;
		__sync_LastProcessedStateChangeSqrDistance = 0f;
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
