using UnityEngine;

public class TravellingVendorEvent : TriggeredEvent
{
	public static TravellingVendor currentVendor = null;

	public static float dontSpawnHoursBeforeWipe = 24f;

	public override void RunEvent()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)currentVendor != (Object)null) && !((Object)(object)TerrainMeta.Path == (Object)null) && TerrainMeta.Path.Roads.Count != 0 && TravellingVendor.should_spawn && RoadBradleys.StaticBradleyCount <= 0 && TerrainMeta.Path.MainRoads.Count != 0)
		{
			TravellingVendor travellingVendor = TravellingVendor.SpawnTravellingVendorForEvent();
			if (Object.op_Implicit((Object)(object)travellingVendor))
			{
				Debug.Log((object)"[event] assets/prefabs/npc/travelling vendor/travellingvendor.prefab");
				currentVendor = travellingVendor;
				BasePlayer.Server_SendWorldNotificationToAllActivePlayers(WorldNotificationConfig.NotificationType.TravellingVendorSpawned, ((Component)currentVendor).transform.position);
			}
			else
			{
				Debug.Log((object)"Failed to spawn travelling vendor.");
			}
		}
	}

	private bool HoursCheck()
	{
		if (WipeTimer.serverinstance.GetTimeSpanUntilWipe().TotalHours > (double)dontSpawnHoursBeforeWipe)
		{
			return true;
		}
		return false;
	}
}
