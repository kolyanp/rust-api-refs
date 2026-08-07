using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Newtonsoft.Json.Converters;

public class ColorConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		Color val = (Color)value;
		writer.WriteStartObject();
		writer.WritePropertyName("a");
		writer.WriteValue(val.a);
		writer.WritePropertyName("r");
		writer.WriteValue(val.r);
		writer.WritePropertyName("g");
		writer.WriteValue(val.g);
		writer.WritePropertyName("b");
		writer.WriteValue(val.b);
		writer.WriteEndObject();
	}

	public override bool CanConvert(Type objectType)
	{
		if (!(objectType == typeof(Color)))
		{
			return objectType == typeof(Color32);
		}
		return true;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if ((int)reader.TokenType == 11)
		{
			return (object)default(Color);
		}
		JObject val = JObject.Load(reader);
		if (objectType == typeof(Color32))
		{
			return (object)new Color32((byte)val["r"], (byte)val["g"], (byte)val["b"], (byte)val["a"]);
		}
		return (object)new Color((float)val["r"], (float)val["g"], (float)val["b"], (float)val["a"]);
	}
}
