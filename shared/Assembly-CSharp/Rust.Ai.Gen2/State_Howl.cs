using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Howl : State_PlayAnimation
{
	public const string WolfNearbyAlreadyHowled = "WolfNearbyAlreadyHowled";

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTarget(out var targetEntity))
		{
			return EFSMStateStatus.Failure;
		}
		if (!base.Agent.CanReach(((Component)targetEntity).transform.position, updateLastValidPath: true))
		{
			base.Agent.ResetPath();
			return EFSMStateStatus.Failure;
		}
		base.Blackboard.Add("WolfNearbyAlreadyHowled");
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			base.Senses.GetInitialAllies((List<BaseEntity>)(object)val);
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				((Component)item).GetComponent<BlackboardComponent>().Add("WolfNearbyAlreadyHowled");
				Wolf2FSM otherWolf = ((Component)item).GetComponent<Wolf2FSM>();
				Owner.Invoke(delegate
				{
					otherWolf.Howl(targetEntity);
				}, 1f);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return base.OnStateEnter(payload);
	}
}
