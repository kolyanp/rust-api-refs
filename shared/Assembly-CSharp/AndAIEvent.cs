public class AndAIEvent : BaseAIEvent
{
	public AndAIEvent()
		: base(AIEventType.And)
	{
		Rate = ExecuteRate.Normal;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		Result = false;
	}
}
