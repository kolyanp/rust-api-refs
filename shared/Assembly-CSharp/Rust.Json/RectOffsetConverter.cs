using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class RectOffsetConverter : JsonConverter<RectOffset>
{
	public override void WriteJson(JsonWriter writer, RectOffset value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteStartObject();
		writer.WritePropertyName("left");
		writer.WriteValue(value.left);
		writer.WritePropertyName("right");
		writer.WriteValue(value.right);
		writer.WritePropertyName("top");
		writer.WriteValue(value.top);
		writer.WritePropertyName("bottom");
		writer.WriteValue(value.bottom);
		writer.WriteEndObject();
	}

	public override RectOffset ReadJson(JsonReader reader, Type objectType, RectOffset existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected Obj, but got Unknown
		if (!UnityJsonConverters.BeginObject(reader, "RectOffset"))
		{
			return null;
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
			case "left":
				num = UnityJsonConverters.Int(reader);
				break;
			case "right":
				num2 = UnityJsonConverters.Int(reader);
				break;
			case "top":
				num3 = UnityJsonConverters.Int(reader);
				break;
			case "bottom":
				num4 = UnityJsonConverters.Int(reader);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return new RectOffset(num, num2, num3, num4);
	}
}
