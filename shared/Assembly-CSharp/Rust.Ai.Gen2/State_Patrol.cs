using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;
using UnityEngine.AI;

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

	private Vector3? spawnPositionNS;

	private Vector3? desiredEndDirection;

	private static RustNavMeshPath _path;

	private NPCHumanoidAnimController ClientAnim => _clientAnim ?? (_clientAnim = ((Component)Owner).GetComponentInChildren<NPCHumanoidAnimController>());

	private NpcShootingComponent Shooting => _shooting ?? (_shooting = ((Component)Owner).GetComponent<NpcShootingComponent>());

	private NpcZoneComponent NpcZoneComponent => _npcZoneComponent ?? (_npcZoneComponent = ((Component)Owner).GetComponent<NpcZoneComponent>());

	private static RustNavMeshPath Path => _path ?? (_path = new RustNavMeshPath());

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Reset();
		if (!spawnPositionNS.HasValue)
		{
			spawnPositionNS = Owner.ServerNavMeshPos;
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
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		Vector3 serverNavMeshPos = Owner.ServerNavMeshPos;
		bool flag = Vector3.Distance(spawnPositionNS.Value, serverNavMeshPos) > homeRadius;
		PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
		try
		{
			float radius = Random.Range(distanceRange.x, distanceRange.y);
			Eqs.SamplePositionsInDonutShape(serverNavMeshPos, (List<Vector3>)(object)val, radius);
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				Vector3 val2 = spawnPositionNS.Value - serverNavMeshPos;
				Vector3 normalized = ((Vector3)(ref val2)).normalized;
				foreach (Vector3 item2 in (List<Vector3>)(object)val)
				{
					float num = 0f;
					if (flag)
					{
						float num2 = num;
						val2 = item2 - serverNavMeshPos;
						num = num2 + Mathx.RemapValClamped(Vector3.Dot(normalized, ((Vector3)(ref val2)).normalized), -1f, 1f, 0f, 1f);
					}
					else
					{
						num += Random.value;
					}
					((List<(Vector3, float)>)(object)pooledScoreList).Add((item2, num));
				}
				pooledScoreList.SortByScoreDesc(Owner);
				Matrix4x4 navMeshToWorldSpace = Owner.NavMeshToWorldSpace;
				foreach (var item3 in (List<(Vector3, float)>)(object)pooledScoreList)
				{
					Vector3 item = item3.Item1;
					if (!base.Agent.SamplePosition(item, out var hitNS, 3.5f))
					{
						continue;
					}
					Vector3 val3 = ((Matrix4x4)(ref navMeshToWorldSpace)).MultiplyPoint(((NavMeshHit)(ref hitNS)).position);
					if (!NpcZoneComponent.IsPointInsideZone(val3) || base.Agent.IsInWater(val3) || !base.Agent.CalculatePath(((NavMeshHit)(ref hitNS)).position, Path))
					{
						continue;
					}
					if ((int)Path.status != 0)
					{
						Vector3 val4 = ((Matrix4x4)(ref navMeshToWorldSpace)).MultiplyPoint(Path.GetDestinationNS());
						if (!NpcZoneComponent.IsPointInsideZone(val4) || base.Agent.IsInWater(val4))
						{
							continue;
						}
					}
					base.Agent.SetPath(Path);
					base.Agent.speed = base.Agent.GetSpeedForGait(speed);
					if (base.Agent.lastValidPath.Count >= 2)
					{
						List<Vector3> lastValidPath = base.Agent.lastValidPath;
						Vector3 val5 = lastValidPath[lastValidPath.Count - 1];
						List<Vector3> lastValidPath2 = base.Agent.lastValidPath;
						Vector3 val6 = Vector3Ex.NormalizeXZ(val5 - lastValidPath2[lastValidPath2.Count - 2]) * 3f;
						Vector3 direction = ((Matrix4x4)(ref navMeshToWorldSpace)).MultiplyVector(val6);
						List<Vector3> lastValidPath3 = base.Agent.lastValidPath;
						Vector3 val7 = ((Matrix4x4)(ref navMeshToWorldSpace)).MultiplyPoint(lastValidPath3[lastValidPath3.Count - 1]);
						if (base.Senses.Trace(val7 + base.Senses.EyeOffset, direction, out var hitInfo, 1218519041, "patrol"))
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
