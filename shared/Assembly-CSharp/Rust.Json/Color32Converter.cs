using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class Color32Converter : JsonConverter<Color32>
{
	public override void WriteJson(JsonWriter writer, Color32 value, JsonSerializer serializer)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("r");
		writer.WriteValue(value.r);
		writer.WritePropertyName("g");
		writer.WriteValue(value.g);
		writer.WritePropertyName("b");
		writer.WriteValue(value.b);
		writer.WritePropertyName("a");
		writer.WriteValue(value.a);
		writer.WriteEndObject();
	}

	public override Color32 ReadJson(JsonReader reader, Type objectType, Color32 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		JsonToken tokenType = reader.TokenType;
		if ((int)tokenType != 2)
		{
			if ((int)tokenType == 11)
			{
				return default;
			}
			UnityJsonConverters.BeginObject(reader, "Color32");
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 255;
			string name;
			while (UnityJsonConverters.NextProperty(reader, out name))
			{
				switch (name)
				{
				case "r":
					num = UnityJsonConverters.Int(reader);
					break;
				case "g":
					num2 = UnityJsonConverters.Int(reader);
					break;
				case "b":
					num3 = UnityJsonConverters.Int(reader);
					break;
				case "a":
					num4 = UnityJsonConverters.Int(reader, 255);
					break;
				default:
					reader.Skip();
					break;
				}
			}
			return new Color32((byte)num, (byte)num2, (byte)num3, (byte)num4);
		}
		Span<int> values = stackalloc int[4];
		int num5 = UnityJsonConverters.ReadInts(reader, values);
		return new Color32((byte)values[0], (byte)values[1], (byte)values[2], (num5 > 3) ? ((byte)values[3]) : byte.MaxValue);
	}
}
