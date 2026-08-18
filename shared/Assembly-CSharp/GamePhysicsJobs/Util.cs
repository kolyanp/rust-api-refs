using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
internal static class Util
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int FindFreeSlot_00008893_0024PostfixBurstDelegate(int rayInd, in NativeArray<RaycastHit> hits, int maxHitsPerRay, out int endInd);

	internal static class FindFreeSlot_00008893_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<FindFreeSlot_00008893_0024PostfixBurstDelegate>((FindFreeSlot_00008893_0024PostfixBurstDelegate)FindFreeSlot).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static int Invoke(int rayInd, in NativeArray<RaycastHit> hits, int maxHitsPerRay, out int endInd)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<int, ref NativeArray<RaycastHit>, int, ref int, int>)functionPointer)(rayInd, ref hits, maxHitsPerRay, ref endInd);
				}
			}
			return FindFreeSlot_0024BurstManaged(rayInd, in hits, maxHitsPerRay, out endInd);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int FindFreeSlot_00008894_0024PostfixBurstDelegate(int rayInd, in ReadOnly<RaycastHit> hits, int maxHitsPerRay, out int endInd);

	internal static class FindFreeSlot_00008894_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<FindFreeSlot_00008894_0024PostfixBurstDelegate>((FindFreeSlot_00008894_0024PostfixBurstDelegate)GamePhysicsJobs.Util.FindFreeSlot).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static int Invoke(int rayInd, in ReadOnly<RaycastHit> hits, int maxHitsPerRay, out int endInd)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<int, ref ReadOnly<RaycastHit>, int, ref int, int>)functionPointer)(rayInd, ref hits, maxHitsPerRay, ref endInd);
				}
			}
			return FindFreeSlot_0024BurstManaged(rayInd, in hits, maxHitsPerRay, out endInd);
		}
	}

	[MonoPInvokeCallback(typeof(GamePhysicsJobs_002EFindFreeSlot_00008893_0024PostfixBurstDelegate))]
	[BurstCompile]
	public static int FindFreeSlot(int rayInd, in NativeArray<RaycastHit> hits, int maxHitsPerRay, out int endInd)
	{
		return FindFreeSlot_00008893_0024BurstDirectCall.Invoke(rayInd, in hits, maxHitsPerRay, out endInd);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(GamePhysicsJobs_002EFindFreeSlot_00008894_0024PostfixBurstDelegate))]
	public static int FindFreeSlot(int rayInd, in ReadOnly<RaycastHit> hits, int maxHitsPerRay, out int endInd)
	{
		return FindFreeSlot_00008894_0024BurstDirectCall.Invoke(rayInd, in hits, maxHitsPerRay, out endInd);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static int FindFreeSlot_0024BurstManaged(int rayInd, in NativeArray<RaycastHit> hits, int maxHitsPerRay, out int endInd)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		int i = rayInd * maxHitsPerRay;
		for (endInd = i + maxHitsPerRay; i < endInd; i++)
		{
			RaycastHit val = hits[i];
			if (((RaycastHit)(ref val)).colliderInstanceID == 0)
			{
				break;
			}
		}
		return i;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static int FindFreeSlot_0024BurstManaged(int rayInd, in ReadOnly<RaycastHit> hits, int maxHitsPerRay, out int endInd)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		int i = rayInd * maxHitsPerRay;
		for (endInd = i + maxHitsPerRay; i < endInd; i++)
		{
			RaycastHit val = hits[i];
			if (((RaycastHit)(ref val)).colliderInstanceID == 0)
			{
				break;
			}
		}
		return i;
	}
}
