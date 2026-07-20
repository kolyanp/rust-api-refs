using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Oxide.Core.Configuration;

public class KeyValuesConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		if (!(objectType == typeof(Dictionary<string, object>)))
		{
			return objectType == typeof(List<object>);
		}
		return true;
	}

	private void Throw(string message)
	{
		throw new Exception(message);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Invalid comparison between Unknown and I4
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Expected I4, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected I4, but got Unknown
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		if (objectType == typeof(Dictionary<string, object>))
		{
			Dictionary<string, object> dictionary = (existingValue as Dictionary<string, object>) ?? new Dictionary<string, object>();
			if ((int)reader.TokenType == 2)
			{
				return dictionary;
			}
			while (reader.Read() && (int)reader.TokenType != 13)
			{
				if ((int)reader.TokenType != 4)
				{
					Throw("Unexpected token: " + ((object)reader.TokenType/*cast due to constrained. prefix*/).ToString());
				}
				string key = reader.Value as string;
				if (!reader.Read())
				{
					Throw("Unexpected end of json");
				}
				JsonToken tokenType = reader.TokenType;
				switch (tokenType - 1)
				{
				case 0:
					dictionary[key] = serializer.Deserialize<Dictionary<string, object>>(reader);
					break;
				case 1:
					dictionary[key] = serializer.Deserialize<List<object>>(reader);
					break;
				case 6:
				{
					string text = reader.Value.ToString();
					if (int.TryParse(text, out var result))
					{
						dictionary[key] = result;
					}
					else
					{
						dictionary[key] = text;
					}
					break;
				}
				case 7:
				case 8:
				case 9:
				case 10:
				case 15:
				case 16:
					dictionary[key] = reader.Value;
					break;
				default:
					Throw("Unexpected token: " + ((object)reader.TokenType/*cast due to constrained. prefix*/).ToString());
					break;
				}
			}
			return dictionary;
		}
		if (objectType == typeof(List<object>))
		{
			List<object> list = (existingValue as List<object>) ?? new List<object>();
			while (reader.Read() && (int)reader.TokenType != 14)
			{
				JsonToken tokenType2 = reader.TokenType;
				switch (tokenType2 - 1)
				{
				case 0:
					list.Add(serializer.Deserialize<Dictionary<string, object>>(reader));
					break;
				case 1:
					list.Add(serializer.Deserialize<List<object>>(reader));
					break;
				case 6:
				{
					string text2 = reader.Value.ToString();
					if (int.TryParse(text2, out var result2))
					{
						list.Add(result2);
					}
					else
					{
						list.Add(text2);
					}
					break;
				}
				case 7:
				case 8:
				case 9:
				case 10:
				case 15:
				case 16:
					list.Add(reader.Value);
					break;
				default:
					Throw("Unexpected token: " + ((object)reader.TokenType/*cast due to constrained. prefix*/).ToString());
					break;
				}
			}
			return list;
		}
		return existingValue;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value is Dictionary<string, object>)
		{
			Dictionary<string, object> source = (Dictionary<string, object>)value;
			writer.WriteStartObject();
			foreach (KeyValuePair<string, object> item in source.OrderBy(delegate(KeyValuePair<string, object> i)
			{
				KeyValuePair<string, object> keyValuePair = i;
				return keyValuePair.Key;
			}))
			{
				writer.WritePropertyName(item.Key, true);
				serializer.Serialize(writer, item.Value);
			}
			writer.WriteEndObject();
		}
		else
		{
			if (!(value is List<object>))
			{
				return;
			}
			List<object> list = (List<object>)value;
			writer.WriteStartArray();
			foreach (object item2 in list)
			{
				serializer.Serialize(writer, item2);
			}
			writer.WriteEndArray();
		}
	}
}
