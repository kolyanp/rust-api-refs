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

	public float SeaLevel;

	public float MaxDisplacement;

	[ReadOnly]
	public NativeList<int> Indices;

	[ReadOnly]
	public NativeArray<Ray> Rays;

	public ReadOnly<float> MaxDists;

	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<bool> HitResults;

	[NativeDisableParallelForRestriction]
	[WriteOnly]
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
		bool flag = Trace(in ray, maxDist, out var result);
		HitResults[num] = flag;
		HitPositions[num] = result;
	}

	private bool Trace(in Ray ray, float maxDist, out Vector3 result)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f - MaxDisplacement;
		Vector3 point = ((Ray)(ref ray)).GetPoint(maxDist);
		if (((Ray)(ref ray)).origin.y > MaxDisplacement + SeaLevel && point.y > MaxDisplacement + SeaLevel)
		{
			result = Vector3.zero;
			return false;
		}
		if (((Ray)(ref ray)).origin.y < num + SeaLevel && point.y < num + SeaLevel)
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
		if (val.y >= MaxDisplacement + SeaLevel)
		{
			num3 = (num2 = (0f - (val.y - MaxDisplacement - SeaLevel)) / direction.y);
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
			num2 = num4 * Mathf.Abs(val.y - height - SeaLevel);
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
			num2 = (0f - (val.y + MaxDisplacement - SeaLevel)) / direction.y;
			Vector3 val2 = val;
			Vector3 val3 = val + num2 * ((Ray)(ref ray)).direction;
			for (int i = 0; i < 16; i++)
			{
				val = (val2 + val3) * 0.5f;
				float height2 = GetHeight(val);
				if (val.y - height2 - SeaLevel > 0f)
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
