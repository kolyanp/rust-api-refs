using UnityEngine;

public class AnimalAnimation : MonoBehaviour, IClientComponent
{
	public BaseEntity Entity;

	public BaseNpc Target;

	public Animator Animator;

	public MaterialEffect FootstepEffects;

	public Transform[] Feet;

	[Tooltip("Ensure there is a float param called idleOffset if this is enabled")]
	public bool hasIdleOffset;

	[Tooltip("Check animation clip weights when playing sounds so we don't play sounds for clips that are blended out")]
	public bool enforceClipWeights;

	[ReadOnly]
	public string BaseFolder;

	public string OverrideBaseFolder;

	[Tooltip("Optional. Redirects the SoundDefinitions animation events pass in, so a variant can reuse the adult's clips with its own sounds")]
	public SoundRemapping SoundRemapping;

	public float RequiredCameraDistanceForSfx;
}
