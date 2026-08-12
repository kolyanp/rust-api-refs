using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine.UI;

namespace Oxide.Game.Rust.Cui;

public class CuiScrollViewComponent : ICuiComponent, ICuiEnableable
{
	[CompilerGenerated]
	private MovementType _003CMovementType_003Ek__BackingField;

	public string Type => "UnityEngine.UI.ScrollView";

	[JsonProperty("contentTransform")]
	public CuiRectTransform ContentTransform { get; set; }

	[JsonProperty("horizontal", DefaultValueHandling = DefaultValueHandling.Include)]
	public bool Horizontal { get; set; }

	[JsonProperty("vertical", DefaultValueHandling = DefaultValueHandling.Include)]
	public bool Vertical { get; set; }

	[JsonProperty("movementType")]
	[JsonConverter(typeof(StringEnumConverter))]
	public MovementType MovementType
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CMovementType_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CMovementType_003Ek__BackingField = value;
		}
	}

	[JsonProperty("elasticity")]
	public float Elasticity { get; set; }

	[JsonProperty("inertia", DefaultValueHandling = DefaultValueHandling.Include)]
	public bool Inertia { get; set; }

	[JsonProperty("decelerationRate")]
	public float DecelerationRate { get; set; }

	[JsonProperty("scrollSensitivity")]
	public float ScrollSensitivity { get; set; }

	[JsonProperty("horizontalScrollbar")]
	public CuiScrollbar HorizontalScrollbar { get; set; }

	[JsonProperty("verticalScrollbar")]
	public CuiScrollbar VerticalScrollbar { get; set; }

	[JsonProperty("horizontalNormalizedPosition")]
	public float? HorizontalNormalizedPosition { get; set; }

	[JsonProperty("verticalNormalizedPosition")]
	public float? VerticalNormalizedPosition { get; set; }

	public bool? Enabled { get; set; }
}
