using System.Collections.Generic;
using Rust.Ai.Gen2.Nav;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

public class State_MoveToLastReachablePointNearTarget : State_MoveToTarget
{
	private const float maxHorizontalDist = 7f;

	private const float projectSampleRadius = 2f;

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
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
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
			RustNavMeshAgent agent = base.Agent;
			List<NavVector3> lastValidPath = base.Agent.lastValidPath;
			Vector3 val2 = agent.NavToWorldSpace(lastValidPath[lastValidPath.Count - 1]);
			if (Vector3.Distance(val2, position) <= 7f && base.Agent.SamplePosition(val2, out var hitWS, 2f) && CanJumpFromPosToPos(Owner, ((NavMeshHit)(ref hitWS)).position, position))
			{
				val = ((NavMeshHit)(ref hitWS)).position;
			}
		}
		if (!val.HasValue && base.Agent.SamplePosition(position, out var hitWS2, 2f) && CanJumpFromPosToPos(Owner, ((NavMeshHit)(ref hitWS2)).position, position))
		{
			val = ((NavMeshHit)(ref hitWS2)).position;
		}
		if (!val.HasValue && base.Agent.lastValidPath.Count > 0)
		{
			List<NavVector3> lastValidPath2 = base.Agent.lastValidPath;
			NavVector3 positionNS = lastValidPath2[lastValidPath2.Count - 1];
			float num = 3f;
			int num2 = base.Agent.lastValidPath.Count - 1;
			while (num2 > 0 && num > 0f)
			{
				float num3 = NavVector3.Distance(base.Agent.lastValidPath[num2], base.Agent.lastValidPath[num2 - 1]);
				if (num3 >= num)
				{
					positionNS = NavVector3.MoveTowards(base.Agent.lastValidPath[num2], base.Agent.lastValidPath[num2 - 1], num);
					num = 0f;
				}
				else
				{
					positionNS = base.Agent.lastValidPath[num2 - 1];
					num -= num3;
				}
				num2--;
			}
			if (base.Agent.SamplePosition(base.Agent.NavToWorldSpace(positionNS), out var hitWS3, 2f) && CanJumpFromPosToPos(Owner, ((NavMeshHit)(ref hitWS3)).position, position))
			{
				val = ((NavMeshHit)(ref hitWS3)).position;
			}
		}
		if (!val.HasValue)
		{
			Vector3 val3 = Vector3Ex.WithY(((Component)Owner).transform.position - position, 0f);
			Vector3 val4 = ((Vector3)(ref val3)).normalized;
			if (((Vector3)(ref val4)).sqrMagnitude < 0.01f)
			{
				val4 = -((Component)Owner).transform.forward;
			}
			if (base.Agent.SamplePosition(position + val4 * 4.5f, out var hitWS4, 2f) && CanJumpFromPosToPos(Owner, ((NavMeshHit)(ref hitWS4)).position, position))
			{
				val = ((NavMeshHit)(ref hitWS4)).position;
			}
		}
		if (!val.HasValue)
		{
			return false;
		}
		location = val.Value;
		return true;
	}

	protected override bool GetMoveDestination(out NavVector3 destination)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		destination = base.Agent.WorldToNavSpace(reachableDestination);
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
