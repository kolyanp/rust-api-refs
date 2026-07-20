using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_LandOrSwimAttack : State_Attack
{
	public RootMotionData swimAttack;

	protected override RootMotionData GetAnimation()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (WaterLevel.GetWaterDepth(((Component)Owner).transform.position, waves: false, volumes: false) > 0f)
		{
			return swimAttack;
		}
		return Animation;
	}
}
