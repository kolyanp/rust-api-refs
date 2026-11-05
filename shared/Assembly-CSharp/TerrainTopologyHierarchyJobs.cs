using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
internal static class TerrainTopologyHierarchyJobs
{
	[BurstCompile]
	internal struct BuildLeavesJob : IJobParallelFor
	{
		public ReadOnly<int> Src;

		public int Res;

		public int Width;

		[NativeDisableParallelForRestriction]
		public NativeArray<int> Summary;

		public void Execute(int leafZ)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < Width; i++)
			{
				Summary[leafZ * Width + i] = OrLeaf(Src, Res, i, leafZ);
			}
		}
	}

	[BurstCompile]
	internal struct BuildParentsJob : IJob
	{
		public NativeArray<int> Summary;

		[NativeDisableContainerSafetyRestriction]
		public ReadOnly<TerrainTopologyHierarchy.Level> Levels;

		public void Execute()
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 1; i < Levels.Length; i++)
			{
				TerrainTopologyHierarchy.Level level = Levels[i];
				for (int j = 0; j < level.Width; j++)
				{
					for (int k = 0; k < level.Width; k++)
					{
						Summary[level.Index(k, j)] = OrChildren(Summary, Levels, i, k, j);
					}
				}
			}
		}
	}

	[BurstCompile]
	internal struct RefreshRegionJob : IJob
	{
		public ReadOnly<int> Src;

		public int Res;

		public NativeArray<int> Summary;

		[NativeDisableContainerSafetyRestriction]
		public ReadOnly<TerrainTopologyHierarchy.Level> Levels;

		public int XMin;

		public int ZMin;

		public int XMax;

		public int ZMax;

		public NativeReference<int> HighestChangedLevel;

		public void Execute()
		{
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			HighestChangedLevel.Value = -1;
			int num = math.max(XMin, 0);
			int num2 = math.max(ZMin, 0);
			int num3 = math.min(XMax, Res - 1);
			int num4 = math.min(ZMax, Res - 1);
			if (num > num3 || num2 > num4)
			{
				return;
			}
			int num5 = int.MaxValue;
			int num6 = int.MaxValue;
			int num7 = -1;
			int num8 = -1;
			TerrainTopologyHierarchy.Level level = Levels[0];
			for (int i = num2 / 4; i <= num4 / 4; i++)
			{
				for (int j = num / 4; j <= num3 / 4; j++)
				{
					int num9 = OrLeaf(Src, Res, j, i);
					int num10 = level.Index(j, i);
					if (Summary[num10] != num9)
					{
						Summary[num10] = num9;
						num5 = math.min(num5, j);
						num6 = math.min(num6, i);
						num7 = math.max(num7, j);
						num8 = math.max(num8, i);
					}
				}
			}
			for (int k = 1; k < Levels.Length; k++)
			{
				if (num7 < 0)
				{
					break;
				}
				HighestChangedLevel.Value = k - 1;
				int num11 = num5 / 2;
				int num12 = num6 / 2;
				int num13 = num7 / 2;
				int num14 = num8 / 2;
				num5 = int.MaxValue;
				num6 = int.MaxValue;
				num7 = -1;
				num8 = -1;
				TerrainTopologyHierarchy.Level level2 = Levels[k];
				for (int l = num12; l <= num14; l++)
				{
					for (int m = num11; m <= num13; m++)
					{
						int num15 = OrChildren(Summary, Levels, k, m, l);
						int num16 = level2.Index(m, l);
						if (Summary[num16] != num15)
						{
							Summary[num16] = num15;
							num5 = math.min(num5, m);
							num6 = math.min(num6, l);
							num7 = math.max(num7, m);
							num8 = math.max(num8, l);
						}
					}
				}
			}
			if (num7 >= 0)
			{
				HighestChangedLevel.Value = Levels.Length - 1;
			}
		}
	}

	private static int OrLeaf(ReadOnly<int> src, int res, int leafX, int leafZ)
	{
		int num = leafX * 4;
		int num2 = leafZ * 4;
		int num3 = math.min(num + 4, res);
		int num4 = math.min(num2 + 4, res);
		int num5 = 0;
		for (int i = num2; i < num4; i++)
		{
			for (int j = num; j < num3; j++)
			{
				num5 |= src[i * res + j];
			}
		}
		return num5;
	}

	private static int OrChildren(NativeArray<int> summary, ReadOnly<TerrainTopologyHierarchy.Level> levels, int level, int parentX, int parentZ)
	{
		TerrainTopologyHierarchy.Level level2 = levels[level - 1];
		int num = math.min(parentX * 2 + 2, level2.Width);
		int num2 = math.min(parentZ * 2 + 2, level2.Width);
		int num3 = 0;
		for (int i = parentZ * 2; i < num2; i++)
		{
			for (int j = parentX * 2; j < num; j++)
			{
				num3 |= summary[level2.Index(j, i)];
			}
		}
		return num3;
	}
}
