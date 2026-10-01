using UnityEngine;

public class IsVisibleAIEvent : BaseAIEvent
{
	public IsVisibleAIEvent()
		: base(AIEventType.IsVisible)
	{
		Rate = ExecuteRate.Fast;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		Result = false;
		BaseEntity baseEntity = memory.Entity.Get(InputEntityMemorySlot);
		if (!((Object)(object)baseEntity == (Object)null) && Owner is IAIAttack)
		{
			bool flag = senses.Memory.IsLOS(baseEntity);
			Result = (Inverted ? (!flag) : flag);
		}
	}
}
