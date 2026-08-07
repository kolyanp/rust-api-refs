using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if ((int)reader.TokenType == 2)
		{
			JArray val = JArray.Load(reader);
			return new Vector4((float)val[0], (float)val[1], (float)val[2], (float)val[3]);
		}
		JObject token = JObject.Load(reader);
		return new Vector4(UnityJsonConverters.F((JToken)(object)token, "x"), UnityJsonConverters.F((JToken)(object)token, "y"), UnityJsonConverters.F((JToken)(object)token, "z"), UnityJsonConverters.F((JToken)(object)token, "w"));
	}
}
