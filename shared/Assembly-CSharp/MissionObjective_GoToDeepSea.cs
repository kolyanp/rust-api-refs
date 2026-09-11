using UnityEngine;

public class MissionObjective_GoToDeepSea : MissionObjective
{
	[InspectorName("Maximum Time To Wipe (s)")]
	[Tooltip("Time in seconds from deep sea wipe in which this objective will be considered as valid. If current time to wipe is less than this, then the mission cannot be started. If value <= 0, then this value is ignored.")]
	public int maximumTimeToDeepSeaWipe;

	[Tooltip("Should fail mission when deep sea closes.")]
	public bool shouldFailMissionWhenDeepSeaCloses;

	public override bool IsObjectiveValid(int index, BaseMission.MissionInstance instance)
	{
		if ((Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null && PointEntity<DeepSeaManager>.ServerInstance.IsOpen())
		{
			if (maximumTimeToDeepSeaWipe > 0)
			{
				return PointEntity<DeepSeaManager>.ServerInstance.GetTimeToWipe() > (float)maximumTimeToDeepSeaWipe;
			}
			return true;
		}
		return false;
	}

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		base.ServerObjectiveStarted(playerFor, index, instance);
		if ((Object)(object)PointEntity<DeepSeaManager>.ServerInstance == (Object)null)
		{
			Debug.LogError((object)("Mission instance for " + ((Object)instance.GetMission()).name + " failed to retrieve server instance for DeepSeaManager"), (Object)(object)instance.GetMission());
			return;
		}
		SetObjectiveWorldLocation(index, instance, ((Bounds)(ref DeepSeaManager.DeepSeaBounds)).center);
		playerFor.MissionsDirty();
	}

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float delta)
	{
		if ((Object)(object)PointEntity<DeepSeaManager>.ServerInstance == (Object)null)
		{
			return;
		}
		if (shouldFailMissionWhenDeepSeaCloses && !PointEntity<DeepSeaManager>.ServerInstance.IsOpen())
		{
			FailMission(instance, assignee, BaseMission.MissionFailReason.DeepSeaClosed);
		}
		if (!CanProgress(index, instance) || (IsCompleted(index, instance) && instance.objectiveStatuses[index].blockReset))
		{
			return;
		}
		bool completed = instance.objectiveStatuses[index].completed;
		bool flag = DeepSeaManager.IsInsideDeepSea((BaseNetworkable)assignee);
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
