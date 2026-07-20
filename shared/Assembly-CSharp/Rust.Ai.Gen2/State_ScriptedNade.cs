using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_ScriptedNade : FSMStateBase
{
	public GameObjectRef deployedGrenadePrefab;

	public const string thrownGrenadeKey = "ThrownScriptedNadeRecently";

	private const float explosionRadius = 6f;

	private const float cooldown = 60f;

	private static RustNavMeshPath _path;

	private NpcBarkComponent _barkComponent;

	private NpcGrenadePositionHint currentHint;

	private static RustNavMeshPath Path => _path ?? (_path = new RustNavMeshPath());

	private NpcBarkComponent BarkComponent => _barkComponent ?? (_barkComponent = ((Component)Owner).GetComponent<NpcBarkComponent>());

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		currentHint = null;
		if (!base.Senses.FindTargetLKP(out var lkp, applyHeightOffset: false, predict: true))
		{
			return EFSMStateStatus.Failure;
		}
		NpcZone npcZone = NpcZone.GetForPoint(Owner, lkp);
		NpcZoneComponent npcZoneComponent = default(NpcZoneComponent);
		if (((Component)Owner).TryGetComponent<NpcZoneComponent>(ref npcZoneComponent) && (Object)(object)npcZoneComponent.zone != (Object)null)
		{
			if ((Object)(object)npcZone == (Object)null)
			{
				npcZone = npcZoneComponent.zone;
			}
			else if ((Object)(object)npcZoneComponent.zone != (Object)(object)npcZone)
			{
				return EFSMStateStatus.Failure;
			}
		}
		if ((Object)(object)npcZone == (Object)null)
		{
			return EFSMStateStatus.Failure;
		}
		PooledList<NpcLevelScript> val = Pool.Get<PooledList<NpcLevelScript>>();
		try
		{
			((Component)npcZone).GetComponentsInChildren<NpcLevelScript>((List<NpcLevelScript>)(object)val);
			if (((List<NpcLevelScript>)(object)val).Count == 0)
			{
				return EFSMStateStatus.Failure;
			}
			PooledList<NpcGrenadePositionHint> val2 = Pool.Get<PooledList<NpcGrenadePositionHint>>();
			try
			{
				BoxCollider val3 = default(BoxCollider);
				foreach (NpcLevelScript item in (List<NpcLevelScript>)(object)val)
				{
					bool flag = false;
					foreach (NpcLevelTrigger linkedTrigger in item.linkedTriggers)
					{
						if (((Behaviour)linkedTrigger).isActiveAndEnabled && ((Component)linkedTrigger).TryGetComponent<BoxCollider>(ref val3) && !(Vector3.Distance(((Collider)val3).ClosestPoint(lkp), lkp) > 2f))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						continue;
					}
					foreach (NpcPositionHint positionHint in item.positionHints)
					{
						if (((Behaviour)positionHint).isActiveAndEnabled && positionHint is NpcGrenadePositionHint npcGrenadePositionHint && !(Vector3.Distance(((Component)npcGrenadePositionHint.landingPoint).transform.position, lkp) > 6f))
						{
							((List<NpcGrenadePositionHint>)(object)val2).Add(npcGrenadePositionHint);
						}
					}
				}
				if (((List<NpcGrenadePositionHint>)(object)val2).Count == 0)
				{
					return EFSMStateStatus.Failure;
				}
				((List<NpcGrenadePositionHint>)(object)val2).Sort((Comparison<NpcGrenadePositionHint>)delegate(NpcGrenadePositionHint a, NpcGrenadePositionHint b)
				{
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					//IL_000c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0011: Unknown result type (might be due to invalid IL or missing references)
					//IL_0016: Unknown result type (might be due to invalid IL or missing references)
					//IL_0027: Unknown result type (might be due to invalid IL or missing references)
					//IL_002d: Unknown result type (might be due to invalid IL or missing references)
					//IL_0032: Unknown result type (might be due to invalid IL or missing references)
					//IL_0037: Unknown result type (might be due to invalid IL or missing references)
					Vector3 val4 = ((Component)a).transform.position - lkp;
					float sqrMagnitude = ((Vector3)(ref val4)).sqrMagnitude;
					val4 = ((Component)b).transform.position - lkp;
					return sqrMagnitude.CompareTo(((Vector3)(ref val4)).sqrMagnitude);
				});
				float num = float.PositiveInfinity;
				NpcGrenadePositionHint npcGrenadePositionHint2 = null;
				foreach (NpcGrenadePositionHint item2 in (List<NpcGrenadePositionHint>)(object)val2)
				{
					if (base.Agent.CalculatePath(((Component)item2).transform.position, Path))
					{
						float pathLength = Path.GetPathLength();
						if (pathLength < num)
						{
							num = pathLength;
							npcGrenadePositionHint2 = item2;
						}
					}
				}
				if ((Object)(object)npcGrenadePositionHint2 == (Object)null)
				{
					return EFSMStateStatus.Failure;
				}
				if (!base.Agent.SetDestinationWithParams(((Component)npcGrenadePositionHint2).transform.position, autoBraking: true, RustNavMeshAgent.Speeds.Run))
				{
					return EFSMStateStatus.Failure;
				}
				currentHint = npcGrenadePositionHint2;
				return EFSMStateStatus.None;
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (base.Agent.hasPath)
		{
			return EFSMStateStatus.None;
		}
		NpcGrenade npcGrenade = GameManager.server.CreateEntity(deployedGrenadePrefab.resourcePath, base.Senses.EyePosition, ((Component)Owner).transform.rotation) as NpcGrenade;
		if ((Object)(object)npcGrenade == (Object)null)
		{
			return EFSMStateStatus.Failure;
		}
		npcGrenade.SetCreatorEntity(Owner);
		npcGrenade.grenadeHint = currentHint;
		npcGrenade.Spawn();
		base.Blackboard.Add("ThrownScriptedNadeRecently", 60f);
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			base.Senses.GetPerceivedAllies((List<BaseEntity>)(object)val);
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				((Component)item).GetComponent<BlackboardComponent>().Add("ThrownScriptedNadeRecently", 60f);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		NpcPushHelper.CoordinatePush(Owner);
		return EFSMStateStatus.Success;
	}
}
