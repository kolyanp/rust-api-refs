using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class LayerMaskConverter : JsonConverter<LayerMask>
{
	public override void WriteJson(JsonWriter writer, LayerMask value, JsonSerializer serializer)
	{
		writer.WriteValue(((LayerMask)(ref value)).value);
	}

	public override LayerMask ReadJson(JsonReader reader, Type objectType, LayerMask existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		JsonToken tokenType = reader.TokenType;
		if ((int)tokenType != 1)
		{
			if ((int)tokenType != 7)
			{
				if ((int)tokenType == 9)
				{
					return LayerMask.op_Implicit(LayerMask.GetMask(new string[1] { (string)reader.Value }));
				}
				return default(LayerMask);
			}
			return LayerMask.op_Implicit(Convert.ToInt32(reader.Value));
		}
		return LayerMask.op_Implicit(UnityJsonConverters.I((JToken)(object)JObject.Load(reader), "value"));
	}
}
