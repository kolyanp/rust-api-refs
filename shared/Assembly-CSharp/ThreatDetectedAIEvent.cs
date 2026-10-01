using ProtoBuf;
using UnityEngine;

public class ThreatDetectedAIEvent : BaseAIEvent
{
	public float Range { get; set; }

	public ThreatDetectedAIEvent()
		: base(AIEventType.ThreatDetected)
	{
		Rate = ExecuteRate.Slow;
	}

	public override void Init(AIEventData data, BaseEntity owner)
	{
		base.Init(data, owner);
		ThreatDetectedAIEventData threatDetectedData = data.threatDetectedData;
		Range = threatDetectedData.range;
	}

	public override AIEventData ToProto()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AIEventData val = base.ToProto();
		val.threatDetectedData = new ThreatDetectedAIEventData();
		val.threatDetectedData.range = Range;
		return val;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		Result = Inverted;
		BaseEntity nearestThreat = senses.GetNearestThreat(Range);
		if (Inverted)
		{
			if ((Object)(object)nearestThreat == (Object)null && ShouldSetOutputEntityMemory)
			{
				memory.Entity.Remove(OutputEntityMemorySlot);
			}
			Result = (Object)(object)nearestThreat == (Object)null;
		}
		else
		{
			if ((Object)(object)nearestThreat != (Object)null && ShouldSetOutputEntityMemory)
			{
				memory.Entity.Set(nearestThreat, OutputEntityMemorySlot);
			}
			Result = (Object)(object)nearestThreat != (Object)null;
		}
	}
}
