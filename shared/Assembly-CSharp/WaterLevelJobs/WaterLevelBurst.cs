using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile]
public static class WaterLevelBurst
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GetBuoyancyWaterInfoBatched_00008434_0024PostfixBurstDelegate(in NativeArray<Vector3> allPositions, in NativeArray<Vector2> allUVPositions, in NativeArray<float> pointTerrainHeightNativeArray, in NativeArray<float> pointWaterHeightNativeArray, in NativeArray<bool> doDeepWaterChecksStateNativeArray, ref NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<int> instancePointCountNativeArray, in int instanceCount, in TerrainTopologyMap.TopologyQueryStructure topologyMap, in NativeArray<bool> waterIgnoreStates, ref NativeArray<bool> needsDeepWaterChecks, bool isDeepSea, out bool hasAnyDeepWaterChecks);

	internal static class GetBuoyancyWaterInfoBatched_00008434_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GetBuoyancyWaterInfoBatched_00008434_0024PostfixBurstDelegate>((GetBuoyancyWaterInfoBatched_00008434_0024PostfixBurstDelegate)GetBuoyancyWaterInfoBatched).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<Vector3> allPositions, in NativeArray<Vector2> allUVPositions, in NativeArray<float> pointTerrainHeightNativeArray, in NativeArray<float> pointWaterHeightNativeArray, in NativeArray<bool> doDeepWaterChecksStateNativeArray, ref NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<int> instancePointCountNativeArray, in int instanceCount, in TerrainTopologyMap.TopologyQueryStructure topologyMap, in NativeArray<bool> waterIgnoreStates, ref NativeArray<bool> needsDeepWaterChecks, bool isDeepSea, out bool hasAnyDeepWaterChecks)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector3>, ref NativeArray<Vector2>, ref NativeArray<float>, ref NativeArray<float>, ref NativeArray<bool>, ref NativeArray<WaterLevel.WaterInfo>, ref NativeArray<int>, ref int, ref TerrainTopologyMap.TopologyQueryStructure, ref NativeArray<bool>, ref NativeArray<bool>, bool, ref bool, void>)functionPointer)(ref allPositions, ref allUVPositions, ref pointTerrainHeightNativeArray, ref pointWaterHeightNativeArray, ref doDeepWaterChecksStateNativeArray, ref pointWaterInfoNativeArray, ref instancePointCountNativeArray, ref instanceCount, ref topologyMap, ref waterIgnoreStates, ref needsDeepWaterChecks, isDeepSea, ref hasAnyDeepWaterChecks);
					return;
				}
			}
			GetBuoyancyWaterInfoBatched_0024BurstManaged(in allPositions, in allUVPositions, in pointTerrainHeightNativeArray, in pointWaterHeightNativeArray, in doDeepWaterChecksStateNativeArray, ref pointWaterInfoNativeArray, in instancePointCountNativeArray, in instanceCount, in topologyMap, in waterIgnoreStates, ref needsDeepWaterChecks, isDeepSea, out hasAnyDeepWaterChecks);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ConstructDeepWaterCommands_00008435_0024PostfixBurstDelegate(in NativeArray<Vector3> allPositions, in NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<bool> needsDeepWaterChecks, out NativeList<RaycastCommand> deepWaterCasts, out NativeList<int> raycastPointIndices, Allocator allocator);

	internal static class ConstructDeepWaterCommands_00008435_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ConstructDeepWaterCommands_00008435_0024PostfixBurstDelegate>((ConstructDeepWaterCommands_00008435_0024PostfixBurstDelegate)ConstructDeepWaterCommands).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<Vector3> allPositions, in NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<bool> needsDeepWaterChecks, out NativeList<RaycastCommand> deepWaterCasts, out NativeList<int> raycastPointIndices, Allocator allocator)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector3>, ref NativeArray<WaterLevel.WaterInfo>, ref NativeArray<bool>, ref NativeList<RaycastCommand>, ref NativeList<int>, Allocator, void>)functionPointer)(ref allPositions, ref pointWaterInfoNativeArray, ref needsDeepWaterChecks, ref deepWaterCasts, ref raycastPointIndices, allocator);
					return;
				}
			}
			ConstructDeepWaterCommands_0024BurstManaged(in allPositions, in pointWaterInfoNativeArray, in needsDeepWaterChecks, out deepWaterCasts, out raycastPointIndices, allocator);
		}
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(WaterLevelJobs_002EGetBuoyancyWaterInfoBatched_00008434_0024PostfixBurstDelegate))]
	public static void GetBuoyancyWaterInfoBatched(in NativeArray<Vector3> allPositions, in NativeArray<Vector2> allUVPositions, in NativeArray<float> pointTerrainHeightNativeArray, in NativeArray<float> pointWaterHeightNativeArray, in NativeArray<bool> doDeepWaterChecksStateNativeArray, ref NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<int> instancePointCountNativeArray, in int instanceCount, in TerrainTopologyMap.TopologyQueryStructure topologyMap, in NativeArray<bool> waterIgnoreStates, ref NativeArray<bool> needsDeepWaterChecks, bool isDeepSea, out bool hasAnyDeepWaterChecks)
	{
		GetBuoyancyWaterInfoBatched_00008434_0024BurstDirectCall.Invoke(in allPositions, in allUVPositions, in pointTerrainHeightNativeArray, in pointWaterHeightNativeArray, in doDeepWaterChecksStateNativeArray, ref pointWaterInfoNativeArray, in instancePointCountNativeArray, in instanceCount, in topologyMap, in waterIgnoreStates, ref needsDeepWaterChecks, isDeepSea, out hasAnyDeepWaterChecks);
	}

	[MonoPInvokeCallback(typeof(WaterLevelJobs_002EConstructDeepWaterCommands_00008435_0024PostfixBurstDelegate))]
	[BurstCompile]
	public static void ConstructDeepWaterCommands(in NativeArray<Vector3> allPositions, in NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<bool> needsDeepWaterChecks, out NativeList<RaycastCommand> deepWaterCasts, out NativeList<int> raycastPointIndices, Allocator allocator)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		ConstructDeepWaterCommands_00008435_0024BurstDirectCall.Invoke(in allPositions, in pointWaterInfoNativeArray, in needsDeepWaterChecks, out deepWaterCasts, out raycastPointIndices, allocator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void GetBuoyancyWaterInfoBatched_0024BurstManaged(in NativeArray<Vector3> allPositions, in NativeArray<Vector2> allUVPositions, in NativeArray<float> pointTerrainHeightNativeArray, in NativeArray<float> pointWaterHeightNativeArray, in NativeArray<bool> doDeepWaterChecksStateNativeArray, ref NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<int> instancePointCountNativeArray, in int instanceCount, in TerrainTopologyMap.TopologyQueryStructure topologyMap, in NativeArray<bool> waterIgnoreStates, ref NativeArray<bool> needsDeepWaterChecks, bool isDeepSea, out bool hasAnyDeepWaterChecks)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		hasAnyDeepWaterChecks = false;
		int num = 0;
		for (int i = 0; i < instanceCount; i++)
		{
			bool flag = doDeepWaterChecksStateNativeArray[i];
			float num2 = pointTerrainHeightNativeArray[i];
			int num3 = instancePointCountNativeArray[i];
			int num4 = num + num3;
			for (int j = num; j < num4; j++)
			{
				Vector3 position = allPositions[j];
				Vector2 uv = allUVPositions[j];
				float num5 = pointWaterHeightNativeArray[j];
				WaterLevel.WaterInfo waterInfo = default(WaterLevel.WaterInfo);
				if (position.y > num5 && WaterVolumeBurst.TestBurst(in position, out var info))
				{
					pointWaterInfoNativeArray[j] = info;
					continue;
				}
				bool flag2 = position.y < num2 - 1f;
				if (flag2 && WaterVolumeBurst.TestBurst(in position, out var info2))
				{
					pointWaterInfoNativeArray[j] = info2;
					continue;
				}
				bool flag3 = flag && (position.y < num5 - 10f || (TerrainMeta.OutOfBoundsBurst(position) && !isDeepSea));
				int topologyFast = topologyMap.GetTopologyFast(uv);
				if (((flag2 | flag3) || (topologyFast & 0x3C180) == 0) && waterIgnoreStates[j])
				{
					pointWaterInfoNativeArray[j] = waterInfo;
					continue;
				}
				if (flag3)
				{
					needsDeepWaterChecks[j] = true;
					hasAnyDeepWaterChecks = true;
				}
				waterInfo.isValid = true;
				waterInfo.currentDepth = Mathf.Max(0f, num5 - position.y);
				waterInfo.overallDepth = Mathf.Max(0f, num5 - num2);
				waterInfo.surfaceLevel = num5;
				waterInfo.terrainHeight = num2;
				waterInfo.topology = topologyFast;
				pointWaterInfoNativeArray[j] = waterInfo;
			}
			num += num3;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void ConstructDeepWaterCommands_0024BurstManaged(in NativeArray<Vector3> allPositions, in NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<bool> needsDeepWaterChecks, out NativeList<RaycastCommand> deepWaterCasts, out NativeList<int> raycastPointIndices, Allocator allocator)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		deepWaterCasts = new NativeList<RaycastCommand>(32, AllocatorHandle.op_Implicit(allocator));
		raycastPointIndices = new NativeList<int>(32, AllocatorHandle.op_Implicit((Allocator)2));
		QueryParameters val = new QueryParameters
		{
			hitTriggers = (QueryTriggerInteraction)2,
			layerMask = 16
		};
		RaycastCommand val3 = default(RaycastCommand);
		for (int i = 0; i < needsDeepWaterChecks.Length; i++)
		{
			if (needsDeepWaterChecks[i])
			{
				Vector3 val2 = allPositions[i];
				((RaycastCommand)(ref val3))._002Ector(val2, Vector3.up, val, float.MaxValue);
				deepWaterCasts.Add(ref val3);
				raycastPointIndices.Add(ref i);
			}
		}
	}
}
