using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/Kill")]
public class MissionObjective_KillEntity : MissionObjective
{
	public BaseEntityRef[] targetEntities;

	public LayerMask targetLayerMask = LayerMask.op_Implicit(-1);

	public int numToKill;

	public bool shouldUpdateMissionLocation;

	public bool pingTargets;

	public bool doKillsFromTeamMembersCount;

	[Tooltip("If enabled, the player must be within the defined distance threshold of the team member which initiated the kill for the objective to progress.")]
	public bool enableDistanceThresholdForTeamkills;

	public float teamkillDistanceThreshold = 50f;

	public Enum mustBeInBiome = (Enum)(-1);

	private readonly HashSet<uint> targetPrefabIDs = new HashSet<uint>();

	private bool isInitialized;

	private float teamkillDistanceThresholdSqr;

	public override BasePlayer.PingType PingType => BasePlayer.PingType.Hostile;

	private void EnsureInitialized()
	{
		if (isInitialized)
		{
			return;
		}
		CacheSqrDistanceForCompletion();
		BaseEntityRef[] array = targetEntities;
		foreach (BaseEntityRef baseEntityRef in array)
		{
			if (!baseEntityRef.isValid)
			{
				break;
			}
			targetPrefabIDs.Add(baseEntityRef.Get().prefabID);
		}
		isInitialized = true;
	}

	public override bool IsEntityValidForObjective<T>(T entity)
	{
		if ((Object)(object)entity == (Object)null)
		{
			return false;
		}
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

	private void CacheSqrDistanceForCompletion()
	{
		teamkillDistanceThresholdSqr = teamkillDistanceThreshold * teamkillDistanceThreshold;
	}

	public override void MissionStarted(int index, BaseMission.MissionInstance instance, BasePlayer forPlayer)
	{
		base.MissionStarted(index, instance, forPlayer);
		instance.objectiveStatuses[index].progressCurrent = 0f;
		instance.objectiveStatuses[index].progressTarget = numToKill;
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Invalid comparison between Unknown and I4
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.KILL_ENTITY || IsCompleted(index, instance) || !CanProgress(index, instance))
		{
			return;
		}
		EnsureInitialized();
		NetworkableId networkIdentifier = payload.NetworkIdentifier;
		uint uintIdentifier = payload.UintIdentifier;
		int intIdentifier = payload.IntIdentifier;
		bool flag = playerFor.net.ID == networkIdentifier;
		if (doKillsFromTeamMembersCount)
		{
			if (!flag)
			{
				if (!(BaseNetworkable.serverEntities.Find(networkIdentifier) is BasePlayer { Team: not null } basePlayer))
				{
					return;
				}
				bool flag2 = false;
				foreach (ulong member in basePlayer.Team.members)
				{
					if (member == (ulong)playerFor.userID)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2 || (enableDistanceThresholdForTeamkills && Vector3.SqrMagnitude(((Component)basePlayer).transform.position - ((Component)playerFor).transform.position) > teamkillDistanceThresholdSqr))
				{
					return;
				}
			}
		}
		else if (!flag)
		{
			return;
		}
		if (((int)mustBeInBiome == -1 || TerrainMeta.IsInBiome(payload.WorldPosition, mustBeInBiome)) && targetPrefabIDs.Contains(uintIdentifier))
		{
			instance.objectiveStatuses[index].progressCurrent += intIdentifier;
			if (instance.objectiveStatuses[index].progressCurrent >= (float)numToKill)
			{
				CompleteObjective(index, instance, playerFor);
			}
			playerFor.DeregisterPingedEntitiesOfType(BasePlayer.PingType.Hostile);
			playerFor.MissionsDirty(saveImmediately: true);
		}
	}

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float timeSinceLastThink)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (!shouldUpdateMissionLocation || !IsStarted(index, instance) || IsCompleted(index, instance))
		{
			return;
		}
		ref RealTimeSince sinceLastThink = ref instance.objectiveStatuses[index].sinceLastThink;
		if (!(RealTimeSince.op_Implicit(sinceLastThink) < 1f))
		{
			sinceLastThink = RealTimeSince.op_Implicit(0f);
			EnsureInitialized();
			assignee.DeregisterPingedEntitiesOfType(BasePlayer.PingType.Hostile);
			if (pingTargets && TryFindNearby<BaseCombatEntity>(((Component)assignee).transform.position, ((LayerMask)(ref targetLayerMask)).value, out var entity, 200f))
			{
				SetObjectiveWorldLocation(index, instance, ((Component)entity).transform.position);
				assignee.MissionsDirty();
				assignee.RegisterPingedEntity(entity, BasePlayer.PingType.Hostile);
			}
		}
	}
}
