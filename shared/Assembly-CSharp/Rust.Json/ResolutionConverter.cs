using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
		writer.WriteValue(((Resolution)(ref value)).width);
		writer.WritePropertyName("height");
		writer.WriteValue(((Resolution)(ref value)).height);
		writer.WritePropertyName("refreshRate");
		RefreshRate refreshRateRatio = ((Resolution)(ref value)).refreshRateRatio;
		writer.WriteValue(((RefreshRate)(ref refreshRateRatio)).value);
		writer.WriteEndObject();
	}

	public override Resolution ReadJson(JsonReader reader, Type objectType, Resolution existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		JObject token = JObject.Load(reader);
		Resolution result = default(Resolution);
		((Resolution)(ref result)).width = UnityJsonConverters.I((JToken)(object)token, "width");
		((Resolution)(ref result)).height = UnityJsonConverters.I((JToken)(object)token, "height");
		return result;
	}
}
