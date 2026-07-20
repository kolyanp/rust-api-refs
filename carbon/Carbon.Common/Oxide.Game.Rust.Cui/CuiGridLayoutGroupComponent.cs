using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.UI;

namespace Oxide.Game.Rust.Cui;

public class CuiGridLayoutGroupComponent : ICuiComponent, ICuiEnableable
{
	public string Type => "UnityEngine.UI.GridLayoutGroup";

	[JsonProperty("cellSize")]
	public string CellSize { get; set; }

	[JsonProperty("spacing")]
	public string Spacing { get; set; }

	[JsonProperty("startCorner")]
	[JsonConverter(typeof(StringEnumConverter))]
	public Corner StartCorner { get; set; }

	[JsonProperty("startAxis")]
	[JsonConverter(typeof(StringEnumConverter))]
	public Axis StartAxis { get; set; }

	[JsonProperty("childAlignment")]
	[JsonConverter(typeof(StringEnumConverter))]
	public TextAnchor ChildAlignment { get; set; }

	[JsonProperty("constraint")]
	[JsonConverter(typeof(StringEnumConverter))]
	public Constraint Constraint { get; set; }

	[JsonProperty("constraintCount")]
	public int ConstraintCount { get; set; }

	[JsonProperty("padding")]
	public string Padding { get; set; }

	[JsonProperty("enabled")]
	public bool? Enabled { get; set; }
}
