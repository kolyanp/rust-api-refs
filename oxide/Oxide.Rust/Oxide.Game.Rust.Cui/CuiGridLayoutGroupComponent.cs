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

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("startCorner")]
	public Corner StartCorner { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("startAxis")]
	public Axis StartAxis { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("childAlignment")]
	public TextAnchor ChildAlignment { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("constraint")]
	public Constraint Constraint { get; set; }

	[JsonProperty("constraintCount")]
	public int ConstraintCount { get; set; }

	[JsonProperty("padding")]
	public string Padding { get; set; }

	public bool? Enabled { get; set; }
}
