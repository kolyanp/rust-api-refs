using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public static class BirdGroupingJobs
{
	[BurstCompile]
	public struct BirdProbeProcessJob : IJob
	{
		public int castCount;

		public int sampleCount;

		public float lookAheadDistance;

		public NativeArray<BirdGrouping.BirdData> birdDataArray;

		public ReadOnly<RaycastHit> castResults;

		public ReadOnly<int> castIndices;

		public ReadOnly<float> sampleSurfaceY;

		public ReadOnly<float3> flockVelocities;

		public void Execute()
		{
		}
	}

	[BurstCompile]
	public struct BirdUpdateJob : IJobParallelFor
	{
		public float3 flockCenter;

		public float3 anchorVelocity;

		public quaternion flockRotation;

		public bool fleeing;

		public bool interested;

		public int birdCount;

		public int threatCount;

		public float dt;

		public float elapsedTime;

		public BirdGrouping.BirdSettings settings;

		public ReadOnly<float3> flockPositions;

		public ReadOnly<float3> flockVelocities;

		public ReadOnly<float3> threatPositions;

		public NativeArray<BirdGrouping.BirdData> birdDataArray;

		public NativeArray<BirdGrouping.BirdRenderData> birdRenderDataArray;

		public void Execute(int i)
		{
		}
	}
}
