using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainHeightMapJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GetNormalsJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<Vector3> Normals;

	public ReadOnly<Vector3> Pos;

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
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		Vector3 min = ((Bounds)(ref HeightMapData.DeepSeaBounds)).min;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(1f / ((Bounds)(ref HeightMapData.DeepSeaBounds)).size.x, 1f / ((Bounds)(ref HeightMapData.DeepSeaBounds)).size.z);
		Enumerator<int> enumerator = Indices.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				bool num = ((Bounds)(ref HeightMapData.DeepSeaBounds)).Contains(Pos[current]);
				Vector3 val2 = (num ? min : HeightMapData.TerrainPos);
				Vector2 val3 = (num ? val : HeightMapData.TerrainOneOverSize);
				float num2 = (Pos[current].x - val2.x) * val3.x;
				float num3 = (Pos[current].z - val2.z) * val3.y;
				ReadOnly<short> data = (num ? HeightMapData.DeepSeaData : HeightMapData.Data);
				Normals[current] = HeightMapData.GetNormal(new Vector2(num2, num3), HeightMapData.NormY, data, HeightMapData.Res);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
