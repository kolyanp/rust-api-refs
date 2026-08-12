using UnityEngine;

public class PlanarReflectionCamera : MonoBehaviour
{
	public static PlanarReflectionCamera instance;

	public float updateRate;

	public float nearClipPlane;

	public float farClipPlane;

	public Color fogColor;

	public float fogDensity;

	public Mesh waterPlaneMesh;

	public Material waterPlaneMaterial;

	public PlanarReflectionCamera()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		updateRate = 1f;
		nearClipPlane = 0.3f;
		farClipPlane = 25f;
		fogColor = Color.white;
		fogDensity = 0.1f;
		((MonoBehaviour)this)._002Ector();
	}
}
