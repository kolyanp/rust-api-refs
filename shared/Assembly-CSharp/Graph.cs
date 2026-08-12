using UnityEngine;

public abstract class Graph : MonoBehaviour
{
	public Material Material;

	public int Resolution;

	public Vector2 ScreenFill;

	public Vector2 ScreenOrigin;

	public Vector2 Pivot;

	public Rect Area;

	internal float CurrentValue;

	private int index;

	private float[] values;

	private float max;

	protected abstract float GetValue();

	protected abstract Color GetColor(float value);

	protected Vector3 GetVertex(float x, float y)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(x, y, 0f);
	}

	protected void Update()
	{
		if (values == null || values.Length != Resolution)
		{
			values = new float[Resolution];
		}
		max = 0f;
		for (int i = 0; i < values.Length - 1; i++)
		{
			max = Mathf.Max(max, values[i] = values[i + 1]);
		}
		max = Mathf.Max(max, CurrentValue = (values[values.Length - 1] = GetValue()));
	}

	protected void OnGUI()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Event.current.type == 7 && values != null && values.Length != 0)
		{
			float num = Mathf.Max(((Rect)(ref Area)).width, ScreenFill.x * (float)Screen.width);
			float num2 = Mathf.Max(((Rect)(ref Area)).height, ScreenFill.y * (float)Screen.height);
			float num3 = ((Rect)(ref Area)).x - Pivot.x * num + ScreenOrigin.x * (float)Screen.width;
			float num4 = ((Rect)(ref Area)).y - Pivot.y * num2 + ScreenOrigin.y * (float)Screen.height;
			GL.PushMatrix();
			Material.SetPass(0);
			GL.LoadPixelMatrix();
			GL.Begin(7);
			for (int i = 0; i < values.Length; i++)
			{
				float num5 = values[i];
				float num6 = num / (float)values.Length;
				float num7 = num2 * num5 / max;
				float num8 = num3 + (float)i * num6;
				float num9 = num4;
				GL.Color(GetColor(num5));
				GL.Vertex(GetVertex(num8 + 0f, num9 + num7));
				GL.Vertex(GetVertex(num8 + num6, num9 + num7));
				GL.Vertex(GetVertex(num8 + num6, num9 + 0f));
				GL.Vertex(GetVertex(num8 + 0f, num9 + 0f));
			}
			GL.End();
			GL.PopMatrix();
		}
	}

	protected Graph()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Resolution = 128;
		ScreenFill = new Vector2(0f, 0f);
		ScreenOrigin = new Vector2(0f, 0f);
		Pivot = new Vector2(0f, 0f);
		Area = new Rect(0f, 0f, 128f, 32f);
		((MonoBehaviour)this)._002Ector();
	}
}
