using Oxide.Core;
using UnityEngine;

public class MetalDetectorFlag : BaseDiggableEntity
{
	public Collider Collision;

	public GameObject FlagModel;

	public float MoveUpBy = 0.2f;

	[ServerVar]
	public static float TimeoutDuration = 10800f;

	public override void ServerInit()
	{
		base.ServerInit();
		ResetTimeout();
	}

	private void ResetTimeout()
	{
		CancelInvoke(Timeout);
		Invoke(Timeout, TimeoutDuration * Random.Range(0.8f, 1.2f));
	}

	private void Timeout()
	{
		Kill();
	}

	public override void OnFullyDug(BasePlayer player)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnPlayerDigComplete", player, this) == null)
		{
			if ((Object)(object)Collision != (Object)null)
			{
				Collision.enabled = false;
			}
			BaseEntity baseEntity = SpawnLootListItem(player);
			BaseMission.MissionEventPayload payload = new BaseMission.MissionEventPayload
			{
				NetworkIdentifier = (NetworkableId)(((Object)(object)baseEntity == (Object)null) ? baseEntity.net.ID : default(NetworkableId)),
				UintIdentifier = (((Object)(object)baseEntity == (Object)null) ? baseEntity.prefabID : 0u),
				WorldPosition = ((Component)this).transform.position
			};
			player.ProcessMissionEvent(BaseMission.MissionEventType.METAL_DETECTOR_FIND, payload, 1f);
		}
	}

	public override void OnSingleDig(BasePlayer player)
	{
		base.OnSingleDig(player);
	}

	public override void OnFirstDig(BasePlayer player)
	{
		base.OnFirstDig(player);
	}
}
