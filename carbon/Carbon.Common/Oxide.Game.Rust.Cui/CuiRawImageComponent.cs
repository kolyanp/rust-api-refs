using System.ComponentModel;
using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public class CuiRawImageComponent : ICuiComponent, ICuiColor, ICuiEnableable, ICuiGraphic
{
	public string Type => "UnityEngine.UI.RawImage";

	[JsonProperty("sprite")]
	public string Sprite { get; set; }

	public string Color { get; set; }

	[JsonProperty("material")]
	public string Material { get; set; }

	[JsonProperty("url")]
	public string Url { get; set; }

	[JsonProperty("png")]
	public string Png { get; set; }

	[JsonProperty("fadeIn")]
	public float FadeIn { get; set; }

	[JsonProperty("steamid")]
	public string SteamId { get; set; }

	[JsonProperty("placeholderParentId")]
	public string PlaceholderParentId { get; set; }

	[JsonProperty("blocksRaycast")]
	[DefaultValue(true)]
	public bool BlocksRaycast { get; set; } = true;

	[JsonProperty("enabled")]
	public bool? Enabled { get; set; }
}
