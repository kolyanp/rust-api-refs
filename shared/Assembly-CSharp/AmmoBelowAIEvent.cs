using ProtoBuf;

public class AmmoBelowAIEvent : BaseAIEvent
{
	public float Value { get; private set; }

	public AmmoBelowAIEvent()
		: base(AIEventType.AmmoBelow)
	{
		Rate = ExecuteRate.Normal;
	}

	public override void Init(AIEventData data, BaseEntity owner)
	{
		base.Init(data, owner);
		AmmoBelowAIEventData ammoBelowData = data.ammoBelowData;
		Value = ammoBelowData.value;
	}

	public override AIEventData ToProto()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AIEventData val = base.ToProto();
		val.ammoBelowData = new AmmoBelowAIEventData();
		val.ammoBelowData.value = Value;
		return val;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		Result = Inverted;
		if (Owner is IAIAttack iAIAttack)
		{
			bool flag = iAIAttack.GetAmmoFraction() < Value;
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
