using UnityEngine;

public class ReloadingAIEvent : BaseAIEvent
{
	public ReloadingAIEvent()
		: base(AIEventType.Reloading)
	{
		Rate = ExecuteRate.Fast;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		BaseEntity baseEntity = memory.Entity.Get(InputEntityMemorySlot);
		Result = false;
		NPCPlayer nPCPlayer = baseEntity as NPCPlayer;
		if (!((Object)(object)nPCPlayer == (Object)null))
		{
			bool flag = nPCPlayer.IsReloading();
			Result = (Inverted ? (!flag) : flag);
		}
	}
}
