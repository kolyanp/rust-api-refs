using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Oxide.Game.Rust.Cui;

public class CuiTextComponent : ICuiComponent, ICuiColor, ICuiEnableable, ICuiGraphic
{
	[CompilerGenerated]
	private TextAnchor _003CAlign_003Ek__BackingField;

	[CompilerGenerated]
	private VerticalWrapMode _003CVerticalOverflow_003Ek__BackingField;

	public string Type => "UnityEngine.UI.Text";

	[JsonProperty("text")]
	public string Text { get; set; }

	[JsonProperty("fontSize")]
	public int FontSize { get; set; }

	[JsonProperty("font")]
	public string Font { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("align")]
	public TextAnchor Align
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CAlign_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CAlign_003Ek__BackingField = value;
		}
	}

	public string Color { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("verticalOverflow")]
	public VerticalWrapMode VerticalOverflow
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CVerticalOverflow_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CVerticalOverflow_003Ek__BackingField = value;
		}
	}

	public float FadeIn { get; set; }

	public string PlaceholderParentId { get; set; }

	public bool? BlocksRaycast { get; set; }

	public bool? Enabled { get; set; }
}
