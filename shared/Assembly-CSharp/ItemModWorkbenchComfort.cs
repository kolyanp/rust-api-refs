using UnityEngine;

public class ItemModWorkbenchComfort : ItemModWorkbenchUpgrade
{
	[Range(0f, 1f)]
	[Tooltip("The comfort level to provide (1.0 = 100% comfort).")]
	[Header("Comfort")]
	public float comfortLevel = 1f;

	public override float GetMinComfortLevel()
	{
		return comfortLevel;
	}
}
