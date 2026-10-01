using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class Vector3Converter : JsonConverter<Vector3>
{
	public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		UnityJsonConverters.WriteVector3(writer, value);
	}

	public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return UnityJsonConverters.ReadVector3(reader);
	}
}
