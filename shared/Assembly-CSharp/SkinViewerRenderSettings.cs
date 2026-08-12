using UnityEngine;

public class SkinViewerRenderSettings : MonoBehaviour
{
	[ItemSelector]
	public ItemDefinition ItemDefinition;

	public int itemID;

	public Transform CustomLightingRig;

	[Space]
	public bool overrideFullScreenPosition;

	public Vector3 fullScreenPositionOffset;

	[Space]
	public bool overrideFullScreenRotation;

	public Vector3 fullScreenRotation;

	[Space]
	public bool overrideZoom;

	public Vector2 minMaxZoom;

	[Space]
	public bool limitVerticalRotation;

	public bool limitHorizontalRotation;

	public float maxRotationAngle;

	public SkinViewerRenderSettings()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		minMaxZoom = new Vector2(20f, 8f);
		maxRotationAngle = 15f;
		((MonoBehaviour)this)._002Ector();
	}
}
