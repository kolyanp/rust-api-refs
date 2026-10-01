using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class QuaternionConverter : JsonConverter<Quaternion>
{
	public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		UnityJsonConverters.WriteQuaternion(writer, value);
	}

	public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return UnityJsonConverters.ReadQuaternion(reader);
	}
}
