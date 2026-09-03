using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/Move")]
public class MissionObjective_Move : MissionObjective
{
	[BaseMission.PositionGenerator.PositionPoint]
	public string positionName = "default";

	[InspectorName("Distance For Completion (m)")]
	[Tooltip("Distance threshold to player for objective to complete.")]
	public float distForCompletion = 3f;

	[Tooltip("If true, this objective will no longer be marked as completed if the objective criteria are no longer met.")]
	public bool canBeReset;

	[Tooltip("If \"Can Be Reset\" is true, then distance ")]
	[InspectorName("Distance For Reset (m)")]
	public float distanceForReset = 3f;

	[Tooltip("If true, disregards distance on the y-plane.")]
	[FormerlySerializedAs("use2D")]
	public bool use2DDistance;

	[Tooltip("If set, player must be mounted on this mountable for objective to complete.")]
	public BaseMountable requiredMountable;

	[Tooltip("If true, displays a UI objective marker for this objective. Only works if at Tutorial Island.")]
	[InspectorName("Should Ping (Tutorial Only)")]
	public bool shouldPing;

	[Tooltip("Ping type for when shouldPing is enabled.")]
	[SerializeField]
	private BasePlayer.PingType pingType = BasePlayer.PingType.GoTo;

	private float sqrDistanceForCompletion;

	private float sqrDistanceForReset;

	public override BasePlayer.PingType PingType => pingType;

	private void OnEnable()
	{
		CacheSqrDistances();
	}

	private void CacheSqrDistances()
	{
		sqrDistanceForCompletion = distForCompletion * distForCompletion;
		sqrDistanceForReset = distanceForReset * distanceForReset;
	}

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

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float delta)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		if (!CanProgress(index, instance) || (IsCompleted(index, instance) && canBeReset && instance.objectiveStatuses[index].blockReset) || (IsCompleted(index, instance) && !canBeReset) || ((Object)(object)requiredMountable != (Object)null && (!assignee.isMounted || assignee.GetMounted().prefabID != requiredMountable.prefabID)))
		{
			return;
		}
		instance.TryGetMissionPoint(positionName, out var point);
		float num = (use2DDistance ? Vector3Ex.SqrMagnitude2D(point - ((Component)assignee).transform.position) : Vector3.SqrMagnitude(point - ((Component)assignee).transform.position));
		bool completed = instance.objectiveStatuses[index].completed;
		bool flag = num <= sqrDistanceForCompletion;
		if (completed == flag)
		{
			return;
		}
		if (flag)
		{
			CompleteObjective(index, instance, assignee);
			if (shouldPing)
			{
				TutorialIsland currentTutorialIsland = assignee.GetCurrentTutorialIsland();
				if ((Object)(object)currentTutorialIsland != (Object)null)
				{
					assignee.RemovePingAtLocation(pingType, GetObjectiveWorldLocation(index, instance), float.MaxValue, currentTutorialIsland.net.ID);
				}
			}
		}
		else if (canBeReset && num >= sqrDistanceForReset)
		{
			ResetObjective(index, instance, assignee);
		}
	}
}
