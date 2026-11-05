using UnityEngine;

public class PropSway : MonoBehaviour, IFlattenKeepsDescendants
{
	public Transform pivotRotator;

	public float swaySpeed = 1f;

	public float swayDistance = 0.25f;

	public float lerpSpeed = 2f;
}
