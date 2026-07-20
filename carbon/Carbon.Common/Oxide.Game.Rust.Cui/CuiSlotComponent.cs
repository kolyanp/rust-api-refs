using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public class CuiSlotComponent : ICuiComponent, ICuiEnableable
{
	public string Type => "Slot";

	[JsonProperty("filter")]
	public string Filter { get; set; }

	[JsonProperty("enabled")]
	public bool? Enabled { get; set; }
}
