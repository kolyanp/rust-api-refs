using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

public class State_MoveToLastReachablePointNearTarget : State_MoveToTarget
{
	private const float maxHorizontalDist = 7f;

	private const float maxVerticalDist = 2.7f;

	private const float traceVerticalOffset = 1f;

	private Vector3 reachableDestination;

	private LockState.LockHandle targetLock;

	public static bool CanJumpFromPosToPos(BaseEntity owner, Vector3 ownerLocation, Vector3 targetPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Abs(targetPos.y - ownerLocation.y) > 2.7f)
		{
			return false;
		}
		if (Vector3.Distance(ownerLocation, targetPos) > 7f)
		{
			return false;
		}
		if (!owner.CanSee(ownerLocation + 1f * Vector3.up, targetPos + 1f * Vector3.up))
		{
			return false;
		}
		return true;
	}

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		if (!FindReachableLocation(out reachableDestination))
		{
			return EFSMStateStatus.Failure;
		}
		targetLock = base.Senses.LockCurrentTarget();
		base.Agent.deceleration.Value = 6f;
		return base.OnStateEnter(payload);
	}

	private bool FindReachableLocation(out Vector3 location)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		location = default(Vector3);
		if (!base.Senses.FindTarget(out var target) || !(target is BasePlayer basePlayer))
		{
			return false;
		}
		if (basePlayer.isMounted)
		{
			return false;
		}
		Vector3 position = ((Component)target).transform.position;
		if (Vector3.Distance(((Component)Owner).transform.position, position) > 50f)
		{
			return false;
		}
		Vector3? val = null;
		if (base.Agent.lastValidPath.Count > 0)
		{
			List<Vector3> lastValidPath = base.Agent.lastValidPath;
			Vector3 val2 = lastValidPath[lastValidPath.Count - 1];
			if (Vector3.Distance(val2, position) <= 7f && base.Agent.SamplePosition(val2, out var hitNS, 7f) && CanJumpFromPosToPos(Owner, ((NavMeshHit)(ref hitNS)).position, position))
			{
				val = ((NavMeshHit)(ref hitNS)).position;
			}
		}
		if (!val.HasValue && base.Agent.SamplePosition(position, out var hitNS2, 7f) && CanJumpFromPosToPos(Owner, ((NavMeshHit)(ref hitNS2)).position, position))
		{
			val = ((NavMeshHit)(ref hitNS2)).position;
		}
		if (!val.HasValue)
		{
			return false;
		}
		location = val.Value;
		return true;
	}

	protected override bool GetMoveDestination(out Vector3 destination)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		destination = reachableDestination;
		return true;
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (Trans_TargetIsNearFire.Test(Owner, base.Senses))
		{
			float ratio = Mathx.RemapValClamped(Vector3.Distance(((Component)Owner).transform.position, reachableDestination), 4f, 16f, 0f, 1f);
			base.Agent.SetSpeedRatio(ratio, RustNavMeshAgent.Speeds.Sneak, RustNavMeshAgent.Speeds.Jog);
		}
		else
		{
			base.Agent.SetGait(speed);
		}
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		base.Senses.UnlockTarget(ref targetLock);
		base.Agent.deceleration.Reset();
	}
}
