using System;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Roar : State_PlayAnimation
{
	public const string AlreadyRoared = "AlreadyRoared";

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		if (!Senses.FindTarget(out var _))
		{
			return EFSMStateStatus.Failure;
		}
		Blackboard.Add("AlreadyRoared");
		return base.OnStateEnter(payload);
	}
}
