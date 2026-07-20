using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public interface ICuiColor
{
	[JsonProperty("color")]
	string Color { get; set; }
}
