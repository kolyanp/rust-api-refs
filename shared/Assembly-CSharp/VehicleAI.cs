using UnityEngine;

public static class VehicleAI
{
	public const string BOAT_AI_PATH = "assets/prefabs/npc/travelling vendor/travellingvendor.prefab";

	public static void AttachBoatAI(BaseBoat boat)
	{
		if (PrefabAttribute.server.Find<AIDriverData>(boat.prefabID) == null)
		{
			Debug.LogError((object)$"No AIDriverData found for {((Object)boat).name} with prefabID {boat.prefabID}. Can't attach AI.");
		}
		GameManager.server.CreatePrefab("assets/prefabs/npc/travelling vendor/travellingvendor.prefab", ((Component)boat).transform);
	}
}
