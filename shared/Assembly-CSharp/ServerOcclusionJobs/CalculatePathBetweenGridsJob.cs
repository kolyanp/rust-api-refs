using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ServerOcclusionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct CalculatePathBetweenGridsJob : IJob
{
	public ServerOcclusion.SubGrid From;

	public ServerOcclusion.SubGrid To;

	public NativeReference<bool> PathBlocked;

	public GridDefinition Grid;

	public int BlockedGridThreshold;

	public int NeighbourThreshold;

	public bool UseNeighbourThresholds;

	public void Execute()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		int3 val = default(int3);
		((int3)(ref val))._002Ector(From.x, From.y, From.z);
		int3 to = default(int3);
		((int3)(ref to))._002Ector(To.x, To.y, To.z);
		PathBlocked.Value = Algorithm.Trace(val, to, in Grid, BlockedGridThreshold, NeighbourThreshold, UseNeighbourThresholds);
	}
}
