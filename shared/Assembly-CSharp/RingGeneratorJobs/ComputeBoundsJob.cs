using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RingGeneratorJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct ComputeBoundsJob : IJob
{
	[ReadOnly]
	public NativeArray<float4> Segments;

	[WriteOnly]
	public NativeArray<float2> Bounds;

	public void Execute()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		float2 val = default(float2);
		((float2)(ref val))._002Ector(float.MaxValue, float.MaxValue);
		float2 val2 = default(float2);
		((float2)(ref val2))._002Ector(float.MinValue, float.MinValue);
		for (int i = 0; i < Segments.Length; i++)
		{
			float4 val3 = Segments[i];
			val = math.min(val, math.min(((float4)(ref val3)).xy, ((float4)(ref val3)).zw));
			val2 = math.max(val2, math.max(((float4)(ref val3)).xy, ((float4)(ref val3)).zw));
		}
		Bounds[0] = val;
		Bounds[1] = val2;
	}
}
