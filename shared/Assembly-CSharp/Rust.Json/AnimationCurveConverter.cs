using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class AnimationCurveConverter : JsonConverter<AnimationCurve>
{
	public override void WriteJson(JsonWriter writer, AnimationCurve value, JsonSerializer serializer)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected I4, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected I4, but got Unknown
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteStartObject();
		writer.WritePropertyName("keys");
		serializer.Serialize(writer, (object)value.keys);
		writer.WritePropertyName("preWrapMode");
		writer.WriteValue((int)value.preWrapMode);
		writer.WritePropertyName("postWrapMode");
		writer.WriteValue((int)value.postWrapMode);
		writer.WriteEndObject();
	}

	public override AnimationCurve ReadJson(JsonReader reader, Type objectType, AnimationCurve existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		if ((int)reader.TokenType == 11)
		{
			return null;
		}
		JObject val = JObject.Load(reader);
		JToken obj = val["keys"];
		return new AnimationCurve(((obj != null) ? obj.ToObject<Keyframe[]>(serializer) : null) ?? Array.Empty<Keyframe>())
		{
			preWrapMode = (WrapMode)UnityJsonConverters.I((JToken)(object)val, "preWrapMode", 8),
			postWrapMode = (WrapMode)UnityJsonConverters.I((JToken)(object)val, "postWrapMode", 8)
		};
	}
}
