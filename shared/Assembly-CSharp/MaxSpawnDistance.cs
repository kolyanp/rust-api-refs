using UnityEngine;

public class MaxSpawnDistance : MonoBehaviour, IClientComponent
{
	public float maxDistance = 30f;

	[Tooltip("Spawn further away while the local player zooms in with a scope or binoculars")]
	public bool scaleWithZoom;
}
