using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class Ray2DConverter : JsonConverter<Ray2D>
{
	public override void WriteJson(JsonWriter writer, Ray2D value, JsonSerializer serializer)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("origin");
		UnityJsonConverters.WriteVector2(writer, value.origin);
		writer.WritePropertyName("direction");
		UnityJsonConverters.WriteVector2(writer, value.direction);
		writer.WriteEndObject();
	}

	public override Ray2D ReadJson(JsonReader reader, Type objectType, Ray2D existingValue, bool hasExistingValue, JsonSerializer serializer)
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
		if (!UnityJsonConverters.BeginObject(reader, "Ray2D"))
		{
			return default;
		}
		Vector2 val = Vector2.zero;
		Vector2 val2 = Vector2.up;
		string name;
		while (UnityJsonConverters.NextProperty(reader, out name))
		{
			if (!(name == "origin"))
			{
				if (name == "direction")
				{
					val2 = (UnityJsonConverters.NextValue(reader) ? UnityJsonConverters.ReadVector2(reader) : Vector2.up);
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				val = (UnityJsonConverters.NextValue(reader) ? UnityJsonConverters.ReadVector2(reader) : Vector2.zero);
			}
		}
		return new Ray2D(val, val2);
	}
}
