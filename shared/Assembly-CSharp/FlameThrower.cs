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
using UnityEngine.Serialization;

public class FlameThrower : AttackEntity
{
	[Header("Flame Thrower")]
	public int maxAmmo = 100;

	public int ammo = 100;

	public ItemDefinition fuelType;

	public float timeSinceLastAttack;

	[FormerlySerializedAs("nextAttackTime")]
	public float nextReadyTime;

	public float flameRange = 10f;

	public float flameRadius = 2.5f;

	public ParticleSystem[] flameEffects;

	public FlameJet jet;

	public GameObjectRef fireballPrefab;

	public List<DamageTypeEntry> damagePerSec;

	public float playerDamageMultiplier = 4f;

	public SoundDefinition flameStart3P;

	public SoundDefinition flameLoop3P;

	public SoundDefinition flameStop3P;

	public SoundDefinition pilotLoopSoundDef;

	private float tickRate = 0.15f;

	private float lastFlameTick;

	public float fuelPerSec;

	private float ammoRemainder;

	public float reloadDuration = 3.5f;

	private float lastReloadTime = -10f;

	private float nextFlameTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("FlameThrower.OnRpcMessage"))
		{
			if (rpc == 3381353917u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DoReload"));
				}
				using (TimeWarning.New("DoReload"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(3381353917u, "DoReload", this, player))
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
							DoReload(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in DoReload");
					}
				}
				return true;
			}
			if (rpc == 3749570935u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SetFiring"));
				}
				using (TimeWarning.New("SetFiring"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(3749570935u, "SetFiring", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage firing = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SetFiring(firing);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SetFiring");
					}
				}
				return true;
			}
			if (rpc == 1057268396 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - TogglePilotLight"));
				}
				using (TimeWarning.New("TogglePilotLight"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(1057268396u, "TogglePilotLight", this, player))
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
							TogglePilotLight(msg3);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in TogglePilotLight");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private bool IsWeaponBusy()
	{
		return Time.realtimeSinceStartup < nextReadyTime;
	}

	private void SetBusyFor(float dur)
	{
		nextReadyTime = Time.realtimeSinceStartup + dur;
	}

	private void ClearBusy()
	{
		nextReadyTime = Time.realtimeSinceStartup - 1f;
	}

	public void ReduceAmmo(float firingTime)
	{
		if (base.UsingInfiniteAmmoCheat)
		{
			return;
		}
		ammoRemainder += fuelPerSec * firingTime;
		if (ammoRemainder >= 1f)
		{
			int num = Mathf.FloorToInt(ammoRemainder);
			ammoRemainder -= num;
			if (ammoRemainder >= 1f)
			{
				num++;
				ammoRemainder--;
			}
			ammo -= num;
			if (ammo <= 0)
			{
				ammo = 0;
			}
			OnFuelAmountChanged();
		}
	}

	private void OnFuelAmountChanged()
	{
		GetItem()?.MarkDirty();
	}

	public void PilotLightToggle_Shared()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdateImmediate);
		flagsUpdateScope.Set(Flags.On, HasFlag(Flags.On));
	}

	public bool IsPilotOn()
	{
		return HasFlag(Flags.On);
	}

	public bool IsFlameOn()
	{
		return HasFlag(Flags.OnFire);
	}

	public bool HasAmmo()
	{
		return GetAmmo() != null;
	}

	public Item GetAmmo()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!Object.op_Implicit((Object)(object)ownerPlayer))
		{
			return null;
		}
		object obj = Interface.CallHook("OnInventoryAmmoItemFind", ownerPlayer.inventory, fuelType);
		if (obj is Item)
		{
			return (Item)obj;
		}
		return ownerPlayer.inventory.FindItemByItemName(fuelType.shortname);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.baseProjectile != null && info.msg.baseProjectile.primaryMagazine != null)
		{
			ammo = info.msg.baseProjectile.primaryMagazine.contents;
		}
	}

	public override void CollectedForCrafting(Item item, BasePlayer crafter)
	{
		ServerCommand(item, "unload_ammo", crafter);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.baseProjectile = Pool.Get<BaseProjectile>();
		info.msg.baseProjectile.primaryMagazine = Pool.Get<Magazine>();
		info.msg.baseProjectile.primaryMagazine.contents = ammo;
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	public void SetFiring(RPCMessage msg)
	{
		bool flameState = msg.read.Bit();
		SetFlameState(flameState);
	}

	public override void ServerUse(HeldEntityServerUseParams parameters)
	{
		if (!IsOnFire() && !ServerIsReloading())
		{
			if (!IsFlameOn())
			{
				SetFlameState(wantsOn: true);
			}
			Invoke(StopFlameState, 1.25f);
			base.ServerUse(parameters);
		}
	}

	public override void TopUpAmmo()
	{
		ammo = maxAmmo;
	}

	public override float AmmoFraction()
	{
		return (float)ammo / (float)maxAmmo;
	}

	public override bool ServerIsReloading()
	{
		return Time.time < lastReloadTime + reloadDuration;
	}

	public override bool CanReload()
	{
		return ammo < maxAmmo;
	}

	public override void ServerReload()
	{
		if (!ServerIsReloading())
		{
			SetFlameState(wantsOn: false);
			lastReloadTime = Time.time;
			StartAttackCooldown(reloadDuration);
			GetOwnerPlayer().SignalBroadcast(Signal.Reload);
			ammo = maxAmmo;
		}
	}

	public void StopFlameState()
	{
		SetFlameState(wantsOn: false);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	public void DoReload(RPCMessage msg)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!((Object)(object)ownerPlayer == (Object)null))
		{
			Item item = null;
			while (ammo < maxAmmo && (item = GetAmmo()) != null && item.amount > 0)
			{
				int num = Mathf.Min(maxAmmo - ammo, item.amount);
				ammo += num;
				item.UseItem(num);
			}
			SendNetworkUpdateImmediate();
			ItemManager.DoRemoves();
			OnFuelAmountChanged();
			ownerPlayer.inventory.ServerUpdate(0f);
		}
	}

	public void SetFlameState(bool wantsOn)
	{
		if (wantsOn)
		{
			if (!base.UsingInfiniteAmmoCheat)
			{
				ammo--;
			}
			if (ammo < 0)
			{
				ammo = 0;
			}
		}
		if (wantsOn && ammo <= 0)
		{
			wantsOn = false;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.OnFire, wantsOn);
		}
		if (IsFlameOn())
		{
			float num = 1f;
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if ((Object)(object)ownerPlayer != (Object)null && ownerPlayer.IsNpc)
			{
				num = 0.4f;
			}
			nextFlameTime = Time.realtimeSinceStartup + num;
			lastFlameTick = Time.realtimeSinceStartup;
			InvokeRepeating(FlameTick, tickRate, tickRate);
		}
		else
		{
			CancelInvoke(FlameTick);
		}
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	public void TogglePilotLight(RPCMessage msg)
	{
		PilotLightToggle_Shared();
	}

	public override void OnHeldChanged()
	{
		SetFlameState(wantsOn: false);
		base.OnHeldChanged();
	}

	public void FlameTick()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.realtimeSinceStartup - lastFlameTick;
		lastFlameTick = Time.realtimeSinceStartup;
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!Object.op_Implicit((Object)(object)ownerPlayer))
		{
			return;
		}
		SingletonComponent<NpcFireManager>.Instance.Move(this);
		ReduceAmmo(num);
		SendNetworkUpdate();
		Ray val = ownerPlayer.eyes.BodyRay();
		Vector3 origin = ((Ray)(ref val)).origin;
		RaycastHit val2 = default(RaycastHit);
		bool num2 = Physics.SphereCast(val, 0.3f, ref val2, flameRange, 1218652417);
		if (!num2)
		{
			((RaycastHit)(ref val2)).point = origin + ((Ray)(ref val)).direction * flameRange;
		}
		float num3 = (ownerPlayer.IsNpc ? npcDamageScale : 1f);
		float amount = damagePerSec[0].amount;
		damagePerSec[0].amount = amount * num * num3;
		int num4 = 2146305;
		int layers = 133376;
		if (!ownerPlayer.IsNpc)
		{
			num4 |= 0x800;
		}
		DamageUtil.RadiusDamage(ownerPlayer, LookupPrefab(), ((RaycastHit)(ref val2)).point - ((Ray)(ref val)).direction * 0.1f, flameRadius * 0.5f, flameRadius, damagePerSec, num4, useLineOfSight: true, ignoreAI: false, ignoreAttackingPlayer: true, extendedLineOfSight: true);
		damagePerSec[0].amount = damagePerSec[0].amount * playerDamageMultiplier;
		DamageUtil.RadiusDamage(ownerPlayer, LookupPrefab(), ((RaycastHit)(ref val2)).point - ((Ray)(ref val)).direction * 0.1f, flameRadius * 0.5f, flameRadius, damagePerSec, layers, useLineOfSight: true, ignoreAI: false, ignoreAttackingPlayer: true, extendedLineOfSight: true);
		damagePerSec[0].amount = amount;
		if (num2 && Time.realtimeSinceStartup >= nextFlameTime && ((RaycastHit)(ref val2)).distance > 1.1f)
		{
			nextFlameTime = Time.realtimeSinceStartup + (ownerPlayer.IsNpc ? 0.25f : 0.45f);
			Vector3 val3 = ((RaycastHit)(ref val2)).point - ((Ray)(ref val)).direction * 0.25f;
			Vector3 val4 = val3 + new Vector3(0f, 0.2f, 0f);
			bool flag = !GamePhysics.CheckSphere(val3, 0.1f, 1084293377, (QueryTriggerInteraction)0);
			if (!flag && GamePhysics.LineOfSight(val3, val4, 1084293377))
			{
				val3 = val4;
				flag = !GamePhysics.CheckSphere(val3, 0.1f, 1084293377, (QueryTriggerInteraction)0);
			}
			if (flag)
			{
				BaseEntity baseEntity = GameManager.server.CreateEntity(fireballPrefab.resourcePath, val3);
				if (Object.op_Implicit((Object)(object)baseEntity))
				{
					baseEntity.creatorEntity = ownerPlayer;
					Interface.CallHook("OnFlameThrowerBurn", this, baseEntity);
					FireBall fireBall = baseEntity as FireBall;
					if ((Object)(object)fireBall != (Object)null && ownerPlayer.IsNpc)
					{
						fireBall.ignoreNPC = true;
					}
					baseEntity.Spawn();
				}
			}
		}
		if (ammo == 0)
		{
			SetFlameState(wantsOn: false);
		}
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null && !base.UsingInfiniteAmmoCheat && !ownerPlayer.IsNpc)
		{
			ownerItem.LoseCondition(num);
		}
	}

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (item == null || !(command == "unload_ammo"))
		{
			return;
		}
		int num = ammo;
		if (num > 0)
		{
			ammo = 0;
			item.MarkDirty();
			SendNetworkUpdateImmediate();
			Item item2 = ItemManager.Create(fuelType, num, 0uL, isServerSide: true, 0uL);
			if (!item2.MoveToContainer(player.inventory.containerMain))
			{
				item2.Drop(player.eyes.position, player.eyes.BodyForward() * 2f);
			}
		}
	}
}
