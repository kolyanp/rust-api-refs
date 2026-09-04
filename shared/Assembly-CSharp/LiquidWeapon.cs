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

public class LiquidWeapon : BaseLiquidVessel
{
	[Header("Liquid Weapon")]
	public float FireRate = 0.2f;

	public float MaxRange = 10f;

	public int FireAmountML = 100;

	public int MaxPressure = 100;

	public int PressureLossPerTick = 5;

	public int PressureGainedPerPump = 25;

	public float MinDmgRadius = 0.15f;

	public float MaxDmgRadius = 0.15f;

	public float SplashRadius = 2f;

	public GameObjectRef ImpactSplashEffect;

	public AnimationCurve PowerCurve;

	public List<DamageTypeEntry> Damage;

	public LiquidWeaponEffects EntityWeaponEffects;

	public bool RequiresPumping;

	public bool AutoPump;

	public bool WaitForFillAnim;

	public bool UseFalloffCurve;

	public AnimationCurve FalloffCurve;

	public float PumpingBlockDuration = 0.5f;

	public float StartFillingBlockDuration = 2f;

	public float StopFillingBlockDuration = 1f;

	public float cooldownTime;

	public bool HoldFireInput;

	public int pressure;

	public const string RadiationFightAchievement = "SUMMER_RADICAL";

	public const string SoakedAchievement = "SUMMER_SOAKED";

	public const string LiquidatorAchievement = "SUMMER_LIQUIDATOR";

	public const string NoPressureAchievement = "SUMMER_NO_PRESSURE";

	public float PressureFraction => (float)pressure / (float)MaxPressure;

	public float MinimumPressureFraction => (float)PressureGainedPerPump / (float)MaxPressure;

	public float CurrentRange
	{
		get
		{
			if (!UseFalloffCurve)
			{
				return MaxRange;
			}
			return MaxRange * FalloffCurve.Evaluate((float)(MaxPressure - pressure) / (float)MaxPressure);
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("LiquidWeapon.OnRpcMessage"))
		{
			if (rpc == 1600824953 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - PumpWater"));
				}
				using (TimeWarning.New("PumpWater"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(1600824953u, "PumpWater", this, player))
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
							PumpWater(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in PumpWater");
					}
				}
				return true;
			}
			if (rpc == 3724096303u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - StartFiring"));
				}
				using (TimeWarning.New("StartFiring"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(3724096303u, "StartFiring", this, player))
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
							StartFiring(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in StartFiring");
					}
				}
				return true;
			}
			if (rpc == 789289044 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - StopFiring"));
				}
				using (TimeWarning.New("StopFiring"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(789289044u, "StopFiring", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							StopFiring();
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in StopFiring");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void StartFiring(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (OnCooldown())
		{
			return;
		}
		if (!RequiresPumping)
		{
			pressure = MaxPressure;
		}
		if (CanFire(player))
		{
			CancelInvoke(FireTick);
			InvokeRepeating(FireTick, 0f, FireRate);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: true);
			}
			StartCooldown(FireRate);
			if (base.isServer)
			{
				SendNetworkUpdateImmediate();
			}
			Interface.CallHook("OnLiquidWeaponFired", this, player);
		}
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void StopFiring()
	{
		CancelInvoke(FireTick);
		if (!RequiresPumping)
		{
			pressure = MaxPressure;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
		}
		if (base.isServer)
		{
			SendNetworkUpdateImmediate();
		}
		Interface.CallHook("OnLiquidWeaponFiringStopped", this);
	}

	private bool CanFire(BasePlayer player)
	{
		object obj = Interface.CallHook("CanFireLiquidWeapon", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (RequiresPumping && pressure < PressureLossPerTick)
		{
			return false;
		}
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (HasFlag(Flags.Open))
		{
			return false;
		}
		if (AmountHeld() <= 0)
		{
			return false;
		}
		if (!player.CanInteract())
		{
			return false;
		}
		if (!player.CanAttack() || player.IsRunning())
		{
			return false;
		}
		Item item = GetItem();
		if (item == null || item.contents == null)
		{
			return false;
		}
		return true;
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	public void PumpWater(RPCMessage msg)
	{
		PumpWater();
	}

	private void PumpWater()
	{
		if (!((Object)(object)GetOwnerPlayer() == (Object)null) && !OnCooldown() && !Firing())
		{
			pressure += PressureGainedPerPump;
			pressure = Mathf.Min(pressure, MaxPressure);
			StartCooldown(PumpingBlockDuration);
			GetOwnerPlayer().SignalBroadcast(Signal.Reload);
			SendNetworkUpdateImmediate();
		}
	}

	private void FireTick()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!CanFire(ownerPlayer))
		{
			StopFiring();
			return;
		}
		int num = Mathf.Min(FireAmountML, AmountHeld());
		if (num == 0)
		{
			StopFiring();
			return;
		}
		float currentRange = CurrentRange;
		pressure -= PressureLossPerTick;
		if (pressure <= 0)
		{
			StopFiring();
		}
		Ray val = ownerPlayer.eyes.BodyRay();
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(val, ref val2, currentRange, 1218652417))
		{
			DoSplash(ownerPlayer, ((RaycastHit)(ref val2)).point, ((Ray)(ref val)).direction, num);
		}
		LoseWater(num);
		SendNetworkUpdate();
	}

	private void DoSplash(BasePlayer attacker, Vector3 position, Vector3 direction, int amount)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Item contents = GetContents();
		if (contents != null && contents.amount > 0 && !((Object)(object)contents.info == (Object)null))
		{
			WaterBall.DoSplash(position, SplashRadius, contents.info, amount, funWater: true);
			DamageUtil.RadiusDamage(attacker, LookupPrefab(), position, MinDmgRadius, MaxDmgRadius, Damage, 131072, useLineOfSight: true);
		}
	}

	public override void OnHeldChanged()
	{
		base.OnHeldChanged();
		StopFiring();
	}

	private void StartCooldown(float duration)
	{
		if (Time.realtimeSinceStartup + duration > cooldownTime)
		{
			cooldownTime = Time.realtimeSinceStartup + duration;
		}
	}

	private bool OnCooldown()
	{
		return Time.realtimeSinceStartup < cooldownTime;
	}

	private bool Firing()
	{
		return HasFlag(Flags.On);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.baseProjectile = Pool.Get<BaseProjectile>();
		info.msg.baseProjectile.primaryMagazine = Pool.Get<Magazine>();
		info.msg.baseProjectile.primaryMagazine.contents = pressure;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.baseProjectile != null && info.msg.baseProjectile.primaryMagazine != null)
		{
			pressure = info.msg.baseProjectile.primaryMagazine.contents;
		}
	}
}
