using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public struct GenerateSpawnPoints : IJob
{
	public NativeList<PlaceEntitiesOffshore.SpawnPoint> spawnPoints;

	public int targetCount;

	public uint seed;

	public PlaceEntitiesOffshore.TerrainSpawnBounds terrainSpawnBounds;

	public float minDistanceFromOtherEntities;

	public void Execute()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		while (spawnPoints.Length < targetCount && num < 10000)
		{
			num++;
			GetSpawnPoint(ref seed, in terrainSpawnBounds, in spawnPoints, in minDistanceFromOtherEntities, out var valid, out var position, out var rotation);
			if (valid)
			{
				ref NativeList<PlaceEntitiesOffshore.SpawnPoint> reference = ref spawnPoints;
				PlaceEntitiesOffshore.SpawnPoint spawnPoint = new PlaceEntitiesOffshore.SpawnPoint
				{
					position = position,
					rotation = rotation
				};
				reference.Add(ref spawnPoint);
			}
		}
	}

	private static void GetSpawnPoint(ref uint seed, in PlaceEntitiesOffshore.TerrainSpawnBounds terrainSpawnBounds, in NativeList<PlaceEntitiesOffshore.SpawnPoint> existingSpawnPoints, in float minDistanceFromOtherEntities, out bool valid, out float3 position, out quaternion rotation)
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 0f;
		switch (seed % 4)
		{
		case 0u:
			num = SeedRandom.Range(ref seed, terrainSpawnBounds.leftOuterX, terrainSpawnBounds.leftInnerX);
			num2 = SeedRandom.Range(ref seed, terrainSpawnBounds.bottomOuterZ, terrainSpawnBounds.topOuterZ);
			break;
		case 1u:
			num = SeedRandom.Range(ref seed, terrainSpawnBounds.rightInnerX, terrainSpawnBounds.rightOuterX);
			num2 = SeedRandom.Range(ref seed, terrainSpawnBounds.bottomOuterZ, terrainSpawnBounds.topOuterZ);
			break;
		case 2u:
			num = SeedRandom.Range(ref seed, terrainSpawnBounds.leftOuterX, terrainSpawnBounds.rightOuterX);
			num2 = SeedRandom.Range(ref seed, terrainSpawnBounds.bottomOuterZ, terrainSpawnBounds.bottomInnerZ);
			break;
		case 3u:
			num = SeedRandom.Range(ref seed, terrainSpawnBounds.leftOuterX, terrainSpawnBounds.rightOuterX);
			num2 = SeedRandom.Range(ref seed, terrainSpawnBounds.topInnerZ, terrainSpawnBounds.topOuterZ);
			break;
		}
		float num3 = SeedRandom.Range(ref seed, 0f, 360f);
		position = new float3(num, 0f, num2);
		rotation = quaternion.Euler(0f, math.radians(num3), 0f, (RotationOrder)4);
		valid = true;
		Enumerator<PlaceEntitiesOffshore.SpawnPoint> enumerator = existingSpawnPoints.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (math.distance(enumerator.Current.position, position) < minDistanceFromOtherEntities)
				{
					valid = false;
					break;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
