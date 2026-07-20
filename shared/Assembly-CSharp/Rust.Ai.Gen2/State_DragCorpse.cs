using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_DragCorpse : FSMStateBase
{
	[SerializeField]
	protected RootMotionData Animation;

	private const int numLoops = 6;

	private int currentLoop;

	private BaseCorpse corpse;

	private RootMotionPlayer.PlayServerState animState;

	private Action _updateCorpsePositionAction;

	private Action UpdateCorpsePositionAction => OnStateFixedUpdate;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		if (!base.Senses.FindFood(out var food))
		{
			return EFSMStateStatus.Failure;
		}
		if (!(food is BaseCorpse food2))
		{
			return EFSMStateStatus.Failure;
		}
		if (!base.Senses.FindTarget(out var _))
		{
			return EFSMStateStatus.Failure;
		}
		if (!SingletonComponent<NpcFoodManager>.Instance.Remove(food2))
		{
			return EFSMStateStatus.Failure;
		}
		corpse = food2;
		using (BaseEntity.FlagsUpdateScope flagsUpdateScope = corpse.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(BaseEntity.Flags.Reserved3, b: true);
		}
		Owner.InvokeRepeatingFixedTime(UpdateCorpsePositionAction);
		animState = base.AnimPlayer.PlayServerAndTakeFromPool(Animation);
		return base.OnStateEnter(payload);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (!corpse.IsValid() || corpse.IsDead())
		{
			return EFSMStateStatus.Failure;
		}
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Failure;
		}
		if (animState != null)
		{
			Quaternion val = Quaternion.LookRotation(Vector3Ex.WithY(targetPosition - ((Component)Owner).transform.position, 0f));
			animState.initialRotation = Quaternion.RotateTowards(animState.initialRotation, val, Time.deltaTime * 60f);
			((Component)Owner).transform.rotation = animState.initialRotation;
		}
		if (!animState.isPlaying)
		{
			currentLoop++;
			if (currentLoop >= 6)
			{
				return EFSMStateStatus.Success;
			}
			base.AnimPlayer.StopServerAndReturnToPool(ref animState, interrupt: false);
			animState = base.AnimPlayer.PlayServerAndTakeFromPool(Animation);
		}
		return base.OnStateUpdate(deltaTime);
	}

	private void OnStateFixedUpdate()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (corpse.IsValid() && !corpse.IsDead())
		{
			Rigidbody component = ((Component)corpse).GetComponent<Rigidbody>();
			if (component != null)
			{
				component.MovePosition(((Component)Owner).transform.position + ((Component)Owner).transform.forward * 1.6f + ((Component)Owner).transform.up * 0.6f);
				component.linearVelocity = Vector3.zero;
				component.angularVelocity = Vector3.zero;
			}
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		base.AnimPlayer.StopServerAndReturnToPool(ref animState);
		currentLoop = 0;
		if (corpse.IsValid())
		{
			SingletonComponent<NpcFoodManager>.Instance.Add(corpse);
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = corpse.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Reserved3, b: false);
		}
		corpse = null;
		Owner.CancelInvokeFixedTime(UpdateCorpsePositionAction);
	}
}
