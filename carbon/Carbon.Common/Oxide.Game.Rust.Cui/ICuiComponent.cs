using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

[JsonConverter(typeof(ComponentConverter))]
public interface ICuiComponent
{
	[JsonProperty("type")]
	string Type { get; }
}
