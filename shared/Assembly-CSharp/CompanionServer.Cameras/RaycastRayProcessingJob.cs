using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CompanionServer.Cameras;

[BurstCompile]
public struct RaycastRayProcessingJob : IJobParallelFor
{
	public const byte WaterMaterialIndex = 2;

	public float3 cameraForward;

	public float farPlane;

	public bool oceanEnabled;

	public float oceanLevel;

	public int oceanTopologyMask;

	public int topologyRes;

	public float2 topologyOrigin;

	public float2 topologyOneOverSize;

	[ReadOnly]
	public ReadOnly<int> topology;

	[ReadOnly]
	public NativeArray<RaycastCommand> raycastCommands;

	[ReadOnly]
	public NativeArray<RaycastHit> raycastHits;

	[ReadOnly]
	public NativeArray<int> colliderIds;

	[ReadOnly]
	public NativeArray<byte> colliderMaterials;

	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<int> colliderHits;

	[NativeMatchesParallelForLength]
	[WriteOnly]
	public NativeArray<int> outputs;

	[NativeDisableParallelForRestriction]
	public NativeArray<int> foundCollidersIndex;

	[NativeDisableParallelForRestriction]
	public NativeArray<int> foundColliders;

	public void Execute(int index)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		ref RaycastHit reference = BurstUtil.GetReadonly<RaycastHit>(ref raycastHits, index);
		int colliderId = reference.GetColliderId();
		bool flag = colliderId != 0;
		byte b = 0;
		if (flag)
		{
			int num = Interlocked.Increment(ref BurstUtil.Get<int>(ref foundCollidersIndex, 0));
			if (num <= foundColliders.Length)
			{
				foundColliders[num - 1] = colliderId;
			}
			int num2 = BinarySearch(colliderIds, colliderId);
			if (num2 >= 0)
			{
				b = colliderMaterials[num2];
				Interlocked.Increment(ref BurstUtil.Get<int>(ref colliderHits, num2));
			}
		}
		float distance;
		RaycastHit val;
		if (!flag)
		{
			distance = farPlane;
		}
		else
		{
			val = reference;
			distance = ((RaycastHit)(ref val)).distance;
		}
		float num3 = distance;
		float3 val2;
		if (!flag)
		{
			val2 = float3.zero;
		}
		else
		{
			val = reference;
			val2 = float3.op_Implicit(((RaycastHit)(ref val)).normal);
		}
		float3 val3 = val2;
		if (oceanEnabled)
		{
			RaycastCommand val4 = raycastCommands[index];
			float3 val5 = float3.op_Implicit(((RaycastCommand)(ref val4)).from);
			float num4 = -1f;
			float3 val6 = float3.zero;
			if (flag)
			{
				val = reference;
				float3 val7 = float3.op_Implicit(((RaycastHit)(ref val)).point);
				if (val5.y > oceanLevel && val7.y < oceanLevel)
				{
					float num5 = (val5.y - oceanLevel) / (val5.y - val7.y);
					val = reference;
					num4 = ((RaycastHit)(ref val)).distance * num5;
					val6 = math.lerp(val5, val7, num5);
				}
			}
			else
			{
				val4 = raycastCommands[index];
				float3 val8 = float3.op_Implicit(((RaycastCommand)(ref val4)).direction);
				if (val5.y > oceanLevel && val8.y < 0f)
				{
					float num6 = (oceanLevel - val5.y) / val8.y;
					if (num6 < farPlane)
					{
						num4 = num6;
						val6 = val5 + val8 * num6;
					}
				}
			}
			if (num4 >= 0f)
			{
				int num7 = topologyRes - 1;
				int num8 = math.clamp((int)((val6.x - topologyOrigin.x) * topologyOneOverSize.x * (float)topologyRes), 0, num7);
				int num9 = math.clamp((int)((val6.z - topologyOrigin.y) * topologyOneOverSize.y * (float)topologyRes), 0, num7);
				if ((topology[num9 * topologyRes + num8] & oceanTopologyMask) != 0)
				{
					b = 2;
					num3 = num4;
					((float3)(ref val3))._002Ector(0f, 1f, 0f);
				}
			}
		}
		float num10 = math.clamp(num3 / farPlane, 0f, 1f);
		float num11 = math.max(math.dot(cameraForward, val3), 0f);
		ushort num12 = (ushort)(num10 * 1023f);
		byte b2 = (byte)(num11 * 63f);
		outputs[index] = (num12 >> 8 << 24) | ((num12 & 0xFF) << 16) | (b2 << 8) | b;
	}

	private static int BinarySearch(NativeArray<int> haystack, int needle)
	{
		int num = 0;
		int num2 = haystack.Length - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num / 2);
			int num4 = Compare(haystack[num3], needle);
			if (num4 == 0)
			{
				return num3;
			}
			if (num4 < 0)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		return ~num;
	}

	private static int Compare(int x, int y)
	{
		if (x < y)
		{
			return -1;
		}
		if (x > y)
		{
			return 1;
		}
		return 0;
	}
}
