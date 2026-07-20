using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_AttackUnreachableWarped : FSMStateBase
{
	private enum Phase
	{
		PreJump,
		Jump,
		Attack,
		JumpBack,
		PostJumpBack
	}

	public float jumpOnStart = 1.03f;

	public float jumpOnEnd = 1.63f;

	public float jumpOffStart = 2.9f;

	public float jumpOffEnd = 3.47f;

	public float totalDuration = 3.6f;

	private const float groundCheckDistance = 2f;

	private const float damage = 35f;

	private const float meleeAttackRange = 1.7f;

	private const DamageType damageType = DamageType.Bite;

	public RootMotionData animClip;

	private float elapsedTime;

	private LockState.LockHandle targetLock;

	private Phase phase;

	private RootMotionPlayer.Warp[] warps = new RootMotionPlayer.Warp[2];

	private RootMotionPlayer.PlayServerState animState;

	private Vector3 destination;

	private bool didHit;

	public static bool SampleGroundPositionUnderTarget(RustNavMeshAgent agent, BasePlayer targetAsPlayer, out Vector3 projectedLocation)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (targetAsPlayer.IsOnGround() && !targetAsPlayer.OnLadder())
		{
			projectedLocation = ((Component)targetAsPlayer).transform.position;
			return true;
		}
		float radius = BasePlayer.GetRadius();
		RaycastHit hitInfoNS;
		bool result = agent.SampleGroundPositionWithPhysics(((Component)targetAsPlayer).transform.position, out hitInfoNS, 2f, radius * 0.5f);
		projectedLocation = ((RaycastHit)(ref hitInfoNS)).point;
		return result;
	}

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTarget(out var target) || !(target is BasePlayer targetAsPlayer))
		{
			return EFSMStateStatus.Failure;
		}
		destination = ((Component)target).transform.position;
		if (!SampleGroundPositionUnderTarget(base.Agent, targetAsPlayer, out destination))
		{
			return EFSMStateStatus.Failure;
		}
		if (!State_MoveToLastReachablePointNearTarget.CanJumpFromPosToPos(Owner, ((Component)Owner).transform.position, destination))
		{
			return EFSMStateStatus.Failure;
		}
		base.Agent.Pause(this);
		didHit = false;
		elapsedTime = 0f;
		targetLock = base.Senses.LockCurrentTarget();
		animState = RootMotionPlayer.PlayServerState.TakeFromPool(animClip, ((Component)Owner).transform);
		animState.warps = warps;
		animState.constrainToNavmesh = false;
		base.AnimPlayer.PlayServer(animState);
		SetPhase(Phase.PreJump);
		return base.OnStateEnter(payload);
	}

	private EFSMStateStatus SetPhase(Phase newPhase)
	{
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		phase = newPhase;
		if (phase == Phase.Jump)
		{
			if (!base.Senses.FindTarget(out var target) || !(target is BasePlayer targetAsPlayer))
			{
				return EFSMStateStatus.Failure;
			}
			if (SampleGroundPositionUnderTarget(base.Agent, targetAsPlayer, out var projectedLocation))
			{
				destination = projectedLocation;
			}
			Vector3 position = ((Component)Owner).transform.position;
			animState.initialRotation = Quaternion.LookRotation(Vector3Ex.WithY(destination - ((Component)Owner).transform.position, 0f));
			((Component)Owner).transform.rotation = animState.initialRotation;
			float num = animClip.zMotionCurve.Evaluate(jumpOnEnd) - animClip.zMotionCurve.Evaluate(jumpOnStart);
			float num2 = animClip.yMotionCurve.Evaluate(jumpOnEnd) - animClip.yMotionCurve.Evaluate(jumpOnStart);
			RootMotionPlayer.Warp warp = new RootMotionPlayer.Warp(jumpOnStart, jumpOnEnd, Vector3.one);
			warp.translationScale.z = Vector3.Distance(Vector3Ex.WithY(destination, 0f), Vector3Ex.WithY(position, 0f)) / num;
			warp.translationScale.y = (destination.y - position.y) / num2;
			warps[0] = warp;
			float num3 = animClip.zMotionCurve.Evaluate(jumpOffStart) - animClip.zMotionCurve.Evaluate(jumpOffEnd);
			float num4 = animClip.yMotionCurve.Evaluate(jumpOffStart) - animClip.yMotionCurve.Evaluate(jumpOffEnd);
			RootMotionPlayer.Warp warp2 = new RootMotionPlayer.Warp(jumpOffStart, jumpOffEnd, Vector3.one);
			warp2.translationScale.z = Vector3.Distance(Vector3Ex.WithY(destination, 0f), Vector3Ex.WithY(position, 0f)) / num3;
			warp2.translationScale.y = (destination.y - position.y) / num4;
			warps[1] = warp2;
			base.Agent.IsJumping = true;
		}
		else if (phase == Phase.Attack)
		{
			if (base.Senses.FindTarget(out var target2))
			{
				if (target2 is BaseCombatEntity baseCombatEntity && Vector3.Distance(((Component)Owner).transform.position, ((Component)baseCombatEntity).transform.position) <= 1.7f)
				{
					didHit = true;
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
		return EFSMStateStatus.None;
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		elapsedTime += deltaTime;
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Failure;
		}
		if (phase == Phase.PreJump)
		{
			Quaternion val = Quaternion.LookRotation(Vector3Ex.WithY(targetPosition - ((Component)Owner).transform.position, 0f));
			animState.initialRotation = Quaternion.RotateTowards(animState.initialRotation, val, Time.deltaTime * 60f);
			((Component)Owner).transform.rotation = animState.initialRotation;
			if (elapsedTime >= jumpOnStart && SetPhase(Phase.Jump) == EFSMStateStatus.Failure)
			{
				return EFSMStateStatus.Failure;
			}
		}
		if (phase == Phase.Jump && elapsedTime >= jumpOnEnd)
		{
			SetPhase(Phase.Attack);
		}
		if (phase == Phase.Attack && elapsedTime > jumpOffStart)
		{
			SetPhase(Phase.JumpBack);
		}
		if (phase == Phase.JumpBack && elapsedTime >= jumpOffEnd)
		{
			SetPhase(Phase.PostJumpBack);
		}
		if (elapsedTime >= animClip.inPlaceAnimation.length - 0.25f)
		{
			if (!didHit)
			{
				return EFSMStateStatus.Failure;
			}
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
