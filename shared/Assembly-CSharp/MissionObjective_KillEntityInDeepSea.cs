using UnityEngine;

public class MissionObjective_KillEntityInDeepSea : MissionObjective_KillEntity
{
	public override bool IsObjectiveValid(int index, BaseMission.MissionInstance instance)
	{
		if ((Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null && PointEntity<DeepSeaManager>.ServerInstance.IsOpen())
		{
			return base.IsObjectiveValid(index, instance);
		}
		return false;
	}

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float delta)
	{
		if ((Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null && !PointEntity<DeepSeaManager>.ServerInstance.IsOpen())
		{
			FailMission(instance, assignee, BaseMission.MissionFailReason.DeepSeaClosed);
		}
		else
		{
			base.DoServerThink(index, instance, assignee, delta);
		}
	}
}
