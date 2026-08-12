using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIBinocularOverlay : UIBlackoutOverlay, IShadowGroupVisibility
{
	public RustText RangeText;

	public CanvasGroup RangeGroup;

	public RectTransform RangeTransform;

	public Vector2 RangeScreenPosition;

	public Material binocularEffectMaterial;

	public Image overlayImage;

	[Header("Day/Night Settings")]
	public float dayFresnel;

	public float nightFresnel;

	public float dayGlare;

	public float nightGlare;

	public float dayCoating;

	public float nightCoating;

	public bool ShouldUpdateShadows => RangeGroup.alpha > 0f;

	public UIBinocularOverlay()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		RangeScreenPosition = new Vector2(0f, 0f);
		base._002Ector();
	}
}
