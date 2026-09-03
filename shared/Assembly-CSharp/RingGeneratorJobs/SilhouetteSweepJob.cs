using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RingGeneratorJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct SilhouetteSweepJob : IJobParallelFor
{
	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<float4> Segments;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float2> Bounds;

	[ReadOnly]
	public NativeArray<float2> Directions;

	[WriteOnly]
	public NativeArray<float> BestT;

	public void Execute(int index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		float2 val = (Bounds[0] + Bounds[1]) * 0.5f;
		float2 val2 = Directions[index];
		float num = -1f;
		for (int i = 0; i < Segments.Length; i++)
		{
			float4 val3 = Segments[i];
			float2 xy = ((float4)(ref val3)).xy;
			float2 val4 = ((float4)(ref val3)).zw - xy;
			float num2 = val2.x * val4.y - val2.y * val4.x;
			if (!(math.abs(num2) < 1E-09f))
			{
				float2 val5 = xy - val;
				float num3 = (val5.x * val4.y - val5.y * val4.x) / num2;
				float num4 = (val5.x * val2.y - val5.y * val2.x) / num2;
				if (num3 > 0f && num4 >= 0f && num4 <= 1f && num3 > num)
				{
					num = num3;
				}
			}
		}
		BestT[index] = num;
	}
}
