using TerrainTopologyMapJobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct CalcMinHeightMapJob : IJobParallelFor
{
	public ReadOnly<float> TerrainHeightMap;

	public int HeightMapRes;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> MinTerrainHeightMap;

	public TerrainTopologyHierarchy.Query TopologyMap;

	public float OceanHeight;

	public float TerrainOneOverSizeX;

	public void Execute(int z)
	{
		for (int i = 0; i < HeightMapRes; i++)
		{
			int num = z * HeightMapRes + i;
			float num2 = TerrainHeightMap[num];
			if (!(num2 < OceanHeight))
			{
				float normX = ((float)i - 0.5f) / (float)HeightMapRes;
				float normZ = ((float)z - 0.5f) / (float)HeightMapRes;
				bool flag = TerrainTopologyMapJobUtil.GetTopologyRadius(in TopologyMap, TerrainOneOverSizeX, 0f, normX, normZ, 82048, stopOnAny: true) != 0;
				float num3 = 8f;
				float num4 = 8f;
				while (num4 > 0f && !flag && TerrainTopologyMapJobUtil.GetTopologyRadius(in TopologyMap, TerrainOneOverSizeX, num4, normX, normZ, 246168, stopOnAny: true) != 0)
				{
					num3 = num4;
					num4 -= 0.25f;
				}
				float num5 = (flag ? 0f : math.unlerp(0f, 8f, num3));
				num2 = math.max(OceanHeight, num2 - 1f * num5);
			}
			MinTerrainHeightMap[num] = num2;
		}
	}
}
