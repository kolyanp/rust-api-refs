using UnityEngine;

public class MetalDetectorMissionFlag : MetalDetectorFlag
{
	public override void OnFullyDug(BasePlayer player)
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		if (player.TryGetActiveMissionInstance(out var instance))
		{
			BaseMission mission = instance.GetMission();
			int count = instance.objectiveStatuses.Count;
			int num = mission.objectives.Length;
			if (count != num)
			{
				Debug.LogError((object)$"Mission instance for mission {((Object)mission).name} contains data for {count} objectives but mission has {num} objectives", (Object)(object)mission);
				return;
			}
			for (int i = 0; i < instance.objectiveStatuses.Count; i++)
			{
				BaseMission.MissionInstance.ObjectiveStatus objectiveStatus = instance.objectiveStatuses[i];
				if (objectiveStatus.IsObjectiveActive() && !objectiveStatus.softCompleted && mission.objectives[i].objective is MissionObjective_MetalDetectorDeepSeaTreasure missionObjective_MetalDetectorDeepSeaTreasure && objectiveStatus.progressCurrent >= (float)missionObjective_MetalDetectorDeepSeaTreasure.minimumDigAttempts && (objectiveStatus.progressCurrent >= (float)missionObjective_MetalDetectorDeepSeaTreasure.maximumDigAttempts || Random.Range(0f, 1f) <= missionObjective_MetalDetectorDeepSeaTreasure.successfulDigChange))
				{
					if ((Object)(object)Collision != (Object)null)
					{
						Collision.enabled = false;
					}
					BaseEntity baseEntity = GameManager.server.CreateEntity(missionObjective_MetalDetectorDeepSeaTreasure.treasurePrefab.resourcePath, ((Component)this).transform.position, Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f));
					baseEntity.Spawn();
					if (baseEntity is SingleUseMissionStorageContainer singleUseMissionStorageContainer)
					{
						singleUseMissionStorageContainer.PermitUserId(player.userID.Get());
					}
					instance.persistentMissionEntities.Add(baseEntity);
					BaseMission.MissionEventPayload payload = new BaseMission.MissionEventPayload
					{
						NetworkIdentifier = baseEntity.net.ID,
						UintIdentifier = baseEntity.prefabID,
						WorldPosition = ((Component)this).transform.position
					};
					player.ProcessMissionEvent(BaseMission.MissionEventType.METAL_DETECTOR_FIND, payload, 1f);
					return;
				}
			}
		}
		base.OnFullyDug(player);
	}
}
