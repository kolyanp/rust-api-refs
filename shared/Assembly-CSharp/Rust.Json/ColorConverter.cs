using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class ColorConverter : JsonConverter<Color>
{
	public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		UnityJsonConverters.WriteColor(writer, value);
	}

	public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return UnityJsonConverters.ReadColor(reader);
	}
}
