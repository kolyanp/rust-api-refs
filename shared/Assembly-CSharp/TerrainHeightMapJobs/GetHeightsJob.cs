using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainHeightMapJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GetHeightsJob : IJobParallelFor
{
	[WriteOnly]
	public NativeArray<float> Heights;

	public ReadOnly<Vector3> Pos;

	public HeightMapData HeightMapData;

	public void Execute(int index)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		Vector3 min = ((Bounds)(ref HeightMapData.DeepSeaBounds)).min;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(1f / ((Bounds)(ref HeightMapData.DeepSeaBounds)).size.x, 1f / ((Bounds)(ref HeightMapData.DeepSeaBounds)).size.z);
		bool flag = ((Bounds)(ref HeightMapData.DeepSeaBounds)).Contains(Pos[index]);
		Vector3 val2 = (flag ? min : HeightMapData.TerrainPos);
		Vector2 val3 = (flag ? val : HeightMapData.TerrainOneOverSize);
		float num = (Pos[index].x - val2.x) * val3.x;
		float num2 = (Pos[index].z - val2.z) * val3.y;
		float num3 = HeightMapData.GetHeight01(data: flag ? HeightMapData.DeepSeaData : HeightMapData.Data, uv: new Vector2(num, num2), res: HeightMapData.Res);
		Heights[index] = HeightMapData.TerrainPos.y + num3 * HeightMapData.TerrainScale;
	}
}
