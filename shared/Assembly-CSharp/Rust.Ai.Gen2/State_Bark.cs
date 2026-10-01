using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Bark : State_PlayAnimation
{
	public const string WolfNearbyAlreadyBarked = "WolfNearbyAlreadyBarked";

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		if (!Senses.FindTarget(out var targetEntity))
		{
			return EFSMStateStatus.Failure;
		}
		Blackboard.Add("WolfNearbyAlreadyBarked");
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			Senses.GetInitialAllies((List<BaseEntity>)(object)val);
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				((Component)item).GetComponent<BlackboardComponent>().Add("WolfNearbyAlreadyBarked");
				Wolf2FSM otherWolf = ((Component)item).GetComponent<Wolf2FSM>();
				Owner.Invoke(() =>
				{
					otherWolf.Bark(targetEntity);
				}, Mathf.Max(0f, Animation.length + Random.Range(-0.5f, 0.5f)));
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return base.OnStateEnter(payload);
	}
}
