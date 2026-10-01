using UnityEngine;

public class BestTargetDetectedAIEvent : BaseAIEvent
{
	public BestTargetDetectedAIEvent()
		: base(AIEventType.BestTargetDetected)
	{
		Rate = ExecuteRate.Normal;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		Result = Inverted;
		if (!(Owner is IAIAttack iAIAttack))
		{
			return;
		}
		BaseEntity bestTarget = iAIAttack.GetBestTarget();
		if (Inverted)
		{
			if ((Object)(object)bestTarget == (Object)null && ShouldSetOutputEntityMemory)
			{
				memory.Entity.Remove(OutputEntityMemorySlot);
			}
			Result = (Object)(object)bestTarget == (Object)null;
		}
		else
		{
			if ((Object)(object)bestTarget != (Object)null && ShouldSetOutputEntityMemory)
			{
				memory.Entity.Set(bestTarget, OutputEntityMemorySlot);
			}
			Result = (Object)(object)bestTarget != (Object)null;
		}
	}
}
