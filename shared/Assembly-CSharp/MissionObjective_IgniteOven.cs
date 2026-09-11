using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/Ignite Oven")]
public class MissionObjective_IgniteOven : MissionObjective
{
	public BaseEntityRef TargetOven;

	public LayerMask targetLayerMask;

	public bool PingTarget;

	[SerializeField]
	[FormerlySerializedAs("PingType")]
	private BasePlayer.PingType pingType;

	public override BasePlayer.PingType PingType => pingType;

	public override void MissionStarted(int index, BaseMission.MissionInstance instance, BasePlayer forPlayer)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		base.MissionStarted(index, instance, forPlayer);
		if (PingTarget && TryFindNearby<BaseCombatEntity>(((Component)forPlayer).transform.position, LayerMask.op_Implicit(targetLayerMask), out var entity, 200f))
		{
			SetObjectiveWorldLocation(index, instance, ((Component)entity).transform.position);
			forPlayer.RegisterPingedEntity(entity, PingType);
		}
	}

	public override bool IsEntityValidForObjective<T>(T entity)
	{
		if (!(entity is BaseCombatEntity baseCombatEntity))
		{
			return false;
		}
		if (!TargetOven.isValid)
		{
			return false;
		}
		BaseEntity baseEntity = TargetOven.Get();
		if ((Object)(object)baseEntity == (Object)null)
		{
			return false;
		}
		if (!baseCombatEntity.IsAlive())
		{
			return false;
		}
		return entity.prefabID == baseEntity.prefabID;
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.STARTOVEN || IsCompleted(index, instance) || !CanProgress(index, instance))
		{
			return;
		}
		if (TargetOven.resourceID == payload.UintIdentifier)
		{
			CompleteObjective(index, instance, playerFor);
			if (PingTarget)
			{
				playerFor.DeregisterPingedEntity(payload.NetworkIdentifier, PingType);
			}
		}
		playerFor.MissionsDirty(saveImmediately: true);
	}

	public MissionObjective_IgniteOven()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		targetLayerMask = LayerMask.op_Implicit(-1);
		pingType = BasePlayer.PingType.GoTo;
		base._002Ector();
	}
}
