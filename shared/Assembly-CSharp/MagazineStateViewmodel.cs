using System;
using UnityEngine;

public class MagazineStateViewmodel : MonoBehaviour, IClientComponent, IViewmodelComponent, IAnimationEventReceiver
{
	public Animator TargetAnimator;

	[Tooltip("These gameobjects will be toggled active if the gun has > 1 bullet.")]
	public GameObject[] ShellRoots = Array.Empty<GameObject>();

	[Tooltip("One per round, element N is shown while the gun has more than N rounds. Order from last round fired to first round fired.")]
	public GameObject[] RoundVisuals = Array.Empty<GameObject>();

	[Tooltip("Optional, e.g. a magazine spring follower. Moved from FollowerRestPosition by FollowerStepPerRound for every hidden RoundVisuals entry.")]
	public Transform Follower;

	[Tooltip("Follower local position with a full magazine. Set here because the rig resets the bone to its import pose at runtime.")]
	public Vector3 FollowerRestPosition;

	public Vector3 FollowerStepPerRound;

	[Tooltip("Will set layer 1 to full weight if the gun has no ammo.")]
	public bool EmptyAmmoLayer;

	[Tooltip("Will update parameter 'hasAmmo' on the vm animator.")]
	public bool SetHasAmmoParam;

	[Tooltip("Will update parameters 'ammo_true' and 'ammo_false' on the vm animator.")]
	public bool SetAmmoTrueFalseParams;
}
