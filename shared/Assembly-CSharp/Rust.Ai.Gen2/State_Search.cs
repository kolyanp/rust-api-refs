using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Search : FSMStateBase
{
	[SerializeField]
	private float searchRadius = 10f;

	[SerializeField]
	private RustNavMeshAgent.Speeds speed;

	[SerializeField]
	private int numCloseSearchesBeforeExpanding = 2;

	public bool predict;

	public bool loop = true;

	private NPCHumanoidAnimController _clientAnim;

	private NpcBarkComponent _barkComponent;

	private Vector3 searchOriginNS;

	private int numIterations;

	private NPCHumanoidAnimController ClientAnim => _clientAnim ?? (_clientAnim = ((Component)Owner).GetComponentInChildren<NPCHumanoidAnimController>());

	private NpcBarkComponent BarkComponent => _barkComponent ?? (_barkComponent = ((Component)Owner).GetComponent<NpcBarkComponent>());

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTarget(out var target))
		{
			return EFSMStateStatus.Failure;
		}
		if (!base.Senses.FindLKP(target, out var lkp, applyHeightOffset: false, predict: true))
		{
			return EFSMStateStatus.Failure;
		}
		Matrix4x4 worldToNavMeshSpace = Owner.WorldToNavMeshSpace;
		Vector3 positionNS = ((Matrix4x4)(ref worldToNavMeshSpace)).MultiplyPoint(lkp);
		if (!base.Agent.SamplePosition(positionNS, out var hitNS, 3.5f) && !base.Agent.SamplePosition(positionNS, out hitNS, 20f))
		{
			return EFSMStateStatus.Failure;
		}
		searchOriginNS = ((NavMeshHit)(ref hitNS)).position;
		if (!TrySetSearchDestination())
		{
			return EFSMStateStatus.Failure;
		}
		if (predict)
		{
			BarkComponent.PlayVoicelineFromCategory(ENPCVoicelineCategory.Lost);
		}
		if (!predict)
		{
			PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
			try
			{
				base.Senses.GetPerceivedAllies((List<BaseEntity>)(object)val);
				Scientist2FSM scientist2FSM = default(Scientist2FSM);
				SenseComponent senseComponent = default(SenseComponent);
				foreach (BaseEntity item in (List<BaseEntity>)(object)val)
				{
					if (((Component)item).TryGetComponent<Scientist2FSM>(ref scientist2FSM) && ((Component)item).TryGetComponent<SenseComponent>(ref senseComponent) && senseComponent.FindTarget(out var target2) && !((Object)(object)target2 != (Object)(object)target))
					{
						scientist2FSM.SearchTrans.Trigger();
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		return base.OnStateEnter(payload);
	}

	private bool TrySetSearchDestination()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
		try
		{
			if (numIterations < numCloseSearchesBeforeExpanding)
			{
				Eqs.SamplePositionsInMultiDonutShape(searchOriginNS, (List<Vector3>)(object)val, searchRadius, searchRadius * 0.5f, 2);
			}
			else
			{
				Eqs.SamplePositionsInDonutShape(Owner.ServerNavMeshPos, (List<Vector3>)(object)val, searchRadius);
			}
			if (!base.Senses.FindTargetLKP(out var lkp, applyHeightOffset: false, predict: true))
			{
				return false;
			}
			if (!base.Senses.FindTargetLKP(out var lkp2, applyHeightOffset: false, predict: true))
			{
				return false;
			}
			Matrix4x4 worldToNavMeshSpace = Owner.WorldToNavMeshSpace;
			Vector3 val2 = ((Matrix4x4)(ref worldToNavMeshSpace)).MultiplyPoint(lkp);
			((Matrix4x4)(ref worldToNavMeshSpace)).MultiplyPoint(lkp2);
			Vector3 val3 = ((Matrix4x4)(ref worldToNavMeshSpace)).MultiplyVector(((Component)Owner).transform.forward);
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				PooledList<ScientistNPC2> val4 = Pool.Get<PooledList<ScientistNPC2>>();
				try
				{
					BaseEntity.Query.Server.GetBrainsInSphere(((Component)Owner).transform.position, searchRadius * 3f, (List<ScientistNPC2>)(object)val4);
					RustNavMeshAgent rustNavMeshAgent = default(RustNavMeshAgent);
					foreach (Vector3 item2 in (List<Vector3>)(object)val)
					{
						float num = 0f;
						if (predict)
						{
							num += Mathx.RemapValClamped(Vector3.Dot(val3, Vector3Ex.NormalizeXZ(item2 - val2)), -1f, 1f, 0f, 1f) * 3f;
							num += Mathx.RemapValClamped(Vector3.Distance(item2, val2), 0f, searchRadius, 0f, 1f);
						}
						else
						{
							foreach (ScientistNPC2 item3 in (List<ScientistNPC2>)(object)val4)
							{
								if (!((Object)(object)item3 == (Object)(object)Owner))
								{
									if (((Component)item3).TryGetComponent<RustNavMeshAgent>(ref rustNavMeshAgent) && rustNavMeshAgent.hasPath && rustNavMeshAgent.lastValidPath.Count > 0)
									{
										float num2 = num;
										List<Vector3> lastValidPath = rustNavMeshAgent.lastValidPath;
										num = num2 + Vector3.Distance(lastValidPath[lastValidPath.Count - 1], item2);
									}
									else
									{
										num += Vector3.Distance(item3.ServerNavMeshPos, item2);
									}
								}
							}
							num += Random.value * 0.01f;
						}
						((List<(Vector3, float)>)(object)pooledScoreList).Add((item2, num));
					}
					pooledScoreList.SortByScoreDesc(Owner);
					Matrix4x4 navMeshToWorldSpace = Owner.NavMeshToWorldSpace;
					foreach (var item4 in (List<(Vector3, float)>)(object)pooledScoreList)
					{
						Vector3 item = item4.Item1;
						if (base.Agent.SamplePosition(item, out var hitNS, 3.5f))
						{
							Vector3 position = ((Matrix4x4)(ref navMeshToWorldSpace)).MultiplyPoint(((NavMeshHit)(ref hitNS)).position);
							if (!base.Agent.IsInWater(position) && !(Vector3.Distance(((NavMeshHit)(ref hitNS)).position, Owner.ServerNavMeshPos) < 2f) && base.Agent.SetDestinationWithParams(((NavMeshHit)(ref hitNS)).position, autoBraking: true, speed))
							{
								numIterations++;
								ClientAnim.IsCrouching = speed == RustNavMeshAgent.Speeds.Sneak;
								return true;
							}
						}
					}
					return false;
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
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
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (base.Agent.lastValidPath.Count > 0)
		{
			List<Vector3> lastValidPath = base.Agent.lastValidPath;
			Vector3 val = lastValidPath[lastValidPath.Count - 1] - Owner.ServerNavMeshPos;
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			Matrix4x4 navMeshToWorldSpace = Owner.NavMeshToWorldSpace;
			base.Agent.overrideDirectionWS = ((Matrix4x4)(ref navMeshToWorldSpace)).MultiplyVector(normalized);
		}
		if (!base.Agent.hasPath)
		{
			if (!loop)
			{
				return EFSMStateStatus.Success;
			}
			if (!TrySetSearchDestination())
			{
				return EFSMStateStatus.Failure;
			}
		}
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		numIterations = 0;
		base.Agent.ResetPath();
		base.Agent.overrideDirectionWS = null;
		ClientAnim.IsCrouching = false;
		base.OnStateExit();
	}
}
