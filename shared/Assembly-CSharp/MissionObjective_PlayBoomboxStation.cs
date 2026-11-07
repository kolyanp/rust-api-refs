using UnityEngine;

public class MissionObjective_PlayBoomboxStation : MissionObjective
{
	public string targetRadioIp;

	[Tooltip("If true, the boombox must be a static environment boombox for this objective to complete.")]
	public bool boomboxMustBeStatic;

	[BaseMission.PositionGenerator.PositionPoint]
	[Tooltip("The boombox must be nearby this mission point for the objective to complete.")]
	public string requireProximityToPosition;

	[Min(0f)]
	[Tooltip("If RequireProximityToPosition is set, this defines the minimum proximity between the boombox and the mission point.")]
	public float minimumDistanceToMissionPoint;

	[Tooltip("If enabled, boomboxes activated by team members also count, as long as the player and team member are inside the same underwater lab.")]
	public bool doActivationsFromTeamMembersCount;

	public bool shouldHideCompassMarkerWhenClose;

	[Min(0f)]
	[Tooltip("If \"Should Hide Compass Marker When Close\" is enabled and player is within this distance of the mission point then hide the compass marker, else the compass marker is visible.")]
	public float hideCompassMarkerDistance = 50f;

	private float sqrDistanceToMissionPoint;

	private float sqrDistanceToHideCompassMarker;

	private void OnEnable()
	{
		CacheSqrDistanceForCompletion();
	}

	private void CacheSqrDistanceForCompletion()
	{
		sqrDistanceToMissionPoint = minimumDistanceToMissionPoint * minimumDistanceToMissionPoint;
		sqrDistanceToHideCompassMarker = hideCompassMarkerDistance * hideCompassMarkerDistance;
	}

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		base.ServerObjectiveStarted(playerFor, index, instance);
		instance.TryGetMissionPoint(requireProximityToPosition, out var point);
		SetObjectiveWorldLocation(index, instance, point);
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.PLAY_BOOMBOX || IsCompleted(index, instance) || !CanProgress(index, instance) || !DoesActivationCount(playerFor, payload, doActivationsFromTeamMembersCount) || (boomboxMustBeStatic && !Mathf.Approximately(1f, amount)) || string.IsNullOrWhiteSpace(payload.StringIdentifier) || payload.StringIdentifier != targetRadioIp)
		{
			return;
		}
		if (!string.IsNullOrWhiteSpace(requireProximityToPosition))
		{
			instance.TryGetMissionPoint(requireProximityToPosition, out var point);
			if (Vector3.SqrMagnitude(point - payload.WorldPosition) > sqrDistanceToMissionPoint)
			{
				return;
			}
		}
		if (BaseNetworkable.serverEntities.TryGetEntity(payload.NetworkIdentifier, out var entity))
		{
			instance.persistentMissionEntities.Add(entity);
		}
		else
		{
			Debug.LogError((object)("Failed to find a server entity with network ID " + ((object)payload.NetworkIdentifier/*cast due to constrained. prefix*/).ToString()));
		}
		CompleteObjective(index, instance, playerFor);
	}

	public static bool DoesActivationCount(BasePlayer playerFor, BaseMission.MissionEventPayload payload, bool doActivationsFromTeamMembersCount)
	{
		BasePlayer initiatorPlayer = payload.InitiatorPlayer;
		if ((Object)(object)initiatorPlayer == (Object)(object)playerFor)
		{
			return true;
		}
		if (!doActivationsFromTeamMembersCount || (Object)(object)initiatorPlayer == (Object)null)
		{
			return false;
		}
		RelationshipManager.PlayerTeam team = initiatorPlayer.Team;
		if (team == null || !team.members.Contains(playerFor.userID))
		{
			return false;
		}
		if (TryGetUnderwaterLab(initiatorPlayer, out var lab) && TryGetUnderwaterLab(playerFor, out var lab2))
		{
			return (Object)(object)lab == (Object)(object)lab2;
		}
		return false;
		static bool TryGetUnderwaterLab(BasePlayer player, out DungeonBaseInfo reference)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			reference = null;
			Vector3 position = ((Component)player).transform.position;
			if ((Object)(object)TerrainMeta.Path == (Object)null)
			{
				return false;
			}
			if (!EnvironmentManager.Check(position, EnvironmentType.UnderwaterLab))
			{
				return false;
			}
			reference = TerrainMeta.Path.FindClosest(TerrainMeta.Path.DungeonBaseEntrances, position);
			return (Object)(object)reference != (Object)null;
		}
	}
}
