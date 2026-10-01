using System;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_FastSneak : State_CircleDynamic
{
	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		Agent.deceleration.Value = 10f;
		return base.OnStateEnter(payload);
	}

	protected override void SetSpeed(BaseEntity target, float distToTarget, float normalizedDist)
	{
		if (!target.ToNonNpcPlayer(out var player))
		{
			base.SetSpeed(target, distToTarget, normalizedDist);
		}
		else if (distToTarget > 50f)
		{
			Agent.speed = 8.25f;
		}
		else if (player.modelState.sprinting && distToTarget < 20f)
		{
			Agent.speed = 6.875f;
		}
		else if (player.modelState.sprinting)
		{
			Agent.speed = 8.25f;
		}
		else if (player.modelState.ducked || player.estimatedSpeed < 0.1f)
		{
			Agent.speed = 1.7f;
		}
		else
		{
			Agent.speed = 3.5f;
		}
	}
}
