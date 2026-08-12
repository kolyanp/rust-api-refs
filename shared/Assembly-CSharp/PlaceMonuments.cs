using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlaceMonuments : ProceduralComponent
{
	private readonly struct PlacementRung(float spacing, int minCorners, bool allowAdjacentTier)
	{
		public readonly float Spacing = spacing;

		public readonly int MinCorners = minCorners;

		public readonly bool AllowAdjacentTier = allowAdjacentTier;
	}

	public struct WorldSizeInfo
	{
		public int WorldSizeMin;

		public int WorldSizeMax;

		public int TargetCount;
	}

	public struct SpawnInfo
	{
		public Prefab<MonumentInfo> prefab;

		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;

		public bool dungeonEntrance;

		public Vector3 dungeonEntrancePos;
	}

	private struct DistanceInfo
	{
		public float minDistanceSameType;

		public float maxDistanceSameType;

		public float minDistanceDifferentType;

		public float maxDistanceDifferentType;

		public float minDistanceDungeonEntrance;

		public float maxDistanceDungeonEntrance;
	}

	public enum DistanceMode
	{
		Any,
		Min,
		Max
	}

	public SpawnFilter Filter;

	[Tooltip("Use this to spawn all monuments in a folder.")]
	public string ResourceFolder = string.Empty;

	[Tooltip("Use this to spawn specific monument prefabs.")]
	public GameObjectRef[] Monuments = Array.Empty<GameObjectRef>();

	public int TargetCount;

	public AnimationCurve TargetCountWorldSizeMultiplier = AnimationCurve.Constant(1000f, 6000f, 1f);

	[FormerlySerializedAs("MinDistance")]
	public int MinDistanceSameType = 500;

	public int MinDistanceDifferentType;

	[FormerlySerializedAs("MinSize")]
	public int MinWorldSize;

	[Tooltip("Distance to monuments of the same type")]
	public DistanceMode DistanceSameType = DistanceMode.Max;

	[Tooltip("Distance to monuments of a different type")]
	public DistanceMode DistanceDifferentType;

	[Tooltip("Enable to only spawn these monuments when running as a nexus")]
	public bool NexusOnly;

	private const PrefabPriority RequiredPriority = PrefabPriority.Highest;

	private const int MinDistanceFloor = 50;

	public const int GroupCandidates = 8;

	public const int IndividualCandidates = 8;

	public const int Attempts = 10000;

	private const int RetryCandidates = 8;

	private const float DistanceScoreWeight = 0.1f;

	private const float SameTypeDistanceWeight = 2f;

	private const float DifferentTypeDistanceWeight = 1f;

	private const float DistanceScoreScale = 1f / 60f;

	private static readonly float[] RelaxationTiers = new float[4] { 1f, 0.75f, 0.5f, 0.25f };

	private static readonly PlacementRung[] PlacementRungs = new PlacementRung[9]
	{
		new PlacementRung(1f, 3, allowAdjacentTier: false),
		new PlacementRung(0.75f, 3, allowAdjacentTier: false),
		new PlacementRung(0.5f, 3, allowAdjacentTier: false),
		new PlacementRung(1f, 2, allowAdjacentTier: false),
		new PlacementRung(0.5f, 2, allowAdjacentTier: false),
		new PlacementRung(1f, 1, allowAdjacentTier: false),
		new PlacementRung(0.5f, 1, allowAdjacentTier: false),
		new PlacementRung(1f, 3, allowAdjacentTier: true),
		new PlacementRung(0.5f, 3, allowAdjacentTier: true)
	};

	private const int RemainingAttemptMultiplier = 4;

	private const int MaxDepth = 100000;

	public override void Process(uint seed)
	{
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		if (NexusOnly && !World.Nexus)
		{
			return;
		}
		string[] array = Array.Empty<string>();
		if (!string.IsNullOrWhiteSpace(ResourceFolder))
		{
			array = (from folder in ResourceFolder.Split(',')
				select "assets/bundled/prefabs/autospawn/" + folder + "/").ToArray();
		}
		if (World.Networked)
		{
			World.Spawn("Monument", array, Monuments);
		}
		else
		{
			if (World.Size < MinWorldSize)
			{
				return;
			}
			PathFinder pathFinder = null;
			List<PathFinder.Point> pathTargets = null;
			List<Prefab<MonumentInfo>> list = new List<Prefab<MonumentInfo>>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!text.Contains("underwater_lab") || World.Config.UnderwaterLabs)
				{
					Prefab<MonumentInfo>[] array3 = Prefab.Load<MonumentInfo>(text, (GameManager)null, (PrefabAttribute.Library)null, true, true);
					ArrayEx.Shuffle(array3, ref seed);
					list.AddRange(array3);
				}
			}
			int num2 = Monuments.Length;
			if (num2 > 0)
			{
				GameObjectRef[] array4 = Monuments;
				if (num2 > 1)
				{
					GameObjectRef[] array5 = Monuments.ToArray();
					ArrayEx.Shuffle(array5, ref seed);
					array4 = array5;
				}
				for (int num3 = 0; num3 < num2; num3++)
				{
					GameObjectRef gameObjectRef = array4[num3];
					if (gameObjectRef.isValid)
					{
						Prefab<MonumentInfo> item = Prefab.Load<MonumentInfo>(gameObjectRef.resourceID, (GameManager)null, (PrefabAttribute.Library)null);
						list.Add(item);
					}
				}
			}
			Prefab<MonumentInfo>[] array6 = list.ToArray();
			if (array6 == null || array6.Length == 0)
			{
				return;
			}
			ArrayEx.BubbleSort(array6);
			long num4 = 0L;
			int num5 = 0;
			List<SpawnInfo> list2 = new List<SpawnInfo>();
			long num6 = 0L;
			int num7 = 0;
			List<SpawnInfo> list3 = new List<SpawnInfo>();
			int num8 = Mathf.RoundToInt((float)TargetCount * TargetCountWorldSizeMultiplier.Evaluate((float)World.Size));
			int num9 = 0;
			Prefab<MonumentInfo>[] array7 = array6;
			foreach (Prefab<MonumentInfo> prefab in array7)
			{
				if (!((Object)(object)prefab.Component == (Object)null) && World.Size >= prefab.Component.MinWorldSize && GetPriority(prefab) >= PrefabPriority.Highest)
				{
					num9++;
				}
			}
			if (num8 > 0)
			{
				num9 = Mathf.Min(num9, num8);
			}
			int num10 = 8 + ((num9 > 0) ? (RelaxationTiers.Length * 8) : 0);
			for (int num11 = 0; num11 < num10; num11++)
			{
				bool flag = num11 >= 8;
				if (flag && num7 >= num9)
				{
					break;
				}
				float relaxation = (flag ? RelaxationTiers[(num11 - 8) / 8] : 1f);
				float num12 = RelaxDistance(MinDistanceSameType, relaxation);
				float num13 = RelaxDistance(MinDistanceDifferentType, relaxation);
				num4 = 0L;
				num5 = 0;
				list2.Clear();
				bool flag2 = false;
				array7 = array6;
				foreach (Prefab<MonumentInfo> prefab2 in array7)
				{
					MonumentInfo component = prefab2.Component;
					if ((Object)(object)component == (Object)null || World.Size < component.MinWorldSize)
					{
						continue;
					}
					_ = component.DungeonEntrance;
					PrefabPriority priority = GetPriority(prefab2);
					bool flag3 = priority >= PrefabPriority.Highest;
					int num14 = (int)(priority + 1);
					int priorityScore = 100000 * num14 * num14 * num14 * num14;
					float minDistanceSameType = (flag3 ? num12 : ((float)MinDistanceSameType));
					float minDistanceDifferentType = (flag3 ? num13 : ((float)MinDistanceDifferentType));
					if (TryFindSpawn(prefab2, list2, ref seed, ref pathFinder, ref pathTargets, minDistanceSameType, minDistanceDifferentType, priorityScore, PlacementRungs[0].MinCorners, PlacementRungs[0].AllowAdjacentTier, 10000, out var resultSpawn, out var resultScore))
					{
						list2.Add(resultSpawn);
						num4 += resultScore;
						if (flag3)
						{
							num5++;
						}
					}
					else if (flag3 & flag)
					{
						flag2 = true;
						break;
					}
					if (num8 > 0 && list2.Count >= num8)
					{
						break;
					}
				}
				if (!flag2 && (num5 > num7 || (num5 == num7 && num4 > num6)))
				{
					num7 = num5;
					num6 = num4;
					GenericsUtil.Swap<List<SpawnInfo>>(ref list2, ref list3);
				}
			}
			PlaceRemainingMonuments(array6, list3, ref seed, ref pathFinder, ref pathTargets, num8);
			foreach (SpawnInfo item2 in list3)
			{
				World.AddPrefab("Monument", item2.prefab, item2.position, item2.rotation, item2.scale);
			}
		}
	}

	private void PlaceRemainingMonuments(Prefab<MonumentInfo>[] prefabs, List<SpawnInfo> spawns, ref uint seed, ref PathFinder pathFinder, ref List<PathFinder.Point> pathTargets, int targetCount)
	{
		foreach (Prefab<MonumentInfo> prefab in prefabs)
		{
			if (targetCount > 0 && spawns.Count >= targetCount)
			{
				break;
			}
			MonumentInfo component = prefab.Component;
			if ((Object)(object)component == (Object)null || World.Size < component.MinWorldSize)
			{
				continue;
			}
			bool flag = false;
			foreach (SpawnInfo spawn in spawns)
			{
				if (spawn.prefab == prefab)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			int num = (int)(GetPriority(prefab) + 1);
			int priorityScore = 100000 * num * num * num * num;
			PlacementRung[] placementRungs = PlacementRungs;
			for (int j = 0; j < placementRungs.Length; j++)
			{
				PlacementRung placementRung = placementRungs[j];
				float minDistanceSameType = RelaxDistance(MinDistanceSameType, placementRung.Spacing);
				float minDistanceDifferentType = RelaxDistance(MinDistanceDifferentType, placementRung.Spacing);
				if (TryFindSpawn(prefab, spawns, ref seed, ref pathFinder, ref pathTargets, minDistanceSameType, minDistanceDifferentType, priorityScore, placementRung.MinCorners, placementRung.AllowAdjacentTier, 40000, out var resultSpawn, out var _))
				{
					spawns.Add(resultSpawn);
					break;
				}
			}
		}
	}

	private bool TryFindSpawn(Prefab<MonumentInfo> prefab, List<SpawnInfo> spawns, ref uint seed, ref PathFinder pathFinder, ref List<PathFinder.Point> pathTargets, float minDistanceSameType, float minDistanceDifferentType, int priorityScore, int minCorners, bool allowAdjacentTier, int attempts, out SpawnInfo resultSpawn, out int resultScore)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_0469: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0482: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		MonumentInfo component = prefab.Component;
		DungeonGridInfo dungeonEntrance = component.DungeonEntrance;
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		Vector3 position = TerrainMeta.Position;
		Vector3 size = TerrainMeta.Size;
		float x = position.x;
		float z = position.z;
		float num = position.x + size.x;
		float num2 = position.z + size.z;
		int num3 = 0;
		bool result = false;
		int num4 = int.MinValue;
		SpawnInfo spawnInfo = default(SpawnInfo);
		Vector3 pos = default(Vector3);
		for (int i = 0; i < attempts; i++)
		{
			float num5 = SeedRandom.Range(ref seed, x, num);
			float num6 = SeedRandom.Range(ref seed, z, num2);
			float normX = TerrainMeta.NormalizeX(num5);
			float normZ = TerrainMeta.NormalizeZ(num6);
			float num7 = SeedRandom.Value(ref seed);
			float factor = Filter.GetFactor(normX, normZ);
			if (factor * factor < num7)
			{
				continue;
			}
			float height = heightMap.GetHeight(normX, normZ);
			((Vector3)(ref pos))._002Ector(num5, height, num6);
			Quaternion rot = prefab.Object.transform.localRotation;
			Vector3 scale = prefab.Object.transform.localScale;
			Vector3 val = pos;
			prefab.ApplyDecorComponents(ref pos, ref rot, ref scale);
			DistanceInfo distanceInfo = GetDistanceInfo(spawns, prefab, pos, rot, scale, val);
			if (distanceInfo.minDistanceSameType < minDistanceSameType || distanceInfo.minDistanceDifferentType < minDistanceDifferentType || (Object.op_Implicit((Object)(object)dungeonEntrance) && distanceInfo.minDistanceDungeonEntrance < dungeonEntrance.MinDistance))
			{
				continue;
			}
			int num8 = priorityScore;
			if (distanceInfo.minDistanceSameType != float.MaxValue)
			{
				float num9 = distanceInfo.minDistanceSameType / (float)World.Size;
				int num10 = Mathf.RoundToInt((float)priorityScore * num9 * num9 * (1f / 60f) * 2f);
				if (DistanceSameType == DistanceMode.Min)
				{
					num8 -= num10;
				}
				else if (DistanceSameType == DistanceMode.Max)
				{
					num8 += num10;
				}
			}
			if (distanceInfo.minDistanceDifferentType != float.MaxValue)
			{
				float num11 = distanceInfo.minDistanceDifferentType / (float)World.Size;
				int num12 = Mathf.RoundToInt((float)priorityScore * num11 * num11 * (1f / 60f) * 1f);
				if (DistanceDifferentType == DistanceMode.Min)
				{
					num8 -= num12;
				}
				else if (DistanceDifferentType == DistanceMode.Max)
				{
					num8 += num12;
				}
			}
			if (num8 <= num4 || !prefab.ApplyTerrainFilters(pos, rot, scale) || !prefab.ApplyTerrainAnchors(ref pos, rot, scale, Filter) || !component.CheckPlacement(pos, rot, scale, minCorners, allowAdjacentTier))
			{
				continue;
			}
			if (Object.op_Implicit((Object)(object)dungeonEntrance))
			{
				Vector3 val2 = pos + rot * Vector3.Scale(scale, ((Component)dungeonEntrance).transform.position);
				Vector3 val3 = dungeonEntrance.SnapPosition(val2);
				pos += val3 - val2;
				if (!dungeonEntrance.IsValidSpawnPosition(val3))
				{
					continue;
				}
				val = val3;
			}
			if (!prefab.ApplyTerrainChecks(pos, rot, scale, Filter) || !prefab.ApplyWaterChecks(pos, rot, scale) || !prefab.ApplyEnvironmentVolumeChecks(pos, rot, scale) || prefab.CheckEnvironmentVolumes(pos, rot, scale, EnvironmentType.Underground | EnvironmentType.TrainTunnels))
			{
				continue;
			}
			bool flag = false;
			TerrainPathConnect[] componentsInChildren = prefab.Object.GetComponentsInChildren<TerrainPathConnect>(true);
			foreach (TerrainPathConnect terrainPathConnect in componentsInChildren)
			{
				if (terrainPathConnect.Type == InfrastructureType.Boat)
				{
					if (pathFinder == null)
					{
						int[,] array = TerrainPath.CreateBoatCostmap(4f);
						int length = array.GetLength(0);
						pathFinder = new PathFinder(array);
						pathTargets = new List<PathFinder.Point>
						{
							new PathFinder.Point(0, 0),
							new PathFinder.Point(0, length / 2),
							new PathFinder.Point(0, length - 1),
							new PathFinder.Point(length / 2, 0),
							new PathFinder.Point(length / 2, length - 1),
							new PathFinder.Point(length - 1, 0),
							new PathFinder.Point(length - 1, length / 2),
							new PathFinder.Point(length - 1, length - 1)
						};
					}
					PathFinder.Point point = PathFinder.GetPoint(pos + rot * Vector3.Scale(scale, ((Component)terrainPathConnect).transform.localPosition), pathFinder.GetResolution(0));
					if (pathFinder.FindPathUndirected(new List<PathFinder.Point> { point }, pathTargets, 100000) == null)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				SpawnInfo spawnInfo2 = new SpawnInfo
				{
					prefab = prefab,
					position = pos,
					rotation = rot,
					scale = scale
				};
				if (Object.op_Implicit((Object)(object)dungeonEntrance))
				{
					spawnInfo2.dungeonEntrance = true;
					spawnInfo2.dungeonEntrancePos = val;
				}
				num4 = num8;
				spawnInfo = spawnInfo2;
				result = true;
				num3++;
				if (num3 >= 8 || DistanceDifferentType == DistanceMode.Any)
				{
					break;
				}
			}
		}
		resultSpawn = spawnInfo;
		resultScore = num4;
		return result;
	}

	private static PrefabPriority GetPriority(Prefab<MonumentInfo> prefab)
	{
		if (!Object.op_Implicit((Object)(object)prefab.Parameters))
		{
			return PrefabPriority.Lowest;
		}
		return prefab.Parameters.Priority;
	}

	private static float RelaxDistance(int configured, float relaxation)
	{
		if (configured <= 50)
		{
			return configured;
		}
		return Mathf.Max((float)configured * relaxation, 50f);
	}

	public DistanceInfo GetDistanceInfo(List<SpawnInfo> spawns, Prefab<MonumentInfo> prefab, Vector3 monumentPos, Quaternion monumentRot, Vector3 monumentScale, Vector3 dungeonPos)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		DistanceInfo result = new DistanceInfo
		{
			minDistanceSameType = float.MaxValue,
			maxDistanceSameType = float.MinValue,
			minDistanceDifferentType = float.MaxValue,
			maxDistanceDifferentType = float.MinValue,
			minDistanceDungeonEntrance = float.MaxValue,
			maxDistanceDungeonEntrance = float.MinValue
		};
		OBB val = default(OBB);
		((OBB)(ref val))._002Ector(monumentPos, monumentScale, monumentRot, prefab.Component.Bounds);
		if (spawns != null)
		{
			foreach (SpawnInfo spawn in spawns)
			{
				OBB val2 = new OBB(spawn.position, spawn.scale, spawn.rotation, spawn.prefab.Component.Bounds);
				float num = ((OBB)(ref val2)).SqrDistance(val);
				if (spawn.prefab.Folder == prefab.Folder)
				{
					if (num < result.minDistanceSameType)
					{
						result.minDistanceSameType = num;
					}
					if (num > result.maxDistanceSameType)
					{
						result.maxDistanceSameType = num;
					}
				}
				else
				{
					if (num < result.minDistanceDifferentType)
					{
						result.minDistanceDifferentType = num;
					}
					if (num > result.maxDistanceDifferentType)
					{
						result.maxDistanceDifferentType = num;
					}
				}
			}
			foreach (SpawnInfo spawn2 in spawns)
			{
				if (spawn2.dungeonEntrance)
				{
					Vector3 val3 = spawn2.dungeonEntrancePos - dungeonPos;
					float sqrMagnitude = ((Vector3)(ref val3)).sqrMagnitude;
					if (sqrMagnitude < result.minDistanceDungeonEntrance)
					{
						result.minDistanceDungeonEntrance = sqrMagnitude;
					}
					if (sqrMagnitude > result.maxDistanceDungeonEntrance)
					{
						result.maxDistanceDungeonEntrance = sqrMagnitude;
					}
				}
			}
		}
		if ((Object)(object)TerrainMeta.Path != (Object)null)
		{
			foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
			{
				float num2 = monument.SqrDistance(val);
				if (num2 < result.minDistanceDifferentType)
				{
					result.minDistanceDifferentType = num2;
				}
				if (num2 > result.maxDistanceDifferentType)
				{
					result.maxDistanceDifferentType = num2;
				}
			}
			foreach (DungeonGridInfo dungeonGridEntrance in TerrainMeta.Path.DungeonGridEntrances)
			{
				float num3 = dungeonGridEntrance.SqrDistance(dungeonPos);
				if (num3 < result.minDistanceDungeonEntrance)
				{
					result.minDistanceDungeonEntrance = num3;
				}
				if (num3 > result.maxDistanceDungeonEntrance)
				{
					result.maxDistanceDungeonEntrance = num3;
				}
			}
		}
		if (result.minDistanceSameType != float.MaxValue)
		{
			result.minDistanceSameType = Mathf.Sqrt(result.minDistanceSameType);
		}
		if (result.maxDistanceSameType != float.MinValue)
		{
			result.maxDistanceSameType = Mathf.Sqrt(result.maxDistanceSameType);
		}
		if (result.minDistanceDifferentType != float.MaxValue)
		{
			result.minDistanceDifferentType = Mathf.Sqrt(result.minDistanceDifferentType);
		}
		if (result.maxDistanceDifferentType != float.MinValue)
		{
			result.maxDistanceDifferentType = Mathf.Sqrt(result.maxDistanceDifferentType);
		}
		if (result.minDistanceDungeonEntrance != float.MaxValue)
		{
			result.minDistanceDungeonEntrance = Mathf.Sqrt(result.minDistanceDungeonEntrance);
		}
		if (result.maxDistanceDungeonEntrance != float.MinValue)
		{
			result.maxDistanceDungeonEntrance = Mathf.Sqrt(result.maxDistanceDungeonEntrance);
		}
		return result;
	}
}
