using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public class CuiLayoutElementComponent : ICuiComponent, ICuiEnableable
{
	public string Type => "UnityEngine.UI.LayoutElement";

	[JsonProperty("preferredWidth")]
	public float PreferredWidth { get; set; }

	[JsonProperty("preferredHeight")]
	public float PreferredHeight { get; set; }

	[JsonProperty("minWidth")]
	public float MinWidth { get; set; }

	[JsonProperty("minHeight")]
	public float MinHeight { get; set; }

	[JsonProperty("flexibleWidth")]
	public float FlexibleWidth { get; set; }

	[JsonProperty("flexibleHeight")]
	public float FlexibleHeight { get; set; }

	[JsonProperty("ignoreLayout")]
	public bool? IgnoreLayout { get; set; }

	public bool? Enabled { get; set; }
}
