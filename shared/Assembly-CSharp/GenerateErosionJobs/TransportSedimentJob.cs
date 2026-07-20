using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct TransportSedimentJob : IJobParallelFor
{
	public NativeArray<float> SedimentMap;

	public ReadOnly<float> SedimentReadOnlyMap;

	public ReadOnly<float2> VelocityMap;

	public int Res;

	public float DT;

	public void Execute(int index)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		int num = index % Res;
		int num2 = index / Res;
		float2 val = VelocityMap[index];
		int num3 = (int)((float)num - DT * val.x);
		int num4 = (int)((float)num2 - DT * val.y);
		num3 = math.clamp(num3, 0, Res - 1);
		num4 = math.clamp(num4, 0, Res - 1);
		SedimentMap[index] = SedimentReadOnlyMap[num4 * Res + num3];
	}
}
