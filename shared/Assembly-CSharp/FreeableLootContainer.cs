using System;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class FreeableLootContainer : LootContainer
{
	private const Flags tiedDown = Flags.Reserved8;

	public Buoyancy buoyancy;

	public GameObjectRef freedEffect;

	private Rigidbody rb;

	private const float untieTime = 6f;

	public uint skinOverride;

	private float startUntieTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("FreeableLootContainer.OnRpcMessage"))
		{
			if (rpc == 2202685945u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_FreeCrate"));
				}
				using (TimeWarning.New("RPC_FreeCrate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2202685945u, "RPC_FreeCrate", this, player, 3f))
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
							RPC_FreeCrate(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_FreeCrate");
					}
				}
				return true;
			}
			if (rpc == 1460413277 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_FreeCrateTimer"));
				}
				using (TimeWarning.New("RPC_FreeCrateTimer"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1460413277u, "RPC_FreeCrateTimer", this, player, 3f))
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
							RPC_FreeCrateTimer(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_FreeCrateTimer");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public Rigidbody GetRB()
	{
		if ((Object)(object)rb == (Object)null)
		{
			rb = ((Component)this).GetComponent<Rigidbody>();
		}
		return rb;
	}

	public bool IsTiedDown()
	{
		return HasFlag(Flags.Reserved8);
	}

	public override void ServerInit()
	{
		GetRB().isKinematic = true;
		buoyancy.buoyancyScale = 0f;
		((Behaviour)buoyancy).enabled = false;
		base.ServerInit();
		if (skinOverride != 0)
		{
			skinID = skinOverride;
			SendNetworkUpdate();
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		if (base.isServer && (Object)(object)info.Weapon != (Object)null)
		{
			BaseMelee component = ((Component)info.Weapon).GetComponent<BaseMelee>();
			if (Object.op_Implicit((Object)(object)component) && component.canUntieCrates && IsTiedDown())
			{
				base.health--;
				info.DidGather = true;
				if (base.health <= 0f)
				{
					base.health = MaxHealth();
					Release(info.InitiatorPlayer);
				}
			}
		}
		base.OnAttacked(info);
	}

	public void Release(BasePlayer ply)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnFreeableContainerRelease", this, ply) == null)
		{
			GetRB().isKinematic = false;
			((Behaviour)buoyancy).enabled = true;
			buoyancy.buoyancyScale = 1f;
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved8, b: false);
			}
			ToggleNetworkPositionTick(isEnabled: true);
			if (freedEffect.isValid)
			{
				Effect.server.Run(freedEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
			}
			if ((Object)(object)ply != (Object)null && !ply.IsNpc && ply.IsConnected && net != null)
			{
				ply.ProcessMissionEvent(BaseMission.MissionEventType.FREE_CRATE, net.ID, 1f);
				Facepunch.Rust.Analytics.Azure.OnFreeUnderwaterCrate(ply, this);
			}
			Interface.CallHook("OnFreeableContainerReleased", this, ply);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_FreeCrate(RPCMessage msg)
	{
		if (IsTiedDown() && !(Mathf.Abs(startUntieTime + 6f - Time.realtimeSinceStartup) > ConVar.AntiHack.rpc_timer_forgiveness))
		{
			BasePlayer player = msg.player;
			Release(player);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_FreeCrateTimer(RPCMessage msg)
	{
		if (IsTiedDown())
		{
			startUntieTime = Time.realtimeSinceStartup;
			Interface.CallHook("OnFreeableContainerReleaseStarted", this, msg.player);
		}
	}
}
