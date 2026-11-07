using System;
using ConVar;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class BallistaGun : BaseMountedWeapon
{
	[SerializeField]
	[Header("Ballista")]
	private bool noHeadshots = true;

	public DamageRenderer damageRenderer;

	[NonSerialized]
	public Ballista ballistaOwner;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BallistaGun.OnRpcMessage"))
		{
			if (rpc == 296086248 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_FireClientProjectile"));
				}
				using (TimeWarning.New("SERVER_FireClientProjectile"))
				{
					using (msg.read.UseRepeatedElementLimit(1))
					{
						using (TimeWarning.New("Conditions"))
						{
							if (!RPC_Server.CallsPerSecond.Test(296086248u, "SERVER_FireClientProjectile", this, player, 1uL))
							{
								return true;
							}
							if (!RPC_Server.FromMounted.Test(296086248u, "SERVER_FireClientProjectile", this, player))
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
								SERVER_FireClientProjectile(msg2);
							}
						}
						catch (Exception ex)
						{
							Debug.LogException(ex);
							player.Kick("RPC Error in SERVER_FireClientProjectile");
						}
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private bool HasOwner()
	{
		return (Object)(object)ballistaOwner != (Object)null;
	}

	public override bool OwnerIsWaterlogged()
	{
		if (HasOwner())
		{
			return ballistaOwner.IsWaterlogged();
		}
		return false;
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
	}

	protected override bool CanRotate()
	{
		if (HasOwner())
		{
			return !ballistaOwner.IsStationary();
		}
		return base.CanRotate();
	}

	protected override bool ChecksOwnSeatClipping()
	{
		return !HasOwner();
	}

	protected override void Server_OnReloadStarted()
	{
		base.Server_OnReloadStarted();
		if (HasOwner())
		{
			ballistaOwner.RefreshLastUseTime();
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.MaxRepeatedElements(1)]
	[RPC_Server.FromMounted]
	private void SERVER_FireClientProjectile(RPCMessage msg)
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (IsFireRPCInvalid(msg, player, out var ammoItem, out var itemModProjectile))
		{
			return;
		}
		ProjectileShoot val = msg.read.Proto<ProjectileShoot>((ProjectileShoot)null);
		try
		{
			if (val.projectiles.Count != 1)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Projectile count mismatch (" + ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, null, "count_mismatch");
				return;
			}
			player.CleanupExpiredProjectiles();
			Guid projectileGroupId = Guid.NewGuid();
			foreach (Projectile projectile in val.projectiles)
			{
				if (player.HasFiredProjectile(projectile.projectileID))
				{
					AntiHack.Log(player, AntiHackType.ProjectileHack, "Duplicate ID (" + projectile.projectileID + ")");
					player.stats.combat.LogInvalid(player, null, "duplicate_id");
				}
				else if (ValidateFirePos(player, projectile.startPos))
				{
					player.NoteFiredProjectile(projectile.projectileID, projectile.startPos, projectile.startVel, null, ammoItem, projectileGroupId, Vector3.zero);
					if (player.HasFiredProjectile(projectile.projectileID))
					{
						Effect effect = new Effect();
						effect.Init(Effect.Type.Projectile, projectile.startPos, projectile.startVel, msg.connection);
						((EffectData)effect).scale = 1f;
						effect.pooledString = itemModProjectile.GetOverrideProjectile(this).resourcePath;
						((EffectData)effect).number = projectile.seed;
						EffectNetwork.Send(effect);
					}
				}
			}
			val.Dispose();
			SERVER_OnProjectileFired(msg.connection, player);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected override void SERVER_OnProjectileFired(Connection connection, BasePlayer player)
	{
		base.SERVER_OnProjectileFired(connection, player);
		if (HasOwner())
		{
			ballistaOwner.RefreshLastUseTime();
			ballistaOwner.OnFired();
		}
	}

	protected bool ValidateFirePos(BasePlayer player, Vector3 firePos)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		if (Vector3Ex.IsNaNOrInfinity(firePos))
		{
			string shortPrefabName = ShortPrefabName;
			AntiHack.Log(player, AntiHackType.EyeHack, "Contains NaN (" + shortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "eye_nan");
			flag = false;
		}
		if (ConVar.AntiHack.eye_protection > 0)
		{
			Vector3 val;
			if (ConVar.AntiHack.eye_protection >= 1)
			{
				val = player.GetParentVelocity();
				float magnitude = val.magnitude;
				val = player.GetMountVelocity();
				float num = magnitude + val.magnitude + ConVar.AntiHack.eye_forgiveness;
				float num2 = Vector3.Distance(((Component)muzzle).transform.position, firePos);
				if (num2 > num)
				{
					string shortPrefabName2 = ShortPrefabName;
					AntiHack.Log(player, AntiHackType.EyeHack, "Distance (" + shortPrefabName2 + " on attack with " + num2 + "m > " + num + "m)");
					player.stats.combat.LogInvalid(player, null, "eye_distance");
					flag = false;
				}
			}
			int num3 = 2162688;
			if (ConVar.AntiHack.eye_terraincheck)
			{
				num3 |= 0x800000;
			}
			if (ConVar.AntiHack.eye_vehiclecheck)
			{
				num3 |= 0x8000000;
			}
			if (ConVar.AntiHack.eye_protection >= 2 && !CanSeeFirePoint(player, 0.05f))
			{
				string shortPrefabName3 = ShortPrefabName;
				string[] array = new string[8] { "Line of sight (", shortPrefabName3, " on attack) ", null, null, null, null, null };
				val = player.eyes.center;
				array[3] = ((object)val/*cast due to constrained. prefix*/).ToString();
				array[4] = " ";
				val = player.eyes.position;
				array[5] = ((object)val/*cast due to constrained. prefix*/).ToString();
				array[6] = " ";
				array[7] = ((object)firePos/*cast due to constrained. prefix*/).ToString();
				AntiHack.Log(player, AntiHackType.EyeHack, string.Concat(array));
				player.stats.combat.LogInvalid(player, null, "eye_los");
				flag = false;
			}
		}
		if (!flag)
		{
			AntiHack.AddViolation(player, AntiHackType.EyeHack, ConVar.AntiHack.eye_penalty);
		}
		return flag;
	}

	public override void DoRepair(BasePlayer player)
	{
		if (HasParent() && HasOwner())
		{
			ballistaOwner.DoRepair(player);
		}
		else
		{
			base.DoRepair(player);
		}
	}

	public override void Hurt(HitInfo info)
	{
		if (HasParent() && HasOwner())
		{
			ballistaOwner.Hurt(info);
		}
		else
		{
			base.Hurt(info);
		}
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		if (HasParent() && HasOwner() && !ballistaOwner.IsDead())
		{
			ballistaOwner.Die();
		}
	}

	protected override bool SavesGunState()
	{
		if (isMountedOnVehicle)
		{
			return HasOwner();
		}
		return true;
	}
}
