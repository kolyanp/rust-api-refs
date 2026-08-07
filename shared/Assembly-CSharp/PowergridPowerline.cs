using UnityEngine;

public class PowergridPowerline : BaseEntity
{
	[Header("Access Point Spawn")]
	public uint accessPointSpawnSeed = 275u;

	public float accessPointSpawnProbability = 0.65f;

	public BaseEntityRef accessPointPrefab;

	public Transform spawnAccessPointHere;

	public GameObject[] disableIfNoAccessPoint;

	public override void ServerInit()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		if (ShouldSpawnAccessPoint())
		{
			if (!World.LoadedFromSave)
			{
				Vector3 pos = default(Vector3);
				Quaternion rot = default(Quaternion);
				spawnAccessPointHere.GetPositionAndRotation(ref pos, ref rot);
				BaseEntity baseEntity = GameManager.server.CreateEntity(accessPointPrefab.resourcePath, pos, rot);
				if ((Object)(object)baseEntity == (Object)null)
				{
					Debug.LogError((object)("Failed to spawn entity from " + accessPointPrefab.resourcePath));
				}
				else
				{
					baseEntity.Spawn();
				}
			}
			OnAccessPointSpawn();
		}
		else
		{
			OnNoAccessPointSpawn();
		}
	}

	public bool ShouldSpawnAccessPoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		uint num = SeedEx.Seed(((Component)this).transform.position, World.Seed + accessPointSpawnSeed);
		return SeedRandom.Value(ref num) > accessPointSpawnProbability;
	}

	private void OnAccessPointSpawn()
	{
		GameObject[] array = disableIfNoAccessPoint;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(true);
		}
	}

	private void OnNoAccessPointSpawn()
	{
		GameObject[] array = disableIfNoAccessPoint;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
	}
}
