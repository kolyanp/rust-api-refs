using UnityEngine;

public static class BaseEntityEx
{
	public static bool IsValidEntityReference<T>(this T obj) where T : class
	{
		return (Object)(object)(obj as BaseEntity) != (Object)null;
	}

	public static bool HasEntityInParents(this BaseEntity ent, BaseEntity toFind)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ent == (Object)null || (Object)(object)toFind == (Object)null)
		{
			return false;
		}
		if (ent == toFind)
		{
			return true;
		}
		NetworkableId otherID = ((toFind.net != null) ? toFind.net.ID : NetworkableId.EmptyId);
		if (((NetworkableId)(ref otherID)).IsValid)
		{
			if (ent.EqualNetID(otherID))
			{
				return true;
			}
			BaseEntity parentEntity = ent.GetParentEntity();
			while ((Object)(object)parentEntity != (Object)null)
			{
				if (parentEntity == toFind || parentEntity.EqualNetID(otherID))
				{
					return true;
				}
				parentEntity = parentEntity.GetParentEntity();
			}
			return false;
		}
		BaseEntity parentEntity2 = ent.GetParentEntity();
		while ((Object)(object)parentEntity2 != (Object)null)
		{
			if (parentEntity2 == toFind)
			{
				return true;
			}
			parentEntity2 = parentEntity2.GetParentEntity();
		}
		return false;
	}

	public static bool HasColorData(this BaseEntity entity)
	{
		return EntityColorSwapLookup.instance.EntityHasColorData(entity);
	}

	public static bool TryGetColorDataset(this BaseEntity entity, out EntityColorSwapLookup.ColorDataset colorDataset)
	{
		return EntityColorSwapLookup.instance.TryGetEntityColorDataset(entity, out colorDataset);
	}
}
