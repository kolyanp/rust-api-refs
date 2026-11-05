using UnityEngine;

public class ItemModFoodVisual : ItemMod
{
	public Mesh TopperMesh;

	public Mesh TopperMeshLod1;

	public Material TopperMaterial;

	public Color CrustTint = Color.white;

	public ItemDefinition InheritFrom;

	public static ItemModFoodVisual Find(ItemDefinition itemDef)
	{
		ItemModFoodVisual itemModFoodVisual = (((Object)(object)itemDef != (Object)null) ? ((Component)itemDef).GetComponent<ItemModFoodVisual>() : null);
		if (!((Object)(object)itemModFoodVisual != (Object)null))
		{
			return null;
		}
		return itemModFoodVisual.Resolve();
	}

	public ItemModFoodVisual Resolve()
	{
		ItemModFoodVisual itemModFoodVisual = this;
		int num = 0;
		while ((Object)(object)itemModFoodVisual.InheritFrom != (Object)null && num < 8)
		{
			ItemModFoodVisual component = ((Component)itemModFoodVisual.InheritFrom).GetComponent<ItemModFoodVisual>();
			if ((Object)(object)component == (Object)null || (Object)(object)component == (Object)(object)itemModFoodVisual)
			{
				break;
			}
			itemModFoodVisual = component;
			num++;
		}
		return itemModFoodVisual;
	}

	public ItemModFoodVisual()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
