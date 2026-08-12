using System;
using ConVar;
using UnityEngine;

public class MissionObjective_SpeakWithTargetNPC : MissionObjective
{
	public BaseEntityRef TargetNPC;

	public LayerMask targetLayerMask;

	public ItemAmount[] requiredReturnItems;

	[BaseMission.PositionGenerator.PositionPoint]
	[Tooltip("The target NPC must be nearby this mission point for the objective to complete.")]
	public string RequireProximityToPosition;

	[Tooltip("This defines the minimum proximity between the target NPC and the mission point.")]
	[Min(0f)]
	public float MinimumDistanceToMissionPoint;

	public bool destroyReturnItems;

	public bool showPing;

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		base.ServerObjectiveStarted(playerFor, index, instance);
		Vector3 value;
		NPCPlayer entity;
		if (string.IsNullOrWhiteSpace(RequireProximityToPosition))
		{
			Debug.LogError((object)("RequireProximityToPosition is not set on objective " + ((Object)this).name), (Object)(object)this);
		}
		else if (!instance.missionPoints.TryGetValue(RequireProximityToPosition, out value))
		{
			Debug.LogError((object)("No mission point found for " + RequireProximityToPosition + " on objective " + ((Object)this).name), (Object)(object)this);
		}
		else if (TryFindNearby<NPCPlayer>(value, LayerMask.op_Implicit(targetLayerMask), out entity, 100f))
		{
			SetObjectiveWorldLocation(index, instance, ((Component)entity).transform.position);
			if (showPing && playerFor.IsInTutorial)
			{
				playerFor.RegisterPingedEntity(entity, PingType);
			}
		}
		else
		{
			Debug.LogError((object)("Failed to find an entity for " + TargetNPC.resourcePath + " on objective " + ((Object)this).name), (Object)(object)this);
		}
	}

	public override bool IsEntityValidForObjective<T>(T entity)
	{
		if (!TargetNPC.isValid)
		{
			return false;
		}
		BaseEntity baseEntity = TargetNPC.Get();
		if ((Object)(object)baseEntity == (Object)null)
		{
			return false;
		}
		return baseEntity.prefabID == entity.prefabID;
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.CONVERSATION)
		{
			return;
		}
		BaseMission mission = instance.GetMission();
		if (mission == null)
		{
			Debug.LogError((object)$"Failed to retrieve mission from mission instance ID {instance.missionID}");
			return;
		}
		if (Debugging.printMissionSpeakInfo)
		{
			Debug.Log((object)string.Format("[MissionSpeakInfo] objective {0} on {1} instance IsCompleted:{2} CanProgress:{3}, amount: {4}", new object[5]
			{
				((Object)this).name,
				((Object)mission).name,
				IsCompleted(index, instance),
				CanProgress(index, instance),
				amount
			}));
		}
		if (IsCompleted(index, instance) || !CanProgress(index, instance))
		{
			return;
		}
		if (!BaseNetworkable.serverEntities.TryGetEntity(payload.NetworkIdentifier, out var entity))
		{
			if (Debugging.printMissionSpeakInfo)
			{
				Debug.Log((object)$"[MissionSpeakInfo] objective {((Object)this).name} on {((Object)mission).name} failed to find a entity from payload NetworkIdentifier: {payload.NetworkIdentifier}");
			}
			return;
		}
		if (entity.prefabID != TargetNPC.Get().prefabID)
		{
			if (Debugging.printMissionSpeakInfo)
			{
				Debug.Log((object)string.Format("[MissionSpeakInfo] objective {0} on {1} entity {2} provided from payload NetworkIdentifier: {3} has prefabID: {4} which does not match target prefabID: {5}", new object[6]
				{
					((Object)this).name,
					((Object)mission).name,
					((Object)entity).name,
					payload.NetworkIdentifier,
					entity.prefabID,
					TargetNPC.Get().prefabID
				}), (Object)(object)entity);
			}
			return;
		}
		if (!string.IsNullOrWhiteSpace(RequireProximityToPosition))
		{
			if (!instance.missionPoints.TryGetValue(RequireProximityToPosition, out var value))
			{
				if (Debugging.printMissionSpeakInfo)
				{
					Debug.Log((object)("[MissionSpeakInfo] objective " + ((Object)this).name + " on " + ((Object)mission).name + " failed to find mission point for " + RequireProximityToPosition));
				}
				return;
			}
			float num = Vector3.SqrMagnitude(value - ((Component)entity).transform.position);
			float num2 = MinimumDistanceToMissionPoint * MinimumDistanceToMissionPoint;
			if (num > num2)
			{
				if (Debugging.printMissionSpeakInfo)
				{
					Debug.Log((object)string.Format("[MissionSpeakInfo] objective {0} on {1} entity {2} provided from payload NetworkIdentifier: {3} is {4} square distance away from point {5}, minimum square distance is {6}", new object[7]
					{
						((Object)this).name,
						((Object)mission).name,
						((Object)entity).name,
						payload.NetworkIdentifier,
						num,
						value,
						num2
					}), (Object)(object)entity);
				}
				return;
			}
		}
		if (Mathf.Approximately(amount, 1f))
		{
			bool flag = true;
			ItemAmount[] array = requiredReturnItems;
			foreach (ItemAmount itemAmount in array)
			{
				if ((float)playerFor.inventory.GetAmount(itemAmount.itemDef.itemid) < itemAmount.amount)
				{
					flag = false;
					break;
				}
			}
			if (mission.HasRewards())
			{
				if (Debugging.printMissionSpeakInfo)
				{
					Debug.Log((object)$"[MissionSpeakInfo] objective {((Object)this).name} on {((Object)mission).name} CheckRewardsSpace: {playerFor.HasSpaceForMissionRewards(instance)}");
				}
				if (flag && destroyReturnItems)
				{
					if (!playerFor.HasSpaceForMissionRewards(instance, showToastOnFailure: true))
					{
						return;
					}
					array = requiredReturnItems;
					foreach (ItemAmount itemAmount2 in array)
					{
						playerFor.inventory.Take(null, itemAmount2.itemDef.itemid, (int)itemAmount2.amount);
					}
				}
				if (Debugging.printMissionSpeakInfo)
				{
					Debug.Log((object)$"[MissionSpeakInfo] objective {((Object)this).name} on {((Object)mission).name} CheckRewardsSpace: {playerFor.HasSpaceForMissionRewards(instance)}");
				}
				if (!playerFor.HasSpaceForMissionRewards(instance, showToastOnFailure: true))
				{
					return;
				}
			}
			if (Debugging.printMissionSpeakInfo)
			{
				Debug.Log((object)string.Format("[MissionSpeakInfo] objective {0} on {1} requiredReturnItems == null: {2}, requiredReturnItems.Length: {3}, hasAllReturnItems: {4}", new object[5]
				{
					((Object)this).name,
					((Object)mission).name,
					requiredReturnItems == null,
					requiredReturnItems.Length,
					flag
				}));
			}
			if ((requiredReturnItems == null || requiredReturnItems.Length == 0) | flag)
			{
				CompleteObjective(index, instance, playerFor);
			}
		}
		else if (Debugging.printMissionSpeakInfo)
		{
			Debug.Log((object)$"[MissionSpeakInfo] objective {((Object)this).name} on {((Object)mission).name} supplied amount {amount} is not approximately 1f");
		}
	}

	public override void ObjectiveCompleted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ObjectiveCompleted(playerFor, index, instance);
		if (showPing)
		{
			DeregisterPing(playerFor, instance);
		}
	}

	private static void DeregisterPing(BasePlayer playerFor, BaseMission.MissionInstance instance)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		IMissionProvider missionProvider = instance.GetMissionProvider();
		if (missionProvider != null)
		{
			playerFor.DeregisterPingedEntity(missionProvider.ProviderID(), BasePlayer.PingType.GoTo);
		}
	}

	public override void ObjectiveFailed(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ObjectiveFailed(playerFor, index, instance);
		if (showPing)
		{
			DeregisterPing(playerFor, instance);
		}
	}

	public MissionObjective_SpeakWithTargetNPC()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		targetLayerMask = LayerMask.op_Implicit(-1);
		requiredReturnItems = Array.Empty<ItemAmount>();
		base._002Ector();
	}
}
