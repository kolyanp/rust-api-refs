using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class BearTrap : BaseTrap
{
	protected Animator animator;

	private GameObject hurtTarget;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BearTrap.OnRpcMessage"))
		{
			if (rpc == 547827602 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Arm"));
				}
				using (TimeWarning.New("RPC_Arm"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(547827602u, "RPC_Arm", this, player, 3f))
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
							RPC_Arm(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_Arm");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool Armed()
	{
		return HasFlag(Flags.On);
	}

	public override void InitShared()
	{
		animator = ((Component)this).GetComponent<Animator>();
		base.InitShared();
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (base.ShouldDisplayPickupOption(player))
		{
			return player.CanBuild();
		}
		return false;
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (Armed())
		{
			pickupErrorToFormat = (format: PickupErrors.ItemIsArmed, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Arm();
	}

	public override void Arm()
	{
		base.Arm();
		RadialResetCorpses(120f);
	}

	public void Fire()
	{
		SetFlagLocal(Flags.On, b: false);
		SendNetworkUpdate();
	}

	public override void ObjectEntered(GameObject obj)
	{
		if (Armed() && Interface.CallHook("OnTrapTrigger", this, obj) == null)
		{
			hurtTarget = obj;
			Invoke(DelayedFire, 0.05f);
		}
	}

	public void DelayedFire()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)hurtTarget))
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(hurtTarget);
			if ((Object)(object)baseEntity != (Object)null)
			{
				HitInfo hitInfo = new HitInfo(this, baseEntity, DamageType.Bite, 50f, ((Component)this).transform.position);
				hitInfo.damageTypes.Add(DamageType.Stab, 30f);
				baseEntity.OnAttacked(hitInfo);
			}
			hurtTarget = null;
		}
		RadialResetCorpses(1800f);
		Fire();
		Hurt(25f);
	}

	public void RadialResetCorpses(float duration)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		List<BaseCorpse> list = Pool.Get<List<BaseCorpse>>();
		Vis.Entities(((Component)this).transform.position, 5f, list, 512, (QueryTriggerInteraction)2);
		foreach (BaseCorpse item in list)
		{
			item.ResetRemovalTime(duration);
		}
		Pool.FreeUnmanaged<BaseCorpse>(ref list);
	}

	public override void OnAttacked(HitInfo info)
	{
		float num = info.damageTypes.Total();
		if ((info.damageTypes.IsMeleeType() && num > 20f) || num > 30f)
		{
			Fire();
		}
		base.OnAttacked(info);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void RPC_Arm(RPCMessage rpc)
	{
		if (!Armed() && Interface.CallHook("OnTrapArm", this, rpc.player) == null)
		{
			Arm();
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (!base.isServer && animator.isInitialized)
		{
			animator.SetBool("armed", Armed());
		}
	}
}
