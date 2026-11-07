using System;
using System.Collections.Generic;
using System.Text;
using ConVar;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

[SoftRequireComponent(typeof(RustNavMeshAgent), typeof(RootMotionPlayer), typeof(SenseComponent))]
[SoftRequireComponent(typeof(BlackboardComponent), typeof(NPCEncounterTimer))]
public class FSMComponent : EntityComponent<BaseEntity>
{
	protected struct AliveLayout
	{
		public FSMStateBase entry;

		public FSMStateBase idle;
	}

	public class TickFSMWorkQueue : PersistentObjectWorkQueue<FSMComponent>
	{
		protected override void RunJob(FSMComponent component)
		{
			if (((PersistentObjectWorkQueue<FSMComponent>)this).ShouldAdd(component) && component.isRunning)
			{
				component.Senses.Tick();
				NPCEncounterTimer nPCEncounterTimer = default;
				if (((Component)component).TryGetComponent<NPCEncounterTimer>(ref nPCEncounterTimer))
				{
					nPCEncounterTimer.Tick();
				}
				component.Tick();
				NpcBarkComponent npcBarkComponent = default;
				if (((Component)component).TryGetComponent<NpcBarkComponent>(ref npcBarkComponent))
				{
					npcBarkComponent.Tick();
				}
				NPCNetworking nPCNetworking = default;
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

	[Header("Mounting")]
	public State_MountVehicle mountVehicle = new State_MountVehicle();

	public State_Mounted mounted = new State_Mounted();

	public State_DismountVehicle dismountVehicle = new State_DismountVehicle();

	[NonSerialized]
	private Trans_Triggerable mountTrans;

	[NonSerialized]
	private Trans_Triggerable dismountTrans;

	[Header("Death")]
	public State_Dead dead = new State_Dead();

	[NonSerialized]
	protected State_Nothing root;

	[NonSerialized]
	protected State_Nothing alive;

	[NonSerialized]
	protected State_Nothing waitForNavMesh;

	[NonSerialized]
	protected State_Nothing onNavmesh;

	[NonSerialized]
	protected Trans_Triggerable_HitInfo DeathTrans;

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

	private FSMTransitionBase pendingStateChangeTransition;

	public static TickFSMWorkQueue scientistWorkQueue = new TickFSMWorkQueue
	{
		Name = "FSM.Scientists"
	};

	public static TickFSMWorkQueue animalWorkQueue = new TickFSMWorkQueue
	{
		Name = "FSM.Animals"
	};

	public static TickFSMWorkQueue livestockWorkQueue = new TickFSMWorkQueue
	{
		Name = "FSM.Livestock"
	};

	[NonSerialized]
	private State_DebugMoveToPosition debugMoveTo;

	[NonSerialized]
	private Trans_Triggerable debugMoveToTrans;

	[NonSerialized]
	private FSMStateBase debugMoveToReturnState;

	public bool SupportsMounting => mountTrans != null;

	public bool IsMounting => CurrentState == mountVehicle;

	public bool IsMounted => CurrentState == mounted;

	public bool IsDismounting => CurrentState == dismountVehicle;

	protected virtual bool WantsMounting => false;

	public FSMStateBase CurrentState { get; private set; }

	protected SenseComponent Senses => _senses ?? (_senses = ((Component)baseEntity).GetComponent<SenseComponent>());

	protected NpcRandom Rng => NpcRandom.Of(baseEntity);

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

	public IReadOnlyList<FSMStateBase> StatesEnteredThisTick => sameFrameStateChangesHistory;

	public static float scientistBudgetMs => AI.fsm_frametime_scientists;

	public static float animalBudgetMs => AI.fsm_frametime_animals;

	public static float livestockBudgetMs => AI.fsm_frametime_livestock;

	protected virtual TickFSMWorkQueue workQueue
	{
		get
		{
			if (!(baseEntity is BaseNPC2 { IsAnimal: not false }))
			{
				return scientistWorkQueue;
			}
			return animalWorkQueue;
		}
	}

	public bool SupportsDebugMove => debugMoveTo != null;

	public bool IsDebugMoving
	{
		get
		{
			if (debugMoveTo != null)
			{
				return CurrentState == debugMoveTo;
			}
			return false;
		}
	}

	protected void RegisterMounting(FSMStateBase attachTo, FSMStateBase returnTo)
	{
		if (attachTo != null && returnTo != null)
		{
			mountTrans = new Trans_Triggerable();
			dismountTrans = new Trans_Triggerable();
			attachTo.AddChildren(mountVehicle, mounted, dismountVehicle);
			attachTo.AddTickTransition(mountVehicle, mountTrans);
			mountVehicle.AddFailureTransition(returnTo);
			mountVehicle.AddEndTransition(mounted);
			mounted.AddTickTransition(dismountVehicle, dismountTrans);
			mounted.AddEndTransition(returnTo);
			dismountVehicle.AddFailureTransition(mounted);
			dismountVehicle.AddEndTransition(returnTo);
		}
	}

	public bool StartMount()
	{
		if (mountTrans == null || CurrentState == null || !isRunning)
		{
			return false;
		}
		mountTrans.Trigger();
		ForceTickOnTheNextUpdate();
		return true;
	}

	public bool StartDismount()
	{
		if (dismountTrans == null || !IsMounted)
		{
			return false;
		}
		dismountTrans.Trigger();
		ForceTickOnTheNextUpdate();
		return true;
	}

	public bool ResumeMounted()
	{
		if (mountTrans == null)
		{
			return false;
		}
		SetState(mounted);
		return true;
	}

	protected virtual AliveLayout BuildAlive(State_Nothing onNavmesh)
	{
		return default;
	}

	protected virtual FSMTransitionBase NewNavmeshReadyTransition()
	{
		return new Trans_IsNavmeshReady();
	}

	public override void InitShared()
	{
		base.InitShared();
		if (!baseEntity.isServer)
		{
			return;
		}
		root = new State_Nothing
		{
			Name = "Root"
		};
		alive = new State_Nothing
		{
			Name = "Alive"
		};
		waitForNavMesh = new State_Nothing
		{
			Name = "WaitForNavMesh"
		};
		onNavmesh = new State_Nothing
		{
			Name = "OnNavmesh"
		};
		DeathTrans = new Trans_Triggerable_HitInfo();
		root.AddChildren(alive.AddTickTransition(dead, DeathTrans).AddChildren(waitForNavMesh, onNavmesh.AddTickTransition(waitForNavMesh, ~NewNavmeshReadyTransition())), dead);
		AliveLayout layout = BuildAlive(onNavmesh);
		if (ValidateLayout(layout))
		{
			waitForNavMesh.AddTickTransition(layout.entry, NewNavmeshReadyTransition());
			RegisterDebugMoveTo(alive);
			if (WantsMounting)
			{
				RegisterMounting(alive, layout.idle);
			}
			SetState(waitForNavMesh);
			SetFsmActive(newActive: true);
		}
	}

	private bool ValidateLayout(AliveLayout layout)
	{
		if (!IsUnderAlive(layout.entry))
		{
			return Reject("entry state is missing or does not hang under alive");
		}
		if (layout.idle == null && WantsMounting)
		{
			return Reject("idle state is missing, mounting needs somewhere to hand back to");
		}
		if (layout.idle != null && !IsUnderAlive(layout.idle))
		{
			return Reject("idle state does not hang under alive");
		}
		return true;
	}

	private bool IsUnderAlive(FSMStateBase state)
	{
		for (FSMStateBase fSMStateBase = state; fSMStateBase != null; fSMStateBase = fSMStateBase.parent)
		{
			if (fSMStateBase == alive)
			{
				return true;
			}
		}
		return false;
	}

	private bool Reject(string reason)
	{
		Debug.LogError((object)("[FSM] " + baseEntity.ShortPrefabName + ": " + reason + ", FSM left unstarted"), (Object)(object)baseEntity);
		return false;
	}

	public override bool OnDied(HitInfo hitInfo)
	{
		DeathTrans.Trigger(hitInfo);
		return false;
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
		if (baseEntity.isServer)
		{
			SetFsmActive(newActive: false);
			base.DestroyShared();
		}
	}

	public static void ShowDebugInfoAroundLocation(BasePlayer player, float radius = 100f)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
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
					string arg = ((item is IFSMDebugInfo iFSMDebugInfo) ? (component.CurrentState.Name + "\n" + iFSMDebugInfo.GetDebugInfo()) : component.CurrentState.Name);
					player.ClientRPC(RpcTarget.Player("CL_ShowStateDebugInfo", player), component.baseEntity.net.ID, arg);
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

	protected virtual void OnTicked(float deltaTime)
	{
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
			OnTicked(deltaTime);
			sameFrameStateChangesHistory.Clear();
			if (pendingStateChange != null)
			{
				SetState(pendingStateChange, pendingStateChangePayload, pendingStateChangeTransition);
			}
			else
			{
				if (CurrentState == null)
				{
					return;
				}
				FSMPayload payload = default;
				using (TimeWarning.New("NormalTransitions"))
				{
					PooledList<FSMStateBase> val = Pool.Get<PooledList<FSMStateBase>>();
					try
					{
						CurrentState.FindAncestry((List<FSMStateBase>)(object)val);
						foreach (FSMStateBase item in (List<FSMStateBase>)(object)val)
						{
							foreach (var (fSMTransitionBase, dstState) in item.transitions)
							{
								if (fSMTransitionBase.Evaluate(ref payload) && !IsInheritedSelfTransition(item, dstState))
								{
									TakeTransition(fSMTransitionBase, dstState, payload);
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
			FSMPayload payload = default;
			PooledList<FSMStateBase> val = Pool.Get<PooledList<FSMStateBase>>();
			try
			{
				CurrentState.FindAncestry((List<FSMStateBase>)(object)val);
				foreach (FSMStateBase item in (List<FSMStateBase>)(object)val)
				{
					foreach (var (fSMTransitionBase, dstState, eFSMStateStatus) in item.endTransitions)
					{
						if (eFSMStateStatus == (EFSMStateStatus.Success | EFSMStateStatus.Failure) || eFSMStateStatus == currentStateStatus)
						{
							bool flag = true;
							if (fSMTransitionBase != null)
							{
								flag = fSMTransitionBase.Evaluate(ref payload);
							}
							if (flag && !IsInheritedSelfTransition(item, dstState))
							{
								TakeTransition(fSMTransitionBase, dstState, payload);
								ForceTickOnTheNextUpdate();
								return;
							}
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

	private bool IsInheritedSelfTransition(FSMStateBase declaringState, FSMStateBase dstState)
	{
		if (dstState == CurrentState)
		{
			return declaringState != dstState;
		}
		return false;
	}

	protected virtual FSMStateBase RedirectStateChange(FSMStateBase newState)
	{
		return newState;
	}

	private void TakeTransition(FSMTransitionBase transition, FSMStateBase dstState, FSMPayload payload)
	{
		FSMStateBase fSMStateBase = RedirectStateChange(dstState);
		if (fSMStateBase != dstState)
		{
			SetState(fSMStateBase, payload);
			return;
		}
		transition?.OnTransitionTaken(CurrentState, dstState);
		SetState(dstState, payload, transition);
	}

	private void BindToOwner(FSMStateBase state)
	{
		if (state == null || (Object)(object)state.Owner == (Object)(object)baseEntity)
		{
			return;
		}
		state.Owner = baseEntity;
		BindToOwner(state.parent);
		foreach (FSMStateBase child in state.children)
		{
			BindToOwner(child);
		}
		foreach (var (transition, state2) in state.transitions)
		{
			BindToOwner(transition);
			BindToOwner(state2);
		}
		foreach (var (transition2, state3, _) in state.endTransitions)
		{
			BindToOwner(transition2);
			BindToOwner(state3);
		}
	}

	private void BindToOwner(FSMTransitionBase transition)
	{
		if (transition != null && (Object)(object)transition.Owner != (Object)(object)baseEntity)
		{
			transition.Init(baseEntity);
		}
	}

	public void SetState(FSMStateBase newState, FSMPayload payload = default(FSMPayload), FSMTransitionBase takenBy = null)
	{
		using (TimeWarning.New("SetState"))
		{
			if (CurrentState == null)
			{
				BindToOwner(newState);
			}
			newState = RedirectStateChange(newState);
			pendingStateChange = null;
			pendingStateChangePayload = default;
			pendingStateChangeTransition = null;
			sameFrameStateChangesHistory.Add(newState);
			if (sameFrameStateChangesHistory.Count > 3)
			{
				pendingStateChange = newState;
				pendingStateChangePayload = payload;
				pendingStateChangeTransition = takenBy;
				if (!AI.logIssues)
				{
					return;
				}
				StringBuilder stringBuilder = Pool.Get<StringBuilder>();
				stringBuilder.AppendFormat("[FSM] Possible endless recursion detected from {0} to {1} on {2}\n", CurrentState?.Name, newState.Name, baseEntity);
				foreach (FSMStateBase item in sameFrameStateChangesHistory)
				{
					stringBuilder.AppendFormat("{0} -> ", item.Name);
				}
				Debug.LogWarning((object)stringBuilder);
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
						foreach (FSMStateBase item2 in (List<FSMStateBase>)(object)val)
						{
							foreach (var endTransition in item2.endTransitions)
							{
								endTransition.transition?.OnStateExit();
							}
							foreach (var transition in item2.transitions)
							{
								transition.transition.OnStateExit();
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
					foreach (FSMStateBase item3 in (List<FSMStateBase>)(object)val2)
					{
						foreach (var endTransition2 in item3.endTransitions)
						{
							endTransition2.transition?.OnStateEnter();
						}
						foreach (var transition2 in item3.transitions)
						{
							transition2.transition.OnStateEnter();
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
					EFSMStateStatus eFSMStateStatus = CurrentState.OnStateEnter(payload);
					payload.Dispose();
					if (eFSMStateStatus != EFSMStateStatus.Failure)
					{
						takenBy?.OnTransitionConfirmed(CurrentState);
					}
					EvaluateEndTransitions(eFSMStateStatus);
				}
			}
		}
	}

	public static void RunBudgetedQueues()
	{
		double num = RunReturningUnusedMs(scientistWorkQueue, scientistBudgetMs);
		num = RunReturningUnusedMs(animalWorkQueue, (double)animalBudgetMs + num);
		RunReturningUnusedMs(livestockWorkQueue, (double)livestockBudgetMs + num);
	}

	private static double RunReturningUnusedMs(TickFSMWorkQueue queue, double budgetMs)
	{
		((PersistentObjectWorkQueue<FSMComponent>)queue).RunList(budgetMs);
		return Math.Max(0.0, budgetMs - ((PersistentObjectWorkQueue)queue).Stats.ExecutionTime.TotalMilliseconds);
	}

	protected void RegisterDebugMoveTo(FSMStateBase attachTo)
	{
		if (attachTo != null)
		{
			debugMoveTo = new State_DebugMoveToPosition
			{
				Name = "DebugMoveTo"
			};
			debugMoveToTrans = new Trans_Triggerable();
			attachTo.AddChild(debugMoveTo);
			attachTo.AddTickTransition(debugMoveTo, debugMoveToTrans);
		}
	}

	public bool DebugMoveTo(Vector3 worldPosition, RustNavMeshAgent.Speeds gait, out string error)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		error = null;
		if (debugMoveTo == null)
		{
			error = baseEntity.ShortPrefabName + "'s FSM has no debug move state registered";
			return false;
		}
		if (CurrentState == null || !isRunning)
		{
			error = baseEntity.ShortPrefabName + "'s FSM is not running - it may be asleep, try ai.sleepwake 0";
			return false;
		}
		RustNavMeshAgent rustNavMeshAgent = default;
		if (!((Component)baseEntity).TryGetComponent<RustNavMeshAgent>(ref rustNavMeshAgent))
		{
			error = baseEntity.ShortPrefabName + " has no RustNavMeshAgent";
			return false;
		}
		Matrix4x4 worldToNavMeshSpace = baseEntity.WorldToNavMeshSpace;
		Vector3 positionWS = worldToNavMeshSpace.MultiplyPoint(worldPosition);
		if (!rustNavMeshAgent.SamplePosition(positionWS, out var _, 5f))
		{
			error = $"No navmesh within {5f}m of that position";
			return false;
		}
		debugMoveTo.destinationWS = worldPosition;
		debugMoveTo.speed = gait;
		if (!IsDebugMoving)
		{
			debugMoveToReturnState = CurrentState;
		}
		debugMoveTo.endTransitions.Clear();
		debugMoveTo.AddEndTransition(debugMoveToReturnState);
		debugMoveToTrans.Trigger();
		ForceTickOnTheNextUpdate();
		return true;
	}

	public bool DebugRelease()
	{
		if (!IsDebugMoving)
		{
			return false;
		}
		SetState(debugMoveToReturnState);
		return true;
	}
}
