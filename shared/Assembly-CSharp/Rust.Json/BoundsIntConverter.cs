using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class BoundsIntConverter : JsonConverter<BoundsInt>
{
	public override void WriteJson(JsonWriter writer, BoundsInt value, JsonSerializer serializer)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("position");
		serializer.Serialize(writer, (object)((BoundsInt)(ref value)).position);
		writer.WritePropertyName("size");
		serializer.Serialize(writer, (object)((BoundsInt)(ref value)).size);
		writer.WriteEndObject();
	}

	public override BoundsInt ReadJson(JsonReader reader, Type objectType, BoundsInt existingValue, bool hasExistingValue, JsonSerializer serializer)
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
		Vector3Int val = ((obj2 != null) ? obj2.ToObject<Vector3Int>(serializer) : Vector3Int.zero);
		JToken obj3 = obj["size"];
		Vector3Int val2 = ((obj3 != null) ? obj3.ToObject<Vector3Int>(serializer) : Vector3Int.zero);
		return new BoundsInt(val, val2);
	}
}
