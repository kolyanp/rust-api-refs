using UnityEngine;

public class TriggerForce : TriggerBase, IServerComponent
{
	public const float GravityMultiplier = 0.1f;

	public const float VelocityLerp = 10f;

	public const float AngularDrag = 10f;

	public Vector3 velocity;

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

	internal override void OnObjects()
	{
		base.OnObjects();
		InvokeRepeatingFixedTime(FixedContentsTick);
	}

	internal override void OnEmpty()
	{
		base.OnEmpty();
		CancelInvokeFixedTime(FixedContentsTick);
	}

	public override void OnEntityEnter(BaseEntity ent)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		base.OnEntityEnter(ent);
		Vector3 val = ((Component)this).transform.TransformDirection(velocity);
		ent.ApplyInheritedVelocity(val);
	}

	public override void OnEntityLeave(BaseEntity ent)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		base.OnEntityLeave(ent);
		ent.ApplyInheritedVelocity(Vector3.zero);
	}

	private void FixedContentsTick()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (entityContents == null)
		{
			return;
		}
		Vector3 val = ((Component)this).transform.TransformDirection(velocity);
		foreach (BaseEntity entityContent in entityContents)
		{
			if ((Object)(object)entityContent != (Object)null)
			{
				entityContent.ApplyInheritedVelocity(val);
			}
		}
	}

	public TriggerForce()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		velocity = Vector3.forward;
		base._002Ector();
	}
}
