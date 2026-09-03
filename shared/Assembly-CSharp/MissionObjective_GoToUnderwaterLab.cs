using UnityEngine;

public class MissionObjective_GoToUnderwaterLab : MissionObjective
{
	[BaseMission.PositionGenerator.PositionPoint]
	public string position;

	[Min(0f)]
	[Tooltip("Player must be within underwater labs environment volume and within this distance of the mission point for the objective to complete.")]
	public float minimumDistanceToPosition = 100f;

	public bool shouldHideCompassMarkerWhenClose;

	[Min(0f)]
	[Tooltip("If \"Should Hide Compass Marker When Close\" is enabled and player is within this distance of the mission point then hide the compass marker, else the compass marker is visible.")]
	public float hideCompassMarkerDistance = 50f;

	private float sqrMinimumDistanceToPosition;

	private float sqrDistanceToHideCompassMarker;

	private void OnEnable()
	{
		CacheSqrDistanceForCompletion();
	}

	private void CacheSqrDistanceForCompletion()
	{
		sqrMinimumDistanceToPosition = minimumDistanceToPosition * minimumDistanceToPosition;
		sqrDistanceToHideCompassMarker = hideCompassMarkerDistance * hideCompassMarkerDistance;
	}

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		base.ServerObjectiveStarted(playerFor, index, instance);
		instance.TryGetMissionPoint(position, out var point);
		SetObjectiveWorldLocation(index, instance, point);
		playerFor.MissionsDirty();
	}

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float delta)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (!CanProgress(index, instance) || (IsCompleted(index, instance) && instance.objectiveStatuses[index].blockReset))
		{
			return;
		}
		float num = Vector3.SqrMagnitude(GetObjectiveWorldLocation(index, instance) - ((Component)assignee).transform.position);
		bool completed = instance.objectiveStatuses[index].completed;
		bool flag = num < sqrMinimumDistanceToPosition && EnvironmentManager.Check(((Component)assignee).transform.position, EnvironmentType.UnderwaterLab);
		if (completed != flag)
		{
			if (flag)
			{
				CompleteObjective(index, instance, assignee);
			}
			else
			{
				ResetObjective(index, instance, assignee);
			}
		}
	}
}
