using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UtilityJobs;

[RequireComponent(typeof(Collider))]
public class TriggerParent : TriggerBase, IServerComponent
{
	private struct AreSwimmingState : IDisposable
	{
		public ref struct ReadOnly
		{
			public ReadOnly<int> EntLookup;

			public BasePlayer[] Players;

			public ReadOnly<Vector3> Posi;

			public ReadOnly<Quaternion> Rots;
		}

		public NativeList<int> EntLookup;

		public BufferList<BasePlayer> Players;

		public NativeList<Vector3> Posi;

		public NativeList<Quaternion> Rots;

		public AreSwimmingState(int playerCount)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			EntLookup = new NativeList<int>(playerCount, AllocatorHandle.op_Implicit((Allocator)2));
			Players = Pool.Get<BufferList<BasePlayer>>();
			if (Players.Capacity < playerCount)
			{
				Players.Resize(playerCount);
			}
			Posi = new NativeList<Vector3>(playerCount, AllocatorHandle.op_Implicit((Allocator)3));
			Rots = new NativeList<Quaternion>(playerCount, AllocatorHandle.op_Implicit((Allocator)3));
		}

		public ReadOnly AsReadOnly()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			return new ReadOnly
			{
				EntLookup = EntLookup.AsReadOnly(),
				Players = Players.Buffer,
				Posi = Posi.AsReadOnly(),
				Rots = Rots.AsReadOnly()
			};
		}

		public void Dispose()
		{
			Rots.Dispose();
			Posi.Dispose();
			Pool.FreeUnmanaged<BasePlayer>(ref Players);
			EntLookup.Dispose();
		}
	}

	private struct HasObjUnderFeetState : IDisposable
	{
		public ref struct ReadOnly
		{
			public ReadOnly<int> TriggerLookup;

			public ReadOnly<int> EntLookup;

			public NativeArray<RaycastCommand> Commands;

			public ReadOnlySpan<BaseEntity> IgnoreEnts;
		}

		public NativeList<int> TriggerLookup;

		public NativeList<int> EntLookup;

		public NativeList<RaycastCommand> Commands;

		public BufferList<BaseEntity> IgnoreEnts;

		public HasObjUnderFeetState(int overlapCount, NativeList<RaycastCommand> commands)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			TriggerLookup = new NativeList<int>(overlapCount, AllocatorHandle.op_Implicit((Allocator)2));
			EntLookup = new NativeList<int>(overlapCount, AllocatorHandle.op_Implicit((Allocator)2));
			Commands = commands;
			IgnoreEnts = Pool.Get<BufferList<BaseEntity>>();
			if (IgnoreEnts.Capacity < overlapCount)
			{
				IgnoreEnts.Resize(overlapCount);
			}
		}

		public ReadOnly AsReadOnly()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			return new ReadOnly
			{
				TriggerLookup = TriggerLookup.AsReadOnly(),
				EntLookup = EntLookup.AsReadOnly(),
				Commands = Commands.AsArray(),
				IgnoreEnts = IgnoreEnts.ContentReadOnlySpan()
			};
		}

		public void Dispose()
		{
			TriggerLookup.Dispose();
			EntLookup.Dispose();
			Pool.FreeUnmanaged<BaseEntity>(ref IgnoreEnts);
		}
	}

	private struct IsClippingState(int overlapCount, NativeList<OBB> obbs, NativeList<int> obbLayerMasks) : IDisposable
	{
		public ref struct ReadOnly
		{
			public ReadOnly<int> TriggerLookup;

			public ReadOnly<int> EntLookup;

			public ReadOnly<OBB> OBBs;

			public ReadOnly<int> OBBLayerMasks;

			public JobHandle OBBOverlapsHandle;
		}

		public const int MaxOverlaps = 6;

		public NativeList<int> TriggerLookup = new NativeList<int>(overlapCount, AllocatorHandle.op_Implicit((Allocator)2));

		public NativeList<int> EntLookup = new NativeList<int>(overlapCount, AllocatorHandle.op_Implicit((Allocator)2));

		public NativeList<OBB> OBBs = obbs;

		public NativeList<int> OBBLayerMasks = obbLayerMasks;

		public JobHandle OBBOverlapsHandle = default(JobHandle);

		public ReadOnly AsReadOnly()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			return new ReadOnly
			{
				TriggerLookup = TriggerLookup.AsReadOnly(),
				EntLookup = EntLookup.AsReadOnly(),
				OBBs = OBBs.AsReadOnly(),
				OBBLayerMasks = OBBLayerMasks.AsReadOnly(),
				OBBOverlapsHandle = OBBOverlapsHandle
			};
		}

		public void Dispose()
		{
			TriggerLookup.Dispose();
			EntLookup.Dispose();
		}
	}

	private struct IsInsideState(int overlapCount, NativeList<OBB> obbs, NativeList<Vector3> entPoints) : IDisposable
	{
		public ref struct ReadOnly
		{
			public ReadOnly<int> EntLookup;

			public ReadOnly<OBB> TriggerOBBs;

			public ReadOnly<Vector3> EntPoints;
		}

		public NativeList<int> EntLookup = new NativeList<int>(overlapCount, AllocatorHandle.op_Implicit((Allocator)3));

		public NativeList<OBB> TriggerOBBs = obbs;

		public NativeList<Vector3> EntPoints = entPoints;

		public ReadOnly AsReadOnly()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			return new ReadOnly
			{
				EntLookup = EntLookup.AsReadOnly(),
				TriggerOBBs = TriggerOBBs.AsReadOnly(),
				EntPoints = EntPoints.AsReadOnly()
			};
		}

		public void Dispose()
		{
			EntLookup.Dispose();
		}
	}

	private static int _TickMode = 1;

	[ServerVar(Help = "Allow triggers to sleep if both they and their contents are stationary (TickMode 1 only)")]
	public static bool AllowTriggerSleeping = true;

	[ServerVar(Help = "world units a trigger can move in WS before it is woken")]
	public static float sleeping_trigger_mask_epsilon = 0.001f;

	private const int InitialCapacity = 64;

	private static StableObjectArray<TriggerParent> ActiveTriggers;

	private static NativeArray<Vector3> LastTriggerWorldPos;

	private static NativeList<RaycastCommand> TraceCommands;

	private static NativeArray<RaycastHit> TraceHits;

	private static NativeList<OBB> ClippingOBBs;

	private static NativeList<int> ClippingOBBLayerMasks;

	private static NativeArray<ColliderHit> ClippingHits;

	private static NativeList<OBB> IsInsideOBBs;

	private static NativeList<Vector3> IsInsideEntPoints;

	private static NativeArray<bool> IsInsideResults;

	[NonSerialized]
	public int StableIndex = -1;

	[Header("General")]
	[Tooltip("Deparent if the parented entity clips into an obstacle")]
	[SerializeField]
	protected bool doClippingCheck;

	[Tooltip("If deparenting via clipping, this will be used (if assigned) to also move the entity to a valid dismount position")]
	public BaseMountable associatedMountable;

	[Tooltip("Needed if the player might dismount inside the trigger and the trigger might be moving. Being mounting inside the trigger lets them dismount in local trigger-space, which means client and server will sync up.Otherwise the client/server delay can have them dismounting into invalid space.")]
	public bool parentMountedPlayers;

	[Tooltip("Sleepers don't have all the checks (e.g. clipping) that awake players get. If that might be a problem,sleeper parenting can be disabled. You'll need an associatedMountable though so that the sleeper can be dismounted.")]
	public bool parentSleepers = true;

	[Tooltip("This was added to allow parenting in some cases with sinking tugboats, it's generally not needed")]
	public bool parentSwimmers;

	[Header("NPC")]
	public bool ParentNPCPlayers;

	[Tooltip("When parenting an NPC don't check if they are shop keepers or mission providers.")]
	public bool SkipNPCChecks;

	[Header("Other")]
	[Tooltip("If the player is already parented to something else, they'll switch over to another parent only if this is true")]
	public bool overrideOtherTriggers;

	[Tooltip("Requires associatedMountable to be set. Prevents players entering the trigger if there's something between their feet and the bottom of the parent trigger")]
	public bool checkForObjUnderFeet;

	[SerializeField]
	[Header("You probably don't need it")]
	private bool doExpensivePlayerClippingChecks;

	[Tooltip("TickMode 1 only - Allow trigger to sleep if it hasn't moved on selected axis. Set to None if it should never ignore")]
	public BaseEntity.Axis TriggerMovementMask = BaseEntity.Axis.XYZ;

	[Tooltip("TickMode 1 only - Allow to ignore entity that haven't moved on selected axis. Set to None if it should never ignore")]
	public BaseEntity.Axis EntityMovementMask = BaseEntity.Axis.XYZ;

	public const int CLIP_CHECK_MASK = 1218511105;

	protected float triggerHeight;

	private BaseEntity cachedEntity;

	private BasePlayer killPlayerTemp;

	[ServerVar(Help = "0 - old InvokeHandler, 1 - Jobs", Default = "1")]
	public static int TickMode
	{
		get
		{
			return _TickMode;
		}
		set
		{
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			if (_TickMode == value)
			{
				return;
			}
			switch (_TickMode)
			{
			case 0:
			{
				ReadOnlySpan<TriggerParent> objects = ActiveTriggers.Objects;
				for (int i = 0; i < objects.Length; i++)
				{
					TriggerParent obj = objects[i];
					obj.CancelInvoke(obj.OnTick);
				}
				break;
			}
			}
			_TickMode = value;
			switch (_TickMode)
			{
			case 0:
			{
				ReadOnlySpan<TriggerParent> objects = ActiveTriggers.Objects;
				for (int i = 0; i < objects.Length; i++)
				{
					TriggerParent obj2 = objects[i];
					obj2.InvokeRepeating(obj2.OnTick, 0f, 0f);
				}
				break;
			}
			case 1:
			{
				LastTriggerWorldPos.Expand<Vector3>(ActiveTriggers.Capacity, (NativeArrayOptions)0, true, false);
				FillJob<Vector3> fillJob = new FillJob<Vector3>
				{
					Values = LastTriggerWorldPos,
					Value = Vector3.negativeInfinity
				};
				IJobExtensions.RunByRef<FillJob<Vector3>>(ref fillJob);
				break;
			}
			}
		}
	}

	protected BaseEntity Entity
	{
		get
		{
			if (cachedEntity.IsRealNull())
			{
				cachedEntity = GameObjectEx.ToBaseEntity(((Component)this).gameObject);
				if (ObjectEx.IsUnityNull(cachedEntity))
				{
					cachedEntity = null;
				}
			}
			else if ((Object)(object)cachedEntity != (Object)null && cachedEntity.IsDestroyed)
			{
				return null;
			}
			return cachedEntity;
		}
	}

	public static void InitInternalState()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		DisposeInternalState();
		ActiveTriggers = new StableObjectArray<TriggerParent>(64);
		TraceCommands = new NativeList<RaycastCommand>(64, AllocatorHandle.op_Implicit((Allocator)4));
		TraceHits = new NativeArray<RaycastHit>(64, (Allocator)4, (NativeArrayOptions)0);
		ClippingOBBs = new NativeList<OBB>(64, AllocatorHandle.op_Implicit((Allocator)4));
		ClippingOBBLayerMasks = new NativeList<int>(64, AllocatorHandle.op_Implicit((Allocator)4));
		ClippingHits = new NativeArray<ColliderHit>(64, (Allocator)4, (NativeArrayOptions)0);
		IsInsideOBBs = new NativeList<OBB>(64, AllocatorHandle.op_Implicit((Allocator)4));
		IsInsideEntPoints = new NativeList<Vector3>(64, AllocatorHandle.op_Implicit((Allocator)4));
		IsInsideResults = new NativeArray<bool>(64, (Allocator)4, (NativeArrayOptions)0);
		LastTriggerWorldPos = new NativeArray<Vector3>(64, (Allocator)4, (NativeArrayOptions)0);
	}

	public static void DisposeInternalState()
	{
		ActiveTriggers?.Dispose();
		ActiveTriggers = null;
		TraceCommands.SafeDispose<RaycastCommand>();
		TraceHits.SafeDispose<RaycastHit>();
		ClippingOBBs.SafeDispose<OBB>();
		NativeListEx.SafeDispose(ref ClippingOBBLayerMasks);
		ClippingHits.SafeDispose<ColliderHit>();
		IsInsideOBBs.SafeDispose<OBB>();
		IsInsideEntPoints.SafeDispose<Vector3>();
		NativeArrayEx.SafeDispose(ref IsInsideResults);
		LastTriggerWorldPos.SafeDispose<Vector3>();
	}

	private void AddToActiveTriggers()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (StableIndex == -1)
		{
			StableIndex = ActiveTriggers.Add(this);
			switch (_TickMode)
			{
			case 0:
				InvokeRepeating(OnTick, 0f, 0f);
				break;
			case 1:
				LastTriggerWorldPos.Expand<Vector3>(ActiveTriggers.Capacity, (NativeArrayOptions)0, true, false);
				LastTriggerWorldPos[StableIndex] = Vector3.negativeInfinity;
				break;
			}
		}
	}

	private void RemoveFromActiveTriggers()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (StableIndex != -1)
		{
			int indexForSyncRemove = ActiveTriggers.GetIndexForSyncRemove(StableIndex);
			ActiveTriggers.RemoveAtSwapback(StableIndex, invalidateStableIndex: true);
			StableIndex = -1;
			int count = ActiveTriggers.Count;
			if (indexForSyncRemove != count)
			{
				Debug.Assert(indexForSyncRemove < count, "Unexpected swap indices, expecting to swap from end to earlier in range!");
				ActiveTriggers.Objects[indexForSyncRemove].StableIndex = indexForSyncRemove;
				LastTriggerWorldPos[indexForSyncRemove] = LastTriggerWorldPos[count];
			}
			if (_TickMode == 0)
			{
				CancelInvoke(OnTick);
			}
		}
	}

	public static void RunOnTick()
	{
		int tickMode = TickMode;
		if (tickMode != 0 && tickMode == 1)
		{
			RunCustomJobsQueue();
		}
	}

	private static void RunCustomJobsQueue()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		NativeList<int> toUpdate = default(NativeList<int>);
		toUpdate._002Ector(ActiveTriggers.Count, AllocatorHandle.op_Implicit((Allocator)2));
		int overlapCount = GatherTriggersToUpdate(toUpdate);
		ShouldParentEntitiesJobs(toUpdate.AsReadOnly(), overlapCount);
		toUpdate.Dispose();
	}

	private static int GatherTriggersToUpdate(NativeList<int> toUpdate)
	{
		int num = 0;
		ReadOnlySpan<TriggerParent> objects = ActiveTriggers.Objects;
		for (int i = 0; i < objects.Length; i++)
		{
			TriggerParent triggerParent = objects[i];
			if (!CollectionEx.IsNullOrEmpty(triggerParent.entityContents))
			{
				toUpdate.AddNoResize(triggerParent.StableIndex);
				num += triggerParent.entityContents.Count;
			}
		}
		return num;
	}

	private static void ShouldParentEntitiesJobs(ReadOnly<int> toUpdate, int overlapCount)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ShouldParentEntitiesJobs"))
		{
			AreSwimmingState areSwimmingState = new AreSwimmingState(overlapCount);
			try
			{
				TraceCommands.Expand<RaycastCommand>(overlapCount, false);
				HasObjUnderFeetState hasObjUnderFeetState = new HasObjUnderFeetState(overlapCount, TraceCommands);
				ClippingOBBs.Expand<OBB>(overlapCount, false);
				NativeListEx.Expand(ref ClippingOBBLayerMasks, overlapCount, copyContents: false);
				IsClippingState isClippingState = new IsClippingState(overlapCount, ClippingOBBs, ClippingOBBLayerMasks);
				IsInsideOBBs.Expand<OBB>(overlapCount, false);
				IsInsideEntPoints.Expand<Vector3>(overlapCount, false);
				IsInsideState isInsideState = new IsInsideState(overlapCount, IsInsideOBBs, IsInsideEntPoints);
				BufferList<BaseEntity> val = Pool.Get<BufferList<BaseEntity>>();
				if (val.Capacity < overlapCount)
				{
					val.Resize(overlapCount);
				}
				NativeList<int> entToTriggerLookup = new NativeList<int>(overlapCount, AllocatorHandle.op_Implicit((Allocator)3));
				try
				{
					NativeList<bool> shouldParentEnt = new NativeList<bool>(overlapCount, AllocatorHandle.op_Implicit((Allocator)3));
					try
					{
						InitialShouldParentChecks(toUpdate, in areSwimmingState, in isClippingState, in hasObjUnderFeetState, in isInsideState, val, entToTriggerLookup, shouldParentEnt);
						isClippingState.OBBOverlapsHandle = default(JobHandle);
						IsClippingState.ReadOnly state;
						if (isClippingState.OBBs.Length > 0)
						{
							state = isClippingState.AsReadOnly();
							isClippingState.OBBOverlapsHandle = ScheduleClippingOverlaps(in state);
						}
						JobHandle val2 = default(JobHandle);
						if (isInsideState.TriggerOBBs.Length > 0)
						{
							NativeArrayEx.Expand(ref IsInsideResults, isInsideState.TriggerOBBs.Length, (NativeArrayOptions)0, copyContents: false);
							ContainsJob val3 = new ContainsJob
							{
								OBBs = isInsideState.TriggerOBBs.AsReadOnly(),
								Points = isInsideState.EntPoints.AsReadOnly(),
								Results = IsInsideResults
							};
							val2 = IJobParallelForExtensions.ScheduleByRef<ContainsJob>(ref val3, isInsideState.TriggerOBBs.Length, isInsideState.TriggerOBBs.Length, default(JobHandle));
						}
						if (areSwimmingState.Players.Count > 0)
						{
							RunAreSwimmingChecks(areSwimmingState.AsReadOnly(), shouldParentEnt.AsArray());
						}
						if (hasObjUnderFeetState.Commands.Length > 0)
						{
							RunCheckForObjUnderFeet(hasObjUnderFeetState.AsReadOnly(), shouldParentEnt.AsArray());
						}
						if (val2 != default(JobHandle))
						{
							((JobHandle)(ref val2)).Complete();
							ScatterToAndJob scatterToAndJob = new ScatterToAndJob
							{
								To = shouldParentEnt.AsArray(),
								From = IsInsideResults.AsReadOnly(),
								Indices = isInsideState.EntLookup.AsReadOnly()
							};
							IJobExtensions.RunByRef<ScatterToAndJob>(ref scatterToAndJob);
						}
						if (isClippingState.OBBs.Length > 0)
						{
							state = isClippingState.AsReadOnly();
							RunClippingChecks(in state, val.ContentReadOnlySpan(), shouldParentEnt.AsArray());
						}
						CommitShouldParentResults(val.ContentReadOnlySpan(), entToTriggerLookup.AsReadOnly(), shouldParentEnt.AsReadOnly());
						Pool.FreeUnmanaged<BaseEntity>(ref val);
						hasObjUnderFeetState.Dispose();
						isClippingState.Dispose();
						isInsideState.Dispose();
					}
					finally
					{
						((IDisposable)shouldParentEnt/*cast due to constrained. prefix*/).Dispose();
					}
				}
				finally
				{
					((IDisposable)entToTriggerLookup/*cast due to constrained. prefix*/).Dispose();
				}
			}
			finally
			{
				((IDisposable)areSwimmingState/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static void InitialShouldParentChecks(ReadOnly<int> toUpdate, in AreSwimmingState areSwimmingState, in IsClippingState isClippingState, in HasObjUnderFeetState hasObjUnderFeetState, in IsInsideState isInsideState, BufferList<BaseEntity> entities, NativeList<int> entToTriggerLookup, NativeList<bool> shouldParentEnt)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_0469: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("InitialShouldParentChecks"))
		{
			int frameCount = Time.frameCount;
			ReadOnlySpan<TriggerParent> objects = ActiveTriggers.Objects;
			ReadOnly<BasePlayer.CachedState> cachedStates = BasePlayer.PlayerReadOnlyStates.CachedStates;
			_ = BasePlayer.PlayerReadOnlyStates;
			Enumerator<int> enumerator = toUpdate.GetEnumerator();
			try
			{
				Bounds val2 = default(Bounds);
				QueryParameters val4 = default(QueryParameters);
				RaycastCommand val5 = default(RaycastCommand);
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					TriggerParent triggerParent = objects[current];
					BaseMountable baseMountable = triggerParent.associatedMountable;
					bool flag = (Object)(object)baseMountable != (Object)null;
					bool flag2 = !AllowTriggerSleeping;
					if (!flag2)
					{
						flag2 = triggerParent.TriggerMovementMask == BaseEntity.Axis.None;
					}
					if (!flag2)
					{
						Vector3 position = ((Component)triggerParent).transform.position;
						flag2 = (BaseEntity.ComparePos(LastTriggerWorldPos[current], position, sleeping_trigger_mask_epsilon) & triggerParent.TriggerMovementMask) != 0;
						if (flag2)
						{
							LastTriggerWorldPos[current] = position;
						}
					}
					bool flag3 = triggerParent is TriggerParentEnclosed;
					OBB val = default(OBB);
					TriggerParentEnclosed.TriggerMode triggerMode = TriggerParentEnclosed.TriggerMode.TriggerPoint;
					if (flag3)
					{
						TriggerParentEnclosed triggerParentEnclosed = triggerParent as TriggerParentEnclosed;
						BoxCollider boxCollider = triggerParentEnclosed.boxCollider;
						((Bounds)(ref val2))._002Ector(boxCollider.center, boxCollider.size);
						if (triggerParentEnclosed.Padding > 0f)
						{
							((Bounds)(ref val2)).Expand(triggerParentEnclosed.Padding);
						}
						((OBB)(ref val))._002Ector(((Component)boxCollider).transform, val2);
						triggerMode = triggerParentEnclosed.intersectionMode;
					}
					foreach (BaseEntity entityContent in triggerParent.entityContents)
					{
						if (!entityContent.IsValid() || entityContent.IsDestroyed)
						{
							continue;
						}
						bool flag4 = triggerParent.EntityMovementMask == BaseEntity.Axis.None;
						if (!flag4)
						{
							flag4 = (entityContent.HasMovedInLS(frameCount) & triggerParent.EntityMovementMask) != 0;
							if (!flag4 && entityContent is BasePlayer basePlayer)
							{
								bool flag5 = basePlayer.lastViolationType == AntiHackType.NoClip && Time.realtimeSinceStartup - basePlayer.lastViolationTime < 1f;
								flag4 = flag4 || flag5;
							}
						}
						if (!flag2 && !flag4)
						{
							continue;
						}
						bool flag6 = false;
						bool flag7 = false;
						BasePlayer basePlayer2 = null;
						if (entityContent.canTriggerParent)
						{
							if (!triggerParent.overrideOtherTriggers)
							{
								BaseEntity parentEntity = entityContent.GetParentEntity();
								if (parentEntity.IsValid() && (Object)(object)parentEntity != (Object)(object)triggerParent.Entity)
								{
									goto IL_0332;
								}
							}
							TriggerParentExclusion triggerParentExclusion = entityContent.FindTrigger<TriggerParentExclusion>();
							if (!((Object)(object)triggerParentExclusion != (Object)null) || !triggerParentExclusion.IgnoreIfOnLadder || !(entityContent is BasePlayer basePlayer3) || !((basePlayer3.ActivePlayerInd != -1) ? cachedStates[basePlayer3.ActivePlayerInd].IsOnLadder : basePlayer3.OnLadder()))
							{
								basePlayer2 = entityContent.ToPlayer();
								if ((Object)(object)basePlayer2 != (Object)null)
								{
									if (!triggerParent.parentSwimmers)
									{
										if (basePlayer2.ActivePlayerInd != -1)
										{
											if (cachedStates[basePlayer2.ActivePlayerInd].IsSwimming)
											{
												goto IL_0332;
											}
										}
										else
										{
											flag7 = true;
										}
									}
									if ((!triggerParent.parentMountedPlayers && basePlayer2.isMounted) || (!triggerParent.parentSleepers && basePlayer2.IsSleeping()) || (triggerParent.parentSleepers && basePlayer2.IsServerFalling() && !basePlayer2.IsLoadingAfterTransfer() && !basePlayer2.IsReceivingSnapshot) || (flag && basePlayer2.isMounted && !triggerParent.IsParentedToUs(basePlayer2) && !baseMountable.HasValidDismountPosition(basePlayer2)))
									{
										goto IL_0332;
									}
								}
								flag6 = true;
							}
						}
						goto IL_0332;
						IL_0332:
						if (flag6)
						{
							if (!triggerParent.parentSwimmers && flag7)
							{
								Transform transform = ((Component)basePlayer2).transform;
								areSwimmingState.Players.Add(basePlayer2);
								areSwimmingState.Posi.AddNoResize(transform.position);
								areSwimmingState.Rots.AddNoResize(transform.rotation);
								areSwimmingState.EntLookup.AddNoResize(shouldParentEnt.Length);
							}
							if (triggerParent.checkForObjUnderFeet)
							{
								Vector3 val3 = entityContent.PivotPoint() + ((Component)entityContent).transform.up * 0.1f;
								float num = triggerParent.triggerHeight + 0.1f;
								((QueryParameters)(ref val4))._002Ector(1503731969, false, (QueryTriggerInteraction)1, false);
								((RaycastCommand)(ref val5))._002Ector(val3, -((Component)triggerParent).transform.up, val4, num);
								hasObjUnderFeetState.Commands.AddNoResize(val5);
								hasObjUnderFeetState.IgnoreEnts.Add(entityContent);
								hasObjUnderFeetState.TriggerLookup.AddNoResize(current);
								hasObjUnderFeetState.EntLookup.AddNoResize(shouldParentEnt.Length);
							}
							if (triggerParent.doClippingCheck && !(entityContent is BaseCorpse))
							{
								isClippingState.OBBs.AddNoResize(entityContent.WorldSpaceBounds());
								isClippingState.OBBLayerMasks.AddNoResize(1218511105);
								isClippingState.TriggerLookup.AddNoResize(current);
								isClippingState.EntLookup.AddNoResize(shouldParentEnt.Length);
							}
							if (flag3)
							{
								isInsideState.TriggerOBBs.AddNoResize(val);
								Vector3 val6 = ((triggerMode == TriggerParentEnclosed.TriggerMode.TriggerPoint) ? entityContent.TriggerPoint() : entityContent.PivotPoint());
								isInsideState.EntPoints.AddNoResize(val6);
								isInsideState.EntLookup.AddNoResize(shouldParentEnt.Length);
							}
						}
						entities.Add(entityContent);
						entToTriggerLookup.AddNoResize(current);
						shouldParentEnt.AddNoResize(flag6);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static JobHandle ScheduleClippingOverlaps(in IsClippingState.ReadOnly state)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ScheduleClippingOverlaps"))
		{
			ClippingHits.Expand<ColliderHit>(state.OBBs.Length * 6, (NativeArrayOptions)0, false, false);
			JobHandle result = GamePhysics.OverlapOBBs(state.OBBs, state.OBBLayerMasks, ClippingHits, 6, (QueryTriggerInteraction)1, GamePhysics.MasksToValidate.Terrain);
			JobHandle.ScheduleBatchedJobs();
			return result;
		}
	}

	private static void RunAreSwimmingChecks(in AreSwimmingState.ReadOnly state, NativeArray<bool> shouldParentEnt)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RunAreSwimmingChecks"))
		{
			NativeList<int> values = new NativeList<int>(state.Posi.Length, AllocatorHandle.op_Implicit((Allocator)3));
			try
			{
				GenerateAscSeqListJob generateAscSeqListJob = new GenerateAscSeqListJob
				{
					Values = values,
					Start = 0,
					Step = 1,
					Count = state.Posi.Length
				};
				IJobExtensions.RunByRef<GenerateAscSeqListJob>(ref generateAscSeqListJob);
				NativeArray<WaterLevel.WaterInfo> infos = new NativeArray<WaterLevel.WaterInfo>(state.Posi.Length, (Allocator)3, (NativeArrayOptions)0);
				try
				{
					NativeArray<float> factors = new NativeArray<float>(state.Posi.Length, (Allocator)3, (NativeArrayOptions)0);
					try
					{
						BasePlayer.GetWaterFactors(state.Players, state.Posi, state.Rots, values.AsReadOnly(), infos, factors);
						for (int i = 0; i < state.Posi.Length; i++)
						{
							if (BasePlayer.IsSwimming(factors[i]))
							{
								int num = state.EntLookup[i];
								shouldParentEnt[num] = false;
							}
						}
					}
					finally
					{
						((IDisposable)factors/*cast due to constrained. prefix*/).Dispose();
					}
				}
				finally
				{
					((IDisposable)infos/*cast due to constrained. prefix*/).Dispose();
				}
			}
			finally
			{
				((IDisposable)values/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static void RunCheckForObjUnderFeet(in HasObjUnderFeetState.ReadOnly state, NativeArray<bool> shouldParentEnt)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RunCheckForObjUnderFeet"))
		{
			TraceHits.Expand<RaycastHit>(state.Commands.Length, (NativeArrayOptions)0, false, false);
			GamePhysics.TraceRealmRays(GamePhysics.Realm.Server, state.Commands, TraceHits, traceWater: false, state.IgnoreEnts);
			ReadOnlySpan<TriggerParent> objects = ActiveTriggers.Objects;
			for (int i = 0; i < state.Commands.Length; i++)
			{
				RaycastHit val = TraceHits[i];
				bool flag = false;
				if (((RaycastHit)(ref val)).colliderInstanceID != 0)
				{
					TriggerParent triggerParent = objects[state.TriggerLookup[i]];
					BaseEntity entity = triggerParent.Entity;
					BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((RaycastHit)(ref val)).collider);
					if ((Object)(object)baseEntity == (Object)null || !baseEntity.HasEntityInParents(entity) || (Object.op_Implicit((Object)(object)triggerParent.associatedMountable) && !baseEntity.HasEntityInParents(triggerParent.associatedMountable)))
					{
						flag = true;
					}
				}
				if (flag)
				{
					int num = state.EntLookup[i];
					shouldParentEnt[num] = false;
				}
			}
		}
	}

	private static void RunClippingChecks(in IsClippingState.ReadOnly state, ReadOnlySpan<BaseEntity> entities, NativeArray<bool> shouldParentEnt)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RunCheckForObjUnderFeet"))
		{
			using (TimeWarning.New("Wait for overlaps"))
			{
				JobHandle oBBOverlapsHandle = state.OBBOverlapsHandle;
				((JobHandle)(ref oBBOverlapsHandle)).Complete();
			}
			ReadOnlySpan<TriggerParent> objects = ActiveTriggers.Objects;
			for (int i = 0; i < state.OBBs.Length; i++)
			{
				int num = state.EntLookup[i];
				if (!shouldParentEnt[num])
				{
					continue;
				}
				TriggerParent triggerParent = objects[state.TriggerLookup[i]];
				bool flag = (Object)(object)triggerParent.associatedMountable != (Object)null;
				bool flag2 = false;
				for (int j = 0; j < 6; j++)
				{
					ColliderHit val = ClippingHits[i * 6 + j];
					if (((ColliderHit)(ref val)).instanceID == 0)
					{
						break;
					}
					if (flag)
					{
						BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((ColliderHit)(ref val)).collider);
						if (!Object.op_Implicit((Object)(object)baseEntity) || !baseEntity.isServer || (Object)(object)baseEntity.GetRootParentEntity() == (Object)(object)triggerParent.associatedMountable)
						{
							continue;
						}
					}
					flag2 = true;
				}
				if (flag2 && triggerParent.doExpensivePlayerClippingChecks)
				{
					BasePlayer basePlayer = entities[num].ToPlayer();
					if ((Object)(object)basePlayer != (Object)null)
					{
						Vector3 val2 = basePlayer.TriggerPoint();
						float radius = BasePlayer.NoClipRadius(ConVar.AntiHack.noclip_margin);
						if (!AntiHack.TestNoClipping(basePlayer, val2, val2 + Vector3.one * 0.0001f, radius, ConVar.AntiHack.noclip_backtracking, out var _, overlapVehicleLayer: true, triggerParent.associatedMountable))
						{
							flag2 = false;
						}
					}
				}
				if (flag2)
				{
					shouldParentEnt[num] = false;
				}
			}
		}
	}

	private static void CommitShouldParentResults(ReadOnlySpan<BaseEntity> entities, ReadOnly<int> triggerLookup, ReadOnly<bool> shouldParentEnt)
	{
		using (TimeWarning.New("CommitShouldParentResults"))
		{
			ReadOnlySpan<TriggerParent> objects = ActiveTriggers.Objects;
			for (int i = 0; i < entities.Length; i++)
			{
				BaseEntity ent = entities[i];
				TriggerParent triggerParent = objects[triggerLookup[i]];
				if (shouldParentEnt[i])
				{
					triggerParent.Parent(ent);
				}
				else
				{
					triggerParent.Unparent(ent);
				}
			}
		}
	}

	private void OnTransformParentChanged()
	{
		cachedEntity = null;
	}

	protected override void Awake()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		Collider component = ((Component)this).GetComponent<Collider>();
		Bounds bounds = component.bounds;
		triggerHeight = ((Bounds)(ref bounds)).size.y;
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

	public override void OnEntityEnter(BaseEntity ent)
	{
		if (!(ent is NPCPlayer npcPly) || (ParentNPCPlayers && CanParentNPC(npcPly)))
		{
			if (ShouldParent(ent))
			{
				Parent(ent);
			}
			base.OnEntityEnter(ent);
			if (entityContents != null && entityContents.Count == 1)
			{
				AddToActiveTriggers();
			}
		}
	}

	public override void OnEntityLeave(BaseEntity ent)
	{
		base.OnEntityLeave(ent);
		if (entityContents == null || entityContents.Count == 0)
		{
			RemoveFromActiveTriggers();
		}
		BasePlayer basePlayer = ent.ToPlayer();
		if (!parentSleepers || !((Object)(object)basePlayer != (Object)null) || !basePlayer.IsSleeping())
		{
			Unparent(ent);
		}
	}

	internal override void OnEmpty()
	{
		base.OnEmpty();
		RemoveFromActiveTriggers();
	}

	internal virtual bool CanParentNPC(NPCPlayer npcPly)
	{
		if (SkipNPCChecks)
		{
			return true;
		}
		if (npcPly is NPCShopKeeper)
		{
			return false;
		}
		return true;
	}

	public virtual bool ShouldParent(BaseEntity ent, bool bypassOtherTriggerCheck = false)
	{
		if (!ent.canTriggerParent)
		{
			return false;
		}
		if (!bypassOtherTriggerCheck && !overrideOtherTriggers)
		{
			BaseEntity parentEntity = ent.GetParentEntity();
			if (parentEntity.IsValid() && (Object)(object)parentEntity != (Object)(object)Entity)
			{
				return false;
			}
		}
		TriggerParentExclusion triggerParentExclusion = ent.FindTrigger<TriggerParentExclusion>();
		if ((Object)(object)triggerParentExclusion != (Object)null && (!triggerParentExclusion.IgnoreIfOnLadder || !(ent is BasePlayer basePlayer) || !basePlayer.OnLadder()))
		{
			return false;
		}
		if (doClippingCheck && !(ent is BaseCorpse) && IsClipping(ent))
		{
			return false;
		}
		if (checkForObjUnderFeet && HasObjUnderFeet(ent))
		{
			return false;
		}
		BasePlayer basePlayer2 = ent.ToPlayer();
		if ((Object)(object)basePlayer2 != (Object)null)
		{
			if (!parentSwimmers && basePlayer2.IsSwimming())
			{
				return false;
			}
			if (!parentMountedPlayers && basePlayer2.isMounted)
			{
				return false;
			}
			if (!parentSleepers && basePlayer2.IsSleeping())
			{
				return false;
			}
			if (parentSleepers && basePlayer2.IsServerFalling() && !basePlayer2.IsLoadingAfterTransfer() && !basePlayer2.IsReceivingSnapshot)
			{
				return false;
			}
			if (basePlayer2.isMounted && (Object)(object)associatedMountable != (Object)null && !IsParentedToUs(basePlayer2) && !associatedMountable.HasValidDismountPosition(basePlayer2))
			{
				return false;
			}
		}
		return true;
	}

	public void ForceParentEarly(BaseEntity ent)
	{
		if (contents == null)
		{
			contents = new HashSet<GameObject>();
		}
		contents.Add(((Component)ent).gameObject);
		OnEntityEnter(ent);
		Invoke(CheckAllParenting, 0.1f);
	}

	private void CheckAllParenting()
	{
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		if (contents != null)
		{
			foreach (GameObject content in contents)
			{
				if (!((Object)(object)content == (Object)null))
				{
					BaseEntity baseEntity = GameObjectEx.ToBaseEntity(content);
					if ((Object)(object)baseEntity != (Object)null && !list.Contains(baseEntity))
					{
						list.Add(baseEntity);
					}
				}
			}
		}
		List<BaseEntity> list2 = Pool.Get<List<BaseEntity>>();
		if (entityContents != null)
		{
			foreach (BaseEntity entityContent in entityContents)
			{
				if (!list.Contains(entityContent))
				{
					list2.Add(entityContent);
				}
			}
		}
		foreach (BaseEntity item in list2)
		{
			OnEntityLeave(item);
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list2);
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	protected void Parent(BaseEntity ent)
	{
		BaseEntity entity = Entity;
		if (!((Object)(object)ent.GetParentEntity() == (Object)(object)entity) && !((Object)(object)entity.GetParentEntity() == (Object)(object)ent))
		{
			ent.SetParent(Entity, worldPositionStays: true, sendImmediate: true);
		}
	}

	protected void Unparent(BaseEntity ent)
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)ent) || (Object)(object)ent.GetParentEntity() != (Object)(object)Entity)
		{
			return;
		}
		if (ent.IsValid() && !ent.IsDestroyed)
		{
			TriggerParent triggerParent = ent.FindSuitableParent();
			if ((Object)(object)triggerParent != (Object)null && triggerParent.Entity.IsValid())
			{
				triggerParent.Parent(ent);
				return;
			}
		}
		ent.SetParent(null, worldPositionStays: true, sendImmediate: true);
		BasePlayer ply = ent.ToPlayer();
		if (!((Object)(object)ply != (Object)null))
		{
			return;
		}
		ply.unparentTime = Time.time;
		ply.PauseFlyHackDetection(5f);
		ply.PauseSpeedHackDetection(5f);
		ply.PauseTickDistanceDetection(5f);
		if (AntiHack.TestNoClipping(ply, ((Component)ply).transform.position, ((Component)ply).transform.position, BasePlayer.NoClipRadius(ConVar.AntiHack.noclip_margin), ConVar.AntiHack.noclip_backtracking, out var _, overlapVehicleLayer: true))
		{
			ply.PauseVehicleNoClipDetection(5f);
		}
		if ((Object)(object)associatedMountable != (Object)null && doClippingCheck && IsClipping(ent))
		{
			if (associatedMountable.GetDismountPosition(ply, out var res))
			{
				ply.MovePosition(res, forceUpdateTriggers: false);
				((Component)ply).transform.rotation = Quaternion.identity;
				ply.SendNetworkUpdateImmediate();
				ply.ClientRPC(RpcTarget.Player("ForcePositionTo", ply), res);
			}
			else
			{
				killPlayerTemp = ply;
				Invoke(KillPlayerDelayed, 0f);
			}
		}
		if (ply.IsSleeping())
		{
			ply.Invoke(delegate
			{
				ply.SetServerFall(wantsOn: true);
			}, 0.05f);
		}
	}

	private bool IsParentedToUs(BaseEntity ent)
	{
		BaseEntity entity = Entity;
		return (Object)(object)ent.GetParentEntity() == (Object)(object)entity;
	}

	private void KillPlayerDelayed()
	{
		if (killPlayerTemp.IsValid() && !killPlayerTemp.IsDead())
		{
			killPlayerTemp.Hurt(1000f, DamageType.Suicide, killPlayerTemp, useProtection: false);
		}
		killPlayerTemp = null;
	}

	private void OnTick()
	{
		if (entityContents == null || !Entity.IsValid())
		{
			return;
		}
		foreach (BaseEntity entityContent in entityContents)
		{
			if (entityContent.IsValid() && !entityContent.IsDestroyed)
			{
				if (ShouldParent(entityContent))
				{
					Parent(entityContent);
				}
				else
				{
					Unparent(entityContent);
				}
			}
		}
	}

	protected virtual bool IsClipping(BaseEntity ent)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		bool flag = (Object)(object)associatedMountable != (Object)null;
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapOBB(ent.WorldSpaceBounds(), list, 1218511105, (QueryTriggerInteraction)1);
		bool flag2 = false;
		foreach (Collider item in list)
		{
			if (flag)
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item);
				if (!Object.op_Implicit((Object)(object)baseEntity) || !baseEntity.isServer || (Object)(object)baseEntity.GetRootParentEntity() == (Object)(object)associatedMountable)
				{
					continue;
				}
			}
			flag2 = true;
			break;
		}
		BasePlayer basePlayer = null;
		if (doExpensivePlayerClippingChecks && flag2 && (Object)(object)(basePlayer = ent.ToPlayer()) != (Object)null)
		{
			Vector3 val = basePlayer.TriggerPoint();
			Collider col;
			bool result = AntiHack.TestNoClipping(basePlayer, val, val + Vector3.one * 0.0001f, BasePlayer.NoClipRadius(ConVar.AntiHack.noclip_margin), ConVar.AntiHack.noclip_backtracking, out col, overlapVehicleLayer: true, associatedMountable);
			Pool.FreeUnmanaged<Collider>(ref list);
			return result;
		}
		Pool.FreeUnmanaged<Collider>(ref list);
		return flag2;
	}

	private bool HasObjUnderFeet(BaseEntity ent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ent.PivotPoint() + ((Component)ent).transform.up * 0.1f;
		float maxDistance = triggerHeight + 0.1f;
		Ray ray = default(Ray);
		((Ray)(ref ray))._002Ector(val, -((Component)this).transform.up);
		if (GamePhysics.TraceRealm(GamePhysics.Realm.Server, ray, 0f, out var hitInfo, maxDistance, 1503731969, (QueryTriggerInteraction)1, ent) && (Object)(object)((RaycastHit)(ref hitInfo)).collider != (Object)null)
		{
			BaseEntity entity = Entity;
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((RaycastHit)(ref hitInfo)).collider);
			if ((Object)(object)baseEntity == (Object)null || !baseEntity.HasEntityInParents(entity) || (Object.op_Implicit((Object)(object)associatedMountable) && !baseEntity.HasEntityInParents(associatedMountable)))
			{
				return true;
			}
		}
		return false;
	}
}
