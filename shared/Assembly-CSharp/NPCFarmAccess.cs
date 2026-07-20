using UnityEngine;

public class NPCFarmAccess : NPCTalking
{
	public override void OnEntityMessage(BaseEntity from, string msg)
	{
		base.OnEntityMessage(from, msg);
		if (msg == "ForceOpenNPCDoor" && (Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null)
		{
			PointEntity<DeepSeaManager>.ServerInstance.RegisterPaidFoodToll(GetActionPlayer());
		}
	}
}
