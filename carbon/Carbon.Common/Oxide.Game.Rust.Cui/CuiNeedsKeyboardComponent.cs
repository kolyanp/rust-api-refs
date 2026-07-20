using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public class CuiNeedsKeyboardComponent : ICuiComponent, ICuiEnableable
{
	public string Type => "NeedsKeyboard";

	[JsonProperty("enabled")]
	public bool? Enabled { get; set; }
}
