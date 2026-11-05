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

	public float DT;

	private const float SedimentCapacityConst = 0.0015f;

	public float NormY;

	private const float DissolveRateConstant = 0.15f;

	public int Res;

	public float EvapFactor;

	private const float SedimentAngleThresholdRad = 0.01047198f;

	private const float DepthMax = 10f;

	public void Execute(int z)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		int num = z * Res;
		if (z == 0 || z == Res - 1)
		{
			for (int i = num; i < num + Res; i++)
			{
				ProcessCell(i, 0f);
			}
			return;
		}
		ProcessCell(num, 0f);
		ProcessCell(num + Res - 1, 0f);
		int num2 = num - Res;
		int num3 = num + Res;
		float num4 = NormY * NormY;
		int j;
		for (j = 1; j <= Res - 5; j += 4)
		{
			int num5 = num + j;
			float4 val = TerrainHeightMapSrcFloat.ReinterpretLoad<float4, float>(num2 + j + 1);
			float4 val2 = TerrainHeightMapSrcFloat.ReinterpretLoad<float4, float>(num2 + j - 1);
			float4 val3 = TerrainHeightMapSrcFloat.ReinterpretLoad<float4, float>(num3 + j - 1);
			float4 val4 = (val - val2) * -0.5f;
			float4 val5 = (val3 - val2) * -0.5f;
			float4 val6 = math.rsqrt(val4 * val4 + num4 + val5 * val5);
			float4 val7 = NormY * val6;
			float4 val8 = val4 * val6;
			float4 val9 = val5 * val6;
			float4 val10 = val8 * val8 + val7 * val7 + val9 * val9;
			float4 val11 = math.acos(math.clamp(val7 * math.rsqrt(val10) / val10, float4.op_Implicit(-1f), float4.op_Implicit(1f)));
			float4 val12 = math.max(float4.op_Implicit(0.01047198f), val11);
			float4x2 val13 = VelocityMap.LoadTransposed2(num5);
			float4 val14 = math.sqrt(val13.c0 * val13.c0 + val13.c1 * val13.c1);
			float4 val15 = 0.0015f * math.sin(val12) * val14;
			float4 val16 = WaterMap.ReinterpretLoad<float4>(num5);
			float4 val17 = 1f - math.smoothstep(float4.op_Implicit(0f), float4.op_Implicit(10f), val16);
			float4 val18 = SedimentMap.ReinterpretLoad<float4>(num5);
			float4 val19 = DT * 0.15f * (val15 - val18) * val17;
			float4 val20 = math.select(new float4(-1f), new float4(1f), val15 > val18) * val19;
			val20 = math.max(val20, float4.op_Implicit(0f));
			float4 val21 = TerrainHeightMapSrcFloat.ReinterpretLoad<float4, float>(num5);
			float4 val22 = val21 - val20;
			val22 = math.max(val22, MinTerrainHeightMap.ReinterpretLoad<float4, float>(num5));
			val20 = val22 - (val21 - val20);
			TerrainHeightMapDstFloat.ReinterpretStore<float4>(num5, val22);
			SedimentMap.ReinterpretStore<float4>(num5, val18 + val20);
			WaterMap.ReinterpretStore<float4>(num5, (val16 + val20) * EvapFactor);
		}
		for (; j < Res - 1; j++)
		{
			float num6 = (BurstUtil.GetReadonly<float>(ref TerrainHeightMapSrcFloat, num2 + j + 1) - BurstUtil.GetReadonly<float>(ref TerrainHeightMapSrcFloat, num2 + j - 1)) * -0.5f;
			float num7 = (BurstUtil.GetReadonly<float>(ref TerrainHeightMapSrcFloat, num3 + j - 1) - BurstUtil.GetReadonly<float>(ref TerrainHeightMapSrcFloat, num2 + j - 1)) * -0.5f;
			float3 val23 = math.normalize(new float3(num6, NormY, num7));
			float num8 = math.dot(val23, val23);
			float slopeAngle = math.acos(math.clamp(math.dot(math.up(), math.normalize(val23)) / num8, -1f, 1f));
			ProcessCell(num + j, slopeAngle);
		}
	}

	private void ProcessCell(int index, float slopeAngle)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		float num = math.max(0.01047198f, slopeAngle);
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
		BurstUtil.Get<float>(ref TerrainHeightMapDstFloat, index) = num7;
		reference2 += num5;
		reference = (reference + num5) * EvapFactor;
	}
}
