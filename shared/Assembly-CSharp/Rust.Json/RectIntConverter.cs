using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class RectIntConverter : JsonConverter<RectInt>
{
	public override void WriteJson(JsonWriter writer, RectInt value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(((RectInt)(ref value)).x);
		writer.WritePropertyName("y");
		writer.WriteValue(((RectInt)(ref value)).y);
		writer.WritePropertyName("width");
		writer.WriteValue(((RectInt)(ref value)).width);
		writer.WritePropertyName("height");
		writer.WriteValue(((RectInt)(ref value)).height);
		writer.WriteEndObject();
	}

	public override RectInt ReadJson(JsonReader reader, Type objectType, RectInt existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		JObject token = JObject.Load(reader);
		return new RectInt(UnityJsonConverters.I((JToken)(object)token, "x"), UnityJsonConverters.I((JToken)(object)token, "y"), UnityJsonConverters.I((JToken)(object)token, "width"), UnityJsonConverters.I((JToken)(object)token, "height"));
	}
}
