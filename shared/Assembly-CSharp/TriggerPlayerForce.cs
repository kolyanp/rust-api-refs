using UnityEngine;

public class TriggerPlayerForce : TriggerBase, IServerComponent
{
	public Collider triggerCollider;

	public float pushVelocity = 5f;

	public bool requireUpAxis;

	public bool pushDown;

	private const float HACK_DISABLE_TIME = 4f;

	internal override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if ((Object)(object)obj == (Object)null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if ((Object)(object)baseEntity != (Object)null && baseEntity.isServer)
		{
			return ((Component)baseEntity).gameObject;
		}
		return null;
	}

	internal override void OnObjects()
	{
		InvokeRepeatingFixedTime(ProcessContentsFixed);
		InvokeRepeating(HackDisableTick, 0f, 3.75f);
	}

	internal override void OnEmpty()
	{
		base.OnEmpty();
		CancelInvokeFixedTime(ProcessContentsFixed);
		CancelInvoke(HackDisableTick);
	}

	protected override void OnDisable()
	{
		CancelInvoke(HackDisableTick);
		base.OnDisable();
	}

	internal override void OnEntityLeave(BaseEntity ent)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		base.OnEntityLeave(ent);
		ent.ApplyInheritedVelocity(Vector3.zero);
	}

	private void HackDisableTick()
	{
		if (entityContents == null || !((Behaviour)this).enabled)
		{
			return;
		}
		foreach (BaseEntity entityContent in entityContents)
		{
			if (IsInterested(entityContent))
			{
				BasePlayer basePlayer = entityContent.ToPlayer();
				if ((Object)(object)basePlayer != (Object)null && !basePlayer.IsNpc)
				{
					basePlayer.PauseVehicleNoClipDetection(4f);
					basePlayer.PauseSpeedHackDetection(4f);
				}
			}
		}
	}

	private void ProcessContentsFixed()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (entityContents == null)
		{
			return;
		}
		foreach (BaseEntity entityContent in entityContents)
		{
			if ((!requireUpAxis || !(Vector3.Dot(((Component)entityContent).transform.up, ((Component)this).transform.up) < 0f)) && IsInterested(entityContent))
			{
				Vector3 velocity = GetPushVelocity(((Component)entityContent).gameObject);
				entityContent.ApplyInheritedVelocity(velocity);
			}
		}
	}

	private Vector3 GetPushVelocity(GameObject obj)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = obj.transform.position;
		Vector3 val = Vector3.zero;
		if (pushDown)
		{
			val = -((Component)triggerCollider).transform.up;
		}
		else
		{
			Bounds bounds = triggerCollider.bounds;
			val = position - ((Bounds)(ref bounds)).center;
			((Vector3)(ref val)).Normalize();
			val.y = 0.2f;
			((Vector3)(ref val)).Normalize();
		}
		return val * pushVelocity;
	}

	private bool IsInterested(BaseEntity entity)
	{
		if ((Object)(object)entity == (Object)null || entity.isClient)
		{
			return false;
		}
		BasePlayer basePlayer = entity.ToPlayer();
		if ((Object)(object)basePlayer != (Object)null)
		{
			if ((basePlayer.IsAdmin || basePlayer.IsDeveloper) && basePlayer.IsFlying)
			{
				return false;
			}
			if ((Object)(object)basePlayer != (Object)null && basePlayer.IsAlive())
			{
				return !basePlayer.isMounted;
			}
			return false;
		}
		return true;
	}
}
