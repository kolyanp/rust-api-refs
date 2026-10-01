using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class Vector4Converter : JsonConverter<Vector4>
{
	public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(value.x);
		writer.WritePropertyName("y");
		writer.WriteValue(value.y);
		writer.WritePropertyName("z");
		writer.WriteValue(value.z);
		writer.WritePropertyName("w");
		writer.WriteValue(value.w);
		writer.WriteEndObject();
	}

	public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		JsonToken tokenType = reader.TokenType;
		if ((int)tokenType != 2)
		{
			if ((int)tokenType == 11)
			{
				return Vector4.zero;
			}
			UnityJsonConverters.BeginObject(reader, "Vector4");
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			string name;
			while (UnityJsonConverters.NextProperty(reader, out name))
			{
				switch (name)
				{
				case "x":
					num = UnityJsonConverters.Float(reader);
					break;
				case "y":
					num2 = UnityJsonConverters.Float(reader);
					break;
				case "z":
					num3 = UnityJsonConverters.Float(reader);
					break;
				case "w":
					num4 = UnityJsonConverters.Float(reader);
					break;
				default:
					reader.Skip();
					break;
				}
			}
			return new Vector4(num, num2, num3, num4);
		}
		Span<float> values = stackalloc float[4];
		UnityJsonConverters.ReadFloats(reader, values);
		return new Vector4(values[0], values[1], values[2], values[3]);
	}
}
