using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class RectConverter : JsonConverter<Rect>
{
	public override void WriteJson(JsonWriter writer, Rect value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(((Rect)(ref value)).x);
		writer.WritePropertyName("y");
		writer.WriteValue(((Rect)(ref value)).y);
		writer.WritePropertyName("width");
		writer.WriteValue(((Rect)(ref value)).width);
		writer.WritePropertyName("height");
		writer.WriteValue(((Rect)(ref value)).height);
		writer.WriteEndObject();
	}

	public override Rect ReadJson(JsonReader reader, Type objectType, Rect existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if ((int)reader.TokenType == 2)
		{
			JArray val = JArray.Load(reader);
			return new Rect((float)val[0], (float)val[1], (float)val[2], (float)val[3]);
		}
		JObject token = JObject.Load(reader);
		return new Rect(UnityJsonConverters.F((JToken)(object)token, "x"), UnityJsonConverters.F((JToken)(object)token, "y"), UnityJsonConverters.F((JToken)(object)token, "width"), UnityJsonConverters.F((JToken)(object)token, "height"));
	}
}
