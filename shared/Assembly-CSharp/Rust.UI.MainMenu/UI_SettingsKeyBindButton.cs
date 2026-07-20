using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_SettingsKeyBindButton : MonoBehaviour
{
	[HideInInspector]
	public string currentBind;

	public RustButton button;

	public StyleAsset boundStyle;

	public StyleAsset notBoundStyle;

	public RustText text;

	public static readonly Phrase pressAKeyPhrase = new Phrase("keybinds.presskey", "Press a key");
}
