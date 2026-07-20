using ConVar;
using Oxide.Core;
using UnityEngine;

public class DestroyOnGroundMissing : MonoBehaviour, IServerComponent
{
	private void OnGroundMissing()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((Component)this).gameObject);
		if ((Object)(object)baseEntity != (Object)null && Interface.CallHook("OnEntityGroundMissing", baseEntity) == null)
		{
			BaseCombatEntity baseCombatEntity = baseEntity as BaseCombatEntity;
			if (Stability.log_ground_missing_death)
			{
				Debug.Log((object)$"Killing '{((object)baseEntity).ToString()}' at position {((Component)this).transform.position} due to ground missing");
			}
			if ((Object)(object)baseCombatEntity != (Object)null)
			{
				baseCombatEntity.Die();
			}
			else
			{
				baseEntity.Kill(BaseNetworkable.DestroyMode.Gib);
			}
		}
	}
}
