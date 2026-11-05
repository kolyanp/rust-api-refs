using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct TransportSedimentJob : IJobParallelFor
{
	[NativeDisableParallelForRestriction]
	public NativeArray<float> SedimentMap;

	public ReadOnly<float> SedimentReadOnlyMap;

	public ReadOnly<float2> VelocityMap;

	public int Res;

	public float DT;

	public void Execute(int y)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Res; i++)
		{
			int num = y * Res + i;
			float2 val = VelocityMap[num];
			int num2 = (int)((float)i - DT * val.x);
			int num3 = (int)((float)y - DT * val.y);
			num2 = math.clamp(num2, 0, Res - 1);
			num3 = math.clamp(num3, 0, Res - 1);
			SedimentMap[num] = SedimentReadOnlyMap[num3 * Res + num2];
		}
	}
}
