using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct AdjustWaterHeightByFluxJob : IJobParallelFor
{
	public NativeArray<float> WaterMap;

	public NativeArray<float2> VelocityMap;

	public ReadOnly<float> FluxLeft;

	public ReadOnly<float> FluxRight;

	public ReadOnly<float> FluxTop;

	public ReadOnly<float> FluxBottom;

	public int Res;

	public float DT;

	public float InvGridCellSquareSize;

	public float WaterFill;

	public void Execute(int y)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		int num = y * Res;
		if (y == 0 || y == Res - 1)
		{
			for (int i = num; i < num + Res; i++)
			{
				BurstUtil.Get<float2>(ref VelocityMap, i) = float2.zero;
				BurstUtil.Get<float>(ref WaterMap, i) += WaterFill;
			}
			return;
		}
		BurstUtil.Get<float2>(ref VelocityMap, num) = float2.zero;
		BurstUtil.Get<float2>(ref VelocityMap, num + Res - 1) = float2.zero;
		BurstUtil.Get<float>(ref WaterMap, num) += WaterFill;
		BurstUtil.Get<float>(ref WaterMap, num + Res - 1) += WaterFill;
		int j;
		for (j = 1; j <= Res - 5; j += 4)
		{
			int num2 = num + j;
			float4 val = FluxLeft.ReinterpretLoad<float4, float>(num2);
			float4 val2 = FluxRight.ReinterpretLoad<float4, float>(num2);
			float4 val3 = FluxTop.ReinterpretLoad<float4, float>(num2);
			float4 val4 = FluxBottom.ReinterpretLoad<float4, float>(num2);
			float4 val5 = val + val2 + (val3 + val4);
			float4 val6 = FluxRight.ReinterpretLoad<float4, float>(num2 - 1);
			float4 val7 = FluxLeft.ReinterpretLoad<float4, float>(num2 + 1);
			float4 val8 = FluxBottom.ReinterpretLoad<float4, float>(num2 + Res);
			float4 val9 = FluxTop.ReinterpretLoad<float4, float>(num2 - Res);
			float4 val10 = val6 + val7 + val8 + val9;
			float4 val11 = DT * (val10 - val5);
			WaterMap.ReinterpretStore<float4>(num2, WaterMap.ReinterpretLoad<float4>(num2) + WaterFill + val11 * InvGridCellSquareSize);
			float4 val12 = (val6 - val + val2 - val7) * 0.5f;
			float4 val13 = (val8 - val3 + val4 - val9) * 0.5f;
			float4x2 value = VelocityMap.LoadTransposed2(num2);
			ref float4 c = ref value.c0;
			c += val12 * DT;
			ref float4 c2 = ref value.c1;
			c2 += val13 * DT;
			VelocityMap.StoreTransposed2(num2, in value);
		}
		for (; j < Res - 1; j++)
		{
			int num3 = num + j;
			float num4 = BurstUtil.GetReadonly<float>(ref FluxLeft, num3);
			float num5 = BurstUtil.GetReadonly<float>(ref FluxRight, num3);
			float num6 = BurstUtil.GetReadonly<float>(ref FluxTop, num3);
			float num7 = BurstUtil.GetReadonly<float>(ref FluxBottom, num3);
			float num8 = num4 + num5 + (num6 + num7);
			float num9 = BurstUtil.GetReadonly<float>(ref FluxRight, num3 - 1);
			float num10 = BurstUtil.GetReadonly<float>(ref FluxLeft, num3 + 1);
			float num11 = BurstUtil.GetReadonly<float>(ref FluxBottom, num3 + Res);
			float num12 = BurstUtil.GetReadonly<float>(ref FluxTop, num3 - Res);
			float num13 = num9 + num10 + num11 + num12;
			float num14 = DT * (num13 - num8);
			ref float reference = ref BurstUtil.Get<float>(ref WaterMap, num3);
			reference = reference + WaterFill + num14 * InvGridCellSquareSize;
			float2 val14 = new float2
			{
				x = num9 - num4 + num5 - num10,
				y = num11 - num6 + num7 - num12
			};
			val14 *= 0.5f;
			ref float2 reference2 = ref BurstUtil.Get<float2>(ref VelocityMap, num3);
			reference2 += val14 * DT;
		}
	}
}
