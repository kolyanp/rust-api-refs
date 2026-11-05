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

	static UI_KeyBindConflictPopup()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected Obj, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected Obj, but got Unknown
	}
}
