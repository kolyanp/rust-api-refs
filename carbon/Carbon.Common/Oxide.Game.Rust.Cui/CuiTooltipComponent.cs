using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Oxide.Game.Rust.Cui;

public class CuiTooltipComponent : ICuiComponent, ICuiEnableable
{
	public string Type => "Tooltip";

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("tooltipType")]
	public Nullable TooltipType { get; set; }

	[JsonProperty("offset")]
	public string Offset { get; set; }

	[JsonProperty("useCentre")]
	public bool? UseCentre { get; set; }

	[JsonProperty("text")]
	public string Text { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("delay")]
	public Nullable Delay { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("position")]
	public Nullable Position { get; set; }

	[JsonProperty("enabled")]
	public bool? Enabled { get; set; }
}
