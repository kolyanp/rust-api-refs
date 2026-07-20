using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class Trans_Lambda : FSMTransitionBase
{
	private Func<BaseEntity, bool> EvaluateFunc;

	public Trans_Lambda(Func<BaseEntity, bool> evaluateFunc)
	{
		EvaluateFunc = evaluateFunc;
	}

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		using (TimeWarning.New("Trans_Lambda"))
		{
			if ((Object)(object)Owner == (Object)null)
			{
				return false;
			}
			return EvaluateFunc(Owner);
		}
	}
}
