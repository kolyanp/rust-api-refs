using UnityEngine;

public class ChildAnimatorSubSystem : AnimationSubSystem
{
	[SerializeField]
	protected RuntimeAnimatorController ChildController;

	[Range(0f, 1f)]
	[SerializeField]
	protected float Weight;
}
