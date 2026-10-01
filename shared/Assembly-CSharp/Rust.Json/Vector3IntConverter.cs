using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class Vector3IntConverter : JsonConverter<Vector3Int>
{
	public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		UnityJsonConverters.WriteVector3Int(writer, value);
	}

	public override Vector3Int ReadJson(JsonReader reader, Type objectType, Vector3Int existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return UnityJsonConverters.ReadVector3Int(reader);
	}
}
