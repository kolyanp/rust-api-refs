using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MySqlConnector.Utilities;

internal static class Utility
{
	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static void Dispose<T>([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] ref T disposable) where T : class, IDisposable
	{
		if (disposable != null)
		{
			disposable.Dispose();
			disposable = null;
		}
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public unsafe static string GetString(this Encoding encoding, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] ReadOnlySpan<byte> span)
	{
		if (span.Length == 0)
		{
			return "";
		}
		fixed (byte* reference = &MemoryMarshal.GetReference(span))
		{
			return encoding.GetString(reference, span.Length);
		}
	}

	public unsafe static int GetByteCount([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] this Encoding encoding, ReadOnlySpan<char> chars)
	{
		if (chars.Length == 0)
		{
			return 0;
		}
		fixed (char* reference = &MemoryMarshal.GetReference(chars))
		{
			return encoding.GetByteCount(reference, chars.Length);
		}
	}

	public unsafe static int GetBytes([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] this Encoding encoding, ReadOnlySpan<char> chars, Span<byte> bytes)
	{
		fixed (char* reference = &MemoryMarshal.GetReference(chars))
		{
			fixed (byte* reference2 = &MemoryMarshal.GetReference(bytes))
			{
				return encoding.GetBytes(reference, chars.Length, reference2, bytes.Length);
			}
		}
	}

	public unsafe static void Convert([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] this Encoder encoder, ReadOnlySpan<char> chars, Span<byte> bytes, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
	{
		fixed (char* reference = &MemoryMarshal.GetReference(chars))
		{
			fixed (byte* reference2 = &MemoryMarshal.GetReference(bytes))
			{
				encoder.Convert(reference, chars.Length, (reference2 == null) ? ((byte*)1) : reference2, bytes.Length, flush, out charsUsed, out bytesUsed, out completed);
			}
		}
	}

	public unsafe static int GetByteCount([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] this Encoder encoder, ReadOnlySpan<char> chars, bool flush)
	{
		fixed (char* reference = &MemoryMarshal.GetReference(chars))
		{
			return encoder.GetByteCount(reference, chars.Length, flush);
		}
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static RSAParameters GetRsaParameters(string key)
	{
		string text;
		bool isPrivate;
		int num;
		if ((num = key.IndexOf("-----BEGIN RSA PRIVATE KEY-----", StringComparison.Ordinal)) > -1)
		{
			num += "-----BEGIN RSA PRIVATE KEY-----".Length;
			text = "-----END RSA PRIVATE KEY-----";
			isPrivate = true;
		}
		else
		{
			if ((num = key.IndexOf("-----BEGIN PUBLIC KEY-----", StringComparison.Ordinal)) <= -1)
			{
				throw new FormatException("Unrecognized PEM header: " + key.Substring(0, Math.Min(key.Length, 80)));
			}
			num += "-----BEGIN PUBLIC KEY-----".Length;
			text = "-----END PUBLIC KEY-----";
			isPrivate = false;
		}
		int num2 = key.IndexOf(text, num, StringComparison.Ordinal);
		int num3;
		if (num2 <= -1)
		{
			string text2 = text;
			string text3 = key;
			num3 = Math.Max(key.Length - 80, 0);
			throw new FormatException("Missing expected '" + text2 + "' PEM footer: " + text3.Substring(num3, text3.Length - num3));
		}
		string text4 = key;
		num3 = num;
		key = text4.Substring(num3, num2 - num3);
		return GetRsaParameters(System.Convert.FromBase64String(key), isPrivate);
	}

	private static RSAParameters GetRsaParameters(ReadOnlySpan<byte> data, bool isPrivate)
	{
		if (data[0] != 48)
		{
			throw new FormatException($"Expected 0x30 but read 0x{data[0]:X2}");
		}
		ref ReadOnlySpan<byte> reference = ref data;
		data = reference.Slice(1, reference.Length - 1);
		if (!TryReadAsnLength(data, out var length, out var bytesConsumed))
		{
			throw new FormatException("Couldn't read key length");
		}
		reference = ref data;
		int num = bytesConsumed;
		data = reference.Slice(num, reference.Length - num);
		if (!isPrivate)
		{
			ReadOnlySpan<byte> other = new byte[15]
			{
				48, 13, 6, 9, 42, 134, 72, 134, 247, 13,
				1, 1, 1, 5, 0
			};
			if (!data.Slice(0, other.Length).SequenceEqual(other))
			{
				throw new FormatException("Expected RSA OID but read " + BitConverter.ToString(data.Slice(0, 15).ToArray()));
			}
			reference = ref data;
			num = other.Length;
			data = reference.Slice(num, reference.Length - num);
			if (data[0] != 3)
			{
				throw new FormatException($"Expected 0x03 but read 0x{data[0]:X2}");
			}
			reference = ref data;
			data = reference.Slice(1, reference.Length - 1);
			if (!TryReadAsnLength(data, out length, out bytesConsumed))
			{
				throw new FormatException("Couldn't read length");
			}
			reference = ref data;
			num = bytesConsumed;
			data = reference.Slice(num, reference.Length - num);
			if (data[0] != 0)
			{
				throw new FormatException($"Expected 0x00 but read 0x{data[0]:X2}");
			}
			reference = ref data;
			data = reference.Slice(1, reference.Length - 1);
			if (data[0] != 48)
			{
				throw new FormatException($"Expected 0x30 but read 0x{data[0]:X2}");
			}
			reference = ref data;
			data = reference.Slice(1, reference.Length - 1);
			if (!TryReadAsnLength(data, out length, out bytesConsumed))
			{
				throw new FormatException("Couldn't read length");
			}
			reference = ref data;
			num = bytesConsumed;
			data = reference.Slice(num, reference.Length - num);
		}
		else
		{
			if (!TryReadAsnInteger(data, out var number, out bytesConsumed) || number.Length != 1 || number[0] != 0)
			{
				throw new FormatException("Couldn't read zero.");
			}
			reference = ref data;
			num = bytesConsumed;
			data = reference.Slice(num, reference.Length - num);
		}
		if (!TryReadAsnInteger(data, out var number2, out bytesConsumed))
		{
			throw new FormatException("Couldn't read modulus");
		}
		reference = ref data;
		num = bytesConsumed;
		data = reference.Slice(num, reference.Length - num);
		if (!TryReadAsnInteger(data, out var number3, out bytesConsumed))
		{
			throw new FormatException("Couldn't read exponent");
		}
		reference = ref data;
		num = bytesConsumed;
		data = reference.Slice(num, reference.Length - num);
		if (!isPrivate)
		{
			return new RSAParameters
			{
				Modulus = number2.ToArray(),
				Exponent = number3.ToArray()
			};
		}
		if (!TryReadAsnInteger(data, out var number4, out bytesConsumed))
		{
			throw new FormatException("Couldn't read D");
		}
		reference = ref data;
		num = bytesConsumed;
		data = reference.Slice(num, reference.Length - num);
		if (!TryReadAsnInteger(data, out var number5, out bytesConsumed))
		{
			throw new FormatException("Couldn't read P");
		}
		reference = ref data;
		num = bytesConsumed;
		data = reference.Slice(num, reference.Length - num);
		if (!TryReadAsnInteger(data, out var number6, out bytesConsumed))
		{
			throw new FormatException("Couldn't read Q");
		}
		reference = ref data;
		num = bytesConsumed;
		data = reference.Slice(num, reference.Length - num);
		if (!TryReadAsnInteger(data, out var number7, out bytesConsumed))
		{
			throw new FormatException("Couldn't read DP");
		}
		reference = ref data;
		num = bytesConsumed;
		data = reference.Slice(num, reference.Length - num);
		if (!TryReadAsnInteger(data, out var number8, out bytesConsumed))
		{
			throw new FormatException("Couldn't read DQ");
		}
		reference = ref data;
		num = bytesConsumed;
		data = reference.Slice(num, reference.Length - num);
		if (!TryReadAsnInteger(data, out var number9, out bytesConsumed))
		{
			throw new FormatException("Couldn't read IQ");
		}
		reference = ref data;
		num = bytesConsumed;
		data = reference.Slice(num, reference.Length - num);
		return new RSAParameters
		{
			Modulus = number2.ToArray(),
			Exponent = number3.ToArray(),
			D = number4.ToArray(),
			P = number5.ToArray(),
			Q = number6.ToArray(),
			DP = number7.ToArray(),
			DQ = number8.ToArray(),
			InverseQ = number9.ToArray()
		};
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public static ArraySegment<T> Slice<T>([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })] this ArraySegment<T> arraySegment, int index)
	{
		return new ArraySegment<T>(arraySegment.Array, arraySegment.Offset + index, arraySegment.Count - index);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public static ArraySegment<T> Slice<T>([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })] this ArraySegment<T> arraySegment, int index, int length)
	{
		return new ArraySegment<T>(arraySegment.Array, arraySegment.Offset + index, length);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static byte[] ArraySlice(byte[] input, int offset, int length)
	{
		if (offset == 0 && length == input.Length)
		{
			return input;
		}
		byte[] array = new byte[length];
		Array.Copy(input, offset, array, 0, array.Length);
		return array;
	}

	public static int FindNextIndex(ReadOnlySpan<byte> data, int offset, ReadOnlySpan<byte> pattern)
	{
		int num = data.Slice(offset, data.Length - offset).IndexOf(pattern);
		if (num != -1)
		{
			return offset + num;
		}
		return -1;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static void Resize<T>([_003C0fcba684_002Db16b_002D492b_002Db6ee_002D61db96fb7cb1_003ENotNull][_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })] ref ResizableArray<T> resizableArray, int newLength)
	{
		if (resizableArray == null)
		{
			resizableArray = new ResizableArray<T>();
		}
		resizableArray.DoResize(newLength);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static bool TryParseRedirectionHeader(string header, out string host, out int port, out string user)
	{
		host = "";
		port = 0;
		user = "";
		if (!header.StartsWith("Location: mysql://", StringComparison.Ordinal) || header.Length < 22)
		{
			return false;
		}
		bool flag;
		int num4;
		int num3;
		if (header[18] == '[')
		{
			flag = true;
			int num = 19;
			int num2 = header.IndexOf(']', num);
			if (num2 == -1)
			{
				return false;
			}
			num3 = num;
			host = header.Substring(num3, num2 - num3);
			if (header.Length <= num2 + 2)
			{
				return false;
			}
			if (header[num2 + 1] != ':')
			{
				return false;
			}
			num4 = num2 + 2;
		}
		else
		{
			flag = false;
			int num5 = 18;
			int num6 = header.IndexOf(':', num5);
			if (num6 == -1)
			{
				return false;
			}
			num3 = num5;
			host = header.Substring(num3, num6 - num3);
			num4 = num6 + 1;
		}
		int num7 = header.IndexOf(flag ? "/?user=" : "/user=", StringComparison.Ordinal);
		if (num7 == -1)
		{
			return false;
		}
		num3 = num4;
		if (!int.TryParse(header.Substring(num3, num7 - num3), out port) || port <= 0)
		{
			return false;
		}
		num7 += (flag ? 7 : 6);
		int num8 = header.IndexOf('&', num7);
		int num9 = header.IndexOf('\n', num7);
		int num10 = ((num8 != -1) ? ((num9 == -1) ? num8 : Math.Min(num8, num9)) : ((num9 == -1) ? header.Length : num9));
		num3 = num7;
		user = header.Substring(num3, num10 - num3);
		return user.Length != 0;
	}

	public static TimeSpan ParseTimeSpan(ReadOnlySpan<byte> value)
	{
		ReadOnlySpan<byte> span = value;
		bool flag = false;
		if (value.Length >= 1 && value[0] == 45)
		{
			flag = true;
			ref ReadOnlySpan<byte> reference = ref value;
			value = reference.Slice(1, reference.Length - 1);
		}
		int value3;
		int value4;
		int value5;
		if (Utf8Parser.TryParse(value, out int value2, out int i, '\0') && value2 >= 0 && value2 <= 838 && value.Length != i && value[i] == 58)
		{
			ref ReadOnlySpan<byte> reference = ref value;
			int num = i + 1;
			value = reference.Slice(num, reference.Length - num);
			if (Utf8Parser.TryParse(value, out value3, out i, '\0') && i == 2 && value3 >= 0 && value3 <= 59 && value.Length >= 3 && value[2] == 58)
			{
				reference = ref value;
				value = reference.Slice(3, reference.Length - 3);
				if (Utf8Parser.TryParse(value, out value4, out i, '\0') && i == 2 && value4 >= 0 && value4 <= 59)
				{
					if (value.Length == 2)
					{
						value5 = 0;
						goto IL_0181;
					}
					if (value[2] == 46)
					{
						reference = ref value;
						value = reference.Slice(3, reference.Length - 3);
						if (Utf8Parser.TryParse(value, out value5, out i, '\0') && i == value.Length && value5 >= 0 && value5 <= 999999)
						{
							for (; i < 6; i++)
							{
								value5 *= 10;
							}
							goto IL_0181;
						}
					}
				}
			}
		}
		throw new FormatException("Couldn't interpret value as a valid TimeSpan: " + GetString(Encoding.UTF8, span));
		IL_0181:
		if (flag)
		{
			value2 = -value2;
			value3 = -value3;
			value4 = -value4;
			value5 = -value5;
		}
		return new TimeSpan(0, value2, value3, value4, value5 / 1000) + TimeSpan.FromTicks(value5 % 1000 * 10);
	}

	public static bool TryComputeHash([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] this HashAlgorithm hashAlgorithm, ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten)
	{
		byte[] array = hashAlgorithm.ComputeHash(source.ToArray());
		MemoryExtensions.AsSpan(array).CopyTo(destination);
		bytesWritten = array.Length;
		return true;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static byte[] TrimZeroByte(byte[] value)
	{
		if (value != null)
		{
			int num = value.Length;
			if (num >= 1 && value[num - 1] == 0)
			{
				Array.Resize(ref value, value.Length - 1);
			}
		}
		return value;
	}

	public static ReadOnlySpan<byte> TrimZeroByte(ReadOnlySpan<byte> value)
	{
		int length = value.Length;
		if (length < 1 || value[length - 1] != 0)
		{
			return value;
		}
		return value.Slice(0, value.Length - 1);
	}

	public static int Read([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] this Stream stream, Memory<byte> buffer)
	{
		MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)buffer, out ArraySegment<byte> segment);
		return stream.Read(segment.Array, segment.Offset, segment.Count);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static Task<int> ReadAsync(this Stream stream, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] Memory<byte> buffer)
	{
		MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)buffer, out ArraySegment<byte> segment);
		return stream.ReadAsync(segment.Array, segment.Offset, segment.Count);
	}

	public static void Write([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] this Stream stream, ReadOnlyMemory<byte> data)
	{
		MemoryMarshal.TryGetArray(data, out var segment);
		stream.Write(segment.Array, segment.Offset, segment.Count);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static Task WriteAsync(this Stream stream, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] ReadOnlyMemory<byte> data)
	{
		MemoryMarshal.TryGetArray(data, out var segment);
		return stream.WriteAsync(segment.Array, segment.Offset, segment.Count);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static bool StartsWith(this string str, char value)
	{
		if (!string.IsNullOrEmpty(str))
		{
			return str[0] == value;
		}
		return false;
	}

	public static void SwapBytes(Span<byte> bytes, int offset1, int offset2)
	{
		ref byte source = ref Unsafe.AsRef(ref bytes[0]);
		ref byte reference = ref Unsafe.Add(ref source, offset2);
		ref byte reference2 = ref Unsafe.Add(ref source, offset1);
		byte b = Unsafe.Add(ref source, offset1);
		byte b2 = Unsafe.Add(ref source, offset2);
		reference = b;
		reference2 = b2;
	}

	public static bool IsWindows()
	{
		try
		{
			return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		}
		catch (PlatformNotSupportedException)
		{
			return false;
		}
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static void GetOSDetails([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] out string os, out string osDescription, out string architecture)
	{
		os = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" : (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macOS" : null)));
		osDescription = RuntimeInformation.OSDescription;
		architecture = RuntimeInformation.ProcessArchitecture.ToString();
	}

	public static int GetElapsedMilliseconds(long startingTimestamp)
	{
		return (int)((Stopwatch.GetTimestamp() - startingTimestamp) * 1000 / Stopwatch.Frequency);
	}

	public static double GetElapsedSeconds(long startingTimestamp, long endingTimestamp)
	{
		return (double)(endingTimestamp - startingTimestamp) / (double)Stopwatch.Frequency;
	}

	public static SslProtocols GetDefaultSslProtocols()
	{
		return SslProtocols.None;
	}

	private static bool TryReadAsnLength(ReadOnlySpan<byte> data, out int length, out int bytesConsumed)
	{
		byte b = data[0];
		if (b < 128)
		{
			length = b;
			bytesConsumed = 1;
			return true;
		}
		switch (b)
		{
		case 129:
			length = data[1];
			bytesConsumed = 2;
			return true;
		case 130:
			length = data[1] * 256 + data[2];
			bytesConsumed = 3;
			return true;
		default:
			length = 0;
			bytesConsumed = 0;
			return false;
		}
	}

	private static bool TryReadAsnInteger(ReadOnlySpan<byte> data, out ReadOnlySpan<byte> number, out int bytesConsumed)
	{
		if (data.Length < 1 || data[0] != 2)
		{
			number = default(ReadOnlySpan<byte>);
			bytesConsumed = 0;
			return false;
		}
		ref ReadOnlySpan<byte> reference = ref data;
		data = reference.Slice(1, reference.Length - 1);
		if (!TryReadAsnLength(data, out var length, out var bytesConsumed2))
		{
			number = default(ReadOnlySpan<byte>);
			bytesConsumed = 0;
			return false;
		}
		number = data.Slice(bytesConsumed2, length);
		bytesConsumed = bytesConsumed2 + length + 1;
		while (true)
		{
			ReadOnlySpan<byte> readOnlySpan = number;
			if (readOnlySpan.Length < 2 || readOnlySpan[0] != 0)
			{
				break;
			}
			reference = ref number;
			number = reference.Slice(1, reference.Length - 1);
		}
		return true;
	}
}
