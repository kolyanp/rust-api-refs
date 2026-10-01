using System;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Primitives/UI Polygon")]
public class UIPolygon : UIPrimitiveBase
{
	public bool fill = true;

	public float thickness = 5f;

	[Range(3f, 360f)]
	public int sides = 3;

	[Range(0f, 360f)]
	public float rotation;

	[Range(0f, 1f)]
	public float[] VerticesDistances = new float[3];

	private float size;

	public void DrawPolygon(int _sides)
	{
		sides = _sides;
		VerticesDistances = new float[_sides + 1];
		for (int i = 0; i < _sides; i++)
		{
			VerticesDistances[i] = 1f;
		}
		rotation = 0f;
		((Graphic)this).SetAllDirty();
	}

	public void DrawPolygon(int _sides, float[] _VerticesDistances)
	{
		sides = _sides;
		VerticesDistances = _VerticesDistances;
		rotation = 0f;
		((Graphic)this).SetAllDirty();
	}

	public void DrawPolygon(int _sides, float[] _VerticesDistances, float _rotation)
	{
		sides = _sides;
		VerticesDistances = _VerticesDistances;
		rotation = _rotation;
		((Graphic)this).SetAllDirty();
	}

	private void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = ((Graphic)this).rectTransform.rect;
		size = rect.width;
		rect = ((Graphic)this).rectTransform.rect;
		float width = rect.width;
		rect = ((Graphic)this).rectTransform.rect;
		if (width > rect.height)
		{
			rect = ((Graphic)this).rectTransform.rect;
			size = rect.height;
		}
		else
		{
			rect = ((Graphic)this).rectTransform.rect;
			size = rect.width;
		}
		thickness = Mathf.Clamp(thickness, 0f, size / 2f);
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		vh.Clear();
		Vector2 val = Vector2.zero;
		Vector2 val2 = Vector2.zero;
		Vector2 val3 = new Vector2(0f, 0f);
		Vector2 val4 = new Vector2(0f, 1f);
		Vector2 val5 = new Vector2(1f, 1f);
		Vector2 val6 = new Vector2(1f, 0f);
		float num = 360f / (float)sides;
		int num2 = sides + 1;
		if (VerticesDistances.Length != num2)
		{
			VerticesDistances = new float[num2];
			for (int i = 0; i < num2 - 1; i++)
			{
				VerticesDistances[i] = 1f;
			}
		}
		VerticesDistances[num2 - 1] = VerticesDistances[0];
		for (int j = 0; j < num2; j++)
		{
			float num3 = (0f - ((Graphic)this).rectTransform.pivot.x) * size * VerticesDistances[j];
			float num4 = (0f - ((Graphic)this).rectTransform.pivot.x) * size * VerticesDistances[j] + thickness;
			float num5 = MathF.PI / 180f * ((float)j * num + rotation);
			float num6 = Mathf.Cos(num5);
			float num7 = Mathf.Sin(num5);
			val3 = new Vector2(0f, 1f);
			val4 = new Vector2(1f, 1f);
			val5 = new Vector2(1f, 0f);
			val6 = new Vector2(0f, 0f);
			Vector2 val7 = val;
			Vector2 val8 = new Vector2(num3 * num6, num3 * num7);
			Vector2 val9;
			Vector2 val10;
			if (fill)
			{
				val9 = Vector2.zero;
				val10 = Vector2.zero;
			}
			else
			{
				val9 = new Vector2(num4 * num6, num4 * num7);
				val10 = val2;
			}
			val = val8;
			val2 = val9;
			vh.AddUIVertexQuad(SetVbo(new Vector2[4] { val7, val8, val9, val10 }, new Vector2[4] { val3, val4, val5, val6 }));
		}
	}
}
