using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class ElectricalBranch : IOEntity
{
	public int branchAmount = 2;

	public GameObjectRef branchPanelPrefab;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ElectricalBranch.OnRpcMessage"))
		{
			if (rpc == 4207410429u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_SetBranchOffPower"));
				}
				using (TimeWarning.New("RPC_SetBranchOffPower"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4207410429u, "RPC_SetBranchOffPower", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(4207410429u, "RPC_SetBranchOffPower", this, player, 3f))
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
							RPC_SetBranchOffPower(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_SetBranchOffPower");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void RPC_SetBranchOffPower(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && player.CanBuild())
		{
			int branchOffPower = msg.read.Int32();
			SetBranchOffPower(branchOffPower);
		}
	}

	public void SetBranchOffPower(int power)
	{
		power = Mathf.Clamp(power, 1, 10000000);
		branchAmount = power;
		MarkDirtyForceUpdateOutputs();
		SendNetworkUpdate();
	}

	public override bool AllowDrainFrom(int outputSlot)
	{
		return true;
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		return outputSlot switch
		{
			0 => Mathf.Clamp(GetCurrentEnergy() - branchAmount, 0, GetCurrentEnergy()), 
			1 => Mathf.Min(GetCurrentEnergy(), branchAmount), 
			_ => 0, 
		};
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.ioEntity.genericInt1 = branchAmount;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			branchAmount = info.msg.ioEntity.genericInt1;
		}
	}
}
