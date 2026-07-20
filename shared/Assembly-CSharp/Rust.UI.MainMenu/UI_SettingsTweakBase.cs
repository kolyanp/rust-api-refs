using UnityEngine;

namespace Rust.UI.MainMenu;

public abstract class UI_SettingsTweakBase : MonoBehaviour
{
	public Phrase tooltip;

	public Sprite tooltipImage;

	public string tooltipVideoURL;

	[Tooltip("For any clickable URL in the tooltip.")]
	public string tooltipExternalURL;
}
