using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_KeyBindConflictPopup : UI_Popup
{
	[Space]
	[SerializeField]
	private RustText keyText;

	[SerializeField]
	private RustText bindText;

	public static readonly Phrase TitlePhrase;

	public static readonly Phrase MessagePhrase;

	public static readonly Phrase ReplacePhrase;

	public static readonly Phrase CancelPhrase;

	static UI_KeyBindConflictPopup()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		TitlePhrase = new Phrase("keybinds.conflict.title", "Conflict");
		MessagePhrase = new Phrase("keybinds.conflict.message", "This key is already bound to another action");
		ReplacePhrase = new Phrase("keybinds.conflict.replace", "Replace");
		CancelPhrase = new Phrase("keybinds.conflict.cancel", "Cancel");
	}
}
