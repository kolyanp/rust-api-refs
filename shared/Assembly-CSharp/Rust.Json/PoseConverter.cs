using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class PoseConverter : JsonConverter<Pose>
{
	public override void WriteJson(JsonWriter writer, Pose value, JsonSerializer serializer)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("position");
		serializer.Serialize(writer, (object)value.position);
		writer.WritePropertyName("rotation");
		serializer.Serialize(writer, (object)value.rotation);
		writer.WriteEndObject();
	}

	public override Pose ReadJson(JsonReader reader, Type objectType, Pose existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		JObject obj = JObject.Load(reader);
		JToken obj2 = obj["position"];
		Vector3 val = ((obj2 != null) ? obj2.ToObject<Vector3>(serializer) : Vector3.zero);
		JToken obj3 = obj["rotation"];
		Quaternion val2 = ((obj3 != null) ? obj3.ToObject<Quaternion>(serializer) : Quaternion.identity);
		return new Pose(val, val2);
	}
}
