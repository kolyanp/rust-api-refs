using ProtoBuf;

public class HungerAboveAIEvent : BaseAIEvent
{
	public float Value { get; private set; }

	public HungerAboveAIEvent()
		: base(AIEventType.HungerAbove)
	{
		Rate = ExecuteRate.Slow;
	}

	public override void Init(AIEventData data, BaseEntity owner)
	{
		base.Init(data, owner);
		HungerAboveAIEventData hungerAboveData = data.hungerAboveData;
		Value = hungerAboveData.value;
	}

	public override AIEventData ToProto()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AIEventData val = base.ToProto();
		val.hungerAboveData = new HungerAboveAIEventData();
		val.hungerAboveData.value = Value;
		return val;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		if (!(Owner is IAIHungerAbove iAIHungerAbove))
		{
			Result = false;
			return;
		}
		bool flag = iAIHungerAbove.IsHungerAbove(Value);
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
