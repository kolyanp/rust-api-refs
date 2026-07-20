using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class CollectableEasterEgg : BaseEntity, INotifyLOD
{
	public Transform artwork;

	public float bounceRange = 0.2f;

	public float bounceSpeed = 1f;

	public GameObjectRef pickupEffect;

	public ItemDefinition itemToGive;

	public GameObject[] vfx;

	[NonSerialized]
	public ulong ownerUserID;

	private float lastPickupStartTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CollectableEasterEgg.OnRpcMessage"))
		{
			if (rpc == 2436818324u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_PickUp"));
				}
				using (TimeWarning.New("RPC_PickUp"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2436818324u, "RPC_PickUp", this, player, 3f))
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
							RPC_PickUp(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_PickUp");
					}
				}
				return true;
			}
			if (rpc == 2243088389u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_StartPickUp"));
				}
				using (TimeWarning.New("RPC_StartPickUp"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2243088389u, "RPC_StartPickUp", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_StartPickUp(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_StartPickUp");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		int num = Random.Range(0, 3);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local))
		{
			flagsUpdateScope.Set(Flags.Reserved1, num == 0);
			flagsUpdateScope.Set(Flags.Reserved2, num == 1);
			flagsUpdateScope.Set(Flags.Reserved3, num == 2);
		}
		base.ServerInit();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_StartPickUp(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null))
		{
			lastPickupStartTime = Time.realtimeSinceStartup;
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_PickUp(RPCMessage msg)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)msg.player == (Object)null)
		{
			return;
		}
		float num = Time.realtimeSinceStartup - lastPickupStartTime;
		if (!Object.op_Implicit((Object)(object)(msg.player.GetHeldEntity() as EasterBasket)) && (num > 2f || num < 0.8f))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)EggHuntEvent.serverEvent))
		{
			if (!EggHuntEvent.serverEvent.IsEventActive() || Interface.CallHook("OnEventCollectablePickup", msg.player, this) != null)
			{
				return;
			}
			EggHuntEvent.serverEvent.OnEggCollected(msg.player, this);
			int iAmount = 1;
			msg.player.GiveItem(ItemManager.Create(itemToGive, iAmount, 0uL, isServerSide: true, 0uL));
		}
		Effect.server.Run(pickupEffect.resourcePath, ((Component)this).transform.position + Vector3.up * 0.3f, Vector3.up);
		Kill();
	}
}
