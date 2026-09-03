using System;
using System.Collections.Generic;
using System.Text;
using ConVar;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

[SoftRequireComponent(typeof(BlackboardComponent), typeof(NPCEncounterTimer))]
[SoftRequireComponent(typeof(RustNavMeshAgent), typeof(RootMotionPlayer), typeof(SenseComponent))]
public class FSMComponent : EntityComponent<BaseEntity>
{
	public class TickFSMWorkQueue : PersistentObjectWorkQueue<FSMComponent>
	{
		protected override void RunJob(FSMComponent component)
		{
			if (((PersistentObjectWorkQueue<FSMComponent>)this).ShouldAdd(component) && component.isRunning)
			{
				component.Senses.Tick();
				NPCEncounterTimer nPCEncounterTimer = default(NPCEncounterTimer);
				if (((Component)component).TryGetComponent<NPCEncounterTimer>(ref nPCEncounterTimer))
				{
					nPCEncounterTimer.Tick();
				}
				component.Tick();
				NpcBarkComponent npcBarkComponent = default(NpcBarkComponent);
				if (((Component)component).TryGetComponent<NpcBarkComponent>(ref npcBarkComponent))
				{
					npcBarkComponent.Tick();
				}
				NPCNetworking nPCNetworking = default(NPCNetworking);
				if (((Component)component).TryGetComponent<NPCNetworking>(ref nPCNetworking))
				{
					nPCNetworking.Tick();
				}
			}
		}

		protected override bool ShouldAdd(FSMComponent component)
		{
			if (base.ShouldAdd(component))
			{
				return component.baseEntity.IsValid();
			}
			return false;
		}
	}

	private bool isRunning;

	private SenseComponent _senses;

	public const float minRefreshIntervalSeconds = 0f;

	public const float maxRefreshIntervalSeconds = 0.5f;

	private double? _lastTickTime;

	private double nextRefreshTime;

	private const int maxStateChangesPerTick = 3;

	private List<FSMStateBase> sameFrameStateChangesHistory = new List<FSMStateBase>();

	private FSMStateBase pendingStateChange;

	private FSMPayload pendingStateChangePayload;

	public static TickFSMWorkQueue workQueue = new TickFSMWorkQueue();

	public const float frameBudgetMs = 1f;

	public FSMStateBase CurrentState { get; private set; }

	private SenseComponent Senses => _senses ?? (_senses = ((Component)base.baseEntity).GetComponent<SenseComponent>());

	private float RefreshInterval
	{
		get
		{
			if (!Senses.ShouldRefreshFast)
			{
				return 0.5f;
			}
			return 0f;
		}
	}

	private double LastTickTime
	{
		get
		{
			double valueOrDefault = _lastTickTime.GetValueOrDefault();
			if (!_lastTickTime.HasValue)
			{
				valueOrDefault = Time.timeAsDouble;
				_lastTickTime = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
		set
		{
			_lastTickTime = value;
		}
	}

	public void SetFsmActive(bool newActive)
	{
		if (newActive != isRunning)
		{
			isRunning = newActive;
			if (isRunning)
			{
				_lastTickTime = null;
				((PersistentObjectWorkQueue<FSMComponent>)workQueue).Add(this);
			}
			else
			{
				((PersistentObjectWorkQueue<FSMComponent>)workQueue).Remove(this);
			}
		}
	}

	public override void DestroyShared()
	{
		if (base.baseEntity.isServer)
		{
			SetFsmActive(newActive: false);
			base.DestroyShared();
		}
	}

	public static void ShowDebugInfoAroundLocation(BasePlayer player, float radius = 100f)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (!player.IsValid())
		{
			return;
		}
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			BaseEntity.Query.Server.GetBrainsInSphere(((Component)player).transform.position, radius, (List<BaseEntity>)(object)val);
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				FSMComponent component = ((Component)item).GetComponent<FSMComponent>();
				if (!((Object)(object)component == (Object)null) && component.CurrentState != null && component.isRunning)
				{
					player.ClientRPC(RpcTarget.Player("CL_ShowStateDebugInfo", player), ((Component)component.baseEntity).transform.position, component.CurrentState.Name);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected void ForceTickOnTheNextUpdate()
	{
		nextRefreshTime = 0.0;
	}

	public void Tick()
	{
		using (TimeWarning.New("FSMComponent.Tick"))
		{
			if (Time.timeAsDouble < nextRefreshTime)
			{
				return;
			}
			nextRefreshTime = Time.timeAsDouble + (double)RefreshInterval;
			float deltaTime = (float)(Time.timeAsDouble - LastTickTime);
			LastTickTime = Time.timeAsDouble;
			sameFrameStateChangesHistory.Clear();
			if (pendingStateChange != null)
			{
				SetState(pendingStateChange, pendingStateChangePayload);
			}
			else
			{
				if (CurrentState == null)
				{
					return;
				}
				FSMPayload payload = default(FSMPayload);
				using (TimeWarning.New("NormalTransitions"))
				{
					PooledList<FSMStateBase> val = Pool.Get<PooledList<FSMStateBase>>();
					try
					{
						CurrentState.FindAncestry((List<FSMStateBase>)(object)val);
						foreach (FSMStateBase item in (List<FSMStateBase>)(object)val)
						{
							foreach (var (fSMTransitionBase, fSMStateBase) in item.transitions)
							{
								if ((Object)(object)fSMTransitionBase.Owner == (Object)null)
								{
									fSMTransitionBase.Init(base.baseEntity);
								}
								if (fSMTransitionBase.Evaluate(ref payload))
								{
									fSMStateBase.Owner = base.baseEntity;
									fSMTransitionBase.OnTransitionTaken(CurrentState, fSMStateBase);
									SetState(fSMStateBase, payload);
									return;
								}
							}
						}
					}
					finally
					{
						((IDisposable)val)?.Dispose();
					}
				}
				EFSMStateStatus currentStateStatus = EFSMStateStatus.None;
				using (TimeWarning.New("StateTick"))
				{
					using (TimeWarning.New(CurrentState.Name))
					{
						currentStateStatus = CurrentState.OnStateUpdate(deltaTime);
					}
				}
				EvaluateEndTransitions(currentStateStatus);
			}
		}
	}

	private void EvaluateEndTransitions(EFSMStateStatus currentStateStatus)
	{
		using (TimeWarning.New("EndTransitions"))
		{
			if (currentStateStatus == EFSMStateStatus.None)
			{
				return;
			}
			FSMPayload payload = default(FSMPayload);
			PooledList<FSMStateBase> val = Pool.Get<PooledList<FSMStateBase>>();
			try
			{
				CurrentState.FindAncestry((List<FSMStateBase>)(object)val);
				foreach (FSMStateBase item in (List<FSMStateBase>)(object)val)
				{
					foreach (var (fSMTransitionBase, fSMStateBase, eFSMStateStatus) in item.endTransitions)
					{
						if (eFSMStateStatus != (EFSMStateStatus.Success | EFSMStateStatus.Failure) && eFSMStateStatus != currentStateStatus)
						{
							continue;
						}
						bool flag = true;
						if (fSMTransitionBase != null)
						{
							if ((Object)(object)fSMTransitionBase.Owner == (Object)null)
							{
								fSMTransitionBase.Init(base.baseEntity);
							}
							flag = fSMTransitionBase.Evaluate(ref payload);
						}
						if (flag)
						{
							fSMStateBase.Owner = base.baseEntity;
							fSMTransitionBase?.OnTransitionTaken(CurrentState, fSMStateBase);
							SetState(fSMStateBase, payload);
							ForceTickOnTheNextUpdate();
							return;
						}
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public void SetState(FSMStateBase newState, FSMPayload payload = default(FSMPayload))
	{
		using (TimeWarning.New("SetState"))
		{
			newState.Owner = base.baseEntity;
			pendingStateChange = null;
			pendingStateChangePayload = default(FSMPayload);
			sameFrameStateChangesHistory.Add(newState);
			if (sameFrameStateChangesHistory.Count > 3)
			{
				if (!AI.logIssues)
				{
					return;
				}
				StringBuilder stringBuilder = Pool.Get<StringBuilder>();
				stringBuilder.AppendFormat("[FSM] Possible endless recursion detected from {0} to {1} on {2}\n", CurrentState?.Name, newState.Name, base.baseEntity);
				foreach (FSMStateBase item5 in sameFrameStateChangesHistory)
				{
					stringBuilder.AppendFormat("{0} -> ", item5.Name);
				}
				Debug.LogWarning((object)stringBuilder);
				pendingStateChange = newState;
				pendingStateChangePayload = payload;
				Pool.FreeUnmanaged(ref stringBuilder);
				return;
			}
			if (CurrentState != null)
			{
				using (TimeWarning.New("Transitions OnStateExit"))
				{
					PooledList<FSMStateBase> val = Pool.Get<PooledList<FSMStateBase>>();
					try
					{
						CurrentState.FindAncestry((List<FSMStateBase>)(object)val);
						foreach (FSMStateBase item6 in (List<FSMStateBase>)(object)val)
						{
							foreach (var endTransition in item6.endTransitions)
							{
								FSMTransitionBase item = endTransition.transition;
								if (item != null && (Object)(object)item.Owner == (Object)null)
								{
									item.Init(base.baseEntity);
								}
								item?.OnStateExit();
							}
							foreach (var transition in item6.transitions)
							{
								FSMTransitionBase item2 = transition.transition;
								if (item2 != null && (Object)(object)item2.Owner == (Object)null)
								{
									item2.Init(base.baseEntity);
								}
								item2.OnStateExit();
							}
						}
					}
					finally
					{
						((IDisposable)val)?.Dispose();
					}
				}
				using (TimeWarning.New("OnStateExit"))
				{
					using (TimeWarning.New(CurrentState.Name))
					{
						CurrentState.OnStateExit();
					}
				}
			}
			CurrentState = newState;
			using (TimeWarning.New("Transitions OnStateEnter"))
			{
				PooledList<FSMStateBase> val2 = Pool.Get<PooledList<FSMStateBase>>();
				try
				{
					CurrentState.FindAncestry((List<FSMStateBase>)(object)val2);
					foreach (FSMStateBase item7 in (List<FSMStateBase>)(object)val2)
					{
						foreach (var endTransition2 in item7.endTransitions)
						{
							FSMTransitionBase item3 = endTransition2.transition;
							if (item3 != null && (Object)(object)item3.Owner == (Object)null)
							{
								item3.Init(base.baseEntity);
							}
							item3?.OnStateEnter();
						}
						foreach (var transition2 in item7.transitions)
						{
							FSMTransitionBase item4 = transition2.transition;
							if (item4 != null && (Object)(object)item4.Owner == (Object)null)
							{
								item4.Init(base.baseEntity);
							}
							item4.OnStateEnter();
						}
					}
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			using (TimeWarning.New("OnStateEnter"))
			{
				using (TimeWarning.New(CurrentState.Name))
				{
					EFSMStateStatus currentStateStatus = CurrentState.OnStateEnter(payload);
					payload.Dispose();
					EvaluateEndTransitions(currentStateStatus);
				}
			}
		}
	}
}
