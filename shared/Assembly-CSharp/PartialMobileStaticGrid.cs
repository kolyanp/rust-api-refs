using Spatial;
using UnityEngine;

public class PartialMobileStaticGrid<T> where T : MonoBehaviour
{
	private Grid<T> grid = new Grid<T>(32, 8096f);

	private ListDictionary<T, Vector3> mobilePositions = new ListDictionary<T, Vector3>();

	private TimeSince lastMobileUpdate;

	public Grid<T> Grid => grid;

	public void UpdateMobileEntities()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		if (TimeSince.op_Implicit(lastMobileUpdate) < 1f)
		{
			return;
		}
		using (TimeWarning.New("UpdateMobileEntities"))
		{
			lastMobileUpdate = TimeSince.op_Implicit(0f);
			float num = 1f;
			for (int i = 0; i < mobilePositions.Keys.Count; i++)
			{
				T val = mobilePositions.Keys[i];
				if (!((Object)(object)val == (Object)null) && !((Object)(object)((Component)(object)val).transform == (Object)null))
				{
					Vector3 position = ((Component)(object)val).transform.position;
					Vector3 val2 = mobilePositions.Values[i];
					Vector3 val3 = position - Vector3Ex.WithY(val2, position.y);
					if (((Vector3)(ref val3)).sqrMagnitude > num)
					{
						grid.Remove(val);
						grid.Add(val, position.x, position.z);
						mobilePositions[val] = position;
					}
				}
			}
		}
	}

	public void OnParentChanged(BaseEntity target, BaseEntity oldParent, BaseEntity newParent)
	{
		OnParentChanged((T)(object)((target is T) ? target : null), target, oldParent, newParent);
	}

	public void OnParentChanged(T target, BaseEntity targetEntity, BaseEntity oldParent, BaseEntity newParent)
	{
		BaseEntity baseEntity = (((Object)(object)oldParent != (Object)null) ? oldParent.GetRootParentEntity() : null);
		BaseEntity baseEntity2 = (((Object)(object)newParent != (Object)null) ? newParent.GetRootParentEntity() : null);
		bool num = (Object)(object)baseEntity != (Object)null && baseEntity.syncPosition;
		bool flag = (Object)(object)baseEntity2 != (Object)null && baseEntity2.syncPosition;
		if (num != flag)
		{
			DeregisterEntity(target);
			RegisterEntity(target, targetEntity, baseEntity2);
		}
	}

	public void RegisterEntity(BaseEntity ent)
	{
		RegisterEntity((T)(object)((ent is T) ? ent : null), ent);
	}

	public void RegisterEntity(T target, BaseEntity associatedEntity, BaseEntity rootParent = null)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target == (Object)null)
		{
			return;
		}
		Vector3 position = ((Component)(object)target).transform.position;
		if (grid.Contains(target))
		{
			grid.Remove(target);
		}
		grid.Add(target, position.x, position.z);
		bool flag = false;
		if ((Object)(object)rootParent == (Object)null)
		{
			rootParent = associatedEntity.GetRootParentEntity();
		}
		if ((Object)(object)rootParent != (Object)null && rootParent.syncPosition)
		{
			flag = true;
		}
		if (flag)
		{
			if (mobilePositions.Contains(target))
			{
				mobilePositions[target] = position;
			}
			else
			{
				mobilePositions.Add(target, position);
			}
		}
		else if (mobilePositions.Contains(target))
		{
			mobilePositions.Remove(target);
		}
	}

	public void DeregisterEntity(T target)
	{
		grid.Remove(target);
		if (mobilePositions.Contains(target))
		{
			mobilePositions.Remove(target);
		}
	}
}
