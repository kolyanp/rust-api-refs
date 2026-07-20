using System.Runtime.CompilerServices;

namespace System.Buffers;

[_003Cfdd897db_002D548f_002D4ce5_002D8c3c_002D053982a32f17_003EIsReadOnly]
internal struct StandardFormat : IEquatable<StandardFormat>
{
	public const byte NoPrecision = byte.MaxValue;

	public const byte MaxPrecision = 99;

	private readonly byte _format;

	private readonly byte _precision;

	public char Symbol => (char)_format;

	public byte Precision => _precision;

	public bool HasPrecision => _precision != byte.MaxValue;

	public bool IsDefault
	{
		get
		{
			if (_format == 0)
			{
				return _precision == 0;
			}
			return false;
		}
	}

	public StandardFormat(char symbol, byte precision = byte.MaxValue)
	{
		if (precision != byte.MaxValue && precision > 99)
		{
			_003Cefefee0c_002Dbc34_002D4852_002Da4f2_002Df3a29a97a3fc_003EThrowHelper.ThrowArgumentOutOfRangeException_PrecisionTooLarge();
		}
		if (symbol != (byte)symbol)
		{
			_003Cefefee0c_002Dbc34_002D4852_002Da4f2_002Df3a29a97a3fc_003EThrowHelper.ThrowArgumentOutOfRangeException_SymbolDoesNotFit();
		}
		_format = (byte)symbol;
		_precision = precision;
	}

	public static implicit operator StandardFormat(char symbol)
	{
		return new StandardFormat(symbol);
	}

	public static StandardFormat Parse(ReadOnlySpan<char> format)
	{
		if (format.Length == 0)
		{
			return default(StandardFormat);
		}
		char symbol = format[0];
		byte precision;
		if (format.Length == 1)
		{
			precision = byte.MaxValue;
		}
		else
		{
			uint num = 0u;
			for (int i = 1; i < format.Length; i++)
			{
				uint num2 = (uint)(format[i] - 48);
				if (num2 > 9)
				{
					throw new FormatException(_003Cc1e4ce84_002Db725_002D4d32_002D90ca_002Da752f1fa7530_003ESR.Format(_003Cc1e4ce84_002Db725_002D4d32_002D90ca_002Da752f1fa7530_003ESR.Argument_CannotParsePrecision, (byte)99));
				}
				num = num * 10 + num2;
				if (num > 99)
				{
					throw new FormatException(_003Cc1e4ce84_002Db725_002D4d32_002D90ca_002Da752f1fa7530_003ESR.Format(_003Cc1e4ce84_002Db725_002D4d32_002D90ca_002Da752f1fa7530_003ESR.Argument_PrecisionTooLarge, (byte)99));
				}
			}
			precision = (byte)num;
		}
		return new StandardFormat(symbol, precision);
	}

	public static StandardFormat Parse(string format)
	{
		if (format != null)
		{
			return Parse(MemoryExtensions.AsSpan(format));
		}
		return default(StandardFormat);
	}

	public override bool Equals(object obj)
	{
		if (obj is StandardFormat other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		byte format = _format;
		int hashCode = format.GetHashCode();
		format = _precision;
		return hashCode ^ format.GetHashCode();
	}

	public bool Equals(StandardFormat other)
	{
		if (_format == other._format)
		{
			return _precision == other._precision;
		}
		return false;
	}

	public unsafe override string ToString()
	{
		char* ptr = stackalloc char[4];
		int length = 0;
		char symbol = Symbol;
		if (symbol != 0)
		{
			ptr[length++] = symbol;
			byte b = Precision;
			if (b != byte.MaxValue)
			{
				if (b >= 100)
				{
					ptr[length++] = (char)(48 + b / 100 % 10);
					b %= 100;
				}
				if (b >= 10)
				{
					ptr[length++] = (char)(48 + b / 10 % 10);
					b %= 10;
				}
				ptr[length++] = (char)(48 + b);
			}
		}
		return new string(ptr, 0, length);
	}

	public static bool operator ==(StandardFormat left, StandardFormat right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(StandardFormat left, StandardFormat right)
	{
		return !left.Equals(right);
	}
}
