using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public static class CrabGroupingJobs
{
	[BurstCompile]
	public struct CrabGroundGatherJob : IJob
	{
		public int castCursor;

		public int castCount;

		public int crabCount;

		public float maxStepHeight;

		public NativeArray<RaycastCommand> castCommands;

		public NativeArray<RaycastCommand> obstacleCommands;

		public NativeArray<Vector3> castPositions;

		public NativeArray<int> crabCastIndices;

		public ReadOnly<CrabGrouping.CrabData> crabDataArray;

		public ReadOnly<CrabGrouping.CrabRenderData> crabRenderDataArray;

		public void Execute()
		{
		}
	}

	[BurstCompile]
	public struct CrabGroundProcessJob : IJob
	{
		public int castCount;

		public bool hasFallbackY;

		public NativeArray<CrabGrouping.CrabData> crabDataArray;

		public ReadOnly<RaycastHit> castResults;

		public ReadOnly<int> crabCastIndices;

		public ReadOnly<float> crabCastFallbackY;

		public void Execute()
		{
		}
	}

	[BurstCompile]
	public struct CrabObstacleProcessJob : IJob
	{
		public int castCount;

		public NativeArray<CrabGrouping.CrabData> crabDataArray;

		public ReadOnly<RaycastHit> obstacleResults;

		public ReadOnly<int> crabCastIndices;

		public ReadOnly<CrabGrouping.CrabRenderData> crabRenderDataArray;

		public void Execute()
		{
		}
	}

	[BurstCompile]
	public struct CrabUpdateJob : IJobParallelFor
	{
		public float3 swarmCenter;

		public int startleCount;

		public uint seed;

		public float dt;

		public CrabGrouping.CrabSettings settings;

		public ReadOnly<float3> startlePositions;

		public NativeArray<CrabGrouping.CrabData> crabDataArray;

		public NativeArray<CrabGrouping.CrabRenderData> crabRenderDataArray;

		public void Execute(int i)
		{
		}
	}

	[BurstCompile]
	public struct KillCrab : IJob
	{
		public NativeArray<CrabGrouping.CrabData> crabDataArray;

		public NativeArray<CrabGrouping.CrabRenderData> crabRenderDataArray;

		public NativeArray<int> crabCount;

		public void Execute()
		{
		}
	}
}
