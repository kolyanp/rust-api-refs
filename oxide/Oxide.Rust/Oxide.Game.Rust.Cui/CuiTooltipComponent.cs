using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Oxide.Game.Rust.Cui;

public class CuiTooltipComponent : ICuiComponent, ICuiEnableable
{
	public string Type => "Tooltip";

	[JsonProperty("text")]
	public string Text { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("tooltipType")]
	public CommunityEntity.TooltipType TooltipType { get; set; }

	[JsonProperty("offset")]
	public string Offset { get; set; }

	[JsonProperty("useCentre")]
	public bool? UseCentre { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("delay")]
	public Tooltip.DelayType Delay { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("position")]
	public TooltipContainer.PositionMode Position { get; set; }

	public bool? Enabled { get; set; }
}
