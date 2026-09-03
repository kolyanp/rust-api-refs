using System;
using System.Runtime.CompilerServices;
using Facepunch;
using OceanSimulationJobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rust.Water5;

public class OceanSimulation : IDisposable
{
	public const int octaveCount = 3;

	public const int simulationSize = 256;

	public const int physicsSimulationSize = 256;

	public const int physicsFrameRate = 4;

	public const int physicsLooptime = 18;

	public const int physicsFrameCount = 72;

	public const float phsyicsDeltaTime = 0.25f;

	public const float oneOverPhysicsSimulationSize = 0.00390625f;

	public const int physicsFrameSize = 65536;

	public const int physicsSpectrumOffset = 4718592;

	private OceanSettings oceanSettings;

	private float[] spectrumRanges;

	private float distanceAttenuationFactor;

	private float depthAttenuationFactor;

	private static float oneOverOctave0Scale;

	private static float[] beaufortValues;

	private int spectrum0;

	private int spectrum1;

	private float spectrumBlend;

	private int frame0;

	private int frame1;

	private float frameBlend;

	private float currentTime;

	private float prevUpdateComputeTime;

	private float deltaTime;

	private Rust.Water5.NativeOceanDisplacementShort3 nativeSimData;

	private Rust.Water5.GetHeightBatchedJob _cachedBatchJob;

	private Rust.Water5.GetHeightsJobIndirect _heightsJobIndirect;

	private NativeArray<float3> _batchPositionQueryArr;

	public int Spectrum0 => spectrum0;

	public int Spectrum1 => spectrum1;

	public float SpectrumBlend => spectrumBlend;

	public int Frame0 => frame0;

	public int Frame1 => frame1;

	public float FrameBlend => frameBlend;

	internal OceanSimulation(OceanSettings oceanSettings, Rust.Water5.NativeOceanDisplacementShort3 simData)
	{
		this.oceanSettings = oceanSettings;
		oneOverOctave0Scale = 1f / oceanSettings.octaveScales[0];
		beaufortValues = new float[oceanSettings.spectrumSettings.Length];
		for (int i = 0; i < oceanSettings.spectrumSettings.Length; i++)
		{
			beaufortValues[i] = oceanSettings.spectrumSettings[i].beaufort;
		}
		nativeSimData = simData;
		spectrumRanges = oceanSettings.spectrumRanges;
		depthAttenuationFactor = oceanSettings.depthAttenuationFactor;
		distanceAttenuationFactor = oceanSettings.distanceAttenuationFactor;
		_cachedBatchJob = new Rust.Water5.GetHeightBatchedJob
		{
			SimData = nativeSimData.AsReadOnly()
		};
		_heightsJobIndirect = new Rust.Water5.GetHeightsJobIndirect
		{
			SimData = nativeSimData
		};
	}

	public void Update(float time, float dt, float beaufort)
	{
		currentTime = time % 18f;
		deltaTime = dt;
		FindFrames(currentTime, out frame0, out frame1, out frameBlend);
		FindSpectra(beaufort, out spectrum0, out spectrum1, out spectrumBlend);
	}

	private static void FindSpectra(float beaufort, out int spectrum0, out int spectrum1, out float spectrumT)
	{
		beaufort = Mathf.Clamp(beaufort, 0f, 10f);
		spectrum0 = (spectrum1 = 0);
		spectrumT = 0f;
		for (int i = 1; i < beaufortValues.Length; i++)
		{
			float num = beaufortValues[i - 1];
			float num2 = beaufortValues[i];
			if (beaufort >= num && beaufort <= num2)
			{
				spectrum0 = i - 1;
				spectrum1 = i;
				spectrumT = math.remap(num, num2, 0f, 1f, beaufort);
				break;
			}
		}
	}

	public static void FindFrames(float time, out int frame0, out int frame1, out float frameBlend)
	{
		frame0 = (int)math.floor(time * 4f);
		frame1 = (int)math.floor(time * 4f);
		frame1 = (frame1 + 1) % 72;
		frameBlend = math.remap((float)frame0 * 0.25f, (float)(frame0 + 1) * 0.25f, 0f, 1f, time);
	}

	public JobHandle TraceBatch(NativeList<int> indices, NativeArray<Ray> rays, NativeArray<float> maxDists, NativeArray<bool> hitResults, NativeArray<Vector3> hitPositions, JobHandle inputDeps)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Lerp(spectrumRanges[spectrum0], spectrumRanges[spectrum1], spectrumBlend);
		if (num <= 0.1f)
		{
			inputDeps = IJobParallelForDeferExtensions.Schedule<OceanSimulationJobs.SmallDisplacementPlaneTraceJob, int>(new OceanSimulationJobs.SmallDisplacementPlaneTraceJob
			{
				SeaPlane = new Plane(Vector3.up, 0f - WaterSystem.OceanLevel),
				Indices = indices,
				Rays = rays,
				MaxDists = maxDists.AsReadOnly(),
				HitResults = hitResults,
				HitPositions = hitPositions
			}, indices, 256, inputDeps);
			return inputDeps;
		}
		inputDeps = IJobParallelForDeferExtensions.Schedule<OceanSimulationJobs.OceanTraceJob, int>(new OceanSimulationJobs.OceanTraceJob
		{
			SeaLevel = WaterSystem.OceanLevel,
			MaxDisplacement = num,
			Indices = indices,
			Rays = rays,
			MaxDists = maxDists.AsReadOnly(),
			HitResults = hitResults,
			HitPositions = hitPositions,
			OneOverOctave0Scale = oneOverOctave0Scale,
			SimData = nativeSimData,
			Spectrum0 = spectrum0,
			Spectrum1 = spectrum1,
			Frame0 = frame0,
			Frame1 = frame1,
			spectrumBlend = spectrumBlend,
			frameBlend = frameBlend,
			HeightMapQueryStructure = TerrainMeta.HeightMap.GetQueryStructure(isForDeepSea: false),
			ShoreVectorQueryStructure = TerrainMeta.Texturing.GetShoreVectorQueryStructure(),
			distanceAttenuationFactor = distanceAttenuationFactor,
			depthAttenuationFactor = depthAttenuationFactor
		}, indices, 256, inputDeps);
		return inputDeps;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Trace(Ray ray, float maxDist, out Vector3 result)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		float oceanLevel = WaterSystem.OceanLevel;
		float num = Mathf.Lerp(spectrumRanges[spectrum0], spectrumRanges[spectrum1], spectrumBlend);
		if (num <= 0.1f)
		{
			Plane val = default(Plane);
			((Plane)(ref val))._002Ector(Vector3.up, 0f - oceanLevel);
			float num2 = default(float);
			if (((Plane)(ref val)).Raycast(ray, ref num2) && num2 < maxDist)
			{
				result = ((Ray)(ref ray)).GetPoint(num2);
				return true;
			}
			result = Vector3.zero;
			return false;
		}
		float num3 = 0f - num;
		Vector3 point = ((Ray)(ref ray)).GetPoint(maxDist);
		if (((Ray)(ref ray)).origin.y > num + oceanLevel && point.y > num + oceanLevel)
		{
			result = Vector3.zero;
			return false;
		}
		if (((Ray)(ref ray)).origin.y < num3 + oceanLevel && point.y < num3 + oceanLevel)
		{
			result = Vector3.zero;
			return false;
		}
		Vector3 val2 = ((Ray)(ref ray)).origin;
		Vector3 direction = ((Ray)(ref ray)).direction;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 2f / (math.abs(direction.y) + 1f);
		result = val2;
		if (direction.y <= -0.99f)
		{
			result.y = GetHeight(val2);
			return math.lengthsq(float3.op_Implicit(result - val2)) < maxDist * maxDist;
		}
		if (val2.y >= num + oceanLevel)
		{
			num5 = (num4 = (0f - (val2.y - num - oceanLevel)) / direction.y);
			val2 += num4 * direction;
			if (num5 >= maxDist)
			{
				result = Vector3.zero;
				return false;
			}
		}
		int num7 = 0;
		while (true)
		{
			float height = GetHeight(val2);
			num4 = num6 * Mathf.Abs(val2.y - height - oceanLevel);
			val2 += num4 * direction;
			num5 += num4;
			if (num7 >= 16 || num4 < 0.1f)
			{
				break;
			}
			if (num5 >= maxDist)
			{
				return false;
			}
			num7++;
		}
		if (num4 < 0.1f && num5 >= 0f)
		{
			result = val2;
			return true;
		}
		if (direction.y < 0f)
		{
			num4 = (0f - (val2.y + num - oceanLevel)) / direction.y;
			Vector3 val3 = val2;
			Vector3 val4 = val2 + num4 * ((Ray)(ref ray)).direction;
			for (int i = 0; i < 16; i++)
			{
				val2 = (val3 + val4) * 0.5f;
				float height2 = GetHeight(val2);
				if (val2.y - height2 - oceanLevel > 0f)
				{
					val3 = val2;
				}
				else
				{
					val4 = val2;
				}
				if (math.abs(val2.y - height2) < 0.1f)
				{
					val2.y = height2;
					break;
				}
			}
			result = val2;
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float MinLevel()
	{
		return 0f - Mathf.Lerp(spectrumRanges[spectrum0], spectrumRanges[spectrum1], spectrumBlend);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float MaxLevel()
	{
		return Mathf.Lerp(spectrumRanges[spectrum0], spectrumRanges[spectrum1], spectrumBlend);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float GetHeight(Vector3[,,] simData, Vector3 position, float time, float beaufort, float distAttenFactor, float depthAttenFactor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		float x = TerrainMeta.Position.x;
		float z = TerrainMeta.Position.z;
		float x2 = TerrainMeta.OneOverSize.x;
		float z2 = TerrainMeta.OneOverSize.z;
		float num = (position.x - x) * x2;
		float num2 = (position.z - z) * z2;
		Vector2 uv = default(Vector2);
		((Vector2)(ref uv))._002Ector(num, num2);
		float num3 = (((Object)(object)TerrainTexturing.Instance != (Object)null) ? TerrainTexturing.Instance.GetCoarseDistanceToShore(position) : 0f);
		float num4 = (((Object)(object)TerrainMeta.HeightMap != (Object)null) ? TerrainMeta.HeightMap.GetHeight(uv) : 0f);
		float num5 = Mathf.Clamp01(num3 / distAttenFactor);
		float num6 = Mathf.Clamp01(Mathf.Abs(num4) / depthAttenFactor);
		Vector3 zero = Vector3.zero;
		zero = GetDisplacement(simData, position, time, beaufort);
		zero = GetDisplacement(simData, position - zero, time, beaufort);
		zero = GetDisplacement(simData, position - zero, time, beaufort);
		return GetDisplacement(simData, position - zero, time, beaufort).y * num5 * num6;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 GetDisplacement(Vector3[,,] simData, Vector3 position, float time, float beaufort)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		FindFrames(time, out var num, out var num2, out var num3);
		FindSpectra(beaufort, out var num4, out var num5, out var spectrumT);
		return GetDisplacement(simData, position, num, num2, num3, num4, num5, spectrumT);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 GetDisplacement(Vector3[,,] simData, Vector3 position, int frame0, int frame1, float frameBlend, int spectrum0, int spectrum1, float spectrumBlend)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		float normX = position.x * oneOverOctave0Scale;
		float normZ = position.z * oneOverOctave0Scale;
		return GetDisplacement(simData, normX, normZ, frame0, frame1, frameBlend, spectrum0, spectrum1, spectrumBlend);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 GetDisplacement(Vector3[,,] simData, float normX, float normZ, int frame0, int frame1, float frameBlend, int spectrum0, int spectrum1, float spectrumBlend)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		normX -= Mathf.Floor(normX);
		normZ -= Mathf.Floor(normZ);
		float num = normX * 256f - 0.5f;
		float num2 = normZ * 256f - 0.5f;
		int num3 = Mathf.FloorToInt(num);
		int num4 = Mathf.FloorToInt(num2);
		float num5 = num - (float)num3;
		float num6 = num2 - (float)num4;
		int x = num3;
		int y = num4;
		int x2 = num3 + 1;
		int y2 = num4 + 1;
		Vector3 displacement = GetDisplacement(simData, x, y, frame0, frame1, frameBlend, spectrum0, spectrum1, spectrumBlend);
		Vector3 displacement2 = GetDisplacement(simData, x2, y, frame0, frame1, frameBlend, spectrum0, spectrum1, spectrumBlend);
		Vector3 displacement3 = GetDisplacement(simData, x, y2, frame0, frame1, frameBlend, spectrum0, spectrum1, spectrumBlend);
		Vector3 displacement4 = GetDisplacement(simData, x2, y2, frame0, frame1, frameBlend, spectrum0, spectrum1, spectrumBlend);
		Vector3 val = Vector3.LerpUnclamped(displacement, displacement2, num5);
		Vector3 val2 = Vector3.LerpUnclamped(displacement3, displacement4, num5);
		return Vector3.LerpUnclamped(val, val2, num6);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 GetDisplacement(Vector3[,,] simData, int x, int y, int frame0, int frame1, float frameBlend, int spectrum0, int spectrum1, float spectrumBlend)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		int num = x * 256 + y;
		Vector3 val = Vector3.LerpUnclamped(simData[spectrum0, frame0, num], simData[spectrum1, frame0, num], spectrumBlend);
		Vector3 val2 = Vector3.LerpUnclamped(simData[spectrum0, frame1, num], simData[spectrum1, frame1, num], spectrumBlend);
		return Vector3.LerpUnclamped(val, val2, frameBlend);
	}

	public void Dispose()
	{
		if (_batchPositionQueryArr.IsCreated)
		{
			_batchPositionQueryArr.Dispose();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void GetHeightBatch(Vector2[] positions, float[] heights, float[] shore, float[] terrainHeight)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		Assert.IsTrue(positions.Length == heights.Length);
		PopulateBatchNative(positions);
		_cachedBatchJob.OneOverOctave0Scale = oneOverOctave0Scale;
		_cachedBatchJob.Frame0 = frame0;
		_cachedBatchJob.Frame1 = frame1;
		_cachedBatchJob.Spectrum0 = spectrum0;
		_cachedBatchJob.Spectrum1 = spectrum1;
		_cachedBatchJob.spectrumBlend = spectrumBlend;
		_cachedBatchJob.frameBlend = frameBlend;
		_cachedBatchJob.Positions = _batchPositionQueryArr;
		_cachedBatchJob.Count = positions.Length;
		ref Rust.Water5.GetHeightBatchedJob cachedBatchJob = ref _cachedBatchJob;
		int num = positions.Length;
		int batchSize = ThreadUtils.GetBatchSize(positions.Length);
		JobHandle val = default(JobHandle);
		val = IJobParallelForExtensions.ScheduleByRef<Rust.Water5.GetHeightBatchedJob>(ref cachedBatchJob, num, batchSize, val);
		((JobHandle)(ref val)).Complete();
		for (int i = 0; i < heights.Length; i++)
		{
			heights[i] = _batchPositionQueryArr[i].x * GetHeightAttenuation(shore[i], terrainHeight[i]);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void GetHeightBatch(ReadOnlySpan<Vector2> positions, Span<float> heights, ReadOnlySpan<float> shore, ReadOnlySpan<float> terrainHeight)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		Assert.IsTrue(positions.Length == heights.Length);
		PopulateBatchNative(positions);
		_cachedBatchJob.OneOverOctave0Scale = oneOverOctave0Scale;
		_cachedBatchJob.Frame0 = frame0;
		_cachedBatchJob.Frame1 = frame1;
		_cachedBatchJob.Spectrum0 = spectrum0;
		_cachedBatchJob.Spectrum1 = spectrum1;
		_cachedBatchJob.spectrumBlend = spectrumBlend;
		_cachedBatchJob.frameBlend = frameBlend;
		_cachedBatchJob.Positions = _batchPositionQueryArr;
		_cachedBatchJob.Count = positions.Length;
		ref Rust.Water5.GetHeightBatchedJob cachedBatchJob = ref _cachedBatchJob;
		int length = positions.Length;
		int batchSize = ThreadUtils.GetBatchSize(positions.Length);
		JobHandle val = default(JobHandle);
		val = IJobParallelForExtensions.ScheduleByRef<Rust.Water5.GetHeightBatchedJob>(ref cachedBatchJob, length, batchSize, val);
		((JobHandle)(ref val)).Complete();
		for (int i = 0; i < heights.Length; i++)
		{
			heights[i] = _batchPositionQueryArr[i].x * GetHeightAttenuation(shore[i], terrainHeight[i]);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void GetHeightBatch(NativeArray<float3> positions, Span<float> heights, ReadOnlySpan<float> shore, ReadOnlySpan<float> terrainHeight)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		Assert.IsTrue(positions.Length == heights.Length);
		_cachedBatchJob.OneOverOctave0Scale = oneOverOctave0Scale;
		_cachedBatchJob.Frame0 = frame0;
		_cachedBatchJob.Frame1 = frame1;
		_cachedBatchJob.Spectrum0 = spectrum0;
		_cachedBatchJob.Spectrum1 = spectrum1;
		_cachedBatchJob.spectrumBlend = spectrumBlend;
		_cachedBatchJob.frameBlend = frameBlend;
		_cachedBatchJob.Positions = positions;
		_cachedBatchJob.Count = positions.Length;
		ref Rust.Water5.GetHeightBatchedJob cachedBatchJob = ref _cachedBatchJob;
		int length = positions.Length;
		int batchSize = ThreadUtils.GetBatchSize(positions.Length);
		JobHandle val = default(JobHandle);
		val = IJobParallelForExtensions.ScheduleByRef<Rust.Water5.GetHeightBatchedJob>(ref cachedBatchJob, length, batchSize, val);
		((JobHandle)(ref val)).Complete();
		for (int i = 0; i < heights.Length; i++)
		{
			heights[i] = positions[i].x * GetHeightAttenuation(shore[i], terrainHeight[i]);
		}
	}

	public void GetHeightsIndirect(ReadOnly<Vector3> pos, ReadOnly<float> shore, ReadOnly<float> terrainHeight, ReadOnly<int> indices, NativeArray<float> results)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_heightsJobIndirect.Heights = results;
		_heightsJobIndirect.Pos = pos;
		_heightsJobIndirect.ShoreDists = shore;
		_heightsJobIndirect.TerrainHeights = terrainHeight;
		_heightsJobIndirect.Indices = indices;
		_heightsJobIndirect.OneOverOctave0Scale = oneOverOctave0Scale;
		_heightsJobIndirect.Frame0 = frame0;
		_heightsJobIndirect.Frame1 = frame1;
		_heightsJobIndirect.Spectrum0 = spectrum0;
		_heightsJobIndirect.Spectrum1 = spectrum1;
		_heightsJobIndirect.SpectrumBlend = spectrumBlend;
		_heightsJobIndirect.FrameBlend = frameBlend;
		_heightsJobIndirect.DistanceAttenuationFactor = distanceAttenuationFactor;
		_heightsJobIndirect.DepthAttenuationFactor = depthAttenuationFactor;
		IJobExtensions.RunByRef<Rust.Water5.GetHeightsJobIndirect>(ref _heightsJobIndirect);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetHeight(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		float heightAttenuation = GetHeightAttenuation(position);
		_cachedBatchJob.OneOverOctave0Scale = oneOverOctave0Scale;
		_cachedBatchJob.Frame0 = frame0;
		_cachedBatchJob.Frame1 = frame1;
		_cachedBatchJob.Spectrum0 = spectrum0;
		_cachedBatchJob.Spectrum1 = spectrum1;
		_cachedBatchJob.spectrumBlend = spectrumBlend;
		_cachedBatchJob.frameBlend = frameBlend;
		PopulateBatchNative(position);
		_cachedBatchJob.Positions = _batchPositionQueryArr;
		_cachedBatchJob.Count = 1;
		IJobExtensions.RunByRef<Rust.Water5.GetHeightBatchedJob>(ref _cachedBatchJob);
		return _cachedBatchJob.Positions[0].x * heightAttenuation;
	}

	private void PopulateBatchNative(Vector3 position)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (!_batchPositionQueryArr.IsCreated || _batchPositionQueryArr.Length < 1)
		{
			if (_batchPositionQueryArr.IsCreated)
			{
				_batchPositionQueryArr.Dispose();
			}
			_batchPositionQueryArr = new NativeArray<float3>(1, (Allocator)4, (NativeArrayOptions)1);
		}
		_batchPositionQueryArr[0] = float3.op_Implicit(position);
	}

	private void PopulateBatchNative(Vector2[] positions)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (!_batchPositionQueryArr.IsCreated || _batchPositionQueryArr.Length < positions.Length)
		{
			if (_batchPositionQueryArr.IsCreated)
			{
				_batchPositionQueryArr.Dispose();
			}
			_batchPositionQueryArr = new NativeArray<float3>(positions.Length, (Allocator)4, (NativeArrayOptions)1);
		}
		for (int i = 0; i < positions.Length; i++)
		{
			_batchPositionQueryArr[i] = float3.op_Implicit(Vector3Ex.XZ3D(positions[i]));
		}
	}

	private void PopulateBatchNative(ReadOnlySpan<Vector2> positions)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (!_batchPositionQueryArr.IsCreated || _batchPositionQueryArr.Length < positions.Length)
		{
			if (_batchPositionQueryArr.IsCreated)
			{
				_batchPositionQueryArr.Dispose();
			}
			_batchPositionQueryArr = new NativeArray<float3>(positions.Length, (Allocator)4, (NativeArrayOptions)1);
		}
		for (int i = 0; i < positions.Length; i++)
		{
			_batchPositionQueryArr[i] = float3.op_Implicit(Vector3Ex.XZ3D(positions[i]));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetHeightAttenuation(float shore, float terrainHeight)
	{
		float num = Mathf.Clamp01(shore / distanceAttenuationFactor);
		float num2 = Mathf.Clamp01(Mathf.Abs(terrainHeight) / depthAttenuationFactor);
		return num * num2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetHeightAttenuation(Vector3 position)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		float num = (((Object)(object)TerrainTexturing.Instance != (Object)null) ? TerrainTexturing.Instance.GetCoarseDistanceToShore(position) : 0f);
		float num2 = (((Object)(object)TerrainMeta.HeightMap != (Object)null) ? TerrainMeta.HeightMap.GetHeight(position) : 0f);
		float num3 = Mathf.Clamp01(num / distanceAttenuationFactor);
		float num4 = Mathf.Clamp01(Mathf.Abs(num2) / depthAttenuationFactor);
		return num3 * num4;
	}
}
