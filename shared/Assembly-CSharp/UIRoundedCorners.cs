using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Rounded Corners")]
[DisallowMultipleComponent]
[RequireComponent(typeof(Graphic))]
public class UIRoundedCorners : BaseMeshEffect
{
	[SerializeField]
	private float topLeft = 16f;

	[SerializeField]
	private float topRight = 16f;

	[SerializeField]
	private float bottomRight = 16f;

	[SerializeField]
	private float bottomLeft = 16f;

	[Range(1f, 32f)]
	[SerializeField]
	private int segmentsPerCorner = 8;

	private static readonly List<UIVertex> stream = new List<UIVertex>();

	private static readonly List<Vector2> points = new List<Vector2>();

	public override void ModifyMesh(VertexHelper vh)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		if (!((UIBehaviour)this).IsActive())
		{
			return;
		}
		vh.GetUIVertexStream(stream);
		if (stream.Count == 0)
		{
			return;
		}
		Color32 color = stream[0].color;
		Vector2 val = Vector4.op_Implicit(stream[0].uv0);
		Vector2 val2 = Vector4.op_Implicit(stream[0].uv0);
		foreach (UIVertex item in stream)
		{
			val = Vector2.Min(val, Vector4.op_Implicit(item.uv0));
			val2 = Vector2.Max(val2, Vector4.op_Implicit(item.uv0));
		}
		Rect rect = ((BaseMeshEffect)this).graphic.rectTransform.rect;
		points.Clear();
		RoundedRect.AppendPerimeter(points, rect, topLeft, topRight, bottomRight, bottomLeft, segmentsPerCorner);
		vh.Clear();
		vh.AddVert(MakeVert(((Rect)(ref rect)).center, color, rect, val, val2));
		foreach (Vector2 point in points)
		{
			vh.AddVert(MakeVert(point, color, rect, val, val2));
		}
		for (int i = 0; i < points.Count; i++)
		{
			int num = (i + 1) % points.Count;
			vh.AddTriangle(0, i + 1, num + 1);
		}
	}

	private static UIVertex MakeVert(Vector2 pos, Color32 color, Rect rect, Vector2 uvMin, Vector2 uvMax)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.position = Vector2.op_Implicit(pos);
		simpleVert.color = color;
		float num = Mathf.InverseLerp(((Rect)(ref rect)).xMin, ((Rect)(ref rect)).xMax, pos.x);
		float num2 = Mathf.InverseLerp(((Rect)(ref rect)).yMin, ((Rect)(ref rect)).yMax, pos.y);
		simpleVert.uv0 = Vector4.op_Implicit(new Vector2(Mathf.Lerp(uvMin.x, uvMax.x, num), Mathf.Lerp(uvMin.y, uvMax.y, num2)));
		return simpleVert;
	}
}
