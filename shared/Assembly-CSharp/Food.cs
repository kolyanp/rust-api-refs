using System;
using System.Collections.Generic;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class Food : BaseMelee
{
	public FoodViewModel.FoodVisualConfig VisualConfig;

	public List<GameObject> VisualRoots;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Food.OnRpcMessage"))
		{
			if (rpc == 1921839088 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Consume"));
				}
				using (TimeWarning.New("Consume"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1921839088u, "Consume", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1921839088u, "Consume", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							Consume();
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Consume");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	public void Consume()
	{
		if (base.isServer)
		{
			if (HasAttackCooldown())
			{
				return;
			}
			StartAttackCooldown(repeatDelay / 2f);
		}
		Item item = GetItem();
		if (item == null)
		{
			return;
		}
		ItemModConsume component = ((Component)item.info).GetComponent<ItemModConsume>();
		if (!((Object)(object)component == (Object)null))
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if (!((Object)(object)ownerPlayer == (Object)null))
			{
				component.DoAction(item, ownerPlayer);
			}
		}
	}
}
