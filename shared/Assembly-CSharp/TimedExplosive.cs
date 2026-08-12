using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Oxide.Core;
using Rust;
using Rust.Ai.Gen2;
using UnityEngine;

public class TimedExplosive : BaseEntity, ServerProjectile.IProjectileImpact
{
	public enum ExplosionEffectOffsetMode
	{
		Local,
		World
	}

	[Header("General")]
	public float timerAmountMin;

	public float timerAmountMax;

	public float minExplosionRadius;

	public float explosionRadius;

	public bool explodeOnContact;

	public bool canStick;

	public bool forceRunClippingChecks;

	public bool onlyDamageParent;

	[Header("AI")]
	public bool IgnoreAI;

	public bool BlindAI;

	public float aiBlindDuration;

	public float aiBlindRange;

	[Header("Offsets")]
	public ExplosionEffectOffsetMode explosionOffsetMode;

	public Vector3 explosionEffectOffset;

	[Header("Normals")]
	public bool explosionMatchesNormal;

	public bool explosionUsesForward;

	public bool explosionMatchesOrientation;

	public bool explosionMatchesVelocity;

	public bool explosionMatchesInverseVelocity;

	[Header("Effects")]
	public GameObjectRef explosionEffect;

	[Tooltip("Optional: Will fall back to watersurfaceExplosionEffect or explosionEffect if not assigned.")]
	public GameObjectRef underwaterExplosionEffect;

	public GameObjectRef stickEffect;

	public GameObjectRef bounceEffect;

	public GameObjectRef watersurfaceExplosionEffect;

	[Min(0f)]
	[Header("Water")]
	public float underwaterExplosionDepth;

	[MinMax(0f, 100f)]
	[Tooltip("Optional: Will fall back to underwaterExplosionEffect or explosionEffect if not assigned.")]
	public MinMax watersurfaceExplosionDepth;

	public bool waterCausesExplosion;

	[Header("Other")]
	public int vibrationLevel;

	public List<DamageTypeEntry> damageTypes;

	public List<DamageTypeEntry> playerDamage;

	public bool splashWallpaperThroughWalls;

	[NonSerialized]
	private float lastBounceTime;

	private bool hadRB;

	private float rbMass;

	private float rbDrag;

	private float rbAngularDrag;

	private Vector3 rbVelocityBeforeCollision;

	private CollisionDetectionMode rbCollisionMode;

	[NonSerialized]
	public ItemOwnershipShare ItemOwnership;

	protected BasePlayer creatorPlayer;

	private const int parentOnlySplashDamage = 166144;

	private const int fullSplashDamage = 1210222849;

	protected Vector3? hitNormal;

	private static BaseEntity[] queryResults = new BaseEntity[64];

	private Vector3 lastPosition;

	public override bool PositionTickFixedTime
	{
		protected get
		{
			return true;
		}
	}

	protected virtual bool AlwaysRunWaterCheck => false;

	public List<DamageTypeEntry> GetDamageList(BaseEntity entity)
	{
		if (ConVar.Server.explosive_testing_mode)
		{
			return new List<DamageTypeEntry>
			{
				new DamageTypeEntry
				{
					amount = 1f,
					type = DamageType.Explosion
				}
			};
		}
		if (entity is BasePlayer && playerDamage != null && playerDamage.Count > 0)
		{
			return playerDamage;
		}
		return damageTypes;
	}

	public void SetDamageScale(float scale)
	{
		foreach (DamageTypeEntry damageType in damageTypes)
		{
			damageType.amount *= scale;
		}
	}

	public void SetCreator(BasePlayer ply)
	{
		creatorPlayer = ply;
	}

	public override float GetNetworkTime()
	{
		return Time.fixedTime;
	}

	public override void ServerInit()
	{
		lastBounceTime = Time.time;
		base.ServerInit();
		SetFuse(GetRandomTimerTime());
		if (ComponentEx.HasComponent<Collider>((Component)(object)((Component)this).transform))
		{
			ReceiveCollisionMessages(b: true);
		}
		if (waterCausesExplosion || AlwaysRunWaterCheck)
		{
			InvokeRepeating(WaterCheck, 0f, 0.5f);
		}
	}

	public virtual void WaterCheck()
	{
		if (waterCausesExplosion && WaterFactor() >= 0.5f)
		{
			Explode();
		}
	}

	public virtual void SetFuse(float fuseLength)
	{
		if (base.isServer)
		{
			object obj = Interface.CallHook("OnExplosiveFuseSet", this, fuseLength);
			if (obj is float)
			{
				fuseLength = (float)obj;
			}
			Invoke(Explode, fuseLength);
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved2, b: true);
		}
	}

	public virtual float GetRandomTimerTime()
	{
		return Random.Range(timerAmountMin, timerAmountMax);
	}

	public virtual void ProjectileImpact(RaycastHit info, Vector3 rayOrigin)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		hitNormal = ((RaycastHit)(ref info)).normal;
		Explode();
	}

	public void ForceExplode()
	{
		if (this is DudTimedExplosive dudTimedExplosive)
		{
			dudTimedExplosive.dudChance = 0f;
		}
		if (this is RFTimedExplosive rFTimedExplosive)
		{
			rFTimedExplosive.DisarmRF();
		}
		Explode();
	}

	public virtual void Explode()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Explode(PivotPoint());
	}

	private Vector3 GetExplosionNormal()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 result;
		if (explosionUsesForward)
		{
			result = ((Component)this).transform.forward;
		}
		else if (!explosionMatchesOrientation)
		{
			result = (explosionMatchesVelocity ? ((Vector3)(ref rbVelocityBeforeCollision)).normalized : ((!explosionMatchesInverseVelocity) ? Vector3.up : (-((Vector3)(ref rbVelocityBeforeCollision)).normalized)));
		}
		else
		{
			Quaternion rotation = ((Component)this).transform.rotation;
			Vector3 forward = Vector3.forward;
			result = rotation * forward;
		}
		if (explosionMatchesNormal && hitNormal.HasValue)
		{
			result = hitNormal.Value;
		}
		return result;
	}

	public virtual void Explode(Vector3 explosionFxPos)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		Facepunch.Rust.Analytics.Azure.OnExplosion(this);
		Collider component = ((Component)this).GetComponent<Collider>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.enabled = false;
		}
		WaterLevel.WaterInfo waterInfo = WaterLevel.GetWaterInfo(explosionFxPos - new Vector3(0f, 0.25f, 0f), waves: true, volumes: true);
		if (underwaterExplosionEffect.isValid && waterInfo.isValid && waterInfo.currentDepth >= underwaterExplosionDepth)
		{
			Effect.server.Run(underwaterExplosionEffect.resourcePath, explosionFxPos, GetExplosionNormal(), null, broadcast: true);
		}
		else if (explosionEffect.isValid)
		{
			Vector3 val = explosionFxPos;
			if (explosionOffsetMode == ExplosionEffectOffsetMode.Local)
			{
				Vector3 val2 = ((Component)this).transform.TransformPoint(explosionEffectOffset) - ((Component)this).transform.position;
				val += val2;
			}
			if (explosionOffsetMode == ExplosionEffectOffsetMode.World)
			{
				val += explosionEffectOffset;
			}
			Effect.server.Run(explosionEffect.resourcePath, val, GetExplosionNormal(), null, broadcast: true);
		}
		if (watersurfaceExplosionEffect.isValid && waterInfo.isValid && waterInfo.overallDepth >= watersurfaceExplosionDepth.x && waterInfo.currentDepth <= watersurfaceExplosionDepth.y)
		{
			Effect.server.Run(watersurfaceExplosionEffect.resourcePath, Vector3Ex.WithY(explosionFxPos, waterInfo.surfaceLevel), GetExplosionNormal(), null, broadcast: true);
		}
		if (GetDamageList(null).Count > 0)
		{
			if (Interface.CallHook("OnTimedExplosiveExplode", this, explosionFxPos) != null)
			{
				return;
			}
			Vector3 val3 = ExplosionCenter();
			if (onlyDamageParent)
			{
				int num = 166144;
				if (!ConVar.AntiHack.explosive_vehicle_parent_damage_deployables && parentEntity.Get(serverside: true) is BaseVehicle)
				{
					num &= -257;
				}
				BaseEntity baseEntity = GetParentEntity();
				BaseCombatEntity baseCombatEntity = baseEntity as BaseCombatEntity;
				while ((Object)(object)baseCombatEntity == (Object)null && (Object)(object)baseEntity != (Object)null && baseEntity.HasParent())
				{
					baseEntity = baseEntity.GetParentEntity();
					baseCombatEntity = baseEntity as BaseCombatEntity;
				}
				DamageUtil.RadiusDamage(creatorEntity, LookupPrefab(), val3, minExplosionRadius, explosionRadius, GetDamageList(null), num, useLineOfSight: true, IgnoreAI, ignoreAttackingPlayer: false, extendedLineOfSight: false, playerDamage, removeWallpaper: false, includeBoatBuildingPieces: false, baseCombatEntity);
				if ((Object)(object)baseEntity == (Object)null || !GameObjectEx.IsOnLayer(((Component)baseEntity).gameObject, (Layer)21))
				{
					List<BuildingBlock> list = Pool.Get<List<BuildingBlock>>();
					Vis.Entities(val3, explosionRadius, list, 2097152, (QueryTriggerInteraction)1);
					BuildingBlock buildingBlock = null;
					float num2 = float.PositiveInfinity;
					foreach (BuildingBlock item in list)
					{
						if (!item.isClient && !item.IsDestroyed && !(item.healthFraction <= 0f))
						{
							float num3 = Vector3.Distance(item.ClosestPoint(val3), val3);
							if (num3 < num2 && item.IsVisible(val3, explosionRadius))
							{
								buildingBlock = item;
								num2 = num3;
							}
						}
					}
					if (Object.op_Implicit((Object)(object)buildingBlock))
					{
						HitInfo hitInfo = new HitInfo();
						hitInfo.Initiator = creatorEntity;
						hitInfo.WeaponPrefab = LookupPrefab();
						hitInfo.damageTypes.Add(GetDamageList(buildingBlock));
						hitInfo.PointStart = val3;
						hitInfo.PointEnd = ((Component)buildingBlock).transform.position;
						float amount = 1f - Mathf.Clamp01((num2 - minExplosionRadius) / (explosionRadius - minExplosionRadius));
						hitInfo.damageTypes.ScaleAll(amount);
						buildingBlock.Hurt(hitInfo);
					}
					Pool.FreeUnmanaged<BuildingBlock>(ref list);
				}
				if (Object.op_Implicit((Object)(object)baseCombatEntity))
				{
					HitInfo hitInfo2 = new HitInfo();
					hitInfo2.Initiator = creatorEntity;
					hitInfo2.WeaponPrefab = LookupPrefab();
					hitInfo2.damageTypes.Add(GetDamageList(baseEntity));
					baseCombatEntity.Hurt(hitInfo2);
				}
				else if ((Object)(object)baseEntity != (Object)null)
				{
					HitInfo hitInfo3 = new HitInfo();
					hitInfo3.Initiator = creatorEntity;
					hitInfo3.WeaponPrefab = LookupPrefab();
					hitInfo3.damageTypes.Add(GetDamageList(baseEntity));
					hitInfo3.PointStart = val3;
					hitInfo3.PointEnd = ((Component)baseEntity).transform.position;
					baseEntity.OnAttacked(hitInfo3);
				}
				if (splashWallpaperThroughWalls)
				{
					List<BuildingBlock> list2 = Pool.Get<List<BuildingBlock>>();
					Vis.Entities(val3, 3.4f, list2, 2097152, (QueryTriggerInteraction)1);
					foreach (BuildingBlock item2 in list2)
					{
						item2.RemoveWallpaper(0);
						item2.RemoveWallpaper(1);
					}
					Pool.FreeUnmanaged<BuildingBlock>(ref list2);
				}
			}
			else
			{
				DamageUtil.RadiusDamage(creatorEntity, LookupPrefab(), val3, minExplosionRadius, explosionRadius, GetDamageList(null), 1210222849, useLineOfSight: true, IgnoreAI, ignoreAttackingPlayer: false, extendedLineOfSight: false, playerDamage, splashWallpaperThroughWalls);
			}
			SeismicSensor.Notify(val3, vibrationLevel);
			BlindAnyAI();
			SingletonComponent<NpcNoiseManager>.Instance.OnExplosion(creatorEntity, this);
		}
		if (!base.IsDestroyed && !HasFlag(Flags.Broken) && !ConVar.Server.explosive_testing_mode)
		{
			Kill(DestroyMode.Gib);
		}
	}

	private Vector3 ExplosionCenter()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (IsStuck() && (parentEntity.Get(base.isServer) is BaseVehicle || parentEntity.Get(base.isServer) is BallistaGun))
		{
			OBB val = WorldSpaceBounds();
			return CenterPoint() - val.forward * (val.extents.z + 0.1f);
		}
		return CenterPoint();
	}

	private void BlindAnyAI()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (!BlindAI)
		{
			return;
		}
		int brainsInSphereFast = Query.Server.GetBrainsInSphereFast(((Component)this).transform.position, 10f, queryResults);
		for (int i = 0; i < brainsInSphereFast; i++)
		{
			BaseEntity baseEntity = queryResults[i];
			if (Vector3.Distance(((Component)this).transform.position, ((Component)baseEntity).transform.position) > aiBlindRange)
			{
				continue;
			}
			BaseAIBrain component = ((Component)baseEntity).GetComponent<BaseAIBrain>();
			if (!((Object)(object)component == (Object)null))
			{
				BaseEntity brainBaseEntity = component.GetBrainBaseEntity();
				if (!((Object)(object)brainBaseEntity == (Object)null) && brainBaseEntity.IsVisible(CenterPoint()))
				{
					float blinded = aiBlindDuration * component.BlindDurationMultiplier * Random.Range(0.6f, 1.4f);
					component.SetBlinded(blinded);
					queryResults[i] = null;
				}
			}
		}
	}

	public void FixedUpdate()
	{
		CheckClippingThroughWalls();
	}

	protected virtual bool ShouldBypassClippingWallCheck()
	{
		return false;
	}

	private void CheckClippingThroughWalls()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		if (!canStick && !forceRunClippingChecks)
		{
			return;
		}
		if (lastPosition == default(Vector3) || !parentEntity.IsValid(serverside: true))
		{
			lastPosition = CenterPoint();
			return;
		}
		Vector3 val = lastPosition;
		Vector3 val2 = CenterPoint();
		Vector3 val3 = val2 - val;
		lastPosition = val2;
		if (val == val2 || !IsStuck(bypassColliderCheck: true) || ShouldBypassClippingWallCheck())
		{
			return;
		}
		Ray ray = default(Ray);
		((Ray)(ref ray))._002Ector(val, val2 - val);
		PooledList<RaycastHit> val4 = Pool.Get<PooledList<RaycastHit>>();
		try
		{
			GamePhysics.TraceAll(ray, 0f, (List<RaycastHit>)(object)val4, Vector3.Distance(val2, val), 2097152, (QueryTriggerInteraction)0);
			foreach (RaycastHit item in (List<RaycastHit>)(object)val4)
			{
				if ((Object)(object)(RaycastHitEx.GetEntity(item) as BuildingBlock) != (Object)null)
				{
					Transform transform = ((Component)this).transform;
					transform.position -= val3;
					ForceExplode();
					break;
				}
			}
		}
		finally
		{
			((IDisposable)val4)?.Dispose();
		}
	}

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (canStick && !IsStuck())
		{
			bool flag = true;
			if (Object.op_Implicit((Object)(object)hitEntity))
			{
				flag = CanStickTo(hitEntity);
				if (!flag)
				{
					Collider component = ((Component)this).GetComponent<Collider>();
					if ((Object)(object)collision.collider != (Object)null && (Object)(object)component != (Object)null)
					{
						Physics.IgnoreCollision(collision.collider, component);
					}
				}
			}
			if (flag)
			{
				DoCollisionStick(collision, hitEntity);
			}
		}
		if (explodeOnContact && !IsBusy())
		{
			hitNormal = ((ContactPoint)(ref collision.contacts[0])).normal;
			SetMotionEnabled(wantsMotion: false);
			SetFlagLocal(Flags.Busy, b: true);
			Invoke(Explode, 0.015f);
		}
		else
		{
			DoBounceEffect();
		}
	}

	public virtual bool CanStickTo(BaseEntity entity)
	{
		object obj = Interface.CallHook("CanExplosiveStick", this, entity);
		if (obj is bool)
		{
			return (bool)obj;
		}
		DecorDeployable decorDeployable = default(DecorDeployable);
		if (((Component)entity).TryGetComponent<DecorDeployable>(ref decorDeployable))
		{
			return false;
		}
		if (entity is Drone)
		{
			return false;
		}
		if (entity is TravellingVendor)
		{
			return false;
		}
		return true;
	}

	private void DoBounceEffect()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (!bounceEffect.isValid || Time.time - lastBounceTime < 0.2f)
		{
			return;
		}
		Rigidbody component = ((Component)this).GetComponent<Rigidbody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Vector3 linearVelocity = component.linearVelocity;
			if (((Vector3)(ref linearVelocity)).magnitude < 1f)
			{
				return;
			}
		}
		if (bounceEffect.isValid)
		{
			Effect.server.Run(bounceEffect.resourcePath, ((Component)this).transform.position, Vector3.up, null, broadcast: true);
		}
		lastBounceTime = Time.time;
	}

	private void DoCollisionStick(Collision collision, BaseEntity ent)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		ContactPoint contact = collision.GetContact(0);
		DoStick(((ContactPoint)(ref contact)).point, ((ContactPoint)(ref contact)).normal, ent, collision.collider);
	}

	public virtual void SetMotionEnabled(bool wantsMotion)
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody component = ((Component)this).GetComponent<Rigidbody>();
		if (wantsMotion)
		{
			if ((Object)(object)component == (Object)null && hadRB)
			{
				component = ((Component)this).gameObject.AddComponent<Rigidbody>();
				component.mass = rbMass;
				component.linearDamping = rbDrag;
				component.angularDamping = rbAngularDrag;
				component.collisionDetectionMode = rbCollisionMode;
				component.useGravity = true;
				component.isKinematic = false;
			}
		}
		else if ((Object)(object)component != (Object)null)
		{
			hadRB = true;
			rbMass = component.mass;
			rbDrag = component.linearDamping;
			rbAngularDrag = component.angularDamping;
			rbCollisionMode = component.collisionDetectionMode;
			rbVelocityBeforeCollision = component.linearVelocity;
			Object.Destroy((Object)(object)component);
		}
	}

	public bool IsStuck(bool bypassColliderCheck = false)
	{
		Rigidbody component = ((Component)this).GetComponent<Rigidbody>();
		if (Object.op_Implicit((Object)(object)component) && !component.isKinematic)
		{
			return false;
		}
		if (!bypassColliderCheck)
		{
			Collider component2 = ((Component)this).GetComponent<Collider>();
			if (Object.op_Implicit((Object)(object)component2) && component2.enabled)
			{
				return false;
			}
		}
		return parentEntity.IsValid(serverside: true);
	}

	public void DoStick(Vector3 position, Vector3 normal, BaseEntity ent, Collider collider)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ent == (Object)null)
		{
			return;
		}
		if (ent is TimedExplosive)
		{
			if (!ent.HasParent())
			{
				return;
			}
			position = ((Component)ent).transform.position;
			ent = ent.parentEntity.Get(serverside: true);
		}
		SetMotionEnabled(wantsMotion: false);
		if (!HasChild(ent))
		{
			((Component)this).transform.position = position;
			((Component)this).transform.rotation = Quaternion.LookRotation(normal, ((Component)this).transform.up);
			if ((Object)(object)collider != (Object)null)
			{
				SetParent(ent, ent.FindBoneID(((Component)collider).transform), worldPositionStays: true);
			}
			else
			{
				SetParent(ent, StringPool.closest, worldPositionStays: true);
			}
			if (ent is BaseCombatEntity baseCombatEntity)
			{
				baseCombatEntity.SetJustAttacked();
			}
			if (stickEffect.isValid)
			{
				Effect.server.Run(stickEffect.resourcePath, ((Component)this).transform.position, Vector3.up, null, broadcast: true);
			}
			ReceiveCollisionMessages(b: false);
		}
	}

	public void UnStick()
	{
		if (Object.op_Implicit((Object)(object)GetParentEntity()))
		{
			SetParent(null, worldPositionStays: true, sendImmediate: true);
			SetMotionEnabled(wantsMotion: true);
			if (ComponentEx.HasComponent<Collider>((Component)(object)((Component)this).transform))
			{
				ReceiveCollisionMessages(b: true);
			}
		}
	}

	internal override void OnParentRemoved()
	{
		UnStick();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
	}

	public override void PostServerLoad()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base.PostServerLoad();
		if (parentEntity.IsValid(serverside: true))
		{
			DoStick(((Component)this).transform.position, ((Component)this).transform.forward, parentEntity.Get(serverside: true), null);
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.explosive != null)
		{
			parentEntity.uid = info.msg.explosive.parentid;
		}
	}

	public virtual void SetCollisionEnabled(bool wantsCollision)
	{
		Collider component = ((Component)this).GetComponent<Collider>();
		if (Object.op_Implicit((Object)(object)component) && component.enabled != wantsCollision)
		{
			component.enabled = wantsCollision;
		}
	}

	public TimedExplosive()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		timerAmountMin = 10f;
		timerAmountMax = 20f;
		explosionRadius = 10f;
		aiBlindDuration = 2.5f;
		aiBlindRange = 4f;
		explosionEffectOffset = Vector3.zero;
		underwaterExplosionDepth = 1f;
		watersurfaceExplosionDepth = new MinMax(0.5f, 10f);
		vibrationLevel = 3;
		damageTypes = new List<DamageTypeEntry>();
		playerDamage = new List<DamageTypeEntry>();
		lastPosition = Vector3.zero;
		base._002Ector();
	}
}
