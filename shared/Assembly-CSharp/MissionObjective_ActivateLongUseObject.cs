using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/Activate Long Use Object")]
public class MissionObjective_ActivateLongUseObject : MissionObjective
{
	public BaseEntity RequiredEntity;

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type == BaseMission.MissionEventType.LONG_USE_OBJECT && !IsCompleted(index, instance) && CanProgress(index, instance))
		{
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(payload.NetworkIdentifier);
			if ((Object)(object)baseNetworkable != (Object)null && (Object)(object)RequiredEntity != (Object)null && RequiredEntity.prefabID == baseNetworkable.prefabID)
			{
				CompleteObjective(index, instance, playerFor);
				playerFor.MissionsDirty(saveImmediately: true);
			}
		}
	}
}
