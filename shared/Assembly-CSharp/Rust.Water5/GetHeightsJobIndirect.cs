using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Rust.Water5;

[BurstCompile]
internal struct GetHeightsJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<float> Heights;

	[ReadOnly]
	public ReadOnly<Vector3> Pos;

	[ReadOnly]
	public ReadOnly<float> ShoreDists;

	[ReadOnly]
	public ReadOnly<float> TerrainHeights;

	[ReadOnly]
	public ReadOnly<int> Indices;

	[ReadOnly]
	public float OneOverOctave0Scale;

	[ReadOnly]
	public Rust.Water5.NativeOceanDisplacementShort3 SimData;

	[ReadOnly]
	public int Spectrum0;

	[ReadOnly]
	public int Spectrum1;

	[ReadOnly]
	public int Frame0;

	[ReadOnly]
	public int Frame1;

	[ReadOnly]
	public float SpectrumBlend;

	[ReadOnly]
	public float FrameBlend;

	[ReadOnly]
	public float DistanceAttenuationFactor;

	[ReadOnly]
	public float DepthAttenuationFactor;

	public void Execute()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			float heightRaw = GetHeightRaw(float3.op_Implicit(Pos[num]));
			Heights[num] = heightRaw * GetHeightAttenuation(ShoreDists[num], TerrainHeights[num]);
		}
	}

	private float GetHeightRaw(float3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		float3 zero = float3.zero;
		zero = float3.op_Implicit(GetDisplacement(position));
		zero = float3.op_Implicit(GetDisplacement(position - zero));
		zero = float3.op_Implicit(GetDisplacement(position - zero));
		return GetDisplacement(position - zero).y;
	}

	private Vector3 GetDisplacement(float3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		float normX = position.x * OneOverOctave0Scale;
		float normZ = position.z * OneOverOctave0Scale;
		return float3.op_Implicit(GetDisplacement(normX, normZ));
	}

	private float3 GetDisplacement(float normX, float normZ)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		normX -= math.floor(normX);
		normZ -= math.floor(normZ);
		float num = normX * 256f - 0.5f;
		float num2 = normZ * 256f - 0.5f;
		int num3 = (int)math.floor(num);
		int num4 = (int)math.floor(num2);
		float num5 = num - (float)num3;
		float num6 = num2 - (float)num4;
		int num7 = num3 % 256;
		int num8 = num4 % 256;
		int x = (num7 + 256) % 256;
		int z = (num8 + 256) % 256;
		int x2 = (num7 + 1 + 256) % 256;
		int z2 = (num8 + 1 + 256) % 256;
		float3 displacementFromSimData = GetDisplacementFromSimData(x, z);
		float3 displacementFromSimData2 = GetDisplacementFromSimData(x2, z);
		float3 displacementFromSimData3 = GetDisplacementFromSimData(x, z2);
		float3 displacementFromSimData4 = GetDisplacementFromSimData(x2, z2);
		float3 val = math.lerp(displacementFromSimData, displacementFromSimData2, num5);
		float3 val2 = math.lerp(displacementFromSimData3, displacementFromSimData4, num5);
		return math.lerp(val, val2, num6);
	}

	private float3 GetDisplacementFromSimData(int x, int z)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		int z2 = x * 256 + z;
		float3 val = math.lerp((float3)SimData[Spectrum0, Frame0, z2], (float3)SimData[Spectrum1, Frame0, z2], SpectrumBlend);
		float3 val2 = math.lerp((float3)SimData[Spectrum0, Frame1, z2], (float3)SimData[Spectrum1, Frame1, z2], SpectrumBlend);
		return math.lerp(val, val2, FrameBlend);
	}

	public float GetHeightAttenuation(float shore, float terrainHeight)
	{
		float num = Mathf.Clamp01(shore / DistanceAttenuationFactor);
		float num2 = Mathf.Clamp01(Mathf.Abs(terrainHeight) / DepthAttenuationFactor);
		return num * num2;
	}
}
