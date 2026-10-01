using UnityEngine;

public class PerformedAttackAIEvent : BaseAIEvent
{
	protected float lastExecuteTime = float.NegativeInfinity;

	private BaseCombatEntity combatEntity;

	public PerformedAttackAIEvent()
		: base(AIEventType.PerformedAttack)
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
		Result = false;
		combatEntity = memory.Entity.Get(InputEntityMemorySlot) as BaseCombatEntity;
		float num = lastExecuteTime;
		lastExecuteTime = Time.time;
		if ((Object)(object)combatEntity == (Object)null)
		{
			return;
		}
		if (combatEntity.lastDealtDamageTime >= num)
		{
			if ((Object)(object)combatEntity.lastDealtDamageTo == (Object)null || (Object)(object)combatEntity.lastDealtDamageTo == (Object)(object)combatEntity)
			{
				return;
			}
			BasePlayer basePlayer = combatEntity as BasePlayer;
			if (!((Object)(object)basePlayer != (Object)null) || ((!((Object)(object)basePlayer == (Object)(object)memory.Entity.Get(5)) || !((Object)(object)basePlayer.lastDealtDamageTo == (Object)(object)Owner)) && (!((Object)(object)basePlayer == (Object)(object)memory.Entity.Get(5)) || (((Component)basePlayer.lastDealtDamageTo).gameObject.layer != 21 && ((Component)basePlayer.lastDealtDamageTo).gameObject.layer != 8))))
			{
				if (ShouldSetOutputEntityMemory)
				{
					memory.Entity.Set(combatEntity.lastDealtDamageTo, OutputEntityMemorySlot);
				}
				Result = !Inverted;
			}
		}
		else
		{
			Result = Inverted;
		}
	}
}
