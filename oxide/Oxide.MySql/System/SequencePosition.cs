using System.ComponentModel;
using System.Numerics.Hashing;
using System.Runtime.CompilerServices;

namespace System;

[_003Cfdd897db_002D548f_002D4ce5_002D8c3c_002D053982a32f17_003EIsReadOnly]
internal struct SequencePosition(object @object, int integer) : IEquatable<SequencePosition>
{
	private readonly object _object = @object;

	private readonly int _integer = integer;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public object GetObject()
	{
		return _object;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public int GetInteger()
	{
		return _integer;
	}

	public bool Equals(SequencePosition other)
	{
		if (_integer == other._integer)
		{
			return object.Equals(_object, other._object);
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		if (obj is SequencePosition other)
		{
			return Equals(other);
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return _003Cc139fa64_002Dff40_002D4487_002D9647_002D1f385a5dbff6_003EHashHelpers.Combine(_object?.GetHashCode() ?? 0, _integer);
	}
}
