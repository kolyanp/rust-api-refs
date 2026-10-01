using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class Vector2Converter : JsonConverter<Vector2>
{
	public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		UnityJsonConverters.WriteVector2(writer, value);
	}

	public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return UnityJsonConverters.ReadVector2(reader);
	}
}
