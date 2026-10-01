public class StateFinishedAIEvent : BaseAIEvent
{
	public StateFinishedAIEvent()
		: base(AIEventType.StateFinished)
	{
		Rate = ExecuteRate.Fast;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		Result = Inverted;
		if (stateStatus == StateStatus.Finished)
		{
			Result = !Inverted;
		}
	}
}
