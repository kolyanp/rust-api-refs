using ProtoBuf;
using UnityEngine;

public class ChanceAIEvent : BaseAIEvent
{
	public float Chance { get; set; }

	public ChanceAIEvent()
		: base(AIEventType.Chance)
	{
		Rate = ExecuteRate.Fast;
	}

	public override void Init(AIEventData data, BaseEntity owner)
	{
		base.Init(data, owner);
		Chance = data.chanceData.value;
	}

	public override AIEventData ToProto()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AIEventData val = base.ToProto();
		val.chanceData = new ChanceAIEventData();
		val.chanceData.value = Chance;
		return val;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		Result = Inverted;
		bool flag = Random.Range(0f, 1f) <= Chance;
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
