using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class QuaternionConverter : JsonConverter<Quaternion>
{
	public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(value.x);
		writer.WritePropertyName("y");
		writer.WriteValue(value.y);
		writer.WritePropertyName("z");
		writer.WriteValue(value.z);
		writer.WritePropertyName("w");
		writer.WriteValue(value.w);
		writer.WriteEndObject();
	}

	public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if ((int)reader.TokenType == 2)
		{
			JArray val = JArray.Load(reader);
			return new Quaternion((float)val[0], (float)val[1], (float)val[2], (float)val[3]);
		}
		JObject val2 = JObject.Load(reader);
		JToken val3 = val2["eulerAngles"];
		if (val3 != null)
		{
			return Quaternion.Euler(UnityJsonConverters.F(val3, "x"), UnityJsonConverters.F(val3, "y"), UnityJsonConverters.F(val3, "z"));
		}
		return new Quaternion(UnityJsonConverters.F((JToken)(object)val2, "x"), UnityJsonConverters.F((JToken)(object)val2, "y"), UnityJsonConverters.F((JToken)(object)val2, "z"), UnityJsonConverters.F((JToken)(object)val2, "w", 1f));
	}
}
