using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Rust.UI.MainMenu;

public class UI_SettingsGestureButton : UI_SettingsGestureDraggable
{
	[SerializeField]
	private StyleAsset unlockedStyle;

	[SerializeField]
	private StyleAsset lockedStyle;

	[Space]
	[SerializeField]
	private GameObject boundLine;

	[SerializeField]
	private GameObject dragIcon;

	[SerializeField]
	private Image gestureIcon;

	[SerializeField]
	private RustText gestureNameText;

	[Space]
	[SerializeField]
	private RectTransform videoPreview;

	[SerializeField]
	private VideoPlayer videoPlayer;

	[SerializeField]
	private RawImage videoRawTexture;

	[SerializeField]
	[Space]
	private GameObject deleteButton;
}
