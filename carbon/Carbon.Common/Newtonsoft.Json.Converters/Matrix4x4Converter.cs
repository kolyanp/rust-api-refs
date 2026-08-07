using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Newtonsoft.Json.Converters;

public class Matrix4x4Converter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		Matrix4x4 val = (Matrix4x4)value;
		writer.WriteStartObject();
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				writer.WritePropertyName($"m{i}{j}");
				writer.WriteValue(((Matrix4x4)(ref val))[i, j]);
			}
		}
		writer.WriteEnd();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if ((int)reader.TokenType == 11)
		{
			return (object)default(Matrix4x4);
		}
		JObject val = JObject.Load(reader);
		Matrix4x4 val2 = default(Matrix4x4);
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				((Matrix4x4)(ref val2))[i, j] = (float)val[$"m{i}{j}"];
			}
		}
		return val2;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Matrix4x4);
	}
}
