using System.Runtime.CompilerServices;
using UnityEngine;

namespace Carbon.Components;

public readonly struct LuiOffset(float xMin, float yMin, float xMax, float yMax)
{
	public static readonly LuiOffset None = new LuiOffset(0f, 0f, 0f, 0f);

	public readonly Vector2 offsetMin = new Vector2(xMin, yMin);

	public readonly Vector2 offsetMax = new Vector2(xMax, yMax);

	public static bool operator ==(LuiOffset a, LuiOffset b)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (a.offsetMax == b.offsetMax)
		{
			return a.offsetMin == b.offsetMin;
		}
		return false;
	}

	public static bool operator !=(LuiOffset a, LuiOffset b)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!(a.offsetMax != b.offsetMax))
		{
			return a.offsetMin != b.offsetMin;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is LuiOffset other)
		{
			return Equals(other);
		}
		return false;
	}

	private bool Equals(LuiOffset other)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (((Vector2)(ref offsetMin)).Equals(other.offsetMin))
		{
			return ((Vector2)(ref offsetMax)).Equals(other.offsetMax);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = 17;
		num = num * 31 + ((object)Unsafe.As<Vector2, Vector2>(ref offsetMin)/*cast due to constrained. prefix*/).GetHashCode();
		return num * 31 + ((object)Unsafe.As<Vector2, Vector2>(ref offsetMax)/*cast due to constrained. prefix*/).GetHashCode();
	}
}
