using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTopologyMapJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GetTopologyByUVJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<int> Topologies;

	[ReadOnly]
	public ReadOnly<Vector2> UV;

	[ReadOnly]
	public ReadOnly<int> Indices;

	[ReadOnly]
	public ReadOnly<int> Data;

	[ReadOnly]
	public int Res;

	public void Execute()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		int max = Res - 1;
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			int num2 = Math.Clamp((int)(UV[num].x * (float)Res), 0, max);
			int num3 = Math.Clamp((int)(UV[num].y * (float)Res), 0, max) * Res + num2;
			Topologies[num] = Data[num3];
		}
	}
}
