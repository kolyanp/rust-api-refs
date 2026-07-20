using System;
using System.Collections.Generic;
using System.Text;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;

public class CargoShip : BaseEntity, ILargeVehicleForProjectiles
{
	[Serializable]
	public struct CargoshipCamera
	{
		public Transform cameraPoint;

		public Transform cameraLookAtPoint;

		public string cameraIdentifier;
	}

	public struct HarborInfo
	{
		public BasePath harborPath;

		public Transform harborTransform;

		public int approachNode;
	}

	public int targetNodeIndex = -1;

	public GameObject wakeParent;

	public GameObjectRef scientistTurretPrefab;

	public Transform[] scientistSpawnPoints;

	public List<Transform> crateSpawns;

	public GameObjectRef lockedCratePrefab;

	public GameObjectRef militaryCratePrefab;

	public GameObjectRef eliteCratePrefab;

	public GameObjectRef junkCratePrefab;

	public Transform waterLine;

	public Transform rudder;

	public Transform propeller;

	public GameObjectRef escapeBoatPrefab;

	public GameObjectRef primitiveEscapeBoatPrefab;

	public Transform escapeBoatPoint;

	public GameObjectRef microphonePrefab;

	public Transform microphonePoint;

	public GameObjectRef speakerPrefab;

	public Transform[] speakerPoints;

	public GameObject radiation;

	public GameObjectRef mapMarkerEntityPrefab;

	public GameObject hornOrigin;

	public SoundDefinition hornDef;

	public CargoShipSounds cargoShipSounds;

	public GameObject[] layouts;

	public GameObjectRef playerTest;

	public Transform bowPoint;

	private uint layoutChoice;

	public const Flags IsDocked = Flags.Reserved1;

	public const Flags HasDocked = Flags.Reserved2;

	public const Flags DockedHarborIndex0 = Flags.Reserved3;

	public const Flags DockedHarborIndex1 = Flags.Reserved4;

	public const Flags Egressing = Flags.Reserved8;

	public GameObjectRef cctvCameraPrefab;

	public CargoshipCamera[] cctvCameras;

	[ServerVar]
	public static bool docking_debug = false;

	[ServerVar]
	public static bool should_dock = true;

	[ServerVar]
	public static float dock_time = 480f;

	[ServerVar]
	public static bool event_enabled = true;

	[ServerVar]
	public static float event_duration_minutes = 50f;

	[ServerVar]
	public static float egress_duration_minutes = 10f;

	[ServerVar]
	public static int loot_rounds = 3;

	[ServerVar]
	public static float loot_round_spacing_minutes = 10f;

	[ServerVar]
	public static bool refresh_loot_on_dock = true;

	[ServerVar]
	public static bool cargo_escape_boat_rhib = true;

	public static List<HarborInfo> harbors = new List<HarborInfo>();

	public int currentHarborApproachNode;

	public int harborIndex;

	public bool isDoingHarborApproach;

	private int dockCount;

	private bool shouldLookAhead;

	private float lifetime;

	private CargoShipContainerDestination[] containerDestinations;

	private HashSet<ulong> boardedPlayerIds = new HashSet<ulong>();

	public static bool hasCalculatedApproaches = false;

	public BaseEntity mapMarkerInstance;

	public Vector3 currentVelocity = Vector3.zero;

	public float currentThrottle;

	public float currentTurnSpeed;

	public float turnScale;

	public int lootRoundsPassed;

	private List<CCTV_RC> cameraEntities;

	public int hornCount;

	public float currentRadiation;

	public bool egressing;

	public BasePath harborApproachPath;

	public HarborProximityManager proxManager;

	private float lastSpeed = 0.3f;

	public bool IsShipDocked => HasFlag(Flags.Reserved1);

	public static int TotalAvailableHarborDockingPaths => harbors.Count;

	private bool HasFinishedDocking
	{
		get
		{
			if (!should_dock)
			{
				return true;
			}
			return dockCount == harbors.Count;
		}
	}

	private float EventTimeRemaining => event_duration_minutes * 60f - lifetime;

	[ServerVar]
	public static void egress(ConsoleSystem.Arg arg)
	{
		CargoShip[] array = Object.FindObjectsByType<CargoShip>((FindObjectsInactive)0, (FindObjectsSortMode)0);
		foreach (CargoShip cargoShip in array)
		{
			if (cargoShip.isServer && !cargoShip.egressing)
			{
				cargoShip.StartEgress();
			}
		}
	}

	public static List<Vector3> GetCargoApproachPath(int index)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (index >= TotalAvailableHarborDockingPaths)
		{
			return null;
		}
		CalculateHarborApproachNodes();
		List<Vector3> list = new List<Vector3>();
		list.Add(TerrainMeta.Path.OceanPatrolFar[harbors[index].approachNode]);
		foreach (BasePathNode node in harbors[index].harborPath.nodes)
		{
			list.Add(node.Position);
		}
		return list;
	}

	public override float GetNetworkTime()
	{
		return Time.fixedTime;
	}

	[ServerVar]
	public static void debug_info(ConsoleSystem.Arg arg)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Harbor Positions:");
		for (int i = 0; i < harbors.Count; i++)
		{
			stringBuilder.AppendLine(string.Format("harbor {0} is at {1}, {2}, {3}, approach index: {4}", new object[5]
			{
				i,
				harbors[i].harborTransform.position.x,
				harbors[i].harborTransform.position.y,
				harbors[i].harborTransform.position.z,
				harbors[i].approachNode
			}));
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar]
	public static void debug_cargo_status(ConsoleSystem.Arg arg)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is CargoShip cargoShip)
				{
					stringBuilder.AppendLine("Cargoship States:");
					stringBuilder.AppendLine("");
					stringBuilder.AppendLine($"Cargoship [{num}] dump");
					stringBuilder.AppendLine($"is at [{((Component)cargoShip).transform.position}]");
					stringBuilder.AppendLine($"dock count [{cargoShip.dockCount}]");
					stringBuilder.AppendLine($"is docked [{cargoShip.IsShipDocked}]");
					stringBuilder.AppendLine($"current approach node [{cargoShip.currentHarborApproachNode}]");
					stringBuilder.AppendLine($"is doing approach [{cargoShip.isDoingHarborApproach}]");
					stringBuilder.AppendLine($"chosen harbor is [{cargoShip.harborIndex}]");
					stringBuilder.AppendLine($"is egressing: {cargoShip.egressing}");
					arg.ReplyWith(stringBuilder.ToString());
					stringBuilder.Clear();
					num++;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.cargoShip == null)
		{
			return;
		}
		layoutChoice = info.msg.cargoShip.layout;
		if (!base.isServer)
		{
			return;
		}
		isDoingHarborApproach = info.msg.cargoShip.isDoingHarborApproach;
		harborIndex = info.msg.cargoShip.harborIndex;
		CalculateHarborApproachNodes();
		if (isDoingHarborApproach && HasFlag(Flags.Reserved1))
		{
			Invoke(LeaveHarbor, dock_time);
			Invoke(PreHarborLeaveHorn, dock_time - 60f);
		}
		currentHarborApproachNode = info.msg.cargoShip.currentHarborApproachNode;
		dockCount = info.msg.cargoShip.dockCount;
		shouldLookAhead = info.msg.cargoShip.shouldLookAhead;
		lifetime = info.msg.cargoShip.lifetime;
		if (info.msg.cargoShip.isEgressing)
		{
			StartEgress();
		}
		if (HasFinishedDocking)
		{
			Invoke(StartEgress, Mathf.Max(EventTimeRemaining, GetTimeRemainingFromCrates()));
		}
		if (HasFlag(Flags.Reserved1) && !IsInvoking(LeaveHarbor))
		{
			Invoke(LeaveHarbor, dock_time);
		}
		boardedPlayerIds.Clear();
		foreach (ulong playerId in info.msg.cargoShip.playerIds)
		{
			boardedPlayerIds.Add(playerId);
		}
	}

	public void RefreshActiveLayout()
	{
		for (int i = 0; i < layouts.Length; i++)
		{
			layouts[i].SetActive(layoutChoice == i);
		}
		if (base.isServer)
		{
			containerDestinations = ((Component)this).GetComponentsInChildren<CargoShipContainerDestination>();
		}
	}

	public static void RegisterHarbor(BasePath path, Transform tf)
	{
		harbors.Add(new HarborInfo
		{
			harborPath = path,
			harborTransform = tf
		});
		if (docking_debug)
		{
			Debug.Log((object)("Added " + ((Object)tf).name + " to harbor list"));
		}
	}

	public void TriggeredEventSpawn()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = TerrainMeta.RandomPointOffshore(avoidDeepSeaPortal: true, avoidDeepSea: true);
		val.y = WaterLevel.GetWaterSurface(val, waves: false, volumes: false);
		((Component)this).transform.position = val;
		if (should_dock)
		{
			CalculateHarborApproachNodes();
		}
		if (!event_enabled || event_duration_minutes == 0f)
		{
			Invoke(DelayedDestroy, 1f);
		}
	}

	public void TriggeredEventSpawnDockingTest(int index)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if (harbors.Count <= 0 || !should_dock)
		{
			TriggeredEventSpawn();
			Debug.Log((object)"No harbors registered.");
			return;
		}
		if (harbors.Count <= 0 || index > harbors.Count - 1)
		{
			Debug.Log((object)"Wrong harbor index or no harbors on map.");
			return;
		}
		CalculateHarborApproachNodes();
		if (harbors.Count > 0)
		{
			if (harbors != null)
			{
				int approachNode = harbors[index].approachNode;
				Vector3 val = TerrainMeta.Path.OceanPatrolFar[approachNode + 5];
				val.y = WaterLevel.GetWaterSurface(val, waves: false, volumes: false);
				((Component)this).transform.position = val;
				((Component)this).transform.LookAt(harbors[index].harborPath.nodes[0].Position);
			}
			if (!event_enabled || event_duration_minutes == 0f)
			{
				Invoke(DelayedDestroy, 1f);
			}
		}
	}

	private static void CalculateHarborApproachNodes()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (hasCalculatedApproaches)
		{
			return;
		}
		hasCalculatedApproaches = true;
		for (int i = 0; i < harbors.Count; i++)
		{
			HarborInfo value = harbors[i];
			float num = float.MaxValue;
			int num2 = -1;
			for (int j = 0; j < TerrainMeta.Path.OceanPatrolFar.Count; j++)
			{
				Vector3 val = TerrainMeta.Path.OceanPatrolFar[j];
				Vector3 position = value.harborPath.nodes[0].Position;
				float num3 = Vector3.Distance(val, position);
				_ = docking_debug;
				float num4 = num3;
				Vector3 val2 = Vector3.up * 3f;
				if (!GamePhysics.LineOfSightRadius(val + val2, position + val2, 1084293377, 3f))
				{
					num4 *= 20f;
				}
				if (num4 < num)
				{
					num = num4;
					num2 = j;
				}
			}
			if (num2 == -1)
			{
				Debug.LogWarning((object)"Cargo couldn't find harbor approach node. Are you sure ocean paths have been generated?");
				break;
			}
			value.approachNode = num2;
			harbors[i] = value;
		}
	}

	public void OnArrivedAtHarbor()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: true);
		List<Transform> list = Pool.Get<List<Transform>>();
		float num = Random.Range(dock_time * 0.05f, dock_time * 0.1f);
		Enumerator<HarborCraneContainerPickup> enumerator = HarborCraneContainerPickup.AllCranes.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				HarborCraneContainerPickup current = enumerator.Current;
				if ((Object)(object)current == (Object)null || current.isClient || current.Distance2D((BaseEntity)this) > 150f)
				{
					continue;
				}
				list.Clear();
				CargoShipContainerDestination[] array = containerDestinations;
				foreach (CargoShipContainerDestination cargoShipContainerDestination in array)
				{
					if (current.IsDestinationValidForCrane(cargoShipContainerDestination))
					{
						list.Add(((Component)cargoShipContainerDestination).transform);
					}
				}
				if (list.Count > 0)
				{
					current.AssignDestination(list, this, num);
					num += dock_time * Random.Range(0.1f, 0.15f);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Pool.FreeUnmanaged<Transform>(ref list);
		Invoke(PreHarborLeaveHorn, dock_time - 60f);
		if (refresh_loot_on_dock)
		{
			RespawnLoot();
		}
		if (harborIndex == 0)
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: true);
		}
		else if (harborIndex == 1)
		{
			flagsUpdateScope.Set(Flags.Reserved4, b: true);
		}
		Invoke(LeaveHarbor, dock_time);
		Interface.CallHook("OnCargoShipHarborArrived", this);
	}

	private void ClearAllHarborEntitiesOnShip()
	{
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		foreach (BaseEntity child in children)
		{
			if (child is CargoShipContainer)
			{
				list.Add(child);
			}
		}
		foreach (BaseEntity item in list)
		{
			item.Kill();
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	public void CreateMapMarker()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)mapMarkerInstance))
		{
			mapMarkerInstance.Kill();
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(mapMarkerEntityPrefab.resourcePath, Vector3.zero, Quaternion.identity);
		baseEntity.Spawn();
		baseEntity.SetParent(this);
		mapMarkerInstance = baseEntity;
	}

	public void DisableCollisionTest()
	{
	}

	public void SpawnCrate(string resourcePath)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (crateSpawns.Count == 0)
		{
			return;
		}
		int index = Random.Range(0, crateSpawns.Count);
		Vector3 position = crateSpawns[index].position;
		Quaternion rotation = crateSpawns[index].rotation;
		crateSpawns.Remove(crateSpawns[index]);
		BaseEntity baseEntity = GameManager.server.CreateEntity(resourcePath, position, rotation);
		if (Object.op_Implicit((Object)(object)baseEntity))
		{
			baseEntity.enableSaving = false;
			((Component)baseEntity).SendMessage("SetWasDropped", (SendMessageOptions)1);
			baseEntity.Spawn();
			baseEntity.SetParent(this, worldPositionStays: true);
			Rigidbody component = ((Component)baseEntity).GetComponent<Rigidbody>();
			if ((Object)(object)component != (Object)null)
			{
				component.isKinematic = true;
			}
		}
	}

	public void RespawnLoot()
	{
		if (Interface.CallHook("OnCargoShipSpawnCrate", this) == null)
		{
			InvokeRepeating(PlayHorn, 0f, 8f);
			SpawnCrate(lockedCratePrefab.resourcePath);
			SpawnCrate(eliteCratePrefab.resourcePath);
			for (int i = 0; i < 4; i++)
			{
				SpawnCrate(militaryCratePrefab.resourcePath);
			}
			for (int j = 0; j < 4; j++)
			{
				SpawnCrate(junkCratePrefab.resourcePath);
			}
			lootRoundsPassed++;
			if (lootRoundsPassed >= loot_rounds)
			{
				CancelInvoke(RespawnLoot);
			}
		}
	}

	public void SpawnSubEntities()
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		if (!Application.isLoadingSave)
		{
			string strPrefab = (cargo_escape_boat_rhib ? escapeBoatPrefab.resourcePath : primitiveEscapeBoatPrefab.resourcePath);
			BaseEntity baseEntity = GameManager.server.CreateEntity(strPrefab, escapeBoatPoint.position, escapeBoatPoint.rotation);
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				baseEntity.SetParent(this, worldPositionStays: true);
				baseEntity.Spawn();
				RHIB component = ((Component)baseEntity).GetComponent<RHIB>();
				component.SetToKinematic();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.AddFuel(50);
				}
			}
		}
		MicrophoneStand microphoneStand = GameManager.server.CreateEntity(microphonePrefab.resourcePath, microphonePoint.position, microphonePoint.rotation) as MicrophoneStand;
		if (Object.op_Implicit((Object)(object)microphoneStand))
		{
			microphoneStand.enableSaving = false;
			microphoneStand.SetParent(this, worldPositionStays: true);
			microphoneStand.Spawn();
			microphoneStand.SpawnChildEntity();
			IOEntity iOEntity = microphoneStand.ioEntity.Get(serverside: true);
			Transform[] array = speakerPoints;
			foreach (Transform val in array)
			{
				IOEntity iOEntity2 = GameManager.server.CreateEntity(speakerPrefab.resourcePath, val.position, val.rotation) as IOEntity;
				iOEntity2.enableSaving = false;
				iOEntity2.SetParent(this, worldPositionStays: true);
				iOEntity2.Spawn();
				iOEntity.outputs[0].connectedTo.Set(iOEntity2);
				iOEntity2.inputs[0].connectedTo.Set(iOEntity);
				iOEntity = iOEntity2;
			}
			microphoneStand.ioEntity.Get(serverside: true).MarkDirtyForceUpdateOutputs();
		}
		cameraEntities = Pool.Get<List<CCTV_RC>>();
		CargoshipCamera[] array2 = cctvCameras;
		for (int i = 0; i < array2.Length; i++)
		{
			CargoshipCamera cargoshipCamera = array2[i];
			CCTV_RC cCTV_RC = GameManager.server.CreateEntity(cctvCameraPrefab.resourcePath, cargoshipCamera.cameraPoint.position, cargoshipCamera.cameraPoint.rotation) as CCTV_RC;
			if (Object.op_Implicit((Object)(object)cCTV_RC))
			{
				cCTV_RC.enableSaving = false;
				cCTV_RC.SetParent(this, worldPositionStays: true);
				cCTV_RC.Spawn();
				cCTV_RC.rcIdentifier = cargoshipCamera.cameraIdentifier;
				cCTV_RC.SetLookingAtPoint(cargoshipCamera.cameraLookAtPoint.position);
				cameraEntities.Add(cCTV_RC);
			}
		}
	}

	internal override void DoServerDestroy()
	{
		Pool.FreeUnmanaged<CCTV_RC>(ref cameraEntities);
		base.DoServerDestroy();
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		base.OnChildAdded(child);
		if (!base.isServer)
		{
			return;
		}
		if (Application.isLoadingSave && child is RHIB rHIB)
		{
			Vector3 localPosition = ((Component)rHIB).transform.localPosition;
			Vector3 val = ((Component)this).transform.InverseTransformPoint(((Component)escapeBoatPoint).transform.position);
			if (Vector3.Distance(localPosition, val) < 1f)
			{
				rHIB.SetToKinematic();
			}
		}
		if (Application.isLoadingSave)
		{
			return;
		}
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		((Component)child).GetComponentsInChildren<BasePlayer>(list);
		foreach (BasePlayer item in list)
		{
			if (!item.IsBot && !item.IsNpc && item.IsConnected && boardedPlayerIds.Add(item.userID) && item.serverClan != null)
			{
				item.AddClanScore((ClanScoreEventType)9);
			}
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.cargoShip = Pool.Get<CargoShip>();
		info.msg.cargoShip.layout = layoutChoice;
		info.msg.cargoShip.currentHarborApproachNode = currentHarborApproachNode;
		info.msg.cargoShip.isDoingHarborApproach = isDoingHarborApproach;
		info.msg.cargoShip.dockCount = dockCount;
		info.msg.cargoShip.shouldLookAhead = shouldLookAhead;
		info.msg.cargoShip.isEgressing = egressing;
		info.msg.cargoShip.harborIndex = harborIndex;
		if (!info.forDisk)
		{
			return;
		}
		info.msg.cargoShip.playerIds = Pool.Get<List<ulong>>();
		foreach (ulong boardedPlayerId in boardedPlayerIds)
		{
			info.msg.cargoShip.playerIds.Add(boardedPlayerId);
		}
		info.msg.cargoShip.lifetime = lifetime;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		RefreshActiveLayout();
	}

	public void PlayHorn()
	{
		ClientRPC(RpcTarget.NetworkGroup("DoHornSound"));
		hornCount++;
		if (hornCount >= 3)
		{
			hornCount = 0;
			CancelInvoke(PlayHorn);
		}
	}

	public override void Spawn()
	{
		if (!Application.isLoadingSave)
		{
			layoutChoice = (uint)Random.Range(0, layouts.Length);
			SendNetworkUpdate();
			RefreshActiveLayout();
		}
		base.Spawn();
	}

	public override void ServerInit()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		CalculateHarborApproachNodes();
		Invoke(FindInitialNode, 2f);
		InvokeRepeating(BuildingCheck, 1f, 5f);
		InvokeRepeating(RespawnLoot, 10f, 60f * loot_round_spacing_minutes);
		Invoke(DisableCollisionTest, 10f);
		float waterSurface = WaterLevel.GetWaterSurface(((Component)this).transform.position, waves: false, volumes: false);
		Vector3 val = ((Component)this).transform.InverseTransformPoint(((Component)waterLine).transform.position);
		((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, waterSurface - val.y, ((Component)this).transform.position.z);
		SpawnSubEntities();
		if (HasFinishedDocking)
		{
			Invoke(StartEgress, Mathf.Max(EventTimeRemaining, 120f));
		}
		CreateMapMarker();
	}

	public void UpdateRadiation()
	{
		currentRadiation += 1f;
		TriggerRadiation[] componentsInChildren = radiation.GetComponentsInChildren<TriggerRadiation>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].RadiationAmountOverride = currentRadiation;
		}
	}

	public void StartEgress()
	{
		if (isDoingHarborApproach || egressing)
		{
			return;
		}
		egressing = true;
		if (Interface.CallHook("OnCargoShipEgress", this) == null)
		{
			CancelInvoke(PlayHorn);
			radiation.SetActive(true);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved8, b: true);
			}
			InvokeRepeating(UpdateRadiation, 10f, 1f);
			Invoke(DelayedDestroy, 60f * egress_duration_minutes);
		}
	}

	public void DelayedDestroy()
	{
		Kill();
	}

	public void FindInitialNode()
	{
		targetNodeIndex = GetClosestNodeToUs();
	}

	private int GetHackableCrateCount()
	{
		int num = 0;
		foreach (BaseEntity child in children)
		{
			if (child is HackableLockedCrate)
			{
				num++;
			}
		}
		return num;
	}

	public void BuildingCheck()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		Vis.Entities(WorldSpaceBounds(), list, 2162689, (QueryTriggerInteraction)2);
		foreach (BaseEntity item in list)
		{
			if (!(item is JunkPileWater junkPileWater))
			{
				if (item is DecayEntity decayEntity && (Object)(object)decayEntity.parentEntity.Get(serverside: true) != (Object)(object)this && decayEntity.isServer && decayEntity.IsAlive() && !decayEntity.AllowOnCargoShip && !PlayerBoat.IsChildOfFinishedPlayerBoat(decayEntity))
				{
					decayEntity.Kill(DestroyMode.Gib);
				}
			}
			else
			{
				junkPileWater.SinkAndDestroy();
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	public void FixedUpdate()
	{
		if (!base.isClient)
		{
			UpdateMovement();
			lifetime += Time.fixedDeltaTime;
		}
	}

	public void UpdateMovement()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (IsOceanPatrolPathAvailable() && IsValidTargetNode())
		{
			InitializeHarborApproach();
			Vector3 approachRotationNode = Vector3.zero;
			CalculateDesiredNodes(out var desiredMoveNode, out approachRotationNode);
			float num = 0f;
			num = CalculateDesiredThrottle(desiredMoveNode);
			UpdateShip(num, desiredMoveNode, approachRotationNode);
			UpdateCameraNetworkGroups();
			float num2 = (isDoingHarborApproach ? 8f : 80f);
			if (Vector3.Distance(((Component)this).transform.position, desiredMoveNode) < num2)
			{
				HandleNodeArrival(desiredMoveNode);
			}
			UpdateHarborApproachProgress();
		}
	}

	private void UpdateCameraNetworkGroups()
	{
		if (cameraEntities == null)
		{
			return;
		}
		foreach (CCTV_RC cameraEntity in cameraEntities)
		{
			if ((Object)(object)cameraEntity != (Object)null && !cameraEntity.IsDestroyed)
			{
				cameraEntity.UpdateNetworkGroup();
			}
		}
	}

	[ContextMenu("Break")]
	public void Break()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		CalculateDesiredNodes(out var desiredMoveNode, out var _);
		Vector3 val = ((Component)this).transform.position - desiredMoveNode;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		((Component)this).transform.forward = normalized;
		currentTurnSpeed = 0f;
	}

	private void UpdateShip(float desiredThrottle, Vector3 desiredWaypoint, Vector3 approachRotationNode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = desiredWaypoint - ((Component)this).transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		normalized.y = 0f;
		float num = Vector3.Dot(((Component)this).transform.right, normalized);
		float num2 = (isDoingHarborApproach ? 6.5f : 2.5f);
		float num3 = Mathf.InverseLerp(0.05f, 0.5f, Mathf.Abs(num));
		if (num3 == 0f && Vector3.Dot(normalized, -((Component)this).transform.forward) >= 0.95f)
		{
			num3 = 1f;
		}
		turnScale = Mathf.Lerp(turnScale, num3, Time.deltaTime * 0.2f);
		float num4 = ((!(num < 0f)) ? 1 : (-1));
		currentTurnSpeed = num2 * turnScale * num4;
		if (!isDoingHarborApproach)
		{
			((Component)this).transform.Rotate(Vector3.up, Time.deltaTime * currentTurnSpeed, (Space)0);
		}
		currentThrottle = Mathf.Lerp(currentThrottle, desiredThrottle, Time.deltaTime * 0.2f);
		currentVelocity = ((Component)this).transform.forward * (8f * currentThrottle);
		if (isDoingHarborApproach)
		{
			currentVelocity = normalized * currentThrottle * 5f;
			val = approachRotationNode - ((Component)this).transform.position;
			Vector3 normalized2 = ((Vector3)(ref val)).normalized;
			normalized2.y = 0f;
			if (normalized2 != Vector3.zero)
			{
				((Component)this).transform.rotation = Quaternion.Slerp(((Component)this).transform.rotation, Quaternion.LookRotation(normalized2), Time.deltaTime * 0.1f);
			}
		}
		if (HasFlag(Flags.Reserved1))
		{
			currentVelocity = Vector3.zero;
		}
		Transform transform = ((Component)this).transform;
		transform.position += currentVelocity * Time.deltaTime;
	}

	private void UpdateHarborApproachProgress()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		HarborProximityManager harborProximityManager = default(HarborProximityManager);
		if (isDoingHarborApproach && (Object)(object)harborApproachPath != (Object)null && ((Component)harborApproachPath).TryGetComponent<HarborProximityManager>(ref harborProximityManager))
		{
			float pathLength = harborApproachPath.GetPathLength();
			float pathProgress = harborApproachPath.GetPathProgress(((Component)this).transform.position);
			harborProximityManager.UpdateNormalisedState(Mathf.Clamp01(pathProgress / pathLength));
		}
	}

	private void InitializeHarborApproach(bool forceInit = false)
	{
		if (forceInit || harbors.Count > 0)
		{
			harborApproachPath = harbors[harborIndex].harborPath;
			proxManager = ((Component)harborApproachPath).GetComponent<HarborProximityManager>();
		}
	}

	private float CalculateDesiredThrottle(Vector3 desiredMoveNode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = desiredMoveNode - ((Component)this).transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		float num = Vector3.Dot(((Component)this).transform.forward, normalized);
		float num2 = Mathf.InverseLerp(0f, 1f, num);
		if (isDoingHarborApproach)
		{
			if (harborApproachPath.nodes[currentHarborApproachNode].maxVelocityOnApproach > 0f)
			{
				lastSpeed = harborApproachPath.nodes[currentHarborApproachNode].maxVelocityOnApproach;
			}
			num2 = Mathf.Clamp(num2, 0.1f, lastSpeed);
		}
		return num2;
	}

	private void CalculateDesiredNodes(out Vector3 desiredMoveNode, out Vector3 approachRotationNode)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		if (isDoingHarborApproach)
		{
			int index = (shouldLookAhead ? Mathf.Min(currentHarborApproachNode + 1, harborApproachPath.nodes.Count - 1) : currentHarborApproachNode);
			approachRotationNode = harborApproachPath.nodes[index].Position;
			desiredMoveNode = harborApproachPath.nodes[currentHarborApproachNode].Position;
			return;
		}
		desiredMoveNode = TerrainMeta.Path.OceanPatrolFar[targetNodeIndex];
		if (egressing)
		{
			Vector3 val = Vector3Ex.WithY(((Component)this).transform.position, 0f);
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			val = Vector3Ex.WithY(((Component)this).transform.forward, 0f);
			Vector3 normalized2 = ((Vector3)(ref val)).normalized;
			Vector3 val2 = ((Component)this).transform.position + Vector3.up * 5f;
			Ray[] array = new Ray[2];
			val = Quaternion.Euler(0f, -7f, 0f) * normalized2;
			array[0] = new Ray(val2, ((Vector3)(ref val)).normalized);
			val = Quaternion.Euler(0f, 7f, 0f) * normalized2;
			array[1] = new Ray(val2, ((Vector3)(ref val)).normalized);
			Ray[] array2 = (Ray[])(object)array;
			int num = 0;
			List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
			for (int i = 0; i < array2.Length; i++)
			{
				GamePhysics.TraceAll(array2[i], 10f, list, 600f, 262144, (QueryTriggerInteraction)2);
				bool flag = false;
				foreach (RaycastHit item in list)
				{
					RaycastHit current = item;
					BaseEntity entity = RaycastHitEx.GetEntity(current);
					if ((!((Object)(object)entity != (Object)null) || (!((Object)(object)entity == (Object)(object)this) && !entity.EqualNetID((BaseNetworkable)this))) && !(entity is CargoShip) && ((RaycastHit)(ref current)).collider.isTrigger && ((Component)((RaycastHit)(ref current)).collider).CompareTag("FerryAvoid"))
					{
						num = ((i != 0) ? 1 : (-1));
						flag = true;
						break;
					}
				}
				list.Clear();
				if (flag)
				{
					break;
				}
			}
			if (num != 0)
			{
				val = Quaternion.Euler(0f, -45f, 0f) * normalized2;
				Vector3 normalized3 = ((Vector3)(ref val)).normalized;
				val = Quaternion.Euler(0f, 45f, 0f) * normalized2;
				Vector3 normalized4 = ((Vector3)(ref val)).normalized;
				Vector3 val3 = ((num == -1) ? normalized4 : normalized3);
				desiredMoveNode = ((Component)this).transform.position + val3 * 10000f;
			}
			else
			{
				desiredMoveNode = ((Component)this).transform.position + normalized * 10000f;
			}
			Pool.FreeUnmanaged<RaycastHit>(ref list);
			val = ((Component)this).transform.position;
			if (((Vector3)(ref val)).sqrMagnitude > 100000000f)
			{
				Debug.LogWarning((object)"Immediately deleting cargo as it is a long way out of bounds");
				Kill();
			}
			if (DeepSeaManager.IsInsideDeepSea(((Component)this).transform.position))
			{
				Debug.LogWarning((object)"Immediately deleting cargo as it reached the deep sea");
				Kill();
			}
		}
		approachRotationNode = Vector3.zero;
	}

	private void HandleNodeArrival(Vector3 waypointPosition)
	{
		if (isDoingHarborApproach)
		{
			if (currentHarborApproachNode == harborApproachPath.nodes.Count - 1)
			{
				EndHarborApproach();
			}
			else
			{
				AdvanceHarborApproach();
			}
			return;
		}
		targetNodeIndex = (targetNodeIndex - 1 + TerrainMeta.Path.OceanPatrolFar.Count) % TerrainMeta.Path.OceanPatrolFar.Count;
		if (HasFinishedDocking)
		{
			return;
		}
		for (int i = 0; i < harbors.Count; i++)
		{
			HarborInfo harborInfo = harbors[i];
			if ((Object)(object)harborInfo.harborPath != (Object)null && harborInfo.approachNode == targetNodeIndex)
			{
				CargoNotifier component = ((Component)harborInfo.harborPath).GetComponent<CargoNotifier>();
				harborApproachPath = harborInfo.harborPath;
				harborIndex = i;
				if ((Object)(object)component != (Object)null)
				{
					StartHarborApproach(component);
					break;
				}
			}
		}
	}

	public void StartHarborApproach(CargoNotifier cn)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnCargoShipHarborApproach", this, cn) != null)
		{
			return;
		}
		PlayHorn();
		isDoingHarborApproach = true;
		dockCount++;
		shouldLookAhead = false;
		if ((Object)(object)proxManager != (Object)null)
		{
			proxManager.StartMovement();
		}
		ClearAllHarborEntitiesOnShip();
		Enumerator<HarborCraneContainerPickup> enumerator = HarborCraneContainerPickup.AllCranes.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				HarborCraneContainerPickup current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && !current.isClient && !(current.Distance2D(harborApproachPath.nodes[harborApproachPath.nodes.Count / 2].Position) > 150f))
				{
					current.ReplenishContainers();
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private float GetTimeRemainingFromCrates()
	{
		float requiredHackSeconds = HackableLockedCrate.requiredHackSeconds;
		if (GetHackableCrateCount() != 0)
		{
			return requiredHackSeconds + requiredHackSeconds * 0.3f;
		}
		return 120f;
	}

	private void EndHarborApproach()
	{
		PlayHorn();
		isDoingHarborApproach = false;
		currentHarborApproachNode = 0;
		FindInitialNode();
		if ((Object)(object)proxManager != (Object)null)
		{
			proxManager.EndMovement();
		}
		if (HasFinishedDocking)
		{
			if (docking_debug)
			{
				Debug.Log((object)$"Finished all docking: {EventTimeRemaining}s left in event");
			}
			Invoke(StartEgress, Mathf.Max(EventTimeRemaining, GetTimeRemainingFromCrates()));
		}
	}

	private void AdvanceHarborApproach()
	{
		if (currentHarborApproachNode + 1 < harborApproachPath.nodes.Count)
		{
			currentHarborApproachNode++;
		}
		if (!shouldLookAhead)
		{
			shouldLookAhead = true;
		}
		if (harborApproachPath.nodes[currentHarborApproachNode].maxVelocityOnApproach == 0f)
		{
			OnArrivedAtHarbor();
		}
	}

	private bool IsOceanPatrolPathAvailable()
	{
		if (TerrainMeta.Path.OceanPatrolFar != null)
		{
			return TerrainMeta.Path.OceanPatrolFar.Count > 0;
		}
		return false;
	}

	private bool IsValidTargetNode()
	{
		return targetNodeIndex != -1;
	}

	private void PreHarborLeaveHorn()
	{
		PlayHorn();
	}

	private void LeaveHarbor()
	{
		if (docking_debug)
		{
			Debug.Log((object)"Cargo is leaving harbor.");
		}
		PlayHorn();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved1, b: false);
			flagsUpdateScope.Set(Flags.Reserved2, b: true);
		}
		currentHarborApproachNode++;
		Interface.CallHook("OnCargoShipHarborLeave", this);
	}

	public int GetClosestNodeToUs()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		int num = -1;
		float num2 = float.PositiveInfinity;
		int num3 = -1;
		float num4 = float.PositiveInfinity;
		Vector3 position = ((Component)this).transform.position;
		for (int i = 0; i < TerrainMeta.Path.OceanPatrolFar.Count; i++)
		{
			Vector3 val = TerrainMeta.Path.OceanPatrolFar[i];
			Vector3 val2 = val - position;
			float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
			if (sqrMagnitude < num2)
			{
				num2 = sqrMagnitude;
				num = i;
			}
			if (!SegmentIntersectsOilRig(position, val) && sqrMagnitude < num4)
			{
				num4 = sqrMagnitude;
				num3 = i;
			}
		}
		if (num3 == -1)
		{
			if (num == -1)
			{
				return 0;
			}
			return num;
		}
		return num3;
	}

	private bool SegmentIntersectsOilRig(Vector3 from, Vector3 to)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Distance(from, to);
		if (num <= 0.1f)
		{
			return false;
		}
		Vector3 val = to - from;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 val2 = from + Vector3.up * 5f;
		List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
		GamePhysics.TraceAll(new Ray(val2, normalized), 10f, list, num + 5f, 262144, (QueryTriggerInteraction)2);
		bool result = false;
		foreach (RaycastHit item in list)
		{
			RaycastHit current = item;
			BaseEntity entity = RaycastHitEx.GetEntity(current);
			if ((!((Object)(object)entity != (Object)null) || (!((Object)(object)entity == (Object)(object)this) && !entity.EqualNetID((BaseNetworkable)this))) && ((RaycastHit)(ref current)).collider.isTrigger && ((Component)((RaycastHit)(ref current)).collider).CompareTag("FerryAvoid"))
			{
				result = true;
				break;
			}
		}
		Pool.FreeUnmanaged<RaycastHit>(ref list);
		return result;
	}

	public override Vector3 GetLocalVelocityServer()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return currentVelocity;
	}

	public override Quaternion GetAngularVelocityServer()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Euler(0f, currentTurnSpeed, 0f);
	}

	public override float InheritedVelocityScale()
	{
		return 1f;
	}

	public override bool BlocksWaterFor(BasePlayer player)
	{
		return true;
	}

	public override float AntiHackVelocity()
	{
		return 8f;
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	public override bool ForceDeployableSetParent()
	{
		return true;
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CargoShip.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}
}
