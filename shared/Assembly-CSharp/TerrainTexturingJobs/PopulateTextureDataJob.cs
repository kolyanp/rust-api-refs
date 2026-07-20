using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace TerrainTexturingJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct PopulateTextureDataJob : IJobParallelFor
{
	[WriteOnly]
	public NativeArray<half4> colors;

	public ReadOnly<Vector4> vectors;

	public ReadOnly<float> distances;

	public void Execute(int index)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		ref Vector4 reference = BurstUtil.GetReadonly<Vector4>(ref vectors, index);
		colors[index] = new half4(math.half(reference.x), math.half(reference.y), math.half(distances[index]), math.half(reference.w));
	}
}
