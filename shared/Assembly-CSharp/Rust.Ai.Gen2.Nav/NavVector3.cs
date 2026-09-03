using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Rust.Ai.Gen2.Nav;

public readonly struct NavVector3 : IEquatable<NavVector3>
{
	public readonly Vector3 Value;

	public static readonly NavVector3 zero;

	public static readonly NavVector3 up;

	public float x => Value.x;

	public float y => Value.y;

	public float z => Value.z;

	public NavVector3 normalized
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return new NavVector3(((Vector3)(ref Value)).normalized);
		}
	}

	public float magnitude => ((Vector3)(ref Value)).magnitude;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NavVector3(Vector3 positionNS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Value = positionNS;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NavVector3(float x, float y, float z)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Value = new Vector3(x, y, z);
	}

	public NavVector3 WithY(float newY)
	{
		return new NavVector3(Value.x, newY, Value.z);
	}

	public NavVector3 Flat()
	{
		return new NavVector3(Value.x, 0f, Value.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static NavVector3 operator +(NavVector3 a, NavVector3 b)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new NavVector3(a.Value + b.Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static NavVector3 operator -(NavVector3 a, NavVector3 b)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new NavVector3(a.Value - b.Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static NavVector3 operator *(NavVector3 positionNS, float scale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new NavVector3(positionNS.Value * scale);
	}

	public static NavVector3 operator *(Quaternion q, NavVector3 positionNS)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new NavVector3(q * positionNS.Value);
	}

	public static NavVector3 operator /(NavVector3 positionNS, float scale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new NavVector3(positionNS.Value / scale);
	}

	public static bool operator ==(NavVector3 aNS, NavVector3 bNS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return aNS.Value == bNS.Value;
	}

	public static bool operator !=(NavVector3 aNS, NavVector3 bNS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return aNS.Value != bNS.Value;
	}

	public static explicit operator Vector3(NavVector3 positionNS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return positionNS.Value;
	}

	public static Vector3 LookDirection(NavVector3 fromNS, NavVector3 toNS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = toNS.Value - fromNS.Value;
		return ((Vector3)(ref val)).normalized;
	}

	public static float Dot(NavVector3 aNS, NavVector3 bNS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Dot(aNS.Value, bNS.Value);
	}

	public static NavVector3 Cross(NavVector3 aNS, NavVector3 bNS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new NavVector3(Vector3.Cross(aNS.Value, bNS.Value));
	}

	public static float Distance(NavVector3 aNS, NavVector3 bNS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(aNS.Value, bNS.Value);
	}

	public static float DistanceXZ(NavVector3 aNS, NavVector3 bNS)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(aNS.Flat().Value, bNS.Flat().Value);
	}

	public NavVector3 NormalizeXZ()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = new Vector3(Value.x, 0f, Value.z);
		return new NavVector3(((Vector3)(ref val)).normalized);
	}

	public static float SqrDistance(NavVector3 aNS, NavVector3 bNS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = aNS.Value - bNS.Value;
		return ((Vector3)(ref val)).sqrMagnitude;
	}

	public static NavVector3 Lerp(NavVector3 aNS, NavVector3 bNS, float t)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return new NavVector3(Vector3.Lerp(aNS.Value, bNS.Value, t));
	}

	public static NavVector3 MoveTowards(NavVector3 currentNS, NavVector3 targetNS, float maxDistanceDelta)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return new NavVector3(Vector3.MoveTowards(currentNS.Value, targetNS.Value, maxDistanceDelta));
	}

	public static NavVector3 RotateTowards(NavVector3 currentNS, NavVector3 targetNS, float maxRadiansDelta, float maxMagnitudeDelta)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return new NavVector3(Vector3.RotateTowards(currentNS.Value, targetNS.Value, maxRadiansDelta, maxMagnitudeDelta));
	}

	public static NavVector3 ClampMagnitude(NavVector3 vectorNS, float maxLength)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new NavVector3(Vector3.ClampMagnitude(vectorNS.Value, maxLength));
	}

	public bool Equals(NavVector3 other)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((Vector3)(ref Value)).Equals(other.Value);
	}

	public override bool Equals(object obj)
	{
		if (obj is NavVector3 other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((object)Unsafe.As<Vector3, Vector3>(ref Value)/*cast due to constrained. prefix*/).GetHashCode();
	}

	public override string ToString()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return $"NS{Value}";
	}

	static NavVector3()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		zero = new NavVector3(Vector3.zero);
		up = new NavVector3(Vector3.up);
	}
}
