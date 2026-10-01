using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class ResolutionConverter : JsonConverter<Resolution>
{
	public override void WriteJson(JsonWriter writer, Resolution value, JsonSerializer serializer)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("width");
		writer.WriteValue(value.width);
		writer.WritePropertyName("height");
		writer.WriteValue(value.height);
		writer.WritePropertyName("refreshRate");
		RefreshRate refreshRateRatio = value.refreshRateRatio;
		writer.WriteValue(refreshRateRatio.value);
		writer.WriteEndObject();
	}

	public override Resolution ReadJson(JsonReader reader, Type objectType, Resolution existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Resolution result;
		if (!UnityJsonConverters.BeginObject(reader, "Resolution"))
		{
			result = default;
			return result;
		}
		int width = 0;
		int height = 0;
		string name;
		while (UnityJsonConverters.NextProperty(reader, out name))
		{
			if (!(name == "width"))
			{
				if (name == "height")
				{
					height = UnityJsonConverters.Int(reader);
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				width = UnityJsonConverters.Int(reader);
			}
		}
		result = default;
		result.width = width;
		result.height = height;
		return result;
	}
}
