using UnityEngine.Events;

public class WearableNotifyLifestate : WearableNotify
{
	public BaseCombatEntity.LifeState TargetState;

	public UnityEvent OnTargetState = new UnityEvent();

	public UnityEvent OnTargetStateFailed = new UnityEvent();

	public WearableNotifyLifestate()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
	}
}
