using UnityEngine;

public class ItemModWorkbenchComfort : ItemModWorkbenchUpgrade
{
	[Header("Comfort")]
	[Tooltip("The comfort level to provide (1.0 = 100% comfort).")]
	[Range(0f, 1f)]
	public float comfortLevel = 1f;

	public override float GetMinComfortLevel()
	{
		return comfortLevel;
	}
}
