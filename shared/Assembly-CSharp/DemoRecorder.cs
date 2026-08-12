using Rust.UI;
using Rust.UI.MainMenu;
using UnityEngine;

public class DemoRecorder : SingletonComponent<DemoRecorder>
{
	public RustInput nameInputField;

	public GameObject RecordingUnderlay;

	public GameObject Panel;

	public CanvasGroup CanvasGroup;

	public UI_Popup confirmationPopup;

	public RustButton autofillButton;

	private bool autoFill;

	public static readonly Phrase overwritePhrase;

	static DemoRecorder()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		overwritePhrase = new Phrase("demo.overwrite", "You are about to overwrite a demo with the same name as {0} - proceed?");
	}
}
