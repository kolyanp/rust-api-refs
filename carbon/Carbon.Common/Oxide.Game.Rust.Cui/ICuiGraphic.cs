using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public interface ICuiGraphic
{
	[JsonProperty("fadeIn")]
	float FadeIn { get; set; }

	[JsonProperty("placeholderParentId")]
	string PlaceholderParentId { get; set; }
}
