using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class RectConverter : JsonConverter<Rect>
{
	public override void WriteJson(JsonWriter writer, Rect value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(value.x);
		writer.WritePropertyName("y");
		writer.WriteValue(value.y);
		writer.WritePropertyName("width");
		writer.WriteValue(value.width);
		writer.WritePropertyName("height");
		writer.WriteValue(value.height);
		writer.WriteEndObject();
	}

	public override Rect ReadJson(JsonReader reader, Type objectType, Rect existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		JsonToken tokenType = reader.TokenType;
		if ((int)tokenType != 2)
		{
			if ((int)tokenType == 11)
			{
				return default;
			}
			UnityJsonConverters.BeginObject(reader, "Rect");
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
				case "width":
					num3 = UnityJsonConverters.Float(reader);
					break;
				case "height":
					num4 = UnityJsonConverters.Float(reader);
					break;
				default:
					reader.Skip();
					break;
				}
			}
			return new Rect(num, num2, num3, num4);
		}
		Span<float> values = stackalloc float[4];
		UnityJsonConverters.ReadFloats(reader, values);
		return new Rect(values[0], values[1], values[2], values[3]);
	}
}
