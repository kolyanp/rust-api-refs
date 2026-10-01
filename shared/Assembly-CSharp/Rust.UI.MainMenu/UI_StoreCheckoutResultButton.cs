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

	[Space]
	[SerializeField]
	private CoverImage takeoverImage;

	[SerializeField]
	private HttpImage httpImage;

	[SerializeField]
	[Space]
	private GameObject gaugeParent;

	[SerializeField]
	private Image gaugeImage;

	[Header("Animation")]
	[SerializeField]
	[Space]
	private CanvasGroup canvasGroup;
}
