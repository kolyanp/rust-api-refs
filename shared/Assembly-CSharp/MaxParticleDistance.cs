using UnityEngine;

public class MaxParticleDistance : MonoBehaviour, IClientComponent, IEffect, IFlattenKeepsDescendants
{
	[Tooltip("Particle systems spawned further than this distance from the camera won't emit. Except the ones spawning particle lights.")]
	public float maxDistance = 60f;

	[Tooltip("Emit further away while the local player zooms in with a scope or binoculars")]
	public bool scaleWithZoom = true;
}
