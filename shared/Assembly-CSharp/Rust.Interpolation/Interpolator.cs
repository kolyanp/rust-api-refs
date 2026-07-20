using System.Collections.Generic;
using UnityEngine;

namespace Rust.Interpolation;

public class Interpolator<T> where T : ISnapshot<T>, new()
{
	public struct Segment
	{
		public T tick;

		public T prev;

		public T next;
	}

	public List<T> list;

	public T last;

	public Interpolator(int listCount)
	{
		list = new List<T>(listCount);
	}

	public void Add(T tick)
	{
		last = tick;
		list.Add(tick);
	}

	public void Cull(float beforeTime)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Time < beforeTime)
			{
				list.RemoveAt(i);
				i--;
			}
		}
	}

	public void Clear()
	{
		list.Clear();
	}

	public Segment Query(float time, float interpolation, float extrapolation, float smoothing, ref T t)
	{
		Segment result = default(Segment);
		if (list.Count == 0)
		{
			result.prev = last;
			result.next = last;
			result.tick = last;
			return result;
		}
		float num = time - interpolation - smoothing * 0.5f;
		float num2 = Mathf.Min(time - interpolation, last.Time);
		float num3 = num2 - smoothing;
		T val = list[0];
		T val2 = last;
		T val3 = list[0];
		T val4 = last;
		foreach (T item in list)
		{
			if (item.Time < num3)
			{
				val = item;
			}
			else if (val2.Time >= item.Time)
			{
				val2 = item;
			}
			if (item.Time < num2)
			{
				val3 = item;
			}
			else if (val4.Time >= item.Time)
			{
				val4 = item;
			}
		}
		T val5 = t.GetNew();
		if (val2.Time - val.Time <= Mathf.Epsilon)
		{
			val5.Time = num3;
			T entry = val2;
			val5.MatchValuesTo(entry);
		}
		else
		{
			val5.Time = num3;
			T prev = val;
			T next = val2;
			float delta = (num3 - val.Time) / (val2.Time - val.Time);
			val5.Lerp(prev, next, delta);
		}
		result.prev = val5;
		T val6 = t.GetNew();
		if (val4.Time - val3.Time <= Mathf.Epsilon)
		{
			val6.Time = num2;
			T entry2 = val4;
			val6.MatchValuesTo(entry2);
		}
		else
		{
			val6.Time = num2;
			T prev2 = val3;
			T next2 = val4;
			float delta2 = (num2 - val3.Time) / (val4.Time - val3.Time);
			val6.Lerp(prev2, next2, delta2);
		}
		result.next = val6;
		if (val6.Time - val5.Time <= Mathf.Epsilon)
		{
			result.prev = val6;
			result.tick = val6;
			return result;
		}
		if (num - val6.Time > extrapolation)
		{
			result.prev = val6;
			result.tick = val6;
			return result;
		}
		T tick = t.GetNew();
		tick.Time = num;
		T prev3 = val5;
		T next3 = val6;
		float delta3 = Mathf.Min(num - val5.Time, val6.Time + extrapolation - val5.Time) / (val6.Time - val5.Time);
		tick.Lerp(prev3, next3, delta3);
		result.tick = tick;
		return result;
	}
}
