using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public class CuiOutlineComponent : ICuiComponent, ICuiColor, ICuiEnableable
{
	public string Type => "UnityEngine.UI.Outline";

	public string Color { get; set; }

	[JsonProperty("distance")]
	public string Distance { get; set; }

	[JsonProperty("useGraphicAlpha")]
	public bool UseGraphicAlpha { get; set; }

	public bool? Enabled { get; set; }
}
