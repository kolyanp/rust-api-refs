using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_Popup : UI_Window
{
	[Space]
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private bool destroyOnClose = true;

	[SerializeField]
	private Transform buttonsParent;

	[SerializeField]
	private RustText titleText;

	[SerializeField]
	protected RustText messageText;

	[SerializeField]
	private RustButton buttonTemplate;

	[SerializeField]
	private RustButton[] buttons;
}
