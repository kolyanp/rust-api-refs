using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct AdjustWaterHeightByFluxJob : IJobParallelFor
{
	public NativeArray<float> WaterMap;

	public NativeArray<float2> VelocityMap;

	public ReadOnly<float4> FluxMap;

	public int Res;

	public float DT;

	public float InvGridCellSquareSize;

	public void Execute(int index)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		int num = index % Res;
		int num2 = index / Res;
		ref float2 reference = ref BurstUtil.Get<float2>(ref VelocityMap, index);
		if (num == 0 || num2 == 0 || num == Res - 1 || num2 == Res - 1)
		{
			reference = float2.zero;
			return;
		}
		float4 val = FluxMap[index];
		float num3 = math.csum(val);
		int4x2 val2 = default(int4x2);
		((int4x2)(ref val2))._002Ector(new int4(num - 1, num + 1, num, num), new int4(num2, num2, num2 + 1, num2 - 1));
		val2.c0 = val2.c1 * Res + val2.c0;
		float y = FluxMap[val2.c0.x].y;
		float x = FluxMap[val2.c0.y].x;
		float w = FluxMap[val2.c0.z].w;
		float z = FluxMap[val2.c0.w].z;
		float num4 = y + x + w + z;
		float num5 = DT * (num4 - num3);
		BurstUtil.Get<float>(ref WaterMap, index) += num5 * InvGridCellSquareSize;
		float2 val3 = new float2
		{
			x = y - val.x + val.y - x,
			y = w - val.z + val.w - z
		};
		val3 *= 0.5f;
		reference += val3 * DT;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int ToIndex([AssumeRange(0L, 2147483647L)] int x, [AssumeRange(0L, 2147483647L)] int y, [AssumeRange(0L, 2147483647L)] int res)
	{
		return y * res + x;
	}
}
