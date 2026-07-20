using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace UnityEngine.Jobs;

public static class JobEx
{
	public static int GetBatchSize(int length)
	{
		return Mathf.Max(length / JobsUtility.JobWorkerCount, 64);
	}

	public static JobHandle ScheduleParallel<T>(this ref T jobData, int arrayLength, JobHandle dependsOn) where T : struct, IJobFor
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return IJobForExtensions.ScheduleParallel<T>(jobData, arrayLength, GetBatchSize(arrayLength), dependsOn);
	}

	public static JobHandle ScheduleParallelByRef<T>(this ref T jobData, int arrayLength, JobHandle dependsOn) where T : struct, IJobFor
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return IJobForExtensions.ScheduleParallelByRef<T>(ref jobData, arrayLength, GetBatchSize(arrayLength), dependsOn);
	}

	public static JobHandle ScheduleParallelReadOnly<T>(this ref T jobData, TransformAccessArray transforms, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobParallelForTransform
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return IJobParallelForTransformExtensions.ScheduleReadOnly<T>(jobData, transforms, GetBatchSize(((TransformAccessArray)(ref transforms)).length), dependsOn);
	}

	public static JobHandle ScheduleParallelReadOnlyByRef<T>(this ref T jobData, TransformAccessArray transforms, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobParallelForTransform
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		return IJobParallelForTransformExtensions.ScheduleReadOnlyByRef<T>(ref jobData, transforms, GetBatchSize(((TransformAccessArray)(ref transforms)).length), dependsOn);
	}
}
