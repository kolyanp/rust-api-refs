using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Landmine : BaseTrap
{
	public GameObjectRef explosionEffect;

	public GameObjectRef triggeredEffect;

	public float minExplosionRadius;

	public float explosionRadius;

	public int vibrationLevel = 1;

	public bool blocked;

	private ulong triggerPlayerID;

	public List<DamageTypeEntry> damageTypes = new List<DamageTypeEntry>();

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Landmine.OnRpcMessage"))
		{
			if (rpc == 1552281787 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Disarm"));
				}
				using (TimeWarning.New("RPC_Disarm"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1552281787u, "RPC_Disarm", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_Disarm(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_Disarm");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool Triggered()
	{
		return HasFlag(Flags.Open);
	}

	public bool Armed()
	{
		return HasFlag(Flags.On);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			info.msg.landmine = Pool.Get<Landmine>();
			info.msg.landmine.triggeredID = triggerPlayerID;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (!info.fromDisk && info.msg.landmine != null)
		{
			triggerPlayerID = info.msg.landmine.triggeredID;
		}
	}

	public override void ServerInit()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
		}
		Invoke(Arm, 1.5f);
		base.ServerInit();
	}

	public override void ObjectEntered(GameObject obj)
	{
		if (!base.isClient)
		{
			if (!Armed())
			{
				CancelInvoke(Arm);
				blocked = true;
			}
			else if (Interface.CallHook("OnTrapTrigger", this, obj) == null)
			{
				BasePlayer ply = GameObjectEx.ToBaseEntity(obj) as BasePlayer;
				Trigger(ply);
			}
		}
	}

	public void Trigger(BasePlayer ply = null)
	{
		if ((Object)(object)ply != (Object)null)
		{
			triggerPlayerID = ply.userID;
		}
		SetFlagLocal(Flags.Open, b: true);
		SendNetworkUpdate();
	}

	public override void OnEmpty()
	{
		if (blocked)
		{
			Arm();
			blocked = false;
		}
		else if (Triggered())
		{
			Invoke(TryExplode, 0.05f);
		}
	}

	public virtual void Explode()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		base.health = float.PositiveInfinity;
		Effect.server.Run(explosionEffect.resourcePath, PivotPoint(), ((Component)this).transform.up, null, broadcast: true);
		DamageUtil.RadiusDamage(this, LookupPrefab(), CenterPoint(), minExplosionRadius, explosionRadius, damageTypes, 2263296, useLineOfSight: true);
		SeismicSensor.Notify(CenterPoint(), vibrationLevel);
		if (!base.IsDestroyed)
		{
			Kill();
		}
	}

	public override void OnDied(HitInfo info)
	{
		Invoke(Explode, Random.Range(0.1f, 0.3f));
	}

	private void OnGroundMissing()
	{
		Explode();
	}

	private void TryExplode()
	{
		if (Armed())
		{
			Explode();
		}
	}

	public override void Arm()
	{
		SetFlagLocal(Flags.On, b: true);
		SendNetworkUpdate();
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void RPC_Disarm(RPCMessage rpc)
	{
		if ((ulong)rpc.player.userID == triggerPlayerID || !Armed() || Interface.CallHook("OnTrapDisarm", this, rpc.player) != null || !Triggered())
		{
			return;
		}
		if (Random.Range(0, 100) < 15)
		{
			Invoke(TryExplode, 0.05f);
			return;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
		}
		rpc.player.GiveItem(ItemManager.CreateByName("trap.landmine", 1, 0uL), GiveItemReason.PickedUp);
		Kill();
	}
}
