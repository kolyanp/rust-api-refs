using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Diagnostics;

[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)]
[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
internal class ActivityTagsCollection : IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
{
	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(0)]
	public struct Enumerator : IEnumerator<KeyValuePair<string, object>>, IDisposable, IEnumerator
	{
		private List<KeyValuePair<string, object>>.Enumerator _enumerator;

		[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })]
		public KeyValuePair<string, object> Current
		{
			[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })]
			get
			{
				return _enumerator.Current;
			}
		}

		[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)]
		object IEnumerator.Current => ((IEnumerator)_enumerator).Current;

		internal Enumerator(List<KeyValuePair<string, object>> list)
		{
			_enumerator = list.GetEnumerator();
		}

		public void Dispose()
		{
			_enumerator.Dispose();
		}

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}

		void IEnumerator.Reset()
		{
			((IEnumerator)_enumerator).Reset();
		}
	}

	private List<KeyValuePair<string, object>> _list = new List<KeyValuePair<string, object>>();

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)]
	public object this[string key]
	{
		[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)]
		get
		{
			int num = FindIndex(key);
			if (num >= 0)
			{
				return _list[num].Value;
			}
			return null;
		}
		[param: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)]
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			int num = FindIndex(key);
			if (value == null)
			{
				if (num >= 0)
				{
					_list.RemoveAt(num);
				}
			}
			else if (num >= 0)
			{
				_list[num] = new KeyValuePair<string, object>(key, value);
			}
			else
			{
				_list.Add(new KeyValuePair<string, object>(key, value));
			}
		}
	}

	public ICollection<string> Keys
	{
		get
		{
			List<string> list = new List<string>(_list.Count);
			foreach (KeyValuePair<string, object> item in _list)
			{
				list.Add(item.Key);
			}
			return list;
		}
	}

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 2 })]
	public ICollection<object> Values
	{
		[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 2 })]
		get
		{
			List<object> list = new List<object>(_list.Count);
			foreach (KeyValuePair<string, object> item in _list)
			{
				list.Add(item.Value);
			}
			return list;
		}
	}

	public bool IsReadOnly => false;

	public int Count => _list.Count;

	public ActivityTagsCollection()
	{
	}

	public ActivityTagsCollection([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0, 1, 2 })] IEnumerable<KeyValuePair<string, object>> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		foreach (KeyValuePair<string, object> item in list)
		{
			if (item.Key != null)
			{
				this[item.Key] = item.Value;
			}
		}
	}

	public void Add(string key, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] object value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		int num = FindIndex(key);
		if (num >= 0)
		{
			throw new InvalidOperationException(_003C98aa27ce_002Da3b7_002D4f67_002D9b18_002D59078b0717c9_003ESR.Format(_003C98aa27ce_002Da3b7_002D4f67_002D9b18_002D59078b0717c9_003ESR.KeyAlreadyExist, key));
		}
		_list.Add(new KeyValuePair<string, object>(key, value));
	}

	public void Add([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> item)
	{
		if (item.Key == null)
		{
			throw new ArgumentNullException("item");
		}
		int num = FindIndex(item.Key);
		if (num >= 0)
		{
			throw new InvalidOperationException(_003C98aa27ce_002Da3b7_002D4f67_002D9b18_002D59078b0717c9_003ESR.Format(_003C98aa27ce_002Da3b7_002D4f67_002D9b18_002D59078b0717c9_003ESR.KeyAlreadyExist, item.Key));
		}
		_list.Add(item);
	}

	public void Clear()
	{
		_list.Clear();
	}

	public bool Contains([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> item)
	{
		return _list.Contains(item);
	}

	public bool ContainsKey(string key)
	{
		return FindIndex(key) >= 0;
	}

	public void CopyTo([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0, 1, 2 })] KeyValuePair<string, object>[] array, int arrayIndex)
	{
		_list.CopyTo(array, arrayIndex);
	}

	IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
	{
		return new Enumerator(_list);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_list);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(_list);
	}

	public bool Remove(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		int num = FindIndex(key);
		if (num >= 0)
		{
			_list.RemoveAt(num);
			return true;
		}
		return false;
	}

	public bool Remove([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> item)
	{
		return _list.Remove(item);
	}

	public bool TryGetValue(string key, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] out object value)
	{
		int num = FindIndex(key);
		if (num >= 0)
		{
			value = _list[num].Value;
			return true;
		}
		value = null;
		return false;
	}

	private int FindIndex(string key)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			if (_list[i].Key == key)
			{
				return i;
			}
		}
		return -1;
	}
}
