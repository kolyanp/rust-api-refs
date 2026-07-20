using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace TerrainTopologyMapJobs;

[BurstCompile]
public struct GetTopologyByIndexJob : IJob
{
	[WriteOnly]
	public NativeArray<int> Topologies;

	[ReadOnly]
	public NativeArray<Vector2i> Indices;

	[ReadOnly]
	public NativeArray<int> Data;

	[ReadOnly]
	public int Res;

	public void Execute()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i].y * Res + Indices[i].x;
			Topologies[i] = Data[num];
		}
	}
}
