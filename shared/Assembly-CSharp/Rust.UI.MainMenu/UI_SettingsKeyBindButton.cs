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

	public static readonly Phrase pressAKeyPhrase;

	static UI_SettingsKeyBindButton()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		pressAKeyPhrase = new Phrase("keybinds.presskey", "Press a key");
	}
}
