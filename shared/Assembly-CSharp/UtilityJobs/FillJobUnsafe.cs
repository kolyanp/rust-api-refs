using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct FillJobUnsafe<T> : IJob where T : unmanaged
{
	[WriteOnly]
	public NativeArray<T> Values;

	[NativeDisableUnsafePtrRestriction]
	[ReadOnly]
	public T Value;

	public void Execute()
	{
		for (int i = 0; i < Values.Length; i++)
		{
			Values[i] = Value;
		}
	}
}
