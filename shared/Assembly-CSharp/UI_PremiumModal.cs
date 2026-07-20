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

	public static readonly Phrase ErrorPhrase = new Phrase("premium.error", "Error");
}
