using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RCMenu : ComputerMenu
{
	public Image backgroundOpaque;

	public InputField newBookmarkEntryField;

	public NeedsCursor needsCursor;

	public float hiddenOffset;

	public RectTransform devicesPanel;

	private Vector3 initialDevicesPosition;

	public static bool isControllingCamera;

	public CanvasGroup overExposure;

	public CanvasGroup interference;

	public float interferenceFadeDuration;

	public float rangeInterferenceScale;

	public Text timeText;

	public Text watchedDurationText;

	public Text deviceNameText;

	public Text noSignalText;

	public Text primaryActionPrompt;

	public Text hostileText;

	public Text healthText;

	public GameObject healthBarParent;

	public RectTransform healthBarBackground;

	public RectTransform healthBarFill;

	public SoundDefinition bookmarkPressedSoundDef;

	public GameObject[] hideIfStatic;

	public GameObject readOnlyIndicator;

	[FormerlySerializedAs("crosshair")]
	public GameObject aimCrosshair;

	public GameObject generalCrosshair;

	public float fogOverrideDensity;

	public float autoTurretFogDistance;

	public float autoTurretDotBaseScale;

	public float autoTurretDotGrowScale;

	public PingManager PingManager;

	public ScrollRectSettable scrollRect;

	public Phrase Phrase_NoSignal;

	public Phrase Phrase_CameraDisabled;

	public RCMenu()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		hiddenOffset = -256f;
		interferenceFadeDuration = 0.2f;
		rangeInterferenceScale = 10000f;
		fogOverrideDensity = 0.1f;
		autoTurretFogDistance = 30f;
		autoTurretDotBaseScale = 2f;
		autoTurretDotGrowScale = 4f;
		Phrase_NoSignal = new Phrase("no_signal", "No Signal");
		Phrase_CameraDisabled = new Phrase("weak_signal", "Weak Signal");
		base._002Ector();
	}
}
