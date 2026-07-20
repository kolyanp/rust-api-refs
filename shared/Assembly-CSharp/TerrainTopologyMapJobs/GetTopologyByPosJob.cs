using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTopologyMapJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GetTopologyByPosJob : IJob
{
	[WriteOnly]
	public NativeArray<int> Topologies;

	[ReadOnly]
	public NativeArray<Vector3> Pos;

	[ReadOnly]
	public NativeArray<int> Data;

	[ReadOnly]
	public int Res;

	[ReadOnly]
	public Vector2 DataOrigin;

	[ReadOnly]
	public Vector2 DataScale;

	public void Execute()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int max = Res - 1;
		for (int i = 0; i < Pos.Length; i++)
		{
			float num = (Pos[i].x - DataOrigin.x) * DataScale.x;
			float num2 = (Pos[i].z - DataOrigin.y) * DataScale.y;
			int num3 = Math.Clamp((int)(num * (float)Res), 0, max);
			int num4 = Math.Clamp((int)(num2 * (float)Res), 0, max) * Res + num3;
			Topologies[i] = Data[num4];
		}
	}
}
