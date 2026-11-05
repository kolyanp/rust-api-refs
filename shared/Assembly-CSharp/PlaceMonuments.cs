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
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
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
		SpawnInfo spawnInfo = default;
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
			Vector3 pos = new Vector3(num5, height, num6);
			Quaternion rot = prefab.Object.transform.localRotation;
			Vector3 scale = prefab.Object.transform.localScale;
			Vector3 val = pos;
			prefab.ApplyDecorComponents(ref pos, ref rot, ref scale);
			if (!component.CheckTierPlacement(pos, rot, scale, minCorners, allowAdjacentTier) || !TryGetDistanceScore(spawns, prefab, pos, rot, scale, val, minDistanceSameType, minDistanceDifferentType, priorityScore, num4, out var score) || !prefab.ApplyTerrainFilters(pos, rot, scale) || !prefab.ApplyTerrainAnchors(ref pos, rot, scale, Filter) || !component.CheckWaterPlacement(pos))
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
				num4 = score;
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

	private bool TryGetDistanceScore(List<SpawnInfo> spawns, Prefab<MonumentInfo> prefab, Vector3 monumentPos, Quaternion monumentRot, Vector3 monumentScale, Vector3 dungeonPos, float minDistanceSameType, float minDistanceDifferentType, int priorityScore, int bestScore, out int score)
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		score = priorityScore;
		DungeonGridInfo dungeonEntrance = prefab.Component.DungeonEntrance;
		if (Object.op_Implicit((Object)(object)dungeonEntrance))
		{
			if (spawns != null)
			{
				foreach (SpawnInfo spawn in spawns)
				{
					if (spawn.dungeonEntrance)
					{
						Vector3 val = spawn.dungeonEntrancePos - dungeonPos;
						if (Mathf.Sqrt(val.sqrMagnitude) < dungeonEntrance.MinDistance)
						{
							return false;
						}
					}
				}
			}
			if ((Object)(object)TerrainMeta.Path != (Object)null)
			{
				foreach (DungeonGridInfo dungeonGridEntrance in TerrainMeta.Path.DungeonGridEntrances)
				{
					if (Mathf.Sqrt(dungeonGridEntrance.SqrDistance(dungeonPos)) < dungeonEntrance.MinDistance)
					{
						return false;
					}
				}
			}
		}
		OBB val2 = new OBB(monumentPos, monumentScale, monumentRot, prefab.Component.Bounds);
		float num = float.MaxValue;
		OBB val3;
		if (spawns != null)
		{
			foreach (SpawnInfo spawn2 in spawns)
			{
				if (!(spawn2.prefab.Folder != prefab.Folder))
				{
					val3 = new OBB(spawn2.position, spawn2.scale, spawn2.rotation, spawn2.prefab.Component.Bounds);
					float num2 = val3.SqrDistance(val2);
					if (Mathf.Sqrt(num2) < minDistanceSameType)
					{
						return false;
					}
					if (num2 < num)
					{
						num = num2;
					}
				}
			}
		}
		score += GetDistanceScore(num, DistanceSameType, 2f, priorityScore);
		if (DistanceDifferentType != DistanceMode.Max && score <= bestScore)
		{
			return false;
		}
		if (DistanceDifferentType != DistanceMode.Min && DistanceDifferentType != DistanceMode.Max && !(minDistanceDifferentType > 0f))
		{
			return true;
		}
		float minSqrDistanceDifferentType = float.MaxValue;
		int sameTypeScore = score;
		if (spawns != null)
		{
			foreach (SpawnInfo spawn3 in spawns)
			{
				if (!(spawn3.prefab.Folder == prefab.Folder))
				{
					val3 = new OBB(spawn3.position, spawn3.scale, spawn3.rotation, spawn3.prefab.Component.Bounds);
					if (!AcceptDifferentType(val3.SqrDistance(val2)))
					{
						return false;
					}
				}
			}
		}
		if ((Object)(object)TerrainMeta.Path != (Object)null)
		{
			foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
			{
				if (!AcceptDifferentType(monument.SqrDistance(val2)))
				{
					return false;
				}
			}
		}
		score += GetDistanceScore(minSqrDistanceDifferentType, DistanceDifferentType, 1f, priorityScore);
		return score > bestScore;
		bool AcceptDifferentType(float sqrDist)
		{
			if (Mathf.Sqrt(sqrDist) < minDistanceDifferentType)
			{
				return false;
			}
			if (!(sqrDist < minSqrDistanceDifferentType))
			{
				return true;
			}
			minSqrDistanceDifferentType = sqrDist;
			if (DistanceDifferentType == DistanceMode.Max)
			{
				return sameTypeScore + GetDistanceScore(sqrDist, DistanceMode.Max, 1f, priorityScore) > bestScore;
			}
			return true;
		}
	}

	private static int GetDistanceScore(float minSqrDistance, DistanceMode mode, float weight, int priorityScore)
	{
		if (minSqrDistance == float.MaxValue)
		{
			return 0;
		}
		float num = Mathf.Sqrt(minSqrDistance) / (float)World.Size;
		int num2 = Mathf.RoundToInt((float)priorityScore * num * num * (1f / 60f) * weight);
		return mode switch
		{
			DistanceMode.Min => -num2, 
			DistanceMode.Max => num2, 
			_ => 0, 
		};
	}
}
