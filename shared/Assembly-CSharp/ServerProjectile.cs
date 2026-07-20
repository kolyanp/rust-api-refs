using System;
using System.Collections.Generic;
using Facepunch;
using Rust.Ai.Gen2;
using UnityEngine;

public class ServerProjectile : EntityComponent<BaseEntity>
{
	public interface IProjectileImpact
	{
		void ProjectileImpact(RaycastHit hitInfo, Vector3 rayOrigin);
	}

	public Vector3 initialVelocity;

	public float drag;

	public float gravityModifier = 1f;

	public float speed = 15f;

	public float scanRange;

	public Vector3 swimScale;

	public Vector3 swimSpeed;

	public float radius;

	public bool IgnoreAI;

	public bool runSpawnHitChecks;

	[HideInInspector]
	public BaseEntity ignoreEntity;

	protected bool shouldMoveProjectile = true;

	public bool impacted;

	public float swimRandom;

	public virtual bool HasRangeLimit => true;

	protected virtual int mask => 1237003025;

	public Vector3 CurrentVelocity { get; set; }

	public bool Impacted => impacted;

	public float GetMaxRange(float maxFuseTime)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (gravityModifier == 0f)
		{
			return float.PositiveInfinity;
		}
		float num = Mathf.Sin(MathF.PI / 2f) * speed * speed / (0f - Physics.gravity.y * gravityModifier);
		float num2 = speed * maxFuseTime;
		return Mathf.Min(num, num2);
	}

	protected void FixedUpdate()
	{
		if ((Object)(object)base.baseEntity != (Object)null && base.baseEntity.isServer && base.baseEntity.IsFullySpawned())
		{
			DoMovement();
		}
	}

	public override void ServerInitPostNetworkGroupAssign()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInitPostNetworkGroupAssign();
		if (runSpawnHitChecks)
		{
			Vector3 currentVelocity = CurrentVelocity;
			CheckSpawnedInsideCollider(((Vector3)(ref currentVelocity)).normalized);
		}
	}

	private void CheckSpawnedInsideCollider(Vector3 direction)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapSphere(((Component)this).transform.position, Mathf.Max(radius, 0.01f), list, mask, (QueryTriggerInteraction)1);
		bool num = list.Count > 0;
		Pool.FreeUnmanaged<Collider>(ref list);
		if (!num)
		{
			return;
		}
		List<RaycastHit> list2 = Pool.Get<List<RaycastHit>>();
		GamePhysics.TraceAll(new Ray(((Component)this).transform.position, -direction), radius, list2, 1f, mask, (QueryTriggerInteraction)1);
		foreach (RaycastHit item in list2)
		{
			BaseEntity entity = RaycastHitEx.GetEntity(item);
			if ((!((Object)(object)entity != (Object)null) || !entity.isClient) && (!IgnoreAI || !IsAnIgnoredAI(entity)) && IsAValidHit(entity) && IsShootable(item))
			{
				Pool.FreeUnmanaged<RaycastHit>(ref list2);
				ProcessHit(item, entity, ((Component)this).transform.position);
				return;
			}
		}
		Pool.FreeUnmanaged<RaycastHit>(ref list2);
	}

	public void AdjustVelocity(Vector3 adjustment)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		CurrentVelocity += adjustment;
	}

	public virtual Vector3 GetVelocityStep()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return Physics.gravity * gravityModifier * Time.fixedDeltaTime * Time.timeScale;
	}

	public virtual void InitializeVelocity(Vector3 overrideVel)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (AutomaticallyRotate())
		{
			((Component)this).transform.rotation = Quaternion.LookRotation(((Vector3)(ref overrideVel)).normalized);
		}
		initialVelocity = overrideVel;
		CurrentVelocity = overrideVel;
	}

	public void SetVelocity(Vector3 overrideVel)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (AutomaticallyRotate() && overrideVel != Vector3.zero)
		{
			((Component)this).transform.rotation = Quaternion.LookRotation(((Vector3)(ref overrideVel)).normalized);
		}
		CurrentVelocity = overrideVel;
	}

	public virtual bool DoMovement()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (base.baseEntity.isClient)
		{
			return false;
		}
		if (impacted)
		{
			return false;
		}
		CurrentVelocity += GetVelocityStep();
		Vector3 val = AddSwim(CurrentVelocity);
		float num = ((Vector3)(ref val)).magnitude * Time.fixedDeltaTime;
		if (DoHitDetection(val, num))
		{
			return false;
		}
		if (shouldMoveProjectile)
		{
			Transform transform = ((Component)this).transform;
			transform.position += ((Component)this).transform.forward * num;
		}
		if (AutomaticallyRotate() && val != Vector3.zero)
		{
			((Component)this).transform.rotation = Quaternion.LookRotation(((Vector3)(ref val)).normalized);
		}
		PostDoMove();
		return true;
	}

	protected virtual bool DoHitDetection(Vector3 velocityToUse, float distance)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
		Vector3 position = ((Component)this).transform.position;
		GamePhysics.TraceAll(new Ray(position, ((Vector3)(ref velocityToUse)).normalized), radius, list, distance + scanRange, mask, (QueryTriggerInteraction)1);
		foreach (RaycastHit item in list)
		{
			BaseEntity entity = RaycastHitEx.GetEntity(item);
			if ((!((Object)(object)entity != (Object)null) || !entity.isClient) && (!IgnoreAI || !IsAnIgnoredAI(entity)) && IsAValidHit(entity) && IsShootable(item))
			{
				ProcessHit(item, entity, position);
				Pool.FreeUnmanaged<RaycastHit>(ref list);
				return true;
			}
		}
		Pool.FreeUnmanaged<RaycastHit>(ref list);
		return false;
	}

	private Vector3 AddSwim(Vector3 currentVelocity)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = currentVelocity;
		if (swimScale != Vector3.zero)
		{
			if (swimRandom == 0f)
			{
				swimRandom = Random.Range(0f, 20f);
			}
			float num = Time.time + swimRandom;
			Vector3 val2 = default(Vector3);
			((Vector3)(ref val2))._002Ector(Mathf.Sin(num * swimSpeed.x) * swimScale.x, Mathf.Cos(num * swimSpeed.y) * swimScale.y, Mathf.Sin(num * swimSpeed.z) * swimScale.z);
			val2 = ((Component)this).transform.InverseTransformDirection(val2);
			val += val2;
		}
		return val;
	}

	protected void ProcessHit(RaycastHit hitInfo, BaseEntity hitEnt, Vector3 rayOrigin)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		transform.position += ((Component)this).transform.forward * Mathf.Max(0f, ((RaycastHit)(ref hitInfo)).distance - 0.1f);
		((Component)this).GetComponent<IProjectileImpact>()?.ProjectileImpact(hitInfo, rayOrigin);
		SingletonComponent<NpcNoiseManager>.Instance.OnServerProjectileHit(base.baseEntity, this, hitInfo);
		impacted = true;
		OnHit(hitInfo, hitEnt);
		PostDoMove();
	}

	protected bool IsShootable(RaycastHit hitInfo)
	{
		ColliderInfo colliderInfo = (((Object)(object)((RaycastHit)(ref hitInfo)).collider != (Object)null) ? ((Component)((RaycastHit)(ref hitInfo)).collider).GetComponent<ColliderInfo>() : null);
		if (!((Object)(object)colliderInfo == (Object)null))
		{
			return colliderInfo.HasFlag(ColliderInfo.Flags.Shootable);
		}
		return true;
	}

	protected virtual void OnHit(RaycastHit rayHit, BaseEntity hitEntity)
	{
	}

	protected virtual void PostDoMove()
	{
	}

	protected virtual bool IsAValidHit(BaseEntity hitEnt)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (!hitEnt.IsValid())
		{
			return true;
		}
		if (base.baseEntity.creatorEntity.IsValid() && hitEnt.net.ID == base.baseEntity.creatorEntity.net.ID)
		{
			return false;
		}
		if (ignoreEntity.IsValid() && hitEnt.net.ID == ignoreEntity.net.ID)
		{
			return false;
		}
		return true;
	}

	protected virtual bool IsAnIgnoredAI(BaseEntity hitEnt)
	{
		return hitEnt is ScientistNPC;
	}

	protected virtual bool AutomaticallyRotate()
	{
		return true;
	}
}
