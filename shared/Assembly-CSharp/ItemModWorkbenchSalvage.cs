using UnityEngine;

public class ItemModWorkbenchSalvage : ItemModWorkbenchUpgrade
{
	[Header("Salvage")]
	[Tooltip("Chance for each research item spent on a tech-tree unlock to be kept (e.g. 0.2 = 20%).")]
	[Range(0f, 1f)]
	public float saveChance = 0.2f;

	public override float GetTechTreeSaveChance()
	{
		return saveChance;
	}
}
