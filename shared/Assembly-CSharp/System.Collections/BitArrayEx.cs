using System.Text;
using Facepunch;

namespace System.Collections;

public static class BitArrayEx
{
	public static bool TryFind(this BitArray array, out int i, int max = int.MaxValue, bool value = false, int start = 0)
	{
		int num = Math.Min(array.Length, max);
		for (i = start; i < num; i++)
		{
			if (array.Get(i) == value)
			{
				return true;
			}
		}
		return false;
	}

	public static string ToStringEx(this BitArray array)
	{
		StringBuilder stringBuilder = Pool.Get<StringBuilder>();
		stringBuilder.EnsureCapacity(array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array.Get(i) ? '|' : '_');
		}
		string result = stringBuilder.ToString();
		Pool.FreeUnmanaged(ref stringBuilder);
		return result;
	}
}
