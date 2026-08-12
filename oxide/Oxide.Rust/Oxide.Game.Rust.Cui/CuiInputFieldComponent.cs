using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.UI;

namespace Oxide.Game.Rust.Cui;

public class CuiInputFieldComponent : ICuiComponent, ICuiColor, ICuiEnableable, ICuiGraphic
{
	[CompilerGenerated]
	private TextAnchor _003CAlign_003Ek__BackingField;

	[CompilerGenerated]
	private LineType _003CLineType_003Ek__BackingField;

	public string Type => "UnityEngine.UI.InputField";

	[JsonProperty("text")]
	public string Text { get; set; } = string.Empty;

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

	[JsonProperty("characterLimit")]
	public int CharsLimit { get; set; }

	[JsonProperty("command")]
	public string Command { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("lineType")]
	public LineType LineType
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CLineType_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CLineType_003Ek__BackingField = value;
		}
	}

	[JsonProperty("readOnly", DefaultValueHandling = DefaultValueHandling.Include)]
	public bool ReadOnly { get; set; }

	[JsonProperty("placeholderId")]
	public string PlaceholderId { get; set; }

	[JsonProperty("password", DefaultValueHandling = DefaultValueHandling.Include)]
	public bool IsPassword { get; set; }

	[JsonProperty("needsKeyboard", DefaultValueHandling = DefaultValueHandling.Include)]
	public bool NeedsKeyboard { get; set; }

	[JsonProperty("hudMenuInput", DefaultValueHandling = DefaultValueHandling.Include)]
	public bool HudMenuInput { get; set; }

	[JsonProperty("autofocus")]
	public bool Autofocus { get; set; }

	[JsonProperty("interactable")]
	public bool? Interactable { get; set; }

	public float FadeIn { get; set; }

	public string PlaceholderParentId { get; set; }

	public bool? BlocksRaycast { get; set; }

	public bool? Enabled { get; set; }
}
