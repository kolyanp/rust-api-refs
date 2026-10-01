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
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		int3 val = new int3(From.x, From.y, From.z);
		int3 to = new int3(To.x, To.y, To.z);
		PathBlocked.Value = Algorithm.Trace(val, to, in Grid, BlockedGridThreshold, NeighbourThreshold, UseNeighbourThresholds);
	}
}
