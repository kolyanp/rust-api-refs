using ProtoBuf;

public class TirednessAboveAIEvent : BaseAIEvent
{
	public float Value { get; private set; }

	public TirednessAboveAIEvent()
		: base(AIEventType.TirednessAbove)
	{
		Rate = ExecuteRate.Slow;
	}

	public override void Init(AIEventData data, BaseEntity owner)
	{
		base.Init(data, owner);
		TirednessAboveAIEventData tirednessAboveData = data.tirednessAboveData;
		Value = tirednessAboveData.value;
	}

	public override AIEventData ToProto()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AIEventData val = base.ToProto();
		val.tirednessAboveData = new TirednessAboveAIEventData();
		val.tirednessAboveData.value = Value;
		return val;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		Result = Inverted;
		if (Owner is IAITirednessAbove iAITirednessAbove)
		{
			bool flag = iAITirednessAbove.IsTirednessAbove(Value);
			if (Inverted)
			{
				Result = !flag;
			}
			else
			{
				Result = flag;
			}
		}
	}
}
