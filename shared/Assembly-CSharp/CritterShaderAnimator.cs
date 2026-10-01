using UnityEngine;

public class CritterShaderAnimator : MonoBehaviour, IClientComponent
{
	[Tooltip("Speed at which the walk animation reaches full blend. Below this the legs scale down, at 0 speed the critter is still.")]
	public float fullAnimationSpeed = 0.5f;

	[Tooltip("How quickly the walk animation blends in and out as the critter starts and stops.")]
	public float animationBlendRate = 6f;

	[Tooltip("Caps how fast the legs cycle, in the same units as the material's Move Speed. At or above Move Speed it has no effect.")]
	public float maxLegSpeed = 12f;
}
