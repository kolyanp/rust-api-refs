using UnityEngine;

public class LerpBetweenPointsBool : MonoBehaviour, IClientComponent, IFlattenKeepsParent, IFlattenKeepsDescendants
{
	public Vector3 offsetPosLocal;

	public float speed;

	public bool UseAsOffset;

	[UnityEvent]
	public void SetAtOffset(bool should)
	{
	}
}
