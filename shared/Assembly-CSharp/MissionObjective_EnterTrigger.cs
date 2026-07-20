using UnityEngine;

public class MissionObjective_EnterTrigger : MissionObjective
{
	public string positionName = "default";

	public float distForCompletion = 3f;

	public bool use2D;

	public BaseMountable requiredMountable;

	public bool shouldPing;

	[SerializeField]
	private BasePlayer.PingType pingType = BasePlayer.PingType.GoTo;

	public override BasePlayer.PingType PingType => pingType;

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		base.ServerObjectiveStarted(playerFor, index, instance);
		instance.TryGetMissionPoint(positionName, out var point);
		SetObjectiveWorldLocation(index, instance, point);
		playerFor.MissionsDirty();
		if (shouldPing)
		{
			TutorialIsland currentTutorialIsland = playerFor.GetCurrentTutorialIsland();
			if ((Object)(object)currentTutorialIsland != (Object)null)
			{
				playerFor.AddPingAtLocation(pingType, GetObjectiveWorldLocation(index, instance), 86400f, currentTutorialIsland.net.ID);
			}
		}
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (IsCompleted(index, instance) || type != BaseMission.MissionEventType.ENTER_TRIGGER || ((Object)(object)requiredMountable != (Object)null && (!playerFor.isMounted || playerFor.GetMounted().prefabID != requiredMountable.prefabID)))
		{
			return;
		}
		CompleteObjective(index, instance, playerFor);
		if (shouldPing)
		{
			TutorialIsland currentTutorialIsland = playerFor.GetCurrentTutorialIsland();
			if ((Object)(object)currentTutorialIsland != (Object)null)
			{
				playerFor.RemovePingAtLocation(pingType, GetObjectiveWorldLocation(index, instance), float.MaxValue, currentTutorialIsland.net.ID);
			}
		}
	}
}
