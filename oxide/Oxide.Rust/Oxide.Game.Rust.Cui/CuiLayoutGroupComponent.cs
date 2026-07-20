using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Oxide.Game.Rust.Cui;

public abstract class CuiLayoutGroupComponent : ICuiComponent, ICuiEnableable
{
	public abstract string Type { get; }

	[JsonProperty("spacing")]
	public float Spacing { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("childAlignment")]
	public TextAnchor ChildAlignment { get; set; }

	[JsonProperty("childForceExpandWidth")]
	public bool? ChildForceExpandWidth { get; set; }

	[JsonProperty("childForceExpandHeight")]
	public bool? ChildForceExpandHeight { get; set; }

	[JsonProperty("childControlWidth")]
	public bool? ChildControlWidth { get; set; }

	[JsonProperty("childControlHeight")]
	public bool? ChildControlHeight { get; set; }

	[JsonProperty("childScaleWidth")]
	public bool? ChildScaleWidth { get; set; }

	[JsonProperty("childScaleHeight")]
	public bool? ChildScaleHeight { get; set; }

	[JsonProperty("padding")]
	public string Padding { get; set; }

	public bool? Enabled { get; set; }
}
