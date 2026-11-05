using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;

namespace TerrainTopologyMapJobs;

[BurstCompile]
public static class TerrainTopologyMapJobUtil
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int OrRadius_00008E13_0024PostfixBurstDelegate(in TerrainTopologyHierarchy.Query topology, int x_min, int x_mid, int x_max, int z_min, int z_mid, int z_max, float radius, int mask);

	internal static class OrRadius_00008E13_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<OrRadius_00008E13_0024PostfixBurstDelegate>((OrRadius_00008E13_0024PostfixBurstDelegate)OrRadius).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static int Invoke(in TerrainTopologyHierarchy.Query topology, int x_min, int x_mid, int x_max, int z_min, int z_mid, int z_max, float radius, int mask)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref TerrainTopologyHierarchy.Query, int, int, int, int, int, int, float, int, int>)functionPointer)(ref topology, x_min, x_mid, x_max, z_min, z_mid, z_max, radius, mask);
				}
			}
			return OrRadius_0024BurstManaged(in topology, x_min, x_mid, x_max, z_min, z_mid, z_max, radius, mask);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int AnyRadius_00008E14_0024PostfixBurstDelegate(in TerrainTopologyHierarchy.Query topology, int x_min, int x_mid, int x_max, int z_min, int z_mid, int z_max, float radius, int mask);

	internal static class AnyRadius_00008E14_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<AnyRadius_00008E14_0024PostfixBurstDelegate>((AnyRadius_00008E14_0024PostfixBurstDelegate)AnyRadius).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static int Invoke(in TerrainTopologyHierarchy.Query topology, int x_min, int x_mid, int x_max, int z_min, int z_mid, int z_max, float radius, int mask)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref TerrainTopologyHierarchy.Query, int, int, int, int, int, int, float, int, int>)functionPointer)(ref topology, x_min, x_mid, x_max, z_min, z_mid, z_max, radius, mask);
				}
			}
			return AnyRadius_0024BurstManaged(in topology, x_min, x_mid, x_max, z_min, z_mid, z_max, radius, mask);
		}
	}

	[MonoPInvokeCallback(typeof(TerrainTopologyMapJobs_002EOrRadius_00008E13_0024PostfixBurstDelegate))]
	[BurstCompile(/*Could not decode attribute arguments.*/)]
	public static int OrRadius(in TerrainTopologyHierarchy.Query topology, int x_min, int x_mid, int x_max, int z_min, int z_mid, int z_max, float radius, int mask)
	{
		return OrRadius_00008E13_0024BurstDirectCall.Invoke(in topology, x_min, x_mid, x_max, z_min, z_mid, z_max, radius, mask);
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	[MonoPInvokeCallback(typeof(TerrainTopologyMapJobs_002EAnyRadius_00008E14_0024PostfixBurstDelegate))]
	public static int AnyRadius(in TerrainTopologyHierarchy.Query topology, int x_min, int x_mid, int x_max, int z_min, int z_mid, int z_max, float radius, int mask)
	{
		return AnyRadius_00008E14_0024BurstDirectCall.Invoke(in topology, x_min, x_mid, x_max, z_min, z_mid, z_max, radius, mask);
	}

	public static int GetTopologyRadius(in TerrainTopologyHierarchy.Query topology, float terrainOneOverSizeX, float radius, float normX, float normZ, int mask = -1, bool stopOnAny = false)
	{
		float num = terrainOneOverSizeX * radius;
		int res = topology.res;
		int x_mid = Index(normX, res);
		int z_mid = Index(normZ, res);
		int x_min = Index(normX - num, res);
		int x_max = Index(normX + num, res);
		int z_min = Index(normZ - num, res);
		int z_max = Index(normZ + num, res);
		return topology.Or(x_min, x_mid, x_max, z_min, z_mid, z_max, radius * radius, mask, stopOnAny);
	}

	private static int Index(float normalized, int res)
	{
		int num = (int)(normalized * (float)res);
		if (num >= 0)
		{
			if (num <= res - 1)
			{
				return num;
			}
			return res - 1;
		}
		return 0;
	}

	internal static int GetTopologyRadius(ReadOnly<int> src, int res, float radius, int x_min, int x_mid, int x_max, int z_min, int z_mid, int z_max)
	{
		int num = 0;
		float num2 = radius * radius;
		for (int i = z_min; i <= z_max; i++)
		{
			int num3 = i - z_mid;
			int num4 = num3 * num3;
			for (int j = x_min; j <= x_max; j++)
			{
				int num5 = j - x_mid;
				if (!((float)(num5 * num5 + num4) > num2))
				{
					num |= src[i * res + j];
				}
			}
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(/*Could not decode attribute arguments.*/)]
	internal static int OrRadius_0024BurstManaged(in TerrainTopologyHierarchy.Query topology, int x_min, int x_mid, int x_max, int z_min, int z_mid, int z_max, float radius, int mask)
	{
		return topology.Or(x_min, x_mid, x_max, z_min, z_mid, z_max, radius * radius, mask, stopOnAny: false);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(/*Could not decode attribute arguments.*/)]
	internal static int AnyRadius_0024BurstManaged(in TerrainTopologyHierarchy.Query topology, int x_min, int x_mid, int x_max, int z_min, int z_mid, int z_max, float radius, int mask)
	{
		return topology.Or(x_min, x_mid, x_max, z_min, z_mid, z_max, radius * radius, mask, stopOnAny: true);
	}
}
