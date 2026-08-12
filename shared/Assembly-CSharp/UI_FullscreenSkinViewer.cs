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
	[Space]
	private UI_SkinInfoPanel skinInfoPanel;

	[SerializeField]
	private UI_StoreAddCartButton cartButton;

	[Space]
	[SerializeField]
	private GameObject navButtonsGroup;

	[Header("Drag")]
	[SerializeField]
	private float inertiaDecay;

	[Header("Pan")]
	[SerializeField]
	private Vector2 panLimitX;

	[SerializeField]
	private Vector2 panLimitY;

	[SerializeField]
	private float panSpeed;

	[SerializeField]
	[Header("Zoom")]
	private float zoomSpeed;

	[SerializeField]
	private Vector2 minMaxFov;

	[Header("Idle")]
	[SerializeField]
	private float idleSwaySpeed;

	[SerializeField]
	private float idleSwayAmount;

	[SerializeField]
	private float swayEaseSpeed;

	[SerializeField]
	private float swayDelay;

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
		inertiaDecay = 5f;
		panLimitX = new Vector2(-0.2f, 0.2f);
		panLimitY = new Vector2(-0.2f, 0.2f);
		panSpeed = 0.0001f;
		zoomSpeed = 0.1f;
		minMaxFov = new Vector2(20f, 8f);
		idleSwaySpeed = 0.1f;
		idleSwayAmount = 12f;
		swayEaseSpeed = 0.05f;
		swayDelay = 0.3f;
		base._002Ector();
	}
}
