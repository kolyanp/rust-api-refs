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
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		float radius = BasePlayer.GetRadius();
		bool result = agent.SampleGroundPositionWithPhysics(((Component)targetAsPlayer).transform.position, out var hitInfoNS, 2f, radius);
		projectedLocation = agent.NavToWorldSpace(hitInfoNS.point);
		return result;
	}

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (!Senses.FindTarget(out var target) || !(target is BasePlayer basePlayer))
		{
			return EFSMStateStatus.Failure;
		}
		destination = ((Component)target).transform.position;
		if (!basePlayer.IsOnGround() && !SampleGroundPositionUnderTarget(Agent, basePlayer, out destination))
		{
			return EFSMStateStatus.Failure;
		}
		if (!State_MoveToLastReachablePointNearTarget.CanJumpFromPosToPos(Owner, ((Component)Owner).transform.position, destination))
		{
			return EFSMStateStatus.Failure;
		}
		Agent.Pause(this);
		elapsedTime = 0f;
		targetLock = Senses.LockCurrentTarget();
		animState = AnimPlayer.PlayServerAndTakeFromPool(animClip.inPlaceAnimation);
		SetPhase(Phase.PreJump);
		return base.OnStateEnter(payload);
	}

	private void SetPhase(Phase newPhase)
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		phase = newPhase;
		previousOffsetZ = animClip.zMotionCurve.Evaluate(elapsedTime);
		if (phase == Phase.Jump)
		{
			if (Senses.FindTarget(out var target) && target is BasePlayer targetAsPlayer)
			{
				SampleGroundPositionUnderTarget(Agent, targetAsPlayer, out destination);
			}
			startLocation = ((Component)Owner).transform.position;
			Agent.Face(Quaternion.LookRotation(Vector3Ex.WithY(destination - ((Component)Owner).transform.position, 0f)));
			Agent.IsJumping = true;
		}
		else if (phase == Phase.Attack)
		{
			startRotation = ((Component)Owner).transform.rotation;
			if (Senses.FindTarget(out var target2))
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
			Agent.IsJumping = false;
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
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		elapsedTime += deltaTime;
		float num = elapsedTime / Mathf.Max(animClip.inPlaceAnimation.length, 0.001f);
		if (phase == Phase.PreJump)
		{
			Quaternion val = Quaternion.LookRotation(Vector3Ex.WithY(destination - ((Component)Owner).transform.position, 0f));
			Agent.Face(Quaternion.Slerp(((Component)Owner).transform.rotation, val, 2f * deltaTime));
			float num2 = animClip.zMotionCurve.Evaluate(elapsedTime);
			Vector3 val2 = ((Component)Owner).transform.forward * (num2 - previousOffsetZ);
			previousOffsetZ = num2;
			Agent.Place(((Component)Owner).transform.position + val2);
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
			Vector3 positionWS = ThreePointLerp(startLocation, b, destination, t);
			Agent.Place(positionWS);
			if (num >= jumpEnd)
			{
				SetPhase(Phase.Attack);
			}
		}
		if (phase == Phase.Attack)
		{
			Agent.Face(startRotation * Quaternion.AngleAxis(animClip.yRotationCurve.Evaluate(elapsedTime), Vector3.up));
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
			Vector3 positionWS2 = ThreePointLerp(destination, b2, startLocation, t2);
			Agent.Place(positionWS2, Quaternion.LookRotation(Vector3Ex.WithY(startLocation - destination, 0f)));
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
			Agent.Place(((Component)Owner).transform.position - val3);
		}
		if (num >= postJumpBackEnd)
		{
			return EFSMStateStatus.Success;
		}
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		AnimPlayer.StopServerAndReturnToPool(ref animState);
		Senses.UnlockTarget(ref targetLock);
		Agent.Unpause(this);
		if (phase != Phase.PostJumpBack)
		{
			Agent.IsJumping = false;
		}
		base.OnStateExit();
	}
}
