public class StateErrorAIEvent : BaseAIEvent
{
	public StateErrorAIEvent()
		: base(AIEventType.StateError)
	{
		Rate = ExecuteRate.Fast;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		Result = Inverted;
		switch (stateStatus)
		{
		case StateStatus.Error:
			Result = !Inverted;
			break;
		case StateStatus.Running:
			Result = Inverted;
			break;
		}
	}
}
