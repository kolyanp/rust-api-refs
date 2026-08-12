using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.UI;

namespace Oxide.Game.Rust.Cui;

public class CuiGridLayoutGroupComponent : ICuiComponent, ICuiEnableable
{
	[CompilerGenerated]
	private Corner _003CStartCorner_003Ek__BackingField;

	[CompilerGenerated]
	private Axis _003CStartAxis_003Ek__BackingField;

	[CompilerGenerated]
	private TextAnchor _003CChildAlignment_003Ek__BackingField;

	[CompilerGenerated]
	private Constraint _003CConstraint_003Ek__BackingField;

	public string Type => "UnityEngine.UI.GridLayoutGroup";

	[JsonProperty("cellSize")]
	public string CellSize { get; set; }

	[JsonProperty("spacing")]
	public string Spacing { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("startCorner")]
	public Corner StartCorner
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CStartCorner_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CStartCorner_003Ek__BackingField = value;
		}
	}

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("startAxis")]
	public Axis StartAxis
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CStartAxis_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CStartAxis_003Ek__BackingField = value;
		}
	}

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("childAlignment")]
	public TextAnchor ChildAlignment
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CChildAlignment_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CChildAlignment_003Ek__BackingField = value;
		}
	}

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("constraint")]
	public Constraint Constraint
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CConstraint_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CConstraint_003Ek__BackingField = value;
		}
	}

	[JsonProperty("constraintCount")]
	public int ConstraintCount { get; set; }

	[JsonProperty("padding")]
	public string Padding { get; set; }

	public bool? Enabled { get; set; }
}
