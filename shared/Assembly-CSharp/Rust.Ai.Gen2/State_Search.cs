using System;
using System.Collections.Generic;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

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

	private NavVector3 searchOriginNS;

	private int numIterations;

	private NPCHumanoidAnimController ClientAnim => _clientAnim ?? (_clientAnim = ((Component)Owner).GetComponentInChildren<NPCHumanoidAnimController>());

	private NpcBarkComponent BarkComponent => _barkComponent ?? (_barkComponent = ((Component)Owner).GetComponent<NpcBarkComponent>());

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTarget(out var target))
		{
			return EFSMStateStatus.Failure;
		}
		if (!base.Senses.FindLKP(target, out var lkp, applyHeightOffset: false, predict: true))
		{
			return EFSMStateStatus.Failure;
		}
		NavVector3 positionNS = base.Agent.WorldToNavSpace(lkp);
		if (!base.Agent.SamplePosition(positionNS, out var hitNS, 3.5f) && !base.Agent.SamplePosition(positionNS, out hitNS, 20f))
		{
			return EFSMStateStatus.Failure;
		}
		searchOriginNS = hitNS.position;
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
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		PooledList<NavVector3> val = Pool.Get<PooledList<NavVector3>>();
		try
		{
			bool flag = ((numIterations >= numCloseSearchesBeforeExpanding) ? Eqs.SampleNavigablePositions(base.Agent, base.Agent.nextPosition, (List<NavVector3>)(object)val, searchRadius, searchRadius, 8) : Eqs.SampleNavigablePositions(base.Agent, searchOriginNS, (List<NavVector3>)(object)val, searchRadius, searchRadius * 0.5f, 16));
			if (!base.Senses.FindTargetLKP(out var lkp, applyHeightOffset: false, predict: true))
			{
				return false;
			}
			if (!base.Senses.FindTargetLKP(out var lkp2, applyHeightOffset: false, predict: true))
			{
				return false;
			}
			NavVector3 navVector = base.Agent.WorldToNavSpace(lkp);
			base.Agent.WorldToNavSpace(lkp2);
			NavVector3 aNS = base.Agent.WorldToNavDirection(((Component)Owner).transform.forward);
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				PooledList<ScientistNPC2> val2 = Pool.Get<PooledList<ScientistNPC2>>();
				try
				{
					BaseEntity.Query.Server.GetBrainsInSphere(((Component)Owner).transform.position, searchRadius * 3f, (List<ScientistNPC2>)(object)val2);
					RustNavMeshAgent rustNavMeshAgent = default(RustNavMeshAgent);
					foreach (NavVector3 item2 in (List<NavVector3>)(object)val)
					{
						float num = 0f;
						if (predict)
						{
							num += Mathx.RemapValClamped(NavVector3.Dot(aNS, (item2 - navVector).NormalizeXZ()), -1f, 1f, 0f, 1f) * 3f;
							num += Mathx.RemapValClamped(NavVector3.Distance(item2, navVector), 0f, searchRadius, 0f, 1f);
						}
						else
						{
							foreach (ScientistNPC2 item3 in (List<ScientistNPC2>)(object)val2)
							{
								if (!((Object)(object)item3 == (Object)(object)Owner))
								{
									Vector3 val3 = base.Agent.NavToWorldSpace(item2);
									if (((Component)item3).TryGetComponent<RustNavMeshAgent>(ref rustNavMeshAgent) && rustNavMeshAgent.hasPath && rustNavMeshAgent.lastValidPath.Count > 0)
									{
										float num2 = num;
										RustNavMeshAgent rustNavMeshAgent2 = rustNavMeshAgent;
										List<NavVector3> lastValidPath = rustNavMeshAgent.lastValidPath;
										num = num2 + Vector3.Distance(rustNavMeshAgent2.NavToWorldSpace(lastValidPath[lastValidPath.Count - 1]), val3);
									}
									else
									{
										num += Vector3.Distance(((Component)item3).transform.position, val3);
									}
								}
							}
							num += Random.value * 0.01f;
						}
						((List<(NavVector3, float)>)(object)pooledScoreList).Add((item2, num));
					}
					pooledScoreList.SortByScoreDesc(Owner);
					foreach (var item4 in (List<(NavVector3, float)>)(object)pooledScoreList)
					{
						NavVector3 item = item4.Item1;
						NavVector3 navVector2 = item;
						if (!flag)
						{
							if (!base.Agent.SamplePosition(item, out var hitNS, 3.5f))
							{
								continue;
							}
							navVector2 = hitNS.position;
						}
						Vector3 positionWS = base.Agent.NavToWorldSpace(navVector2);
						if (!base.Agent.IsInWater(positionWS) && !(NavVector3.Distance(navVector2, base.Agent.nextPosition) < 2f) && base.Agent.SetDestinationWithParams(navVector2, autoBraking: true, speed))
						{
							numIterations++;
							ClientAnim.IsCrouching = speed == RustNavMeshAgent.Speeds.Sneak;
							return true;
						}
					}
					return false;
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
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
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (base.Agent.lastValidPath.Count > 0)
		{
			List<NavVector3> lastValidPath = base.Agent.lastValidPath;
			NavVector3 normalized = (lastValidPath[lastValidPath.Count - 1] - base.Agent.nextPosition).normalized;
			base.Agent.overrideDirectionWS = base.Agent.NavToWorldDirection(normalized);
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
