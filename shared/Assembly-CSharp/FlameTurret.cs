using System.Collections.Generic;
using Facepunch;
using Facepunch.Rust;
using Oxide.Core;
using Rust;
using UnityEngine;

public class FlameTurret : StorageContainer
{
	public class UpdateFlameTurretWorkQueue : ObjectWorkQueue<FlameTurret>
	{
		protected override void RunJob(FlameTurret entity)
		{
			if (((ObjectWorkQueue<FlameTurret>)this).ShouldAdd(entity))
			{
				entity.ServerThink();
			}
		}

		protected override bool ShouldAdd(FlameTurret entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	public static UpdateFlameTurretWorkQueue updateFlameTurretQueueServer = new UpdateFlameTurretWorkQueue();

	public Transform upper;

	public float arc = 45f;

	public float triggeredDuration = 5f;

	public float flameRange = 7f;

	public float flameRadius = 4f;

	public float fuelPerSec = 1f;

	public Transform eyeTransform;

	public List<DamageTypeEntry> damagePerSec;

	public GameObjectRef triggeredEffect;

	public GameObjectRef fireballPrefab;

	public GameObjectRef explosionEffect;

	public TargetTrigger trigger;

	public const Flags Flag_Triggered = Flags.Reserved4;

	private int turnDir = 1;

	private Vector3 aimDir;

	private float lastMovementUpdate;

	private float nextFireballTime;

	private float triggeredTime;

	private float lastServerThink;

	private float triggerCheckRate = 2f;

	private float nextTriggerCheckTime;

	private float _cacheTimeout;

	private IPrivilege _cachedPriv;

	private float pendingFuel;

	public void MovementUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.realtimeSinceStartup - lastMovementUpdate;
		lastMovementUpdate = Time.realtimeSinceStartup;
		aimDir += new Vector3(0f, num * GetSpinSpeed(), 0f) * (float)turnDir;
		if (aimDir.y >= arc || aimDir.y <= 0f - arc)
		{
			turnDir *= -1;
			aimDir.y = Mathf.Clamp(aimDir.y, 0f - arc, arc);
		}
		if (base.isServer)
		{
			((ObjectWorkQueue<FlameTurret>)updateFlameTurretQueueServer).Add(this);
		}
	}

	public float GetSpinSpeed()
	{
		return IsTriggered() ? 180 : 45;
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

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (!IsTriggered())
		{
			return base.ShouldDisplayPickupOption(player);
		}
		return false;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRepeating(MovementUpdate, 0f, 0.1f);
	}

	public void ServerThink()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (!base.isClient)
		{
			bool num = IsTriggered();
			float delta = Time.realtimeSinceStartup - lastServerThink;
			lastServerThink = Time.realtimeSinceStartup;
			if (IsTriggered() && (Time.realtimeSinceStartup - triggeredTime > triggeredDuration || !HasFuel()))
			{
				SetTriggered(triggered: false);
			}
			if (!IsTriggered() && HasFuel() && CheckTrigger())
			{
				SetTriggered(triggered: true);
				Effect.server.Run(triggeredEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
			}
			if (num != IsTriggered())
			{
				SendNetworkUpdateImmediate();
			}
			if (IsTriggered())
			{
				DoFlame(delta);
			}
		}
	}

	public void SetTriggered(bool triggered)
	{
		if (triggered && HasFuel())
		{
			triggeredTime = Time.realtimeSinceStartup;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved4, triggered && HasFuel());
	}

	public override void OnAttacked(HitInfo info)
	{
		if (!base.isClient)
		{
			if (info.damageTypes.IsMeleeType())
			{
				SetTriggered(triggered: true);
			}
			base.OnAttacked(info);
		}
	}

	public bool CheckTrigger()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		if (Time.realtimeSinceStartup < nextTriggerCheckTime)
		{
			return false;
		}
		nextTriggerCheckTime = Time.realtimeSinceStartup + 1f / triggerCheckRate;
		List<RaycastHit> list = null;
		try
		{
			HashSet<BaseEntity> entityContents = trigger.entityContents;
			if (entityContents == null || entityContents.Count == 0)
			{
				return false;
			}
			bool flag = false;
			bool flag2 = false;
			IPrivilege privilege = null;
			foreach (BaseEntity item in entityContents)
			{
				BasePlayer basePlayer = item as BasePlayer;
				if ((Object)(object)basePlayer == (Object)null || basePlayer.IsSleeping() || !basePlayer.IsAlive() || ((Component)basePlayer).transform.position.y > GetEyePosition().y + 0.5f)
				{
					continue;
				}
				object obj = Interface.CallHook("CanBeTargeted", basePlayer, this);
				if (obj is bool)
				{
					if (list != null)
					{
						Pool.FreeUnmanaged<RaycastHit>(ref list);
					}
					return (bool)obj;
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

	public override void OnDied(HitInfo info)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)GetFuelAmount() / 500f;
		DamageUtil.RadiusDamage(this, LookupPrefab(), GetEyePosition(), 2f, 6f, damagePerSec, 133120, useLineOfSight: true);
		SeismicSensor.Notify(GetEyePosition(), 1);
		Effect.server.Run(explosionEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
		int num2 = Mathf.CeilToInt(Mathf.Clamp(num * 8f, 1f, 8f));
		for (int i = 0; i < num2; i++)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(fireballPrefab.resourcePath, ((Component)this).transform.position, ((Component)this).transform.rotation);
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				Vector3 onUnitSphere = Random.onUnitSphere;
				((Component)baseEntity).transform.position = ((Component)this).transform.position + new Vector3(0f, 1.5f, 0f) + onUnitSphere * Random.Range(-1f, 1f);
				baseEntity.Spawn();
				baseEntity.SetVelocity(onUnitSphere * (float)Random.Range(3, 10));
			}
		}
		base.OnDied(info);
	}

	public int GetFuelAmount()
	{
		Item slot = base.inventory.GetSlot(0);
		if (slot == null || slot.amount < 1)
		{
			return 0;
		}
		return slot.amount;
	}

	public bool HasFuel()
	{
		return GetFuelAmount() > 0;
	}

	public bool UseFuel(float seconds)
	{
		Item slot = base.inventory.GetSlot(0);
		if (slot == null || slot.amount < 1)
		{
			return false;
		}
		pendingFuel += seconds * fuelPerSec;
		if (pendingFuel >= 1f)
		{
			int num = Mathf.FloorToInt(pendingFuel);
			slot.UseItem(num);
			Facepunch.Rust.Analytics.Azure.AddPendingItems(this, slot.info.shortname, num, "flame_turret");
			pendingFuel -= num;
		}
		return true;
	}

	public void DoFlame(float delta)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		if (!UseFuel(delta))
		{
			return;
		}
		Ray val = default(Ray);
		((Ray)(ref val))._002Ector(GetEyePosition(), ((Component)this).transform.TransformDirection(Quaternion.Euler(aimDir) * Vector3.forward));
		Vector3 origin = ((Ray)(ref val)).origin;
		RaycastHit val2 = default(RaycastHit);
		bool flag = Physics.SphereCast(val, 0.4f, ref val2, flameRange, 1218652417);
		if (!flag)
		{
			((RaycastHit)(ref val2)).point = origin + ((Ray)(ref val)).direction * flameRange;
		}
		float amount = damagePerSec[0].amount;
		damagePerSec[0].amount = amount * delta;
		DamageUtil.RadiusDamage(this, LookupPrefab(), ((RaycastHit)(ref val2)).point - ((Ray)(ref val)).direction * 0.1f, flameRadius * 0.5f, flameRadius, damagePerSec, 2230272, useLineOfSight: true);
		DamageUtil.RadiusDamage(this, LookupPrefab(), ((Component)this).transform.position + new Vector3(0f, 1.25f, 0f), 0.25f, 0.25f, damagePerSec, 133120, useLineOfSight: false);
		damagePerSec[0].amount = amount;
		if (Time.realtimeSinceStartup >= nextFireballTime)
		{
			nextFireballTime = Time.realtimeSinceStartup + Random.Range(1f, 2f);
			Vector3 val3 = (((Random.Range(0, 10) <= 7) & flag) ? ((RaycastHit)(ref val2)).point : (((Ray)(ref val)).origin + ((Ray)(ref val)).direction * (flag ? ((RaycastHit)(ref val2)).distance : flameRange) * Random.Range(0.4f, 1f)));
			BaseEntity baseEntity = GameManager.server.CreateEntity(fireballPrefab.resourcePath, val3 - ((Ray)(ref val)).direction * 0.25f);
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				baseEntity.creatorEntity = this;
				baseEntity.Spawn();
			}
		}
	}
}
