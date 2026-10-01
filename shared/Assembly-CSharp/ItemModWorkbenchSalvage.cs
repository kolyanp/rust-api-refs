using UnityEngine;

public class ItemModWorkbenchSalvage : ItemModWorkbenchUpgrade
{
	[Header("Salvage")]
	[Tooltip("Fraction to reduce tech-tree unlock cost by (e.g. 0.2 = 20% cheaper).")]
	[Range(0f, 1f)]
	public float costReduction = 0.2f;

	public override float GetTechTreeCostMultiplier()
	{
		return 1f - costReduction;
	}
}
