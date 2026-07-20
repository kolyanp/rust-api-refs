using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public class CuiScrollbar : ICuiComponent, ICuiEnableable
{
	public string Type => "UnityEngine.UI.Scrollbar";

	[JsonProperty("invert")]
	public bool Invert { get; set; }

	[JsonProperty("autoHide")]
	public bool AutoHide { get; set; }

	[JsonProperty("handleSprite")]
	public string HandleSprite { get; set; }

	[JsonProperty("size")]
	public float Size { get; set; }

	[JsonProperty("handleColor")]
	public string HandleColor { get; set; }

	[JsonProperty("highlightColor")]
	public string HighlightColor { get; set; }

	[JsonProperty("pressedColor")]
	public string PressedColor { get; set; }

	[JsonProperty("trackSprite")]
	public string TrackSprite { get; set; }

	[JsonProperty("trackColor")]
	public string TrackColor { get; set; }

	[JsonProperty("enabled")]
	public bool? Enabled { get; set; }
}
