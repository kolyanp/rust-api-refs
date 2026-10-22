using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_ServerMap : UI_Window
{
	[SerializeField]
	[Header("References")]
	private HttpImage _httpImage;

	[SerializeField]
	private GameObject _loadingObject;

	[SerializeField]
	private RustButton _gridButton;
}
