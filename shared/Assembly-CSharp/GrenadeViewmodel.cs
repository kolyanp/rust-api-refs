using UnityEngine;

public class GrenadeViewmodel : MonoBehaviour, IViewmodelComponent, IAnimationEventReceiver
{
	public Animator TargetAnimator;

	[Tooltip("The grenade the drop clip's drop_reveal event hands over to the predicted one, which appears lined up with this mesh")]
	public Renderer DropRevealMesh;

	[Tooltip("Turns the predicted grenade to line up with DropRevealMesh, for world models that aren't modelled facing the same way")]
	public Vector3 DropRevealRotationOffset;

	[Tooltip("Useful for aligning the predicted grenade to line up with viewmodel DropRevealMesh")]
	public Vector3 DropRevealPositionOffset;

	[Tooltip("These will be hidden after the drop_reveal event until the drop_restore event, once the clip has carried them off screen")]
	public Renderer[] RenderersHiddenByDropEvent;
}
