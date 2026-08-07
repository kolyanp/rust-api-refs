using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class Vector3Converter : JsonConverter<Vector3>
{
	public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(value.x);
		writer.WritePropertyName("y");
		writer.WriteValue(value.y);
		writer.WritePropertyName("z");
		writer.WriteValue(value.z);
		writer.WriteEndObject();
	}

	public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if ((int)reader.TokenType == 2)
		{
			JArray val = JArray.Load(reader);
			return new Vector3((float)val[0], (float)val[1], (float)val[2]);
		}
		JObject token = JObject.Load(reader);
		return new Vector3(UnityJsonConverters.F((JToken)(object)token, "x"), UnityJsonConverters.F((JToken)(object)token, "y"), UnityJsonConverters.F((JToken)(object)token, "z"));
	}
}
