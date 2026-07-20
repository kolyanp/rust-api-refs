using TerrainTopologyMapJobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct PaintSplatJob : IJobFor
{
	[NativeDisableParallelForRestriction]
	public ReadOnly<float> HeightMapDelta;

	public int HeightMapRes;

	[NativeDisableParallelForRestriction]
	public ReadOnly<float> AngleMapDeg;

	[NativeDisableParallelForRestriction]
	public ReadOnly<int> TopologyMap;

	public int TopologyMapRes;

	[NativeDisableParallelForRestriction]
	public NativeArray<byte> SplatMap;

	public int SplatMapRes;

	public int SplatNum;

	[NativeDisableParallelForRestriction]
	public ReadOnly<int, int> SplatType2Index;

	public float TerrainOneOverSizeX;

	public void Execute(int index)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		int num = index % HeightMapRes;
		int num2 = index / HeightMapRes;
		float num3 = ((float)num - 0.5f) / (float)HeightMapRes;
		float num4 = ((float)num2 - 0.5f) / (float)HeightMapRes;
		if ((TerrainTopologyMapJobUtil.GetTopologyRadius(TopologyMap, TopologyMapRes, TerrainOneOverSizeX, 0f, num3, num4) & 0xB4990) != 0 || (TerrainTopologyMapJobUtil.GetTopologyRadius(TopologyMap, TopologyMapRes, TerrainOneOverSizeX, 8f, num3, num4) & 2) != 0 || AngleMapDeg[num2 * HeightMapRes + num] < 3f)
		{
			return;
		}
		float grad;
		float num5 = ConcavityFactor(HeightMapDelta, HeightMapRes, num, num2, out grad);
		if (!(num5 < 3.5762787E-07f))
		{
			int x = Index(num3, SplatMapRes);
			int z = Index(num4, SplatMapRes);
			float splat = GetSplat(SplatMap, SplatMapRes, SplatNum, SplatType2Index, num3, num4, 2);
			float splat2 = GetSplat(SplatMap, SplatMapRes, SplatNum, SplatType2Index, num3, num4, 4);
			if (splat > 0.25f || splat2 > 0.25f)
			{
				num5 = math.saturate(num5 * 3f);
				grad = math.pow(grad, 2f);
				AddSplat(SplatMap, SplatMapRes, SplatNum, SplatType2Index, x, z, 64, math.pow(num5, 0.8f) * grad);
				AddSplat(SplatMap, SplatMapRes, SplatNum, SplatType2Index, x, z, 128, math.pow(num5, 1.5f) * grad);
			}
			else
			{
				num5 = math.saturate(num5 * 3f);
				AddSplat(SplatMap, SplatMapRes, SplatNum, SplatType2Index, x, z, 1, math.pow(num5, 4f) * math.pow(grad, 1.5f));
				grad = math.pow(grad, 2f);
				AddSplat(SplatMap, SplatMapRes, SplatNum, SplatType2Index, x, z, 64, math.pow(num5, 0.8f) * grad);
				AddSplat(SplatMap, SplatMapRes, SplatNum, SplatType2Index, x, z, 128, math.pow(num5, 1.4f) * grad);
			}
		}
		static int Index(float normalized, int res)
		{
			int num6 = (int)(normalized * (float)res);
			if (num6 >= 0)
			{
				if (num6 <= res - 1)
				{
					return num6;
				}
				return res - 1;
			}
			return 0;
		}
	}

	private static void AddSplat(NativeArray<byte> src, int res, int splatNum, ReadOnly<int, int> type2Index, int x, int z, int id, float d)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		float splat = GetSplat(src, res, splatNum, type2Index, x, z, id);
		float num = math.saturate(splat + d);
		int num2 = type2Index[id];
		if (splat >= 1f)
		{
			return;
		}
		float num3 = (1f - num) / (1f - splat);
		for (int i = 0; i < splatNum; i++)
		{
			if (i == num2)
			{
				src[(i * res + z) * res + x] = BitUtility.Float2Byte(num);
			}
			else
			{
				src[(i * res + z) * res + x] = BitUtility.Float2Byte(num3 * BitUtility.Byte2Float((int)src[(i * res + z) * res + x]));
			}
		}
	}

	private static float GetSplat(NativeArray<byte> src, int res, int splatNum, ReadOnly<int, int> type2Index, float xn, float zn, int mask)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		int num = res - 1;
		float num2 = xn * (float)num;
		float num3 = zn * (float)num;
		int num4 = Mathf.Clamp((int)num2, 0, num);
		int num5 = Mathf.Clamp((int)num3, 0, num);
		int x = Mathf.Min(num4 + 1, num);
		int z = Mathf.Min(num5 + 1, num);
		float num6 = Mathf.Lerp(GetSplat(src, res, splatNum, type2Index, num4, num5, mask), GetSplat(src, res, splatNum, type2Index, x, num5, mask), num2 - (float)num4);
		float num7 = Mathf.Lerp(GetSplat(src, res, splatNum, type2Index, num4, z, mask), GetSplat(src, res, splatNum, type2Index, x, z, mask), num2 - (float)num4);
		return Mathf.Lerp(num6, num7, num3 - (float)num5);
	}

	private static float GetSplat(NativeArray<byte> src, int res, int splatNum, ReadOnly<int, int> type2Index, int x, int z, int mask)
	{
		if (Mathf.IsPowerOfTwo(mask))
		{
			return BitUtility.Byte2Float((int)src[(type2Index[mask] * res + z) * res + x]);
		}
		int num = 0;
		for (int i = 0; i < splatNum; i++)
		{
			if ((TerrainSplat.IndexToType(i) & mask) != 0)
			{
				num += src[(i * res + z) * res + x];
			}
		}
		return Mathf.Clamp01(BitUtility.Byte2Float(num));
	}

	private static float ConcavityFactor(ReadOnly<float> data, int res, int x, int z, out float grad)
	{
		int num = x - 1;
		int num2 = x + 1;
		int num3 = z - 1;
		int num4 = z + 1;
		float num5 = data[z * res + x];
		float num6 = data[z * res + num];
		float num7 = data[z * res + num2];
		float num8 = data[num4 * res + x];
		float num9 = data[num3 * res + x];
		float num10 = num6 + num7 + num8 + num9;
		float num11 = data[num3 * res + num] + data[num3 * res + num2] + data[num4 * res + num] + data[num4 * res + num2];
		float num12 = num7 - num6;
		float num13 = num8 - num9;
		grad = math.sqrt(num12 * num12 + num13 * num13);
		return math.max(num10 / 4f + num11 / 4f - num5, 0f);
	}
}
