using ProtoBuf;

public class TimeSinceThreatAIEvent : BaseAIEvent
{
	public float Value { get; private set; }

	public TimeSinceThreatAIEvent()
		: base(AIEventType.TimeSinceThreat)
	{
		Rate = ExecuteRate.Fast;
	}

	public override void Init(AIEventData data, BaseEntity owner)
	{
		base.Init(data, owner);
		TimeSinceThreatAIEventData timeSinceThreatData = data.timeSinceThreatData;
		Value = timeSinceThreatData.value;
	}

	public override AIEventData ToProto()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AIEventData val = base.ToProto();
		val.timeSinceThreatData = new TimeSinceThreatAIEventData();
		val.timeSinceThreatData.value = Value;
		return val;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		Result = Inverted;
		if (Inverted)
		{
			Result = senses.TimeSinceThreat < Value;
		}
		else
		{
			Result = senses.TimeSinceThreat >= Value;
		}
	}
}
