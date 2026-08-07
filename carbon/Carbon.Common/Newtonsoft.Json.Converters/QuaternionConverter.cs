using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Newtonsoft.Json.Converters;

public class QuaternionConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = (Quaternion)value;
		writer.WriteStartObject();
		writer.WritePropertyName("w");
		writer.WriteValue(val.w);
		writer.WritePropertyName("x");
		writer.WriteValue(val.x);
		writer.WritePropertyName("y");
		writer.WriteValue(val.y);
		writer.WritePropertyName("z");
		writer.WriteValue(val.z);
		writer.WritePropertyName("eulerAngles");
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(((Quaternion)(ref val)).eulerAngles.x);
		writer.WritePropertyName("y");
		writer.WriteValue(((Quaternion)(ref val)).eulerAngles.y);
		writer.WritePropertyName("z");
		writer.WriteValue(((Quaternion)(ref val)).eulerAngles.z);
		writer.WriteEndObject();
		writer.WriteEndObject();
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Quaternion);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		JObject val = JObject.Load(reader);
		List<JProperty> source = val.Properties().ToList();
		Quaternion val2 = default(Quaternion);
		if (source.Any((JProperty p) => p.Name == "w"))
		{
			val2.w = (float)val["w"];
		}
		if (source.Any((JProperty p) => p.Name == "x"))
		{
			val2.x = (float)val["x"];
		}
		if (source.Any((JProperty p) => p.Name == "y"))
		{
			val2.y = (float)val["y"];
		}
		if (source.Any((JProperty p) => p.Name == "z"))
		{
			val2.z = (float)val["z"];
		}
		if (source.Any((JProperty p) => p.Name == "eulerAngles"))
		{
			JToken val3 = val["eulerAngles"];
			((Quaternion)(ref val2)).eulerAngles = new Vector3((float)val3[(object)"x"], (float)val3[(object)"y"], (float)val3[(object)"z"]);
		}
		return val2;
	}
}
