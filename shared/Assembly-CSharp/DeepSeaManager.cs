using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Network.Visibility;
using Oxide.Core;
using Prefabs.Misc;
using ProtoBuf;
using Rust;
using Rust.Ai.Gen2;
using UnityEngine;
using UnityEngine.Rendering;

public class DeepSeaManager : PointEntity<DeepSeaManager>
{
	[Serializable]
	public struct WipeStep
	{
		public float timeLeft;

		public Phrase message;

		public bool triggerFloatingCityAlarm;

		public GameObjectRef alarmEffect;
	}

	private struct PlacedPoint
	{
		public Vector2 pos;

		public float radius;

		public PlacedPoint(Vector2 pos, float radius)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			this.pos = pos;
			this.radius = radius;
		}
	}

	[NonSerialized]
	public CardinalDirection ExitPortalDirection;

	public GameObjectRef PortalPrefab;

	public GameObjectRef MainIslandBillboardPrefab;

	public GameObjectRef DeepSeaBillboardPrefab;

	public List<BaseEntity> vehicleWhitelist;

	public static List<DeepSeaPortal> ServerPortals;

	public static List<DeepSeaPortal> ClientPortals;

	public static Transform PortalEntranceTransform;

	public static Transform PortalExitTransform;

	public static OBB PortalEntranceBounds;

	public static OBB PortalExitBounds;

	[NonSerialized]
	public static ListHashSet<IslandBillboard> ServerBillboards;

	private readonly HashSet<uint> vehiclePrefabWhitelist = new HashSet<uint>();

	[SerializeField]
	private WipeStep[] wipeSteps;

	public static Phrase AboutToClose_Phrase;

	public static Phrase BoatNotStrongEnough_ToDeepSea_Phrase;

	public static Phrase BoatNotStrongEnough_ToMainLand_Phrase;

	public static Phrase NeedBoat_ToDeepSea_Phrase;

	public static Phrase NeedBoat_ToMainLand_Phrase;

	public static Phrase UnauthorizedVehicle_Phrase;

	private Transform serverVolumesParent;

	private int nextWipeStepIndex;

	public const string ACHIEVEMENT_ENTER_DEEP_SEA_NAME = "ENTER_DEEP_SEA";

	private readonly List<ulong> foodTollPaid = new List<ulong>();

	public GameObjectRef[] islandRefs;

	public GameObjectRef[] floatingCityRefs;

	public GameObjectRef[] ghostShipRefs;

	public Material SeaFloorMaterial;

	[NonSerialized]
	public static ListHashSet<DeepSeaIsland> ServerIslands;

	[NonSerialized]
	public static ListHashSet<GhostShip> ServerGhostShips;

	[NonSerialized]
	public static ListHashSet<DeepSeaFloatingCity> ServerFloatingCities;

	[NonSerialized]
	public static ListHashSet<RHIB> ServerRHIBS;

	public static WaitForSeconds WaitSpawnGroupInterval;

	public static WaitForSeconds WaitEntitySpawnInterval;

	public static WaitForSeconds WaitNavMeshInterval;

	private static readonly List<PlacedPoint> placedPoints;

	private List<int>[,] grid;

	private int gridSizeX;

	private int gridSizeY;

	private float cellSize;

	private Queue<GameObjectRef> shuffledPrefabs;

	public AnimationCurve EndWipeProgressToWeatherLerp;

	public AnimationCurve EndWipeRadiationCurve;

	public static Bounds DeepSeaBounds;

	public static float SeaFloorDepth;

	private const float RadVolumeWidth = 250f;

	private Transform sharedCollidersParent;

	private TriggerRadiation wipeRadiationVolume;

	public const Flags Flag_AboutToClose = Flags.Reserved1;

	public const Flags Flag_AlreadyOpenedOnce = Flags.Reserved2;

	private float __sync_TimeToWipe;

	private float __sync_TimeToNextOpening;

	private int __sync_CurrentEntrancePortalDirection;

	public CardinalDirection EntrancePortalDirection => (CardinalDirection)CurrentEntrancePortalDirection;

	[Sync(Autosave = true)]
	public float TimeToWipe
	{
		[CompilerGenerated]
		get
		{
			return __sync_TimeToWipe;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_TimeToWipe, value))
			{
				__sync_TimeToWipe = value;
				byte nameID = __GetWeaverID("TimeToWipe");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public float TimeToNextOpening
	{
		[CompilerGenerated]
		get
		{
			return __sync_TimeToNextOpening;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_TimeToNextOpening, value))
			{
				__sync_TimeToNextOpening = value;
				byte nameID = __GetWeaverID("TimeToNextOpening");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public int CurrentEntrancePortalDirection
	{
		[CompilerGenerated]
		get
		{
			return __sync_CurrentEntrancePortalDirection;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_CurrentEntrancePortalDirection, value))
			{
				__sync_CurrentEntrancePortalDirection = value;
				byte nameID = __GetWeaverID("CurrentEntrancePortalDirection");
				QueueSyncVar(nameID);
			}
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("DeepSeaManager.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static float GetPortalLerp(BaseNetworkable entity, DeepSeaPortal.PortalModeEnum portalMode)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)entity == (Object)null)
		{
			return 0f;
		}
		if (portalMode == DeepSeaPortal.PortalModeEnum.None)
		{
			return 0f;
		}
		if (!IsInsidePortal(entity, portalMode))
		{
			return 0f;
		}
		Transform val = ((portalMode == DeepSeaPortal.PortalModeEnum.Entrance) ? PortalEntranceTransform : PortalExitTransform);
		if ((Object)(object)val == (Object)null)
		{
			return 0f;
		}
		Vector3 val2 = val.InverseTransformPoint(((Component)entity).transform.position);
		return Mathf.Clamp01(Mathf.InverseLerp(-0.5f, 0.5f, val2.z));
	}

	public static bool IsInsidePortal(BaseNetworkable entity, DeepSeaPortal.PortalModeEnum portalMode)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return IsInsidePortal(((Component)entity).transform.position, portalMode);
	}

	public static bool IsInsidePortal(Vector3 position, DeepSeaPortal.PortalModeEnum portalMode)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (portalMode == DeepSeaPortal.PortalModeEnum.Entrance)
		{
			return ((OBB)(ref PortalEntranceBounds)).Contains(position);
		}
		return ((OBB)(ref PortalExitBounds)).Contains(position);
	}

	public static bool IsInsideAnyPortal(Vector3 position, DeepSeaPortal.PortalModeEnum portalMode, out DeepSeaPortal deepSeaPortal)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		deepSeaPortal = null;
		foreach (DeepSeaPortal serverPortal in ServerPortals)
		{
			if (serverPortal.PortalMode == portalMode)
			{
				OBB val = serverPortal.WorldSpaceBounds();
				if (((OBB)(ref val)).Contains(position))
				{
					deepSeaPortal = serverPortal;
					return true;
				}
			}
		}
		return false;
	}

	public void GeneratePortalDirections()
	{
		CardinalDirection cardinalDirection = PickNewEntranceDirection();
		ExitPortalDirection = cardinalDirection.Opposite();
		CurrentEntrancePortalDirection = (int)cardinalDirection;
		CardinalDirection PickNewEntranceDirection()
		{
			if (DeepSea.forceEntrancePortalDirection > 0)
			{
				return (CardinalDirection)Mathf.Clamp(DeepSea.forceEntrancePortalDirection, 1, 4);
			}
			List<CardinalDirection> list = new List<CardinalDirection>();
			list.Add(CardinalDirection.North);
			list.Add(CardinalDirection.East);
			list.Add(CardinalDirection.South);
			list.Add(CardinalDirection.West);
			list.Remove((CardinalDirection)CurrentEntrancePortalDirection);
			return ListEx.GetRandom<CardinalDirection>(list);
		}
	}

	public void SpawnEntrancePortals()
	{
		if (PortalPrefab.isValid)
		{
			CardinalDirection[] array = new CardinalDirection[4]
			{
				CardinalDirection.North,
				CardinalDirection.East,
				CardinalDirection.South,
				CardinalDirection.West
			};
			foreach (CardinalDirection direction in array)
			{
				SpawnEntrancePortal(direction);
			}
			Log("Spawned portals");
		}
		void SpawnEntrancePortal(CardinalDirection cardinalDirection)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			float depth = 300f;
			Bounds entrancePortalBounds = GetEntrancePortalBounds(cardinalDirection, depth);
			Quaternion portalDirection = GetPortalDirection(cardinalDirection);
			DeepSeaPortal obj = GameManager.server.CreateEntity(PortalPrefab.resourcePath, ((Bounds)(ref entrancePortalBounds)).center, portalDirection) as DeepSeaPortal;
			((Component)obj).transform.localScale = ((Bounds)(ref entrancePortalBounds)).size;
			obj.PortalMode = DeepSeaPortal.PortalModeEnum.Entrance;
			obj.PortalDirection = cardinalDirection;
			obj.Spawn();
		}
	}

	public void ActivateEntrancePortal(CardinalDirection direction)
	{
		DeactivateAllEntrancePortals();
		foreach (DeepSeaPortal serverPortal in ServerPortals)
		{
			if (serverPortal.PortalMode == DeepSeaPortal.PortalModeEnum.Entrance && serverPortal.PortalDirection == direction)
			{
				using (FlagsUpdateScope flagsUpdateScope = serverPortal.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope.Set(Flags.Open, b: true);
				}
				serverPortal.InitBounds();
				Log($"Activated entrance portal facing {direction}");
				return;
			}
		}
		Debug.LogError((object)$"DeepSea: No entrance portal found for direction {direction}");
	}

	public void DeactivateAllEntrancePortals()
	{
		foreach (DeepSeaPortal serverPortal in ServerPortals)
		{
			if (serverPortal.PortalMode == DeepSeaPortal.PortalModeEnum.Entrance && serverPortal.HasFlag(Flags.Open))
			{
				using FlagsUpdateScope flagsUpdateScope = serverPortal.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
				flagsUpdateScope.Set(Flags.Open, b: false);
			}
		}
	}

	public void SpawnExitPortal()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (ExitPortalDirection == CardinalDirection.None)
		{
			ExitPortalDirection = EntrancePortalDirection.Opposite();
		}
		float num = 300f;
		float offset = num / 2f;
		Bounds exitPortalBounds = GetExitPortalBounds(ExitPortalDirection, num, 300f, offset);
		Quaternion portalDirection = GetPortalDirection(ExitPortalDirection);
		Vector3 pos = ((Bounds)(ref exitPortalBounds)).center + ((Component)this).transform.position;
		DeepSeaPortal obj = GameManager.server.CreateEntity(PortalPrefab.resourcePath, pos, portalDirection) as DeepSeaPortal;
		((Component)obj).transform.localScale = ((Bounds)(ref exitPortalBounds)).size;
		obj.PortalMode = DeepSeaPortal.PortalModeEnum.Exit;
		obj.PortalDirection = ExitPortalDirection;
		obj.Spawn();
	}

	private void KillExitPortal()
	{
		DeepSeaPortal[] array = ServerPortals.ToArray();
		foreach (DeepSeaPortal deepSeaPortal in array)
		{
			if ((Object)(object)deepSeaPortal != (Object)null && deepSeaPortal.PortalMode == DeepSeaPortal.PortalModeEnum.Exit)
			{
				deepSeaPortal.Kill();
				break;
			}
		}
	}

	public void KillAllPortals()
	{
		if (ServerPortals == null || ServerPortals.Count == 0)
		{
			return;
		}
		DeepSeaPortal[] array = ServerPortals.ToArray();
		foreach (DeepSeaPortal deepSeaPortal in array)
		{
			if ((Object)(object)deepSeaPortal != (Object)null)
			{
				deepSeaPortal.Kill();
			}
		}
		ServerPortals.Clear();
	}

	private void SpawnBillboards()
	{
		KillAllBillboards();
		SpawnDeepSeaBillboards();
		SpawnMainIslandBillboard();
	}

	private void KillAllBillboards()
	{
		if (ServerBillboards == null || ServerBillboards.Count == 0)
		{
			return;
		}
		IslandBillboard[] array = ((IEnumerable<IslandBillboard>)ServerBillboards).ToArray();
		foreach (IslandBillboard islandBillboard in array)
		{
			if ((Object)(object)islandBillboard != (Object)null)
			{
				islandBillboard.Kill();
			}
		}
		ServerBillboards.Clear();
	}

	public void SpawnDeepSeaBillboards(int count = 5)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = PortalEntranceTransform.position;
		float x = PortalEntranceBounds.extents.x;
		Quaternion portalDirection = GetPortalDirection(ExitPortalDirection);
		Vector3 val = portalDirection * Vector3.back;
		Vector3 val2 = portalDirection * Vector3.right;
		float num = x * 0.3f;
		float num2 = x * 0.2f;
		List<Vector3> list = Pool.Get<List<Vector3>>();
		for (int i = 0; i < count; i++)
		{
			Vector3 val3 = Vector3.zero;
			Vector3 lastCandidate = Vector3.zero;
			for (int j = 0; j < 20; j++)
			{
				float num3 = Random.Range(0f - num, num);
				float num4 = Random.Range(0f - num2, num2);
				lastCandidate = position + val * 2000f + val2 * num3 + portalDirection * Vector3.forward * num4;
				if (!list.Any(delegate(Vector3 p)
				{
					//IL_0000: Unknown result type (might be due to invalid IL or missing references)
					//IL_0002: Unknown result type (might be due to invalid IL or missing references)
					return Vector3.Distance(p, lastCandidate) < 250f;
				}))
				{
					val3 = lastCandidate;
					break;
				}
			}
			if (val3 == Vector3.zero)
			{
				val3 = lastCandidate;
			}
			list.Add(val3);
			(GameManager.server.CreateEntity(DeepSeaBillboardPrefab.resourcePath, val3, portalDirection) as IslandBillboard).Spawn();
		}
		Pool.FreeUnmanaged<Vector3>(ref list);
	}

	public void SpawnMainIslandBillboard()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = PortalExitTransform.position;
		Quaternion portalDirection = GetPortalDirection(ExitPortalDirection);
		Vector3 val = portalDirection * Vector3.back;
		Vector3 pos = position + val * -2000f;
		(GameManager.server.CreateEntity(MainIslandBillboardPrefab.resourcePath, pos, portalDirection) as IslandBillboard).Spawn();
	}

	public Bounds GetEntrancePortalBounds(CardinalDirection direction, float depth, float height = 300f, float offset = 0f)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		float num = 60f;
		Vector3 extents = ((Bounds)(ref SingletonComponent<ValidBounds>.Instance.worldBounds)).extents;
		Vector3 val = TerrainMeta.MarginSize / 2f;
		float num2 = Mathf.Min(Mathf.Min(extents.x, extents.z), Mathf.Min(val.x, val.z));
		float num3 = vehicle.world_boundary_force_start_distance - vehicle.world_boundary_force_offset;
		float num4 = num2 - num3 - depth / 2f - num;
		float num5 = num2 * 2f;
		float num6 = Mathf.Min(TerrainMeta.Size.x / 2f + DeepSea.island_portal_terrain_distance, num4);
		num6 += offset;
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(num5, height, depth);
		return (Bounds)(direction switch
		{
			CardinalDirection.North => new Bounds(new Vector3(0f, 0f, num6), val2), 
			CardinalDirection.South => new Bounds(new Vector3(0f, 0f, 0f - num6), val2), 
			CardinalDirection.East => new Bounds(new Vector3(num6, 0f, 0f), val2), 
			CardinalDirection.West => new Bounds(new Vector3(0f - num6, 0f, 0f), val2), 
			_ => default(Bounds), 
		});
	}

	public Bounds GetExitPortalBounds(CardinalDirection direction, float depth, float height = 300f, float offset = 0f)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		return (Bounds)(direction switch
		{
			CardinalDirection.North => new Bounds(new Vector3(0f, 0f, ((Bounds)(ref DeepSeaBounds)).size.z / 2f - offset), new Vector3(((Bounds)(ref DeepSeaBounds)).size.x, height, depth)), 
			CardinalDirection.South => new Bounds(new Vector3(0f, 0f, ((Bounds)(ref DeepSeaBounds)).size.z / -2f + offset), new Vector3(((Bounds)(ref DeepSeaBounds)).size.x, height, depth)), 
			CardinalDirection.East => new Bounds(new Vector3(((Bounds)(ref DeepSeaBounds)).size.x / 2f - offset, 0f, 0f), new Vector3(((Bounds)(ref DeepSeaBounds)).size.z, height, depth)), 
			CardinalDirection.West => new Bounds(new Vector3(((Bounds)(ref DeepSeaBounds)).size.x / -2f + offset, 0f, 0f), new Vector3(((Bounds)(ref DeepSeaBounds)).size.z, height, depth)), 
			_ => default(Bounds), 
		});
	}

	private Quaternion GetPortalDirection(CardinalDirection direction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.LookRotation(direction.ToVectorDirection(), Vector3.up);
	}

	public Vector3 GetDeepSeaEntrancePosition(BaseEntity entity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = SetToOceanHeight(((Bounds)(ref DeepSeaBounds)).center);
		if (!FindPortals(out var entrancePortal, out var exitPortal))
		{
			Debug.LogWarning((object)"No entrance/exit portals found in deep sea, defaulting to center of deep sea");
			return val;
		}
		Vector3 normalizedPosition = ConvertInputPositionToOutputPosition(entrancePortal, exitPortal, ((Component)entity).transform.position);
		OBB val2 = exitPortal.WorldSpaceBounds();
		if (FindUnoccupiedPortalSpawnLocation(normalizedPosition, ((OBB)(ref val2)).ToBounds(), val, entity.bounds, out var spawnPos))
		{
			return spawnPos;
		}
		return ((Component)exitPortal).transform.position;
	}

	public Vector3 GetDeepSeaExitPosition(BaseEntity entity)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (!FindPortals(out var entrancePortal, out var exitPortal))
		{
			Debug.LogWarning((object)"No entrance/exit portals found in deep sea, defaulting to spawn beach");
			return SpawnHandler.GetSpawnPoint()?.pos ?? Vector3.zero;
		}
		Vector3 val = ConvertInputPositionToOutputPosition(exitPortal, entrancePortal, ((Component)entity).transform.position);
		Vector3 centerOfArea = SetToOceanHeight(TerrainMeta.Center);
		OBB val2 = entrancePortal.WorldSpaceBounds();
		Bounds portalWorldBounds = ((OBB)(ref val2)).ToBounds();
		Vector3 right = ((Component)entrancePortal).transform.right;
		Vector3 position = ((Component)entrancePortal).transform.position;
		float num = ((Vector3)(ref position)).magnitude - ((Component)entrancePortal).transform.localScale.z * 0.5f - 50f;
		float num2 = Mathf.Max(0f, ((Component)entrancePortal).transform.localScale.x - num * 2f);
		((Bounds)(ref portalWorldBounds)).Expand(-new Vector3(Mathf.Abs(right.x) * num2, 0f, Mathf.Abs(right.z) * num2));
		val = ((Bounds)(ref portalWorldBounds)).ClosestPoint(val);
		if (FindUnoccupiedPortalSpawnLocation(val, portalWorldBounds, centerOfArea, entity.bounds, out var spawnPos))
		{
			return spawnPos;
		}
		return ((Component)entrancePortal).transform.position;
	}

	public bool FindPortals(out DeepSeaPortal entrancePortal, out DeepSeaPortal exitPortal)
	{
		entrancePortal = null;
		exitPortal = null;
		foreach (DeepSeaPortal serverPortal in ServerPortals)
		{
			if (serverPortal.PortalMode == DeepSeaPortal.PortalModeEnum.Entrance && serverPortal.HasFlag(Flags.Open))
			{
				entrancePortal = serverPortal;
			}
			else if (serverPortal.PortalMode == DeepSeaPortal.PortalModeEnum.Exit)
			{
				exitPortal = serverPortal;
			}
		}
		if ((Object)(object)entrancePortal != (Object)null)
		{
			return (Object)(object)exitPortal != (Object)null;
		}
		return false;
	}

	private Vector3 ConvertInputPositionToOutputPosition(DeepSeaPortal inputPortal, DeepSeaPortal outputPortal, Vector3 worldPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)inputPortal).transform.InverseTransformPoint(worldPosition);
		val.x *= -1f;
		Vector3 val2 = ((Component)outputPortal).transform.TransformPoint(val);
		OBB val3 = outputPortal.WorldSpaceBounds();
		return ((OBB)(ref val3)).ClosestPoint(val2);
	}

	private Vector3 AdjustPortalExitPosition(Vector3 position, Vector3 centerOfArea, float distanceToMove = 20f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		position = SetToOceanHeight(position);
		return Vector3.MoveTowards(position, centerOfArea, distanceToMove);
	}

	private Vector3 SetToOceanHeight(Vector3 position)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		position.y = 0f;
		return position;
	}

	private bool FindUnoccupiedPortalSpawnLocation(Vector3 normalizedPosition, Bounds portalWorldBounds, Vector3 centerOfArea, Bounds entityBounds, out Vector3 spawnPos, float distanceToMove = 20f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = AdjustPortalExitPosition(normalizedPosition, centerOfArea, distanceToMove);
		Vector3 val2 = AdjustPortalExitPosition(((Bounds)(ref portalWorldBounds)).min, centerOfArea, distanceToMove);
		Vector3 val3 = AdjustPortalExitPosition(((Bounds)(ref portalWorldBounds)).max, centerOfArea, distanceToMove);
		float duration = 30f;
		if (DeepSea.debug_portal_spawnattempts)
		{
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(val, duration, Color.yellow, 10f));
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Text(val, duration, Color.yellow, "Start"));
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(val2, duration, Color.yellow, 10f));
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Text(val2, duration, Color.yellow, "Min"));
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(val3, duration, Color.yellow, 10f));
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Text(val3, duration, Color.yellow, "Max"));
		}
		Vector3 extents = ((Bounds)(ref entityBounds)).extents;
		float magnitude = ((Vector3)(ref extents)).magnitude;
		if (TestPortalSpawnLocations(val, val2, magnitude, out spawnPos))
		{
			return true;
		}
		if (TestPortalSpawnLocations(val, val3, magnitude, out spawnPos))
		{
			return true;
		}
		spawnPos = default(Vector3);
		return false;
	}

	private bool TestPortalSpawnLocations(Vector3 startPos, Vector3 endPos, float radius, out Vector3 spawnPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = startPos;
		int num = 0;
		while (Vector3.Distance(val, endPos) > 1f && num++ < 100)
		{
			bool flag = IsValidPortalSpawnLocation(val, radius);
			if (DeepSea.debug_portal_spawnattempts)
			{
				Color color = (flag ? Color.green : Color.red);
				float duration = 30f;
				ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(val, duration, color, radius));
				ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Text(val, duration, color, num.ToString()));
			}
			if (flag)
			{
				spawnPos = val;
				return true;
			}
			val = Vector3.MoveTowards(val, endPos, radius);
		}
		spawnPos = default(Vector3);
		return false;
	}

	public bool IsValidPortalSpawnLocation(Vector3 position, float radius, BasePlayer ignorePlayer = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Collider> val = Pool.Get<PooledList<Collider>>();
		try
		{
			GamePhysics.OverlapSphere(position, radius, (List<Collider>)(object)val, 1218511105, (QueryTriggerInteraction)1);
			if (((IEnumerable<Collider>)val).Any())
			{
				foreach (Collider item in (List<Collider>)(object)val)
				{
					if (!((Object)(object)item == (Object)null) && (!GameObjectEx.IsOnLayer(((Component)item).gameObject, (Layer)16) || !(((Object)((Component)item).gameObject).name == "TerrainMargin")) && !(GameObjectEx.ToBaseEntity(item) is DeepSeaPortal))
					{
						if (DeepSea.debug_portal_spawnattempts)
						{
							Debug.Log((object)("Invalid portal spawn pos: " + ((Object)item).name));
						}
						return false;
					}
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		PooledList<BasePlayer> val2 = Pool.Get<PooledList<BasePlayer>>();
		try
		{
			Vis.Entities(position, radius, (List<BasePlayer>)(object)val2, 131072, (QueryTriggerInteraction)1);
			foreach (BasePlayer item2 in (List<BasePlayer>)(object)val2)
			{
				if (!((Object)(object)item2 == (Object)(object)ignorePlayer))
				{
					return false;
				}
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		return true;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		bool flag = false;
		if ((Object)(object)WaterSystem.Instance == (Object)null && !flag)
		{
			Kill();
			return;
		}
		if (!DeepSea.enabled)
		{
			((MonoBehaviour)this).StartCoroutine(CloseAndDestroyDeepSea());
			return;
		}
		FillVehicleWhitelist();
		KillAllPortals();
		SpawnEntrancePortals();
		CacheWaitForSeconds(Application.isEditor);
		if (wipeSteps != null && wipeSteps.Length != 0)
		{
			Array.Sort(wipeSteps, (WipeStep a, WipeStep b) => b.timeLeft.CompareTo(a.timeLeft));
		}
		if (!Application.isLoadingSave)
		{
			StartServerTick();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (IsOpen())
		{
			if ((Object)(object)serverVolumesParent == (Object)null)
			{
				CreateRadVolumes();
			}
			if (EntrancePortalDirection == CardinalDirection.None)
			{
				int currentEntrancePortalDirection = Mathf.RoundToInt(TerrainMeta.LootAxisAngle / 90f) + 1;
				CurrentEntrancePortalDirection = currentEntrancePortalDirection;
			}
			ActivateEntrancePortal(EntrancePortalDirection);
			ExitPortalDirection = EntrancePortalDirection.Opposite();
			SpawnExitPortal();
			RestoreIslandNavMeshes();
		}
		StartServerTick();
	}

	private void StartServerTick()
	{
		if (!HasFlag(Flags.Reserved2))
		{
			if (DeepSea.openOnServerWipe || Application.isEditor)
			{
				SetTimeToNextOpening(0f);
			}
			else
			{
				ResetTimeToNextOpening();
			}
		}
		if (!IsInvoking(ServerTick))
		{
			InvokeRepeating(ServerTick, 5f, 1f);
		}
	}

	public void ServerTick()
	{
		if (!IsOpen() && !IsBusy())
		{
			if (TimeToNextOpening <= 0f)
			{
				OpenDeepSea();
			}
			else
			{
				TimeToNextOpening--;
			}
		}
		if (!IsOpen() || IsBusy())
		{
			return;
		}
		if (TimeToWipe <= 0f)
		{
			CloseDeepSea();
			return;
		}
		TimeToWipe--;
		ProcessWipeSteps();
		float wipeRadiationPhaseDuration = DeepSea.wipeRadiationPhaseDuration;
		bool flag = TimeToWipe <= wipeRadiationPhaseDuration && TimeToWipe > 0f;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved1, flag);
		}
		if (!((Object)(object)wipeRadiationVolume != (Object)null) || !(wipeRadiationPhaseDuration > 0f))
		{
			return;
		}
		if (flag)
		{
			float num = 1f - Mathf.Clamp01(TimeToWipe / wipeRadiationPhaseDuration);
			float radiationAmountOverride = Mathf.Lerp(0f, 60f, EndWipeRadiationCurve.Evaluate(num));
			wipeRadiationVolume.RadiationAmountOverride = radiationAmountOverride;
			if (!((Component)wipeRadiationVolume).gameObject.activeSelf)
			{
				ComponentExtensions.SetActive<TriggerRadiation>(wipeRadiationVolume, true);
			}
		}
		else if (TimeToWipe > wipeRadiationPhaseDuration)
		{
			ComponentExtensions.SetActive<TriggerRadiation>(wipeRadiationVolume, false);
			wipeRadiationVolume.RadiationAmountOverride = 0f;
		}
	}

	private void ResetWipeSteps()
	{
		nextWipeStepIndex = 0;
	}

	private void ProcessWipeSteps()
	{
		if (wipeSteps != null && wipeSteps.Length != 0)
		{
			while (nextWipeStepIndex < wipeSteps.Length && TimeToWipe > 0f && TimeToWipe <= wipeSteps[nextWipeStepIndex].timeLeft)
			{
				WipeStep step = wipeSteps[nextWipeStepIndex];
				OnWipeStepReached(step);
				nextWipeStepIndex++;
			}
		}
	}

	private void OnWipeStepReached(WipeStep step)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.CeilToInt(step.timeLeft / 60f);
		if (num < 1)
		{
			num = 1;
		}
		string text = num.ToString();
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (IsInsideDeepSea((BaseNetworkable)current))
				{
					current.ShowToast(GameTip.Styles.Blue_Long, step.message, true, text);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		if (!step.triggerFloatingCityAlarm)
		{
			return;
		}
		Enumerator<DeepSeaFloatingCity> enumerator2 = ServerFloatingCities.GetEnumerator();
		try
		{
			while (enumerator2.MoveNext())
			{
				enumerator2.Current.TriggerWipeAlarm(step.alarmEffect);
			}
		}
		finally
		{
			((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public void MoveToDeepSea(BaseEntity entity)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)entity == (Object)null)
		{
			return;
		}
		if (entity.isClient)
		{
			Debug.LogError((object)"DeepSea: Can't move a clientside entity to the deep sea");
		}
		else if (!IsInsideDeepSea((BaseNetworkable)entity))
		{
			PooledList<BasePlayer> val = Pool.Get<PooledList<BasePlayer>>();
			try
			{
				BaseVehicle.GetPassengersForVehicle(entity, (List<BasePlayer>)(object)val);
				PreTeleportEntity(entity, (List<BasePlayer>)(object)val, isEnterDeepSea: true);
				Vector3 deepSeaEntrancePosition = GetDeepSeaEntrancePosition(entity);
				TeleportEntity(entity, deepSeaEntrancePosition);
				PostTeleportEntity(entity, (List<BasePlayer>)(object)val);
				NotifyEntityMovedToDeepSea(entity);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public void MoveToMainIsland(BaseEntity entity)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)entity == (Object)null)
		{
			return;
		}
		if (entity.isClient)
		{
			Debug.LogError((object)"DeepSea: Can't move a clientside entity to the main island");
			return;
		}
		if (!IsInsideDeepSea((BaseNetworkable)entity))
		{
			Debug.LogWarning((object)$"DeepSea: Trying to remove entity {entity} from the deep sea but not inside it");
			return;
		}
		Vector3 deepSeaExitPosition = GetDeepSeaExitPosition(entity);
		PooledList<BasePlayer> val = Pool.Get<PooledList<BasePlayer>>();
		try
		{
			BaseVehicle.GetPassengersForVehicle(entity, (List<BasePlayer>)(object)val);
			PreTeleportEntity(entity, (List<BasePlayer>)(object)val, isEnterDeepSea: false);
			TeleportEntity(entity, deepSeaExitPosition);
			PostTeleportEntity(entity, (List<BasePlayer>)(object)val);
			NotifyEntityMovedToMainIsland(entity);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void PreTeleportEntity(BaseEntity entity, List<BasePlayer> passengers, bool isEnterDeepSea)
	{
		if (entity is BasePlayer player)
		{
			PreTeleportPlayer(player, isEnterDeepSea);
			return;
		}
		foreach (BasePlayer passenger in passengers)
		{
			PreTeleportPlayer(passenger, isEnterDeepSea);
			if (isEnterDeepSea)
			{
				NotifyEntityMovedToDeepSea(passenger);
			}
			else
			{
				NotifyEntityMovedToMainIsland(passenger);
			}
		}
	}

	private static void PreTeleportPlayer(BasePlayer player, bool isEnterDeepSea)
	{
		if (!player.IsNpcPlayer())
		{
			player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, b: true);
			player.ClientRPC(RpcTarget.Player("StartLoading", player));
			PointEntity<DeepSeaManager>.ServerInstance.ClientRPC(RpcTarget.Player("CLIENT_PlayerEnterOrLeaveDeepSea", player), isEnterDeepSea);
			if (!player.isMounted)
			{
				player.StartSleeping();
			}
		}
	}

	private static void TeleportEntity(BaseEntity entity, Vector3 position)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (entity is BasePlayer basePlayer && basePlayer.IsNonNpcPlayer())
		{
			if (!basePlayer.isMounted)
			{
				basePlayer.Teleport(position);
			}
		}
		else
		{
			((Component)entity).transform.position = position;
			entity.ClientRPC(RpcTarget.NetworkGroup("SnapPositionTo"), position, ((Component)entity).transform.eulerAngles);
		}
	}

	private static void PostTeleportEntity(BaseEntity entity, List<BasePlayer> passengers)
	{
		Dictionary<BasePlayer, BaseMountable> dictionary = Pool.Get<Dictionary<BasePlayer, BaseMountable>>();
		foreach (BasePlayer passenger in passengers)
		{
			BaseMountable mounted = passenger.GetMounted();
			if ((Object)(object)mounted != (Object)null)
			{
				dictionary[passenger] = mounted;
			}
			passenger.EnsureDismounted();
			passenger.SendNetworkUpdateImmediate();
			if (Rust.GameInfo.HasAchievements)
			{
				passenger.GiveAchievement("ENTER_DEEP_SEA");
			}
		}
		entity.UpdateNetworkGroup();
		foreach (BasePlayer passenger2 in passengers)
		{
			passenger2.UpdateNetworkGroup();
		}
		entity.SendNetworkUpdateImmediate();
		foreach (BasePlayer passenger3 in passengers)
		{
			passenger3.SendNetworkUpdateImmediate();
		}
		foreach (KeyValuePair<BasePlayer, BaseMountable> item in dictionary)
		{
			BaseMountable value = item.Value;
			if ((Object)(object)value != (Object)null)
			{
				value.MountPlayer(item.Key);
			}
		}
		Pool.FreeUnmanaged<BasePlayer, BaseMountable>(ref dictionary);
	}

	private static void NotifyEntityMovedToMainIsland(BaseEntity entity)
	{
		if (entity is IReceiveDeepSeaNotifications receiveDeepSeaNotifications)
		{
			receiveDeepSeaNotifications.OnExitDeepSea();
		}
	}

	private static void NotifyEntityMovedToDeepSea(BaseEntity entity)
	{
		if (entity is IReceiveDeepSeaNotifications receiveDeepSeaNotifications)
		{
			receiveDeepSeaNotifications.OnEnterDeepSea();
		}
	}

	private void DestroyRadVolumes()
	{
		if ((Object)(object)serverVolumesParent != (Object)null && (Object)(object)((Component)serverVolumesParent).gameObject != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)serverVolumesParent).gameObject);
			serverVolumesParent = null;
		}
	}

	private void CreateRadVolumes()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		DestroyRadVolumes();
		if ((Object)(object)serverVolumesParent == (Object)null)
		{
			serverVolumesParent = new GameObject("Volumes").transform;
			serverVolumesParent.SetParent(((Component)this).transform, false);
		}
		CardinalDirection num = GetEntrancePortalDirection().Opposite();
		if (num != CardinalDirection.West)
		{
			CreateRadiationVolume("DeepSea_RadVolume_Left", new Vector3(((Bounds)(ref DeepSeaBounds)).size.x / -2f, 0f, 0f), new Vector3(250f, ((Bounds)(ref DeepSeaBounds)).size.y, ((Bounds)(ref DeepSeaBounds)).size.z));
		}
		if (num != CardinalDirection.East)
		{
			CreateRadiationVolume("DeepSea_RadVolume_Right", new Vector3(((Bounds)(ref DeepSeaBounds)).size.x / 2f, 0f, 0f), new Vector3(250f, ((Bounds)(ref DeepSeaBounds)).size.y, ((Bounds)(ref DeepSeaBounds)).size.z));
		}
		if (num != CardinalDirection.North)
		{
			CreateRadiationVolume("DeepSea_RadVolume_Front", new Vector3(0f, 0f, ((Bounds)(ref DeepSeaBounds)).size.z / 2f), new Vector3(((Bounds)(ref DeepSeaBounds)).size.x, ((Bounds)(ref DeepSeaBounds)).size.y, 250f));
		}
		if (num != CardinalDirection.South)
		{
			CreateRadiationVolume("DeepSea_RadVolume_Back", new Vector3(0f, 0f, ((Bounds)(ref DeepSeaBounds)).size.z / -2f), new Vector3(((Bounds)(ref DeepSeaBounds)).size.x, ((Bounds)(ref DeepSeaBounds)).size.y, 250f));
		}
		wipeRadiationVolume = CreateRadiationVolume("DeepSea_RadVolume_Wipe", Vector3.zero, ((Bounds)(ref DeepSeaBounds)).size);
		ComponentExtensions.SetActive<TriggerRadiation>(wipeRadiationVolume, false);
	}

	private TriggerRadiation CreateRadiationVolume(string name, Vector3 pos, Vector3 size)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		BoxCollider obj = CreateCollider(name, pos, size, serverVolumesParent);
		((Collider)obj).isTrigger = true;
		TransformEx.SetLayerRecursive(((Component)obj).gameObject, 18);
		TriggerRadiation triggerRadiation = ((Component)obj).gameObject.AddComponent<TriggerRadiation>();
		triggerRadiation.InterestLayers = LayerMask.op_Implicit(131072);
		triggerRadiation.usePerAxisFalloff = true;
		triggerRadiation.BypassArmor = true;
		if (size.x == 250f)
		{
			triggerRadiation.falloffPerAxis.x = 60f;
		}
		if (size.y == 250f)
		{
			triggerRadiation.falloffPerAxis.y = 60f;
		}
		if (size.z == 250f)
		{
			triggerRadiation.falloffPerAxis.z = 60f;
		}
		return triggerRadiation;
	}

	public void OpenDeepSea()
	{
		GeneratePortalDirections();
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(OpenDeepSeaAsync());
		SetFlagLocal(Flags.Reserved2, b: true);
	}

	private IEnumerator OpenDeepSeaAsync()
	{
		Interface.CallHook("OnDeepSeaOpen", this);
		Log("Opening...");
		SetFlagLocal(Flags.Busy, b: true);
		SetTimeToWipe(DeepSea.wipeDuration);
		SetTimeToNextOpening(0f);
		yield return GenerateContentAsync();
		yield return TriggerSpawnGroups();
		yield return GenerateIslandNavMeshes();
		ActivateEntrancePortal(EntrancePortalDirection);
		SpawnExitPortal();
		SpawnBillboards();
		NetworkTreesScale();
		SpawnGhostShipHackableCrate();
		CreateRadVolumes();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Open, b: true);
		}
		SetFlagLocal(Flags.Busy, b: false);
		ResetWipeSteps();
		DoOpenNotification();
		PlanterBoxStatic.OnDeepSeaSpawned();
		Facepunch.Rust.Analytics.Azure.OnDeepSeaToggled(toggle: true);
		Log("Done!");
		Interface.CallHook("OnDeepSeaOpened", this);
	}

	public void CloseDeepSea(bool forceClose = false)
	{
		if (forceClose || IsOpen())
		{
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(CloseDeepSeaAsync());
		}
	}

	private IEnumerator CloseDeepSeaAsync()
	{
		Interface.CallHook("OnDeepSeaClose", this);
		Log("Closing...");
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Open, b: false);
			flagsUpdateScope.Set(Flags.Reserved1, b: false);
		}
		SetFlagLocal(Flags.Busy, b: true);
		TimeToWipe = 0f;
		KillAllBillboards();
		DeactivateAllEntrancePortals();
		KillExitPortal();
		DestroyRadVolumes();
		for (int i = 0; i < 3; i++)
		{
			yield return KillAllEntitiesAsync();
		}
		ClearDeepSeaTerrainData();
		BoatAICoordination.WipeCoordination();
		foodTollPaid.Clear();
		if ((Object)(object)wipeRadiationVolume != (Object)null)
		{
			((Component)wipeRadiationVolume).gameObject.SetActive(false);
			wipeRadiationVolume.RadiationAmountOverride = 0f;
		}
		if ((Object)(object)BaseGameMode.GetActiveGameMode(serverside: true) == (Object)null || !BaseGameMode.GetActiveGameMode(serverside: true).blockMapInDeepSea)
		{
			foreach (BasePlayer allPlayer in BasePlayer.allPlayerList)
			{
				if ((Object)(object)allPlayer != (Object)null)
				{
					allPlayer.ServerClearFog(mainland: false, deepSea: true);
				}
			}
		}
		ResetTimeToNextOpening();
		SetFlagLocal(Flags.Busy, b: false);
		Log("All cleared!");
		if (!DeepSea.enabled)
		{
			Kill();
		}
		Facepunch.Rust.Analytics.Azure.OnDeepSeaToggled(toggle: false);
		Interface.CallHook("OnDeepSeaClosed", this);
	}

	public IEnumerator CloseAndOpenDeepSeaAsync()
	{
		yield return (object)new WaitUntil((Func<bool>)(() => !Application.isLoadingSave));
		yield return CloseDeepSeaAsync();
		OpenDeepSea();
	}

	public IEnumerator CloseAndDestroyDeepSea()
	{
		yield return (object)new WaitUntil((Func<bool>)(() => !Application.isLoadingSave));
		if (IsOpen())
		{
			Log("Deep sea is disabled, cleaning saved entities and closing...");
			CloseDeepSea();
		}
		else
		{
			Log("Deep sea is disabled, killing");
			Kill();
		}
	}

	private IEnumerator KillAllEntitiesAsync()
	{
		HashSet<BaseEntity> entities = Pool.Get<HashSet<BaseEntity>>();
		GetAllDeepSeaEntities(entities);
		foreach (BasePlayer item in entities.OfType<BasePlayer>().ToList())
		{
			if (item.IsDead() || item.IsDestroyed)
			{
				entities.Remove(item);
				continue;
			}
			if (item.IsNpc)
			{
				item.Kill();
			}
			else
			{
				item.Hurt(1000f, DamageType.Radiation);
				if (item.IsAlive())
				{
					PointEntity<DeepSeaManager>.ServerInstance.ClientRPC(RpcTarget.Player("CLIENT_ClearDeepSeaShoreData", item));
				}
			}
			entities.Remove(item);
		}
		foreach (BaseEntity item2 in entities)
		{
			if (!((Object)(object)item2 == (Object)null) && !item2.IsDestroyed)
			{
				try
				{
					item2.Kill();
				}
				catch (Exception arg)
				{
					Debug.LogError((object)$"DeepSea: Wipe exception when killing entity {item2.PrefabName}\n{arg}");
				}
				yield return null;
			}
		}
		ServerIslands.Clear();
		ServerGhostShips.Clear();
		ServerFloatingCities.Clear();
		ServerRHIBS.Clear();
		Pool.FreeUnmanaged<BaseEntity>(ref entities);
	}

	public void GetAllDeepSeaEntities(HashSet<BaseEntity> entities, bool skipParented = true)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		AddAll((IEnumerable<BaseEntity>)ServerIslands);
		AddAll((IEnumerable<BaseEntity>)ServerGhostShips);
		AddAll((IEnumerable<BaseEntity>)ServerFloatingCities);
		AddAll((IEnumerable<BaseEntity>)ServerRHIBS);
		if (BaseNetworkable.DeepSeaGroup.networkables != null)
		{
			Enumerator<Networkable> enumerator = BaseNetworkable.DeepSeaGroup.networkables.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Networkable current = enumerator.Current;
					BaseEntity ent = BaseNetworkable.serverEntities.Find(current.ID) as BaseEntity;
					TryAddEntity(ent);
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
		if (BaseNetworkable.GlobalNetworkGroup.networkables != null)
		{
			Enumerator<Networkable> enumerator = BaseNetworkable.GlobalNetworkGroup.networkables.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Networkable current2 = enumerator.Current;
					BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(current2.ID) as BaseEntity;
					if ((Object)(object)baseEntity != (Object)null && IsInsideDeepSea((BaseNetworkable)baseEntity))
					{
						TryAddEntity(baseEntity);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
		if (BaseNetworkable.LimboNetworkGroup.networkables != null)
		{
			Enumerator<Networkable> enumerator = BaseNetworkable.LimboNetworkGroup.networkables.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Networkable current3 = enumerator.Current;
					BaseEntity baseEntity2 = BaseNetworkable.serverEntities.Find(current3.ID) as BaseEntity;
					if ((Object)(object)baseEntity2 != (Object)null && IsInsideDeepSea((BaseNetworkable)baseEntity2))
					{
						TryAddEntity(baseEntity2);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
		Net.sv.visibility.ForEachGroup(5, ProcessGroup);
		void AddAll(IEnumerable<BaseEntity> list)
		{
			foreach (BaseEntity item in list)
			{
				TryAddEntity(item);
			}
		}
		void ProcessGroup(Group group)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			if (CollectionEx.IsNullOrEmpty((ICollection<Networkable>)group.networkables))
			{
				return;
			}
			Enumerator<Networkable> enumerator2 = group.networkables.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					Networkable current4 = enumerator2.Current;
					BaseEntity ent2 = BaseNetworkable.serverEntities.Find(current4.ID) as BaseEntity;
					TryAddEntity(ent2);
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
		}
		void TryAddEntity(BaseEntity baseEntity3)
		{
			if (!((Object)(object)baseEntity3 == (Object)null) && baseEntity3.isServer && !(baseEntity3 is DeepSeaManager) && !(baseEntity3 is DeepSeaPortal) && (!skipParented || !baseEntity3.parentEntity.IsValid(serverside: true)))
			{
				entities.Add(baseEntity3);
			}
		}
	}

	private void FillVehicleWhitelist()
	{
		foreach (BaseEntity item in vehicleWhitelist)
		{
			if (!((Object)(object)item == (Object)null))
			{
				vehiclePrefabWhitelist.Add(item.prefabID);
			}
		}
	}

	private static bool IsVehicleAllowed(BaseEntity standingOnEnt)
	{
		if (DeepSea.allow_all_vehicles)
		{
			return true;
		}
		return PointEntity<DeepSeaManager>.ServerInstance.vehiclePrefabWhitelist.Contains(standingOnEnt.prefabID);
	}

	private void NetworkTreesScale()
	{
		HashSet<TreeEntity> hashSet = Pool.Get<HashSet<TreeEntity>>();
		GetAllDeepSeaTrees(hashSet);
		foreach (TreeEntity item in hashSet)
		{
			item.networkEntityScale = true;
			item.SendNetworkUpdate();
		}
		Pool.FreeUnmanaged<TreeEntity>(ref hashSet);
		static void GetAllDeepSeaTrees(HashSet<TreeEntity> trees)
		{
			Net.sv.visibility.ForEachGroup(5, ProcessGroup);
			void ProcessGroup(Group group)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				if (CollectionEx.IsNullOrEmpty((ICollection<Networkable>)group.networkables))
				{
					return;
				}
				Enumerator<Networkable> enumerator2 = group.networkables.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						Networkable current2 = enumerator2.Current;
						TreeEntity treeEntity = BaseNetworkable.serverEntities.Find(current2.ID) as TreeEntity;
						if (!((Object)(object)treeEntity == (Object)null) && treeEntity.isServer)
						{
							trees.Add(treeEntity);
						}
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
			}
		}
	}

	public static (bool, Phrase) CanTeleportToMainIsland(BaseEntity entity)
	{
		return IsAllowedInDeepSea(entity, fromMainLand: false);
	}

	public static (bool, Phrase) CanTeleportToDeepSea(BaseEntity entity)
	{
		if ((Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null && PointEntity<DeepSeaManager>.ServerInstance.TimeToWipe <= DeepSea.wipeRadiationPhaseDuration)
		{
			return (false, AboutToClose_Phrase);
		}
		return IsAllowedInDeepSea(entity, fromMainLand: true);
	}

	private static (bool, Phrase) IsAllowedInDeepSea(BaseEntity entity, bool fromMainLand)
	{
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)PointEntity<DeepSeaManager>.ServerInstance == (Object)null)
		{
			return (false, null);
		}
		if (entity.HasParent())
		{
			return (false, null);
		}
		if (entity is BasePlayer basePlayer)
		{
			if (basePlayer.IsNpc)
			{
				return (false, null);
			}
			if (basePlayer.isMounted || basePlayer.IsSleeping() || basePlayer.IsReceivingSnapshot)
			{
				return (false, null);
			}
			BaseEntity standingOnEntity = basePlayer.GetStandingOnEntity(134217728);
			if ((Object)(object)standingOnEntity != (Object)null)
			{
				if (standingOnEntity is BoatBuildingBlock boatBuildingBlock)
				{
					standingOnEntity = boatBuildingBlock.GetParentEntity();
				}
				if (IsVehicleAllowed(standingOnEntity))
				{
					return (false, null);
				}
			}
			if (basePlayer.IsFlying)
			{
				return (true, null);
			}
			if (DeepSea.allow_swimmers)
			{
				return (true, null);
			}
			return (false, fromMainLand ? NeedBoat_ToDeepSea_Phrase : NeedBoat_ToMainLand_Phrase);
		}
		PooledList<BasePlayer> val = Pool.Get<PooledList<BasePlayer>>();
		try
		{
			BaseVehicle.GetPassengersForVehicle(entity, (List<BasePlayer>)(object)val);
			foreach (BasePlayer item in (List<BasePlayer>)(object)val)
			{
				if (item.IsNpcPlayer())
				{
					return (false, null);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (entity is BaseVehicle baseVehicle)
		{
			if (baseVehicle is BaseBoat)
			{
				bool flag = IsVehicleAllowed(baseVehicle);
				if (flag)
				{
					if (baseVehicle is PlayerBoat playerBoat)
					{
						OBB val2 = playerBoat.WorldSpaceBounds();
						float radius = Mathf.Max(new float[3]
						{
							val2.extents.x,
							val2.extents.z,
							val2.extents.y
						});
						PooledList<BaseVehicle> val3 = Pool.Get<PooledList<BaseVehicle>>();
						try
						{
							Vis.Entities(val2.position, radius, (List<BaseVehicle>)(object)val3, 32768, (QueryTriggerInteraction)1);
							foreach (BaseVehicle item2 in (List<BaseVehicle>)(object)val3)
							{
								if (!item2.isClient && (item2 is PlayerHelicopter || item2 is BaseBoat || item2 is BaseSubmarine) && !IsVehicleAllowed(item2))
								{
									return (false, UnauthorizedVehicle_Phrase);
								}
							}
						}
						finally
						{
							((IDisposable)val3)?.Dispose();
						}
					}
					else
					{
						foreach (BaseEntity child in entity.children)
						{
							if (child is BaseVehicle standingOnEnt && (child is PlayerHelicopter || child is BaseBoat || child is BaseSubmarine) && !IsVehicleAllowed(standingOnEnt))
							{
								return (false, UnauthorizedVehicle_Phrase);
							}
						}
					}
				}
				return (flag, fromMainLand ? BoatNotStrongEnough_ToDeepSea_Phrase : BoatNotStrongEnough_ToMainLand_Phrase);
			}
			return (false, fromMainLand ? NeedBoat_ToDeepSea_Phrase : NeedBoat_ToMainLand_Phrase);
		}
		return (false, null);
	}

	public void RegisterPaidFoodToll(BasePlayer player)
	{
		if (!((Object)(object)player == (Object)null))
		{
			foodTollPaid.Add(player.userID);
			SendNetworkUpdate();
		}
	}

	private void DoOpenNotification()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer.Server_SendWorldNotificationToAllActivePlayers(WorldNotificationConfig.NotificationType.DeepSeaOpen, PortalEntranceTransform.position);
	}

	public new static void Log(string message)
	{
		if (DeepSea.logs)
		{
			Debug.Log((object)("DeepSea: " + message));
		}
	}

	public static bool IsRespawnVariant(SpawnGroup spawnGroup)
	{
		return ((Object)((Component)spawnGroup).gameObject).name.EndsWith("_Respawn", StringComparison.Ordinal);
	}

	public static void ApplyPopulationScale(SpawnGroup spawnGroup, float scale)
	{
		if (scale != 1f)
		{
			spawnGroup.maxPopulation = Mathf.Max(0, Mathf.RoundToInt((float)spawnGroup.maxPopulation * scale));
			spawnGroup.numToSpawnPerTickMin = Mathf.Max(0, Mathf.RoundToInt((float)spawnGroup.numToSpawnPerTickMin * scale));
			spawnGroup.numToSpawnPerTickMax = Mathf.Max(0, Mathf.RoundToInt((float)spawnGroup.numToSpawnPerTickMax * scale));
		}
	}

	private IEnumerator GenerateContentAsync()
	{
		placedPoints.Clear();
		float num = Mathf.Min(new float[4]
		{
			DeepSea.floatingcity_radius,
			DeepSea.island_radius,
			DeepSea.ghostship_radius,
			DeepSea.rhib_radius
		});
		cellSize = num / Mathf.Sqrt(2f);
		gridSizeX = Mathf.CeilToInt(((Bounds)(ref DeepSeaBounds)).size.x / cellSize);
		gridSizeY = Mathf.CeilToInt(((Bounds)(ref DeepSeaBounds)).size.z / cellSize);
		grid = new List<int>[gridSizeX, gridSizeY];
		if (floatingCityRefs.Length != 0)
		{
			shuffledPrefabs = GetShuffledPrefabQueue(floatingCityRefs);
			yield return ScatterPointsAsync(DeepSea.floatingcity_count, DeepSea.floatingcity_radius, DeepSea.floatingcity_edgeMargin, DeepSea.floatingcity_minDist, delegate(Vector2 point)
			{
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0031: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Unknown result type (might be due to invalid IL or missing references)
				//IL_004a: Unknown result type (might be due to invalid IL or missing references)
				//IL_004f: Unknown result type (might be due to invalid IL or missing references)
				GameObjectRef nextPrefab = GetNextPrefab(ref shuffledPrefabs, floatingCityRefs);
				Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
				DeepSeaFloatingCity deepSeaFloatingCity = SpawnEntityAt(nextPrefab.resourcePath, new Vector3(point.x, 0f, point.y), rotation) as DeepSeaFloatingCity;
				if ((Object)(object)deepSeaFloatingCity != (Object)null)
				{
					ServerFloatingCities.TryAdd(deepSeaFloatingCity);
				}
			});
		}
		Log($"Spawned {ServerFloatingCities.Count} floating cities");
		shuffledPrefabs = GetShuffledPrefabQueue(islandRefs);
		yield return ScatterPointsAsync(DeepSea.island_count, DeepSea.island_radius, DeepSea.island_edgeMargin, DeepSea.island_minDist, delegate(Vector2 point)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			GameObjectRef nextPrefab = GetNextPrefab(ref shuffledPrefabs, islandRefs);
			Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
			DeepSeaIsland deepSeaIsland = SpawnEntityAt(nextPrefab.resourcePath, new Vector3(point.x, 0f, point.y), rotation) as DeepSeaIsland;
			if ((Object)(object)deepSeaIsland != (Object)null)
			{
				ServerIslands.TryAdd(deepSeaIsland);
			}
		});
		Log($"Spawned {ServerIslands.Count} islands");
		if (ghostShipRefs.Length != 0)
		{
			shuffledPrefabs = GetShuffledPrefabQueue(ghostShipRefs);
			yield return ScatterPointsAsync(DeepSea.ghostship_count, DeepSea.ghostship_radius, DeepSea.ghostship_edgeMargin, DeepSea.ghostship_minDist, delegate(Vector2 point)
			{
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0031: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Unknown result type (might be due to invalid IL or missing references)
				//IL_004a: Unknown result type (might be due to invalid IL or missing references)
				//IL_004f: Unknown result type (might be due to invalid IL or missing references)
				GameObjectRef nextPrefab = GetNextPrefab(ref shuffledPrefabs, ghostShipRefs);
				Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
				GhostShip ghostShip = SpawnEntityAt(nextPrefab.resourcePath, new Vector3(point.x, 0f, point.y), rotation) as GhostShip;
				if ((Object)(object)ghostShip != (Object)null)
				{
					ServerGhostShips.TryAdd(ghostShip);
				}
			});
		}
		Log($"Spawned {ServerGhostShips.Count} ghost ships");
		if (AI.scientist_spawners_enabled)
		{
			yield return ScatterPointsAsync(DeepSea.rhib_count, DeepSea.rhib_radius, DeepSea.rhib_edgeMargin, DeepSea.rhib_minDist, delegate(Vector2 pos)
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Unknown result type (might be due to invalid IL or missing references)
				Vector3 val = ((Bounds)(ref DeepSeaBounds)).center - new Vector3(pos.x, 0f, pos.y);
				Quaternion rot = Quaternion.LookRotation(((Vector3)(ref val)).normalized);
				BoatAI.SpawnBoatGroup(pos, rot, null, registerWithDeepSea: true);
				Log($"Spawning boat AI group at {pos}");
			});
			Log($"Spawned {ServerRHIBS.Count} boat AI groups");
		}
		else
		{
			Debug.Log((object)"Skipping Scientist Boat spawning: ai.scientist_spawners_enabled is false");
		}
	}

	private IEnumerator ScatterPointsAsync(int count, float radius, float margin, float minDist, Action<Vector2> onPlaced)
	{
		int startCount = placedPoints.Count;
		int maxAttempts = count * 50;
		for (int i = 0; i < maxAttempts; i++)
		{
			if (placedPoints.Count >= startCount + count)
			{
				break;
			}
			Vector2 randomPosInBounds = GetRandomPosInBounds(margin);
			if (IsValidPosition(randomPosInBounds, radius, minDist, startCount))
			{
				int count2 = placedPoints.Count;
				placedPoints.Add(new PlacedPoint(randomPosInBounds, radius));
				AddToGrid(randomPosInBounds, count2);
				onPlaced(randomPosInBounds);
				yield return WaitEntitySpawnInterval;
			}
		}
	}

	private bool IsValidPosition(Vector2 candidatePos, float candidateRadius, float minDist, int startCount)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = ToCell(candidatePos);
		int num = Mathf.CeilToInt(Mathf.Max(candidateRadius, minDist) / cellSize);
		int num2 = Mathf.Max(0, ((Vector2Int)(ref val)).x - num);
		int num3 = Mathf.Min(gridSizeX - 1, ((Vector2Int)(ref val)).x + num);
		int num4 = Mathf.Max(0, ((Vector2Int)(ref val)).y - num);
		int num5 = Mathf.Min(gridSizeY - 1, ((Vector2Int)(ref val)).y + num);
		for (int i = num4; i <= num5; i++)
		{
			for (int j = num2; j <= num3; j++)
			{
				List<int> list = grid[j, i];
				if (list == null)
				{
					continue;
				}
				foreach (int item in list)
				{
					PlacedPoint placedPoint = placedPoints[item];
					float num6 = ((item < startCount || minDist == 0f) ? Mathf.Max(placedPoint.radius, candidateRadius) : Mathf.Max(minDist, Mathf.Max(placedPoint.radius, candidateRadius)));
					Vector2 val2 = candidatePos - placedPoint.pos;
					if (((Vector2)(ref val2)).sqrMagnitude < num6 * num6)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private void SpawnGhostShipHackableCrate()
	{
		int num = Mathf.Min(DeepSea.hackablecrate_count, ServerGhostShips.Count);
		if (num > 0)
		{
			List<GhostShip> list = Pool.Get<List<GhostShip>>();
			list.AddRange((IEnumerable<GhostShip>)ServerGhostShips.Values);
			ListEx.Shuffle<GhostShip>(list, (uint)Random.Range(0, 1000));
			for (int i = 0; i < num; i++)
			{
				list[i].SpawnHackableLockedCrate();
			}
			Pool.FreeUnmanaged<GhostShip>(ref list);
		}
	}

	private void AddToGrid(Vector2 point, int index)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = ToCell(point);
		if (grid[((Vector2Int)(ref val)).x, ((Vector2Int)(ref val)).y] == null)
		{
			grid[((Vector2Int)(ref val)).x, ((Vector2Int)(ref val)).y] = new List<int>(32);
		}
		grid[((Vector2Int)(ref val)).x, ((Vector2Int)(ref val)).y].Add(index);
	}

	private Vector2Int ToCell(Vector2 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		float num = (point.x - ((Bounds)(ref DeepSeaBounds)).min.x) / cellSize;
		float num2 = (point.y - ((Bounds)(ref DeepSeaBounds)).min.z) / cellSize;
		return new Vector2Int(Mathf.FloorToInt(num), Mathf.FloorToInt(num2));
	}

	private Vector2 GetRandomPosInBounds(float edgeMargin)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.Range(((Bounds)(ref DeepSeaBounds)).min.x + edgeMargin, ((Bounds)(ref DeepSeaBounds)).max.x - edgeMargin);
		float num2 = Random.Range(((Bounds)(ref DeepSeaBounds)).min.z + edgeMargin, ((Bounds)(ref DeepSeaBounds)).max.z - edgeMargin);
		return new Vector2(num, num2);
	}

	private Queue<GameObjectRef> GetShuffledPrefabQueue(GameObjectRef[] refs)
	{
		return new Queue<GameObjectRef>(refs.OrderBy((GameObjectRef _) => Random.value));
	}

	private GameObjectRef GetNextPrefab(ref Queue<GameObjectRef> queue, GameObjectRef[] source)
	{
		if (queue.Count > 0)
		{
			return queue.Dequeue();
		}
		return ArrayEx.GetRandom(source);
	}

	public void RegisterRHIBs(HashSet<RHIB> set)
	{
		ServerRHIBS.AddRange((IEnumerable<RHIB>)set);
	}

	public BaseEntity SpawnEntityAt(string prefabPath, Vector3 position, Quaternion rotation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Log($"Spawning {prefabPath} at {position}");
		BaseEntity baseEntity = GameManager.server.CreateEntity(prefabPath, position, rotation);
		if ((Object)(object)baseEntity == (Object)null)
		{
			return null;
		}
		baseEntity.Spawn();
		EntityParentSettings entityParentSettings = default(EntityParentSettings);
		if (((Component)baseEntity).TryGetComponent<EntityParentSettings>(ref entityParentSettings))
		{
			entityParentSettings.TryDetachChildren(baseEntity);
		}
		baseEntity.UpdateNetworkGroup();
		return baseEntity;
	}

	private IEnumerator TriggerSpawnGroups()
	{
		Log("Populating spawn groups...");
		HashSet<BaseEntity> entities = Pool.Get<HashSet<BaseEntity>>();
		PointEntity<DeepSeaManager>.ServerInstance.GetAllDeepSeaEntities(entities, skipParented: false);
		foreach (BaseEntity item in entities)
		{
			if (item is IDeepSeaSpawner deepSeaSpawner)
			{
				yield return deepSeaSpawner.TriggerSpawnGroups();
				yield return WaitEntitySpawnInterval;
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref entities);
	}

	private IEnumerator GenerateIslandNavMeshes()
	{
		Log("Generating islands nav meshes...");
		for (int i = 0; i < ServerIslands.Count; i++)
		{
			DeepSeaIsland deepSeaIsland = ServerIslands[i];
			if ((Object)(object)deepSeaIsland == (Object)null || (Object)(object)deepSeaIsland.monumentNavMesh == (Object)null)
			{
				continue;
			}
			if (AI.move)
			{
				yield return ((MonoBehaviour)deepSeaIsland).StartCoroutine(deepSeaIsland.UpdateNavMesh());
				if (AI.useUnityNavmesh)
				{
					yield return WaitNavMeshInterval;
				}
			}
			else
			{
				deepSeaIsland.GenerateNavMesh();
			}
		}
	}

	private void RestoreIslandNavMeshes()
	{
		Log("Dwellings loaded from save");
		((MonoBehaviour)this).StartCoroutine(GenerateIslandNavMeshes());
	}

	public void CacheWaitForSeconds(bool fast = false)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		if (fast)
		{
			WaitEntitySpawnInterval = new WaitForSeconds(1f);
			WaitSpawnGroupInterval = new WaitForSeconds(0.1f);
			WaitNavMeshInterval = new WaitForSeconds(1f);
		}
		else
		{
			WaitEntitySpawnInterval = new WaitForSeconds(DeepSea.entities_spawninterval);
			WaitSpawnGroupInterval = new WaitForSeconds(DeepSea.spawngroups_spawninterval);
			WaitNavMeshInterval = new WaitForSeconds(DeepSea.navmesh_spawninterval);
		}
	}

	public float GetWipeWeatherLerp()
	{
		float wipeEndProgress = GetWipeEndProgress();
		return EndWipeProgressToWeatherLerp.Evaluate(wipeEndProgress);
	}

	public static DeepSeaManager Get(bool server)
	{
		if (server)
		{
			return PointEntity<DeepSeaManager>.ServerInstance;
		}
		return null;
	}

	public override void InitShared()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		base.InitShared();
		CreateColliders();
		CreateSeaFloor();
		((Bounds)(ref bounds)).center = Vector3.zero;
		((Bounds)(ref bounds)).size = ((Bounds)(ref DeepSeaBounds)).size;
	}

	private void CreateColliders()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)sharedCollidersParent != (Object)null)
		{
			Object.Destroy((Object)(object)sharedCollidersParent);
			sharedCollidersParent = null;
		}
		if ((Object)(object)sharedCollidersParent == (Object)null)
		{
			sharedCollidersParent = new GameObject("Colliders").transform;
			sharedCollidersParent.SetParent(((Component)this).transform, false);
		}
		CreateCollider("DeepSea_Left", new Vector3(((Bounds)(ref DeepSeaBounds)).size.x / -2f, 0f, 0f), new Vector3(1f, ((Bounds)(ref DeepSeaBounds)).size.y, ((Bounds)(ref DeepSeaBounds)).size.z), sharedCollidersParent);
		CreateCollider("DeepSea_Right", new Vector3(((Bounds)(ref DeepSeaBounds)).size.x / 2f, 0f, 0f), new Vector3(1f, ((Bounds)(ref DeepSeaBounds)).size.y, ((Bounds)(ref DeepSeaBounds)).size.z), sharedCollidersParent);
		CreateCollider("DeepSea_Back", new Vector3(0f, 0f, ((Bounds)(ref DeepSeaBounds)).size.z / -2f), new Vector3(((Bounds)(ref DeepSeaBounds)).size.x, ((Bounds)(ref DeepSeaBounds)).size.y, 1f), sharedCollidersParent);
		CreateCollider("DeepSea_Front", new Vector3(0f, 0f, ((Bounds)(ref DeepSeaBounds)).size.z / 2f), new Vector3(((Bounds)(ref DeepSeaBounds)).size.x, ((Bounds)(ref DeepSeaBounds)).size.y, 1f), sharedCollidersParent);
	}

	private BoxCollider CreateCollider(string colName, Vector3 pos, Vector3 size, Transform parent, bool worldPos = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject(colName);
		val.transform.SetParent(parent, false);
		BoxCollider val2 = val.AddComponent<BoxCollider>();
		val2.size = size;
		if (worldPos)
		{
			((Component)val2).transform.position = pos;
		}
		else
		{
			val2.center = pos;
		}
		return val2;
	}

	private void CreateSeaFloor()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = default(Vector3);
		((Vector3)(ref localPosition))._002Ector(0f, SeaFloorDepth, 0f);
		Vector3 localScale = default(Vector3);
		((Vector3)(ref localScale))._002Ector(((Bounds)(ref DeepSeaBounds)).size.x, 1f, ((Bounds)(ref DeepSeaBounds)).size.z);
		GameObject obj = GameObject.CreatePrimitive((PrimitiveType)3);
		((Object)obj).name = "DeepSea_Bottom";
		obj.transform.SetParent(((Component)sharedCollidersParent).transform, false);
		obj.transform.localPosition = localPosition;
		obj.transform.localScale = localScale;
		TransformEx.SetLayerRecursive(obj, 16);
		MeshRenderer component = obj.GetComponent<MeshRenderer>();
		((Renderer)component).sharedMaterial = SeaFloorMaterial;
		((Renderer)component).shadowCastingMode = (ShadowCastingMode)0;
	}

	public static bool IsInsideDeepSea(Vector3 position)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return ((Bounds)(ref DeepSeaBounds)).Contains(position);
	}

	public static bool IsInsideDeepSea(Bounds bounds)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return ((Bounds)(ref DeepSeaBounds)).Intersects(bounds);
	}

	public static bool IsInsideDeepSea(BaseNetworkable entity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return IsInsideDeepSea(((Component)entity).transform.position);
	}

	public float GetTimeToWipe()
	{
		return TimeToWipe;
	}

	public float GetWipeEndProgress()
	{
		if (TimeToWipe == 0f || TimeToWipe > DeepSea.wipeEndPhaseDuration)
		{
			return 0f;
		}
		return 1f - Mathf.Clamp01(TimeToWipe / DeepSea.wipeEndPhaseDuration);
	}

	public float GetWipeRadiationAmount()
	{
		if (!((Object)(object)wipeRadiationVolume != (Object)null))
		{
			return 0f;
		}
		return wipeRadiationVolume.RadiationAmountOverride;
	}

	public static CardinalDirection GetEntrancePortalDirection()
	{
		DeepSeaManager deepSeaManager = null;
		deepSeaManager = PointEntity<DeepSeaManager>.ServerInstance;
		if ((Object)(object)deepSeaManager != (Object)null)
		{
			return (CardinalDirection)deepSeaManager.CurrentEntrancePortalDirection;
		}
		return CardinalDirection.None;
	}

	public void SetTimeToWipe(float time)
	{
		TimeToWipe = time;
	}

	public void SetTimeToNextOpening(float time)
	{
		TimeToNextOpening = time;
	}

	private void ResetTimeToNextOpening()
	{
		float wipeCooldownMin = DeepSea.wipeCooldownMin;
		float num = Mathf.Max(wipeCooldownMin, DeepSea.wipeCooldownMax);
		SetTimeToNextOpening(Random.Range(wipeCooldownMin, num));
	}

	public bool HasPaidFoodToll(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (base.isServer)
		{
			return foodTollPaid.Contains(player.userID);
		}
		return false;
	}

	public static Vector3 NormalizePosInDeepSea(Vector3 worldPos)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		worldPos.x = Mathx.RemapValClamped(worldPos.x, ((Bounds)(ref DeepSeaBounds)).min.x, ((Bounds)(ref DeepSeaBounds)).max.x, 0f, 1f);
		worldPos.z = Mathx.RemapValClamped(worldPos.z, ((Bounds)(ref DeepSeaBounds)).min.z, ((Bounds)(ref DeepSeaBounds)).max.z, 0f, 1f);
		worldPos.y = 0f;
		return worldPos;
	}

	public static float NormalizeX(float x)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return (x - ((Bounds)(ref DeepSeaBounds)).min.x) / ((Bounds)(ref DeepSeaBounds)).size.x;
	}

	public static float NormalizeZ(float z)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return (z - ((Bounds)(ref DeepSeaBounds)).min.z) / ((Bounds)(ref DeepSeaBounds)).size.z;
	}

	public bool IsAccessible()
	{
		if (IsOpen())
		{
			return !HasFlag(Flags.Reserved1);
		}
		return false;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		bool flag = (old & Flags.Open) == Flags.Open;
		bool flag2 = (next & Flags.Open) == Flags.Open;
		bool flag3 = flag2 && (next & Flags.Reserved1) != Flags.Reserved1;
		if (base.isServer && flag != flag2)
		{
			NPCVendingMachine.RefreshDeepSeaMapMarkers();
		}
	}

	public static void ClearDeepSeaTerrainData()
	{
		if (Object.op_Implicit((Object)(object)TerrainMeta.Texturing))
		{
			TerrainMeta.Texturing.ClearDeepSeaData();
		}
		if (Object.op_Implicit((Object)(object)TerrainMeta.HeightMap))
		{
			TerrainMeta.HeightMap.ResetDeepSeaToFloor();
		}
	}

	public override bool CanUseNetworkCache(Connection connection)
	{
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.deepSeaManager = Pool.Get<DeepSeaManager>();
		if (info.forDisk)
		{
			info.msg.deepSeaManager.foodPaid = (List<ulong>)(object)Pool.Get<PooledList<ulong>>();
			info.msg.deepSeaManager.foodPaid.AddRange(foodTollPaid);
			return;
		}
		BasePlayer basePlayer = info.forConnection.player as BasePlayer;
		if ((Object)(object)basePlayer != (Object)null && HasPaidFoodToll(basePlayer))
		{
			info.msg.deepSeaManager.localPlayerPaidFoodToll = true;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.deepSeaManager == null || !base.isServer)
		{
			return;
		}
		if (info.fromDisk && IsBusy())
		{
			if (TimeToWipe <= 0f)
			{
				Log("Loaded save with deep sea closing, closing again...");
				CloseDeepSea(forceClose: true);
			}
			else if (TimeToNextOpening <= 0f)
			{
				Log("Loaded save with deep sea opening, closing and reopening...");
				((MonoBehaviour)this).StartCoroutine(CloseAndOpenDeepSeaAsync());
			}
		}
		foodTollPaid.Clear();
		if (info.msg.deepSeaManager.foodPaid != null)
		{
			foodTollPaid.AddRange(info.msg.deepSeaManager.foodPaid);
		}
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: TimeToWipe for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_TimeToWipe);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: TimeToNextOpening for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_TimeToNextOpening);
			return true;
		case 2:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: CurrentEntrancePortalDirection for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_CurrentEntrancePortalDirection);
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
				_ = __sync_TimeToWipe;
				float _sync_TimeToWipe = reader.Float();
				__sync_TimeToWipe = _sync_TimeToWipe;
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_TimeToNextOpening;
				float _sync_TimeToNextOpening = reader.Float();
				__sync_TimeToNextOpening = _sync_TimeToNextOpening;
			}
			catch (Exception ex3)
			{
				Debug.LogException(ex3);
			}
			return true;
		case 2:
			try
			{
				_ = __sync_CurrentEntrancePortalDirection;
				int _sync_CurrentEntrancePortalDirection = reader.Int32();
				__sync_CurrentEntrancePortalDirection = _sync_CurrentEntrancePortalDirection;
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
		return propertyName switch
		{
			"TimeToWipe" => 0, 
			"TimeToNextOpening" => 1, 
			"CurrentEntrancePortalDirection" => 2, 
			_ => byte.MaxValue, 
		};
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
		WriteSyncVar(1, writer);
		WriteSyncVar(2, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
		OnSyncVar(1, reader, fromAutoSave: true);
		OnSyncVar(2, reader, fromAutoSave: true);
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
		__sync_TimeToWipe = 0f;
		__sync_TimeToNextOpening = 0f;
		__sync_CurrentEntrancePortalDirection = 0;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			2 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}

	static DeepSeaManager()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		ServerPortals = new List<DeepSeaPortal>();
		ClientPortals = new List<DeepSeaPortal>();
		ServerBillboards = new ListHashSet<IslandBillboard>();
		AboutToClose_Phrase = new Phrase("deepsea.abouttoclose", "The deep sea is no longer safe to enter");
		BoatNotStrongEnough_ToDeepSea_Phrase = new Phrase("deepsea.wrongboat-todeepsea", "Your boat cannot survive the journey to the deep sea");
		BoatNotStrongEnough_ToMainLand_Phrase = new Phrase("deepsea.wrongboat-tomainland", "Your boat cannot survive the journey to the main land");
		NeedBoat_ToDeepSea_Phrase = new Phrase("deepsea.needboat-todeepsea", "You cannot reach the deep sea without a sturdy boat");
		NeedBoat_ToMainLand_Phrase = new Phrase("deepsea.needboat-tomainland", "You cannot reach the main land without a sturdy boat");
		UnauthorizedVehicle_Phrase = new Phrase("deepsea.unauthorizedvehicle", "You cannot reach the deep sea with other vehicles on board");
		ServerIslands = new ListHashSet<DeepSeaIsland>();
		ServerGhostShips = new ListHashSet<GhostShip>();
		ServerFloatingCities = new ListHashSet<DeepSeaFloatingCity>();
		ServerRHIBS = new ListHashSet<RHIB>();
		placedPoints = new List<PlacedPoint>();
		DeepSeaBounds = new Bounds(new Vector3(-5900f, 0f, 0f), new Vector3(4000f, 4000f, 4000f));
		SeaFloorDepth = -50f;
	}
}
