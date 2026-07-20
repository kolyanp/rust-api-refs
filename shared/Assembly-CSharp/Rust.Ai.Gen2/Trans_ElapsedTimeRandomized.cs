using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_ElapsedTimeRandomized : Trans_ElapsedTime
{
	[SerializeField]
	public double MinDuration = 2.0;

	[SerializeField]
	public double MaxDuration = 6.0;

	public override void OnStateEnter()
	{
		Duration = MinDuration + (double)Random.value * (MaxDuration - MinDuration);
		base.OnStateEnter();
	}
}
