using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_KeyBindConflictPopup : UI_Popup
{
	[Space]
	[SerializeField]
	private RustText keyText;

	[SerializeField]
	private RustText bindText;

	public static readonly Phrase TitlePhrase = new Phrase("keybinds.conflict.title", "Conflict");

	public static readonly Phrase MessagePhrase = new Phrase("keybinds.conflict.message", "This key is already bound to another action");

	public static readonly Phrase ReplacePhrase = new Phrase("keybinds.conflict.replace", "Replace");

	public static readonly Phrase CancelPhrase = new Phrase("keybinds.conflict.cancel", "Cancel");
}
