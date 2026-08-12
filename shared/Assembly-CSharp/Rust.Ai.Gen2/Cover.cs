using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public struct Cover : IEquatable<Cover>
{
	[Flags]
	public enum Peeks
	{
		None = 0,
		Left = 1,
		Right = 2,
		Up = 4,
		Sides = Left | Right,
		All = Sides | Up
	}

	public Vector3 position;

	public float yaw;

	public Peeks peeks;

	public const float sidePeekLength = 1f;

	private const int ShotTestLayer = 1486954497;

	public bool NeedDucking => (peeks & Peeks.Up) == Peeks.Up;

	public Cover(Vector3 position, float yaw, Peeks peeks)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		this.position = position;
		this.yaw = yaw;
		this.peeks = peeks;
	}

	public bool ProtectsFrom(Vector3 threatLocation)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = threatLocation - position;
		return Mathf.Abs(Mathf.DeltaAngle(Mathf.Atan2(val.x, val.z) * 57.29578f, yaw)) <= 50f;
	}

	public Peeks GetAnyPeek()
	{
		if ((peeks & Peeks.Left) == Peeks.Left)
		{
			return Peeks.Left;
		}
		if ((peeks & Peeks.Right) == Peeks.Right)
		{
			return Peeks.Right;
		}
		if ((peeks & Peeks.Up) == Peeks.Up)
		{
			return Peeks.Up;
		}
		return Peeks.None;
	}

	public Peeks GetFirstUnoccludedPeek(Vector3 target, BaseEntity entity = null)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if ((peeks & Peeks.Left) == Peeks.Left && !IsPeekOccluded(Peeks.Left, target, entity))
		{
			return Peeks.Left;
		}
		if ((peeks & Peeks.Right) == Peeks.Right && !IsPeekOccluded(Peeks.Right, target, entity))
		{
			return Peeks.Right;
		}
		if ((peeks & Peeks.Up) == Peeks.Up && !IsPeekOccluded(Peeks.Up, target, entity))
		{
			return Peeks.Up;
		}
		return Peeks.None;
	}

	public bool IsPeekOccluded(Peeks peek, Vector3 target, BaseEntity entity = null)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Cover.IsPeekOccluded"))
		{
			Vector3 peekLocation = GetPeekLocation(peek);
			Vector3 val = target - peekLocation;
			float magnitude = ((Vector3)(ref val)).magnitude;
			Vector3 val2 = val / magnitude;
			RaycastHit hitInfo;
			return GamePhysics.Trace(new Ray(peekLocation, val2), 0f, out hitInfo, magnitude, 1486954497, (QueryTriggerInteraction)1);
		}
	}

	public Vector3 GetPeekLocation(Peeks peek)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = GetForward();
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(forward.z, 0f, 0f - forward.x);
		Vector3 val2 = position + PlayerEyes.EyeOffset;
		if ((peek & Peeks.Right) == Peeks.Right)
		{
			val2 += val * 1f;
		}
		if ((peek & Peeks.Left) == Peeks.Left)
		{
			val2 -= val * 1f;
		}
		return val2;
	}

	public Vector3 GetPeekGroundLocation(Peeks peek)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return GetPeekLocation(peek) - PlayerEyes.EyeOffset;
	}

	public Vector3 GetForward()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(Mathf.Sin(yaw * (MathF.PI / 180f)), 0f, Mathf.Cos(yaw * (MathF.PI / 180f)));
	}

	public bool Equals(Cover other)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return position == other.position;
	}

	public override bool Equals(object obj)
	{
		if (obj is Cover other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((object)Unsafe.As<Vector3, Vector3>(ref position)/*cast due to constrained. prefix*/).GetHashCode();
	}

	public static bool operator ==(Cover left, Cover right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Cover left, Cover right)
	{
		return !(left == right);
	}
}
