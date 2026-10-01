using System;
using UnityEngine;

[Serializable]
public class MetabolismAttribute
{
	public enum Type
	{
		Calories,
		Hydration,
		Heartrate,
		Poison,
		Radiation,
		Bleeding,
		Health,
		HealthOverTime
	}

	public float startMin;

	public float startMax;

	public float min;

	public float max;

	public float value;

	[NonSerialized]
	public float bonusMax;

	public float lastValue;

	internal float lastGreatFraction;

	private const float greatInterval = 0.1f;

	public float EffectiveMax => max + bonusMax;

	public float greatFraction => Mathf.Floor(Fraction() / 0.1f) / 10f;

	public void Reset()
	{
		bonusMax = 0f;
		value = Mathf.Clamp(Random.Range(startMin, startMax), min, max);
	}

	public float Fraction()
	{
		return Mathf.InverseLerp(min, max, value);
	}

	public float InverseFraction()
	{
		return 1f - Fraction();
	}

	public float OverfillFraction()
	{
		if (max <= 0f)
		{
			return 0f;
		}
		return Mathf.Clamp01((value - max) / max);
	}

	public void Add(float val)
	{
		value = Mathf.Clamp(value + val, min, EffectiveMax);
	}

	public void Subtract(float val)
	{
		value = Mathf.Clamp(value - val, min, EffectiveMax);
	}

	public void Set(float val)
	{
		value = Mathf.Clamp(val, min, EffectiveMax);
	}

	public void Increase(float fTarget)
	{
		fTarget = Mathf.Clamp(fTarget, min, EffectiveMax);
		if (!(fTarget <= value))
		{
			value = fTarget;
		}
	}

	public void MoveTowards(float fTarget, float fRate)
	{
		if (fRate != 0f)
		{
			value = Mathf.Clamp(Mathf.MoveTowards(value, fTarget, fRate), min, EffectiveMax);
		}
	}

	public bool HasChanged()
	{
		bool flag = Mathf.Abs(lastValue - value) > 0.01f;
		if (flag)
		{
			lastValue = value;
		}
		return flag;
	}

	public bool HasGreatlyChanged()
	{
		float num = greatFraction;
		bool result = lastGreatFraction != num || ((value == min || value == EffectiveMax) && value != lastValue);
		lastGreatFraction = num;
		lastValue = value;
		return result;
	}

	public void SetValue(float newValue)
	{
		value = newValue;
	}
}
