using UnityEngine;

public class ReflectionPlane : DecayEntity
{
	private static readonly int _ColorTexID = Shader.PropertyToID("_ColorTex");

	private static readonly int _DepthTexID = Shader.PropertyToID("_DepthTex");

	private static readonly int _ReflectionLerpID = Shader.PropertyToID("_ReflectionLerp");

	[Header("Reflection Plane")]
	public LayerMask layerMask;

	public float nearClip;

	public float farClip;

	public Material reflectionMaterial;

	public Renderer reflectionRenderer;

	public float maxDistance;

	public float fadeTime;

	public ReflectionPlane()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		layerMask = LayerMask.op_Implicit(-1);
		fadeTime = 0.25f;
		base._002Ector();
	}
}
