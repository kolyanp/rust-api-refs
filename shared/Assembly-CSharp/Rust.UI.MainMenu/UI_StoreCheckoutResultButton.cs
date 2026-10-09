using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_StoreCheckoutResultButton : MonoBehaviour
{
	public RustButton button;

	[Space]
	[SerializeField]
	private RustText titleText;

	[SerializeField]
	private RustText subtitleText;

	[SerializeField]
	[Space]
	private CoverImage takeoverImage;

	[SerializeField]
	private HttpImage httpImage;

	[SerializeField]
	[Space]
	private GameObject gaugeParent;

	[SerializeField]
	private Image gaugeImage;

	[Space]
	[SerializeField]
	[Header("Animation")]
	private CanvasGroup canvasGroup;
}
