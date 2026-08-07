using System;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseLauncher : BaseProjectile
{
	public float initialSpeedMultiplier = 1f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseLauncher.OnRpcMessage"))
		{
			if (rpc == 853319324 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_Launch"));
				}
				using (TimeWarning.New("SV_Launch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(853319324u, "SV_Launch", this, player))
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
							SV_Launch(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SV_Launch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool ForceSendMagazine(SaveInfo saveInfo)
	{
		return true;
	}

	public override void ServerUse(HeldEntityServerUseParams parameters)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		ItemModProjectile component = ((Component)primaryMagazine.ammoType).GetComponent<ItemModProjectile>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		if (primaryMagazine.contents <= 0)
		{
			SignalBroadcast(Signal.DryFire);
			StartAttackCooldown(1f);
			return;
		}
		if (!Object.op_Implicit((Object)(object)component.projectileObject.Get().GetComponent<ServerProjectile>()))
		{
			base.ServerUse(parameters);
			return;
		}
		ModifyAmmoCount(-1);
		if (primaryMagazine.contents < 0)
		{
			SetAmmoCount(0);
		}
		Vector3 val = ((Component)MuzzlePoint).transform.forward;
		Vector3 position = ((Component)MuzzlePoint).transform.position;
		float num = GetAimCone() + component.projectileSpread;
		if (num > 0f)
		{
			val = AimConeUtil.GetModifiedAimConeDirection(num, val);
		}
		float num2 = 1f;
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(position, val, ref val2, num2, 1237003025))
		{
			num2 = ((RaycastHit)(ref val2)).distance - 0.1f;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(component.GetOverrideProjectile(this).resourcePath, position + val * num2);
		if (!((Object)(object)baseEntity == (Object)null))
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			bool flag = (Object)(object)ownerPlayer != (Object)null && ownerPlayer.IsNpc;
			ServerProjectile component2 = ((Component)baseEntity).GetComponent<ServerProjectile>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.InitializeVelocity(val * component2.speed * initialSpeedMultiplier);
			}
			((Component)baseEntity).SendMessage("SetDamageScale", (object)(flag ? npcDamageScale : turretDamageScale));
			baseEntity.Spawn();
			StartAttackCooldown(ScaleRepeatDelay(repeatDelay));
			SignalBroadcast(Signal.Attack, string.Empty);
			GetOwnerItem()?.LoseCondition(Random.Range(1f, 2f));
		}
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void SV_Launch(RPCMessage msg)
	{
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
			return;
		}
		if (reloadFinished && HasReloadCooldown())
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Reloading (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "reload_cooldown");
			return;
		}
		reloadStarted = false;
		reloadFinished = false;
		if (!base.UsingInfiniteAmmoCheat)
		{
			if (primaryMagazine.contents <= 0)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Launch magazine empty (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "magazine_empty_launch");
				return;
			}
			ModifyAmmoCount(-1);
		}
		SignalBroadcast(Signal.Attack, string.Empty, player.net.connection);
		Vector3 val = msg.read.Vector3();
		Vector3 val2 = msg.read.Vector3();
		Vector3 val3 = ((Vector3)(ref val2)).normalized;
		bool num = msg.read.Bit();
		BaseEntity mounted = player.GetParentEntity();
		if ((Object)(object)mounted == (Object)null)
		{
			mounted = player.GetMounted();
		}
		if (num)
		{
			if ((Object)(object)mounted != (Object)null)
			{
				val = ((Component)mounted).transform.TransformPoint(val);
				val3 = ((Component)mounted).transform.TransformDirection(val3);
			}
			else
			{
				val = player.eyes.position;
				val3 = player.eyes.BodyForward();
			}
		}
		if (!ValidateEyePos(player, val))
		{
			return;
		}
		ItemModProjectile component = ((Component)primaryMagazine.ammoType).GetComponent<ItemModProjectile>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Item mod not found (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "mod_missing");
			return;
		}
		float num2 = GetAimCone() + component.projectileSpread;
		if (num2 > 0f)
		{
			val3 = AimConeUtil.GetModifiedAimConeDirection(num2, val3);
		}
		float num3 = 1f;
		RaycastHit val4 = default(RaycastHit);
		if (Physics.Raycast(val, val3, ref val4, num3, 1237003025))
		{
			num3 = ((RaycastHit)(ref val4)).distance - 0.1f;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(component.GetOverrideProjectile(this).resourcePath, val + val3 * num3);
		if ((Object)(object)baseEntity == (Object)null)
		{
			return;
		}
		baseEntity.creatorEntity = player;
		ServerProjectile component2 = ((Component)baseEntity).GetComponent<ServerProjectile>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			component2.InitializeVelocity(GetInheritedVelocity(player, val3) + val3 * component2.speed * initialSpeedMultiplier);
		}
		baseEntity.Spawn();
		ProjectileLaunched_Server(component2);
		Facepunch.Rust.Analytics.Azure.OnExplosiveLaunched(player, baseEntity, this);
		Interface.CallHook("OnRocketLaunched", player, baseEntity);
		StartAttackCooldown(ScaleRepeatDelay(repeatDelay));
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null)
		{
			if (!base.UsingInfiniteAmmoCheat)
			{
				ownerItem.LoseCondition(Random.Range(1f, 2f));
			}
			BaseMountable mounted2 = player.GetMounted();
			if ((Object)(object)mounted2 != (Object)null)
			{
				mounted2.OnWeaponFired(this);
			}
		}
	}

	public virtual void ProjectileLaunched_Server(ServerProjectile justLaunched)
	{
	}
}
