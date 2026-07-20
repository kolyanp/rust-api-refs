using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_IsInWater_Slow : FSMSlowTransitionBase
{
	protected override bool EvaluateAtInterval(ref FSMPayload payload)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_IsInWater_Slow"))
		{
			if (base.Agent.canSwim)
			{
				return base.Agent.IsSwimming;
			}
			return WaterLevel.GetWaterDepth(((Component)Owner).transform.position, waves: false, volumes: false) >= 0.3f;
		}
	}
}
