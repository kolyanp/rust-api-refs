using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine.UI;

namespace Oxide.Game.Rust.Cui;

public class CuiContentSizeFitterComponent : ICuiComponent, ICuiEnableable
{
	public string Type => "UnityEngine.UI.ContentSizeFitter";

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("horizontalFit")]
	public FitMode HorizontalFit
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

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("verticalFit")]
	public FitMode VerticalFit
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

	public bool? Enabled { get; set; }
}
