using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class GradientConverter : JsonConverter<Gradient>
{
	public override void WriteJson(JsonWriter writer, Gradient value, JsonSerializer serializer)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected I4, but got Unknown
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteStartObject();
		writer.WritePropertyName("colorKeys");
		writer.WriteStartArray();
		GradientColorKey[] colorKeys = value.colorKeys;
		foreach (GradientColorKey val in colorKeys)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("color");
			serializer.Serialize(writer, (object)val.color);
			writer.WritePropertyName("time");
			writer.WriteValue(val.time);
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
		writer.WritePropertyName("alphaKeys");
		writer.WriteStartArray();
		GradientAlphaKey[] alphaKeys = value.alphaKeys;
		foreach (GradientAlphaKey val2 in alphaKeys)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("alpha");
			writer.WriteValue(val2.alpha);
			writer.WritePropertyName("time");
			writer.WriteValue(val2.time);
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
		writer.WritePropertyName("mode");
		writer.WriteValue((int)value.mode);
		writer.WriteEndObject();
	}

	public override Gradient ReadJson(JsonReader reader, Type objectType, Gradient existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if ((int)reader.TokenType == 11)
		{
			return null;
		}
		JObject val = JObject.Load(reader);
		GradientColorKey[] array = Array.Empty<GradientColorKey>();
		JToken obj = val["colorKeys"];
		JArray val2 = (JArray)(object)((obj is JArray) ? obj : null);
		if (val2 != null)
		{
			array = (GradientColorKey[])(object)new GradientColorKey[((JContainer)val2).Count];
			for (int i = 0; i < ((JContainer)val2).Count; i++)
			{
				GradientColorKey[] array2 = array;
				int num = i;
				JToken obj2 = val2[i][(object)"color"];
				array2[num] = new GradientColorKey((obj2 != null) ? obj2.ToObject<Color>(serializer) : Color.white, UnityJsonConverters.F(val2[i], "time"));
			}
		}
		GradientAlphaKey[] array3 = Array.Empty<GradientAlphaKey>();
		JToken obj3 = val["alphaKeys"];
		JArray val3 = (JArray)(object)((obj3 is JArray) ? obj3 : null);
		if (val3 != null)
		{
			array3 = (GradientAlphaKey[])(object)new GradientAlphaKey[((JContainer)val3).Count];
			for (int j = 0; j < ((JContainer)val3).Count; j++)
			{
				array3[j] = new GradientAlphaKey(UnityJsonConverters.F(val3[j], "alpha", 1f), UnityJsonConverters.F(val3[j], "time"));
			}
		}
		Gradient val4 = new Gradient
		{
			mode = (GradientMode)UnityJsonConverters.I((JToken)(object)val, "mode")
		};
		val4.SetKeys(array, array3);
		return val4;
	}
}
