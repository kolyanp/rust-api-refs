using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if ((int)reader.TokenType == 2)
		{
			JArray val = JArray.Load(reader);
			return new Color32((byte)(int)val[0], (byte)(int)val[1], (byte)(int)val[2], (((JContainer)val).Count > 3) ? ((byte)(int)val[3]) : byte.MaxValue);
		}
		JObject token = JObject.Load(reader);
		return new Color32((byte)UnityJsonConverters.I((JToken)(object)token, "r"), (byte)UnityJsonConverters.I((JToken)(object)token, "g"), (byte)UnityJsonConverters.I((JToken)(object)token, "b"), (byte)UnityJsonConverters.I((JToken)(object)token, "a", 255));
	}
}
