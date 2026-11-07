using UnityEngine;

public class ItemModWorkbenchCraftSpeed : ItemModWorkbenchUpgrade
{
	[Header("Craft Speed Upgrade")]
	[Tooltip("Multiplier applied to craft duration. 0.75 = 25% faster, 0.5 = 50% faster.")]
	[Range(0.1f, 1f)]
	public float speedMultiplier = 0.75f;

	public override float GetCraftSpeedMultiplier(ItemCraftTask task)
	{
		return speedMultiplier;
	}
}
