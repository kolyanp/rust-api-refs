using System;
using ConVar;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_AnimalBite : State_AnimalChase
{
	[SerializeField]
	public AnimationClip Animation;

	[SerializeField]
	public float Damage = 20f;

	[SerializeField]
	public DamageType DamageType = DamageType.Bite;

	[SerializeField]
	[Tooltip("How long to hold the state when there is no clip to play.")]
	public float FallbackDuration = 1f;

	[Tooltip("Rate the clip plays at. Must match the m_Speed on the animator state of the same name, because the client reads that one and the server reads this one.")]
	[SerializeField]
	public float PlaybackSpeed = 1f;

	[SerializeField]
	[Range(0f, 0.9f)]
	[Tooltip("Where in the bite the damage lands, as a fraction of the clip. A fraction rather than seconds so it stays on the same frame of the animation whatever length the clip is and whatever rate PlaybackSpeed runs it at.")]
	public float DamageAtClipFraction = 0.35f;

	[NonSerialized]
	public float Range;

	[NonSerialized]
	public Vector3 AttackOffset;

	private Action _doDamageAction;

	private RootMotionPlayer.PlayServerState animState;

	private float remainingFallback;

	private Action DoDamageAction => DoDamage;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		if (!Senses.FindTarget(out var _))
		{
			return EFSMStateStatus.Failure;
		}
		ScheduleDamage();
		PlayMontage();
		return base.OnStateEnter(payload);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		if (base.OnStateUpdate(deltaTime) == EFSMStateStatus.Failure)
		{
			return EFSMStateStatus.Failure;
		}
		if (animState != null)
		{
			if (!animState.isPlaying)
			{
				return EFSMStateStatus.Success;
			}
			return EFSMStateStatus.None;
		}
		remainingFallback -= deltaTime;
		if (!(remainingFallback > 0f))
		{
			return EFSMStateStatus.Success;
		}
		return EFSMStateStatus.None;
	}

	public override void OnStateExit()
	{
		Owner.CancelInvoke(DoDamageAction);
		AnimPlayer.StopServerAndReturnToPool(ref animState);
		base.OnStateExit();
	}

	private void PlayMontage()
	{
		if ((Object)(object)Animation == (Object)null)
		{
			animState = null;
			remainingFallback = FallbackDuration;
		}
		else
		{
			animState = AnimPlayer.PlayServerAndTakeFromPool(Animation, PlaybackSpeed, pauseAgent: false);
			remainingFallback = 0f;
		}
	}

	private void ScheduleDamage()
	{
		if ((Object)(object)Animation == (Object)null || DamageAtClipFraction <= 0f)
		{
			DoDamage();
			return;
		}
		float num = Animation.length / ((PlaybackSpeed > 0f) ? PlaybackSpeed : 1f);
		float num2 = num * DamageAtClipFraction + AI.defaultInterpolationDelay;
		Owner.Invoke(DoDamageAction, Mathf.Min(num2, Mathf.Max(0f, num - 0.25f)));
	}

	private void DoDamage()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (Senses.FindTarget(out var target) && target is BaseCombatEntity baseCombatEntity)
		{
			Vector3 val = Owner.ServerPosition + ((Component)Owner).transform.TransformDirection(AttackOffset);
			if (!(Range > 0f) || !(Vector3.Distance(((Component)target).transform.position, val) > Range))
			{
				baseCombatEntity.OnAttacked(Damage, DamageType, Owner, ignoreShield: false);
			}
		}
	}
}
