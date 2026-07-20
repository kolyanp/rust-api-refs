using System.Linq;
using Rust;
using UnityEngine;

public class TriggerHurt : TriggerBase, IServerComponent, IHurtTrigger
{
	public float DamagePerSecond = 1f;

	public float DamageTickRate = 4f;

	public DamageType damageType;

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

	public override void OnObjects()
	{
		InvokeRepeating(OnTick, 0f, 1f / DamageTickRate);
	}

	public override void OnEmpty()
	{
		CancelInvoke(OnTick);
	}

	private void OnTick()
	{
		BaseEntity attacker = GameObjectEx.ToBaseEntity(((Component)this).gameObject);
		if (entityContents == null)
		{
			return;
		}
		BaseEntity[] array = entityContents.ToArray();
		foreach (BaseEntity baseEntity in array)
		{
			if (baseEntity.IsValid())
			{
				BaseCombatEntity baseCombatEntity = baseEntity as BaseCombatEntity;
				if (!((Object)(object)baseCombatEntity == (Object)null) && CanHurt(baseCombatEntity))
				{
					baseCombatEntity.Hurt(DamagePerSecond * (1f / DamageTickRate), damageType, attacker);
				}
			}
		}
	}

	protected virtual bool CanHurt(BaseCombatEntity ent)
	{
		return true;
	}
}
