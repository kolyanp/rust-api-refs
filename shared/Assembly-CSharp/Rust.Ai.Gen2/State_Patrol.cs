using System;
using System.Collections.Generic;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Patrol : FSMStateBase
{
	[SerializeField]
	private Vector2 distanceRange;

	[SerializeField]
	private float homeRadius;

	[SerializeField]
	private RustNavMeshAgent.Speeds speed;

	private NPCHumanoidAnimController _clientAnim;

	private NpcShootingComponent _shooting;

	private NpcZoneComponent _npcZoneComponent;

	private NavVector3? spawnPositionNS;

	private Vector3? desiredEndDirection;

	private static RustNavMeshPath _path;

	private NPCHumanoidAnimController ClientAnim => _clientAnim ?? (_clientAnim = ((Component)Owner).GetComponentInChildren<NPCHumanoidAnimController>());

	private NpcShootingComponent Shooting => _shooting ?? (_shooting = ((Component)Owner).GetComponent<NpcShootingComponent>());

	private NpcZoneComponent NpcZoneComponent => _npcZoneComponent ?? (_npcZoneComponent = ((Component)Owner).GetComponent<NpcZoneComponent>());

	private static RustNavMeshPath Path => _path ?? (_path = new RustNavMeshPath());

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		Reset();
		if (!spawnPositionNS.HasValue)
		{
			spawnPositionNS = base.Agent.nextPosition;
		}
		if (!TrySetPatrolDestination())
		{
			return EFSMStateStatus.Failure;
		}
		ClientAnim.IsRelaxed = true;
		ClientAnim.IsAiming = false;
		Shooting.AllowShooting = false;
		return base.OnStateEnter(payload);
	}

	private bool TrySetPatrolDestination()
	{
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		NavVector3 nextPosition = base.Agent.nextPosition;
		bool flag = NavVector3.Distance(spawnPositionNS.Value, nextPosition) > homeRadius;
		PooledList<NavVector3> val = Pool.Get<PooledList<NavVector3>>();
		try
		{
			float num = Random.Range(distanceRange.x, distanceRange.y);
			bool flag2 = Eqs.SampleNavigablePositions(base.Agent, nextPosition, (List<NavVector3>)(object)val, num, num, 8);
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				NavVector3 normalized = (spawnPositionNS.Value - nextPosition).normalized;
				foreach (NavVector3 item2 in (List<NavVector3>)(object)val)
				{
					float num2 = 0f;
					num2 = ((!flag) ? (num2 + Random.value) : (num2 + Mathx.RemapValClamped(NavVector3.Dot(normalized, (item2 - nextPosition).NormalizeXZ()), -1f, 1f, 0f, 1f)));
					((List<(NavVector3, float)>)(object)pooledScoreList).Add((item2, num2));
				}
				pooledScoreList.SortByScoreDesc(Owner);
				foreach (var item3 in (List<(NavVector3, float)>)(object)pooledScoreList)
				{
					NavVector3 item = item3.Item1;
					NavVector3 navVector = item;
					if (!flag2)
					{
						if (!base.Agent.SamplePosition(item, out var hitNS, 3.5f))
						{
							continue;
						}
						navVector = hitNS.position;
					}
					Vector3 val2 = base.Agent.NavToWorldSpace(navVector);
					if (!NpcZoneComponent.IsPointInsideZone(val2) || base.Agent.IsInWater(val2) || !base.Agent.CalculatePath(navVector, Path))
					{
						continue;
					}
					if ((int)Path.status != 0)
					{
						Vector3 val3 = base.Agent.NavToWorldSpace(Path.GetDestinationNS());
						if (!NpcZoneComponent.IsPointInsideZone(val3) || base.Agent.IsInWater(val3))
						{
							continue;
						}
					}
					base.Agent.SetPath(Path);
					base.Agent.speed = base.Agent.GetSpeedForGait(speed);
					if (base.Agent.lastValidPath.Count >= 2)
					{
						List<NavVector3> lastValidPath = base.Agent.lastValidPath;
						NavVector3 navVector2 = lastValidPath[lastValidPath.Count - 1];
						List<NavVector3> lastValidPath2 = base.Agent.lastValidPath;
						NavVector3 directionNS = (navVector2 - lastValidPath2[lastValidPath2.Count - 2]).NormalizeXZ() * 3f;
						Vector3 direction = base.Agent.NavToWorldDirection(directionNS);
						RustNavMeshAgent agent = base.Agent;
						List<NavVector3> lastValidPath3 = base.Agent.lastValidPath;
						Vector3 val4 = agent.NavToWorldSpace(lastValidPath3[lastValidPath3.Count - 1]);
						if (base.Senses.Trace(val4 + base.Senses.EyeOffset, direction, out var hitInfo, 1218519041, "patrol"))
						{
							desiredEndDirection = Vector3Ex.WithY(((RaycastHit)(ref hitInfo)).normal, 0f);
						}
					}
					return true;
				}
				return false;
			}
			finally
			{
				((IDisposable)(object)pooledScoreList)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Agent.hasPath)
		{
			return EFSMStateStatus.Success;
		}
		if (desiredEndDirection.HasValue && !base.Agent.overrideDirectionWS.HasValue && base.Agent.remainingDistance < 2.5f)
		{
			base.Agent.overrideDirectionWS = desiredEndDirection.Value;
		}
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		ClientAnim.IsRelaxed = false;
		ClientAnim.IsAiming = true;
		Shooting.AllowShooting = true;
		desiredEndDirection = null;
		base.Agent.overrideDirectionWS = null;
		base.Agent.ResetPath();
		base.OnStateExit();
	}

	private void Reset()
	{
		base.Senses.ClearTarget();
		base.Blackboard.Clear();
		if (Owner is BaseCombatEntity { healthFraction: <1f, SecondsSinceAttacked: >120f } baseCombatEntity)
		{
			baseCombatEntity.SetHealth(Owner.MaxHealth());
		}
	}

	public State_Patrol()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		distanceRange = new Vector2(4f, 6f);
		homeRadius = 10f;
		base._002Ector();
	}
}
