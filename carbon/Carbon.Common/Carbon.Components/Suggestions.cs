using System;
using System.Collections.Generic;
using System.Linq;
using Facepunch;

namespace Carbon.Components;

public class Suggestions
{
	public class BufferBank : List<BufferInstance>
	{
		public int[,] Get(int x, int y)
		{
			BufferInstance bufferInstance = this.FirstOrDefault((BufferInstance instance) => instance.X == x && instance.Y == y);
			if (bufferInstance.Value == null)
			{
				BufferInstance item = new BufferInstance
				{
					X = x,
					Y = y,
					Value = new int[x, y]
				};
				Add(item);
				return item.Value;
			}
			for (int num = 0; num < x; num++)
			{
				for (int num2 = 0; num2 < y; num2++)
				{
					bufferInstance.Value[num, num2] = 0;
				}
			}
			return bufferInstance.Value;
		}
	}

	public struct BufferInstance
	{
		public int X;

		public int Y;

		public int[,] Value;
	}

	public struct SuggestionResult
	{
		public string Result;

		public int Confidence;
	}

	public static BufferBank Buffer = new BufferBank();

	public static SuggestionResult SingleLookup(string input, IEnumerable<string> values, int minimumConfidence = -1)
	{
		return Lookup(input, values, 1, minimumConfidence).FirstOrDefault();
	}

	public static IEnumerable<SuggestionResult> Lookup(string input, IEnumerable<string> values, int count = 3, int minimumConfidence = -1)
	{
		List<SuggestionResult> buffer = Pool.Get<List<SuggestionResult>>();
		int num = int.MaxValue;
		_ = string.Empty;
		foreach (string value in values)
		{
			int num2 = Compute(input, value);
			if (num2 < num)
			{
				num = num2;
				string result = value;
				SuggestionResult item = new SuggestionResult
				{
					Result = result,
					Confidence = num
				};
				buffer.Add(item);
			}
		}
		foreach (SuggestionResult item2 in (from x in buffer
			orderby x.Confidence
			where x.Confidence <= minimumConfidence
			select x).Take(count))
		{
			yield return item2;
		}
		Pool.FreeUnmanaged<SuggestionResult>(ref buffer);
	}

	internal static int Compute(string s, string t)
	{
		using (TimeMeasure.New("Suggestions.Compute"))
		{
			int length = s.Length;
			int length2 = t.Length;
			int[,] array = Buffer.Get(length + 1, length2 + 1);
			if (length == 0)
			{
				return length2;
			}
			if (length2 == 0)
			{
				return length;
			}
			int num = 0;
			while (num <= length)
			{
				array[num, 0] = num++;
			}
			int num2 = 0;
			while (num2 <= length2)
			{
				array[0, num2] = num2++;
			}
			for (int i = 1; i <= length; i++)
			{
				for (int j = 1; j <= length2; j++)
				{
					int num3 = ((t[j - 1] != s[i - 1]) ? 1 : 0);
					array[i, j] = Math.Min(Math.Min(array[i - 1, j] + 1, array[i, j - 1] + 1), array[i - 1, j - 1] + num3);
				}
			}
			return array[length, length2];
		}
	}
}
