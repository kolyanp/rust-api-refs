using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_BringFoodBackToWater : State_GoBackToWater
{
	private BaseCorpse corpse;

	private Action _updateCorpsePositionAction;

	private Action UpdateCorpsePositionAction => OnStateFixedUpdate;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindFood(out var food))
		{
			return EFSMStateStatus.Failure;
		}
		if (!(food is BaseCorpse baseCorpse))
		{
			return EFSMStateStatus.Failure;
		}
		if (!SingletonComponent<NpcFoodManager>.Instance.Remove(baseCorpse))
		{
			return EFSMStateStatus.Failure;
		}
		((Component)Owner).transform.forward = Vector3Ex.NormalizeXZ(((Component)baseCorpse).transform.position - ((Component)Owner).transform.position);
		corpse = baseCorpse;
		using (BaseEntity.FlagsUpdateScope flagsUpdateScope = corpse.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(BaseEntity.Flags.Reserved3, b: true);
		}
		Owner.InvokeRepeatingFixedTime(UpdateCorpsePositionAction);
		return base.OnStateEnter(payload);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		if (!corpse.IsValid() || corpse.IsDead())
		{
			return EFSMStateStatus.Failure;
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
		if (corpse.IsValid())
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = corpse.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Reserved3, b: false);
		}
		Owner.CancelInvokeFixedTime(UpdateCorpsePositionAction);
		corpse = null;
	}
}
