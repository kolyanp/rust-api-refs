using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct ToUVJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<Vector2> UV;

	[ReadOnly]
	public ReadOnly<Vector3> Pos;

	[ReadOnly]
	public ReadOnly<int> Indices;

	[ReadOnly]
	public Vector2 TerrainPos;

	[ReadOnly]
	public Vector2 TerrainOneOverSize;

	public void Execute()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val2 = default(Vector2);
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			Vector3 val = Pos[num];
			((Vector2)(ref val2))._002Ector(val.x, val.z);
			UV[num] = (val2 - TerrainPos) * TerrainOneOverSize;
		}
	}
}
