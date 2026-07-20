using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PlaceEntitiesOffshore : ProceduralComponent
{
	public struct SpawnPoint
	{
		public float3 position;

		public quaternion rotation;
	}

	public struct TerrainSpawnBounds
	{
		public float leftOuterX;

		public float leftInnerX;

		public float rightInnerX;

		public float rightOuterX;

		public float bottomOuterZ;

		public float bottomInnerZ;

		public float topInnerZ;

		public float topOuterZ;
	}

	[SerializeField]
	private GameObjectRef prefab;

	[SerializeField]
	private float minWorldSize;

	[SerializeField]
	private int targetCount;

	[SerializeField]
	private float minDistanceFromTerrain = 100f;

	[SerializeField]
	private float maxDistanceFromTerrain = 500f;

	[SerializeField]
	private float minDistanceFromOtherEntities = 100f;

	public const int Attempts = 10000;

	public override void Process(uint seed)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if ((float)World.Size < minWorldSize)
		{
			return;
		}
		GetTerrainSpawnBounds(float3.op_Implicit(TerrainMeta.Position), float3.op_Implicit(TerrainMeta.Size), in minDistanceFromTerrain, in maxDistanceFromTerrain, out var bounds);
		NativeList<SpawnPoint> spawnPoints = default(NativeList<SpawnPoint>);
		spawnPoints._002Ector(targetCount, AllocatorHandle.op_Implicit((Allocator)3));
		try
		{
			GenerateSpawnPoints generateSpawnPoints = new GenerateSpawnPoints
			{
				spawnPoints = spawnPoints,
				targetCount = targetCount,
				seed = seed,
				terrainSpawnBounds = bounds,
				minDistanceFromOtherEntities = minDistanceFromOtherEntities
			};
			GenerateSpawnPoints generateSpawnPoints2 = generateSpawnPoints;
			JobHandle val = default(JobHandle);
			val = IJobExtensions.Schedule<GenerateSpawnPoints>(generateSpawnPoints2, val);
			((JobHandle)(ref val)).Complete();
			PlacePrefabs(in generateSpawnPoints.spawnPoints);
		}
		finally
		{
			((IDisposable)spawnPoints/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void GetTerrainSpawnBounds(in float3 terrainPosition, in float3 terrainSize, in float maxDistance, in float minDistance, out TerrainSpawnBounds bounds)
	{
		bounds.leftOuterX = terrainPosition.x - maxDistance;
		bounds.leftInnerX = terrainPosition.x - minDistance;
		bounds.rightInnerX = terrainPosition.x + terrainSize.x + minDistance;
		bounds.rightOuterX = terrainPosition.x + terrainSize.x + maxDistance;
		bounds.bottomOuterZ = terrainPosition.z - maxDistance;
		bounds.bottomInnerZ = terrainPosition.z - minDistance;
		bounds.topInnerZ = terrainPosition.z + terrainSize.z + minDistance;
		bounds.topOuterZ = terrainPosition.z + terrainSize.z + maxDistance;
	}

	private unsafe void PlacePrefabs(in NativeList<SpawnPoint> spawnPoints)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		int length = spawnPoints.Length;
		Span<Vector3> span = new Span<Vector3>(stackalloc Vector3[length], length);
		length = spawnPoints.Length;
		Span<Quaternion> span2 = new Span<Quaternion>(stackalloc Quaternion[length], length);
		for (int i = 0; i < spawnPoints.Length; i++)
		{
			span[i] = float3.op_Implicit(spawnPoints[i].position);
			span2[i] = quaternion.op_Implicit(spawnPoints[i].rotation);
		}
		Object.InstantiateAsync<GameObject>(new GameObject("TestEntityOffshore"), spawnPoints.Length, (ReadOnlySpan<Vector3>)span, (ReadOnlySpan<Quaternion>)span2);
	}
}
