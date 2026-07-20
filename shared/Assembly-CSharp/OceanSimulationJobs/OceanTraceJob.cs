using Rust.Water5;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OceanSimulationJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct OceanTraceJob : IJobParallelForDefer
{
	public const int maxSteps = 16;

	public const int maxBinarySteps = 16;

	public const float intersectionThreshold = 0.1f;

	public const float seaLevel = 0f;

	public float MaxDisplacement;

	[ReadOnly]
	public NativeList<int> Indices;

	[ReadOnly]
	public NativeArray<Ray> Rays;

	public ReadOnly<float> MaxDists;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<bool> HitResults;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Vector3> HitPositions;

	public float OneOverOctave0Scale;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public Rust.Water5.NativeOceanDisplacementShort3 SimData;

	public int Spectrum0;

	public int Spectrum1;

	public int Frame0;

	public int Frame1;

	public float spectrumBlend;

	public float frameBlend;

	public TerrainHeightMap.HeightMapQueryStructure HeightMapQueryStructure;

	public TerrainTexturing.ShoreVectorQueryStructure ShoreVectorQueryStructure;

	public float distanceAttenuationFactor;

	public float depthAttenuationFactor;

	public void Execute(int indicesIndex)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		int num = Indices[indicesIndex];
		Ray ray = Rays[num];
		float maxDist = MaxDists[num];
		Vector3 result;
		bool flag = Trace(in ray, maxDist, out result);
		HitResults[num] = flag;
		HitPositions[num] = result;
	}

	private bool Trace(in Ray ray, float maxDist, out Vector3 result)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f - MaxDisplacement;
		Vector3 point = ((Ray)(ref ray)).GetPoint(maxDist);
		if (((Ray)(ref ray)).origin.y > MaxDisplacement && point.y > MaxDisplacement)
		{
			result = Vector3.zero;
			return false;
		}
		if (((Ray)(ref ray)).origin.y < num && point.y < num)
		{
			result = Vector3.zero;
			return false;
		}
		Vector3 val = ((Ray)(ref ray)).origin;
		Vector3 direction = ((Ray)(ref ray)).direction;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 2f / (math.abs(direction.y) + 1f);
		result = val;
		if (direction.y <= -0.99f)
		{
			result.y = GetHeight(val);
			return math.lengthsq(float3.op_Implicit(result - val)) < maxDist * maxDist;
		}
		if (val.y >= MaxDisplacement + 0f)
		{
			num3 = (num2 = (0f - (val.y - MaxDisplacement - 0f)) / direction.y);
			val += num2 * direction;
			if (num3 >= maxDist)
			{
				result = Vector3.zero;
				return false;
			}
		}
		int num5 = 0;
		while (true)
		{
			float height = GetHeight(val);
			num2 = num4 * Mathf.Abs(val.y - height - 0f);
			val += num2 * direction;
			num3 += num2;
			if (num5 >= 16 || num2 < 0.1f)
			{
				break;
			}
			if (num3 >= maxDist)
			{
				return false;
			}
			num5++;
		}
		if (num2 < 0.1f && num3 >= 0f)
		{
			result = val;
			return true;
		}
		if (direction.y < 0f)
		{
			num2 = (0f - (val.y + MaxDisplacement - 0f)) / direction.y;
			Vector3 val2 = val;
			Vector3 val3 = val + num2 * ((Ray)(ref ray)).direction;
			for (int i = 0; i < 16; i++)
			{
				val = (val2 + val3) * 0.5f;
				float height2 = GetHeight(val);
				if (val.y - height2 - 0f > 0f)
				{
					val2 = val;
				}
				else
				{
					val3 = val;
				}
				if (math.abs(val.y - height2) < 0.1f)
				{
					val.y = height2;
					break;
				}
			}
			result = val;
			return true;
		}
		return false;
	}

	private float GetHeight(Vector3 point)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return GetHeightRaw(point) * GetHeightAttenuation(point);
	}

	public float GetHeightAttenuation(Vector3 position)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		float x = HeightMapQueryStructure.TerrainPosition.x;
		float z = HeightMapQueryStructure.TerrainPosition.z;
		float x2 = HeightMapQueryStructure.TerrainOneOverSize.x;
		float z2 = HeightMapQueryStructure.TerrainOneOverSize.z;
		float num = (position.x - x) * x2;
		float num2 = (position.z - z) * z2;
		Vector2 uv = default(Vector2);
		((Vector2)(ref uv))._002Ector(num, num2);
		float coarseDistanceToShore = ShoreVectorQueryStructure.GetCoarseDistanceToShore(position);
		float heightFromUV = HeightMapQueryStructure.GetHeightFromUV(uv);
		float num3 = Mathf.Clamp01(coarseDistanceToShore / distanceAttenuationFactor);
		float num4 = Mathf.Clamp01(Mathf.Abs(heightFromUV) / depthAttenuationFactor);
		return num3 * num4;
	}

	private float GetHeightRaw(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector3 zero = Vector3.zero;
		zero = GetDisplacement(position);
		zero = GetDisplacement(position - zero);
		zero = GetDisplacement(position - zero);
		return GetDisplacement(position - zero).y;
	}

	private Vector3 GetDisplacement(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		float normX = position.x * OneOverOctave0Scale;
		float normZ = position.z * OneOverOctave0Scale;
		return GetDisplacement(normX, normZ);
	}

	private Vector3 GetDisplacement(float normX, float normZ)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
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
		Vector3 displacementFromSimData = GetDisplacementFromSimData(x, z);
		Vector3 displacementFromSimData2 = GetDisplacementFromSimData(x2, z);
		Vector3 displacementFromSimData3 = GetDisplacementFromSimData(x, z2);
		Vector3 displacementFromSimData4 = GetDisplacementFromSimData(x2, z2);
		float3 val = math.lerp(float3.op_Implicit(displacementFromSimData), float3.op_Implicit(displacementFromSimData2), num5);
		float3 val2 = math.lerp(float3.op_Implicit(displacementFromSimData3), float3.op_Implicit(displacementFromSimData4), num5);
		return float3.op_Implicit(math.lerp(val, val2, num6));
	}

	private Vector3 GetDisplacementFromSimData(int x, int z)
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
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		int z2 = x * 256 + z;
		float3 val = math.lerp((float3)SimData[Spectrum0, Frame0, z2], (float3)SimData[Spectrum1, Frame0, z2], spectrumBlend);
		float3 val2 = math.lerp((float3)SimData[Spectrum0, Frame1, z2], (float3)SimData[Spectrum1, Frame1, z2], spectrumBlend);
		return float3.op_Implicit(math.lerp(val, val2, frameBlend));
	}
}
