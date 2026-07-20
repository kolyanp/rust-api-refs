using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public static class TerrainWaterFlowMapBurst
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GetFlowDirections_000067DB_0024PostfixBurstDelegate(in NativeArray<Vector3> positions3D, ref NativeArray<float3> results, in NativeArray<byte> source, in int res);

	internal static class GetFlowDirections_000067DB_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GetFlowDirections_000067DB_0024PostfixBurstDelegate>((GetFlowDirections_000067DB_0024PostfixBurstDelegate)GetFlowDirections).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<Vector3> positions3D, ref NativeArray<float3> results, in NativeArray<byte> source, in int res)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector3>, ref NativeArray<float3>, ref NativeArray<byte>, ref int, void>)functionPointer)(ref positions3D, ref results, ref source, ref res);
					return;
				}
			}
			GetFlowDirections_0024BurstManaged(in positions3D, ref results, in source, in res);
		}
	}

	[MonoPInvokeCallback(typeof(GetFlowDirections_000067DB_0024PostfixBurstDelegate))]
	[BurstCompile]
	public static void GetFlowDirections(in NativeArray<Vector3> positions3D, ref NativeArray<float3> results, in NativeArray<byte> source, in int res)
	{
		GetFlowDirections_000067DB_0024BurstDirectCall.Invoke(in positions3D, ref results, in source, in res);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void GetFlowDirections_0024BurstManaged(in NativeArray<Vector3> positions3D, ref NativeArray<float3> results, in NativeArray<byte> source, in int res)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		TerrainMeta.BurstData data = TerrainMeta.sharedBurstData.Data;
		Vector3 position = data.Position;
		Vector3 oneOverSize = data.OneOverSize;
		for (int i = 0; i < positions3D.Length; i++)
		{
			Vector3 val = positions3D[i];
			float num = (val.x - position.x) * oneOverSize.x;
			float num2 = (val.z - position.z) * oneOverSize.z;
			int num3 = (int)(num * (float)(res - 1));
			int num4 = (int)(num2 * (float)(res - 1));
			num3 = math.clamp(num3, 0, res - 1);
			num4 = math.clamp(num4, 0, res - 1);
			float num5 = TerrainWaterFlowMap.ByteToAngle(source[num4 * res + num3]);
			results[i] = new float3(math.sin(num5), 0f, math.cos(num5));
		}
	}
}
