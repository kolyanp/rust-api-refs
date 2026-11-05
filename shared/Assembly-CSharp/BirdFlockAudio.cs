using UnityEngine;

public class BirdFlockAudio : MonoBehaviour, IClientComponent
{
	[Header("Movement Audio")]
	public SoundDefinition movementLoopDef;

	[Tooltip("How many birds near the listener get their own wing loop.")]
	public int maxMovementVoices = 3;

	[Tooltip("Loop pitch range. The wing beat rate from the sim is clamped into this: gliding sits at the bottom, a hard climb at the top.")]
	public Vector2 movementPitchRange = new Vector2(0.9f, 1.15f);

	[Tooltip("Wing beat rate at or below which the loop is silent, so a gliding bird doesn't flap. 1 is level flight.")]
	public float glideFlapRate = 0.4f;

	public float movementFadeInTime = 0.2f;

	public float movementFadeOutTime = 0.3f;

	[Tooltip("A silent bird steals a voice when its squared distance is below this fraction of the farthest voiced bird's. 0.5 means it has to be about 30 percent closer, which stops two birds at similar range swapping back and forth.")]
	public float stealDistanceRatio = 0.5f;

	[Tooltip("One shot definition holding the close call clips.")]
	[Header("Close Calls")]
	public SoundDefinition closeCallDef;

	[Tooltip("How many birds can be calling at once. Each is on its own timer and picks a fresh random bird for every call.")]
	public int closeCallVoices = 3;

	[Tooltip("Seconds between one voice's calls, rolled randomly each time.")]
	public Vector2 closeCallIntervalRange = new Vector2(1f, 3f);

	[Header("Far Calls")]
	[Tooltip("One shot definition holding the distant call clips. Played from the entity at any range.")]
	public SoundDefinition farCallDef;

	[Tooltip("Seconds between far calls, rolled randomly each time.")]
	public Vector2 farCallIntervalRange = new Vector2(2f, 6f);

	public BirdFlockAudio()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
	}
}
