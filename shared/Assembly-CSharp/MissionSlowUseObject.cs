using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class MissionSlowUseObject : BaseEntity
{
	public float InteractTime = 5f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MissionSlowUseObject.OnRpcMessage"))
		{
			if (rpc == 2005407348 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerUse"));
				}
				using (TimeWarning.New("ServerUse"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2005407348u, "ServerUse", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ServerUse(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ServerUse");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool CanPlayerUse(BasePlayer player)
	{
		if (player.TryGetActiveMissionInstance(out var instance))
		{
			BaseMission.MissionObjectiveEntry[] objectives = instance.GetMission().objectives;
			for (int i = 0; i < objectives.Length; i++)
			{
				MissionObjective objective = objectives[i].objective;
				if (objective is MissionObjective_ActivateLongUseObject missionObjective_ActivateLongUseObject && !objective.IsCompleted(i, instance) && objective.CanProgress(i, instance) && missionObjective_ActivateLongUseObject.RequiredEntity.prefabID == prefabID)
				{
					return true;
				}
			}
		}
		return false;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void ServerUse(RPCMessage msg)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (CanPlayerUse(player))
		{
			player.ProcessMissionEvent(BaseMission.MissionEventType.LONG_USE_OBJECT, net.ID, 1f);
		}
	}
}
