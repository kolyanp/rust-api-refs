using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Newtonsoft.Json.Converters;

public class ResolutionConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Resolution val = (Resolution)value;
		writer.WriteStartObject();
		writer.WritePropertyName("height");
		writer.WriteValue(((Resolution)(ref val)).height);
		writer.WritePropertyName("width");
		writer.WriteValue(((Resolution)(ref val)).width);
		writer.WritePropertyName("refreshRate");
		writer.WriteValue(((Resolution)(ref val)).refreshRate);
		writer.WriteEndObject();
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Resolution);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		JObject val = JObject.Load(reader);
		Resolution val2 = default(Resolution);
		((Resolution)(ref val2)).height = (int)val["height"];
		((Resolution)(ref val2)).width = (int)val["width"];
		((Resolution)(ref val2)).refreshRate = (int)val["refreshRate"];
		return val2;
	}
}
