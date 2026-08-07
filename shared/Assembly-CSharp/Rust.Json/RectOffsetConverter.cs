using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		if ((int)reader.TokenType == 11)
		{
			return null;
		}
		JObject token = JObject.Load(reader);
		return new RectOffset(UnityJsonConverters.I((JToken)(object)token, "left"), UnityJsonConverters.I((JToken)(object)token, "right"), UnityJsonConverters.I((JToken)(object)token, "top"), UnityJsonConverters.I((JToken)(object)token, "bottom"));
	}
}
