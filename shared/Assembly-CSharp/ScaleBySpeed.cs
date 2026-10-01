using UnityEngine;

public class ScaleBySpeed : MonoBehaviour, IClientComponent
{
	public float minScale = 0.001f;

	public float maxScale = 1f;

	public float minSpeed;

	public float maxSpeed = 1f;

	public MonoBehaviour component;

	public bool toggleComponent = true;

	public bool onlyWhenSubmerged;

	public float submergedThickness = 0.33f;

	private Vector3 prevPosition = Vector3.zero;

	public ScaleBySpeed()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
	}
}
