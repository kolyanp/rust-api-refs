using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class RectIntConverter : JsonConverter<RectInt>
{
	public override void WriteJson(JsonWriter writer, RectInt value, JsonSerializer serializer)
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

	public override RectInt ReadJson(JsonReader reader, Type objectType, RectInt existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (!UnityJsonConverters.BeginObject(reader, "RectInt"))
		{
			return default;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		string name;
		while (UnityJsonConverters.NextProperty(reader, out name))
		{
			switch (name)
			{
			case "x":
				num = UnityJsonConverters.Int(reader);
				break;
			case "y":
				num2 = UnityJsonConverters.Int(reader);
				break;
			case "width":
				num3 = UnityJsonConverters.Int(reader);
				break;
			case "height":
				num4 = UnityJsonConverters.Int(reader);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return new RectInt(num, num2, num3, num4);
	}
}
