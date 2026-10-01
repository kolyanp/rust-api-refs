using System;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class LayerMaskConverter : JsonConverter<LayerMask>
{
	public override void WriteJson(JsonWriter writer, LayerMask value, JsonSerializer serializer)
	{
		writer.WriteValue(value.value);
	}

	public override LayerMask ReadJson(JsonReader reader, Type objectType, LayerMask existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		JsonToken tokenType = reader.TokenType;
		if ((int)tokenType != 1)
		{
			if (tokenType - 7 > 1)
			{
				if ((int)tokenType == 9)
				{
					return LayerMask.op_Implicit(LayerMask.GetMask(new string[1] { (string)reader.Value }));
				}
				reader.Skip();
				return default;
			}
			return LayerMask.op_Implicit(Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture));
		}
		int num = 0;
		string name;
		while (UnityJsonConverters.NextProperty(reader, out name))
		{
			if (name == "value")
			{
				num = UnityJsonConverters.Int(reader);
			}
			else
			{
				reader.Skip();
			}
		}
		return LayerMask.op_Implicit(num);
	}
}
