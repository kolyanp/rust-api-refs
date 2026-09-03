using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_SteamInventoryItemBaseModal : UI_Window
{
	[SerializeField]
	protected GameObject skinViewerGroup;

	[SerializeField]
	protected GameObject staticIconGroup;

	[Space]
	[SerializeField]
	protected HttpImage iconImage;

	[SerializeField]
	private UI_BackgroundAspectRatioFitter background;

	[SerializeField]
	protected UI_SkinInfoPanel skinInfoPanel;

	[Header("Gallery")]
	[SerializeField]
	private GameObject gallery;

	[SerializeField]
	private GameObject arrowButtons;

	[SerializeField]
	private RectTransform galleryItemParent;

	[SerializeField]
	private GameObjectRef galleryItemPrefab;

	[SerializeField]
	private ScrollRect galleryScrollRect;

	[SerializeField]
	private CanvasGroup leftArrow;

	[SerializeField]
	private CanvasGroup rightArrow;

	[Header("Skin Viewer")]
	[SerializeField]
	public CoverImage skinViewerImage;

	[SerializeField]
	protected GameObject icon3D;

	[SerializeField]
	private GameObject skinFullscreenButton;

	[SerializeField]
	private GameObject loadingOverlay;

	[SerializeField]
	private Color loadingColor;

	[SerializeField]
	private AnimationCurve loadingCompletePunchCurve;
}
