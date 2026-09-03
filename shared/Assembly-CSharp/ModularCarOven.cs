using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class ModularCarOven : BaseOven
{
	private BaseVehicleModule moduleParent;

	private BaseVehicleModule ModuleParent
	{
		get
		{
			if ((Object)(object)moduleParent != (Object)null)
			{
				return moduleParent;
			}
			moduleParent = GetParentEntity() as BaseVehicleModule;
			return moduleParent;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ModularCarOven.OnRpcMessage"))
		{
			if (rpc == 4167839872u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SVSwitch"));
				}
				using (TimeWarning.New("SVSwitch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4167839872u, "SVSwitch", this, player, 3f))
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
							SVSwitch(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SVSwitch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	protected override void SVSwitch(RPCMessage msg)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ModuleParent == (Object)null) && ModuleParent.CanBeLooted(msg.player) && !WaterLevel.Test(((Component)this).transform.position, waves: true, volumes: false))
		{
			base.SVSwitch(msg);
		}
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		if ((Object)(object)ModuleParent == (Object)null || !ModuleParent.CanBeLooted(player))
		{
			return false;
		}
		return base.PlayerOpenLoot(player, panelToOpen, doPositionChecks);
	}

	protected override void OnCooked()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		base.OnCooked();
		if (WaterLevel.Test(((Component)this).transform.position, waves: true, volumes: false))
		{
			StopCooking();
		}
	}
}
