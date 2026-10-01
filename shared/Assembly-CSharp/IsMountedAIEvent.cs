public class IsMountedAIEvent : BaseAIEvent
{
	public IsMountedAIEvent()
		: base(AIEventType.IsMounted)
	{
		Rate = ExecuteRate.Fast;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		IAIMounted iAIMounted = memory.Entity.Get(InputEntityMemorySlot) as IAIMounted;
		Result = false;
		if (iAIMounted != null)
		{
			if (Inverted && !iAIMounted.IsMounted())
			{
				Result = true;
			}
			if (!Inverted && iAIMounted.IsMounted())
			{
				Result = true;
			}
			if (Result && ShouldSetOutputEntityMemory)
			{
				memory.Entity.Set(memory.Entity.Get(InputEntityMemorySlot), OutputEntityMemorySlot);
			}
		}
	}
}
