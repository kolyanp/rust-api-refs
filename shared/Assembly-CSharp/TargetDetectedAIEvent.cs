using ProtoBuf;
using UnityEngine;

public class TargetDetectedAIEvent : BaseAIEvent
{
	public float Range { get; set; }

	public TargetDetectedAIEvent()
		: base(AIEventType.TargetDetected)
	{
		Rate = ExecuteRate.Slow;
	}

	public override void Init(AIEventData data, BaseEntity owner)
	{
		base.Init(data, owner);
		TargetDetectedAIEventData targetDetectedData = data.targetDetectedData;
		Range = targetDetectedData.range;
	}

	public override AIEventData ToProto()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AIEventData val = base.ToProto();
		val.targetDetectedData = new TargetDetectedAIEventData();
		val.targetDetectedData.range = Range;
		return val;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		Result = Inverted;
		BaseEntity nearestTarget = senses.GetNearestTarget(Range);
		if (Inverted)
		{
			if ((Object)(object)nearestTarget == (Object)null && ShouldSetOutputEntityMemory)
			{
				memory.Entity.Remove(OutputEntityMemorySlot);
			}
			Result = (Object)(object)nearestTarget == (Object)null;
		}
		else
		{
			if ((Object)(object)nearestTarget != (Object)null && ShouldSetOutputEntityMemory)
			{
				memory.Entity.Set(nearestTarget, OutputEntityMemorySlot);
			}
			Result = (Object)(object)nearestTarget != (Object)null;
		}
	}
}
