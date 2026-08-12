using Rust.UI;
using Rust.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

public class UI_PremiumModal : UI_Window
{
	[Space]
	public RustText UsernameLabel;

	public RustText MoneyLabel;

	public RustText ActiveStatusLabel;

	public RawImage ProfilePicture;

	public RustButton RefreshButton;

	public Phrase ActivePhrase;

	public Phrase InactivePhrase;

	public Phrase SearchingPhrase;

	public static readonly Phrase ErrorPhrase;

	static UI_PremiumModal()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		ErrorPhrase = new Phrase("premium.error", "Error");
	}
}
