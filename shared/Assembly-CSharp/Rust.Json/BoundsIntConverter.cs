using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class BoundsIntConverter : JsonConverter<BoundsInt>
{
	public override void WriteJson(JsonWriter writer, BoundsInt value, JsonSerializer serializer)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("position");
		UnityJsonConverters.WriteVector3Int(writer, value.position);
		writer.WritePropertyName("size");
		UnityJsonConverters.WriteVector3Int(writer, value.size);
		writer.WriteEndObject();
	}

	public override BoundsInt ReadJson(JsonReader reader, Type objectType, BoundsInt existingValue, bool hasExistingValue, JsonSerializer serializer)
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
		if (!UnityJsonConverters.BeginObject(reader, "BoundsInt"))
		{
			return default;
		}
		Vector3Int val = Vector3Int.zero;
		Vector3Int val2 = Vector3Int.zero;
		string name;
		while (UnityJsonConverters.NextProperty(reader, out name))
		{
			if (!(name == "position"))
			{
				if (name == "size")
				{
					val2 = (UnityJsonConverters.NextValue(reader) ? UnityJsonConverters.ReadVector3Int(reader) : Vector3Int.zero);
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				val = (UnityJsonConverters.NextValue(reader) ? UnityJsonConverters.ReadVector3Int(reader) : Vector3Int.zero);
			}
		}
		return new BoundsInt(val, val2);
	}
}
