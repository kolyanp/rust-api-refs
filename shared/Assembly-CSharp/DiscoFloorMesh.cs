using UnityEngine;

public class DiscoFloorMesh : MonoBehaviour, IClientComponent
{
	public int GridRows;

	public int GridColumns;

	public float GridSize;

	[Range(0f, 10f)]
	public float TestOffset;

	public Color OffColor;

	public MeshRenderer Renderer;

	public bool DrawInEditor;

	public MeshFilter Filter;

	public AnimationCurve customCurveX;

	public AnimationCurve customCurveY;

	public DiscoFloorMesh()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		GridRows = 5;
		GridColumns = 5;
		GridSize = 1f;
		OffColor = Color.grey;
		customCurveX = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		customCurveY = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		((MonoBehaviour)this)._002Ector();
	}
}
