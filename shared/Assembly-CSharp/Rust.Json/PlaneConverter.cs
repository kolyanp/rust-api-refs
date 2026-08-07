using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class PlaneConverter : JsonConverter<Plane>
{
	public override void WriteJson(JsonWriter writer, Plane value, JsonSerializer serializer)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("normal");
		serializer.Serialize(writer, (object)((Plane)(ref value)).normal);
		writer.WritePropertyName("distance");
		writer.WriteValue(((Plane)(ref value)).distance);
		writer.WriteEndObject();
	}

	public override Plane ReadJson(JsonReader reader, Type objectType, Plane existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		JObject val = JObject.Load(reader);
		JToken obj = val["normal"];
		return new Plane((obj != null) ? obj.ToObject<Vector3>(serializer) : Vector3.up, UnityJsonConverters.F((JToken)(object)val, "distance"));
	}
}
