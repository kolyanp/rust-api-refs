using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public class CuiMaskComponent : ICuiComponent, ICuiEnableable
{
	public string Type => "UnityEngine.UI.Mask";

	[JsonProperty("showMaskGraphic")]
	public bool? ShowMaskGraphic { get; set; }

	[JsonProperty("enabled")]
	public bool? Enabled { get; set; }
}
