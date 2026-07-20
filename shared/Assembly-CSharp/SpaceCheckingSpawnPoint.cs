using UnityEngine;

public class SpaceCheckingSpawnPoint : GenericSpawnPoint
{
	public bool useCustomBoundsCheckMask;

	public LayerMask customBoundsCheckMask;

	public float customBoundsCheckScale = 1f;

	public override bool IsAvailableTo(GameObject prefab)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsAvailableTo(prefab))
		{
			return false;
		}
		if (customBoundsCheckScale == 0f)
		{
			return true;
		}
		if (useCustomBoundsCheckMask)
		{
			return SpawnHandler.CheckBounds(prefab, ((Component)this).transform.position, ((Component)this).transform.rotation, Vector3.one * customBoundsCheckScale, customBoundsCheckMask);
		}
		if ((Object)(object)SingletonComponent<SpawnHandler>.Instance != (Object)null)
		{
			return SingletonComponent<SpawnHandler>.Instance.CheckBounds(prefab, ((Component)this).transform.position, ((Component)this).transform.rotation, Vector3.one * customBoundsCheckScale);
		}
		return false;
	}
}
