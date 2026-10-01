using UnityEngine;

public class InAttackRangeAIEvent : BaseAIEvent
{
	public InAttackRangeAIEvent()
		: base(AIEventType.InAttackRange)
	{
		Rate = ExecuteRate.Fast;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		BaseEntity baseEntity = memory.Entity.Get(InputEntityMemorySlot);
		Result = false;
		if (!((Object)(object)baseEntity == (Object)null) && Owner is IAIAttack iAIAttack)
		{
			bool flag = iAIAttack.IsTargetInRange(baseEntity, out var _);
			Result = (Inverted ? (!flag) : flag);
		}
	}
}
