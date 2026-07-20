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
	public NativeArray<float> MinTerrainHeightMap;

	public ReadOnly<int> TopologyMap;

	public int TopologyMapRes;

	public float OceanHeight;

	public float TerrainOneOverSizeX;

	public void Execute(int index)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		float num = TerrainHeightMap[index];
		if (!(num < OceanHeight))
		{
			int num2 = index % HeightMapRes;
			int num3 = index / HeightMapRes;
			float normX = ((float)num2 - 0.5f) / (float)HeightMapRes;
			float normZ = ((float)num3 - 0.5f) / (float)HeightMapRes;
			bool flag = (TerrainTopologyMapJobUtil.GetTopologyRadius(TopologyMap, TopologyMapRes, TerrainOneOverSizeX, 0f, normX, normZ) & 0x14080) != 0;
			float num4 = 8f;
			float num5 = 8f;
			while (num5 > 0f && !flag && (TerrainTopologyMapJobUtil.GetTopologyRadius(TopologyMap, TopologyMapRes, TerrainOneOverSizeX, num5, normX, normZ) & 0x3C198) != 0)
			{
				num4 = num5;
				num5 -= 0.25f;
			}
			float num6 = (flag ? 0f : math.unlerp(0f, 8f, num4));
			num = math.max(OceanHeight, num - 1f * num6);
		}
		MinTerrainHeightMap[index] = num;
	}
}
