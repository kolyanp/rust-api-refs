using System.Runtime.CompilerServices;
using UnityEngine;

namespace Carbon.Components;

public readonly struct LuiPosition(float xMin, float yMin, float xMax, float yMax)
{
	public static readonly LuiPosition None = new LuiPosition(0f, 0f, 0f, 0f);

	public static readonly LuiPosition Full = new LuiPosition(0f, 0f, 1f, 1f);

	public static readonly LuiPosition UpperLeft = new LuiPosition(0f, 1f, 0f, 1f);

	public static readonly LuiPosition UpperCenter = new LuiPosition(0.5f, 1f, 0.5f, 1f);

	public static readonly LuiPosition UpperRight = new LuiPosition(1f, 1f, 1f, 1f);

	public static readonly LuiPosition MiddleLeft = new LuiPosition(0f, 0.5f, 0f, 0.5f);

	public static readonly LuiPosition MiddleCenter = new LuiPosition(0.5f, 0.5f, 0.5f, 0.5f);

	public static readonly LuiPosition MiddleRight = new LuiPosition(1f, 0.5f, 1f, 0.5f);

	public static readonly LuiPosition LowerLeft = new LuiPosition(0f, 0f, 0f, 0f);

	public static readonly LuiPosition LowerCenter = new LuiPosition(0.5f, 0f, 0.5f, 0f);

	public static readonly LuiPosition LowerRight = new LuiPosition(1f, 0f, 1f, 0f);

	public readonly Vector2 anchorMin = new Vector2(xMin, yMin);

	public readonly Vector2 anchorMax = new Vector2(xMax, yMax);

	public static bool operator ==(LuiPosition a, LuiPosition b)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (a.anchorMax == b.anchorMax)
		{
			return a.anchorMin == b.anchorMin;
		}
		return false;
	}

	public static bool operator !=(LuiPosition a, LuiPosition b)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!(a.anchorMax != b.anchorMax))
		{
			return a.anchorMin != b.anchorMin;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is LuiPosition other)
		{
			return Equals(other);
		}
		return false;
	}

	private bool Equals(LuiPosition other)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (((Vector2)(ref anchorMax)).Equals(other.anchorMax))
		{
			return ((Vector2)(ref anchorMin)).Equals(other.anchorMin);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = 17;
		num = num * 31 + ((object)Unsafe.As<Vector2, Vector2>(ref anchorMax)/*cast due to constrained. prefix*/).GetHashCode();
		return num * 31 + ((object)Unsafe.As<Vector2, Vector2>(ref anchorMin)/*cast due to constrained. prefix*/).GetHashCode();
	}
}
