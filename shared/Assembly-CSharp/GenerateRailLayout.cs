using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateRailLayout : ProceduralComponent
{
	private class PathNode
	{
		public MonumentInfo monument;

		public TerrainPathConnect target;

		public PathFinder.Node node;
	}

	private class PathSegment
	{
		public PathFinder.Node start;

		public PathFinder.Node end;

		public TerrainPathConnect origin;

		public TerrainPathConnect target;
	}

	public const float Width = 4f;

	public const float InnerPadding = 1f;

	public const float OuterPadding = 1f;

	public const float InnerFade = 1f;

	public const float OuterFade = 32f;

	public const float RandomScale = 1f;

	public const float MeshOffset = 0f;

	public const float TerrainOffset = -0.125f;

	private static Quaternion rot90;

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
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0506: Unknown result type (might be due to invalid IL or missing references)
		//IL_0508: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0511: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06be: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0857: Unknown result type (might be due to invalid IL or missing references)
		//IL_0865: Unknown result type (might be due to invalid IL or missing references)
		//IL_086a: Unknown result type (might be due to invalid IL or missing references)
		//IL_086f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0874: Unknown result type (might be due to invalid IL or missing references)
		//IL_0876: Unknown result type (might be due to invalid IL or missing references)
		//IL_087b: Unknown result type (might be due to invalid IL or missing references)
		//IL_087d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0882: Unknown result type (might be due to invalid IL or missing references)
		//IL_0804: Unknown result type (might be due to invalid IL or missing references)
		//IL_0809: Unknown result type (might be due to invalid IL or missing references)
		//IL_080b: Unknown result type (might be due to invalid IL or missing references)
		//IL_080d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0814: Unknown result type (might be due to invalid IL or missing references)
		//IL_0816: Unknown result type (might be due to invalid IL or missing references)
		//IL_081b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0738: Unknown result type (might be due to invalid IL or missing references)
		//IL_073e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0893: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08da: Unknown result type (might be due to invalid IL or missing references)
		//IL_091f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0924: Unknown result type (might be due to invalid IL or missing references)
		//IL_0928: Unknown result type (might be due to invalid IL or missing references)
		//IL_092d: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0f: Unknown result type (might be due to invalid IL or missing references)
		if (World.Networked)
		{
			TerrainMeta.Path.Rails.Clear();
			TerrainMeta.Path.Rails.AddRange(World.GetPaths("Rail"));
			{
				foreach (PathList rail in TerrainMeta.Path.Rails)
				{
					Func<int, float> filter = delegate(int i)
					{
						float num20 = Mathf.InverseLerp(0f, 8f, (float)i);
						float num21 = Mathf.InverseLerp((float)rail.Path.DefaultMaxIndex, (float)(rail.Path.DefaultMaxIndex - 8), (float)i);
						return Mathf.SmoothStep(0f, 1f, Mathf.Min(num20, num21));
					};
					Vector3[] points = rail.Path.Points;
					for (int num = 1; num < points.Length - 1; num++)
					{
						Vector3 val = points[num];
						val.y = Mathf.Max(TerrainMeta.HeightMap.GetHeight(val), 1f);
						points[num] = val;
					}
					rail.Path.Smoothen(64, new Vector3(0f, 1f, 0f), filter);
					rail.Path.RecalculateTangents();
				}
				return;
			}
		}
		if (!World.Config.AboveGroundRails)
		{
			return;
		}
		List<PathList> list = new List<PathList>();
		int[,] array = TerrainPath.CreateRailCostmap(ref seed);
		PathFinder pathFinder = new PathFinder(array);
		PathFinder pathFinder2 = new PathFinder(array);
		int length = array.GetLength(0);
		new List<PathSegment>();
		List<PathFinder.Point> list2 = new List<PathFinder.Point>();
		List<PathFinder.Point> list3 = new List<PathFinder.Point>();
		List<PathFinder.Point> list4 = new List<PathFinder.Point>();
		List<Vector3> list5 = new List<Vector3>();
		foreach (PathList rail3 in TerrainMeta.Path.Rails)
		{
			for (PathFinder.Node node = rail3.ProcgenStartNode; node != null; node = node.next)
			{
				list2.Add(node.point);
			}
		}
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			pathFinder.PushPoint = monument.GetPathFinderPoint(length);
			pathFinder.PushRadius = (pathFinder.PushDistance = monument.GetPathFinderRadius(length));
			pathFinder.PushMultiplier = 50000;
			int num2 = int.MaxValue;
			TerrainPathConnect[] array2 = (from target in ((Component)monument).GetComponentsInChildren<TerrainPathConnect>(true)
				where target.Type == InfrastructureType.Rail
				select target).OrderBy(delegate(TerrainPathConnect target)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return DistanceToRail(((Component)target).transform.position);
			}).ToArray();
			TerrainPathConnect[] array3 = array2;
			foreach (TerrainPathConnect terrainPathConnect in array3)
			{
				pathFinder.PushPointsAdditional.Clear();
				pathFinder.BlockedPointsAdditional.Clear();
				Vector3 val2 = ((Component)terrainPathConnect).transform.position;
				TerrainPathConnect[] array4 = array2;
				foreach (TerrainPathConnect terrainPathConnect2 in array4)
				{
					if (!((Object)(object)terrainPathConnect == (Object)(object)terrainPathConnect2))
					{
						Vector3 position = ((Component)terrainPathConnect2).transform.position;
						PathFinder.Point point = PathFinder.GetPoint(((Component)terrainPathConnect2).transform.position, length);
						pathFinder.PushPointsAdditional.Add(point);
						val2 += position;
					}
				}
				val2 /= (float)array2.Length;
				Vector3 val3;
				if (array2.Length <= 1)
				{
					val3 = ((Component)terrainPathConnect).transform.forward;
				}
				else
				{
					Vector3 val4 = ((Component)terrainPathConnect).transform.position - val2;
					val3 = ((Vector3)(ref val4)).normalized;
				}
				Vector3 val5 = val3;
				foreach (PathList item in list)
				{
					pathFinder.PushPointsAdditional.Add(PathFinder.GetPoint(item.Path.GetEndPoint(), length));
					PathFinder.Point point2 = PathFinder.GetPoint(item.Path.GetStartPoint(), length);
					Vector3[] points2 = item.Path.Points;
					for (int num4 = 0; num4 < points2.Length; num4++)
					{
						PathFinder.Point point3 = PathFinder.GetPoint(points2[num4], length);
						pathFinder.BlockedPointsAdditional.Add(point3);
						pathFinder.BlockedPointsAdditional.Add(new PathFinder.Point(point3.x, point2.y));
						pathFinder.BlockedPointsAdditional.Add(new PathFinder.Point(point2.x, point3.y));
						point2 = point3;
					}
					if (item.ProcgenStartNode != null)
					{
						PathFinder.Point point4 = item.ProcgenStartNode.point;
						for (PathFinder.Node node2 = item.ProcgenStartNode; node2 != null; node2 = node2.next)
						{
							PathFinder.Point point5 = node2.point;
							pathFinder.BlockedPointsAdditional.Add(point5);
							pathFinder.BlockedPointsAdditional.Add(new PathFinder.Point(point5.x, point4.y));
							pathFinder.BlockedPointsAdditional.Add(new PathFinder.Point(point4.x, point5.y));
							point4 = point5;
						}
					}
				}
				list5.Clear();
				Vector3 val6 = ((Component)terrainPathConnect).transform.position;
				Vector3 val7 = ((Component)terrainPathConnect).transform.forward * 7.5f;
				PathFinder.Point point6 = PathFinder.GetPoint(val6, length);
				for (int num5 = 0; (num5 < 8 && pathFinder.Heuristic(point6, list2) > 1) || (num5 < 16 && !pathFinder.IsWalkable(point6)); num5++)
				{
					list5.Add(val6);
					val6 += val7;
					point6 = PathFinder.GetPoint(val6, length);
				}
				if (!pathFinder.IsWalkable(point6))
				{
					continue;
				}
				list3.Clear();
				list3.Add(point6);
				list4.Clear();
				list4.AddRange(list2);
				PathFinder.Node node3 = pathFinder.FindPathDirected(list3, list4, 250000);
				bool flag = false;
				if (node3 == null && list.Count > 0 && num2 != int.MaxValue)
				{
					PathList pathList = list[list.Count - 1];
					list4.Clear();
					for (int num6 = 0; num6 < pathList.Path.Points.Length; num6++)
					{
						list4.Add(PathFinder.GetPoint(pathList.Path.Points[num6], length));
					}
					node3 = pathFinder2.FindPathDirected(list3, list4, 250000);
					flag = true;
				}
				if (node3 == null)
				{
					continue;
				}
				PathFinder.Node node4 = null;
				PathFinder.Node node5 = null;
				PathFinder.Node node6 = node3;
				while (node6 != null && node6.next != null)
				{
					if (node6 == node3.next)
					{
						node4 = node6;
					}
					if (node6.next.next == null)
					{
						node5 = node6;
						node5.next = null;
					}
					node6 = node6.next;
				}
				for (PathFinder.Node node7 = node4; node7 != null; node7 = node7.next)
				{
					float normX = ((float)node7.point.x + 0.5f) / (float)length;
					float normZ = ((float)node7.point.y + 0.5f) / (float)length;
					float num7 = TerrainMeta.DenormalizeX(normX);
					float num8 = TerrainMeta.DenormalizeZ(normZ);
					float num9 = Mathf.Max(TerrainMeta.HeightMap.GetHeight(normX, normZ), 1f);
					list5.Add(new Vector3(num7, num9, num8));
				}
				if (list5.Count < 1)
				{
					continue;
				}
				Vector3 val8 = list5[list5.Count - 1];
				Vector3 val9 = val5;
				PathList pathList2 = null;
				float num10 = float.MaxValue;
				int num11 = -1;
				if (!flag)
				{
					foreach (PathList rail4 in TerrainMeta.Path.Rails)
					{
						Vector3[] points3 = rail4.Path.Points;
						for (int num12 = 0; num12 < points3.Length; num12++)
						{
							float num13 = Vector3.Distance(val8, points3[num12]);
							if (num13 < num10)
							{
								num10 = num13;
								pathList2 = rail4;
								num11 = num12;
							}
						}
					}
				}
				else
				{
					foreach (PathList item2 in list)
					{
						Vector3[] points4 = item2.Path.Points;
						for (int num14 = 0; num14 < points4.Length; num14++)
						{
							float num15 = Vector3.Distance(val8, points4[num14]);
							if (num15 < num10)
							{
								num10 = num15;
								pathList2 = item2;
								num11 = num14;
							}
						}
					}
				}
				int num16 = 1;
				if (!flag)
				{
					Vector3 tangentByIndex = pathList2.Path.GetTangentByIndex(num11);
					num16 = ((Vector3.Angle(tangentByIndex, val9) < Vector3.Angle(-tangentByIndex, val9)) ? 1 : (-1));
					if (num2 != int.MaxValue)
					{
						GenericsUtil.Swap<int>(ref num2, ref num16);
						num16 = -num16;
					}
					else
					{
						num2 = num16;
					}
				}
				Vector3 val10 = Vector3.Normalize(pathList2.Path.GetPointByIndex(num11 + num16 * 8 * 2) - pathList2.Path.GetPointByIndex(num11));
				Vector3 val11 = rot90 * val10;
				if (!flag)
				{
					Vector3 val12 = Vector3.Normalize(list5[list5.Count - 1] - list5[Mathf.Max(0, list5.Count - 1 - 16)]);
					if (0f - Vector3.SignedAngle(val10, val12, Vector3.up) < 0f)
					{
						val11 = -val11;
					}
				}
				for (int num17 = 0; num17 < 8; num17++)
				{
					float num18 = Mathf.InverseLerp(7f, 0f, (float)num17);
					float num19 = Mathf.SmoothStep(0f, 2f, num18) * 4f;
					list5.Add(pathList2.Path.GetPointByIndex(num11 + num16 * num17) + val11 * num19);
				}
				if (list5.Count >= 2)
				{
					int number = TerrainMeta.Path.Rails.Count + list.Count;
					PathList rail2 = CreateSegment(number, list5.ToArray());
					rail2.Start = true;
					rail2.End = false;
					rail2.ProcgenStartNode = node4;
					rail2.ProcgenEndNode = node5;
					Func<int, float> filter2 = delegate(int i)
					{
						float num20 = Mathf.InverseLerp(0f, 8f, (float)i);
						float num21 = Mathf.InverseLerp((float)rail2.Path.DefaultMaxIndex, (float)(rail2.Path.DefaultMaxIndex - 8), (float)i);
						return Mathf.SmoothStep(0f, 1f, Mathf.Min(num20, num21));
					};
					rail2.Path.Smoothen(32, new Vector3(1f, 0f, 1f), filter2);
					rail2.Path.Smoothen(64, new Vector3(0f, 1f, 0f), filter2);
					rail2.Path.Resample(7.5f);
					rail2.Path.RecalculateTangents();
					list.Add(rail2);
				}
			}
		}
		foreach (PathList item3 in list)
		{
			item3.AdjustPlacementMap(20f);
		}
		TerrainMeta.Path.Rails.AddRange(list);
		static float DistanceToRail(Vector3 vec)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			float num20 = float.MaxValue;
			foreach (PathList rail5 in TerrainMeta.Path.Rails)
			{
				Vector3[] points5 = rail5.Path.Points;
				foreach (Vector3 val13 in points5)
				{
					num20 = Mathf.Min(num20, Vector3Ex.Magnitude2D(vec - val13));
				}
			}
			return num20;
		}
	}

	static GenerateRailLayout()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		rot90 = Quaternion.Euler(0f, 90f, 0f);
	}
}
