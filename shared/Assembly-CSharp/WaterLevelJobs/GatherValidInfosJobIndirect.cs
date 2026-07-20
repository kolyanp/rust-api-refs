using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace WaterLevelJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GatherValidInfosJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<int> ValidIndices;

	[WriteOnly]
	public NativeReference<int> ValidIndexCount;

	[ReadOnly]
	public ReadOnly<WaterLevel.WaterInfo> Infos;

	[ReadOnly]
	public ReadOnly<int> Indices;

	public void Execute()
	{
		int value = 0;
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			if (Infos[num].isValid)
			{
				ValidIndices[value++] = num;
			}
		}
		ValidIndexCount.Value = value;
	}
}
