using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust;
using UnityEngine;

public class ThrownBoomerangServerProjectile : ServerProjectile
{
	public DamageProperties damageProperties;

	public List<DamageTypeEntry> damageTypes = new List<DamageTypeEntry>();

	public float worldAttackRadius;

	private Vector3 startPosition;

	private bool willKill;

	public void ProjectileHandleMovement(bool state)
	{
		shouldMoveProjectile = state;
	}

	public void SetStartPosition(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		startPosition = position;
	}

	public void CalculateDamage(HitInfo info, float scale)
	{
		foreach (DamageTypeEntry damageType in damageTypes)
		{
			info.damageTypes.Add(damageType.type, damageType.amount * scale);
		}
		if (Global.developer > 0)
		{
			Debug.Log((object)(" Projectile damage: " + info.damageTypes.Total() + " (scalar=" + scale + ")"));
		}
	}

	protected override bool AutomaticallyRotate()
	{
		return false;
	}

	protected override void OnHit(RaycastHit rayHit, BaseEntity hitEntity)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		base.OnHit(rayHit, hitEntity);
		willKill = true;
		HitInfo hitInfo = new HitInfo();
		hitInfo.Initiator = base.baseEntity.creatorEntity;
		hitInfo.WeaponPrefab = base.baseEntity;
		hitInfo.IsPredicting = false;
		hitInfo.DoDecals = true;
		hitInfo.DoHitEffects = true;
		hitInfo.DidHit = true;
		hitInfo.HitPositionWorld = ((RaycastHit)(ref rayHit)).point;
		hitInfo.HitNormalWorld = ((RaycastHit)(ref rayHit)).normal;
		hitInfo.ProjectileVelocity = base.CurrentVelocity;
		hitInfo.PointStart = startPosition;
		hitInfo.PointEnd = ((RaycastHit)(ref rayHit)).point;
		hitInfo.damageProperties = damageProperties;
		CalculateDamage(hitInfo, 1f);
		hitInfo.HitMaterial = StringPool.Get(GetMaterialName(rayHit));
		ThrownBoomerang obj = base.baseEntity as ThrownBoomerang;
		obj.OnHit();
		if (hitEntity.IsValid())
		{
			hitInfo.HitEntity = hitEntity;
			hitInfo.HitPositionLocal = ((Component)hitInfo.HitEntity).transform.InverseTransformPoint(hitInfo.HitPositionWorld);
			hitInfo.HitNormalLocal = ((Component)hitInfo.HitEntity).transform.InverseTransformDirection(hitInfo.HitNormalWorld);
			Shield shield = hitInfo.HitEntity as Shield;
			if (hitInfo.HitEntity is BasePlayer || hitInfo.HitEntity is BaseNpc || (Object)(object)shield != (Object)null)
			{
				hitInfo.HitMaterial = StringPool.Get(((Object)(object)shield != (Object)null) ? shield.GetHitMaterialString() : "Flesh");
			}
			if (!(hitInfo.HitEntity is BasePlayer) && !(hitInfo.HitEntity is BaseNpc))
			{
				hitInfo.damageTypes.ScaleAll(0.03f);
			}
			hitInfo.HitEntity.OnAttacked(hitInfo);
		}
		Vector3 currentVelocity = base.CurrentVelocity;
		obj.CreateWorldModel(hitInfo, ((Vector3)(ref currentVelocity)).normalized);
		Effect.server.ImpactEffect(hitInfo);
	}

	protected override bool DoHitDetection(Vector3 velocityToUse, float distance)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
		List<RaycastHit> list2 = Pool.Get<List<RaycastHit>>();
		Vector3 position = ((Component)this).transform.position;
		GamePhysics.TraceAll(new Ray(position, ((Vector3)(ref velocityToUse)).normalized), radius, list, distance + scanRange, mask, (QueryTriggerInteraction)1);
		GamePhysics.TraceAll(new Ray(position, ((Vector3)(ref velocityToUse)).normalized), worldAttackRadius, list2, distance + scanRange, mask, (QueryTriggerInteraction)1);
		foreach (RaycastHit item in list)
		{
			RaycastHit current = item;
			BaseEntity entity = RaycastHitEx.GetEntity(current);
			if ((!((Object)(object)entity != (Object)null) || !entity.isClient) && (!IgnoreAI || !IsAnIgnoredAI(entity)) && (entity is BasePlayer || entity is BaseNpc) && IsAValidHit(entity) && GamePhysics.LineOfSight(((Component)this).transform.position, ((RaycastHit)(ref current)).point, mask, 0f))
			{
				ProcessHit(current, entity, position);
				Pool.FreeUnmanaged<RaycastHit>(ref list);
				Pool.FreeUnmanaged<RaycastHit>(ref list2);
				return true;
			}
		}
		foreach (RaycastHit item2 in list2)
		{
			BaseEntity entity2 = RaycastHitEx.GetEntity(item2);
			if ((!((Object)(object)entity2 != (Object)null) || !entity2.isClient) && (!IgnoreAI || !IsAnIgnoredAI(entity2)) && IsAValidHit(entity2) && IsShootable(item2))
			{
				ProcessHit(item2, entity2, position);
				Pool.FreeUnmanaged<RaycastHit>(ref list);
				Pool.FreeUnmanaged<RaycastHit>(ref list2);
				return true;
			}
		}
		Pool.FreeUnmanaged<RaycastHit>(ref list);
		Pool.FreeUnmanaged<RaycastHit>(ref list2);
		return false;
	}

	protected override void PostDoMove()
	{
		if (willKill)
		{
			base.baseEntity.Kill();
		}
	}

	protected override bool IsAValidHit(BaseEntity hitEnt)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)hitEnt != (Object)null)
		{
			if (base.baseEntity.creatorEntity.IsValid() && hitEnt.net.ID == base.baseEntity.creatorEntity.net.ID)
			{
				return false;
			}
			if (ignoreEntity.IsValid() && hitEnt.net.ID == ignoreEntity.net.ID)
			{
				return false;
			}
		}
		return true;
	}

	private string GetMaterialName(RaycastHit rayHit)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		string result = "generic";
		if ((Object)(object)RaycastHitEx.GetCollider(rayHit) != (Object)null && (Object)(object)RaycastHitEx.GetCollider(rayHit).sharedMaterial != (Object)null)
		{
			result = ((Object)RaycastHitEx.GetCollider(rayHit).sharedMaterial).name;
		}
		if (RaycastHitEx.IsWaterHit(rayHit))
		{
			result = "Water";
		}
		return result;
	}
}
