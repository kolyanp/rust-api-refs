using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainWaterMapJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GetHeightByPosJob : IJob
{
	[WriteOnly]
	public NativeArray<float> Heights;

	[ReadOnly]
	public NativeArray<Vector3> Pos;

	[ReadOnly]
	public NativeArray<short> Data;

	[ReadOnly]
	public int Res;

	[ReadOnly]
	public float Offset;

	[ReadOnly]
	public float Scale;

	[ReadOnly]
	public Vector2 DataOrigin;

	[ReadOnly]
	public Vector2 DataScale;

	public void Execute()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int num = Res - 1;
		for (int i = 0; i < Pos.Length; i++)
		{
			float num2 = (Pos[i].x - DataOrigin.x) * DataScale.x;
			float num3 = (Pos[i].z - DataOrigin.y) * DataScale.y;
			float num4 = num2 * (float)num;
			float num5 = num3 * (float)num;
			int num6 = Mathf.Clamp((int)num4, 0, num);
			int num7 = Mathf.Clamp((int)num5, 0, num);
			int num8 = Mathf.Min(num6 + 1, num);
			int num9 = Mathf.Min(num7 + 1, num);
			float num10 = BitUtility.Short2Float((int)Data[num7 * Res + num6]);
			float num11 = BitUtility.Short2Float((int)Data[num7 * Res + num8]);
			float num12 = BitUtility.Short2Float((int)Data[num9 * Res + num6]);
			float num13 = BitUtility.Short2Float((int)Data[num9 * Res + num8]);
			float num14 = Mathf.Lerp(num10, num11, num4 - (float)num6);
			float num15 = Mathf.Lerp(num12, num13, num4 - (float)num6);
			Heights[i] = Offset + Mathf.Lerp(num14, num15, num5 - (float)num7) * Scale;
		}
	}
}
