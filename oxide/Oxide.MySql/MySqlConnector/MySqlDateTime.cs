using System;
using System.Runtime.CompilerServices;

namespace MySqlConnector;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
public struct MySqlDateTime(int year, int month, int day, int hour, int minute, int second, int microsecond) : IComparable, IComparable<MySqlDateTime>, IConvertible, IEquatable<MySqlDateTime>
{
	public readonly bool IsValidDateTime
	{
		get
		{
			if (Year != 0 && Month != 0)
			{
				return Day != 0;
			}
			return false;
		}
	}

	public int Year { get; set; } = year;

	public int Month { get; set; } = month;

	public int Day { get; set; } = day;

	public int Hour { get; set; } = hour;

	public int Minute { get; set; } = minute;

	public int Second { get; set; } = second;

	public int Microsecond { get; set; } = microsecond;

	public int Millisecond
	{
		readonly get
		{
			return Microsecond / 1000;
		}
		set
		{
			Microsecond = value * 1000;
		}
	}

	public MySqlDateTime(DateTime dt)
		: this(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, (int)(dt.Ticks % 10000000) / 10)
	{
	}

	public MySqlDateTime(MySqlDateTime other)
		: this(other.Year, other.Month, other.Day, other.Hour, other.Minute, other.Second, other.Microsecond)
	{
	}

	public readonly DateTime GetDateTime()
	{
		if (IsValidDateTime)
		{
			return new DateTime(Year, Month, Day, Hour, Minute, Second, DateTimeKind.Unspecified).AddTicks(Microsecond * 10);
		}
		throw new MySqlConversionException("Cannot convert MySqlDateTime to DateTime when IsValidDateTime is false.");
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public override readonly string ToString()
	{
		if (!IsValidDateTime)
		{
			return "0000-00-00";
		}
		return GetDateTime().ToString();
	}

	public static explicit operator DateTime(MySqlDateTime val)
	{
		if (val.IsValidDateTime)
		{
			return val.GetDateTime();
		}
		return DateTime.MinValue;
	}

	public override bool Equals(object obj)
	{
		if (obj is MySqlDateTime other)
		{
			return ((IEquatable<MySqlDateTime>)this).Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((((((((((Year * 33) ^ Month) * 33) ^ Day) * 33) ^ Hour) * 33) ^ Minute) * 33) ^ Second) * 33) ^ Microsecond;
	}

	public static bool operator ==(MySqlDateTime left, MySqlDateTime right)
	{
		return ((IComparable<MySqlDateTime>)left).CompareTo(right) == 0;
	}

	public static bool operator !=(MySqlDateTime left, MySqlDateTime right)
	{
		return ((IComparable<MySqlDateTime>)left).CompareTo(right) != 0;
	}

	public static bool operator <(MySqlDateTime left, MySqlDateTime right)
	{
		return ((IComparable<MySqlDateTime>)left).CompareTo(right) < 0;
	}

	public static bool operator <=(MySqlDateTime left, MySqlDateTime right)
	{
		return ((IComparable<MySqlDateTime>)left).CompareTo(right) <= 0;
	}

	public static bool operator >(MySqlDateTime left, MySqlDateTime right)
	{
		return ((IComparable<MySqlDateTime>)left).CompareTo(right) > 0;
	}

	public static bool operator >=(MySqlDateTime left, MySqlDateTime right)
	{
		return ((IComparable<MySqlDateTime>)left).CompareTo(right) >= 0;
	}

	readonly int IComparable.CompareTo(object obj)
	{
		if (!(obj is MySqlDateTime other))
		{
			throw new ArgumentException("CompareTo can only be called with another MySqlDateTime", "obj");
		}
		return ((IComparable<MySqlDateTime>)this).CompareTo(other);
	}

	readonly int IComparable<MySqlDateTime>.CompareTo(MySqlDateTime other)
	{
		if (Year < other.Year)
		{
			return -1;
		}
		if (Year > other.Year)
		{
			return 1;
		}
		if (Month < other.Month)
		{
			return -1;
		}
		if (Month > other.Month)
		{
			return 1;
		}
		if (Day < other.Day)
		{
			return -1;
		}
		if (Day > other.Day)
		{
			return 1;
		}
		if (Hour < other.Hour)
		{
			return -1;
		}
		if (Hour > other.Hour)
		{
			return 1;
		}
		if (Minute < other.Minute)
		{
			return -1;
		}
		if (Minute > other.Minute)
		{
			return 1;
		}
		if (Second < other.Second)
		{
			return -1;
		}
		if (Second > other.Second)
		{
			return 1;
		}
		return Microsecond.CompareTo(other.Microsecond);
	}

	readonly bool IEquatable<MySqlDateTime>.Equals(MySqlDateTime other)
	{
		return ((IComparable<MySqlDateTime>)this).CompareTo(other) == 0;
	}

	DateTime IConvertible.ToDateTime(IFormatProvider provider)
	{
		if (!IsValidDateTime)
		{
			throw new InvalidCastException();
		}
		return GetDateTime();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	string IConvertible.ToString([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] IFormatProvider provider)
	{
		if (!IsValidDateTime)
		{
			return "0000-00-00";
		}
		return GetDateTime().ToString(provider);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	object IConvertible.ToType(Type conversionType, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] IFormatProvider provider)
	{
		if (!(conversionType == typeof(DateTime)))
		{
			if (!(conversionType == typeof(string)))
			{
				throw new InvalidCastException();
			}
			return ((IConvertible)this).ToString(provider);
		}
		return GetDateTime();
	}

	TypeCode IConvertible.GetTypeCode()
	{
		return TypeCode.Object;
	}

	bool IConvertible.ToBoolean(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	char IConvertible.ToChar(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	sbyte IConvertible.ToSByte(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	byte IConvertible.ToByte(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	short IConvertible.ToInt16(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	ushort IConvertible.ToUInt16(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	int IConvertible.ToInt32(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	uint IConvertible.ToUInt32(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	long IConvertible.ToInt64(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	ulong IConvertible.ToUInt64(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	float IConvertible.ToSingle(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	double IConvertible.ToDouble(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	decimal IConvertible.ToDecimal(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}
}
