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

	public NativeArray<float> FluxLeft;

	public NativeArray<float> FluxRight;

	public NativeArray<float> FluxTop;

	public NativeArray<float> FluxBottom;

	public int Res;

	public float DT;

	public float GridCellSquareSize;

	public float PipeLength;

	public float PipeArea;

	public float WaterFill;

	private const float Gravity = 10f;

	[SkipLocalsInit]
	public void Execute(int y)
	{
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		int num = y * Res;
		if (y == 0 || y == Res - 1)
		{
			for (int i = num; i < num + Res; i++)
			{
				BurstUtil.Get<float>(ref FluxLeft, i) = 0f;
				BurstUtil.Get<float>(ref FluxRight, i) = 0f;
				BurstUtil.Get<float>(ref FluxTop, i) = 0f;
				BurstUtil.Get<float>(ref FluxBottom, i) = 0f;
			}
			return;
		}
		BurstUtil.Get<float>(ref FluxLeft, num) = 0f;
		BurstUtil.Get<float>(ref FluxRight, num) = 0f;
		BurstUtil.Get<float>(ref FluxTop, num) = 0f;
		BurstUtil.Get<float>(ref FluxBottom, num) = 0f;
		BurstUtil.Get<float>(ref FluxLeft, num + Res - 1) = 0f;
		BurstUtil.Get<float>(ref FluxRight, num + Res - 1) = 0f;
		BurstUtil.Get<float>(ref FluxTop, num + Res - 1) = 0f;
		BurstUtil.Get<float>(ref FluxBottom, num + Res - 1) = 0f;
		int j;
		for (j = 1; j <= Res - 5; j += 4)
		{
			int num2 = num + j;
			float4 val = WaterMap.ReinterpretLoad<float4, float>(num2) + WaterFill;
			float4 val2 = TerrainHeightMapFloatVal.ReinterpretLoad<float4, float>(num2) + val;
			float4 val3 = TerrainHeightMapFloatVal.ReinterpretLoad<float4, float>(num2 - 1) + (WaterMap.ReinterpretLoad<float4, float>(num2 - 1) + WaterFill);
			float4 val4 = TerrainHeightMapFloatVal.ReinterpretLoad<float4, float>(num2 + 1) + (WaterMap.ReinterpretLoad<float4, float>(num2 + 1) + WaterFill);
			float4 val5 = TerrainHeightMapFloatVal.ReinterpretLoad<float4, float>(num2 + Res) + (WaterMap.ReinterpretLoad<float4, float>(num2 + Res) + WaterFill);
			float4 val6 = TerrainHeightMapFloatVal.ReinterpretLoad<float4, float>(num2 - Res) + (WaterMap.ReinterpretLoad<float4, float>(num2 - Res) + WaterFill);
			float4 val7 = math.max(float4.zero, FluxLeft.ReinterpretLoad<float4>(num2) + DT * PipeArea * (10f * (val2 - val3) / PipeLength));
			float4 val8 = math.max(float4.zero, FluxRight.ReinterpretLoad<float4>(num2) + DT * PipeArea * (10f * (val2 - val4) / PipeLength));
			float4 val9 = math.max(float4.zero, FluxTop.ReinterpretLoad<float4>(num2) + DT * PipeArea * (10f * (val2 - val5) / PipeLength));
			float4 val10 = math.max(float4.zero, FluxBottom.ReinterpretLoad<float4>(num2) + DT * PipeArea * (10f * (val2 - val6) / PipeLength));
			float4 val11 = math.min(float4.op_Implicit(1f), val * GridCellSquareSize / ((val7 + val8 + (val9 + val10)) * DT));
			FluxLeft.ReinterpretStore<float4>(num2, val7 * val11);
			FluxRight.ReinterpretStore<float4>(num2, val8 * val11);
			FluxTop.ReinterpretStore<float4>(num2, val9 * val11);
			FluxBottom.ReinterpretStore<float4>(num2, val10 * val11);
		}
		for (; j < Res - 1; j++)
		{
			int num3 = num + j;
			float num4 = WaterMap[num3] + WaterFill;
			float num5 = TerrainHeightMapFloatVal[num3] + num4;
			float num6 = BurstUtil.GetReadonly<float>(ref TerrainHeightMapFloatVal, num3 - 1) + (BurstUtil.GetReadonly<float>(ref WaterMap, num3 - 1) + WaterFill);
			float num7 = BurstUtil.GetReadonly<float>(ref TerrainHeightMapFloatVal, num3 + 1) + (BurstUtil.GetReadonly<float>(ref WaterMap, num3 + 1) + WaterFill);
			float num8 = BurstUtil.GetReadonly<float>(ref TerrainHeightMapFloatVal, num3 + Res) + (BurstUtil.GetReadonly<float>(ref WaterMap, num3 + Res) + WaterFill);
			float num9 = BurstUtil.GetReadonly<float>(ref TerrainHeightMapFloatVal, num3 - Res) + (BurstUtil.GetReadonly<float>(ref WaterMap, num3 - Res) + WaterFill);
			float num10 = math.max(0f, BurstUtil.Get<float>(ref FluxLeft, num3) + DT * PipeArea * (10f * (num5 - num6) / PipeLength));
			float num11 = math.max(0f, BurstUtil.Get<float>(ref FluxRight, num3) + DT * PipeArea * (10f * (num5 - num7) / PipeLength));
			float num12 = math.max(0f, BurstUtil.Get<float>(ref FluxTop, num3) + DT * PipeArea * (10f * (num5 - num8) / PipeLength));
			float num13 = math.max(0f, BurstUtil.Get<float>(ref FluxBottom, num3) + DT * PipeArea * (10f * (num5 - num9) / PipeLength));
			float num14 = math.min(1f, num4 * GridCellSquareSize / ((num10 + num11 + (num12 + num13)) * DT));
			BurstUtil.Get<float>(ref FluxLeft, num3) = num10 * num14;
			BurstUtil.Get<float>(ref FluxRight, num3) = num11 * num14;
			BurstUtil.Get<float>(ref FluxTop, num3) = num12 * num14;
			BurstUtil.Get<float>(ref FluxBottom, num3) = num13 * num14;
		}
	}
}
