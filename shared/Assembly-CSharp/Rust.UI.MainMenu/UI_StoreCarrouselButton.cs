using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_StoreCarrouselButton : MonoBehaviour
{
	public RustButton button;

	[Space]
	public RustText titleText;

	public RustText subtitleText;

	public CoverImage coverImage;

	public GameObject videoIcon;

	[Space]
	public GameObject gaugeParent;

	public Image gaugeImage;

	public GameObject variantGroup;

	public RustText variantText;

	[Space]
	public GameObject skinViewerGroup;

	public HttpImage httpImage;
}
