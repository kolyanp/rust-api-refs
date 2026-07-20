using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct ErosionAndDepositionJob : IJobParallelFor
{
	public NativeArray<float> SedimentMap;

	public ReadOnly<float> MinTerrainHeightMap;

	public ReadOnly<float> TerrainHeightMapSrcFloat;

	public NativeArray<float> TerrainHeightMapDstFloat;

	public NativeArray<float> WaterMap;

	public ReadOnly<float2> VelocityMap;

	public ReadOnly<float> AngleMap;

	public float DT;

	private const float SedimentCapacityConst = 0.0015f;

	private const float DissolveRateConstant = 0.15f;

	public void Execute(int index)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		float num = math.max(0.01047198f, AngleMap[index]);
		float2 val = VelocityMap[index];
		float num2 = 0.0015f * math.sin(num) * math.length(val);
		ref float reference = ref BurstUtil.Get<float>(ref WaterMap, index);
		float num3 = 1f - math.smoothstep(0f, 10f, reference);
		ref float reference2 = ref BurstUtil.Get<float>(ref SedimentMap, index);
		float num4 = DT * 0.15f * (num2 - reference2) * num3;
		float num5 = math.select(-1f, 1f, num2 > reference2) * num4;
		num5 = math.max(num5, 0f);
		float num6 = TerrainHeightMapSrcFloat[index];
		float num7 = num6 - num5;
		num7 = math.max(num7, MinTerrainHeightMap[index]);
		num5 = num7 - (num6 - num5);
		TerrainHeightMapDstFloat[index] = num7;
		reference2 += num5;
		reference += num5;
	}
}
