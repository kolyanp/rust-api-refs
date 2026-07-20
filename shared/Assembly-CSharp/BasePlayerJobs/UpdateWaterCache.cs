using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace BasePlayerJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct UpdateWaterCache : IJob
{
	[WriteOnly]
	public NativeArray<BasePlayer.CachedState> States;

	[ReadOnly]
	public ReadOnly<float> Factors;

	[ReadOnly]
	public ReadOnly<WaterLevel.WaterInfo> Infos;

	[ReadOnly]
	public ReadOnly<int> Indices;

	public unsafe void Execute()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			ref BasePlayer.CachedState reference = ref UnsafeUtility.ArrayElementAsRef<BasePlayer.CachedState>(NativeArrayUnsafeUtility.GetUnsafePtr<BasePlayer.CachedState>(States), num);
			reference.WaterFactor = Factors[num];
			reference.WaterInfo = Infos[num];
			reference.IsSwimming = BasePlayer.IsSwimming(reference.WaterFactor);
		}
	}
}
