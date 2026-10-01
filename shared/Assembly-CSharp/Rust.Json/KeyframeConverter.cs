using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class KeyframeConverter : JsonConverter<Keyframe>
{
	public override void WriteJson(JsonWriter writer, Keyframe value, JsonSerializer serializer)
	{
		UnityJsonConverters.WriteKeyframe(writer, in value);
	}

	public override Keyframe ReadJson(JsonReader reader, Type objectType, Keyframe existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return UnityJsonConverters.ReadKeyframe(reader);
	}
}
