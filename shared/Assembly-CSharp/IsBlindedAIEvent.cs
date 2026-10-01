public class IsBlindedAIEvent : BaseAIEvent
{
	public IsBlindedAIEvent()
		: base(AIEventType.IsBlinded)
	{
		Rate = ExecuteRate.Fast;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		bool flag = senses.brain.Blinded();
		if (Inverted)
		{
			Result = !flag;
		}
		else
		{
			Result = flag;
		}
	}
}
