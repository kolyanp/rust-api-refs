using System.Buffers.Binary;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security;

namespace System.Diagnostics;

[_003Cb470df79_002Da5b8_002D442e_002D9a19_002Ddc7ef3b9a2cb_003EIsReadOnly]
[SecuritySafeCritical]
internal struct ActivitySpanId : IEquatable<ActivitySpanId>
{
	private readonly string _hexString;

	internal ActivitySpanId(string hexString)
	{
		_hexString = hexString;
	}

	public unsafe static ActivitySpanId CreateRandom()
	{
		ulong num = default(ulong);
		ActivityTraceId.SetToRandomBytes(new Span<byte>(&num, 8));
		return new ActivitySpanId(System.HexConverter.ToString(new ReadOnlySpan<byte>(&num, 8), System.HexConverter.Casing.Lower));
	}

	public static ActivitySpanId CreateFromBytes(ReadOnlySpan<byte> idData)
	{
		if (idData.Length != 8)
		{
			throw new ArgumentOutOfRangeException("idData");
		}
		return new ActivitySpanId(System.HexConverter.ToString(idData, System.HexConverter.Casing.Lower));
	}

	public static ActivitySpanId CreateFromUtf8String(ReadOnlySpan<byte> idData)
	{
		return new ActivitySpanId(idData);
	}

	public static ActivitySpanId CreateFromString(ReadOnlySpan<char> idData)
	{
		if (idData.Length != 16 || !ActivityTraceId.IsLowerCaseHexAndNotAllZeros(idData))
		{
			throw new ArgumentOutOfRangeException("idData");
		}
		return new ActivitySpanId(idData.ToString());
	}

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
	public string ToHexString()
	{
		return _hexString ?? "0000000000000000";
	}

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
	public override string ToString()
	{
		return ToHexString();
	}

	public static bool operator ==(ActivitySpanId spanId1, ActivitySpanId spandId2)
	{
		return spanId1._hexString == spandId2._hexString;
	}

	public static bool operator !=(ActivitySpanId spanId1, ActivitySpanId spandId2)
	{
		return spanId1._hexString != spandId2._hexString;
	}

	public bool Equals(ActivitySpanId spanId)
	{
		return _hexString == spanId._hexString;
	}

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(2)]
	public override bool Equals([_003C8138f099_002Ddb66_002D4e8d_002Da0f5_002D5476a41f5864_003ENotNullWhen(true)] object obj)
	{
		if (obj is ActivitySpanId activitySpanId)
		{
			return _hexString == activitySpanId._hexString;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ToHexString().GetHashCode();
	}

	private unsafe ActivitySpanId(ReadOnlySpan<byte> idData)
	{
		if (idData.Length != 16)
		{
			throw new ArgumentOutOfRangeException("idData");
		}
		if (!Utf8Parser.TryParse(idData, out ulong value, out int _, 'x'))
		{
			_hexString = CreateRandom()._hexString;
			return;
		}
		if (BitConverter.IsLittleEndian)
		{
			value = BinaryPrimitives.ReverseEndianness(value);
		}
		_hexString = System.HexConverter.ToString(new ReadOnlySpan<byte>(&value, 8), System.HexConverter.Casing.Lower);
	}

	public void CopyTo(Span<byte> destination)
	{
		ActivityTraceId.SetSpanFromHexChars(MemoryExtensions.AsSpan(ToHexString()), destination);
	}
}
