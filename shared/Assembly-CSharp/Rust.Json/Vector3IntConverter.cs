using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class Vector3IntConverter : JsonConverter<Vector3Int>
{
	public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(((Vector3Int)(ref value)).x);
		writer.WritePropertyName("y");
		writer.WriteValue(((Vector3Int)(ref value)).y);
		writer.WritePropertyName("z");
		writer.WriteValue(((Vector3Int)(ref value)).z);
		writer.WriteEndObject();
	}

	public override Vector3Int ReadJson(JsonReader reader, Type objectType, Vector3Int existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if ((int)reader.TokenType == 2)
		{
			JArray val = JArray.Load(reader);
			return new Vector3Int((int)val[0], (int)val[1], (int)val[2]);
		}
		JObject token = JObject.Load(reader);
		return new Vector3Int(UnityJsonConverters.I((JToken)(object)token, "x"), UnityJsonConverters.I((JToken)(object)token, "y"), UnityJsonConverters.I((JToken)(object)token, "z"));
	}
}
