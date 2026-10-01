using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class Hash128Converter : JsonConverter<Hash128>
{
	public override void WriteJson(JsonWriter writer, Hash128 value, JsonSerializer serializer)
	{
		writer.WriteValue(((object)value/*cast due to constrained. prefix*/).ToString());
	}

	public override Hash128 ReadJson(JsonReader reader, Type objectType, Hash128 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if ((int)reader.TokenType == 9)
		{
			return Hash128.Parse((string)reader.Value);
		}
		reader.Skip();
		return default;
	}
}
