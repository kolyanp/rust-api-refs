using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust;
using UnityEngine;

public class TriggerHurtNotChild : TriggerBase, IServerComponent, IHurtTrigger
{
	public interface IHurtTriggerUser
	{
		BasePlayer GetPlayerDamageInitiator();

		float GetDamageMultiplier(BaseEntity ent);

		void OnHurtTriggerOccupant(BaseEntity hurtEntity, DamageType damageType, float damageTotal);
	}

	public float DamagePerSecond = 1f;

	public float DamageTickRate = 4f;

	public float DamageDelay;

	public DamageType damageType;

	public bool ignoreNPC = true;

	public float npcMultiplier = 1f;

	public float resourceMultiplier = 1f;

	public float lootContainerMultiplier = 1f;

	public bool triggerHitImpacts = true;

	public bool RequireUpAxis;

	public BaseEntity SourceEntity;

	public bool UseSourceEntityDamageMultiplier = true;

	public bool ignoreAllVehicleMounted;

	public float activationDelay;

	public bool ScaleWithDistToTriggerCenter;

	public Collider triggerCollider;

	public float DamageMultAtCenter = 1f;

	private Dictionary<BaseEntity, float> entryTimes;

	private TimeSince timeSinceAcivation;

	private IHurtTriggerUser hurtTiggerUser;

	public void SetSourceEntity(BaseEntity source)
	{
		SourceEntity = source;
		hurtTiggerUser = source as IHurtTriggerUser;
	}

	public void ClearSourceEntity()
	{
		SourceEntity = null;
		hurtTiggerUser = null;
	}

	public override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if ((Object)(object)obj == (Object)null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if ((Object)(object)baseEntity == (Object)null)
		{
			return null;
		}
		if (baseEntity.isClient)
		{
			return null;
		}
		if (ignoreNPC && baseEntity.IsNpc)
		{
			return null;
		}
		if (!Physics.treecollision && obj.layer == 30)
		{
			return null;
		}
		return ((Component)baseEntity).gameObject;
	}

	public override void OnObjects()
	{
		InvokeRepeating(OnTick, 0f, 1f / DamageTickRate);
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		if ((Object)(object)ent != (Object)null && DamageDelay > 0f)
		{
			if (entryTimes == null)
			{
				entryTimes = new Dictionary<BaseEntity, float>();
			}
			entryTimes.Add(ent, Time.time);
		}
	}

	internal override void OnEntityLeave(BaseEntity ent)
	{
		if ((Object)(object)ent != (Object)null && entryTimes != null)
		{
			entryTimes.Remove(ent);
		}
		base.OnEntityLeave(ent);
	}

	public override void OnEmpty()
	{
		CancelInvoke(OnTick);
	}

	protected void OnEnable()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		timeSinceAcivation = TimeSince.op_Implicit(0f);
		hurtTiggerUser = SourceEntity as IHurtTriggerUser;
	}

	public new void OnDisable()
	{
		CancelInvoke(OnTick);
		base.OnDisable();
	}

	private bool IsInterested(BaseEntity ent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (TimeSince.op_Implicit(timeSinceAcivation) < activationDelay)
		{
			return false;
		}
		BasePlayer basePlayer = ent.ToPlayer();
		if ((Object)(object)basePlayer != (Object)null)
		{
			if (basePlayer.isMounted)
			{
				BaseVehicle mountedVehicle = basePlayer.GetMountedVehicle();
				if ((Object)(object)SourceEntity != (Object)null && (Object)(object)mountedVehicle == (Object)(object)SourceEntity)
				{
					return false;
				}
				if (ignoreAllVehicleMounted && (Object)(object)mountedVehicle != (Object)null)
				{
					return false;
				}
			}
			if ((Object)(object)SourceEntity != (Object)null && basePlayer.HasEntityInParents(SourceEntity))
			{
				return false;
			}
		}
		return true;
	}

	private void OnTick()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		if (CollectionEx.IsNullOrEmpty(entityContents))
		{
			return;
		}
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		list.AddRange(entityContents);
		foreach (BaseEntity item in list)
		{
			if (item.IsValid() && IsInterested(item) && (!(DamageDelay > 0f) || entryTimes == null || !entryTimes.TryGetValue(item, out var value) || !(value + DamageDelay > Time.time)) && (!RequireUpAxis || !(Vector3.Dot(((Component)item).transform.up, ((Component)this).transform.up) < 0f)))
			{
				float num = DamagePerSecond * 1f / DamageTickRate;
				if (UseSourceEntityDamageMultiplier && hurtTiggerUser != null)
				{
					num *= hurtTiggerUser.GetDamageMultiplier(item);
				}
				if (item.IsNpc)
				{
					num *= npcMultiplier;
				}
				if (item is ResourceEntity)
				{
					num *= resourceMultiplier;
				}
				if (item is LootContainer)
				{
					num *= lootContainerMultiplier;
				}
				if (ScaleWithDistToTriggerCenter && Object.op_Implicit((Object)(object)triggerCollider))
				{
					Vector3 val = item.TriggerPoint();
					Bounds bounds = triggerCollider.bounds;
					float num2 = Vector3.Distance(val, BoundsEx.ClosestPointOnSurface(bounds, val));
					float num3 = Vector3.Distance(val, ((Bounds)(ref bounds)).center);
					float num4 = num2 + num3;
					float num5 = Mathf.Lerp(1f, DamageMultAtCenter, 1f - num3 / num4);
					num *= num5;
				}
				Vector3 val2 = ((Component)item).transform.position + Vector3.up * 1f;
				bool flag = item is BasePlayer || item is BaseNpc;
				BaseEntity baseEntity = null;
				BaseEntity weaponPrefab = null;
				if (hurtTiggerUser != null)
				{
					baseEntity = hurtTiggerUser.GetPlayerDamageInitiator();
					weaponPrefab = SourceEntity.LookupPrefab();
				}
				if ((Object)(object)baseEntity == (Object)null)
				{
					baseEntity = ((!((Object)(object)SourceEntity != (Object)null)) ? GameObjectEx.ToBaseEntity(((Component)this).gameObject) : SourceEntity);
				}
				HitInfo hitInfo = new HitInfo
				{
					DoHitEffects = true,
					HitEntity = item,
					HitPositionWorld = val2,
					HitPositionLocal = ((Component)item).transform.InverseTransformPoint(val2),
					HitNormalWorld = Vector3.up,
					HitMaterial = (flag ? StringPool.Get("Flesh") : 0u),
					WeaponPrefab = weaponPrefab,
					Initiator = baseEntity
				};
				hitInfo.damageTypes = new DamageTypeList();
				hitInfo.damageTypes.Set(damageType, num);
				item.OnAttacked(hitInfo);
				if (hurtTiggerUser != null)
				{
					hurtTiggerUser.OnHurtTriggerOccupant(item, damageType, num);
				}
				if (triggerHitImpacts)
				{
					Effect.server.ImpactEffect(hitInfo);
				}
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
		RemoveInvalidEntities();
	}
}
