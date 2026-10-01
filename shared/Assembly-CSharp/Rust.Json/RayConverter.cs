using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class RayConverter : JsonConverter<Ray>
{
	public override void WriteJson(JsonWriter writer, Ray value, JsonSerializer serializer)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("origin");
		UnityJsonConverters.WriteVector3(writer, value.origin);
		writer.WritePropertyName("direction");
		UnityJsonConverters.WriteVector3(writer, value.direction);
		writer.WriteEndObject();
	}

	public override Ray ReadJson(JsonReader reader, Type objectType, Ray existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!UnityJsonConverters.BeginObject(reader, "Ray"))
		{
			return default;
		}
		Vector3 val = Vector3.zero;
		Vector3 val2 = Vector3.forward;
		string name;
		while (UnityJsonConverters.NextProperty(reader, out name))
		{
			if (!(name == "origin"))
			{
				if (name == "direction")
				{
					val2 = (UnityJsonConverters.NextValue(reader) ? UnityJsonConverters.ReadVector3(reader) : Vector3.forward);
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				val = (UnityJsonConverters.NextValue(reader) ? UnityJsonConverters.ReadVector3(reader) : Vector3.zero);
			}
		}
		return new Ray(val, val2);
	}
}
