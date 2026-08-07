using System;

namespace Newtonsoft.Json.Converters;

public class UriConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Uri);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		JsonToken tokenType = reader.TokenType;
		if ((int)tokenType != 9)
		{
			if ((int)tokenType == 11)
			{
				return null;
			}
			throw new InvalidOperationException("Unhandled case for UriConverter. Check to see if this converter has been applied to the wrong serialization type.");
		}
		return new Uri((string)reader.Value);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		if (!(value is Uri uri))
		{
			throw new InvalidOperationException("Unhandled case for UriConverter. Check to see if this converter has been applied to the wrong serialization type.");
		}
		writer.WriteValue(uri.OriginalString);
	}
}
