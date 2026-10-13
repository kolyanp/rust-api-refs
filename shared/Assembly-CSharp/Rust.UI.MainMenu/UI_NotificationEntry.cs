using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_NotificationEntry : MonoBehaviour
{
	[SerializeField]
	[Header("Icons")]
	private RustIcon basicIcon;

	[SerializeField]
	private RustIcon standardIcon;

	[SerializeField]
	private RustIcon banIcon;

	[SerializeField]
	private RustIcon warningIcon;

	[SerializeField]
	private RustIcon popupIcon;

	[SerializeField]
	[Header("UI Elements")]
	private GameObject linkIcon;

	public RustButton linkButton;

	[SerializeField]
	private RustText notificationText;
}
