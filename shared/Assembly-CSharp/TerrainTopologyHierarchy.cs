using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Jobs;

[JsonModel]
public sealed class TerrainTopologyHierarchy : IDisposable
{
	public struct Level
	{
		public int Offset;

		public int Width;

		public readonly int Index(int x, int z)
		{
			return Offset + z * Width + x;
		}
	}

	[BurstCompile]
	public struct Query
	{
		internal ReadOnly<int> src;

		internal int res;

		internal ReadOnly<int> summary;

		internal ReadOnly<Level> levels;

		public unsafe readonly int Or(int x_min, int x_mid, int x_max, int z_min, int z_mid, int z_max, float r2, int mask, bool stopOnAny)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			FixedList512Bytes<int> val = default;
			int num = math.max(x_max - x_min, z_max - z_min) + 1;
			int i;
			for (i = 0; i < levels.Length - 1 && 4 << i < num; i++)
			{
			}
			int num2 = 4 << i;
			for (int j = z_min / num2; j <= z_max / num2; j++)
			{
				for (int k = x_min / num2; k <= x_max / num2; k++)
				{
					int num3 = PackNode(i, k, j);
					val.Add(ref num3);
				}
			}
			int num4 = 0;
			while (val.Length > 0)
			{
				int node = val[val.Length - 1];
				int num3 = val.Length;
				val.Length = num3 - 1;
				UnpackNode(node, out var level, out var x, out var z);
				int num5 = summary.Ptr[levels.Ptr[level].Index(x, z)];
				if ((num5 & mask & ~num4) == 0)
				{
					continue;
				}
				int num6 = 4 << level;
				int num7 = x * num6;
				int num8 = z * num6;
				int num9 = math.min(num7 + num6, res) - 1;
				int num10 = math.min(num8 + num6, res) - 1;
				int num11 = math.max(num7, x_min);
				int num12 = math.max(num8, z_min);
				int num13 = math.min(num9, x_max);
				int num14 = math.min(num10, z_max);
				if (num11 > num13 || num12 > num14)
				{
					continue;
				}
				int num15 = math.clamp(x_mid, num11, num13) - x_mid;
				int num16 = math.clamp(z_mid, num12, num14) - z_mid;
				if ((float)(num15 * num15 + num16 * num16) > r2)
				{
					continue;
				}
				int num17 = math.max(x_mid - num11, num13 - x_mid);
				int num18 = math.max(z_mid - num12, num14 - z_mid);
				if (num11 == num7 && num12 == num8 && num13 == num9 && num14 == num10 && !((float)(num17 * num17 + num18 * num18) > r2))
				{
					num4 |= num5 & mask;
				}
				else if (level == 0)
				{
					num4 |= OrPixels(num11, x_mid, num13, num12, z_mid, num14, r2) & mask;
				}
				else
				{
					int width = levels.Ptr[level - 1].Width;
					int num19 = math.min(x * 2 + 2, width);
					int num20 = math.min(z * 2 + 2, width);
					for (int l = z * 2; l < num20; l++)
					{
						for (int m = x * 2; m < num19; m++)
						{
							num3 = PackNode(level - 1, m, l);
							val.Add(ref num3);
						}
					}
				}
				if (stopOnAny ? (num4 != 0) : (num4 == mask))
				{
					break;
				}
			}
			return num4;
		}

		private unsafe readonly int OrPixels(int x_min, int x_mid, int x_max, int z_min, int z_mid, int z_max, float r2)
		{
			int num = 0;
			for (int i = z_min; i <= z_max; i++)
			{
				int num2 = i - z_mid;
				int num3 = num2 * num2;
				for (int j = x_min; j <= x_max; j++)
				{
					int num4 = j - x_mid;
					if (!((float)(num4 * num4 + num3) > r2))
					{
						num |= src.Ptr[i * res + j];
					}
				}
			}
			return num;
		}

		private static int PackNode(int level, int x, int z)
		{
			return (level << 26) | (x << 13) | z;
		}

		private static void UnpackNode(int node, out int level, out int x, out int z)
		{
			level = node >> 26;
			x = (node >> 13) & 0x1FFF;
			z = node & 0x1FFF;
		}
	}

	public const int LeafSize = 4;

	private readonly int res;

	private NativeArray<int> summary;

	private NativeArray<Level> levels;

	private NativeReference<int> highestRefreshedLevel;

	public int LevelCount => levels.Length;

	public TerrainTopologyHierarchy(int res)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		this.res = res;
		int num = 1;
		for (int num2 = CeilDiv(res, 4); num2 > 1; num2 = CeilDiv(num2, 2))
		{
			num++;
		}
		levels = new NativeArray<Level>(num, (Allocator)4, (NativeArrayOptions)1);
		int num3 = 0;
		int num4 = CeilDiv(res, 4);
		for (int i = 0; i < num; i++)
		{
			levels[i] = new Level
			{
				Offset = num3,
				Width = num4
			};
			num3 += num4 * num4;
			num4 = CeilDiv(num4, 2);
		}
		summary = new NativeArray<int>(num3, (Allocator)4, (NativeArrayOptions)1);
		highestRefreshedLevel = new NativeReference<int>(AllocatorHandle.op_Implicit((Allocator)4), (NativeArrayOptions)1);
	}

	public void Build(ReadOnly<int> src)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		TerrainTopologyHierarchyJobs.BuildLeavesJob jobData = new TerrainTopologyHierarchyJobs.BuildLeavesJob
		{
			Src = src,
			Res = res,
			Summary = summary,
			Width = levels[0].Width
		};
		int width = levels[0].Width;
		JobHandle dependsOn = default;
		JobHandle val = ParallelJobEx.ScheduleParallelByRef<TerrainTopologyHierarchyJobs.BuildLeavesJob>(ref jobData, width, dependsOn);
		TerrainTopologyHierarchyJobs.BuildParentsJob buildParentsJob = new TerrainTopologyHierarchyJobs.BuildParentsJob
		{
			Summary = summary,
			Levels = levels.AsReadOnly()
		};
		dependsOn = IJobExtensions.ScheduleByRef<TerrainTopologyHierarchyJobs.BuildParentsJob>(ref buildParentsJob, val);
		dependsOn.Complete();
	}

	public int RefreshRegion(ReadOnly<int> src, int xMin, int zMin, int xMax, int zMax)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		TerrainTopologyHierarchyJobs.RefreshRegionJob refreshRegionJob = new TerrainTopologyHierarchyJobs.RefreshRegionJob
		{
			Src = src,
			Res = res,
			Summary = summary,
			Levels = levels.AsReadOnly(),
			XMin = xMin,
			ZMin = zMin,
			XMax = xMax,
			ZMax = zMax,
			HighestChangedLevel = highestRefreshedLevel
		};
		IJobExtensions.RunByRef<TerrainTopologyHierarchyJobs.RefreshRegionJob>(ref refreshRegionJob);
		return highestRefreshedLevel.Value;
	}

	public int GetNode(int level, int x, int z)
	{
		return summary[levels[level].Index(x, z)];
	}

	public int GetLevelWidth(int level)
	{
		return levels[level].Width;
	}

	public Query GetQuery(ReadOnly<int> src)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		return new Query
		{
			src = src.AsUnsafeListReadonly<int>(),
			res = res,
			summary = summary.AsReadOnly().AsUnsafeListReadonly<int>(),
			levels = levels.AsReadOnly().AsUnsafeListReadonly<Level>()
		};
	}

	public void Dispose()
	{
		if (summary.IsCreated)
		{
			summary.Dispose();
		}
		if (levels.IsCreated)
		{
			levels.Dispose();
		}
		if (highestRefreshedLevel.IsCreated)
		{
			highestRefreshedLevel.Dispose();
		}
	}

	private static int CeilDiv(int value, int divisor)
	{
		return (value + divisor - 1) / divisor;
	}
}
