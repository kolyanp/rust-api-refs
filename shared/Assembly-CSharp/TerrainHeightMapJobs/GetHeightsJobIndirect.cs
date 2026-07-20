using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainHeightMapJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GetHeightsJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<float> Heights;

	[ReadOnly]
	public ReadOnly<Vector3> Pos;

	[ReadOnly]
	public ReadOnly<int> Indices;

	public HeightMapData HeightMapData;

	public void Execute()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		Vector3 min = ((Bounds)(ref HeightMapData.DeepSeaBounds)).min;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(1f / ((Bounds)(ref HeightMapData.DeepSeaBounds)).size.x, 1f / ((Bounds)(ref HeightMapData.DeepSeaBounds)).size.z);
		Enumerator<int> enumerator = Indices.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				bool flag = ((Bounds)(ref HeightMapData.DeepSeaBounds)).Contains(Pos[current]);
				Vector3 val2 = (flag ? min : HeightMapData.TerrainPos);
				Vector2 val3 = (flag ? val : HeightMapData.TerrainOneOverSize);
				float num = (Pos[current].x - val2.x) * val3.x;
				float num2 = (Pos[current].z - val2.z) * val3.y;
				float num3 = HeightMapData.GetHeight01(data: flag ? HeightMapData.DeepSeaData : HeightMapData.Data, uv: new Vector2(num, num2), res: HeightMapData.Res);
				Heights[current] = HeightMapData.TerrainPos.y + num3 * HeightMapData.TerrainScale;
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
