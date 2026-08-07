using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/HurtEntityType")]
public class MissionObjective_HurtEntityType : MissionObjective
{
	public BaseEntityRef[] targetEntities;

	public LayerMask targetLayerMask = LayerMask.op_Implicit(-1);

	public float targetDamage = 1f;

	public bool shouldUpdateMissionLocation = true;

	private bool isInitalized;

	private readonly HashSet<uint> targetPrefabIDs = new HashSet<uint>();

	private void EnsureInitialized()
	{
		if (isInitalized)
		{
			return;
		}
		BaseEntityRef[] array = targetEntities;
		foreach (BaseEntityRef baseEntityRef in array)
		{
			if (baseEntityRef.isValid)
			{
				targetPrefabIDs.Add(baseEntityRef.Get().prefabID);
			}
		}
		isInitalized = true;
	}

	public override bool IsEntityValidForObjective<T>(T entity)
	{
		if (!(entity is BaseCombatEntity baseCombatEntity))
		{
			return false;
		}
		if (!targetPrefabIDs.Contains(entity.prefabID))
		{
			return false;
		}
		if (!baseCombatEntity.IsAlive())
		{
			return false;
		}
		return true;
	}

	public override void MissionStarted(int index, BaseMission.MissionInstance instance, BasePlayer forPlayer)
	{
		base.MissionStarted(index, instance, forPlayer);
		instance.objectiveStatuses[index].progressCurrent = 0f;
		instance.objectiveStatuses[index].progressTarget = targetDamage;
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.HURT_ENTITY || IsCompleted(index, instance) || !CanProgress(index, instance))
		{
			return;
		}
		EnsureInitialized();
		EntityRef<BaseCombatEntity> entityRef = new EntityRef<BaseCombatEntity>
		{
			uid = payload.NetworkIdentifier
		};
		BaseCombatEntity baseCombatEntity = entityRef.Get(serverside: true);
		if (baseCombatEntity.IsValid() && targetPrefabIDs.Contains(baseCombatEntity.prefabID))
		{
			instance.objectiveStatuses[index].progressCurrent += amount;
			if (instance.objectiveStatuses[index].progressCurrent >= targetDamage)
			{
				CompleteObjective(index, instance, playerFor);
			}
			playerFor.MissionsDirty(saveImmediately: true);
		}
	}
}
