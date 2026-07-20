using System;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRiverLayout : ProceduralComponent
{
	public const float Width = 8f;

	public const float InnerPadding = 1f;

	public const float OuterPadding = 1f;

	public const float InnerFade = 16f;

	public const float OuterFade = 64f;

	public const float RandomScale = 0.75f;

	public const float MeshOffset = -0.5f;

	public const float TerrainOffset = -1.5f;

	private static Quaternion rot90 = Quaternion.Euler(0f, 90f, 0f);

	public override void Process(uint seed)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0687: Unknown result type (might be due to invalid IL or missing references)
		//IL_0714: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_076d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0772: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_09de: Unknown result type (might be due to invalid IL or missing references)
		//IL_080c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0811: Unknown result type (might be due to invalid IL or missing references)
		//IL_078b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0790: Unknown result type (might be due to invalid IL or missing references)
		//IL_0792: Unknown result type (might be due to invalid IL or missing references)
		//IL_0794: Unknown result type (might be due to invalid IL or missing references)
		//IL_0796: Unknown result type (might be due to invalid IL or missing references)
		//IL_079b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_044f: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_048b: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_049b: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Unknown result type (might be due to invalid IL or missing references)
		//IL_060f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_0616: Unknown result type (might be due to invalid IL or missing references)
		//IL_0618: Unknown result type (might be due to invalid IL or missing references)
		//IL_061d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0626: Unknown result type (might be due to invalid IL or missing references)
		//IL_0628: Unknown result type (might be due to invalid IL or missing references)
		//IL_062a: Unknown result type (might be due to invalid IL or missing references)
		//IL_062f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0633: Unknown result type (might be due to invalid IL or missing references)
		//IL_063d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0642: Unknown result type (might be due to invalid IL or missing references)
		//IL_0646: Unknown result type (might be due to invalid IL or missing references)
		//IL_064b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0607: Unknown result type (might be due to invalid IL or missing references)
		if (World.Networked)
		{
			TerrainMeta.Path.Rivers.Clear();
			TerrainMeta.Path.Rivers.AddRange(World.GetPaths("River"));
		}
		else
		{
			if (!World.Config.Rivers)
			{
				return;
			}
			List<PathList> list = new List<PathList>();
			TerrainHeightMap heightMap = TerrainMeta.HeightMap;
			TerrainTopologyMap topologyMap = TerrainMeta.TopologyMap;
			TerrainBiomeMap biomeMap = TerrainMeta.BiomeMap;
			List<Vector3> list2 = new List<Vector3>();
			int num = 3;
			if (World.Size <= 4000)
			{
				num = 2;
			}
			Vector3[] array = (Vector3[])(object)new Vector3[4]
			{
				new Vector3(-1f, 0f, -1f),
				new Vector3(-1f, 0f, 1f),
				new Vector3(1f, 0f, -1f),
				new Vector3(1f, 0f, 1f)
			};
			Vector3 val2 = default(Vector3);
			Line val6 = default(Line);
			Vector3 val8 = default(Vector3);
			for (float num2 = TerrainMeta.Center.z + 250f; num2 < TerrainMeta.Max.z - 750f; num2 += 5f)
			{
				for (float num3 = TerrainMeta.Center.x + 250f; num3 < TerrainMeta.Max.x - 750f; num3 += 5f)
				{
					Vector3[] array2 = array;
					foreach (Vector3 val in array2)
					{
						((Vector3)(ref val2))._002Ector(val.x * num3, 0f, val.z * num2);
						float num4 = (val2.y = heightMap.GetHeight(val2));
						if (val2.y <= 15f)
						{
							continue;
						}
						Vector3 normal = heightMap.GetNormal(val2);
						if (normal.y <= 0.01f || normal.y >= 0.99f)
						{
							continue;
						}
						bool flag = false;
						foreach (PathList item in list)
						{
							Vector3[] points = item.Path.Points;
							foreach (Vector3 val3 in points)
							{
								if (Vector3Ex.SqrMagnitude2D(val2 - val3) < 67600f)
								{
									flag = true;
									break;
								}
							}
							if (flag)
							{
								break;
							}
						}
						if (flag)
						{
							continue;
						}
						Vector2 val4 = Vector3Ex.XZ2D(normal);
						Vector2 normalized = ((Vector2)(ref val4)).normalized;
						float num5 = Vector3.Angle(Vector3.up, normal);
						list2.Add(val2);
						float baseRadius = 4f;
						int num6 = 0;
						for (int k = 0; k < 5000; k++)
						{
							int num7 = k * 4;
							Vector2 val5 = Vector2Ex.Rotate(normalized, Mathf.Sin((float)num7 * (MathF.PI / 180f) * 0.5f) * Mathf.InverseLerp(30f, 10f, num5) * 60f);
							val2.x += val5.x * 4f;
							val2.z += val5.y * 4f;
							bool flag2 = false;
							for (int l = 0; l < list2.Count - 10; l++)
							{
								((Line)(ref val6))._002Ector(list2[l], list2[l + 1]);
								Vector3 val7 = ((Line)(ref val6)).ClosestPoint(val2);
								if (Vector3Ex.SqrMagnitude2D(val2 - val7) < 16900f)
								{
									flag2 = true;
									break;
								}
							}
							if (flag2)
							{
								break;
							}
							float height = heightMap.GetHeight(val2);
							if (height > num4 + 32f)
							{
								break;
							}
							float num8 = Mathf.Min(height, num4);
							float num9 = Mathf.Lerp(0.15f, 0.95f, Mathf.InverseLerp(10f, 0f, num8));
							val2.y = Mathf.Lerp(val2.y, num8, num9);
							float radius = PathList.GetRadius(num7, 0f, baseRadius, 0.75f, scaleWidthWithLength: true);
							float radius2 = PathList.GetRadius(num7, num7, baseRadius, 0.75f, scaleWidthWithLength: true);
							int num10 = Mathf.RoundToInt(radius2 / 4f);
							((Vector3)(ref val8))._002Ector(val5.x, 0f, val5.y);
							Vector3 val9 = val8 * (radius * 1.5f);
							Vector3 val10 = val8 * (radius2 + 1f + 64f);
							Vector3 val11 = rot90 * val8;
							Vector3 val12 = val11 * (radius * 1.5f);
							Vector3 val13 = val11 * (radius2 + 1f + 64f);
							int topology = topologyMap.GetTopology(val2, radius + 1f + 64f);
							int num11 = topologyMap.GetTopology(val2) & topologyMap.GetTopology(val2 - val9) & topologyMap.GetTopology(val2 + val9) & topologyMap.GetTopology(val2 + val10) & topologyMap.GetTopology(val2 - val12) & topologyMap.GetTopology(val2 - val13) & topologyMap.GetTopology(val2 + val12) & topologyMap.GetTopology(val2 + val13);
							int topology2 = topologyMap.GetTopology(val2);
							int num12 = 3742724;
							int num13 = 128;
							int num14 = 128;
							if ((topology & num12) != 0)
							{
								break;
							}
							if ((num11 & num13) != 0)
							{
								list2.Add(val2);
								if (list2.Count >= 62)
								{
									PathList pathList = new PathList("River " + (TerrainMeta.Path.Rivers.Count + list.Count), list2.ToArray());
									pathList.Spline = true;
									pathList.Width = 8f;
									pathList.InnerPadding = 1f;
									pathList.OuterPadding = 1f;
									pathList.InnerFade = 16f;
									pathList.OuterFade = 64f;
									pathList.RandomScale = 0.75f;
									pathList.MeshOffset = -0.5f;
									pathList.TerrainOffset = -1.5f;
									pathList.Topology = 16384;
									pathList.Splat = 128;
									pathList.Start = true;
									pathList.End = true;
									list.Add(pathList);
								}
								break;
							}
							if ((topology2 & num14) != 0 || val2.y < 0f)
							{
								if (num6++ >= num10)
								{
									break;
								}
							}
							else if (num6 > 0)
							{
								break;
							}
							if (k % 4 == 0)
							{
								list2.Add(val2);
							}
							normal = heightMap.GetNormal(val2);
							num5 = Vector3.Angle(Vector3.up, normal);
							Vector2 val14 = normalized;
							val4 = Vector3Ex.XZ2D(normal);
							val4 = Vector2.Lerp(val14, ((Vector2)(ref val4)).normalized, 0.025f);
							normalized = ((Vector2)(ref val4)).normalized;
							num4 = num8;
						}
						list2.Clear();
					}
				}
			}
			list.Sort((PathList a, PathList b) => b.Path.Points.Length.CompareTo(a.Path.Points.Length));
			int num15 = (int)(World.Size / 16);
			bool[,] array3 = new bool[num15, num15];
			int num16 = 0;
			for (int num17 = 0; num17 < list.Count; num17++)
			{
				PathList pathList2 = list[num17];
				bool flag3 = biomeMap.GetBiomeMaxType(pathList2.Path.GetEndPoint()) == 16;
				if (num16 >= num && !flag3)
				{
					list.RemoveAt(num17--);
					continue;
				}
				bool flag4 = false;
				for (int num18 = 0; num18 < num17; num18++)
				{
					PathList pathList3 = list[num18];
					Vector3[] array2 = pathList2.Path.Points;
					foreach (Vector3 val15 in array2)
					{
						Vector3[] points = pathList3.Path.Points;
						foreach (Vector3 val16 in points)
						{
							Vector3 val17 = val15 - val16;
							if (((Vector3)(ref val17)).sqrMagnitude < 67600f)
							{
								list.RemoveAt(num17--);
								flag4 = true;
							}
							if (flag4)
							{
								break;
							}
						}
						if (flag4)
						{
							break;
						}
					}
					if (flag4)
					{
						break;
					}
				}
				if (flag4)
				{
					continue;
				}
				for (int num19 = 0; num19 < pathList2.Path.Points.Length; num19++)
				{
					Vector3 val18 = pathList2.Path.Points[num19];
					int num20 = Mathf.Clamp((int)(TerrainMeta.NormalizeX(val18.x) * (float)num15), 0, num15 - 1);
					int num21 = Mathf.Clamp((int)(TerrainMeta.NormalizeZ(val18.z) * (float)num15), 0, num15 - 1);
					if (array3[num21, num20])
					{
						list.RemoveAt(num17--);
						flag4 = true;
						break;
					}
				}
				if (flag4)
				{
					continue;
				}
				int num22 = -1;
				int num23 = -1;
				for (int num24 = 0; num24 < pathList2.Path.Points.Length; num24++)
				{
					Vector3 val19 = pathList2.Path.Points[num24];
					int num25 = Mathf.Clamp((int)(TerrainMeta.NormalizeX(val19.x) * (float)num15), 0, num15 - 1);
					int num26 = Mathf.Clamp((int)(TerrainMeta.NormalizeZ(val19.z) * (float)num15), 0, num15 - 1);
					if (num22 != -1)
					{
						array3[num26, num22] = true;
					}
					if (num23 != -1)
					{
						array3[num23, num25] = true;
					}
					array3[num26, num25] = true;
					num22 = num25;
					num23 = num26;
				}
				if (!flag3)
				{
					num16++;
				}
			}
			for (int num27 = 0; num27 < list.Count; num27++)
			{
				list[num27].Name = "River " + (TerrainMeta.Path.Rivers.Count + num27);
			}
			foreach (PathList item2 in list)
			{
				item2.Path.Smoothen(4, new Vector3(1f, 0f, 1f));
				item2.Path.Smoothen(8, new Vector3(0f, 1f, 0f));
				item2.Path.Resample(7.5f);
				item2.Path.RecalculateTangents();
			}
			TerrainMeta.Path.Rivers.AddRange(list);
		}
	}
}
