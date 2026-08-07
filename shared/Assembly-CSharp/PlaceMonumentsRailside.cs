using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlaceMonumentsRailside : ProceduralComponent
{
	private struct SpawnInfo
	{
		public Prefab<MonumentInfo> prefab;

		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;
	}

	private class SpawnInfoGroup
	{
		public bool processed;

		public Prefab<MonumentInfo> prefab;

		public List<SpawnInfo> candidates;
	}

	private struct DistanceInfo
	{
		public float minDistanceSameType;

		public float maxDistanceSameType;

		public float minDistanceDifferentType;

		public float maxDistanceDifferentType;
	}

	public enum DistanceMode
	{
		Any,
		Min,
		Max
	}

	public SpawnFilter Filter;

	public string ResourceFolder = string.Empty;

	public int TargetCount;

	public int PositionOffset = 100;

	public int TangentInterval = 100;

	[FormerlySerializedAs("MinDistance")]
	public int MinDistanceSameType = 500;

	public int MinDistanceDifferentType;

	[FormerlySerializedAs("MinSize")]
	public int MinWorldSize;

	[Tooltip("Distance to monuments of the same type")]
	public DistanceMode DistanceSameType = DistanceMode.Max;

	[Tooltip("Distance to monuments of a different type")]
	public DistanceMode DistanceDifferentType;

	private const int GroupCandidates = 8;

	private const int IndividualCandidates = 8;

	private static Quaternion rot90 = Quaternion.Euler(0f, 90f, 0f);

	public override void Process(uint seed)
	{
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0832: Unknown result type (might be due to invalid IL or missing references)
		//IL_0839: Unknown result type (might be due to invalid IL or missing references)
		//IL_0840: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0519: Unknown result type (might be due to invalid IL or missing references)
		//IL_0520: Unknown result type (might be due to invalid IL or missing references)
		//IL_0527: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0615: Unknown result type (might be due to invalid IL or missing references)
		//IL_061c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0623: Unknown result type (might be due to invalid IL or missing references)
		//IL_0644: Unknown result type (might be due to invalid IL or missing references)
		//IL_064b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0652: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_066d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0674: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0701: Unknown result type (might be due to invalid IL or missing references)
		//IL_0708: Unknown result type (might be due to invalid IL or missing references)
		//IL_070f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0694: Unknown result type (might be due to invalid IL or missing references)
		//IL_069b: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06de: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0725: Unknown result type (might be due to invalid IL or missing references)
		//IL_072c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0733: Unknown result type (might be due to invalid IL or missing references)
		//IL_0743: Unknown result type (might be due to invalid IL or missing references)
		//IL_074a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0751: Unknown result type (might be due to invalid IL or missing references)
		//IL_0761: Unknown result type (might be due to invalid IL or missing references)
		//IL_0768: Unknown result type (might be due to invalid IL or missing references)
		//IL_076f: Unknown result type (might be due to invalid IL or missing references)
		string[] array = (from folder in ResourceFolder.Split(',')
			select "assets/bundled/prefabs/autospawn/" + folder + "/").ToArray();
		if (World.Networked)
		{
			World.Spawn("Monument", array);
		}
		else
		{
			if (World.Size < MinWorldSize)
			{
				return;
			}
			_ = TerrainMeta.HeightMap;
			List<Prefab<MonumentInfo>> list = new List<Prefab<MonumentInfo>>();
			string[] array2 = array;
			for (int num = 0; num < array2.Length; num++)
			{
				Prefab<MonumentInfo>[] array3 = Prefab.Load<MonumentInfo>(array2[num], (GameManager)null, (PrefabAttribute.Library)null, true, true);
				ArrayEx.Shuffle(array3, ref seed);
				list.AddRange(array3);
			}
			Prefab<MonumentInfo>[] array4 = list.ToArray();
			if (array4 == null || array4.Length == 0)
			{
				return;
			}
			ArrayEx.BubbleSort(array4);
			SpawnInfoGroup[] array5 = new SpawnInfoGroup[array4.Length];
			for (int num2 = 0; num2 < array4.Length; num2++)
			{
				Prefab<MonumentInfo> prefab = array4[num2];
				SpawnInfoGroup spawnInfoGroup = null;
				for (int num3 = 0; num3 < num2; num3++)
				{
					SpawnInfoGroup spawnInfoGroup2 = array5[num3];
					Prefab<MonumentInfo> prefab2 = spawnInfoGroup2.prefab;
					if (prefab == prefab2)
					{
						spawnInfoGroup = spawnInfoGroup2;
						break;
					}
				}
				if (spawnInfoGroup == null)
				{
					spawnInfoGroup = new SpawnInfoGroup();
					spawnInfoGroup.prefab = array4[num2];
					spawnInfoGroup.candidates = new List<SpawnInfo>();
				}
				array5[num2] = spawnInfoGroup;
			}
			SpawnInfoGroup[] array6 = array5;
			foreach (SpawnInfoGroup spawnInfoGroup3 in array6)
			{
				if (spawnInfoGroup3.processed)
				{
					continue;
				}
				Prefab<MonumentInfo> prefab3 = spawnInfoGroup3.prefab;
				MonumentInfo component = prefab3.Component;
				if ((Object)(object)component == (Object)null || World.Size < component.MinWorldSize)
				{
					continue;
				}
				int num4 = 0;
				Vector3 val = Vector3.zero;
				Vector3 val2 = Vector3.zero;
				Vector3 val3 = Vector3.zero;
				TerrainPathConnect[] componentsInChildren = prefab3.Object.GetComponentsInChildren<TerrainPathConnect>(true);
				foreach (TerrainPathConnect terrainPathConnect in componentsInChildren)
				{
					if (terrainPathConnect.Type == InfrastructureType.Rail)
					{
						switch (num4)
						{
						case 0:
							val2 = ((Component)terrainPathConnect).transform.position;
							break;
						case 1:
							val3 = ((Component)terrainPathConnect).transform.position;
							break;
						}
						val += ((Component)terrainPathConnect).transform.position;
						num4++;
					}
				}
				Vector3 val4 = val3 - val2;
				Vector3 normalized = ((Vector3)(ref val4)).normalized;
				val4 = rot90 * normalized;
				Vector3 normalized2 = ((Vector3)(ref val4)).normalized;
				if (num4 > 1)
				{
					val /= (float)num4;
				}
				foreach (PathList rail in TerrainMeta.Path.Rails)
				{
					PathInterpolator path = rail.Path;
					float num6 = TangentInterval / 2;
					float num7 = 5f;
					float num8 = 5f;
					float num9 = path.StartOffset + num8;
					float num10 = path.Length - path.EndOffset - num8;
					for (float num11 = num9; num11 <= num10; num11 += num7)
					{
						Vector3 val5 = (rail.Spline ? path.GetPointCubicHermite(num11) : path.GetPoint(num11));
						val4 = path.GetPoint(num11 + num6) - path.GetPoint(num11 - num6);
						Vector3 normalized3 = ((Vector3)(ref val4)).normalized;
						for (int num12 = Mathf.RoundToInt((float)PositionOffset); num12 <= Mathf.CeilToInt((float)PositionOffset * 1.5f); num12 += 25)
						{
							for (int num13 = -1; num13 <= 1; num13 += 2)
							{
								Quaternion val6 = Quaternion.LookRotation((float)num13 * Vector3Ex.XZ3D(normalized3));
								Vector3 val7 = val5;
								Quaternion val8 = val6;
								Vector3 localScale = prefab3.Object.transform.localScale;
								val8 *= Quaternion.LookRotation(normalized);
								val7 -= val8 * (val + (float)num12 * normalized2);
								if (!(GetDistanceToAboveGroundRail(val7) < (float)PositionOffset * 0.5f))
								{
									SpawnInfo item = new SpawnInfo
									{
										prefab = prefab3,
										position = val7,
										rotation = val8,
										scale = localScale
									};
									spawnInfoGroup3.candidates.Add(item);
								}
							}
						}
					}
				}
				spawnInfoGroup3.processed = true;
			}
			int num14 = 0;
			List<SpawnInfo> list2 = new List<SpawnInfo>();
			int num15 = 0;
			List<SpawnInfo> list3 = new List<SpawnInfo>();
			for (int num16 = 0; num16 < 8; num16++)
			{
				num14 = 0;
				list2.Clear();
				ArrayEx.Shuffle(array5, ref seed);
				array6 = array5;
				foreach (SpawnInfoGroup spawnInfoGroup4 in array6)
				{
					Prefab<MonumentInfo> prefab4 = spawnInfoGroup4.prefab;
					MonumentInfo component2 = prefab4.Component;
					if ((Object)(object)component2 == (Object)null || World.Size < component2.MinWorldSize)
					{
						continue;
					}
					DungeonGridInfo dungeonEntrance = component2.DungeonEntrance;
					int num17 = (int)((!Object.op_Implicit((Object)(object)prefab4.Parameters)) ? PrefabPriority.Low : (prefab4.Parameters.Priority + 1));
					int num18 = 100000 * num17 * num17 * num17 * num17;
					int num19 = 0;
					int num20 = 0;
					SpawnInfo item2 = default(SpawnInfo);
					ListEx.Shuffle<SpawnInfo>(spawnInfoGroup4.candidates, ref seed);
					for (int num21 = 0; num21 < spawnInfoGroup4.candidates.Count; num21++)
					{
						SpawnInfo spawnInfo = spawnInfoGroup4.candidates[num21];
						DistanceInfo distanceInfo = GetDistanceInfo(list2, prefab4, spawnInfo.position, spawnInfo.rotation, spawnInfo.scale);
						if (distanceInfo.minDistanceSameType < (float)MinDistanceSameType || distanceInfo.minDistanceDifferentType < (float)MinDistanceDifferentType)
						{
							continue;
						}
						int num22 = num18;
						if (distanceInfo.minDistanceSameType != float.MaxValue)
						{
							if (DistanceSameType == DistanceMode.Min)
							{
								num22 -= Mathf.RoundToInt(distanceInfo.minDistanceSameType * distanceInfo.minDistanceSameType * 2f);
							}
							else if (DistanceSameType == DistanceMode.Max)
							{
								num22 += Mathf.RoundToInt(distanceInfo.minDistanceSameType * distanceInfo.minDistanceSameType * 2f);
							}
						}
						if (distanceInfo.minDistanceDifferentType != float.MaxValue)
						{
							if (DistanceDifferentType == DistanceMode.Min)
							{
								num22 -= Mathf.RoundToInt(distanceInfo.minDistanceDifferentType * distanceInfo.minDistanceDifferentType);
							}
							else if (DistanceDifferentType == DistanceMode.Max)
							{
								num22 += Mathf.RoundToInt(distanceInfo.minDistanceDifferentType * distanceInfo.minDistanceDifferentType);
							}
						}
						if (!component2.CheckPlacement(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale))
						{
							num22 /= 2;
						}
						if (num22 <= num20 || !prefab4.ApplyTerrainFilters(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale) || !prefab4.ApplyTerrainAnchors(ref spawnInfo.position, spawnInfo.rotation, spawnInfo.scale, Filter))
						{
							continue;
						}
						if (Object.op_Implicit((Object)(object)dungeonEntrance))
						{
							Vector3 val9 = spawnInfo.position + spawnInfo.rotation * Vector3.Scale(spawnInfo.scale, ((Component)dungeonEntrance).transform.position);
							Vector3 val10 = dungeonEntrance.SnapPosition(val9);
							ref Vector3 position = ref spawnInfo.position;
							position += val10 - val9;
							if (!dungeonEntrance.IsValidSpawnPosition(val10))
							{
								continue;
							}
						}
						if (prefab4.ApplyTerrainChecks(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale, Filter) && prefab4.ApplyWaterChecks(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale) && prefab4.ApplyEnvironmentVolumeChecks(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale) && !prefab4.CheckEnvironmentVolumes(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale, EnvironmentType.Underground | EnvironmentType.TrainTunnels))
						{
							num20 = num22;
							item2 = spawnInfo;
							num19++;
							if (num19 >= 8 || DistanceDifferentType == DistanceMode.Any)
							{
								break;
							}
						}
					}
					if (num20 > 0)
					{
						list2.Add(item2);
						num14 += num20;
					}
					if (TargetCount > 0 && list2.Count >= TargetCount)
					{
						break;
					}
				}
				if (num14 > num15)
				{
					num15 = num14;
					GenericsUtil.Swap<List<SpawnInfo>>(ref list2, ref list3);
				}
			}
			foreach (SpawnInfo item3 in list3)
			{
				World.AddPrefab("Monument", item3.prefab, item3.position, item3.rotation, item3.scale);
			}
		}
	}

	private DistanceInfo GetDistanceInfo(List<SpawnInfo> spawns, Prefab<MonumentInfo> prefab, Vector3 monumentPos, Quaternion monumentRot, Vector3 monumentScale)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		DistanceInfo result = new DistanceInfo
		{
			minDistanceDifferentType = float.MaxValue,
			maxDistanceDifferentType = float.MinValue,
			minDistanceSameType = float.MaxValue,
			maxDistanceSameType = float.MinValue
		};
		OBB val = default(OBB);
		((OBB)(ref val))._002Ector(monumentPos, monumentScale, monumentRot, prefab.Component.Bounds);
		if ((Object)(object)TerrainMeta.Path != (Object)null)
		{
			foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
			{
				if (!prefab.Component.HasDungeonLink || (!monument.HasDungeonLink && monument.WantsDungeonLink))
				{
					float num = monument.SqrDistance(val);
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
			if (result.minDistanceDifferentType != float.MaxValue)
			{
				result.minDistanceDifferentType = Mathf.Sqrt(result.minDistanceDifferentType);
			}
			if (result.maxDistanceDifferentType != float.MinValue)
			{
				result.maxDistanceDifferentType = Mathf.Sqrt(result.maxDistanceDifferentType);
			}
		}
		if (spawns != null)
		{
			foreach (SpawnInfo spawn in spawns)
			{
				OBB val2 = new OBB(spawn.position, spawn.scale, spawn.rotation, spawn.prefab.Component.Bounds);
				float num2 = ((OBB)(ref val2)).SqrDistance(val);
				if (num2 < result.minDistanceSameType)
				{
					result.minDistanceSameType = num2;
				}
				if (num2 > result.maxDistanceSameType)
				{
					result.maxDistanceSameType = num2;
				}
			}
			if (prefab.Component.HasDungeonLink)
			{
				foreach (MonumentInfo monument2 in TerrainMeta.Path.Monuments)
				{
					if (monument2.HasDungeonLink || !monument2.WantsDungeonLink)
					{
						float num3 = monument2.SqrDistance(val);
						if (num3 < result.minDistanceSameType)
						{
							result.minDistanceSameType = num3;
						}
						if (num3 > result.maxDistanceSameType)
						{
							result.maxDistanceSameType = num3;
						}
					}
				}
				foreach (DungeonGridInfo dungeonGridEntrance in TerrainMeta.Path.DungeonGridEntrances)
				{
					float num4 = dungeonGridEntrance.SqrDistance(monumentPos);
					if (num4 < result.minDistanceSameType)
					{
						result.minDistanceSameType = num4;
					}
					if (num4 > result.maxDistanceSameType)
					{
						result.maxDistanceSameType = num4;
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
		}
		return result;
	}

	private float GetDistanceToAboveGroundRail(Vector3 pos)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		if (Object.op_Implicit((Object)(object)TerrainMeta.Path))
		{
			foreach (PathList rail in TerrainMeta.Path.Rails)
			{
				Vector3[] points = rail.Path.Points;
				foreach (Vector3 val in points)
				{
					num = Mathf.Min(num, Vector3Ex.Distance2D(val, pos));
				}
			}
		}
		return num;
	}
}
