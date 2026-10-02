using UnityEngine;

public class ItemModWorkbenchRange : ItemModWorkbenchUpgrade
{
	[Header("Range")]
	[Tooltip("Multiplier applied to the workbench's TriggerWorkbench sphere collider radius (e.g. 2 = double range).")]
	[Range(1f, 10f)]
	public float rangeMultiplier = 2f;

	public override float GetRangeMultiplier()
	{
		return rangeMultiplier;
	}
}
