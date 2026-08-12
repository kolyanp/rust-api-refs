using Rust.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class UINexusMapBase : BaseMonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	public RawImage BackgroundImage;

	public RawImage BackgroundFillImage;

	public RectTransform LoadingView;

	public RectTransform MissingView;

	public ScrollRectEx MapScrollRect;

	public ScrollRectZoom MapScrollZoom;

	public RectTransform CameraPositon;

	public CanvasGroup ZoneNameCanvasGroup;

	public RectTransform ZoneNameContainer;

	public GameObjectRef ZoneNameMarkerPrefab;

	[Header("Zone Details")]
	public CanvasGroup ZoneDetails;

	public RustText ZoneName;

	public RustText OnlineCount;

	public RustText MaxCount;

	public GameObjectRef ZoneNameLabelPrefab;

	public GameObject InboundFerriesSection;

	public RectTransform InboundFerriesList;

	public GameObject OutboundFerriesSection;

	public RectTransform OutboundFerriesList;

	public GameObject ConnectionsSection;

	public RectTransform ConnectionsList;

	[Header("Behavior")]
	public bool ShowLocalPlayer;

	public float OutOfBoundsScaleFactor;

	public float ZoneNameAlphaPower;

	public UnityEvent OnMapLoaded;

	public UnityEvent OnClicked;

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	protected UINexusMapBase()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		OutOfBoundsScaleFactor = 5f;
		ZoneNameAlphaPower = 100f;
		OnMapLoaded = new UnityEvent();
		OnClicked = new UnityEvent();
		base._002Ector();
	}
}
