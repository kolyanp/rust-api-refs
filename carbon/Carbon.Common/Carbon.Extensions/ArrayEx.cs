using System;
using System.Linq;

namespace Carbon.Extensions;

public static class ArrayEx
{
	public static T[] Randomize<T>(this T[] list)
	{
		T[] array = list.MakeCopy();
		int num = array.Length;
		while (num > 1)
		{
			num--;
			int randomInteger = RandomEx.GetRandomInteger(0, num);
			T val = array[randomInteger];
			array[randomInteger] = array[num];
			array[num] = val;
		}
		return array;
	}

	public static bool IsSame<T>(this T[] source, T[] target)
	{
		return !source.Except(target).Any();
	}

	public static T[][] Chunkify<T>(this T[] source, int chunkSize)
	{
		int num = (int)Math.Ceiling((double)source.Length / (double)chunkSize);
		T[][] array = new T[num][];
		for (int i = 0; i < num; i++)
		{
			int num2 = i * chunkSize;
			int num3 = Math.Min(num2 + chunkSize, source.Length);
			int num4 = num3 - num2;
			array[i] = new T[num4];
			Array.Copy(source, num2, array[i], 0, num4);
		}
		return array;
	}
}
