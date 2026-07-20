using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_StoreCheckoutResultButton : MonoBehaviour
{
	public RustButton button;

	[SerializeField]
	[Space]
	private RustText titleText;

	[SerializeField]
	private RustText subtitleText;

	[Space]
	[SerializeField]
	private CoverImage takeoverImage;

	[SerializeField]
	private HttpImage httpImage;

	[Space]
	[SerializeField]
	private GameObject gaugeParent;

	[SerializeField]
	private Image gaugeImage;

	[SerializeField]
	[Space]
	[Header("Animation")]
	private CanvasGroup canvasGroup;
}
