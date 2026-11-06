using UnityEngine;

namespace Rust.Ai.Gen2;

public class SeagullGroupBirds : MonoBehaviour, IClientComponent
{
	private enum BirdMode : byte
	{
		Ground,
		TakingOff,
		Flying,
		Landing
	}

	[SerializeField]
	private SeagullGroup group;

	[SerializeField]
	private BirdFlockSwarm swarm;

	[SerializeField]
	private Transform[] birds;

	[SerializeField]
	private Animator[] animators;

	[SerializeField]
	[Tooltip("Base of each bird's body on its rig, which with bodyChests gives the angle the animation holds the body at.")]
	private Transform[] bodyTails;

	[SerializeField]
	private Transform[] bodyChests;

	[SerializeField]
	private float wanderRadius = 2.5f;

	[SerializeField]
	private float walkSpeed = 0.5f;

	[SerializeField]
	private float turnSpeed = 360f;

	[SerializeField]
	private Vector2 idleDurationRange = new Vector2(2f, 8f);

	[SerializeField]
	[Tooltip("Closest a walking bird passes another, in metres. It only walks to a spot it can reach without passing closer, and stops to let others by.")]
	private float walkClearance = 0.4f;

	[Tooltip("Furthest up or down a bird steps as it walks or runs, in metres. It never walks or runs onto ground any higher or lower than this.")]
	[SerializeField]
	private float maxStepHeight = 0.1f;

	[SerializeField]
	private string[] idleStates = new string[5] { "seagull_idle", "seagull_idle_look", "seagull_peck", "seagull_groom", "seagull_caw" };

	[SerializeField]
	[Tooltip("How close to the player a flying bird can come before it swaps back to its Animator to land, in metres.")]
	[Header("Handover")]
	private float handoverDistance = 5f;

	[Tooltip("How close to the edge of the group's bounds a bird taking off can get before it joins the flock, in metres. Past the bounds a hit on it is rejected, flock birds stay well inside them.")]
	[SerializeField]
	private float boundsMargin = 1f;

	[Header("Takeoff")]
	[SerializeField]
	private string takeoffState = "seagull_take_off";

	[Tooltip("How long into the takeoff clip the bird leaves the ground, in seconds.")]
	[SerializeField]
	private float liftoffDelay = 0.3f;

	[SerializeField]
	[Tooltip("Widest a bird strays either side of straight away from the startle, in degrees. Higher scatters the group more.")]
	private float scatterAngle = 80f;

	[Tooltip("Longest a bird hesitates before it reacts to the startle, in seconds. Each bird picks its own, so they don't all go at once.")]
	[SerializeField]
	private float maxReactionDelay = 0.8f;

	[Tooltip("How much one bird's run, takeoff speed and climb can differ from another's, as a fraction either side.")]
	[SerializeField]
	private float takeoffSpeedVariance = 0.4f;

	[Tooltip("How fast a startled bird runs along the ground before it takes off, in metres per second. Matches the run entry in the Locomotion blend tree.")]
	[SerializeField]
	private float scatterRunSpeed = 1.2f;

	[Tooltip("How long a startled bird runs before it takes off, in seconds. Rolled per bird, so some take straight off and others run first.")]
	[SerializeField]
	private Vector2 scatterRunDuration = new Vector2(0f, 1.2f);

	[SerializeField]
	[Tooltip("How fast a bird turns to face the way it takes off, in degrees per second.")]
	private float takeoffTurnSpeed = 720f;

	[Tooltip("Shortest time a bird plays the takeoff clip before it can join the flock for nearing the edge of the bounds, in seconds. Longer than liftoffDelay so it visibly takes off.")]
	[SerializeField]
	private float minTakeoffDuration = 1f;

	[Tooltip("How long a bird animates its own takeoff before joining the flock, in seconds. No more than the clip's length.")]
	[SerializeField]
	private float takeoffWindow = 3f;

	[Tooltip("How fast a bird climbs while taking off, in metres per second.")]
	[SerializeField]
	private float takeoffClimbSpeed = 2f;

	[Tooltip("How quickly a bird taking off picks up forward speed, in metres per second per second.")]
	[SerializeField]
	private float takeoffAcceleration = 4f;

	[Header("Landing")]
	[SerializeField]
	private string landState = "seagull_land";

	[SerializeField]
	[Tooltip("Height above the ground a flying bird swaps back to its Animator to land, in metres. Rolled per bird, so they peel off one at a time.")]
	private Vector2 landHandoverHeightRange = new Vector2(2f, 6f);

	[Tooltip("Longest a bird keeps flying once the group starts coming down before it lands, in seconds. Rolled per bird.")]
	[SerializeField]
	private float maxLandingDelay = 1.5f;

	[SerializeField]
	[Tooltip("How much faster or slower one bird plays the land clip than another, as a fraction either side. The glide is timed to match.")]
	private float landingSpeedVariance = 0.15f;

	[Tooltip("How far the glide carries on along the bird's heading before curving into its spot, as a share of the distance to it. 0 flies straight at the spot.")]
	[SerializeField]
	private float landingCurve = 0.5f;

	[Tooltip("How far into the land clip the bird's feet touch the ground, in seconds. The glide down is timed to arrive then.")]
	[SerializeField]
	private float touchdownTime = 2.25f;

	[Tooltip("How long the land clip plays before the bird starts idling, in seconds. No less than the clip's length.")]
	[SerializeField]
	private float landSettleTime = 2.93f;

	[Tooltip("Lowest the flock flies over the ground while the group comes down, in metres.")]
	[SerializeField]
	private float landingClearance = 0.5f;

	[Tooltip("How far below the group a landing bird looks for the ground, in metres. Needs to reach down from the height the group starts coming down at.")]
	[SerializeField]
	private float landingProbeDepth = 60f;

	public SeagullGroupBirds()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
	}
}
