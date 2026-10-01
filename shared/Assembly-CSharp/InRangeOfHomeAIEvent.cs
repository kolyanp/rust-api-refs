using ProtoBuf;
using UnityEngine;

public class InRangeOfHomeAIEvent : BaseAIEvent
{
	public float Range { get; set; }

	public InRangeOfHomeAIEvent()
		: base(AIEventType.InRangeOfHome)
	{
		Rate = ExecuteRate.Fast;
	}

	public override void Init(AIEventData data, BaseEntity owner)
	{
		base.Init(data, owner);
		InRangeOfHomeAIEventData inRangeOfHomeData = data.inRangeOfHomeData;
		Range = inRangeOfHomeData.range;
	}

	public override AIEventData ToProto()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AIEventData val = base.ToProto();
		val.inRangeOfHomeData = new InRangeOfHomeAIEventData();
		val.inRangeOfHomeData.range = Range;
		return val;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = memory.Position.Get(4);
		Result = false;
		bool flag = Vector3Ex.Distance2D(((Component)Owner).transform.position, val) <= Range;
		Result = (Inverted ? (!flag) : flag);
	}
}
