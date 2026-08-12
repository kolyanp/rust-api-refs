using System;
using ConVar;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/SpeakWith")]
public class MissionObjective_SpeakWith : MissionObjective
{
	public ItemAmount[] requiredReturnItems = Array.Empty<ItemAmount>();

	public bool destroyReturnItems;

	public bool showPing;

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		base.ServerObjectiveStarted(playerFor, index, instance);
		IMissionProvider missionProvider = instance.GetMissionProvider();
		if (missionProvider != null)
		{
			SetObjectiveWorldLocation(index, instance, missionProvider.ProviderPosition());
			if (showPing && playerFor.IsInTutorial)
			{
				playerFor.RegisterPingedEntity(missionProvider.GetEntity(), BasePlayer.PingType.GoTo);
			}
			playerFor.MissionsDirty();
		}
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
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
		IMissionProvider missionProvider = instance.GetMissionProvider();
		if (missionProvider == null)
		{
			if (Debugging.printMissionSpeakInfo)
			{
				Debug.Log((object)("[MissionSpeakInfo] objective " + ((Object)this).name + " on " + ((Object)mission).name + " failed to find a provider entity attached to this mission instance"));
			}
			return;
		}
		if (Debugging.printMissionSpeakInfo)
		{
			Debug.Log((object)string.Format("[MissionSpeakInfo] objective {0} on {1} looking for provider: {2}/{3} Supplied NPC:{4}", new object[5]
			{
				((Object)this).name,
				((Object)mission).name,
				instance.providerID.Value,
				((Object)missionProvider.GetEntity()).name,
				payload.NetworkIdentifier
			}));
		}
		if (missionProvider.ProviderID() == payload.NetworkIdentifier && Mathf.Approximately(amount, 1f))
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
			if (missionProvider.ProviderID() != payload.NetworkIdentifier)
			{
				Debug.Log((object)string.Format("[MissionSpeakInfo] objective {0} on {1} failed to match supplied network ID {2} with instance missionProvider.ProviderID(): {3}. ProviderID() should match instance.providerID: {4}", new object[5]
				{
					((Object)this).name,
					((Object)mission).name,
					payload.NetworkIdentifier,
					missionProvider.ProviderID(),
					instance.providerID
				}));
			}
			if (!Mathf.Approximately(amount, 1f))
			{
				Debug.Log((object)$"[MissionSpeakInfo] objective {((Object)this).name} on {((Object)mission).name} supplied amount {amount} is not approximately 1f");
			}
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
}
