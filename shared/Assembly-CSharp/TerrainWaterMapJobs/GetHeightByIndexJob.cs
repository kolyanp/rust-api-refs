using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace TerrainWaterMapJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GetHeightByIndexJob : IJob
{
	[WriteOnly]
	public NativeArray<float> Heights;

	[ReadOnly]
	public NativeArray<Vector2i> Indices;

	[ReadOnly]
	public NativeArray<short> Data;

	[ReadOnly]
	public int Res;

	[ReadOnly]
	public float Offset;

	[ReadOnly]
	public float Scale;

	public void Execute()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i].y * Res + Indices[i].x;
			Heights[i] = Offset + BitUtility.Short2Float((int)Data[num]) * Scale;
		}
	}
}
