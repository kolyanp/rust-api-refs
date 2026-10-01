using ProtoBuf;

public class AggressionTimerAIEvent : BaseAIEvent
{
	public float Value { get; private set; }

	public AggressionTimerAIEvent()
		: base(AIEventType.AggressionTimer)
	{
		Rate = ExecuteRate.Fast;
	}

	public override void Init(AIEventData data, BaseEntity owner)
	{
		base.Init(data, owner);
		AggressionTimerAIEventData aggressionTimerData = data.aggressionTimerData;
		Value = aggressionTimerData.value;
	}

	public override AIEventData ToProto()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AIEventData val = base.ToProto();
		val.aggressionTimerData = new AggressionTimerAIEventData();
		val.aggressionTimerData.value = Value;
		return val;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		Result = Inverted;
		if (Inverted)
		{
			Result = senses.TimeInAgressiveState < Value;
		}
		else
		{
			Result = senses.TimeInAgressiveState >= Value;
		}
	}
}
