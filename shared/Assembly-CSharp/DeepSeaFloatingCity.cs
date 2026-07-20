using UnityEngine;

public class DeepSeaFloatingCity : BaseEntity
{
	public GameObjectRef MapMarker;

	public override void ServerInit()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		DeepSeaManager.ServerFloatingCities.Add(this);
		BakedShoreVectors bakedShoreVectors = PrefabAttribute.server.Find<BakedShoreVectors>(prefabID);
		if (bakedShoreVectors != null)
		{
			Invoke(delegate
			{
				TerrainMeta.Texturing.ApplyBakedDeepSeaVectors(bakedShoreVectors, ((Component)this).transform);
			}, 1f);
		}
		if (MapMarker.isValid)
		{
			base.gameManager.CreateEntity(MapMarker.resourcePath, ((Component)this).transform.position, ((Component)this).transform.rotation).Spawn();
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		DeepSeaManager.ServerFloatingCities.Remove(this);
	}

	public override void AdminKill()
	{
	}

	public void TriggerWipeAlarm(GameObjectRef effectPrefab)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Effect.server.Run(effectPrefab.resourcePath, this, 0u, default(Vector3), default(Vector3), null, false, null, 0, Effect.Type.Generic);
	}
}
