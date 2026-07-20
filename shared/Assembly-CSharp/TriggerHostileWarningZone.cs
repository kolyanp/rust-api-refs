using UnityEngine;

public class TriggerHostileWarningZone : TriggerBase
{
	public GameObject TargetGameObject;

	public Collider triggerCollider { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		triggerCollider = ((Component)this).GetComponent<Collider>();
	}

	protected void OnEnable()
	{
		ResizeTrigger();
	}

	[UnityEvent]
	public void ResizeTrigger()
	{
		if ((Object)(object)TargetGameObject == (Object)null)
		{
			return;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(TargetGameObject);
		if (!((Object)(object)baseEntity == (Object)null) && baseEntity is IHostileWarningEntity hostileWarningEntity)
		{
			Collider obj = triggerCollider;
			SphereCollider val = (SphereCollider)(object)((obj is SphereCollider) ? obj : null);
			if ((Object)(object)val != (Object)null)
			{
				val.radius = hostileWarningEntity.WarningRange();
			}
		}
	}

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
		return ((Component)baseEntity).gameObject;
	}

	public bool WarningEnabled(BaseEntity forEntity)
	{
		if ((Object)(object)TargetGameObject == (Object)null)
		{
			return true;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(TargetGameObject);
		if ((Object)(object)baseEntity == (Object)null)
		{
			return true;
		}
		if (baseEntity is IHostileWarningEntity hostileWarningEntity)
		{
			return hostileWarningEntity.WarningEnabled(forEntity);
		}
		return true;
	}
}
