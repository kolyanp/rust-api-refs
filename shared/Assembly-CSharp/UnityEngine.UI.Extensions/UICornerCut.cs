namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Primitives/Cut Corners")]
public class UICornerCut : UIPrimitiveBase
{
	public Vector2 cornerSize = new Vector2(16f, 16f);

	[SerializeField]
	[Header("Corners to cut")]
	private bool m_cutUL = true;

	[SerializeField]
	private bool m_cutUR;

	[SerializeField]
	private bool m_cutLL;

	[SerializeField]
	private bool m_cutLR;

	[Tooltip("Up-Down colors become Left-Right colors")]
	[SerializeField]
	private bool m_makeColumns;

	[Header("Color the cut bars differently")]
	[SerializeField]
	private bool m_useColorUp;

	[SerializeField]
	private Color32 m_colorUp;

	[SerializeField]
	private bool m_useColorDown;

	[SerializeField]
	private Color32 m_colorDown;

	public bool CutUL
	{
		get
		{
			return m_cutUL;
		}
		set
		{
			m_cutUL = value;
			((Graphic)this).SetAllDirty();
		}
	}

	public bool CutUR
	{
		get
		{
			return m_cutUR;
		}
		set
		{
			m_cutUR = value;
			((Graphic)this).SetAllDirty();
		}
	}

	public bool CutLL
	{
		get
		{
			return m_cutLL;
		}
		set
		{
			m_cutLL = value;
			((Graphic)this).SetAllDirty();
		}
	}

	public bool CutLR
	{
		get
		{
			return m_cutLR;
		}
		set
		{
			m_cutLR = value;
			((Graphic)this).SetAllDirty();
		}
	}

	public bool MakeColumns
	{
		get
		{
			return m_makeColumns;
		}
		set
		{
			m_makeColumns = value;
			((Graphic)this).SetAllDirty();
		}
	}

	public bool UseColorUp
	{
		get
		{
			return m_useColorUp;
		}
		set
		{
			m_useColorUp = value;
		}
	}

	public Color32 ColorUp
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_colorUp;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_colorUp = value;
		}
	}

	public bool UseColorDown
	{
		get
		{
			return m_useColorDown;
		}
		set
		{
			m_useColorDown = value;
		}
	}

	public Color32 ColorDown
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_colorDown;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_colorDown = value;
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = ((Graphic)this).rectTransform.rect;
		Rect val = rect;
		Color32 val2 = Color32.op_Implicit(((Graphic)this).color);
		bool flag = m_cutUL | m_cutUR;
		bool flag2 = m_cutLL | m_cutLR;
		bool flag3 = m_cutLL | m_cutUL;
		bool flag4 = m_cutLR | m_cutUR;
		if (!(flag | flag2) || !(cornerSize.sqrMagnitude > 0f))
		{
			return;
		}
		vh.Clear();
		if (flag3)
		{
			val.xMin += cornerSize.x;
		}
		if (flag2)
		{
			val.yMin += cornerSize.y;
		}
		if (flag)
		{
			val.yMax -= cornerSize.y;
		}
		if (flag4)
		{
			val.xMax -= cornerSize.x;
		}
		Vector2 val3;
		Vector2 val4;
		Vector2 val5;
		Vector2 val6;
		if (m_makeColumns)
		{
			val3 = new Vector2(rect.xMin, m_cutUL ? val.yMax : rect.yMax);
			val4 = new Vector2(rect.xMax, m_cutUR ? val.yMax : rect.yMax);
			val5 = new Vector2(rect.xMin, m_cutLL ? val.yMin : rect.yMin);
			val6 = new Vector2(rect.xMax, m_cutLR ? val.yMin : rect.yMin);
			if (flag3)
			{
				AddSquare(val5, val3, new Vector2(val.xMin, rect.yMax), new Vector2(val.xMin, rect.yMin), rect, m_useColorUp ? m_colorUp : val2, vh);
			}
			if (flag4)
			{
				AddSquare(val4, val6, new Vector2(val.xMax, rect.yMin), new Vector2(val.xMax, rect.yMax), rect, m_useColorDown ? m_colorDown : val2, vh);
			}
		}
		else
		{
			val3 = new Vector2(m_cutUL ? val.xMin : rect.xMin, rect.yMax);
			val4 = new Vector2(m_cutUR ? val.xMax : rect.xMax, rect.yMax);
			val5 = new Vector2(m_cutLL ? val.xMin : rect.xMin, rect.yMin);
			val6 = new Vector2(m_cutLR ? val.xMax : rect.xMax, rect.yMin);
			if (flag2)
			{
				AddSquare(val6, val5, new Vector2(rect.xMin, val.yMin), new Vector2(rect.xMax, val.yMin), rect, m_useColorDown ? m_colorDown : val2, vh);
			}
			if (flag)
			{
				AddSquare(val3, val4, new Vector2(rect.xMax, val.yMax), new Vector2(rect.xMin, val.yMax), rect, m_useColorUp ? m_colorUp : val2, vh);
			}
		}
		if (m_makeColumns)
		{
			AddSquare(new Rect(val.xMin, rect.yMin, val.width, rect.height), rect, val2, vh);
		}
		else
		{
			AddSquare(new Rect(rect.xMin, val.yMin, rect.width, val.height), rect, val2, vh);
		}
	}

	private static void AddSquare(Rect rect, Rect rectUV, Color32 color32, VertexHelper vh)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		int num = AddVert(rect.xMin, rect.yMin, rectUV, color32, vh);
		int num2 = AddVert(rect.xMin, rect.yMax, rectUV, color32, vh);
		int num3 = AddVert(rect.xMax, rect.yMax, rectUV, color32, vh);
		int num4 = AddVert(rect.xMax, rect.yMin, rectUV, color32, vh);
		vh.AddTriangle(num, num2, num3);
		vh.AddTriangle(num3, num4, num);
	}

	private static void AddSquare(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Rect rectUV, Color32 color32, VertexHelper vh)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		int num = AddVert(a.x, a.y, rectUV, color32, vh);
		int num2 = AddVert(b.x, b.y, rectUV, color32, vh);
		int num3 = AddVert(c.x, c.y, rectUV, color32, vh);
		int num4 = AddVert(d.x, d.y, rectUV, color32, vh);
		vh.AddTriangle(num, num2, num3);
		vh.AddTriangle(num3, num4, num);
	}

	private static int AddVert(float x, float y, Rect area, Color32 color32, VertexHelper vh)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = new Vector2(Mathf.InverseLerp(area.xMin, area.xMax, x), Mathf.InverseLerp(area.yMin, area.yMax, y));
		vh.AddVert(new Vector3(x, y), color32, Vector4.op_Implicit(val));
		return vh.currentVertCount - 1;
	}

	public UICornerCut()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
	}
}
