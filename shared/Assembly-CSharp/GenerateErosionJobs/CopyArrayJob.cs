using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct CopyArrayJob<T> : IJob where T : unmanaged
{
	[WriteOnly]
	public NativeArray<T> CopyTarget;

	[ReadOnly]
	public NativeArray<T> CopySource;

	public void Execute()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		CopyTarget.CopyFrom(CopySource);
	}
}
