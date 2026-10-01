using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class PoseConverter : JsonConverter<Pose>
{
	public override void WriteJson(JsonWriter writer, Pose value, JsonSerializer serializer)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("position");
		UnityJsonConverters.WriteVector3(writer, value.position);
		writer.WritePropertyName("rotation");
		UnityJsonConverters.WriteQuaternion(writer, value.rotation);
		writer.WriteEndObject();
	}

	public override Pose ReadJson(JsonReader reader, Type objectType, Pose existingValue, bool hasExistingValue, JsonSerializer serializer)
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
		if (!UnityJsonConverters.BeginObject(reader, "Pose"))
		{
			return default;
		}
		Vector3 val = Vector3.zero;
		Quaternion val2 = Quaternion.identity;
		string name;
		while (UnityJsonConverters.NextProperty(reader, out name))
		{
			if (!(name == "position"))
			{
				if (name == "rotation")
				{
					val2 = (UnityJsonConverters.NextValue(reader) ? UnityJsonConverters.ReadQuaternion(reader) : Quaternion.identity);
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
		return new Pose(val, val2);
	}
}
