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

	public static readonly Phrase FiltersPhrase;

	public static readonly Phrase EnabledPhrase;

	public static readonly Phrase DisabledPhrase;

	static UI_TabBox()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		FiltersPhrase = new Phrase("tabbox.filters", "{0} filters");
		EnabledPhrase = new Phrase("tabbox.filters.enabled", "enabled");
		DisabledPhrase = new Phrase("tabbox.filters.disabled", "disabled");
	}
}
