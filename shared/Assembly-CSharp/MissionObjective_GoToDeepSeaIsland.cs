using UnityEngine;

public class MissionObjective_GoToDeepSeaIsland : MissionObjective
{
	private const float ISLAND_PLAYER_DISTANCE_THRESHOLD = 150f;

	private const float ISLAND_PLAYER_DISTANCE_THRESHOLD_SQR = 22500f;

	public override bool IsObjectiveValid(int index, BaseMission.MissionInstance instance)
	{
		if ((Object)(object)PointEntity<DeepSeaManager>.ServerInstance == (Object)null)
		{
			return false;
		}
		if (!PointEntity<DeepSeaManager>.ServerInstance.IsOpen())
		{
			return false;
		}
		for (int i = 0; i < DeepSeaManager.ServerIslands.Count; i++)
		{
			if ((Object)(object)DeepSeaManager.ServerIslands[i] != (Object)null)
			{
				return true;
			}
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
		}
		else
		{
			SetObjectiveWorldLocation(index, instance, ((Bounds)(ref DeepSeaManager.DeepSeaBounds)).center);
		}
	}

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float delta)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (!CanProgress(index, instance) || (IsCompleted(index, instance) && instance.objectiveStatuses[index].blockReset))
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (DeepSeaManager.IsInsideDeepSea((BaseNetworkable)assignee))
		{
			for (int i = 0; i < DeepSeaManager.ServerIslands.Count; i++)
			{
				DeepSeaIsland deepSeaIsland = DeepSeaManager.ServerIslands[i];
				if (!((Object)(object)deepSeaIsland == (Object)null) && !(Vector3.SqrMagnitude(((Component)deepSeaIsland).transform.position - ((Component)assignee).transform.position) > 22500f))
				{
					flag2 = true;
					if (assignee.IsStandingOnEntity(deepSeaIsland, 8388608))
					{
						flag = true;
						break;
					}
				}
			}
		}
		bool completed = instance.objectiveStatuses[index].completed;
		bool flag3 = flag;
		if (completed != flag3)
		{
			if (flag3)
			{
				CompleteObjective(index, instance, assignee);
			}
			else if (!flag2)
			{
				ResetObjective(index, instance, assignee);
			}
		}
	}
}
