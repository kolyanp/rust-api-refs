using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class Matrix4x4Converter : JsonConverter<Matrix4x4>
{
	public override void WriteJson(JsonWriter writer, Matrix4x4 value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				writer.WritePropertyName($"m{i}{j}");
				writer.WriteValue(((Matrix4x4)(ref value))[i, j]);
			}
		}
		writer.WriteEndObject();
	}

	public override Matrix4x4 ReadJson(JsonReader reader, Type objectType, Matrix4x4 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 identity = Matrix4x4.identity;
		if ((int)reader.TokenType == 2)
		{
			JArray val = JArray.Load(reader);
			for (int i = 0; i < 16 && i < ((JContainer)val).Count; i++)
			{
				((Matrix4x4)(ref identity))[i / 4, i % 4] = (float)val[i];
			}
			return identity;
		}
		JObject token = JObject.Load(reader);
		for (int j = 0; j < 4; j++)
		{
			for (int k = 0; k < 4; k++)
			{
				((Matrix4x4)(ref identity))[j, k] = UnityJsonConverters.F((JToken)(object)token, $"m{j}{k}", ((Matrix4x4)(ref identity))[j, k]);
			}
		}
		return identity;
	}
}
