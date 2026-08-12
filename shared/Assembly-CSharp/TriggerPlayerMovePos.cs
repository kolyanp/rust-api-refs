using UnityEngine;

public class TriggerPlayerMovePos : TriggerBase, IServerComponent
{
	public BoxCollider triggerCollider;

	public Vector3 relativeMoveVector;

	public bool shouldPauseMarkHostile;

	private const float HACK_DISABLE_TIME = 1.5f;

	internal override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if ((Object)(object)obj == (Object)null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if ((Object)(object)baseEntity != (Object)null)
		{
			return ((Component)baseEntity).gameObject;
		}
		return null;
	}

	internal override void OnObjects()
	{
		InvokeRepeating(HackDisableTick, 0f, 1.25f);
	}

	internal override void OnEmpty()
	{
		base.OnEmpty();
		CancelInvoke(HackDisableTick);
	}

	protected override void OnDisable()
	{
		CancelInvoke(HackDisableTick);
		base.OnDisable();
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
					basePlayer.PauseVehicleNoClipDetection(1.5f);
					basePlayer.PauseSpeedHackDetection(1.5f);
				}
			}
		}
	}

	protected void FixedUpdate()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (entityContents == null)
		{
			return;
		}
		foreach (BaseEntity entityContent in entityContents)
		{
			if (IsInterested(entityContent))
			{
				BasePlayer basePlayer = entityContent.ToPlayer();
				if ((Object)(object)basePlayer != (Object)null && shouldPauseMarkHostile)
				{
					basePlayer.SetHostilePauseTime();
				}
				Transform transform = ((Component)entityContent).transform;
				Bounds bounds = ((Collider)triggerCollider).bounds;
				transform.position = ((Bounds)(ref bounds)).center + relativeMoveVector;
			}
		}
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
			if (basePlayer.IsNpc)
			{
				return false;
			}
			if (basePlayer.IsAlive() && !basePlayer.IsSleeping())
			{
				return !basePlayer.isMounted;
			}
			return false;
		}
		return false;
	}

	public TriggerPlayerMovePos()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		relativeMoveVector = Vector3.up;
		base._002Ector();
	}
}
