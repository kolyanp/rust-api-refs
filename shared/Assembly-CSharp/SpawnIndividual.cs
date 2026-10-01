using UnityEngine;

public struct SpawnIndividual(uint prefabID, Vector3 position, Quaternion rotation)
{
	public uint PrefabID = prefabID;

	public Vector3 Position = position;

	public Quaternion Rotation = rotation;
}
