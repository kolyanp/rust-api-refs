using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct CalculateOutputFluxJob : IJobParallelFor
{
	public ReadOnly<float> TerrainHeightMapFloatVal;

	public ReadOnly<float> WaterMap;

	public NativeArray<float4> FluxMap;

	public int Res;

	public float DT;

	public float GridCellSquareSize;

	public float PipeLength;

	public float PipeArea;

	private const float Gravity = 10f;

	[SkipLocalsInit]
	public unsafe void Execute(int index)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		ref float4 reference = ref BurstUtil.Get<float4>(ref FluxMap, index);
		int num = index % Res;
		int num2 = index / Res;
		if (num == 0 || num2 == 0 || num == Res - 1 || num2 == Res - 1)
		{
			reference = float4.zero;
			return;
		}
		float num3 = TerrainHeightMapFloatVal[index];
		float num4 = WaterMap[index];
		float num5 = num3 + num4;
		int4x2 val = default(int4x2);
		((int4x2)(ref val))._002Ector(new int4(num - 1, num + 1, num, num), new int4(num2, num2, num2 + 1, num2 - 1));
		int4 val2 = math.mad(val.c1, int4.op_Implicit(Res), val.c0);
		float4 val3 = default(float4);
		val3.x = *(float*)BurstUtil.GetReadonly<float>(ref TerrainHeightMapFloatVal, val2.x) + *(float*)BurstUtil.GetReadonly<float>(ref WaterMap, val2.x);
		val3.y = *(float*)BurstUtil.GetReadonly<float>(ref TerrainHeightMapFloatVal, val2.y) + *(float*)BurstUtil.GetReadonly<float>(ref WaterMap, val2.y);
		val3.z = *(float*)BurstUtil.GetReadonly<float>(ref TerrainHeightMapFloatVal, val2.z) + *(float*)BurstUtil.GetReadonly<float>(ref WaterMap, val2.z);
		val3.w = *(float*)BurstUtil.GetReadonly<float>(ref TerrainHeightMapFloatVal, val2.w) + *(float*)BurstUtil.GetReadonly<float>(ref WaterMap, val2.w);
		reference = math.max(float4.zero, reference + DT * PipeArea * (10f * (num5 - val3) / PipeLength));
		float num6 = math.min(1f, num4 * GridCellSquareSize / (math.csum(reference) * DT));
		reference *= num6;
	}
}
