using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine.UI;

namespace Oxide.Game.Rust.Cui;

public class CuiImageComponent : ICuiComponent, ICuiColor, ICuiEnableable, ICuiGraphic
{
	public string Type => "UnityEngine.UI.Image";

	[JsonProperty("sprite")]
	public string Sprite { get; set; }

	[JsonProperty("material")]
	public string Material { get; set; }

	public string Color { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("imagetype")]
	public Type ImageType { get; set; }

	[JsonProperty("fillCenter")]
	public bool? FillCenter { get; set; }

	[JsonProperty("png")]
	public string Png { get; set; }

	[JsonProperty("slice")]
	public string Slice { get; set; }

	[JsonProperty("itemid")]
	public int ItemId { get; set; }

	[JsonProperty("skinid")]
	public ulong SkinId { get; set; }

	[JsonProperty("ppuMultiplier")]
	public float PixelsPerUnitMultiplier { get; set; }

	public float FadeIn { get; set; }

	public string PlaceholderParentId { get; set; }

	public bool? BlocksRaycast { get; set; }

	public bool? Enabled { get; set; }
}
