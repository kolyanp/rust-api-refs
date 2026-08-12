using UnityEngine;

public class RepositionOnGroundMissing : EntityComponent<BaseEntity>, IServerComponent
{
	public GameObjectRef originalPrefab;

	public bool killIfInvalid;

	public LayerMask castLayers;

	private void OnGroundMissing()
	{
		Invoke(Process, 0.1f);
	}

	private void Process()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((Component)this).gameObject);
		if (!((Object)(object)baseEntity == (Object)null))
		{
			BaseCombatEntity baseCombatEntity = baseEntity as BaseCombatEntity;
			Vector3 position = ((Component)baseCombatEntity).transform.position;
			Quaternion rotation = ((Component)baseCombatEntity).transform.rotation;
			if (GamePhysics.Trace(new Ray(((Component)this).transform.position, Vector3.down), 0f, out var hitInfo, 100f, LayerMask.op_Implicit(castLayers), (QueryTriggerInteraction)0))
			{
				position = ((RaycastHit)(ref hitInfo)).point;
				rotation = Quaternion.FromToRotation(((Component)baseEntity).transform.up, ((RaycastHit)(ref hitInfo)).normal) * ((Component)baseCombatEntity).transform.rotation;
			}
			else
			{
				float height = TerrainMeta.HeightMap.GetHeight(((Component)this).transform.position);
				Vector3 normal = TerrainMeta.HeightMap.GetNormal(((Component)this).transform.position);
				position = Vector3Ex.WithY(baseEntity.ServerPosition, height);
				rotation = Quaternion.LookRotation(((Component)baseEntity).transform.forward, normal);
			}
			uint prefabID = (originalPrefab.isValid ? originalPrefab.resourceID : baseEntity.prefabID);
			if (baseEntity is ContainerCorpse containerCorpse)
			{
				prefabID = containerCorpse.entityToSpawn.resourceID;
			}
			if (!ContainerCorpse.IsValidPointForEntity(prefabID, position, rotation, baseEntity) && killIfInvalid)
			{
				Debug.LogWarning((object)$"Killing {baseCombatEntity.ShortPrefabName} instead of repositioning as we couldn't find a valid position for {position}");
				baseCombatEntity.Kill();
			}
			else
			{
				baseEntity.ServerPosition = position;
				baseEntity.ServerRotation = rotation;
				baseEntity.SendNetworkUpdate();
			}
		}
	}

	public RepositionOnGroundMissing()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		castLayers = LayerMask.op_Implicit(10551552);
		base._002Ector();
	}
}
