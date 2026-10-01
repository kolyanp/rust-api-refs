public class AttackTickAIEvent : BaseAIEvent
{
	public AttackTickAIEvent()
		: base(AIEventType.AttackTick)
	{
		Rate = ExecuteRate.VeryFast;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		Result = Inverted;
		if (Owner is IAIAttack iAIAttack)
		{
			BaseEntity baseEntity = memory.Entity.Get(InputEntityMemorySlot);
			iAIAttack.AttackTick(deltaTime, baseEntity, senses.Memory.IsLOS(baseEntity));
			Result = !Inverted;
		}
	}
}
