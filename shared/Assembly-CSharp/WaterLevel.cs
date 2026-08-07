using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Facepunch;
using Rust.Water5;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UtilityJobs;
using WaterLevelJobs;

public static class WaterLevel
{
	public struct WaterInfo
	{
		[MarshalAs(UnmanagedType.U1)]
		public bool isValid;

		public float currentDepth;

		public float overallDepth;

		public float surfaceLevel;

		public float terrainHeight;

		[MarshalAs(UnmanagedType.U1)]
		public bool artificalWater;

		public int topology;

		public static bool ApproxEquals(in WaterInfo left, in WaterInfo right, float epsilon = 1E-05f)
		{
			float num = left.currentDepth - right.currentDepth;
			float num2 = left.overallDepth - right.overallDepth;
			float num3 = left.surfaceLevel - right.surfaceLevel;
			float num4 = left.terrainHeight - right.terrainHeight;
			float num5 = epsilon * epsilon;
			if (left.isValid == right.isValid && num * num < num5 && num2 * num2 < num5 && num3 * num3 < num5 && num4 * num4 < num5 && left.artificalWater == right.artificalWater)
			{
				return left.topology == right.topology;
			}
			return false;
		}
	}

	public const float InvalidWaterHeight = -1000f;

	private static NativeReference<int> CounterRef;

	private static NativeReference<int> DeepCounterRef;

	private static NativeArray<Vector3> Centers;

	private static NativeArray<float> WaterHeights;

	private static NativeArray<float> TerrainHeights;

	private static NativeArray<int> Indices;

	private static NativeArray<int> DeepIndices;

	private static NativeArray<bool> GetIgnoreResults;

	private static NativeArray<Vector3> GetIgnoreHeadStarts;

	private static NativeArray<float> GetIgnoreHeadRadii;

	private static NativeArray<Vector2> UVs;

	private static NativeArray<int> Topologies;

	private static NativeArray<float> ShoreDists;

	private static NativeArray<float> WaveHeights;

	private static NativeArray<float> WaterLevels;

	public static float Factor(Vector3 start, Vector3 end, float radius, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterLevel.Factor"))
		{
			return Factor(GetWaterInfo(start, end, radius, waves, volumes, forEntity), start, end, radius);
		}
	}

	public static float Factor(in WaterInfo info, Vector3 start, Vector3 end, float radius)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!info.isValid)
		{
			return 0f;
		}
		return Mathf.InverseLerp(Mathf.Min(start.y, end.y) - radius, Mathf.Max(start.y, end.y) + radius, info.surfaceLevel);
	}

	public static float Factor(Bounds bounds, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterLevel.Factor"))
		{
			if (((Bounds)(ref bounds)).size == Vector3.zero)
			{
				((Bounds)(ref bounds)).size = new Vector3(0.1f, 0.1f, 0.1f);
			}
			WaterInfo waterInfo = GetWaterInfo(bounds, waves, volumes, forEntity);
			return waterInfo.isValid ? Mathf.InverseLerp(((Bounds)(ref bounds)).min.y, ((Bounds)(ref bounds)).max.y, waterInfo.surfaceLevel) : 0f;
		}
	}

	public static float Factor(in WaterInfo info, Bounds bounds)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (((Bounds)(ref bounds)).size == Vector3.zero)
		{
			((Bounds)(ref bounds)).size = new Vector3(0.1f, 0.1f, 0.1f);
		}
		if (!info.isValid)
		{
			return 0f;
		}
		return Mathf.InverseLerp(((Bounds)(ref bounds)).min.y, ((Bounds)(ref bounds)).max.y, info.surfaceLevel);
	}

	public static bool Test(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterLevel.Test"))
		{
			return GetWaterInfo(pos, waves, volumes, forEntity).isValid;
		}
	}

	public static bool Test(in WaterInfo info, bool volumes, Vector3 pos, BaseEntity forEntity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		bool flag = pos.y >= info.terrainHeight - 1f && pos.y <= info.surfaceLevel;
		if (!flag && volumes)
		{
			flag = GetWaterInfoFromVolumes(pos, forEntity).isValid;
		}
		return flag;
	}

	public static (float, float) GetWaterAndTerrainSurface(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterLevel.GetWaterDepth"))
		{
			WaterInfo waterInfo = GetWaterInfo(pos, waves, volumes, forEntity);
			return (waterInfo.surfaceLevel, waterInfo.terrainHeight);
		}
	}

	public static float GetWaterOrTerrainSurface(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterLevel.GetWaterDepth"))
		{
			WaterInfo waterInfo = GetWaterInfo(pos, waves, volumes, forEntity);
			return Mathf.Max(waterInfo.surfaceLevel, waterInfo.terrainHeight);
		}
	}

	public static float GetWaterSurface(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterLevel.GetWaterDepth"))
		{
			return GetWaterInfo(pos, waves, volumes, forEntity).surfaceLevel;
		}
	}

	public static float GetWaterDepth(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterLevel.GetWaterDepth"))
		{
			return GetWaterInfo(pos, waves, volumes, forEntity).currentDepth;
		}
	}

	public static float GetOverallWaterDepth(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterLevel.GetOverallWaterDepth"))
		{
			return GetWaterInfo(pos, waves, volumes, forEntity).overallDepth;
		}
	}

	public static Vector3 GetWaterFlowDirection(Vector3 worldPosition)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TerrainMeta.WaterFlowMap == (Object)null)
		{
			return Vector3.zero;
		}
		return TerrainMeta.WaterFlowMap.GetFlowDirection(worldPosition);
	}

	public static Vector3 GetWaterNormal(Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.up;
	}

	public static WaterInfo GetBuoyancyWaterInfo(Vector3 pos, Vector2 posUV, float terrainHeight, float waterHeight, bool doDeepwaterChecks, BaseEntity forEntity)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterLevel.GetWaterInfo"))
		{
			WaterInfo result = default(WaterInfo);
			if (pos.y > waterHeight)
			{
				return GetWaterInfoFromVolumes(pos, forEntity);
			}
			bool flag = pos.y < terrainHeight - 1f;
			if (flag)
			{
				return GetWaterInfoFromVolumes(pos, forEntity);
			}
			bool flag2 = doDeepwaterChecks && (pos.y < waterHeight - 10f || (TerrainMeta.OutOfBounds(pos) && !DeepSeaManager.IsInsideDeepSea(pos)));
			int num = (Object.op_Implicit((Object)(object)TerrainMeta.TopologyMap) ? TerrainMeta.TopologyMap.GetTopologyFast(posUV) : 0);
			if ((flag || flag2 || (num & 0x3C180) == 0) && Object.op_Implicit((Object)(object)WaterSystem.Collision) && WaterSystem.Collision.GetIgnore(pos))
			{
				return result;
			}
			RaycastHit val = default(RaycastHit);
			if (flag2 && Physics.Raycast(pos, Vector3.up, ref val, 5f, 16, (QueryTriggerInteraction)2))
			{
				float num2 = waterHeight;
				Bounds bounds = ((RaycastHit)(ref val)).collider.bounds;
				waterHeight = Mathf.Min(num2, ((Bounds)(ref bounds)).max.y);
			}
			result.isValid = true;
			result.currentDepth = Mathf.Max(0f, waterHeight - pos.y);
			result.overallDepth = Mathf.Max(0f, waterHeight - terrainHeight);
			result.surfaceLevel = waterHeight;
			result.terrainHeight = terrainHeight;
			result.topology = num;
			return result;
		}
	}

	public static WaterInfo GetWaterInfo(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterLevel.GetWaterInfo"))
		{
			WaterInfo result = default(WaterInfo);
			float num = GetWaterLevel(pos, waves);
			float num2 = ((Object.op_Implicit((Object)(object)TerrainMeta.HeightMap) && TerrainMeta.HeightMap.isInitialized) ? TerrainMeta.HeightMap.GetHeight(pos) : 0f);
			result.isValid = true;
			if (pos.y > num)
			{
				result.isValid = false;
			}
			else if (pos.y < num2 - 1f)
			{
				result.isValid = false;
			}
			bool flag = false;
			if (!result.isValid && volumes)
			{
				result = GetWaterInfoFromVolumes(pos, forEntity);
				if (result.isValid)
				{
					flag = true;
					num = result.surfaceLevel;
				}
			}
			if (result.isValid && Object.op_Implicit((Object)(object)WaterSystem.Collision) && WaterSystem.Collision.GetIgnore(pos))
			{
				result.isValid = false;
				num = -1000f;
			}
			result.currentDepth = Mathf.Max(0f, num - pos.y);
			if (!flag)
			{
				result.overallDepth = Mathf.Max(0f, num - num2);
			}
			result.surfaceLevel = num;
			result.terrainHeight = num2;
			return result;
		}
	}

	public static WaterInfo GetWaterInfo(Bounds bounds, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterLevel.GetWaterInfo"))
		{
			WaterInfo result = default(WaterInfo);
			float num = GetWaterLevel(((Bounds)(ref bounds)).center, waves);
			float num2 = (Object.op_Implicit((Object)(object)TerrainMeta.HeightMap) ? TerrainMeta.HeightMap.GetHeight(((Bounds)(ref bounds)).center) : 0f);
			result.isValid = true;
			if (((Bounds)(ref bounds)).min.y > num)
			{
				result.isValid = false;
			}
			else if (((Bounds)(ref bounds)).max.y < num2 - 1f)
			{
				result.isValid = false;
			}
			if (!result.isValid && volumes)
			{
				result = GetWaterInfoFromVolumes(bounds, forEntity);
				if (result.isValid)
				{
					num = result.surfaceLevel;
				}
			}
			if (result.isValid && Object.op_Implicit((Object)(object)WaterSystem.Collision) && WaterSystem.Collision.GetIgnore(bounds))
			{
				result.isValid = false;
				num = -1000f;
			}
			result.currentDepth = Mathf.Max(0f, num - ((Bounds)(ref bounds)).min.y);
			result.overallDepth = Mathf.Max(0f, num - num2);
			result.surfaceLevel = num;
			result.terrainHeight = num2;
			return result;
		}
	}

	public static void InitInternalState(int initCap)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		DisposeInternalState();
		CounterRef = new NativeReference<int>(AllocatorHandle.op_Implicit((Allocator)4), (NativeArrayOptions)0);
		DeepCounterRef = new NativeReference<int>(AllocatorHandle.op_Implicit((Allocator)4), (NativeArrayOptions)0);
		Centers = new NativeArray<Vector3>(initCap, (Allocator)4, (NativeArrayOptions)0);
		WaterHeights = new NativeArray<float>(initCap, (Allocator)4, (NativeArrayOptions)0);
		TerrainHeights = new NativeArray<float>(initCap, (Allocator)4, (NativeArrayOptions)0);
		Indices = new NativeArray<int>(initCap, (Allocator)4, (NativeArrayOptions)0);
		DeepIndices = new NativeArray<int>(initCap, (Allocator)4, (NativeArrayOptions)0);
		GetIgnoreResults = new NativeArray<bool>(initCap, (Allocator)4, (NativeArrayOptions)0);
		GetIgnoreHeadStarts = new NativeArray<Vector3>(initCap, (Allocator)4, (NativeArrayOptions)0);
		GetIgnoreHeadRadii = new NativeArray<float>(initCap, (Allocator)4, (NativeArrayOptions)0);
		UVs = new NativeArray<Vector2>(initCap, (Allocator)4, (NativeArrayOptions)0);
		Topologies = new NativeArray<int>(initCap, (Allocator)4, (NativeArrayOptions)0);
		ShoreDists = new NativeArray<float>(initCap, (Allocator)4, (NativeArrayOptions)0);
		WaveHeights = new NativeArray<float>(initCap, (Allocator)4, (NativeArrayOptions)0);
		WaterLevels = new NativeArray<float>(initCap, (Allocator)4, (NativeArrayOptions)0);
	}

	public static void DisposeInternalState()
	{
		NativeReferenceEx.SafeDispose(ref CounterRef);
		NativeReferenceEx.SafeDispose(ref DeepCounterRef);
		Centers.SafeDispose<Vector3>();
		NativeArrayEx.SafeDispose(ref WaterHeights);
		NativeArrayEx.SafeDispose(ref TerrainHeights);
		NativeArrayEx.SafeDispose(ref Indices);
		NativeArrayEx.SafeDispose(ref DeepIndices);
		NativeArrayEx.SafeDispose(ref GetIgnoreResults);
		GetIgnoreHeadStarts.SafeDispose<Vector3>();
		NativeArrayEx.SafeDispose(ref GetIgnoreHeadRadii);
		UVs.SafeDispose<Vector2>();
		NativeArrayEx.SafeDispose(ref Topologies);
		NativeArrayEx.SafeDispose(ref ShoreDists);
		NativeArrayEx.SafeDispose(ref WaveHeights);
		NativeArrayEx.SafeDispose(ref WaterLevels);
	}

	public static void GetWaterInfos(ReadOnly<Vector3> poses, bool waves, bool volumes, ReadOnlySpan<BaseEntity> entities, NativeArray<WaterInfo> results)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GetWaterInfos"))
		{
			NativeList<int> values = new NativeList<int>(poses.Length, AllocatorHandle.op_Implicit((Allocator)3));
			try
			{
				GenerateAscSeqListJob generateAscSeqListJob = new GenerateAscSeqListJob
				{
					Values = values,
					Start = 0,
					Step = 1,
					Count = poses.Length
				};
				IJobExtensions.RunByRef<GenerateAscSeqListJob>(ref generateAscSeqListJob);
				FillJob<WaterInfo> fillJob = new FillJob<WaterInfo>
				{
					Values = results,
					Value = new WaterInfo
					{
						isValid = true
					}
				};
				IJobExtensions.RunByRef<FillJob<WaterInfo>>(ref fillJob);
				NativeArray<bool> useVolumeDepths = new NativeArray<bool>(poses.Length, (Allocator)3, (NativeArrayOptions)1);
				try
				{
					NativeArrayEx.Expand(ref WaterHeights, poses.Length, (NativeArrayOptions)0, copyContents: false);
					GetWaterLevels(poses, values.AsReadOnly(), waves, WaterHeights);
					NativeArrayEx.Expand(ref TerrainHeights, poses.Length, (NativeArrayOptions)0, copyContents: false);
					TerrainMeta.HeightMap?.GetHeightsIndirect(poses, values.AsReadOnly(), TerrainHeights);
					InitialValidateSimpleInfoJobIndirect initialValidateSimpleInfoJobIndirect = new InitialValidateSimpleInfoJobIndirect
					{
						Results = results,
						Poses = poses,
						WaterHeights = WaterHeights.AsReadOnly(),
						TerrainHeights = TerrainHeights.AsReadOnly(),
						Indices = values.AsReadOnly()
					};
					IJobExtensions.RunByRef<InitialValidateSimpleInfoJobIndirect>(ref initialValidateSimpleInfoJobIndirect);
					if (volumes)
					{
						using (TimeWarning.New("WaterTestFromVolumes"))
						{
							GatherInvalidInfosJobIndirect gatherInvalidInfosJobIndirect = new GatherInvalidInfosJobIndirect
							{
								InvalidIndices = Indices,
								InvalidIndexCount = CounterRef,
								Infos = results.AsReadOnly(),
								Indices = values.AsReadOnly()
							};
							IJobExtensions.RunByRef<GatherInvalidInfosJobIndirect>(ref gatherInvalidInfosJobIndirect);
							int value = CounterRef.Value;
							if (value > 0)
							{
								NativeArray<int> subArray = Indices.GetSubArray(0, value);
								BaseEntity.WaterTestFromVolumesIndirect(entities, ReadOnly<Vector3>.op_Implicit(ref poses), NativeArray<int>.op_Implicit(ref subArray), NativeArray<WaterInfo>.op_Implicit(ref results));
								UpdateWaterHeightsWithVolumesJobIndirect updateWaterHeightsWithVolumesJobIndirect = new UpdateWaterHeightsWithVolumesJobIndirect
								{
									WaterHeights = WaterHeights,
									UseVolumeDepths = useVolumeDepths,
									Infos = results,
									Indices = subArray
								};
								IJobExtensions.RunByRef<UpdateWaterHeightsWithVolumesJobIndirect>(ref updateWaterHeightsWithVolumesJobIndirect);
							}
						}
					}
					if (Object.op_Implicit((Object)(object)WaterSystem.Collision))
					{
						NativeArrayEx.Expand(ref Indices, poses.Length, (NativeArrayOptions)0, copyContents: false);
						GatherValidInfosJobIndirect gatherValidInfosJobIndirect = new GatherValidInfosJobIndirect
						{
							ValidIndices = Indices,
							ValidIndexCount = CounterRef,
							Infos = results.AsReadOnly(),
							Indices = values.AsReadOnly()
						};
						IJobExtensions.RunByRef<GatherValidInfosJobIndirect>(ref gatherValidInfosJobIndirect);
						int value2 = CounterRef.Value;
						if (value2 > 0)
						{
							using (TimeWarning.New("WaterSystem.Collision.Entity"))
							{
								NativeArray<float> values2 = new NativeArray<float>(poses.Length, (Allocator)3, (NativeArrayOptions)0);
								try
								{
									FillJob<float> fillJob2 = new FillJob<float>
									{
										Values = values2,
										Value = 0.01f
									};
									IJobExtensions.RunByRef<FillJob<float>>(ref fillJob2);
									NativeArray<int> subArray2 = Indices.GetSubArray(0, value2);
									NativeArrayEx.Expand(ref GetIgnoreResults, poses.Length, (NativeArrayOptions)0, copyContents: false);
									WaterSystem.Collision.GetIgnoreIndirect(poses, values2.AsReadOnly(), subArray2.AsReadOnly(), GetIgnoreResults);
									ResolveIgnoreWaterInfosJob resolveIgnoreWaterInfosJob = new ResolveIgnoreWaterInfosJob
									{
										Infos = results,
										WaterHeights = WaterHeights,
										Indices = values.AsReadOnly(),
										Results = GetIgnoreResults.AsReadOnly()
									};
									IJobExtensions.RunByRef<ResolveIgnoreWaterInfosJob>(ref resolveIgnoreWaterInfosJob);
								}
								finally
								{
									((IDisposable)values2/*cast due to constrained. prefix*/).Dispose();
								}
							}
						}
					}
					ResolveWaterInfosSimpleJobIndirect resolveWaterInfosSimpleJobIndirect = new ResolveWaterInfosSimpleJobIndirect
					{
						Infos = results,
						Poses = poses,
						WaterHeights = WaterHeights.AsReadOnly(),
						TerrainHeights = TerrainHeights.AsReadOnly(),
						UseVolumeDepths = useVolumeDepths.AsReadOnly(),
						Indices = values.AsReadOnly()
					};
					IJobExtensions.RunByRef<ResolveWaterInfosSimpleJobIndirect>(ref resolveWaterInfosSimpleJobIndirect);
				}
				finally
				{
					((IDisposable)useVolumeDepths/*cast due to constrained. prefix*/).Dispose();
				}
			}
			finally
			{
				((IDisposable)values/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	public static void GetWaterInfos(ReadOnly<Vector3> starts, ReadOnly<Vector3> ends, ReadOnly<float> radii, ReadOnlySpan<BaseEntity> entities, ReadOnly<int> indices, bool waves, bool volumes, NativeArray<WaterInfo> results)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_0454: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_0471: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GetWaterInfos"))
		{
			Centers.Expand<Vector3>(starts.Length, (NativeArrayOptions)0, false, false);
			CalcCenterJobIndirect calcCenterJobIndirect = new CalcCenterJobIndirect
			{
				Results = Centers,
				Starts = starts,
				Ends = ends,
				Indices = indices
			};
			IJobExtensions.RunByRef<CalcCenterJobIndirect>(ref calcCenterJobIndirect);
			NativeArrayEx.Expand(ref WaterHeights, starts.Length, (NativeArrayOptions)0, copyContents: false);
			GetWaterLevels(Centers.AsReadOnly(), indices, waves, WaterHeights);
			NativeArrayEx.Expand(ref TerrainHeights, starts.Length, (NativeArrayOptions)0, copyContents: false);
			TerrainMeta.HeightMap?.GetHeightsIndirect(Centers.AsReadOnly(), indices, TerrainHeights);
			NativeArrayEx.Expand(ref Indices, starts.Length, (NativeArrayOptions)0, copyContents: false);
			InitialValidateInfoJobIndirect initialValidateInfoJobIndirect = new InitialValidateInfoJobIndirect
			{
				Results = results,
				Starts = starts,
				Ends = ends,
				Radii = radii,
				WaterHeights = WaterHeights.AsReadOnly(),
				TerrainHeights = TerrainHeights.AsReadOnly(),
				Indices = indices
			};
			IJobExtensions.RunByRef<InitialValidateInfoJobIndirect>(ref initialValidateInfoJobIndirect);
			if (volumes)
			{
				using (TimeWarning.New("WaterTestFromVolumes"))
				{
					GatherInvalidInfosJobIndirect gatherInvalidInfosJobIndirect = new GatherInvalidInfosJobIndirect
					{
						InvalidIndices = Indices,
						InvalidIndexCount = CounterRef,
						Infos = results.AsReadOnly(),
						Indices = indices
					};
					IJobExtensions.RunByRef<GatherInvalidInfosJobIndirect>(ref gatherInvalidInfosJobIndirect);
					int value = CounterRef.Value;
					if (value > 0)
					{
						NativeArray<int> subArray = Indices.GetSubArray(0, value);
						BaseEntity.WaterTestFromVolumesIndirect(entities, ReadOnly<Vector3>.op_Implicit(ref starts), ReadOnly<Vector3>.op_Implicit(ref ends), ReadOnly<float>.op_Implicit(ref radii), NativeArray<int>.op_Implicit(ref subArray), NativeArray<WaterInfo>.op_Implicit(ref results));
						UpdateWaterHeightsJobIndirect updateWaterHeightsJobIndirect = new UpdateWaterHeightsJobIndirect
						{
							WaterHeights = WaterHeights,
							Infos = results,
							Indices = subArray
						};
						IJobExtensions.RunByRef<UpdateWaterHeightsJobIndirect>(ref updateWaterHeightsJobIndirect);
					}
				}
			}
			if (Object.op_Implicit((Object)(object)WaterSystem.Collision))
			{
				using (TimeWarning.New("WaterSystem.Collision"))
				{
					GatherValidInfosJobIndirect gatherValidInfosJobIndirect = new GatherValidInfosJobIndirect
					{
						ValidIndices = Indices,
						ValidIndexCount = CounterRef,
						Infos = results.AsReadOnly(),
						Indices = indices
					};
					IJobExtensions.RunByRef<GatherValidInfosJobIndirect>(ref gatherValidInfosJobIndirect);
					int value2 = CounterRef.Value;
					if (value2 > 0)
					{
						using (TimeWarning.New("WaterSystem.Collision.Entity"))
						{
							NativeArray<int> subArray2 = Indices.GetSubArray(0, value2);
							NativeArrayEx.Expand(ref GetIgnoreResults, starts.Length, (NativeArrayOptions)0, copyContents: false);
							WaterSystem.Collision.GetIgnoreIndirect(starts, ends, radii, subArray2.AsReadOnly(), GetIgnoreResults);
							GetIgnoreHeadStarts.Expand<Vector3>(starts.Length, (NativeArrayOptions)0, false, false);
							NativeArrayEx.Expand(ref GetIgnoreHeadRadii, starts.Length, (NativeArrayOptions)0, copyContents: false);
							SetupHeadQueryJobIndirect setupHeadQueryJobIndirect = new SetupHeadQueryJobIndirect
							{
								Indices = subArray2,
								QueryIndexCount = CounterRef,
								QueryStarts = GetIgnoreHeadStarts,
								QueryRadii = GetIgnoreHeadRadii,
								ValidInfos = GetIgnoreResults.AsReadOnly(),
								Starts = starts,
								Ends = ends,
								Radii = radii
							};
							IJobExtensions.RunByRef<SetupHeadQueryJobIndirect>(ref setupHeadQueryJobIndirect);
							int value3 = CounterRef.Value;
							if (value3 > 0)
							{
								using (TimeWarning.New("WaterSystem.Collision.Head"))
								{
									NativeArray<int> subArray3 = Indices.GetSubArray(0, value3);
									WaterSystem.Collision.GetIgnoreIndirect(GetIgnoreHeadStarts.AsReadOnly(), GetIgnoreHeadRadii.AsReadOnly(), subArray3.AsReadOnly(), GetIgnoreResults);
									ApplyHeadQueryResultsJobIndirect applyHeadQueryResultsJobIndirect = new ApplyHeadQueryResultsJobIndirect
									{
										WaterHeights = WaterHeights,
										Infos = results,
										Indices = subArray3.AsReadOnly(),
										ValidInfos = GetIgnoreResults.AsReadOnly(),
										Starts = GetIgnoreHeadStarts.AsReadOnly()
									};
									IJobExtensions.RunByRef<ApplyHeadQueryResultsJobIndirect>(ref applyHeadQueryResultsJobIndirect);
								}
							}
						}
					}
				}
			}
			ResolveWaterInfosJobIndirect resolveWaterInfosJobIndirect = new ResolveWaterInfosJobIndirect
			{
				Infos = results,
				Starts = starts,
				Ends = ends,
				Radii = radii,
				WaterHeights = WaterHeights.AsReadOnly(),
				TerrainHeights = TerrainHeights.AsReadOnly(),
				Indices = indices
			};
			IJobExtensions.RunByRef<ResolveWaterInfosJobIndirect>(ref resolveWaterInfosJobIndirect);
		}
	}

	public static WaterInfo GetWaterInfo(Vector3 start, Vector3 end, float radius, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterLevel.GetWaterInfo"))
		{
			Vector3 val = (start + end) * 0.5f;
			float num = Mathf.Min(start.y, end.y) - radius;
			float num2 = Mathf.Max(start.y, end.y) + radius;
			float num3 = GetWaterLevel(val, waves);
			float num4 = (Object.op_Implicit((Object)(object)TerrainMeta.HeightMap) ? TerrainMeta.HeightMap.GetHeight(val) : 0f);
			WaterInfo result = InitialValidate(num, num2, num3, num4);
			if (!result.isValid && volumes)
			{
				result = GetWaterInfoFromVolumes(start, end, radius, forEntity);
				if (result.isValid)
				{
					num3 = result.surfaceLevel;
				}
			}
			if (result.isValid && Object.op_Implicit((Object)(object)WaterSystem.Collision) && WaterSystem.Collision.GetIgnore(start, end, radius))
			{
				Vector3 val2 = Vector3Ex.WithY(val, Mathf.Lerp(num, num2, 0.75f));
				if (!WaterSystem.Collision.GetIgnore(val2))
				{
					num3 = Mathf.Min(num3, val2.y);
				}
				else
				{
					result.isValid = false;
					num3 = -1000f;
				}
			}
			result.currentDepth = Mathf.Max(0f, num3 - num);
			result.overallDepth = Mathf.Max(0f, num3 - num4);
			result.surfaceLevel = num3;
			result.terrainHeight = num4;
			return result;
		}
	}

	public static WaterInfo GetWaterInfo(Camera cam, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterLevel.GetWaterInfo"))
		{
			waves = waves && (Object)(object)WaterSystem.Instance != (Object)null;
			float num = WaterSystem.OceanLevel;
			if (waves)
			{
				num += WaterSystem.Instance.GetOceanSimulation(((Component)cam).transform.position).MinLevel();
			}
			if (((Component)cam).transform.position.y < num - 1f)
			{
				return GetWaterInfo(((Component)cam).transform.position, waves, volumes, forEntity);
			}
			return GetWaterInfo(((Component)cam).transform.position - Vector3.up, waves, volumes, forEntity);
		}
	}

	public static float GetWaterLevel(Vector3 pos, bool waves)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		waves = waves && (Object)(object)WaterSystem.Instance != (Object)null;
		float normX = TerrainMeta.NormalizeX(pos.x);
		float normZ = TerrainMeta.NormalizeZ(pos.z);
		float num = (Object.op_Implicit((Object)(object)TerrainMeta.WaterMap) ? TerrainMeta.WaterMap.GetHeight(pos) : TerrainMeta.Position.y);
		float num2 = WaterSystem.OceanLevel;
		OceanSimulation oceanSimulation = (waves ? WaterSystem.Instance.GetOceanSimulation(pos) : null);
		if (waves)
		{
			num2 += oceanSimulation.MaxLevel();
		}
		if (num < num2 && (!Object.op_Implicit((Object)(object)TerrainMeta.TopologyMap) || TerrainMeta.TopologyMap.GetTopology(normX, normZ, 384)))
		{
			float num3 = WaterSystem.OceanLevel;
			if (waves)
			{
				num3 += oceanSimulation.GetHeight(pos);
			}
			return Mathf.Max(num, num3);
		}
		return num;
	}

	public static float RaycastWaterColliders(Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (!Physics.Raycast(Vector3Ex.WithY(pos, TerrainMeta.Max.y), Vector3.down, ref val, TerrainMeta.Size.y, 16, (QueryTriggerInteraction)2))
		{
			return WaterSystem.OceanLevel;
		}
		return ((RaycastHit)(ref val)).point.y;
	}

	public static void GetWaterLevels(ReadOnly<Vector3> positions, ReadOnly<int> indices, bool waves, NativeArray<float> heights)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_045c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0461: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_0504: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0571: Unknown result type (might be due to invalid IL or missing references)
		//IL_0527: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_054f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0554: Unknown result type (might be due to invalid IL or missing references)
		//IL_0556: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Unknown result type (might be due to invalid IL or missing references)
		//IL_060a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0614: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Unknown result type (might be due to invalid IL or missing references)
		//IL_061b: Unknown result type (might be due to invalid IL or missing references)
		//IL_062f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0630: Unknown result type (might be due to invalid IL or missing references)
		//IL_063c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0641: Unknown result type (might be due to invalid IL or missing references)
		//IL_0648: Unknown result type (might be due to invalid IL or missing references)
		//IL_064a: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0676: Unknown result type (might be due to invalid IL or missing references)
		//IL_067c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0686: Unknown result type (might be due to invalid IL or missing references)
		//IL_068b: Unknown result type (might be due to invalid IL or missing references)
		//IL_068d: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bc: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterLevels"))
		{
			waves = waves && (Object)(object)WaterSystem.Instance != (Object)null;
			NativeList<int> overworldIndices = new NativeList<int>(positions.Length, AllocatorHandle.op_Implicit((Allocator)3));
			try
			{
				NativeList<int> deepSeaIndices = new NativeList<int>(positions.Length, AllocatorHandle.op_Implicit((Allocator)3));
				try
				{
					FilterPositionsJobIndirect filterPositionsJobIndirect = new FilterPositionsJobIndirect
					{
						DeepSeaIndices = deepSeaIndices,
						OverworldIndices = overworldIndices,
						Positions = positions,
						Indices = indices,
						DeepSeaBounds = DeepSeaManager.DeepSeaBounds
					};
					IJobExtensions.RunByRef<FilterPositionsJobIndirect>(ref filterPositionsJobIndirect);
					NativeArrayEx.Expand(ref WaterLevels, positions.Length, (NativeArrayOptions)0, copyContents: false);
					if (waves)
					{
						GatherMaxWaterLevelsJob gatherMaxWaterLevelsJob = new GatherMaxWaterLevelsJob
						{
							WaterLevels = WaterLevels,
							Positions = positions,
							DeepSeaBounds = DeepSeaManager.DeepSeaBounds,
							waterLevelMain = WaterSystem.Instance.GetOceanSimulation(isDeep: false).MaxLevel() + WaterSystem.OceanLevel,
							waterLevelDeep = WaterSystem.Instance.GetOceanSimulation(isDeep: true).MaxLevel() + WaterSystem.OceanLevel
						};
						IJobExtensions.RunByRef<GatherMaxWaterLevelsJob>(ref gatherMaxWaterLevelsJob);
					}
					else
					{
						FillJobIndirect<float> fillJobIndirect = new FillJobIndirect<float>
						{
							Value = WaterSystem.OceanLevel,
							Values = WaterLevels,
							Indices = indices
						};
						IJobExtensions.RunByRef<FillJobIndirect<float>>(ref fillJobIndirect);
					}
					UVs.Expand<Vector2>(positions.Length, (NativeArrayOptions)0, false, false);
					NativeArray<Vector2> subArray = UVs.GetSubArray(0, positions.Length);
					ToUVJobIndirect toUVJobIndirect = new ToUVJobIndirect
					{
						UV = subArray,
						Pos = positions,
						Indices = overworldIndices.AsReadOnly(),
						TerrainPos = Vector3Ex.XZ2D(TerrainMeta.Position),
						TerrainOneOverSize = Vector3Ex.XZ2D(TerrainMeta.OneOverSize)
					};
					IJobExtensions.RunByRef<ToUVJobIndirect>(ref toUVJobIndirect);
					toUVJobIndirect.Indices = deepSeaIndices.AsReadOnly();
					toUVJobIndirect.TerrainPos = Vector2.op_Implicit(((Bounds)(ref DeepSeaManager.DeepSeaBounds)).min);
					toUVJobIndirect.TerrainOneOverSize = new Vector2(1f / ((Bounds)(ref DeepSeaManager.DeepSeaBounds)).size.x, 1f / ((Bounds)(ref DeepSeaManager.DeepSeaBounds)).size.z);
					IJobExtensions.RunByRef<ToUVJobIndirect>(ref toUVJobIndirect);
					if (Object.op_Implicit((Object)(object)TerrainMeta.WaterMap))
					{
						TerrainMeta.WaterMap.GetHeightsIndirect(subArray.AsReadOnly(), overworldIndices.AsReadOnly(), heights);
						FillJobIndirect<float> fillJobIndirect2 = new FillJobIndirect<float>
						{
							Values = heights,
							Value = TerrainMeta.WaterMap.DeepSeaDepth(),
							Indices = deepSeaIndices.AsReadOnly()
						};
						IJobExtensions.RunByRef<FillJobIndirect<float>>(ref fillJobIndirect2);
					}
					else
					{
						FillJob<float> fillJob = new FillJob<float>
						{
							Values = heights,
							Value = TerrainMeta.Position.y
						};
						IJobExtensions.RunByRef<FillJob<float>>(ref fillJob);
					}
					NativeArrayEx.Expand(ref Topologies, positions.Length, (NativeArrayOptions)0, copyContents: false);
					NativeArray<int> subArray2 = Topologies.GetSubArray(0, positions.Length);
					if (Object.op_Implicit((Object)(object)TerrainMeta.TopologyMap))
					{
						TerrainMeta.TopologyMap.GetTopologiesIndirect(subArray.AsReadOnly(), overworldIndices.AsReadOnly(), subArray2);
						FillJobIndirect<int> fillJobIndirect3 = new FillJobIndirect<int>
						{
							Values = subArray2,
							Value = 128,
							Indices = deepSeaIndices.AsReadOnly()
						};
						IJobExtensions.RunByRef<FillJobIndirect<int>>(ref fillJobIndirect3);
					}
					else
					{
						FillJob<int> fillJob2 = new FillJob<int>
						{
							Values = subArray2,
							Value = 384
						};
						IJobExtensions.RunByRef<FillJob<int>>(ref fillJob2);
					}
					if (!waves)
					{
						ApplyMaxHeightsJobIndirect applyMaxHeightsJobIndirect = new ApplyMaxHeightsJobIndirect
						{
							Heights = heights,
							Topologies = subArray2.AsReadOnly(),
							Indices = indices,
							WaterLevels = WaterLevels.AsReadOnly(),
							OceanLevel = WaterSystem.OceanLevel
						};
						IJobExtensions.RunByRef<ApplyMaxHeightsJobIndirect>(ref applyMaxHeightsJobIndirect);
						return;
					}
					NativeArrayEx.Expand(ref Indices, positions.Length, (NativeArrayOptions)0, copyContents: false);
					NativeArrayEx.Expand(ref DeepIndices, positions.Length, (NativeArrayOptions)0, copyContents: false);
					GatherWavesIndicesJobIndirect gatherWavesIndicesJobIndirect = new GatherWavesIndicesJobIndirect
					{
						WaveIndices = Indices,
						WaveIndexCount = CounterRef,
						Positions = positions,
						Topologies = subArray2.AsReadOnly(),
						Heights = heights.AsReadOnly(),
						Indices = overworldIndices.AsReadOnly(),
						WaterLevels = WaterLevels.AsReadOnly()
					};
					IJobExtensions.RunByRef<GatherWavesIndicesJobIndirect>(ref gatherWavesIndicesJobIndirect);
					gatherWavesIndicesJobIndirect.WaveIndices = DeepIndices;
					gatherWavesIndicesJobIndirect.WaveIndexCount = DeepCounterRef;
					gatherWavesIndicesJobIndirect.Indices = deepSeaIndices.AsReadOnly();
					IJobExtensions.RunByRef<GatherWavesIndicesJobIndirect>(ref gatherWavesIndicesJobIndirect);
					int value = CounterRef.Value;
					int value2 = DeepCounterRef.Value;
					if (value == 0 && value2 == 0)
					{
						return;
					}
					using (TimeWarning.New("Waves"))
					{
						NativeArrayEx.Expand(ref TerrainHeights, positions.Length, (NativeArrayOptions)0, copyContents: false);
						NativeArrayEx.Expand(ref ShoreDists, positions.Length, (NativeArrayOptions)0, copyContents: false);
						NativeArrayEx.Expand(ref WaveHeights, positions.Length, (NativeArrayOptions)0, copyContents: false);
						ReadOnly<int> indices2 = Indices.GetSubArray(0, value).AsReadOnly();
						ReadOnly<int> indices3 = DeepIndices.GetSubArray(0, value2).AsReadOnly();
						if (Object.op_Implicit((Object)(object)TerrainMeta.HeightMap))
						{
							TerrainHeightMap heightMap = TerrainMeta.HeightMap;
							if (value > 0)
							{
								heightMap.GetHeightsIndirect(subArray.AsReadOnly(), heightMap.Data, indices2, TerrainHeights);
							}
							if (value2 > 0)
							{
								heightMap.GetHeightsIndirect(subArray.AsReadOnly(), heightMap.DeepSeaData, indices3, TerrainHeights);
							}
						}
						else
						{
							FillJob<float> fillJob3 = new FillJob<float>
							{
								Values = TerrainHeights,
								Value = 0f
							};
							IJobExtensions.RunByRef<FillJob<float>>(ref fillJob3);
						}
						if (Object.op_Implicit((Object)(object)TerrainTexturing.Instance))
						{
							if (value > 0)
							{
								TerrainTexturing.Instance.GetCoarseDistancesToShoreIndirect(positions, indices2, ShoreDists);
							}
							if (value2 > 0)
							{
								TerrainTexturing.Instance.GetCoarseDistancesToShoreIndirect(positions, indices3, ShoreDists);
							}
						}
						else
						{
							FillJob<float> fillJob4 = new FillJob<float>
							{
								Values = ShoreDists,
								Value = 0f
							};
							IJobExtensions.RunByRef<FillJob<float>>(ref fillJob4);
						}
						if (value > 0)
						{
							WaterSystem.Instance.GetOceanSimulation(isDeep: false).GetHeightsIndirect(positions, ShoreDists.AsReadOnly(), TerrainHeights.AsReadOnly(), indices2, WaveHeights);
							SelectMaxWaterLevelJobIndirect selectMaxWaterLevelJobIndirect = new SelectMaxWaterLevelJobIndirect
							{
								Heights = heights,
								DynamicHeights = WaveHeights.AsReadOnly(),
								Indices = indices2,
								OceanLevel = WaterSystem.OceanLevel
							};
							IJobExtensions.RunByRef<SelectMaxWaterLevelJobIndirect>(ref selectMaxWaterLevelJobIndirect);
						}
						if (value2 > 0)
						{
							WaterSystem.Instance.GetOceanSimulation(isDeep: true).GetHeightsIndirect(positions, ShoreDists.AsReadOnly(), TerrainHeights.AsReadOnly(), indices3, WaveHeights);
							SelectMaxWaterLevelJobIndirect selectMaxWaterLevelJobIndirect2 = new SelectMaxWaterLevelJobIndirect
							{
								Heights = heights,
								DynamicHeights = WaveHeights.AsReadOnly(),
								Indices = indices3,
								OceanLevel = WaterSystem.OceanLevel
							};
							IJobExtensions.RunByRef<SelectMaxWaterLevelJobIndirect>(ref selectMaxWaterLevelJobIndirect2);
						}
					}
				}
				finally
				{
					((IDisposable)deepSeaIndices/*cast due to constrained. prefix*/).Dispose();
				}
			}
			finally
			{
				((IDisposable)overworldIndices/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static WaterInfo GetWaterInfoFromVolumes(Bounds bounds, BaseEntity forEntity)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		WaterInfo info = default(WaterInfo);
		if ((Object)(object)forEntity == (Object)null)
		{
			List<WaterVolume> list = Pool.Get<List<WaterVolume>>();
			Vis.Components<WaterVolume>(new OBB(bounds), list, 262144, (QueryTriggerInteraction)2);
			using (List<WaterVolume>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext() && !enumerator.Current.Test(bounds, out info))
				{
				}
			}
			Pool.FreeUnmanaged<WaterVolume>(ref list);
			return info;
		}
		forEntity.WaterTestFromVolumes(bounds, out info);
		return info;
	}

	private static WaterInfo GetWaterInfoFromVolumes(Vector3 pos, BaseEntity forEntity)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		WaterInfo info = default(WaterInfo);
		if ((Object)(object)forEntity == (Object)null)
		{
			List<WaterVolume> list = Pool.Get<List<WaterVolume>>();
			Vis.Components<WaterVolume>(pos, 0.1f, list, 262144, (QueryTriggerInteraction)2);
			foreach (WaterVolume item in list)
			{
				if (item.Test(pos, out info))
				{
					info.artificalWater = !item.naturalSource;
					break;
				}
			}
			Pool.FreeUnmanaged<WaterVolume>(ref list);
			return info;
		}
		forEntity.WaterTestFromVolumes(pos, out info);
		return info;
	}

	private static WaterInfo GetWaterInfoFromVolumes(Vector3 start, Vector3 end, float radius, BaseEntity forEntity)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		WaterInfo info = default(WaterInfo);
		if ((Object)(object)forEntity == (Object)null)
		{
			List<WaterVolume> list = Pool.Get<List<WaterVolume>>();
			Vis.Components<WaterVolume>(start, end, radius, list, 262144, (QueryTriggerInteraction)2);
			using (List<WaterVolume>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext() && !enumerator.Current.Test(start, end, radius, out info))
				{
				}
			}
			Pool.FreeUnmanaged<WaterVolume>(ref list);
			return info;
		}
		forEntity.WaterTestFromVolumes(start, end, radius, out info);
		return info;
	}

	public static WaterInfo InitialValidate(float minY, float maxY, float waterHeight, float terrainHeight)
	{
		WaterInfo result = new WaterInfo
		{
			isValid = true
		};
		if (minY > waterHeight)
		{
			result.isValid = false;
		}
		else if (maxY < terrainHeight - 1f)
		{
			result.isValid = false;
		}
		if (result.isValid && terrainHeight >= waterHeight + 0.015f)
		{
			result.isValid = false;
		}
		return result;
	}
}
