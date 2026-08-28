using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public static class BuoyancyForceAccumulationBurst
{
	public struct InstanceInput
	{
		public int pointStartIndex;

		public int pointCount;

		public float buoyancyScale;

		public float rigidBodyMass;

		public float wavesEffect;

		public bool scaleForceWithMass;

		public bool flowForceDisabled;

		public float flowMovementScale;

		public float3 worldCom;
	}

	public struct InstanceOutput
	{
		public float3 netForce;

		public float3 netTorque;

		public int numSubmerged;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void Compute_000057EB_0024PostfixBurstDelegate(in NativeArray<InstanceInput> instances, in NativeArray<float3> allPositions3D, in NativeArray<float> pointShoreDistance, in NativeArray<WaterLevel.WaterInfo> pointWaterInfo, in NativeArray<float> pointSize, in NativeArray<float> pointBuoyancyForce, in NativeArray<float> pointRandomOffset, in NativeArray<float> pointWaveFrequency, in NativeArray<float> pointWaveScale, in NativeArray<float3> pointFlowDirection, float time, ref NativeArray<InstanceOutput> results);

	internal static class Compute_000057EB_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<Compute_000057EB_0024PostfixBurstDelegate>((Compute_000057EB_0024PostfixBurstDelegate)Compute).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<InstanceInput> instances, in NativeArray<float3> allPositions3D, in NativeArray<float> pointShoreDistance, in NativeArray<WaterLevel.WaterInfo> pointWaterInfo, in NativeArray<float> pointSize, in NativeArray<float> pointBuoyancyForce, in NativeArray<float> pointRandomOffset, in NativeArray<float> pointWaveFrequency, in NativeArray<float> pointWaveScale, in NativeArray<float3> pointFlowDirection, float time, ref NativeArray<InstanceOutput> results)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<InstanceInput>, ref NativeArray<float3>, ref NativeArray<float>, ref NativeArray<WaterLevel.WaterInfo>, ref NativeArray<float>, ref NativeArray<float>, ref NativeArray<float>, ref NativeArray<float>, ref NativeArray<float>, ref NativeArray<float3>, float, ref NativeArray<InstanceOutput>, void>)functionPointer)(ref instances, ref allPositions3D, ref pointShoreDistance, ref pointWaterInfo, ref pointSize, ref pointBuoyancyForce, ref pointRandomOffset, ref pointWaveFrequency, ref pointWaveScale, ref pointFlowDirection, time, ref results);
					return;
				}
			}
			Compute_0024BurstManaged(in instances, in allPositions3D, in pointShoreDistance, in pointWaterInfo, in pointSize, in pointBuoyancyForce, in pointRandomOffset, in pointWaveFrequency, in pointWaveScale, in pointFlowDirection, time, ref results);
		}
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(Compute_000057EB_0024PostfixBurstDelegate))]
	public static void Compute(in NativeArray<InstanceInput> instances, in NativeArray<float3> allPositions3D, in NativeArray<float> pointShoreDistance, in NativeArray<WaterLevel.WaterInfo> pointWaterInfo, in NativeArray<float> pointSize, in NativeArray<float> pointBuoyancyForce, in NativeArray<float> pointRandomOffset, in NativeArray<float> pointWaveFrequency, in NativeArray<float> pointWaveScale, in NativeArray<float3> pointFlowDirection, float time, ref NativeArray<InstanceOutput> results)
	{
		Compute_000057EB_0024BurstDirectCall.Invoke(in instances, in allPositions3D, in pointShoreDistance, in pointWaterInfo, in pointSize, in pointBuoyancyForce, in pointRandomOffset, in pointWaveFrequency, in pointWaveScale, in pointFlowDirection, time, ref results);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void Compute_0024BurstManaged(in NativeArray<InstanceInput> instances, in NativeArray<float3> allPositions3D, in NativeArray<float> pointShoreDistance, in NativeArray<WaterLevel.WaterInfo> pointWaterInfo, in NativeArray<float> pointSize, in NativeArray<float> pointBuoyancyForce, in NativeArray<float> pointRandomOffset, in NativeArray<float> pointWaveFrequency, in NativeArray<float> pointWaveScale, in NativeArray<float3> pointFlowDirection, float time, ref NativeArray<InstanceOutput> results)
	{
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		float3 val4 = default(float3);
		for (int i = 0; i < instances.Length; i++)
		{
			InstanceInput instanceInput = instances[i];
			int pointStartIndex = instanceInput.pointStartIndex;
			int pointCount = instanceInput.pointCount;
			float wavesEffect = instanceInput.wavesEffect;
			bool flag = wavesEffect < 1f;
			float3 worldCom = instanceInput.worldCom;
			float3 val = float3.zero;
			float3 val2 = float3.zero;
			int num = 0;
			for (int j = 0; j < pointCount; j++)
			{
				int num2 = pointStartIndex + j;
				float3 val3 = allPositions3D[num2];
				WaterLevel.WaterInfo waterInfo = pointWaterInfo[num2];
				if (!waterInfo.isValid)
				{
					continue;
				}
				float surfaceLevel = waterInfo.surfaceLevel;
				float num3 = waterInfo.currentDepth;
				if (flag)
				{
					num3 = math.lerp(num3, surfaceLevel - val3.y, wavesEffect);
				}
				if (val3.y >= surfaceLevel)
				{
					continue;
				}
				num++;
				float num4 = pointSize[num2];
				float num5 = pointBuoyancyForce[num2];
				float num6 = pointRandomOffset[num2];
				float num7 = pointWaveFrequency[num2];
				float num8 = pointWaveScale[num2];
				float num9 = math.saturate(math.unlerp(0f, num4, num3));
				float num10 = 1f + Mathf.PerlinNoise(num6 + time * num7, 0f) * num8;
				float num11 = num5 * instanceInput.buoyancyScale;
				if (instanceInput.scaleForceWithMass)
				{
					num11 *= instanceInput.rigidBodyMass;
				}
				((float3)(ref val4))._002Ector(0f, num10 * num9 * num11, 0f);
				if (!waterInfo.artificalWater && !instanceInput.flowForceDisabled && (waterInfo.topology & 0x10000) == 0)
				{
					float num12 = math.abs(pointShoreDistance[num2]);
					float num13 = math.saturate(math.unlerp(60f, 0f, num12));
					if (num13 > 1E-06f)
					{
						num13 = math.pow(num13, 0.5f);
						float3 val5 = pointFlowDirection[num2];
						float2 xz = ((float3)(ref val5)).xz;
						float num14 = num11 * 0.025f * num13 * instanceInput.flowMovementScale;
						val4.x += xz.x * num14;
						val4.z += xz.y * num14;
					}
				}
				val += val4;
				val2 += math.cross(val3 - worldCom, val4);
			}
			results[i] = new InstanceOutput
			{
				netForce = val,
				netTorque = val2,
				numSubmerged = num
			};
		}
	}
}
