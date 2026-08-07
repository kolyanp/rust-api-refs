using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public class CuiRectTransform
{
	[JsonProperty("anchormin")]
	public string AnchorMin { get; set; }

	[JsonProperty("anchormax")]
	public string AnchorMax { get; set; }

	[JsonProperty("offsetmin")]
	public string OffsetMin { get; set; }

	[JsonProperty("offsetmax")]
	public string OffsetMax { get; set; }

	[JsonProperty("rotation")]
	public float Rotation { get; set; }

	[JsonProperty("pivot")]
	public string Pivot { get; set; }

	[JsonProperty("setParent")]
	public string SetParent { get; set; }

	[JsonProperty("setTransformIndex")]
	public int SetTransformIndex { get; set; }
}
