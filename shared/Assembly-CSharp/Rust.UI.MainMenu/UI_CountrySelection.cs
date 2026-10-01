using Facepunch.Flexbox;
using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_CountrySelection : UI_Window
{
	public RustInput searchInput;

	public UI_CountryEntry[] entries;

	public CanvasGroup entriesCanvasGroup;

	public StyleAsset buttonStyle;

	public StyleAsset buttonDarkStyle;

	public RustText detectedCountryText;

	public GameObject detectedCountryLoading;

	public RustButton autoDetectToggle;

	public GameObject autoDetectOverlay;

	public RustButton[] continentTabs;

	public ScrollRect countryScroll;

	public FlexTransition resultsTransition;

	public RustText emptyResultsText;

	private static readonly Phrase automaticPhrase = new Phrase("countryselect.automaticdetect", "Automatically detect country");

	private static readonly Phrase autoOverlayPhrase = new Phrase("countryselect.auto", "Auto");

	static UI_CountrySelection()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
	}
}
