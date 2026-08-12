using UnityEngine;

public class ScaleBySpeed : MonoBehaviour, IClientComponent
{
	public float minScale;

	public float maxScale;

	public float minSpeed;

	public float maxSpeed;

	public MonoBehaviour component;

	public bool toggleComponent;

	public bool onlyWhenSubmerged;

	public float submergedThickness;

	private Vector3 prevPosition;

	public ScaleBySpeed()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		minScale = 0.001f;
		maxScale = 1f;
		maxSpeed = 1f;
		toggleComponent = true;
		submergedThickness = 0.33f;
		prevPosition = Vector3.zero;
		((MonoBehaviour)this)._002Ector();
	}
}
