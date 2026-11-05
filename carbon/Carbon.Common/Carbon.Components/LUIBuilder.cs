using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Carbon.Components;

public static class LUIBuilder
{
	private const int floatStringBuffer = 32;

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

	private const int maxIntChars = 11;

	private const int maxUlongChars = 20;

	private const int maxFloatChars = 24;

	private static readonly long[] _pow10 = new long[10] { 1L, 10L, 100L, 1000L, 10000L, 100000L, 1000000L, 10000000L, 100000000L, 1000000000L };

	public static string GetStringFloat(float value)
	{
		Span<char> buffer = stackalloc char[32];
		int length = WriteFloatDigits(value, buffer);
		return MakeString(buffer, length);
	}

	public static string GetStringFloat(float x, float y)
	{
		Span<char> buffer = stackalloc char[32];
		int num = WriteFloatDigits(x, buffer);
		buffer[num++] = ' ';
		num += WriteFloatDigits(y, buffer.Slice(num));
		return MakeString(buffer, num);
	}

	public static string GetStringFloat(float x, float y, float z)
	{
		Span<char> buffer = stackalloc char[32];
		int num = WriteFloatDigits(x, buffer);
		buffer[num++] = ' ';
		num += WriteFloatDigits(y, buffer.Slice(num));
		buffer[num++] = ' ';
		num += WriteFloatDigits(z, buffer.Slice(num));
		return MakeString(buffer, num);
	}

	public static string GetStringFloat(float x, float y, float z, float w)
	{
		Span<char> buffer = stackalloc char[32];
		int num = WriteFloatDigits(x, buffer);
		buffer[num++] = ' ';
		num += WriteFloatDigits(y, buffer.Slice(num));
		buffer[num++] = ' ';
		num += WriteFloatDigits(z, buffer.Slice(num));
		buffer[num++] = ' ';
		num += WriteFloatDigits(w, buffer.Slice(num));
		return MakeString(buffer, num);
	}

	public static string GetStringFloat(params float[] values)
	{
		Span<char> span = ((values.Length * 9 > 32) ? ((Span<char>)new char[values.Length * 9]) : stackalloc char[32]);
		Span<char> buffer = span;
		int num = 0;
		for (int i = 0; i < values.Length; i++)
		{
			num += WriteFloatDigits(values[i], buffer.Slice(num));
			if (i < values.Length - 1)
			{
				buffer[num++] = ' ';
			}
		}
		return MakeString(buffer, num);
	}

	public static string BuildElementId(string elementId, int id)
	{
		Span<char> span = stackalloc char[elementId.Length + 1 + 11];
		((ReadOnlySpan<char>)elementId).CopyTo(span);
		span[elementId.Length] = '_';
		int length = elementId.Length + 1 + WriteIntDigits(id, span.Slice(elementId.Length + 1));
		return MakeString(span, length);
	}

	private unsafe static string MakeString(Span<char> buffer, int length)
	{
		fixed (char* value = buffer)
		{
			return new string(value, 0, length);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteStartObject(this ref LuiBuilderInstance inst)
	{
		inst.Reserve(1);
		inst.Put('{');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteEndObject(this ref LuiBuilderInstance inst)
	{
		inst.Reserve(1);
		inst.Put('}');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteStartArray(this ref LuiBuilderInstance inst)
	{
		inst.Reserve(1);
		inst.Put('[');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteEndArray(this ref LuiBuilderInstance inst)
	{
		inst.Reserve(1);
		inst.Put(']');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteComma(this ref LuiBuilderInstance inst)
	{
		inst.Reserve(1);
		inst.Put(',');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteMark(this ref LuiBuilderInstance inst)
	{
		inst.Reserve(1);
		inst.Put('"');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteDots(this ref LuiBuilderInstance inst)
	{
		inst.Reserve(1);
		inst.Put(':');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteSpace(this ref LuiBuilderInstance inst)
	{
		inst.Reserve(1);
		inst.Put(' ');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteStartObject(this ref LuiBuilderInstance inst, string key)
	{
		inst.Reserve(key.Length + 4);
		inst.PutKey(key);
		inst.Put('{');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteStartArray(this ref LuiBuilderInstance inst, string key)
	{
		inst.Reserve(key.Length + 4);
		inst.PutKey(key);
		inst.Put('[');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteField(this ref LuiBuilderInstance inst, string key, int value)
	{
		inst.Reserve(key.Length + 3 + 11);
		inst.PutKey(key);
		inst.PutInt(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteField(this ref LuiBuilderInstance inst, string key, Vector2 value)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		inst.Reserve(key.Length + 6 + 48);
		inst.PutKey(key);
		inst.Put('"');
		inst.PutFloat(value.x);
		inst.Put(' ');
		inst.PutFloat(value.y);
		inst.Put('"');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteField(this ref LuiBuilderInstance inst, string key, float value)
	{
		inst.Reserve(key.Length + 3 + 24);
		inst.PutKey(key);
		inst.PutFloat(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteField(this ref LuiBuilderInstance inst, string key, ulong value)
	{
		inst.Reserve(key.Length + 3 + 20);
		inst.PutKey(key);
		inst.PutUlong(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteField(this ref LuiBuilderInstance inst, string key, bool value)
	{
		inst.Reserve(key.Length + 8);
		inst.PutKey(key);
		inst.Put(value ? "true" : "false");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteField(this ref LuiBuilderInstance inst, string key, string value)
	{
		inst.Reserve(key.Length + value.Length + 5);
		inst.PutKey(key);
		inst.Put('"');
		inst.Put(value);
		inst.Put('"');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteFieldId(this ref LuiBuilderInstance inst, string key, string identifier, int id)
	{
		inst.Reserve(key.Length + identifier.Length + 6 + 11);
		inst.PutKey(key);
		inst.Put('"');
		inst.Put(identifier);
		inst.Put('_');
		inst.PutInt(id);
		inst.Put('"');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteFieldIdComma(this ref LuiBuilderInstance inst, string key, string identifier, int id)
	{
		inst.Reserve(key.Length + identifier.Length + 7 + 11);
		inst.Put(',');
		inst.PutKey(key);
		inst.Put('"');
		inst.Put(identifier);
		inst.Put('_');
		inst.PutInt(id);
		inst.Put('"');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteFieldComma(this ref LuiBuilderInstance inst, string key, int value)
	{
		inst.Reserve(key.Length + 4 + 11);
		inst.Put(',');
		inst.PutKey(key);
		inst.PutInt(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteFieldComma(this ref LuiBuilderInstance inst, string key, Vector2 value)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		inst.Reserve(key.Length + 7 + 48);
		inst.Put(',');
		inst.PutKey(key);
		inst.Put('"');
		inst.PutFloat(value.x);
		inst.Put(' ');
		inst.PutFloat(value.y);
		inst.Put('"');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteFieldComma(this ref LuiBuilderInstance inst, string key, float value)
	{
		inst.Reserve(key.Length + 4 + 24);
		inst.Put(',');
		inst.PutKey(key);
		inst.PutFloat(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteFieldComma(this ref LuiBuilderInstance inst, string key, ulong value)
	{
		inst.Reserve(key.Length + 4 + 20);
		inst.Put(',');
		inst.PutKey(key);
		inst.PutUlong(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteFieldComma(this ref LuiBuilderInstance inst, string key, bool value)
	{
		inst.Reserve(key.Length + 9);
		inst.Put(',');
		inst.PutKey(key);
		inst.Put(value ? "true" : "false");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteFieldComma(this ref LuiBuilderInstance inst, string key, string value)
	{
		inst.Reserve(key.Length + value.Length + 6);
		inst.Put(',');
		inst.PutKey(key);
		inst.Put('"');
		inst.Put(value);
		inst.Put('"');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void Reserve(this ref LuiBuilderInstance inst, int required)
	{
		if (inst._charIndex + required > inst._charBuffer.Length)
		{
			inst.Grow(required);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Grow(this ref LuiBuilderInstance inst, int required)
	{
		int minimumLength = Math.Max(inst._charBuffer.Length * 2, inst._charIndex + required);
		char[] array = ArrayPool<char>.Shared.Rent(minimumLength);
		inst._charBuffer.AsSpan(0, inst._charIndex).CopyTo(array);
		ArrayPool<char>.Shared.Return(inst._charBuffer);
		inst._charBuffer = array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Put(this ref LuiBuilderInstance inst, char character)
	{
		inst._charBuffer[inst._charIndex++] = character;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Put(this ref LuiBuilderInstance inst, string text)
	{
		((ReadOnlySpan<char>)text).CopyTo(inst._charBuffer.AsSpan(inst._charIndex));
		inst._charIndex += text.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void PutKey(this ref LuiBuilderInstance inst, string key)
	{
		Span<char> span = inst._charBuffer.AsSpan(inst._charIndex);
		span[0] = '"';
		((ReadOnlySpan<char>)key).CopyTo(span.Slice(1));
		int num = key.Length + 1;
		span[num++] = '"';
		span[num++] = ':';
		inst._charIndex += num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void PutInt(this ref LuiBuilderInstance inst, int value)
	{
		inst._charIndex += WriteIntDigits(value, inst._charBuffer.AsSpan(inst._charIndex));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void PutFloat(this ref LuiBuilderInstance inst, float value)
	{
		inst._charIndex += WriteFloatDigits(value, inst._charBuffer.AsSpan(inst._charIndex));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void PutUlong(this ref LuiBuilderInstance inst, ulong value)
	{
		inst._charIndex += WriteUlongDigits(value, inst._charBuffer.AsSpan(inst._charIndex));
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
		if (num5 == num4)
		{
			num2++;
			num5 = 0L;
		}
		num += WriteIntDigits(num2, buffer.Slice(num));
		if (num5 == 0L)
		{
			return num - startIndex;
		}
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
