using UnityEngine;

public class TriggerSplashable : TriggerBase
{
	public CapsuleCollider Capsule;

	private ListDictionary<BaseEntity, (bool visible, Vector3 lastCheckPos)> visibleState = new ListDictionary<BaseEntity, (bool, Vector3)>();

	internal override GameObject InterestedInObject(GameObject obj)
	{
		if (obj.GetComponent<ISplashable>() == null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if ((Object)(object)baseEntity == (Object)null || baseEntity.isClient)
		{
			return null;
		}
		return base.InterestedInObject(obj);
	}

	internal override void OnEntityLeave(BaseEntity ent)
	{
		base.OnEntityLeave(ent);
		if (visibleState.ContainsKey(ent))
		{
			visibleState.Remove(ent);
		}
	}

	public bool ShouldCheckLineOfSight(BaseEntity ent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)ent).transform.position;
		if (visibleState.ContainsKey(ent))
		{
			Vector3 val = visibleState[ent].Item2 - position;
			return ((Vector3)(ref val)).sqrMagnitude > 1f;
		}
		return true;
	}

	public bool HasLineOfSight(BaseEntity ent)
	{
		if (visibleState.ContainsKey(ent))
		{
			return visibleState[ent].Item1;
		}
		return false;
	}

	public void RecordLineOfSight(BaseEntity ent, bool state)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (visibleState.ContainsKey(ent))
		{
			visibleState[ent] = (state, ((Component)ent).transform.position);
		}
		else
		{
			visibleState.Add(ent, (state, ((Component)ent).transform.position));
		}
	}
}
