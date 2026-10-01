using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class Vector2IntConverter : JsonConverter<Vector2Int>
{
	public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(value.x);
		writer.WritePropertyName("y");
		writer.WriteValue(value.y);
		writer.WriteEndObject();
	}

	public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return UnityJsonConverters.ReadVector2Int(reader);
	}
}
