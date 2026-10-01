using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class PlaneConverter : JsonConverter<Plane>
{
	public override void WriteJson(JsonWriter writer, Plane value, JsonSerializer serializer)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("normal");
		UnityJsonConverters.WriteVector3(writer, value.normal);
		writer.WritePropertyName("distance");
		writer.WriteValue(value.distance);
		writer.WriteEndObject();
	}

	public override Plane ReadJson(JsonReader reader, Type objectType, Plane existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (!UnityJsonConverters.BeginObject(reader, "Plane"))
		{
			return default;
		}
		Vector3 val = Vector3.up;
		float num = 0f;
		string name;
		while (UnityJsonConverters.NextProperty(reader, out name))
		{
			if (!(name == "normal"))
			{
				if (name == "distance")
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
				val = (UnityJsonConverters.NextValue(reader) ? UnityJsonConverters.ReadVector3(reader) : Vector3.up);
			}
		}
		return new Plane(val, num);
	}
}
