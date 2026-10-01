using System.Collections.Generic;
using Rust.UI;
using Rust.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

public class UI_FullscreenSkinViewer : UI_Window
{
	public static UI_FullscreenSkinViewer Instance;

	[Space]
	public CanvasGroup background;

	public CanvasGroup uiGroup;

	public GameObject controlsGroup;

	public RectTransform coverImageParent;

	public RectTransform bgImageParent;

	public Image bgImage;

	[Space]
	public Image glowImage;

	public GameObject closeButton;

	public GameObject toggleButtonsGroup;

	public RustButton worldmodelButton;

	[SerializeField]
	private RustButton animatorToggleButton;

	[Space]
	[SerializeField]
	private UI_SkinInfoPanel skinInfoPanel;

	[SerializeField]
	private UI_StoreAddCartButton cartButton;

	[SerializeField]
	[Space]
	private GameObject navButtonsGroup;

	[Header("Drag")]
	[SerializeField]
	private float inertiaDecay = 5f;

	[Header("Pan")]
	[SerializeField]
	private Vector2 panLimitX = new Vector2(-0.2f, 0.2f);

	[SerializeField]
	private Vector2 panLimitY = new Vector2(-0.2f, 0.2f);

	[SerializeField]
	private float panSpeed = 0.0001f;

	[SerializeField]
	[Header("Zoom")]
	private float zoomSpeed = 0.1f;

	[SerializeField]
	private Vector2 minMaxFov = new Vector2(20f, 8f);

	[SerializeField]
	[Header("Idle")]
	private float idleSwaySpeed = 0.1f;

	[SerializeField]
	private float idleSwayAmount = 12f;

	[SerializeField]
	private float swayEaseSpeed = 0.05f;

	[SerializeField]
	private float swayDelay = 0.3f;

	private UI_SkinViewerControls source;

	private List<IPlayerItemDefinition> navigationItems;

	private bool isTransitioning;

	public UI_FullscreenSkinViewer()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
	}
}
