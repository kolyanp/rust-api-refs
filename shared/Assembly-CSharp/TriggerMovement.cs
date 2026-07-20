using System;
using UnityEngine;

public class TriggerMovement : TriggerBase
{
	[Tooltip("If set, the entering object must have line of sight to this transform to be added, note this is only checked on entry")]
	public Transform losEyes;

	public BaseEntity.MovementModify movementModify;

	[NonSerialized]
	private float scale = 1f;

	public void SetMovementScale(float newScale)
	{
		scale = newScale;
	}

	public float GetMovementScale()
	{
		return scale;
	}

	internal override GameObject InterestedInObject(GameObject obj)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
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
		if ((Object)(object)losEyes != (Object)null)
		{
			if (entityContents != null && entityContents.Contains(baseEntity))
			{
				return ((Component)baseEntity).gameObject;
			}
			if (!baseEntity.IsVisible(((Component)losEyes).transform.position, baseEntity.CenterPoint()))
			{
				return null;
			}
		}
		return ((Component)baseEntity).gameObject;
	}
}
