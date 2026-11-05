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
		if (CurrentNavigationType == NavigationType.NavMesh && (NPCPlayerEntity.IsDormant || !NPCPlayerEntity.syncPosition) && ((Behaviour)Agent).enabled)
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
			if (CurrentNavigationType == NavigationType.NavMesh)
			{
				NPCPlayer nPCPlayerEntity = NPCPlayerEntity;
				Vector3 desiredVelocityWS = Agent.desiredVelocityWS;
				nPCPlayerEntity.SetAimDirection(desiredVelocityWS.normalized);
			}
			else if (CurrentNavigationType == NavigationType.AStar || CurrentNavigationType == NavigationType.Base)
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
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (ObjectEx.IsUnityNull(target))
		{
			return Vector3Ex.Direction2D(((Component)aimingPlayer).transform.position + ((aimingPlayer.eyes != null) ? aimingPlayer.eyes.BodyForward() : ((Component)aimingPlayer).transform.forward) * 1000f, ((Component)aimingPlayer).transform.position);
		}
		if (Vector3Ex.Distance2D(((Component)aimingPlayer).transform.position, ((Component)target).transform.position) <= 0.75f)
		{
			return Vector3Ex.Direction2D(((Component)target).transform.position, ((Component)aimingPlayer).transform.position);
		}
		Vector3 val = TargetAimPositionOffset(target) - ((aimingPlayer.eyes != null) ? aimingPlayer.eyes.position : ((Component)aimingPlayer).transform.position);
		return val.normalized;
	}

	private static Vector3 TargetAimPositionOffset(BaseEntity target)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = target as BasePlayer;
		if ((Object)(object)basePlayer != (Object)null)
		{
			if (basePlayer.IsSleeping() || basePlayer.IsWounded())
			{
				return ((Component)basePlayer).transform.position + Vector3.up * 0.1f;
			}
			if (basePlayer.eyes != null)
			{
				return basePlayer.eyes.position - Vector3.up * 0.15f;
			}
		}
		return target.CenterPoint();
	}
}
