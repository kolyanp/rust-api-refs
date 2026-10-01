using UnityEngine;

public class TargetLostAIEvent : BaseAIEvent
{
	public float Range { get; set; }

	public TargetLostAIEvent()
		: base(AIEventType.TargetLost)
	{
		Rate = ExecuteRate.Fast;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Result = Inverted;
		BaseEntity baseEntity = memory.Entity.Get(InputEntityMemorySlot);
		if ((Object)(object)baseEntity == (Object)null)
		{
			Result = !Inverted;
			return;
		}
		if (Vector3.Distance(((Component)baseEntity).transform.position, ((Component)Owner).transform.position) > senses.TargetLostRange)
		{
			Result = !Inverted;
			return;
		}
		BasePlayer basePlayer = baseEntity as BasePlayer;
		if (baseEntity.Health() <= 0f || ((Object)(object)basePlayer != (Object)null && basePlayer.IsDead()))
		{
			Result = !Inverted;
		}
		else if (senses.ignoreSafeZonePlayers && (Object)(object)basePlayer != (Object)null && basePlayer.InSafeZone())
		{
			Result = !Inverted;
		}
	}
}
