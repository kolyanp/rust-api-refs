using UnityEngine;

namespace Rust.Ai.Gen2;

public static class LivestockGrass
{
	public struct Ground
	{
		private TerrainTopologyMap.TopologyQueryStructure topology;

		private TerrainSplatMap.SplatQueryStructure splats;

		private TerrainBiomeMap.BiomeQueryStructure biomes;

		private Vector3 terrainPosition;

		private Vector3 oneOverSize;

		private Vector3 deepSeaMin;

		private Vector3 deepSeaMax;

		private bool allDeepSea;

		public static Ground Read()
		{
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			TerrainTopologyMap topologyMap = TerrainMeta.TopologyMap;
			TerrainSplatMap splatMap = TerrainMeta.SplatMap;
			TerrainBiomeMap biomeMap = TerrainMeta.BiomeMap;
			if ((Object)(object)topologyMap == (Object)null || (Object)(object)splatMap == (Object)null || (Object)(object)biomeMap == (Object)null)
			{
				return default;
			}
			return new Ground
			{
				topology = topologyMap.GetQueryStructure(),
				splats = splatMap.GetQueryStructure(),
				biomes = biomeMap.GetQueryStructure(),
				terrainPosition = TerrainMeta.Position,
				oneOverSize = TerrainMeta.OneOverSize,
				deepSeaMin = DeepSeaManager.DeepSeaBounds.min,
				deepSeaMax = DeepSeaManager.DeepSeaBounds.max
			};
		}

		public bool IsGrass(Vector3 position)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			if (!splats.source.IsCreated || IsDeepSea(position))
			{
				return false;
			}
			float normX = (position.x - terrainPosition.x) * oneOverSize.x;
			float normZ = (position.z - terrainPosition.z) * oneOverSize.z;
			if (!topology.GetTopology(normX, normZ, 97))
			{
				return false;
			}
			if ((splats.GetSplatMaxType(normX, normZ) & 0x30) == 0)
			{
				return false;
			}
			return (biomes.GetBiomeMaxType(normX, normZ) & 0x17) != 0;
		}

		private bool IsDeepSea(Vector3 position)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			if (!allDeepSea)
			{
				if (position.x >= deepSeaMin.x && position.x <= deepSeaMax.x && position.y >= deepSeaMin.y && position.y <= deepSeaMax.y && position.z >= deepSeaMin.z)
				{
					return position.z <= deepSeaMax.z;
				}
				return false;
			}
			return true;
		}
	}

	private const Enum GrassTopology = (Enum)97;

	private const Enum GrassSplats = (Enum)48;

	private const Enum GrassBiomes = (Enum)23;

	public const int BuiltOverMask = 2097408;

	public const float ProbeLift = 1f;

	public const float ProbeLength = 1.3f;

	public static bool IsGrassGround(Vector3 position)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TerrainMeta.TopologyMap == (Object)null || (Object)(object)TerrainMeta.SplatMap == (Object)null || (Object)(object)TerrainMeta.BiomeMap == (Object)null)
		{
			return false;
		}
		if ((TerrainMeta.TopologyMap.GetTopology(position) & 0x61) == 0)
		{
			return false;
		}
		if ((TerrainMeta.SplatMap.GetSplatMaxType(position) & 0x30) == 0)
		{
			return false;
		}
		return (TerrainMeta.BiomeMap.GetBiomeMaxType(position) & 0x17) != 0;
	}

	public static bool IsOnOpenGround(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return !GamePhysics.CheckSphere(GamePhysics.Realm.Server, position - Vector3.up * 0.2f, 0.25f, 2097408, (QueryTriggerInteraction)1);
	}
}
