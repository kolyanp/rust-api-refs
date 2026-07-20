using System;
using UnityEngine;

public struct InvokeAction(Behaviour sender, Action action, InvokeTrackingData tracking, float initial = 0f, float repeat = -1f, float random = 0f) : IEquatable<InvokeAction>
{
	public InvokeTrackingKey Key = tracking?.Key ?? InvokeTrackingKey.Unknown;

	public InvokeTrackingData TrackingData = tracking;

	public Behaviour sender = sender;

	public Action action = action;

	public float initial = initial;

	public float repeat = repeat;

	public float random = random;

	public bool Equals(InvokeAction other)
	{
		if ((Object)(object)sender == (Object)(object)other.sender)
		{
			return action == other.action;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is InvokeAction)
		{
			return Equals((InvokeAction)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((object)sender).GetHashCode();
	}

	public static bool operator ==(InvokeAction x, InvokeAction y)
	{
		return x.Equals(y);
	}

	public static bool operator !=(InvokeAction x, InvokeAction y)
	{
		return !x.Equals(y);
	}
}
