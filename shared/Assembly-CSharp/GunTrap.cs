using System;
using System.Collections.Generic;
using Facepunch;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;

public class GunTrap : StorageContainer
{
	public class GunTrapScanWorkQueue : PersistentObjectWorkQueue<GunTrap>
	{
		protected override void RunJob(GunTrap entity)
		{
			if (((PersistentObjectWorkQueue<GunTrap>)this).ShouldAdd(entity))
			{
				entity.ServerThink();
			}
		}

		protected override bool ShouldAdd(GunTrap entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	[ServerVar(Help = "How many milliseconds to spend on target scanning per frame")]
	public static float gun_trap_budget_ms = 0.5f;

	public static GunTrapScanWorkQueue updateGunTrapWorkQueue = new GunTrapScanWorkQueue();

	public GameObjectRef gun_fire_effect;

	public GameObjectRef bulletEffect;

	public GameObjectRef triggeredEffect;

	public Transform muzzlePos;

	public Transform eyeTransform;

	public int numPellets = 15;

	public int aimCone = 30;

	public float sensorRadius = 1.25f;

	public ItemDefinition ammoType;

	public TargetTrigger trigger;

	public const Flags Flag_Triggered = Flags.Reserved4;

	private float triggeredTime;

	private readonly float triggerCooldownDuration = 0.5f;

	private float triggerCooldown;

	private float _cacheTimeout;

	private IPrivilege _cachedPriv;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("GunTrap.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override string Categorize()
	{
		return "GunTrap";
	}

	public bool IsTriggered()
	{
		return HasFlag(Flags.Reserved4);
	}

	public Vector3 GetEyePosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return eyeTransform.position;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		((PersistentObjectWorkQueue<GunTrap>)updateGunTrapWorkQueue).Add(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		((PersistentObjectWorkQueue<GunTrap>)updateGunTrapWorkQueue).Remove(this);
	}

	public void ServerThink()
	{
		if (IsTriggered() && Time.realtimeSinceStartup - triggeredTime > triggerCooldownDuration)
		{
			SetTriggered(triggered: false);
		}
		if (!(triggerCooldown > Time.realtimeSinceStartup) && CanFire() && CheckTrigger())
		{
			SetTriggered(triggered: true);
			FireWeapon();
			triggerCooldown = Time.realtimeSinceStartup + triggerCooldownDuration;
		}
	}

	public bool CheckTrigger()
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		List<RaycastHit> list = null;
		try
		{
			HashSet<BaseEntity> entityContents = trigger.entityContents;
			if (entityContents == null || entityContents.Count == 0)
			{
				return false;
			}
			if (!CanFire())
			{
				return false;
			}
			bool flag = false;
			bool flag2 = false;
			IPrivilege privilege = null;
			foreach (BaseEntity item in entityContents)
			{
				BasePlayer basePlayer = item as BasePlayer;
				if ((Object)(object)basePlayer == (Object)null || basePlayer.IsSleeping() || !basePlayer.IsAlive())
				{
					continue;
				}
				object obj = Interface.CallHook("CanBeTargeted", basePlayer, this);
				if (obj is bool)
				{
					flag = (bool)obj;
					break;
				}
				if (!flag2)
				{
					flag2 = true;
					privilege = GetCachedPrivilege();
				}
				if (privilege != null && privilege.IsAuthed(basePlayer))
				{
					continue;
				}
				if (list == null)
				{
					list = Pool.Get<List<RaycastHit>>();
				}
				else
				{
					list.Clear();
				}
				Vector3 position = basePlayer.eyes.position;
				Vector3 val = GetEyePosition() - basePlayer.eyes.position;
				GamePhysics.TraceAll(new Ray(position, ((Vector3)(ref val)).normalized), 0f, list, 9f, 1218519297, (QueryTriggerInteraction)0);
				for (int i = 0; i < list.Count; i++)
				{
					BaseEntity entity = RaycastHitEx.GetEntity(list[i]);
					if ((Object)(object)entity != (Object)null && ((Object)(object)entity == (Object)(object)this || entity.EqualNetID((BaseNetworkable)this)))
					{
						flag = true;
						break;
					}
					if (!((Object)(object)entity != (Object)null) || entity.ShouldBlockProjectiles())
					{
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			return flag;
		}
		finally
		{
			if (list != null)
			{
				Pool.FreeUnmanaged<RaycastHit>(ref list);
			}
		}
	}

	public void FireWeapon()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (UseAmmo())
		{
			Effect.server.Run(gun_fire_effect.resourcePath, this, StringPool.Get(((Object)((Component)muzzlePos).gameObject).name), Vector3.zero, Vector3.zero);
			for (int i = 0; i < numPellets; i++)
			{
				FireBullet();
			}
		}
	}

	public void FireBullet()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		float damageAmount = 10f;
		Vector3 val = ((Component)muzzlePos).transform.position - muzzlePos.forward * 0.25f;
		Vector3 forward = ((Component)muzzlePos).transform.forward;
		Vector3 modifiedAimConeDirection = AimConeUtil.GetModifiedAimConeDirection(aimCone, forward);
		Vector3 arg = val + modifiedAimConeDirection * 300f;
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_FireGun"), arg);
		PooledList<RaycastHit> val2 = Pool.Get<PooledList<RaycastHit>>();
		try
		{
			int layerMask = 1220225793;
			GamePhysics.TraceAll(new Ray(val, modifiedAimConeDirection), 0.1f, (List<RaycastHit>)(object)val2, 300f, layerMask, (QueryTriggerInteraction)0);
			for (int i = 0; i < ((List<RaycastHit>)(object)val2).Count; i++)
			{
				RaycastHit hit = ((List<RaycastHit>)(object)val2)[i];
				BaseEntity entity = RaycastHitEx.GetEntity(hit);
				if ((Object)(object)entity != (Object)null && ((Object)(object)entity == (Object)(object)this || entity.EqualNetID((BaseNetworkable)this)))
				{
					continue;
				}
				if ((Object)(object)(entity as BaseCombatEntity) != (Object)null)
				{
					HitInfo info = new HitInfo(this, entity, DamageType.Bullet, damageAmount, ((RaycastHit)(ref hit)).point);
					entity.OnAttacked(info);
					if (entity is BasePlayer || entity is BaseNpc)
					{
						Effect.server.ImpactEffect(new HitInfo
						{
							HitPositionWorld = ((RaycastHit)(ref hit)).point,
							HitNormalWorld = -((RaycastHit)(ref hit)).normal,
							HitMaterial = StringPool.Get("Flesh")
						});
					}
				}
				if (!((Object)(object)entity != (Object)null) || entity.ShouldBlockProjectiles())
				{
					arg = ((RaycastHit)(ref hit)).point;
					break;
				}
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	public bool CanFire()
	{
		foreach (Item item in base.inventory.itemList)
		{
			if ((Object)(object)item.info == (Object)(object)ammoType && item.amount > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool UseAmmo()
	{
		foreach (Item item in base.inventory.itemList)
		{
			if ((Object)(object)item.info == (Object)(object)ammoType && item.amount > 0)
			{
				item.UseItem();
				return true;
			}
		}
		return false;
	}

	private IPrivilege GetCachedPrivilege()
	{
		if (_cachedPriv != null && ((BaseEntity)_cachedPriv).IsDestroyed)
		{
			_cachedPriv = null;
		}
		if (_cachedPriv == null || Time.realtimeSinceStartup > _cacheTimeout)
		{
			_cachedPriv = null;
			_cachedPriv = GetPrivilege();
			_cacheTimeout = Time.realtimeSinceStartup + 3f;
		}
		return _cachedPriv;
	}

	public void SetTriggered(bool triggered)
	{
		if (triggered && CanFire())
		{
			triggeredTime = Time.realtimeSinceStartup;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved4, triggered && CanFire());
	}
}
