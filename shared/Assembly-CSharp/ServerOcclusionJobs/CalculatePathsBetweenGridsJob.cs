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
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		int3 val = default(int3);
		int3 to = default(int3);
		for (int i = startIndex; i < startIndex + count; i++)
		{
			var (subGrid, subGrid2) = Paths[i];
			((int3)(ref val))._002Ector(subGrid.x, subGrid.y, subGrid.z);
			((int3)(ref to))._002Ector(subGrid2.x, subGrid2.y, subGrid2.z);
			PathsBlocked[i] = Algorithm.Trace(val, to, in Grid, BlockedGridThreshold, NeighbourThreshold, UseNeighbourThresholds);
		}
	}
}
