using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace ServerOcclusionJobs;

public struct GridDefinition
{
	[NativeDisableContainerSafetyRestriction]
	public ReadOnly<NativeBitArray> OcclusionSubGridBlocked;

	public int3 ChunkCount;

	public int3 SubChunkCount;

	public bool IsValidGrid(int3 p)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (p.x < 0 || p.y < 0 || p.z < 0)
		{
			return false;
		}
		if (p.x >= ChunkCount.x || p.y >= ChunkCount.y || p.z >= ChunkCount.z)
		{
			return false;
		}
		return true;
	}

	public bool IsValidSubGrid(int3 p)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (p.x < 0 || p.y < 0 || p.z < 0)
		{
			return false;
		}
		if (p.x >= SubChunkCount.x || p.y >= SubChunkCount.y || p.z >= SubChunkCount.z)
		{
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetGridIndex(int x, int y, int z)
	{
		return z * ChunkCount.x * ChunkCount.y + y * ChunkCount.z + x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsBlocked(int3 p)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return IsBlocked(p.x, p.y, p.z);
	}

	private bool IsBlocked(int x, int y, int z)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		int num = Math.DivRem(x, 8, out var result);
		int num2 = Math.DivRem(y, 8, out var result2);
		int num3 = Math.DivRem(z, 8, out var result3);
		int gridIndex = GetGridIndex(num, num2, num3);
		NativeBitArray val = (NativeBitArray)(IsValidGrid(math.int3(num, num2, num3)) ? OcclusionSubGridBlocked[gridIndex] : default(NativeBitArray));
		int num4 = result3 * 8 * 8 + result2 * 8 + result;
		if (((NativeBitArray)(ref val)).IsCreated)
		{
			return ((NativeBitArray)(ref val)).IsSet(num4);
		}
		return false;
	}
}
