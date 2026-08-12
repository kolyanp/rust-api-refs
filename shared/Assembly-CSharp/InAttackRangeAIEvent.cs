using UnityEngine;

public class InAttackRangeAIEvent : BaseAIEvent
{
	public InAttackRangeAIEvent()
		: base(AIEventType.InAttackRange)
	{
		base.Rate = ExecuteRate.Fast;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		BaseEntity baseEntity = memory.Entity.Get(base.InputEntityMemorySlot);
		base.Result = false;
		if (!((Object)(object)baseEntity == (Object)null) && base.Owner is IAIAttack iAIAttack)
		{
			bool flag = iAIAttack.IsTargetInRange(baseEntity, out var _);
			base.Result = (base.Inverted ? (!flag) : flag);
		}
	}
}
