using UnityEngine;

public class MetalDetectorMissionSource : MetalDetectorSource
{
	[Header("Mission settings")]
	public BaseMission mission;

	public MissionObjective objective;

	public override bool IsValidSource(BasePlayer forPlayer)
	{
		if ((Object)(object)forPlayer == (Object)null)
		{
			return false;
		}
		if (!forPlayer.TryGetActiveMissionInstance(out var instance))
		{
			return false;
		}
		BaseMission baseMission = instance.GetMission();
		if (baseMission == null)
		{
			return false;
		}
		if (baseMission != mission)
		{
			return false;
		}
		int count = instance.objectiveStatuses.Count;
		int num = baseMission.objectives.Length;
		if (count != num)
		{
			Debug.LogError((object)$"Mission instance for mission {((Object)baseMission).name} contains data for {count} objectives but mission has {num} objectives", (Object)(object)baseMission);
			return false;
		}
		for (int i = 0; i < instance.objectiveStatuses.Count; i++)
		{
			if (instance.objectiveStatuses[i].IsObjectiveActive() && (Object)(object)baseMission.objectives[i].objective == (Object)(object)objective)
			{
				return true;
			}
		}
		return false;
	}
}
