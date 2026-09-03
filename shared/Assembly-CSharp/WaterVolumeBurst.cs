using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public class WaterVolumeBurst
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool TestBurst_00006518_0024PostfixBurstDelegate(in Vector3 position, out WaterLevel.WaterInfo info, float queryRadius = 100f);

	internal static class TestBurst_00006518_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<TestBurst_00006518_0024PostfixBurstDelegate>((TestBurst_00006518_0024PostfixBurstDelegate)TestBurst).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static bool Invoke(in Vector3 position, out WaterLevel.WaterInfo info, float queryRadius = 100f)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref Vector3, ref WaterLevel.WaterInfo, float, bool>)functionPointer)(ref position, ref info, queryRadius);
				}
			}
			return TestBurst_0024BurstManaged(in position, out info, queryRadius);
		}
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(TestBurst_00006518_0024PostfixBurstDelegate))]
	public static bool TestBurst(in Vector3 position, out WaterLevel.WaterInfo info, float queryRadius = 100f)
	{
		return TestBurst_00006518_0024BurstDirectCall.Invoke(in position, out info, queryRadius);
	}

	private static bool CheckCutOffPlanesBurst(in WaterVolumeBurstData data, in Vector3 pos, out float bottomCutY)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		int length = data.cutOffPlanePoses.Length;
		bottomCutY = float.MaxValue;
		bool flag = true;
		for (int i = 0; i < length; i++)
		{
			float4 val = math.mul(math.inverse(float4x4.op_Implicit(data.cutOffPlaneMatrices[i])), new float4(float3.op_Implicit(pos), 1f));
			float3 xyz = ((float4)(ref val)).xyz;
			Vector3 position = data.cutOffPlanePoses[i].position;
			Pose val2 = data.cutOffPlanePoses[i];
			if (math.dot(float3.op_Implicit(((Pose)(ref val2)).up), float3.op_Implicit(data.bounds.up)) < -0.1f)
			{
				bottomCutY = math.min(bottomCutY, position.y);
			}
			if (xyz.y > 0f)
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static bool TestBurst_0024BurstManaged(in Vector3 position, out WaterLevel.WaterInfo info, float queryRadius = 100f)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		NativeList<WaterVolumeBurstData> val = WaterVolume.WaterVolumeBoundsGrid.Data.Query((Allocator)2, position.x, position.z, queryRadius);
		Plane val2 = default(Plane);
		for (int i = 0; i < val.Length; i++)
		{
			OBB bounds = val[i].bounds;
			if (((OBB)(ref bounds)).Contains(position) && CheckCutOffPlanesBurst(val[i], in position, out var bottomCutY))
			{
				((Plane)(ref val2))._002Ector(bounds.up, bounds.position);
				Vector3 val3 = ((Plane)(ref val2)).ClosestPointOnPlane(position);
				float y = (val3 + bounds.up * bounds.extents.y).y;
				float y2 = (val3 + -bounds.up * bounds.extents.y).y;
				y2 = math.max(y2, bottomCutY);
				info = default(WaterLevel.WaterInfo);
				info.isValid = true;
				info.artificalWater = !val[i].naturalSource;
				info.currentDepth = Mathf.Max(0f, y - position.y);
				info.overallDepth = Mathf.Max(0f, y - y2);
				info.surfaceLevel = y;
				return true;
			}
		}
		info = default(WaterLevel.WaterInfo);
		return false;
	}
}
