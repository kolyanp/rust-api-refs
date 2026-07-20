using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

public static class NpcPushHelper
{
	public static void CoordinatePush(BaseEntity coordinator, float maxDistance = 50f)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		SenseComponent senseComponent = default(SenseComponent);
		NpcZoneComponent npcZoneComponent = default(NpcZoneComponent);
		Scientist2FSM scientist2FSM = default(Scientist2FSM);
		if (((Component)coordinator).TryGetComponent<SenseComponent>(ref senseComponent) && ((Component)coordinator).TryGetComponent<NpcZoneComponent>(ref npcZoneComponent) && senseComponent.FindTarget(out var target) && senseComponent.FindLKP(target, out var lkp) && FindBestPartner(((Component)coordinator).transform.position, senseComponent, npcZoneComponent, out var bestPartner, maxDistance) && ((Component)bestPartner).TryGetComponent<Scientist2FSM>(ref scientist2FSM))
		{
			scientist2FSM.RushPositionTrans.Trigger(new FSMPayload
			{
				entity = target,
				position = lkp
			});
			NpcBarkComponent npcBarkComponent = default(NpcBarkComponent);
			if (AI.npcBarksEnabled && ((Component)coordinator).TryGetComponent<NpcBarkComponent>(ref npcBarkComponent))
			{
				npcBarkComponent.PlayVoicelineFromCategory(ENPCVoicelineCategory.Push, bestPartner);
			}
		}
	}

	public static bool FindBestPartner(Vector3 worldPosition, SenseComponent SenseComponent, NpcZoneComponent NpcZoneComponent, out BaseEntity bestPartner, float maxDistance)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("NpcPushHelper.FindBestPartner"))
		{
			bestPartner = null;
			PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
			try
			{
				SenseComponent.GetPerceivedAllies((List<BaseEntity>)(object)val);
				float num = float.MaxValue;
				BaseEntity baseEntity = null;
				float num2 = float.MaxValue;
				BaseEntity baseEntity2 = null;
				foreach (BaseEntity item in (List<BaseEntity>)(object)val)
				{
					float num3 = DistanceWithExaggeratedY(worldPosition, ((Component)item).transform.position);
					if (!(num3 > maxDistance))
					{
						if (num3 < num2)
						{
							num2 = num3;
							baseEntity2 = item;
						}
						if (NpcZoneComponent.IsInSameZone(item) && num3 < num)
						{
							num = num3;
							baseEntity = item;
						}
					}
				}
				if ((Object)(object)baseEntity2 == (Object)null && (Object)(object)baseEntity == (Object)null)
				{
					return false;
				}
				bestPartner = baseEntity ?? baseEntity2;
				return true;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private static float DistanceWithExaggeratedY(Vector3 a, Vector3 b, float yDistMultiplier = 6f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
		float num2 = Mathf.Abs(a.y - b.y);
		return num + num2 * yDistMultiplier;
	}
}
