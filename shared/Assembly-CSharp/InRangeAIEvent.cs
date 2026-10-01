using ProtoBuf;
using UnityEngine;

public class InRangeAIEvent : BaseAIEvent
{
	public float Range { get; set; }

	public InRangeAIEvent()
		: base(AIEventType.InRange)
	{
		Rate = ExecuteRate.Fast;
	}

	public override void Init(AIEventData data, BaseEntity owner)
	{
		base.Init(data, owner);
		InRangeAIEventData inRangeData = data.inRangeData;
		Range = inRangeData.range;
	}

	public override AIEventData ToProto()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AIEventData val = base.ToProto();
		val.inRangeData = new InRangeAIEventData();
		val.inRangeData.range = Range;
		return val;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = memory.Entity.Get(InputEntityMemorySlot);
		Result = false;
		if (!((Object)(object)baseEntity == (Object)null))
		{
			bool flag = Vector3Ex.Distance2D(((Component)Owner).transform.position, ((Component)baseEntity).transform.position) <= Range;
			Result = (Inverted ? (!flag) : flag);
		}
	}
}
