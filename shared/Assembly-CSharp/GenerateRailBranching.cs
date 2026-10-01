using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateRailBranching : ProceduralComponent
{
	public const float Width = 4f;

	public const float InnerPadding = 1f;

	public const float OuterPadding = 1f;

	public const float InnerFade = 1f;

	public const float OuterFade = 32f;

	public const float RandomScale = 1f;

	public const float MeshOffset = 0f;

	public const float TerrainOffset = -0.125f;

	private static Quaternion rot90 = Quaternion.Euler(0f, 90f, 0f);

	private const int MaxDepth = 250000;

	private PathList CreateSegment(int number, Vector3[] points)
	{
		return new PathList("Rail " + number, points)
		{
			Spline = true,
			Width = 4f,
			InnerPadding = 1f,
			OuterPadding = 1f,
			InnerFade = 1f,
			OuterFade = 32f,
			RandomScale = 1f,
			MeshOffset = 0f,
			TerrainOffset = -0.125f,
			Topology = 524288,
			Splat = 128,
			Hierarchy = 1
		};
	}

	public override void Process(uint seed)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_081c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0469: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_0516: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0520: Unknown result type (might be due to invalid IL or missing references)
		//IL_0525: Unknown result type (might be due to invalid IL or missing references)
		//IL_0527: Unknown result type (might be due to invalid IL or missing references)
		//IL_0529: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_057d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0582: Unknown result type (might be due to invalid IL or missing references)
		//IL_0584: Unknown result type (might be due to invalid IL or missing references)
		//IL_0589: Unknown result type (might be due to invalid IL or missing references)
		//IL_058b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0590: Unknown result type (might be due to invalid IL or missing references)
		//IL_0592: Unknown result type (might be due to invalid IL or missing references)
		//IL_0597: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		//IL_0615: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Unknown result type (might be due to invalid IL or missing references)
		//IL_061e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0638: Unknown result type (might be due to invalid IL or missing references)
		//IL_063d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0641: Unknown result type (might be due to invalid IL or missing references)
		//IL_0646: Unknown result type (might be due to invalid IL or missing references)
		//IL_0672: Unknown result type (might be due to invalid IL or missing references)
		//IL_0677: Unknown result type (might be due to invalid IL or missing references)
		//IL_067e: Unknown result type (might be due to invalid IL or missing references)
		if (World.Networked)
		{
			return;
		}
		int num = Mathf.RoundToInt(40f);
		int num2 = Mathf.RoundToInt(53.333332f);
		int num3 = Mathf.RoundToInt(40f);
		int num4 = Mathf.RoundToInt(120f);
		float num5 = 120f * 120f;
		List<(PathList, PathInterpolator, int, int)> list = new List<(PathList, PathInterpolator, int, int)>();
		int[,] array = TerrainPath.CreateRailCostmap(ref seed);
		PathFinder pathFinder = new PathFinder(array);
		int length = array.GetLength(0);
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			TerrainPathConnect[] array2 = (from target in ((Component)monument).GetComponentsInChildren<TerrainPathConnect>(true)
				where target.Type == InfrastructureType.Rail
				select target).ToArray();
			foreach (TerrainPathConnect terrainPathConnect in array2)
			{
				pathFinder.PushPointsAdditional.Add(PathFinder.GetPoint(((Component)terrainPathConnect).transform.position, length));
			}
		}
		if (pathFinder.PushPointsAdditional.Count > 0)
		{
			pathFinder.PushDistance = 10;
			pathFinder.PushMultiplier = int.MaxValue;
		}
		List<Vector3> list2 = new List<Vector3>();
		List<Vector3> list3 = new List<Vector3>();
		List<Vector3> list4 = new List<Vector3>();
		HashSet<Vector3> hashSet = new HashSet<Vector3>();
		foreach (PathList rail2 in TerrainMeta.Path.Rails)
		{
			foreach (PathList rail3 in TerrainMeta.Path.Rails)
			{
				if (rail2 == rail3)
				{
					continue;
				}
				Vector3[] points = rail2.Path.Points;
				foreach (Vector3 val in points)
				{
					Vector3[] points2 = rail3.Path.Points;
					foreach (Vector3 val2 in points2)
					{
						Vector3 val3 = val - val2;
						if (val3.sqrMagnitude < num5)
						{
							hashSet.Add(val);
							break;
						}
					}
				}
			}
		}
		foreach (PathList rail4 in TerrainMeta.Path.Rails)
		{
			PathInterpolator path = rail4.Path;
			Vector3[] points3 = path.Points;
			Vector3[] tangents = path.Tangents;
			int num8 = path.MinIndex + 1 + 8;
			int num9 = path.MaxIndex - 1 - 8;
			for (int num10 = num8; num10 <= num9; num10++)
			{
				list2.Clear();
				list3.Clear();
				list4.Clear();
				int num11 = SeedRandom.Range(ref seed, num3, num4);
				int num12 = SeedRandom.Range(ref seed, num, num2);
				int num13 = num10;
				int num14 = num10 + num11;
				if (num14 >= num9)
				{
					continue;
				}
				Vector3 val4 = tangents[num13];
				Vector3 val5 = tangents[num14];
				if (Vector3.Angle(val4, val5) > 30f)
				{
					continue;
				}
				Vector3 val6 = points3[num13];
				Vector3 val7 = points3[num14];
				if (hashSet.Contains(val6) || hashSet.Contains(val7))
				{
					continue;
				}
				PathFinder.Point point = PathFinder.GetPoint(val6, length);
				PathFinder.Point point2 = PathFinder.GetPoint(val7, length);
				num10 += num12;
				PathFinder.Node node = pathFinder.FindPath(point, point2, 250000);
				if (node == null)
				{
					continue;
				}
				PathFinder.Node node2 = null;
				PathFinder.Node node3 = null;
				PathFinder.Node node4 = node;
				while (node4 != null && node4.next != null)
				{
					if (node4 == node.next)
					{
						node2 = node4;
					}
					if (node4.next.next == null)
					{
						node3 = node4;
					}
					node4 = node4.next;
				}
				if (node2 == null || node3 == null)
				{
					continue;
				}
				node = node2;
				node3.next = null;
				for (int num15 = 0; num15 < 8; num15++)
				{
					int num16 = num13 + (num15 + 1 - 8);
					int num17 = num14 + num15;
					list2.Add(points3[num16]);
					list3.Add(points3[num17]);
				}
				list4.AddRange(list2);
				for (PathFinder.Node node5 = node2; node5 != null; node5 = node5.next)
				{
					float normX = ((float)node5.point.x + 0.5f) / (float)length;
					float normZ = ((float)node5.point.y + 0.5f) / (float)length;
					float num18 = TerrainMeta.DenormalizeX(normX);
					float num19 = TerrainMeta.DenormalizeZ(normZ);
					float num20 = Mathf.Max(TerrainMeta.HeightMap.GetHeight(normX, normZ), 1f);
					list4.Add(new Vector3(num18, num20, num19));
				}
				list4.AddRange(list3);
				int num21 = 8;
				int num22 = list4.Count - 1 - 8;
				Vector3 val8 = Vector3.Normalize(list4[num21 + 16] - list4[num21]);
				Vector3 val9 = Vector3.Normalize(list4[num22] - list4[num22 - 16]);
				Vector3 val10 = Vector3.Normalize(points3[num13 + 16] - points3[num13]);
				Vector3 val11 = Vector3.Normalize(points3[num14] - points3[(num14 - 16 + points3.Length) % points3.Length]);
				float num23 = Vector3.SignedAngle(val10, val8, Vector3.up);
				float num24 = 0f - Vector3.SignedAngle(val11, val9, Vector3.up);
				if (Mathf.Sign(num23) != Mathf.Sign(num24) || Mathf.Abs(num23) > 60f || Mathf.Abs(num24) > 60f)
				{
					continue;
				}
				Vector3 val12 = rot90 * val10;
				Vector3 val13 = rot90 * val11;
				if (num23 < 0f)
				{
					val12 = -val12;
				}
				if (num24 < 0f)
				{
					val13 = -val13;
				}
				for (int num25 = 0; num25 < 16; num25++)
				{
					int num26 = num25;
					int num27 = list4.Count - num25 - 1;
					float num28 = Mathf.InverseLerp(0f, 8f, (float)num25);
					float num29 = Mathf.SmoothStep(0f, 2f, num28) * 4f;
					List<Vector3> list5 = list4;
					int num6 = num26;
					list5[num6] += val12 * num29;
					list5 = list4;
					num6 = num27;
					list5[num6] += val13 * num29;
				}
				bool flag = false;
				bool flag2 = false;
				foreach (Vector3 item4 in list4)
				{
					bool blocked = TerrainMeta.PlacementMap.GetBlocked(item4);
					if (!flag2 && !flag && !blocked)
					{
						flag = true;
					}
					if ((flag && !flag2) & blocked)
					{
						flag2 = true;
					}
					if ((flag & flag2) && !blocked)
					{
						list4.Clear();
						break;
					}
				}
				if (list4.Count != 0)
				{
					if (list4.Count >= 2)
					{
						int number = TerrainMeta.Path.Rails.Count + list.Count;
						PathList pathList = CreateSegment(number, list4.ToArray());
						pathList.Start = false;
						pathList.End = false;
						pathList.ProcgenStartNode = node2;
						pathList.ProcgenEndNode = node3;
						list.Add((pathList, path, num13 + 1 - 8, num14 + 8 - 1));
					}
					num10 += num11;
				}
			}
		}
		for (int num30 = 0; num30 < list.Count; num30++)
		{
			(PathList, PathInterpolator, int, int) tuple = list[num30];
			PathList rail = tuple.Item1;
			PathInterpolator item = tuple.Item2;
			int item2 = tuple.Item3;
			int item3 = tuple.Item4;
			Func<int, float> filter = (int i) =>
			{
				float num31 = Mathf.InverseLerp(0f, 8f, (float)i);
				float num32 = Mathf.InverseLerp((float)rail.Path.DefaultMaxIndex, (float)(rail.Path.DefaultMaxIndex - 8), (float)i);
				return Mathf.SmoothStep(0f, 1f, Mathf.Min(num31, num32));
			};
			rail.Path.Smoothen(32, new Vector3(1f, 0f, 1f), filter);
			rail.Path.Smoothen(64, new Vector3(0f, 1f, 0f), filter);
			rail.Path.Resample(7.5f);
			if (!RailJunction.TryAlignBranch(rail.Path, item, item2, item3, 4f))
			{
				list.RemoveAt(num30--);
			}
			else
			{
				rail.AdjustPlacementMap(20f);
			}
		}
		foreach (var item5 in list)
		{
			TerrainMeta.Path.Rails.Add(item5.Item1);
		}
	}

	static GenerateRailBranching()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
	}
}
