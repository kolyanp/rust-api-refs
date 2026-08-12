using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_CountrySelection : UI_Window
{
	public RustInput searchInput;

	public UI_CountryEntry[] entries;

	public CanvasGroup entriesCanvasGroup;

	public StyleAsset buttonStyle;

	public StyleAsset buttonDarkStyle;

	public RustText selectedCountryText;

	public RustButton autoDetectToggle;

	public RustButton changeCountryButton;

	private static readonly Phrase automaticPhrase;

	static UI_CountrySelection()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		automaticPhrase = new Phrase("countryselect.automatic", "Automatic");
	}
}
