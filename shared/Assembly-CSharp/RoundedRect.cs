using System;
using System.Collections.Generic;
using UnityEngine;

public static class RoundedRect
{
	public static void AppendPerimeter(List<Vector2> points, Rect rect, float topLeft, float topRight, float bottomRight, float bottomLeft, int segmentsPerCorner)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Min(((Rect)(ref rect)).width, ((Rect)(ref rect)).height) * 0.5f;
		float num2 = Mathf.Clamp(topLeft, 0f, num);
		float num3 = Mathf.Clamp(topRight, 0f, num);
		float num4 = Mathf.Clamp(bottomRight, 0f, num);
		float num5 = Mathf.Clamp(bottomLeft, 0f, num);
		AppendArc(points, new Vector2(((Rect)(ref rect)).xMax - num4, ((Rect)(ref rect)).yMin + num4), num4, 270f, segmentsPerCorner);
		AppendArc(points, new Vector2(((Rect)(ref rect)).xMax - num3, ((Rect)(ref rect)).yMax - num3), num3, 0f, segmentsPerCorner);
		AppendArc(points, new Vector2(((Rect)(ref rect)).xMin + num2, ((Rect)(ref rect)).yMax - num2), num2, 90f, segmentsPerCorner);
		AppendArc(points, new Vector2(((Rect)(ref rect)).xMin + num5, ((Rect)(ref rect)).yMin + num5), num5, 180f, segmentsPerCorner);
	}

	private static void AppendArc(List<Vector2> points, Vector2 center, float radius, float startAngleDeg, int segments)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i <= segments; i++)
		{
			float num = (startAngleDeg + 90f * (float)i / (float)segments) * (MathF.PI / 180f);
			points.Add(center + new Vector2(Mathf.Cos(num), Mathf.Sin(num)) * radius);
		}
	}
}
