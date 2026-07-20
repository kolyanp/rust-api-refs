using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Newtonsoft.Json.Converters;

public class QuaternionConverter : JsonConverter
{
	public override bool CanRead => true;

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
		return (object)objectType == typeof(Quaternion);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		JObject jObject = JObject.Load(reader);
		List<JProperty> source = jObject.Properties().ToList();
		Quaternion val = default(Quaternion);
		if (source.Any((JProperty p) => p.Name == "w"))
		{
			val.w = (float)jObject["w"];
		}
		if (source.Any((JProperty p) => p.Name == "x"))
		{
			val.x = (float)jObject["x"];
		}
		if (source.Any((JProperty p) => p.Name == "y"))
		{
			val.y = (float)jObject["y"];
		}
		if (source.Any((JProperty p) => p.Name == "z"))
		{
			val.z = (float)jObject["z"];
		}
		if (source.Any((JProperty p) => p.Name == "eulerAngles"))
		{
			JToken jToken = jObject["eulerAngles"];
			((Quaternion)(ref val)).eulerAngles = new Vector3
			{
				x = (float)jToken["x"],
				y = (float)jToken["y"],
				z = (float)jToken["z"]
			};
		}
		return val;
	}
}
