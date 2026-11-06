using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class EaselDeployable : DecorDeployable
{
	[Header("Painting Easel")]
	public Transform easelTopBar;

	public Transform easelBottomBar;

	public Transform cameraPaintingAnchor;

	public IEaselPaintable paintable;

	private const Flags HasPaintingFlag = Flags.Reserved3;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("EaselDeployable.OnRpcMessage"))
		{
			if (rpc == 1365820335 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_StartPainting"));
				}
				using (TimeWarning.New("Server_StartPainting"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1365820335u, "Server_StartPainting", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1365820335u, "Server_StartPainting", this, player, 6f))
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
							Server_StartPainting(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_StartPainting");
					}
				}
				return true;
			}
			if (rpc == 3444709649u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_StopPainting"));
				}
				using (TimeWarning.New("Server_StopPainting"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3444709649u, "Server_StopPainting", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3444709649u, "Server_StopPainting", this, player, 6f))
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
							Server_StopPainting(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in Server_StopPainting");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void SetPainting(IEaselPaintable newPaintable)
	{
		paintable = newPaintable;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved3, paintable != null);
		}
		_ = paintable;
	}

	public void RemovePainting(IEaselPaintable newSign)
	{
		if (newSign != paintable)
		{
			return;
		}
		paintable = null;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, b: false);
	}

	public override void OnDied(HitInfo info)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (paintable != null)
		{
			DecayEntity component = paintable.GameObject.GetComponent<DecayEntity>();
			if ((Object)(object)component != (Object)null)
			{
				Item item = ItemManager.Create(component.pickup.itemTarget, 1, 0uL, isServerSide: true, 0uL);
				paintable.SaveSignageToItem(item);
				item.CreateWorldObject(cameraPaintingAnchor.position);
				component.Die();
			}
		}
		base.OnDied(info);
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server.MaxDistance(6f)]
	public void Server_StartPainting(RPCMessage msg)
	{
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server.MaxDistance(6f)]
	public void Server_StopPainting(RPCMessage msg)
	{
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}
}
