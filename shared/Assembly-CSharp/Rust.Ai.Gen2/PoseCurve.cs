using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai.Gen2;

public struct PoseCurve
{
	public enum Piece : byte
	{
		None,
		Left,
		Right,
		Straight,
		TurnLeft,
		TurnRight
	}

	public const float TurnOnTheSpotLength = 1f;

	private const float TwoPi = MathF.PI * 2f;

	private const float FullTurnSnap = 0.0001f;

	public Vector2 Start;

	public float StartHeading;

	public float Radius;

	private Piece piece0;

	private Piece piece1;

	private Piece piece2;

	private Piece piece3;

	private float length0;

	private float length1;

	private float length2;

	private float length3;

	public readonly float Length => length0 + length1 + length2 + length3;

	public readonly float Turning => TurningOf(piece0, length0) + TurningOf(piece1, length1) + TurningOf(piece2, length2) + TurningOf(piece3, length3);

	public static Vector2 Direction(float heading)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(Mathf.Cos(heading), Mathf.Sin(heading));
	}

	public static float HeadingOf(Vector2 direction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Atan2(direction.y, direction.x);
	}

	public readonly void GetPiece(int index, out Piece piece, out float length)
	{
		switch (index)
		{
		case 0:
			piece = piece0;
			length = length0;
			break;
		case 1:
			piece = piece1;
			length = length1;
			break;
		case 2:
			piece = piece2;
			length = length2;
			break;
		case 3:
			piece = piece3;
			length = length3;
			break;
		default:
			piece = Piece.None;
			length = 0f;
			break;
		}
	}

	public bool Append(Piece piece, float length)
	{
		if (!(length > 0f) || float.IsInfinity(length))
		{
			return true;
		}
		if (piece0 == Piece.None)
		{
			piece0 = piece;
			length0 = length;
			return true;
		}
		if (piece1 == Piece.None)
		{
			piece1 = piece;
			length1 = length;
			return true;
		}
		if (piece2 == Piece.None)
		{
			piece2 = piece;
			length2 = length;
			return true;
		}
		if (piece3 == Piece.None)
		{
			piece3 = piece;
			length3 = length;
			return true;
		}
		return false;
	}

	public readonly void Sample(float distance, out Vector2 position, out float heading)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		position = Start;
		heading = StartHeading;
		float num = Mathf.Clamp(distance, 0f, Length);
		for (int i = 0; i < 4; i++)
		{
			if (!(num > 0f))
			{
				break;
			}
			GetPiece(i, out var piece, out var length);
			if (piece != Piece.None)
			{
				float num2 = Mathf.Min(num, length);
				Advance(piece, num2, Radius, ref position, ref heading);
				num -= num2;
				continue;
			}
			break;
		}
	}

	private static void Advance(Piece piece, float distance, float radius, ref Vector2 position, ref float heading)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		switch (piece)
		{
		case Piece.Straight:
			position += Direction(heading) * distance;
			break;
		case Piece.Left:
		{
			float num2 = distance / radius;
			position += new Vector2(Mathf.Sin(heading + num2) - Mathf.Sin(heading), Mathf.Cos(heading) - Mathf.Cos(heading + num2)) * radius;
			heading += num2;
			break;
		}
		case Piece.Right:
		{
			float num = distance / radius;
			position += new Vector2(Mathf.Sin(heading) - Mathf.Sin(heading - num), Mathf.Cos(heading - num) - Mathf.Cos(heading)) * radius;
			heading -= num;
			break;
		}
		case Piece.TurnLeft:
			heading += distance / 1f;
			break;
		case Piece.TurnRight:
			heading -= distance / 1f;
			break;
		}
	}

	private readonly float TurningOf(Piece piece, float length)
	{
		switch (piece)
		{
		case Piece.Left:
		case Piece.Right:
			return length / Radius;
		case Piece.TurnLeft:
		case Piece.TurnRight:
			return length / 1f;
		default:
			return 0f;
		}
	}

	public static PoseCurve TurnWalkTurn(Vector2 from, float fromHeading, Vector2 to, float toHeading)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		PoseCurve result = new PoseCurve
		{
			Start = from,
			StartHeading = fromHeading,
			Radius = 0f
		};
		Vector2 direction = to - from;
		float magnitude = direction.magnitude;
		float num = fromHeading;
		if (magnitude > 0.0001f)
		{
			float num2 = HeadingOf(direction);
			result.AppendTurn(Mathf.DeltaAngle(num * 57.29578f, num2 * 57.29578f) * (MathF.PI / 180f));
			result.Append(Piece.Straight, magnitude);
			num = num2;
		}
		result.AppendTurn(Mathf.DeltaAngle(num * 57.29578f, toHeading * 57.29578f) * (MathF.PI / 180f));
		return result;
	}

	private void AppendTurn(float radians)
	{
		if (!(Mathf.Abs(radians) < 0.001f))
		{
			Append((radians > 0f) ? Piece.TurnLeft : Piece.TurnRight, Mathf.Abs(radians) * 1f);
		}
	}

	public static void AddDubins(Vector2 from, float fromHeading, Vector2 to, float toHeading, float radius, PooledList<PoseCurve> curves)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		if (!(radius <= 0f))
		{
			Vector2 val = to - from;
			float num = val.magnitude / radius;
			float num2 = ((num > 0f) ? Mod2Pi(Mathf.Atan2(val.y, val.x)) : 0f);
			float num3 = Mod2Pi(fromHeading - num2);
			float num4 = Mod2Pi(toHeading - num2);
			float num5 = Mathf.Sin(num3);
			float num6 = Mathf.Sin(num4);
			float num7 = Mathf.Cos(num3);
			float num8 = Mathf.Cos(num4);
			float num9 = Mathf.Cos(num3 - num4);
			float num10 = num * num;
			float num11 = 2f + num10 - 2f * num9 + 2f * num * (num5 - num6);
			if (num11 >= 0f)
			{
				float num12 = Mathf.Atan2(num8 - num7, num + num5 - num6);
				AddWord(curves, from, fromHeading, radius, Piece.Left, Mod2Pi(num12 - num3), Piece.Straight, Mathf.Sqrt(num11), Piece.Left, Mod2Pi(num4 - num12));
			}
			num11 = 2f + num10 - 2f * num9 + 2f * num * (num6 - num5);
			if (num11 >= 0f)
			{
				float num13 = Mathf.Atan2(num7 - num8, num - num5 + num6);
				AddWord(curves, from, fromHeading, radius, Piece.Right, Mod2Pi(num3 - num13), Piece.Straight, Mathf.Sqrt(num11), Piece.Right, Mod2Pi(num13 - num4));
			}
			num11 = -2f + num10 + 2f * num9 + 2f * num * (num5 + num6);
			if (num11 >= 0f)
			{
				float num14 = Mathf.Sqrt(num11);
				float num15 = Mathf.Atan2(0f - num7 - num8, num + num5 + num6) - Mathf.Atan2(-2f, num14);
				AddWord(curves, from, fromHeading, radius, Piece.Left, Mod2Pi(num15 - num3), Piece.Straight, num14, Piece.Right, Mod2Pi(num15 - num4));
			}
			num11 = -2f + num10 + 2f * num9 - 2f * num * (num5 + num6);
			if (num11 >= 0f)
			{
				float num16 = Mathf.Sqrt(num11);
				float num17 = Mathf.Atan2(num7 + num8, num - num5 - num6) - Mathf.Atan2(2f, num16);
				AddWord(curves, from, fromHeading, radius, Piece.Right, Mod2Pi(num3 - num17), Piece.Straight, num16, Piece.Left, Mod2Pi(num4 - num17));
			}
			float num18 = (6f - num10 + 2f * num9 + 2f * num * (num5 - num6)) / 8f;
			if (Mathf.Abs(num18) <= 1f)
			{
				float num19 = Mathf.Atan2(num7 - num8, num - num5 + num6);
				float num20 = Mod2Pi(MathF.PI * 2f - Mathf.Acos(num18));
				float num21 = Mod2Pi(num3 - num19 + num20 * 0.5f);
				AddWord(curves, from, fromHeading, radius, Piece.Right, num21, Piece.Left, num20, Piece.Right, Mod2Pi(num3 - num4 - num21 + num20));
			}
			num18 = (6f - num10 + 2f * num9 + 2f * num * (num6 - num5)) / 8f;
			if (Mathf.Abs(num18) <= 1f)
			{
				float num22 = Mathf.Atan2(num7 - num8, num + num5 - num6);
				float num23 = Mod2Pi(MathF.PI * 2f - Mathf.Acos(num18));
				float num24 = Mod2Pi(0f - num3 - num22 + num23 * 0.5f);
				AddWord(curves, from, fromHeading, radius, Piece.Left, num24, Piece.Right, num23, Piece.Left, Mod2Pi(num4 - num3 - num24 + num23));
			}
		}
	}

	private static void AddWord(PooledList<PoseCurve> curves, Vector2 from, float fromHeading, float radius, Piece first, float firstAngle, Piece second, float secondLength, Piece third, float thirdAngle)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		PoseCurve item = new PoseCurve
		{
			Start = from,
			StartHeading = fromHeading,
			Radius = radius
		};
		item.Append(first, OuterArc(firstAngle) * radius);
		item.Append(second, secondLength * radius);
		item.Append(third, OuterArc(thirdAngle) * radius);
		((List<PoseCurve>)(object)curves).Add(item);
	}

	private static float OuterArc(float angle)
	{
		if (!(angle > 6.2830853f))
		{
			return angle;
		}
		return 0f;
	}

	private static float Mod2Pi(float angle)
	{
		angle %= MathF.PI * 2f;
		if (!(angle < 0f))
		{
			return angle;
		}
		return angle + MathF.PI * 2f;
	}
}
