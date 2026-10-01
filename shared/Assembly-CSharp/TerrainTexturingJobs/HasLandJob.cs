using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace TerrainTexturingJobs;

[BurstCompile]
internal struct HasLandJob : IJob
{
	public ReadOnly<byte> bitmap;

	[WriteOnly]
	public NativeReference<bool> hasLand;

	public void Execute()
	{
		hasLand.Value = false;
		for (int i = 0; i < bitmap.Length; i++)
		{
			if (bitmap[i] > 127)
			{
				hasLand.Value = true;
				break;
			}
		}
	}
}
