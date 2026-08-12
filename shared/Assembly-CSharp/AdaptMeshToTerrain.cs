using UnityEngine;

[ExecuteInEditMode]
public class AdaptMeshToTerrain : MonoBehaviour
{
	public LayerMask LayerMask;

	public float RayHeight;

	public float RayMaxDistance;

	public float MinDisplacement;

	public float MaxDisplacement;

	[Range(8f, 64f)]
	public int PlaneResolution;

	public AdaptMeshToTerrain()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		LayerMask = LayerMask.op_Implicit(-1);
		RayHeight = 10f;
		RayMaxDistance = 20f;
		MinDisplacement = 0.01f;
		MaxDisplacement = 0.33f;
		PlaneResolution = 24;
		((MonoBehaviour)this)._002Ector();
	}
}
