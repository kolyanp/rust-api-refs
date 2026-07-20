using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Carbon.Components;

public class LuiComponentDictionary : IEnumerable
{
	private readonly LuiCompBase[] _values;

	private int _count;

	private const int DictionarySize = 10;

	public int Count => _count;

	public LuiComponentDictionary()
	{
		_values = new LuiCompBase[10];
		_count = 0;
	}

	public void Add<T>(LuiCompType key, T value) where T : LuiCompBase
	{
		if (_count >= _values.Length)
		{
			throw new InvalidOperationException("Dictionary is full");
		}
		_values[_count] = value;
		_count++;
	}

	public void Clear()
	{
		_count = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetValue<T>(LuiCompType key, out T value) where T : LuiCompBase
	{
		for (int i = 0; i < _count; i++)
		{
			if (_values[i].type == key && _values[i] is T val)
			{
				value = val;
				return true;
			}
		}
		value = null;
		return false;
	}

	public IEnumerator GetEnumerator()
	{
		for (int i = 0; i < _count; i++)
		{
			yield return _values[i];
		}
	}
}
