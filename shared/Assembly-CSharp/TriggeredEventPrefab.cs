using Oxide.Core;
using UnityEngine;

public class TriggeredEventPrefab : TriggeredEvent
{
	public GameObjectRef targetPrefab;

	public bool shouldBroadcastSpawn;

	public WorldNotificationConfig.NotificationType notificationType;

	public BaseEntity spawnedEntity;

	public override void RunEvent()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnEventTrigger", this) != null)
		{
			return;
		}
		Debug.Log((object)("[event] " + targetPrefab.resourcePath));
		BaseEntity baseEntity = GameManager.server.CreateEntity(targetPrefab.resourcePath);
		if (Object.op_Implicit((Object)(object)baseEntity))
		{
			((Component)baseEntity).SendMessage("TriggeredEventSpawn", (SendMessageOptions)1);
			baseEntity.Spawn();
			((Component)baseEntity).SendMessage("TriggeredEventPostSpawn", (SendMessageOptions)1);
			spawnedEntity = baseEntity;
			if (shouldBroadcastSpawn)
			{
				BasePlayer.Server_SendWorldNotificationToAllActivePlayers(notificationType, ((Component)spawnedEntity).transform.position);
			}
		}
	}

	public override void Kill()
	{
		if (!((Object)(object)spawnedEntity == (Object)null))
		{
			base.Kill();
			spawnedEntity.Kill();
			spawnedEntity = null;
			Debug.Log((object)("Killed " + ((Object)this).name));
		}
	}
}
