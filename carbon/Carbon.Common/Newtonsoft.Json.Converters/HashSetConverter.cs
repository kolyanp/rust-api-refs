using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Converters;

public class HashSetConverter : JsonConverter
{
	public override bool CanWrite => false;

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		bool flag = (int)serializer.ObjectCreationHandling == 2;
		if ((int)reader.TokenType == 11)
		{
			if (!flag)
			{
				return existingValue;
			}
			return null;
		}
		object obj = ((!flag && existingValue != null) ? existingValue : Activator.CreateInstance(objectType));
		Type type = objectType.GetGenericArguments()[0];
		MethodInfo method = objectType.GetMethod("Add");
		JArray val = JArray.Load(reader);
		for (int i = 0; i < ((JContainer)val).Count; i++)
		{
			method.Invoke(obj, new object[1] { serializer.Deserialize(val[i].CreateReader(), type) });
		}
		return obj;
	}

	public override bool CanConvert(Type objectType)
	{
		if (objectType.IsGenericType)
		{
			return objectType.GetGenericTypeDefinition() == typeof(HashSet<>);
		}
		return false;
	}
}
