using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class Vector2IntConverter : JsonConverter<Vector2Int>
{
	public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(((Vector2Int)(ref value)).x);
		writer.WritePropertyName("y");
		writer.WriteValue(((Vector2Int)(ref value)).y);
		writer.WriteEndObject();
	}

	public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if ((int)reader.TokenType == 2)
		{
			JArray val = JArray.Load(reader);
			return new Vector2Int((int)val[0], (int)val[1]);
		}
		JObject token = JObject.Load(reader);
		return new Vector2Int(UnityJsonConverters.I((JToken)(object)token, "x"), UnityJsonConverters.I((JToken)(object)token, "y"));
	}
}
