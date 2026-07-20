using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_AttackUnreachable : FSMStateBase
{
	private enum Phase
	{
		PreJump,
		Jump,
		Attack,
		JumpBack,
		PostJumpBack
	}

	public float preJumpEnd = 0.29f;

	public float jumpEnd = 0.395f;

	public float attackEnd = 0.67f;

	public float jumpBackEnd = 0.765f;

	public float postJumpBackEnd = 0.95f;

	private const float groundCheckDistance = 2f;

	private const float damage = 35f;

	private const float meleeAttackRange = 1.7f;

	private const DamageType damageType = DamageType.Bite;

	public RootMotionData animClip;

	private Vector3 startLocation;

	private Quaternion startRotation;

	private Vector3 destination;

	private float elapsedTime;

	private LockState.LockHandle targetLock;

	private Phase phase;

	private float previousOffsetZ;

	private RootMotionPlayer.PlayServerState animState;

	public static bool SampleGroundPositionUnderTarget(RustNavMeshAgent agent, BasePlayer targetAsPlayer, out Vector3 projectedLocation)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		float radius = BasePlayer.GetRadius();
		RaycastHit hitInfoNS;
		bool result = agent.SampleGroundPositionWithPhysics(((Component)targetAsPlayer).transform.position, out hitInfoNS, 2f, radius);
		projectedLocation = ((RaycastHit)(ref hitInfoNS)).point;
		return result;
	}

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTarget(out var target) || !(target is BasePlayer basePlayer))
		{
			return EFSMStateStatus.Failure;
		}
		destination = ((Component)target).transform.position;
		if (!basePlayer.IsOnGround() && !SampleGroundPositionUnderTarget(base.Agent, basePlayer, out destination))
		{
			return EFSMStateStatus.Failure;
		}
		if (!State_MoveToLastReachablePointNearTarget.CanJumpFromPosToPos(Owner, ((Component)Owner).transform.position, destination))
		{
			return EFSMStateStatus.Failure;
		}
		base.Agent.Pause(this);
		elapsedTime = 0f;
		targetLock = base.Senses.LockCurrentTarget();
		animState = base.AnimPlayer.PlayServerAndTakeFromPool(animClip.inPlaceAnimation);
		SetPhase(Phase.PreJump);
		return base.OnStateEnter(payload);
	}

	private void SetPhase(Phase newPhase)
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		phase = newPhase;
		previousOffsetZ = animClip.zMotionCurve.Evaluate(elapsedTime);
		if (phase == Phase.Jump)
		{
			if (base.Senses.FindTarget(out var target) && target is BasePlayer targetAsPlayer)
			{
				SampleGroundPositionUnderTarget(base.Agent, targetAsPlayer, out destination);
			}
			startLocation = ((Component)Owner).transform.position;
			((Component)Owner).transform.rotation = Quaternion.LookRotation(Vector3Ex.WithY(destination - ((Component)Owner).transform.position, 0f));
			base.Agent.IsJumping = true;
		}
		else if (phase == Phase.Attack)
		{
			startRotation = ((Component)Owner).transform.rotation;
			if (base.Senses.FindTarget(out var target2))
			{
				if (target2 is BaseCombatEntity baseCombatEntity && Vector3.Distance(((Component)Owner).transform.position, ((Component)baseCombatEntity).transform.position) <= 1.7f)
				{
					baseCombatEntity.OnAttacked(35f, DamageType.Bite, Owner, ignoreShield: false);
				}
				if (target2 is BasePlayer basePlayer && Vector3.Distance(((Component)Owner).transform.position, ((Component)basePlayer).transform.position) <= 1f)
				{
					basePlayer.DoPush(((Component)Owner).transform.forward * 10f + Vector3.up * 3f);
				}
			}
		}
		else if (phase == Phase.PostJumpBack)
		{
			base.Agent.IsJumping = false;
		}
	}

	private Vector3 ThreePointLerp(Vector3 a, Vector3 b, Vector3 c, float t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Lerp(Vector3.Lerp(a, b, t), Vector3.Lerp(b, c, t), t);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		elapsedTime += deltaTime;
		float num = elapsedTime / Mathf.Max(animClip.inPlaceAnimation.length, 0.001f);
		if (phase == Phase.PreJump)
		{
			Quaternion val = Quaternion.LookRotation(Vector3Ex.WithY(destination - ((Component)Owner).transform.position, 0f));
			((Component)Owner).transform.rotation = Quaternion.Slerp(((Component)Owner).transform.rotation, val, 2f * deltaTime);
			float num2 = animClip.zMotionCurve.Evaluate(elapsedTime);
			Vector3 val2 = ((Component)Owner).transform.forward * (num2 - previousOffsetZ);
			previousOffsetZ = num2;
			Transform transform = ((Component)Owner).transform;
			transform.position += val2;
			if (num >= preJumpEnd)
			{
				SetPhase(Phase.Jump);
			}
		}
		if (phase == Phase.Jump)
		{
			Vector3 b = (startLocation + destination) * 0.5f;
			b.y = Mathf.Max(startLocation.y, destination.y);
			float t = Mathx.RemapValClamped(num, preJumpEnd, jumpEnd, 0f, 1f);
			Vector3 position = ThreePointLerp(startLocation, b, destination, t);
			((Component)Owner).transform.position = position;
			if (num >= jumpEnd)
			{
				SetPhase(Phase.Attack);
			}
		}
		if (phase == Phase.Attack)
		{
			((Component)Owner).transform.rotation = startRotation * Quaternion.AngleAxis(animClip.yRotationCurve.Evaluate(elapsedTime), Vector3.up);
			if (num > attackEnd)
			{
				SetPhase(Phase.JumpBack);
			}
		}
		if (phase == Phase.JumpBack)
		{
			Vector3 b2 = (startLocation + destination) * 0.5f;
			b2.y = Mathf.Max(startLocation.y, destination.y);
			float t2 = Mathx.RemapValClamped(num, attackEnd, jumpBackEnd, 0f, 1f);
			Vector3 position2 = ThreePointLerp(destination, b2, startLocation, t2);
			((Component)Owner).transform.position = position2;
			((Component)Owner).transform.rotation = Quaternion.LookRotation(Vector3Ex.WithY(startLocation - destination, 0f));
			if (num >= jumpBackEnd)
			{
				SetPhase(Phase.PostJumpBack);
			}
		}
		if (phase == Phase.PostJumpBack)
		{
			float num3 = animClip.zMotionCurve.Evaluate(elapsedTime);
			Vector3 val3 = ((Component)Owner).transform.forward * (num3 - previousOffsetZ);
			previousOffsetZ = num3;
			Transform transform2 = ((Component)Owner).transform;
			transform2.position -= val3;
		}
		if (num >= postJumpBackEnd)
		{
			return EFSMStateStatus.Success;
		}
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		base.AnimPlayer.StopServerAndReturnToPool(ref animState);
		base.Senses.UnlockTarget(ref targetLock);
		base.Agent.Unpause(this);
		if (phase != Phase.PostJumpBack)
		{
			base.Agent.IsJumping = false;
		}
		base.OnStateExit();
	}
}
