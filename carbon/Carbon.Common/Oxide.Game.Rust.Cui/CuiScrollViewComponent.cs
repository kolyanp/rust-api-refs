using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine.UI;

namespace Oxide.Game.Rust.Cui;

public class CuiScrollViewComponent : ICuiComponent, ICuiEnableable
{
	public string Type => "UnityEngine.UI.ScrollView";

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public bool Vertical { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public bool Horizontal { get; set; }

	[JsonProperty("movementType")]
	[JsonConverter(typeof(StringEnumConverter))]
	public MovementType MovementType { get; set; }

	[JsonProperty("elasticity")]
	public float Elasticity { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public bool Inertia { get; set; }

	[JsonProperty("decelerationRate")]
	public float DecelerationRate { get; set; }

	[JsonProperty("scrollSensitivity")]
	public float ScrollSensitivity { get; set; }

	[JsonProperty("contentTransform")]
	public CuiRectTransform ContentTransform { get; set; }

	[JsonProperty("horizontalScrollbar")]
	public CuiScrollbar HorizontalScrollbar { get; set; }

	[JsonProperty("verticalScrollbar")]
	public CuiScrollbar VerticalScrollbar { get; set; }

	[JsonProperty("horizontalNormalizedPosition")]
	public float? HorizontalNormalizedPosition { get; set; }

	[JsonProperty("verticalNormalizedPosition")]
	public float? VerticalNormalizedPosition { get; set; }

	[JsonProperty("enabled")]
	public bool? Enabled { get; set; }
}
