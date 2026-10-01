using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.UI;

namespace Oxide.Game.Rust.Cui;

public class CuiInputFieldComponent : ICuiComponent, ICuiColor, ICuiEnableable, ICuiGraphic
{
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

	public string Color { get; set; }

	[JsonProperty("characterLimit")]
	public int CharsLimit { get; set; }

	[JsonProperty("command")]
	public string Command { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public bool IsPassword { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public bool ReadOnly { get; set; }

	[JsonProperty("placeholderId")]
	public string PlaceholderId { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public bool NeedsKeyboard { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("lineType")]
	public LineType LineType
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

	[JsonProperty("autofocus")]
	public bool Autofocus { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public bool HudMenuInput { get; set; }

	[JsonProperty("fadeIn")]
	public float FadeIn { get; set; }

	[JsonProperty("interactable")]
	public bool? Interactable { get; set; }

	[JsonProperty("placeholderParentId")]
	public string PlaceholderParentId { get; set; }

	[JsonProperty("blocksRaycast")]
	public bool? BlocksRaycast { get; set; }

	[JsonProperty("enabled")]
	public bool? Enabled { get; set; }
}
