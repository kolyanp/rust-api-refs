using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public interface ICuiEnableable
{
	[JsonProperty("enabled")]
	bool? Enabled { get; set; }
}
