using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTopologyMapJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GetTopologyByUVJob : IJob
{
	[WriteOnly]
	public NativeArray<int> Topologies;

	[ReadOnly]
	public NativeArray<Vector2> UV;

	[ReadOnly]
	public NativeArray<int> Data;

	[ReadOnly]
	public int Res;

	public void Execute()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		int max = Res - 1;
		for (int i = 0; i < UV.Length; i++)
		{
			int num = Math.Clamp((int)(UV[i].x * (float)Res), 0, max);
			int num2 = Math.Clamp((int)(UV[i].y * (float)Res), 0, max) * Res + num;
			Topologies[i] = Data[num2];
		}
	}
}
