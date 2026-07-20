using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Carbon.Components;

public static class LUIBuilder
{
	public readonly struct WriteArray(byte[] values, int size)
	{
		public readonly byte[] values = values;

		public readonly int size = size;
	}

	private static readonly char[] charPreset = new char[32];

	private const int segmentCheck = 2048;

	private const char startObject = '{';

	private const char endObject = '}';

	private const char startArray = '[';

	private const char endArray = ']';

	private const char coma = ',';

	private const char mark = '"';

	private const char dots = ':';

	private const char under = '_';

	private const char minus = '-';

	private const char zero = '0';

	private const char dot = '.';

	private const char whitespace = ' ';

	private const float zeroFloat = 0f;

	private const string trueValue = "true";

	private const string falseValue = "false";

	private static readonly long[] _pow10 = new long[10] { 1L, 10L, 100L, 1000L, 10000L, 100000L, 1000000L, 10000000L, 100000000L, 1000000000L };

	private static int stringLengthCheck = Mathf.CeilToInt(3891.2f);

	public static string GetStringFloat(params float[] values)
	{
		char[] array = charPreset;
		int num = 0;
		for (int i = 0; i < values.Length; i++)
		{
			int num2 = WriteFloatDigits(values[i], array, num);
			num += num2;
			if (i < values.Length - 1)
			{
				array[num++] = ' ';
			}
		}
		return new string(array, 0, num);
	}

	public static string BuildElementId(string elementId, int id)
	{
		int num = id;
		int num2 = 0;
		while (num > 0)
		{
			num2++;
			num /= 10;
		}
		char[] array = new char[elementId.Length + 1 + num2];
		int start = 0;
		for (int i = 0; i < elementId.Length; i++)
		{
			array[start++] = elementId[i];
		}
		array[start++] = '_';
		Span<char> buffer = stackalloc char[11];
		buffer[..WriteIntDigits(id, buffer)].CopyTo(array.AsSpan(start));
		return new string(array);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteStartObject(this ref LuiBuilderInstance inst)
	{
		inst.Write('{');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteEndObject(this ref LuiBuilderInstance inst)
	{
		inst.Write('}');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteStartArray(this ref LuiBuilderInstance inst)
	{
		inst.Write('[');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteEndArray(this ref LuiBuilderInstance inst)
	{
		inst.Write(']');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteComma(this ref LuiBuilderInstance inst)
	{
		inst.Write(',');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteMark(this ref LuiBuilderInstance inst)
	{
		inst.Write('"');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteDots(this ref LuiBuilderInstance inst)
	{
		inst.Write(':');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteSpace(this ref LuiBuilderInstance inst)
	{
		inst.Write(' ');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteStartObject(this ref LuiBuilderInstance inst, string key)
	{
		inst.WriteMark();
		inst.Write(key);
		inst.WriteMark();
		inst.WriteDots();
		inst.WriteStartObject();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteStartArray(this ref LuiBuilderInstance inst, string key)
	{
		inst.WriteMark();
		inst.Write(key);
		inst.WriteMark();
		inst.WriteDots();
		inst.WriteStartArray();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteField(this ref LuiBuilderInstance inst, string key, int value)
	{
		inst.WriteMark();
		inst.Write(key);
		inst.WriteMark();
		inst.WriteDots();
		inst.Write(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteField(this ref LuiBuilderInstance inst, string key, Vector2 value)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		inst.WriteMark();
		inst.Write(key);
		inst.WriteMark();
		inst.WriteDots();
		inst.WriteMark();
		inst.Write(value.x);
		inst.WriteSpace();
		inst.Write(value.y);
		inst.WriteMark();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteField(this ref LuiBuilderInstance inst, string key, float value)
	{
		inst.WriteMark();
		inst.Write(key);
		inst.WriteMark();
		inst.WriteDots();
		inst.Write(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteField(this ref LuiBuilderInstance inst, string key, ulong value)
	{
		inst.WriteMark();
		inst.Write(key);
		inst.WriteMark();
		inst.WriteDots();
		inst.Write(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteField(this ref LuiBuilderInstance inst, string key, bool value)
	{
		inst.WriteMark();
		inst.Write(key);
		inst.WriteMark();
		inst.WriteDots();
		if (value)
		{
			inst.Write("true");
		}
		else
		{
			inst.Write("false");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteField(this ref LuiBuilderInstance inst, string key, string value)
	{
		inst.WriteMark();
		inst.Write(key);
		inst.WriteMark();
		inst.WriteDots();
		inst.WriteMark();
		inst.Write(value);
		inst.WriteMark();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Write(this ref LuiBuilderInstance inst, char character)
	{
		inst._charBuffer[inst._charIndex] = character;
		inst._charIndex++;
		if (inst._charIndex >= 2048)
		{
			inst.Flush();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Write(this ref LuiBuilderInstance inst, int value)
	{
		Span<char> buffer = stackalloc char[11];
		int num = WriteIntDigits(value, buffer);
		buffer.Slice(0, num).CopyTo(inst._charBuffer.AsSpan(inst._charIndex));
		inst._charIndex += num;
		if (inst._charIndex >= 2048)
		{
			inst.Flush();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Write(this ref LuiBuilderInstance inst, float value)
	{
		Span<char> buffer = stackalloc char[16];
		int num = WriteFloatDigits(value, buffer);
		buffer.Slice(0, num).CopyTo(inst._charBuffer.AsSpan(inst._charIndex));
		inst._charIndex += num;
		if (inst._charIndex >= 2048)
		{
			inst.Flush();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Write(this ref LuiBuilderInstance inst, ulong value)
	{
		Span<char> buffer = stackalloc char[20];
		int num = WriteUlongDigits(value, buffer);
		buffer.Slice(0, num).CopyTo(inst._charBuffer.AsSpan(inst._charIndex));
		inst._charIndex += num;
		if (inst._charIndex >= 2048)
		{
			inst.Flush();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Write(this ref LuiBuilderInstance inst, string text, int startPos = 0)
	{
		int length = text.Length;
		Span<char> span = inst._charBuffer.AsSpan();
		int charIndex = inst._charIndex;
		int num = 0;
		bool flag = length > 2048;
		for (int i = startPos; i < length; i++)
		{
			int num2 = charIndex + num;
			span[num2] = text[i];
			num++;
			if (flag && num2 > stringLengthCheck)
			{
				inst._charIndex += num;
				inst.Flush();
				inst.Write(text, num);
				return;
			}
		}
		inst._charIndex += length - startPos;
		if (inst._charIndex > 2048)
		{
			inst.Flush();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int WriteFloatDigits(float value, Span<char> buffer, int startIndex = 0, int precision = 3)
	{
		int num = startIndex;
		if (value == 0f)
		{
			buffer[num++] = '0';
			return num - startIndex;
		}
		if (value < 0f)
		{
			buffer[num++] = '-';
			value = 0f - value;
		}
		int num2 = (int)value;
		float num3 = value - (float)num2;
		long num4 = _pow10[precision];
		long num5 = (long)(num3 * (float)num4 + 0.5f);
		if (num5 == _pow10[precision])
		{
			num2++;
		}
		num += WriteIntDigits(num2, buffer.Slice(num));
		buffer[num++] = '.';
		int num6 = num;
		for (int i = 0; i < precision; i++)
		{
			buffer[num + precision - i - 1] = (char)(48 + num5 % 10);
			num5 /= 10;
		}
		num += precision;
		int num7 = num - 1;
		while (num7 >= num6 && buffer[num7] == '0')
		{
			num7--;
		}
		num = ((num7 >= num6) ? (num7 + 1) : (num6 - 1));
		return num - startIndex;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int WriteUlongDigits(ulong value, Span<char> buffer)
	{
		if (value == 0L)
		{
			buffer[0] = '0';
			return 1;
		}
		int num = 0;
		while (value != 0)
		{
			buffer[num++] = (char)(48 + value % 10);
			value /= 10;
		}
		int num2 = 0;
		int num3 = num - 1;
		while (num2 < num3)
		{
			ref char reference = ref buffer[num2];
			ref char reference2 = ref buffer[num3];
			char c = buffer[num3];
			char c2 = buffer[num2];
			reference = c;
			reference2 = c2;
			num2++;
			num3--;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int WriteIntDigits(int value, Span<char> buffer)
	{
		if (value == 0)
		{
			buffer[0] = '0';
			return 1;
		}
		int num = 0;
		bool flag = value < 0;
		if (flag)
		{
			value = -value;
		}
		while (value > 0)
		{
			buffer[num++] = (char)(48 + value % 10);
			value /= 10;
		}
		if (flag)
		{
			buffer[num++] = '-';
		}
		int num2 = 0;
		int num3 = num - 1;
		while (num2 < num3)
		{
			ref char reference = ref buffer[num2];
			ref char reference2 = ref buffer[num3];
			char c = buffer[num3];
			char c2 = buffer[num2];
			reference = c;
			reference2 = c2;
			num2++;
			num3--;
		}
		return num;
	}
}
