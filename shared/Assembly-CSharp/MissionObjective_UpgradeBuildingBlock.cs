using System.Collections.Generic;
using Facepunch;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/Upgrade Building Block")]
public class MissionObjective_UpgradeBuildingBlock : MissionObjective
{
	public bool ShouldPingBlocksLessThanTargetGrade;

	[SerializeField]
	[FormerlySerializedAs("PingType")]
	private BasePlayer.PingType pingType;

	public BuildingGrade.Enum TargetGrade;

	public int RequiredCount = 6;

	public override BasePlayer.PingType PingType => pingType;

	public override void MissionStarted(int index, BaseMission.MissionInstance instance, BasePlayer forPlayer)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		base.MissionStarted(index, instance, forPlayer);
		instance.objectiveStatuses[index].progressCurrent = 0f;
		instance.objectiveStatuses[index].progressTarget = RequiredCount;
		if (!ShouldPingBlocksLessThanTargetGrade)
		{
			return;
		}
		TutorialIsland currentTutorialIsland = forPlayer.GetCurrentTutorialIsland();
		if (!((Object)(object)currentTutorialIsland != (Object)null))
		{
			return;
		}
		Vector3 worldPosOfBuildTarget = currentTutorialIsland.GetWorldPosOfBuildTarget(0);
		List<BuildingBlock> list = Pool.Get<List<BuildingBlock>>();
		Vis.Entities(worldPosOfBuildTarget, 32f, list, 2097152, (QueryTriggerInteraction)2);
		if (list.Count != RequiredCount)
		{
			Debug.LogWarning((object)("Non matching building block count, check RequiredCount on " + ((Object)this).name));
		}
		foreach (BuildingBlock item in list)
		{
			forPlayer.RegisterPingedEntity(item, pingType);
		}
		Pool.FreeUnmanaged<BuildingBlock>(ref list);
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type == BaseMission.MissionEventType.UPGRADE_BUILDING_GRADE && !IsCompleted(index, instance) && CanProgress(index, instance) && payload.IntIdentifier >= (int)TargetGrade)
		{
			instance.objectiveStatuses[index].progressCurrent += 1f;
			if (instance.objectiveStatuses[index].progressCurrent >= (float)RequiredCount)
			{
				CompleteObjective(index, instance, playerFor);
			}
			playerFor.MissionsDirty(saveImmediately: true);
			if (ShouldPingBlocksLessThanTargetGrade)
			{
				playerFor.DeregisterPingedEntity(payload.NetworkIdentifier, pingType);
			}
		}
	}
}
