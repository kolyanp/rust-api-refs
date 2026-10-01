using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ServerOcclusionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct CalculatePathsBetweenGridsJob : IJobParallelForBatch
{
	public ReadOnly<(ServerOcclusion.SubGrid from, ServerOcclusion.SubGrid to)> Paths;

	public NativeArray<bool> PathsBlocked;

	public GridDefinition Grid;

	public int BlockedGridThreshold;

	public int NeighbourThreshold;

	public bool UseNeighbourThresholds;

	public void Execute(int startIndex, int count)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		for (int i = startIndex; i < startIndex + count; i++)
		{
			(ServerOcclusion.SubGrid, ServerOcclusion.SubGrid) tuple = Paths[i];
			ServerOcclusion.SubGrid item = tuple.Item1;
			ServerOcclusion.SubGrid item2 = tuple.Item2;
			int3 val = new int3(item.x, item.y, item.z);
			int3 to = new int3(item2.x, item2.y, item2.z);
			PathsBlocked[i] = Algorithm.Trace(val, to, in Grid, BlockedGridThreshold, NeighbourThreshold, UseNeighbourThresholds);
		}
	}
}
