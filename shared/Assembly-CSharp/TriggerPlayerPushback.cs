using UnityEngine;

public class TriggerPlayerPushback : TriggerBase, IClientComponent
{
	[Tooltip("The colliders a player can't stand on")]
	public Collider[] slideOffColliders;

	public float pushSpeed = 1f;
}
