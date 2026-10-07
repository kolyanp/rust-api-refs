using UnityEngine;

public class ItemModWorkbenchRange : ItemModWorkbenchUpgrade
{
	[Range(1f, 10f)]
	[Tooltip("Multiplier applied to the workbench's TriggerWorkbench sphere collider radius (e.g. 2 = double range).")]
	[Header("Range")]
	public float rangeMultiplier = 2f;

	public override float GetRangeMultiplier()
	{
		return rangeMultiplier;
	}
}
