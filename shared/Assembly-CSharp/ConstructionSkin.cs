using System.Collections.Generic;
using UnityEngine;

public class ConstructionSkin : BasePrefab
{
	public List<GameObject> conditionals;

	public ulong DetermineConditionalModelState(BuildingBlock parent)
	{
		ConditionalModel[] array = PrefabAttribute.server.FindAll<ConditionalModel>(prefabID);
		if (array.Length > 64)
		{
			Debug.LogError((object)("Too many ConditionalModels on " + ((Object)parent).name + "! Maximum supported is 64"));
		}
		ulong num = 0uL;
		for (int i = 0; i < array.Length && i < 64; i++)
		{
			if (array[i].RunTests(parent))
			{
				num |= (ulong)(1L << i);
			}
		}
		return num;
	}

	private void CreateConditionalModels(BuildingBlock parent)
	{
		ConditionalModel[] array = PrefabAttribute.server.FindAll<ConditionalModel>(prefabID);
		for (int i = 0; i < array.Length; i++)
		{
			if (!parent.GetConditionalModel(i))
			{
				continue;
			}
			GameObject val = array[i].InstantiateSkin(parent);
			if (!((Object)(object)val == (Object)null))
			{
				if (conditionals == null)
				{
					conditionals = new List<GameObject>();
				}
				conditionals.Add(val);
			}
		}
	}

	private void DestroyConditionalModels(BuildingBlock parent)
	{
		if (conditionals != null)
		{
			for (int i = 0; i < conditionals.Count; i++)
			{
				parent.gameManager.Retire(conditionals[i]);
			}
			conditionals.Clear();
		}
	}

	public virtual void Refresh(BuildingBlock parent)
	{
		DestroyConditionalModels(parent);
		CreateConditionalModels(parent);
	}

	public void Destroy(BuildingBlock parent)
	{
		DestroyConditionalModels(parent);
		parent.gameManager.Retire(((Component)this).gameObject);
	}

	public virtual uint GetStartingDetailColour(uint playerColourIndex)
	{
		return 0u;
	}
}
