using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

[BurstCompile]
public class BuoyancyBurstUtility
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void FillPointData_000057E9_0024PostfixBurstDelegate(in int pointIndexOffset, ref NativeArray<Vector2> pointPositionArray, ref NativeArray<Vector2> pointPositionUVArray, in Matrix4x4 rootToWorld, ref NativeArray<Buoyancy.BuoyancyPointData> pointData, in Bounds deepSeaBounds, in Vector3 terrainPosition, in Vector3 terrainOneOverSize, in bool isDeepSea, ref NativeArray<Vector3> allPositions3D, out int pointCount);

	internal static class FillPointData_000057E9_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<FillPointData_000057E9_0024PostfixBurstDelegate>((FillPointData_000057E9_0024PostfixBurstDelegate)FillPointData).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in int pointIndexOffset, ref NativeArray<Vector2> pointPositionArray, ref NativeArray<Vector2> pointPositionUVArray, in Matrix4x4 rootToWorld, ref NativeArray<Buoyancy.BuoyancyPointData> pointData, in Bounds deepSeaBounds, in Vector3 terrainPosition, in Vector3 terrainOneOverSize, in bool isDeepSea, ref NativeArray<Vector3> allPositions3D, out int pointCount)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref int, ref NativeArray<Vector2>, ref NativeArray<Vector2>, ref Matrix4x4, ref NativeArray<Buoyancy.BuoyancyPointData>, ref Bounds, ref Vector3, ref Vector3, ref bool, ref NativeArray<Vector3>, ref int, void>)functionPointer)(ref pointIndexOffset, ref pointPositionArray, ref pointPositionUVArray, ref rootToWorld, ref pointData, ref deepSeaBounds, ref terrainPosition, ref terrainOneOverSize, ref isDeepSea, ref allPositions3D, ref pointCount);
					return;
				}
			}
			FillPointData_0024BurstManaged(in pointIndexOffset, ref pointPositionArray, ref pointPositionUVArray, in rootToWorld, ref pointData, in deepSeaBounds, in terrainPosition, in terrainOneOverSize, in isDeepSea, ref allPositions3D, out pointCount);
		}
	}

	[MonoPInvokeCallback(typeof(FillPointData_000057E9_0024PostfixBurstDelegate))]
	[BurstCompile]
	public static void FillPointData(in int pointIndexOffset, ref NativeArray<Vector2> pointPositionArray, ref NativeArray<Vector2> pointPositionUVArray, in Matrix4x4 rootToWorld, ref NativeArray<Buoyancy.BuoyancyPointData> pointData, in Bounds deepSeaBounds, in Vector3 terrainPosition, in Vector3 terrainOneOverSize, in bool isDeepSea, ref NativeArray<Vector3> allPositions3D, out int pointCount)
	{
		FillPointData_000057E9_0024BurstDirectCall.Invoke(in pointIndexOffset, ref pointPositionArray, ref pointPositionUVArray, in rootToWorld, ref pointData, in deepSeaBounds, in terrainPosition, in terrainOneOverSize, in isDeepSea, ref allPositions3D, out pointCount);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void FillPointData_0024BurstManaged(in int pointIndexOffset, ref NativeArray<Vector2> pointPositionArray, ref NativeArray<Vector2> pointPositionUVArray, in Matrix4x4 rootToWorld, ref NativeArray<Buoyancy.BuoyancyPointData> pointData, in Bounds deepSeaBounds, in Vector3 terrainPosition, in Vector3 terrainOneOverSize, in bool isDeepSea, ref NativeArray<Vector3> allPositions3D, out int pointCount)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		float x;
		float z;
		float x2;
		float z2;
		if (isDeepSea)
		{
			x = ((Bounds)(ref deepSeaBounds)).min.x;
			z = ((Bounds)(ref deepSeaBounds)).min.z;
			x2 = Vector3Ex.Inverse(((Bounds)(ref deepSeaBounds)).size).x;
			z2 = Vector3Ex.Inverse(((Bounds)(ref deepSeaBounds)).size).z;
		}
		else
		{
			x = terrainPosition.x;
			z = terrainPosition.z;
			x2 = terrainOneOverSize.x;
			z2 = terrainOneOverSize.z;
		}
		for (int i = 0; i < pointData.Length; i++)
		{
			Vector3 val = ((Matrix4x4)(ref rootToWorld)).MultiplyPoint3x4(pointData[i].rootToPoint);
			float num = (val.x - x) * x2;
			float num2 = (val.z - z) * z2;
			pointPositionArray[i + pointIndexOffset] = new Vector2(val.x, val.z);
			pointPositionUVArray[i + pointIndexOffset] = new Vector2(num, num2);
			allPositions3D[i + pointIndexOffset] = val;
		}
		pointCount = pointData.Length;
	}
}
