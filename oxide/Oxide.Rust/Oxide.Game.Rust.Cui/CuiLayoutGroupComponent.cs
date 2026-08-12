using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Oxide.Game.Rust.Cui;

public abstract class CuiLayoutGroupComponent : ICuiComponent, ICuiEnableable
{
	[CompilerGenerated]
	private TextAnchor _003CChildAlignment_003Ek__BackingField;

	public abstract string Type { get; }

	[JsonProperty("spacing")]
	public float Spacing { get; set; }

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
