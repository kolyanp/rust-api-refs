public class OnPositionMemorySetAIEvent : BaseAIEvent
{
	public OnPositionMemorySetAIEvent()
		: base(AIEventType.OnPositionMemorySet)
	{
		Rate = ExecuteRate.Fast;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		Result = false;
		if (memory.Position.GetTimeSinceSet(5) <= 0.5f)
		{
			Result = !Inverted;
		}
		else
		{
			Result = Inverted;
		}
	}
}
