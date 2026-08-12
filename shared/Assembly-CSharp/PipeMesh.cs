using UnityEngine;

public class PipeMesh : MonoBehaviour
{
	public float PipeRadius;

	public Material PipeMaterial;

	public float StraightLength;

	public int PipeSubdivisions;

	public int BendTesselation;

	public float RidgeHeight;

	public float UvScaleMultiplier;

	public float RidgeIncrements;

	public float RidgeLength;

	public Vector2 HorizontalUvRange;

	public PipeMesh()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		PipeRadius = 0.04f;
		StraightLength = 0.3f;
		PipeSubdivisions = 8;
		BendTesselation = 6;
		RidgeHeight = 0.05f;
		UvScaleMultiplier = 2f;
		RidgeIncrements = 0.5f;
		RidgeLength = 0.05f;
		HorizontalUvRange = new Vector2(0f, 0.2f);
		((MonoBehaviour)this)._002Ector();
	}
}
