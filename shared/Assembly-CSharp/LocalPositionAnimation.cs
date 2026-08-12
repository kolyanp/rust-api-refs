using UnityEngine;

public class LocalPositionAnimation : MonoBehaviour, IClientComponent
{
	public Vector3 centerPosition;

	public bool worldSpace;

	public float scaleX;

	public float timeScaleX;

	public AnimationCurve movementX;

	public float scaleY;

	public float timeScaleY;

	public AnimationCurve movementY;

	public float scaleZ;

	public float timeScaleZ;

	public AnimationCurve movementZ;

	public LocalPositionAnimation()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		scaleX = 1f;
		timeScaleX = 1f;
		movementX = new AnimationCurve();
		scaleY = 1f;
		timeScaleY = 1f;
		movementY = new AnimationCurve();
		scaleZ = 1f;
		timeScaleZ = 1f;
		movementZ = new AnimationCurve();
		((MonoBehaviour)this)._002Ector();
	}
}
