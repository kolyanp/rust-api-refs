using System;
using UnityEngine;

public static class RailJunction
{
	private const int JunctionSteps = 16;

	private const int JunctionSamplesPerStep = 8;

	private const int ValidationLookaheadSteps = 2;

	private const float MaxConnectionAngle = 8f;

	private const float MaxTurnAngle = 20f;

	private const float MinDepartureAngle = 0.5f;

	private const float MinSelectionAngle = 0.01f;

	private const float CrossoverTolerance = 0.001f;

	private const int SampleCount = 265;

	public static bool TryAlignBranch(PathInterpolator path, PathInterpolator parent, int startIndex, int endIndex, float width)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] points = path.Points;
		int num = points.Length - 1;
		if (num <= 32)
		{
			return false;
		}
		points[0] = parent.Points[startIndex];
		points[num] = parent.Points[endIndex];
		Span<Vector3> span = stackalloc Vector3[265];
		Span<Vector3> span2 = stackalloc Vector3[265];
		SamplePath(parent, startIndex - 1, 1, span);
		SamplePath(parent, endIndex + 1, -1, span2);
		float side = GetSide(points[16], span);
		float side2 = GetSide(points[num - 16], span2);
		if (Mathf.Abs(side) < width || Mathf.Abs(side2) < width || Mathf.Sign(side) == Mathf.Sign(side2))
		{
			return false;
		}
		ShapeJunction(path, 0, 1, Mathf.Sign(side), parent.Tangents[startIndex], span, 16, width);
		ShapeJunction(path, num, -1, Mathf.Sign(side2), -parent.Tangents[endIndex], span2, 16, width);
		path.RecalculateTangents();
		if (IsValidJunction(path, parent, 0, startIndex, 1, 1, Mathf.Sign(side), span, 16))
		{
			return IsValidJunction(path, parent, num, endIndex, -1, -1, Mathf.Sign(side2), span2, 16);
		}
		return false;
	}

	public static bool TryAlignConnection(PathInterpolator path, PathInterpolator parent, int attachment, int parentDirection, float width)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] points = path.Points;
		int num = points.Length - 1;
		int num2 = Mathf.Min(16, points.Length - 8);
		if (!parent.Circular)
		{
			int num3 = ((parentDirection > 0) ? (parent.Points.Length - 1 - attachment) : attachment);
			num2 = Mathf.Min(num2, num3 - 2);
		}
		if (num2 < 8 || path.Length <= 0f || parent.Length <= 0f)
		{
			return false;
		}
		Span<Vector3> span = stackalloc Vector3[265];
		span = span.Slice(0, (num2 * 2 + 1) * 8 + 1);
		SamplePath(parent, attachment - parentDirection, parentDirection, span);
		float side = GetSide(points[num - num2], span);
		if (Mathf.Abs(side) < width)
		{
			return false;
		}
		Span<Vector3> destination = stackalloc Vector3[16];
		Span<Vector3> destination2 = points.AsSpan(points.Length - num2, num2);
		destination2.CopyTo(destination);
		points[num] = parent.Points[attachment];
		ShapeJunction(path, num, -1, Mathf.Sign(side), parent.Tangents[attachment] * (float)parentDirection, span, num2, width);
		path.RecalculateTangents();
		if (IsValidJunction(path, parent, num, attachment, -1, parentDirection, Mathf.Sign(side), span, num2))
		{
			return true;
		}
		destination.Slice(0, num2).CopyTo(destination2);
		path.RecalculateTangents();
		return false;
	}

	public static bool TrySeparateEntrances(PathInterpolator first, PathInterpolator second, Vector3 firstForward, Vector3 secondForward, float width, Func<Vector3, bool> isWalkable)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		if (first.Points.Length <= 33 || second.Points.Length <= 33)
		{
			return false;
		}
		Vector3 val = second.Points[0] - first.Points[0];
		Vector3 val2 = Vector3Ex.WithY(val, 0f);
		if (!(val2.magnitude < width * 2f) && !(val2.magnitude > width * 4f) && !(Mathf.Abs(val.y) > 0.1f) && !(Vector3.Angle(firstForward, secondForward) > 1f))
		{
			Vector3 normalized = val2.normalized;
			Vector3 val3 = Vector3Ex.WithY(firstForward, 0f);
			if (!(Mathf.Abs(Vector3.Dot(normalized, val3.normalized)) > 0.1f))
			{
				if (Vector3.Dot(first.Points[16] - second.Points[16], val2) <= 0f)
				{
					return false;
				}
				Span<Vector3> span = stackalloc Vector3[145];
				Span<Vector3> span2 = stackalloc Vector3[145];
				SamplePath(first, 0, 1, span);
				SamplePath(second, 0, 1, span2);
				if (!AreSampledPathsCrossing(span, span2))
				{
					return false;
				}
				Span<Vector3> destination = stackalloc Vector3[16];
				Span<Vector3> destination2 = stackalloc Vector3[16];
				first.Points.AsSpan(0, 16).CopyTo(destination);
				second.Points.AsSpan(0, 16).CopyTo(destination2);
				Span<Vector3> span3 = stackalloc Vector3[145];
				Span<Vector3> span4 = stackalloc Vector3[145];
				for (int i = 8; i <= 16; i += 8)
				{
					for (int j = 0; j < i; j++)
					{
						float num = 1f - Mathf.SmoothStep(0f, 1f, (float)(j - 1) / (float)(i - 1));
						ref Vector3 reference = ref first.Points[j];
						reference += val * num;
						ref Vector3 reference2 = ref second.Points[j];
						reference2 -= val * num;
					}
					first.Points[0] = destination2[0];
					second.Points[0] = destination[0];
					first.RecalculateTangents();
					second.RecalculateTangents();
					SamplePath(first, 0, 1, span3);
					SamplePath(second, 0, 1, span4);
					if (IsValidEntrance(first, secondForward, span, span3, isWalkable) && IsValidEntrance(second, firstForward, span2, span4, isWalkable) && !AreSampledPathsCrossing(span3, span4))
					{
						return true;
					}
					destination.CopyTo(first.Points.AsSpan(0, 16));
					destination2.CopyTo(second.Points.AsSpan(0, 16));
				}
				first.RecalculateTangents();
				second.RecalculateTangents();
				return false;
			}
		}
		return false;
	}

	private static bool IsValidEntrance(PathInterpolator path, Vector3 forward, ReadOnlySpan<Vector3> originalSamples, ReadOnlySpan<Vector3> samples, Func<Vector3, bool> isWalkable)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Angle(path.Tangents[0], forward) >= 8f)
		{
			return false;
		}
		if (HasSharpTurns(path, 0, 1, 16))
		{
			return false;
		}
		for (int i = 0; i < samples.Length; i++)
		{
			if (!isWalkable(samples[i]) && isWalkable(originalSamples[i]))
			{
				return false;
			}
		}
		return true;
	}

	private static void ShapeJunction(PathInterpolator path, int endpoint, int direction, float side, Vector3 parentTangent, ReadOnlySpan<Vector3> parentSamples, int steps, float width)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i < steps; i++)
		{
			int num = (i + 1) * 8;
			Vector3 val = parentSamples[num + 1] - parentSamples[num - 1];
			Vector3 val2 = Vector3.Cross(Vector3.up, val);
			Vector3 normalized = val2.normalized;
			float num2 = Mathf.SmoothStep(0f, 1f, (float)i / (float)steps);
			Vector3 val3 = parentSamples[num] + normalized * (side * width * num2);
			int num3 = endpoint + direction * i;
			path.Points[num3] = Vector3.Lerp(val3, path.Points[num3], num2);
		}
		Vector3 val4 = path.Points[endpoint + direction] - path.Points[endpoint];
		float num4 = side * Vector3.SignedAngle(Vector3Ex.WithY(parentTangent, 0f), Vector3Ex.WithY(val4, 0f), Vector3.up);
		if (num4 < 0.5f)
		{
			Vector3 val5 = Quaternion.AngleAxis(side * (0.5f - num4), Vector3.up) * val4 - val4;
			for (int j = 1; j < 4; j++)
			{
				ref Vector3 reference = ref path.Points[endpoint + direction * j];
				reference += val5 * (1f - Mathf.SmoothStep(0f, 1f, (float)(j - 1) / 3f));
			}
		}
	}

	private static bool IsValidJunction(PathInterpolator path, PathInterpolator parent, int endpoint, int attachment, int direction, int parentDirection, float side, ReadOnlySpan<Vector3> parentSamples, int steps)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Angle(path.Tangents[endpoint] * (float)direction, parent.Tangents[attachment] * (float)parentDirection) >= 8f)
		{
			return false;
		}
		Vector3 val = parent.GetPointByIndex(attachment + parentDirection) - parent.Points[attachment];
		if (parent.Circular && val.sqrMagnitude == 0f)
		{
			val = parent.GetPointByIndex(attachment + parentDirection * 2) - parent.Points[attachment];
		}
		Vector3 val2 = path.Points[endpoint + direction] - path.Points[endpoint];
		if (side * Vector3.SignedAngle(val, val2, Vector3.up) <= 0.01f)
		{
			return false;
		}
		if (HasSharpTurns(path, endpoint, direction, steps))
		{
			return false;
		}
		float num = path.Length / (float)(path.Points.Length - 1);
		for (int i = 1; i <= (steps + 2) * 8; i++)
		{
			float num2 = (float)endpoint + (float)(direction * i) / 8f;
			Vector3 pointCubicHermite = path.GetPointCubicHermite(num2 * num);
			if (side * GetSide(pointCubicHermite, parentSamples) < -0.001f)
			{
				return false;
			}
		}
		return true;
	}

	private static bool HasSharpTurns(PathInterpolator path, int endpoint, int direction, int steps)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i <= steps; i++)
		{
			int num = endpoint + direction * i;
			Vector3 val = path.Points[num] - path.Points[num - direction];
			Vector3 val2 = path.Points[num + direction] - path.Points[num];
			if (Vector3.Angle(val, val2) > 20f)
			{
				return true;
			}
		}
		return false;
	}

	private static float GetSide(Vector3 point, ReadOnlySpan<Vector3> parentSamples)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		float num2 = 1f;
		float num3 = 0f;
		for (int i = 1; i < parentSamples.Length; i++)
		{
			Vector3 val = Vector3Ex.WithY(parentSamples[i] - parentSamples[i - 1], 0f);
			if (val.sqrMagnitude != 0f)
			{
				Vector3 val2 = Vector3Ex.WithY(point - parentSamples[i - 1], 0f);
				val2 -= val * Mathf.Clamp01(Vector3.Dot(val2, val) / val.sqrMagnitude);
				if (val2.sqrMagnitude < num)
				{
					num = val2.sqrMagnitude;
					num2 = val.sqrMagnitude;
					num3 = Vector3.Dot(val2, Vector3.Cross(Vector3.up, val));
				}
			}
		}
		return num3 / Mathf.Sqrt(num2);
	}

	private static bool AreSampledPathsCrossing(ReadOnlySpan<Vector3> first, ReadOnlySpan<Vector3> second)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i < first.Length; i++)
		{
			for (int j = 1; j < second.Length; j++)
			{
				Vector3 val = first[i - 1];
				Vector3 val2 = first[i];
				Vector3 val3 = second[j - 1];
				Vector3 val4 = second[j];
				if (!(Mathf.Max(val.x, val2.x) < Mathf.Min(val3.x, val4.x)) && !(Mathf.Max(val3.x, val4.x) < Mathf.Min(val.x, val2.x)) && !(Mathf.Max(val.z, val2.z) < Mathf.Min(val3.z, val4.z)) && !(Mathf.Max(val3.z, val4.z) < Mathf.Min(val.z, val2.z)) && Vector3.Cross(val2 - val, val3 - val).y * Vector3.Cross(val2 - val, val4 - val).y <= 0f && Vector3.Cross(val4 - val3, val - val3).y * Vector3.Cross(val4 - val3, val2 - val3).y <= 0f)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void SamplePath(PathInterpolator path, int startIndex, int direction, Span<Vector3> samples)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		float num = path.Length / (float)(path.Points.Length - 1);
		for (int i = 0; i < samples.Length; i++)
		{
			float num2 = ((float)startIndex + (float)(direction * i) / 8f) * num;
			if (path.Circular)
			{
				num2 = Mathf.Repeat(num2, path.Length);
			}
			samples[i] = path.GetPointCubicHermite(num2);
		}
	}
}
