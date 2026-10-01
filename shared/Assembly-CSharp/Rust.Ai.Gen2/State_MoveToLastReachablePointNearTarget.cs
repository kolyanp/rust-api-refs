using System.Collections.Generic;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

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

	private bool IsJumpSpot(Vector3 spot, Vector3 targetPos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (CanJumpFromPosToPos(Owner, spot, targetPos))
		{
			return Agent.CanReach(spot);
		}
		return false;
	}

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		if (!FindReachableLocation(out reachableDestination))
		{
			return EFSMStateStatus.Failure;
		}
		targetLock = Senses.LockCurrentTarget();
		Agent.deceleration.Value = 6f;
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
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		location = default;
		if (!Senses.FindTarget(out var target) || !(target is BasePlayer basePlayer))
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
		if (Agent.lastValidPath.Count > 0)
		{
			RustNavMeshAgent agent = Agent;
			List<NavVector3> lastValidPath = Agent.lastValidPath;
			Vector3 val2 = agent.NavToWorldSpace(lastValidPath[lastValidPath.Count - 1]);
			if (Vector3.Distance(val2, position) <= 7f && Agent.SamplePosition(val2, out var hitWS, 2f) && IsJumpSpot(hitWS.position, position))
			{
				val = hitWS.position;
			}
		}
		if (!val.HasValue && Agent.SamplePosition(position, out var hitWS2, 2f) && IsJumpSpot(hitWS2.position, position))
		{
			val = hitWS2.position;
		}
		if (!val.HasValue && Agent.lastValidPath.Count > 0)
		{
			List<NavVector3> lastValidPath2 = Agent.lastValidPath;
			NavVector3 positionNS = lastValidPath2[lastValidPath2.Count - 1];
			float num = 3f;
			int num2 = Agent.lastValidPath.Count - 1;
			while (num2 > 0 && num > 0f)
			{
				float num3 = NavVector3.Distance(Agent.lastValidPath[num2], Agent.lastValidPath[num2 - 1]);
				if (num3 >= num)
				{
					positionNS = NavVector3.MoveTowards(Agent.lastValidPath[num2], Agent.lastValidPath[num2 - 1], num);
					num = 0f;
				}
				else
				{
					positionNS = Agent.lastValidPath[num2 - 1];
					num -= num3;
				}
				num2--;
			}
			if (Agent.SamplePosition(Agent.NavToWorldSpace(positionNS), out var hitWS3, 2f) && IsJumpSpot(hitWS3.position, position))
			{
				val = hitWS3.position;
			}
		}
		if (!val.HasValue)
		{
			Vector3 val3 = Vector3Ex.WithY(((Component)Owner).transform.position - position, 0f);
			Vector3 val4 = val3.normalized;
			if (val4.sqrMagnitude < 0.01f)
			{
				val4 = -((Component)Owner).transform.forward;
			}
			if (Agent.SamplePosition(position + val4 * 4.5f, out var hitWS4, 2f) && IsJumpSpot(hitWS4.position, position))
			{
				val = hitWS4.position;
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
		destination = Agent.WorldToNavSpace(reachableDestination);
		return true;
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (Trans_TargetIsNearFire.Test(Owner, Senses))
		{
			float ratio = Mathx.RemapValClamped(Vector3.Distance(((Component)Owner).transform.position, reachableDestination), 4f, 16f, 0f, 1f);
			Agent.SetSpeedRatio(ratio, RustNavMeshAgent.Speeds.Sneak, RustNavMeshAgent.Speeds.Jog);
		}
		else
		{
			Agent.SetGait(speed);
		}
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		Senses.UnlockTarget(ref targetLock);
		Agent.deceleration.Reset();
	}
}
