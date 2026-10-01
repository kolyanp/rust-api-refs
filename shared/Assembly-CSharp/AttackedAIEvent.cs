using UnityEngine;

public class AttackedAIEvent : BaseAIEvent
{
	protected float lastExecuteTime = float.NegativeInfinity;

	private BaseCombatEntity combatEntity;

	public AttackedAIEvent()
		: base(AIEventType.Attacked)
	{
		Rate = ExecuteRate.Fast;
	}

	public override void Reset()
	{
		base.Reset();
		lastExecuteTime = Time.time;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		Result = Inverted;
		combatEntity = memory.Entity.Get(InputEntityMemorySlot) as BaseCombatEntity;
		float num = lastExecuteTime;
		lastExecuteTime = Time.time;
		if ((Object)(object)combatEntity == (Object)null || !(combatEntity.lastAttackedTime >= num) || (Object)(object)combatEntity.lastAttacker == (Object)null || (Object)(object)combatEntity.lastAttacker == (Object)(object)combatEntity)
		{
			return;
		}
		BasePlayer basePlayer = combatEntity.lastAttacker as BasePlayer;
		if (!((Object)(object)basePlayer != (Object)null) || !((Object)(object)basePlayer == (Object)(object)memory.Entity.Get(5)) || !((Object)(object)basePlayer.lastDealtDamageTo == (Object)(object)Owner))
		{
			if (ShouldSetOutputEntityMemory)
			{
				memory.Entity.Set(combatEntity.lastAttacker, OutputEntityMemorySlot);
			}
			Result = !Inverted;
		}
	}
}
