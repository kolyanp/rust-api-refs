using UnityEngine;

public class MissionObjective_UnderwaterLabsBoomboxBonus : MissionObjective
{
	public string targetRadioIp;

	[BaseMission.PositionGenerator.PositionPoint]
	[Tooltip("The boombox must be nearby this mission point for the objective to complete.")]
	public string requireProximityToPosition;

	[Min(0f)]
	[Tooltip("If RequireProximityToPosition is set, this defines the minimum proximity between the boombox and the mission point.")]
	public float minimumDistanceToMissionPoint;

	[Tooltip("If enabled, boomboxes activated by team members also count, as long as the player and team member are inside the same underwater lab.")]
	public bool doActivationsFromTeamMembersCount;

	public bool shouldHideCompassMarkerWhenClose;

	[Tooltip("If \"Should Hide Compass Marker When Close\" is enabled and player is within this distance of the mission point then hide the compass marker, else the compass marker is visible.")]
	[Min(0f)]
	public float hideCompassMarkerDistance = 50f;

	private float sqrMinimumDistanceToMissionPoint;

	private float sqrDistanceToHideCompassMarker;

	private void OnEnable()
	{
		CacheSqrDistanceForCompletion();
	}

	private void CacheSqrDistanceForCompletion()
	{
		sqrMinimumDistanceToMissionPoint = minimumDistanceToMissionPoint * minimumDistanceToMissionPoint;
		sqrDistanceToHideCompassMarker = hideCompassMarkerDistance * hideCompassMarkerDistance;
	}

	public override bool IsObjectiveValid(int index, BaseMission.MissionInstance instance)
	{
		return CountValidBoomboxes(instance) >= 2;
	}

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		base.ServerObjectiveStarted(playerFor, index, instance);
		instance.TryGetMissionPoint(requireProximityToPosition, out var point);
		SetObjectiveWorldLocation(index, instance, point);
	}

	public override void MissionStarted(int index, BaseMission.MissionInstance instance, BasePlayer forPlayer)
	{
		base.MissionStarted(index, instance, forPlayer);
		instance.objectiveStatuses[index].progressTarget = CountValidBoomboxes(instance) - 1;
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.PLAY_BOOMBOX || !Mathf.Approximately(amount, 1f) || IsCompleted(index, instance) || !CanProgress(index, instance) || !MissionObjective_PlayBoomboxStation.DoesActivationCount(playerFor, payload, doActivationsFromTeamMembersCount) || string.IsNullOrWhiteSpace(payload.StringIdentifier) || payload.StringIdentifier != targetRadioIp || !IsBoomboxPositionValid(payload.WorldPosition, instance))
		{
			return;
		}
		if (!BaseNetworkable.serverEntities.TryGetEntity(payload.NetworkIdentifier, out var entity))
		{
			Debug.LogError((object)("Failed to find a server entity with network ID " + ((object)payload.NetworkIdentifier/*cast due to constrained. prefix*/).ToString()));
			return;
		}
		for (int i = 0; i < instance.persistentMissionEntities.Count; i++)
		{
			if ((Object)(object)instance.persistentMissionEntities[i] == (Object)(object)entity)
			{
				return;
			}
		}
		instance.persistentMissionEntities.Add(entity);
		BaseMission.MissionInstance.ObjectiveStatus objectiveStatus = instance.objectiveStatuses[index];
		objectiveStatus.progressCurrent++;
		if (objectiveStatus.progressCurrent >= objectiveStatus.progressTarget)
		{
			CompleteObjective(index, instance, playerFor);
		}
		playerFor.MissionsDirty(saveImmediately: true);
	}

	public int CountValidBoomboxes(Vector3 labPosition)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < DeployableBoomBox.ServerStaticInstances.Count; i++)
		{
			DeployableBoomBox deployableBoomBox = DeployableBoomBox.ServerStaticInstances[i];
			if ((Object)(object)deployableBoomBox == (Object)null)
			{
				Debug.LogWarning((object)string.Format("Null boombox at index {0} in {1}", i, "ServerStaticInstances"));
			}
			else if (IsBoomboxInLab(((Component)deployableBoomBox).transform.position, labPosition))
			{
				num++;
			}
		}
		return num;
	}

	private int CountValidBoomboxes(BaseMission.MissionInstance instance)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetLabPosition(instance, out var labPosition))
		{
			return 0;
		}
		return CountValidBoomboxes(labPosition);
	}

	private bool IsBoomboxPositionValid(Vector3 boomboxWorldPosition, BaseMission.MissionInstance instance)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetLabPosition(instance, out var labPosition))
		{
			return IsBoomboxInLab(boomboxWorldPosition, labPosition);
		}
		return false;
	}

	private bool TryGetLabPosition(BaseMission.MissionInstance instance, out Vector3 labPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		labPosition = Vector3.zero;
		if (!string.IsNullOrWhiteSpace(requireProximityToPosition))
		{
			return instance.TryGetMissionPoint(requireProximityToPosition, out labPosition);
		}
		return true;
	}

	private bool IsBoomboxInLab(Vector3 boomboxWorldPosition, Vector3 labPosition)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrWhiteSpace(requireProximityToPosition) && Vector3.SqrMagnitude(labPosition - boomboxWorldPosition) > sqrMinimumDistanceToMissionPoint)
		{
			return false;
		}
		return EnvironmentManager.Check(boomboxWorldPosition, EnvironmentType.UnderwaterLab);
	}
}
