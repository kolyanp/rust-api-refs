using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class Ray2DConverter : JsonConverter<Ray2D>
{
	public override void WriteJson(JsonWriter writer, Ray2D value, JsonSerializer serializer)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("origin");
		serializer.Serialize(writer, (object)((Ray2D)(ref value)).origin);
		writer.WritePropertyName("direction");
		serializer.Serialize(writer, (object)((Ray2D)(ref value)).direction);
		writer.WriteEndObject();
	}

	public override Ray2D ReadJson(JsonReader reader, Type objectType, Ray2D existingValue, bool hasExistingValue, JsonSerializer serializer)
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
		JToken obj2 = obj["origin"];
		Vector2 val = ((obj2 != null) ? obj2.ToObject<Vector2>(serializer) : Vector2.zero);
		JToken obj3 = obj["direction"];
		Vector2 val2 = ((obj3 != null) ? obj3.ToObject<Vector2>(serializer) : Vector2.up);
		return new Ray2D(val, val2);
	}
}
