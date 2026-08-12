using UnityEngine.Events;

public class WearableNotifyLifestate : WearableNotify
{
	public BaseCombatEntity.LifeState TargetState;

	public UnityEvent OnTargetState;

	public UnityEvent OnTargetStateFailed;

	public WearableNotifyLifestate()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		OnTargetState = new UnityEvent();
		OnTargetStateFailed = new UnityEvent();
		base._002Ector();
	}
}
