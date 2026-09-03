using System.Collections.Generic;
using Oxide.Core;
using UnityEngine;

public class TriggerComfort : TriggerBase
{
	public float triggerSize;

	public float baseComfort = 0.5f;

	public float minComfortRange = 2.5f;

	public bool applyToHorses;

	private const float perPlayerComfortBonus = 0.25f;

	private const float horseComfortBonus = 0.5f;

	private const float bonusComfort = 0f;

	private List<BaseEntity> _entities = new List<BaseEntity>();

	private void OnValidate()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		triggerSize = ((Component)this).GetComponent<SphereCollider>().radius * ((Component)this).transform.localScale.y;
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
		return ((Component)baseEntity).gameObject;
	}

	public float CalculateComfort(Vector3 position, BasePlayer forPlayer = null)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Distance(((Component)this).gameObject.transform.position, position);
		float num2 = Mathf.Clamp(minComfortRange, 0f, triggerSize);
		float num3 = triggerSize - num2;
		float num4 = ((num3 > 0f) ? (num / num3) : 0f);
		float num5 = 1f - Mathf.Clamp(num - num2, 0f, num4);
		bool flag = false;
		float num6 = 0f;
		foreach (BaseEntity entity in _entities)
		{
			if ((Object)(object)entity == (Object)(object)forPlayer)
			{
				continue;
			}
			if (entity is BasePlayer { IsNpc: false } basePlayer)
			{
				float num7 = 1f;
				if (basePlayer.IsSleeping())
				{
					num7 = 0.5f;
				}
				else if (!basePlayer.IsAlive())
				{
					num7 = 0f;
				}
				num6 += 0.25f * num7;
			}
			if (applyToHorses && (entity is RidableHorse || entity is RidableHorse) && !flag)
			{
				num6 += 0.5f;
				flag = true;
			}
		}
		float num8 = 0f + num6;
		return (baseComfort + num8) * num5;
	}

	public override void OnEntityEnter(BaseEntity ent)
	{
		if ((ent is BasePlayer || ent is RidableHorse || ent is RidableHorse) && Interface.CallHook("OnEntityEnter", this, ent) == null)
		{
			_entities.Add(ent);
		}
	}

	public override void OnEntityLeave(BaseEntity ent)
	{
		if ((ent is BasePlayer || ent is RidableHorse || ent is RidableHorse) && Interface.CallHook("OnEntityLeave", this, ent) == null)
		{
			_entities.Remove(ent);
		}
	}
}
