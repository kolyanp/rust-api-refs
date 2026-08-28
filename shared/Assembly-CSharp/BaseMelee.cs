using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Ai.Gen2;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseMelee : AttackEntity
{
	[Serializable]
	public class MaterialFX
	{
		public string materialName;

		public GameObjectRef fx;
	}

	[Header("Throwing")]
	public bool canThrowAsProjectile;

	public bool canThrowAsEntity;

	public bool canAiHearIt;

	public bool canScareAiWhenAimed;

	public bool onlyThrowAsProjectile;

	public bool ThrowFullStack = true;

	[Header("Melee")]
	public DamageProperties damageProperties;

	public List<DamageTypeEntry> damageTypes;

	public List<DamageTypeEntry> deployableDamageOverrides;

	public float maxDistance = 1.5f;

	public float attackRadius = 0.3f;

	public bool isAutomatic = true;

	public bool blockSprintOnAttack = true;

	public bool canUntieCrates;

	public bool longResourceForgiveness;

	[Header("Third Person Animation")]
	public MeleeWeaponAnimationSubSystem PlayerAnimSystem;

	[Header("Effects")]
	public GameObjectRef strikeFX;

	public bool useStandardHitEffects = true;

	[Header("NPCUsage")]
	public float aiStrikeDelay = 0.2f;

	public GameObjectRef swingEffect;

	public List<MaterialFX> materialStrikeFX = new List<MaterialFX>();

	[Range(0f, 1f)]
	[Header("Other")]
	public float heartStress = 0.5f;

	public ResourceDispenser.GatherProperties gathering;

	public bool canThrowCheck
	{
		get
		{
			if (!canThrowAsProjectile)
			{
				return canThrowAsEntity;
			}
			return true;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseMelee.OnRpcMessage"))
		{
			if (rpc == 2215098782u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - CLEntityThrow"));
				}
				using (TimeWarning.New("CLEntityThrow"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(2215098782u, "CLEntityThrow", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(2215098782u, "CLEntityThrow", this, player))
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
							CLEntityThrow(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in CLEntityThrow");
					}
				}
				return true;
			}
			if (rpc == 3168282921u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - CLProject"));
				}
				using (TimeWarning.New("CLProject"))
				{
					using (msg.read.UseRepeatedElementLimit(1))
					{
						using (TimeWarning.New("Conditions"))
						{
							if (!RPC_Server.FromOwner.Test(3168282921u, "CLProject", this, player))
							{
								return true;
							}
							if (!RPC_Server.IsActiveItem.Test(3168282921u, "CLProject", this, player))
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
								CLProject(msg3);
							}
						}
						catch (Exception ex2)
						{
							Debug.LogException(ex2);
							player.Kick("RPC Error in CLProject");
						}
					}
				}
				return true;
			}
			if (rpc == 4088326849u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - PlayerAttack"));
				}
				using (TimeWarning.New("PlayerAttack"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(4088326849u, "PlayerAttack", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							PlayerAttack(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in PlayerAttack");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override Vector3 GetInheritedVelocity(BasePlayer player, Vector3 direction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return player.GetInheritedThrowVelocity(direction);
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server.FromOwner]
	[RPC_Server]
	private void CLEntityThrow(RPCMessage msg)
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
		}
		else
		{
			if ((Object)(object)player == (Object)null || player.IsHeadUnderwater())
			{
				return;
			}
			if (!canThrowAsEntity)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Not throwable (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "not_throwable");
				return;
			}
			Item item = GetItem();
			if (item == null)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Item not found (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "item_missing");
				return;
			}
			ItemModEntityThrow component = ((Component)item.info).GetComponent<ItemModEntityThrow>();
			if ((Object)(object)component == (Object)null)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Item mod not found (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "mod_missing");
				return;
			}
			Vector3 val = msg.read.Vector3();
			Vector3 val2 = msg.read.Vector3();
			Vector3 normalized = ((Vector3)(ref val2)).normalized;
			if (msg.player.isMounted || msg.player.HasParent())
			{
				val = msg.player.eyes.position;
			}
			else if (!ValidateEyePos(msg.player, val))
			{
				return;
			}
			BaseEntity baseEntity = GameManager.server.CreateEntity(component.entityPrefab.resourcePath, val, Quaternion.LookRotation(normalized));
			if ((Object)(object)baseEntity == (Object)null)
			{
				return;
			}
			baseEntity.SetCreatorEntity(player);
			baseEntity.skinID = skinID;
			baseEntity.OwnerID = player.userID;
			baseEntity.SetVelocity(GetInheritedVelocity(msg.player, normalized) + normalized * component.throwVelocity + msg.player.estimatedVelocity * 0.5f);
			baseEntity.Spawn();
			if (component.consumeOnThrow)
			{
				if (ThrowFullStack)
				{
					item.SetParent(null);
				}
				else
				{
					item.UseItem();
					if (item.amount == 0)
					{
						item.SetParent(null);
					}
				}
			}
			SingletonComponent<NpcNoiseManager>.Instance.OnWeaponThrown(player, this, canAiHearIt);
			OnEntityThrow(baseEntity);
		}
	}

	protected virtual void OnEntityThrow(BaseEntity ent)
	{
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server.MaxRepeatedElements(1)]
	[RPC_Server]
	[RPC_Server.FromOwner]
	private void CLProject(RPCMessage msg)
	{
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
		}
		else
		{
			if ((Object)(object)player == (Object)null || player.IsHeadUnderwater())
			{
				return;
			}
			if (!canThrowAsProjectile)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Not throwable (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "not_throwable");
				return;
			}
			Item item = GetItem();
			if (item == null)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Item not found (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "item_missing");
				return;
			}
			ItemModProjectile component = ((Component)item.info).GetComponent<ItemModProjectile>();
			if (!((Object)(object)component == (Object)null))
			{
				ProjectileShoot val = msg.read.Proto<ProjectileShoot>((ProjectileShoot)null);
				try
				{
					if (val.projectiles.Count != 1)
					{
						AntiHack.Log(player, AntiHackType.ProjectileHack, "Projectile count mismatch (" + base.ShortPrefabName + ")");
						player.stats.combat.LogInvalid(player, this, "count_mismatch");
						return;
					}
					player.CleanupExpiredProjectiles();
					Guid projectileGroupId = Guid.NewGuid();
					foreach (Projectile projectile in val.projectiles)
					{
						if (player.HasFiredProjectile(projectile.projectileID))
						{
							AntiHack.Log(player, AntiHackType.ProjectileHack, $"Duplicate ID ({projectile.projectileID})");
							player.stats.combat.LogInvalid(player, this, "duplicate_id");
							continue;
						}
						Vector3 positionOffset = Vector3.zero;
						if (ConVar.AntiHack.projectile_positionoffset && (player.isMounted || player.HasParent()))
						{
							if (!ValidateEyePos(player, projectile.startPos, checkLineOfSight: false))
							{
								continue;
							}
							Vector3 position = player.eyes.position;
							positionOffset = position - projectile.startPos;
							projectile.startPos = position;
						}
						else if (!ValidateEyePos(player, projectile.startPos))
						{
							continue;
						}
						Item pickupItem = (ThrowFullStack ? item : ItemManager.CreateByItemID(item.info.itemid, 1, 0uL, 0uL));
						player.NoteFiredProjectile(projectile.projectileID, projectile.startPos, projectile.startVel, this, item.info, projectileGroupId, positionOffset, pickupItem);
						Effect effect = new Effect();
						effect.Init(Effect.Type.Projectile, projectile.startPos, projectile.startVel, msg.connection);
						((EffectData)effect).scale = 1f;
						effect.pooledString = component.GetOverrideProjectile(this).resourcePath;
						((EffectData)effect).number = projectile.seed;
						EffectNetwork.Send(effect);
					}
					if (ThrowFullStack)
					{
						item.SetParent(null);
					}
					else
					{
						item.UseItem();
						if (item.amount == 0)
						{
							item.SetParent(null);
						}
					}
					Interface.CallHook("OnMeleeThrown", player, item);
					SingletonComponent<NpcNoiseManager>.Instance.OnWeaponThrown(player, this, canAiHearIt);
					return;
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Item mod not found (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "mod_missing");
		}
	}

	public override void GetAttackStats(HitInfo info)
	{
		List<DamageTypeEntry> entries = damageTypes;
		if (deployableDamageOverrides != null && deployableDamageOverrides.Count > 0 && info.HitEntity is DecayEntity && !(info.HitEntity is BuildingBlock) && !(info.HitEntity is BasePlayer) && !(info.HitEntity is Door) && !(info.HitEntity is SimpleBuildingBlock) && !(info.HitEntity is LootContainer))
		{
			entries = deployableDamageOverrides;
		}
		info.damageTypes.Add(entries);
		info.CanGather = gathering.Any();
	}

	public virtual void DoAttackShared(HitInfo info)
	{
		if (Interface.CallHook("OnPlayerAttack", GetOwnerPlayer(), info) != null)
		{
			return;
		}
		GetAttackStats(info);
		if ((Object)(object)info.HitEntity != (Object)null)
		{
			using (TimeWarning.New("OnAttacked", 50))
			{
				info.HitEntity.OnAttacked(info);
			}
		}
		if (info.DoHitEffects && base.isServer)
		{
			using (TimeWarning.New("ImpactEffect", 20))
			{
				Effect.server.ImpactEffect(info);
			}
			if (!base.IsDestroyed)
			{
				SingletonComponent<NpcNoiseManager>.Instance.OnMeleeHit(this, info);
			}
		}
		if (base.isServer && !base.IsDestroyed)
		{
			using (TimeWarning.New("UpdateItemCondition", 50))
			{
				UpdateItemCondition(info);
			}
			StartAttackCooldown(repeatDelay);
		}
	}

	public ResourceDispenser.GatherPropertyEntry GetGatherInfoFromIndex(ResourceDispenser.GatherType index)
	{
		return gathering.GetFromIndex(index);
	}

	public virtual bool CanHit(HitTest info)
	{
		return true;
	}

	public float TotalDamage()
	{
		float num = 0f;
		foreach (DamageTypeEntry damageType in damageTypes)
		{
			if (!(damageType.amount <= 0f))
			{
				num += damageType.amount;
			}
		}
		return num;
	}

	public bool IsItemBroken()
	{
		return GetOwnerItem()?.isBroken ?? true;
	}

	public void LoseCondition(float amount)
	{
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null && !base.UsingInfiniteAmmoCheat)
		{
			ownerItem.LoseCondition(amount);
		}
	}

	public virtual float GetConditionLoss()
	{
		return 1f;
	}

	public void UpdateItemCondition(HitInfo info)
	{
		Item ownerItem = GetOwnerItem();
		if (ownerItem == null || !ownerItem.hasCondition || info == null || !info.DidHit || info.DidGather)
		{
			return;
		}
		float conditionLoss = GetConditionLoss();
		float num = 0f;
		foreach (DamageTypeEntry damageType in damageTypes)
		{
			if (!(damageType.amount <= 0f))
			{
				num += Mathf.Clamp(damageType.amount - info.damageTypes.Get(damageType.type), 0f, damageType.amount);
			}
		}
		conditionLoss += num * 0.2f;
		if (!base.UsingInfiniteAmmoCheat)
		{
			ownerItem.LoseCondition(conditionLoss);
		}
	}

	private static bool MeleeLineOfSightEntity(Vector3 p0_playerEyesCenter, Vector3 p1_playerEyes, Vector3 p2_hitRaycastStartPos, Vector3 p3_closestRayPos, Vector3 p4_worldHitPos, int lineOfSightLayerMask)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		Vector3 val2 = Vector3.zero;
		Vector3 val3 = Vector3.zero;
		if (ConVar.AntiHack.melee_backtracking > 0f)
		{
			Vector3 val4 = p1_playerEyes - p0_playerEyesCenter;
			val = ((Vector3)(ref val4)).normalized * ConVar.AntiHack.melee_backtracking;
			val4 = p2_hitRaycastStartPos - p1_playerEyes;
			val2 = ((Vector3)(ref val4)).normalized * ConVar.AntiHack.melee_backtracking;
			val4 = p3_closestRayPos - p2_hitRaycastStartPos;
			val3 = ((Vector3)(ref val4)).normalized * ConVar.AntiHack.melee_backtracking;
		}
		if (!GamePhysics.LineOfSight(p0_playerEyesCenter - val, p1_playerEyes + val, lineOfSightLayerMask))
		{
			return false;
		}
		if (!GamePhysics.LineOfSight(p1_playerEyes - val2, p2_hitRaycastStartPos + val2, lineOfSightLayerMask))
		{
			return false;
		}
		if (!GamePhysics.LineOfSight(p2_hitRaycastStartPos - val3, p3_closestRayPos, lineOfSightLayerMask))
		{
			return false;
		}
		if (!GamePhysics.LineOfSight(p3_closestRayPos, p4_worldHitPos, lineOfSightLayerMask))
		{
			return false;
		}
		if (!GamePhysics.LineOfSight(p1_playerEyes, p4_worldHitPos, lineOfSightLayerMask))
		{
			return false;
		}
		return true;
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	public void PlayerAttack(RPCMessage msg)
	{
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_074d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0752: Unknown result type (might be due to invalid IL or missing references)
		//IL_075a: Unknown result type (might be due to invalid IL or missing references)
		//IL_075f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0762: Unknown result type (might be due to invalid IL or missing references)
		//IL_0767: Unknown result type (might be due to invalid IL or missing references)
		//IL_076a: Unknown result type (might be due to invalid IL or missing references)
		//IL_076f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0771: Unknown result type (might be due to invalid IL or missing references)
		//IL_0773: Unknown result type (might be due to invalid IL or missing references)
		//IL_0775: Unknown result type (might be due to invalid IL or missing references)
		//IL_0777: Unknown result type (might be due to invalid IL or missing references)
		//IL_077c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0780: Unknown result type (might be due to invalid IL or missing references)
		//IL_078a: Unknown result type (might be due to invalid IL or missing references)
		//IL_078f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0794: Unknown result type (might be due to invalid IL or missing references)
		//IL_0797: Unknown result type (might be due to invalid IL or missing references)
		//IL_0799: Unknown result type (might be due to invalid IL or missing references)
		//IL_079e: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0683: Unknown result type (might be due to invalid IL or missing references)
		//IL_0688: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0522: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_09cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_09dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0900: Unknown result type (might be due to invalid IL or missing references)
		//IL_0902: Unknown result type (might be due to invalid IL or missing references)
		//IL_0904: Unknown result type (might be due to invalid IL or missing references)
		//IL_090d: Unknown result type (might be due to invalid IL or missing references)
		//IL_090f: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0923: Unknown result type (might be due to invalid IL or missing references)
		//IL_0925: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a17: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a19: Unknown result type (might be due to invalid IL or missing references)
		//IL_096f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0979: Unknown result type (might be due to invalid IL or missing references)
		//IL_086f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0879: Unknown result type (might be due to invalid IL or missing references)
		//IL_0883: Unknown result type (might be due to invalid IL or missing references)
		//IL_088d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0897: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a72: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a7c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a86: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a90: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a2c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a2e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b33: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b38: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b41: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b7e: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
			return;
		}
		using (TimeWarning.New("PlayerAttack", 50))
		{
			PlayerAttack val = msg.read.Proto<PlayerAttack>((PlayerAttack)null);
			try
			{
				if (val == null)
				{
					return;
				}
				HitInfo hitInfo = Pool.Get<HitInfo>();
				hitInfo.LoadFromAttack(val.attack, serverSide: true);
				hitInfo.Initiator = player;
				hitInfo.Weapon = this;
				hitInfo.WeaponPrefab = this;
				hitInfo.Predicted = msg.connection;
				hitInfo.damageProperties = damageProperties;
				if (Interface.CallHook("OnMeleeAttack", player, hitInfo) != null)
				{
					return;
				}
				if (hitInfo.IsNaNOrInfinity())
				{
					string shortPrefabName = base.ShortPrefabName;
					AntiHack.Log(player, AntiHackType.MeleeHack, "Contains NaN (" + shortPrefabName + ")");
					player.stats.combat.LogInvalid(hitInfo, "melee_nan");
					return;
				}
				BaseEntity hitEntity = hitInfo.HitEntity;
				BasePlayer basePlayer = hitInfo.HitEntity as BasePlayer;
				bool flag = (Object)(object)basePlayer != (Object)null;
				bool flag2 = flag && basePlayer.IsSleeping();
				bool flag3 = flag && basePlayer.IsWounded();
				bool flag4 = flag && basePlayer.isMounted;
				bool flag5 = flag && basePlayer.HasParent();
				bool flag6 = (Object)(object)hitEntity != (Object)null;
				bool flag7 = flag6 && hitEntity.IsNpc;
				if (ConVar.AntiHack.melee_protection > 0)
				{
					bool flag8 = true;
					float num = 1f + ConVar.AntiHack.melee_forgiveness;
					float melee_clientframes = ConVar.AntiHack.melee_clientframes;
					float melee_serverframes = ConVar.AntiHack.melee_serverframes;
					float num2 = melee_clientframes / 60f;
					float num3 = melee_serverframes * Mathx.Max(Time.deltaTime, Time.smoothDeltaTime, Time.fixedDeltaTime);
					float num4 = (player.desyncTimeClamped + num2 + num3) * num;
					int num5 = 1075904512;
					if (ConVar.AntiHack.melee_terraincheck)
					{
						num5 |= 0x800000;
					}
					if (ConVar.AntiHack.melee_vehiclecheck)
					{
						num5 |= 0x8000000;
					}
					if (flag && hitInfo.boneArea == (HitArea)(-1))
					{
						string shortPrefabName2 = base.ShortPrefabName;
						string shortPrefabName3 = basePlayer.ShortPrefabName;
						AntiHack.Log(player, AntiHackType.MeleeHack, $"Bone is invalid  ({shortPrefabName2} on {shortPrefabName3} bone {hitInfo.HitBone})");
						player.stats.combat.LogInvalid(hitInfo, "melee_bone");
						flag8 = false;
					}
					Vector3 val2;
					if (ConVar.AntiHack.melee_protection >= 2)
					{
						if (flag6)
						{
							float num6 = hitEntity.AntiHackVelocity();
							val2 = hitEntity.GetParentVelocity();
							float num7 = num6 + ((Vector3)(ref val2)).magnitude;
							float num8 = hitEntity.AntiHackPadding() + num4 * num7;
							float num9 = hitEntity.Distance(hitInfo.HitPositionWorld);
							if (num9 > num8)
							{
								string shortPrefabName4 = base.ShortPrefabName;
								string shortPrefabName5 = hitEntity.ShortPrefabName;
								AntiHack.Log(player, AntiHackType.MeleeHack, string.Format("Entity too far away ({0} on {1} with {2}m > {3}m in {4}s)", new object[5] { shortPrefabName4, shortPrefabName5, num9, num8, num4 }));
								player.stats.combat.LogInvalid(hitInfo, "melee_target");
								flag8 = false;
							}
						}
						if (((ConVar.AntiHack.melee_protection >= 4) & flag8 & flag) && !flag7 && !flag2 && !flag3 && !flag4 && !flag5)
						{
							val2 = basePlayer.GetParentVelocity();
							float magnitude = ((Vector3)(ref val2)).magnitude;
							float num10 = basePlayer.AntiHackPadding() + num4 * magnitude + ConVar.AntiHack.tickhistoryforgiveness;
							float num11 = basePlayer.tickHistory.Distance(basePlayer, hitInfo.HitPositionWorld);
							if (num11 > num10)
							{
								string shortPrefabName6 = base.ShortPrefabName;
								string shortPrefabName7 = basePlayer.ShortPrefabName;
								AntiHack.Log(player, AntiHackType.MeleeHack, string.Format("Player too far away ({0} on {1} with {2}m > {3}m in {4}s)", new object[5] { shortPrefabName6, shortPrefabName7, num11, num10, num4 }));
								player.stats.combat.LogInvalid(hitInfo, "player_distance");
								flag8 = false;
							}
						}
						if (((((ConVar.AntiHack.melee_protection >= 4) & flag8 & flag) && !flag7 && !flag2 && !flag3) & flag5) && ConVar.AntiHack.parenthistory && basePlayer.tickHistory.ParentCount > 0)
						{
							val2 = basePlayer.GetParentVelocity();
							float magnitude2 = ((Vector3)(ref val2)).magnitude;
							float num12 = basePlayer.AntiHackPadding() + num4 * magnitude2 + ConVar.AntiHack.tickhistoryforgiveness;
							float num13 = basePlayer.tickHistory.DistanceParented(basePlayer, hitInfo.HitPositionWorld);
							if (num13 > num12)
							{
								string shortPrefabName8 = base.ShortPrefabName;
								string shortPrefabName9 = basePlayer.ShortPrefabName;
								AntiHack.Log(player, AntiHackType.MeleeHack, string.Format("Player (parented) too far away ({0} on {1} with {2}m > {3}m in {4}s)", new object[5] { shortPrefabName8, shortPrefabName9, num13, num12, num4 }));
								player.stats.combat.LogInvalid(hitInfo, "player_distance_parent");
								flag8 = false;
							}
						}
					}
					if (ConVar.AntiHack.melee_protection >= 1)
					{
						if (ConVar.AntiHack.melee_protection >= 4 && player.HasParent() && ConVar.AntiHack.parenthistory && player.tickHistory.ParentCount > 0)
						{
							val2 = player.GetParentVelocity();
							float magnitude3 = ((Vector3)(ref val2)).magnitude;
							float num14 = player.AntiHackPadding() + num4 * magnitude3 + num * maxDistance;
							float num15 = player.tickHistory.DistanceParented(player, hitInfo.HitPositionWorld);
							if (num15 > num14)
							{
								string shortPrefabName10 = base.ShortPrefabName;
								string text = (flag6 ? hitEntity.ShortPrefabName : "world");
								AntiHack.Log(player, AntiHackType.MeleeHack, string.Format("Initiator too far away (parent tick history) ({0} on {1} with {2}m > {3}m in {4}s)", new object[5] { shortPrefabName10, text, num15, num14, num4 }));
								player.stats.combat.LogInvalid(hitInfo, "melee_initiator_tick_parent");
								flag8 = false;
							}
						}
						else if (ConVar.AntiHack.melee_protection >= 4)
						{
							val2 = player.GetParentVelocity();
							float magnitude4 = ((Vector3)(ref val2)).magnitude;
							float num16 = player.AntiHackPadding() + num4 * magnitude4 + num * maxDistance;
							float num17 = player.tickHistory.Distance(player, hitInfo.HitPositionWorld);
							if (num17 > num16)
							{
								string shortPrefabName11 = base.ShortPrefabName;
								string text2 = (flag6 ? hitEntity.ShortPrefabName : "world");
								AntiHack.Log(player, AntiHackType.MeleeHack, string.Format("Initiator too far away (tick history) ({0} on {1} with {2}m > {3}m in {4}s)", new object[5] { shortPrefabName11, text2, num17, num16, num4 }));
								player.stats.combat.LogInvalid(hitInfo, "melee_initiator_tick");
								flag8 = false;
							}
						}
						else
						{
							float num18 = player.AntiHackVelocity();
							val2 = player.GetParentVelocity();
							float num19 = num18 + ((Vector3)(ref val2)).magnitude;
							float num20 = player.AntiHackPadding() + num4 * num19 + num * maxDistance;
							float num21 = player.Distance(hitInfo.HitPositionWorld);
							if (num21 > num20)
							{
								string shortPrefabName12 = base.ShortPrefabName;
								string text3 = (flag6 ? hitEntity.ShortPrefabName : "world");
								AntiHack.Log(player, AntiHackType.MeleeHack, string.Format("Initiator too far away ({0} on {1} with {2}m > {3}m in {4}s)", new object[5] { shortPrefabName12, text3, num21, num20, num4 }));
								player.stats.combat.LogInvalid(hitInfo, "melee_initiator");
								flag8 = false;
							}
						}
					}
					if (ConVar.AntiHack.melee_protection >= 3)
					{
						if (flag6)
						{
							Vector3 center = player.eyes.center;
							Vector3 position = player.eyes.position;
							Vector3 pointStart = hitInfo.PointStart;
							Vector3 hitPositionWorld = hitInfo.HitPositionWorld;
							Vector3 val3 = hitPositionWorld;
							val2 = hitPositionWorld - pointStart;
							hitPositionWorld = val3 - ((Vector3)(ref val2)).normalized * 0.001f;
							Vector3 val4 = hitInfo.PositionOnRay(hitPositionWorld);
							bool flag9 = MeleeLineOfSightEntity(center, position, pointStart, val4, hitPositionWorld, num5);
							string text4 = hitEntity.Categorize();
							string text5 = string.Empty;
							switch (text4)
							{
							case "player":
								text5 = (flag9 ? "hit_player_direct_los" : "hit_player_indirect_los");
								break;
							case "building":
								text5 = (flag9 ? "hit_building_direct_los" : "hit_building_indirect_los");
								break;
							case "entity":
								text5 = (flag9 ? "hit_entity_direct_los" : "hit_entity_indirect_los");
								break;
							}
							if (!string.IsNullOrEmpty(text5))
							{
								player.stats.Add(text5, 1, Stats.Server);
							}
							if (!flag9)
							{
								string shortPrefabName13 = base.ShortPrefabName;
								string shortPrefabName14 = hitEntity.ShortPrefabName;
								AntiHack.Log(player, AntiHackType.MeleeHack, string.Format("Line of sight entity ({0} on {1}) {2} {3} {4} {5} {6}", new object[7] { shortPrefabName13, shortPrefabName14, center, position, pointStart, val4, hitPositionWorld }));
								player.stats.combat.LogInvalid(hitInfo, "melee_los_entity");
								flag8 = false;
							}
						}
						if (flag6 && !flag && ConVar.AntiHack.melee_los_entity_realpos)
						{
							Vector3 position2 = player.eyes.position;
							float melee_losforgiveness = ConVar.AntiHack.melee_losforgiveness;
							Vector3 hitPositionWorld2 = hitInfo.HitPositionWorld;
							Vector3 val5 = hitEntity.ClosestPoint(hitPositionWorld2);
							float num22 = Vector3.Distance(val5, hitPositionWorld2);
							if (!GamePhysics.LineOfSight(position2, val5, num5, 0f, melee_losforgiveness, hitEntity) || !GamePhysics.LineOfSight(val5, position2, num5, melee_losforgiveness, 0f, hitEntity) || num22 > ConVar.AntiHack.melee_los_entity_realpos_distance)
							{
								string shortPrefabName15 = base.ShortPrefabName;
								string shortPrefabName16 = hitEntity.ShortPrefabName;
								AntiHack.Log(player, AntiHackType.MeleeHack, string.Format("Line of sight entity real position ({0} on {1}) {2} {3} {4}", new object[5] { shortPrefabName15, shortPrefabName16, position2, val5, num22 }));
								player.stats.combat.LogInvalid(hitInfo, "melee_los_entity_realpos");
								flag8 = false;
							}
						}
						if ((flag8 & flag) && !flag7)
						{
							Vector3 hitPositionWorld3 = hitInfo.HitPositionWorld;
							Vector3 position3 = basePlayer.eyes.position;
							Vector3 val6 = basePlayer.CenterPoint();
							float melee_losforgiveness2 = ConVar.AntiHack.melee_losforgiveness;
							bool flag10 = GamePhysics.LineOfSight(hitPositionWorld3, position3, num5, 0f, melee_losforgiveness2) && GamePhysics.LineOfSight(position3, hitPositionWorld3, num5, melee_losforgiveness2, 0f);
							if (!flag10)
							{
								flag10 = GamePhysics.LineOfSight(hitPositionWorld3, val6, num5, 0f, melee_losforgiveness2) && GamePhysics.LineOfSight(val6, hitPositionWorld3, num5, melee_losforgiveness2, 0f);
							}
							if (!flag10)
							{
								string shortPrefabName17 = base.ShortPrefabName;
								string shortPrefabName18 = basePlayer.ShortPrefabName;
								AntiHack.Log(player, AntiHackType.MeleeHack, string.Format("Line of sight player ({0} on {1}) {2} {3} or {4} {5}", new object[6] { shortPrefabName17, shortPrefabName18, hitPositionWorld3, position3, hitPositionWorld3, val6 }));
								player.stats.combat.LogInvalid(hitInfo, "melee_los_player");
								flag8 = false;
							}
						}
					}
					if (ConVar.AntiHack.melee_protection >= 5 && (flag8 & flag6) && !flag && hitEntity.AntiHackVelocity() == 0f && !hitEntity.IsOnMovingObject() && !(hitEntity is ResourceEntity) && !(hitEntity is CollectibleEntity) && !(hitEntity is BaseLock) && !(hitEntity is BaseCombatEntity { ValidateMeleeColliderAntihack: false }))
					{
						Vector3 hitPositionWorld4 = hitInfo.HitPositionWorld;
						float melee_entity_bounds_radius = ConVar.AntiHack.melee_entity_bounds_radius;
						if (!GamePhysics.OverlapSphereHasEntity(hitPositionWorld4, melee_entity_bounds_radius, hitEntity, 1270440705, (QueryTriggerInteraction)1))
						{
							string shortPrefabName19 = base.ShortPrefabName;
							string shortPrefabName20 = hitEntity.ShortPrefabName;
							AntiHack.Log(player, AntiHackType.MeleeHack, string.Format("Entity hit too far from collider ({0} on {1}) {2} with {3} radius", new object[4] { shortPrefabName19, shortPrefabName20, hitPositionWorld4, melee_entity_bounds_radius }));
							player.stats.combat.LogInvalid(hitInfo, "melee_collider_entity");
							flag8 = false;
						}
					}
					if (!flag8)
					{
						AntiHack.AddViolation(player, AntiHackType.MeleeHack, ConVar.AntiHack.melee_penalty);
						return;
					}
				}
				player.metabolism.UseHeart(heartStress * 0.2f);
				using (TimeWarning.New("DoAttackShared", 50))
				{
					DoAttackShared(hitInfo);
				}
				Pool.Free<HitInfo>(ref hitInfo);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public override bool CanBeUsedInWater()
	{
		return true;
	}

	public virtual string GetStrikeEffectPath(string materialName)
	{
		for (int i = 0; i < materialStrikeFX.Count; i++)
		{
			if (materialStrikeFX[i].materialName == materialName && materialStrikeFX[i].fx.isValid)
			{
				return materialStrikeFX[i].fx.resourcePath;
			}
		}
		return strikeFX.resourcePath;
	}

	public override void ServerUse(HeldEntityServerUseParams parameters)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (base.isClient || HasAttackCooldown())
		{
			return;
		}
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!((Object)(object)ownerPlayer == (Object)null))
		{
			StartAttackCooldown(repeatDelay * 2f);
			ownerPlayer.SignalBroadcast(Signal.Attack, string.Empty);
			if (swingEffect.isValid)
			{
				Effect.server.Run(swingEffect.resourcePath, ((Component)this).transform.position, Vector3.forward, ownerPlayer.net.connection);
			}
			if (IsInvoking(ServerUse_Strike))
			{
				CancelInvoke(ServerUse_Strike);
			}
			Invoke(ServerUse_Strike, aiStrikeDelay);
		}
	}

	public virtual void ServerUse_OnHit(HitInfo info)
	{
	}

	public void ServerUse_Strike()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer == (Object)null)
		{
			return;
		}
		Vector3 position = ownerPlayer.eyes.position;
		Vector3 val = ownerPlayer.eyes.BodyForward();
		for (int i = 0; i < 2; i++)
		{
			List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
			GamePhysics.TraceAll(new Ray(position - val * ((i == 0) ? 0f : 0.2f), val), (i == 0) ? 0f : attackRadius, list, effectiveRange + 0.2f, 1220225809, (QueryTriggerInteraction)0);
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				RaycastHit hit = list[j];
				BaseEntity entity = RaycastHitEx.GetEntity(hit);
				if ((Object)(object)entity == (Object)null || ((Object)(object)entity != (Object)null && ((Object)(object)entity == (Object)(object)ownerPlayer || entity.EqualNetID((BaseNetworkable)ownerPlayer))) || ((Object)(object)entity != (Object)null && entity.isClient) || entity.Categorize() == ownerPlayer.Categorize())
				{
					continue;
				}
				float num = 0f;
				foreach (DamageTypeEntry damageType in damageTypes)
				{
					num += damageType.amount;
				}
				entity.OnAttacked(new HitInfo(ownerPlayer, entity, DamageType.Slash, num * npcDamageScale));
				HitInfo hitInfo = Pool.Get<HitInfo>();
				hitInfo.HitEntity = entity;
				hitInfo.HitPositionWorld = ((RaycastHit)(ref hit)).point;
				hitInfo.HitNormalWorld = -val;
				if (entity is BaseNpc || entity is BasePlayer)
				{
					hitInfo.HitMaterial = StringPool.Get("Flesh");
				}
				else
				{
					hitInfo.HitMaterial = StringPool.Get(((Object)(object)RaycastHitEx.GetCollider(hit).sharedMaterial != (Object)null) ? AssetNameCache.GetName(RaycastHitEx.GetCollider(hit).sharedMaterial) : "generic");
				}
				ServerUse_OnHit(hitInfo);
				Effect.server.ImpactEffect(hitInfo);
				Pool.Free<HitInfo>(ref hitInfo);
				flag = true;
				if (!((Object)(object)entity != (Object)null) || entity.ShouldBlockProjectiles())
				{
					break;
				}
			}
			Pool.FreeUnmanaged<RaycastHit>(ref list);
			if (flag)
			{
				break;
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			info.msg.baseMelee = Pool.Get<BaseMelee>();
			info.msg.baseMelee.canThrowAsProjectile = canThrowAsProjectile;
			info.msg.baseMelee.onlyThrowAsProjectile = onlyThrowAsProjectile;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
	}
}
