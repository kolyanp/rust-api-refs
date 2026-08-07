using System.Collections.Generic;
using Facepunch;
using Oxide.Core;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class TerrainMeta : SingletonComponent<TerrainMeta>
{
	public enum PaintMode
	{
		None,
		Splats,
		Biomes,
		Alpha,
		Blend,
		Field,
		Cliff,
		Summit,
		Beachside,
		Beach,
		Forest,
		Forestside,
		Ocean,
		Oceanside,
		Decor,
		Monument,
		Road,
		Roadside,
		Swamp,
		River,
		Riverside,
		Lake,
		Lakeside,
		Offshore,
		Rail,
		Railside,
		Building,
		Cliffside,
		Mountain,
		Clutter,
		Alt,
		Tier0,
		Tier1,
		Tier2,
		Mainland,
		Hilltop
	}

	public struct BurstData
	{
		public Vector3 Position;

		public Vector3 Size;

		public Vector3 OneOverSize;
	}

	public TerrainRenderer terrainRenderer = new TerrainRenderer();

	public TerrainData terrainData;

	public TerrainConfig config;

	public PaintMode paint;

	[HideInInspector]
	public PaintMode currentPaintMode;

	public static readonly SharedStatic<BurstData> sharedBurstData = SharedStatic<BurstData>.GetOrCreateUnsafe(0u, 5411825963348367585L, -2546176521858529784L);

	public static TerrainConfig Config { get; private set; }

	public static TerrainRenderer TerrainRenderer { get; private set; }

	public static Transform Transform { get; private set; }

	public static TerrainData TerrainData { get; private set; }

	public static Vector3 Position { get; private set; }

	public static Vector3 Size { get; private set; }

	public static Vector3 Center => Position + Size * 0.5f;

	public static Vector3 Max => Position + Size;

	public static Vector3 OneOverSize { get; private set; }

	public static Vector3 HighestPoint { get; set; }

	public static Vector3 LowestPoint { get; set; }

	public static float LootAxisAngle { get; private set; }

	public static float BiomeAxisAngle { get; private set; }

	public static TerrainData Data { get; private set; }

	public static TerrainCollider Collider { get; private set; }

	public static TerrainCollision Collision { get; private set; }

	public static TerrainPhysics Physics { get; private set; }

	public static TerrainColors Colors { get; private set; }

	public static TerrainQuality Quality { get; private set; }

	public static TerrainPath Path { get; private set; }

	public static TerrainBiomeMap BiomeMap { get; private set; }

	public static TerrainAlphaMap AlphaMap { get; private set; }

	public static TerrainBlendMap BlendMap { get; private set; }

	public static TerrainHeightMap HeightMap { get; private set; }

	public static TerrainSplatMap SplatMap { get; private set; }

	public static TerrainTopologyMap TopologyMap { get; private set; }

	public static TerrainWaterMap WaterMap { get; private set; }

	public static TerrainWaterFlowMap WaterFlowMap { get; private set; }

	public static TerrainDistanceMap DistanceMap { get; private set; }

	public static TerrainPlacementMap PlacementMap { get; private set; }

	public static TerrainTexturing Texturing { get; private set; }

	public static TerrainHoleRenderer HoleRenderer { get; private set; }

	public static Vector3 MarginSize => Size * 3f;

	public static void SetReflectionProbeUsage(ReflectionProbeUsage value)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		TerrainRenderer.SetReflectionProbeUsage(value);
	}

	public static float SampleTerrainMeshHeight(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		float num = NormalizeX(worldPos.x);
		float num2 = NormalizeZ(worldPos.z);
		float interpolatedHeight = TerrainData.GetInterpolatedHeight(num, num2);
		return Position.y + interpolatedHeight;
	}

	public static bool OutOfBounds(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (worldPos.x < Position.x)
		{
			return true;
		}
		if (worldPos.z < Position.z)
		{
			return true;
		}
		if (worldPos.x > Position.x + Size.x)
		{
			return true;
		}
		if (worldPos.z > Position.z + Size.z)
		{
			return true;
		}
		return false;
	}

	public static bool OutOfBoundsBurst(Vector3 worldPos)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		BurstData data = sharedBurstData.Data;
		Vector3 position = data.Position;
		Vector3 size = data.Size;
		if (worldPos.x < position.x)
		{
			return true;
		}
		if (worldPos.z < position.z)
		{
			return true;
		}
		if (worldPos.x > position.x + size.x)
		{
			return true;
		}
		if (worldPos.z > position.z + size.z)
		{
			return true;
		}
		return false;
	}

	public static bool OutOfMargin(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (worldPos.x < Position.x - Size.x)
		{
			return true;
		}
		if (worldPos.z < Position.z - Size.z)
		{
			return true;
		}
		if (worldPos.x > Position.x + Size.x + Size.x)
		{
			return true;
		}
		if (worldPos.z > Position.z + Size.z + Size.z)
		{
			return true;
		}
		return false;
	}

	public static bool OutOfMarginPlusTutorialBounds(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (worldPos.x < Position.x - Size.x - TutorialIsland.TutorialBoundsSize)
		{
			return true;
		}
		if (worldPos.z < Position.z - Size.z - TutorialIsland.TutorialBoundsSize)
		{
			return true;
		}
		if (worldPos.x > Position.x + Size.x + Size.x + TutorialIsland.TutorialBoundsSize)
		{
			return true;
		}
		if (worldPos.z > Position.z + Size.z + Size.z + TutorialIsland.TutorialBoundsSize)
		{
			return true;
		}
		return false;
	}

	public static float InnerDistToEdge2D(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		float num = Position.x - Size.x;
		float num2 = Position.x + Size.x + Size.x;
		float num3 = Position.z - Size.z;
		float num4 = Position.z + Size.z + Size.z;
		float num5 = Mathf.Abs(worldPos.x - num);
		float num6 = Mathf.Abs(worldPos.x - num2);
		float num7 = Mathf.Abs(worldPos.z - num3);
		float num8 = Mathf.Abs(worldPos.z - num4);
		return Mathf.Min(num5, Mathf.Min(num6, Mathf.Min(num7, num8)));
	}

	public static bool IsPointWithinTutorialBounds(Vector3 worldPos)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		float tutorialBoundsSize = TutorialIsland.TutorialBoundsSize;
		float maximumPointTutorial = ValidBounds.GetMaximumPointTutorial();
		float num = 0f - maximumPointTutorial + tutorialBoundsSize;
		float num2 = maximumPointTutorial - tutorialBoundsSize;
		float num3 = 0f - maximumPointTutorial + tutorialBoundsSize;
		float num4 = maximumPointTutorial - tutorialBoundsSize;
		if (!(worldPos.x < num) && !(worldPos.x > num2) && !(worldPos.z < num3))
		{
			return worldPos.z > num4;
		}
		return true;
	}

	public static bool RandomWaterPointInAnnulus(Vector3 centre, float minRadius, float maxRadius, out Vector3 randomPoint)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 100; i++)
		{
			Vector3 val = Vector2.op_Implicit(Random.insideUnitCircle);
			float num = Random.Range(minRadius, maxRadius);
			Vector3 val2 = centre + new Vector3(val.x, 0f, val.y) * num;
			float height = HeightMap.GetHeight(val2);
			float height2 = WaterMap.GetHeight(val2);
			if (height <= height2)
			{
				randomPoint = val2;
				return true;
			}
		}
		randomPoint = Vector3.zero;
		return false;
	}

	public static bool IsInBiome(Vector3 worldPos, Enum biome)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		return ((((Object)(object)BiomeMap == (Object)null) ? 2 : BiomeMap.GetBiomeMaxType(worldPos)) & biome) > 0;
	}

	public static Vector3 RandomPointOffshore(bool avoidDeepSeaPortal = false, bool avoidDeepSea = false)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.Range(-1f, 1f);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(Mathf.Min(Size.x, 4000f) - 100f, 0f, Mathf.Min(Size.z, 4000f) - 100f);
		avoidDeepSeaPortal = avoidDeepSeaPortal && (Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null && PointEntity<DeepSeaManager>.ServerInstance.IsOpen();
		avoidDeepSea = avoidDeepSea && (Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null;
		List<CardinalDirection> list = Pool.Get<List<CardinalDirection>>();
		list.Add(CardinalDirection.North);
		list.Add(CardinalDirection.East);
		list.Add(CardinalDirection.South);
		list.Add(CardinalDirection.West);
		if (avoidDeepSeaPortal)
		{
			CardinalDirection entrancePortalDirection = DeepSeaManager.GetEntrancePortalDirection();
			list.Remove(entrancePortalDirection);
		}
		if (avoidDeepSea)
		{
			list.Remove(CardinalDirection.West);
		}
		Vector3 result = (Vector3)(ListEx.GetRandom<CardinalDirection>(list) switch
		{
			CardinalDirection.West => Center + new Vector3(0f - val.x, 0f, num * val.z), 
			CardinalDirection.East => Center + new Vector3(val.x, 0f, num * val.z), 
			CardinalDirection.South => Center + new Vector3(num * val.x, 0f, 0f - val.z), 
			_ => Center + new Vector3(num * val.x, 0f, val.z), 
		});
		Pool.FreeUnmanaged<CardinalDirection>(ref list);
		return result;
	}

	public static Vector3 RandomPoint(bool excludeWater = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		while (((Vector3)(ref val)).Equals(default(Vector3)) || (excludeWater && WaterLevel.Test(val, waves: true, volumes: true)))
		{
			float num = Random.Range(0f, Data.size.x);
			float num2 = Random.Range(0f, Data.size.z);
			float height = HeightMap.GetHeight(new Vector3(num, 0f, num2));
			((Vector3)(ref val))._002Ector(num, height, num2);
		}
		return val;
	}

	public static Vector3 Normalize(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		float num = (worldPos.x - Position.x) * OneOverSize.x;
		float num2 = (worldPos.y - Position.y) * OneOverSize.y;
		float num3 = (worldPos.z - Position.z) * OneOverSize.z;
		return new Vector3(num, num2, num3);
	}

	public static float NormalizeX(float x)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return (x - Position.x) * OneOverSize.x;
	}

	public static float NormalizeY(float y)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return (y - Position.y) * OneOverSize.y;
	}

	public static float NormalizeZ(float z)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return (z - Position.z) * OneOverSize.z;
	}

	public static Vector3 Denormalize(Vector3 normPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		float num = Position.x + normPos.x * Size.x;
		float num2 = Position.y + normPos.y * Size.y;
		float num3 = Position.z + normPos.z * Size.z;
		return new Vector3(num, num2, num3);
	}

	public static float DenormalizeX(float normX)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Position.x + normX * Size.x;
	}

	public static float DenormalizeY(float normY)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Position.y + normY * Size.y;
	}

	public static float DenormalizeZ(float normZ)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Position.z + normZ * Size.z;
	}

	protected override void Awake()
	{
		base.Awake();
		if (Application.isPlaying)
		{
			Shader.DisableKeyword("TERRAIN_PAINTING");
		}
	}

	public void Init(TerrainData terrainDataOverride = null, TerrainConfig configOverride = null)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		if (configOverride != null)
		{
			config = configOverride;
		}
		if ((Object)(object)terrainDataOverride != (Object)null)
		{
			terrainData = terrainDataOverride;
		}
		TerrainData = terrainData;
		Config = config;
		Transform = ((Component)this).transform;
		Data = terrainData;
		Size = terrainData.size;
		OneOverSize = Vector3Ex.Inverse(Size);
		Position = ((Component)this).transform.position;
		Collider = ((Component)this).gameObject.GetComponent<TerrainCollider>();
		Collision = ((Component)this).gameObject.GetComponent<TerrainCollision>();
		Physics = ((Component)this).gameObject.GetComponent<TerrainPhysics>();
		Colors = ((Component)this).gameObject.GetComponent<TerrainColors>();
		Quality = ((Component)this).gameObject.GetComponent<TerrainQuality>();
		Path = ((Component)this).gameObject.GetComponent<TerrainPath>();
		BiomeMap = ((Component)this).gameObject.GetComponent<TerrainBiomeMap>();
		AlphaMap = ((Component)this).gameObject.GetComponent<TerrainAlphaMap>();
		BlendMap = ((Component)this).gameObject.GetComponent<TerrainBlendMap>();
		HeightMap = ((Component)this).gameObject.GetComponent<TerrainHeightMap>();
		SplatMap = ((Component)this).gameObject.GetComponent<TerrainSplatMap>();
		TopologyMap = ((Component)this).gameObject.GetComponent<TerrainTopologyMap>();
		WaterMap = ((Component)this).gameObject.GetComponent<TerrainWaterMap>();
		WaterFlowMap = ((Component)this).gameObject.GetComponent<TerrainWaterFlowMap>();
		DistanceMap = ((Component)this).gameObject.GetComponent<TerrainDistanceMap>();
		PlacementMap = ((Component)this).gameObject.GetComponent<TerrainPlacementMap>();
		Texturing = ((Component)this).gameObject.GetComponent<TerrainTexturing>();
		HoleRenderer = ((Component)this).gameObject.GetComponent<TerrainHoleRenderer>();
		TerrainRenderer = terrainRenderer;
		if (Object.op_Implicit((Object)(object)terrainRenderer.terrain))
		{
			terrainRenderer.terrain.drawInstanced = false;
		}
		HighestPoint = new Vector3(Position.x, Position.y + Size.y, Position.z);
		LowestPoint = new Vector3(Position.x, Position.y, Position.z);
		TerrainExtension[] components = ((Component)this).GetComponents<TerrainExtension>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].Init(terrainRenderer, terrainData, config);
		}
		uint seed = World.Seed;
		int num = SeedRandom.Range(ref seed, 0, 4) * 90;
		int num2 = SeedRandom.Range(ref seed, -45, 46);
		int num3 = SeedRandom.Sign(ref seed);
		LootAxisAngle = num;
		BiomeAxisAngle = num + num2 + num3 * 90;
		InitBurstData();
	}

	private static void InitBurstData()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		sharedBurstData.Data = new BurstData
		{
			Position = Position,
			Size = Size,
			OneOverSize = OneOverSize
		};
	}

	public static void InitNoTerrain(bool createPath = false)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Size = new Vector3(4096f, 4096f, 4096f);
		OneOverSize = Vector3Ex.Inverse(Size);
		Position = -0.5f * Size;
		InitBurstData();
	}

	public void SetupComponents()
	{
		TerrainExtension[] components = ((Component)this).GetComponents<TerrainExtension>();
		foreach (TerrainExtension obj in components)
		{
			obj.Setup();
			obj.isInitialized = true;
		}
	}

	public void PostSetupComponents()
	{
		TerrainExtension[] components = ((Component)this).GetComponents<TerrainExtension>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].PostSetup();
		}
		Interface.CallHook("OnTerrainInitialized");
	}

	public void BindShaderProperties()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)config))
		{
			Shader.SetGlobalTexture("Terrain_AlbedoArray", config.AlbedoArray);
			Shader.SetGlobalTexture("Terrain_NormalArray", config.NormalArray);
			Shader.SetGlobalVector("Terrain_TexelSize", Vector4.op_Implicit(new Vector2(1f / config.GetMinSplatTiling(), 1f / config.GetMinSplatTiling())));
			Shader.SetGlobalVector("Terrain_TexelSize0", new Vector4(1f / config.Splats[0].SplatTiling, 1f / config.Splats[1].SplatTiling, 1f / config.Splats[2].SplatTiling, 1f / config.Splats[3].SplatTiling));
			Shader.SetGlobalVector("Terrain_TexelSize1", new Vector4(1f / config.Splats[4].SplatTiling, 1f / config.Splats[5].SplatTiling, 1f / config.Splats[6].SplatTiling, 1f / config.Splats[7].SplatTiling));
			Shader.SetGlobalVector("Splat0_UVMIX", Vector4.op_Implicit(new Vector3(config.Splats[0].UVMIXMult, config.Splats[0].UVMIXStart, 1f / config.Splats[0].UVMIXDist)));
			Shader.SetGlobalVector("Splat1_UVMIX", Vector4.op_Implicit(new Vector3(config.Splats[1].UVMIXMult, config.Splats[1].UVMIXStart, 1f / config.Splats[1].UVMIXDist)));
			Shader.SetGlobalVector("Splat2_UVMIX", Vector4.op_Implicit(new Vector3(config.Splats[2].UVMIXMult, config.Splats[2].UVMIXStart, 1f / config.Splats[2].UVMIXDist)));
			Shader.SetGlobalVector("Splat3_UVMIX", Vector4.op_Implicit(new Vector3(config.Splats[3].UVMIXMult, config.Splats[3].UVMIXStart, 1f / config.Splats[3].UVMIXDist)));
			Shader.SetGlobalVector("Splat4_UVMIX", Vector4.op_Implicit(new Vector3(config.Splats[4].UVMIXMult, config.Splats[4].UVMIXStart, 1f / config.Splats[4].UVMIXDist)));
			Shader.SetGlobalVector("Splat5_UVMIX", Vector4.op_Implicit(new Vector3(config.Splats[5].UVMIXMult, config.Splats[5].UVMIXStart, 1f / config.Splats[5].UVMIXDist)));
			Shader.SetGlobalVector("Splat6_UVMIX", Vector4.op_Implicit(new Vector3(config.Splats[6].UVMIXMult, config.Splats[6].UVMIXStart, 1f / config.Splats[6].UVMIXDist)));
			Shader.SetGlobalVector("Splat7_UVMIX", Vector4.op_Implicit(new Vector3(config.Splats[7].UVMIXMult, config.Splats[7].UVMIXStart, 1f / config.Splats[7].UVMIXDist)));
		}
		if (Object.op_Implicit((Object)(object)HeightMap))
		{
			Shader.SetGlobalVector("_Terrain_HeightParams", new Vector4(Position.y, Size.y, OneOverSize.y, 0f));
			Shader.SetGlobalTexture("Terrain_Normal", (Texture)(object)HeightMap.NormalTexture);
		}
		if (Object.op_Implicit((Object)(object)AlphaMap))
		{
			Shader.SetGlobalTexture("Terrain_Alpha", (Texture)(object)AlphaMap.AlphaTexture);
		}
		if (Object.op_Implicit((Object)(object)BiomeMap))
		{
			Shader.SetGlobalTexture("Terrain_Biome", (Texture)(object)BiomeMap.BiomeTexture);
		}
		if (Object.op_Implicit((Object)(object)SplatMap))
		{
			Shader.SetGlobalTexture("Terrain_Control0", (Texture)(object)SplatMap.SplatTexture0);
			Shader.SetGlobalTexture("Terrain_Control1", (Texture)(object)SplatMap.SplatTexture1);
		}
		Object.op_Implicit((Object)(object)WaterMap);
		Object.op_Implicit((Object)(object)DistanceMap);
		if (!terrainRenderer)
		{
			return;
		}
		Shader.SetGlobalVector("Terrain_Position", Vector4.op_Implicit(Position));
		Shader.SetGlobalVector("Terrain_Size", Vector4.op_Implicit(Size));
		Shader.SetGlobalVector("Terrain_RcpSize", Vector4.op_Implicit(OneOverSize));
		Shader.SetGlobalVector("Terrain_Global_Position", Vector4.op_Implicit(Position));
		Shader.SetGlobalVector("Terrain_Global_Size", Vector4.op_Implicit(Size));
		Shader.SetGlobalVector("Terrain_Global_RcpSize", Vector4.op_Implicit(OneOverSize));
		if (Object.op_Implicit((Object)(object)terrainRenderer.material))
		{
			Material material = terrainRenderer.material;
			if (material.IsKeywordEnabled("_TERRAIN_BLEND_LINEAR"))
			{
				material.DisableKeyword("_TERRAIN_BLEND_LINEAR");
			}
			if (material.IsKeywordEnabled("_TERRAIN_VERTEX_NORMALS"))
			{
				material.DisableKeyword("_TERRAIN_VERTEX_NORMALS");
			}
		}
	}
}
