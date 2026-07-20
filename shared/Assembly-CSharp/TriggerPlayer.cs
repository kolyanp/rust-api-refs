using UnityEngine;

public class TriggerPlayer : TriggerBase
{
	internal override GameObject InterestedInObject(GameObject obj)
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
		if (!(baseEntity is BasePlayer) || baseEntity.IsNpc)
		{
			return null;
		}
		return ((Component)baseEntity).gameObject;
	}
}
