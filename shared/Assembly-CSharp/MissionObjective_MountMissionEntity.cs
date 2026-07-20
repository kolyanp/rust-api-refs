using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/MountMissionEntity")]
public class MissionObjective_MountMissionEntity : MissionObjective
{
	public string targetIdentifier;

	public bool shouldUpdateMissionLocation = true;

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ServerObjectiveStarted(playerFor, index, instance);
		instance.GetSpawnedMissionEntity(targetIdentifier, playerFor);
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.MOUNT_ENTITY || IsCompleted(index, instance) || !CanProgress(index, instance))
		{
			return;
		}
		MissionEntity spawnedMissionEntity = instance.GetSpawnedMissionEntity(targetIdentifier, playerFor);
		if ((Object)(object)spawnedMissionEntity == (Object)null)
		{
			FailObjective(index, instance, playerFor);
			return;
		}
		BaseEntity entity = spawnedMissionEntity.GetEntity();
		if (!entity.IsValid())
		{
			FailObjective(index, instance, playerFor);
			return;
		}
		EntityRef<BaseMountable> entityRef = new EntityRef<BaseMountable>
		{
			uid = payload.NetworkIdentifier
		};
		BaseMountable baseMountable = entityRef.Get(serverside: true);
		if (baseMountable.IsValid())
		{
			BaseVehicle baseVehicle = baseMountable.VehicleParent();
			if (baseMountable.EqualNetID((BaseNetworkable)entity) || ((Object)(object)baseVehicle != (Object)null && baseVehicle.EqualNetID((BaseNetworkable)entity)))
			{
				CompleteObjective(index, instance, playerFor);
			}
		}
	}

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float delta)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (!IsStarted(index, instance) || IsCompleted(index, instance))
		{
			return;
		}
		ref RealTimeSince sinceLastThink = ref instance.objectiveStatuses[index].sinceLastThink;
		if (RealTimeSince.op_Implicit(sinceLastThink) < 1f)
		{
			return;
		}
		sinceLastThink = RealTimeSince.op_Implicit(0f);
		MissionEntity spawnedMissionEntity = instance.GetSpawnedMissionEntity(targetIdentifier, assignee);
		if ((Object)(object)spawnedMissionEntity == (Object)null)
		{
			FailObjective(index, instance, assignee);
		}
		else if (shouldUpdateMissionLocation)
		{
			Vector3 position = ((Component)spawnedMissionEntity).transform.position;
			if (position != GetObjectiveWorldLocation(index, instance))
			{
				SetObjectiveWorldLocation(index, instance, position);
				assignee.MissionsDirty();
			}
		}
	}
}
