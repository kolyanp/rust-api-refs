using UnityEngine;

public class SkinViewerPivotShift : MonoBehaviour, IClientComponent
{
	public SkinViewerModelAnimator modelAnimator;

	public Transform model;

	public Vector3 toggledOffset;

	public float smoothTime = 0.6f;
}
