using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/MountEntityType")]
public class MissionObjective_MountEntityType : MissionObjective
{
	public BaseEntityRef[] targetEntities;

	public LayerMask targetLayerMask;

	public int numToMount;

	public bool shouldUpdateMissionLocation;

	private bool isInitialized;

	private readonly ListHashSet<uint> targetPrefabIDs;

	private void EnsureInitialized()
	{
		if (isInitialized)
		{
			return;
		}
		BaseEntityRef[] array = targetEntities;
		foreach (BaseEntityRef baseEntityRef in array)
		{
			if (baseEntityRef.isValid)
			{
				targetPrefabIDs.TryAdd(baseEntityRef.Get().prefabID);
			}
		}
		isInitialized = true;
	}

	public override bool IsEntityValidForObjective<T>(T entity)
	{
		return targetPrefabIDs.Contains(entity.prefabID);
	}

	public override void MissionStarted(int index, BaseMission.MissionInstance instance, BasePlayer forPlayer)
	{
		base.MissionStarted(index, instance, forPlayer);
		instance.objectiveStatuses[index].progressCurrent = 0f;
		if (numToMount > 1)
		{
			instance.objectiveStatuses[index].progressTarget = numToMount;
		}
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.MOUNT_ENTITY || IsCompleted(index, instance) || !CanProgress(index, instance))
		{
			return;
		}
		EnsureInitialized();
		EntityRef<BaseMountable> entityRef = new EntityRef<BaseMountable>
		{
			uid = payload.NetworkIdentifier
		};
		BaseMountable baseMountable = entityRef.Get(serverside: true);
		if (!baseMountable.IsValid())
		{
			return;
		}
		for (int i = 0; i < targetPrefabIDs.Count; i++)
		{
			uint num = targetPrefabIDs[i];
			BaseVehicle baseVehicle = baseMountable.VehicleParent();
			if (num == baseMountable.prefabID || (!((Object)(object)baseVehicle == (Object)null) && num == baseVehicle.prefabID))
			{
				instance.objectiveStatuses[index].progressCurrent += (int)amount;
				if (instance.objectiveStatuses[index].progressCurrent >= (float)numToMount)
				{
					CompleteObjective(index, instance, playerFor);
				}
				playerFor.MissionsDirty(saveImmediately: true);
				break;
			}
		}
	}

	public MissionObjective_MountEntityType()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		targetLayerMask = LayerMask.op_Implicit(-1);
		numToMount = 1;
		shouldUpdateMissionLocation = true;
		targetPrefabIDs = new ListHashSet<uint>();
		base._002Ector();
	}
}
