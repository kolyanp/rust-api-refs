using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainHeightMapJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GetHeightsByUVJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<float> Heights;

	[ReadOnly]
	public ReadOnly<Vector2> UVs;

	[ReadOnly]
	public ReadOnly<int> Indices;

	public HeightMapData HeightMapData;

	public ReadOnly<short> Data;

	public void Execute()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		int res = HeightMapData.Res;
		float y = HeightMapData.TerrainPos.y;
		float terrainScale = HeightMapData.TerrainScale;
		Enumerator<int> enumerator = Indices.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				float height = HeightMapData.GetHeight01(UVs[current], Data, res);
				Heights[current] = y + height * terrainScale;
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
