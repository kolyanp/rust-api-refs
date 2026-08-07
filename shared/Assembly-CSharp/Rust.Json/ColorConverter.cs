using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class ColorConverter : JsonConverter<Color>
{
	public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
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

	public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if ((int)reader.TokenType == 2)
		{
			JArray val = JArray.Load(reader);
			return new Color((float)val[0], (float)val[1], (float)val[2], (((JContainer)val).Count > 3) ? ((float)val[3]) : 1f);
		}
		JObject token = JObject.Load(reader);
		return new Color(UnityJsonConverters.F((JToken)(object)token, "r"), UnityJsonConverters.F((JToken)(object)token, "g"), UnityJsonConverters.F((JToken)(object)token, "b"), UnityJsonConverters.F((JToken)(object)token, "a", 1f));
	}
}
