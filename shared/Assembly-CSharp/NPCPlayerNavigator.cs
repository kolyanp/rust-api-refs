using Rust.Ai.Gen2;
using UnityEngine;

public class NPCPlayerNavigator : BaseNavigator
{
	public NPCPlayer NPCPlayerEntity { get; private set; }

	public override void Init(BaseCombatEntity entity, RustNavMeshAgent agent)
	{
		base.Init(entity, agent);
		NPCPlayerEntity = entity as NPCPlayer;
	}

	protected override bool CanEnableNavMeshNavigation()
	{
		if (!base.CanEnableNavMeshNavigation())
		{
			return false;
		}
		if (NPCPlayerEntity.isMounted && !CanNavigateMounted)
		{
			return false;
		}
		return true;
	}

	protected override bool CanUpdateMovement()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!base.CanUpdateMovement())
		{
			return false;
		}
		if (NPCPlayerEntity.IsWounded())
		{
			return false;
		}
		if (base.CurrentNavigationType == NavigationType.NavMesh && (NPCPlayerEntity.IsDormant || !NPCPlayerEntity.syncPosition) && ((Behaviour)base.Agent).enabled)
		{
			SetDestination(NPCPlayerEntity.ServerPosition);
			return false;
		}
		return true;
	}

	protected override void UpdatePositionAndRotation(Vector3 moveToPosition, float delta)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		base.UpdatePositionAndRotation(moveToPosition, delta);
		if (overrideFacingDirectionMode == OverrideFacingDirectionMode.None)
		{
			if (base.CurrentNavigationType == NavigationType.NavMesh)
			{
				NPCPlayer nPCPlayerEntity = NPCPlayerEntity;
				Vector3 desiredVelocityWS = base.Agent.desiredVelocityWS;
				nPCPlayerEntity.SetAimDirection(((Vector3)(ref desiredVelocityWS)).normalized);
			}
			else if (base.CurrentNavigationType == NavigationType.AStar || base.CurrentNavigationType == NavigationType.Base)
			{
				NPCPlayerEntity.SetAimDirection(Vector3Ex.Direction2D(moveToPosition, ((Component)this).transform.position));
			}
		}
	}

	public override void ApplyFacingDirectionOverride()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		base.ApplyFacingDirectionOverride();
		if (overrideFacingDirectionMode != OverrideFacingDirectionMode.None && !ObjectEx.IsUnityNull(NPCPlayerEntity))
		{
			if (overrideFacingDirectionMode == OverrideFacingDirectionMode.Direction)
			{
				NPCPlayerEntity.SetAimDirection(facingDirectionOverride);
			}
			else if ((Object)(object)facingDirectionEntity != (Object)null)
			{
				Vector3 aimDirection = GetAimDirection(NPCPlayerEntity, facingDirectionEntity);
				facingDirectionOverride = aimDirection;
				NPCPlayerEntity.SetAimDirection(facingDirectionOverride);
			}
		}
	}

	private static Vector3 GetAimDirection(BasePlayer aimingPlayer, BaseEntity target)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (ObjectEx.IsUnityNull(target))
		{
			return Vector3Ex.Direction2D(((Component)aimingPlayer).transform.position + (((Object)(object)aimingPlayer.eyes != (Object)null) ? aimingPlayer.eyes.BodyForward() : ((Component)aimingPlayer).transform.forward) * 1000f, ((Component)aimingPlayer).transform.position);
		}
		if (Vector3Ex.Distance2D(((Component)aimingPlayer).transform.position, ((Component)target).transform.position) <= 0.75f)
		{
			return Vector3Ex.Direction2D(((Component)target).transform.position, ((Component)aimingPlayer).transform.position);
		}
		Vector3 val = TargetAimPositionOffset(target) - (((Object)(object)aimingPlayer.eyes != (Object)null) ? aimingPlayer.eyes.position : ((Component)aimingPlayer).transform.position);
		return ((Vector3)(ref val)).normalized;
	}

	private static Vector3 TargetAimPositionOffset(BaseEntity target)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = target as BasePlayer;
		if ((Object)(object)basePlayer != (Object)null)
		{
			if (basePlayer.IsSleeping() || basePlayer.IsWounded())
			{
				return ((Component)basePlayer).transform.position + Vector3.up * 0.1f;
			}
			if ((Object)(object)basePlayer.eyes != (Object)null)
			{
				return basePlayer.eyes.position - Vector3.up * 0.15f;
			}
		}
		return target.CenterPoint();
	}
}
