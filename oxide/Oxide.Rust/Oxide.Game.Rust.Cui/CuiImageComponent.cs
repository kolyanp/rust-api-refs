using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine.UI;

namespace Oxide.Game.Rust.Cui;

public class CuiImageComponent : ICuiComponent, ICuiColor, ICuiEnableable, ICuiGraphic
{
	[CompilerGenerated]
	private Type _003CImageType_003Ek__BackingField;

	public string Type => "UnityEngine.UI.Image";

	[JsonProperty("sprite")]
	public string Sprite { get; set; }

	[JsonProperty("material")]
	public string Material { get; set; }

	public string Color { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("imagetype")]
	public Type ImageType
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CImageType_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CImageType_003Ek__BackingField = value;
		}
	}

	[JsonProperty("fillCenter")]
	public bool? FillCenter { get; set; }

	[JsonProperty("png")]
	public string Png { get; set; }

	[JsonProperty("slice")]
	public string Slice { get; set; }

	[JsonProperty("itemid")]
	public int ItemId { get; set; }

	[JsonProperty("skinid")]
	public ulong SkinId { get; set; }

	[JsonProperty("ppuMultiplier")]
	public float PixelsPerUnitMultiplier { get; set; }

	public float FadeIn { get; set; }

	public string PlaceholderParentId { get; set; }

	public bool? BlocksRaycast { get; set; }

	public bool? Enabled { get; set; }
}
