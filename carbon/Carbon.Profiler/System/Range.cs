namespace System;

internal readonly struct Range(Index start, Index end) : IEquatable<Range>
{
	public Index Start { get; } = start;

	public Index End { get; } = end;

	public static Range All => Index.Start..Index.End;

	public static Range StartAt(Index start)
	{
		return start..Index.End;
	}

	public static Range EndAt(Index end)
	{
		return Index.Start..end;
	}

	public override bool Equals(object? value)
	{
		return value is Range other && Equals(other);
	}

	public bool Equals(Range other)
	{
		return other.Start.Equals(Start) && other.End.Equals(End);
	}

	public override int GetHashCode()
	{
		return ((Start.GetHashCode() << 5) + Start.GetHashCode()) ^ End.GetHashCode();
	}

	public override string ToString()
	{
		return Start.ToString() + ".." + End;
	}

	public (int Offset, int Length) GetOffsetAndLength(int length)
	{
		int num = (Start.IsFromEnd ? (length - Start.Value) : Start.Value);
		int num2 = (End.IsFromEnd ? (length - End.Value) : End.Value);
		if ((uint)num2 > (uint)length || (uint)num > (uint)num2)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		return (Offset: num, Length: num2 - num);
	}
}
