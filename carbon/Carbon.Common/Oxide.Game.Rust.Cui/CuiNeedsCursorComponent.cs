using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public class CuiNeedsCursorComponent : ICuiComponent, ICuiEnableable
{
	public string Type => "NeedsCursor";

	[JsonProperty("enabled")]
	public bool? Enabled { get; set; }
}
