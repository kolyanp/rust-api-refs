using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_TabBox : MonoBehaviour
{
	[SerializeField]
	private bool _autoClose = true;

	[SerializeField]
	private RustText _filterEnabledText;

	[SerializeField]
	private RustButton _collapseButton;

	[SerializeField]
	private Image _spacerImage;

	public static readonly Phrase FiltersPhrase = new Phrase("tabbox.filters", "{0} filters");

	public static readonly Phrase EnabledPhrase = new Phrase("tabbox.filters.enabled", "enabled");

	public static readonly Phrase DisabledPhrase = new Phrase("tabbox.filters.disabled", "disabled");
}
