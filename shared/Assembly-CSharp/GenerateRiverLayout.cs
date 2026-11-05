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
		//IL_06a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0686: Unknown result type (might be due to invalid IL or missing references)
		//IL_0713: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_076c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0771: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_09dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_080b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0810: Unknown result type (might be due to invalid IL or missing references)
		//IL_078a: Unknown result type (might be due to invalid IL or missing references)
		//IL_078f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0791: Unknown result type (might be due to invalid IL or missing references)
		//IL_0793: Unknown result type (might be due to invalid IL or missing references)
		//IL_0795: Unknown result type (might be due to invalid IL or missing references)
		//IL_079a: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_08a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_0482: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_0496: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_05de: Unknown result type (might be due to invalid IL or missing references)
		//IL_060e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		//IL_0615: Unknown result type (might be due to invalid IL or missing references)
		//IL_0617: Unknown result type (might be due to invalid IL or missing references)
		//IL_061c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		//IL_0627: Unknown result type (might be due to invalid IL or missing references)
		//IL_0629: Unknown result type (might be due to invalid IL or missing references)
		//IL_062e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0632: Unknown result type (might be due to invalid IL or missing references)
		//IL_063c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0641: Unknown result type (might be due to invalid IL or missing references)
		//IL_0645: Unknown result type (might be due to invalid IL or missing references)
		//IL_064a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0606: Unknown result type (might be due to invalid IL or missing references)
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
			Vector3[] array = new Vector3[4]
			{
				new Vector3(-1f, 0f, -1f),
				new Vector3(-1f, 0f, 1f),
				new Vector3(1f, 0f, -1f),
				new Vector3(1f, 0f, 1f)
			};
			for (float num2 = TerrainMeta.Center.z + 250f; num2 < TerrainMeta.Max.z - 750f; num2 += 5f)
			{
				for (float num3 = TerrainMeta.Center.x + 250f; num3 < TerrainMeta.Max.x - 750f; num3 += 5f)
				{
					Vector3[] array2 = array;
					foreach (Vector3 val in array2)
					{
						Vector3 val2 = new Vector3(val.x * num3, 0f, val.z * num2);
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
								if (Vector3Ex.SqrMagnitude2D(val2 - val3) < 270400f)
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
						Vector2 normalized = val4.normalized;
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
								Line val6 = new Line(list2[l], list2[l + 1]);
								Vector3 val7 = val6.ClosestPoint(val2);
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
							Vector3 val8 = new Vector3(val5.x, 0f, val5.y);
							Vector3 val9 = val8 * (radius * 1.5f);
							Vector3 val10 = val8 * (radius2 + 1f + 64f);
							Vector3 val11 = rot90 * val8;
							Vector3 val12 = val11 * (radius * 1.5f);
							Vector3 val13 = val11 * (radius2 + 1f + 64f);
							int mask = 3742724;
							int num11 = 128;
							int num12 = 128;
							bool flag3 = topologyMap.HasTopology(val2, radius + 1f + 64f, mask);
							int num13 = topologyMap.GetTopology(val2) & topologyMap.GetTopology(val2 - val9) & topologyMap.GetTopology(val2 + val9) & topologyMap.GetTopology(val2 + val10) & topologyMap.GetTopology(val2 - val12) & topologyMap.GetTopology(val2 - val13) & topologyMap.GetTopology(val2 + val12) & topologyMap.GetTopology(val2 + val13);
							int topology = topologyMap.GetTopology(val2);
							if (flag3)
							{
								break;
							}
							if ((num13 & num11) != 0)
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
							if ((topology & num12) != 0 || val2.y < 0f)
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
							val4 = Vector2.Lerp(val14, val4.normalized, 0.025f);
							normalized = val4.normalized;
							num4 = num8;
						}
						list2.Clear();
					}
				}
			}
			list.Sort((PathList a, PathList b) => b.Path.Points.Length.CompareTo(a.Path.Points.Length));
			int num14 = (int)(World.Size / 16);
			bool[,] array3 = new bool[num14, num14];
			int num15 = 0;
			for (int num16 = 0; num16 < list.Count; num16++)
			{
				PathList pathList2 = list[num16];
				bool flag4 = biomeMap.GetBiomeMaxType(pathList2.Path.GetEndPoint()) == 16;
				if (num15 >= num && !flag4)
				{
					list.RemoveAt(num16--);
					continue;
				}
				bool flag5 = false;
				for (int num17 = 0; num17 < num16; num17++)
				{
					PathList pathList3 = list[num17];
					Vector3[] array2 = pathList2.Path.Points;
					foreach (Vector3 val15 in array2)
					{
						Vector3[] points = pathList3.Path.Points;
						foreach (Vector3 val16 in points)
						{
							Vector3 val17 = val15 - val16;
							if (val17.sqrMagnitude < 270400f)
							{
								list.RemoveAt(num16--);
								flag5 = true;
							}
							if (flag5)
							{
								break;
							}
						}
						if (flag5)
						{
							break;
						}
					}
					if (flag5)
					{
						break;
					}
				}
				if (flag5)
				{
					continue;
				}
				for (int num18 = 0; num18 < pathList2.Path.Points.Length; num18++)
				{
					Vector3 val18 = pathList2.Path.Points[num18];
					int num19 = Mathf.Clamp((int)(TerrainMeta.NormalizeX(val18.x) * (float)num14), 0, num14 - 1);
					int num20 = Mathf.Clamp((int)(TerrainMeta.NormalizeZ(val18.z) * (float)num14), 0, num14 - 1);
					if (array3[num20, num19])
					{
						list.RemoveAt(num16--);
						flag5 = true;
						break;
					}
				}
				if (flag5)
				{
					continue;
				}
				int num21 = -1;
				int num22 = -1;
				for (int num23 = 0; num23 < pathList2.Path.Points.Length; num23++)
				{
					Vector3 val19 = pathList2.Path.Points[num23];
					int num24 = Mathf.Clamp((int)(TerrainMeta.NormalizeX(val19.x) * (float)num14), 0, num14 - 1);
					int num25 = Mathf.Clamp((int)(TerrainMeta.NormalizeZ(val19.z) * (float)num14), 0, num14 - 1);
					if (num21 != -1)
					{
						array3[num25, num21] = true;
					}
					if (num22 != -1)
					{
						array3[num22, num24] = true;
					}
					array3[num25, num24] = true;
					num21 = num24;
					num22 = num25;
				}
				if (!flag4)
				{
					num15++;
				}
			}
			for (int num26 = 0; num26 < list.Count; num26++)
			{
				list[num26].Name = "River " + (TerrainMeta.Path.Rivers.Count + num26);
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

	static GenerateRiverLayout()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
	}
}
