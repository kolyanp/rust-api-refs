using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class AnimationCurveConverter : JsonConverter<AnimationCurve>
{
	public override void WriteJson(JsonWriter writer, AnimationCurve value, JsonSerializer serializer)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected I4, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected I4, but got Unknown
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteStartObject();
		writer.WritePropertyName("keys");
		writer.WriteStartArray();
		for (int i = 0; i < value.length; i++)
		{
			UnityJsonConverters.WriteKeyframe(writer, value[i]);
		}
		writer.WriteEndArray();
		writer.WritePropertyName("preWrapMode");
		writer.WriteValue((int)value.preWrapMode);
		writer.WritePropertyName("postWrapMode");
		writer.WriteValue((int)value.postWrapMode);
		writer.WriteEndObject();
	}

	public override AnimationCurve ReadJson(JsonReader reader, Type objectType, AnimationCurve existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected Obj, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (!UnityJsonConverters.BeginObject(reader, "AnimationCurve"))
		{
			return null;
		}
		Keyframe[] array = Array.Empty<Keyframe>();
		WrapMode preWrapMode = (WrapMode)8;
		WrapMode postWrapMode = (WrapMode)8;
		string name;
		while (UnityJsonConverters.NextProperty(reader, out name))
		{
			switch (name)
			{
			case "keys":
				array = ReadKeys(reader);
				break;
			case "preWrapMode":
				preWrapMode = (WrapMode)UnityJsonConverters.Int(reader, 8);
				break;
			case "postWrapMode":
				postWrapMode = (WrapMode)UnityJsonConverters.Int(reader, 8);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return new AnimationCurve(array)
		{
			preWrapMode = preWrapMode,
			postWrapMode = postWrapMode
		};
	}

	private static Keyframe[] ReadKeys(JsonReader reader)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Invalid comparison between Unknown and I4
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!UnityJsonConverters.NextValue(reader) || (int)reader.TokenType == 11)
		{
			return Array.Empty<Keyframe>();
		}
		if ((int)reader.TokenType != 2)
		{
			throw UnityJsonConverters.Unexpected(reader, "keys");
		}
		List<Keyframe> list = null;
		while (reader.Read() && (int)reader.TokenType != 14)
		{
			if (list == null)
			{
				list = new List<Keyframe>();
			}
			list.Add(UnityJsonConverters.ReadKeyframe(reader));
		}
		if (list != null)
		{
			return list.ToArray();
		}
		return Array.Empty<Keyframe>();
	}
}
