using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class BoundsConverter : JsonConverter<Bounds>
{
	public override void WriteJson(JsonWriter writer, Bounds value, JsonSerializer serializer)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("center");
		UnityJsonConverters.WriteVector3(writer, value.center);
		writer.WritePropertyName("size");
		UnityJsonConverters.WriteVector3(writer, value.size);
		writer.WriteEndObject();
	}

	public override Bounds ReadJson(JsonReader reader, Type objectType, Bounds existingValue, bool hasExistingValue, JsonSerializer serializer)
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
		if (!UnityJsonConverters.BeginObject(reader, "Bounds"))
		{
			return default;
		}
		Vector3 val = Vector3.zero;
		Vector3 val2 = Vector3.zero;
		string name;
		while (UnityJsonConverters.NextProperty(reader, out name))
		{
			if (!(name == "center"))
			{
				if (name == "size")
				{
					val2 = (UnityJsonConverters.NextValue(reader) ? UnityJsonConverters.ReadVector3(reader) : Vector3.zero);
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
		return new Bounds(val, val2);
	}
}
