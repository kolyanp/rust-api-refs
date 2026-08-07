using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine.UI;

namespace Oxide.Game.Rust.Cui;

public class CuiButtonComponent : ICuiComponent, ICuiColor, ICuiEnableable, ICuiGraphic
{
	public string Type => "UnityEngine.UI.Button";

	[JsonProperty("command")]
	public string Command { get; set; }

	[JsonProperty("close")]
	public string Close { get; set; }

	[JsonProperty("sprite")]
	public string Sprite { get; set; }

	[JsonProperty("material")]
	public string Material { get; set; }

	public string Color { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("imagetype")]
	public Type ImageType { get; set; }

	[JsonProperty("normalColor")]
	public string NormalColor { get; set; }

	[JsonProperty("highlightedColor")]
	public string HighlightedColor { get; set; }

	[JsonProperty("pressedColor")]
	public string PressedColor { get; set; }

	[JsonProperty("selectedColor")]
	public string SelectedColor { get; set; }

	[JsonProperty("disabledColor")]
	public string DisabledColor { get; set; }

	[JsonProperty("colorMultiplier")]
	public float ColorMultiplier { get; set; }

	[JsonProperty("fadeDuration")]
	public float FadeDuration { get; set; }

	[JsonProperty("fadeIn")]
	public float FadeIn { get; set; }

	[JsonProperty("interactable")]
	public bool? Interactable { get; set; }

	[JsonProperty("placeholderParentId")]
	public string PlaceholderParentId { get; set; }

	[JsonProperty("blocksRaycast")]
	public bool? BlocksRaycast { get; set; }

	[JsonProperty("enabled")]
	public bool? Enabled { get; set; }
}
