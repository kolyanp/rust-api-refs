using System;
using System.Globalization;
using System.Numerics;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public static class UnityJsonConverters
{
	private static readonly JsonConverter[] all = new JsonConverter[24]
	{
		(JsonConverter)new Vector2Converter(),
		(JsonConverter)new Vector3Converter(),
		(JsonConverter)new Vector4Converter(),
		(JsonConverter)new Vector2IntConverter(),
		(JsonConverter)new Vector3IntConverter(),
		(JsonConverter)new QuaternionConverter(),
		(JsonConverter)new Matrix4x4Converter(),
		(JsonConverter)new ColorConverter(),
		(JsonConverter)new Color32Converter(),
		(JsonConverter)new RectConverter(),
		(JsonConverter)new RectIntConverter(),
		(JsonConverter)new RectOffsetConverter(),
		(JsonConverter)new BoundsConverter(),
		(JsonConverter)new BoundsIntConverter(),
		(JsonConverter)new LayerMaskConverter(),
		(JsonConverter)new RayConverter(),
		(JsonConverter)new Ray2DConverter(),
		(JsonConverter)new PlaneConverter(),
		(JsonConverter)new PoseConverter(),
		(JsonConverter)new Hash128Converter(),
		(JsonConverter)new ResolutionConverter(),
		(JsonConverter)new KeyframeConverter(),
		(JsonConverter)new AnimationCurveConverter(),
		(JsonConverter)new GradientConverter()
	};

	public static JsonConverter[] All => all;

	internal static bool NextProperty(JsonReader reader, out string name)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		while (reader.Read())
		{
			JsonToken tokenType = reader.TokenType;
			if ((int)tokenType != 4)
			{
				if ((int)tokenType != 5)
				{
					if ((int)tokenType == 13)
					{
						name = null;
						return false;
					}
					throw Unexpected(reader, "object members");
				}
				continue;
			}
			name = (string)reader.Value;
			return true;
		}
		name = null;
		return false;
	}

	internal static float Float(JsonReader reader, float fallback = 0f)
	{
		object obj = ReadScalar(reader);
		if (obj != null)
		{
			return Convert.ToSingle(obj, CultureInfo.InvariantCulture);
		}
		return fallback;
	}

	internal static int Int(JsonReader reader, int fallback = 0)
	{
		object obj = ReadScalar(reader);
		if (obj != null)
		{
			return Convert.ToInt32(obj, CultureInfo.InvariantCulture);
		}
		return fallback;
	}

	internal static ulong ULong(JsonReader reader, ulong fallback = 0uL)
	{
		object obj = ReadScalar(reader);
		if (obj != null)
		{
			if (obj is BigInteger bigInteger)
			{
				return (ulong)bigInteger;
			}
			return Convert.ToUInt64(obj, CultureInfo.InvariantCulture);
		}
		return fallback;
	}

	internal static bool Bool(JsonReader reader, bool fallback = false)
	{
		object obj = ReadScalar(reader);
		if (obj != null)
		{
			return Convert.ToBoolean(obj, CultureInfo.InvariantCulture);
		}
		return fallback;
	}

	internal static string String(JsonReader reader)
	{
		object obj = ReadScalar(reader);
		if (obj != null)
		{
			if (obj is string result)
			{
				return result;
			}
			return Convert.ToString(obj, CultureInfo.InvariantCulture);
		}
		return null;
	}

	private static object ReadScalar(JsonReader reader)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		reader.Read();
		JsonToken tokenType = reader.TokenType;
		if (tokenType - 7 > 3)
		{
			if (tokenType - 11 <= 1)
			{
				return null;
			}
			throw Unexpected(reader, "a number");
		}
		return reader.Value;
	}

	internal static int ReadFloats(JsonReader reader, Span<float> values)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		int num = 0;
		while (reader.Read() && (int)reader.TokenType != 14)
		{
			if ((int)reader.TokenType == 1 || (int)reader.TokenType == 2)
			{
				reader.Skip();
				continue;
			}
			if (num < values.Length)
			{
				values[num] = ((reader.Value == null) ? 0f : Convert.ToSingle(reader.Value, CultureInfo.InvariantCulture));
			}
			num++;
		}
		return num;
	}

	internal static int ReadInts(JsonReader reader, Span<int> values)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		int num = 0;
		while (reader.Read() && (int)reader.TokenType != 14)
		{
			if ((int)reader.TokenType == 1 || (int)reader.TokenType == 2)
			{
				reader.Skip();
				continue;
			}
			if (num < values.Length)
			{
				values[num] = ((reader.Value != null) ? Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture) : 0);
			}
			num++;
		}
		return num;
	}

	internal static bool NextValue(JsonReader reader)
	{
		return reader.Read();
	}

	internal static bool BeginObject(JsonReader reader, string typeName)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		if ((int)reader.TokenType == 11)
		{
			return false;
		}
		if ((int)reader.TokenType != 1)
		{
			throw Unexpected(reader, typeName);
		}
		return true;
	}

	internal static JsonSerializationException Unexpected(JsonReader reader, string typeName)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected Obj, but got Unknown
		return new JsonSerializationException($"Unexpected token {reader.TokenType} when reading {typeName}. Path '{reader.Path}'.");
	}

	internal static Vector2 ReadVector2(JsonReader reader)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		JsonToken tokenType = reader.TokenType;
		if ((int)tokenType != 1)
		{
			if ((int)tokenType != 2)
			{
				if ((int)tokenType == 11)
				{
					return Vector2.zero;
				}
				throw Unexpected(reader, "Vector2");
			}
			Span<float> values = stackalloc float[2];
			ReadFloats(reader, values);
			return new Vector2(values[0], values[1]);
		}
		float num = 0f;
		float num2 = 0f;
		string name;
		while (NextProperty(reader, out name))
		{
			if (!(name == "x"))
			{
				if (name == "y")
				{
					num2 = Float(reader);
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				num = Float(reader);
			}
		}
		return new Vector2(num, num2);
	}

	internal static Vector3 ReadVector3(JsonReader reader)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		JsonToken tokenType = reader.TokenType;
		if ((int)tokenType != 1)
		{
			if ((int)tokenType != 2)
			{
				if ((int)tokenType == 11)
				{
					return Vector3.zero;
				}
				throw Unexpected(reader, "Vector3");
			}
			Span<float> values = stackalloc float[3];
			ReadFloats(reader, values);
			return new Vector3(values[0], values[1], values[2]);
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		string name;
		while (NextProperty(reader, out name))
		{
			switch (name)
			{
			case "x":
				num = Float(reader);
				break;
			case "y":
				num2 = Float(reader);
				break;
			case "z":
				num3 = Float(reader);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return new Vector3(num, num2, num3);
	}

	internal static Vector2Int ReadVector2Int(JsonReader reader)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		JsonToken tokenType = reader.TokenType;
		if ((int)tokenType != 1)
		{
			if ((int)tokenType != 2)
			{
				if ((int)tokenType == 11)
				{
					return Vector2Int.zero;
				}
				throw Unexpected(reader, "Vector2Int");
			}
			Span<int> values = stackalloc int[2];
			ReadInts(reader, values);
			return new Vector2Int(values[0], values[1]);
		}
		int num = 0;
		int num2 = 0;
		string name;
		while (NextProperty(reader, out name))
		{
			if (!(name == "x"))
			{
				if (name == "y")
				{
					num2 = Int(reader);
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				num = Int(reader);
			}
		}
		return new Vector2Int(num, num2);
	}

	internal static Vector3Int ReadVector3Int(JsonReader reader)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		JsonToken tokenType = reader.TokenType;
		if ((int)tokenType != 1)
		{
			if ((int)tokenType != 2)
			{
				if ((int)tokenType == 11)
				{
					return Vector3Int.zero;
				}
				throw Unexpected(reader, "Vector3Int");
			}
			Span<int> values = stackalloc int[3];
			ReadInts(reader, values);
			return new Vector3Int(values[0], values[1], values[2]);
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		string name;
		while (NextProperty(reader, out name))
		{
			switch (name)
			{
			case "x":
				num = Int(reader);
				break;
			case "y":
				num2 = Int(reader);
				break;
			case "z":
				num3 = Int(reader);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return new Vector3Int(num, num2, num3);
	}

	internal static Quaternion ReadQuaternion(JsonReader reader)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Invalid comparison between Unknown and I4
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		JsonToken tokenType = reader.TokenType;
		if ((int)tokenType != 1)
		{
			if ((int)tokenType != 2)
			{
				if ((int)tokenType == 11)
				{
					return Quaternion.identity;
				}
				throw Unexpected(reader, "Quaternion");
			}
			Span<float> values = stackalloc float[4];
			ReadFloats(reader, values);
			return new Quaternion(values[0], values[1], values[2], values[3]);
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 1f;
		bool flag = false;
		Vector3 val = Vector3.zero;
		string name;
		while (NextProperty(reader, out name))
		{
			switch (name)
			{
			case "x":
				num = Float(reader);
				break;
			case "y":
				num2 = Float(reader);
				break;
			case "z":
				num3 = Float(reader);
				break;
			case "w":
				num4 = Float(reader, 1f);
				break;
			case "eulerAngles":
				if (NextValue(reader) && (int)reader.TokenType != 11)
				{
					flag = true;
					val = ReadVector3(reader);
				}
				break;
			default:
				reader.Skip();
				break;
			}
		}
		if (!flag)
		{
			return new Quaternion(num, num2, num3, num4);
		}
		return Quaternion.Euler(val);
	}

	internal static Color ReadColor(JsonReader reader)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		JsonToken tokenType = reader.TokenType;
		if ((int)tokenType != 1)
		{
			if ((int)tokenType != 2)
			{
				if ((int)tokenType == 11)
				{
					return default;
				}
				throw Unexpected(reader, "Color");
			}
			Span<float> values = stackalloc float[4];
			int num = ReadFloats(reader, values);
			return new Color(values[0], values[1], values[2], (num > 3) ? values[3] : 1f);
		}
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 1f;
		string name;
		while (NextProperty(reader, out name))
		{
			switch (name)
			{
			case "r":
				num2 = Float(reader);
				break;
			case "g":
				num3 = Float(reader);
				break;
			case "b":
				num4 = Float(reader);
				break;
			case "a":
				num5 = Float(reader, 1f);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return new Color(num2, num3, num4, num5);
	}

	internal static Keyframe ReadKeyframe(JsonReader reader)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		Keyframe result;
		if ((int)reader.TokenType == 11)
		{
			result = default;
			return result;
		}
		if ((int)reader.TokenType != 1)
		{
			throw Unexpected(reader, "Keyframe");
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		WeightedMode weightedMode = (WeightedMode)0;
		string name;
		while (NextProperty(reader, out name))
		{
			switch (name)
			{
			case "time":
				num = Float(reader);
				break;
			case "value":
				num2 = Float(reader);
				break;
			case "inTangent":
				num3 = Float(reader);
				break;
			case "outTangent":
				num4 = Float(reader);
				break;
			case "inWeight":
				num5 = Float(reader);
				break;
			case "outWeight":
				num6 = Float(reader);
				break;
			case "weightedMode":
				weightedMode = (WeightedMode)Int(reader);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		result = new Keyframe(num, num2, num3, num4, num5, num6);
		result.weightedMode = weightedMode;
		return result;
	}

	internal static void WriteVector2(JsonWriter writer, Vector2 value)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(value.x);
		writer.WritePropertyName("y");
		writer.WriteValue(value.y);
		writer.WriteEndObject();
	}

	internal static void WriteVector3(JsonWriter writer, Vector3 value)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(value.x);
		writer.WritePropertyName("y");
		writer.WriteValue(value.y);
		writer.WritePropertyName("z");
		writer.WriteValue(value.z);
		writer.WriteEndObject();
	}

	internal static void WriteVector3Int(JsonWriter writer, Vector3Int value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(value.x);
		writer.WritePropertyName("y");
		writer.WriteValue(value.y);
		writer.WritePropertyName("z");
		writer.WriteValue(value.z);
		writer.WriteEndObject();
	}

	internal static void WriteQuaternion(JsonWriter writer, Quaternion value)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(value.x);
		writer.WritePropertyName("y");
		writer.WriteValue(value.y);
		writer.WritePropertyName("z");
		writer.WriteValue(value.z);
		writer.WritePropertyName("w");
		writer.WriteValue(value.w);
		writer.WriteEndObject();
	}

	internal static void WriteColor(JsonWriter writer, Color value)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteStartObject();
		writer.WritePropertyName("r");
		writer.WriteValue(value.r);
		writer.WritePropertyName("g");
		writer.WriteValue(value.g);
		writer.WritePropertyName("b");
		writer.WriteValue(value.b);
		writer.WritePropertyName("a");
		writer.WriteValue(value.a);
		writer.WriteEndObject();
	}

	internal static void WriteKeyframe(JsonWriter writer, in Keyframe value)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected I4, but got Unknown
		writer.WriteStartObject();
		writer.WritePropertyName("time");
		Keyframe val = value;
		writer.WriteValue(val.time);
		writer.WritePropertyName("value");
		val = value;
		writer.WriteValue(val.value);
		writer.WritePropertyName("inTangent");
		val = value;
		writer.WriteValue(val.inTangent);
		writer.WritePropertyName("outTangent");
		val = value;
		writer.WriteValue(val.outTangent);
		writer.WritePropertyName("inWeight");
		val = value;
		writer.WriteValue(val.inWeight);
		writer.WritePropertyName("outWeight");
		val = value;
		writer.WriteValue(val.outWeight);
		writer.WritePropertyName("weightedMode");
		val = value;
		writer.WriteValue((int)val.weightedMode);
		writer.WriteEndObject();
	}
}
