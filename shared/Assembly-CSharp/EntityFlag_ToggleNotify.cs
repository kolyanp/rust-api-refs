using UnityEngine;

public class EntityFlag_ToggleNotify : EntityFlag_Toggle
{
	public bool UseEntityParent;

	protected override void OnStateToggled(bool state)
	{
		base.OnStateToggled(state);
		if (!UseEntityParent && (Object)(object)baseEntity != (Object)null && baseEntity is IFlagNotify flagNotify)
		{
			flagNotify.OnFlagToggled(state);
		}
		if (UseEntityParent && (Object)(object)baseEntity != (Object)null && (Object)(object)baseEntity.GetParentEntity() != (Object)null && baseEntity.GetParentEntity() is IFlagNotify flagNotify2)
		{
			flagNotify2.OnFlagToggled(state);
		}
	}
}
