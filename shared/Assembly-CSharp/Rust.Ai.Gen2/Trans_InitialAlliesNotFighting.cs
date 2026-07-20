using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_InitialAlliesNotFighting : FSMTransitionBase
{
	[SerializeField]
	public float MinAllyHealthFraction = 0.3f;

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		using (TimeWarning.New("Trans_InitialAlliesNotFighting"))
		{
			PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
			try
			{
				base.Senses.GetInitialAllies((List<BaseEntity>)(object)val);
				foreach (BaseEntity item in (List<BaseEntity>)(object)val)
				{
					if (!((Component)item).GetComponent<SenseComponent>().FindTarget(out var _) && (!(item is BaseCombatEntity baseCombatEntity) || !(baseCombatEntity.healthFraction < MinAllyHealthFraction)))
					{
						return true;
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			return false;
		}
	}
}
