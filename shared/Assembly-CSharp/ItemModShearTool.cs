using UnityEngine;

public class ItemModShearTool : ItemMod
{
	[Tooltip("Multiplies the wool each sheared quarter gives. Blades without this mod shear at 1")]
	public float WoolYieldScale = 1.25f;
}
