using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Shims;
using UnityEngine;

namespace Newtonsoft.Json.Converters;

[Preserve]
public class VectorConverter : JsonConverter
{
	private static readonly Type V2 = typeof(Vector2);

	private static readonly Type V3 = typeof(Vector3);

	private static readonly Type V4 = typeof(Vector4);

	public bool EnableVector2 { get; set; }

	public bool EnableVector3 { get; set; }

	public bool EnableVector4 { get; set; }

	public VectorConverter()
	{
		EnableVector2 = true;
		EnableVector3 = true;
		EnableVector4 = true;
	}

	public VectorConverter(bool enableVector2, bool enableVector3, bool enableVector4)
		: this()
	{
		EnableVector2 = enableVector2;
		EnableVector3 = enableVector3;
		EnableVector4 = enableVector4;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		Type type = value.GetType();
		if (type == V2)
		{
			Vector2 val = (Vector2)value;
			WriteVector(writer, val.x, val.y, null, null);
		}
		else if (type == V3)
		{
			Vector3 val2 = (Vector3)value;
			WriteVector(writer, val2.x, val2.y, val2.z, null);
		}
		else if (type == V4)
		{
			Vector4 val3 = (Vector4)value;
			WriteVector(writer, val3.x, val3.y, val3.z, val3.w);
		}
		else
		{
			writer.WriteNull();
		}
	}

	private static void WriteVector(JsonWriter writer, float x, float y, float? z, float? w)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(x);
		writer.WritePropertyName("y");
		writer.WriteValue(y);
		if (z.HasValue)
		{
			writer.WritePropertyName("z");
			writer.WriteValue(z.Value);
			if (w.HasValue)
			{
				writer.WritePropertyName("w");
				writer.WriteValue(w.Value);
			}
		}
		writer.WriteEndObject();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (objectType == V2)
		{
			return PopulateVector2(reader);
		}
		if (objectType == V3)
		{
			return PopulateVector3(reader);
		}
		return PopulateVector4(reader);
	}

	public override bool CanConvert(Type objectType)
	{
		if ((!EnableVector2 || !(objectType == V2)) && (!EnableVector3 || !(objectType == V3)))
		{
			if (EnableVector4)
			{
				return objectType == V4;
			}
			return false;
		}
		return true;
	}

	private static Vector2 PopulateVector2(JsonReader reader)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vector2 result = default(Vector2);
		if ((int)reader.TokenType != 11)
		{
			JObject val = JObject.Load(reader);
			result.x = Extensions.Value<float>((IEnumerable<JToken>)val["x"]);
			result.y = Extensions.Value<float>((IEnumerable<JToken>)val["y"]);
		}
		return result;
	}

	private static Vector3 PopulateVector3(JsonReader reader)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 result = default(Vector3);
		if ((int)reader.TokenType != 11)
		{
			JObject val = JObject.Load(reader);
			result.x = Extensions.Value<float>((IEnumerable<JToken>)val["x"]);
			result.y = Extensions.Value<float>((IEnumerable<JToken>)val["y"]);
			result.z = Extensions.Value<float>((IEnumerable<JToken>)val["z"]);
		}
		return result;
	}

	private static Vector4 PopulateVector4(JsonReader reader)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Vector4 result = default(Vector4);
		if ((int)reader.TokenType != 11)
		{
			JObject val = JObject.Load(reader);
			result.x = Extensions.Value<float>((IEnumerable<JToken>)val["x"]);
			result.y = Extensions.Value<float>((IEnumerable<JToken>)val["y"]);
			result.z = Extensions.Value<float>((IEnumerable<JToken>)val["z"]);
			result.w = Extensions.Value<float>((IEnumerable<JToken>)val["w"]);
		}
		return result;
	}
}
