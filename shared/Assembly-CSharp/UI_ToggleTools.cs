using Rust.UI.MainMenu;
using UnityEngine;

public class UI_ToggleTools : UI_Window
{
	[SerializeField]
	[Space]
	private GameObjectRef hudTogglePrefabRef;

	[SerializeField]
	private Transform hudTogglesParent;
}
