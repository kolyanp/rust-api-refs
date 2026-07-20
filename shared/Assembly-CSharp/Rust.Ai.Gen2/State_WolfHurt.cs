using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_WolfHurt : State_Hurt
{
	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		EFSMStateStatus result = base.OnStateEnter(payload);
		if (!base.Senses.FindTarget(out var target))
		{
			if (AI.logIssues)
			{
				Debug.LogWarning((object)"Got attacked but couldn't find a target");
			}
			return result;
		}
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			base.Senses.GetInitialAllies((List<BaseEntity>)(object)val);
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				((Component)item).GetComponent<Wolf2FSM>().Intimidate(target);
			}
			return result;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
