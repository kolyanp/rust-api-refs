using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainWaterMapJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GetHeightByUVJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<float> Heights;

	[ReadOnly]
	public ReadOnly<Vector2> UV;

	[ReadOnly]
	public ReadOnly<int> Indices;

	[ReadOnly]
	public ReadOnly<short> Data;

	[ReadOnly]
	public int Res;

	[ReadOnly]
	public float Offset;

	[ReadOnly]
	public float Scale;

	public void Execute()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		int num = Res - 1;
		for (int i = 0; i < Indices.Length; i++)
		{
			int num2 = Indices[i];
			float num3 = UV[num2].x * (float)num;
			float num4 = UV[num2].y * (float)num;
			int num5 = Mathf.Clamp((int)num3, 0, num);
			int num6 = Mathf.Clamp((int)num4, 0, num);
			int num7 = Mathf.Min(num5 + 1, num);
			int num8 = Mathf.Min(num6 + 1, num);
			float num9 = BitUtility.Short2Float((int)Data[num6 * Res + num5]);
			float num10 = BitUtility.Short2Float((int)Data[num6 * Res + num7]);
			float num11 = BitUtility.Short2Float((int)Data[num8 * Res + num5]);
			float num12 = BitUtility.Short2Float((int)Data[num8 * Res + num7]);
			float num13 = Mathf.Lerp(num9, num10, num3 - (float)num5);
			float num14 = Mathf.Lerp(num11, num12, num3 - (float)num5);
			Heights[num2] = Offset + Mathf.Lerp(num13, num14, num4 - (float)num6) * Scale;
		}
	}
}
