using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainWaterMapJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GetHeightByUVJob : IJob
{
	[WriteOnly]
	public NativeArray<float> Heights;

	[ReadOnly]
	public NativeArray<Vector2> UV;

	[ReadOnly]
	public NativeArray<short> Data;

	[ReadOnly]
	public int Res;

	[ReadOnly]
	public float Offset;

	[ReadOnly]
	public float Scale;

	public void Execute()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		int num = Res - 1;
		for (int i = 0; i < UV.Length; i++)
		{
			float num2 = UV[i].x * (float)num;
			float num3 = UV[i].y * (float)num;
			int num4 = Mathf.Clamp((int)num2, 0, num);
			int num5 = Mathf.Clamp((int)num3, 0, num);
			int num6 = Mathf.Min(num4 + 1, num);
			int num7 = Mathf.Min(num5 + 1, num);
			float num8 = BitUtility.Short2Float((int)Data[num5 * Res + num4]);
			float num9 = BitUtility.Short2Float((int)Data[num5 * Res + num6]);
			float num10 = BitUtility.Short2Float((int)Data[num7 * Res + num4]);
			float num11 = BitUtility.Short2Float((int)Data[num7 * Res + num6]);
			float num12 = Mathf.Lerp(num8, num9, num2 - (float)num4);
			float num13 = Mathf.Lerp(num10, num11, num2 - (float)num4);
			Heights[i] = Offset + Mathf.Lerp(num12, num13, num3 - (float)num5) * Scale;
		}
	}
}
