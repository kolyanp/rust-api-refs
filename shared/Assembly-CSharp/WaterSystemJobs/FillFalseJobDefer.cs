using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterSystemJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct FillFalseJobDefer : IJobParallelForDefer
{
	[ReadOnly]
	public NativeList<Ray> rays;

	public NativeArray<bool> HitResults;

	public void Execute(int index)
	{
		HitResults[index] = false;
	}
}
