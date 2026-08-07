using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class KeyframeConverter : JsonConverter<Keyframe>
{
	public override void WriteJson(JsonWriter writer, Keyframe value, JsonSerializer serializer)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected I4, but got Unknown
		writer.WriteStartObject();
		writer.WritePropertyName("time");
		writer.WriteValue(((Keyframe)(ref value)).time);
		writer.WritePropertyName("value");
		writer.WriteValue(((Keyframe)(ref value)).value);
		writer.WritePropertyName("inTangent");
		writer.WriteValue(((Keyframe)(ref value)).inTangent);
		writer.WritePropertyName("outTangent");
		writer.WriteValue(((Keyframe)(ref value)).outTangent);
		writer.WritePropertyName("inWeight");
		writer.WriteValue(((Keyframe)(ref value)).inWeight);
		writer.WritePropertyName("outWeight");
		writer.WriteValue(((Keyframe)(ref value)).outWeight);
		writer.WritePropertyName("weightedMode");
		writer.WriteValue((int)((Keyframe)(ref value)).weightedMode);
		writer.WriteEndObject();
	}

	public override Keyframe ReadJson(JsonReader reader, Type objectType, Keyframe existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		JObject token = JObject.Load(reader);
		Keyframe result = default(Keyframe);
		((Keyframe)(ref result))._002Ector(UnityJsonConverters.F((JToken)(object)token, "time"), UnityJsonConverters.F((JToken)(object)token, "value"), UnityJsonConverters.F((JToken)(object)token, "inTangent"), UnityJsonConverters.F((JToken)(object)token, "outTangent"), UnityJsonConverters.F((JToken)(object)token, "inWeight"), UnityJsonConverters.F((JToken)(object)token, "outWeight"));
		((Keyframe)(ref result)).weightedMode = (WeightedMode)UnityJsonConverters.I((JToken)(object)token, "weightedMode");
		return result;
	}
}
