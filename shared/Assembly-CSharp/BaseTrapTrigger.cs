using Oxide.Core;
using UnityEngine;

public class BaseTrapTrigger : TriggerBase
{
	public BaseTrap _trap;

	public override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if ((Object)(object)obj == (Object)null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if ((Object)(object)baseEntity == (Object)null)
		{
			return null;
		}
		if (baseEntity.isClient)
		{
			return null;
		}
		return ((Component)baseEntity).gameObject;
	}

	internal override void OnObjectAdded(GameObject obj, Collider col)
	{
		Interface.CallHook("OnTrapSnapped", this, obj, col);
		base.OnObjectAdded(obj, col);
		_trap.ObjectEntered(obj);
	}

	public override void OnEmpty()
	{
		base.OnEmpty();
		_trap.OnEmpty();
	}
}
