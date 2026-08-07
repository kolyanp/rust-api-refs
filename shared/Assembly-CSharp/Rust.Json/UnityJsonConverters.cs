using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rust.Json;

public static class UnityJsonConverters
{
	public static JsonConverter[] CreateAll()
	{
		return (JsonConverter[])(object)new JsonConverter[24]
		{
			(JsonConverter)new Vector2Converter(),
			(JsonConverter)new Vector3Converter(),
			(JsonConverter)new Vector4Converter(),
			(JsonConverter)new Vector2IntConverter(),
			(JsonConverter)new Vector3IntConverter(),
			(JsonConverter)new QuaternionConverter(),
			(JsonConverter)new Matrix4x4Converter(),
			(JsonConverter)new ColorConverter(),
			(JsonConverter)new Color32Converter(),
			(JsonConverter)new RectConverter(),
			(JsonConverter)new RectIntConverter(),
			(JsonConverter)new RectOffsetConverter(),
			(JsonConverter)new BoundsConverter(),
			(JsonConverter)new BoundsIntConverter(),
			(JsonConverter)new LayerMaskConverter(),
			(JsonConverter)new RayConverter(),
			(JsonConverter)new Ray2DConverter(),
			(JsonConverter)new PlaneConverter(),
			(JsonConverter)new PoseConverter(),
			(JsonConverter)new Hash128Converter(),
			(JsonConverter)new ResolutionConverter(),
			(JsonConverter)new KeyframeConverter(),
			(JsonConverter)new AnimationCurveConverter(),
			(JsonConverter)new GradientConverter()
		};
	}

	internal static float F(JToken token, string name, float fallback = 0f)
	{
		return ((float?)((token != null) ? token[(object)name] : null)) ?? fallback;
	}

	internal static int I(JToken token, string name, int fallback = 0)
	{
		return ((int?)((token != null) ? token[(object)name] : null)) ?? fallback;
	}
}
