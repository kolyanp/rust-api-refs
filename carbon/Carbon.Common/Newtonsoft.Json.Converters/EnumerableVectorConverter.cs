using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Newtonsoft.Json.Converters;

public class EnumerableVectorConverter<T> : JsonConverter
{
	private static readonly VectorConverter VectorConverter = new VectorConverter();

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		T[] array = (value as IEnumerable<T>)?.ToArray();
		if (array == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < array.Length; i++)
		{
			((JsonConverter)VectorConverter).WriteJson(writer, (object)array[i], serializer);
		}
		writer.WriteEndArray();
	}

	public override bool CanConvert(Type objectType)
	{
		if (!typeof(IEnumerable<Vector2>).IsAssignableFrom(objectType) && !typeof(IEnumerable<Vector3>).IsAssignableFrom(objectType))
		{
			return typeof(IEnumerable<Vector4>).IsAssignableFrom(objectType);
		}
		return true;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)reader.TokenType == 11)
		{
			return null;
		}
		JArray val = JArray.Load(reader);
		List<T> list = new List<T>(((JContainer)val).Count);
		for (int i = 0; i < ((JContainer)val).Count; i++)
		{
			list.Add(JsonConvert.DeserializeObject<T>(((object)val[i]).ToString()));
		}
		return list;
	}
}
