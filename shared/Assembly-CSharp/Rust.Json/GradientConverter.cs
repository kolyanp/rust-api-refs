using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class GradientConverter : JsonConverter<Gradient>
{
	public override void WriteJson(JsonWriter writer, Gradient value, JsonSerializer serializer)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected I4, but got Unknown
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
			UnityJsonConverters.WriteColor(writer, val.color);
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
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected Obj, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (!UnityJsonConverters.BeginObject(reader, "Gradient"))
		{
			return null;
		}
		GradientColorKey[] array = Array.Empty<GradientColorKey>();
		GradientAlphaKey[] array2 = Array.Empty<GradientAlphaKey>();
		GradientMode mode = (GradientMode)0;
		string name;
		while (UnityJsonConverters.NextProperty(reader, out name))
		{
			switch (name)
			{
			case "colorKeys":
				array = ReadColorKeys(reader);
				break;
			case "alphaKeys":
				array2 = ReadAlphaKeys(reader);
				break;
			case "mode":
				mode = (GradientMode)UnityJsonConverters.Int(reader);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		Gradient val = new Gradient
		{
			mode = mode
		};
		val.SetKeys(array, array2);
		return val;
	}

	private static bool BeginKeys(JsonReader reader)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if (!UnityJsonConverters.NextValue(reader))
		{
			return false;
		}
		if ((int)reader.TokenType != 2)
		{
			reader.Skip();
			return false;
		}
		return true;
	}

	private static void BeginKey(JsonReader reader, string typeName)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)reader.TokenType != 1)
		{
			throw UnityJsonConverters.Unexpected(reader, typeName);
		}
	}

	private static GradientColorKey[] ReadColorKeys(JsonReader reader)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Invalid comparison between Unknown and I4
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (!BeginKeys(reader))
		{
			return Array.Empty<GradientColorKey>();
		}
		List<GradientColorKey> list = null;
		while (reader.Read() && (int)reader.TokenType != 14)
		{
			BeginKey(reader, "GradientColorKey");
			Color val = Color.white;
			float num = 0f;
			string name;
			while (UnityJsonConverters.NextProperty(reader, out name))
			{
				if (!(name == "color"))
				{
					if (name == "time")
					{
						num = UnityJsonConverters.Float(reader);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					val = (UnityJsonConverters.NextValue(reader) ? UnityJsonConverters.ReadColor(reader) : Color.white);
				}
			}
			if (list == null)
			{
				list = new List<GradientColorKey>();
			}
			list.Add(new GradientColorKey(val, num));
		}
		if (list != null)
		{
			return list.ToArray();
		}
		return Array.Empty<GradientColorKey>();
	}

	private static GradientAlphaKey[] ReadAlphaKeys(JsonReader reader)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Invalid comparison between Unknown and I4
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (!BeginKeys(reader))
		{
			return Array.Empty<GradientAlphaKey>();
		}
		List<GradientAlphaKey> list = null;
		while (reader.Read() && (int)reader.TokenType != 14)
		{
			BeginKey(reader, "GradientAlphaKey");
			float num = 1f;
			float num2 = 0f;
			string name;
			while (UnityJsonConverters.NextProperty(reader, out name))
			{
				if (!(name == "alpha"))
				{
					if (name == "time")
					{
						num2 = UnityJsonConverters.Float(reader);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					num = UnityJsonConverters.Float(reader, 1f);
				}
			}
			if (list == null)
			{
				list = new List<GradientAlphaKey>();
			}
			list.Add(new GradientAlphaKey(num, num2));
		}
		if (list != null)
		{
			return list.ToArray();
		}
		return Array.Empty<GradientAlphaKey>();
	}
}
