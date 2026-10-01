using System.Runtime.CompilerServices;
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
	public Corner StartCorner
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	[JsonProperty("startAxis")]
	[JsonConverter(typeof(StringEnumConverter))]
	public Axis StartAxis
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	[JsonProperty("childAlignment")]
	[JsonConverter(typeof(StringEnumConverter))]
	public TextAnchor ChildAlignment
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	[JsonProperty("constraint")]
	[JsonConverter(typeof(StringEnumConverter))]
	public Constraint Constraint
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	[JsonProperty("constraintCount")]
	public int ConstraintCount { get; set; }

	[JsonProperty("padding")]
	public string Padding { get; set; }

	[JsonProperty("enabled")]
	public bool? Enabled { get; set; }
}
