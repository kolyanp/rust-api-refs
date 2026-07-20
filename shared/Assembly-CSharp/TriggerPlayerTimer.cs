using Rust;
using UnityEngine;

public class TriggerPlayerTimer : TriggerBase, IServerComponent
{
	public BaseEntity TargetEntity;

	public float DamageAmount = 20f;

	public float TimeToDamage = 3f;

	internal override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if ((Object)(object)obj != (Object)null)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
			if (baseEntity is BasePlayer basePlayer && baseEntity.isServer && !basePlayer.isMounted)
			{
				return ((Component)baseEntity).gameObject;
			}
		}
		return obj;
	}

	internal override void OnObjects()
	{
		base.OnObjects();
		Invoke(DamageTarget, TimeToDamage);
	}

	internal override void OnEmpty()
	{
		base.OnEmpty();
		CancelInvoke(DamageTarget);
	}

	private void DamageTarget()
	{
		bool flag = false;
		foreach (BaseEntity entityContent in entityContents)
		{
			if (entityContent is BasePlayer { isMounted: false })
			{
				flag = true;
			}
		}
		if (flag && (Object)(object)TargetEntity != (Object)null)
		{
			TargetEntity.OnAttacked(new HitInfo(null, TargetEntity, DamageType.Generic, DamageAmount));
		}
		Invoke(DamageTarget, TimeToDamage);
	}
}
