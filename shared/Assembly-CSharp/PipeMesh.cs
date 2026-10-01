using UnityEngine;

public class PipeMesh : MonoBehaviour
{
	public float PipeRadius = 0.04f;

	public Material PipeMaterial;

	public float StraightLength = 0.3f;

	public int PipeSubdivisions = 8;

	public int BendTesselation = 6;

	public float RidgeHeight = 0.05f;

	public float UvScaleMultiplier = 2f;

	public float RidgeIncrements = 0.5f;

	public float RidgeLength = 0.05f;

	public Vector2 HorizontalUvRange = new Vector2(0f, 0.2f);

	public PipeMesh()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
	}
}
