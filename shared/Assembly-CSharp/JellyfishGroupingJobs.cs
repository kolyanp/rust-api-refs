using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public static class JellyfishGroupingJobs
{
	[BurstCompile]
	public struct JellyfishWaterProcessJob : IJob
	{
		public int sampleCount;

		public NativeArray<JellyfishGrouping.JellyfishData> jellyfishDataArray;

		public ReadOnly<int> sampleIndices;

		public ReadOnly<float> sampleFloorY;

		public ReadOnly<float> sampleSurfaceY;

		public void Execute()
		{
		}
	}

	[BurstCompile]
	public struct JellyfishUpdateJob : IJobParallelFor
	{
		public float3 swarmCenter;

		public int displacerCount;

		public uint seed;

		public float dt;

		public float elapsedTime;

		public JellyfishGrouping.JellyfishSettings settings;

		public ReadOnly<float3> displacerPositions;

		public ReadOnly<float3> displacerVelocities;

		public NativeArray<JellyfishGrouping.JellyfishData> jellyfishDataArray;

		public NativeArray<JellyfishGrouping.JellyfishRenderData> jellyfishRenderDataArray;

		public void Execute(int i)
		{
		}
	}

	[BurstCompile]
	public struct KillJellyfish : IJob
	{
		public NativeArray<JellyfishGrouping.JellyfishData> jellyfishDataArray;

		public NativeArray<JellyfishGrouping.JellyfishRenderData> jellyfishRenderDataArray;

		public NativeArray<int> jellyfishCount;

		public void Execute()
		{
		}
	}
}
