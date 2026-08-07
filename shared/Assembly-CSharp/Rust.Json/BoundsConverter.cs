using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class BoundsConverter : JsonConverter<Bounds>
{
	public override void WriteJson(JsonWriter writer, Bounds value, JsonSerializer serializer)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("center");
		serializer.Serialize(writer, (object)((Bounds)(ref value)).center);
		writer.WritePropertyName("size");
		serializer.Serialize(writer, (object)((Bounds)(ref value)).size);
		writer.WriteEndObject();
	}

	public override Bounds ReadJson(JsonReader reader, Type objectType, Bounds existingValue, bool hasExistingValue, JsonSerializer serializer)
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
		JToken obj2 = obj["center"];
		Vector3 val = ((obj2 != null) ? obj2.ToObject<Vector3>(serializer) : Vector3.zero);
		JToken obj3 = obj["size"];
		Vector3 val2 = ((obj3 != null) ? obj3.ToObject<Vector3>(serializer) : Vector3.zero);
		return new Bounds(val, val2);
	}
}
