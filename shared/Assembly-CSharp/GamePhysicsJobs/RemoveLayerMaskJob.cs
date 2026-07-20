using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GamePhysicsJobs;

[BurstCompile]
public struct RemoveLayerMaskJob : IJob
{
	public NativeArray<int> LayerMasks;

	[ReadOnly]
	public ReadOnly<bool> ShouldIgnore;

	[ReadOnly]
	public int MaskToRemove;

	public void Execute()
	{
		for (int i = 0; i < ShouldIgnore.Length; i++)
		{
			if (ShouldIgnore[i])
			{
				ref NativeArray<int> layerMasks = ref LayerMasks;
				int num = i;
				layerMasks[num] &= ~MaskToRemove;
			}
		}
	}
}
