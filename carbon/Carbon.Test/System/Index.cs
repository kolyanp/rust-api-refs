namespace System;

internal readonly struct Index : IEquatable<Index>
{
	private readonly int _value;

	public static Index Start => new Index(0);

	public static Index End => new Index(-1);

	public int Value
	{
		get
		{
			if (_value >= 0)
			{
				return _value;
			}
			return ~_value;
		}
	}

	public bool IsFromEnd => _value < 0;

	public Index(int value, bool fromEnd = false)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException("value", "Non-negative number required.");
		}
		_value = (fromEnd ? (~value) : value);
	}

	private Index(int value)
	{
		_value = value;
	}

	public static Index FromStart(int value)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException("value", "Non-negative number required.");
		}
		return new Index(value);
	}

	public static Index FromEnd(int value)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException("value", "Non-negative number required.");
		}
		return new Index(~value);
	}

	public int GetOffset(int length)
	{
		if (!IsFromEnd)
		{
			return _value;
		}
		return length + _value + 1;
	}

	public override bool Equals(object? value)
	{
		if (value is Index index)
		{
			return _value == index._value;
		}
		return false;
	}

	public bool Equals(Index other)
	{
		return _value == other._value;
	}

	public override int GetHashCode()
	{
		return _value;
	}

	public static implicit operator Index(int value)
	{
		return FromStart(value);
	}

	public override string ToString()
	{
		if (!IsFromEnd)
		{
			return ((uint)Value).ToString();
		}
		return "^" + Value;
	}
}
