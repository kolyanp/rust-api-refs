using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
internal class Trans_TooFarFromWater : FSMTransitionBase
{
	public float maxDistance = 20f;

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_TooFarFromWater"))
		{
			return TerrainTexturing.Instance.GetCoarseDistanceToShore(((Component)Owner).transform.position) < 0f - maxDistance;
		}
	}
}
