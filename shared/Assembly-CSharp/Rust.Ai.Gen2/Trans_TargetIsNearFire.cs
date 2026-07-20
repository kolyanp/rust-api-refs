using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_TargetIsNearFire : FSMTransitionBase
{
	public bool onlySeeFireWhenClose;

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		using (TimeWarning.New("Trans_TargetIsNearFire"))
		{
			return Test(Owner, base.Senses, onlySeeFireWhenClose);
		}
	}

	public static bool Test(BaseEntity owner, SenseComponent senses, bool onlySeeFireWhenClose = false)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Test"))
		{
			if (!senses.FindTarget(out var target))
			{
				return false;
			}
			if (target.ToNonNpcPlayer(out var player) && SingletonComponent<NpcNoiseManager>.Instance.HasPlayerSpokenNear(owner, player))
			{
				return true;
			}
			if (!senses.FindFire(out var fire))
			{
				return false;
			}
			bool flag = Vector3.Distance(((Component)target).transform.position, ((Component)fire).transform.position) < 16f;
			bool flag2 = Vector3.Distance(((Component)owner).transform.position, ((Component)target).transform.position) < 18f;
			if (onlySeeFireWhenClose)
			{
				return flag && flag2;
			}
			return flag;
		}
	}
}
