using UnityEngine;

public class VitalInfoUpgradeIcons : FacepunchBehaviour, IClientComponent
{
	[Tooltip("Size of each upgrade icon in pixels.")]
	public Vector2 iconSize = new Vector2(20f, 20f);

	[Tooltip("Maximum number of upgrade icons per row.")]
	public int maxIconsPerRow = 5;

	[Tooltip("The RectTransform of the vital panel to resize. If null, uses parent.")]
	public RectTransform vitalPanel;

	public VitalInfoUpgradeIcons()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
	}
}
