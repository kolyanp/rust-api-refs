using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class Matrix4x4Converter : JsonConverter<Matrix4x4>
{
	private static readonly string[] memberNames = new string[16]
	{
		"m00", "m01", "m02", "m03", "m10", "m11", "m12", "m13", "m20", "m21",
		"m22", "m23", "m30", "m31", "m32", "m33"
	};

	public override void WriteJson(JsonWriter writer, Matrix4x4 value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		for (int i = 0; i < 16; i++)
		{
			writer.WritePropertyName(memberNames[i]);
			writer.WriteValue(value[i / 4, i % 4]);
		}
		writer.WriteEndObject();
	}

	public override Matrix4x4 ReadJson(JsonReader reader, Type objectType, Matrix4x4 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 identity = Matrix4x4.identity;
		if ((int)reader.TokenType == 11)
		{
			return identity;
		}
		if ((int)reader.TokenType == 2)
		{
			Span<float> values = stackalloc float[16];
			int num = UnityJsonConverters.ReadFloats(reader, values);
			for (int i = 0; i < 16 && i < num; i++)
			{
				identity[i / 4, i % 4] = values[i];
			}
			return identity;
		}
		UnityJsonConverters.BeginObject(reader, "Matrix4x4");
		string name;
		while (UnityJsonConverters.NextProperty(reader, out name))
		{
			if (name.Length == 3 && name[0] == 'm')
			{
				int num2 = name[1] - 48;
				int num3 = name[2] - 48;
				if ((uint)num2 < 4u && (uint)num3 < 4u)
				{
					identity[num2, num3] = UnityJsonConverters.Float(reader, identity[num2, num3]);
					continue;
				}
			}
			reader.Skip();
		}
		return identity;
	}
}
