using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class BorderGraphic : MaskableGraphic
{
	private float top;

	private float right;

	private float bottom;

	private float left;

	private float topLeft;

	private float topRight;

	private float bottomRight;

	private float bottomLeft;

	private int segmentsPerCorner = 8;

	private static readonly List<Vector2> outer = new List<Vector2>();

	private static readonly List<Vector2> inner = new List<Vector2>();

	public void SetSides(float top, float right, float bottom, float left, Color color)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		this.top = top;
		this.right = right;
		this.bottom = bottom;
		this.left = left;
		((Graphic)this).color = color;
		((Graphic)this).SetVerticesDirty();
	}

	public void SetCorners(float topLeft, float topRight, float bottomRight, float bottomLeft, int segmentsPerCorner)
	{
		this.topLeft = topLeft;
		this.topRight = topRight;
		this.bottomRight = bottomRight;
		this.bottomLeft = bottomLeft;
		this.segmentsPerCorner = segmentsPerCorner;
		((Graphic)this).SetVerticesDirty();
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		vh.Clear();
		Rect rect = ((Graphic)this).rectTransform.rect;
		if (topLeft <= 0f && topRight <= 0f && bottomRight <= 0f && bottomLeft <= 0f)
		{
			AddSquareBorder(vh, rect);
			return;
		}
		outer.Clear();
		inner.Clear();
		RoundedRect.AppendPerimeter(outer, rect, topLeft, topRight, bottomRight, bottomLeft, segmentsPerCorner);
		Rect rect2 = Rect.MinMaxRect(((Rect)(ref rect)).xMin + left, ((Rect)(ref rect)).yMin + bottom, ((Rect)(ref rect)).xMax - right, ((Rect)(ref rect)).yMax - top);
		float num = Mathf.Max(0f, topLeft - Mathf.Max(top, left));
		float num2 = Mathf.Max(0f, topRight - Mathf.Max(top, right));
		float num3 = Mathf.Max(0f, bottomRight - Mathf.Max(bottom, right));
		float num4 = Mathf.Max(0f, bottomLeft - Mathf.Max(bottom, left));
		RoundedRect.AppendPerimeter(inner, rect2, num, num2, num3, num4, segmentsPerCorner);
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.color = Color32.op_Implicit(((Graphic)this).color);
		for (int i = 0; i < outer.Count; i++)
		{
			simpleVert.position = Vector2.op_Implicit(outer[i]);
			vh.AddVert(simpleVert);
			simpleVert.position = Vector2.op_Implicit(inner[i]);
			vh.AddVert(simpleVert);
		}
		int count = outer.Count;
		for (int j = 0; j < count; j++)
		{
			int num5 = (j + 1) % count;
			int num6 = j * 2;
			int num7 = j * 2 + 1;
			int num8 = num5 * 2;
			int num9 = num5 * 2 + 1;
			vh.AddTriangle(num6, num8, num9);
			vh.AddTriangle(num9, num7, num6);
		}
	}

	private void AddSquareBorder(VertexHelper vh, Rect rect)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		float xMin = ((Rect)(ref rect)).xMin;
		float xMax = ((Rect)(ref rect)).xMax;
		float yMin = ((Rect)(ref rect)).yMin;
		float yMax = ((Rect)(ref rect)).yMax;
		Color color = ((Graphic)this).color;
		if (top > 0f)
		{
			AddQuad(vh, xMin, yMax - top, xMax, yMax, color);
		}
		if (bottom > 0f)
		{
			AddQuad(vh, xMin, yMin, xMax, yMin + bottom, color);
		}
		if (left > 0f)
		{
			AddQuad(vh, xMin, yMin + bottom, xMin + left, yMax - top, color);
		}
		if (right > 0f)
		{
			AddQuad(vh, xMax - right, yMin + bottom, xMax, yMax - top, color);
		}
	}

	private static void AddQuad(VertexHelper vh, float xMin, float yMin, float xMax, float yMax, Color color)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		int currentVertCount = vh.currentVertCount;
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.color = Color32.op_Implicit(color);
		simpleVert.position = new Vector3(xMin, yMin);
		vh.AddVert(simpleVert);
		simpleVert.position = new Vector3(xMin, yMax);
		vh.AddVert(simpleVert);
		simpleVert.position = new Vector3(xMax, yMax);
		vh.AddVert(simpleVert);
		simpleVert.position = new Vector3(xMax, yMin);
		vh.AddVert(simpleVert);
		vh.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
		vh.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
	}
}
