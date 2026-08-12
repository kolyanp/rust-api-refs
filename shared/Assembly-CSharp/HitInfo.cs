using System;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;

public class HitInfo : IPooled, IDisposable
{
	public BaseEntity Initiator;

	public BaseEntity WeaponPrefab;

	public AttackEntity Weapon;

	public bool DoHitEffects;

	public bool DoDecals;

	public bool IsPredicting;

	public bool UseProtection;

	public bool UseProtectionForNPCs;

	public Connection Predicted;

	public bool DidHit;

	public BaseEntity HitEntity;

	public uint HitBone;

	public uint HitPart;

	public uint HitMaterial;

	public Vector3 HitPositionWorld;

	public Vector3 HitPositionLocal;

	public Vector3 HitNormalWorld;

	public Vector3 HitNormalLocal;

	public Vector3 PointStart;

	public Vector3 PointEnd;

	public int ProjectileID;

	public int ProjectileHits;

	public float ProjectileDistance;

	public float ProjectileIntegrity;

	public float ProjectileTravelTime;

	public float ProjectileTrajectoryMismatch;

	public Vector3 ProjectileVelocity;

	public Projectile ProjectilePrefab;

	public PhysicsMaterial material;

	public DamageProperties damageProperties;

	public DamageTypeList damageTypes;

	public bool CanGather;

	public bool DidGather;

	public float gatherScale;

	public BasePlayer InitiatorPlayer
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)Initiator))
			{
				return null;
			}
			return Initiator.ToPlayer();
		}
	}

	public Vector3 attackNormal
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = PointEnd - PointStart;
			return ((Vector3)(ref val)).normalized;
		}
	}

	public bool hasDamage => damageTypes.Total() > 0f;

	public bool InitiatorParented
	{
		get
		{
			if ((Object)(object)Initiator != (Object)null && (Object)(object)Initiator.GetParentEntity() != (Object)null)
			{
				return Initiator.GetParentEntity().IsValid();
			}
			return false;
		}
	}

	public bool HitEntityParented
	{
		get
		{
			if ((Object)(object)HitEntity != (Object)null && (Object)(object)HitEntity.GetParentEntity() != (Object)null)
			{
				return HitEntity.GetParentEntity().IsValid();
			}
			return false;
		}
	}

	public bool isHeadshot
	{
		get
		{
			if ((Object)(object)HitEntity == (Object)null)
			{
				return false;
			}
			BaseCombatEntity baseCombatEntity = HitEntity as BaseCombatEntity;
			if ((Object)(object)baseCombatEntity == (Object)null)
			{
				return false;
			}
			if ((Object)(object)baseCombatEntity.skeletonProperties == (Object)null)
			{
				return false;
			}
			SkeletonProperties.BoneProperty boneProperty = baseCombatEntity.skeletonProperties.FindBone(HitBone);
			if (boneProperty == null)
			{
				return false;
			}
			return boneProperty.area == HitArea.Head;
		}
	}

	public string boneName
	{
		get
		{
			if ((Object)(object)HitEntity == (Object)null)
			{
				return null;
			}
			BaseCombatEntity baseCombatEntity = HitEntity as BaseCombatEntity;
			if ((Object)(object)baseCombatEntity == (Object)null)
			{
				return null;
			}
			if ((Object)(object)baseCombatEntity.skeletonProperties == (Object)null)
			{
				return null;
			}
			return baseCombatEntity.skeletonProperties.FindBone(HitBone)?.boneName;
		}
	}

	public HitArea boneArea
	{
		get
		{
			if ((Object)(object)HitEntity == (Object)null)
			{
				return (HitArea)(-1);
			}
			BaseCombatEntity baseCombatEntity = HitEntity as BaseCombatEntity;
			if ((Object)(object)baseCombatEntity == (Object)null)
			{
				return (HitArea)(-1);
			}
			return baseCombatEntity.SkeletonLookup(HitBone);
		}
	}

	void IPooled.EnterPool()
	{
		Clear();
	}

	void IPooled.LeavePool()
	{
	}

	void IDisposable.Dispose()
	{
		HitInfo hitInfo = this;
		Pool.Free<HitInfo>(ref hitInfo);
	}

	public void Clear()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		Initiator = null;
		WeaponPrefab = null;
		Weapon = null;
		DoHitEffects = true;
		DoDecals = true;
		IsPredicting = false;
		UseProtection = true;
		UseProtectionForNPCs = false;
		Predicted = null;
		DidHit = false;
		HitEntity = null;
		HitBone = 0u;
		HitPart = 0u;
		HitMaterial = 0u;
		HitPositionWorld = default(Vector3);
		HitPositionLocal = default(Vector3);
		HitNormalWorld = default(Vector3);
		HitNormalLocal = default(Vector3);
		PointStart = default(Vector3);
		PointEnd = default(Vector3);
		ProjectileID = 0;
		ProjectileHits = 0;
		ProjectileDistance = 0f;
		ProjectileIntegrity = 0f;
		ProjectileTravelTime = 0f;
		ProjectileTrajectoryMismatch = 0f;
		ProjectileVelocity = default(Vector3);
		ProjectilePrefab = null;
		material = null;
		damageProperties = null;
		damageTypes.Clear();
		CanGather = false;
		DidGather = false;
		gatherScale = 1f;
	}

	public void CopyFrom(HitInfo other)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		Initiator = other.Initiator;
		WeaponPrefab = other.WeaponPrefab;
		Weapon = other.Weapon;
		DoHitEffects = other.DoHitEffects;
		DoDecals = other.DoDecals;
		IsPredicting = other.IsPredicting;
		UseProtection = other.UseProtection;
		UseProtectionForNPCs = other.UseProtectionForNPCs;
		Predicted = other.Predicted;
		DidHit = other.DidHit;
		HitEntity = other.HitEntity;
		HitBone = other.HitBone;
		HitPart = other.HitPart;
		HitMaterial = other.HitMaterial;
		HitPositionWorld = other.HitPositionWorld;
		HitPositionLocal = other.HitPositionLocal;
		HitNormalWorld = other.HitNormalWorld;
		HitNormalLocal = other.HitNormalLocal;
		PointStart = other.PointStart;
		PointEnd = other.PointEnd;
		ProjectileID = other.ProjectileID;
		ProjectileHits = other.ProjectileHits;
		ProjectileDistance = other.ProjectileDistance;
		ProjectileIntegrity = other.ProjectileIntegrity;
		ProjectileTravelTime = other.ProjectileTravelTime;
		ProjectileTrajectoryMismatch = other.ProjectileTrajectoryMismatch;
		ProjectileVelocity = other.ProjectileVelocity;
		ProjectilePrefab = other.ProjectilePrefab;
		material = other.material;
		damageProperties = other.damageProperties;
		for (int i = 0; i < damageTypes.types.Length; i++)
		{
			damageTypes.types[i] = other.damageTypes.types[i];
		}
		CanGather = other.CanGather;
		DidGather = other.DidGather;
		gatherScale = other.gatherScale;
	}

	public bool IsProjectile()
	{
		return ProjectileID != 0;
	}

	public void Init(BaseEntity attacker, BaseEntity target, DamageType type, float damageAmount, Vector3 vhitPosition)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Initiator = attacker;
		HitEntity = target;
		HitPositionWorld = vhitPosition;
		if ((Object)(object)attacker != (Object)null)
		{
			PointStart = ((Component)attacker).transform.position;
		}
		damageTypes.Add(type, damageAmount);
	}

	public HitInfo()
	{
		DoHitEffects = true;
		DoDecals = true;
		UseProtection = true;
		damageTypes = new DamageTypeList();
		gatherScale = 1f;
		base._002Ector();
	}

	public HitInfo(BaseEntity attacker, BaseEntity target, DamageType type, float damageAmount, Vector3 vhitPosition)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		DoHitEffects = true;
		DoDecals = true;
		UseProtection = true;
		damageTypes = new DamageTypeList();
		gatherScale = 1f;
		base._002Ector();
		Init(attacker, target, type, damageAmount, vhitPosition);
	}

	public HitInfo(BaseEntity attacker, BaseEntity target, DamageType type, float damageAmount)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		DoHitEffects = true;
		DoDecals = true;
		UseProtection = true;
		damageTypes = new DamageTypeList();
		gatherScale = 1f;
		base._002Ector();
		Init(attacker, target, type, damageAmount, ((Component)target).transform.position);
	}

	public void LoadFromAttack(Attack attack, bool serverSide)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		HitEntity = null;
		PointStart = attack.pointStart;
		PointEnd = attack.pointEnd;
		if (((NetworkableId)(ref attack.hitID)).IsValid)
		{
			DidHit = true;
			if (serverSide)
			{
				HitEntity = BaseNetworkable.serverEntities.Find(attack.hitID) as BaseEntity;
			}
			if (Object.op_Implicit((Object)(object)HitEntity))
			{
				HitBone = attack.hitBone;
				HitPart = attack.hitPartID;
			}
		}
		DidHit = true;
		HitPositionLocal = attack.hitPositionLocal;
		HitPositionWorld = attack.hitPositionWorld;
		HitNormalLocal = ((Vector3)(ref attack.hitNormalLocal)).normalized;
		HitNormalWorld = ((Vector3)(ref attack.hitNormalWorld)).normalized;
		HitMaterial = attack.hitMaterialID;
		if (((NetworkableId)(ref attack.srcParentID)).IsValid)
		{
			BaseEntity baseEntity = null;
			if (serverSide)
			{
				baseEntity = BaseNetworkable.serverEntities.Find(attack.srcParentID) as BaseEntity;
			}
			if (baseEntity.IsValid())
			{
				PointStart = ((Component)baseEntity).transform.TransformPoint(PointStart);
			}
		}
		if (((NetworkableId)(ref attack.dstParentID)).IsValid)
		{
			BaseEntity baseEntity2 = null;
			if (serverSide)
			{
				baseEntity2 = BaseNetworkable.serverEntities.Find(attack.dstParentID) as BaseEntity;
			}
			if (baseEntity2.IsValid())
			{
				PointEnd = ((Component)baseEntity2).transform.TransformPoint(PointEnd);
				HitPositionWorld = ((Component)baseEntity2).transform.TransformPoint(HitPositionWorld);
				HitNormalWorld = ((Component)baseEntity2).transform.TransformDirection(HitNormalWorld);
			}
		}
	}

	public Vector3 PositionOnRay(Vector3 position)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Ray val = default(Ray);
		((Ray)(ref val))._002Ector(PointStart, attackNormal);
		if ((Object)(object)ProjectilePrefab == (Object)null)
		{
			return RayEx.ClosestPoint(val, position);
		}
		Sphere val2 = default(Sphere);
		((Sphere)(ref val2))._002Ector(position, ProjectilePrefab.thickness);
		RaycastHit val3 = default(RaycastHit);
		if (((Sphere)(ref val2)).Trace(val, ref val3, float.PositiveInfinity))
		{
			return ((RaycastHit)(ref val3)).point;
		}
		return position;
	}

	public Vector3 HitPositionOnRay()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return PositionOnRay(HitPositionWorld);
	}

	public bool IsNaNOrInfinity()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3Ex.IsNaNOrInfinity(PointStart))
		{
			return true;
		}
		if (Vector3Ex.IsNaNOrInfinity(PointEnd))
		{
			return true;
		}
		if (Vector3Ex.IsNaNOrInfinity(HitPositionWorld))
		{
			return true;
		}
		if (Vector3Ex.IsNaNOrInfinity(HitPositionLocal))
		{
			return true;
		}
		if (Vector3Ex.IsNaNOrInfinity(HitNormalWorld))
		{
			return true;
		}
		if (Vector3Ex.IsNaNOrInfinity(HitNormalLocal))
		{
			return true;
		}
		if (Vector3Ex.IsNaNOrInfinity(ProjectileVelocity))
		{
			return true;
		}
		if (float.IsNaN(ProjectileDistance))
		{
			return true;
		}
		if (float.IsInfinity(ProjectileDistance))
		{
			return true;
		}
		if (float.IsNaN(ProjectileIntegrity))
		{
			return true;
		}
		if (float.IsInfinity(ProjectileIntegrity))
		{
			return true;
		}
		if (float.IsNaN(ProjectileTravelTime))
		{
			return true;
		}
		if (float.IsInfinity(ProjectileTravelTime))
		{
			return true;
		}
		if (float.IsNaN(ProjectileTrajectoryMismatch))
		{
			return true;
		}
		if (float.IsInfinity(ProjectileTrajectoryMismatch))
		{
			return true;
		}
		return false;
	}
}
