using UnityEngine;

public class CrabSwarm : MonoBehaviour, IClientComponent
{
	public CrabGrouping.CrabType[] crabTypes;

	public CrabGrouping[] crabGroupings;

	[Tooltip("Media capture only. Skips the swarm sim and draws a single crab sat on the entity, so it moves exactly like the entity does.")]
	public bool pinSingleCrabToEntity;
}
