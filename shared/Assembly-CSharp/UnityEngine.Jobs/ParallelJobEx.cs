using Unity.Jobs;

namespace UnityEngine.Jobs;

public static class ParallelJobEx
{
	public static JobHandle ScheduleParallel<T>(this ref T jobData, int arrayLength, JobHandle dependsOn) where T : struct, IJobParallelFor
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return IJobParallelForExtensions.Schedule<T>(jobData, arrayLength, JobEx.GetBatchSize(arrayLength), dependsOn);
	}

	public static JobHandle ScheduleParallelByRef<T>(this ref T jobData, int arrayLength, JobHandle dependsOn) where T : struct, IJobParallelFor
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return IJobParallelForExtensions.ScheduleByRef<T>(ref jobData, arrayLength, JobEx.GetBatchSize(arrayLength), dependsOn);
	}
}
