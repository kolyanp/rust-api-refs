using UnityEngine;

public class MissionObjective_OpenStorageContainer : MissionObjective
{
	public BaseEntity TargetEntity;

	[BaseMission.PositionGenerator.PositionPoint]
	[Tooltip("If set, the objective world location will be set to this position.")]
	public string SetObjectiveLocation;

	[BaseMission.PositionGenerator.PositionPoint]
	[Tooltip("The opened container must be nearby this mission point for the objective to complete.")]
	public string RequireProximityToPosition;

	[Min(0f)]
	[Tooltip("If RequireProximityToPosition is set, this defines the minimum proximity between the opened storage container and the mission point.")]
	public float MinimumDistanceToMissionPoint;

	private float sqrDistanceToMissionPoint;

	private void OnEnable()
	{
		CacheSqrDistanceForCompletion();
	}

	private void CacheSqrDistanceForCompletion()
	{
		sqrDistanceToMissionPoint = MinimumDistanceToMissionPoint * MinimumDistanceToMissionPoint;
	}

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		base.ServerObjectiveStarted(playerFor, index, instance);
		if (!string.IsNullOrWhiteSpace(SetObjectiveLocation))
		{
			if (instance.TryGetMissionPoint(SetObjectiveLocation, out var point))
			{
				SetObjectiveWorldLocation(index, instance, point);
				playerFor.MissionsDirty();
				return;
			}
			Debug.LogError((object)("Objective " + ((Object)this).name + " on mission " + ((Object)instance.GetMission()).name + " failed to find an objective location for identifier " + SetObjectiveLocation), (Object)(object)instance.GetMission());
		}
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.OPEN_STORAGE || IsCompleted(index, instance) || !CanProgress(index, instance) || TargetEntity.prefabID != payload.UintIdentifier)
		{
			return;
		}
		if (!BaseNetworkable.serverEntities.TryGetEntity(payload.NetworkIdentifier, out var entity))
		{
			Debug.LogError((object)$"Failed to find {payload.NetworkIdentifier} in server entities", (Object)(object)this);
			return;
		}
		if (!string.IsNullOrWhiteSpace(RequireProximityToPosition))
		{
			instance.TryGetMissionPoint(RequireProximityToPosition, out var point);
			if (Vector3.SqrMagnitude(point - ((Component)entity).transform.position) > sqrDistanceToMissionPoint)
			{
				return;
			}
		}
		CompleteObjective(index, instance, playerFor);
	}
}
