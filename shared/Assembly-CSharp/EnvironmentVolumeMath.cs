using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public static class EnvironmentVolumeMath
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CalculateTransformationBoundsBurst_00005F11_0024PostfixBurstDelegate(in float4x4 transformationMatrix, in bool capsule, out Bounds bounds);

	internal static class CalculateTransformationBoundsBurst_00005F11_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CalculateTransformationBoundsBurst_00005F11_0024PostfixBurstDelegate>((CalculateTransformationBoundsBurst_00005F11_0024PostfixBurstDelegate)CalculateTransformationBoundsBurst).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float4x4 transformationMatrix, in bool capsule, out Bounds bounds)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float4x4, ref bool, ref Bounds, void>)functionPointer)(ref transformationMatrix, ref capsule, ref bounds);
					return;
				}
			}
			CalculateTransformationBoundsBurst_0024BurstManaged(in transformationMatrix, in capsule, out bounds);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void MultiplyPoint3X4_00005F12_0024PostfixBurstDelegate(in float4x4 transformationMatrix, in float3 point, out float3 result);

	internal static class MultiplyPoint3X4_00005F12_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<MultiplyPoint3X4_00005F12_0024PostfixBurstDelegate>((MultiplyPoint3X4_00005F12_0024PostfixBurstDelegate)MultiplyPoint3X4).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float4x4 transformationMatrix, in float3 point, out float3 result)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float4x4, ref float3, ref float3, void>)functionPointer)(ref transformationMatrix, ref point, ref result);
					return;
				}
			}
			MultiplyPoint3X4_0024BurstManaged(in transformationMatrix, in point, out result);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void UpdateVolumeTransformationAndBoundsBurst_00005F13_0024PostfixBurstDelegate(in float3 size, in float3 center, in float4x4 localToWorldMatrix, in bool isCapsule, out float4x4 volumeTransformation, out float4x4 volumeTransformationInverse, out float3 volumePosition, out Bounds volumeBounds);

	internal static class UpdateVolumeTransformationAndBoundsBurst_00005F13_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<UpdateVolumeTransformationAndBoundsBurst_00005F13_0024PostfixBurstDelegate>((UpdateVolumeTransformationAndBoundsBurst_00005F13_0024PostfixBurstDelegate)UpdateVolumeTransformationAndBoundsBurst).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 size, in float3 center, in float4x4 localToWorldMatrix, in bool isCapsule, out float4x4 volumeTransformation, out float4x4 volumeTransformationInverse, out float3 volumePosition, out Bounds volumeBounds)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float4x4, ref bool, ref float4x4, ref float4x4, ref float3, ref Bounds, void>)functionPointer)(ref size, ref center, ref localToWorldMatrix, ref isCapsule, ref volumeTransformation, ref volumeTransformationInverse, ref volumePosition, ref volumeBounds);
					return;
				}
			}
			UpdateVolumeTransformationAndBoundsBurst_0024BurstManaged(in size, in center, in localToWorldMatrix, in isCapsule, out volumeTransformation, out volumeTransformationInverse, out volumePosition, out volumeBounds);
		}
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(CalculateTransformationBoundsBurst_00005F11_0024PostfixBurstDelegate))]
	private static void CalculateTransformationBoundsBurst(in float4x4 transformationMatrix, in bool capsule, out Bounds bounds)
	{
		CalculateTransformationBoundsBurst_00005F11_0024BurstDirectCall.Invoke(in transformationMatrix, in capsule, out bounds);
	}

	[MonoPInvokeCallback(typeof(MultiplyPoint3X4_00005F12_0024PostfixBurstDelegate))]
	[BurstCompile]
	private static void MultiplyPoint3X4(in float4x4 transformationMatrix, in float3 point, out float3 result)
	{
		MultiplyPoint3X4_00005F12_0024BurstDirectCall.Invoke(in transformationMatrix, in point, out result);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UpdateVolumeTransformationAndBoundsBurst_00005F13_0024PostfixBurstDelegate))]
	public static void UpdateVolumeTransformationAndBoundsBurst(in float3 size, in float3 center, in float4x4 localToWorldMatrix, in bool isCapsule, out float4x4 volumeTransformation, out float4x4 volumeTransformationInverse, out float3 volumePosition, out Bounds volumeBounds)
	{
		UpdateVolumeTransformationAndBoundsBurst_00005F13_0024BurstDirectCall.Invoke(in size, in center, in localToWorldMatrix, in isCapsule, out volumeTransformation, out volumeTransformationInverse, out volumePosition, out volumeBounds);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal unsafe static void CalculateTransformationBoundsBurst_0024BurstManaged(in float4x4 transformationMatrix, in bool capsule, out Bounds bounds)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		float3 val = default(float3);
		((float3)(ref val))._002Ector(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		float3 val2 = default(float3);
		((float3)(ref val2))._002Ector(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		ReadOnlySpan<float3> readOnlySpan = (Span<float3>)stackalloc float3[8]
		{
			new float3(-0.5f, -0.5f, -0.5f),
			new float3(0.5f, -0.5f, -0.5f),
			new float3(0.5f, 0.5f, -0.5f),
			new float3(-0.5f, 0.5f, -0.5f),
			new float3(-0.5f, -0.5f, 0.5f),
			new float3(0.5f, -0.5f, 0.5f),
			new float3(0.5f, 0.5f, 0.5f),
			new float3(-0.5f, 0.5f, 0.5f)
		};
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			MultiplyPoint3X4(in transformationMatrix, in readOnlySpan[i], out var result);
			val = math.min(val, result);
			val2 = math.max(val2, result);
		}
		if (capsule)
		{
			float num = math.abs(val2.y - val.y) * 0.5f;
			val.y -= num;
			val2.y += num;
		}
		bounds = new Bounds(Vector3.zero, Vector3.one);
		((Bounds)(ref bounds)).SetMinMax(float3.op_Implicit(val), float3.op_Implicit(val2));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void MultiplyPoint3X4_0024BurstManaged(in float4x4 transformationMatrix, in float3 point, out float3 result)
	{
		result.x = transformationMatrix.c0.x * point.x + transformationMatrix.c1.x * point.y + transformationMatrix.c2.x * point.z + transformationMatrix.c3.x;
		result.y = transformationMatrix.c0.y * point.x + transformationMatrix.c1.y * point.y + transformationMatrix.c2.y * point.z + transformationMatrix.c3.y;
		result.z = transformationMatrix.c0.z * point.x + transformationMatrix.c1.z * point.y + transformationMatrix.c2.z * point.z + transformationMatrix.c3.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void UpdateVolumeTransformationAndBoundsBurst_0024BurstManaged(in float3 size, in float3 center, in float4x4 localToWorldMatrix, in bool isCapsule, out float4x4 volumeTransformation, out float4x4 volumeTransformationInverse, out float3 volumePosition, out Bounds volumeBounds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		float3 val = size + new float3(0.001f, 0.001f, 0.001f);
		float4x4 val2 = math.mul(float4x4.Translate(center), float4x4.Scale(val));
		volumeTransformation = math.mul(localToWorldMatrix, val2);
		volumeTransformationInverse = math.inverse(volumeTransformation);
		volumePosition = Float4x4Ex.ToPosition(volumeTransformation);
		CalculateTransformationBoundsBurst(in volumeTransformation, in isCapsule, out volumeBounds);
	}
}
