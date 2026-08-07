using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BasePlayerJobs;
using CompanionServer;
using ConVar;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Epic.OnlineServices.AntiCheatCommon;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Math;
using Facepunch.Models;
using Facepunch.Rust;
using JetBrains.Annotations;
using Network;
using Network.Relay;
using Network.Visibility;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using ProtoBuf;
using ProtoBuf.Nexus;
using Rust;
using Rust.Ai.Gen2;
using Rust.Ai.Gen2.Nav;
using Rust.Safety;
using SilentOrbit.ProtocolBuffers;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;
using UtilityJobs;

public class BasePlayer : BaseCombatEntity, LootPanel.IHasLootPanel, IIdealSlotEntity, IInventoryProvider, PlayerInventory.ICanMoveFrom, IReceiveDeepSeaNotifications, ISplashable, IMedicalToolTarget
{
	private struct NavDrawTile
	{
		public int navId;

		public Vector2Int coord;

		public RustNavmesh navmesh;

		public Matrix4x4? transform;

		public Vector3 worldCenter;

		public bool alwaysResend;
	}

	public enum CameraMode
	{
		FirstPerson = 0,
		ThirdPerson = 1,
		Eyes = 2,
		FirstPersonWithArms = 3,
		DeathCamClassic = 4,
		LastCyclableMode = 3
	}

	public enum NetworkQueue
	{
		Update,
		UpdateDistance,
		Count
	}

	private class NetworkQueueList
	{
		public HashSet<BaseNetworkable> queueInternal = new HashSet<BaseNetworkable>();

		public int MaxLength;

		public int Length => queueInternal.Count;

		public bool Contains(BaseNetworkable ent)
		{
			return queueInternal.Contains(ent);
		}

		public void Add(BaseNetworkable ent)
		{
			if (!Contains(ent))
			{
				queueInternal.Add(ent);
			}
			MaxLength = Mathf.Max(MaxLength, queueInternal.Count);
		}

		public void Add(BaseNetworkable[] ent)
		{
			foreach (BaseNetworkable ent2 in ent)
			{
				Add(ent2);
			}
		}

		public void Clear(Group group)
		{
			using (TimeWarning.New("NetworkQueueList.Clear"))
			{
				if (group != null)
				{
					if (group.isGlobal)
					{
						return;
					}
					List<BaseNetworkable> list = Pool.Get<List<BaseNetworkable>>();
					foreach (BaseNetworkable item in queueInternal)
					{
						if ((Object)(object)item == (Object)null || item.net?.group == null || item.net.group == group)
						{
							list.Add(item);
						}
					}
					foreach (BaseNetworkable item2 in list)
					{
						queueInternal.Remove(item2);
					}
					Pool.FreeUnmanaged<BaseNetworkable>(ref list);
				}
				else
				{
					queueInternal.RemoveWhere((BaseNetworkable x) => (Object)(object)x == (Object)null || x.net?.group == null || !x.net.group.isGlobal);
				}
			}
		}
	}

	private class SendEntitySnapshots_AsyncState : IPooled
	{
		public BufferList<(BaseEntity from, BasePlayer to)> Pairs;

		public BufferList<NetWrite> NetWrites;

		public BufferList<int> Chains;

		public BufferList<int> ChainIndices;

		public int ChainCounter;

		void IPooled.EnterPool()
		{
			Debug.Assert(ChainCounter == 0 || ChainCounter == ChainIndices.Count, "Releasing before all work started!");
			Pool.FreeUnmanaged<int>(ref Chains);
			Pool.FreeUnmanaged<int>(ref ChainIndices);
			Pool.FreeUnmanaged<NetWrite>(ref NetWrites);
		}

		void IPooled.LeavePool()
		{
			NetWrites = Pool.Get<BufferList<NetWrite>>();
			Chains = Pool.Get<BufferList<int>>();
			ChainIndices = Pool.Get<BufferList<int>>();
			ChainCounter = 0;
		}
	}

	[Flags]
	public enum PlayerFlags
	{
		Unused1 = 1,
		CombatZone = 2,
		IsAdmin = 4,
		ReceivingSnapshot = 8,
		Sleeping = 0x10,
		Spectating = 0x20,
		Wounded = 0x40,
		IsDeveloper = 0x80,
		Connected = 0x100,
		ThirdPersonViewmode = 0x400,
		EyesViewmode = 0x800,
		ChatMute = 0x1000,
		NoSprint = 0x2000,
		Aiming = 0x4000,
		DisplaySash = 0x8000,
		Relaxed = 0x10000,
		SafeZone = 0x20000,
		ServerFall = 0x40000,
		Incapacitated = 0x80000,
		Workbench1 = 0x100000,
		Workbench2 = 0x200000,
		Workbench3 = 0x400000,
		VoiceRangeBoost = 0x800000,
		ModifyClan = 0x1000000,
		LoadingAfterTransfer = 0x2000000,
		NoRespawnZone = 0x4000000,
		IsInTutorial = 0x8000000,
		IsRestrained = 0x10000000,
		CreativeMode = 0x20000000,
		WaitingForGestureInteraction = 0x40000000,
		Ragdolling = int.MinValue
	}

	public enum FogMode
	{
		Mainland = 1,
		DeepSea = 0x20
	}

	private enum RPSWinState
	{
		Win,
		Loss,
		Draw
	}

	public static class GestureIds
	{
		public const uint FlashBlindId = 235662700u;
	}

	public enum GestureStartSource
	{
		ServerAction,
		Player
	}

	public enum MapNoteType
	{
		Death,
		PointOfInterest
	}

	public enum PingType
	{
		Hostile = 0,
		GoTo = 1,
		Dollar = 2,
		Loot = 3,
		Node = 4,
		Gun = 5,
		Build = 6,
		LAST = 6
	}

	public struct PingStyle(int icon, int colour, Phrase title, Phrase desc, PingType pType)
	{
		public int IconIndex = icon;

		public int ColourIndex = colour;

		public Phrase PingTitle = title;

		public Phrase PingDescription = desc;

		public PingType Type = pType;
	}

	[JsonModel]
	public struct FiredProjectileUpdate
	{
		public Vector3 OldPosition;

		public Vector3 NewPosition;

		public Vector3 OldVelocity;

		public Vector3 NewVelocity;

		public float Mismatch;

		public float PartialTime;
	}

	public class FiredProjectile : IPooled
	{
		public ItemDefinition itemDef;

		public ItemModProjectile itemMod;

		public Projectile projectilePrefab;

		public float firedTime;

		public float travelTime;

		public float partialTime;

		public AttackEntity weaponSource;

		public AttackEntity weaponPrefab;

		public Projectile.Modifier projectileModifier;

		public Item pickupItem;

		public float integrity;

		public float trajectoryMismatch;

		public float startPointMismatch;

		public float endPointMismatch;

		public float entityDistance;

		public Vector3 position;

		public Vector3 initialPositionOffset;

		public Vector3 positionOffset;

		public Vector3 velocity;

		public Vector3 initialPosition;

		public Vector3 initialVelocity;

		public Vector3 inheritedVelocity;

		public int protection;

		public int ricochets;

		public int hits;

		public BaseEntity lastEntityHit;

		public float desyncLifeTime;

		public int id;

		public BasePlayer attacker;

		public bool invalid;

		public List<FiredProjectileUpdate> updates = new List<FiredProjectileUpdate>();

		public List<Vector3> simulatedPositions = new List<Vector3>();

		public void EnterPool()
		{
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			itemDef = null;
			itemMod = null;
			projectilePrefab = null;
			firedTime = 0f;
			travelTime = 0f;
			partialTime = 0f;
			weaponSource = null;
			weaponPrefab = null;
			projectileModifier = default(Projectile.Modifier);
			pickupItem = null;
			integrity = 0f;
			trajectoryMismatch = 0f;
			startPointMismatch = 0f;
			endPointMismatch = 0f;
			entityDistance = 0f;
			position = default(Vector3);
			velocity = default(Vector3);
			initialPosition = default(Vector3);
			initialVelocity = default(Vector3);
			inheritedVelocity = default(Vector3);
			protection = 0;
			ricochets = 0;
			hits = 0;
			lastEntityHit = null;
			desyncLifeTime = 0f;
			id = 0;
			attacker = null;
			invalid = false;
			updates.Clear();
			simulatedPositions.Clear();
		}

		public void LeavePool()
		{
		}
	}

	public enum TimeCategory
	{
		Wilderness = 1,
		Monument = 2,
		Base = 4,
		Flying = 8,
		Boating = 0x10,
		Swimming = 0x20,
		Driving = 0x40
	}

	public class LifeStoryWorkQueue : ObjectWorkQueue<BasePlayer>
	{
		protected override void RunJob(BasePlayer entity)
		{
			entity.UpdateTimeCategory();
		}

		protected override bool ShouldAdd(BasePlayer entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	public class SpawnPoint
	{
		public Vector3 pos;

		public Quaternion rot;

		public bool isProcedualSpawn;
	}

	internal struct DeathBlow
	{
		public BaseEntity Initiator;

		public BaseEntity WeaponPrefab;

		public uint HitBone;

		public bool IsValid;

		public static void From(HitInfo hitInfo, out DeathBlow deathBlow)
		{
			deathBlow = default(DeathBlow);
			deathBlow.IsValid = hitInfo != null;
			if (deathBlow.IsValid)
			{
				deathBlow.Initiator = hitInfo.Initiator;
				deathBlow.WeaponPrefab = hitInfo.WeaponPrefab;
				deathBlow.HitBone = hitInfo.HitBone;
			}
			else
			{
				deathBlow.IsValid = false;
				deathBlow.Initiator = null;
				deathBlow.WeaponPrefab = null;
			}
		}

		public static void Reset(ref DeathBlow deathBlow)
		{
			deathBlow.IsValid = false;
			deathBlow.Initiator = null;
			deathBlow.WeaponPrefab = null;
			deathBlow.HitBone = 0u;
		}
	}

	public class BotColliderWorkQueue : PersistentObjectWorkQueue<BasePlayer>
	{
		protected override void RunJob(BasePlayer entity)
		{
			entity.ServerUpdateBots(Time.deltaTime);
		}

		protected override void OnRemoved(BasePlayer entity)
		{
			base.OnRemoved(entity);
			if (entity.IsSleeping())
			{
				entity.RefreshColliderSize(forced: true);
			}
		}
	}

	public class RelationshipUpdateQueue : PersistentObjectWorkQueueListBacked<BasePlayer>
	{
		public override BufferList<BasePlayer> AssignedList => activePlayerList.Values;

		protected override void RunJob(BasePlayer entity)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			if (!(TimeSince.op_Implicit(entity.lastAcquaintanceUpdate) < 1f))
			{
				RelationshipManager.ServerInstance.UpdateAcquaintancesFor(entity, TimeSince.op_Implicit(entity.lastAcquaintanceUpdate));
				entity.lastAcquaintanceUpdate = TimeSince.op_Implicit(0f);
			}
		}
	}

	private class OcclusionPairWorkerBuffers : IPooled
	{
		public BufferList<OcclusionPlayerPair> ToCheck;

		public BufferList<OcclusionPlayerPair> Found;

		public BufferList<(BasePlayer target, BasePlayer observer)> SubAdds;

		public BufferList<(ulong fromId, ulong toId)> CacheAdds;

		public void EnterPool()
		{
			Pool.FreeUnmanaged<OcclusionPlayerPair>(ref ToCheck);
			Pool.FreeUnmanaged<OcclusionPlayerPair>(ref Found);
			Pool.FreeUnmanaged<(BasePlayer, BasePlayer)>(ref SubAdds);
			Pool.FreeUnmanaged<(ulong, ulong)>(ref CacheAdds);
		}

		public void LeavePool()
		{
			ToCheck = Pool.Get<BufferList<OcclusionPlayerPair>>();
			Found = Pool.Get<BufferList<OcclusionPlayerPair>>();
			SubAdds = Pool.Get<BufferList<(BasePlayer, BasePlayer)>>();
			CacheAdds = Pool.Get<BufferList<(ulong, ulong)>>();
		}
	}

	public struct OcclusionPlayerPair
	{
		public BasePlayer from;

		public BasePlayer to;

		public OcclusionLastSeenStatus lastSeenStatus;
	}

	public enum OcclusionLastSeenStatus : byte
	{
		None,
		Expired,
		Valid
	}

	public class SpectatorSubStrategy : ISubscriberStrategy, IPooled
	{
		public BasePlayer SpectatedPlayer { get; set; }

		public Group LastGroup { get; set; }

		public void GatherHighPrioSubscriptions(Networkable net, ListHashSet<Group> visible)
		{
			if ((Object)(object)SpectatedPlayer != (Object)null)
			{
				Network.Server.DefaultSubscriberStrategy.GatherHighPrioSubscriptions(SpectatedPlayer.net, visible);
			}
			else if (LastGroup != null)
			{
				Network.Server.DefaultSubscriberStrategy.GatherHighPrioSubscriptions(LastGroup, null, Net.sv.visibility, visible);
			}
			else
			{
				Network.Server.DefaultSubscriberStrategy.GatherHighPrioSubscriptions(net, visible);
			}
		}

		public void GatherSubscriptions(Networkable net, ListHashSet<Group> visible)
		{
			if ((Object)(object)SpectatedPlayer != (Object)null)
			{
				Network.Server.DefaultSubscriberStrategy.GatherSubscriptions(SpectatedPlayer.net, visible);
			}
			else if (LastGroup != null)
			{
				Network.Server.DefaultSubscriberStrategy.GatherSubscriptions(LastGroup, null, Net.sv.visibility, visible);
			}
			else
			{
				Network.Server.DefaultSubscriberStrategy.GatherSubscriptions(net, visible);
			}
		}

		void IPooled.EnterPool()
		{
			SpectatedPlayer = null;
			LastGroup = null;
		}

		void IPooled.LeavePool()
		{
		}
	}

	public class SpectatedSubStrategy : ISubscriberStrategy, IPooled
	{
		private List<BasePlayer> spectators;

		public bool IsEmpty => spectators == null;

		public void AddSpectator(BasePlayer spectator)
		{
			spectators.Add(spectator);
		}

		public bool RemoveSpectator(BasePlayer spectator)
		{
			spectators.Remove(spectator);
			return CollectionEx.IsEmpty(spectators);
		}

		public ReadOnlySpan<BasePlayer> GetSpectators()
		{
			return UnsafeListAccess.ListAsReadOnlySpan<BasePlayer>(spectators);
		}

		public void GatherHighPrioSubscriptions(Networkable net, ListHashSet<Group> visible)
		{
			Network.Server.DefaultSubscriberStrategy.GatherHighPrioSubscriptions(net, visible);
			foreach (BasePlayer spectator in spectators)
			{
				spectator.net.OnSubscriptionChange();
			}
		}

		public void GatherSubscriptions(Networkable net, ListHashSet<Group> visible)
		{
			Network.Server.DefaultSubscriberStrategy.GatherSubscriptions(net, visible);
		}

		void IPooled.EnterPool()
		{
			Pool.FreeUnmanaged<BasePlayer>(ref spectators);
		}

		void IPooled.LeavePool()
		{
			spectators = Pool.Get<List<BasePlayer>>();
		}
	}

	private class NearbyStash
	{
		public StashContainer Entity;

		public float LookingAtTime;

		public NearbyStash(StashContainer stash)
		{
			Entity = stash;
			LookingAtTime = 0f;
		}
	}

	public struct CachedState
	{
		public WaterLevel.WaterInfo WaterInfo;

		public float WaterFactor;

		public bool IsSwimming;

		public Quaternion EyeRot;

		public Vector3 EyePos;

		public Vector3 Center;

		public MovementModify MovementModify;

		public PlayerFlags PlayerFlags;

		public float Health;

		public float ModifiersMovementMultiplier;

		public float ClothingMoveSpeedReduction;

		public float ClothingWaterSpeedBonus;

		public float WeaponMoveSpeedScale;

		public bool IsOnLadder;

		public static CachedState Default => new CachedState
		{
			ModifiersMovementMultiplier = 1f,
			WeaponMoveSpeedScale = 1f
		};
	}

	public struct EACTickState
	{
		public LogPlayerTickOptions TickOptions;

		public long Timestamp;
	}

	public enum PositionChange
	{
		Same,
		Valid,
		Invalid
	}

	public struct PlayerServerStates
	{
		public struct ReadOnly
		{
			public StableObjectArray<BasePlayer> PlayerCache;

			public ReadOnly<Vector3> PlayerLocalPos;

			public ReadOnly<Vector3> PlayerPos;

			public ReadOnly<Vector3> LastFramePlayerPos;

			public ReadOnly<Quaternion> PlayerLocalRots;

			public ReadOnly<Quaternion> PlayerRots;

			public ReadOnly<WaterLevel.WaterInfo> WaterInfos;

			public ReadOnly<float> WaterFactors;

			public ReadOnly<CachedState> CachedStates;

			public TickInterpolatorCache.ReadOnlyState TickCache;

			public ReadOnly<Flag> PlayerModelStateFlags;

			public ReadOnly<float> PlayerModelStateDucking;

			public TransformAccessArray PlayerTransformsAccess;

			public ReadOnly<bool> IsMounted;

			public BufferList<BaseMountable> Mountables;

			public ReadOnly<float> TickDeltaTime;

			public ReadOnly<bool> TickNeedsFinalizing;
		}

		public StableObjectArray<BasePlayer> PlayerCache;

		public NativeArray<Vector3> PlayerLocalPos;

		public NativeArray<Vector3> PlayerPos;

		public NativeArray<Vector3> LastFramePlayerPos;

		public NativeArray<Quaternion> PlayerLocalRots;

		public NativeArray<Quaternion> PlayerRots;

		public NativeArray<WaterLevel.WaterInfo> WaterInfos;

		public NativeArray<float> WaterFactors;

		public NativeArray<CachedState> CachedStates;

		public TickInterpolatorCache TickCache;

		public NativeArray<Flag> PlayerModelStateFlags;

		public NativeArray<float> PlayerModelStateDucking;

		public TransformAccessArray PlayerTransformsAccess;

		public NativeArray<bool> IsMounted;

		public BufferList<BaseMountable> Mountables;

		public NativeArray<float> TickDeltaTime;

		public NativeArray<bool> TickNeedsFinalizing;

		public ReadOnly AsReadOnly()
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			return new ReadOnly
			{
				PlayerCache = PlayerCache,
				PlayerLocalPos = PlayerLocalPos.AsReadOnly(),
				PlayerPos = PlayerPos.AsReadOnly(),
				LastFramePlayerPos = LastFramePlayerPos.AsReadOnly(),
				PlayerLocalRots = PlayerLocalRots.AsReadOnly(),
				PlayerRots = PlayerRots.AsReadOnly(),
				WaterInfos = WaterInfos.AsReadOnly(),
				WaterFactors = WaterFactors.AsReadOnly(),
				CachedStates = CachedStates.AsReadOnly(),
				TickCache = TickCache.ReadOnly,
				PlayerModelStateFlags = PlayerModelStateFlags.AsReadOnly(),
				PlayerModelStateDucking = PlayerModelStateDucking.AsReadOnly(),
				PlayerTransformsAccess = PlayerTransformsAccess,
				IsMounted = IsMounted.AsReadOnly(),
				Mountables = Mountables,
				TickDeltaTime = TickDeltaTime.AsReadOnly(),
				TickNeedsFinalizing = TickNeedsFinalizing.AsReadOnly()
			};
		}

		public void Init(int initCap = 32)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			PlayerCache = new StableObjectArray<BasePlayer>(initCap);
			PlayerLocalPos = new NativeArray<Vector3>(initCap, (Allocator)4, (NativeArrayOptions)0);
			PlayerPos = new NativeArray<Vector3>(initCap, (Allocator)4, (NativeArrayOptions)0);
			LastFramePlayerPos = new NativeArray<Vector3>(initCap, (Allocator)4, (NativeArrayOptions)0);
			PlayerLocalRots = new NativeArray<Quaternion>(initCap, (Allocator)4, (NativeArrayOptions)0);
			PlayerRots = new NativeArray<Quaternion>(initCap, (Allocator)4, (NativeArrayOptions)0);
			WaterInfos = new NativeArray<WaterLevel.WaterInfo>(initCap, (Allocator)4, (NativeArrayOptions)0);
			WaterFactors = new NativeArray<float>(initCap, (Allocator)4, (NativeArrayOptions)0);
			CachedStates = new NativeArray<CachedState>(initCap, (Allocator)4, (NativeArrayOptions)1);
			TickCache = new TickInterpolatorCache(initCap);
			PlayerModelStateFlags = new NativeArray<Flag>(initCap, (Allocator)4, (NativeArrayOptions)0);
			PlayerModelStateDucking = new NativeArray<float>(initCap, (Allocator)4, (NativeArrayOptions)0);
			PlayerTransformsAccess = new TransformAccessArray(initCap, -1);
			IsMounted = new NativeArray<bool>(initCap, (Allocator)4, (NativeArrayOptions)0);
			Mountables = new BufferList<BaseMountable>(initCap);
			TickDeltaTime = new NativeArray<float>(initCap, (Allocator)4, (NativeArrayOptions)0);
			TickNeedsFinalizing = new NativeArray<bool>(initCap, (Allocator)4, (NativeArrayOptions)0);
		}

		public void SafeDispose()
		{
			PlayerCache?.Dispose();
			PlayerCache = null;
			PlayerLocalPos.SafeDispose<Vector3>();
			PlayerPos.SafeDispose<Vector3>();
			LastFramePlayerPos.SafeDispose<Vector3>();
			PlayerLocalRots.SafeDispose<Quaternion>();
			PlayerRots.SafeDispose<Quaternion>();
			NativeArrayEx.SafeDispose(ref WaterInfos);
			NativeArrayEx.SafeDispose(ref WaterFactors);
			NativeArrayEx.SafeDispose(ref CachedStates);
			TickCache?.Dispose();
			PlayerModelStateFlags.SafeDispose<Flag>();
			NativeArrayEx.SafeDispose(ref PlayerModelStateDucking);
			if (((TransformAccessArray)(ref PlayerTransformsAccess)).isCreated)
			{
				((TransformAccessArray)(ref PlayerTransformsAccess)).Dispose();
			}
			NativeArrayEx.SafeDispose(ref IsMounted);
			Mountables = null;
			NativeArrayEx.SafeDispose(ref TickDeltaTime);
			NativeArrayEx.SafeDispose(ref TickNeedsFinalizing);
		}
	}

	public enum TutorialItemAllowance
	{
		AlwaysAllowed = -1,
		None = 0,
		Level1_HatchetPickaxe = 10,
		Level2_Planner = 20,
		Level3_Bag_TC_Door = 30,
		Level3_Hammer = 35,
		Level4_Spear_Fire = 40,
		Level5_PrepareForCombat = 50,
		Level6_Furnace = 60,
		Level7_WorkBench = 70,
		Level8_Kayak = 80
	}

	public enum InjureState
	{
		Normal,
		Crawling,
		Incapacitated,
		Dead
	}

	[Serializable]
	public struct CapsuleColliderInfo(float height, float radius, Vector3 center)
	{
		public float height = height;

		public float radius = radius;

		public Vector3 center = center;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public ReadOnly<int> toBroadcast;

		public StableObjectArray<BasePlayer> playerCache;

		public ReadOnly<Vector3> playerPos;

		public ReadOnly<CachedState> cachedStates;

		public ReadOnly<bool> isMounted;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					SwitchToThreadPoolAwaitable val = UniTask.SwitchToThreadPool();
					val2 = ((SwitchToThreadPoolAwaitable)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
				}
				((Awaiter)(ref val2)).GetResult();
				TimeWarning timeWarning = TimeWarning.New("UpdateAnalytics");
				try
				{
					Enumerator<int> enumerator = toBroadcast.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							int current = enumerator.Current;
							Facepunch.Rust.Analytics.Azure.OnPlayerTick(playerCache.Objects[current], playerPos[current], cachedStates[current], isMounted[current]);
						}
					}
					finally
					{
						if (num < 0)
						{
							((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
						}
					}
				}
				finally
				{
					if (num < 0)
					{
						((IDisposable)timeWarning)?.Dispose();
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public ReadOnly<int> validPlayers;

		public StableObjectArray<BasePlayer> playerCache;

		public ReadOnly<PositionChange> positionChanges;

		public ReadOnly<CachedState> cachedStates;

		public ReadOnly<EACTickState> tickStates;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					SwitchToThreadPoolAwaitable val = UniTask.SwitchToThreadPool();
					val2 = ((SwitchToThreadPoolAwaitable)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
				}
				((Awaiter)(ref val2)).GetResult();
				TimeWarning timeWarning = TimeWarning.New("EACStateUpdateJob");
				try
				{
					Enumerator<int> enumerator = validPlayers.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							int current = enumerator.Current;
							BasePlayer basePlayer = playerCache.Objects[current];
							basePlayer.lastEACTickIndex = 0;
							if (positionChanges[current] == PositionChange.Invalid)
							{
								continue;
							}
							CachedState cachedState = cachedStates[current];
							for (int i = 0; i < (int)Player.clientTickRate; i++)
							{
								EACTickState tickState = tickStates[current * (int)Player.clientTickRate + i];
								if (tickState.Timestamp == 0L)
								{
									break;
								}
								basePlayer.EACStateUpdate(in cachedState, in tickState);
							}
						}
					}
					finally
					{
						if (num < 0)
						{
							((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
						}
					}
				}
				finally
				{
					if (num < 0)
					{
						((IDisposable)timeWarning)?.Dispose();
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003CSendEntityDestroyMessages_003Eg__ProcessBatch_007C79_0_003Ed : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public int index;

		public int batchSize;

		public BufferList<(BaseEntity from, BasePlayer to)> pairs;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					SwitchToThreadPoolAwaitable val = UniTask.SwitchToThreadPool();
					val2 = ((SwitchToThreadPoolAwaitable)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003C_003CSendEntityDestroyMessages_003Eg__ProcessBatch_007C79_0_003Ed>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
				}
				((Awaiter)(ref val2)).GetResult();
				int num2 = index * batchSize;
				int num3 = num2 + Math.Min(batchSize, pairs.Count - num2);
				for (int i = num2; i < num3; i++)
				{
					var (baseEntity, basePlayer) = pairs[i];
					baseEntity.DestroyOnClient(basePlayer.net.connection);
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public BufferList<int> chainIndices;

		public int batchIndex;

		public BufferList<int> chains;

		public BufferList<(BaseEntity from, BasePlayer to)> pairs;

		public ThreadSafeTime time;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					SwitchToThreadPoolAwaitable val = UniTask.SwitchToThreadPool();
					val2 = ((SwitchToThreadPoolAwaitable)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
				}
				((Awaiter)(ref val2)).GetResult();
				TimeWarning timeWarning = TimeWarning.New("SendEntitySnapshots - Process Batch");
				try
				{
					int num2 = chainIndices[batchIndex];
					Debug.Assert(num2 < chains.Count, "Went out of bounds of snapshot chain!");
					Network.Server sv = Net.sv;
					int num3 = chains[num2];
					num2++;
					int num4 = num2 + num3;
					for (int i = num2; i < num4; i++)
					{
						int num5 = chains[i];
						(BaseEntity, BasePlayer) tuple = pairs[num5];
						BaseEntity item = tuple.Item1;
						BasePlayer item2 = tuple.Item2;
						NetWrite write = sv.StartWrite();
						item.SendAsSnapshot(item2.net.connection, write, in time, ordered: false);
					}
				}
				finally
				{
					if (num < 0)
					{
						((IDisposable)timeWarning)?.Dispose();
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003CSendEntitySnapshotsWithChildren_003Eg__Cleanup_007C74_1_003Ed : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public UniTask workTask;

		public BufferList<(BaseEntity from, BasePlayer to)> pairs;

		private Awaiter _003C_003Eu__1;

		private Awaiter _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val;
				Awaiter val3;
				if (num != 0)
				{
					if (num == 1)
					{
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_00bf;
					}
					SwitchToThreadPoolAwaitable val2 = UniTask.SwitchToThreadPool();
					val3 = ((SwitchToThreadPoolAwaitable)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val3)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val3;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003C_003CSendEntitySnapshotsWithChildren_003Eg__Cleanup_007C74_1_003Ed>(ref val3, ref this);
						return;
					}
				}
				else
				{
					val3 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
				}
				((Awaiter)(ref val3)).GetResult();
				val = ((UniTask)(ref workTask)).GetAwaiter();
				if (!((Awaiter)(ref val)).IsCompleted)
				{
					num = (_003C_003E1__state = 1);
					_003C_003Eu__2 = val;
					((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003C_003CSendEntitySnapshotsWithChildren_003Eg__Cleanup_007C74_1_003Ed>(ref val, ref this);
					return;
				}
				goto IL_00bf;
				IL_00bf:
				((Awaiter)(ref val)).GetResult();
				Pool.FreeUnmanaged<(BaseEntity, BasePlayer)>(ref pairs);
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public int start;

		public BufferList<(BaseEntity from, BasePlayer to)> pairs;

		public ThreadSafeTime time;

		public int count;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					SwitchToThreadPoolAwaitable val = UniTask.SwitchToThreadPool();
					val2 = ((SwitchToThreadPoolAwaitable)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
				}
				((Awaiter)(ref val2)).GetResult();
				for (int i = start; i < start + count; i++)
				{
					var (baseEntity, basePlayer) = pairs[i];
					baseEntity.SendAsSnapshot(basePlayer.net.connection, in time, ordered: false);
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003CSendEntityUpdates_003Eg__ProcessPlayerBatchAsync_007C70_1_003Ed : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public BasePlayer[] players;

		public BufferList<int> indices;

		public ThreadSafeTime time;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					SwitchToThreadPoolAwaitable val = UniTask.SwitchToThreadPool();
					val2 = ((SwitchToThreadPoolAwaitable)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003C_003CSendEntityUpdates_003Eg__ProcessPlayerBatchAsync_007C70_1_003Ed>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
				}
				((Awaiter)(ref val2)).GetResult();
				_003CSendEntityUpdates_003Eg__ProcessPlayerBatch_007C70_2(players, indices, in time);
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CGatherOcclusionPairsChunk_003Ed__825 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public OcclusionPairWorkerBuffers buffers;

		public StableObjectArray<BasePlayer> playerCache;

		public int start;

		public ReadOnly<Vector3> observerPositions;

		public bool deepSeaEnabled;

		public float networkTime;

		public int count;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0205: Unknown result type (might be due to invalid IL or missing references)
			//IL_0219: Unknown result type (might be due to invalid IL or missing references)
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					SwitchToThreadPoolAwaitable val = UniTask.SwitchToThreadPool();
					val2 = ((SwitchToThreadPoolAwaitable)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CGatherOcclusionPairsChunk_003Ed__825>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
				}
				((Awaiter)(ref val2)).GetResult();
				BufferList<OcclusionPlayerPair> toCheck = buffers.ToCheck;
				BufferList<OcclusionPlayerPair> found = buffers.Found;
				BufferList<(BasePlayer, BasePlayer)> subAdds = buffers.SubAdds;
				BufferList<(ulong, ulong)> cacheAdds = buffers.CacheAdds;
				TimeWarning timeWarning = TimeWarning.New("GatherOcclusionPairsChunk");
				try
				{
					BasePlayer[] unsafeObjects = playerCache.UnsafeObjects;
					for (int i = start; i < start + count; i++)
					{
						BasePlayer basePlayer = unsafeObjects[i];
						subAdds.Add((basePlayer, basePlayer));
						Enumerator<BaseNetworkable> enumerator;
						if (basePlayer.IsSpectating())
						{
							if (!(basePlayer.net.SubStrategy is SpectatorSubStrategy spectatorSubStrategy))
							{
								continue;
							}
							ServerOcclusion.Group value = null;
							if ((Object)(object)spectatorSubStrategy.SpectatedPlayer != (Object)null)
							{
								value = spectatorSubStrategy.SpectatedPlayer.OcclusionGroup;
							}
							else if (spectatorSubStrategy.LastGroup != null)
							{
								ServerOcclusion.Occludees.TryGetValue(spectatorSubStrategy.LastGroup, out value);
							}
							if (value == null)
							{
								continue;
							}
							enumerator = ((ListHashSet<BaseNetworkable>)value).GetEnumerator();
							try
							{
								while (enumerator.MoveNext())
								{
									BasePlayer basePlayer2 = enumerator.Current as BasePlayer;
									if (!((Object)(object)basePlayer2 == (Object)null) && !((Object)(object)basePlayer == (Object)(object)basePlayer2))
									{
										if (basePlayer2.IsConnected)
										{
											subAdds.Add((basePlayer2, basePlayer));
										}
										cacheAdds.Add((basePlayer2.net.ID.Value, basePlayer.net.ID.Value));
									}
								}
							}
							finally
							{
								if (num < 0)
								{
									((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
								}
							}
							continue;
						}
						ServerOcclusion.Group occlusionGroup = basePlayer.OcclusionGroup;
						if (occlusionGroup == null || ((ListHashSet<BaseNetworkable>)occlusionGroup).Count <= 1)
						{
							continue;
						}
						bool observerShouldSkipOcclusion = basePlayer.ComputeObserverShouldSkipOcclusion(observerPositions[basePlayer.ActivePlayerInd], deepSeaEnabled);
						enumerator = ((ListHashSet<BaseNetworkable>)occlusionGroup).GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								BasePlayer basePlayer3 = enumerator.Current as BasePlayer;
								if ((Object)(object)basePlayer3 == (Object)null || (Object)(object)basePlayer == (Object)(object)basePlayer3)
								{
									continue;
								}
								bool flag = true;
								bool flag2 = ConVar.AntiHack.server_occlusion_disable_sleeper_los;
								if (basePlayer3.IsConnected)
								{
									flag = CustomShouldNetworkTo(basePlayer3, basePlayer);
									flag2 = false;
									if (flag)
									{
										flag2 = CustomShouldSkipServerOcclusionParallel(basePlayer3, basePlayer, observerShouldSkipOcclusion);
									}
								}
								if (!flag)
								{
									continue;
								}
								OcclusionLastSeenStatus occlusionLastSeenStatus = basePlayer.OcclusionGetRecentlySeen(basePlayer3, networkTime);
								OcclusionPlayerPair occlusionPlayerPair = new OcclusionPlayerPair
								{
									from = basePlayer3,
									to = basePlayer,
									lastSeenStatus = occlusionLastSeenStatus
								};
								if (occlusionLastSeenStatus == OcclusionLastSeenStatus.Valid)
								{
									if (occlusionPlayerPair.from.IsConnected)
									{
										subAdds.Add((occlusionPlayerPair.from, occlusionPlayerPair.to));
									}
									cacheAdds.Add((basePlayer3.net.ID.Value, basePlayer.net.ID.Value));
								}
								else if (flag2)
								{
									if (occlusionPlayerPair.from.IsConnected)
									{
										subAdds.Add((occlusionPlayerPair.from, occlusionPlayerPair.to));
									}
									found.Add(occlusionPlayerPair);
									cacheAdds.Add((basePlayer3.net.ID.Value, basePlayer.net.ID.Value));
								}
								else
								{
									toCheck.Add(occlusionPlayerPair);
								}
							}
						}
						finally
						{
							if (num < 0)
							{
								((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
							}
						}
					}
				}
				finally
				{
					if (num < 0)
					{
						((IDisposable)timeWarning)?.Dispose();
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private HashSet<(int navId, Vector2Int coord)> navmeshSentTiles;

	private HashSet<(int navId, Vector2Int coord)> navmeshDirtyTiles;

	private int navmeshDrawTickCounter;

	[NonSerialized]
	public TimeAverageValueLookup<uint> rpcHistory = new TimeAverageValueLookup<uint>();

	public static readonly Phrase ClanInviteSuccess = new Phrase("clan.action.invite.success", "Invited {name} to your clan.");

	public static readonly Phrase ClanInviteFailure = new Phrase("clan.action.invite.failure", "Failed to invite {name} to your clan. Please wait a minute and try again.");

	public static readonly Phrase ClanInviteFull = new Phrase("clan.action.invite.full", "Cannot invite {name} to your clan because your clan is full.");

	[NonSerialized]
	public long clanId;

	[NonSerialized]
	public IClan serverClan;

	public ViewModel GestureViewModel;

	[NonSerialized]
	public NPCTalking activeTalkingToNpc;

	public const int MaxLootCountdownDebugRequests = 32;

	public const float drinkRange = 1.5f;

	public const float drinkMovementSpeed = 0.1f;

	[NonSerialized]
	private NetworkQueueList[] networkQueue = new NetworkQueueList[2]
	{
		new NetworkQueueList(),
		new NetworkQueueList()
	};

	[NonSerialized]
	private NetworkQueueList SnapshotQueue = new NetworkQueueList();

	private const int FogImagesCount = 16;

	private bool hasSentFogOfWar;

	public const string GestureCancelString = "cancel";

	public TimeUntil gestureFinishedTime;

	public TimeSince blockHeldInputTimer;

	public GestureConfig currentGesture;

	public static Phrase WinRPSPhrase = new Phrase("rps_win", "You win the game!");

	public static Phrase LoseRPSPhrase = new Phrase("rps_lose", "You lose the game!");

	public static Phrase DrawRPSPhrase = new Phrase("rps_draw", "The game was a draw!");

	private HashSet<NetworkableId> recentWaveTargets = new HashSet<NetworkableId>();

	public const string WAVED_PLAYERS_STAT = "waved_at_players";

	private NetworkableId rpsTarget;

	private int selectedRpsOption = -1;

	private Action _actionTimeoutGestureServer;

	private Action _actionMonitorLoopingGesture;

	private Action _actionBotRPSRandomise;

	private Action _actionMonitorRPSGame;

	private Action _actionServer_CancelGesture;

	public const float RPSWaitTime = 10f;

	private TimeSince interactiveGestureStartTime;

	public ulong currentTeam;

	public static readonly Phrase MaxTeamSizeToast = new Phrase("maxteamsizetip", "Your team is full. Remove a member to invite another player.");

	private bool sentInstrumentTeamAchievement;

	private bool sentSummerTeamAchievement;

	private const int TEAMMATE_INSTRUMENT_COUNT_ACHIEVEMENT = 4;

	private const int TEAMMATE_SUMMER_FLOATING_COUNT_ACHIEVEMENT = 4;

	private const string TEAMMATE_INSTRUMENT_ACHIEVEMENT = "TEAM_INSTRUMENTS";

	private const string TEAMMATE_SUMMER_ACHIEVEMENT = "SUMMER_INFLATABLE";

	public static readonly Phrase ToggleOn = new Phrase("itemmodmenu.toggleon", "Toggle On");

	public static readonly Phrase ToggleOff = new Phrase("itemmodmenu.toggleoff", "Toggle Off");

	public static Phrase MarkerLimitPhrase = new Phrase("map.marker.limited", "Cannot place more than {0} markers.");

	public const int MaxMapNoteLabelLength = 10;

	public static readonly Phrase NoSpaceInInventoryPhrase = new Phrase("no_space_mission_reward", "No space for rewards in inventory, please clear some space");

	public static readonly Phrase FailedToCheckRewardsSpace = new Phrase("failed_check_rewards_space", "Failed to get required rewards space");

	private bool _missionsDirty;

	private Action _actionAssignFollowUpMission;

	[NonSerialized]
	public BufferList<BaseMission.MissionInstance> acceptedMissions = new BufferList<BaseMission.MissionInstance>();

	private int _activeMissionIndex = -1;

	private float timeSinceServerMissionThink;

	private BaseMission followupMission;

	private IMissionProvider followupMissionProvider;

	[NonSerialized]
	public ModelState modelState = new ModelState();

	private ModelState lastModelState;

	[NonSerialized]
	public EntityRef mounted;

	public float nextSeatSwapTime;

	public BaseEntity PetEntity;

	[NonSerialized]
	public IPet Pet;

	private float lastPetCommandIssuedTime;

	private static readonly Phrase HostileTitle = new Phrase("ping_hostile", "Hostile");

	private static readonly Phrase HostileDesc = new Phrase("ping_hostile_desc", "Danger in area");

	private static readonly PingStyle HostileMarker = new PingStyle(4, 3, HostileTitle, HostileDesc, PingType.Hostile);

	private static readonly Phrase GoToTitle = new Phrase("ping_goto", "Go To");

	private static readonly Phrase GoToDesc = new Phrase("ping_goto_desc", "Look at this");

	private static readonly PingStyle GoToMarker = new PingStyle(0, 2, GoToTitle, GoToDesc, PingType.GoTo);

	private static readonly Phrase DollarTitle = new Phrase("ping_dollar", "Value");

	private static readonly Phrase DollarDesc = new Phrase("ping_dollar_desc", "Something valuable is here");

	private static readonly PingStyle DollarMarker = new PingStyle(1, 1, DollarTitle, DollarDesc, PingType.Dollar);

	private static readonly Phrase LootTitle = new Phrase("ping_loot", "Loot");

	private static readonly Phrase LootDesc = new Phrase("ping_loot_desc", "Loot is here");

	private static readonly PingStyle LootMarker = new PingStyle(11, 0, LootTitle, LootDesc, PingType.Loot);

	private static readonly Phrase NodeTitle = new Phrase("ping_node", "Node");

	private static readonly Phrase NodeDesc = new Phrase("ping_node_desc", "An ore node is here");

	private static readonly PingStyle NodeMarker = new PingStyle(10, 4, NodeTitle, NodeDesc, PingType.Node);

	private static readonly Phrase GunTitle = new Phrase("ping_gun", "Weapon");

	private static readonly Phrase GunDesc = new Phrase("ping_weapon_desc", "A dropped weapon is here");

	private static readonly PingStyle GunMarker = new PingStyle(9, 5, GunTitle, GunDesc, PingType.Gun);

	private static readonly PingStyle BuildMarker = new PingStyle(12, 5, new Phrase("", ""), new Phrase("", ""), PingType.Build);

	private TimeSince lastTick;

	private List<(ItemDefinition item, PingType pingType)> tutorialDesiredResource = new List<(ItemDefinition, PingType)>();

	private List<(NetworkableId id, PingType pingType)> pingedEntities = new List<(NetworkableId, PingType)>();

	private TimeSince lastResourcePingUpdate;

	private bool _playerStateDirty;

	private string _wipeId;

	private float cachedVehicleBuildingPrivilegeTime;

	private BaseEntity cachedVehicleBuildingPrivilege;

	private bool cachedVehicleBuildingPrivilegeBlocked;

	private Vector3 cachedVehicleBuildingPrivilegePosition;

	private float cachedEntityBuildingPrivilegeTime;

	private BaseEntity cachedEntityBuildingPrivilege;

	private bool cachedEntityBuildingPrivilegeBlocked;

	private Vector3 cachedEntityBuildingPrivilegePosition;

	[NonSerialized]
	private TimeUntil mortarCooldown;

	[NonSerialized]
	public Dictionary<int, FiredProjectile> firedProjectiles = new Dictionary<int, FiredProjectile>();

	private const float radiationDamageTime = 1f;

	private const float radiationDamageThreshold = 2500f;

	private const float radiationRatioAdjustment = 0.05f;

	private const float containerCheckRadTime = 2500f;

	private const float containerRadRatioAdjustment = 0.05f;

	private Action inflictInventoryRadsAction;

	private float inventoryRads;

	private bool hasOpenedLoot;

	private List<ItemContainer> radiationCheckContainers = new List<ItemContainer>();

	private float containerRads;

	private Action inflictRadsAction;

	private Action checkRadsAction;

	private const string RagdollPath = "assets/prefabs/player/player_temp_ragdoll.prefab";

	private const int WILDERNESS = 1;

	private const int MONUMENT = 2;

	private const int BASE = 4;

	private const int FLYING = 8;

	private const int BOATING = 16;

	private const int SWIMMING = 32;

	private const int DRIVING = 64;

	[ServerVar]
	[Help("How many milliseconds to budget for processing life story updates per frame")]
	public static float lifeStoryFramebudgetms = 0.25f;

	[NonSerialized]
	public PlayerLifeStory lifeStory;

	[NonSerialized]
	public PlayerLifeStory previousLifeStory;

	public const float TimeCategoryUpdateFrequency = 7f;

	public float nextTimeCategoryUpdate;

	private bool hasSentPresenceState;

	private bool LifeStoryInWilderness;

	private bool LifeStoryInMonument;

	private bool LifeStoryInBase;

	private bool LifeStoryFlying;

	private bool LifeStoryBoating;

	private bool LifeStorySwimming;

	private bool LifeStoryDriving;

	private bool waitingForLifeStoryUpdate;

	public static LifeStoryWorkQueue lifeStoryQueue = new LifeStoryWorkQueue();

	[CanBeNull]
	private DeathInfo cachedOverrideDeathInfo;

	[NonSerialized]
	public PlayerStatistics stats;

	[NonSerialized]
	public GameObjectRef DeathIconOverride;

	[NonSerialized]
	public ItemId svActiveItemID;

	[NonSerialized]
	public float NextChatTime;

	[NonSerialized]
	public float nextSuicideTime;

	[NonSerialized]
	public float nextRespawnTime;

	[NonSerialized]
	public string respawnId;

	[NonSerialized]
	public float nextMuteCheckTime;

	[NonSerialized]
	public int server_paintballColor;

	[NonSerialized]
	public bool isInvisible;

	public static ListHashSet<BasePlayer> invisPlayers = new ListHashSet<BasePlayer>();

	public static ListHashSet<BasePlayer> playersRecordingClientDemos = new ListHashSet<BasePlayer>();

	private RealTimeUntil timeUntilLoadingExpires;

	public Dictionary<ulong, float> lastPlayerVisibility = new Dictionary<ulong, float>();

	public static NativeArray<IntPtr> ClientHandles;

	private byte onLadderCount;

	public Vector3 viewAngles;

	private static ulong botIdCounter = 1uL;

	private static List<ulong> freeBotIds = new List<ulong>();

	public float lastSubscriptionTick;

	public double lastPlayerTick;

	public float sleepStartTime = -1f;

	public float fallTickRate = 0.1f;

	public float lastFallTime;

	public float fallVelocity;

	private DeathBlow cachedNonSuicideHit;

	private float timeSinceLastStung;

	private float timeSinceLastStungRPC;

	public static ListHashSet<BasePlayer> activePlayerList = new ListHashSet<BasePlayer>();

	public static ListHashSet<BasePlayer> sleepingPlayerList = new ListHashSet<BasePlayer>();

	public static Dictionary<ulong, BasePlayer> activePlayerLookup = new Dictionary<ulong, BasePlayer>();

	public static Dictionary<ulong, BasePlayer> sleepingPlayerLookup = new Dictionary<ulong, BasePlayer>();

	public static ListHashSet<BasePlayer> bots = new ListHashSet<BasePlayer>();

	private readonly object[] noParameterCommandArgs = new object[0];

	private readonly object[] singleParameterCommandArgs = new object[1];

	private readonly object[] doubleParameterCommandArgs = new object[2];

	private readonly object[] tripleParameterCommandArgs = new object[3];

	private readonly object[] quadParameterCommandArgs = new object[4];

	public float cachedCraftLevel;

	public float nextCheckTime;

	private NetworkableId lastSentActiveWorkbenchId;

	private Workbench _cachedWorkbench;

	private Action _actionMonitorServerDemoRecording;

	public PersistantPlayer cachedPersistantPlayer;

	private static OceanPaths cachedOceanPaths = null;

	private static readonly Phrase TakingRestraintItemError = new Phrase("error.takingrestraintitem", "Cannot take the item keeping the player restrained!");

	[ServerVar(Help = "(Generated) Per-frame CPU budget in milliseconds for the bot collider work queue that updates NPC physics colliders")]
	public static float botColliderFrameBudgetMs = 0.05f;

	public static BotColliderWorkQueue botColliderWorkQueue = new BotColliderWorkQueue();

	[ServerVar(Help = "(Generated) Per-frame CPU budget in milliseconds for processing the player relationship (contacts/team) update queue")]
	public static float relationshipUpdateQueueFrameBudgetMs = 0.05f;

	[ServerVar(Saved = true, Help = "(Generated) When enabled, server occlusion is taken into account when updating player relationship visibility data; saved between restarts")]
	public static bool allowRelationshipServerOcclusion = true;

	public static RelationshipUpdateQueue relationshipUpdateQueue = new RelationshipUpdateQueue();

	private TimeSince lastAcquaintanceUpdate;

	public static HashSet<(ulong fromId, ulong toId)> OcclusionFrameCache = new HashSet<(ulong, ulong)>();

	private List<Network.Connection> unoccludedSubscribers;

	private bool IsSpectatingTeamInfo;

	private TimeSince lastSpectateTeamInfoUpdate;

	public int SpectateOffset = 1000000;

	public string spectateFilter = "";

	private BasePlayer spectatingTarget;

	private TimeSince timeSinceLastWaterSplash;

	private List<NearbyStash> nearbyStashes = new List<NearbyStash>();

	public float lastUpdateTime = float.NegativeInfinity;

	public float cachedThreatLevel;

	private float hostilePauseTime = float.NegativeInfinity;

	[NonSerialized]
	public float weaponDrawnDuration;

	private TimeSince timeLastInCombatZone;

	[NonSerialized]
	public float lastTickTime;

	[NonSerialized]
	private int lastEACTickIndex;

	[NonSerialized]
	public float lastStallTime;

	[NonSerialized]
	private float stallProtectionTime;

	[NonSerialized]
	public float lastInputTime;

	[NonSerialized]
	private float tutorialKickTime;

	[NonSerialized]
	public ItemId? restraintItemId;

	[NonSerialized]
	public int ActivePlayerInd = -1;

	public PlayerTick lastReceivedTick = new PlayerTick();

	private List<IReceivePlayerTickListener> receiveTickListeners = new List<IReceivePlayerTickListener>();

	private readonly TimeAverageValue ticksPerSecond = new TimeAverageValue();

	private readonly TimeAverageValue rawTicksPerSecond = new TimeAverageValue();

	public Deque<Vector3> eyeHistory = new Deque<Vector3>(16);

	public TickHistory tickHistory = new TickHistory(16);

	public static NativeArray<EACTickState> EACTickStates;

	public static PlayerServerStates PlayerStates;

	private float startTutorialCooldown;

	public float nextUnderwearValidationTime;

	public uint lastValidUnderwearSkin;

	private static Comparison<BasePlayer> _displayNameComparison;

	private InjureState playerInjureState;

	public float woundedDuration;

	public float lastWoundedStartTime = float.NegativeInfinity;

	public float healingWhileCrawling;

	public bool woundedByFallDamage;

	private const float INCAPACITATED_HEALTH_MIN = 2f;

	private const float INCAPACITATED_HEALTH_MAX = 6f;

	public const int MaxBotIdRange = 10000000;

	[Header("BasePlayer")]
	public GameObjectRef fallDamageEffect;

	public GameObjectRef drownEffect;

	[InspectorFlags]
	public PlayerFlags playerFlags;

	private HiddenValue<PlayerEyes> eyesValue = Pool.Get<HiddenValue<PlayerEyes>>();

	private HiddenValue<PlayerInventory> inventoryValue = Pool.Get<HiddenValue<PlayerInventory>>();

	[NonSerialized]
	public PlayerBlueprints blueprints;

	[NonSerialized]
	public PlayerMetabolism metabolism;

	[NonSerialized]
	public PlayerModifiers modifiers;

	private HiddenValue<CapsuleCollider> colliderValue = Pool.Get<HiddenValue<CapsuleCollider>>();

	public PlayerBelt Belt;

	public Rigidbody playerRigidbody;

	[NonSerialized]
	public EncryptedValue<ulong> userID = 0uL;

	[NonSerialized]
	public string UserIDString;

	[NonSerialized]
	public int gamemodeteam = -1;

	[NonSerialized]
	public int reputation;

	protected string _displayName;

	public string _lastSetName;

	public const float crouchSpeed = 1.7f;

	public const float walkSpeed = 2.8f;

	public const float runSpeed = 5.5f;

	public const float crawlSpeed = 0.72f;

	public CapsuleColliderInfo playerColliderStanding;

	public CapsuleColliderInfo playerColliderDucked;

	public CapsuleColliderInfo playerColliderCrawling;

	public CapsuleColliderInfo playerColliderLyingDown;

	public ProtectionProperties cachedProtection;

	private ProtectionProperties protectionAgainstNPCs;

	public const float DuckedHeight = 1.1f;

	public const float Height = 1.8f;

	public const float Radius = 0.5f;

	public const float JumpHeight = 1.5f;

	public float nextColliderRefreshTime = -1f;

	public float weaponMoveSpeedScale = 1f;

	public bool clothingBlocksAiming;

	public float clothingMoveSpeedReduction;

	public float clothingWaterSpeedBonus;

	public float clothingAccuracyBonus;

	public bool equippingBlocked;

	public float eggVision;

	public PhoneController activeTelephone;

	public BaseEntity designingAIEntity;

	[NonSerialized]
	public IPlayer IPlayer;

	public float ViolationLevel
	{
		get
		{
			if (ActivePlayerInd != -1)
			{
				return AntiHack.PlayerStates[ActivePlayerInd].ViolationLevel;
			}
			return 0f;
		}
	}

	public Phrase LootPanelTitle => Phrase.op_Implicit(displayName);

	public bool IsReceivingSnapshot => HasPlayerFlag(PlayerFlags.ReceivingSnapshot);

	public bool IsAdmin => HasPlayerFlag(PlayerFlags.IsAdmin);

	public bool IsDeveloper => HasPlayerFlag(PlayerFlags.IsDeveloper);

	public bool IsInCreativeMode
	{
		get
		{
			if (!Creative.allUsers)
			{
				return HasPlayerFlag(PlayerFlags.CreativeMode);
			}
			return true;
		}
	}

	public bool AllSkinsLocked => GetSkinsAccessLevel() == -1;

	public bool AllSkinsUnlocked => GetSkinsAccessLevel() == 1;

	public bool DefaultSkinAccess
	{
		get
		{
			int skinsAccessLevel = GetSkinsAccessLevel();
			if (skinsAccessLevel != -1)
			{
				return skinsAccessLevel != 1;
			}
			return false;
		}
	}

	public bool IsAiming => HasPlayerFlag(PlayerFlags.Aiming);

	public bool IsFlying
	{
		get
		{
			if (modelState == null)
			{
				return false;
			}
			return modelState.flying;
		}
	}

	public bool IsConnected
	{
		get
		{
			if (base.isServer)
			{
				if (Net.sv == null)
				{
					return false;
				}
				if (net == null)
				{
					return false;
				}
				if (net.connection == null)
				{
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsInTutorial => HasPlayerFlag(PlayerFlags.IsInTutorial);

	public bool IsRestrained
	{
		get
		{
			if (IsAlive())
			{
				return HasPlayerFlag(PlayerFlags.IsRestrained);
			}
			return false;
		}
	}

	public bool IsRestrainedOrSurrendering
	{
		get
		{
			if (!IsRestrained)
			{
				return CurrentGestureIsSurrendering;
			}
			return true;
		}
	}

	public bool ShouldRunFogOfWar
	{
		get
		{
			if (!ConVar.Server.fogofwar || CurrentFogMode != FogMode.Mainland)
			{
				if (ConVar.Server.deepSeaFogofwar)
				{
					return CurrentFogMode == FogMode.DeepSea;
				}
				return false;
			}
			return true;
		}
	}

	public FogMode CurrentFogMode
	{
		get
		{
			if (DeepSeaManager.IsInsideDeepSea((BaseNetworkable)this))
			{
				return FogMode.DeepSea;
			}
			return FogMode.Mainland;
		}
	}

	public bool InGesture
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)currentGesture != (Object)null)
			{
				if (!(TimeUntil.op_Implicit(gestureFinishedTime) > 0f))
				{
					return currentGesture.animationType == GestureConfig.AnimationType.Loop;
				}
				return true;
			}
			return false;
		}
	}

	private bool CurrentGestureBlocksMovement
	{
		get
		{
			if (InGesture)
			{
				return currentGesture.movementMode == GestureConfig.MovementCapabilities.NoMovement;
			}
			return false;
		}
	}

	public bool CurrentGestureIsDance
	{
		get
		{
			if (InGesture)
			{
				return currentGesture.actionType == GestureConfig.GestureActionType.DanceAchievement;
			}
			return false;
		}
	}

	public bool CurrentGestureIsFullBody
	{
		get
		{
			if (InGesture)
			{
				return currentGesture.playerModelLayer == GestureConfig.PlayerModelLayer.FullBody;
			}
			return false;
		}
	}

	public bool CurrentGestureIsUpperBody
	{
		get
		{
			if (InGesture)
			{
				return currentGesture.playerModelLayer == GestureConfig.PlayerModelLayer.UpperBody;
			}
			return false;
		}
	}

	public bool CurrentGestureIsSurrendering
	{
		get
		{
			if (InGesture)
			{
				return currentGesture.actionType == GestureConfig.GestureActionType.Surrender;
			}
			return false;
		}
	}

	private bool InGestureCancelCooldown => TimeSince.op_Implicit(blockHeldInputTimer) < 0.5f;

	private Action actionTimeoutGestureServer => TimeoutGestureServer;

	private Action actionMonitorLoopingGesture => MonitorLoopingGesture;

	private Action actionBotRPSRandomise => BotRPSRandomise;

	private Action actionMonitorRPSGame => MonitorRPSGame;

	public Action actionServer_CancelGesture => Server_CancelGesture;

	public RelationshipManager.PlayerTeam Team
	{
		get
		{
			if ((Object)(object)RelationshipManager.ServerInstance == (Object)null)
			{
				return null;
			}
			return RelationshipManager.ServerInstance.FindTeam(currentTeam);
		}
	}

	private bool CanUseMapMarkers
	{
		get
		{
			BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(base.isServer);
			if ((Object)(object)activeGameMode != (Object)null)
			{
				return activeGameMode.mapMarkers;
			}
			return true;
		}
	}

	public MapNote ServerCurrentDeathNote
	{
		get
		{
			return State.deathMarker;
		}
		set
		{
			State.deathMarker = value;
		}
	}

	private Action actionAssignFollowUpMission
	{
		get
		{
			if (_actionAssignFollowUpMission == null)
			{
				_actionAssignFollowUpMission = AssignFollowUpMission;
			}
			return _actionAssignFollowUpMission;
		}
	}

	public bool HasPendingFollowupMission => IsInvoking(actionAssignFollowUpMission);

	public ModelState modelStateTick { get; private set; }

	public bool isMounted => mounted.IsValid(base.isServer);

	public bool isMountingHidingWeapon
	{
		get
		{
			if (isMounted)
			{
				return !GetMounted().CanHoldItems();
			}
			return false;
		}
	}

	private int TotalPingCount
	{
		get
		{
			if (State.pings == null)
			{
				return 0;
			}
			return State.pings.Count;
		}
	}

	public PlayerState State
	{
		get
		{
			if ((ulong)userID == 0L)
			{
				throw new InvalidOperationException("Cannot get player state without a SteamID");
			}
			return SingletonComponent<ServerMgr>.Instance.playerStateManager.Get(userID);
		}
	}

	public string WipeId
	{
		get
		{
			if (_wipeId == null)
			{
				_wipeId = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(userID);
			}
			return _wipeId;
		}
	}

	public bool hasPreviousLife => previousLifeStory != null;

	public int currentTimeCategory { get; private set; }

	public virtual BaseNpc.AiStatistics.FamilyEnum Family => BaseNpc.AiStatistics.FamilyEnum.Player;

	public override float PositionTickRate
	{
		protected get
		{
			return -1f;
		}
	}

	public int DebugMapMarkerIndex { get; set; }

	public bool PlayHeavyLandingAnimation { get; set; }

	public bool requestingReputationUpdate { get; set; }

	public ServerOcclusion.Grid Chunk { get; set; }

	public ServerOcclusion.SubGrid SubGrid { get; set; }

	public Vector3 estimatedVelocity { get; private set; }

	public Vector3 estimatedVelocityClamped => Vector3.ClampMagnitude(estimatedVelocity, GetMaxSpeed());

	public float inferedSpeed
	{
		get
		{
			if (estimatedSpeed < 0.01f)
			{
				return 0f;
			}
			if (modelState.sprinting)
			{
				return 5.5f;
			}
			if (modelState.ducked)
			{
				return 1.7f;
			}
			return 2.8f;
		}
	}

	public Vector3 inferedVelocity
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			float num = inferedSpeed;
			Vector3 val = estimatedVelocity;
			return num * ((Vector3)(ref val)).normalized;
		}
	}

	public float estimatedSpeed { get; private set; }

	public float estimatedSpeed2D { get; private set; }

	public int secondsConnected { get; private set; }

	public float desyncTimeRaw { get; set; }

	public float desyncTimeClamped { get; set; }

	public float secondsSleeping
	{
		get
		{
			if (sleepStartTime == -1f || !IsSleeping())
			{
				return 0f;
			}
			return Time.time - sleepStartTime;
		}
	}

	public static IEnumerable<BasePlayer> allPlayerList
	{
		get
		{
			HashSet<BasePlayer> set = Pool.Get<HashSet<BasePlayer>>();
			Enumerator<BasePlayer> enumerator = sleepingPlayerList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BasePlayer current = enumerator.Current;
					if (set.Add(current))
					{
						yield return current;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			enumerator = activePlayerList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BasePlayer current2 = enumerator.Current;
					if (set.Add(current2))
					{
						yield return current2;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			Pool.FreeUnmanaged<BasePlayer>(ref set);
		}
	}

	public float currentCraftLevel
	{
		get
		{
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			if (triggers == null)
			{
				_cachedWorkbench = null;
				return 0f;
			}
			if (nextCheckTime > Time.realtimeSinceStartup)
			{
				return cachedCraftLevel;
			}
			_cachedWorkbench = null;
			nextCheckTime = Time.realtimeSinceStartup + Random.Range(0.4f, 0.5f);
			float num = 0f;
			int num2 = -1;
			for (int i = 0; i < triggers.Count; i++)
			{
				TriggerWorkbench triggerWorkbench = triggers[i] as TriggerWorkbench;
				if ((Object)(object)triggerWorkbench == (Object)null || (Object)(object)triggerWorkbench.parentBench == (Object)null || triggerWorkbench.parentBench.isClient || !triggerWorkbench.parentBench.IsVisible(eyes.position))
				{
					continue;
				}
				float num3 = triggerWorkbench.WorkbenchLevel();
				if (num3 > num)
				{
					num = num3;
					_cachedWorkbench = triggerWorkbench.parentBench;
					num2 = triggerWorkbench.parentBench.InstalledUpgradeCount;
				}
				else if (num3 == num)
				{
					int installedUpgradeCount = triggerWorkbench.parentBench.InstalledUpgradeCount;
					if (installedUpgradeCount > num2)
					{
						_cachedWorkbench = triggerWorkbench.parentBench;
						num2 = installedUpgradeCount;
					}
				}
			}
			cachedCraftLevel = num;
			return num;
		}
	}

	public float currentComfort
	{
		get
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			float num = 0f;
			if (isMounted)
			{
				num = GetMounted().GetComfort();
			}
			if (triggers != null)
			{
				for (int i = 0; i < triggers.Count; i++)
				{
					TriggerComfort triggerComfort = triggers[i] as TriggerComfort;
					if (!((Object)(object)triggerComfort == (Object)null))
					{
						float num2 = triggerComfort.CalculateComfort(((Component)this).transform.position, this);
						if (num2 > num)
						{
							num = num2;
						}
					}
				}
			}
			float num3 = (((Object)(object)modifiers != (Object)null) ? modifiers.GetValue(Modifier.ModifierType.Comfort) : 0f);
			return num + num3;
		}
	}

	private Action actionMonitorServerDemoRecording => MonitorServerDemoRecording;

	public PersistantPlayer PersistantPlayerInfo
	{
		get
		{
			if (cachedPersistantPlayer == null)
			{
				cachedPersistantPlayer = SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerInfo(userID);
			}
			return cachedPersistantPlayer;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			cachedPersistantPlayer = value;
			SingletonComponent<ServerMgr>.Instance.persistance.SetPlayerInfo(userID, value);
		}
	}

	public bool wantsSpectate { get; set; }

	public bool IsBeingSpectated
	{
		get
		{
			if (net != null)
			{
				return net.SubStrategy is SpectatedSubStrategy;
			}
			return false;
		}
	}

	public BasePlayer SpectatingTarget => spectatingTarget;

	public TimeSince TimeSinceLastWaterSplash => timeSinceLastWaterSplash;

	public InputState serverInput { get; private set; } = new InputState();

	public float timeSinceLastTick
	{
		get
		{
			if (lastTickTime == 0f)
			{
				return 0f;
			}
			return Time.time - lastTickTime;
		}
	}

	public float timeSinceLastStall
	{
		get
		{
			if (lastStallTime == 0f)
			{
				return 60f;
			}
			return Time.time - lastStallTime;
		}
	}

	public float IdleTime
	{
		get
		{
			if (lastInputTime == 0f)
			{
				return 0f;
			}
			return Time.time - lastInputTime;
		}
	}

	public bool isStalled
	{
		get
		{
			if (IsDead() || IsSleeping())
			{
				lastStallTime = 0f;
				return false;
			}
			if (stallProtectionTime <= 0f && timeSinceLastTick != 0f && timeSinceLastTick > ConVar.AntiHack.rpcstallthreshold)
			{
				lastStallTime = Time.time;
				return true;
			}
			return false;
		}
	}

	public bool wasStalled
	{
		get
		{
			if (stallProtectionTime <= 0f)
			{
				if (!isStalled)
				{
					return timeSinceLastStall < ConVar.AntiHack.rpcstallfade;
				}
				return true;
			}
			return false;
		}
	}

	public Vector3 tickViewAngles { get; private set; }

	public Vector3 tickMouseDelta { get; private set; }

	public int tickHistoryCapacity => Mathf.Max(1, Mathf.CeilToInt((float)ticksPerSecond.Calculate() * ConVar.AntiHack.tickhistorytime));

	public Matrix4x4 tickHistoryMatrix
	{
		get
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)((Component)this).transform.parent))
			{
				return Matrix4x4.identity;
			}
			return ((Component)this).transform.parent.localToWorldMatrix;
		}
	}

	public ulong rawTickCount { get; set; }

	public static PlayerServerStates.ReadOnly PlayerReadOnlyStates => PlayerStates.AsReadOnly();

	public TutorialItemAllowance CurrentTutorialAllowance { get; private set; }

	public static Comparison<BasePlayer> DisplayNameComparison
	{
		get
		{
			if (_displayNameComparison == null)
			{
				_displayNameComparison = CompareByDisplayName;
			}
			return _displayNameComparison;
		}
	}

	public InjureState PlayerInjureState
	{
		get
		{
			return playerInjureState;
		}
		set
		{
			if (playerInjureState != value)
			{
				Facepunch.Rust.Analytics.Azure.OnPlayerChangeInjureState(this, PlayerInjureState, value);
				playerInjureState = value;
			}
		}
	}

	public float TimeSinceWoundedStarted => Time.realtimeSinceStartup - lastWoundedStartTime;

	public Network.Connection Connection
	{
		get
		{
			if (net != null)
			{
				return net.connection;
			}
			return null;
		}
	}

	public bool IsBot => (ulong)userID < 10000000;

	public PlayerEyes eyes
	{
		get
		{
			if (eyesValue == null)
			{
				return null;
			}
			return eyesValue.Get();
		}
		set
		{
			eyesValue.Set(value);
		}
	}

	public PlayerInventory inventory
	{
		get
		{
			if (inventoryValue == null)
			{
				return null;
			}
			return inventoryValue.Get();
		}
	}

	public CapsuleCollider playerCollider
	{
		get
		{
			if (colliderValue == null)
			{
				return null;
			}
			return colliderValue.Get();
		}
	}

	public virtual string displayName
	{
		get
		{
			return NameHelper.Get(userID, _displayName, base.isClient);
		}
		set
		{
			if (!(_lastSetName == value))
			{
				_lastSetName = value;
				_displayName = SanitizePlayerNameString(value, userID);
			}
		}
	}

	public override TraitFlag Traits => base.Traits | TraitFlag.Human | TraitFlag.Food | TraitFlag.Meat | TraitFlag.Alive;

	public bool HasActiveTelephone => (Object)(object)activeTelephone != (Object)null;

	public bool IsDesigningAI => (Object)(object)designingAIEntity != (Object)null;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		//IL_1f82: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f9b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2375: Unknown result type (might be due to invalid IL or missing references)
		//IL_21ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_347a: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BasePlayer.OnRpcMessage"))
		{
			if (rpc == 935768323 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ClientKeepConnectionAlive"));
				}
				using (TimeWarning.New("ClientKeepConnectionAlive"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(935768323u, "ClientKeepConnectionAlive", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ClientKeepConnectionAlive(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ClientKeepConnectionAlive");
					}
				}
				return true;
			}
			if (rpc == 3782818894u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ClientLoadingComplete"));
				}
				using (TimeWarning.New("ClientLoadingComplete"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(3782818894u, "ClientLoadingComplete", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ClientLoadingComplete(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in ClientLoadingComplete");
					}
				}
				return true;
			}
			if (rpc == 1217424607 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - FogImageUpdate"));
				}
				using (TimeWarning.New("FogImageUpdate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1217424607u, "FogImageUpdate", this, player, 16uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1217424607u, "FogImageUpdate", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							FogImageUpdate(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in FogImageUpdate");
					}
				}
				return true;
			}
			if (rpc == 1497207530 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - IssuePetCommand"));
				}
				using (TimeWarning.New("IssuePetCommand"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							IssuePetCommand(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in IssuePetCommand");
					}
				}
				return true;
			}
			if (rpc == 2041023702 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - IssuePetCommandRaycast"));
				}
				using (TimeWarning.New("IssuePetCommandRaycast"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg6 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							IssuePetCommandRaycast(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in IssuePetCommandRaycast");
					}
				}
				return true;
			}
			if (rpc == 495414158 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - NotifyDebugCameraEnded"));
				}
				using (TimeWarning.New("NotifyDebugCameraEnded"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg7 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							NotifyDebugCameraEnded(msg7);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in NotifyDebugCameraEnded");
					}
				}
				return true;
			}
			if (rpc == 3441821928u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - OnFeedbackReport"));
				}
				using (TimeWarning.New("OnFeedbackReport"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3441821928u, "OnFeedbackReport", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(3441821928u, "OnFeedbackReport", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg8 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							OnFeedbackReport(msg8);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
						player.Kick("RPC Error in OnFeedbackReport");
					}
				}
				return true;
			}
			if (rpc == 1998170713 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - OnPlayerLanded"));
				}
				using (TimeWarning.New("OnPlayerLanded"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(1998170713u, "OnPlayerLanded", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg9 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							OnPlayerLanded(msg9);
						}
					}
					catch (Exception ex8)
					{
						Debug.LogException(ex8);
						player.Kick("RPC Error in OnPlayerLanded");
					}
				}
				return true;
			}
			if (rpc == 2147041557 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - OnPlayerReported"));
				}
				using (TimeWarning.New("OnPlayerReported"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2147041557u, "OnPlayerReported", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(2147041557u, "OnPlayerReported", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg10 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							OnPlayerReported(msg10);
						}
					}
					catch (Exception ex9)
					{
						Debug.LogException(ex9);
						player.Kick("RPC Error in OnPlayerReported");
					}
				}
				return true;
			}
			if (rpc == 363681694 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - OnProjectileAttack"));
				}
				using (TimeWarning.New("OnProjectileAttack"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(363681694u, "OnProjectileAttack", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg11 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							OnProjectileAttack(msg11);
						}
					}
					catch (Exception ex10)
					{
						Debug.LogException(ex10);
						player.Kick("RPC Error in OnProjectileAttack");
					}
				}
				return true;
			}
			if (rpc == 1500391289 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - OnProjectileRicochet"));
				}
				using (TimeWarning.New("OnProjectileRicochet"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(1500391289u, "OnProjectileRicochet", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg12 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							OnProjectileRicochet(msg12);
						}
					}
					catch (Exception ex11)
					{
						Debug.LogException(ex11);
						player.Kick("RPC Error in OnProjectileRicochet");
					}
				}
				return true;
			}
			if (rpc == 2324190493u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - OnProjectileUpdate"));
				}
				using (TimeWarning.New("OnProjectileUpdate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(2324190493u, "OnProjectileUpdate", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg13 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							OnProjectileUpdate(msg13);
						}
					}
					catch (Exception ex12)
					{
						Debug.LogException(ex12);
						player.Kick("RPC Error in OnProjectileUpdate");
					}
				}
				return true;
			}
			if (rpc == 3167788018u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - PerformanceReport"));
				}
				using (TimeWarning.New("PerformanceReport"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3167788018u, "PerformanceReport", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(3167788018u, "PerformanceReport", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg14 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							PerformanceReport(msg14);
						}
					}
					catch (Exception ex13)
					{
						Debug.LogException(ex13);
						player.Kick("RPC Error in PerformanceReport");
					}
				}
				return true;
			}
			if (rpc == 4081064578u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - PlayerRequestedTutorialStart"));
				}
				using (TimeWarning.New("PlayerRequestedTutorialStart"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4081064578u, "PlayerRequestedTutorialStart", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(4081064578u, "PlayerRequestedTutorialStart", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg15 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							PlayerRequestedTutorialStart(msg15);
						}
					}
					catch (Exception ex14)
					{
						Debug.LogException(ex14);
						player.Kick("RPC Error in PlayerRequestedTutorialStart");
					}
				}
				return true;
			}
			if (rpc == 3227458058u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ReqLightToggle"));
				}
				using (TimeWarning.New("ReqLightToggle"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(3227458058u, "ReqLightToggle", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg16 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ReqLightToggle(msg16);
						}
					}
					catch (Exception ex15)
					{
						Debug.LogException(ex15);
						player.Kick("RPC Error in ReqLightToggle");
					}
				}
				return true;
			}
			if (rpc == 1280830738 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ReqLightToggleEntity"));
				}
				using (TimeWarning.New("ReqLightToggleEntity"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(1280830738u, "ReqLightToggleEntity", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg17 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ReqLightToggleEntity(msg17);
						}
					}
					catch (Exception ex16)
					{
						Debug.LogException(ex16);
						player.Kick("RPC Error in ReqLightToggleEntity");
					}
				}
				return true;
			}
			if (rpc == 56793194 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RequestJoinGesture"));
				}
				using (TimeWarning.New("RequestJoinGesture"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(56793194u, "RequestJoinGesture", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg18 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RequestJoinGesture(msg18);
						}
					}
					catch (Exception ex17)
					{
						Debug.LogException(ex17);
						player.Kick("RPC Error in RequestJoinGesture");
					}
				}
				return true;
			}
			if (rpc == 1024003327 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RequestParachuteDeploy"));
				}
				using (TimeWarning.New("RequestParachuteDeploy"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1024003327u, "RequestParachuteDeploy", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1024003327u, "RequestParachuteDeploy", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg19 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RequestParachuteDeploy(msg19);
						}
					}
					catch (Exception ex18)
					{
						Debug.LogException(ex18);
						player.Kick("RPC Error in RequestParachuteDeploy");
					}
				}
				return true;
			}
			if (rpc == 52352806 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RequestRespawnInformation"));
				}
				using (TimeWarning.New("RequestRespawnInformation"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(52352806u, "RequestRespawnInformation", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(52352806u, "RequestRespawnInformation", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg20 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RequestRespawnInformation(msg20);
						}
					}
					catch (Exception ex19)
					{
						Debug.LogException(ex19);
						player.Kick("RPC Error in RequestRespawnInformation");
					}
				}
				return true;
			}
			if (rpc == 1774681338 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RequestServerEmoji"));
				}
				using (TimeWarning.New("RequestServerEmoji"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1774681338u, "RequestServerEmoji", this, player, 1uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RequestServerEmoji();
						}
					}
					catch (Exception ex20)
					{
						Debug.LogException(ex20);
						player.Kick("RPC Error in RequestServerEmoji");
					}
				}
				return true;
			}
			if (rpc == 970468557 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Assist"));
				}
				using (TimeWarning.New("RPC_Assist"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(970468557u, "RPC_Assist", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg21 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_Assist(msg21);
						}
					}
					catch (Exception ex21)
					{
						Debug.LogException(ex21);
						player.Kick("RPC Error in RPC_Assist");
					}
				}
				return true;
			}
			if (rpc == 3263238541u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_KeepAlive"));
				}
				using (TimeWarning.New("RPC_KeepAlive"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3263238541u, "RPC_KeepAlive", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg22 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_KeepAlive(msg22);
						}
					}
					catch (Exception ex22)
					{
						Debug.LogException(ex22);
						player.Kick("RPC Error in RPC_KeepAlive");
					}
				}
				return true;
			}
			if (rpc == 3692395068u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_LootPlayer"));
				}
				using (TimeWarning.New("RPC_LootPlayer"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3692395068u, "RPC_LootPlayer", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg23 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_LootPlayer(msg23);
						}
					}
					catch (Exception ex23)
					{
						Debug.LogException(ex23);
						player.Kick("RPC Error in RPC_LootPlayer");
					}
				}
				return true;
			}
			if (rpc == 2659547586u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ReqDoRestrainedPush"));
				}
				using (TimeWarning.New("RPC_ReqDoRestrainedPush"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2659547586u, "RPC_ReqDoRestrainedPush", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2659547586u, "RPC_ReqDoRestrainedPush", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2659547586u, "RPC_ReqDoRestrainedPush", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_ReqDoRestrainedPush(rpc2);
						}
					}
					catch (Exception ex24)
					{
						Debug.LogException(ex24);
						player.Kick("RPC Error in RPC_ReqDoRestrainedPush");
					}
				}
				return true;
			}
			if (rpc == 3974264977u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ReqEquipHood"));
				}
				using (TimeWarning.New("RPC_ReqEquipHood"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3974264977u, "RPC_ReqEquipHood", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3974264977u, "RPC_ReqEquipHood", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3974264977u, "RPC_ReqEquipHood", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_ReqEquipHood(rpc3);
						}
					}
					catch (Exception ex25)
					{
						Debug.LogException(ex25);
						player.Kick("RPC Error in RPC_ReqEquipHood");
					}
				}
				return true;
			}
			if (rpc == 4144905368u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ReqForceMountNearest"));
				}
				using (TimeWarning.New("RPC_ReqForceMountNearest"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4144905368u, "RPC_ReqForceMountNearest", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(4144905368u, "RPC_ReqForceMountNearest", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(4144905368u, "RPC_ReqForceMountNearest", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_ReqForceMountNearest(rpc4);
						}
					}
					catch (Exception ex26)
					{
						Debug.LogException(ex26);
						player.Kick("RPC Error in RPC_ReqForceMountNearest");
					}
				}
				return true;
			}
			if (rpc == 3816898909u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ReqForceSwapSeat"));
				}
				using (TimeWarning.New("RPC_ReqForceSwapSeat"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3816898909u, "RPC_ReqForceSwapSeat", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3816898909u, "RPC_ReqForceSwapSeat", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3816898909u, "RPC_ReqForceSwapSeat", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_ReqForceSwapSeat(rpc5);
						}
					}
					catch (Exception ex27)
					{
						Debug.LogException(ex27);
						player.Kick("RPC Error in RPC_ReqForceSwapSeat");
					}
				}
				return true;
			}
			if (rpc == 626234931 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ReqRemoveCuffs"));
				}
				using (TimeWarning.New("RPC_ReqRemoveCuffs"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(626234931u, "RPC_ReqRemoveCuffs", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(626234931u, "RPC_ReqRemoveCuffs", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(626234931u, "RPC_ReqRemoveCuffs", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc6 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_ReqRemoveCuffs(rpc6);
						}
					}
					catch (Exception ex28)
					{
						Debug.LogException(ex28);
						player.Kick("RPC Error in RPC_ReqRemoveCuffs");
					}
				}
				return true;
			}
			if (rpc == 2289764809u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ReqRemoveHood"));
				}
				using (TimeWarning.New("RPC_ReqRemoveHood"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2289764809u, "RPC_ReqRemoveHood", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2289764809u, "RPC_ReqRemoveHood", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2289764809u, "RPC_ReqRemoveHood", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc7 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_ReqRemoveHood(rpc7);
						}
					}
					catch (Exception ex29)
					{
						Debug.LogException(ex29);
						player.Kick("RPC Error in RPC_ReqRemoveHood");
					}
				}
				return true;
			}
			if (rpc == 1539133504 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_StartClimb"));
				}
				using (TimeWarning.New("RPC_StartClimb"))
				{
					using (TimeWarning.New("Conditions"))
					{
						long position = msg.read.Position;
						msg.read.Read<bool>();
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Read<NetworkableId>();
						msg.read.Position = position;
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg24 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_StartClimb(msg24);
						}
					}
					catch (Exception ex30)
					{
						Debug.LogException(ex30);
						player.Kick("RPC Error in RPC_StartClimb");
					}
				}
				return true;
			}
			if (rpc == 1777651896 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SelectedRPSOption"));
				}
				using (TimeWarning.New("SelectedRPSOption"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(1777651896u, "SelectedRPSOption", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg25 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SelectedRPSOption(msg25);
						}
					}
					catch (Exception ex31)
					{
						Debug.LogException(ex31);
						player.Kick("RPC Error in SelectedRPSOption");
					}
				}
				return true;
			}
			if (rpc == 3047177092u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_AddMarker"));
				}
				using (TimeWarning.New("Server_AddMarker"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3047177092u, "Server_AddMarker", this, player, 8uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(3047177092u, "Server_AddMarker", this, player))
						{
							return true;
						}
						long position2 = msg.read.Position;
						MapNote val = msg.read.Proto<MapNote>((MapNote)null);
						try
						{
							if (!RPC_Server.InputValidation.Test(val.worldPosition))
							{
								return true;
							}
							if (!RPC_Server.InputValidation.Test(val.timeRemaining))
							{
								return true;
							}
							if (!RPC_Server.InputValidation.Test(val.totalDuration))
							{
								return true;
							}
							msg.read.Position = position2;
						}
						finally
						{
							((IDisposable)val)?.Dispose();
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg26 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_AddMarker(msg26);
						}
					}
					catch (Exception ex32)
					{
						Debug.LogException(ex32);
						player.Kick("RPC Error in Server_AddMarker");
					}
				}
				return true;
			}
			if (rpc == 3618659425u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_AddPing"));
				}
				using (TimeWarning.New("Server_AddPing"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3618659425u, "Server_AddPing", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(3618659425u, "Server_AddPing", this, player))
						{
							return true;
						}
						long position3 = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Read<int>();
						msg.read.Read<bool>();
						msg.read.Position = position3;
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg27 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_AddPing(msg27);
						}
					}
					catch (Exception ex33)
					{
						Debug.LogException(ex33);
						player.Kick("RPC Error in Server_AddPing");
					}
				}
				return true;
			}
			if (rpc == 1005040107 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_CancelGesture"));
				}
				using (TimeWarning.New("Server_CancelGesture"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1005040107u, "Server_CancelGesture", this, player, 10uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1005040107u, "Server_CancelGesture", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							Server_CancelGesture();
						}
					}
					catch (Exception ex34)
					{
						Debug.LogException(ex34);
						player.Kick("RPC Error in Server_CancelGesture");
					}
				}
				return true;
			}
			if (rpc == 706157120 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_ClearMapMarkers"));
				}
				using (TimeWarning.New("Server_ClearMapMarkers"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(706157120u, "Server_ClearMapMarkers", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(706157120u, "Server_ClearMapMarkers", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg28 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_ClearMapMarkers(msg28);
						}
					}
					catch (Exception ex35)
					{
						Debug.LogException(ex35);
						player.Kick("RPC Error in Server_ClearMapMarkers");
					}
				}
				return true;
			}
			if (rpc == 310453544 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_ClearPointsOfInterest"));
				}
				using (TimeWarning.New("Server_ClearPointsOfInterest"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(310453544u, "Server_ClearPointsOfInterest", this, player, 8uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(310453544u, "Server_ClearPointsOfInterest", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg29 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_ClearPointsOfInterest(msg29);
						}
					}
					catch (Exception ex36)
					{
						Debug.LogException(ex36);
						player.Kick("RPC Error in Server_ClearPointsOfInterest");
					}
				}
				return true;
			}
			if (rpc == 2895394689u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_OnClientDemoRecordingStateChanged"));
				}
				using (TimeWarning.New("Server_OnClientDemoRecordingStateChanged"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(2895394689u, "Server_OnClientDemoRecordingStateChanged", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg30 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_OnClientDemoRecordingStateChanged(msg30);
						}
					}
					catch (Exception ex37)
					{
						Debug.LogException(ex37);
						player.Kick("RPC Error in Server_OnClientDemoRecordingStateChanged");
					}
				}
				return true;
			}
			if (rpc == 1032755717 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_RemovePing"));
				}
				using (TimeWarning.New("Server_RemovePing"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1032755717u, "Server_RemovePing", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1032755717u, "Server_RemovePing", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg31 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_RemovePing(msg31);
						}
					}
					catch (Exception ex38)
					{
						Debug.LogException(ex38);
						player.Kick("RPC Error in Server_RemovePing");
					}
				}
				return true;
			}
			if (rpc == 31713840 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_RemovePointOfInterest"));
				}
				using (TimeWarning.New("Server_RemovePointOfInterest"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(31713840u, "Server_RemovePointOfInterest", this, player, 10uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(31713840u, "Server_RemovePointOfInterest", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg32 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_RemovePointOfInterest(msg32);
						}
					}
					catch (Exception ex39)
					{
						Debug.LogException(ex39);
						player.Kick("RPC Error in Server_RemovePointOfInterest");
					}
				}
				return true;
			}
			if (rpc == 2844621823u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_RequestLootCountdowns"));
				}
				using (TimeWarning.New("Server_RequestLootCountdowns"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2844621823u, "Server_RequestLootCountdowns", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(2844621823u, "Server_RequestLootCountdowns", this, player))
						{
							return true;
						}
						long position4 = msg.read.Position;
						msg.read.Position = position4;
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg33 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_RequestLootCountdowns(msg33);
						}
					}
					catch (Exception ex40)
					{
						Debug.LogException(ex40);
						player.Kick("RPC Error in Server_RequestLootCountdowns");
					}
				}
				return true;
			}
			if (rpc == 2567683804u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_RequestMarkers"));
				}
				using (TimeWarning.New("Server_RequestMarkers"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2567683804u, "Server_RequestMarkers", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(2567683804u, "Server_RequestMarkers", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg34 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_RequestMarkers(msg34);
						}
					}
					catch (Exception ex41)
					{
						Debug.LogException(ex41);
						player.Kick("RPC Error in Server_RequestMarkers");
					}
				}
				return true;
			}
			if (rpc == 3637080058u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_RequestValidMissionsUpdate"));
				}
				using (TimeWarning.New("Server_RequestValidMissionsUpdate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3637080058u, "Server_RequestValidMissionsUpdate", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(3637080058u, "Server_RequestValidMissionsUpdate", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage _ = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_RequestValidMissionsUpdate(_);
						}
					}
					catch (Exception ex42)
					{
						Debug.LogException(ex42);
						player.Kick("RPC Error in Server_RequestValidMissionsUpdate");
					}
				}
				return true;
			}
			if (rpc == 1572722245 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_StartGesture"));
				}
				using (TimeWarning.New("Server_StartGesture"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1572722245u, "Server_StartGesture", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1572722245u, "Server_StartGesture", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg35 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_StartGesture(msg35);
						}
					}
					catch (Exception ex43)
					{
						Debug.LogException(ex43);
						player.Kick("RPC Error in Server_StartGesture");
					}
				}
				return true;
			}
			if (rpc == 1180369886 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_UpdateMarker"));
				}
				using (TimeWarning.New("Server_UpdateMarker"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1180369886u, "Server_UpdateMarker", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1180369886u, "Server_UpdateMarker", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg36 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_UpdateMarker(msg36);
						}
					}
					catch (Exception ex44)
					{
						Debug.LogException(ex44);
						player.Kick("RPC Error in Server_UpdateMarker");
					}
				}
				return true;
			}
			if (rpc == 2192544725u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerRequestEmojiData"));
				}
				using (TimeWarning.New("ServerRequestEmojiData"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2192544725u, "ServerRequestEmojiData", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg37 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ServerRequestEmojiData(msg37);
						}
					}
					catch (Exception ex45)
					{
						Debug.LogException(ex45);
						player.Kick("RPC Error in ServerRequestEmojiData");
					}
				}
				return true;
			}
			if (rpc == 3635568749u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerRPC_UnderwearChange"));
				}
				using (TimeWarning.New("ServerRPC_UnderwearChange"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg38 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ServerRPC_UnderwearChange(msg38);
						}
					}
					catch (Exception ex46)
					{
						Debug.LogException(ex46);
						player.Kick("RPC Error in ServerRPC_UnderwearChange");
					}
				}
				return true;
			}
			if (rpc == 3222472445u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - StartTutorial"));
				}
				using (TimeWarning.New("StartTutorial"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg39 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							StartTutorial(msg39);
						}
					}
					catch (Exception ex47)
					{
						Debug.LogException(ex47);
						player.Kick("RPC Error in StartTutorial");
					}
				}
				return true;
			}
			if (rpc == 970114602 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_Drink"));
				}
				using (TimeWarning.New("SV_Drink"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg40 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SV_Drink(msg40);
						}
					}
					catch (Exception ex48)
					{
						Debug.LogException(ex48);
						player.Kick("RPC Error in SV_Drink");
					}
				}
				return true;
			}
			if (rpc == 1361044246 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - UpdateSpectatePositionFromDebugCamera"));
				}
				using (TimeWarning.New("UpdateSpectatePositionFromDebugCamera"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1361044246u, "UpdateSpectatePositionFromDebugCamera", this, player, 10uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1361044246u, "UpdateSpectatePositionFromDebugCamera", this, player))
						{
							return true;
						}
						long position5 = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Position = position5;
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg41 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							UpdateSpectatePositionFromDebugCamera(msg41);
						}
					}
					catch (Exception ex49)
					{
						Debug.LogException(ex49);
						player.Kick("RPC Error in UpdateSpectatePositionFromDebugCamera");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void ToggleShowFSMStateDebugInfo()
	{
		if (!IsInvoking(ShowStateDebugInfo))
		{
			InvokeRepeating(ShowStateDebugInfo, 0f, 0.1f);
		}
		else
		{
			CancelInvoke(ShowStateDebugInfo);
		}
	}

	private void ShowStateDebugInfo()
	{
		FSMComponent.ShowDebugInfoAroundLocation(this);
	}

	public void MarkNavmeshTileDirty(int tx, int ty)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (navmeshDirtyTiles == null)
		{
			navmeshDirtyTiles = new HashSet<(int, Vector2Int)>();
		}
		navmeshDirtyTiles.Add((0, new Vector2Int(tx, ty)));
	}

	public void ResetNavmeshDrawState()
	{
		navmeshSentTiles?.Clear();
		navmeshDirtyTiles?.Clear();
		navmeshDrawTickCounter = 0;
	}

	public void DrawNavmesh()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BasePlayer.DrawNavmesh"))
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return;
			}
			RustNavigation instance = RustNavigation.Instance;
			if ((Object)(object)instance == (Object)null)
			{
				return;
			}
			RustNavmesh defaultNavmesh = instance.DefaultNavmesh;
			if (defaultNavmesh == null || !defaultNavmesh.IsValid())
			{
				return;
			}
			if (navmeshSentTiles == null)
			{
				navmeshSentTiles = new HashSet<(int, Vector2Int)>();
			}
			if (navmeshDirtyTiles == null)
			{
				navmeshDirtyTiles = new HashSet<(int, Vector2Int)>();
			}
			float drawRadius = RustNav.drawRadius;
			Bounds worldBounds = new Bounds(((Component)this).transform.position, new Vector3(drawRadius * 2f, drawRadius * 2f, drawRadius * 2f));
			Vector3 selfPos = ((Component)this).transform.position;
			PooledList<NavDrawTile> val = Pool.Get<PooledList<NavDrawTile>>();
			try
			{
				GatherNavmeshTiles(defaultNavmesh, 0, null, alwaysResend: false, worldBounds, (List<NavDrawTile>)(object)val);
				PooledList<IndependantNavmesh> val2 = Pool.Get<PooledList<IndependantNavmesh>>();
				try
				{
					IndependantNavmesh.FindNavmeshesInBounds(worldBounds, (List<IndependantNavmesh>)(object)val2);
					foreach (IndependantNavmesh item2 in (List<IndependantNavmesh>)(object)val2)
					{
						RustNavmesh navmesh = item2.Navmesh;
						if (navmesh != null && navmesh.IsValid())
						{
							int drawNavId = RustNavigation.GetDrawNavId(item2);
							Matrix4x4? navToWorld = (item2.canMove ? new Matrix4x4?(item2.NavToWorldMatrix) : ((Matrix4x4?)null));
							GatherNavmeshTiles(navmesh, drawNavId, navToWorld, item2.canMove, worldBounds, (List<NavDrawTile>)(object)val);
						}
					}
					PooledHashSet<(int, Vector2Int)> val3 = Pool.Get<PooledHashSet<(int, Vector2Int)>>();
					try
					{
						foreach (NavDrawTile item3 in (List<NavDrawTile>)(object)val)
						{
							((HashSet<(int, Vector2Int)>)(object)val3).Add((item3.navId, item3.coord));
						}
						navmeshDrawTickCounter++;
						if (RustNav.drawManifestInterval > 0 && navmeshDrawTickCounter % RustNav.drawManifestInterval == 0)
						{
							SendNavmeshManifest((List<NavDrawTile>)(object)val);
						}
						PooledList<(int, Vector2Int)> val4 = Pool.Get<PooledList<(int, Vector2Int)>>();
						try
						{
							foreach (var navmeshSentTile in navmeshSentTiles)
							{
								if (!((HashSet<(int, Vector2Int)>)(object)val3).Contains(navmeshSentTile))
								{
									((List<(int, Vector2Int)>)(object)val4).Add(navmeshSentTile);
								}
							}
							foreach (var item4 in (List<(int, Vector2Int)>)(object)val4)
							{
								navmeshSentTiles.Remove(item4);
								navmeshDirtyTiles.Remove(item4);
							}
							PooledList<NavDrawTile> val5 = Pool.Get<PooledList<NavDrawTile>>();
							try
							{
								foreach (NavDrawTile item5 in (List<NavDrawTile>)(object)val)
								{
									(int, Vector2Int) item = (item5.navId, item5.coord);
									if (item5.alwaysResend || navmeshDirtyTiles.Contains(item) || !navmeshSentTiles.Contains(item))
									{
										((List<NavDrawTile>)(object)val5).Add(item5);
									}
								}
								if (((List<NavDrawTile>)(object)val5).Count == 0)
								{
									return;
								}
								((List<NavDrawTile>)(object)val5).Sort((Comparison<NavDrawTile>)delegate(NavDrawTile a, NavDrawTile b)
								{
									//IL_0001: Unknown result type (might be due to invalid IL or missing references)
									//IL_0007: Unknown result type (might be due to invalid IL or missing references)
									//IL_000c: Unknown result type (might be due to invalid IL or missing references)
									//IL_0011: Unknown result type (might be due to invalid IL or missing references)
									//IL_001b: Unknown result type (might be due to invalid IL or missing references)
									//IL_0021: Unknown result type (might be due to invalid IL or missing references)
									//IL_0026: Unknown result type (might be due to invalid IL or missing references)
									//IL_002b: Unknown result type (might be due to invalid IL or missing references)
									Vector3 val6 = a.worldCenter - selfPos;
									float sqrMagnitude = ((Vector3)(ref val6)).sqrMagnitude;
									val6 = b.worldCenter - selfPos;
									float sqrMagnitude2 = ((Vector3)(ref val6)).sqrMagnitude;
									return sqrMagnitude.CompareTo(sqrMagnitude2);
								});
								int num = Mathf.Max(1, RustNav.drawTileBudget);
								int num2 = 0;
								foreach (NavDrawTile item6 in (List<NavDrawTile>)(object)val5)
								{
									if (item6.alwaysResend || num2 < num)
									{
										SendNavmeshTile(item6);
										navmeshSentTiles.Add((item6.navId, item6.coord));
										navmeshDirtyTiles.Remove((item6.navId, item6.coord));
										if (!item6.alwaysResend)
										{
											num2++;
										}
									}
								}
							}
							finally
							{
								((IDisposable)val5)?.Dispose();
							}
						}
						finally
						{
							((IDisposable)val4)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
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
	}

	private void GatherNavmeshTiles(RustNavmesh navmesh, int navId, Matrix4x4? navToWorld, bool alwaysResend, Bounds worldBounds, List<NavDrawTile> candidates)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		Bounds val = worldBounds;
		Matrix4x4 value;
		if (navToWorld.HasValue)
		{
			value = navToWorld.Value;
			Matrix4x4 inverse = ((Matrix4x4)(ref value)).inverse;
			OBB val2 = default(OBB);
			((OBB)(ref val2))._002Ector(worldBounds);
			((OBB)(ref val2)).Transform(((Matrix4x4)(ref inverse)).GetPosition(), ((Matrix4x4)(ref inverse)).lossyScale, ((Matrix4x4)(ref inverse)).rotation);
			val = ((OBB)(ref val2)).ToBounds();
		}
		PooledList<Vector2Int> val3 = Pool.Get<PooledList<Vector2Int>>();
		try
		{
			navmesh.GetTilesInBounds(val, (List<Vector2Int>)(object)val3);
			foreach (Vector2Int item in (List<Vector2Int>)(object)val3)
			{
				Bounds val4 = navmesh.rcCalcTileBounds(item);
				Vector3 val5 = ((Bounds)(ref val4)).center;
				if (navToWorld.HasValue)
				{
					value = navToWorld.Value;
					val5 = ((Matrix4x4)(ref value)).MultiplyPoint3x4(val5);
				}
				candidates.Add(new NavDrawTile
				{
					navId = navId,
					coord = item,
					navmesh = navmesh,
					transform = navToWorld,
					worldCenter = val5,
					alwaysResend = alwaysResend
				});
			}
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	private void SendNavmeshTile(NavDrawTile tile)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BasePlayer.SendNavmeshTile"))
		{
			NavMeshData val = Pool.Get<NavMeshData>();
			try
			{
				val.polygons = Pool.Get<List<VectorList>>();
				tile.navmesh.FillDebugDrawProtoForTile(val, ((Vector2Int)(ref tile.coord)).x, ((Vector2Int)(ref tile.coord)).y, tile.transform);
				ClientRPC(RpcTarget.Player("CL_DrawNavmeshTile", this), tile.navId, ((Vector2Int)(ref tile.coord)).x, ((Vector2Int)(ref tile.coord)).y, tile.worldCenter, ProtoStreamExtensions.ToProtoBytes((IProto)(object)val));
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private void SendNavmeshManifest(List<NavDrawTile> inRange)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BasePlayer.SendNavmeshManifest"))
		{
			NavMeshData val = Pool.Get<NavMeshData>();
			try
			{
				val.polygons = Pool.Get<List<VectorList>>();
				for (int i = 0; i < inRange.Count; i++)
				{
					VectorList val2 = Pool.Get<VectorList>();
					val2.vectorPoints = Pool.Get<List<Vector3>>();
					List<Vector3> vectorPoints = val2.vectorPoints;
					NavDrawTile navDrawTile = inRange[i];
					float num = ((Vector2Int)(ref navDrawTile.coord)).x;
					navDrawTile = inRange[i];
					vectorPoints.Add(new Vector3(num, (float)((Vector2Int)(ref navDrawTile.coord)).y, (float)inRange[i].navId));
					val.polygons.Add(val2);
				}
				ClientRPC(RpcTarget.Player("CL_DrawNavmeshManifest", this), ProtoStreamExtensions.ToProtoBytes((IProto)(object)val));
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public bool TriggeredAntiHack(float seconds = 1f, float score = float.PositiveInfinity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return TriggeredAntiHack(AntiHack.PlayerStates.AsReadOnly(), seconds, score);
	}

	public bool TriggeredAntiHack(ReadOnly<AntiHack.PlayerState> ahStates, float seconds = 1f, float score = float.PositiveInfinity)
	{
		if (ActivePlayerInd != -1)
		{
			AntiHack.PlayerState playerState = ahStates[ActivePlayerInd];
			if (!(Time.realtimeSinceStartup - playerState.LastViolationTime < seconds))
			{
				return playerState.ViolationLevel > score;
			}
			return true;
		}
		return false;
	}

	public bool TriggeredMovementAntiHack(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			AntiHack.PlayerState playerState = AntiHack.PlayerStates[ActivePlayerInd];
			return Time.realtimeSinceStartup - playerState.LastMovementViolationTime < seconds;
		}
		return false;
	}

	public bool UsedAdminCheat(float seconds = 2f)
	{
		if (ActivePlayerInd != -1)
		{
			AntiHack.PlayerState playerState = AntiHack.PlayerStates[ActivePlayerInd];
			return Time.realtimeSinceStartup - playerState.LastAdminCheatTime < seconds;
		}
		return false;
	}

	public bool TriggeredNoclip(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			AntiHack.PlayerState playerState = AntiHack.PlayerStates[ActivePlayerInd];
			if (playerState.LastViolationType == AntiHackType.NoClip)
			{
				return Time.realtimeSinceStartup - playerState.LastViolationTime < seconds;
			}
			return false;
		}
		return false;
	}

	public void PauseVehicleNoClipDetection(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			ref AntiHack.PlayerNoclipState reference = ref NativeArray<AntiHack.PlayerNoclipState>.op_Implicit(ref AntiHack.PlayerNoclipStates)[ActivePlayerInd];
			reference.VehiclePauseTime = Mathf.Max(reference.VehiclePauseTime, seconds);
		}
	}

	public void PauseFlyHackDetection(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			ref AntiHack.PlayerFlyhackState reference = ref NativeArray<AntiHack.PlayerFlyhackState>.op_Implicit(ref AntiHack.PlayerFlyhackStates)[ActivePlayerInd];
			reference.PauseTime = Mathf.Max(reference.PauseTime, seconds);
		}
	}

	public void AddTempSpeedHackBudget(float totalDistanceExpected = 1f, float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			ref AntiHack.PlayerSpeedhackState reference = ref NativeArray<AntiHack.PlayerSpeedhackState>.op_Implicit(ref AntiHack.PlayerSpeedhackStates)[ActivePlayerInd];
			reference.ExtraSpeed = totalDistanceExpected / seconds;
			reference.ExtraSpeedTime = seconds;
		}
	}

	public void PauseSpeedHackDetection(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			ref AntiHack.PlayerSpeedhackState reference = ref NativeArray<AntiHack.PlayerSpeedhackState>.op_Implicit(ref AntiHack.PlayerSpeedhackStates)[ActivePlayerInd];
			reference.PauseTime = Mathf.Max(reference.PauseTime, seconds);
		}
	}

	public void PauseTickDistanceDetection(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			ref AntiHack.PlayerState reference = ref NativeArray<AntiHack.PlayerState>.op_Implicit(ref AntiHack.PlayerStates)[ActivePlayerInd];
			reference.TickDistancePausetime = Mathf.Max(reference.TickDistancePausetime, seconds);
		}
	}

	public void ForceCastNoClip(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			ref AntiHack.PlayerNoclipState reference = ref NativeArray<AntiHack.PlayerNoclipState>.op_Implicit(ref AntiHack.PlayerNoclipStates)[ActivePlayerInd];
			reference.ForceCastTime = Mathf.Max(reference.ForceCastTime, seconds);
		}
	}

	public void UpdateUnparentTime()
	{
		if (ActivePlayerInd != -1)
		{
			NativeArray<AntiHack.PlayerState>.op_Implicit(ref AntiHack.PlayerStates)[ActivePlayerInd].UnparentTime = Time.time;
		}
	}

	public bool RecentlyUnparented(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			return Time.time - AntiHack.PlayerStates[ActivePlayerInd].UnparentTime <= seconds;
		}
		return false;
	}

	public bool RecentlyInAir(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			float lastInAirTime = AntiHack.PlayerFlyhackStates[ActivePlayerInd].LastInAirTime;
			return Time.realtimeSinceStartup - lastInAirTime < seconds;
		}
		return false;
	}

	public int GetAntiHackKicks()
	{
		return AntiHack.GetKickRecord(this);
	}

	public static void ResetAntiHack(BasePlayer player, NativeArray<AntiHack.PlayerState> playerStates, NativeArray<AntiHack.PlayerNoclipState> noclipStates, NativeArray<AntiHack.PlayerSpeedhackState> speedhackStates, NativeArray<AntiHack.PlayerFlyhackState> flyhackStates)
	{
		if (player.ActivePlayerInd != -1)
		{
			if (playerStates.IsCreated)
			{
				playerStates[player.ActivePlayerInd] = default(AntiHack.PlayerState);
			}
			if (noclipStates.IsCreated)
			{
				noclipStates[player.ActivePlayerInd] = default(AntiHack.PlayerNoclipState);
			}
			if (speedhackStates.IsCreated)
			{
				speedhackStates[player.ActivePlayerInd] = default(AntiHack.PlayerSpeedhackState);
			}
			if (flyhackStates.IsCreated)
			{
				flyhackStates[player.ActivePlayerInd] = default(AntiHack.PlayerFlyhackState);
			}
		}
		player.rpcHistory.Clear();
	}

	public bool CanModifyClan()
	{
		if (!Clan.editsRequireClanTable)
		{
			return true;
		}
		if (base.isServer)
		{
			if (triggers == null || (Object)(object)ClanManager.ServerInstance == (Object)null)
			{
				return false;
			}
			foreach (TriggerBase trigger in triggers)
			{
				if (trigger is TriggerClanModify)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public void LoadClanInfo()
	{
		ClanManager clanManager = ClanManager.ServerInstance;
		if (Clan.enabled && !((Object)(object)clanManager == (Object)null))
		{
			LoadClanInfoImpl();
		}
		async void LoadClanInfoImpl()
		{
			try
			{
				ClanValueResult<IClan> val = await clanManager.Backend.GetByMember((ulong)userID);
				if (!val.IsSuccess)
				{
					if ((int)val.Result != 3)
					{
						Debug.LogError((object)$"Failed to find clan for {userID.Get()}: {val.Result}");
						Invoke(LoadClanInfo, 45 + Random.Range(0, 30));
						return;
					}
					serverClan = null;
					clanId = 0L;
				}
				else
				{
					serverClan = val.Value;
					clanId = serverClan.ClanId;
				}
				SendNetworkUpdate();
				if (net?.connection != null)
				{
					UpdateClanLastSeen();
					if (clanId != 0L)
					{
						clanManager.ClanMemberConnectionsChanged(clanId);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
	}

	public void UpdateClanLastSeen()
	{
		ClanManager clanManager = ClanManager.ServerInstance;
		if (!((Object)(object)clanManager == (Object)null) && clanId != 0L)
		{
			UpdateClanLastSeenImpl();
		}
		async void UpdateClanLastSeenImpl()
		{
			_ = 1;
			try
			{
				ClanValueResult<IClan> val = await clanManager.Backend.Get(clanId);
				if (!val.IsSuccess)
				{
					LoadClanInfo();
				}
				else
				{
					ClanResult val2 = await val.Value.UpdateLastSeen((ulong)userID);
					if ((int)val2 != 1)
					{
						Debug.LogWarning((object)$"Couldn't update clan last seen for {userID.Get()}: {val2}");
					}
				}
			}
			catch (Exception arg)
			{
				Debug.LogError((object)$"Failed to update clan last seen for {userID.Get()}: {arg}");
			}
		}
	}

	public void AddClanScore(ClanScoreEventType type, int multiplier = 1, BasePlayer otherPlayer = null, IClan otherClan = null, string arg1 = null, string arg2 = null)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		ClanManager serverInstance = ClanManager.ServerInstance;
		if (!((Object)(object)serverInstance == (Object)null) && serverClan != null && !IsBot && !IsNpc && multiplier != 0)
		{
			int scoreForEvent = Clan.GetScoreForEvent(type);
			if (scoreForEvent != 0)
			{
				bool flag = (Object)(object)otherPlayer != (Object)null && !otherPlayer.IsBot && !otherPlayer.IsNpc;
				serverInstance.AddScore(serverClan, new ClanScoreEvent
				{
					Type = type,
					SteamId = userID,
					Score = scoreForEvent,
					Multiplier = multiplier,
					OtherSteamId = (flag ? new ulong?(otherPlayer.userID) : ((ulong?)null)),
					OtherClanId = ((otherClan != null && otherClan != serverClan) ? new long?(otherClan.ClanId) : ((flag && otherPlayer.clanId != 0L) ? new long?(otherPlayer.clanId) : ((long?)null))),
					Arg1 = arg1,
					Arg2 = arg2
				});
			}
		}
	}

	private void HandleClanPlayerKilled(BasePlayer killedByPlayer)
	{
		if (!((Object)(object)killedByPlayer == (Object)null) && !((Object)(object)killedByPlayer == (Object)(object)this))
		{
			if (serverClan != null && killedByPlayer.serverClan != null && serverClan != killedByPlayer.serverClan)
			{
				AddClanScore((ClanScoreEventType)2, 1, killedByPlayer);
				killedByPlayer.AddClanScore((ClanScoreEventType)1, 1, this);
			}
			if (!HasPlayerFlag(PlayerFlags.DisplaySash) && killedByPlayer.serverClan != null)
			{
				killedByPlayer.AddClanScore((ClanScoreEventType)3, 1, this);
			}
		}
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		object obj = Interface.CallHook("CanLootPlayer", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if ((Object)(object)player == (Object)(object)this)
		{
			return false;
		}
		if ((IsWounded() || IsSleeping() || CurrentGestureIsSurrendering || IsRestrainedOrSurrendering) && !IsLoadingAfterTransfer())
		{
			return !IsTransferring();
		}
		return false;
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_LootPlayer(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (Object.op_Implicit((Object)(object)player) && player.CanInteract() && CanBeLooted(player) && player.inventory.loot.StartLootingEntity(this))
		{
			player.inventory.loot.AddContainer(inventory.containerMain);
			player.inventory.loot.AddContainer(inventory.containerWear);
			player.inventory.loot.AddContainer(inventory.containerBelt);
			Interface.CallHook("OnLootPlayer", this, player);
			player.inventory.loot.SendImmediate();
			player.RadioactiveLootCheck(player.inventory.loot.containers);
			player.ClientRPC(RpcTarget.Player("RPC_OpenLootPanel", player), "player_corpse");
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_Assist(RPCMessage msg)
	{
		if (msg.player.CanInteract() && !((Object)(object)msg.player == (Object)(object)this) && IsWounded() && Interface.CallHook("OnPlayerAssist", this, msg.player) == null)
		{
			StopWounded(msg.player);
			msg.player.stats.Add("wounded_assisted", 1, (Stats)5);
			stats.Add("wounded_healed", 1);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_KeepAlive(RPCMessage msg)
	{
		if (msg.player.CanInteract() && !((Object)(object)msg.player == (Object)(object)this) && IsWounded() && Interface.CallHook("OnPlayerKeepAlive", this, msg.player) == null)
		{
			ProlongWounding(10f);
		}
	}

	public void SetActiveTalkingToNpc(NPCTalking npc)
	{
		if (!((Object)(object)activeTalkingToNpc == (Object)(object)npc))
		{
			EndActiveConversation();
			activeTalkingToNpc = npc;
		}
	}

	public void ClearActiveTalkingToNpc(NPCTalking npc)
	{
		if ((Object)(object)npc != (Object)(object)activeTalkingToNpc)
		{
			string text = (((Object)(object)activeTalkingToNpc == (Object)null) ? "null" : ((Object)activeTalkingToNpc).name);
			Debug.LogWarning((object)(((Object)npc).name + " tried to clear active talking to NPC on " + ((Object)this).name + " but their NPC is " + text), (Object)(object)this);
		}
		activeTalkingToNpc = null;
	}

	public void EndActiveConversation()
	{
		if (!((Object)(object)activeTalkingToNpc == (Object)null))
		{
			activeTalkingToNpc.Server_OnConversationEnded(this);
		}
	}

	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	[RPC_Server.InputValidation(new Type[] { })]
	private void Server_RequestLootCountdowns(RPCMessage msg)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if ((!msg.player.IsAdmin && !msg.player.IsDeveloper) || !Net.sv.IsConnected() || net == null)
		{
			return;
		}
		int num = Mathf.Min((int)msg.read.UInt16(), 32);
		PooledList<ILootContainer> val = Pool.Get<PooledList<ILootContainer>>();
		try
		{
			for (int i = 0; i < num; i++)
			{
				if (BaseNetworkable.serverEntities.Find(msg.read.EntityID()) is ILootContainer item)
				{
					((List<ILootContainer>)(object)val).Add(item);
				}
			}
			NetWrite netWrite = ClientRPCStart("Client_ReceiveLootCountdowns");
			netWrite.UInt16((ushort)((List<ILootContainer>)(object)val).Count);
			foreach (ILootContainer item2 in (List<ILootContainer>)(object)val)
			{
				netWrite.EntityID(item2.GetEntity().net.ID);
				netWrite.Int32((int)item2.GetLootCountdownTimeRemaining());
			}
			ClientRPCSend(netWrite, new SendInfo(net.connection));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[RPC_Server]
	private void SV_Drink(RPCMessage msg)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		Vector3 val = msg.read.Vector3();
		if (!Vector3Ex.IsNaNOrInfinity(val) && Object.op_Implicit((Object)(object)player) && player.metabolism.CanConsume() && !(Vector3.Distance(((Component)player).transform.position, val) > 5f) && WaterLevel.Test(val, waves: true, volumes: true, this) && (!isMounted || GetMounted().canDrinkWhileMounted))
		{
			ItemDefinition itemDefinition = WaterResource.SV_GetAtPoint(val);
			ItemModConsumable component = ((Component)itemDefinition).GetComponent<ItemModConsumable>();
			Item item = ItemManager.Create(itemDefinition, component.amountToConsume, 0uL, isServerSide: true, 0uL);
			ItemModConsume component2 = ((Component)item.info).GetComponent<ItemModConsume>();
			if (component2.CanDoAction(item, player))
			{
				component2.DoAction(item, player);
			}
			item?.Remove();
			player.metabolism.MarkConsumption();
		}
	}

	[RPC_Server.InputValidation(new Type[]
	{
		typeof(bool),
		typeof(Vector3),
		typeof(NetworkableId)
	})]
	[RPC_Server]
	public void RPC_StartClimb(RPCMessage msg)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		bool flag = msg.read.Bit();
		Vector3 val = msg.read.Vector3();
		NetworkableId val2 = msg.read.EntityID();
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(val2);
		Vector3 val3 = (flag ? ((Component)baseNetworkable).transform.TransformPoint(val) : val);
		if (player.IsRestrained || !player.isMounted || player.Distance(val3) > 5f || !GamePhysics.LineOfSight(player.eyes.position, val3, 1218519041) || !GamePhysics.LineOfSight(val3, val3 + player.eyes.offset, 1218519041))
		{
			return;
		}
		Vector3 val4 = val3 - player.eyes.position;
		Vector3 end = val3 - ((Vector3)(ref val4)).normalized * 0.25f;
		if (!GamePhysics.CheckCapsule(player.eyes.position, end, 0.25f, 1218519041, (QueryTriggerInteraction)0) && !AntiHack.TestNoClipping(player, val3 + NoClipOffset(), val3 + NoClipOffset(), NoClipRadius(ConVar.AntiHack.noclip_margin), ConVar.AntiHack.noclip_backtracking, out var _))
		{
			player.EnsureDismounted();
			player.MovePosition(val3);
			Collider component = ((Component)player).GetComponent<Collider>();
			component.enabled = false;
			component.enabled = true;
			if (flag)
			{
				player.ClientRPC(RpcTarget.Player("ForcePositionToParentOffset", player), val, val2);
			}
			else
			{
				player.ClientRPC(RpcTarget.Player("ForcePositionTo", player), val3);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	private void RequestServerEmoji()
	{
		RustEmojiLibrary.FindAllServerEmoji();
		if (RustEmojiLibrary.allServerEmoji.Count > 0)
		{
			ClientRPCList(RpcTarget.Player("ClientReceiveEmojiList", this), RustEmojiLibrary.cachedServerList);
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	private void ServerRequestEmojiData(RPCMessage msg)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		string text = msg.read.String();
		if (RustEmojiLibrary.allServerEmoji.TryGetValue(text, out var value))
		{
			byte[] array = FileStorage.server.Get(value.CRC, value.FileType, RustEmojiLibrary.EmojiStorageNetworkId);
			ClientRPC(RpcTarget.Player("ClientReceiveEmojiData", msg.player), (uint)array.Length, array, text, value.CRC, (int)value.FileType);
		}
	}

	public int GetQueuedUpdateCount(NetworkQueue queue)
	{
		return networkQueue[(int)queue].Length;
	}

	public void SendSnapshots(ListHashSet<Networkable> ents)
	{
		if (ents == null)
		{
			return;
		}
		using (TimeWarning.New("SendSnapshots"))
		{
			int count = ents.Values.Count;
			Networkable[] buffer = ents.Values.Buffer;
			for (int i = 0; i < count; i++)
			{
				SnapshotQueue.Add(buffer[i].handler as BaseNetworkable);
			}
		}
	}

	public void QueueUpdate(NetworkQueue queue, BaseNetworkable ent)
	{
		if (!IsConnected)
		{
			return;
		}
		switch (queue)
		{
		case NetworkQueue.Update:
			networkQueue[0].Add(ent);
			break;
		case NetworkQueue.UpdateDistance:
			if (!IsReceivingSnapshot && !networkQueue[1].Contains(ent) && !networkQueue[0].Contains(ent))
			{
				NetworkQueueList networkQueueList = networkQueue[1];
				if (Distance(ent as BaseEntity) < 20f)
				{
					QueueUpdate(NetworkQueue.Update, ent);
				}
				else
				{
					networkQueueList.Add(ent);
				}
			}
			break;
		}
	}

	public void SendEntityUpdate()
	{
		using (TimeWarning.New("SendEntityUpdate"))
		{
			SendEntityUpdates(SnapshotQueue);
			SendEntityUpdates(networkQueue[0]);
			SendEntityUpdates(networkQueue[1]);
		}
	}

	public static void SendEntityUpdates(BasePlayer[] players, ReadOnlySpan<int> indices)
	{
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SendEntityUpdates"))
		{
			ThreadSafeTime time = ThreadSafeTime.TakeSnapshot();
			ReadOnlySpan<int> readOnlySpan;
			if (BaseNetworkable.UseParallelSaves)
			{
				int num = 0;
				List<UniTask> list = Pool.Get<List<UniTask>>();
				BufferList<int> val = Pool.Get<BufferList<int>>();
				readOnlySpan = indices;
				for (int i = 0; i < readOnlySpan.Length; i++)
				{
					int num2 = readOnlySpan[i];
					BasePlayer basePlayer = players[num2];
					int val2 = (basePlayer.IsReceivingSnapshot ? ConVar.Server.updatebatchspawn : ConVar.Server.updatebatch);
					int num3 = Math.Min(basePlayer.SnapshotQueue.Length, val2);
					for (int j = 0; j < 2; j++)
					{
						num3 += Math.Min(basePlayer.networkQueue[j].Length, val2);
					}
					if (num3 != 0)
					{
						val.Add(num2);
						num += num3;
						if (num >= ConVar.Server.ParallelNetworkQueueBatchSize)
						{
							list.Add(ProcessPlayerBatchAsync(players, val, time));
							val = Pool.Get<BufferList<int>>();
							num = 0;
						}
					}
				}
				if (val.Count > 0)
				{
					ProcessPlayerBatch(players, val, in time);
				}
				else
				{
					Pool.FreeUnmanaged<int>(ref val);
				}
				WaitForTasks(list);
				Pool.FreeUnmanaged<UniTask>(ref list);
				return;
			}
			BufferList<(BaseEntity, BasePlayer)> val3 = Pool.Get<BufferList<(BaseEntity, BasePlayer)>>();
			HashSet<BaseEntity> hashSet = Pool.Get<HashSet<BaseEntity>>();
			BufferList<(BaseEntity, BasePlayer)> val4 = Pool.Get<BufferList<(BaseEntity, BasePlayer)>>();
			BufferList<int> val5 = Pool.Get<BufferList<int>>();
			readOnlySpan = indices;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				int num4 = readOnlySpan[i];
				BasePlayer basePlayer2 = players[num4];
				int batchSize = (basePlayer2.IsReceivingSnapshot ? ConVar.Server.updatebatchspawn : ConVar.Server.updatebatch);
				NetworkQueueList snapshotQueue = basePlayer2.SnapshotQueue;
				GatherFromQueue(basePlayer2, snapshotQueue, batchSize, hashSet, val3);
				int count = val4.Count;
				Enumerator<(BaseEntity, BasePlayer)> enumerator = val3.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						(BaseEntity, BasePlayer) current = enumerator.Current;
						if (current.Item1.ShouldNetworkTo(current.Item2))
						{
							val4.Add(current);
						}
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
				}
				val3.Clear();
				if (val4.Count > count)
				{
					BuildSnapshotDependencyChains(val4.ContentReadOnlySpan().Slice(count), count, val5);
				}
				for (int k = 0; k < 2; k++)
				{
					snapshotQueue = basePlayer2.networkQueue[k];
					GatherFromQueue(basePlayer2, snapshotQueue, batchSize, hashSet, val3);
					count = val4.Count;
					enumerator = val3.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							(BaseEntity, BasePlayer) current2 = enumerator.Current;
							if (current2.Item1.ShouldNetworkTo(current2.Item2))
							{
								val4.Add(current2);
							}
						}
					}
					finally
					{
						((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
					}
					val3.Clear();
					if (val4.Count > count)
					{
						BuildSnapshotDependencyChains(val4.ContentReadOnlySpan().Slice(count), count, val5);
					}
				}
				hashSet.Clear();
			}
			Pool.FreeUnmanaged<BaseEntity>(ref hashSet);
			Pool.FreeUnmanaged<(BaseEntity, BasePlayer)>(ref val3);
			if (val4.Count == 0)
			{
				Pool.FreeUnmanaged<(BaseEntity, BasePlayer)>(ref val4);
				Pool.FreeUnmanaged<int>(ref val5);
			}
			else
			{
				SendEntitySnapshots(val4, val5.ContentReadOnlySpan(), in time);
				Pool.FreeUnmanaged<(BaseEntity, BasePlayer)>(ref val4);
				Pool.FreeUnmanaged<int>(ref val5);
			}
		}
		static void GatherFromQueue(BasePlayer player, NetworkQueueList queue, int num6, HashSet<BaseEntity> alreadyScheduledPairs, BufferList<(BaseEntity from, BasePlayer to)> shouldNetworkToPairs)
		{
			if (CollectionEx.IsEmpty(queue.queueInternal))
			{
				return;
			}
			using (TimeWarning.New("GatherFromQueue"))
			{
				PooledList<BaseNetworkable> val6 = Pool.Get<PooledList<BaseNetworkable>>();
				try
				{
					int num5 = 0;
					foreach (BaseNetworkable item2 in queue.queueInternal)
					{
						((List<BaseNetworkable>)(object)val6).Add(item2);
						if (!((Object)(object)item2 == (Object)null) && item2.net != null)
						{
							BaseEntity baseEntity = item2 as BaseEntity;
							if (!alreadyScheduledPairs.Contains(baseEntity))
							{
								alreadyScheduledPairs.Add(baseEntity);
								shouldNetworkToPairs.Add((baseEntity, player));
								if (++num5 > num6)
								{
									break;
								}
							}
						}
					}
					if (((List<BaseNetworkable>)(object)val6).Count == queue.queueInternal.Count)
					{
						queue.queueInternal.Clear();
						if (queue.MaxLength > 2048)
						{
							queue.queueInternal = new HashSet<BaseNetworkable>();
							queue.MaxLength = 0;
						}
						return;
					}
					foreach (BaseNetworkable item3 in (List<BaseNetworkable>)(object)val6)
					{
						queue.queueInternal.Remove(item3);
					}
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
				}
			}
		}
		static void ProcessPlayerBatch(ReadOnlySpan<BasePlayer> readOnlySpan2, BufferList<int> val7, in ThreadSafeTime time2)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("ProcessPlayerBatch"))
			{
				HashSet<BaseEntity> hashSet2 = Pool.Get<HashSet<BaseEntity>>();
				BufferList<(BaseEntity, BasePlayer)> val6 = Pool.Get<BufferList<(BaseEntity, BasePlayer)>>();
				bool errorLogged = false;
				Enumerator<int> enumerator2 = val7.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current3 = enumerator2.Current;
						BasePlayer basePlayer3 = readOnlySpan2[current3];
						int batchSize2 = (basePlayer3.IsReceivingSnapshot ? ConVar.Server.updatebatchspawn : ConVar.Server.updatebatch);
						Network.Connection connection = basePlayer3.net.connection;
						NetworkQueueList snapshotQueue2 = basePlayer3.SnapshotQueue;
						GatherFromQueue(basePlayer3, snapshotQueue2, batchSize2, hashSet2, val6);
						SendQueue(basePlayer3, connection, val6, in time2, ref errorLogged);
						val6.Clear();
						for (int l = 0; l < 2; l++)
						{
							snapshotQueue2 = basePlayer3.networkQueue[l];
							GatherFromQueue(basePlayer3, snapshotQueue2, batchSize2, hashSet2, val6);
							SendQueue(basePlayer3, connection, val6, in time2, ref errorLogged);
							val6.Clear();
						}
						hashSet2.Clear();
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
				Pool.FreeUnmanaged<(BaseEntity, BasePlayer)>(ref val6);
				Pool.FreeUnmanaged<BaseEntity>(ref hashSet2);
				Pool.FreeUnmanaged<int>(ref val7);
			}
		}
		[AsyncStateMachine(typeof(_003C_003CSendEntityUpdates_003Eg__ProcessPlayerBatchAsync_007C70_1_003Ed))]
		static UniTask ProcessPlayerBatchAsync(BasePlayer[] players2, BufferList<int> indices2, ThreadSafeTime time2)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			_003C_003CSendEntityUpdates_003Eg__ProcessPlayerBatchAsync_007C70_1_003Ed _003C_003CSendEntityUpdates_003Eg__ProcessPlayerBatchAsync_007C70_1_003Ed2 = default(_003C_003CSendEntityUpdates_003Eg__ProcessPlayerBatchAsync_007C70_1_003Ed);
			_003C_003CSendEntityUpdates_003Eg__ProcessPlayerBatchAsync_007C70_1_003Ed2._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
			_003C_003CSendEntityUpdates_003Eg__ProcessPlayerBatchAsync_007C70_1_003Ed2.players = players2;
			_003C_003CSendEntityUpdates_003Eg__ProcessPlayerBatchAsync_007C70_1_003Ed2.indices = indices2;
			_003C_003CSendEntityUpdates_003Eg__ProcessPlayerBatchAsync_007C70_1_003Ed2.time = time2;
			_003C_003CSendEntityUpdates_003Eg__ProcessPlayerBatchAsync_007C70_1_003Ed2._003C_003E1__state = -1;
			((AsyncUniTaskMethodBuilder)(ref _003C_003CSendEntityUpdates_003Eg__ProcessPlayerBatchAsync_007C70_1_003Ed2._003C_003Et__builder)).Start<_003C_003CSendEntityUpdates_003Eg__ProcessPlayerBatchAsync_007C70_1_003Ed>(ref _003C_003CSendEntityUpdates_003Eg__ProcessPlayerBatchAsync_007C70_1_003Ed2);
			return ((AsyncUniTaskMethodBuilder)(ref _003C_003CSendEntityUpdates_003Eg__ProcessPlayerBatchAsync_007C70_1_003Ed2._003C_003Et__builder)).Task;
		}
		static void SendQueue(BasePlayer player, Network.Connection conn, BufferList<(BaseEntity from, BasePlayer to)> pairs, in ThreadSafeTime time2, ref bool errorLogged)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			Enumerator<(BaseEntity, BasePlayer)> enumerator2 = pairs.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					BaseEntity item = enumerator2.Current.Item1;
					try
					{
						if (item.ShouldNetworkTo(player))
						{
							NetWrite write = Net.sv.StartWrite();
							item.SendAsSnapshot(conn, write, in time2, ordered: false);
						}
					}
					catch (Exception arg)
					{
						if (!errorLogged)
						{
							Debug.LogError((object)$"ProcessPlayerBatch: {arg}");
							errorLogged = true;
						}
					}
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	public static void BuildSnapshotDependencyChains(ReadOnlySpan<(BaseEntity from, BasePlayer to)> queuePairs, int indexOffset, BufferList<int> depChains)
	{
		if (queuePairs.Length == 1)
		{
			depChains.Add(1);
			depChains.Add(indexOffset);
			return;
		}
		using (TimeWarning.New("BuildSnapshotDependencyChains"))
		{
			Dictionary<ulong, List<int>> dictionary = Pool.Get<Dictionary<ulong, List<int>>>();
			List<int> list = Pool.Get<List<int>>();
			HashSet<ulong> hashSet = Pool.Get<HashSet<ulong>>();
			for (int i = 0; i < queuePairs.Length; i++)
			{
				BaseEntity item = queuePairs[i].from;
				if (item.ChildCount == 0)
				{
					BaseEntity baseEntity = GetRootOfChain(item, hashSet);
					if (item == baseEntity)
					{
						list.Add(i);
					}
					else
					{
						ulong value = baseEntity.net.ID.Value;
						if (!dictionary.TryGetValue(value, out var value2))
						{
							value2 = (dictionary[value] = Pool.Get<List<int>>());
						}
						value2.Add(i);
					}
				}
				else
				{
					ulong value3 = GetRootOfChain(item, hashSet).net.ID.Value;
					if (!dictionary.TryGetValue(value3, out var value4))
					{
						value4 = (dictionary[value3] = Pool.Get<List<int>>());
					}
					value4.Add(i);
				}
				hashSet.Add(item.net.ID.Value);
			}
			foreach (var (_, list5) in dictionary)
			{
				depChains.Add(list5.Count);
				foreach (int item2 in list5)
				{
					int num2 = item2 + indexOffset;
					depChains.Add(num2);
				}
				List<int> list6 = list5;
				Pool.FreeUnmanaged<int>(ref list6);
			}
			foreach (int item3 in list)
			{
				depChains.Add(1);
				int num3 = item3 + indexOffset;
				depChains.Add(num3);
			}
			Pool.FreeUnmanaged<ulong, List<int>>(ref dictionary);
			Pool.FreeUnmanaged<ulong>(ref hashSet);
			Pool.FreeUnmanaged<int>(ref list);
		}
		static BaseEntity GetRootOfChain(BaseEntity entity, HashSet<ulong> processedEntitySet)
		{
			BaseEntity baseEntity2 = entity.GetParentEntity();
			while (!baseEntity2.IsRealNull() && processedEntitySet.Contains(baseEntity2.net.ID.Value))
			{
				entity = baseEntity2;
				baseEntity2 = entity.GetParentEntity();
			}
			return entity;
		}
	}

	private static void SendEntitySnapshots(BufferList<(BaseEntity, BasePlayer)> allPairs, ReadOnlySpan<int> chains, in ThreadSafeTime time)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SendEntitySnapshots"))
		{
			BufferList<int> val = Pool.Get<BufferList<int>>();
			BufferList<int> val2 = Pool.Get<BufferList<int>>();
			FilterPairsForThreads(allPairs.ContentReadOnlySpan(), chains, val, val2);
			SendEntitySnapshots_AsyncState sendEntitySnapshots_AsyncState = Pool.Get<SendEntitySnapshots_AsyncState>();
			MergeDepsChains(chains, val2.ContentReadOnlySpan(), ConVar.Server.SnapshotTaskBatchCount, sendEntitySnapshots_AsyncState.Chains, sendEntitySnapshots_AsyncState.ChainIndices);
			Pool.FreeUnmanaged<int>(ref val2);
			sendEntitySnapshots_AsyncState.Pairs = allPairs;
			PooledList<UniTask> val3 = Pool.Get<PooledList<UniTask>>();
			try
			{
				if (sendEntitySnapshots_AsyncState.ChainIndices.Count > 0)
				{
					for (int i = 0; i < sendEntitySnapshots_AsyncState.ChainIndices.Count; i++)
					{
						((List<UniTask>)(object)val3).Add(SendSnapshotsAsync(sendEntitySnapshots_AsyncState.Pairs, sendEntitySnapshots_AsyncState.Chains, sendEntitySnapshots_AsyncState.ChainIndices, i, time));
					}
				}
				SendSnapshotsMain(allPairs.ContentReadOnlySpan(), chains, val.ContentReadOnlySpan(), in time);
				WaitForTasks((List<UniTask>)(object)val3);
				Pool.FreeUnmanaged<int>(ref val);
				Pool.Free<SendEntitySnapshots_AsyncState>(ref sendEntitySnapshots_AsyncState);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		static void FilterPairsForThreads(ReadOnlySpan<(BaseEntity from, BasePlayer to)> readOnlySpan2, ReadOnlySpan<int> readOnlySpan, BufferList<int> toSerializeAndSend, BufferList<int> toSend)
		{
			using (TimeWarning.New("FilterPairsForThreads"))
			{
				int num;
				for (num = 0; num < readOnlySpan.Length; num++)
				{
					int num2 = readOnlySpan[num];
					int num3 = num + 1;
					int num4 = num3 + num2;
					bool flag = false;
					for (int j = num3; j < num4; j++)
					{
						int index = readOnlySpan[j];
						var (baseEntity, basePlayer) = readOnlySpan2[index];
						if (NeedsSerialization(baseEntity, basePlayer.net.connection))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						toSerializeAndSend.Add(num);
					}
					else
					{
						toSend.Add(num);
					}
					num += num2;
				}
			}
		}
		static void MergeDepsChains(ReadOnlySpan<int> readOnlySpan, ReadOnlySpan<int> chainIndices, int mergeLimit, BufferList<int> newChains, BufferList<int> newChainIndices)
		{
			int num = 0;
			int num2 = 0;
			for (int j = 0; j < chainIndices.Length; j++)
			{
				int index = chainIndices[j];
				int num3 = readOnlySpan[index];
				num2 += num3;
				if (num2 > mergeLimit)
				{
					int count = newChains.Count;
					newChains.Add(num2);
					for (int k = num; k <= j; k++)
					{
						int num4 = chainIndices[k];
						int length = readOnlySpan[num4];
						int start = num4 + 1;
						newChains.AddSpan(readOnlySpan.Slice(start, length));
					}
					newChainIndices.Add(count);
					num = j + 1;
					num2 = 0;
				}
			}
			if (num2 > 0)
			{
				int count2 = newChains.Count;
				newChains.Add(num2);
				for (int l = num; l < chainIndices.Length; l++)
				{
					int num5 = chainIndices[l];
					int length2 = readOnlySpan[num5];
					int start2 = num5 + 1;
					newChains.AddSpan(readOnlySpan.Slice(start2, length2));
				}
				newChainIndices.Add(count2);
			}
		}
		static bool NeedsSerialization(BaseEntity from, Network.Connection to)
		{
			if (from.HasNetworkCache)
			{
				return !from.CanUseNetworkCache(to);
			}
			return true;
		}
		[AsyncStateMachine(typeof(_003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed))]
		static UniTask SendSnapshotsAsync(BufferList<(BaseEntity from, BasePlayer to)> pairs, BufferList<int> chains2, BufferList<int> chainIndices, int batchIndex, ThreadSafeTime time2)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			_003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed _003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed2 = default(_003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed);
			_003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed2._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
			_003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed2.pairs = pairs;
			_003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed2.chains = chains2;
			_003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed2.chainIndices = chainIndices;
			_003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed2.batchIndex = batchIndex;
			_003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed2.time = time2;
			_003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed2._003C_003E1__state = -1;
			((AsyncUniTaskMethodBuilder)(ref _003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed2._003C_003Et__builder)).Start<_003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed>(ref _003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed2);
			return ((AsyncUniTaskMethodBuilder)(ref _003C_003CSendEntitySnapshots_003Eg__SendSnapshotsAsync_007C73_0_003Ed2._003C_003Et__builder)).Task;
		}
		static void SendSnapshotsMain(ReadOnlySpan<(BaseEntity from, BasePlayer to)> readOnlySpan3, ReadOnlySpan<int> readOnlySpan2, ReadOnlySpan<int> chainIndices, in ThreadSafeTime time2)
		{
			using (TimeWarning.New("SendSnapshotsMain"))
			{
				Network.Server sv = Net.sv;
				ReadOnlySpan<int> readOnlySpan = chainIndices;
				for (int j = 0; j < readOnlySpan.Length; j++)
				{
					int num = readOnlySpan[j];
					int num2 = readOnlySpan2[num];
					int num3 = num + 1;
					int num4 = num3 + num2;
					for (int k = num3; k < num4; k++)
					{
						int index = readOnlySpan2[k];
						(BaseEntity from, BasePlayer to) tuple = readOnlySpan3[index];
						BaseEntity item = tuple.from;
						BasePlayer item2 = tuple.to;
						NetWrite write = sv.StartWrite();
						item.SendAsSnapshot(item2.net.connection, write, in time2);
					}
				}
			}
		}
	}

	private static void SendEntitySnapshotsWithChildren(ReadOnlySpan<(BaseEntity, BasePlayer)> allPairs, List<UniTask> tasks)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SendEntitySnapshotsWithChildren"))
		{
			BufferList<(BaseEntity, BasePlayer)> val = Pool.Get<BufferList<(BaseEntity, BasePlayer)>>();
			BufferList<(BaseEntity, BasePlayer)> val2 = Pool.Get<BufferList<(BaseEntity, BasePlayer)>>();
			BufferList<(int, int)> val3 = Pool.Get<BufferList<(int, int)>>();
			FilterPairsForThreads(allPairs, val, val2, val3, ConVar.Server.SnapshotTaskBatchCount);
			ThreadSafeTime time = ThreadSafeTime.TakeSnapshot();
			if (val3.Count > 0)
			{
				PooledList<UniTask> val4 = Pool.Get<PooledList<UniTask>>();
				try
				{
					for (int i = 0; i < val3.Count; i++)
					{
						var (start, count) = val3[i];
						((List<UniTask>)(object)val4).Add(ProcessBatch(val2, start, count, time));
					}
					UniTask workTask = UniTask.WhenAll((IEnumerable<UniTask>)val4);
					UniTask item = Cleanup(val2, workTask);
					tasks.Add(item);
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
			else
			{
				Pool.FreeUnmanaged<(BaseEntity, BasePlayer)>(ref val2);
			}
			Pool.FreeUnmanaged<(int, int)>(ref val3);
			SendSnapshotsMain(val.ContentReadOnlySpan(), in time);
			Pool.FreeUnmanaged<(BaseEntity, BasePlayer)>(ref val);
		}
		[AsyncStateMachine(typeof(_003C_003CSendEntitySnapshotsWithChildren_003Eg__Cleanup_007C74_1_003Ed))]
		static UniTask Cleanup(BufferList<(BaseEntity from, BasePlayer to)> pairs, UniTask workTask2)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			_003C_003CSendEntitySnapshotsWithChildren_003Eg__Cleanup_007C74_1_003Ed _003C_003CSendEntitySnapshotsWithChildren_003Eg__Cleanup_007C74_1_003Ed2 = default(_003C_003CSendEntitySnapshotsWithChildren_003Eg__Cleanup_007C74_1_003Ed);
			_003C_003CSendEntitySnapshotsWithChildren_003Eg__Cleanup_007C74_1_003Ed2._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
			_003C_003CSendEntitySnapshotsWithChildren_003Eg__Cleanup_007C74_1_003Ed2.pairs = pairs;
			_003C_003CSendEntitySnapshotsWithChildren_003Eg__Cleanup_007C74_1_003Ed2.workTask = workTask2;
			_003C_003CSendEntitySnapshotsWithChildren_003Eg__Cleanup_007C74_1_003Ed2._003C_003E1__state = -1;
			((AsyncUniTaskMethodBuilder)(ref _003C_003CSendEntitySnapshotsWithChildren_003Eg__Cleanup_007C74_1_003Ed2._003C_003Et__builder)).Start<_003C_003CSendEntitySnapshotsWithChildren_003Eg__Cleanup_007C74_1_003Ed>(ref _003C_003CSendEntitySnapshotsWithChildren_003Eg__Cleanup_007C74_1_003Ed2);
			return ((AsyncUniTaskMethodBuilder)(ref _003C_003CSendEntitySnapshotsWithChildren_003Eg__Cleanup_007C74_1_003Ed2._003C_003Et__builder)).Task;
		}
		[AsyncStateMachine(typeof(_003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed))]
		static UniTask ProcessBatch(BufferList<(BaseEntity from, BasePlayer to)> pairs, int start2, int count2, ThreadSafeTime time2)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			_003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed _003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed2 = default(_003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed);
			_003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed2._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
			_003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed2.pairs = pairs;
			_003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed2.start = start2;
			_003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed2.count = count2;
			_003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed2.time = time2;
			_003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed2._003C_003E1__state = -1;
			((AsyncUniTaskMethodBuilder)(ref _003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed2._003C_003Et__builder)).Start<_003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed>(ref _003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed2);
			return ((AsyncUniTaskMethodBuilder)(ref _003C_003CSendEntitySnapshotsWithChildren_003Eg__ProcessBatch_007C74_0_003Ed2._003C_003Et__builder)).Task;
		}
	}

	private static void FilterPairsForThreads(ReadOnlySpan<(BaseEntity from, BasePlayer to)> allPairs, BufferList<(BaseEntity, BasePlayer)> toSerializeAndSend, BufferList<(BaseEntity, BasePlayer)> toSend, BufferList<(int start, int count)> toSendBatches, int snapshotsPerBatch)
	{
		using (TimeWarning.New("FilterPairsForThreads"))
		{
			PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
			try
			{
				int num = 0;
				ReadOnlySpan<(BaseEntity, BasePlayer)> readOnlySpan = allPairs;
				for (int i = 0; i < readOnlySpan.Length; i++)
				{
					(BaseEntity, BasePlayer) tuple = readOnlySpan[i];
					if (NeedsSerializationWithChildren(tuple.Item1, tuple.Item2, (List<BaseEntity>)(object)val))
					{
						foreach (BaseEntity item in (List<BaseEntity>)(object)val)
						{
							toSerializeAndSend.Add((item, tuple.Item2));
						}
					}
					else
					{
						foreach (BaseEntity item2 in (List<BaseEntity>)(object)val)
						{
							toSend.Add((item2, tuple.Item2));
						}
						int num2 = toSend.Count - num;
						if (num2 >= snapshotsPerBatch)
						{
							toSendBatches.Add((num, num2));
							num = toSend.Count;
						}
					}
					((List<BaseEntity>)(object)val).Clear();
				}
				if (num != toSend.Count)
				{
					toSendBatches.Add((num, toSend.Count - num));
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private static bool NeedsSerializationWithChildren(BaseEntity from, BasePlayer to, List<BaseEntity> visited)
	{
		bool flag = NeedsSerialization(from, to.net.connection);
		visited.Add(from);
		foreach (BaseEntity child in from.children)
		{
			if (child.ShouldNetworkTo(to))
			{
				flag |= NeedsSerializationWithChildren(child, to, visited);
			}
		}
		return flag;
	}

	private static bool NeedsSerialization(BaseEntity from, Network.Connection to)
	{
		if (from.HasNetworkCache)
		{
			return !from.CanUseNetworkCache(to);
		}
		return true;
	}

	private static void SendSnapshotsMain(ReadOnlySpan<(BaseEntity from, BasePlayer to)> pairs, in ThreadSafeTime time)
	{
		using (TimeWarning.New("SendSnapshotsMain"))
		{
			ReadOnlySpan<(BaseEntity, BasePlayer)> readOnlySpan = pairs;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				var (baseEntity, basePlayer) = readOnlySpan[i];
				baseEntity.SendAsSnapshot(basePlayer.net.connection, in time);
			}
		}
	}

	private static void SendEntityDestroyMessages(BufferList<(BaseEntity from, BasePlayer to)> pairs, List<UniTask> tasks)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SendEntityDestroyMessages"))
		{
			int destroyTaskBatchCount = ConVar.Server.DestroyTaskBatchCount;
			int num = (pairs.Count + destroyTaskBatchCount - 1) / destroyTaskBatchCount;
			for (int i = 0; i < num; i++)
			{
				tasks.Add(ProcessBatch(pairs, i, destroyTaskBatchCount));
			}
		}
		[AsyncStateMachine(typeof(_003C_003CSendEntityDestroyMessages_003Eg__ProcessBatch_007C79_0_003Ed))]
		static UniTask ProcessBatch(BufferList<(BaseEntity from, BasePlayer to)> pairs2, int index, int batchSize)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			_003C_003CSendEntityDestroyMessages_003Eg__ProcessBatch_007C79_0_003Ed _003C_003CSendEntityDestroyMessages_003Eg__ProcessBatch_007C79_0_003Ed2 = default(_003C_003CSendEntityDestroyMessages_003Eg__ProcessBatch_007C79_0_003Ed);
			_003C_003CSendEntityDestroyMessages_003Eg__ProcessBatch_007C79_0_003Ed2._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
			_003C_003CSendEntityDestroyMessages_003Eg__ProcessBatch_007C79_0_003Ed2.pairs = pairs2;
			_003C_003CSendEntityDestroyMessages_003Eg__ProcessBatch_007C79_0_003Ed2.index = index;
			_003C_003CSendEntityDestroyMessages_003Eg__ProcessBatch_007C79_0_003Ed2.batchSize = batchSize;
			_003C_003CSendEntityDestroyMessages_003Eg__ProcessBatch_007C79_0_003Ed2._003C_003E1__state = -1;
			((AsyncUniTaskMethodBuilder)(ref _003C_003CSendEntityDestroyMessages_003Eg__ProcessBatch_007C79_0_003Ed2._003C_003Et__builder)).Start<_003C_003CSendEntityDestroyMessages_003Eg__ProcessBatch_007C79_0_003Ed>(ref _003C_003CSendEntityDestroyMessages_003Eg__ProcessBatch_007C79_0_003Ed2);
			return ((AsyncUniTaskMethodBuilder)(ref _003C_003CSendEntityDestroyMessages_003Eg__ProcessBatch_007C79_0_003Ed2._003C_003Et__builder)).Task;
		}
	}

	private static void WaitForTasks(List<UniTask> tasks)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (CollectionEx.IsEmpty(tasks))
		{
			return;
		}
		using (TimeWarning.New("WaitForTasks"))
		{
			bool flag;
			do
			{
				flag = false;
				foreach (UniTask task in tasks)
				{
					UniTask current = task;
					flag |= !UniTaskStatusExtensions.IsCompleted(((UniTask)(ref current)).Status);
				}
			}
			while (flag);
			foreach (UniTask task2 in tasks)
			{
				UniTask current2 = task2;
				Awaiter awaiter = ((UniTask)(ref current2)).GetAwaiter();
				((Awaiter)(ref awaiter)).GetResult();
			}
		}
	}

	public void ClearEntityQueue(Group group = null)
	{
		SnapshotQueue.Clear(group);
		networkQueue[0].Clear(group);
		networkQueue[1].Clear(group);
	}

	private void SendEntityUpdates(NetworkQueueList queue)
	{
		if (queue.queueInternal.Count == 0)
		{
			return;
		}
		int num = (IsReceivingSnapshot ? ConVar.Server.updatebatchspawn : ConVar.Server.updatebatch);
		List<BaseNetworkable> list = Pool.Get<List<BaseNetworkable>>();
		using (TimeWarning.New("SendEntityUpdates.SendEntityUpdates"))
		{
			int num2 = 0;
			foreach (BaseNetworkable item in queue.queueInternal)
			{
				SendEntitySnapshot(item);
				list.Add(item);
				num2++;
				if (num2 > num)
				{
					break;
				}
			}
		}
		if (num > queue.queueInternal.Count)
		{
			queue.queueInternal.Clear();
		}
		else
		{
			using (TimeWarning.New("SendEntityUpdates.Remove"))
			{
				for (int i = 0; i < list.Count; i++)
				{
					queue.queueInternal.Remove(list[i]);
				}
			}
		}
		if (queue.queueInternal.Count == 0 && queue.MaxLength > 2048)
		{
			queue.queueInternal.Clear();
			queue.queueInternal = new HashSet<BaseNetworkable>();
			queue.MaxLength = 0;
		}
		Pool.FreeUnmanaged<BaseNetworkable>(ref list);
	}

	public void SendEntitySnapshot(BaseNetworkable ent)
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnEntitySnapshot", ent, net.connection) != null)
		{
			return;
		}
		using (TimeWarning.New("SendEntitySnapshot"))
		{
			if (!((Object)(object)ent == (Object)null) && ent.net != null && ent.ShouldNetworkTo(this))
			{
				NetWrite netWrite = Net.sv.StartWrite();
				net.connection.validate.entityUpdates++;
				SaveInfo saveInfo = new SaveInfo
				{
					forConnection = net.connection,
					forDisk = false,
					cachedTime = ThreadSafeTime.TakeSnapshot()
				};
				netWrite.PacketID(Message.Type.Entities);
				netWrite.UInt32(net.connection.validate.entityUpdates);
				ent.ToStreamForNetwork(netWrite, saveInfo);
				if (PacketProfiler.shouldCaptureDetailedProfiling)
				{
					PacketProfiler.LogDetailedOutbound(Message.Type.Entities, net.ID, ((Object)(object)ent != (Object)null) ? ent.PrefabName : null, (int)netWrite.Length, null, Epoch.Current, server: true);
				}
				netWrite.Send(new SendInfo(net.connection));
			}
		}
	}

	public bool HasPlayerFlag(PlayerFlags f)
	{
		return HasPlayerFlag(playerFlags, f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasPlayerFlag(PlayerFlags flags, PlayerFlags f)
	{
		return (flags & f) == f;
	}

	public int GetSkinsAccessLevel()
	{
		if (!IsDeveloper)
		{
			return 0;
		}
		if (base.isServer && IsConnected)
		{
			return net.connection.info.GetInt("client.skins_access");
		}
		return 0;
	}

	public void SetPlayerFlag(PlayerFlags f, bool b)
	{
		if (b)
		{
			if (HasPlayerFlag(f))
			{
				return;
			}
			playerFlags |= f;
		}
		else
		{
			if (!HasPlayerFlag(f))
			{
				return;
			}
			playerFlags &= ~f;
		}
		SendNetworkUpdate();
	}

	[RPC_Server.CallsPerSecond(16uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	public void FogImageUpdate(RPCMessage msg)
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		byte b = msg.read.UInt8();
		byte b2 = msg.read.UInt8();
		uint num = msg.read.UInt32();
		uint num2 = msg.read.UInt32();
		if (num2 > 32)
		{
			return;
		}
		List<uint> fogImageList = GetFogImageList();
		if (fogImageList.Count != 16)
		{
			fogImageList.Clear();
			for (int i = 0; i < 16; i++)
			{
				fogImageList.Add(0u);
			}
		}
		if (b != 0 || fogImageList[b2] != num)
		{
			byte[] array = msg.read.BytesWithSize(5000u);
			if (array != null && Interface.CallHook("OnFogOfWarImageUpdate", this, b, b2, num, num2, array) == null)
			{
				FileStorage.server.RemoveEntityNum(net.ID, num2);
				uint value = FileStorage.server.Store(array, FileStorage.Type.png, net.ID, num2);
				fogImageList[b2] = value;
				DirtyPlayerState();
			}
		}
	}

	private List<uint> GetFogImageList()
	{
		return GetFogImageList(CurrentFogMode);
	}

	private List<uint> GetFogImageList(FogMode mode)
	{
		if (mode == FogMode.Mainland)
		{
			if (State.fogImagesMainland == null)
			{
				State.fogImagesMainland = (List<uint>)(object)Pool.Get<PooledList<uint>>();
			}
			return State.fogImagesMainland;
		}
		if (State.fogImagesDeepSea == null)
		{
			State.fogImagesDeepSea = (List<uint>)(object)Pool.Get<PooledList<uint>>();
		}
		return State.fogImagesDeepSea;
	}

	public void ServerClearFog(bool mainland, bool deepSea)
	{
		if (mainland)
		{
			ClearFogList(ref State.fogImagesMainland);
		}
		if (deepSea)
		{
			ClearFogList(ref State.fogImagesDeepSea);
		}
		Interface.CallHook("OnFogOfWarCleared", this, mainland, deepSea);
		DirtyPlayerState();
		SendFogImagesToClient();
		void ClearFogList(ref List<uint> fog)
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			if (fog == null || fog.Count != 16)
			{
				fog = (List<uint>)(object)Pool.Get<PooledList<uint>>();
				for (int i = 0; i < 16; i++)
				{
					fog.Add(0u);
				}
			}
			else
			{
				for (int j = 0; j < fog.Count; j++)
				{
					if (fog[j] != 0)
					{
						FileStorage.server.Remove(fog[j], FileStorage.Type.png, net.ID);
						fog[j] = 0u;
					}
				}
			}
		}
	}

	private void SendFogImagesToClient()
	{
		PooledList<uint> val = Pool.Get<PooledList<uint>>();
		try
		{
			((List<uint>)(object)val).AddRange((IEnumerable<uint>)GetFogImageList(FogMode.Mainland));
			((List<uint>)(object)val).AddRange((IEnumerable<uint>)GetFogImageList(FogMode.DeepSea));
			ClientRPCList(RpcTarget.Player("ReceiveFogOfWarImages", this), (List<uint>)(object)val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void OnFogOfWarStale()
	{
		ClearFogList(GetFogImageList(FogMode.Mainland));
		ClearFogList(GetFogImageList(FogMode.DeepSea));
		Interface.CallHook("OnFogOfWarStale", this);
		static void ClearFogList(List<uint> l)
		{
			l.Clear();
			for (int i = 0; i < 16; i++)
			{
				l.Add(0u);
			}
		}
	}

	private RPSWinState Opposite(RPSWinState state)
	{
		return state switch
		{
			RPSWinState.Win => RPSWinState.Loss, 
			RPSWinState.Loss => RPSWinState.Win, 
			_ => state, 
		};
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(1uL)]
	public void Server_StartGesture(RPCMessage msg)
	{
		if (!IsGestureBlocked())
		{
			uint id = msg.read.UInt32();
			GestureConfig toPlay = GestureCollection.Instance.IdToGesture(id);
			Server_StartGesture(toPlay);
		}
	}

	public void Server_StartGesture(uint gestureId, GestureStartSource startSource = GestureStartSource.Player, bool bypassOwnershipCheck = false)
	{
		GestureConfig toPlay = GestureCollection.Instance.IdToGesture(gestureId);
		Server_StartGesture(toPlay, startSource, bypassOwnershipCheck);
	}

	public void Server_StartGesture(string gestureConvarName, GestureStartSource startSource = GestureStartSource.Player, bool bypassOwnershipCheck = false)
	{
		GestureConfig toPlay = GestureCollection.Instance.GestureConvarNameToGesture(gestureConvarName);
		Server_StartGesture(toPlay, startSource, bypassOwnershipCheck);
	}

	public void Server_StartGesture(GestureConfig toPlay, GestureStartSource startSource = GestureStartSource.Player, bool bypassOwnershipCheck = false)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)toPlay == (Object)null || (toPlay.hideInWheel && startSource == GestureStartSource.Player && !ConVar.Server.cinematic) || (!bypassOwnershipCheck && startSource != GestureStartSource.ServerAction && !toPlay.IsOwnedBy(this)) || !toPlay.CanBeUsedBy(this))
		{
			return;
		}
		if (toPlay.animationType == GestureConfig.AnimationType.OneShot)
		{
			Invoke(actionTimeoutGestureServer, toPlay.duration);
		}
		else if (toPlay.animationType == GestureConfig.AnimationType.Loop)
		{
			InvokeRepeating(actionMonitorLoopingGesture, 0f, 0f);
		}
		ClientRPC(RpcTarget.NetworkGroup("Client_StartGesture"), toPlay.gestureId);
		gestureFinishedTime = TimeUntil.op_Implicit(toPlay.duration);
		currentGesture = toPlay;
		if (!IsNpc && !IsBot)
		{
			switch (toPlay.actionType)
			{
			case GestureConfig.GestureActionType.Surrender:
				inventory.SetLockedByRestraint(flag: true);
				break;
			case GestureConfig.GestureActionType.ShowNameTag:
				if (Rust.GameInfo.HasAchievements)
				{
					int val = CountWaveTargets(((Component)this).transform.position, 4f, 0.6f, eyes.HeadForward(), recentWaveTargets, 5);
					stats.Add("waved_at_players", val);
					stats.Save(forceSteamSave: true);
				}
				break;
			case GestureConfig.GestureActionType.DanceAchievement:
			{
				TriggerDanceAchievement triggerDanceAchievement = FindTrigger<TriggerDanceAchievement>();
				if ((Object)(object)triggerDanceAchievement != (Object)null)
				{
					triggerDanceAchievement.NotifyDanceStarted();
				}
				break;
			}
			}
		}
		if (startSource == GestureStartSource.Player && toPlay.hasMultiplayerInteraction)
		{
			SetPlayerFlag(PlayerFlags.WaitingForGestureInteraction, b: true);
		}
		if (toPlay.animationType == GestureConfig.AnimationType.Loop)
		{
			SendNetworkUpdate();
		}
	}

	private void TimeoutGestureServer()
	{
		currentGesture = null;
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(10uL)]
	public void Server_CancelGesture()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)currentGesture != (Object)null && currentGesture.actionType == GestureConfig.GestureActionType.Surrender)
		{
			Handcuffs handcuffs = GetHeldEntity() as Handcuffs;
			if ((Object)(object)handcuffs == (Object)null || !handcuffs.Locked)
			{
				inventory.SetLockedByRestraint(flag: false);
			}
		}
		currentGesture = null;
		blockHeldInputTimer = TimeSince.op_Implicit(0f);
		SetPlayerFlag(PlayerFlags.WaitingForGestureInteraction, b: false);
		ClientRPC(RpcTarget.NetworkGroup("Client_RemoteCancelledGesture"));
		CancelInvoke(actionMonitorLoopingGesture);
		CancelInvoke(actionTimeoutGestureServer);
	}

	private void MonitorLoopingGesture()
	{
		bool flag = (Object)(object)currentGesture != (Object)null && currentGesture.canDuckDuringGesture;
		if (modelState == null || (!flag && modelState.ducked) || modelState.sleeping || IsWounded() || IsSwimming() || IsDead() || (isMounted && GetMounted().allowedGestures == BaseMountable.MountGestureType.UpperBody && !CurrentGestureIsUpperBody) || (isMounted && GetMounted().allowedGestures == BaseMountable.MountGestureType.None))
		{
			Server_CancelGesture();
		}
	}

	private void NotifyGesturesNewItemEquipped()
	{
		if (InGesture)
		{
			Server_CancelGesture();
		}
	}

	public int CountWaveTargets(Vector3 position, float distance, float minimumDot, Vector3 forward, HashSet<NetworkableId> workingList, int maxCount)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		float sqrDistance = distance * distance;
		Group obj = net.group;
		if (obj == null)
		{
			return 0;
		}
		List<Network.Connection> subscribers = obj.subscribers;
		int num = 0;
		for (int i = 0; i < subscribers.Count; i++)
		{
			Network.Connection connection = subscribers[i];
			if (!connection.active)
			{
				continue;
			}
			BasePlayer basePlayer = connection.player as BasePlayer;
			if (CheckPlayer(basePlayer))
			{
				workingList.Add(basePlayer.net.ID);
				num++;
				if (num >= maxCount)
				{
					break;
				}
			}
		}
		return num;
		bool CheckPlayer(BasePlayer player)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)player == (Object)null)
			{
				return false;
			}
			if ((Object)(object)player == (Object)(object)this)
			{
				return false;
			}
			if (player.SqrDistance(position) > sqrDistance)
			{
				return false;
			}
			Vector3 val = ((Component)player).transform.position - position;
			if (Vector3.Dot(((Vector3)(ref val)).normalized, forward) < minimumDot)
			{
				return false;
			}
			if (workingList.Contains(player.net.ID))
			{
				return false;
			}
			return true;
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void RequestJoinGesture(RPCMessage msg)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId uid = msg.read.EntityID();
		BasePlayer basePlayer = BaseNetworkable.serverEntities.Find(uid) as BasePlayer;
		if (!HasPlayerFlag(PlayerFlags.WaitingForGestureInteraction) || !InGesture || (Object)(object)currentGesture == (Object)null)
		{
			return;
		}
		interactiveGestureStartTime = TimeSince.op_Implicit(0f);
		if ((Object)(object)msg.player != (Object)(object)basePlayer || !((Object)(object)basePlayer != (Object)null))
		{
			return;
		}
		SetPlayerFlag(PlayerFlags.WaitingForGestureInteraction, b: false);
		if (currentGesture.actionType == GestureConfig.GestureActionType.RockPaperScissors)
		{
			rpsTarget = uid;
			basePlayer.rpsTarget = net.ID;
			basePlayer.Server_StartGesture(GestureCollection.Instance.GestureConvarNameToGesture("rps"), GestureStartSource.ServerAction);
			ClientRPC(RpcTarget.Player("PromptToPickRPSHand", basePlayer), 10f);
			ClientRPC(RpcTarget.Player("PromptToPickRPSHand", this), 10f);
			if (basePlayer.IsBot)
			{
				basePlayer.Invoke(actionBotRPSRandomise, 2f);
			}
			if (IsBot)
			{
				Invoke(actionBotRPSRandomise, 2f);
			}
			InvokeRepeating(MonitorRPSGame, 0f, 0f);
		}
	}

	private void BotRPSRandomise()
	{
		selectedRpsOption = Random.Range(0, 3);
		Debug.Log((object)$"Bot randomly selected {selectedRpsOption}");
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	private void SelectedRPSOption(RPCMessage msg)
	{
		selectedRpsOption = msg.read.Int32();
	}

	private void MonitorRPSGame()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		BasePlayer basePlayer = BaseNetworkable.serverEntities.Find(rpsTarget) as BasePlayer;
		if ((Object)(object)basePlayer == (Object)null || Distance((BaseEntity)basePlayer) > 5f || IsWounded() || basePlayer.IsWounded() || IsDead() || basePlayer.IsDead())
		{
			flag = true;
		}
		if (!flag && TimeSince.op_Implicit(interactiveGestureStartTime) > 10f)
		{
			flag = true;
		}
		if (flag)
		{
			ClientRPC(RpcTarget.Player("CancelRPSGame", this));
			Server_CancelGesture();
			if ((Object)(object)basePlayer != (Object)null)
			{
				ClientRPC(RpcTarget.Player("CancelRPSGame", basePlayer));
				basePlayer.Server_CancelGesture();
			}
			CancelInvoke(actionMonitorRPSGame);
		}
		if ((Object)(object)basePlayer != (Object)null && basePlayer.selectedRpsOption != -1 && selectedRpsOption != -1)
		{
			RPSWinState rPSWinState = (((selectedRpsOption != 0 || basePlayer.selectedRpsOption != 2) && (selectedRpsOption != 1 || basePlayer.selectedRpsOption != 0) && (selectedRpsOption != 2 || basePlayer.selectedRpsOption != 1)) ? RPSWinState.Loss : RPSWinState.Win);
			if (selectedRpsOption == basePlayer.selectedRpsOption)
			{
				rPSWinState = RPSWinState.Draw;
			}
			ClientRPC(RpcTarget.NetworkGroup("OnRPSResult"), (int)rPSWinState, selectedRpsOption);
			basePlayer.ClientRPC(RpcTarget.NetworkGroup("OnRPSResult"), (int)Opposite(rPSWinState), basePlayer.selectedRpsOption);
			basePlayer.selectedRpsOption = -1;
			basePlayer.rpsTarget = default(NetworkableId);
			selectedRpsOption = -1;
			rpsTarget = default(NetworkableId);
			CancelInvoke(actionMonitorRPSGame);
			float time = ((rPSWinState == RPSWinState.Draw) ? 2.5f : 5f);
			Invoke(actionServer_CancelGesture, time);
			basePlayer.Invoke(basePlayer.actionServer_CancelGesture, time);
		}
	}

	private bool IsGestureBlocked()
	{
		if (isMounted && GetMounted().allowedGestures == BaseMountable.MountGestureType.None)
		{
			return true;
		}
		if (Object.op_Implicit((Object)(object)GetHeldEntity()) && GetHeldEntity().BlocksGestures())
		{
			return true;
		}
		bool flag = (Object)(object)currentGesture != (Object)null;
		if (flag && currentGesture.gestureType == GestureConfig.GestureType.Cinematic)
		{
			flag = false;
		}
		if (!(IsWounded() || flag) && !IsDead() && !IsSleeping())
		{
			return IsRestrained;
		}
		return true;
	}

	public bool InATeam()
	{
		if (currentTeam == 0L)
		{
			return false;
		}
		if (Team == null)
		{
			Debug.LogWarning((object)$"currentTeam on ({((Object)this).name}), userID ({userID.Get()}) is {currentTeam} but Team is null", (Object)(object)this);
			return false;
		}
		return true;
	}

	public void DelayedTeamUpdate()
	{
		UpdateTeam(currentTeam);
	}

	public void TeamUpdate()
	{
		TeamUpdate(fullTeamUpdate: false);
	}

	public void TeamUpdate(bool fullTeamUpdate)
	{
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		if (!RelationshipManager.TeamsEnabled() || !IsConnected || currentTeam == 0L)
		{
			return;
		}
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindTeam(currentTeam);
		if (playerTeam == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		PlayerTeam val = Pool.Get<PlayerTeam>();
		try
		{
			val.teamLeader = playerTeam.teamLeader;
			val.teamID = playerTeam.teamID;
			val.teamName = playerTeam.teamName;
			val.members = Pool.Get<List<TeamMember>>();
			val.teamLifetime = playerTeam.teamLifetime;
			val.teamPings = Pool.Get<List<MapNote>>();
			foreach (ulong member in playerTeam.members)
			{
				BasePlayer basePlayer = RelationshipManager.FindByID(member);
				if (Object.op_Implicit((Object)(object)basePlayer) && basePlayer.IsInTutorial)
				{
					continue;
				}
				TeamMember val2 = Pool.Get<TeamMember>();
				val2.displayName = (((Object)(object)basePlayer != (Object)null) ? basePlayer.displayName : (SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(member) ?? "DEAD"));
				val2.healthFraction = (((Object)(object)basePlayer != (Object)null && basePlayer.IsAlive()) ? basePlayer.healthFraction : 0f);
				val2.position = (((Object)(object)basePlayer != (Object)null) ? ((Component)basePlayer).transform.position : Vector3.zero);
				val2.online = (Object)(object)basePlayer != (Object)null && !basePlayer.IsSleeping();
				val2.wounded = (Object)(object)basePlayer != (Object)null && basePlayer.IsWounded();
				if ((!sentInstrumentTeamAchievement || !sentSummerTeamAchievement) && (Object)(object)basePlayer != (Object)null)
				{
					if (Object.op_Implicit((Object)(object)basePlayer.GetHeldEntity()) && basePlayer.GetHeldEntity().IsInstrument())
					{
						num++;
					}
					if (basePlayer.isMounted)
					{
						if (basePlayer.GetMounted().IsInstrument())
						{
							num++;
						}
						if (basePlayer.GetMounted().IsSummerDlcVehicle)
						{
							num2++;
						}
					}
					if (num >= 4 && !sentInstrumentTeamAchievement)
					{
						GiveAchievement("TEAM_INSTRUMENTS");
						sentInstrumentTeamAchievement = true;
					}
					if (num2 >= 4)
					{
						GiveAchievement("SUMMER_INFLATABLE");
						sentSummerTeamAchievement = true;
					}
				}
				val2.userID = member;
				val.members.Add(val2);
				if ((Object)(object)basePlayer != (Object)null)
				{
					if (basePlayer.State.pings != null && basePlayer.State.pings.Count > 0 && (Object)(object)basePlayer != (Object)(object)this)
					{
						val.teamPings.AddRange(basePlayer.State.pings);
					}
					if (fullTeamUpdate && (Object)(object)basePlayer != (Object)(object)this)
					{
						basePlayer.TeamUpdate(fullTeamUpdate: false);
					}
				}
			}
			val.leaderMapNotes = Pool.Get<List<MapNote>>();
			PlayerState val3 = SingletonComponent<ServerMgr>.Instance.playerStateManager.Get(playerTeam.teamLeader);
			if (val3?.pointsOfInterest != null)
			{
				foreach (MapNote item in val3.pointsOfInterest)
				{
					val.leaderMapNotes.Add(item);
				}
			}
			if (Interface.CallHook("OnTeamUpdated", currentTeam, val, this) == null)
			{
				ClientRPC(RpcTarget.PlayerAndSpectators("CLIENT_ReceiveTeamInfo", this), val);
				if (val.leaderMapNotes != null)
				{
					val.leaderMapNotes.Clear();
				}
				if (val.teamPings != null)
				{
					val.teamPings.Clear();
				}
				BasePlayer basePlayer2 = FindByID(playerTeam.teamLeader);
				if (fullTeamUpdate && (Object)(object)basePlayer2 != (Object)null && (Object)(object)basePlayer2 != (Object)(object)this)
				{
					basePlayer2.TeamUpdate(fullTeamUpdate: false);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void UpdateTeam(ulong newTeam)
	{
		if (Interface.CallHook("OnTeamUpdate", currentTeam, newTeam, this) == null)
		{
			currentTeam = newTeam;
			SendNetworkUpdate();
			if (RelationshipManager.ServerInstance.FindTeam(newTeam) == null)
			{
				ClearTeam();
			}
			else
			{
				TeamUpdate();
			}
		}
	}

	public void ClearTeam()
	{
		currentTeam = 0uL;
		ClientRPC(RpcTarget.PlayerAndSpectators("CLIENT_ClearTeam", this));
		SendNetworkUpdate();
	}

	public void ClearPendingInvite()
	{
		ClientRPC(RpcTarget.Player("CLIENT_PendingInvite", this), "", 0uL, 0uL);
	}

	public HeldEntity GetHeldEntity()
	{
		if (base.isServer)
		{
			Item activeItem = GetActiveItem();
			if (activeItem == null)
			{
				return null;
			}
			return activeItem.GetHeldEntity() as HeldEntity;
		}
		return null;
	}

	public bool TryGetHeldEntity<T>(out T heldEntity) where T : HeldEntity
	{
		heldEntity = null;
		HeldEntity heldEntity2 = GetHeldEntity();
		if ((Object)(object)heldEntity2 == (Object)null)
		{
			return false;
		}
		if (heldEntity2 is T val)
		{
			heldEntity = val;
			return true;
		}
		return false;
	}

	public bool TryGetHeldEntity(out HeldEntity heldEntity)
	{
		return this.TryGetHeldEntity<HeldEntity>(out heldEntity);
	}

	public bool IsHoldingEntity<T>()
	{
		HeldEntity heldEntity = GetHeldEntity();
		if ((Object)(object)heldEntity == (Object)null)
		{
			return false;
		}
		return heldEntity is T;
	}

	public Item GetActiveItem()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			if (!((ItemId)(ref svActiveItemID)).IsValid)
			{
				return null;
			}
			if (IsDead())
			{
				return null;
			}
			if ((Object)(object)inventory == (Object)null || inventory.containerBelt == null)
			{
				return null;
			}
			return inventory.containerBelt.FindItemByUID(svActiveItemID);
		}
		return null;
	}

	public bool TryGetActiveItem(out Item item)
	{
		item = GetActiveItem();
		return item != null;
	}

	public Shield GetActiveShield()
	{
		if (!Object.op_Implicit((Object)(object)GetHeldEntity()))
		{
			return null;
		}
		if (!GetHeldEntity().canBeUsedWithShield)
		{
			return null;
		}
		Item anyBackpack = inventory.GetAnyBackpack();
		ItemModShield itemModShield = default(ItemModShield);
		if (anyBackpack != null && ((Component)anyBackpack.info).TryGetComponent<ItemModShield>(ref itemModShield))
		{
			return anyBackpack.GetHeldEntity() as Shield;
		}
		return null;
	}

	public bool TryGetActiveShield(out Shield foundShield)
	{
		foundShield = GetActiveShield();
		return (Object)(object)foundShield != (Object)null;
	}

	public bool WantsShieldOnBack()
	{
		if (base.isServer)
		{
			return GetInfoBool("client.shieldonback", defaultVal: false);
		}
		return false;
	}

	public bool IsHostileItem(Item item)
	{
		if (!item.info.isHoldable)
		{
			return false;
		}
		ItemModEntity component = ((Component)item.info).GetComponent<ItemModEntity>();
		if ((Object)(object)component == (Object)null)
		{
			return false;
		}
		GameObject val = component.entityPrefab.Get();
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		AttackEntity component2 = val.GetComponent<AttackEntity>();
		if ((Object)(object)component2 == (Object)null)
		{
			return false;
		}
		return component2.hostile;
	}

	public bool IsItemHoldRestricted(Item item)
	{
		if (IsNpc)
		{
			return false;
		}
		if (InSafeZone() && item != null && IsHostileItem(item) && !HasPlayerFlag(PlayerFlags.CombatZone))
		{
			return true;
		}
		return false;
	}

	public virtual void HeldEntityServerTick()
	{
		HeldEntity heldEntity = GetHeldEntity();
		if ((Object)(object)heldEntity != (Object)null)
		{
			heldEntity.ServerTick(this);
			if (heldEntity.canBeUsedWithShield && TryGetActiveShield(out var foundShield))
			{
				foundShield.ServerTick(this);
			}
		}
	}

	public void LightToggle(bool mask = true)
	{
		Item activeItem = GetActiveItem();
		if (activeItem != null)
		{
			BaseEntity heldEntity = activeItem.GetHeldEntity();
			if ((Object)(object)heldEntity != (Object)null)
			{
				HeldEntity component = ((Component)heldEntity).GetComponent<HeldEntity>();
				if (Object.op_Implicit((Object)(object)component))
				{
					((Component)component).SendMessage("SetLightsOn", (object)(mask && !component.LightsOn()), (SendMessageOptions)1);
				}
			}
		}
		foreach (Item item in inventory.containerWear.itemList)
		{
			ItemModWearable component2 = ((Component)item.info).GetComponent<ItemModWearable>();
			if (Object.op_Implicit((Object)(object)component2) && component2.emissive)
			{
				LightToggle(item, mask);
			}
		}
		if (isMounted)
		{
			GetMounted().LightToggle(this);
		}
	}

	public void LightToggleItem(ulong itemUID)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Item item = inventory.FindItemByUID(new ItemId(itemUID));
		if (item != null)
		{
			LightToggle(item, mask: true);
		}
	}

	public void LightToggleEntity(ulong itemUID)
	{
		Item activeItem = GetActiveItem();
		object obj;
		if (activeItem == null)
		{
			obj = null;
		}
		else
		{
			BaseEntity heldEntity = activeItem.GetHeldEntity();
			obj = ((heldEntity != null) ? ((Component)heldEntity).GetComponent<HeldEntity>() : null);
		}
		HeldEntity heldEntity2 = (HeldEntity)obj;
		if ((Object)(object)heldEntity2 == (Object)null)
		{
			return;
		}
		if (heldEntity2.net.ID.Value == itemUID)
		{
			heldEntity2.SetLightsOn(!heldEntity2.HasFlag(Flags.Reserved5));
		}
		else
		{
			if (!(heldEntity2 is BaseProjectile baseProjectile))
			{
				return;
			}
			foreach (BaseEntity child in baseProjectile.children)
			{
				if (child.net.ID.Value == itemUID && child is ProjectileWeaponMod projectileWeaponMod)
				{
					bool flag = !projectileWeaponMod.HasFlag(Flags.On);
					using (FlagsUpdateScope flagsUpdateScope = projectileWeaponMod.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
					{
						flagsUpdateScope.Set(Flags.On, flag);
					}
					if (projectileWeaponMod.isLight)
					{
						heldEntity2.SetLightsOn(flag);
					}
				}
			}
		}
	}

	public void LightToggle(Item item, bool mask)
	{
		if (item != null)
		{
			item.SetFlag(Item.Flag.IsOn, mask && !item.HasFlag(Item.Flag.IsOn));
			item.MarkDirty();
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void ReqLightToggle(RPCMessage msg)
	{
		ulong itemUID = msg.read.UInt64();
		LightToggleItem(itemUID);
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void ReqLightToggleEntity(RPCMessage msg)
	{
		ulong itemUID = msg.read.UInt64();
		LightToggleEntity(itemUID);
	}

	public void ClearDeathMarker(bool sendToClient = false)
	{
		if (!IsNpc)
		{
			if (ServerCurrentDeathNote != null)
			{
				Pool.Free<MapNote>(ref State.deathMarker);
			}
			DirtyPlayerState();
			if (sendToClient)
			{
				SendMarkersToClient();
			}
		}
	}

	public void Server_LogDeathMarker(Vector3 position)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!IsNpc)
		{
			if (ServerCurrentDeathNote == null)
			{
				ServerCurrentDeathNote = Pool.Get<MapNote>();
				ServerCurrentDeathNote.noteType = 0;
			}
			ServerCurrentDeathNote.worldPosition = position;
			ClientRPC(RpcTarget.Player("Client_AddNewDeathMarker", this), ServerCurrentDeathNote);
			DirtyPlayerState();
		}
	}

	[RPC_Server.InputValidation(new Type[] { typeof(MapNote) })]
	[RPC_Server.CallsPerSecond(8uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	public void Server_AddMarker(RPCMessage msg)
	{
		MapNote val = msg.read.Proto<MapNote>((MapNote)null);
		if (Interface.CallHook("OnMapMarkerAdd", this, val) != null || !CanUseMapMarkers)
		{
			return;
		}
		if (State.pointsOfInterest == null)
		{
			State.pointsOfInterest = Pool.Get<List<MapNote>>();
		}
		if (State.pointsOfInterest.Count >= ConVar.Server.maximumMapMarkers)
		{
			msg.player.ShowToast(GameTip.Styles.Blue_Short, MarkerLimitPhrase, false, ConVar.Server.maximumMapMarkers.ToString());
			return;
		}
		if (val.label == "auto-name")
		{
			int num = FindUnusedNumberName();
			if (num != -1)
			{
				val.label = num.ToString();
			}
		}
		ValidateMapNote(val);
		if (val.colourIndex == -1)
		{
			val.colourIndex = FindUnusedPointOfInterestColour();
		}
		State.pointsOfInterest.Add(val);
		DirtyPlayerState();
		SendMarkersToClient();
		TeamUpdate();
		Interface.CallHook("OnMapMarkerAdded", this, val);
	}

	private int FindUnusedNumberName(int maxToCheck = 100)
	{
		List<MapNote> pointsOfInterest = State.pointsOfInterest;
		for (int i = 1; i < maxToCheck; i++)
		{
			bool flag = false;
			foreach (MapNote item in pointsOfInterest)
			{
				if (item.label == i.ToString())
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return i;
			}
		}
		return -1;
	}

	private int FindUnusedPointOfInterestColour()
	{
		if (State.pointsOfInterest == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			if (HasColour(num))
			{
				num++;
			}
		}
		return num;
		bool HasColour(int index)
		{
			foreach (MapNote item in State.pointsOfInterest)
			{
				if (item.colourIndex == index)
				{
					return true;
				}
			}
			return false;
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(1uL)]
	public void Server_UpdateMarker(RPCMessage msg)
	{
		if (State.pointsOfInterest == null)
		{
			State.pointsOfInterest = Pool.Get<List<MapNote>>();
		}
		int num = msg.read.Int32();
		if (State.pointsOfInterest.Count <= num)
		{
			return;
		}
		MapNote val = msg.read.Proto<MapNote>((MapNote)null);
		try
		{
			ValidateMapNote(val);
			val.CopyTo(State.pointsOfInterest[num]);
			DirtyPlayerState();
			SendMarkersToClient();
			TeamUpdate();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void ValidateMapNote(MapNote n)
	{
		if (n.label != null)
		{
			n.label = StringExtensions.Truncate(n.label, 10, (string)null).ToUpperInvariant();
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(10uL)]
	public void Server_RemovePointOfInterest(RPCMessage msg)
	{
		int num = msg.read.Int32();
		if (State.pointsOfInterest != null && State.pointsOfInterest.Count > num && num >= 0 && Interface.CallHook("OnMapMarkerRemove", this, State.pointsOfInterest, num) == null)
		{
			State.pointsOfInterest[num].Dispose();
			State.pointsOfInterest.RemoveAt(num);
			DirtyPlayerState();
			SendMarkersToClient();
			TeamUpdate();
		}
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	public void Server_RequestMarkers(RPCMessage msg)
	{
		SendMarkersToClient();
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.FromOwner]
	public void Server_ClearMapMarkers(RPCMessage msg)
	{
		if (Interface.CallHook("OnMapMarkersClear", this, State.pointsOfInterest) != null)
		{
			return;
		}
		MapNote serverCurrentDeathNote = ServerCurrentDeathNote;
		if (serverCurrentDeathNote != null)
		{
			serverCurrentDeathNote.Dispose();
		}
		ServerCurrentDeathNote = null;
		if (State.pointsOfInterest != null)
		{
			foreach (MapNote item in State.pointsOfInterest)
			{
				if (item != null)
				{
					item.Dispose();
				}
			}
			State.pointsOfInterest.Clear();
		}
		DirtyPlayerState();
		TeamUpdate();
		Interface.CallHook("OnMapMarkersCleared", this);
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(8uL)]
	public void Server_ClearPointsOfInterest(RPCMessage msg)
	{
		if (State.pointsOfInterest != null)
		{
			foreach (MapNote item in State.pointsOfInterest)
			{
				if (item != null)
				{
					item.Dispose();
				}
			}
			State.pointsOfInterest.Clear();
		}
		DirtyPlayerState();
		TeamUpdate();
	}

	public void SendMarkersToClient()
	{
		MapNoteList val = Pool.Get<MapNoteList>();
		try
		{
			val.notes = Pool.Get<List<MapNote>>();
			if (ServerCurrentDeathNote != null)
			{
				val.notes.Add(ServerCurrentDeathNote);
			}
			if (State.pointsOfInterest != null)
			{
				val.notes.AddRange(State.pointsOfInterest);
			}
			Interface.CallHook("OnPlayerMarkersSend", this, val);
			ClientRPC(RpcTarget.Player("Client_ReceiveMarkers", this), val);
			val.notes.Clear();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool HasAttemptedMission(uint missionID)
	{
		for (int i = 0; i < acceptedMissions.Count; i++)
		{
			if (acceptedMissions[i].missionID == missionID)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyMissionActive()
	{
		for (int i = 0; i < acceptedMissions.Count; i++)
		{
			if (acceptedMissions[i].IsActive())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCompletedMission(uint missionID)
	{
		for (int i = 0; i < acceptedMissions.Count; i++)
		{
			BaseMission.MissionInstance missionInstance = acceptedMissions[i];
			if (missionInstance.missionID == missionID && missionInstance.status == BaseMission.MissionStatus.Completed)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasFailedMission(uint missionID)
	{
		for (int i = 0; i < acceptedMissions.Count; i++)
		{
			BaseMission.MissionInstance missionInstance = acceptedMissions[i];
			if (missionInstance.missionID == missionID && missionInstance.status == BaseMission.MissionStatus.Failed)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanAcceptMission(BaseMission mission)
	{
		using (TimeWarning.New("BasePlayer-Mission.CanAcceptMission"))
		{
			if (HasActiveMission())
			{
				return false;
			}
			if (mission.prerequisiteMissions != null && mission.prerequisiteMissions.Length != 0)
			{
				BaseMission.MissionDependancy[] prerequisiteMissions = mission.prerequisiteMissions;
				foreach (BaseMission.MissionDependancy missionDependancy in prerequisiteMissions)
				{
					bool flag = false;
					for (int j = 0; j < acceptedMissions.Count; j++)
					{
						BaseMission.MissionInstance missionInstance = acceptedMissions[j];
						if (missionInstance.missionID == missionDependancy.missionID && missionInstance.status == missionDependancy.desiredStatus)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return false;
					}
				}
			}
			uint id = mission.id;
			if (mission.isRepeatable)
			{
				bool num = HasCompletedMission(id);
				bool flag2 = HasFailedMission(id);
				if (num && mission.repeatDelaySecondsSuccess <= -1)
				{
					return false;
				}
				if (flag2 && mission.repeatDelaySecondsFailed <= -1)
				{
					return false;
				}
				for (int k = 0; k < acceptedMissions.Count; k++)
				{
					BaseMission.MissionInstance missionInstance2 = acceptedMissions[k];
					if (missionInstance2.missionID == id && missionInstance2.endTimeUtcSeconds != long.MinValue)
					{
						float num2 = 0f;
						if (missionInstance2.status == BaseMission.MissionStatus.Completed)
						{
							num2 = mission.repeatDelaySecondsSuccess;
						}
						else if (missionInstance2.status == BaseMission.MissionStatus.Failed)
						{
							num2 = mission.repeatDelaySecondsFailed;
						}
						if ((float)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - missionInstance2.endTimeUtcSeconds) * Time.missiontimerscale < num2)
						{
							return false;
						}
					}
				}
			}
			else if (HasCompletedMission(id))
			{
				return false;
			}
			return true;
		}
	}

	public bool Server_CanAcceptMission(IMissionProvider provider, uint missionId)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		BaseMission fromID = MissionManifest.GetFromID(missionId);
		if (fromID == null)
		{
			Debug.LogError((object)$"Mission ID {missionId} not found in manifest");
			return false;
		}
		return Server_CanAcceptMission(provider.ProviderID(), fromID);
	}

	public bool Server_CanAcceptMission(IMissionProvider provider, BaseMission mission)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Server_CanAcceptMission(provider.ProviderID(), mission);
	}

	public bool Server_CanAcceptMission(NetworkableId providerNetId, BaseMission mission)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		mission.Server_UpdateMissionValidState(providerNetId, out var isValid);
		if (isValid)
		{
			return CanAcceptMission(mission);
		}
		return false;
	}

	private void PrepareMissionsForTutorial()
	{
		PooledList<int> val = Pool.Get<PooledList<int>>();
		try
		{
			if (acceptedMissions.Count > 0)
			{
				for (int num = acceptedMissions.Count - 1; num >= 0; num--)
				{
					BaseMission.MissionInstance missionInstance = acceptedMissions[num];
					if (missionInstance != null)
					{
						if (missionInstance.IsActive())
						{
							missionInstance.GetMission().MissionFailed(missionInstance, this, BaseMission.MissionFailReason.ResetPlayerState, saveImmediately: false);
						}
						if (missionInstance.GetMission() is TutorialMission)
						{
							((List<int>)(object)val).Add(num);
						}
					}
				}
			}
			for (int i = 0; i < ((List<int>)(object)val).Count; i++)
			{
				int num2 = ((List<int>)(object)val)[i];
				BaseMission.MissionInstance missionInstance2 = acceptedMissions[num2];
				Pool.Free<BaseMission.MissionInstance>(ref missionInstance2);
				acceptedMissions.RemoveAt(num2);
			}
			SetActiveMissionIndex(-1);
			MissionsDirty(saveImmediately: true);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void AbandonActiveMission()
	{
		if (TryGetActiveMissionInstance(out var instance))
		{
			instance.GetMission().MissionFailed(instance, this, BaseMission.MissionFailReason.Abandon);
		}
	}

	public void ServerThinkMissions(float delta)
	{
		if (timeSinceServerMissionThink < 1f)
		{
			timeSinceServerMissionThink += delta;
			return;
		}
		try
		{
			for (int i = 0; i < acceptedMissions.Count; i++)
			{
				BaseMission.MissionInstance missionInstance = acceptedMissions[i];
				try
				{
					missionInstance.ServerThink(this, timeSinceServerMissionThink);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}
		finally
		{
		}
		timeSinceServerMissionThink = 0f;
	}

	private static void ServerThinkMissionsParallel(in PlayerServerStates.ReadOnly playerStates, float delta)
	{
		using (TimeWarning.New("ServerThinkMissionsParallel"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			for (int i = 0; i < objects.Length; i++)
			{
				objects[i].ServerThinkMissions(delta);
			}
		}
	}

	public void MissionsDirty(bool saveImmediately = false)
	{
		if (BaseMission.missionsenabled)
		{
			_missionsDirty = true;
			if (saveImmediately)
			{
				SaveMissionsIfDirty();
			}
		}
	}

	public void SaveMissionsIfDirty()
	{
		if (_missionsDirty && BaseMission.missionsenabled)
		{
			UpdatePlayerStateMissionsData();
			SendNetworkUpdate();
			_missionsDirty = false;
		}
	}

	public void ProcessMissionEvent(BaseMission.MissionEventType type, uint identifier, float amount)
	{
		ProcessMissionEvent(type, new BaseMission.MissionEventPayload
		{
			UintIdentifier = identifier
		}, amount);
	}

	public void ProcessMissionEvent(BaseMission.MissionEventType type, uint identifier, float amount, Vector3 worldPos)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		ProcessMissionEvent(type, new BaseMission.MissionEventPayload
		{
			UintIdentifier = identifier,
			WorldPosition = worldPos
		}, amount);
	}

	public void ProcessMissionEvent(BaseMission.MissionEventType type, int identifier, float amount)
	{
		ProcessMissionEvent(type, new BaseMission.MissionEventPayload
		{
			IntIdentifier = identifier
		}, amount);
	}

	public void ProcessMissionEvent(BaseMission.MissionEventType type, NetworkableId identifier, float amount)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		ProcessMissionEvent(type, new BaseMission.MissionEventPayload
		{
			NetworkIdentifier = identifier
		}, amount);
	}

	public void ProcessMissionEvent(BaseMission.MissionEventType type, string identifier, float amount)
	{
		ProcessMissionEvent(type, new BaseMission.MissionEventPayload
		{
			StringIdentifier = identifier
		}, amount);
	}

	public void ProcessMissionEvent(BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		if (BaseMission.missionsenabled && acceptedMissions != null)
		{
			for (int i = 0; i < acceptedMissions.Count; i++)
			{
				acceptedMissions[i].ProcessMissionEvent(this, type, payload, amount);
			}
		}
	}

	public void RegisterFollowupMission(BaseMission targetMission, IMissionProvider provider)
	{
		followupMission = targetMission;
		followupMissionProvider = provider;
		if (followupMission != null && followupMissionProvider != null)
		{
			Invoke(actionAssignFollowUpMission, 1.5f);
		}
	}

	private void AssignFollowUpMission()
	{
		if (followupMission != null && followupMissionProvider != null)
		{
			BaseMission.AssignMission(this, followupMissionProvider, followupMission);
		}
		followupMission = null;
		followupMissionProvider = null;
	}

	private void UpdatePlayerStateMissionsData()
	{
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		Missions missions = State.missions;
		if (missions != null)
		{
			missions.Dispose();
		}
		State.missions = Pool.Get<Missions>();
		State.missions.missions = Pool.Get<List<MissionInstance>>();
		int num = GetActiveMissionIndex();
		for (int i = 0; i < acceptedMissions.Count; i++)
		{
			BaseMission.MissionInstance missionInstance = acceptedMissions[i];
			BaseMission mission = null;
			bool flag = missionInstance != null && MissionManifest.TryGetFromID(missionInstance.missionID, out mission);
			bool flag2 = missionInstance != null && missionInstance.status != BaseMission.MissionStatus.Undefined && missionInstance.status != BaseMission.MissionStatus.Pending;
			if (missionInstance == null || !flag || !flag2)
			{
				if (missionInstance == null)
				{
					Debug.LogError((object)$"Null mission instance at index {i} on player {((Object)this).name}", (Object)(object)this);
				}
				else
				{
					if (!flag)
					{
						Debug.LogError((object)$"Failed to find a mission for instance ID {missionInstance.missionID} at index {i} on player {((Object)this).name}", (Object)(object)this);
					}
					if (!flag2)
					{
						Debug.LogError((object)string.Format("Mission for instance ID {0} at index {1} on player {2} has invalid status: {3}", new object[4]
						{
							missionInstance.missionID,
							i,
							((Object)this).name,
							missionInstance.status
						}), (Object)(object)this);
					}
				}
				if (num != -1 && i == num)
				{
					num = -1;
				}
				continue;
			}
			MissionInstance val = Pool.Get<MissionInstance>();
			val.missionID = missionInstance.missionID;
			val.missionStatus = (uint)missionInstance.status;
			MissionInstanceData val2 = Pool.Get<MissionInstanceData>();
			val2.providerID = missionInstance.providerID;
			val2.startTimeUtcSeconds = missionInstance.startTimeUtcSeconds;
			val2.endTimeUtcSeconds = missionInstance.endTimeUtcSeconds;
			val2.hasDispensedRewards = missionInstance.hasDispensedRewards;
			val2.missionPoints = Pool.Get<List<MissionPoint>>();
			foreach (KeyValuePair<string, Vector3> missionPoint in missionInstance.missionPoints)
			{
				MissionPoint val3 = Pool.Get<MissionPoint>();
				val3.identifier = missionPoint.Key;
				val3.location = missionPoint.Value;
				val2.missionPoints.Add(val3);
			}
			val2.objectiveStatuses = Pool.Get<List<ObjectiveStatus>>();
			int count = missionInstance.objectiveStatuses.Count;
			int num2 = mission.objectives.Length;
			if (count != num2)
			{
				Debug.LogError((object)$"Mission instance for mission {((Object)mission).name} contains data for {count} objectives but mission has {num2} objectives", (Object)(object)mission);
			}
			for (int j = 0; j < count; j++)
			{
				BaseMission.MissionInstance.ObjectiveStatus objectiveStatus = missionInstance.objectiveStatuses[j];
				ObjectiveStatus val4 = Pool.Get<ObjectiveStatus>();
				val4.softCompleted = objectiveStatus.softCompleted;
				val4.blockReset = objectiveStatus.blockReset;
				val4.completed = objectiveStatus.completed;
				val4.failed = objectiveStatus.failed;
				val4.started = objectiveStatus.started;
				val4.progressCurrent = objectiveStatus.progressCurrent;
				val4.progressTarget = objectiveStatus.progressTarget;
				val4.worldLocation = objectiveStatus.worldLocation;
				val2.objectiveStatuses.Add(val4);
			}
			val2.missionEntities = Pool.Get<List<MissionEntity>>();
			foreach (KeyValuePair<string, MissionEntity> spawnedMissionEntity in missionInstance.spawnedMissionEntities)
			{
				BaseEntity baseEntity = (((Object)(object)spawnedMissionEntity.Value != (Object)null) ? spawnedMissionEntity.Value.GetEntity() : null);
				if (baseEntity.IsValid())
				{
					MissionEntity val5 = Pool.Get<MissionEntity>();
					val5.identifier = spawnedMissionEntity.Key;
					val5.entityID = baseEntity.net.ID;
					val2.missionEntities.Add(val5);
				}
			}
			val2.persistentMissionEntities = Pool.Get<List<PersistentMissionEntityData>>();
			for (int k = 0; k < missionInstance.persistentMissionEntities.Count; k++)
			{
				BaseEntity baseEntity2 = missionInstance.persistentMissionEntities[k];
				if (baseEntity2.IsValid())
				{
					PersistentMissionEntityData val6 = Pool.Get<PersistentMissionEntityData>();
					val6.entityID = baseEntity2.net.ID;
					val2.persistentMissionEntities.Add(val6);
				}
			}
			val.instanceData = val2;
			State.missions.missions.Add(val);
		}
		State.missions.activeMission = num;
		DirtyPlayerState();
	}

	public void WipeMissions(bool saveImmediately)
	{
		for (int num = acceptedMissions.Count - 1; num >= 0; num--)
		{
			BaseMission.MissionInstance missionInstance = acceptedMissions[num];
			if (missionInstance != null)
			{
				missionInstance.GetMission().MissionFailed(missionInstance, this, BaseMission.MissionFailReason.ResetPlayerState, saveImmediately: false);
				Pool.Free<BaseMission.MissionInstance>(ref missionInstance);
			}
		}
		acceptedMissions.Clear();
		SetActiveMissionIndex(-1);
		MissionsDirty(saveImmediately);
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(1uL)]
	private void Server_RequestValidMissionsUpdate(RPCMessage _)
	{
		if (IsNpc)
		{
			Debug.LogError((object)(((Object)this).name + " is a NPC, cannot proceed"), (Object)(object)this);
		}
		else
		{
			BaseMission.PlayerRequestedValidStatesUpdate(this);
		}
	}

	public void Server_SendValidMissionStates()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		MissionAcceptStatesList val = Pool.Get<MissionAcceptStatesList>();
		try
		{
			val.missionAcceptStates = Pool.Get<List<MissionAcceptState>>();
			foreach (KeyValuePair<BaseMission.MissionIdentifierData, BaseMission.MissionValidStateData> server_missionInstanceValidState in BaseMission.server_missionInstanceValidStates)
			{
				MissionAcceptState val2 = Pool.Get<MissionAcceptState>();
				val2.providerNetId = server_missionInstanceValidState.Key.missionProviderNetId;
				val2.missionID = server_missionInstanceValidState.Key.mission.id;
				val2.canAccept = server_missionInstanceValidState.Value.isValid;
				val.missionAcceptStates.Add(val2);
			}
			ClientRPC(RpcTarget.Player("Client_ReceiveValidMissionStates", this), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void Server_SendCanAcceptMissionsFromProvider(IMissionProvider missionProvider)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (IsNpc)
		{
			Debug.LogError((object)(((Object)this).name + " is a NPC, cannot proceed"), (Object)(object)this);
			return;
		}
		MissionAcceptStatesList val = Pool.Get<MissionAcceptStatesList>();
		try
		{
			val.missionAcceptStates = Pool.Get<List<MissionAcceptState>>();
			BufferList<BaseMission> allMissions = missionProvider.GetAllMissions();
			int count = allMissions.Count;
			if (count > 0)
			{
				for (int i = 0; i < count; i++)
				{
					BaseMission baseMission = allMissions[i];
					MissionAcceptState val2 = Pool.Get<MissionAcceptState>();
					val2.providerNetId = missionProvider.ProviderID();
					val2.missionID = baseMission.id;
					val2.canAccept = Server_CanAcceptMission(missionProvider, baseMission);
					val.missionAcceptStates.Add(val2);
				}
				ClientRPC(RpcTarget.Player("Client_ReceiveMissionStatesForProvider", this), val);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void Server_SendMissionFailed(uint missionID, BaseMission.MissionFailReason reason)
	{
		if (IsNpc)
		{
			Debug.LogError((object)(((Object)this).name + " is a NPC, cannot proceed"), (Object)(object)this);
		}
		else
		{
			ClientRPC(RpcTarget.Player("Client_ReceiveMissionFailed", this), missionID, (int)reason);
		}
	}

	public void SetActiveMissionIndex(int index)
	{
		_activeMissionIndex = index;
		if (IsInTutorial && (Object)(object)GetCurrentTutorialIsland() != (Object)null)
		{
			GetCurrentTutorialIsland().OnPlayerStartedMission(this);
		}
	}

	public int GetActiveMissionIndex()
	{
		return _activeMissionIndex;
	}

	public bool HasActiveMission()
	{
		bool flag = GetActiveMissionIndex() != -1;
		bool flag2 = false;
		BaseMission.MissionInstance missionInstance = null;
		for (int i = 0; i < acceptedMissions.Count; i++)
		{
			BaseMission.MissionInstance missionInstance2 = acceptedMissions[i];
			if (missionInstance2.IsActive())
			{
				missionInstance = missionInstance2;
				flag2 = true;
				break;
			}
		}
		if (flag != flag2)
		{
			string arg = ((missionInstance == null) ? "null" : $"ID: {missionInstance.missionID.ToString()}, mission: {((Object)missionInstance.GetMission()).name}, status: {missionInstance.status}");
			Debug.LogWarning((object)$"Discrepancy between active mission index {GetActiveMissionIndex()} and active mission instance {arg}");
		}
		return flag || flag2;
	}

	public BaseMission.MissionInstance GetActiveMissionInstance()
	{
		int activeMissionIndex = GetActiveMissionIndex();
		if (activeMissionIndex >= 0 && activeMissionIndex < acceptedMissions.Count)
		{
			return acceptedMissions[activeMissionIndex];
		}
		return null;
	}

	public bool TryGetActiveMissionInstance(out BaseMission.MissionInstance instance)
	{
		instance = GetActiveMissionInstance();
		return instance != null;
	}

	private void LoadMissions(Missions loadedMissions)
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		if (acceptedMissions.Count > 0)
		{
			for (int num = acceptedMissions.Count - 1; num >= 0; num--)
			{
				BaseMission.MissionInstance missionInstance = acceptedMissions[num];
				if (missionInstance != null)
				{
					Pool.Free<BaseMission.MissionInstance>(ref missionInstance);
				}
			}
		}
		acceptedMissions.Clear();
		bool flag = true;
		if (loadedMissions != null && loadedMissions.missions != null && loadedMissions.missions.Count > 0)
		{
			MissionEntity missionEntity2 = default(MissionEntity);
			for (int i = 0; i < loadedMissions.missions.Count; i++)
			{
				MissionInstance val = loadedMissions.missions[i];
				if (!MissionManifest.TryGetFromID(val.missionID, out var mission))
				{
					flag = false;
					continue;
				}
				BaseMission.MissionInstance missionInstance2 = Pool.Get<BaseMission.MissionInstance>();
				missionInstance2.missionID = val.missionID;
				missionInstance2.status = (BaseMission.MissionStatus)val.missionStatus;
				MissionInstanceData instanceData = val.instanceData;
				if (instanceData != null)
				{
					missionInstance2.providerID = instanceData.providerID;
					missionInstance2.startTimeUtcSeconds = instanceData.startTimeUtcSeconds;
					missionInstance2.endTimeUtcSeconds = instanceData.endTimeUtcSeconds;
					missionInstance2.hasDispensedRewards = instanceData.hasDispensedRewards;
					if (base.isServer && instanceData.missionPoints != null)
					{
						for (int j = 0; j < instanceData.missionPoints.Count; j++)
						{
							MissionPoint obj = instanceData.missionPoints[j];
							string identifier = obj.identifier;
							Vector3 location = obj.location;
							missionInstance2.missionPoints.Add(identifier, location);
							if (missionInstance2.IsActive() && mission.TryGetPositionGenerator(identifier, out var positionGenerator) && positionGenerator.positionsAreExclusive)
							{
								BaseMission.AddPositionBlocker(missionInstance2, location);
							}
						}
					}
					int count = instanceData.objectiveStatuses.Count;
					int num2 = mission.objectives.Length;
					if (base.isServer && count != num2)
					{
						Debug.LogError((object)string.Format("Loaded mission instance data for mission {0} contains data for {1} objectives but mission has {2} objectives. Loaded mission points: {3}", new object[4]
						{
							((Object)mission).name,
							count,
							num2,
							GenerateMissionPointsDataDebug(instanceData)
						}), (Object)(object)mission);
					}
					for (int k = 0; k < count; k++)
					{
						ObjectiveStatus val2 = instanceData.objectiveStatuses[k];
						BaseMission.MissionInstance.ObjectiveStatus objectiveStatus = Pool.Get<BaseMission.MissionInstance.ObjectiveStatus>();
						objectiveStatus.started = val2.started;
						objectiveStatus.softCompleted = val2.softCompleted;
						objectiveStatus.blockReset = val2.blockReset;
						objectiveStatus.completed = val2.completed;
						objectiveStatus.failed = val2.failed;
						objectiveStatus.progressTarget = val2.progressTarget;
						objectiveStatus.progressCurrent = val2.progressCurrent;
						objectiveStatus.worldLocation = val2.worldLocation;
						missionInstance2.objectiveStatuses.Add(objectiveStatus);
					}
					if (base.isServer)
					{
						if (instanceData.missionEntities != null)
						{
							missionInstance2.spawnedMissionEntities.Clear();
							BaseMission mission2 = missionInstance2.GetMission();
							for (int l = 0; l < instanceData.missionEntities.Count; l++)
							{
								MissionEntity val3 = instanceData.missionEntities[l];
								MissionEntity missionEntity = null;
								if (BaseNetworkable.serverEntities.TryGetEntity(val3.entityID, out var entity))
								{
									missionEntity = (((Component)entity).gameObject.TryGetComponent<MissionEntity>(ref missionEntity2) ? missionEntity2 : ((Component)entity).gameObject.AddComponent<MissionEntity>());
									BaseMission.MissionEntityEntry missionEntityEntry = ((mission2 != null) ? List.FindWith<BaseMission.MissionEntityEntry, string>((IReadOnlyCollection<BaseMission.MissionEntityEntry>)mission2.spawnMissionEntityDefinitions, (Func<BaseMission.MissionEntityEntry, string>)((BaseMission.MissionEntityEntry ed) => ed.identifier), val3.identifier, (IEqualityComparer<string>)null) : null);
									missionEntity.Setup(this, missionInstance2, val3.identifier, missionEntityEntry?.cleanupOnMissionSuccess ?? true, missionEntityEntry?.cleanupOnMissionFailed ?? true);
								}
								missionInstance2.spawnedMissionEntities.Add(val3.identifier, missionEntity);
							}
						}
						if (instanceData.persistentMissionEntities != null)
						{
							missionInstance2.persistentMissionEntities.Clear();
							for (int num3 = 0; num3 < instanceData.persistentMissionEntities.Count; num3++)
							{
								PersistentMissionEntityData val4 = instanceData.persistentMissionEntities[num3];
								if (BaseNetworkable.serverEntities.TryGetEntity(val4.entityID, out var entity2))
								{
									missionInstance2.persistentMissionEntities.TryAdd(entity2);
								}
							}
						}
					}
				}
				acceptedMissions.Add(missionInstance2);
			}
		}
		else
		{
			flag = false;
		}
		SetActiveMissionIndex(flag ? loadedMissions.activeMission : (-1));
		if (base.isServer && TryGetActiveMissionInstance(out var instance))
		{
			instance.PostServerLoad(this);
		}
	}

	public bool HasSpaceForMissionRewards(BaseMission.MissionInstance missionInstance, bool showToastOnFailure = false)
	{
		if (!missionInstance.TryGetTotalRequiredRewardItemSlots(out var requiredSlots))
		{
			if (showToastOnFailure)
			{
				ShowToast(GameTip.Styles.Red_Normal, FailedToCheckRewardsSpace, false);
			}
			return false;
		}
		if (!inventory.HasEmptySlots(requiredSlots))
		{
			if (showToastOnFailure)
			{
				ShowToast(GameTip.Styles.Red_Normal, NoSpaceInInventoryPhrase, false);
			}
			return false;
		}
		return true;
	}

	private string GenerateMissionPointsDataDebug(MissionInstanceData missionInstanceData)
	{
		string text = string.Empty;
		if (missionInstanceData.missionPoints != null)
		{
			for (int i = 0; i < missionInstanceData.missionPoints.Count; i++)
			{
				text = text + "\n" + missionInstanceData.missionPoints[i].identifier + ": " + ((object)Unsafe.As<Vector3, Vector3>(ref missionInstanceData.missionPoints[i].location)/*cast due to constrained. prefix*/).ToString();
			}
		}
		return text;
	}

	private void UpdateModelState(ModelState ms)
	{
		if (!IsDead() && !IsSpectating() && !isInvisible)
		{
			ms.sleeping = IsSleeping();
			ms.mounted = isMounted;
			ms.ragdolling = IsRagdolling();
			ms.relaxed = IsRelaxed();
			ms.crawling = IsCrawling();
			ms.loading = IsLoadingAfterTransfer();
		}
	}

	public void SendModelState(bool force = false)
	{
		if (force || lastModelState == null || HasModelStateChanged())
		{
			if (lastModelState == null)
			{
				lastModelState = modelState.Copy();
			}
			else
			{
				modelState.CopyTo(lastModelState);
			}
			if (!base.limitNetworking && Interface.CallHook("OnSendModelState", this) == null)
			{
				ClientRPC(RpcTarget.NetworkGroup("OnModelState"), modelState);
			}
		}
	}

	private bool HasModelStateChanged()
	{
		if (lastModelState == null)
		{
			return true;
		}
		return !ModelState.Equal(lastModelState, modelState);
	}

	public BaseMountable GetMounted()
	{
		return mounted.Get(base.isServer) as BaseMountable;
	}

	public BaseVehicle GetMountedVehicle()
	{
		BaseMountable baseMountable = GetMounted();
		if (!baseMountable.IsValid())
		{
			return null;
		}
		return baseMountable.VehicleParent();
	}

	public void MarkSwapSeat()
	{
		nextSeatSwapTime = Time.time + 0.75f;
	}

	public bool SwapSeatCooldown()
	{
		return Time.time < nextSeatSwapTime;
	}

	public bool CanMountMountablesNow()
	{
		if (!IsDead())
		{
			return !IsWounded();
		}
		return false;
	}

	public void SetMounted(BaseMountable mount)
	{
		mounted.Set(mount);
		RefreshColliderSize(forced: true);
		if (ActivePlayerInd != -1)
		{
			PlayerStates.IsMounted[ActivePlayerInd] = mount.IsValid();
			PlayerStates.Mountables[ActivePlayerInd] = mount;
		}
	}

	public void EnsureDismounted()
	{
		if (isMounted)
		{
			GetMounted().DismountPlayer(this);
		}
	}

	public virtual void DismountObject()
	{
		SetMounted(null);
		SendNetworkUpdate();
		PauseSpeedHackDetection(5f);
		PauseTickDistanceDetection(5f);
	}

	public void HandleMountedOnLoad()
	{
		if (!mounted.IsValid(base.isServer))
		{
			return;
		}
		BaseMountable baseMountable = mounted.Get(base.isServer) as BaseMountable;
		if ((Object)(object)baseMountable != (Object)null)
		{
			baseMountable.MountPlayer(this);
			if (!AllowSleeperMounting(baseMountable))
			{
				baseMountable.DismountPlayer(this);
			}
		}
		else
		{
			SetMounted(null);
		}
	}

	public bool AllowSleeperMounting(BaseMountable mountable)
	{
		if (mountable.allowSleeperMounting)
		{
			return true;
		}
		if (!IsLoadingAfterTransfer())
		{
			return IsTransferProtected();
		}
		return true;
	}

	public PlayerSecondaryData SaveSecondaryData()
	{
		PlayerSecondaryData val = Pool.Get<PlayerSecondaryData>();
		val.userId = userID;
		PlayerState val2 = State.Copy();
		if (val2.pointsOfInterest != null)
		{
			Pool.Free<MapNote>(ref val2.pointsOfInterest, true);
		}
		if (val2.pings != null)
		{
			Pool.Free<MapNote>(ref val2.pings, true);
		}
		MapNote deathMarker = val2.deathMarker;
		if (deathMarker != null)
		{
			deathMarker.Dispose();
		}
		val2.deathMarker = null;
		Missions missions = val2.missions;
		if (missions != null)
		{
			missions.Dispose();
		}
		val2.missions = null;
		val2.numberOfTimesReported = 0;
		val.playerState = val2;
		if (currentTeam != 0L)
		{
			RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindTeam(currentTeam);
			if (playerTeam != null)
			{
				val.teamId = playerTeam.teamID;
				val.isTeamLeader = playerTeam.teamLeader == (ulong)userID;
			}
		}
		val.relationships = Pool.Get<List<RelationshipData>>();
		foreach (RelationshipManager.PlayerRelationshipInfo value in RelationshipManager.ServerInstance.GetRelationships(userID).relations.Values)
		{
			RelationshipData val3 = Pool.Get<RelationshipData>();
			val3.info = value.ToProto();
			val3.mugshotData = GetPoolableMugshotData(value);
			val.relationships.Add(val3);
		}
		return val;
		ArraySegment<byte> GetPoolableMugshotData(RelationshipManager.PlayerRelationshipInfo relationshipInfo)
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			if (relationshipInfo.mugshotCrc == 0)
			{
				return default(ArraySegment<byte>);
			}
			try
			{
				uint steamIdHash = RelationshipManager.GetSteamIdHash(userID, relationshipInfo.player);
				byte[] array = FileStorage.server.Get(relationshipInfo.mugshotCrc, FileStorage.Type.jpg, RelationshipManager.ServerInstance.net.ID, steamIdHash);
				if (array == null)
				{
					return default(ArraySegment<byte>);
				}
				byte[] array2 = Shared.ArrayPool.Rent(array.Length);
				new Span<byte>(array).CopyTo(array2);
				return new ArraySegment<byte>(array2, 0, array.Length);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				return default(ArraySegment<byte>);
			}
		}
	}

	public void LoadSecondaryData(PlayerSecondaryData data)
	{
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		if (data == null)
		{
			return;
		}
		if (data.userId != (ulong)userID)
		{
			Debug.LogError((object)$"Attempted to load secondary data with an incorrect userID! Expected {data.userId} but player has {userID.Get()}, not loading it.");
			return;
		}
		if (data.playerState != null)
		{
			State.unHostileTimestamp = data.playerState.unHostileTimestamp;
			DirtyPlayerState();
		}
		if (data.relationships == null)
		{
			return;
		}
		RelationshipManager.PlayerRelationships relationships = RelationshipManager.ServerInstance.GetRelationships(userID);
		relationships.ClearRelations();
		foreach (RelationshipData relationship in data.relationships)
		{
			if (relationship.mugshotData.Count > 0)
			{
				try
				{
					byte[] array = new byte[relationship.mugshotData.Count];
					relationship.mugshotData.AsSpan().CopyTo(array);
					uint steamIdHash = RelationshipManager.GetSteamIdHash(userID, relationship.info.playerID);
					uint num = FileStorage.server.Store(array, FileStorage.Type.jpg, RelationshipManager.ServerInstance.net.ID, steamIdHash);
					if (num != relationship.info.mugshotCrc)
					{
						Debug.LogWarning((object)$"Mugshot data for {userID.Get()}->{relationship.info.playerID} had a CRC mismatch, updating it");
						relationship.info.mugshotCrc = num;
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			relationships.relations.Add(relationship.info.playerID, RelationshipManager.PlayerRelationshipInfo.FromProto(relationship.info));
		}
		RelationshipManager.ServerInstance.MarkRelationshipsDirtyFor(this);
	}

	public override void DisableTransferProtection()
	{
		BaseVehicle vehicleParent = GetVehicleParent();
		if ((Object)(object)vehicleParent != (Object)null && vehicleParent.IsTransferProtected())
		{
			vehicleParent.DisableTransferProtection();
		}
		BaseMountable baseMountable = GetMounted();
		if ((Object)(object)baseMountable != (Object)null && baseMountable.IsTransferProtected())
		{
			baseMountable.DisableTransferProtection();
		}
		base.DisableTransferProtection();
	}

	public void KickAfterServerTransfer()
	{
		if (IsConnected)
		{
			Kick("Redirecting to another zone...");
		}
		Kill();
	}

	public void Server_UpdatePaintballColor(int newColor)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		if (PaintballColorLookup.instance == null)
		{
			Debug.LogError((object)"Failed to retrieve PaintballColorLookup instance");
			return;
		}
		server_paintballColor = Mathf.Clamp(newColor, 0, PaintballColorLookup.instance.GetColorsCount() - 1);
		ItemDefinition paintballGunItemDefinition = PaintballColorLookup.instance.paintballGunItemDefinition;
		ItemDefinition overallsItemDefinition = PaintballColorLookup.instance.overallsItemDefinition;
		if ((Object)(object)inventory != (Object)null)
		{
			if (TryGetActiveItem(out var item) && (Object)(object)item.info == (Object)(object)paintballGunItemDefinition)
			{
				if (item.instanceData == null)
				{
					item.instanceData = new InstanceData();
					item.instanceData.ShouldPool = false;
				}
				item.instanceData.dataInt = server_paintballColor;
				item.MarkDirty();
			}
			if (inventory.containerWear != null)
			{
				PooledList<Item> val = Pool.Get<PooledList<Item>>();
				try
				{
					inventory.containerWear.FindItemsByItemID((List<Item>)(object)val, overallsItemDefinition.itemid);
					for (int i = 0; i < ((List<Item>)(object)val).Count; i++)
					{
						Item item2 = ((List<Item>)(object)val)[i];
						if (item2.instanceData == null)
						{
							item2.instanceData = new InstanceData();
							item2.instanceData.ShouldPool = false;
						}
						item2.instanceData.dataInt = server_paintballColor;
						item2.MarkDirty();
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
		Server_SendPaintballColorUpdate();
	}

	private void Server_SendPaintballColorUpdate()
	{
		ClientRPC(RpcTarget.Player("Client_ReceivePaintballColorUpdate", this), server_paintballColor);
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(5uL)]
	private void RequestParachuteDeploy(RPCMessage msg)
	{
		RequestParachuteDeploy();
	}

	public void RequestParachuteDeploy()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (isMounted || !CheckParachuteClearance())
		{
			return;
		}
		Item slot = inventory.containerWear.GetSlot(7);
		ItemModParachute itemModParachute = default(ItemModParachute);
		if (slot == null || !(slot.conditionNormalized > 0f) || slot.isBroken || !((Component)slot.info).TryGetComponent<ItemModParachute>(ref itemModParachute))
		{
			return;
		}
		Parachute parachute = GameManager.server.CreateEntity(itemModParachute.ParachuteVehiclePrefab.resourcePath, ((Component)this).transform.position, eyes.rotation) as Parachute;
		if ((Object)(object)parachute != (Object)null)
		{
			parachute.skinID = slot.skin;
			parachute.Spawn();
			parachute.SetHealth(parachute.MaxHealth() * slot.conditionNormalized);
			parachute.AttemptMount(this);
			if (isMounted)
			{
				slot.Remove();
				ItemManager.DoRemoves();
				SendNetworkUpdate();
			}
			else
			{
				parachute.Kill();
			}
		}
	}

	public bool CheckParachuteClearance()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		if (!WaterLevel.Test(position - Vector3.up * 5f, waves: false, volumes: true, this) && !GamePhysics.Trace(new Ray(position, -Vector3.up), 1f, out var _, 6f, 1218674945, (QueryTriggerInteraction)0, this))
		{
			return !GamePhysics.CheckSphere(position + Vector3.up * 3.5f, 2f, 1218543873, (QueryTriggerInteraction)0);
		}
		return false;
	}

	public bool HasValidParachuteEquipped()
	{
		if ((Object)(object)inventory == (Object)null || inventory.containerWear == null)
		{
			return false;
		}
		Item slot = inventory.containerWear.GetSlot(7);
		ItemModParachute itemModParachute = default(ItemModParachute);
		if (slot != null && slot.conditionNormalized > 0f && !slot.isBroken && ((Component)slot.info).TryGetComponent<ItemModParachute>(ref itemModParachute))
		{
			return true;
		}
		return false;
	}

	public void ClearClientPetLink()
	{
		ClientRPC(RpcTarget.Player("CLIENT_SetPetPrefabID", this), 0u, 0uL);
	}

	public void SendClientPetLink()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)PetEntity == (Object)null && BasePet.ActivePetByOwnerID.TryGetValue(userID, out var value) && (Object)(object)value.Brain != (Object)null)
		{
			value.Brain.SetOwningPlayer(this);
		}
		ClientRPC(RpcTarget.Player("CLIENT_SetPetPrefabID", this), ((Object)(object)PetEntity != (Object)null) ? PetEntity.prefabID : 0u, (NetworkableId)(((Object)(object)PetEntity != (Object)null) ? PetEntity.net.ID : default(NetworkableId)));
		if ((Object)(object)PetEntity != (Object)null)
		{
			SendClientPetStateIndex();
		}
	}

	public void SendClientPetStateIndex()
	{
		BasePet basePet = PetEntity as BasePet;
		if (!((Object)(object)basePet == (Object)null))
		{
			ClientRPC(RpcTarget.Player("CLIENT_SetPetPetLoadedStateIndex", this), basePet.Brain.LoadedDesignIndex());
		}
	}

	[RPC_Server]
	private void IssuePetCommand(RPCMessage msg)
	{
		ParsePetCommand(msg, raycast: false);
	}

	[RPC_Server]
	private void IssuePetCommandRaycast(RPCMessage msg)
	{
		ParsePetCommand(msg, raycast: true);
	}

	private void ParsePetCommand(RPCMessage msg, bool raycast)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (Time.time - lastPetCommandIssuedTime <= 1f)
		{
			return;
		}
		lastPetCommandIssuedTime = Time.time;
		if (!((Object)(object)msg.player == (Object)null) && Pet != null && Pet.IsOwnedBy(msg.player))
		{
			int cmd = msg.read.Int32();
			int param = msg.read.Int32();
			if (raycast)
			{
				Ray value = msg.read.Ray();
				Pet.IssuePetCommand((PetCommandType)cmd, param, value);
			}
			else
			{
				Pet.IssuePetCommand((PetCommandType)cmd, param, null);
			}
		}
	}

	public bool CanPing(bool disregardHeldEntity = false)
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(base.isServer);
		if ((Object)(object)activeGameMode != (Object)null && !activeGameMode.allowPings)
		{
			return false;
		}
		if ((disregardHeldEntity || GetHeldEntity() is Binocular || (isMounted && GetMounted() is ComputerStation computerStation && computerStation.AllowPings()) || (GetHeldEntity() is BaseProjectile baseProjectile && baseProjectile.AllowsPingUsage())) && IsAlive() && !IsWounded())
		{
			return !IsSpectating();
		}
		return false;
	}

	public static PingStyle GetPingStyle(PingType t)
	{
		PingStyle pingStyle = default(PingStyle);
		return t switch
		{
			PingType.Hostile => HostileMarker, 
			PingType.GoTo => GoToMarker, 
			PingType.Dollar => DollarMarker, 
			PingType.Loot => LootMarker, 
			PingType.Node => NodeMarker, 
			PingType.Gun => GunMarker, 
			PingType.Build => BuildMarker, 
			_ => pingStyle, 
		};
	}

	private void ApplyPingStyle(MapNote note, PingType type)
	{
		PingStyle pingStyle = GetPingStyle(type);
		note.colourIndex = pingStyle.ColourIndex;
		note.icon = pingStyle.IconIndex;
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server.InputValidation(new Type[]
	{
		typeof(Vector3),
		typeof(int),
		typeof(bool)
	})]
	[RPC_Server]
	[RPC_Server.FromOwner]
	private void Server_AddPing(RPCMessage msg)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		if (State.pings == null)
		{
			State.pings = new List<MapNote>();
		}
		if (ConVar.Server.maximumPings == 0 || !CanPing())
		{
			return;
		}
		Vector3 val = msg.read.Vector3();
		PingType pingType = (PingType)Mathf.Clamp(msg.read.Int32(), 0, 6);
		bool wasViaWheel = msg.read.Bit();
		PingStyle pingStyle = GetPingStyle(pingType);
		foreach (MapNote ping in State.pings)
		{
			if (ping.icon == pingStyle.IconIndex)
			{
				Vector3 val2 = ping.worldPosition - val;
				if (((Vector3)(ref val2)).sqrMagnitude < 0.75f)
				{
					return;
				}
			}
		}
		if (State.pings.Count >= ConVar.Server.maximumPings)
		{
			State.pings.RemoveAt(0);
		}
		MapNote val3 = Pool.Get<MapNote>();
		val3.worldPosition = val;
		val3.isPing = true;
		val3.timeRemaining = (val3.totalDuration = ConVar.Server.pingDuration);
		ApplyPingStyle(val3, pingType);
		State.pings.Add(val3);
		DirtyPlayerState();
		SendPingsToClient();
		TeamUpdate(fullTeamUpdate: true);
		Facepunch.Rust.Analytics.Azure.OnPlayerPinged(this, pingType, wasViaWheel);
	}

	public void AddPingAtLocation(PingType type, Vector3 location, float time, NetworkableId associatedId)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (State.pings != null)
		{
			PingStyle pingStyle = GetPingStyle(type);
			foreach (MapNote ping in State.pings)
			{
				if (ping.icon == pingStyle.IconIndex && Vector3.Distance(location, ping.worldPosition) < 0.25f)
				{
					return;
				}
			}
		}
		if (State.pings == null)
		{
			State.pings = new List<MapNote>();
		}
		MapNote val = Pool.Get<MapNote>();
		val.worldPosition = location;
		val.isPing = true;
		val.timeRemaining = (val.totalDuration = time);
		val.associatedId = associatedId;
		ApplyPingStyle(val, type);
		State.pings.Add(val);
		DirtyPlayerState();
		SendPingsToClient();
		TeamUpdate(fullTeamUpdate: false);
	}

	public void RemovePingAtLocation(PingType type, Vector3 location, float tolerance, NetworkableId associatedId)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (State.pings == null)
		{
			return;
		}
		PingStyle pingStyle = GetPingStyle(type);
		for (int i = 0; i < State.pings.Count; i++)
		{
			MapNote val = State.pings[i];
			if (val.icon == pingStyle.IconIndex && Vector3.Distance(location, val.worldPosition) < tolerance)
			{
				State.pings.RemoveAt(i);
				DirtyPlayerState();
				SendPingsToClient();
				TeamUpdate(fullTeamUpdate: false);
			}
		}
	}

	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	private void Server_RemovePing(RPCMessage msg)
	{
		if (State.pings == null)
		{
			State.pings = new List<MapNote>();
		}
		int num = msg.read.Int32();
		if (num >= 0 && num < State.pings.Count)
		{
			State.pings.RemoveAt(num);
			DirtyPlayerState();
			SendPingsToClient();
			TeamUpdate(fullTeamUpdate: true);
		}
	}

	public void SendPingsToClient()
	{
		MapNoteList val = Pool.Get<MapNoteList>();
		try
		{
			val.notes = Pool.Get<List<MapNote>>();
			val.notes.AddRange(State.pings);
			Interface.CallHook("OnPlayerPingsSend", this, val);
			ClientRPC(RpcTarget.Player("Client_ReceivePings", this), val);
			val.notes.Clear();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void TickPings()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (TimeSince.op_Implicit(lastTick) < 0.5f)
		{
			return;
		}
		TimeSince val = lastTick;
		lastTick = TimeSince.op_Implicit(0f);
		UpdateResourcePings();
		if (State.pings == null)
		{
			return;
		}
		List<MapNote> list = Pool.Get<List<MapNote>>();
		foreach (MapNote ping in State.pings)
		{
			ping.timeRemaining -= TimeSince.op_Implicit(val);
			if (ping.timeRemaining <= 0f)
			{
				list.Add(ping);
			}
		}
		int count = list.Count;
		foreach (MapNote item in list)
		{
			if (State.pings.Contains(item))
			{
				State.pings.Remove(item);
			}
		}
		Pool.Free<MapNote>(ref list, false);
		if (count > 0)
		{
			DirtyPlayerState();
			SendPingsToClient();
			TeamUpdate(fullTeamUpdate: true);
		}
	}

	public void RegisterPingedEntity(BaseEntity entity, PingType type)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!pingedEntities.Contains((entity.net.ID, type)))
		{
			pingedEntities.Add((entity.net.ID, type));
		}
	}

	public void DeregisterPingedEntitiesOfType(PingType type)
	{
		if (pingedEntities.Count <= 0)
		{
			return;
		}
		for (int num = pingedEntities.Count - 1; num >= 0; num--)
		{
			if (pingedEntities[num].pingType == type)
			{
				pingedEntities.RemoveAt(num);
			}
		}
	}

	public void DeregisterPingedEntity(NetworkableId id, PingType type)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!pingedEntities.Contains((id, type)))
		{
			return;
		}
		pingedEntities.Remove((id, type));
		for (int i = 0; i < State.pings.Count; i++)
		{
			if (State.pings[i].associatedId == id)
			{
				State.pings.RemoveAt(i);
				break;
			}
		}
		DirtyPlayerState();
		SendPingsToClient();
	}

	public void EnableResourcePings(ItemDefinition forItem, PingType pingType)
	{
		if (!tutorialDesiredResource.Contains((forItem, pingType)))
		{
			tutorialDesiredResource.Add((forItem, pingType));
		}
	}

	private void UpdateResourcePings()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		if (State == null || TimeSince.op_Implicit(lastResourcePingUpdate) < 3f || !IsInTutorial)
		{
			return;
		}
		lastResourcePingUpdate = TimeSince.op_Implicit(0f);
		if (State.pings == null)
		{
			State.pings = new List<MapNote>();
		}
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			PooledList<(BaseEntity, PingType)> val2 = Pool.Get<PooledList<(BaseEntity, PingType)>>();
			try
			{
				PooledList<BaseEntity> val3 = Pool.Get<PooledList<BaseEntity>>();
				try
				{
					ResourceDispenser resourceDispenser = default(ResourceDispenser);
					foreach (var item2 in tutorialDesiredResource)
					{
						((List<BaseEntity>)(object)val3).Clear();
						if (net.group.networkables != null)
						{
							Enumerator<Networkable> enumerator2 = net.group.networkables.GetEnumerator();
							try
							{
								while (enumerator2.MoveNext())
								{
									Networkable current2 = enumerator2.Current;
									BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(current2.ID) as BaseEntity;
									if ((Object)(object)baseEntity != (Object)null && Distance(baseEntity) < 128f && baseEntity.isServer)
									{
										if (((Component)baseEntity).TryGetComponent<ResourceDispenser>(ref resourceDispenser) && resourceDispenser.HasItemToDispense(item2.item))
										{
											((List<BaseEntity>)(object)val3).Add(baseEntity);
										}
										else if (baseEntity is CollectibleEntity collectibleEntity && collectibleEntity.HasItem(item2.item))
										{
											((List<BaseEntity>)(object)val3).Add(baseEntity);
										}
										else if (baseEntity is StorageContainer { inventory: not null } storageContainer && storageContainer.inventory.HasItem(item2.item))
										{
											((List<BaseEntity>)(object)val3).Add(baseEntity);
										}
									}
								}
							}
							finally
							{
								((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
							}
						}
						if (((List<BaseEntity>)(object)val3).Count <= 0)
						{
							continue;
						}
						float num = float.MaxValue;
						BaseEntity baseEntity2 = null;
						foreach (BaseEntity item3 in (List<BaseEntity>)(object)val3)
						{
							float num2 = Distance(item3);
							if (num2 < num)
							{
								num = num2;
								baseEntity2 = item3;
							}
						}
						if ((Object)(object)baseEntity2 != (Object)null)
						{
							((List<(BaseEntity, PingType)>)(object)val2).Add((baseEntity2, item2.pingType));
						}
					}
					PooledList<(NetworkableId, PingType)> val4 = Pool.Get<PooledList<(NetworkableId, PingType)>>();
					try
					{
						foreach (var pingedEntity in pingedEntities)
						{
							BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(pingedEntity.id);
							if ((Object)(object)baseNetworkable != (Object)null && !baseNetworkable.IsDestroyed)
							{
								((List<(BaseEntity, PingType)>)(object)val2).Add((baseNetworkable as BaseEntity, pingedEntity.pingType));
							}
							else
							{
								((List<(NetworkableId, PingType)>)(object)val4).Add(pingedEntity);
							}
						}
						foreach (var item4 in (List<(NetworkableId, PingType)>)(object)val4)
						{
							pingedEntities.Remove(item4);
						}
						PooledList<MapNote> val5 = Pool.Get<PooledList<MapNote>>();
						try
						{
							foreach (MapNote ping in State.pings)
							{
								if (ping.associatedId.Value == 0L)
								{
									continue;
								}
								bool flag = false;
								foreach (var item5 in (List<(BaseEntity, PingType)>)(object)val2)
								{
									if (ping.associatedId == item5.Item1.net.ID)
									{
										flag = true;
										break;
									}
								}
								if (!flag)
								{
									BaseNetworkable baseNetworkable2 = BaseNetworkable.serverEntities.Find(ping.associatedId);
									if ((Object)(object)baseNetworkable2 != (Object)null && baseNetworkable2 is IEntityPingSource entityPingSource && entityPingSource.IsPingValid(ping))
									{
										flag = true;
									}
								}
								if (!flag)
								{
									((List<MapNote>)(object)val5).Add(ping);
								}
							}
							bool flag2 = ((List<MapNote>)(object)val5).Count > 0;
							foreach (MapNote item6 in (List<MapNote>)(object)val5)
							{
								if (State.pings.Contains(item6))
								{
									State.pings.Remove(item6);
								}
							}
							foreach (var item7 in (List<(BaseEntity, PingType)>)(object)val2)
							{
								if (HasPingForEntity(item7.Item1))
								{
									continue;
								}
								PingType item = item7.Item2;
								foreach (var pingedEntity2 in pingedEntities)
								{
									if (pingedEntity2.id == item7.Item1.net.ID)
									{
										item = pingedEntity2.pingType;
									}
								}
								State.pings.Add(CreatePingForEntity(item7.Item1, item));
								flag2 = true;
							}
							if (flag2)
							{
								DirtyPlayerState();
								SendPingsToClient();
							}
						}
						finally
						{
							((IDisposable)val5)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
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

	private MapNote CreatePingForEntity(BaseEntity baseEntity, PingType type)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		MapNote val = Pool.Get<MapNote>();
		val.worldPosition = ((Component)baseEntity).transform.position;
		val.isPing = true;
		val.timeRemaining = (val.totalDuration = 30f);
		val.associatedId = baseEntity.net.ID;
		ApplyPingStyle(val, type);
		return val;
	}

	private bool HasPingForEntity(BaseEntity ent)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return HasPingForEntity(ent.net.ID);
	}

	private bool HasPingForEntity(NetworkableId id)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		foreach (MapNote ping in State.pings)
		{
			if (ping.associatedId == id)
			{
				return true;
			}
		}
		return false;
	}

	public void DisableResourcePings(ItemDefinition forItem, PingType type)
	{
		if (tutorialDesiredResource.Contains((forItem, type)))
		{
			tutorialDesiredResource.Remove((forItem, type));
		}
		if (tutorialDesiredResource.Count == 0)
		{
			UpdateResourcePings();
		}
	}

	private void ClearAllPings()
	{
		if (State != null && State.pings != null)
		{
			State.pings.Clear();
		}
		tutorialDesiredResource.Clear();
		pingedEntities.Clear();
	}

	public void DirtyPlayerState()
	{
		_playerStateDirty = true;
	}

	public void SavePlayerState()
	{
		if (_playerStateDirty)
		{
			_playerStateDirty = false;
			State.protocol = 287;
			State.seed = World.Seed;
			State.saveCreatedTime = Epoch.FromDateTime(SaveRestore.SaveCreatedTime);
			SingletonComponent<ServerMgr>.Instance.playerStateManager.Save(userID);
		}
	}

	public void ResetPlayerState()
	{
		SingletonComponent<ServerMgr>.Instance.playerStateManager.Reset(userID);
		ClientRPC(RpcTarget.Player("SetHostileLength", this), 0f);
		SendMarkersToClient();
		WipeMissions(saveImmediately: true);
		if ((Object)(object)modifiers != (Object)null)
		{
			modifiers.RemoveAll();
		}
		DirtyPlayerState();
		SavePlayerState();
		AdventCalendar.playerRewardHistory.Remove(userID);
	}

	public static void RecordToastToPlayOnReconnect(GameTip.Styles style, Phrase phrase, ulong playerId)
	{
		if ((Object)(object)FindByID(playerId) != (Object)null)
		{
			return;
		}
		PlayerState val = SingletonComponent<ServerMgr>.Instance.playerStateManager.Get(playerId);
		if (val != null)
		{
			if (val.toastOnReconnect == null)
			{
				val.toastOnReconnect = (List<ReconnectToast>)(object)Pool.Get<PooledList<ReconnectToast>>();
			}
			ReconnectToast val2 = Pool.Get<ReconnectToast>();
			val2.phrase = phrase.token;
			val2.type = (int)style;
			val.toastOnReconnect.Add(val2);
		}
		SingletonComponent<ServerMgr>.Instance.playerStateManager.Save(playerId);
	}

	public bool IsSleeping()
	{
		return HasPlayerFlag(PlayerFlags.Sleeping);
	}

	public bool IsSpectating()
	{
		return HasPlayerFlag(PlayerFlags.Spectating);
	}

	public bool IsRelaxed()
	{
		return HasPlayerFlag(PlayerFlags.Relaxed);
	}

	public bool IsServerFalling()
	{
		return HasPlayerFlag(PlayerFlags.ServerFall);
	}

	public bool IsLoadingAfterTransfer()
	{
		return HasPlayerFlag(PlayerFlags.LoadingAfterTransfer);
	}

	public bool CanBuild()
	{
		return CanBuild(PrivilegeCacheDefaultValue());
	}

	public bool IsBuildBlockedByMonument()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ConstructionErrors.IsBuildBlockedByMonument(((Component)playerCollider).transform.position);
	}

	public bool CanBuild(bool cached, float cacheDuration = 1f)
	{
		if (IsBuildingBlockedByVehicle(cached, cacheDuration))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(cached, cacheDuration))
		{
			return false;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(cached, cacheDuration);
		if ((Object)(object)buildingPrivilege == (Object)null)
		{
			return true;
		}
		return buildingPrivilege.CanBuild(this);
	}

	public bool CanBuild(Vector3 position, Quaternion rotation, Bounds bounds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return CanBuild(position, rotation, bounds, PrivilegeCacheDefaultValue());
	}

	public bool CanBuild(Vector3 position, Quaternion rotation, Bounds bounds, bool cached)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		OBB obb = default(OBB);
		((OBB)(ref obb))._002Ector(position, rotation, bounds);
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return false;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if ((Object)(object)buildingPrivilege == (Object)null)
		{
			return true;
		}
		return buildingPrivilege.CanBuild(this);
	}

	public bool CanBuild(OBB obb)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return CanBuild(obb, PrivilegeCacheDefaultValue());
	}

	public bool CanBuild(OBB obb, bool cached)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return false;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if ((Object)(object)buildingPrivilege == (Object)null)
		{
			return true;
		}
		return buildingPrivilege.CanBuild(this);
	}

	public bool IsBuildingBlocked()
	{
		return IsBuildingBlocked(PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingBlocked(bool cached)
	{
		if (IsBuildingBlockedByVehicle(cached))
		{
			return true;
		}
		if (IsBuildingBlockedByEntity(cached))
		{
			return true;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(cached);
		if ((Object)(object)buildingPrivilege == (Object)null)
		{
			return false;
		}
		return !buildingPrivilege.CanBuild(this);
	}

	public bool IsBuildingBlocked(Vector3 position, Quaternion rotation, Bounds bounds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return IsBuildingBlocked(position, rotation, bounds, PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingBlocked(Vector3 position, Quaternion rotation, Bounds bounds, bool cached)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		OBB obb = default(OBB);
		((OBB)(ref obb))._002Ector(position, rotation, bounds);
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return true;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return true;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if ((Object)(object)buildingPrivilege == (Object)null)
		{
			return false;
		}
		return !buildingPrivilege.CanBuild(this);
	}

	public bool IsBuildingBlocked(OBB obb)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return IsBuildingBlocked(obb, PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingBlocked(OBB obb, bool cached)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return true;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return true;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if ((Object)(object)buildingPrivilege == (Object)null)
		{
			return false;
		}
		return !buildingPrivilege.CanBuild(this);
	}

	public bool IsBuildingAuthed()
	{
		return IsBuildingAuthed(PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingAuthed(bool cached, float cacheDuration = 1f)
	{
		if (IsBuildingBlockedByVehicle(cached, cacheDuration))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(cached, cacheDuration))
		{
			return false;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(cached, cacheDuration);
		if ((Object)(object)buildingPrivilege == (Object)null)
		{
			return false;
		}
		return buildingPrivilege.CanBuild(this);
	}

	public bool IsBuildingAuthed(Vector3 position, Quaternion rotation, Bounds bounds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return IsBuildingAuthed(position, rotation, bounds, PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingAuthed(Vector3 position, Quaternion rotation, Bounds bounds, bool cached)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		OBB obb = default(OBB);
		((OBB)(ref obb))._002Ector(position, rotation, bounds);
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return false;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if ((Object)(object)buildingPrivilege == (Object)null)
		{
			return false;
		}
		return buildingPrivilege.CanBuild(this);
	}

	public bool IsBuildingAuthed(OBB obb)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return IsBuildingAuthed(obb, PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingAuthed(OBB obb, bool cached)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return false;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if ((Object)(object)buildingPrivilege == (Object)null)
		{
			return false;
		}
		return buildingPrivilege.CanBuild(this);
	}

	public bool CanPlaceBuildingPrivilege()
	{
		return CanPlaceBuildingPrivilege(PrivilegeCacheDefaultValue());
	}

	public bool CanPlaceBuildingPrivilege(bool cached)
	{
		if (IsBuildingBlockedByVehicle(cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(cached))
		{
			return false;
		}
		return (Object)(object)GetBuildingPrivilege(cached) == (Object)null;
	}

	public bool CanPlaceBuildingPrivilege(Vector3 position, Quaternion rotation, Bounds bounds, BuildingPrivlidge exclude = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return CanPlaceBuildingPrivilege(position, rotation, bounds, PrivilegeCacheDefaultValue(), exclude);
	}

	public bool CanPlaceBuildingPrivilege(Vector3 position, Quaternion rotation, Bounds bounds, bool cached, BuildingPrivlidge exclude = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		OBB obb = default(OBB);
		((OBB)(ref obb))._002Ector(position, rotation, bounds);
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return false;
		}
		return (Object)(object)GetBuildingPrivilege(obb, cached, 1f, exclude) == (Object)null;
	}

	public bool CanPlaceBuildingPrivilege(OBB obb)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return CanPlaceBuildingPrivilege(obb, PrivilegeCacheDefaultValue());
	}

	public bool CanPlaceBuildingPrivilege(OBB obb, bool cached)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return false;
		}
		return (Object)(object)GetBuildingPrivilege(obb, cached) == (Object)null;
	}

	public bool IsNearEnemyBase()
	{
		return IsNearEnemyBase(PrivilegeCacheDefaultValue());
	}

	public bool IsNearEnemyBase(bool cached)
	{
		if (IsBuildingBlockedByVehicle(cached))
		{
			return true;
		}
		if (IsBuildingBlockedByEntity(cached))
		{
			return true;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(cached);
		if ((Object)(object)buildingPrivilege == (Object)null)
		{
			return false;
		}
		if (!buildingPrivilege.IsAuthed(this))
		{
			return buildingPrivilege.AnyAuthed();
		}
		return false;
	}

	public bool IsNearEnemyBase(Vector3 position, Quaternion rotation, Bounds bounds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return IsNearEnemyBase(position, rotation, bounds, PrivilegeCacheDefaultValue());
	}

	public bool IsNearEnemyBase(Vector3 position, Quaternion rotation, Bounds bounds, bool cached)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		OBB obb = default(OBB);
		((OBB)(ref obb))._002Ector(position, rotation, bounds);
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return true;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return true;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if ((Object)(object)buildingPrivilege == (Object)null)
		{
			return false;
		}
		if (!buildingPrivilege.IsAuthed(this))
		{
			return buildingPrivilege.AnyAuthed();
		}
		return false;
	}

	public bool IsNearEnemyBase(OBB obb)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return IsNearEnemyBase(obb, PrivilegeCacheDefaultValue());
	}

	public bool IsNearEnemyBase(OBB obb, bool cached)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return true;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return true;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if ((Object)(object)buildingPrivilege == (Object)null)
		{
			return false;
		}
		if (!buildingPrivilege.IsAuthed(this))
		{
			return buildingPrivilege.AnyAuthed();
		}
		return false;
	}

	public bool IsBuildingBlockedByVehicle()
	{
		return IsBuildingBlockedByVehicle(PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingBlockedByVehicle(bool cached, float cacheDuration = 1f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return IsBuildingBlockedByVehicle(WorldSpaceBounds(), cached);
	}

	public BaseEntity GetVehicleBuildingPrivilege(bool cached, float cacheDuration = 1f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return GetVehicleBuildingPrivilege(WorldSpaceBounds(), cached, cacheDuration);
	}

	public BaseEntity GetVehicleBuildingPrivilege(OBB obb, bool cached, float cacheDuration = 1f)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		if (cached && BaseEntity.IsCacheValid(cachedVehicleBuildingPrivilegeTime, cacheDuration, cachedVehicleBuildingPrivilegePosition, obb.position))
		{
			return cachedVehicleBuildingPrivilege;
		}
		cachedVehicleBuildingPrivilege = null;
		cachedVehicleBuildingPrivilegeBlocked = false;
		BoatBuildingStation forPlayer = BoatBuildingStation.GetForPlayer(this);
		if ((Object)(object)forPlayer != (Object)null)
		{
			cachedVehicleBuildingPrivilege = forPlayer;
			cachedVehicleBuildingPrivilegeTime = Time.time;
			cachedVehicleBuildingPrivilegePosition = obb.position;
			cachedVehicleBuildingPrivilegeBlocked = !forPlayer.CanPlayerBuild(this);
			return cachedVehicleBuildingPrivilege;
		}
		List<BaseVehicle> list = Pool.Get<List<BaseVehicle>>();
		Vis.Entities(obb.position, 2f + ((Vector3)(ref obb.extents)).magnitude, list, 134217728, (QueryTriggerInteraction)2);
		for (int i = 0; i < list.Count; i++)
		{
			BaseVehicle baseVehicle = list[i];
			if (baseVehicle.isServer == base.isServer && !baseVehicle.IsDead() && !(((OBB)(ref obb)).Distance(baseVehicle.WorldSpaceBounds()) > 2f) && baseVehicle.HasBuildingPrivilege)
			{
				VehiclePrivilege childPrivilege = baseVehicle.GetChildPrivilege();
				if ((Object)(object)childPrivilege != (Object)null)
				{
					cachedVehicleBuildingPrivilege = childPrivilege;
					cachedVehicleBuildingPrivilegeBlocked = !childPrivilege.IsAuthed(this);
				}
				break;
			}
		}
		Pool.FreeUnmanaged<BaseVehicle>(ref list);
		cachedVehicleBuildingPrivilegeTime = Time.time;
		cachedVehicleBuildingPrivilegePosition = obb.position;
		return cachedVehicleBuildingPrivilege;
	}

	private bool IsBuildingBlockedByVehicle(OBB obb, bool cached, float cacheDuration = 1f)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (cached && BaseEntity.IsCacheValid(cachedVehicleBuildingPrivilegeTime, cacheDuration, cachedVehicleBuildingPrivilegePosition, obb.position))
		{
			if ((Object)(object)cachedVehicleBuildingPrivilege != (Object)null)
			{
				return cachedVehicleBuildingPrivilegeBlocked;
			}
			return false;
		}
		if ((Object)(object)GetVehicleBuildingPrivilege(obb, cached, cacheDuration) != (Object)null)
		{
			return cachedVehicleBuildingPrivilegeBlocked;
		}
		return false;
	}

	public bool IsBuildingBlockedByEntity()
	{
		return IsBuildingBlockedByEntity(PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingBlockedByEntity(bool cached, float cacheDuration = 1f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return IsBuildingBlockedByEntity(WorldSpaceBounds(), cached, cacheDuration);
	}

	private bool IsBuildingBlockedByEntity(OBB obb, bool cached, float cacheDuration = 1f)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (cached && BaseEntity.IsCacheValid(cachedEntityBuildingPrivilegeTime, cacheDuration, cachedEntityBuildingPrivilegePosition, obb.position))
		{
			if ((Object)(object)cachedEntityBuildingPrivilege != (Object)null)
			{
				return cachedEntityBuildingPrivilegeBlocked;
			}
			return false;
		}
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		Vis.Entities(obb.position, 3f + ((Vector3)(ref obb.extents)).magnitude, list, 2097152, (QueryTriggerInteraction)2);
		cachedEntityBuildingPrivilege = null;
		cachedEntityBuildingPrivilegeBlocked = false;
		for (int i = 0; i < list.Count; i++)
		{
			BaseEntity baseEntity = list[i];
			if (baseEntity.isServer != base.isServer || ((OBB)(ref obb)).Distance(baseEntity.WorldSpaceBounds()) > 3f)
			{
				continue;
			}
			EntityPrivilege entityBuildingPrivilege = baseEntity.GetEntityBuildingPrivilege();
			if (!((Object)(object)entityBuildingPrivilege == (Object)null))
			{
				cachedEntityBuildingPrivilege = baseEntity;
				if (!entityBuildingPrivilege.IsAuthed(this))
				{
					cachedEntityBuildingPrivilegeBlocked = true;
					break;
				}
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
		cachedEntityBuildingPrivilegeTime = Time.time;
		cachedEntityBuildingPrivilegePosition = obb.position;
		return cachedEntityBuildingPrivilegeBlocked;
	}

	public bool HasPrivilegeFromOther()
	{
		return HasPrivilegeFromOther(PrivilegeCacheDefaultValue());
	}

	public bool HasPrivilegeFromOther(bool cached)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (IsBuildingBlockedByVehicle(WorldSpaceBounds(), cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(WorldSpaceBounds(), cached))
		{
			return false;
		}
		if (!((Object)(object)cachedVehicleBuildingPrivilege != (Object)null))
		{
			return (Object)(object)cachedEntityBuildingPrivilege != (Object)null;
		}
		return true;
	}

	public void SetMortarCooldown(float duration)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		mortarCooldown = TimeUntil.op_Implicit(duration);
		if (base.isServer)
		{
			SendNetworkUpdate();
		}
	}

	public bool HasMortarCooldown()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return TimeUntil.op_Implicit(mortarCooldown) > 0f;
	}

	private static bool LineOfSightBidirectional(Vector3 p0, Vector3 p1, int lineOfSightLayerMask, BaseEntity ignoreEntity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (!GamePhysics.LineOfSight(p0, p1, lineOfSightLayerMask, ignoreEntity))
		{
			return false;
		}
		if (!GamePhysics.LineOfSight(p1, p0, lineOfSightLayerMask, ignoreEntity))
		{
			return false;
		}
		return true;
	}

	private static bool LineOfSightBasic(Vector3 p0_curProjectilePos, Vector3 p1_hitRaycastStartPos, Vector3 p2_closestRayPos, Vector3 p3_worldHitPos, FiredProjectile firedProjectile, int lineOfSightLayerMask)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		Vector3 val2 = Vector3.zero;
		if (ConVar.AntiHack.projectile_backtracking > 0f)
		{
			Vector3 val3 = p1_hitRaycastStartPos - p0_curProjectilePos;
			val = ((Vector3)(ref val3)).normalized * ConVar.AntiHack.projectile_backtracking;
			val3 = p2_closestRayPos - p1_hitRaycastStartPos;
			val2 = ((Vector3)(ref val3)).normalized * ConVar.AntiHack.projectile_backtracking;
		}
		if (!LineOfSightBidirectional(p0_curProjectilePos - val, p1_hitRaycastStartPos + val, lineOfSightLayerMask, firedProjectile.lastEntityHit))
		{
			return false;
		}
		if (!LineOfSightBidirectional(p1_hitRaycastStartPos - val2, p2_closestRayPos, lineOfSightLayerMask, firedProjectile.lastEntityHit))
		{
			return false;
		}
		if (!LineOfSightBidirectional(p2_closestRayPos, p3_worldHitPos, lineOfSightLayerMask, firedProjectile.lastEntityHit))
		{
			return false;
		}
		return true;
	}

	private static bool LineOfSightDetailed(Vector3 p0_curProjectilePos, FiredProjectile firedProjectile, int lineOfSightLayerMask)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3> simulatedPositions = firedProjectile.simulatedPositions;
		for (int i = 1; i < simulatedPositions.Count; i++)
		{
			if (!GamePhysics.LineOfSight(simulatedPositions[i - 1], simulatedPositions[i], lineOfSightLayerMask, firedProjectile.lastEntityHit))
			{
				return false;
			}
		}
		if (simulatedPositions.Count >= 1 && !GamePhysics.LineOfSight(simulatedPositions[simulatedPositions.Count - 1], p0_curProjectilePos, lineOfSightLayerMask, firedProjectile.lastEntityHit))
		{
			return false;
		}
		return true;
	}

	private static bool LineOfSightPlayer(Vector3 p0_worldHitPos, Vector3 p1_posOnPlayer, int lineOfSightLayerMask, float padding)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!GamePhysics.LineOfSight(p0_worldHitPos, p1_posOnPlayer, lineOfSightLayerMask, 0f, padding))
		{
			return false;
		}
		if (!GamePhysics.LineOfSight(p1_posOnPlayer, p0_worldHitPos, lineOfSightLayerMask, padding, 0f))
		{
			return false;
		}
		return true;
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	public void OnProjectileAttack(RPCMessage msg)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0936: Unknown result type (might be due to invalid IL or missing references)
		//IL_093b: Unknown result type (might be due to invalid IL or missing references)
		//IL_141f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1424: Unknown result type (might be due to invalid IL or missing references)
		//IL_1431: Unknown result type (might be due to invalid IL or missing references)
		//IL_1441: Unknown result type (might be due to invalid IL or missing references)
		//IL_1446: Unknown result type (might be due to invalid IL or missing references)
		//IL_14a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bfc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c0b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c0d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c0f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c14: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c1d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c1f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c27: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c28: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c29: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c2b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c3d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c4f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c61: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c7a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c8c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c9e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bbc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1109: Unknown result type (might be due to invalid IL or missing references)
		//IL_110e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1116: Unknown result type (might be due to invalid IL or missing references)
		//IL_111b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1123: Unknown result type (might be due to invalid IL or missing references)
		//IL_1128: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_09aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_1157: Unknown result type (might be due to invalid IL or missing references)
		//IL_1159: Unknown result type (might be due to invalid IL or missing references)
		//IL_115e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1160: Unknown result type (might be due to invalid IL or missing references)
		//IL_1162: Unknown result type (might be due to invalid IL or missing references)
		//IL_1164: Unknown result type (might be due to invalid IL or missing references)
		//IL_1166: Unknown result type (might be due to invalid IL or missing references)
		//IL_112e: Unknown result type (might be due to invalid IL or missing references)
		//IL_113b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1145: Unknown result type (might be due to invalid IL or missing references)
		//IL_114a: Unknown result type (might be due to invalid IL or missing references)
		//IL_114f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ef0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ef2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ef8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0efd: Unknown result type (might be due to invalid IL or missing references)
		//IL_130b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1310: Unknown result type (might be due to invalid IL or missing references)
		//IL_131d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1322: Unknown result type (might be due to invalid IL or missing references)
		//IL_132a: Unknown result type (might be due to invalid IL or missing references)
		//IL_132f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1338: Unknown result type (might be due to invalid IL or missing references)
		//IL_133a: Unknown result type (might be due to invalid IL or missing references)
		//IL_117e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f0e: Unknown result type (might be due to invalid IL or missing references)
		//IL_134b: Unknown result type (might be due to invalid IL or missing references)
		//IL_134d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f1e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f23: Unknown result type (might be due to invalid IL or missing references)
		//IL_1399: Unknown result type (might be due to invalid IL or missing references)
		//IL_13a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_13b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_128f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1299: Unknown result type (might be due to invalid IL or missing references)
		//IL_12a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_12ad: Unknown result type (might be due to invalid IL or missing references)
		PlayerProjectileAttack val = msg.read.Proto<PlayerProjectileAttack>((PlayerProjectileAttack)null);
		try
		{
			if (val == null)
			{
				return;
			}
			PlayerAttack playerAttack = val.playerAttack;
			HitInfo hitInfo = Pool.Get<HitInfo>();
			try
			{
				hitInfo.LoadFromAttack(playerAttack.attack, serverSide: true);
				hitInfo.Initiator = this;
				hitInfo.ProjectileID = playerAttack.projectileID;
				hitInfo.ProjectileDistance = val.hitDistance;
				hitInfo.ProjectileVelocity = val.hitVelocity;
				hitInfo.ProjectileTravelTime = val.travelTime;
				hitInfo.Predicted = msg.connection;
				if (hitInfo.IsNaNOrInfinity() || float.IsNaN(val.travelTime) || float.IsInfinity(val.travelTime))
				{
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Contains NaN ({playerAttack.projectileID})");
					stats.combat.LogInvalid(hitInfo, "projectile_nan");
					return;
				}
				if (!firedProjectiles.TryGetValue(playerAttack.projectileID, out var firedProjectile))
				{
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Missing ID ({playerAttack.projectileID})", logToAnalytics: false);
					stats.combat.LogInvalid(hitInfo, "projectile_invalid");
					return;
				}
				hitInfo.ProjectileHits = firedProjectile.hits;
				hitInfo.ProjectileIntegrity = firedProjectile.integrity;
				hitInfo.ProjectileTrajectoryMismatch = firedProjectile.trajectoryMismatch;
				if (firedProjectile.integrity <= 0f)
				{
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Integrity is zero ({playerAttack.projectileID})");
					Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
					stats.combat.LogInvalid(hitInfo, "projectile_integrity");
					return;
				}
				if (firedProjectile.firedTime < Time.realtimeSinceStartup - 8f)
				{
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Lifetime is zero ({playerAttack.projectileID})");
					Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
					stats.combat.LogInvalid(hitInfo, "projectile_lifetime");
					return;
				}
				if (firedProjectile.ricochets > 0)
				{
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile attack is ricochet ({playerAttack.projectileID})");
					Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
					stats.combat.LogInvalid(hitInfo, "projectile_ricochet_attack");
					return;
				}
				if (val.hitVelocity == Vector3.zero)
				{
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile hitVelocity is zero ({playerAttack.projectileID})");
					Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
					stats.combat.LogInvalid(hitInfo, "projectile_zero_hit_velocity");
					return;
				}
				hitInfo.Weapon = firedProjectile.weaponSource;
				hitInfo.WeaponPrefab = firedProjectile.weaponPrefab;
				hitInfo.ProjectilePrefab = firedProjectile.projectilePrefab;
				hitInfo.damageProperties = firedProjectile.projectilePrefab.damageProperties;
				Vector3 position = firedProjectile.position;
				Vector3 initialPositionOffset = firedProjectile.initialPositionOffset;
				Vector3 positionOffset = firedProjectile.positionOffset;
				Vector3 velocity = firedProjectile.velocity;
				float partialTime = firedProjectile.partialTime;
				float travelTime = firedProjectile.travelTime;
				float num = Mathf.Clamp(val.travelTime, firedProjectile.travelTime, 8f);
				Vector3 gravity = Physics.gravity * firedProjectile.projectilePrefab.gravityModifier;
				float drag = firedProjectile.projectilePrefab.drag;
				BaseEntity hitEntity = hitInfo.HitEntity;
				BasePlayer hitPlayer = hitEntity as BasePlayer;
				bool flag = (Object)(object)hitPlayer != (Object)null;
				bool flag2 = flag && hitPlayer.IsSleeping();
				bool flag3 = flag && hitPlayer.IsWounded();
				bool flag4 = flag && hitPlayer.isMounted;
				bool flag5 = flag && hitPlayer.HasParent();
				bool flag6 = (Object)(object)hitEntity != (Object)null;
				bool flag7 = flag6 && hitEntity.IsNpc;
				bool flag8 = hitInfo.HitMaterial == Projectile.WaterMaterialID();
				if (firedProjectile.protection <= 0)
				{
					return;
				}
				bool valid = true;
				float num2 = 1f + ConVar.AntiHack.projectile_forgiveness;
				float num3 = 1f - ConVar.AntiHack.projectile_forgiveness;
				float projectile_clientframes = ConVar.AntiHack.projectile_clientframes;
				float projectile_serverframes = ConVar.AntiHack.projectile_serverframes;
				float num4 = Mathx.Decrement(firedProjectile.firedTime);
				float num5 = Mathf.Clamp(Mathx.Increment(Time.realtimeSinceStartup) - num4, 0f, 8f);
				float num6 = num;
				float num7 = Mathf.Abs(num5 - num6);
				firedProjectile.desyncLifeTime = num7;
				float num8 = Mathf.Min(num5, num6);
				float num9 = projectile_clientframes / 60f;
				float num10 = projectile_serverframes * Mathx.Max(Time.deltaTime, Time.smoothDeltaTime, Time.fixedDeltaTime);
				float num11 = (desyncTimeClamped + num8 + num9 + num10) * num2;
				float entityDeltaTime = ((firedProjectile.protection >= 6) ? ((desyncTimeClamped + num9 + num10) * num2) : num11);
				_ = desyncTimeClamped;
				float num12 = Vector3.Distance(firedProjectile.initialPosition, hitInfo.HitPositionWorld);
				int num13 = 1075904512;
				if (ConVar.AntiHack.projectile_terraincheck)
				{
					num13 |= 0x800000;
				}
				if (ConVar.AntiHack.projectile_vehiclecheck)
				{
					num13 |= 0x8000000;
				}
				if (ConVar.AntiHack.projectile_defaultcheck)
				{
					num13 |= 1;
				}
				if (ConVar.AntiHack.projectile_deployedcheck)
				{
					num13 |= 0x100;
				}
				if (flag6 && net.group != null && hitEntity.net != null && hitEntity.net.group != null && !net.subscriber.IsSubscribed(hitEntity.net.group))
				{
					AntiHack.Log(this, AntiHackType.ProjectileHack, "Entity out of network range");
					stats.combat.LogInvalid(hitInfo, "projectile_network_range");
					valid = false;
				}
				if (flag && hitInfo.boneArea == (HitArea)(-1))
				{
					string name = ((Object)hitInfo.ProjectilePrefab).name;
					string arg = (flag6 ? hitEntity.ShortPrefabName : "world");
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Bone is invalid ({name} on {arg} bone {hitInfo.HitBone})");
					stats.combat.LogInvalid(hitInfo, "projectile_bone");
					valid = false;
				}
				if (flag8)
				{
					if (flag6)
					{
						string name2 = ((Object)hitInfo.ProjectilePrefab).name;
						string text = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, "Projectile water hit on entity (" + name2 + " on " + text + ")");
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "water_entity");
						valid = false;
					}
					if (!WaterLevel.Test(hitInfo.HitPositionWorld - 0.5f * Vector3.up, waves: true, volumes: true, this))
					{
						string name3 = ((Object)hitInfo.ProjectilePrefab).name;
						string text2 = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, "Projectile water level (" + name3 + " on " + text2 + ")");
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "water_level");
						valid = false;
					}
				}
				if (firedProjectile.protection >= 2)
				{
					bool flag9 = flag && !flag7 && !flag2 && !flag3;
					if (firedProjectile.protection >= 6 && flag9)
					{
						if (flag5 || flag4)
						{
							if (flag5 && ConVar.AntiHack.parenthistory && hitPlayer.tickHistory.ParentCount > 0)
							{
								VerifyParentedPlayerDistance();
							}
							else
							{
								VerifyEntityDistance();
							}
						}
						else
						{
							VerifyPlayerDistance();
						}
					}
					else if (flag6)
					{
						VerifyEntityDistance();
					}
				}
				Vector3 parentVelocity;
				if (firedProjectile.protection >= 1)
				{
					float num14;
					if (!flag6)
					{
						num14 = 0f;
					}
					else
					{
						float num15 = hitEntity.AntiHackVelocity();
						parentVelocity = hitEntity.GetParentVelocity();
						num14 = num15 + ((Vector3)(ref parentVelocity)).magnitude;
					}
					float num16 = num14;
					float num17 = (flag6 ? (entityDeltaTime * num16) : 0f);
					float magnitude = ((Vector3)(ref firedProjectile.initialVelocity)).magnitude;
					float num18 = hitInfo.ProjectilePrefab.initialDistance + num11 * magnitude;
					float num19 = hitInfo.ProjectileDistance + 1f + ((Vector3)(ref positionOffset)).magnitude + num17;
					parentVelocity = estimatedVelocity;
					float num20 = num19 + ((Vector3)(ref parentVelocity)).magnitude;
					if (num12 > num18)
					{
						string name4 = ((Object)hitInfo.ProjectilePrefab).name;
						string text3 = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, string.Format("Projectile too fast ({0} on {1} with {2}m > {3}m in {4}s)", new object[5] { name4, text3, num12, num18, num11 }));
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "projectile_maxspeed");
						valid = false;
					}
					if (num12 > num20)
					{
						string name5 = ((Object)hitInfo.ProjectilePrefab).name;
						string text4 = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, string.Format("Projectile too far away ({0} on {1} with {2}m > {3}m in {4}s)", new object[5] { name5, text4, num12, num20, num11 }));
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "projectile_distance");
						valid = false;
					}
					if (num7 > ConVar.AntiHack.projectile_desync)
					{
						string name6 = ((Object)hitInfo.ProjectilePrefab).name;
						string text5 = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, string.Format("Projectile desync ({0} on {1} with {2}s > {3}s)", new object[4]
						{
							name6,
							text5,
							num7,
							ConVar.AntiHack.projectile_desync
						}));
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "projectile_desync");
						valid = false;
					}
				}
				if (firedProjectile.protection >= 4)
				{
					float num21 = 0f;
					if (flag6)
					{
						parentVelocity = hitEntity.GetParentVelocity();
						float num22 = ((Vector3)(ref parentVelocity)).magnitude;
						if (hitEntity is ILargeVehicleForProjectiles)
						{
							num22 += hitEntity.AntiHackVelocity();
						}
						num21 = entityDeltaTime * num22;
					}
					SimulateProjectile(ref position, ref velocity, ref partialTime, num - travelTime, gravity, drag, out var prevPosition, out var prevVelocity);
					Line val2 = default(Line);
					((Line)(ref val2))._002Ector(prevPosition - prevVelocity, prevPosition);
					Line val3 = default(Line);
					((Line)(ref val3))._002Ector(prevPosition, position);
					Line val4 = default(Line);
					((Line)(ref val4))._002Ector(position, position + velocity);
					float num23 = Mathx.Min(((Line)(ref val2)).Distance(hitInfo.PointStart), ((Line)(ref val3)).Distance(hitInfo.PointStart), ((Line)(ref val4)).Distance(hitInfo.PointStart));
					float num24 = Mathx.Min(((Line)(ref val2)).Distance(hitInfo.HitPositionWorld), ((Line)(ref val3)).Distance(hitInfo.HitPositionWorld), ((Line)(ref val4)).Distance(hitInfo.HitPositionWorld));
					float num25 = (firedProjectile.startPointMismatch = Mathf.Max(num23 - ((Vector3)(ref initialPositionOffset)).magnitude - num21, 0f));
					float num26 = (firedProjectile.endPointMismatch = Mathf.Max(num24 - ((Vector3)(ref initialPositionOffset)).magnitude - num21, 0f));
					if (num25 > ConVar.AntiHack.projectile_trajectory)
					{
						string name7 = ((Object)firedProjectile.projectilePrefab).name;
						string text6 = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, string.Format("Start position trajectory ({0} on {1} with {2}m > {3}m)", new object[4]
						{
							name7,
							text6,
							num25,
							ConVar.AntiHack.projectile_trajectory
						}));
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "trajectory_start");
						valid = false;
					}
					if (num26 > ConVar.AntiHack.projectile_trajectory)
					{
						string name8 = ((Object)firedProjectile.projectilePrefab).name;
						string text7 = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, string.Format("End position trajectory ({0} on {1} with {2}m > {3}m)", new object[4]
						{
							name8,
							text7,
							num26,
							ConVar.AntiHack.projectile_trajectory
						}));
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "trajectory_end");
						valid = false;
					}
					if (hitInfo.ProjectileTrajectoryMismatch > ConVar.AntiHack.projectile_trajectory_update)
					{
						string name9 = ((Object)firedProjectile.projectilePrefab).name;
						string text8 = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, string.Format("Update position trajectory ({0} on {1} with {2}m > {3}m)", new object[4]
						{
							name9,
							text8,
							hitInfo.ProjectileTrajectoryMismatch,
							ConVar.AntiHack.projectile_trajectory_update
						}));
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "trajectory_update_total");
						valid = false;
					}
					hitInfo.ProjectileVelocity = velocity;
					if (val.hitVelocity != Vector3.zero && velocity != Vector3.zero)
					{
						float num27 = Vector3.Angle(val.hitVelocity, velocity);
						float num28 = ((Vector3)(ref val.hitVelocity)).magnitude / ((Vector3)(ref velocity)).magnitude;
						if (num27 > ConVar.AntiHack.projectile_anglechange)
						{
							string name10 = ((Object)firedProjectile.projectilePrefab).name;
							string text9 = (flag6 ? hitEntity.ShortPrefabName : "world");
							AntiHack.Log(this, AntiHackType.ProjectileHack, string.Format("Trajectory angle change ({0} on {1} with {2}deg > {3}deg)", new object[4]
							{
								name10,
								text9,
								num27,
								ConVar.AntiHack.projectile_anglechange
							}));
							Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
							stats.combat.LogInvalid(hitInfo, "angle_change");
							valid = false;
						}
						if (num28 > ConVar.AntiHack.projectile_velocitychange)
						{
							string name11 = ((Object)firedProjectile.projectilePrefab).name;
							string text10 = (flag6 ? hitEntity.ShortPrefabName : "world");
							AntiHack.Log(this, AntiHackType.ProjectileHack, string.Format("Trajectory velocity change ({0} on {1} with {2} > {3})", new object[4]
							{
								name11,
								text10,
								num28,
								ConVar.AntiHack.projectile_velocitychange
							}));
							Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
							stats.combat.LogInvalid(hitInfo, "velocity_change");
							valid = false;
						}
					}
				}
				if (firedProjectile.protection >= 3)
				{
					if (firedProjectile.simulatedPositions.Count > ConVar.AntiHack.projectile_update_limit)
					{
						AntiHack.Log(this, AntiHackType.ProjectileHack, $"projectile_update_limit exceeded on attack ({firedProjectile.simulatedPositions.Count} > {ConVar.AntiHack.projectile_update_limit})");
						stats.combat.LogInvalid(hitInfo, "projectile_update_limit");
						valid = false;
					}
					if (valid)
					{
						Vector3 position2 = firedProjectile.position;
						Vector3 pointStart = hitInfo.PointStart;
						Vector3 val5 = hitInfo.HitPositionWorld;
						if (!flag8)
						{
							val5 -= ((Vector3)(ref hitInfo.ProjectileVelocity)).normalized * 0.001f;
						}
						Vector3 val6 = hitInfo.PositionOnRay(val5);
						bool flag10 = LineOfSightBasic(position2, pointStart, val6, val5, firedProjectile, num13);
						bool flag11 = true;
						if (flag10)
						{
							flag11 = LineOfSightDetailed(position2, firedProjectile, num13);
						}
						bool flag12 = flag10 && flag11;
						string text11 = (flag6 ? hitEntity.Categorize() : "world");
						string text12 = string.Empty;
						switch (text11)
						{
						case "player":
							text12 = (flag12 ? "hit_player_direct_los" : "hit_player_indirect_los");
							break;
						case "building":
							text12 = (flag12 ? "hit_building_direct_los" : "hit_building_indirect_los");
							break;
						case "entity":
							text12 = (flag12 ? "hit_entity_direct_los" : "hit_entity_indirect_los");
							break;
						}
						if (!string.IsNullOrEmpty(text12))
						{
							stats.Add(text12, 1, Stats.Server);
						}
						if (!flag12 && flag6)
						{
							string name12 = ((Object)hitInfo.ProjectilePrefab).name;
							string shortPrefabName = hitEntity.ShortPrefabName;
							string text13 = ((!flag10) ? "projectile_los" : "projectile_los_detailed");
							AntiHack.Log(this, AntiHackType.ProjectileHack, string.Format("Line of sight {0} ({1} on {2}) {3} {4} {5} {6}", new object[7] { text13, name12, shortPrefabName, position2, pointStart, val6, val5 }));
							Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
							stats.combat.LogInvalid(hitInfo, text13);
						}
						if (!flag12)
						{
							valid = false;
						}
					}
					if (valid && flag && !flag7)
					{
						Vector3 hitPositionWorld = hitInfo.HitPositionWorld;
						Vector3 position3 = hitPlayer.eyes.position;
						Vector3 val7 = hitPlayer.CenterPoint();
						float projectile_losforgiveness = ConVar.AntiHack.projectile_losforgiveness;
						bool flag13 = LineOfSightPlayer(hitPositionWorld, position3, num13, projectile_losforgiveness);
						if (!flag13)
						{
							flag13 = LineOfSightPlayer(hitPositionWorld, val7, num13, projectile_losforgiveness);
						}
						if (!flag13)
						{
							string name13 = ((Object)hitInfo.ProjectilePrefab).name;
							string shortPrefabName2 = hitEntity.ShortPrefabName;
							AntiHack.Log(this, AntiHackType.ProjectileHack, string.Format("Line of sight player ({0} on {1}) {2} {3} or {4} {5}", new object[6] { name13, shortPrefabName2, hitPositionWorld, position3, hitPositionWorld, val7 }));
							Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
							stats.combat.LogInvalid(hitInfo, "projectile_los_player");
							valid = false;
						}
					}
					if (!valid)
					{
						AntiHack.AddViolation(this, AntiHackType.ProjectileHack, ConVar.AntiHack.projectile_penalty);
						return;
					}
				}
				firedProjectile.position = hitInfo.HitPositionWorld;
				firedProjectile.velocity = ((Vector3)(ref velocity)).normalized * ((Vector3)(ref val.hitVelocity)).magnitude;
				firedProjectile.travelTime = num;
				firedProjectile.partialTime = partialTime;
				firedProjectile.hits++;
				firedProjectile.lastEntityHit = hitEntity;
				firedProjectile.simulatedPositions.Clear();
				firedProjectile.simulatedPositions.Add(position);
				hitInfo.ProjectilePrefab.CalculateDamage(hitInfo, firedProjectile.projectileModifier, firedProjectile.integrity);
				if (flag8)
				{
					if (hitInfo.ProjectilePrefab.waterIntegrityLoss > 0f)
					{
						firedProjectile.integrity = Mathf.Clamp01(firedProjectile.integrity - hitInfo.ProjectilePrefab.waterIntegrityLoss);
					}
				}
				else if (hitInfo.ProjectilePrefab.penetrationPower <= 0f || !flag6)
				{
					firedProjectile.integrity = 0f;
				}
				else
				{
					float num29 = hitEntity.PenetrationResistance(hitInfo) / hitInfo.ProjectilePrefab.penetrationPower;
					firedProjectile.integrity = Mathf.Clamp01(firedProjectile.integrity - num29);
				}
				if (flag6)
				{
					stats.Add(firedProjectile.itemMod.category + "_hit_" + hitEntity.Categorize(), 1);
				}
				if (Interface.CallHook("OnPlayerAttack", this, hitInfo) != null)
				{
					return;
				}
				if (firedProjectile.integrity <= 0f)
				{
					if (hitInfo.ProjectilePrefab.remainInWorld)
					{
						CreateWorldProjectile(hitInfo, firedProjectile.itemDef, firedProjectile.itemMod, hitInfo.ProjectilePrefab, firedProjectile.pickupItem);
					}
					if (firedProjectile.hits <= ConVar.AntiHack.projectile_impactspawndepth)
					{
						firedProjectile.itemMod.ServerProjectileHit(hitInfo);
					}
				}
				else if (firedProjectile.hits == ConVar.AntiHack.projectile_impactspawndepth)
				{
					firedProjectile.itemMod.ServerProjectileHit(hitInfo);
				}
				firedProjectiles[playerAttack.projectileID] = firedProjectile;
				if (flag6)
				{
					if (firedProjectile.hits <= ConVar.AntiHack.projectile_damagedepth)
					{
						hitEntity.OnAttacked(hitInfo);
						firedProjectile.itemMod.ServerProjectileHitEntity(hitInfo);
					}
					else
					{
						stats.combat.LogInvalid(hitInfo, "ricochet");
					}
				}
				Projectile.CustomEffectData clientEffectData = firedProjectile.projectilePrefab.clientEffectData;
				bool playDefaultHitEffects = firedProjectile.projectilePrefab.playDefaultHitEffects;
				GameObjectRef clientEffectPrefab = firedProjectile.projectilePrefab.clientEffectPrefab;
				if (!clientEffectData.UseCustomEffect || playDefaultHitEffects)
				{
					Effect.server.ImpactEffect(hitInfo);
				}
				if (clientEffectData.UseCustomEffect)
				{
					string text14 = null;
					if (clientEffectPrefab != null && clientEffectPrefab.isValid)
					{
						text14 = clientEffectPrefab.resourcePath;
					}
					if (text14 != null)
					{
						Effect.server.ImpactEffect(hitInfo, text14);
					}
				}
				hitInfo.DoHitEffects = hitInfo.ProjectilePrefab.doHitEffects;
				SingletonComponent<NpcNoiseManager>.Instance.OnProjectileHit(this, hitInfo);
				void VerifyEntityDistance()
				{
					//IL_0011: Unknown result type (might be due to invalid IL or missing references)
					//IL_0016: Unknown result type (might be due to invalid IL or missing references)
					//IL_0047: Unknown result type (might be due to invalid IL or missing references)
					float num30 = hitEntity.AntiHackVelocity();
					Vector3 parentVelocity2 = hitEntity.GetParentVelocity();
					float num31 = num30 + ((Vector3)(ref parentVelocity2)).magnitude;
					float num32 = hitEntity.AntiHackPadding() + entityDeltaTime * num31;
					float num33 = (firedProjectile.entityDistance = hitEntity.Distance(hitInfo.HitPositionWorld));
					if (num33 > num32)
					{
						string name14 = ((Object)hitInfo.ProjectilePrefab).name;
						string shortPrefabName3 = hitEntity.ShortPrefabName;
						AntiHack.Log(this, AntiHackType.ProjectileHack, string.Format("Entity too far away ({0} on {1} with {2}m > {3}m in {4}s)", new object[5] { name14, shortPrefabName3, num33, num32, entityDeltaTime }));
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "entity_distance");
						valid = false;
					}
				}
				void VerifyParentedPlayerDistance()
				{
					//IL_0024: Unknown result type (might be due to invalid IL or missing references)
					float num30 = hitPlayer.AntiHackPadding() + ConVar.AntiHack.tickhistoryforgiveness;
					float num31 = (firedProjectile.entityDistance = hitPlayer.TickHistoryDistanceParented(hitInfo.HitPositionWorld));
					if (num31 > num30)
					{
						string name14 = ((Object)hitInfo.ProjectilePrefab).name;
						string shortPrefabName3 = hitPlayer.ShortPrefabName;
						AntiHack.Log(this, AntiHackType.ProjectileHack, string.Format("Parented player too far away ({0} on {1} with {2}m > {3}m in {4}s)", new object[5] { name14, shortPrefabName3, num31, num30, entityDeltaTime }));
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "player_distance");
						valid = false;
					}
				}
				void VerifyPlayerDistance()
				{
					//IL_002f: Unknown result type (might be due to invalid IL or missing references)
					float num30 = hitPlayer.AntiHackPadding() + ConVar.AntiHack.tickhistoryforgiveness;
					float num31 = (firedProjectile.entityDistance = hitPlayer.tickHistory.Distance(hitPlayer, hitInfo.HitPositionWorld));
					if (num31 > num30)
					{
						string name14 = ((Object)hitInfo.ProjectilePrefab).name;
						string shortPrefabName3 = hitPlayer.ShortPrefabName;
						AntiHack.Log(this, AntiHackType.ProjectileHack, string.Format("Player too far away ({0} on {1} with {2}m > {3}m in {4}s)", new object[5] { name14, shortPrefabName3, num31, num30, entityDeltaTime }));
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "player_distance");
						valid = false;
					}
				}
			}
			finally
			{
				if (hitInfo != null)
				{
					((IDisposable)hitInfo).Dispose();
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	public void OnProjectileRicochet(RPCMessage msg)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		PlayerProjectileRicochet val = msg.read.Proto<PlayerProjectileRicochet>((PlayerProjectileRicochet)null);
		try
		{
			if (val != null)
			{
				FiredProjectile value;
				if (Vector3Ex.IsNaNOrInfinity(val.hitPosition) || Vector3Ex.IsNaNOrInfinity(val.inVelocity) || Vector3Ex.IsNaNOrInfinity(val.outVelocity) || Vector3Ex.IsNaNOrInfinity(val.hitNormal) || float.IsNaN(val.travelTime) || float.IsInfinity(val.travelTime))
				{
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Contains NaN ({val.projectileID})");
				}
				else if (!firedProjectiles.TryGetValue(val.projectileID, out value))
				{
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Missing ID ({val.projectileID})", logToAnalytics: false);
				}
				else if (value.firedTime < Time.realtimeSinceStartup - 8f)
				{
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Lifetime is zero ({val.projectileID})");
				}
				else if (Interface.CallHook("OnProjectileRicochet", this, val) == null)
				{
					value.ricochets++;
					firedProjectiles[val.projectileID] = value;
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	public void OnProjectileUpdate(RPCMessage msg)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_071f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0724: Unknown result type (might be due to invalid IL or missing references)
		//IL_072b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0730: Unknown result type (might be due to invalid IL or missing references)
		//IL_0757: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0508: Unknown result type (might be due to invalid IL or missing references)
		//IL_050d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		//IL_0515: Unknown result type (might be due to invalid IL or missing references)
		//IL_0517: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05da: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0603: Unknown result type (might be due to invalid IL or missing references)
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		//IL_061d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_054a: Unknown result type (might be due to invalid IL or missing references)
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Unknown result type (might be due to invalid IL or missing references)
		//IL_0553: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_052a: Unknown result type (might be due to invalid IL or missing references)
		//IL_052c: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_0541: Unknown result type (might be due to invalid IL or missing references)
		//IL_0546: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_0651: Unknown result type (might be due to invalid IL or missing references)
		//IL_0656: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0586: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0663: Unknown result type (might be due to invalid IL or missing references)
		//IL_0668: Unknown result type (might be due to invalid IL or missing references)
		//IL_066a: Unknown result type (might be due to invalid IL or missing references)
		//IL_066f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0672: Unknown result type (might be due to invalid IL or missing references)
		//IL_0677: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_042e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		PlayerProjectileUpdate val = msg.read.Proto<PlayerProjectileUpdate>((PlayerProjectileUpdate)null);
		try
		{
			if (val == null)
			{
				return;
			}
			if (Vector3Ex.IsNaNOrInfinity(val.curPosition) || Vector3Ex.IsNaNOrInfinity(val.curVelocity) || float.IsNaN(val.travelTime) || float.IsInfinity(val.travelTime))
			{
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Contains NaN ({val.projectileID})");
				return;
			}
			if (!firedProjectiles.TryGetValue(val.projectileID, out var value))
			{
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Missing ID ({val.projectileID})", logToAnalytics: false);
				return;
			}
			if (value.firedTime < Time.realtimeSinceStartup - 8f)
			{
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Lifetime is zero ({val.projectileID})");
				Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(value);
				return;
			}
			if (value.ricochets > 0)
			{
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile update is ricochet ({val.projectileID})");
				Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(value);
				return;
			}
			Vector3 position = value.position;
			Vector3 positionOffset = value.positionOffset;
			Vector3 velocity = value.velocity;
			float num = value.trajectoryMismatch;
			float partialTime = value.partialTime;
			float travelTime = value.travelTime;
			float num2 = Mathf.Clamp(val.travelTime, value.travelTime, 8f);
			Vector3 val2 = Physics.gravity * value.projectilePrefab.gravityModifier;
			float drag = value.projectilePrefab.drag;
			if (value.protection > 0)
			{
				float num3 = 1f - ConVar.AntiHack.projectile_forgiveness;
				float num4 = 1f + ConVar.AntiHack.projectile_forgiveness;
				float projectile_clientframes = ConVar.AntiHack.projectile_clientframes;
				float projectile_serverframes = ConVar.AntiHack.projectile_serverframes;
				float num5 = Mathx.Decrement(value.firedTime);
				float num6 = Mathf.Clamp(Mathx.Increment(Time.realtimeSinceStartup) - num5, 0f, 8f);
				float num7 = num2;
				float num8 = (value.desyncLifeTime = Mathf.Abs(num6 - num7));
				float num9 = Mathf.Min(num6, num7);
				float num10 = projectile_clientframes / 60f;
				float num11 = projectile_serverframes * Mathx.Max(Time.deltaTime, Time.smoothDeltaTime, Time.fixedDeltaTime);
				float num12 = (num9 + desyncTimeClamped + num10 + num11) * num4;
				float num13 = Mathf.Max(0f, (num9 - desyncTimeClamped - num10 - num11) * num3);
				int num14 = 1075904512;
				if (ConVar.AntiHack.projectile_terraincheck)
				{
					num14 |= 0x800000;
				}
				if (ConVar.AntiHack.projectile_vehiclecheck)
				{
					num14 |= 0x8000000;
				}
				if (value.protection >= 1)
				{
					float num15 = value.projectilePrefab.initialDistance + num12 * ((Vector3)(ref value.initialVelocity)).magnitude;
					float num16 = Vector3.Distance(value.initialPosition, val.curPosition);
					if (num16 > num15)
					{
						string name = ((Object)value.projectilePrefab).name;
						AntiHack.Log(this, AntiHackType.ProjectileHack, string.Format("Projectile distance ({0} with {1}m > {2}m in {3}s)", new object[4] { name, num16, num15, num12 }));
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(value);
						return;
					}
					if (num8 > ConVar.AntiHack.projectile_desync)
					{
						string name2 = ((Object)value.projectilePrefab).name;
						AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile desync ({name2} with {num8}s > {ConVar.AntiHack.projectile_desync}s)");
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(value);
						return;
					}
					Vector3 curVelocity = val.curVelocity;
					Vector3 val3 = value.initialVelocity;
					Vector3 val4 = ((value.hits == 0) ? val3 : value.velocity);
					float num17 = drag * (1f / 32f);
					Vector3 val5 = val2 * (1f / 32f);
					int num18 = Mathf.FloorToInt(num13 / (1f / 32f));
					int num19 = Mathf.CeilToInt(num12 / (1f / 32f));
					for (int i = 0; i < num18; i++)
					{
						val3 += val5;
						val3 -= val3 * num17;
						val4 += val5;
						val4 -= val4 * num17;
					}
					float magnitude = ((Vector3)(ref curVelocity)).magnitude;
					float num20 = ((Vector3)(ref val3)).magnitude;
					float num21 = ((Vector3)(ref val4)).magnitude;
					for (int j = num18; j < num19; j++)
					{
						val3 += val5;
						val3 -= val3 * num17;
						val4 += val5;
						val4 -= val4 * num17;
						num21 = Mathf.Min(num21, ((Vector3)(ref val4)).magnitude);
						num20 = Mathf.Max(num20, ((Vector3)(ref val3)).magnitude);
					}
					if (magnitude < num21 * num3)
					{
						string name3 = ((Object)value.projectilePrefab).name;
						AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile velocity too low ({name3} with {magnitude} < {num21})");
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(value);
						return;
					}
					if (magnitude > num20 * num4)
					{
						string name4 = ((Object)value.projectilePrefab).name;
						AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile velocity too high ({name4} with {magnitude} > {num20})");
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(value);
						return;
					}
				}
				if (value.protection >= 3)
				{
					Vector3 position2 = value.position;
					Vector3 curPosition = val.curPosition;
					Vector3 val6 = Vector3.zero;
					if (ConVar.AntiHack.projectile_backtracking > 0f)
					{
						Vector3 val7 = curPosition - position2;
						val6 = ((Vector3)(ref val7)).normalized * ConVar.AntiHack.projectile_backtracking;
					}
					if (!GamePhysics.LineOfSight(position2 - val6, curPosition + val6, num14, value.lastEntityHit))
					{
						string name5 = ((Object)value.projectilePrefab).name;
						AntiHack.Log(this, AntiHackType.ProjectileHack, $"Line of sight ({name5} on update) {position2} {curPosition}");
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(value);
						return;
					}
				}
				if (value.protection >= 4)
				{
					SimulateProjectile(ref position, ref velocity, ref partialTime, num2 - travelTime, val2, drag, out var prevPosition, out var prevVelocity);
					value.simulatedPositions.Add(position);
					Line val8 = default(Line);
					((Line)(ref val8))._002Ector(prevPosition - prevVelocity, prevPosition);
					Line val9 = default(Line);
					((Line)(ref val9))._002Ector(prevPosition, position);
					Line val10 = default(Line);
					((Line)(ref val10))._002Ector(position, position + velocity);
					float num22 = Mathx.Min(((Line)(ref val8)).Distance(val.curPosition), ((Line)(ref val9)).Distance(val.curPosition), ((Line)(ref val10)).Distance(val.curPosition));
					num += Mathf.Max(num22 - ((Vector3)(ref positionOffset)).magnitude, 0f);
				}
				if (value.protection >= 5)
				{
					if (value.inheritedVelocity != Vector3.zero)
					{
						Vector3 curVelocity2 = value.inheritedVelocity + velocity;
						Vector3 curVelocity3 = val.curVelocity;
						if (((Vector3)(ref curVelocity3)).magnitude > 2f * ((Vector3)(ref curVelocity2)).magnitude || ((Vector3)(ref curVelocity3)).magnitude < 0.5f * ((Vector3)(ref curVelocity2)).magnitude)
						{
							val.curVelocity = curVelocity2;
						}
						value.inheritedVelocity = Vector3.zero;
					}
					else
					{
						val.curVelocity = velocity;
					}
				}
			}
			value.updates.Add(new FiredProjectileUpdate
			{
				OldPosition = value.position,
				NewPosition = val.curPosition,
				OldVelocity = value.velocity,
				NewVelocity = val.curVelocity,
				Mismatch = num,
				PartialTime = partialTime
			});
			value.position = val.curPosition;
			value.velocity = val.curVelocity;
			value.travelTime = val.travelTime;
			value.partialTime = partialTime;
			value.trajectoryMismatch = num;
			value.positionOffset = default(Vector3);
			firedProjectiles[val.projectileID] = value;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void SimulateProjectile(ref Vector3 position, ref Vector3 velocity, ref float partialTime, float travelTime, Vector3 gravity, float drag, out Vector3 prevPosition, out Vector3 prevVelocity)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f / 32f;
		prevPosition = position;
		prevVelocity = velocity;
		if (partialTime > Mathf.Epsilon)
		{
			float num2 = num - partialTime;
			if (travelTime < num2)
			{
				prevPosition = position;
				prevVelocity = velocity;
				position += velocity * travelTime;
				partialTime += travelTime;
				return;
			}
			prevPosition = position;
			prevVelocity = velocity;
			position += velocity * num2;
			velocity += gravity * num;
			velocity -= velocity * (drag * num);
			travelTime -= num2;
		}
		int num3 = Mathf.FloorToInt(travelTime / num);
		for (int i = 0; i < num3; i++)
		{
			prevPosition = position;
			prevVelocity = velocity;
			position += velocity * num;
			velocity += gravity * num;
			velocity -= velocity * (drag * num);
		}
		partialTime = travelTime - num * (float)num3;
		if (partialTime > Mathf.Epsilon)
		{
			prevPosition = position;
			prevVelocity = velocity;
			position += velocity * partialTime;
		}
	}

	protected virtual void CreateWorldProjectile(HitInfo info, ItemDefinition itemDef, ItemModProjectile itemMod, Projectile projectilePrefab, Item recycleItem)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("CanCreateWorldProjectile", info, itemDef) != null)
		{
			return;
		}
		Vector3 projectileVelocity = info.ProjectileVelocity;
		Item item = ((recycleItem != null) ? recycleItem : ItemManager.Create(itemDef, 1, 0uL, isServerSide: true, 0uL));
		if (Interface.CallHook("OnWorldProjectileCreate", info, item) != null)
		{
			return;
		}
		BaseEntity baseEntity = null;
		if (!info.DidHit)
		{
			baseEntity = item.CreateWorldObject(info.HitPositionWorld, Quaternion.LookRotation(((Vector3)(ref projectileVelocity)).normalized));
			baseEntity.Kill(DestroyMode.Gib);
			return;
		}
		if (projectilePrefab.breakProbability > 0f && Random.value <= projectilePrefab.breakProbability)
		{
			baseEntity = item.CreateWorldObject(info.HitPositionWorld, Quaternion.LookRotation(((Vector3)(ref projectileVelocity)).normalized));
			baseEntity.Kill(DestroyMode.Gib);
			return;
		}
		if (projectilePrefab.conditionLoss > 0f)
		{
			item.LoseCondition(projectilePrefab.conditionLoss * 100f);
			if (item.isBroken)
			{
				baseEntity = item.CreateWorldObject(info.HitPositionWorld, Quaternion.LookRotation(((Vector3)(ref projectileVelocity)).normalized));
				baseEntity.Kill(DestroyMode.Gib);
				return;
			}
		}
		if (projectilePrefab.stickProbability > 0f && Random.value <= projectilePrefab.stickProbability)
		{
			baseEntity = (((Object)(object)info.HitEntity == (Object)null) ? item.CreateWorldObject(info.HitPositionWorld, Quaternion.LookRotation(((Vector3)(ref projectileVelocity)).normalized)) : ((info.HitBone != 0) ? item.CreateWorldObject(info.HitPositionLocal, Quaternion.LookRotation(info.HitNormalLocal * -1f), info.HitEntity, info.HitBone) : item.CreateWorldObject(info.HitPositionLocal, Quaternion.LookRotation(((Component)info.HitEntity).transform.InverseTransformDirection(((Vector3)(ref projectileVelocity)).normalized)), info.HitEntity)));
			DroppedItem droppedItem = baseEntity as DroppedItem;
			if ((Object)(object)droppedItem != (Object)null)
			{
				droppedItem.StickIn();
			}
			else
			{
				((Component)baseEntity).GetComponent<Rigidbody>().isKinematic = true;
			}
		}
		else
		{
			baseEntity = item.CreateWorldObject(info.HitPositionWorld, Quaternion.LookRotation(((Vector3)(ref projectileVelocity)).normalized));
			Rigidbody component = ((Component)baseEntity).GetComponent<Rigidbody>();
			component.AddForce(((Vector3)(ref projectileVelocity)).normalized * 200f);
			component.WakeUp();
		}
	}

	public void CleanupExpiredProjectiles()
	{
		foreach (KeyValuePair<int, FiredProjectile> item in firedProjectiles.Where((KeyValuePair<int, FiredProjectile> x) => x.Value.firedTime < Time.realtimeSinceStartup - 8f - 1f).ToList())
		{
			Facepunch.Rust.Analytics.Azure.OnFiredProjectileRemoved(this, item.Value);
			firedProjectiles.Remove(item.Key);
			FiredProjectile value = item.Value;
			Pool.Free<FiredProjectile>(ref value);
		}
	}

	public bool HasFiredProjectile(int id)
	{
		return firedProjectiles.ContainsKey(id);
	}

	public void NoteFiredProjectile(int projectileid, Vector3 startPos, Vector3 startVel, AttackEntity attackEnt, ItemDefinition firedItemDef, Guid projectileGroupId, Vector3 positionOffset, Item pickupItem = null)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		BaseProjectile baseProjectile = attackEnt as BaseProjectile;
		ItemModProjectile component = ((Component)firedItemDef).GetComponent<ItemModProjectile>();
		Projectile component2 = component.GetOverrideProjectile(baseProjectile).Get().GetComponent<Projectile>();
		if (Vector3Ex.IsNaNOrInfinity(startPos) || Vector3Ex.IsNaNOrInfinity(startVel))
		{
			string name = ((Object)component2).name;
			AntiHack.Log(this, AntiHackType.ProjectileHack, "Contains NaN (" + name + ")");
			stats.combat.LogInvalid(this, baseProjectile, "projectile_nan");
			return;
		}
		int projectile_protection = ConVar.AntiHack.projectile_protection;
		Vector3 inheritedVelocity = (((Object)(object)attackEnt != (Object)null) ? attackEnt.GetInheritedVelocity(this, ((Vector3)(ref startVel)).normalized) : Vector3.zero);
		if (projectile_protection >= 1)
		{
			float num = 1f - ConVar.AntiHack.projectile_forgiveness;
			float num2 = 1f + ConVar.AntiHack.projectile_forgiveness;
			float magnitude = ((Vector3)(ref startVel)).magnitude;
			float num3 = component.GetMinVelocity();
			float num4 = component.GetMaxVelocity();
			BaseProjectile baseProjectile2 = attackEnt as BaseProjectile;
			if (Object.op_Implicit((Object)(object)baseProjectile2))
			{
				num3 *= baseProjectile2.GetProjectileVelocityScale();
				num4 *= baseProjectile2.GetProjectileVelocityScale(getMax: true);
			}
			num3 *= num;
			num4 *= num2;
			if (magnitude < num3)
			{
				string name2 = ((Object)component2).name;
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Velocity ({name2} with {magnitude} < {num3})");
				stats.combat.LogInvalid(this, baseProjectile, "projectile_minvelocity");
				return;
			}
			if (magnitude > num4)
			{
				string name3 = ((Object)component2).name;
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Velocity ({name3} with {magnitude} > {num4})");
				stats.combat.LogInvalid(this, baseProjectile, "projectile_maxvelocity");
				return;
			}
		}
		FiredProjectile firedProjectile = Pool.Get<FiredProjectile>();
		firedProjectile.itemDef = firedItemDef;
		firedProjectile.itemMod = component;
		firedProjectile.projectilePrefab = component2;
		firedProjectile.firedTime = Time.realtimeSinceStartup;
		firedProjectile.travelTime = 0f;
		firedProjectile.weaponSource = attackEnt;
		firedProjectile.weaponPrefab = (((Object)(object)attackEnt == (Object)null) ? null : GameManager.server.FindPrefab(StringPool.Get(attackEnt.prefabID)).GetComponent<AttackEntity>());
		firedProjectile.projectileModifier = (((Object)(object)baseProjectile == (Object)null) ? Projectile.Modifier.Default : baseProjectile.GetProjectileModifier());
		firedProjectile.pickupItem = pickupItem;
		firedProjectile.integrity = 1f;
		firedProjectile.position = startPos;
		firedProjectile.initialPositionOffset = positionOffset;
		firedProjectile.positionOffset = positionOffset;
		firedProjectile.velocity = startVel;
		firedProjectile.initialPosition = startPos;
		firedProjectile.initialVelocity = startVel;
		firedProjectile.inheritedVelocity = inheritedVelocity;
		firedProjectile.protection = projectile_protection;
		firedProjectile.ricochets = 0;
		firedProjectile.hits = 0;
		firedProjectile.id = projectileid;
		firedProjectile.attacker = this;
		firedProjectile.simulatedPositions.Add(startPos);
		firedProjectiles.Add(projectileid, firedProjectile);
		Facepunch.Rust.Analytics.Azure.OnFiredProjectile(this, firedProjectile, projectileGroupId);
	}

	public void ServerNoteFiredProjectile(int projectileid, Vector3 startPos, Vector3 startVel, AttackEntity attackEnt, ItemDefinition firedItemDef, Item pickupItem = null)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		BaseProjectile baseProjectile = attackEnt as BaseProjectile;
		ItemModProjectile component = ((Component)firedItemDef).GetComponent<ItemModProjectile>();
		Projectile component2 = component.GetOverrideProjectile(baseProjectile).Get().GetComponent<Projectile>();
		int protection = 0;
		Vector3 zero = Vector3.zero;
		if (!Vector3Ex.IsNaNOrInfinity(startPos) && !Vector3Ex.IsNaNOrInfinity(startVel))
		{
			FiredProjectile firedProjectile = Pool.Get<FiredProjectile>();
			firedProjectile.itemDef = firedItemDef;
			firedProjectile.itemMod = component;
			firedProjectile.projectilePrefab = component2;
			firedProjectile.firedTime = Time.realtimeSinceStartup;
			firedProjectile.travelTime = 0f;
			firedProjectile.weaponSource = attackEnt;
			firedProjectile.weaponPrefab = (((Object)(object)attackEnt == (Object)null) ? null : GameManager.server.FindPrefab(StringPool.Get(attackEnt.prefabID)).GetComponent<AttackEntity>());
			firedProjectile.projectileModifier = (((Object)(object)baseProjectile == (Object)null) ? Projectile.Modifier.Default : baseProjectile.GetProjectileModifier());
			firedProjectile.pickupItem = pickupItem;
			firedProjectile.integrity = 1f;
			firedProjectile.trajectoryMismatch = 0f;
			firedProjectile.position = startPos;
			firedProjectile.positionOffset = Vector3.zero;
			firedProjectile.velocity = startVel;
			firedProjectile.initialPosition = startPos;
			firedProjectile.initialVelocity = startVel;
			firedProjectile.inheritedVelocity = zero;
			firedProjectile.protection = protection;
			firedProjectile.ricochets = 0;
			firedProjectile.hits = 0;
			firedProjectile.id = projectileid;
			firedProjectile.attacker = this;
			firedProjectiles.Add(projectileid, firedProjectile);
		}
	}

	public void ApplyRadiation(float radsAmount, bool protection = true)
	{
		if (IsAlive() && !IsSleeping() && !InSafeZone())
		{
			float num = 0f;
			num = (protection ? Radiation.GetRadiationAfterProtection(radsAmount, RadiationProtection()) : Mathf.Max(0f, radsAmount));
			metabolism.ApplyChange(MetabolismAttribute.Type.Radiation, num, 0f);
		}
	}

	public void PlayerInventoryRadioactivityChange(float radAmount, bool hasRads)
	{
		if (!Radiation.water_inventory_damage)
		{
			return;
		}
		if (inflictInventoryRadsAction == null)
		{
			inflictInventoryRadsAction = InflictRadsFromInventory;
		}
		inventoryRads = radAmount;
		if (!hasRads || radAmount < 2500f)
		{
			if (IsInvoking(inflictInventoryRadsAction))
			{
				CancelInvoke(inflictInventoryRadsAction);
			}
		}
		else if (!IsInvoking(inflictInventoryRadsAction))
		{
			InvokeRepeating(inflictInventoryRadsAction, 1f, 1f);
		}
	}

	private void InflictRadsFromInventory()
	{
		if (Radiation.water_inventory_damage)
		{
			float num = inventoryRads * Radiation.MaterialToRadsRatio;
			num *= 0.05f;
			ApplyRadiation(num);
		}
	}

	public void RadioactiveLootCheck(List<ItemContainer> containerRefs)
	{
		radiationCheckContainers.Clear();
		radiationCheckContainers.AddRange(containerRefs);
		HasOpenedLoot();
	}

	private void HasOpenedLoot()
	{
		if (Radiation.water_loot_damage)
		{
			hasOpenedLoot = true;
			CheckRadsInContainer();
			InflictRadsFromContainer();
			if (inflictRadsAction == null)
			{
				inflictRadsAction = InflictRadsFromContainer;
			}
			if (checkRadsAction == null)
			{
				checkRadsAction = CheckRadsInContainer;
			}
			if (!IsInvoking(checkRadsAction))
			{
				InvokeRepeating(checkRadsAction, 1f, 2500f);
			}
			if (!IsInvoking(inflictRadsAction))
			{
				InvokeRepeating(inflictRadsAction, 1f, 1f);
			}
		}
	}

	public void HasClosedLoot()
	{
		if (IsInvoking(inflictRadsAction))
		{
			CancelInvoke(inflictRadsAction);
		}
		hasOpenedLoot = false;
	}

	private void InflictRadsFromContainer()
	{
		if (!Radiation.water_loot_damage)
		{
			return;
		}
		if (!hasOpenedLoot)
		{
			if (IsInvoking(checkRadsAction))
			{
				CancelInvoke(checkRadsAction);
			}
			if (IsInvoking(inflictRadsAction))
			{
				CancelInvoke(inflictRadsAction);
			}
		}
		else
		{
			ApplyRadiation(containerRads);
		}
	}

	private void CheckRadsInContainer()
	{
		if (!hasOpenedLoot)
		{
			return;
		}
		containerRads = 0f;
		foreach (ItemContainer radiationCheckContainer in radiationCheckContainers)
		{
			containerRads += radiationCheckContainer.GetRadioactiveMaterialInContainer() * Radiation.MaterialToRadsRatio;
		}
		containerRads *= 0.05f;
	}

	public bool IsRagdolling()
	{
		return HasPlayerFlag(PlayerFlags.Ragdolling);
	}

	protected virtual bool AllowRagdoll()
	{
		return true;
	}

	public void Ragdoll(Vector3 velocityOverride = default(Vector3), bool matchPlayerGravity = true, bool flailInAir = false, bool dieOnImpact = false, BaseEntity initiator = null)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!Physics.allowplayertempragdoll)
		{
			EnsureDismounted();
		}
		else if (!UsedAdminCheat() && AllowRagdoll())
		{
			BaseRagdoll baseRagdoll = CreateRagdoll(((Component)this).transform.position, ((Component)this).transform.rotation, velocityOverride, matchPlayerGravity, flailInAir, dieOnImpact, initiator);
			EnsureDismounted();
			baseRagdoll.AttemptMount(this, doMountChecks: false);
			if (mounted.Get(serverside: true) is BaseRagdoll)
			{
				SetPlayerFlag(PlayerFlags.Ragdolling, b: true);
			}
			SendNetworkUpdateImmediate();
		}
	}

	private BaseRagdoll CreateRagdoll(Vector3 position, Quaternion rotation, Vector3 velocityOverride, bool matchPlayerGravity, bool flailInAir, bool dieOnImpact, BaseEntity initiator)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		BaseRagdoll baseRagdoll = GameManager.server.CreateEntity("assets/prefabs/player/player_temp_ragdoll.prefab") as BaseRagdoll;
		((Component)baseRagdoll).transform.SetPositionAndRotation(position, rotation);
		Ragdoll component = ((Component)baseRagdoll).GetComponent<Ragdoll>();
		if ((Object)(object)component != (Object)null)
		{
			component.simOnServer = true;
		}
		baseRagdoll.InitFromPlayer(this, velocityOverride, matchPlayerGravity, flailInAir, dieOnImpact, initiator);
		baseRagdoll.Spawn();
		BaseMountable baseMountable = GetMounted();
		if (Object.op_Implicit((Object)(object)baseMountable))
		{
			GameObjectExtensions.SetIgnoreCollisions(((Component)baseRagdoll).gameObject, ((Component)baseMountable).gameObject, true);
		}
		return baseRagdoll;
	}

	public override bool CanUseNetworkCache(Network.Connection connection)
	{
		if (net == null)
		{
			return true;
		}
		if (connection.authLevel != 0)
		{
			return false;
		}
		if (net.connection != connection)
		{
			return true;
		}
		return false;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		HandleMountedOnLoad();
		if (ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion())
		{
			OcclusionInitGroup(canBeInAGroup: true);
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0564: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		BasePlayer basePlayer = ((info.forConnection != null && info.forConnection.player is BasePlayer basePlayer2) ? basePlayer2 : null);
		bool flag = (Object)(object)basePlayer != (Object)null;
		bool flag2 = net != null && net.connection == info.forConnection;
		bool flag3 = !info.forDisk && flag && basePlayer.IsAdmin;
		bool flag4 = flag && playersRecordingClientDemos.Contains(basePlayer);
		info.msg.basePlayer = Pool.Get<BasePlayer>();
		info.msg.basePlayer.userid = userID;
		info.msg.basePlayer.name = displayName;
		info.msg.basePlayer.playerFlags = (int)playerFlags;
		info.msg.basePlayer.currentTeam = currentTeam;
		info.msg.basePlayer.heldEntity = svActiveItemID;
		info.msg.basePlayer.reputation = reputation;
		if (!info.forDisk && (Object)(object)currentGesture != (Object)null && currentGesture.animationType == GestureConfig.AnimationType.Loop)
		{
			info.msg.basePlayer.loopingGesture = currentGesture.gestureId;
		}
		if (IsConnected && (IsAdmin || IsDeveloper))
		{
			info.msg.basePlayer.skinCol = net.connection.info.GetFloat("global.skincol", -1f);
			info.msg.basePlayer.skinTex = net.connection.info.GetFloat("global.skintex", -1f);
			info.msg.basePlayer.skinMesh = net.connection.info.GetFloat("global.skinmesh", -1f);
		}
		else
		{
			info.msg.basePlayer.skinCol = -1f;
			info.msg.basePlayer.skinTex = -1f;
			info.msg.basePlayer.skinMesh = -1f;
		}
		info.msg.basePlayer.underwear = GetUnderwearSkin(info.cachedTime.Time);
		info.msg.basePlayer.paintballColor = server_paintballColor;
		if (info.forDisk || flag2 || flag4)
		{
			info.msg.basePlayer.metabolism = metabolism.Save();
			info.msg.basePlayer.modifiers = null;
			if ((Object)(object)modifiers != (Object)null)
			{
				info.msg.basePlayer.modifiers = modifiers.Save(info.forDisk);
			}
		}
		if (!info.forDisk && !flag2)
		{
			BasePlayer basePlayer3 = info.msg.basePlayer;
			basePlayer3.playerFlags &= -5;
			BasePlayer basePlayer4 = info.msg.basePlayer;
			basePlayer4.playerFlags &= -129;
			if (info.msg.baseCombat != null && !flag3)
			{
				info.msg.baseCombat.health = 100f;
				BasePlayer basePlayer5 = info.msg.basePlayer;
				basePlayer5.playerFlags &= -33;
			}
		}
		info.msg.basePlayer.inventory = inventory.Save(info.forDisk || flag2);
		ModelState ms = modelState.Copy();
		UpdateModelState(ms);
		info.msg.basePlayer.modelState = ms;
		if (info.forDisk)
		{
			BaseEntity baseEntity = mounted.Get(base.isServer);
			if (baseEntity.IsValid())
			{
				if (baseEntity.enableSaving)
				{
					info.msg.basePlayer.mounted = mounted.uid;
				}
				else
				{
					BaseVehicle mountedVehicle = GetMountedVehicle();
					if (mountedVehicle.IsValid() && mountedVehicle.enableSaving)
					{
						info.msg.basePlayer.mounted = mountedVehicle.net.ID;
					}
				}
			}
			info.msg.basePlayer.respawnId = respawnId;
		}
		else
		{
			info.msg.basePlayer.mounted = mounted.uid;
		}
		if (flag2)
		{
			info.msg.basePlayer.persistantData = PersistantPlayerInfo.Copy();
			if (!info.forDisk && State.missions != null)
			{
				Missions missions = info.msg.basePlayer.missions;
				if (missions != null)
				{
					missions.Dispose();
				}
				info.msg.basePlayer.missions = State.missions.Copy();
			}
		}
		info.msg.basePlayer.bagCount = SleepingBag.GetSleepingBagCount(userID);
		info.msg.basePlayer.shelterCount = LegacyShelter.GetShelterCount(userID);
		info.msg.basePlayer.bbsCount = BoatBuildingStation.GetBBSCount(userID);
		info.msg.basePlayer.mortarCooldown = ((TimeUntil)(ref mortarCooldown)).LeftFrom(info.cachedTime.Time);
		if (info.forDisk)
		{
			info.msg.basePlayer.loadingTimeout = RealTimeUntil.op_Implicit(timeUntilLoadingExpires);
			info.msg.basePlayer.currentLife = lifeStory;
			info.msg.basePlayer.previousLife = previousLifeStory;
		}
		if (!info.forDisk)
		{
			info.msg.basePlayer.clanId = clanId;
		}
		if (info.forDisk && (Object)(object)inventory.crafting != (Object)null)
		{
			info.msg.basePlayer.itemCrafter = inventory.crafting.Save();
		}
		if (info.forDisk && !IsBot)
		{
			SavePlayerState();
		}
		info.msg.basePlayer.tutorialAllowance = (int)CurrentTutorialAllowance;
	}

	public override void Load(LoadInfo info)
	{
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.basePlayer == null)
		{
			return;
		}
		BasePlayer basePlayer = info.msg.basePlayer;
		if (info.fromDisk && IsBot && (ulong)userID != basePlayer.userid)
		{
			freeBotIds.Add(userID);
		}
		userID = basePlayer.userid;
		UserIDString = userID.Get().ToString();
		if (basePlayer.name != null)
		{
			displayName = basePlayer.name;
		}
		_ = playerFlags;
		playerFlags = (PlayerFlags)basePlayer.playerFlags;
		currentTeam = basePlayer.currentTeam;
		reputation = basePlayer.reputation;
		if (basePlayer.modifiers != null && (Object)(object)modifiers != (Object)null)
		{
			modifiers.Load(basePlayer.modifiers, info.fromDisk);
		}
		if (basePlayer.metabolism != null)
		{
			metabolism.Load(basePlayer.metabolism);
		}
		if (basePlayer.inventory != null)
		{
			inventory.Load(basePlayer.inventory);
		}
		if (basePlayer.modelState != null)
		{
			if (modelState != null)
			{
				modelState.ResetToPool();
				modelState = null;
			}
			modelState = basePlayer.modelState;
			basePlayer.modelState = null;
		}
		if (info.fromDisk)
		{
			timeUntilLoadingExpires = RealTimeUntil.op_Implicit(info.msg.basePlayer.loadingTimeout);
			if (RealTimeUntil.op_Implicit(timeUntilLoadingExpires) > 0f)
			{
				float time = Mathf.Clamp(RealTimeUntil.op_Implicit(timeUntilLoadingExpires), 0f, Nexus.loadingTimeout);
				Invoke(RemoveLoadingPlayerFlag, time);
			}
			lifeStory = info.msg.basePlayer.currentLife;
			if (lifeStory != null)
			{
				lifeStory.ShouldPool = false;
			}
			previousLifeStory = info.msg.basePlayer.previousLife;
			if (previousLifeStory != null)
			{
				previousLifeStory.ShouldPool = false;
			}
			SetPlayerFlag(PlayerFlags.Sleeping, b: false);
			StartSleeping();
			SetPlayerFlag(PlayerFlags.Connected, b: false);
			if (lifeStory == null && IsAlive())
			{
				LifeStoryStart();
			}
			mounted.uid = info.msg.basePlayer.mounted;
			if (IsWounded())
			{
				Die();
			}
			respawnId = info.msg.basePlayer.respawnId;
			if (info.msg.basePlayer.itemCrafter?.queue != null)
			{
				inventory.crafting.Load(info.msg.basePlayer.itemCrafter);
			}
			server_paintballColor = info.msg.basePlayer.paintballColor;
		}
		if (!info.fromDisk)
		{
			clanId = info.msg.basePlayer.clanId;
		}
		CurrentTutorialAllowance = (TutorialItemAllowance)info.msg.basePlayer.tutorialAllowance;
		mortarCooldown = TimeUntil.op_Implicit(info.msg.basePlayer.mortarCooldown);
	}

	internal void LifeStoryStart()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		if (lifeStory != null)
		{
			lifeStory = null;
		}
		lifeStory = new PlayerLifeStory
		{
			ShouldPool = false,
			wipeId = SaveRestore.WipeId
		};
		lifeStory.timeBorn = (uint)Epoch.Current;
		hasSentPresenceState = false;
	}

	public void LifeStoryEnd()
	{
		SingletonComponent<ServerMgr>.Instance.persistance.AddLifeStory(userID, lifeStory);
		if (lifeStory != null)
		{
			Facepunch.Rust.Analytics.Azure.OnPlayerLifeStoryEnd(this, lifeStory);
		}
		previousLifeStory = lifeStory;
		lifeStory = null;
	}

	internal void LifeStoryUpdate(float deltaTime, float moveSpeed)
	{
		if (lifeStory != null)
		{
			PlayerLifeStory obj = lifeStory;
			obj.secondsAlive += deltaTime;
			nextTimeCategoryUpdate -= deltaTime * ((moveSpeed > 0.1f) ? 1f : 0.25f);
			if (nextTimeCategoryUpdate <= 0f && !waitingForLifeStoryUpdate)
			{
				nextTimeCategoryUpdate = 7f + 7f * Random.Range(0.2f, 1f);
				waitingForLifeStoryUpdate = true;
				((ObjectWorkQueue<BasePlayer>)lifeStoryQueue).Add(this);
			}
			if (LifeStoryInWilderness)
			{
				PlayerLifeStory obj2 = lifeStory;
				obj2.secondsWilderness += deltaTime;
			}
			if (LifeStoryInMonument)
			{
				PlayerLifeStory obj3 = lifeStory;
				obj3.secondsInMonument += deltaTime;
			}
			if (LifeStoryInBase)
			{
				PlayerLifeStory obj4 = lifeStory;
				obj4.secondsInBase += deltaTime;
			}
			if (LifeStoryFlying)
			{
				PlayerLifeStory obj5 = lifeStory;
				obj5.secondsFlying += deltaTime;
			}
			if (LifeStoryBoating)
			{
				PlayerLifeStory obj6 = lifeStory;
				obj6.secondsBoating += deltaTime;
			}
			if (LifeStorySwimming)
			{
				PlayerLifeStory obj7 = lifeStory;
				obj7.secondsSwimming += deltaTime;
			}
			if (LifeStoryDriving)
			{
				PlayerLifeStory obj8 = lifeStory;
				obj8.secondsDriving += deltaTime;
			}
			if (IsSleeping())
			{
				PlayerLifeStory obj9 = lifeStory;
				obj9.secondsSleeping += deltaTime;
			}
			else if (IsRunning())
			{
				PlayerLifeStory obj10 = lifeStory;
				obj10.metersRun += moveSpeed * deltaTime;
			}
			else
			{
				PlayerLifeStory obj11 = lifeStory;
				obj11.metersWalked += moveSpeed * deltaTime;
			}
		}
	}

	private static void LifeStoryUpdate(in PlayerServerStates.ReadOnly playerStates, float deltaTime)
	{
		using (TimeWarning.New("LifeStoryUpdate"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			ReadOnlySpan<Flag> readOnlySpan = ReadOnly<Flag>.op_Implicit(ref playerStates.PlayerModelStateFlags);
			ReadOnlySpan<BasePlayer> readOnlySpan2 = objects;
			for (int i = 0; i < readOnlySpan2.Length; i++)
			{
				BasePlayer basePlayer = readOnlySpan2[i];
				bool flag = ((uint)readOnlySpan[basePlayer.ActivePlayerInd] & 4u) != 0;
				basePlayer.LifeStoryUpdate(deltaTime, flag ? basePlayer.estimatedSpeed : 0f);
			}
		}
	}

	public void UpdateTimeCategory()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("UpdateTimeCategory"))
		{
			waitingForLifeStoryUpdate = false;
			int num = currentTimeCategory;
			currentTimeCategory = 1;
			if (IsBuildingAuthed(cached: true, 45f))
			{
				currentTimeCategory = 4;
			}
			Vector3 position = ((Component)this).transform.position;
			if ((Object)(object)TerrainMeta.TopologyMap != (Object)null && (TerrainMeta.TopologyMap.GetTopology(position) & 0x400) != 0 && (Object)(object)TerrainMeta.Path != (Object)null)
			{
				foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
				{
					if (monument.shouldDisplayOnMap && monument.IsInBounds(position))
					{
						currentTimeCategory = 2;
						break;
					}
				}
			}
			if (IsSwimming())
			{
				currentTimeCategory |= 32;
			}
			if (isMounted)
			{
				BaseMountable baseMountable = GetMounted();
				if (baseMountable.mountTimeStatType == BaseMountable.MountStatType.Boating)
				{
					currentTimeCategory |= 16;
				}
				else if (baseMountable.mountTimeStatType == BaseMountable.MountStatType.Flying)
				{
					currentTimeCategory |= 8;
				}
				else if (baseMountable.mountTimeStatType == BaseMountable.MountStatType.Driving)
				{
					currentTimeCategory |= 64;
				}
			}
			else if (HasParent() && GetParentEntity() is BaseMountable baseMountable2)
			{
				if (baseMountable2.mountTimeStatType == BaseMountable.MountStatType.Boating)
				{
					currentTimeCategory |= 16;
				}
				else if (baseMountable2.mountTimeStatType == BaseMountable.MountStatType.Flying)
				{
					currentTimeCategory |= 8;
				}
				else if (baseMountable2.mountTimeStatType == BaseMountable.MountStatType.Driving)
				{
					currentTimeCategory |= 64;
				}
			}
			if (num != currentTimeCategory || !hasSentPresenceState)
			{
				LifeStoryInWilderness = (1 & currentTimeCategory) != 0;
				LifeStoryInMonument = (2 & currentTimeCategory) != 0;
				LifeStoryInBase = (4 & currentTimeCategory) != 0;
				LifeStoryFlying = (8 & currentTimeCategory) != 0;
				LifeStoryBoating = (0x10 & currentTimeCategory) != 0;
				LifeStorySwimming = (0x20 & currentTimeCategory) != 0;
				LifeStoryDriving = (0x40 & currentTimeCategory) != 0;
				ClientRPC(RpcTarget.Player("UpdateRichPresenceState", this), currentTimeCategory);
				hasSentPresenceState = true;
			}
		}
	}

	public void LifeStoryShotFired(BaseEntity withWeapon)
	{
		if (lifeStory == null)
		{
			return;
		}
		if (lifeStory.weaponStats == null)
		{
			lifeStory.weaponStats = Pool.Get<List<WeaponStats>>();
		}
		foreach (WeaponStats weaponStat in lifeStory.weaponStats)
		{
			if (weaponStat.weaponName == withWeapon.ShortPrefabName)
			{
				weaponStat.shotsFired++;
				return;
			}
		}
		WeaponStats val = Pool.Get<WeaponStats>();
		val.weaponName = withWeapon.ShortPrefabName;
		val.shotsFired++;
		lifeStory.weaponStats.Add(val);
	}

	public void LifeStoryShotHit(BaseEntity withWeapon)
	{
		if (lifeStory == null || (Object)(object)withWeapon == (Object)null)
		{
			return;
		}
		if (lifeStory.weaponStats == null)
		{
			lifeStory.weaponStats = Pool.Get<List<WeaponStats>>();
		}
		foreach (WeaponStats weaponStat in lifeStory.weaponStats)
		{
			if (weaponStat.weaponName == withWeapon.ShortPrefabName)
			{
				weaponStat.shotsHit++;
				return;
			}
		}
		WeaponStats val = Pool.Get<WeaponStats>();
		val.weaponName = withWeapon.ShortPrefabName;
		val.shotsHit++;
		lifeStory.weaponStats.Add(val);
	}

	public void LifeStoryKill(BaseCombatEntity killed)
	{
		if (lifeStory != null)
		{
			if (killed is ScientistNPC || killed is ScientistNPC2)
			{
				PlayerLifeStory obj = lifeStory;
				obj.killedScientists++;
			}
			else if (killed is BasePlayer)
			{
				PlayerLifeStory obj2 = lifeStory;
				obj2.killedPlayers++;
			}
			else if (killed is BaseAnimalNPC || killed is BaseNPC2 { IsAnimal: not false } || killed is SnakeHazard)
			{
				PlayerLifeStory obj3 = lifeStory;
				obj3.killedAnimals++;
			}
		}
	}

	public void LifeStoryGenericStat(string key, int value)
	{
		if (lifeStory == null)
		{
			return;
		}
		if (lifeStory.genericStats == null)
		{
			lifeStory.genericStats = Pool.Get<List<GenericStat>>();
		}
		foreach (GenericStat genericStat in lifeStory.genericStats)
		{
			if (genericStat.key == key)
			{
				genericStat.value += value;
				return;
			}
		}
		GenericStat val = Pool.Get<GenericStat>();
		val.key = key;
		val.value = value;
		lifeStory.genericStats.Add(val);
	}

	public void LifeStoryHurt(float amount)
	{
		if (lifeStory != null)
		{
			PlayerLifeStory obj = lifeStory;
			obj.totalDamageTaken += amount;
		}
	}

	public void LifeStoryHeal(float amount)
	{
		if (lifeStory != null)
		{
			PlayerLifeStory obj = lifeStory;
			obj.totalHealing += amount;
		}
	}

	public void SetOverrideDeathBlow(DeathInfo info)
	{
		cachedOverrideDeathInfo = info;
	}

	internal void LifeStoryLogDeath(in DeathBlow deathBlow, DamageType lastDamage)
	{
		if (lifeStory == null)
		{
			return;
		}
		lifeStory.timeDied = (uint)Epoch.Current;
		DeathInfo val = cachedOverrideDeathInfo ?? Pool.Get<DeathInfo>();
		val.lastDamageType = (int)lastDamage;
		cachedOverrideDeathInfo = null;
		if (deathBlow.IsValid)
		{
			if ((Object)(object)deathBlow.Initiator != (Object)null)
			{
				deathBlow.Initiator.AttackerInfo(val);
				val.attackerDistance = Distance(deathBlow.Initiator);
			}
			if ((Object)(object)deathBlow.WeaponPrefab != (Object)null)
			{
				val.inflictorName = deathBlow.WeaponPrefab.ShortPrefabName;
			}
			if (deathBlow.HitBone != 0)
			{
				val.hitBone = StringPool.Get(deathBlow.HitBone);
			}
			else
			{
				val.hitBone = "";
			}
		}
		else if (base.SecondsSinceAttacked <= 60f && (Object)(object)lastAttacker != (Object)null)
		{
			lastAttacker.AttackerInfo(val);
		}
		lifeStory.deathInfo = val;
	}

	internal override void OnParentRemoved()
	{
		if (IsNpc)
		{
			base.OnParentRemoved();
		}
		else
		{
			SetParent(null, worldPositionStays: true, sendImmediate: true);
		}
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		bool flag = ActivePlayerInd == -1;
		bool flag2 = false;
		if ((Object)(object)oldParent != (Object)null)
		{
			TransformState(((Component)oldParent).transform.localToWorldMatrix);
			flag2 = flag && oldParent.syncPosition && oldParent.net?.group?.isGlobal == true;
		}
		bool flag3 = false;
		if ((Object)(object)newParent != (Object)null)
		{
			TransformState(((Component)newParent).transform.worldToLocalMatrix);
			flag3 = flag && newParent.syncPosition && newParent.net?.group?.isGlobal == true;
		}
		if (flag && PositionTickRate < 0f)
		{
			bool flag4 = base.NetworkPosTickCallback != null && SingletonComponent<InvokeHandler>.Instance.IsInvoking(base.NetworkPosTickCallback);
			if (flag3 && !flag4)
			{
				if (base.NetworkPosTickCallback == null)
				{
					Action action = (base.NetworkPosTickCallback = base.NetworkPositionTick);
				}
				InvokeRandomized(base.NetworkPosTickCallback, base.PositionTickRate, base.PositionTickRate - PositionTickRate * 0.05f, base.PositionTickRate * 0.05f);
			}
			else if (flag2 && flag4)
			{
				CancelInvoke(base.NetworkPosTickCallback);
			}
		}
		tickHistory.Reset();
		if ((Object)(object)newParent != (Object)null && ConVar.AntiHack.parenthistory)
		{
			tickHistory.AddPoint(((Component)newParent).transform.InverseTransformPoint(((Component)this).transform.position), tickHistoryCapacity);
			tickHistory.AddParentPoint(((Component)newParent).transform.position, tickHistoryCapacity);
		}
	}

	private void TransformState(Matrix4x4 matrix)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (ActivePlayerInd != -1)
		{
			PlayerStates.TickCache.TransformEntries(ActivePlayerInd, in matrix);
		}
		tickHistory.TransformEntries(matrix);
		if ((Object)(object)eyes != (Object)null)
		{
			Quaternion rotation = ((Matrix4x4)(ref matrix)).rotation;
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(0f, ((Quaternion)(ref rotation)).eulerAngles.y, 0f);
			eyes.bodyRotation = Quaternion.Euler(val) * eyes.bodyRotation;
		}
	}

	public void RecordParentPosition(int limit)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (ConVar.AntiHack.parenthistory)
		{
			Transform parent = ((Component)this).transform.parent;
			if (!((Object)(object)parent == (Object)null))
			{
				tickHistory.AddParentPoint(parent.position, limit);
			}
		}
	}

	public float TickHistoryDistanceParented(Vector3 point)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return tickHistory.DistanceParented(this, point);
	}

	public bool CanSuicide()
	{
		if (IsAdmin || IsDeveloper)
		{
			return true;
		}
		return Time.realtimeSinceStartup > nextSuicideTime;
	}

	public void MarkSuicide()
	{
		nextSuicideTime = Time.realtimeSinceStartup + 60f;
	}

	public bool CanRespawn()
	{
		return Time.realtimeSinceStartup > nextRespawnTime;
	}

	public void MarkRespawn(float nextSpawnDelay = 5f)
	{
		nextRespawnTime = Time.realtimeSinceStartup + nextSpawnDelay;
	}

	public void MovePosition(Vector3 newPos, bool forceUpdateTriggers = true)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = newPos;
		if (ActivePlayerInd != -1)
		{
			BaseEntity baseEntity = parentEntity.Get(base.isServer);
			Vector3 point = (((Object)(object)baseEntity != (Object)null) ? ((Component)baseEntity).transform.InverseTransformPoint(newPos) : newPos);
			PlayerStates.TickCache.Reset(this, point);
		}
		ticksPerSecond.Increment();
		tickHistory.AddPoint(newPos, tickHistoryCapacity);
		RecordParentPosition(tickHistoryCapacity);
		NetworkPositionTick();
		if ((!IsNpc || !isMounted) && forceUpdateTriggers)
		{
			ForceUpdateTriggers();
		}
	}

	public void OverrideViewAngles(Vector3 newAng)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		viewAngles = newAng;
	}

	public override void ServerInit()
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		stats = new PlayerStatistics(this);
		if ((ulong)userID == 0L)
		{
			if (!CollectionEx.IsEmpty(freeBotIds))
			{
				userID = freeBotIds[freeBotIds.Count - 1];
				freeBotIds.RemoveAt(freeBotIds.Count - 1);
			}
			else if (botIdCounter < 10000000)
			{
				userID = botIdCounter++;
			}
			else
			{
				userID = (ulong)Random.Range(0, 10000000);
				Debug.LogError((object)"Exhausted all bot user IDs! This can cause unexpected issues");
			}
			UserIDString = userID.Get().ToString();
			displayName = UserIDString;
			bots.Add(this);
			((PersistentObjectWorkQueue<BasePlayer>)botColliderWorkQueue).Add(this);
		}
		EnablePlayerCollider();
		SetPlayerRigidbodyState(!IsSleeping());
		base.ServerInit();
		eyes.bodyRotation = ((Component)this).transform.rotation;
		if (Query.Server != null)
		{
			Query.Server.AddPlayer(this);
		}
		inventory.ServerInit(this);
		metabolism.ServerInit(this);
		metabolism.MarkNeedsFullSnapshot();
		if ((Object)(object)modifiers != (Object)null)
		{
			modifiers.ServerInit(this);
		}
		if (recentWaveTargets != null)
		{
			recentWaveTargets.Clear();
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		Query.Server.RemovePlayer(this);
		if (ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion())
		{
			FreeUnoccludedSubscribers();
		}
		lastPlayerVisibility.Clear();
		if (Object.op_Implicit((Object)(object)inventory))
		{
			inventory.DoDestroy();
		}
		sleepingPlayerList.Remove(this);
		sleepingPlayerLookup.Remove(userID);
		if (IsBot)
		{
			bots.Remove(this);
			((PersistentObjectWorkQueue<BasePlayer>)botColliderWorkQueue).Remove(this);
			freeBotIds.Add(userID);
		}
		SavePlayerState();
		if (cachedPersistantPlayer != null)
		{
			cachedPersistantPlayer.Dispose();
			cachedPersistantPlayer = null;
		}
	}

	private static void AddToPlayerCache(BasePlayer player, Network.Connection c, ref PlayerServerStates playerStates)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		Debug.Assert(player.ActivePlayerInd == -1, "Player already in PlayerCache!");
		StableObjectArray<BasePlayer> playerCache = playerStates.PlayerCache;
		player.ActivePlayerInd = playerCache.Add(player);
		int count = playerCache.Count;
		Transform transform = ((Component)player).transform;
		Vector3 val = default(Vector3);
		Quaternion val2 = default(Quaternion);
		transform.GetPositionAndRotation(ref val, ref val2);
		playerStates.PlayerLocalPos.Expand<Vector3>(count, (NativeArrayOptions)1, true, false);
		playerStates.PlayerLocalPos[player.ActivePlayerInd] = transform.localPosition;
		playerStates.PlayerPos.Expand<Vector3>(count, (NativeArrayOptions)1, true, false);
		playerStates.PlayerPos[player.ActivePlayerInd] = val;
		playerStates.LastFramePlayerPos.Expand<Vector3>(count, (NativeArrayOptions)1, true, false);
		playerStates.LastFramePlayerPos[player.ActivePlayerInd] = Vector3.zero;
		playerStates.PlayerLocalRots.Expand<Quaternion>(count, (NativeArrayOptions)1, true, false);
		playerStates.PlayerLocalRots[player.ActivePlayerInd] = transform.localRotation;
		playerStates.PlayerRots.Expand<Quaternion>(count, (NativeArrayOptions)1, true, false);
		playerStates.PlayerRots[player.ActivePlayerInd] = val2;
		NativeArrayEx.Expand(ref playerStates.WaterInfos, count, (NativeArrayOptions)1);
		NativeArrayEx.Expand(ref playerStates.WaterFactors, count, (NativeArrayOptions)1);
		NativeArrayEx.Expand(ref playerStates.CachedStates, count, (NativeArrayOptions)1);
		playerStates.TickCache.Expand(count);
		((TransformAccessArray)(ref playerStates.PlayerTransformsAccess)).Add(transform);
		playerStates.PlayerModelStateFlags.Expand<Flag>(count, (NativeArrayOptions)1, true, false);
		NativeArrayEx.Expand(ref playerStates.PlayerModelStateDucking, count, (NativeArrayOptions)1);
		if (player.modelState != null)
		{
			playerStates.PlayerModelStateFlags[player.ActivePlayerInd] = (Flag)player.modelState.flags;
			playerStates.PlayerModelStateDucking[player.ActivePlayerInd] = player.modelState.ducking;
		}
		else
		{
			playerStates.PlayerModelStateFlags[player.ActivePlayerInd] = (Flag)4;
			playerStates.PlayerModelStateDucking[player.ActivePlayerInd] = 0f;
		}
		NativeArrayEx.Expand(ref playerStates.IsMounted, count, (NativeArrayOptions)1);
		playerStates.IsMounted[player.ActivePlayerInd] = false;
		if (playerStates.Mountables.Capacity < count)
		{
			playerStates.Mountables.Resize(count);
		}
		playerStates.Mountables[player.ActivePlayerInd] = null;
		NativeArrayEx.Expand(ref playerStates.TickDeltaTime, count, (NativeArrayOptions)1);
		playerStates.TickDeltaTime[player.ActivePlayerInd] = 0f;
		NativeArrayEx.Expand(ref playerStates.TickNeedsFinalizing, count, (NativeArrayOptions)1);
		playerStates.TickNeedsFinalizing[player.ActivePlayerInd] = false;
		if (EACServer.CanSendAnalytics)
		{
			NativeArrayEx.Expand(ref EACTickStates, count * (int)Player.clientTickRate, (NativeArrayOptions)1);
		}
		if (EACServer.ValidInterface)
		{
			NativeArrayEx.Expand(ref ClientHandles, count, (NativeArrayOptions)1);
			ClientHandles[player.ActivePlayerInd] = EACServer.GetClient(c);
		}
		AntiHack.OnPlayerAddedToCache(player, playerCache, player.ActivePlayerInd);
	}

	private static void RemoveFromPlayerCache(BasePlayer player, ref PlayerServerStates playerStates)
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		Debug.Assert(player.ActivePlayerInd != -1, "Player not in the PlayerCache!");
		StableObjectArray<BasePlayer> playerCache = playerStates.PlayerCache;
		int activePlayerInd = player.ActivePlayerInd;
		int indexForSyncRemove = playerCache.GetIndexForSyncRemove(activePlayerInd);
		playerCache.RemoveAtSwapback(activePlayerInd, invalidateStableIndex: true);
		player.ActivePlayerInd = -1;
		((TransformAccessArray)(ref playerStates.PlayerTransformsAccess)).RemoveAtSwapBack(indexForSyncRemove);
		int count = playerCache.Count;
		if (indexForSyncRemove != count)
		{
			Debug.Assert(indexForSyncRemove < count, "Unexpected swap indices, expecting to swap from end to earlier in range!");
			playerCache.Objects[indexForSyncRemove].ActivePlayerInd = indexForSyncRemove;
			playerStates.PlayerLocalPos[indexForSyncRemove] = playerStates.PlayerLocalPos[count];
			playerStates.PlayerPos[indexForSyncRemove] = playerStates.PlayerPos[count];
			playerStates.LastFramePlayerPos[indexForSyncRemove] = playerStates.LastFramePlayerPos[count];
			playerStates.PlayerLocalRots[indexForSyncRemove] = playerStates.PlayerLocalRots[count];
			playerStates.PlayerRots[indexForSyncRemove] = playerStates.PlayerRots[count];
			playerStates.WaterInfos[indexForSyncRemove] = playerStates.WaterInfos[count];
			playerStates.WaterFactors[indexForSyncRemove] = playerStates.WaterFactors[count];
			playerStates.CachedStates[indexForSyncRemove] = playerStates.CachedStates[count];
			playerStates.TickCache.MovePlayer(count, indexForSyncRemove);
			playerStates.PlayerModelStateFlags[indexForSyncRemove] = playerStates.PlayerModelStateFlags[count];
			playerStates.PlayerModelStateDucking[indexForSyncRemove] = playerStates.PlayerModelStateDucking[count];
			playerStates.IsMounted[indexForSyncRemove] = playerStates.IsMounted[count];
			playerStates.Mountables[indexForSyncRemove] = playerStates.Mountables[count];
			playerStates.Mountables[count] = null;
			playerStates.TickDeltaTime[indexForSyncRemove] = playerStates.TickDeltaTime[count];
			playerStates.TickNeedsFinalizing[indexForSyncRemove] = playerStates.TickNeedsFinalizing[count];
			if (EACServer.CanSendAnalytics)
			{
				for (int i = 0; i < (int)Player.clientTickRate; i++)
				{
					EACTickStates[indexForSyncRemove * (int)Player.clientTickRate + i] = EACTickStates[count * (int)Player.clientTickRate + i];
				}
			}
			if (EACServer.ValidInterface)
			{
				ClientHandles[indexForSyncRemove] = ClientHandles[count];
			}
		}
		AntiHack.OnPlayerRemovedFromCache(player, count, indexForSyncRemove);
	}

	internal static void ServerUpdateParallel(float deltaTime, in PlayerServerStates playerStates)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if (!Net.sv.IsConnected())
		{
			return;
		}
		using (TimeWarning.New("ServerUpdateParallel"))
		{
			CachePlayerTransforms(in playerStates);
			StableObjectArray<BasePlayer> playerCache = playerStates.PlayerCache;
			LifeStoryUpdate(playerStates.AsReadOnly(), deltaTime);
			NativeList<int> toUpdate = new NativeList<int>(playerCache.Count, AllocatorHandle.op_Implicit((Allocator)2));
			try
			{
				FinalizeTickParallel(in playerStates, deltaTime, toUpdate);
				if (BaseMission.missionsenabled)
				{
					ServerThinkMissionsParallel(playerStates.AsReadOnly(), deltaTime);
				}
				if (ConVar.AntiHack.terrain_protection > 0)
				{
					AntiHack.ValidateAgainstTerrain(playerStates.AsReadOnly());
				}
				float serverTickInterval = Player.serverTickInterval;
				ConnectedPlayersUpdate(playerStates.AsReadOnly(), toUpdate.AsReadOnly(), deltaTime, serverTickInterval);
				ServerUpdatePlayerTickMisc(playerStates.AsReadOnly(), toUpdate.AsReadOnly());
				ServerUpdatePlayerMutes(playerStates.AsReadOnly());
				ServerEnforceViolations(in playerStates);
				ServerKickIdlePlayers(in playerStates);
				ServerKickUnresponsivePlayers(in playerStates);
			}
			finally
			{
				((IDisposable)toUpdate/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static void CachePlayerTransforms(in PlayerServerStates playerStates)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		RecacheTransforms recacheTransforms = new RecacheTransforms
		{
			LocalPos = playerStates.PlayerLocalPos,
			Pos = playerStates.PlayerPos,
			LocalRots = playerStates.PlayerLocalRots,
			Rots = playerStates.PlayerRots
		};
		IJobParallelForTransformExtensions.RunReadOnlyByRef<RecacheTransforms>(ref recacheTransforms, playerStates.PlayerTransformsAccess);
	}

	private static void ServerUpdatePlayerMutes(in PlayerServerStates.ReadOnly playerStates)
	{
		using (TimeWarning.New("ServerUpdatePlayerMutes"))
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			long num = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			for (int i = 0; i < objects.Length; i++)
			{
				BasePlayer basePlayer = objects[i];
				if (basePlayer.HasPlayerFlag(PlayerFlags.ChatMute) && realtimeSinceStartup > basePlayer.nextMuteCheckTime)
				{
					basePlayer.nextMuteCheckTime = realtimeSinceStartup + 60f;
					if (basePlayer.State.chatMuteExpiryTimestamp > 0.0 && (double)num > basePlayer.State.chatMuteExpiryTimestamp)
					{
						basePlayer.State.chatMuted = false;
						basePlayer.State.chatMuteExpiryTimestamp = 0.0;
						basePlayer.SetPlayerFlag(PlayerFlags.ChatMute, b: false);
						basePlayer.ChatMessage("You have been unmuted");
					}
				}
			}
		}
	}

	private static void ServerEnforceViolations(in PlayerServerStates playerStates)
	{
		if (ConVar.AntiHack.enforcementlevel <= 0)
		{
			return;
		}
		using (TimeWarning.New("ServerEnforceViolations"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			int num = objects.Length;
			for (int i = 0; i < num; i++)
			{
				if (AntiHack.EnforceViolations(objects[i]))
				{
					num--;
					i--;
				}
			}
		}
	}

	private static void ServerKickIdlePlayers(in PlayerServerStates playerStates)
	{
		if (ConVar.Server.idlekick <= 0 || ((SingletonComponent<ServerMgr>.Instance.AvailableSlots > 0 || ConVar.Server.idlekickmode != 1) && ConVar.Server.idlekickmode != 2))
		{
			return;
		}
		using (TimeWarning.New("ServerKickIdlePlayers"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			int num = objects.Length;
			for (int i = 0; i < num; i++)
			{
				BasePlayer basePlayer = objects[i];
				if (!(basePlayer.IdleTime < (float)(ConVar.Server.idlekick * 60)) && (!basePlayer.IsAdmin || ConVar.Server.idlekickadmins != 0) && (!basePlayer.IsDeveloper || ConVar.Server.idlekickadmins != 0))
				{
					basePlayer.Kick($"Idle for {ConVar.Server.idlekick} minutes");
					num--;
					i--;
				}
			}
		}
	}

	private static void ServerKickUnresponsivePlayers(in PlayerServerStates playerStates)
	{
		using (TimeWarning.New("ServerKickUnresponsivePlayers"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			int num = objects.Length;
			for (int i = 0; i < num; i++)
			{
				BasePlayer basePlayer = objects[i];
				if (!basePlayer.IsReceivingSnapshot && basePlayer.IsAlive() && basePlayer.timeSinceLastTick > (float)ConVar.Server.playertimeout)
				{
					basePlayer.lastTickTime = 0f;
					basePlayer.Kick("Unresponsive");
					num--;
					i--;
				}
			}
		}
	}

	private static void ServerUpdatePlayerTickMisc(in PlayerServerStates.ReadOnly playerStates, ReadOnly<int> indices)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ServerUpdatePlayerTickMisc"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			Enumerator<int> enumerator = indices.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					BasePlayer basePlayer = objects[current];
					if (!basePlayer.IsNpc)
					{
						using (TimeWarning.New("TickPings"))
						{
							basePlayer.TickPings();
						}
					}
					using (TimeWarning.New("HeldEntityServerCycle"))
					{
						basePlayer.HeldEntityServerTick();
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static void GatherPlayersToUpdate(in PlayerServerStates playerStates, float deltaTime, NativeList<int> indices)
	{
		using (TimeWarning.New("GatherPlayersToUpdate"))
		{
			double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
			float serverTickInterval = Player.serverTickInterval;
			float maxdesync = ConVar.AntiHack.maxdesync;
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			for (int i = 0; i < objects.Length; i++)
			{
				BasePlayer basePlayer = objects[i];
				basePlayer.desyncTimeRaw = Mathf.Max(basePlayer.timeSinceLastTick - deltaTime, 0f);
				basePlayer.desyncTimeClamped = Mathf.Min(basePlayer.desyncTimeRaw, maxdesync);
				if (!(realtimeSinceStartupAsDouble < basePlayer.lastPlayerTick + (double)serverTickInterval))
				{
					if (basePlayer.lastPlayerTick < realtimeSinceStartupAsDouble - (double)(serverTickInterval * 100f))
					{
						basePlayer.lastPlayerTick = realtimeSinceStartupAsDouble - (double)Random.Range(0f, serverTickInterval);
					}
					while (basePlayer.lastPlayerTick < realtimeSinceStartupAsDouble)
					{
						basePlayer.lastPlayerTick += serverTickInterval;
					}
					indices.AddNoResize(basePlayer.ActivePlayerInd);
				}
			}
		}
	}

	private void ServerUpdateBots(float deltaTime)
	{
		RefreshColliderSize(forced: false);
	}

	private static void ConnectedPlayersUpdate(in PlayerServerStates.ReadOnly playerStates, ReadOnly<int> indices, float deltaTime, float tickDeltaTime)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ConnectedPlayersUpdate"))
		{
			SendEntityUpdates(playerStates.PlayerCache.UnsafeObjects, ReadOnly<int>.op_Implicit(ref indices));
			NativeList<int> val = new NativeList<int>(indices.Length, AllocatorHandle.op_Implicit((Allocator)2));
			try
			{
				NativeList<int> val2 = new NativeList<int>(indices.Length, AllocatorHandle.op_Implicit((Allocator)2));
				try
				{
					ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
					Enumerator<int> enumerator = indices.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							int current = enumerator.Current;
							BasePlayer basePlayer = objects[current];
							if (basePlayer.IsReceivingSnapshot)
							{
								if (basePlayer.SnapshotQueue.Length == 0 && EACServer.IsAuthenticated(basePlayer.net.connection))
								{
									basePlayer.EnterGame();
								}
								continue;
							}
							val.AddNoResize(current);
							if (basePlayer.IsAlive())
							{
								val2.AddNoResize(current);
							}
						}
					}
					finally
					{
						((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
					}
					UpdateMetabolism(objects, val2.AsReadOnly(), tickDeltaTime);
					UpdateModifiers(objects, val2.AsReadOnly());
					UpdateHostility(objects, val2.AsReadOnly(), tickDeltaTime);
					UpdateHeavyLandingAnims(objects, val2.AsReadOnly());
					UpdateConnectedStates(objects, val.AsReadOnly(), deltaTime);
					RefreshColliderSizes(objects, val.AsReadOnly(), playerStates.CachedStates);
					SendModelStates(objects, val.AsReadOnly());
				}
				finally
				{
					((IDisposable)val2/*cast due to constrained. prefix*/).Dispose();
				}
			}
			finally
			{
				((IDisposable)val/*cast due to constrained. prefix*/).Dispose();
			}
		}
		static void RefreshColliderSizes(ReadOnlySpan<BasePlayer> players, ReadOnly<int> val3, ReadOnly<CachedState> cachedStates)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("RefreshColliderSizes"))
			{
				Enumerator<int> enumerator2 = val3.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current2 = enumerator2.Current;
						BasePlayer obj = players[current2];
						bool isSwimming = cachedStates[current2].IsSwimming;
						obj.RefreshColliderSize(forced: false, isSwimming);
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
			}
		}
		static void SendModelStates(ReadOnlySpan<BasePlayer> players, ReadOnly<int> val3)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("SendModelStates"))
			{
				Enumerator<int> enumerator2 = val3.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current2 = enumerator2.Current;
						players[current2].SendModelState();
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
			}
		}
		static void UpdateConnectedStates(ReadOnlySpan<BasePlayer> players, ReadOnly<int> val3, float num)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("UpdateConnectedStates"))
			{
				Enumerator<int> enumerator2 = val3.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current2 = enumerator2.Current;
						BasePlayer basePlayer2 = players[current2];
						if (basePlayer2.stallProtectionTime > 0f)
						{
							basePlayer2.stallProtectionTime -= num;
						}
						int num2 = (int)basePlayer2.net.connection.GetSecondsConnected();
						int num3 = num2 - basePlayer2.secondsConnected;
						if (num3 > 0)
						{
							basePlayer2.stats.Add("time", num3, Stats.Server);
							basePlayer2.secondsConnected = num2;
						}
						if (basePlayer2.IsLoadingAfterTransfer())
						{
							Debug.LogWarning((object)"Force removing loading flag for player (sanity check failed)", (Object)(object)basePlayer2);
							basePlayer2.SetPlayerFlag(PlayerFlags.LoadingAfterTransfer, b: false);
						}
						if (basePlayer2.State != null)
						{
							basePlayer2.SetPlayerFlag(PlayerFlags.ChatMute, basePlayer2.State.chatMuted);
						}
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
			}
		}
		static void UpdateHeavyLandingAnims(ReadOnlySpan<BasePlayer> players, ReadOnly<int> val3)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("UpdateHeavyLandingAnims"))
			{
				Enumerator<int> enumerator2 = val3.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current2 = enumerator2.Current;
						BasePlayer basePlayer2 = players[current2];
						if (basePlayer2.PlayHeavyLandingAnimation && !basePlayer2.modelState.mounted && basePlayer2.modelState.onground && Parachute.LandingAnimations)
						{
							basePlayer2.Server_StartGesture(GestureCollection.HeavyLandingId);
							basePlayer2.PlayHeavyLandingAnimation = false;
						}
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
			}
		}
		static void UpdateHostility(ReadOnlySpan<BasePlayer> players, ReadOnly<int> val3, float num2)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("UpdateHostility"))
			{
				Enumerator<int> enumerator2 = val3.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current2 = enumerator2.Current;
						BasePlayer basePlayer2 = players[current2];
						if (basePlayer2.InSafeZone() || basePlayer2.InHostileWarningZone())
						{
							float num = 0f;
							HeldEntity heldEntity = basePlayer2.GetHeldEntity();
							if (Object.op_Implicit((Object)(object)heldEntity) && heldEntity.hostile)
							{
								num = num2;
							}
							if (num == 0f)
							{
								basePlayer2.MarkWeaponDrawnDuration(0f);
							}
							else
							{
								basePlayer2.AddWeaponDrawnDuration(num);
							}
							if (basePlayer2.weaponDrawnDuration >= 8f)
							{
								basePlayer2.MarkHostileFor(30f);
							}
						}
						else
						{
							basePlayer2.MarkWeaponDrawnDuration(0f);
						}
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
			}
		}
		static void UpdateMetabolism(ReadOnlySpan<BasePlayer> players, ReadOnly<int> val3, float delta)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("UpdateMetabolism"))
			{
				Enumerator<int> enumerator2 = val3.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current2 = enumerator2.Current;
						BasePlayer basePlayer2 = players[current2];
						basePlayer2.metabolism.ServerUpdate(basePlayer2, delta);
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
			}
		}
		static void UpdateModifiers(ReadOnlySpan<BasePlayer> players, ReadOnly<int> val3)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("UpdateModifiers"))
			{
				Enumerator<int> enumerator2 = val3.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current2 = enumerator2.Current;
						BasePlayer basePlayer2 = players[current2];
						if ((Object)(object)basePlayer2.modifiers != (Object)null)
						{
							basePlayer2.modifiers.ServerUpdate(basePlayer2);
						}
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
			}
		}
	}

	public static void UpdateSubscriptions(in PlayerServerStates.ReadOnly playerStates, ReadOnly<int> indices, float currTime)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("UpdateSubscriptions"))
		{
			BufferList<Networkable> val = Pool.Get<BufferList<Networkable>>();
			NativeList<int> val2 = new NativeList<int>(indices.Length, AllocatorHandle.op_Implicit((Allocator)3));
			NativeList<int> val3 = new NativeList<int>(indices.Length, AllocatorHandle.op_Implicit((Allocator)3));
			NativeList<int> val4 = new NativeList<int>(indices.Length, AllocatorHandle.op_Implicit((Allocator)3));
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			Enumerator<int> enumerator = indices.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					BasePlayer basePlayer = objects[current];
					Debug.Assert(basePlayer.IsConnected);
					if (basePlayer.net.ShouldUpdateSubscriptions)
					{
						if (basePlayer.IsReceivingSnapshot)
						{
							val.Add(basePlayer.net);
							val2.AddNoResize(int.MaxValue);
							val3.AddNoResize(int.MaxValue);
							val4.AddNoResize(current);
						}
						else if (currTime > basePlayer.lastSubscriptionTick + ConVar.Server.entitybatchtime)
						{
							val.Add(basePlayer.net);
							val2.AddNoResize(ConVar.Server.entitybatchsize);
							val3.AddNoResize(ConVar.Server.entitybatchsize * 2);
							val4.AddNoResize(current);
							basePlayer.lastSubscriptionTick = currTime;
						}
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			if (val.Count > 0)
			{
				Networkable.UpdateSubscriptions(val, val3.AsArray(), val2.AsArray());
			}
			for (int i = 0; i < val.Count; i++)
			{
				int index = val4[i];
				bool updateSubscriptions = val3[i] == int.MinValue || val2[i] == int.MinValue;
				objects[index].net.SetUpdateSubscriptions(updateSubscriptions);
			}
			val4.Dispose();
			val3.Dispose();
			val2.Dispose();
			Pool.FreeUnmanaged<Networkable>(ref val);
		}
	}

	internal void EnterGame()
	{
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		SetPlayerFlag(PlayerFlags.ReceivingSnapshot, b: false);
		bool flag = false;
		if (IsLoadingAfterTransfer())
		{
			SetPlayerFlag(PlayerFlags.LoadingAfterTransfer, b: false);
			EndSleeping();
			flag = true;
		}
		if (IsTransferProtected())
		{
			BaseVehicle vehicleParent = GetVehicleParent();
			if ((Object)(object)vehicleParent == (Object)null || vehicleParent.ShouldDisableTransferProtectionOnLoad(this))
			{
				DisableTransferProtection();
				flag = true;
			}
		}
		if (flag)
		{
			SendNetworkUpdateImmediate();
		}
		ClientRPC(RpcTarget.Player("FinishLoading", this));
		Invoke(DelayedTeamUpdate, 1f);
		if (PlayerStateEx.IsSaveStale(State))
		{
			State.protocol = 287;
			State.seed = World.Seed;
			State.saveCreatedTime = Epoch.FromDateTime(SaveRestore.SaveCreatedTime);
			Debug.Log((object)"PlayerState was from old protocol or different seed, or not from a loaded save. Clearing player state");
			WipeMissions(saveImmediately: true);
			OnFogOfWarStale();
			if (State.toastOnReconnect != null)
			{
				State.toastOnReconnect.Clear();
			}
		}
		else
		{
			LoadMissions(State.missions);
			MissionsDirty(saveImmediately: true);
		}
		BaseMission.PlayerRequestedValidStatesUpdate(this);
		double num = State.unHostileTimestamp - TimeEx.currentTimestamp;
		if (num > 0.0)
		{
			ClientRPC(RpcTarget.Player("SetHostileLength", this), (float)num);
		}
		if (IsTransferProtected() && base.TransferProtectionRemaining > 0f)
		{
			ClientRPC(RpcTarget.Player("SetTransferProtectionDuration", this), base.TransferProtectionRemaining);
		}
		if ((ConVar.Server.deepSeaFogofwar || ConVar.Server.fogofwar) && !hasSentFogOfWar)
		{
			if (State.fogImageNetId != net.ID && State.fogImageNetId.Value != 0L)
			{
				FileStorage.server.ReassignEntityId(State.fogImageNetId, net.ID);
			}
			State.fogImageNetId = net.ID;
			hasSentFogOfWar = true;
			SendFogImagesToClient();
		}
		if ((Object)(object)modifiers != (Object)null)
		{
			modifiers.ResetTicking();
		}
		if (net != null)
		{
			EACServer.OnFinishLoading(net.connection);
		}
		Debug.Log((object)$"{this} has spawned");
		if ((Demo.recordlistmode == 0) ? Demo.recordlist.Contains(UserIDString) : (!Demo.recordlist.Contains(UserIDString)))
		{
			StartServerDemoRecording();
		}
		SendClientPetLink();
		ClientRPC(RpcTarget.Player("ForceViewAnglesTo", this), ((Component)this).transform.forward);
		HandleTutorialOnGameEnter();
	}

	private void HandleTutorialOnGameEnter()
	{
		bool isInTutorial = IsInTutorial;
		BaseMission.MissionInstance instance;
		bool flag = TryGetActiveMissionInstance(out instance) && instance.GetMission() is TutorialMission;
		bool flag2 = !isInTutorial && flag;
		if (!flag2 && isInTutorial && (Object)(object)TutorialIsland.RestoreOrCreateIslandForPlayer(this, triggerAnalytics: false) == (Object)null)
		{
			flag2 = true;
		}
		if (flag2)
		{
			ClearTutorial();
			Hurt(999999f);
			ClearTutorial_PostDeath();
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void ClientKeepConnectionAlive(RPCMessage msg)
	{
		lastTickTime = Time.time;
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void ClientLoadingComplete(RPCMessage msg)
	{
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	private void Server_OnClientDemoRecordingStateChanged(RPCMessage msg)
	{
		if (net == null || net.connection == null || !(net.connection.player is BasePlayer basePlayer))
		{
			return;
		}
		if (!basePlayer.IsAdmin)
		{
			playersRecordingClientDemos.Remove(basePlayer);
			return;
		}
		bool flag = msg.read.Bool();
		bool flag2 = playersRecordingClientDemos.Contains(basePlayer);
		if (flag != flag2)
		{
			if (flag)
			{
				playersRecordingClientDemos.TryAdd(basePlayer);
				basePlayer.SendCompleteSnapshot();
			}
			else
			{
				playersRecordingClientDemos.Remove(basePlayer);
			}
		}
	}

	public void PlayerInit(Network.Connection c)
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("PlayerInit", 10))
		{
			CancelInvoke(base.KillMessage);
			CancelInvoke(OfflineMetabolism);
			SetPlayerFlag(PlayerFlags.Connected, b: true);
			activePlayerList.Add(this);
			activePlayerLookup[c.userid] = this;
			AddToPlayerCache(this, c, ref PlayerStates);
			bots.Remove(this);
			((PersistentObjectWorkQueue<BasePlayer>)botColliderWorkQueue).Remove(this);
			userID = c.userid;
			UserIDString = userID.Get().ToString();
			displayName = c.username;
			c.player = (MonoBehaviour)(object)this;
			secondsConnected = 0;
			currentTeam = RelationshipManager.ServerInstance.FindPlayersTeam(userID)?.teamID ?? 0;
			SingletonComponent<ServerMgr>.Instance.persistance.SetPlayerName(userID, displayName);
			Vector3 position = ((Component)this).transform.position;
			PlayerStates.TickCache.Reset(this, position);
			tickHistory.Reset(position);
			eyeHistory.Clear();
			lastTickTime = 0f;
			lastInputTime = 0f;
			SetPlayerFlag(PlayerFlags.ReceivingSnapshot, b: true);
			lastSentActiveWorkbenchId = default(NetworkableId);
			stats.Init();
			InvokeRandomized(StatSave, Random.Range(5f, 10f), 30f, Random.Range(0f, 6f));
			previousLifeStory = SingletonComponent<ServerMgr>.Instance.persistance.GetLastLifeStory(userID);
			if (previousLifeStory != null && previousLifeStory.wipeId != SaveRestore.WipeId)
			{
				previousLifeStory = null;
			}
			SetPlayerFlag(PlayerFlags.IsAdmin, c.authLevel != 0);
			SetPlayerFlag(PlayerFlags.IsDeveloper, DeveloperList.IsDeveloper(this));
			if (ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion())
			{
				OcclusionInitGroup(canBeInAGroup: false);
			}
			if (IsDead() && net.SwitchGroup(BaseNetworkable.LimboNetworkGroup))
			{
				SendNetworkGroupChange();
			}
			net.OnConnected(c);
			net.StartSubscriber();
			SendAsSnapshot(net.connection);
			GlobalNetworkHandler.server.StartSendingSnapshot(this);
			ClientRPC(RpcTarget.Player("StartLoading", this));
			if (Object.op_Implicit((Object)(object)BaseGameMode.GetActiveGameMode(serverside: true)))
			{
				BaseGameMode.GetActiveGameMode(serverside: true).OnPlayerConnected(this);
			}
			if (net != null)
			{
				EACServer.OnStartLoading(net.connection);
			}
			Interface.CallHook("IOnPlayerConnected", this);
			if (IsAdmin)
			{
				if (ConVar.AntiHack.noclip_protection <= 0)
				{
					ChatMessage("antihack.noclip_protection is disabled!");
				}
				if (ConVar.AntiHack.speedhack_protection <= 0)
				{
					ChatMessage("antihack.speedhack_protection is disabled!");
				}
				if (ConVar.AntiHack.flyhack_protection <= 0)
				{
					ChatMessage("antihack.flyhack_protection is disabled!");
				}
				if (ConVar.AntiHack.projectile_protection <= 0)
				{
					ChatMessage("antihack.projectile_protection is disabled!");
				}
				if (ConVar.AntiHack.melee_protection <= 0)
				{
					ChatMessage("antihack.melee_protection is disabled!");
				}
				if (ConVar.AntiHack.eye_protection <= 0)
				{
					ChatMessage("antihack.eye_protection is disabled!");
				}
				Command("debug.setcreative_ui", IsInCreativeMode);
				Command("debug.setinvis_ui", isInvisible);
				if (isInvisible)
				{
					invisPlayers.Add(this);
				}
			}
			inventory.crafting.SendToOwner();
			if ((Object)(object)TerrainMeta.Path != (Object)null && TerrainMeta.Path.OceanPatrolFar != null)
			{
				SendCargoPatrolPath();
			}
			if (currentTeam == 0L && RelationshipManager.ServerInstance.HasPendingInvite(userID, out var foundTeamID) && RelationshipManager.ServerInstance.GetTeamLeaderInfo(foundTeamID, out var leaderDisplayName, out var leaderID))
			{
				ClientRPC(RpcTarget.Player("CLIENT_PendingInvite", this), leaderDisplayName, leaderID, foundTeamID);
			}
			requestingReputationUpdate = true;
		}
	}

	public void StatSave()
	{
		if (stats != null)
		{
			stats.Save();
		}
	}

	public void SendDeathInformation()
	{
		ClientRPC(RpcTarget.Player("OnDied", this));
	}

	public void SendRespawnOptions()
	{
		if (NexusServer.Started && ZoneController.Instance.CanRespawnAcrossZones(this))
		{
			CollectExternalAndSend();
			return;
		}
		List<SpawnOptions> list = Pool.Get<List<SpawnOptions>>();
		GetRespawnOptionsForPlayer(list, userID);
		Interface.CallHook("OnRespawnInformationGiven", this, list);
		SendToPlayer(list, loading: false);
		async void CollectExternalAndSend()
		{
			List<SpawnOptions> list2 = Pool.Get<List<SpawnOptions>>();
			GetRespawnOptionsForPlayer(list2, userID);
			List<SpawnOptions> allSpawnOptions = Pool.Get<List<SpawnOptions>>();
			foreach (SpawnOptions item in list2)
			{
				allSpawnOptions.Add(item.Copy());
			}
			SendToPlayer(list2, loading: true);
			try
			{
				Request obj = Pool.Get<Request>();
				obj.spawnOptions = Pool.Get<SpawnOptionsRequest>();
				obj.spawnOptions.userId = userID;
				using (NexusRpcResult nexusRpcResult = await NexusServer.BroadcastRpc(obj, 10f))
				{
					foreach (KeyValuePair<string, Response> response in nexusRpcResult.Responses)
					{
						string key = response.Key;
						SpawnOptionsResponse spawnOptions = response.Value.spawnOptions;
						if (spawnOptions != null && spawnOptions.spawnOptions.Count != 0)
						{
							foreach (SpawnOptions spawnOption in spawnOptions.spawnOptions)
							{
								SpawnOptions val = spawnOption.Copy();
								val.nexusZone = key;
								allSpawnOptions.Add(val);
							}
						}
					}
				}
				SendToPlayer(allSpawnOptions, loading: false);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		void SendToPlayer(List<SpawnOptions> spawnOptions, bool loading)
		{
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			RespawnInformation val = Pool.Get<RespawnInformation>();
			try
			{
				val.spawnOptions = spawnOptions;
				val.loading = loading;
				if (LegacyShelter.max_shelters == LegacyShelter.FpShelterDefault && LegacyShelter.SheltersPerPlayer.ContainsKey(userID) && LegacyShelter.SheltersPerPlayer[userID].Count > 0)
				{
					val.shelterPositions = Pool.Get<List<Vector3>>();
					foreach (LegacyShelter item2 in LegacyShelter.SheltersPerPlayer[userID])
					{
						val.shelterPositions.Add(((Component)item2).transform.position);
					}
				}
				if (IsDead())
				{
					val.previousLife = previousLifeStory;
					if (!ConVar.Server.skipDeathScreenFade)
					{
						val.fadeIn = previousLifeStory != null && previousLifeStory.timeDied > Epoch.Current - 5;
					}
					else
					{
						val.fadeIn = false;
					}
				}
				ClientRPC(RpcTarget.Player("OnRespawnInformation", this), val);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public static void GetRespawnOptionsForPlayer(List<SpawnOptions> spawnOptions, ulong userID)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = FindByID(userID);
		PooledList<SleepingBag> val = SleepingBag.FindForPlayer(userID);
		try
		{
			foreach (SleepingBag item in (List<SleepingBag>)(object)val)
			{
				if ((!(item is StaticRespawnArea staticRespawnArea) || staticRespawnArea.IsAuthed(userID)) && (!((Object)(object)basePlayer != (Object)null) || basePlayer.IsInTutorial == item.IsTutorialBag))
				{
					SpawnOptions val2 = Pool.Get<SpawnOptions>();
					val2.id = item.net.ID;
					val2.name = item.niceName;
					val2.worldPosition = ((Component)item).transform.position;
					val2.type = (RespawnType)(item.isStatic ? 5 : ((int)item.RespawnType));
					val2.unlockSeconds = item.GetUnlockSeconds(userID);
					val2.respawnState = item.GetRespawnState(userID);
					val2.mobile = item.IsMobile();
					val2.corpse = item.HasFlag(Flags.Reserved14);
					val2.deepSea = item.IsInsideDeepSea();
					val2.showOnCompass = item.showOnCompass;
					val2.favourite = item.favourite;
					spawnOptions.Add(val2);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool HasRespawnOptions()
	{
		List<SpawnOptions> list = Pool.Get<List<SpawnOptions>>();
		GetRespawnOptionsForPlayer(list, userID);
		bool result = list.Count > 0;
		Pool.Free<SpawnOptions>(ref list, true);
		return result;
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	private void RequestRespawnInformation(RPCMessage msg)
	{
		SendRespawnOptions();
	}

	public void ScheduledDeath()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		foreach (TriggerSafeZone allSafeZone in TriggerSafeZone.allSafeZones)
		{
			Bounds val = allSafeZone.triggerCollider.bounds;
			OBB val2 = WorldSpaceBounds();
			if (((Bounds)(ref val)).Intersects(((OBB)(ref val2)).ToBounds()) && !((Object)(object)allSafeZone.Apartment == (Object)null))
			{
				ApartmentRoom playerApartment = allSafeZone.Apartment.GetPlayerApartment(this);
				if (!((Object)(object)playerApartment == (Object)null) && playerApartment.IsInsideRoom(this))
				{
					return;
				}
			}
		}
		DeathInfo val3 = Pool.Get<DeathInfo>();
		val3.attackerName = "safezone";
		SetOverrideDeathBlow(val3);
		Hurt(999f, DamageType.Suicide, null, useProtection: false);
	}

	public virtual void StartSleeping()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (IsSleeping())
		{
			return;
		}
		Interface.CallHook("OnPlayerSleep", this);
		if (IsRestrained)
		{
			inventory.SetLockedByRestraint(flag: false);
		}
		bool flag = InSafeZone();
		float num = 1f;
		if (!flag && Application.isLoadingSave)
		{
			TriggerSafeZone.IsBoundsInsideSafeZone(WorldSpaceBounds());
			if (flag)
			{
				num = 4f;
			}
		}
		if (flag && !IsInvoking(ScheduledDeath))
		{
			Invoke(ScheduledDeath, NPCAutoTurret.sleeperhostiledelay * num);
		}
		BaseMountable baseMountable = GetMounted();
		if ((Object)(object)baseMountable != (Object)null && !AllowSleeperMounting(baseMountable))
		{
			EnsureDismounted();
		}
		SetPlayerFlag(PlayerFlags.Sleeping, b: true);
		sleepStartTime = Time.time;
		sleepingPlayerList.TryAdd(this);
		sleepingPlayerLookup[userID] = this;
		bots.Remove(this);
		((PersistentObjectWorkQueue<BasePlayer>)botColliderWorkQueue).Remove(this);
		CancelInvoke(InventoryUpdate);
		CancelInvoke(TeamUpdate);
		CancelInvoke(UpdateClanLastSeen);
		inventory.loot.Clear();
		inventory.containerMain.OnChanged();
		inventory.containerBelt.OnChanged();
		inventory.containerWear.OnChanged();
		EnablePlayerCollider();
		if (!IsLoadingAfterTransfer())
		{
			RemovePlayerRigidbody();
			TurnOffAllLights();
		}
		SetServerFall(wantsOn: true);
		RunOfflineMetabolism(state: true);
		EndActiveConversation();
	}

	private void TurnOffAllLights()
	{
		LightToggle(mask: false);
		HeldEntity heldEntity = GetHeldEntity();
		if ((Object)(object)heldEntity != (Object)null)
		{
			TorchWeapon component = ((Component)heldEntity).GetComponent<TorchWeapon>();
			if ((Object)(object)component != (Object)null)
			{
				component.SetIsOn(isOn: false);
			}
		}
	}

	private void OnPhysicsNeighbourChanged()
	{
		if (IsSleeping() || IsIncapacitated())
		{
			Invoke(DelayedServerFall, 0.05f);
		}
	}

	private void DelayedServerFall()
	{
		SetServerFall(wantsOn: true);
	}

	public void SetServerFall(bool wantsOn)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (wantsOn && ConVar.Server.playerserverfall)
		{
			if (!IsInvoking(ServerFall))
			{
				SetPlayerFlag(PlayerFlags.ServerFall, b: true);
				lastFallTime = Time.time - fallTickRate;
				InvokeRandomized(ServerFall, 0f, fallTickRate, fallTickRate * 0.1f);
				fallVelocity = estimatedVelocity.y;
			}
		}
		else
		{
			CancelInvoke(ServerFall);
			SetPlayerFlag(PlayerFlags.ServerFall, b: false);
		}
	}

	public void ServerFall()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		if (IsDead() || HasParent() || (!IsIncapacitated() && !IsSleeping()))
		{
			SetServerFall(wantsOn: false);
			return;
		}
		float num = Time.time - lastFallTime;
		lastFallTime = Time.time;
		float radius = GetRadius();
		float num2 = GetHeight(ducked: true) * 0.5f;
		float num3 = 2.5f;
		float num4 = 0.5f;
		fallVelocity += Physics.gravity.y * num3 * num4 * num;
		float num5 = Mathf.Abs(fallVelocity * num);
		Vector3 val = ((Component)this).transform.position + Vector3.up * (radius + num2);
		Vector3 position = ((Component)this).transform.position;
		Vector3 val2 = ((Component)this).transform.position;
		int layerMask = 1537286401;
		layerMask = GamePhysics.HandleIgnoreCollision(val, layerMask);
		layerMask = GamePhysics.HandleIgnoreCollision(val + Vector3.down * (num5 + num2), layerMask);
		RaycastHit val3 = default(RaycastHit);
		if (Physics.SphereCast(val, radius, Vector3.down, ref val3, num5 + num2, layerMask, (QueryTriggerInteraction)1))
		{
			SetServerFall(wantsOn: false);
			if (((RaycastHit)(ref val3)).distance > num2)
			{
				val2 += Vector3.down * (((RaycastHit)(ref val3)).distance - num2);
			}
			ApplyFallDamageFromVelocity(fallVelocity);
			UpdateEstimatedVelocity(val2, val2, num);
			fallVelocity = 0f;
		}
		else if (Physics.Raycast(val, Vector3.down, ref val3, num5 + radius + num2, layerMask, (QueryTriggerInteraction)1))
		{
			SetServerFall(wantsOn: false);
			if (((RaycastHit)(ref val3)).distance > num2 - radius)
			{
				val2 += Vector3.down * (((RaycastHit)(ref val3)).distance - num2 - radius);
			}
			ApplyFallDamageFromVelocity(fallVelocity);
			UpdateEstimatedVelocity(val2, val2, num);
			fallVelocity = 0f;
		}
		else
		{
			val2 += Vector3.down * num5;
			UpdateEstimatedVelocity(position, val2, num);
			if (WaterLevel.Test(val2, waves: true, volumes: true, this) || AntiHack.TestInsideTerrain(val2))
			{
				SetServerFall(wantsOn: false);
			}
		}
		MovePosition(val2, forceUpdateTriggers: false);
	}

	public void RunOfflineMetabolism(bool state)
	{
		if (state)
		{
			InvokeRandomized(OfflineMetabolism, ConVar.Server.metabolismtick, ConVar.Server.metabolismtick, ConVar.Server.metabolismtick / 10f);
		}
		else
		{
			CancelInvoke(OfflineMetabolism);
		}
	}

	private void OfflineMetabolism()
	{
		if (!base.IsDestroyed)
		{
			inventory.containerWear.OnCycle(ConVar.Server.metabolismtick);
			metabolism.ServerUpdate(this, ConVar.Server.metabolismtick);
		}
	}

	public void DelayedRigidbodyDisable()
	{
		RemovePlayerRigidbody();
	}

	public virtual void EndSleeping()
	{
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		if (IsSleeping() && Interface.CallHook("OnPlayerSleepEnd", this) == null)
		{
			if (IsRestrained)
			{
				inventory.SetLockedByRestraint(flag: true);
			}
			SetPlayerFlag(PlayerFlags.Sleeping, b: false);
			sleepStartTime = -1f;
			sleepingPlayerList.Remove(this);
			sleepingPlayerLookup.Remove(userID);
			if ((ulong)userID < 10000000 && !bots.Contains(this))
			{
				bots.Add(this);
				((PersistentObjectWorkQueue<BasePlayer>)botColliderWorkQueue).Add(this);
			}
			CancelInvoke(ScheduledDeath);
			InvokeRepeating(InventoryUpdate, 1f, 0.1f * Random.Range(0.99f, 1.01f));
			if (RelationshipManager.TeamsEnabled())
			{
				InvokeRandomized(TeamUpdate, 1f, 4f, 1f);
			}
			InvokeRandomized(UpdateClanLastSeen, 300f, 300f, 60f);
			EnablePlayerCollider();
			RefreshColliderSize(forced: true);
			AddPlayerRigidbody();
			SetServerFall(wantsOn: false);
			RunOfflineMetabolism(state: false);
			if (HasParent())
			{
				SetParent(null, worldPositionStays: true);
				RemoveFromTriggers();
				ForceUpdateTriggers();
			}
			inventory.containerMain.OnChanged();
			inventory.containerBelt.OnChanged();
			inventory.containerWear.OnChanged();
			Interface.CallHook("OnPlayerSleepEnded", this);
			EACServer.LogPlayerSpawn(this);
			if (TotalPingCount > 0)
			{
				SendPingsToClient();
			}
			if (TutorialIsland.ShouldPlayerBeAskedToStartTutorial(this))
			{
				ClientRPC(RpcTarget.Player("PromptToStartTutorial", this));
			}
			if (AntiHack.TestNoClipping(this, ((Component)this).transform.position, ((Component)this).transform.position, NoClipRadius(ConVar.AntiHack.noclip_margin), ConVar.AntiHack.noclip_backtracking, out var _))
			{
				ForceCastNoClip();
			}
			if (State.toastOnReconnect != null && State.toastOnReconnect.Count > 0)
			{
				Invoke(ProcessReconnectToast, 2f);
			}
		}
	}

	private void ProcessReconnectToast()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		if (State.toastOnReconnect != null && State.toastOnReconnect.Count != 0)
		{
			ReconnectToast val = State.toastOnReconnect[0];
			State.toastOnReconnect.RemoveAt(0);
			ShowToast((GameTip.Styles)val.type, new Phrase(val.phrase, ""), false);
			if (State.toastOnReconnect.Count > 0)
			{
				Invoke(ProcessReconnectToast, 10f);
			}
		}
	}

	public virtual void EndLooting()
	{
		if (Object.op_Implicit((Object)(object)inventory.loot))
		{
			inventory.loot.Clear();
		}
	}

	public virtual void OnDisconnected()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		startTutorialCooldown = 0f;
		stats.Save(forceSteamSave: true);
		EndLooting();
		ClearDesigningAIEntity();
		Server_CancelGesture();
		if (IsAlive() || IsSleeping())
		{
			UpdateActiveItem(default(ItemId));
			StartSleeping();
		}
		else
		{
			Invoke(base.KillMessage, 0f);
		}
		if (isInvisible)
		{
			invisPlayers.Remove(this);
		}
		activePlayerList.Remove(this);
		activePlayerLookup.Remove(userID);
		if (ActivePlayerInd != -1)
		{
			RemoveFromPlayerCache(this, ref PlayerStates);
		}
		SetPlayerFlag(PlayerFlags.Connected, b: false);
		StopServerDemoRecording();
		playersRecordingClientDemos.Remove(this);
		if (net != null)
		{
			if (ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion())
			{
				OcclusionOnDisconnect();
			}
			net.OnDisconnected();
		}
		RefreshColliderSize(forced: true);
		if (Object.op_Implicit((Object)(object)BaseGameMode.GetActiveGameMode(serverside: true)))
		{
			BaseGameMode.GetActiveGameMode(serverside: true).OnPlayerDisconnected(this);
		}
		BaseMission.PlayerDisconnected(this);
		ClanManager serverInstance = ClanManager.ServerInstance;
		if (clanId != 0L && (Object)(object)serverInstance != (Object)null)
		{
			serverInstance.ClanMemberConnectionsChanged(clanId);
		}
		hasSentFogOfWar = false;
		UpdateClanLastSeen();
		DropSpectators();
		EndActiveConversation();
	}

	private void InventoryUpdate()
	{
		if (IsConnected && !IsDead())
		{
			inventory.ServerUpdate(0.1f);
		}
	}

	public void ApplyFallDamageFromVelocity(float velocity)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		if (IsGod())
		{
			return;
		}
		float num = Mathf.InverseLerp(-15f, -100f, velocity);
		if (num != 0f && Interface.CallHook("OnPlayerLand", this, num) == null)
		{
			float num2 = (((Object)(object)modifiers != (Object)null) ? Mathf.Clamp01(1f - modifiers.GetValue(Modifier.ModifierType.Clotting)) : 1f);
			metabolism.bleeding.Add(num * 0.5f * num2);
			float num3 = num * 500f;
			Facepunch.Rust.Analytics.Azure.OnFallDamage(this, velocity, num3);
			Hurt(num3, DamageType.Fall);
			if (num3 > 20f && fallDamageEffect.isValid && !isInvisible)
			{
				Effect.server.Run(fallDamageEffect.resourcePath, ((Component)this).transform.position, Vector3.zero);
			}
			Interface.CallHook("OnPlayerLanded", this, num);
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void OnPlayerLanded(RPCMessage msg)
	{
		float num = msg.read.Float();
		if (!float.IsNaN(num) && !float.IsInfinity(num))
		{
			ApplyFallDamageFromVelocity(num);
			fallVelocity = 0f;
		}
	}

	public void SendSubscribedGroupsSnapshot()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SendSubscribedGroupsSnapshot"))
		{
			Enumerator<Group> enumerator = net.subscriber.subscribed.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Group current = enumerator.Current;
					if (current.ID != 0)
					{
						EnterVisibility(current);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	public override void OnNetworkGroupLeave(Group group)
	{
		base.OnNetworkGroupLeave(group);
		LeaveVisibility(group);
	}

	private void LeaveVisibility(Group group)
	{
		ServerMgr.OnLeaveVisibility(net.connection, group);
	}

	public override void OnNetworkGroupEnter(Group group)
	{
		base.OnNetworkGroupEnter(group);
		EnterVisibility(group);
	}

	private void EnterVisibility(Group group)
	{
		ServerMgr.OnEnterVisibility(net.connection, group);
		SendSnapshots(group.networkables);
	}

	public void CheckDeathCondition(HitInfo info = null)
	{
		Assert.IsTrue(base.isServer, "CheckDeathCondition called on client!");
		if (!IsSpectating() && !IsDead() && metabolism.ShouldDie())
		{
			Die(info);
		}
	}

	public virtual BaseCorpse CreateCorpse(PlayerFlags flagsOnDeath, Vector3 posOnDeath, Quaternion rotOnDeath, List<TriggerBase> triggersOnDeath, bool forceServerSide = false)
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnPlayerCorpseSpawn", this) != null)
		{
			return null;
		}
		using (TimeWarning.New("Create corpse"))
		{
			string strCorpsePrefab = ((!(Physics.serversideragdolls || forceServerSide)) ? "assets/prefabs/player/player_corpse.prefab" : "assets/prefabs/player/player_corpse_new.prefab");
			bool flag = false;
			if (Global.cinematicGingerbreadCorpses)
			{
				ItemCorpseOverride itemCorpseOverride = default(ItemCorpseOverride);
				foreach (Item item in inventory.containerWear.itemList)
				{
					if (item != null && ((Component)item.info).TryGetComponent<ItemCorpseOverride>(ref itemCorpseOverride))
					{
						strCorpsePrefab = ((GetFloatBasedOnUserID(userID, 4332uL) > 0.5f) ? itemCorpseOverride.FemaleCorpse.resourcePath : itemCorpseOverride.MaleCorpse.resourcePath);
						flag = itemCorpseOverride.BlockWearableCopy;
						break;
					}
				}
			}
			PlayerCorpse playerCorpse = DropCorpse(strCorpsePrefab, posOnDeath, rotOnDeath, flagsOnDeath, modelState) as PlayerCorpse;
			if (Object.op_Implicit((Object)(object)playerCorpse))
			{
				using (FlagsUpdateScope flagsUpdateScope = playerCorpse.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope.Set(Flags.Reserved5, HasPlayerFlag(PlayerFlags.DisplaySash));
				}
				if (!flag)
				{
					playerCorpse.TakeFrom(this, inventory.containerMain, inventory.containerWear, inventory.containerBelt);
				}
				playerCorpse.playerName = displayName;
				playerCorpse.streamerName = RandomUsernames.Get((ulong)userID);
				playerCorpse.playerSteamID = userID;
				playerCorpse.underwearSkin = GetUnderwearSkin(Time.time);
				if (!CollectionEx.IsNullOrEmpty(triggersOnDeath))
				{
					foreach (TriggerBase item2 in triggersOnDeath)
					{
						if (item2 is TriggerParent triggerParent)
						{
							triggerParent.ForceParentEarly(playerCorpse);
						}
					}
				}
				playerCorpse.Spawn();
				playerCorpse.TakeChildren(this);
				ResourceDispenser component = ((Component)playerCorpse).GetComponent<ResourceDispenser>();
				int num = 2;
				if (lifeStory != null)
				{
					num += Mathf.Clamp(Mathf.FloorToInt(lifeStory.secondsAlive / 180f), 0, 20);
				}
				component.containedItems.Add(new ItemAmount(ItemManager.FindItemDefinition("fat.animal"), num));
				Interface.CallHook("OnPlayerCorpseSpawned", this, playerCorpse);
				return playerCorpse;
			}
		}
		return null;
		static float GetFloatBasedOnUserID(ulong steamid, ulong seed)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			State state = Random.state;
			Random.InitState((int)(seed + steamid));
			float result = Random.Range(0f, 1f);
			Random.state = state;
			return result;
		}
	}

	public unsafe override void OnDied(HitInfo info)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08de: Unknown result type (might be due to invalid IL or missing references)
		//IL_086e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0873: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_054a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0603: Unknown result type (might be due to invalid IL or missing references)
		//IL_0608: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e9: Unknown result type (might be due to invalid IL or missing references)
		PlayerFlags flagsOnDeath = playerFlags;
		Vector3 position = ((Component)this).transform.position;
		List<TriggerBase> list = Pool.Get<List<TriggerBase>>();
		if (triggers != null)
		{
			foreach (TriggerBase trigger in triggers)
			{
				if ((Object)(object)trigger != (Object)null)
				{
					list.Add(trigger);
				}
			}
		}
		BaseMountable baseMountable = GetMounted();
		Vector3 val = Vector3.zero;
		Quaternion rotOnDeath;
		if (baseMountable.IsValid())
		{
			rotOnDeath = baseMountable.mountAnchor.rotation;
			val = baseMountable.GetMountRagdollVelocity(this);
		}
		else
		{
			float x = ((Component)this).transform.eulerAngles.x;
			Quaternion bodyRotation = eyes.bodyRotation;
			rotOnDeath = Quaternion.Euler(x, ((Quaternion)(ref bodyRotation)).eulerAngles.y, ((Component)this).transform.eulerAngles.z);
		}
		RemoveReceiveTickListenersOnDeath();
		EnsureDismounted();
		EndSleeping();
		EndLooting();
		stats.Add("deaths", 1, Stats.All);
		if (info != null && (Object)(object)info.InitiatorPlayer != (Object)null && !info.InitiatorPlayer.IsNpc && !IsNpc)
		{
			RelationshipManager.ServerInstance.SetSeen(info.InitiatorPlayer, this);
			RelationshipManager.ServerInstance.SetSeen(this, info.InitiatorPlayer);
			RelationshipManager.ServerInstance.SetRelationship(this, info.InitiatorPlayer, RelationshipManager.RelationshipType.Enemy);
			HandleClanPlayerKilled(info.InitiatorPlayer);
		}
		if (Object.op_Implicit((Object)(object)BaseGameMode.GetActiveGameMode(serverside: true)))
		{
			BasePlayer instigator = info?.InitiatorPlayer;
			BaseGameMode.GetActiveGameMode(serverside: true).OnPlayerDeath(instigator, this, info);
		}
		inventory.DropBackpackOnDeath(wounded: false);
		DisablePlayerCollider();
		RemovePlayerRigidbody();
		List<BasePlayer> list2 = Pool.Get<List<BasePlayer>>();
		if (IsIncapacitated())
		{
			Enumerator<BasePlayer> enumerator2 = activePlayerList.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					BasePlayer current2 = enumerator2.Current;
					if ((Object)(object)current2 != (Object)null && (Object)(object)current2.inventory != (Object)null && (Object)(object)current2.inventory.loot != (Object)null && (Object)(object)current2.inventory.loot.entitySource == (Object)(object)this)
					{
						list2.Add(current2);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
		}
		bool flag = IsWounded();
		StopWounded();
		if ((Object)(object)inventory.crafting != (Object)null)
		{
			inventory.crafting.CancelAll();
		}
		EACServer.LogPlayerDespawn(this);
		Ray val2 = eyes.HeadRay();
		bool flag2 = ((Ray)(ref val2)).direction.y > 0.8f;
		bool flag3 = false;
		if (flag2)
		{
			Vector3 val3 = -eyes.MovementForward();
			if (GamePhysics.Trace(new Ray(eyes.position, val3), 0f, out var _, 1f, 2097152, (QueryTriggerInteraction)0))
			{
				flag3 = true;
			}
		}
		Vector3 val4;
		if (!wantsSpectate)
		{
			BaseCorpse baseCorpse = CreateCorpse(flagsOnDeath, position, rotOnDeath, list, flag2 && flag3);
			if ((Object)(object)baseCorpse != (Object)null)
			{
				if (baseCorpse.CorpseIsRagdoll && (Object)(object)baseMountable != (Object)null)
				{
					BaseVehicle baseVehicle = baseMountable.VehicleParent();
					if ((Object)(object)baseVehicle != (Object)null && baseVehicle.mountedPlayerRagdolls == BaseVehicle.RagdollMode.FallThrough)
					{
						GameObjectExtensions.SetIgnoreCollisions(((Component)baseCorpse).gameObject, ((Component)baseVehicle).gameObject, true);
					}
				}
				if (info != null)
				{
					Rigidbody component = ((Component)baseCorpse).GetComponent<Rigidbody>();
					if ((Object)(object)component != (Object)null)
					{
						float num = (baseCorpse.CorpseIsRagdoll ? 5f : 1f);
						val4 = info.attackNormal + Vector3.up * 0.5f;
						Vector3 val5 = ((Vector3)(ref val4)).normalized * num;
						component.AddForce(val5 + val, (ForceMode)2);
					}
				}
				if (baseCorpse is PlayerCorpse { containers: not null } playerCorpse)
				{
					foreach (BasePlayer item in list2)
					{
						if ((Object)(object)item == (Object)null)
						{
							continue;
						}
						item.inventory.loot.StartLootingEntity(playerCorpse);
						ItemContainer[] containers = playerCorpse.containers;
						foreach (ItemContainer itemContainer in containers)
						{
							if (itemContainer != null)
							{
								item.inventory.loot.AddContainer(itemContainer);
							}
						}
						item.inventory.loot.SendImmediate();
					}
				}
			}
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list2);
		inventory.Strip();
		DeathBlow deathBlow;
		if (flag && lastDamage == DamageType.Suicide && cachedNonSuicideHit.IsValid)
		{
			deathBlow = cachedNonSuicideHit;
			DeathBlow.Reset(ref cachedNonSuicideHit);
			lastDamage = info.damageTypes.GetMajorityDamageType();
		}
		else
		{
			DeathBlow.From(info, out deathBlow);
		}
		if (lastDamage == DamageType.Fall)
		{
			stats.Add("death_fall", 1);
		}
		string text = "";
		string text2 = "";
		if (info != null)
		{
			if (Object.op_Implicit((Object)(object)info.Initiator))
			{
				if ((Object)(object)info.Initiator == (Object)(object)this)
				{
					string[] obj = new string[5]
					{
						((object)this).ToString(),
						" was killed by ",
						lastDamage.ToString(),
						" at ",
						null
					};
					val4 = ((Component)this).transform.position;
					obj[4] = ((object)(*(Vector3*)(&val4))/*cast due to constrained. prefix*/).ToString();
					text = string.Concat(obj);
					text2 = "You died: killed by " + lastDamage;
					if (lastDamage == DamageType.Suicide)
					{
						stats.Add("death_suicide", 1, Stats.All);
					}
					else
					{
						stats.Add("death_selfinflicted", 1);
					}
				}
				else if (info.Initiator is BasePlayer)
				{
					BasePlayer basePlayer = info.Initiator.ToPlayer();
					string[] obj2 = new string[5]
					{
						((object)this).ToString(),
						" was killed by ",
						((object)basePlayer).ToString(),
						" at ",
						null
					};
					val4 = ((Component)this).transform.position;
					obj2[4] = ((object)(*(Vector3*)(&val4))/*cast due to constrained. prefix*/).ToString();
					text = string.Concat(obj2);
					text2 = "You died: killed by " + basePlayer.displayName + " (" + basePlayer.userID.Get() + ")";
					basePlayer.stats.Add("kill_player", 1, Stats.All);
					basePlayer.LifeStoryKill(this);
					OnKilledByPlayer(basePlayer);
					if (lastDamage == DamageType.Fun_Water)
					{
						basePlayer.GiveAchievement("SUMMER_LIQUIDATOR");
						LiquidWeapon liquidWeapon = basePlayer.GetHeldEntity() as LiquidWeapon;
						if ((Object)(object)liquidWeapon != (Object)null && liquidWeapon.RequiresPumping && liquidWeapon.PressureFraction <= liquidWeapon.MinimumPressureFraction)
						{
							basePlayer.GiveAchievement("SUMMER_NO_PRESSURE");
						}
					}
					else if (Rust.GameInfo.HasAchievements && lastDamage == DamageType.Explosion && (Object)(object)info.WeaponPrefab != (Object)null && info.WeaponPrefab.ShortPrefabName.Contains("mlrs") && (Object)(object)basePlayer != (Object)null)
					{
						basePlayer.stats.Add("mlrs_kills", 1, Stats.All);
						basePlayer.stats.Save(forceSteamSave: true);
					}
					else if ((Object)(object)info.WeaponPrefab != (Object)null && info.WeaponPrefab.ShortPrefabName.Contains("50cal") && (Object)(object)basePlayer != (Object)null && basePlayer.IsNonNpcPlayer() && basePlayer.GetMountedVehicle() is PTBoat)
					{
						basePlayer.GiveAchievement("STOLEN_PTBOAT_KILL");
					}
					Facepunch.Rust.Analytics.Azure.OnPlayerDeath(this, basePlayer);
				}
				else
				{
					string[] obj3 = new string[7]
					{
						((object)this).ToString(),
						" was killed by ",
						info.Initiator.ShortPrefabName,
						" (",
						info.Initiator.Categorize(),
						") at ",
						null
					};
					val4 = ((Component)this).transform.position;
					obj3[6] = ((object)(*(Vector3*)(&val4))/*cast due to constrained. prefix*/).ToString();
					text = string.Concat(obj3);
					text2 = "You died: killed by " + info.Initiator.Categorize();
					stats.Add("death_" + info.Initiator.Categorize(), 1);
				}
			}
			else if (lastDamage == DamageType.Fall)
			{
				string? text3 = ((object)this).ToString();
				val4 = ((Component)this).transform.position;
				text = text3 + " was killed by fall at " + ((object)(*(Vector3*)(&val4))/*cast due to constrained. prefix*/).ToString();
				text2 = "You died: killed by fall";
			}
			else
			{
				string[] obj4 = new string[5]
				{
					((object)this).ToString(),
					" was killed by ",
					info.damageTypes.GetMajorityDamageType().ToString(),
					" at ",
					null
				};
				val4 = ((Component)this).transform.position;
				obj4[4] = ((object)(*(Vector3*)(&val4))/*cast due to constrained. prefix*/).ToString();
				text = string.Concat(obj4);
				text2 = "You died: " + info.damageTypes.GetMajorityDamageType();
			}
		}
		else
		{
			text = ((object)this).ToString() + " died (" + lastDamage.ToString() + ")";
			text2 = "You died: " + lastDamage;
		}
		using (TimeWarning.New("LogMessage"))
		{
			DebugEx.Log(text, (StackTraceLogType)0);
			ConsoleMessage(text2);
		}
		if (net.connection == null && (Object)(object)info?.Initiator != (Object)null && (Object)(object)info.Initiator != (Object)(object)this)
		{
			CompanionServer.Util.SendDeathNotification(this, info.Initiator);
		}
		EndActiveConversation();
		SendNetworkUpdateImmediate();
		LifeStoryLogDeath(in deathBlow, lastDamage);
		Server_LogDeathMarker(((Component)this).transform.position);
		LifeStoryEnd();
		if (net.connection == null)
		{
			Invoke(base.KillMessage, 0f);
		}
		else
		{
			SendRespawnOptions();
			SendDeathInformation();
			stats.Save();
		}
		PlayerInjureState = GetInjureState();
		Pool.FreeUnmanaged<TriggerBase>(ref list);
	}

	public void RespawnAt(Vector3 position, Quaternion rotation, BaseEntity spawnPointEntity = null)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (Object.op_Implicit((Object)(object)activeGameMode) && !activeGameMode.CanPlayerRespawn(this))
		{
			return;
		}
		SetPlayerFlag(PlayerFlags.Wounded, b: false);
		SetPlayerFlag(PlayerFlags.Incapacitated, b: false);
		SetPlayerFlag(PlayerFlags.ReceivingSnapshot, b: true);
		SetPlayerFlag(PlayerFlags.DisplaySash, b: false);
		respawnId = Guid.NewGuid().ToString("N");
		ServerPerformance.spawns++;
		Vector3 position2 = ((Component)this).transform.position;
		SetParent(null, worldPositionStays: true);
		((Component)this).transform.SetPositionAndRotation(position, rotation);
		if (ActivePlayerInd != -1)
		{
			PlayerStates.TickCache.Reset(this, position);
		}
		tickHistory.Reset(position);
		eyeHistory.Clear();
		ForceUpdateTriggers();
		estimatedVelocity = Vector3.zero;
		estimatedSpeed = 0f;
		estimatedSpeed2D = 0f;
		lastTickTime = 0f;
		lastStallTime = 0f;
		StopWounded();
		ResetWoundingVars();
		StopSpectating();
		UpdateNetworkGroup();
		EnablePlayerCollider();
		RemovePlayerRigidbody();
		StartSleeping();
		LifeStoryStart();
		metabolism.Reset();
		metabolism.MarkNeedsFullSnapshot();
		if ((Object)(object)modifiers != (Object)null)
		{
			if (Player.keepteaondeath)
			{
				modifiers.RemoveAllExceptFromSource(Modifier.ModifierSource.Tea);
			}
			else
			{
				modifiers.RemoveAll();
			}
		}
		InitializeHealth(StartHealth(), StartMaxHealth());
		bool flag = false;
		if (ConVar.Server.respawnWithLoadout)
		{
			string infoString = GetInfoString("client.respawnloadout", string.Empty);
			if (!string.IsNullOrEmpty(infoString) && Inventory.LoadLoadout(infoString, out var so))
			{
				so.LoadItemsOnTo(this);
				flag = true;
			}
		}
		if (!flag)
		{
			inventory.GiveDefaultItems();
		}
		SendNetworkUpdateImmediate();
		ClientRPC(RpcTarget.Player("StartLoading", this));
		if (DeepSea.enabled && (Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null)
		{
			bool num = DeepSeaManager.IsInsideDeepSea(position2);
			bool flag2 = DeepSeaManager.IsInsideDeepSea(position);
			if (num != flag2)
			{
				PointEntity<DeepSeaManager>.ServerInstance.ClientRPC(RpcTarget.Player("CLIENT_PlayerEnterOrLeaveDeepSea", this), flag2);
			}
		}
		Facepunch.Rust.Analytics.Azure.OnPlayerRespawned(this, spawnPointEntity);
		if (Object.op_Implicit((Object)(object)activeGameMode))
		{
			BaseGameMode.GetActiveGameMode(serverside: true).OnPlayerRespawn(this);
		}
		if (IsConnected)
		{
			EACServer.OnStartLoading(net.connection);
		}
		Interface.CallHook("OnPlayerRespawned", this);
		ProcessMissionEvent(BaseMission.MissionEventType.RESPAWN, 0, 0f);
		PlayerInjureState = GetInjureState();
	}

	public void Respawn()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		SpawnPoint spawnPoint = ServerMgr.FindSpawnPoint(this, 0uL);
		if (ConVar.Server.respawnAtDeathPosition && ServerCurrentDeathNote != null)
		{
			spawnPoint.pos = ServerCurrentDeathNote.worldPosition;
		}
		object obj = Interface.CallHook("OnPlayerRespawn", this, spawnPoint);
		if (obj is SpawnPoint)
		{
			spawnPoint = (SpawnPoint)obj;
		}
		RespawnAt(spawnPoint.pos, spawnPoint.rot);
	}

	public bool IsImmortalTo(HitInfo info)
	{
		if (IsGod())
		{
			return true;
		}
		if (WoundingCausingImmortality(info))
		{
			return true;
		}
		BaseVehicle mountedVehicle = GetMountedVehicle();
		if ((Object)(object)mountedVehicle != (Object)null && mountedVehicle.ignoreDamageFromOutside)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if ((Object)(object)initiatorPlayer != (Object)null && (Object)(object)initiatorPlayer.GetMountedVehicle() != (Object)(object)mountedVehicle)
			{
				return true;
			}
		}
		if (IsInTutorial)
		{
			_ = (Object)(object)info.InitiatorPlayer != (Object)(object)this;
			return false;
		}
		return false;
	}

	public float TimeAlive()
	{
		return lifeStory.secondsAlive;
	}

	public override void Hurt(HitInfo info)
	{
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0516: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a2: Unknown result type (might be due to invalid IL or missing references)
		if (IsDead() || IsTransferProtected() || (IsImmortalTo(info) && info.damageTypes.Total() >= 0f) || Interface.CallHook("IOnBasePlayerHurt", this, info) != null)
		{
			return;
		}
		bool wasWounded = IsWounded();
		if (ConVar.Server.pve && !IsNpc && Object.op_Implicit((Object)(object)info.Initiator) && info.Initiator is BasePlayer && (Object)(object)info.Initiator != (Object)(object)this)
		{
			(info.Initiator as BasePlayer).Hurt(info.damageTypes.Total(), DamageType.Generic);
			return;
		}
		if (info.damageTypes.Has(DamageType.Fun_Water))
		{
			bool flag = true;
			Item activeItem = GetActiveItem();
			if (activeItem != null && (activeItem.info.shortname == "gun.water" || activeItem.info.shortname == "pistol.water"))
			{
				float value = metabolism.wetness.value;
				metabolism.wetness.Add(ConVar.Server.funWaterWetnessGain);
				bool flag2 = metabolism.wetness.value >= ConVar.Server.funWaterDamageThreshold;
				flag = !flag2;
				if ((Object)(object)info.InitiatorPlayer != (Object)null)
				{
					if (flag2 && value < ConVar.Server.funWaterDamageThreshold)
					{
						info.InitiatorPlayer.GiveAchievement("SUMMER_SOAKED");
					}
					if (metabolism.radiation_level.Fraction() > 0.2f && !string.IsNullOrEmpty("SUMMER_RADICAL"))
					{
						info.InitiatorPlayer.GiveAchievement("SUMMER_RADICAL");
					}
				}
			}
			if (flag)
			{
				info.damageTypes.Scale(DamageType.Fun_Water, 0f);
			}
		}
		if (info.damageTypes.Has(DamageType.BeeSting))
		{
			float num = Mathf.Abs(timeSinceLastStung - Time.time);
			float num2 = 1f;
			if (num < 2f)
			{
				num2 = Mathf.Lerp(0.2f, 0.05f, Mathf.Exp((0f - num) * 1.5f));
			}
			else
			{
				num2 = 1f;
				timeSinceLastStung = Time.time;
			}
			info.damageTypes.ScaleAll(num2);
			if (baseProtection.Get(DamageType.BeeSting) > 0f)
			{
				info.damageTypes.ScaleAll(0f);
			}
		}
		if (info.damageTypes.Get(DamageType.Drowned) > 5f && drownEffect.isValid)
		{
			Effect.server.Run(drownEffect.resourcePath, this, StringPool.Get("head"), Vector3.zero, Vector3.zero);
		}
		if ((Object)(object)modifiers != (Object)null)
		{
			if (info.damageTypes.Has(DamageType.Radiation))
			{
				info.damageTypes.Scale(DamageType.Radiation, 1f - Mathf.Clamp01(modifiers.GetValue(Modifier.ModifierType.Radiation_Resistance)));
			}
			if (info.damageTypes.Has(DamageType.RadiationExposure))
			{
				info.damageTypes.Scale(DamageType.RadiationExposure, 1f - Mathf.Clamp01(modifiers.GetValue(Modifier.ModifierType.Radiation_Exposure_Resistance)));
			}
		}
		metabolism.pending_health.Subtract(info.damageTypes.Total() * 10f);
		BasePlayer initiatorPlayer = info.InitiatorPlayer;
		if (Object.op_Implicit((Object)(object)initiatorPlayer) && (Object)(object)initiatorPlayer != (Object)(object)this)
		{
			if (initiatorPlayer.InSafeZone() || InSafeZone())
			{
				initiatorPlayer.MarkHostileFor(300f);
			}
			if (!initiatorPlayer.InSafeCombatZone() && initiatorPlayer.InSafeZone() && !initiatorPlayer.IsNpc)
			{
				info.damageTypes.ScaleAll(0f);
				return;
			}
			if (initiatorPlayer.IsNpc && initiatorPlayer.Family == BaseNpc.AiStatistics.FamilyEnum.Murderer && info.damageTypes.Get(DamageType.Explosion) > 0f)
			{
				info.damageTypes.ScaleAll(Halloween.scarecrow_beancan_vs_player_dmg_modifier);
			}
		}
		if ((Object)(object)initiatorPlayer != (Object)null && !initiatorPlayer.IsNpc && !IsNpc)
		{
			float num3 = 1f / Mathf.Max(float.MinValue, ConVar.Server.pvp_ttk_global);
			float num4 = 1f / Mathf.Max(float.MinValue, ConVar.Server.pvp_ttk_bullet);
			float num5 = 1f / Mathf.Max(float.MinValue, ConVar.Server.pvp_ttk_melee);
			if (num3 != 1f)
			{
				info.damageTypes.ScaleAll(num3);
			}
			if (num4 != 1f)
			{
				info.damageTypes.Scale(DamageType.Bullet, num4);
			}
			if (num5 != 1f)
			{
				info.damageTypes.Scale(DamageType.Slash, num5);
				info.damageTypes.Scale(DamageType.Blunt, num5);
				info.damageTypes.Scale(DamageType.Stab, num5);
			}
		}
		base.Hurt(info);
		if (Object.op_Implicit((Object)(object)BaseGameMode.GetActiveGameMode(serverside: true)))
		{
			BasePlayer instigator = info?.InitiatorPlayer;
			BaseGameMode.GetActiveGameMode(serverside: true).OnPlayerHurt(instigator, this, info);
		}
		if (IsRestrained && info.damageTypes.GetMajorityDamageType().InterruptsRestraintMinigame())
		{
			Handcuffs handcuffs = GetHeldEntity() as Handcuffs;
			if ((Object)(object)handcuffs != (Object)null)
			{
				handcuffs.InterruptUnlockMiniGame(wasPushedOrDamaged: true);
			}
		}
		EACServer.LogPlayerTakeDamage(this, info, wasWounded);
		PlayerInjureState = GetInjureState();
		metabolism.SendChanges();
		if (info.PointStart != Vector3.zero && (info.damageTypes.Total() >= 0f || IsGod()))
		{
			int arg = (int)info.damageTypes.GetMajorityDamageType();
			if ((Object)(object)info.Weapon != (Object)null && info.damageTypes.Has(DamageType.Bullet))
			{
				BaseProjectile component = ((Component)info.Weapon).GetComponent<BaseProjectile>();
				if ((Object)(object)component != (Object)null && component.IsSilenced())
				{
					arg = 12;
				}
			}
			ClientRPC(RpcTarget.PlayerAndSpectators("DirectionalDamage", this), info.PointStart, arg, Mathf.CeilToInt(info.damageTypes.Total()));
			if (info.damageTypes.Has(DamageType.BeeSting) && Time.time > timeSinceLastStungRPC + 2f)
			{
				ClientRPC(RpcTarget.Player("OnStungByBees", this));
				timeSinceLastStungRPC = Time.time;
			}
		}
		DeathBlow.From(info, out cachedNonSuicideHit);
	}

	public override void Heal(float amount)
	{
		if (IsCrawling())
		{
			float num = base.health;
			base.Heal(amount);
			healingWhileCrawling += base.health - num;
		}
		else
		{
			base.Heal(amount);
		}
		ProcessMissionEvent(BaseMission.MissionEventType.HEAL, 0, amount);
	}

	public static BasePlayer FindBot(ulong userId)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = bots.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if ((ulong)current.userID == userId)
				{
					return current;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return FindBotClosestMatch(userId.ToString());
	}

	public static BasePlayer FindBotClosestMatch(string name)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		Enumerator<BasePlayer> enumerator = bots.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (current.displayName.Contains(name))
				{
					return current;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return null;
	}

	public static BasePlayer FindByID(ulong userID)
	{
		using (TimeWarning.New("BasePlayer.FindByID"))
		{
			activePlayerLookup.TryGetValue(userID, out var value);
			return value;
		}
	}

	public static bool TryFindByID(ulong userID, out BasePlayer basePlayer)
	{
		basePlayer = FindByID(userID);
		return (Object)(object)basePlayer != (Object)null;
	}

	public static BasePlayer FindSleeping(ulong userID)
	{
		using (TimeWarning.New("BasePlayer.FindSleeping"))
		{
			sleepingPlayerLookup.TryGetValue(userID, out var value);
			return value;
		}
	}

	public static BasePlayer FindAwakeOrSleepingByID(ulong userID)
	{
		if (userID == 0L)
		{
			return null;
		}
		BasePlayer basePlayer = FindByID(userID);
		if (!((Object)(object)basePlayer != (Object)null))
		{
			return FindSleeping(userID);
		}
		return basePlayer;
	}

	public static bool TryFindAwakeOrSleepingByID(ulong userID, out BasePlayer basePlayer)
	{
		basePlayer = FindAwakeOrSleepingByID(userID);
		return (Object)(object)basePlayer != (Object)null;
	}

	private void ResetArgumentArray(object[] targetArray)
	{
		for (int i = 0; i < targetArray.Length; i++)
		{
			targetArray[i] = null;
		}
	}

	public void Command(string strCommand)
	{
		Command(strCommand, noParameterCommandArgs);
	}

	public void Command(string strCommand, object arg0)
	{
		singleParameterCommandArgs[0] = arg0;
		Command(strCommand, singleParameterCommandArgs);
		ResetArgumentArray(singleParameterCommandArgs);
	}

	public void Command(string strCommand, object arg0, object arg1)
	{
		doubleParameterCommandArgs[0] = arg0;
		doubleParameterCommandArgs[1] = arg1;
		Command(strCommand, doubleParameterCommandArgs);
		ResetArgumentArray(doubleParameterCommandArgs);
	}

	public void Command(string strCommand, object arg0, object arg1, object arg2)
	{
		tripleParameterCommandArgs[0] = arg0;
		tripleParameterCommandArgs[1] = arg1;
		tripleParameterCommandArgs[2] = arg2;
		Command(strCommand, tripleParameterCommandArgs);
		ResetArgumentArray(tripleParameterCommandArgs);
	}

	public void Command(string strCommand, object arg0, object arg1, object arg2, object arg3)
	{
		quadParameterCommandArgs[0] = arg0;
		quadParameterCommandArgs[1] = arg1;
		quadParameterCommandArgs[2] = arg2;
		quadParameterCommandArgs[3] = arg3;
		Command(strCommand, quadParameterCommandArgs);
		ResetArgumentArray(quadParameterCommandArgs);
	}

	public void Command(string strCommand, params object[] arguments)
	{
		if (IsBot)
		{
			BotCommand(strCommand, arguments);
		}
		if (net.connection != null)
		{
			ConsoleNetwork.SendClientCommand(net.connection, strCommand, arguments);
		}
	}

	private void BotCommand(string strCommand, params object[] arguments)
	{
		ConsoleSystem.Option server = ConsoleSystem.Option.Server;
		server.Connection = new Network.Connection
		{
			player = (MonoBehaviour)(object)this
		};
		SetPlayerFlag(PlayerFlags.IsDeveloper, b: true);
		SetPlayerFlag(PlayerFlags.IsAdmin, b: true);
		ConsoleSystem.Run(server, strCommand, arguments);
	}

	public override void OnInvalidPosition()
	{
		if (!IsDead())
		{
			Die();
		}
	}

	public static BasePlayer FindByNameOrIP(string strNameOrIDOrIP, IEnumerable<BasePlayer> list)
	{
		BasePlayer basePlayer = list.FirstOrDefault((BasePlayer x) => x.displayName.StartsWith(strNameOrIDOrIP, StringComparison.CurrentCultureIgnoreCase));
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			return basePlayer;
		}
		BasePlayer basePlayer2 = list.FirstOrDefault((BasePlayer x) => x.net != null && x.net.connection != null && x.net.connection.ipaddress == strNameOrIDOrIP);
		if (Object.op_Implicit((Object)(object)basePlayer2))
		{
			return basePlayer2;
		}
		return null;
	}

	public static BasePlayer Find(string strNameOrIDOrIP)
	{
		if (ulong.TryParse(strNameOrIDOrIP, out var result))
		{
			BasePlayer basePlayer = FindByID(result);
			if ((Object)(object)basePlayer != (Object)null)
			{
				return basePlayer;
			}
		}
		return FindByNameOrIP(strNameOrIDOrIP, (IEnumerable<BasePlayer>)activePlayerList);
	}

	public static BasePlayer FindSleeping(string strNameOrIDOrIP)
	{
		if (ulong.TryParse(strNameOrIDOrIP, out var result))
		{
			BasePlayer basePlayer = FindSleeping(result);
			if ((Object)(object)basePlayer != (Object)null)
			{
				return basePlayer;
			}
		}
		return FindByNameOrIP(strNameOrIDOrIP, (IEnumerable<BasePlayer>)sleepingPlayerList);
	}

	public static BasePlayer FindAwakeOrSleeping(string strNameOrIDOrIP)
	{
		if (ulong.TryParse(strNameOrIDOrIP, out var result))
		{
			BasePlayer basePlayer = FindByID(result);
			if ((Object)(object)basePlayer != (Object)null)
			{
				return basePlayer;
			}
			BasePlayer basePlayer2 = FindSleeping(result);
			if ((Object)(object)basePlayer2 != (Object)null)
			{
				return basePlayer2;
			}
		}
		return FindByNameOrIP(strNameOrIDOrIP, allPlayerList);
	}

	public void SendConsoleCommand(string command, params object[] obj)
	{
		ConsoleNetwork.SendClientCommand(net.connection, command, obj);
	}

	public void UpdateRadiation(float fAmount)
	{
		metabolism.radiation_level.Increase(fAmount);
	}

	public override float RadiationExposureFraction()
	{
		float num = Mathf.Clamp(baseProtection.amounts[17], -1f, Radiation.MaxExposureProtection);
		return 1f - num;
	}

	public override float RadiationProtection()
	{
		return Mathf.Clamp(baseProtection.amounts[17], -1f, Radiation.MaxExposureProtection) * 100f;
	}

	public override void OnHealthChanged(float oldvalue, float newvalue)
	{
		if (Interface.CallHook("OnPlayerHealthChange", this, oldvalue, newvalue) != null)
		{
			return;
		}
		base.OnHealthChanged(oldvalue, newvalue);
		if (base.isServer)
		{
			if (oldvalue > newvalue)
			{
				LifeStoryHurt(oldvalue - newvalue);
			}
			else
			{
				LifeStoryHeal(newvalue - oldvalue);
			}
			metabolism.isDirty = true;
		}
	}

	public void SV_ClothingChanged()
	{
		UpdateProtectionFromClothing();
		UpdateMoveSpeedFromClothing();
	}

	public bool IsNoob()
	{
		return !HasPlayerFlag(PlayerFlags.DisplaySash);
	}

	public bool HasHostileItem()
	{
		using (TimeWarning.New("BasePlayer.HasHostileItem"))
		{
			foreach (Item item in inventory.containerBelt.itemList)
			{
				if (IsHostileItem(item))
				{
					return true;
				}
			}
			foreach (Item item2 in inventory.containerMain.itemList)
			{
				if (IsHostileItem(item2))
				{
					return true;
				}
			}
			return false;
		}
	}

	public override void GiveItem(Item item, GiveItemReason reason = GiveItemReason.Generic, GiveItemOptions options = GiveItemOptions.None)
	{
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		if (reason == GiveItemReason.ResourceHarvested)
		{
			stats.Add(item.info.HarvestStatKey, item.amount, (Stats)6);
		}
		if (reason == GiveItemReason.ResourceHarvested || reason == GiveItemReason.Crafted)
		{
			ProcessMissionEvent(BaseMission.MissionEventType.HARVEST, item.info.itemid, item.amount);
		}
		int amount = item.amount;
		if (inventory.GiveItem(item, null, options))
		{
			bool infoBool = GetInfoBool("global.streamermode", defaultVal: false);
			string name = item.GetName(infoBool);
			if (!string.IsNullOrEmpty(name))
			{
				Command("note.inv", item.info.itemid, amount, name, (int)reason);
			}
			else
			{
				Command("note.inv", item.info.itemid, amount, string.Empty, (int)reason);
			}
		}
		else
		{
			item.Drop(inventory.containerMain.dropPosition, inventory.containerMain.dropVelocity);
		}
	}

	public override void AttackerInfo(DeathInfo info)
	{
		info.attackerName = displayName;
		info.attackerSteamID = userID;
	}

	public void InvalidateWorkbenchCache()
	{
		nextCheckTime = 0f;
	}

	public Workbench GetCachedCraftLevelWorkbench()
	{
		return _cachedWorkbench;
	}

	public void SendActiveWorkbenchIfChanged()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId val = (NetworkableId)(((Object)(object)_cachedWorkbench != (Object)null && _cachedWorkbench.net != null) ? _cachedWorkbench.net.ID : default(NetworkableId));
		if (!(val == lastSentActiveWorkbenchId))
		{
			lastSentActiveWorkbenchId = val;
			ClientRPC(RpcTarget.Player("RPC_SetActiveWorkbench", this), val);
		}
	}

	public virtual bool ShouldDropActiveItem()
	{
		object obj = Interface.CallHook("CanDropActiveItem", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return true;
	}

	public override void Die(HitInfo info = null)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Player.Die"))
		{
			if (!IsDead())
			{
				Handcuffs restraintItem = Belt.GetRestraintItem();
				if ((Object)(object)restraintItem != (Object)null)
				{
					restraintItem.HeldWhenOwnerDied(this);
				}
				if (InGesture)
				{
					Server_CancelGesture();
				}
				if (Belt != null && ShouldDropActiveItem())
				{
					Vector3 val = default(Vector3);
					((Vector3)(ref val))._002Ector(Random.Range(-2f, 2f), 0.2f, Random.Range(-2f, 2f));
					Belt.DropActive(GetDropPosition(), GetInheritedDropVelocity() + ((Vector3)(ref val)).normalized * 3f);
				}
				if (!WoundInsteadOfDying(info) && Interface.CallHook("OnPlayerDeath", this, info) == null)
				{
					SleepingBag.OnPlayerDeath(this);
					base.Die(info);
				}
			}
		}
	}

	public void Kick(string reason, bool reserveSlot = true)
	{
		if (IsConnected)
		{
			net.connection.canReserveSlot = reserveSlot;
			Net.sv.Kick(net.connection, reason);
			Interface.CallHook("OnPlayerKicked", this, reason, reserveSlot);
		}
	}

	public override Vector3 GetDropPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return eyes.position;
	}

	public override Vector3 GetDropVelocity()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		return GetInheritedDropVelocity() + eyes.BodyForward() * 4f + Vector3Ex.Range(-0.5f, 0.5f);
	}

	public override void ApplyInheritedVelocity(Vector3 velocity)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = GetParentEntity();
		if ((Object)(object)baseEntity != (Object)null)
		{
			ClientRPC(RpcTarget.Player("SetInheritedVelocity", this), ((Component)baseEntity).transform.InverseTransformDirection(velocity), baseEntity.net.ID);
		}
		else
		{
			((BaseEntity)this).ClientRPC(RpcTarget.Player("SetInheritedVelocity", this), velocity, default(NetworkableId));
		}
		PauseSpeedHackDetection();
	}

	public virtual void SetInfo(string key, string val)
	{
		if (IsConnected)
		{
			Interface.CallHook("OnPlayerSetInfo", net.connection, key, val);
			net.connection.info.Set(key, val);
		}
	}

	public virtual int GetInfoInt(string key, int defaultVal)
	{
		if (!IsConnected)
		{
			return defaultVal;
		}
		return net.connection.info.GetInt(key, defaultVal);
	}

	public virtual bool GetInfoBool(string key, bool defaultVal)
	{
		if (!IsConnected)
		{
			return defaultVal;
		}
		return net.connection.info.GetBool(key, defaultVal);
	}

	public virtual string GetInfoString(string key, string defaultVal)
	{
		if (!IsConnected)
		{
			return defaultVal;
		}
		return net.connection.info.GetString(key, defaultVal);
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	public void PerformanceReport(RPCMessage msg)
	{
		string text = msg.read.String();
		PerformanceReport val = msg.read.Proto<PerformanceReport>((PerformanceReport)null);
		try
		{
			if (val.user_id != UserIDString)
			{
				DebugEx.Log($"Client performance report from {this} has incorrect user_id ({UserIDString})", (StackTraceLogType)0);
				return;
			}
			switch (text)
			{
			case "json":
				DebugEx.Log(ConvertPerfReportToJSON(val), (StackTraceLogType)0);
				break;
			case "legacy":
			{
				string text2 = (val.memory_managed_heap + "MB").PadRight(9);
				string text3 = (val.memory_system + "MB").PadRight(9);
				string text4 = (val.fps.ToString("0") + "FPS").PadRight(8);
				string text5 = NumberExtensions.FormatSeconds((long)val.fps).PadRight(9);
				string text6 = UserIDString.PadRight(20);
				string text7 = val.streamer_mode.ToString().PadRight(7);
				DebugEx.Log(text2 + text3 + text4 + text5 + text7 + text6 + displayName, (StackTraceLogType)0);
				break;
			}
			case "none":
				break;
			case "rcon":
				RCon.Broadcast(RCon.LogType.ClientPerf, ConvertPerfReportToJSON(val));
				break;
			default:
				Debug.LogError((object)("Unknown PerformanceReport format '" + text + "'"));
				break;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private string ConvertPerfReportToJSON(PerformanceReport report)
	{
		return JsonConvert.SerializeObject((object)new ClientPerformanceReport
		{
			request_id = report.request_id,
			user_id = report.user_id,
			fps_average = report.fps_average,
			fps = report.fps,
			frame_id = report.frame_id,
			frame_time = report.frame_time,
			frame_time_average = report.frame_time_average,
			memory_system = report.memory_system,
			memory_collections = report.memory_collections,
			memory_managed_heap = report.memory_managed_heap,
			realtime_since_startup = report.realtime_since_startup,
			streamer_mode = report.streamer_mode,
			ping = report.ping,
			tasks_invokes = report.tasks_invokes,
			tasks_load_balancer = report.tasks_load_balancer,
			workshop_skins_queued = report.workshop_skins_queued
		});
	}

	public override bool ShouldNetworkTo(BasePlayer player)
	{
		object obj = Interface.CallHook("CanNetworkTo", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		bool flag = ShouldNetworkToSkipOcclusion(player);
		if (flag && ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion() && player.SupportsServerOcclusion() && (Object)(object)this != (Object)(object)player)
		{
			flag = OcclusionGetCachedVisibility(player);
		}
		return flag;
	}

	public bool ShouldNetworkToSkipOcclusion(BasePlayer player)
	{
		if ((Object)(object)player == (Object)(object)this)
		{
			return true;
		}
		if (IsSpectating())
		{
			return false;
		}
		if (isInvisible)
		{
			return player.IsSpectating();
		}
		if (player.OcclusionShouldSeeAllPlayers())
		{
			return true;
		}
		return base.ShouldNetworkTo(player);
	}

	internal void GiveAchievement(string name, bool allowTutorial = false)
	{
		if (Rust.GameInfo.HasAchievements && (!IsInTutorial || allowTutorial))
		{
			ClientRPC(RpcTarget.Player("RecieveAchievement", this), name);
		}
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	public async void OnPlayerReported(RPCMessage msg)
	{
		try
		{
			string text = msg.read.String();
			string text2 = msg.read.StringMultiLine();
			string message = ((text2 != null && text2.Length > 1400) ? text2.Substring(0, 1400) : text2);
			string text3 = msg.read.String();
			string targetId = msg.read.String();
			string text4 = msg.read.String();
			DebugEx.Log(string.Format("[PlayerReport] {0} reported {1}[{2}] - \"{3}\"", new object[4] { this, text4, targetId, text }), (StackTraceLogType)0);
			RCon.Broadcast(RCon.LogType.Report, new
			{
				PlayerId = UserIDString,
				PlayerName = displayName,
				TargetId = targetId,
				TargetName = text4,
				Subject = text,
				Message = message,
				Type = text3
			});
			Interface.CallHook("OnPlayerReported", this, text4, targetId, text, text2, text3);
			if (!string.IsNullOrEmpty(ConVar.Server.reportsServerEndpoint))
			{
				ReportType type = (ReportType)3;
				if (text3.Equals("cheat"))
				{
					type = (ReportType)2;
				}
				if (text3.Equals("break_server_rules"))
				{
					type = (ReportType)6;
				}
				Feedback val = new Feedback
				{
					Subject = text,
					Message = message,
					TargetReportType = text3,
					TargetId = targetId,
					TargetName = text4,
					Type = type
				};
				DebugEx.Log("[OnPlayerReported to endpoint] " + await Feedback.ServerReport(ConVar.Server.reportsServerEndpoint, (ulong)userID, ConVar.Server.reportsServerEndpointKey, val), (StackTraceLogType)0);
			}
			BasePlayer basePlayer = FindAwakeOrSleeping(targetId);
			if ((Object)(object)basePlayer != (Object)null)
			{
				PlayerState state = basePlayer.State;
				state.numberOfTimesReported++;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[OnPlayerReported] Exception occurred when sending F7 report to endpoint: " + ex.Message));
			Debug.LogException(ex);
		}
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	public async void OnFeedbackReport(RPCMessage msg)
	{
		try
		{
			string text = msg.read.String();
			string text2 = msg.read.StringMultiLine();
			string text3 = ((text2 != null && text2.Length > 1400) ? text2.Substring(0, 1400) : text2);
			ReportType val = (ReportType)Mathf.Clamp(msg.read.Int32(), 0, 6);
			if (ConVar.Server.printReportsToConsole)
			{
				DebugEx.Log(string.Format("[FeedbackReport] {0} reported {1} - \"{2}\" \"{3}\"", new object[4] { this, val, text, text3 }), (StackTraceLogType)0);
				RCon.Broadcast(RCon.LogType.Report, new
				{
					PlayerId = UserIDString,
					PlayerName = displayName,
					Subject = text,
					Message = text3,
					Type = val
				});
			}
			Interface.CallHook("OnFeedbackReported", this, text, text2, val);
			if (!string.IsNullOrEmpty(ConVar.Server.reportsServerEndpoint))
			{
				string image = msg.read.StringMultiLine(60000);
				Feedback val2 = new Feedback
				{
					Type = val,
					Message = text3,
					Subject = text
				};
				((AppInfo)(ref val2.AppInfo)).Image = image;
				DebugEx.Log("[OnFeedbackReport to endpoint] " + await Feedback.ServerReport(ConVar.Server.reportsServerEndpoint, (ulong)userID, ConVar.Server.reportsServerEndpointKey, val2), (StackTraceLogType)0);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[OnFeedbackReport] Exception occurred when sending F7 report to endpoint: " + ex.Message));
			Debug.LogException(ex);
		}
	}

	public void StartServerDemoRecording()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		if (net != null && net.connection != null && !net.connection.IsRecording)
		{
			string text = $"demos/{UserIDString}/{DateTime.Now:yyyy-MM-dd-hhmmss}.dem";
			if (Interface.CallHook("OnDemoRecordingStart", text, this) == null)
			{
				Debug.Log((object)(((object)this).ToString() + " recording started: " + text));
				Network.Connection connection = net.connection;
				Demo.Header obj = new Demo.Header
				{
					version = Demo.Version
				};
				Scene activeScene = SceneManager.GetActiveScene();
				((DemoHeader)obj).level = ((Scene)(ref activeScene)).name;
				((DemoHeader)obj).levelSeed = World.Seed;
				((DemoHeader)obj).levelSize = World.Size;
				((DemoHeader)obj).checksum = World.Checksum;
				((DemoHeader)obj).localclient = userID;
				((DemoHeader)obj).position = eyes.position;
				((DemoHeader)obj).rotation = eyes.HeadForward();
				((DemoHeader)obj).levelUrl = World.Url;
				((DemoHeader)obj).recordedTime = DateTime.Now.ToBinary();
				connection.StartRecording(text, obj);
				SendCompleteSnapshot();
				InvokeRepeating(actionMonitorServerDemoRecording, 10f, 10f);
				Interface.CallHook("OnDemoRecordingStarted", text, this);
			}
		}
	}

	public void SendGlobalSnapshot()
	{
		using (TimeWarning.New("SendGlobalSnapshot", 10))
		{
			EnterVisibility(BaseNetworkable.GlobalNetworkGroup);
		}
	}

	public void SendCompleteSnapshot()
	{
		SendNetworkUpdateImmediate();
		SendGlobalSnapshot();
		SendSubscribedGroupsSnapshot();
		SendEntityUpdate();
		TreeManager.SendSnapshot(this);
		ServerMgr.SendReplicatedVars(net.connection);
	}

	public void StopServerDemoRecording()
	{
		if (net != null && net.connection != null && net.connection.IsRecording && Interface.CallHook("OnDemoRecordingStop", net.connection.recordFilename, this) == null)
		{
			Debug.Log((object)(((object)this).ToString() + " recording stopped: " + net.connection.RecordFilename));
			net.connection.StopRecording();
			CancelInvoke(actionMonitorServerDemoRecording);
			Interface.CallHook("OnDemoRecordingStopped", net.connection.recordFilename, this);
		}
	}

	public void MonitorServerDemoRecording()
	{
		if (net != null && net.connection != null && net.connection.IsRecording && (net.connection.RecordTimeElapsed.TotalSeconds >= (double)Demo.splitseconds || (float)net.connection.RecordFilesize >= Demo.splitmegabytes * 1024f * 1024f))
		{
			StopServerDemoRecording();
			StartServerDemoRecording();
		}
	}

	public void InvalidateCachedPeristantPlayer()
	{
		cachedPersistantPlayer = null;
	}

	public bool IsPlayerVisibleToUs(BasePlayer otherPlayer, Vector3 fromOffset, int layerMask)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)otherPlayer == (Object)null)
		{
			return false;
		}
		Vector3 val = (isMounted ? eyes.worldMountedPosition : (IsDucked() ? eyes.worldCrouchedPosition : ((!IsCrawling()) ? eyes.worldStandingPosition : eyes.worldCrawlingPosition)));
		val += fromOffset;
		if (!otherPlayer.IsVisibleSpecificLayers(val, otherPlayer.CenterPoint(), layerMask) && !otherPlayer.IsVisibleSpecificLayers(val, ((Component)otherPlayer).transform.position, layerMask) && !otherPlayer.IsVisibleSpecificLayers(val, otherPlayer.eyes.position, layerMask))
		{
			return false;
		}
		if (!IsVisibleSpecificLayers(otherPlayer.CenterPoint(), val, layerMask) && !IsVisibleSpecificLayers(((Component)otherPlayer).transform.position, val, layerMask) && !IsVisibleSpecificLayers(otherPlayer.eyes.position, val, layerMask))
		{
			return false;
		}
		return true;
	}

	protected virtual void OnKilledByPlayer(BasePlayer p)
	{
	}

	public override void OnKilled()
	{
		CancelInvoke(OfflineMetabolism);
		base.OnKilled();
	}

	public int GetIdealSlot(BasePlayer player, ItemContainer container, Item item)
	{
		if (container.HasFlag(ItemContainer.Flag.Clothing))
		{
			if (item.IsBackpack())
			{
				return 7;
			}
			if (!item.info.isWearable)
			{
				return -1;
			}
			foreach (Item item2 in container.itemList)
			{
				if (!item2.info.ItemModWearable.CanExistWith(item.info.ItemModWearable) && item2.position == 7 == item.IsBackpack())
				{
					return item2.position;
				}
			}
		}
		return -1;
	}

	public ItemContainerId GetIdealContainer(BasePlayer looter, Item item, ItemMoveModifier modifier)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Invalid comparison between Unknown and I4
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I4
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		bool flag = (modifier & 2) != 2 && looter.inventory.loot.containers.Count > 0;
		ItemContainer parent = item.parent;
		BaseEntity baseEntity = parent?.GetEntityOwner();
		Item activeItem = looter.GetActiveItem();
		Item backpackWithInventory = inventory.GetBackpackWithInventory();
		bool flag2 = backpackWithInventory != null && backpackWithInventory == item.parentItem;
		bool flag3 = false;
		if ((modifier & 0x10) == 16 && (Object)(object)looter == (Object)(object)this && backpackWithInventory != null)
		{
			if (backpackWithInventory.contents.HasSpaceFor(item))
			{
				if (!flag)
				{
					if (item.parentItem == null || !item.parentItem.IsBackpack() || item.parentItem.parent != inventory.containerWear)
					{
						return backpackWithInventory.contents.uid;
					}
				}
				else if (inventory.loot.FindItem(item.uid) != null && !inventory.containerMain.HasSpaceFor(item))
				{
					return backpackWithInventory.contents.uid;
				}
			}
			else
			{
				flag3 = true;
			}
		}
		if (activeItem != null && !flag3 && !flag && activeItem.contents != null && activeItem.contents != item.parent && activeItem.contents.capacity > 0 && activeItem.contents.CanAcceptItem(item, -1) == ItemContainer.CanAcceptResult.CanAccept)
		{
			return activeItem.contents.uid;
		}
		if (item.info.isWearable && item.info.ItemModWearable.equipOnRightClick && item.parent != inventory.containerWear && !flag && !flag2)
		{
			if (flag3)
			{
				if ((Object)(object)baseEntity != (Object)(object)this)
				{
					if (!inventory.containerMain.IsFull())
					{
						return inventory.containerMain.uid;
					}
					if (!inventory.containerWear.IsFull())
					{
						return inventory.containerWear.uid;
					}
				}
				return ItemContainerId.Invalid;
			}
			if (backpackWithInventory == null || item.parent != backpackWithInventory.contents)
			{
				return inventory.containerWear.uid;
			}
		}
		if (parent == inventory.containerMain)
		{
			if (flag)
			{
				return default(ItemContainerId);
			}
			return inventory.containerBelt.uid;
		}
		if (parent == inventory.containerWear)
		{
			return inventory.containerMain.uid;
		}
		if (parent == inventory.containerBelt)
		{
			return inventory.containerMain.uid;
		}
		return default(ItemContainerId);
	}

	private BaseVehicle GetVehicleParent()
	{
		BaseVehicle mountedVehicle = GetMountedVehicle();
		if ((Object)(object)mountedVehicle != (Object)null)
		{
			return mountedVehicle;
		}
		BaseEntity baseEntity = GetParentEntity();
		if ((Object)(object)baseEntity != (Object)null && baseEntity is BaseVehicle result)
		{
			return result;
		}
		return null;
	}

	private void RemoveLoadingPlayerFlag()
	{
		if (IsLoadingAfterTransfer())
		{
			SetPlayerFlag(PlayerFlags.LoadingAfterTransfer, b: false);
			if (IsSleeping())
			{
				SetPlayerFlag(PlayerFlags.Sleeping, b: false);
				StartSleeping();
			}
		}
	}

	public bool InNoRespawnZone()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		Vector3 position = ((Component)this).transform.position;
		if (triggers != null)
		{
			for (int i = 0; i < triggers.Count; i++)
			{
				TriggerNoRespawnZone triggerNoRespawnZone = triggers[i] as TriggerNoRespawnZone;
				if (!((Object)(object)triggerNoRespawnZone == (Object)null))
				{
					flag = triggerNoRespawnZone.InNoRespawnZone(position, checkRadius: false);
					if (flag)
					{
						break;
					}
				}
			}
		}
		return flag;
	}

	private void SendCargoPatrolPath()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		if (!BaseBoat.generate_paths)
		{
			return;
		}
		if (cachedOceanPaths == null)
		{
			cachedOceanPaths = Pool.Get<OceanPaths>();
			cachedOceanPaths.cargoPatrolPath = TerrainMeta.Path.OceanPatrolFar;
			cachedOceanPaths.harborApproaches = new List<VectorList>();
			for (int i = 0; i < CargoShip.TotalAvailableHarborDockingPaths; i++)
			{
				VectorList val = new VectorList();
				val.vectorPoints = CargoShip.GetCargoApproachPath(i);
				cachedOceanPaths.harborApproaches.Add(val);
			}
		}
		ClientRPC(RpcTarget.Player("ReceiveCargoPatrolPath", this), cachedOceanPaths);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void RPC_ReqDoRestrainedPush(RPCMessage rpc)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (IsSleeping() || IsDead() || !IsRestrained)
		{
			return;
		}
		BasePlayer player = rpc.player;
		if ((Object)(object)player == (Object)null || (Object)(object)player == (Object)(object)this)
		{
			return;
		}
		Handcuffs handcuffs = GetHeldEntity() as Handcuffs;
		if ((Object)(object)handcuffs != (Object)null)
		{
			handcuffs.InterruptUnlockMiniGame(wasPushedOrDamaged: true);
			handcuffs.RepairOnPush();
		}
		if (isMounted)
		{
			BaseMountable baseMountable = GetMounted();
			if ((Object)(object)baseMountable != (Object)null)
			{
				baseMountable.DismountPlayer(this);
				return;
			}
		}
		Vector3 val = player.eyes.BodyForward() * 10f;
		val.y = 0f;
		val += Vector3.up * 3f;
		DoPush(val, isRestrained: true);
		Hurt(Handcuffs.restrainedPushDamage, DamageType.Generic, player, useProtection: false);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void RPC_ReqRemoveCuffs(RPCMessage rpc)
	{
		if (IsDead() || !IsRestrained)
		{
			return;
		}
		BasePlayer player = rpc.player;
		if (!((Object)(object)player == (Object)null) && !((Object)(object)player == (Object)(object)this))
		{
			Handcuffs handcuffs = GetHeldEntity() as Handcuffs;
			if ((Object)(object)handcuffs != (Object)null)
			{
				handcuffs.UnlockAndReturnToPlayer(player);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	private void RPC_ReqRemoveHood(RPCMessage rpc)
	{
		BasePlayer player = rpc.player;
		if (!((Object)(object)player == (Object)null) && !((Object)(object)player == (Object)(object)this))
		{
			RemoveAndReturnPrisonerHood(player);
		}
	}

	private void RemoveAndReturnPrisonerHood(BasePlayer returnToPlayer)
	{
		if (!((Object)(object)returnToPlayer == (Object)null) && !IsDead() && IsRestrained)
		{
			Item equippedPrisonerHoodItem = inventory.GetEquippedPrisonerHoodItem();
			if (equippedPrisonerHoodItem != null)
			{
				bool isLocked = inventory.containerWear.IsLocked();
				inventory.containerWear.SetLocked(isLocked: false);
				returnToPlayer.GiveItem(equippedPrisonerHoodItem);
				inventory.containerWear.SetLocked(isLocked);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	private void RPC_ReqEquipHood(RPCMessage rpc)
	{
		BasePlayer player = rpc.player;
		if (!((Object)(object)player == (Object)null))
		{
			EquipPrisonerHood(player);
		}
	}

	private void EquipPrisonerHood(BasePlayer placingPlayer)
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)placingPlayer == (Object)null || IsDead() || !IsRestrained || (Object)(object)inventory == (Object)null || inventory.GetEquippedPrisonerHoodItem() != null)
		{
			return;
		}
		Item usableHoodItem = placingPlayer.inventory.GetUsableHoodItem();
		if (usableHoodItem == null)
		{
			return;
		}
		inventory.SetLockedByRestraint(flag: false);
		if (!usableHoodItem.MoveToContainer(inventory.containerBelt))
		{
			Item slot = inventory.containerBelt.GetSlot(0);
			if (slot != null && slot == Belt.GetRestraintItem()?.GetItem())
			{
				slot = inventory.containerBelt.GetSlot(1);
			}
			if (slot != null)
			{
				if (!slot.MoveToContainer(inventory.containerMain))
				{
					slot.DropAndTossUpwards(((Component)this).transform.position);
				}
				usableHoodItem.MoveToContainer(inventory.containerBelt);
			}
		}
		inventory.SetLockedByRestraint(flag: true);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	private void RPC_ReqForceMountNearest(RPCMessage rpc)
	{
		BasePlayer player = rpc.player;
		if (!((Object)(object)player == (Object)null))
		{
			ForceRestrainedMountNearest(player);
		}
	}

	private void ForceRestrainedMountNearest(BasePlayer forcingPlayer)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)forcingPlayer == (Object)null || isMounted || !IsRestrained || IsDead() || IsSleeping() || IsWounded())
		{
			return;
		}
		List<BaseMountable> list = Pool.Get<List<BaseMountable>>();
		Vis.Entities(((Component)this).transform.position, 2f, list, -1, (QueryTriggerInteraction)2);
		list.Sort(delegate(BaseMountable a, BaseMountable b)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = ((Component)this).transform.position - ((Component)a).transform.position;
			float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
			val = ((Component)this).transform.position - ((Component)b).transform.position;
			return sqrMagnitude.CompareTo(((Vector3)(ref val)).sqrMagnitude);
		});
		foreach (BaseMountable item in list)
		{
			if (item.isClient || !item.AllowForceMountWhenRestrained || (Object)(object)item.VehicleParent() != (Object)null || !item.DirectlyMountable() || item.Distance(eyes.position) > 3f || !GamePhysics.LineOfSight(eyes.center, eyes.position, 1218519041) || (!item.IsVisible(eyes.HeadRay(), 1218519041, 3f) && !item.IsVisible(eyes.position, 3f)))
			{
				continue;
			}
			bool flag = false;
			ModularCar modularCar = item as ModularCar;
			if ((Object)(object)modularCar != (Object)null && modularCar.CarLock.HasALock)
			{
				flag = !modularCar.CarLock.HasLockPermission(this);
				if (modularCar.CarLock.HasLockPermission(forcingPlayer))
				{
					modularCar.CarLock.TryAddPlayer(userID);
				}
			}
			item.AttemptMount(this);
			if ((Object)(object)modularCar != (Object)null && modularCar.CarLock.HasALock && flag)
			{
				modularCar.CarLock.TryRemovePlayer(userID);
			}
			if (isMounted)
			{
				break;
			}
		}
		Pool.FreeUnmanaged<BaseMountable>(ref list);
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	private void RPC_ReqForceSwapSeat(RPCMessage rpc)
	{
		if (!isMounted || !IsRestrained || IsDead() || IsSleeping() || IsWounded() || (Object)(object)rpc.player == (Object)null)
		{
			return;
		}
		BasePlayer player = rpc.player;
		BaseMountable baseMountable = GetMounted();
		if ((Object)(object)baseMountable == (Object)null)
		{
			return;
		}
		BaseVehicle baseVehicle = ((Component)baseMountable).GetComponent<BaseVehicle>();
		if ((Object)(object)baseVehicle == (Object)null)
		{
			baseVehicle = baseMountable.VehicleParent();
		}
		if ((Object)(object)baseVehicle == (Object)null)
		{
			return;
		}
		bool flag = false;
		ModularCar modularCar = baseVehicle as ModularCar;
		if ((Object)(object)modularCar != (Object)null && modularCar.CarLock.HasALock)
		{
			flag = !modularCar.CarLock.HasLockPermission(this);
			if (modularCar.CarLock.HasLockPermission(player))
			{
				modularCar.CarLock.TryAddPlayer(userID);
			}
		}
		baseVehicle.SwapSeats(this, 0, forcingRestrainedPlayer: true);
		if ((Object)(object)modularCar != (Object)null && modularCar.CarLock.HasALock && flag)
		{
			modularCar.CarLock.TryRemovePlayer(userID);
		}
	}

	public PlayerInventory.CanMoveFromResponse CanMoveFrom(BasePlayer player, Item item)
	{
		if (IsRestrainedOrSurrendering)
		{
			ItemContainer itemContainer = item?.parent;
			if (itemContainer == null)
			{
				return PlayerInventory.CanMoveFromResponse.Success();
			}
			if (itemContainer.IsLocked())
			{
				return PlayerInventory.CanMoveFromResponse.Failure(PlayerInventoryErrors.InventoryLockedError);
			}
			if (itemContainer == inventory.containerBelt && item.IsOn() && (Object)(object)((Component)item.info).GetComponent<ItemModRestraint>() != (Object)null)
			{
				return PlayerInventory.CanMoveFromResponse.Failure(TakingRestraintItemError);
			}
		}
		return PlayerInventory.CanMoveFromResponse.Success();
	}

	public void GetAllInventories(List<ItemContainer> list)
	{
		list.Add(inventory.containerMain);
		list.Add(inventory.containerBelt);
		list.Add(inventory.containerWear);
	}

	public void DoPush(Vector3 force, bool isRestrained = false)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		AddTempSpeedHackBudget(5f, 2f);
		PauseTickDistanceDetection(2f);
		ClientRPC(RpcTarget.Player(isRestrained ? "RPC_DoRestrainedPush" : "RPC_DoPush", this), force);
	}

	public override bool SupportsServerOcclusion()
	{
		if (!IsNpc && !IsBot)
		{
			return !RustRelayFakePlayer.IsFakePlayer(this);
		}
		return false;
	}

	public void OnEnterDeepSea()
	{
		Facepunch.Rust.Analytics.Azure.OnDeepSeaTraverse(this, entering: true, PointEntity<DeepSeaManager>.ServerInstance.TimeToWipe);
	}

	public void OnExitDeepSea()
	{
		Facepunch.Rust.Analytics.Azure.OnDeepSeaTraverse(this, entering: false, PointEntity<DeepSeaManager>.ServerInstance.TimeToWipe);
	}

	public override bool EnterTrigger(TriggerBase trigger)
	{
		if (trigger is TriggerLadder)
		{
			onLadderCount++;
		}
		return base.EnterTrigger(trigger);
	}

	public override void LeaveTrigger(TriggerBase trigger)
	{
		if (trigger is TriggerLadder)
		{
			onLadderCount--;
		}
		base.LeaveTrigger(trigger);
	}

	public void FreeUnoccludedSubscribers()
	{
		if (unoccludedSubscribers != null)
		{
			Pool.FreeUnmanaged<Network.Connection>(ref unoccludedSubscribers);
		}
	}

	protected override bool OcclusionLeavePlayersGroup(BaseNetworkable other)
	{
		bool result = base.OcclusionLeavePlayersGroup(other);
		if (other is BasePlayer basePlayer)
		{
			lastPlayerVisibility.Remove(basePlayer.net.ID.Value);
		}
		return result;
	}

	private static void ServerUpdateOcclusionParallel(in PlayerServerStates.ReadOnly playerStates, float networkTime)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
		if (objects.Length == 0)
		{
			OcclusionFrameCache.Clear();
			return;
		}
		OcclusionFrameCache.Clear();
		RecalculateOcclusionPositions(objects, playerStates.PlayerPos);
		int num = objects.Length * 8;
		BufferList<OcclusionPlayerPair> val = Pool.Get<BufferList<OcclusionPlayerPair>>();
		if (val.Capacity < num)
		{
			val.Resize(num);
		}
		BufferList<OcclusionPlayerPair> val2 = Pool.Get<BufferList<OcclusionPlayerPair>>();
		if (ConVar.Server.UsePlayerUpdateJobs >= 4)
		{
			GatherPairsParallel(playerStates.PlayerCache, playerStates.PlayerPos, val, val2, networkTime, DeepSea.enabled);
		}
		else
		{
			GatherPairs(objects, val, val2, networkTime);
		}
		BufferList<OcclusionPlayerPair> val3 = Pool.Get<BufferList<OcclusionPlayerPair>>();
		if (val.Count > 0)
		{
			using (TimeWarning.New("Run Occlusion Checks"))
			{
				NativeArray<bool> results = default(NativeArray<bool>);
				results._002Ector(val.Count, (Allocator)3, (NativeArrayOptions)0);
				OcclusionLineOfSight(val.ContentReadOnlySpan(), results);
				for (int i = 0; i < val.Count; i++)
				{
					OcclusionPlayerPair occlusionPlayerPair = val[i];
					if (results[i])
					{
						if (occlusionPlayerPair.from.IsConnected)
						{
							occlusionPlayerPair.from.unoccludedSubscribers.Add(occlusionPlayerPair.to.net.connection);
						}
						OcclusionFrameCache.Add((occlusionPlayerPair.from.net.ID.Value, occlusionPlayerPair.to.net.ID.Value));
						val2.Add(occlusionPlayerPair);
					}
					else
					{
						val3.Add(occlusionPlayerPair);
					}
				}
				results.Dispose();
			}
		}
		if (val2.Count + val3.Count > 0)
		{
			OcclusionSendUpdates(val2.ContentReadOnlySpan(), val3.ContentReadOnlySpan(), networkTime);
		}
		Pool.FreeUnmanaged<OcclusionPlayerPair>(ref val);
		Pool.FreeUnmanaged<OcclusionPlayerPair>(ref val2);
		Pool.FreeUnmanaged<OcclusionPlayerPair>(ref val3);
		static void RecalculateOcclusionPositions(ReadOnlySpan<BasePlayer> players, ReadOnly<Vector3> playerPos)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("Recalculate player grid positions"))
			{
				ReadOnlySpan<BasePlayer> readOnlySpan = players;
				for (int j = 0; j < readOnlySpan.Length; j++)
				{
					BasePlayer basePlayer = readOnlySpan[j];
					Vector3 position = playerPos[basePlayer.ActivePlayerInd] + PlayerEyes.EyeOffset;
					basePlayer.SubGrid = ServerOcclusion.GetSubGrid(position);
					basePlayer.Chunk = ServerOcclusion.GetGrid(position);
					basePlayer.OcclusionResetUnoccludedSubscribers();
				}
			}
		}
	}

	public static void GatherPairs(ReadOnlySpan<BasePlayer> players, BufferList<OcclusionPlayerPair> pairsToCheck, BufferList<OcclusionPlayerPair> pairsFound, float networkTime)
	{
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Gather Occlusion Pairs For Checking"))
		{
			ReadOnlySpan<BasePlayer> readOnlySpan = players;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				BasePlayer basePlayer = readOnlySpan[i];
				basePlayer.unoccludedSubscribers.Add(basePlayer.net.connection);
				Enumerator<BaseNetworkable> enumerator;
				if (basePlayer.IsSpectating())
				{
					if (!(basePlayer.net.SubStrategy is SpectatorSubStrategy spectatorSubStrategy))
					{
						continue;
					}
					ServerOcclusion.Group value = null;
					if ((Object)(object)spectatorSubStrategy.SpectatedPlayer != (Object)null)
					{
						value = spectatorSubStrategy.SpectatedPlayer.OcclusionGroup;
					}
					else if (spectatorSubStrategy.LastGroup != null)
					{
						ServerOcclusion.Occludees.TryGetValue(spectatorSubStrategy.LastGroup, out value);
					}
					if (value == null)
					{
						continue;
					}
					enumerator = ((ListHashSet<BaseNetworkable>)value).GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							BasePlayer basePlayer2 = enumerator.Current as BasePlayer;
							if (!((Object)(object)basePlayer2 == (Object)null) && !((Object)(object)basePlayer == (Object)(object)basePlayer2))
							{
								if (basePlayer2.IsConnected)
								{
									basePlayer2.unoccludedSubscribers.Add(basePlayer.net.connection);
								}
								OcclusionFrameCache.Add((basePlayer2.net.ID.Value, basePlayer.net.ID.Value));
							}
						}
					}
					finally
					{
						((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
					}
					continue;
				}
				ServerOcclusion.Group obj = basePlayer.OcclusionGroup;
				if (obj == null || ((ListHashSet<BaseNetworkable>)obj).Count <= 1)
				{
					continue;
				}
				enumerator = ((ListHashSet<BaseNetworkable>)obj).GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						BasePlayer basePlayer3 = enumerator.Current as BasePlayer;
						if ((Object)(object)basePlayer3 == (Object)null || (Object)(object)basePlayer == (Object)(object)basePlayer3)
						{
							continue;
						}
						bool flag = true;
						bool flag2 = ConVar.AntiHack.server_occlusion_disable_sleeper_los;
						if (basePlayer3.IsConnected)
						{
							flag = CustomShouldNetworkTo(basePlayer3, basePlayer);
							flag2 = false;
							if (flag)
							{
								flag2 = CustomShouldSkipServerOcclusion(basePlayer3, basePlayer);
							}
						}
						if (!flag)
						{
							continue;
						}
						OcclusionLastSeenStatus occlusionLastSeenStatus = basePlayer.OcclusionGetRecentlySeen(basePlayer3, networkTime);
						OcclusionPlayerPair occlusionPlayerPair = new OcclusionPlayerPair
						{
							from = basePlayer3,
							to = basePlayer,
							lastSeenStatus = occlusionLastSeenStatus
						};
						if (occlusionLastSeenStatus == OcclusionLastSeenStatus.Valid)
						{
							if (occlusionPlayerPair.from.IsConnected)
							{
								occlusionPlayerPair.from.unoccludedSubscribers.Add(occlusionPlayerPair.to.net.connection);
							}
							OcclusionFrameCache.Add((occlusionPlayerPair.from.net.ID.Value, occlusionPlayerPair.to.net.ID.Value));
						}
						else if (flag2)
						{
							if (occlusionPlayerPair.from.IsConnected)
							{
								occlusionPlayerPair.from.unoccludedSubscribers.Add(occlusionPlayerPair.to.net.connection);
							}
							pairsFound.Add(occlusionPlayerPair);
							OcclusionFrameCache.Add((occlusionPlayerPair.from.net.ID.Value, occlusionPlayerPair.to.net.ID.Value));
						}
						else
						{
							pairsToCheck.Add(occlusionPlayerPair);
						}
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
				}
			}
		}
	}

	private static bool CustomShouldNetworkTo(BasePlayer from, BasePlayer to)
	{
		if (from.IsSpectating())
		{
			return false;
		}
		if (from.isInvisible)
		{
			return to.IsSpectating();
		}
		if (from.limitNetworking)
		{
			BaseEntity baseEntity = from.GetParentEntity();
			if ((Object)(object)baseEntity == (Object)null)
			{
				return false;
			}
			if ((Object)(object)baseEntity != (Object)(object)to)
			{
				return false;
			}
		}
		if (from.ShouldInheritNetworkGroup())
		{
			BaseEntity baseEntity2 = from.GetParentEntity();
			if ((Object)(object)baseEntity2 != (Object)null)
			{
				return baseEntity2.ShouldNetworkTo(to);
			}
		}
		return true;
	}

	private static bool CustomShouldSkipServerOcclusion(BasePlayer from, BasePlayer to)
	{
		if (from.SubGrid.Equals(default(ServerOcclusion.SubGrid)) || to.SubGrid.Equals(default(ServerOcclusion.SubGrid)))
		{
			return true;
		}
		if (from.SubGrid.GetDistance(to.SubGrid) < ServerOcclusion.MinOcclusionDistance)
		{
			return true;
		}
		if (from.ShouldSkipServerOcclusion(to))
		{
			return true;
		}
		return false;
	}

	private static bool CustomShouldSkipServerOcclusionParallel(BasePlayer from, BasePlayer to, bool observerShouldSkipOcclusion)
	{
		if (from.SubGrid.Equals(default(ServerOcclusion.SubGrid)) || to.SubGrid.Equals(default(ServerOcclusion.SubGrid)))
		{
			return true;
		}
		if (from.SubGrid.GetDistance(to.SubGrid) < ServerOcclusion.MinOcclusionDistance)
		{
			return true;
		}
		return observerShouldSkipOcclusion;
	}

	public static void GatherPairsParallel(StableObjectArray<BasePlayer> playerCache, ReadOnly<Vector3> playerPoses, BufferList<OcclusionPlayerPair> pairsToCheck, BufferList<OcclusionPlayerPair> pairsFound, float networkTime, bool deepSeaEnabled)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Gather Occlusion Pairs For Checking (Parallel)"))
		{
			int length = playerCache.Objects.Length;
			int num = Mathf.Max(1, ConVar.Server.OcclusionGatherBatchPlayerCount);
			int num2 = (length + num - 1) / num;
			BufferList<OcclusionPairWorkerBuffers> val = Pool.Get<BufferList<OcclusionPairWorkerBuffers>>();
			for (int i = 0; i < num2; i++)
			{
				val.Add(Pool.Get<OcclusionPairWorkerBuffers>());
			}
			using (TimeWarning.New("UniTask Accumulation"))
			{
				PooledList<UniTask> val2 = Pool.Get<PooledList<UniTask>>();
				try
				{
					for (int j = 0; j < num2; j++)
					{
						int num3 = j * num;
						int count = Math.Min(num, length - num3);
						OcclusionPairWorkerBuffers buffers = val[j];
						((List<UniTask>)(object)val2).Add(GatherOcclusionPairsChunk(playerCache, playerPoses, num3, count, networkTime, deepSeaEnabled, buffers));
					}
					WaitForTasks((List<UniTask>)(object)val2);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			using (TimeWarning.New("Merge Occlusion Pairs"))
			{
				for (int k = 0; k < num2; k++)
				{
					OcclusionPairWorkerBuffers occlusionPairWorkerBuffers = val[k];
					pairsToCheck.AddSpan(occlusionPairWorkerBuffers.ToCheck.ContentReadOnlySpan());
					pairsFound.AddSpan(occlusionPairWorkerBuffers.Found.ContentReadOnlySpan());
					BufferList<(BasePlayer, BasePlayer)> subAdds = occlusionPairWorkerBuffers.SubAdds;
					for (int l = 0; l < subAdds.Count; l++)
					{
						var (basePlayer, basePlayer2) = subAdds[l];
						basePlayer.unoccludedSubscribers.Add(basePlayer2.net.connection);
					}
					BufferList<(ulong, ulong)> cacheAdds = occlusionPairWorkerBuffers.CacheAdds;
					for (int m = 0; m < cacheAdds.Count; m++)
					{
						var (item, item2) = cacheAdds[m];
						OcclusionFrameCache.Add((item, item2));
					}
				}
			}
			for (int n = 0; n < num2; n++)
			{
				OcclusionPairWorkerBuffers occlusionPairWorkerBuffers2 = val[n];
				Pool.Free<OcclusionPairWorkerBuffers>(ref occlusionPairWorkerBuffers2);
			}
			Pool.FreeUnmanaged<OcclusionPairWorkerBuffers>(ref val);
		}
	}

	[AsyncStateMachine(typeof(_003CGatherOcclusionPairsChunk_003Ed__825))]
	private static UniTask GatherOcclusionPairsChunk(StableObjectArray<BasePlayer> playerCache, ReadOnly<Vector3> observerPositions, int start, int count, float networkTime, bool deepSeaEnabled, OcclusionPairWorkerBuffers buffers)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		_003CGatherOcclusionPairsChunk_003Ed__825 _003CGatherOcclusionPairsChunk_003Ed__826 = default(_003CGatherOcclusionPairsChunk_003Ed__825);
		_003CGatherOcclusionPairsChunk_003Ed__826._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CGatherOcclusionPairsChunk_003Ed__826.playerCache = playerCache;
		_003CGatherOcclusionPairsChunk_003Ed__826.observerPositions = observerPositions;
		_003CGatherOcclusionPairsChunk_003Ed__826.start = start;
		_003CGatherOcclusionPairsChunk_003Ed__826.count = count;
		_003CGatherOcclusionPairsChunk_003Ed__826.networkTime = networkTime;
		_003CGatherOcclusionPairsChunk_003Ed__826.deepSeaEnabled = deepSeaEnabled;
		_003CGatherOcclusionPairsChunk_003Ed__826.buffers = buffers;
		_003CGatherOcclusionPairsChunk_003Ed__826._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CGatherOcclusionPairsChunk_003Ed__826._003C_003Et__builder)).Start<_003CGatherOcclusionPairsChunk_003Ed__825>(ref _003CGatherOcclusionPairsChunk_003Ed__826);
		return ((AsyncUniTaskMethodBuilder)(ref _003CGatherOcclusionPairsChunk_003Ed__826._003C_003Et__builder)).Task;
	}

	private bool ShouldSkipServerOcclusion(BasePlayer player)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return player.ComputeObserverShouldSkipOcclusion(((Component)player).transform.position, DeepSea.enabled);
	}

	public void OcclusionResetUnoccludedSubscribers()
	{
		if (unoccludedSubscribers == null)
		{
			unoccludedSubscribers = Pool.Get<List<Network.Connection>>();
		}
		else
		{
			unoccludedSubscribers.Clear();
		}
	}

	private bool ComputeObserverShouldSkipOcclusion(Vector3 pos, bool deepSeaEnabled)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		bool server_occlusion_disable_los = ConVar.AntiHack.server_occlusion_disable_los;
		bool flag = GetMounted() is ComputerStation;
		bool flag2 = OcclusionShouldSeeAllPlayers();
		bool flag3 = deepSeaEnabled && DeepSeaManager.IsInsideDeepSea(pos);
		return server_occlusion_disable_los || flag || flag2 || flag3;
	}

	public bool OcclusionLineOfSight(BasePlayer player)
	{
		ServerOcclusion.SubGrid subGrid = player.SubGrid;
		if (SubGrid.GetDistance(subGrid) < ServerOcclusion.MinOcclusionDistance)
		{
			return true;
		}
		if (SubGrid.Equals(default(ServerOcclusion.SubGrid)) || subGrid.Equals(default(ServerOcclusion.SubGrid)))
		{
			return true;
		}
		if (ConVar.AntiHack.server_occlusion_caching)
		{
			using (TimeWarning.New("OcclusionCache"))
			{
				if (ServerOcclusion.GetCachedVisibility(SubGrid, subGrid, out var result))
				{
					return result;
				}
			}
		}
		using (TimeWarning.New("CalculatePathBetweenGrids"))
		{
			ServerOcclusion.CalculatePathBetweenGrids(SubGrid, subGrid, out var pathBlocked);
			if (ConVar.AntiHack.server_occlusion_caching)
			{
				ServerOcclusion.CacheVisibility(SubGrid, subGrid, !pathBlocked);
			}
			return !pathBlocked;
		}
	}

	public static void OcclusionLineOfSight(ReadOnlySpan<OcclusionPlayerPair> pairsToCheck, NativeArray<bool> results)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		NativeList<(ServerOcclusion.SubGrid, ServerOcclusion.SubGrid)> val = default(NativeList<(ServerOcclusion.SubGrid, ServerOcclusion.SubGrid)>);
		val._002Ector(pairsToCheck.Length, AllocatorHandle.op_Implicit((Allocator)3));
		NativeList<int> val2 = default(NativeList<int>);
		val2._002Ector(pairsToCheck.Length, AllocatorHandle.op_Implicit((Allocator)2));
		NativeHashMap<long, int> val3 = default(NativeHashMap<long, int>);
		val3._002Ector(pairsToCheck.Length, AllocatorHandle.op_Implicit((Allocator)2));
		NativeList<(int, int)> val4 = default(NativeList<(int, int)>);
		val4._002Ector(pairsToCheck.Length, AllocatorHandle.op_Implicit((Allocator)2));
		int item = default(int);
		for (int i = 0; i < pairsToCheck.Length; i++)
		{
			BasePlayer basePlayer = pairsToCheck[i].from;
			BasePlayer to = pairsToCheck[i].to;
			if (ConVar.AntiHack.server_occlusion_caching && ServerOcclusion.GetCachedVisibility(basePlayer.SubGrid, to.SubGrid, out var flag))
			{
				results[i] = flag;
				continue;
			}
			int num = basePlayer.SubGrid.GetIndex();
			int num2 = to.SubGrid.GetIndex();
			if (num > num2)
			{
				int num3 = num2;
				int num4 = num;
				num = num3;
				num2 = num4;
			}
			long num5 = ((long)num << 32) + num2;
			if (val3.TryGetValue(num5, ref item))
			{
				val4.AddNoResize((i, item));
				continue;
			}
			val3.Add(num5, i);
			val.AddNoResize((basePlayer.SubGrid, to.SubGrid));
			val2.AddNoResize(i);
		}
		val3.Dispose();
		NativeArray<bool> pathsBlocked = default(NativeArray<bool>);
		pathsBlocked._002Ector(val.Length, (Allocator)3, (NativeArrayOptions)1);
		JobHandle val5 = ServerOcclusion.CalculatePathsBetweenGridsJob(val.AsReadOnly(), pathsBlocked);
		((JobHandle)(ref val5)).Complete();
		if (ConVar.AntiHack.server_occlusion_caching)
		{
			for (int j = 0; j < val.Length; j++)
			{
				ServerOcclusion.SubGrid item2 = val[j].Item1;
				ServerOcclusion.SubGrid item3 = val[j].Item2;
				bool flag2 = pathsBlocked[j];
				ServerOcclusion.CacheVisibility(item2, item3, !flag2);
			}
		}
		for (int k = 0; k < val.Length; k++)
		{
			int num6 = val2[k];
			bool flag3 = pathsBlocked[k];
			results[num6] = !flag3;
		}
		val.Dispose();
		pathsBlocked.Dispose();
		val2.Dispose();
		Enumerator<(int, int)> enumerator = val4.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				(int, int) current = enumerator.Current;
				int item4 = current.Item1;
				int item5 = current.Item2;
				bool flag4 = results[item5];
				results[item4] = flag4;
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		val4.Dispose();
	}

	private static void OcclusionSendUpdates(ReadOnlySpan<OcclusionPlayerPair> pairsFound, ReadOnlySpan<OcclusionPlayerPair> pairsLost, float networkTime)
	{
		BufferList<(BaseEntity, BasePlayer)> val = Pool.Get<BufferList<(BaseEntity, BasePlayer)>>();
		OcclusionGatherFoundPairsToSend(pairsFound, val, networkTime);
		BufferList<(BaseEntity, BasePlayer)> val2 = Pool.Get<BufferList<(BaseEntity, BasePlayer)>>();
		OcclusionGatherLostPairsToSend(pairsLost, val2);
		PooledList<UniTask> val3 = Pool.Get<PooledList<UniTask>>();
		try
		{
			SendEntityDestroyMessages(val2, (List<UniTask>)(object)val3);
			SendEntitySnapshotsWithChildren(val.ContentReadOnlySpan(), (List<UniTask>)(object)val3);
			WaitForTasks((List<UniTask>)(object)val3);
			Pool.FreeUnmanaged<(BaseEntity, BasePlayer)>(ref val);
			Pool.FreeUnmanaged<(BaseEntity, BasePlayer)>(ref val2);
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	private static void OcclusionGatherFoundPairsToSend(ReadOnlySpan<OcclusionPlayerPair> pairsFound, BufferList<(BaseEntity, BasePlayer)> toSendPairs, float networkTime)
	{
		ReadOnlySpan<OcclusionPlayerPair> readOnlySpan = pairsFound;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			OcclusionPlayerPair occlusionPlayerPair = readOnlySpan[i];
			occlusionPlayerPair.to.lastPlayerVisibility[occlusionPlayerPair.from.net.ID.Value] = networkTime;
			if (occlusionPlayerPair.lastSeenStatus == OcclusionLastSeenStatus.None)
			{
				toSendPairs.Add(((BaseEntity)occlusionPlayerPair.from, occlusionPlayerPair.to));
			}
		}
	}

	private static void OcclusionGatherLostPairsToSend(ReadOnlySpan<OcclusionPlayerPair> pairsLost, BufferList<(BaseEntity, BasePlayer)> toSendPairs)
	{
		ReadOnlySpan<OcclusionPlayerPair> readOnlySpan = pairsLost;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			OcclusionPlayerPair occlusionPlayerPair = readOnlySpan[i];
			if (occlusionPlayerPair.lastSeenStatus == OcclusionLastSeenStatus.Expired)
			{
				occlusionPlayerPair.to.lastPlayerVisibility.Remove(occlusionPlayerPair.from.net.ID.Value);
				toSendPairs.Add(((BaseEntity)occlusionPlayerPair.from, occlusionPlayerPair.to));
			}
		}
	}

	private OcclusionLastSeenStatus OcclusionGetRecentlySeen(BasePlayer player, float networkTime)
	{
		ulong value = player.net.ID.Value;
		if (lastPlayerVisibility.TryGetValue(value, out var value2))
		{
			if (networkTime - value2 < ServerOcclusion.OcclusionPollRate)
			{
				return OcclusionLastSeenStatus.Valid;
			}
			return OcclusionLastSeenStatus.Expired;
		}
		return OcclusionLastSeenStatus.None;
	}

	private bool OcclusionShouldSeeAllPlayers()
	{
		if (IsSpectating())
		{
			return true;
		}
		if (isInvisible)
		{
			return true;
		}
		if (ConVar.AntiHack.server_occlusion_admin_bypass && (IsAdmin || IsDeveloper))
		{
			return true;
		}
		return false;
	}

	public void OcclusionMakeSubscribersForget()
	{
		ulong value = net.ID.Value;
		foreach (Network.Connection subscriber in net.group.subscribers)
		{
			(subscriber.player as BasePlayer).lastPlayerVisibility.Remove(value);
		}
	}

	public bool OcclusionGetCachedVisibility(BaseEntity ent)
	{
		Debug.Assert(ent.SupportsServerOcclusion());
		return OcclusionFrameCache.Contains((net.ID.Value, ent.net.ID.Value));
	}

	public ReadOnlySpan<BasePlayer> GetSpectators()
	{
		if (IsBeingSpectated)
		{
			return (net.SubStrategy as SpectatedSubStrategy).GetSpectators();
		}
		return default(ReadOnlySpan<BasePlayer>);
	}

	public void SetSpectateTeamInfo(bool state)
	{
		IsSpectatingTeamInfo = state;
	}

	private void Tick_Spectator()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		if (serverInput.WasJustPressed(BUTTON.JUMP))
		{
			num++;
		}
		if (serverInput.WasJustPressed(BUTTON.DUCK))
		{
			num--;
		}
		if (num != 0)
		{
			SpectateOffset += num;
			using (TimeWarning.New("UpdateSpectateTarget"))
			{
				UpdateSpectateTarget(spectateFilter);
			}
		}
		if (!(TimeSince.op_Implicit(lastSpectateTeamInfoUpdate) > 0.5f) || !IsSpectatingTeamInfo)
		{
			return;
		}
		lastSpectateTeamInfoUpdate = TimeSince.op_Implicit(0f);
		SpectateTeamInfo val = Pool.Get<SpectateTeamInfo>();
		try
		{
			val.teams = Pool.Get<List<SpectateTeam>>();
			val.teams.Clear();
			foreach (KeyValuePair<ulong, RelationshipManager.PlayerTeam> team in RelationshipManager.ServerInstance.teams)
			{
				SpectateTeam val2 = Pool.Get<SpectateTeam>();
				val2.teamId = team.Key;
				val2.teamMembers = Pool.Get<List<TeamMember>>();
				val2.teamMembers.Clear();
				foreach (ulong member in team.Value.members)
				{
					TeamMember val3 = Pool.Get<TeamMember>();
					val3.userID = member;
					BasePlayer basePlayer = RelationshipManager.FindByID(member);
					val3.displayName = (((Object)(object)basePlayer != (Object)null) ? basePlayer.displayName : (SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(member) ?? "DEAD"));
					val3.healthFraction = (((Object)(object)basePlayer != (Object)null && basePlayer.IsAlive()) ? basePlayer.healthFraction : 0f);
					val3.position = (((Object)(object)basePlayer != (Object)null) ? ((Component)basePlayer).transform.position : Vector3.zero);
					val3.online = (Object)(object)basePlayer != (Object)null && !basePlayer.IsSleeping();
					val3.wounded = (Object)(object)basePlayer != (Object)null && basePlayer.IsWounded();
					val2.teamMembers.Add(val3);
				}
				val.teams.Add(val2);
			}
			ClientRPC(RpcTarget.Player("ReceiveSpectateTeamInfo", this), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void UpdateSpectateTarget(string strName, bool invalidateIfNone = false)
	{
		if (Interface.CallHook("CanSpectateTarget", this, strName) != null)
		{
			return;
		}
		BasePlayer basePlayer = this;
		bool checkName;
		using (TimeWarning.New("BasePlayer.UpdateSpectateTarget"))
		{
			spectateFilter = strName;
			checkName = !string.IsNullOrWhiteSpace(strName);
			PooledList<BasePlayer> val = Pool.Get<PooledList<BasePlayer>>();
			try
			{
				int i = 0;
				for (int count = activePlayerList.Count; i < count; i++)
				{
					BasePlayer basePlayer2 = activePlayerList[i];
					if (IsPlayerEligible(basePlayer2))
					{
						((List<BasePlayer>)(object)val).Add(basePlayer2);
					}
				}
				if (((List<BasePlayer>)(object)val).Count > 0)
				{
					((List<BasePlayer>)(object)val).Sort(DisplayNameComparison);
				}
				if (net.connection.info.GetBool("global.spectatebots"))
				{
					PooledList<BasePlayer> val2 = Pool.Get<PooledList<BasePlayer>>();
					try
					{
						int j = 0;
						for (int count2 = bots.Count; j < count2; j++)
						{
							BasePlayer basePlayer3 = bots[j];
							if (IsPlayerEligible(basePlayer3))
							{
								((List<BasePlayer>)(object)val2).Add(basePlayer3);
							}
						}
						if (((List<BasePlayer>)(object)val2).Count > 0)
						{
							((List<BasePlayer>)(object)val2).Sort(DisplayNameComparison);
							((List<BasePlayer>)(object)val).AddRange((IEnumerable<BasePlayer>)val2);
						}
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				int count3 = ((List<BasePlayer>)(object)val).Count;
				if (count3 == 0)
				{
					ChatMessage("No valid spectate targets for filter " + spectateFilter + "!");
					if (invalidateIfNone)
					{
						SpectatePlayer(null);
					}
				}
				else
				{
					BasePlayer target = ((List<BasePlayer>)(object)val)[SpectateOffset % count3];
					SpectatePlayer(target);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		bool IsPlayerEligible(BasePlayer player)
		{
			if ((Object)(object)player == (Object)(object)this)
			{
				return false;
			}
			if ((Object)(object)player == (Object)null)
			{
				return false;
			}
			if (player.IsNpc)
			{
				return false;
			}
			if (player.IsNpc || player.IsSpectating() || player.IsDead() || player.IsSleeping())
			{
				return false;
			}
			if (checkName)
			{
				if (!StringEx.Contains(player.displayName, spectateFilter, CompareOptions.IgnoreCase))
				{
					return player.UserIDString.Contains(spectateFilter);
				}
				return true;
			}
			return true;
		}
	}

	public void UpdateSpectateTarget(ulong id)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if ((Object)(object)current != (Object)null && (ulong)current.userID == id)
				{
					spectateFilter = string.Empty;
					SpectatePlayer(current);
					break;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private void DropSpectators()
	{
		ISubscriberStrategy subStrategy = net.SubStrategy;
		if (!(subStrategy is SpectatedSubStrategy spectatedSubStrategy))
		{
			if (subStrategy is SpectatorSubStrategy)
			{
				StopSpectating();
			}
			return;
		}
		ReadOnlySpan<BasePlayer> spectators = spectatedSubStrategy.GetSpectators();
		for (int num = spectators.Length - 1; num >= 0; num--)
		{
			spectators[num].SpectatePlayer(null);
		}
	}

	private void SpectatePlayer(BasePlayer target)
	{
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target == (Object)(object)this)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)spectatingTarget))
		{
			SpectatedSubStrategy spectatedSubStrategy = spectatingTarget.net.SubStrategy as SpectatedSubStrategy;
			if (spectatedSubStrategy.RemoveSpectator(this))
			{
				Pool.Free<SpectatedSubStrategy>(ref spectatedSubStrategy);
				spectatingTarget.net.SubStrategy = Network.Server.DefaultSubscriberStrategy;
			}
		}
		if ((Object)(object)target != (Object)null)
		{
			ChatMessage("Spectating: " + target.displayName + ". SteamID: " + target.UserIDString);
			if (target.net.SubStrategy is SpectatedSubStrategy spectatedSubStrategy2)
			{
				spectatedSubStrategy2.AddSpectator(this);
			}
			else
			{
				SpectatedSubStrategy spectatedSubStrategy3 = Pool.Get<SpectatedSubStrategy>();
				spectatedSubStrategy3.AddSpectator(this);
				target.net.SubStrategy = spectatedSubStrategy3;
			}
			using (TimeWarning.New("SendEntitySnapshot"))
			{
				if (ServerOcclusion.OcclusionEnabled)
				{
					OcclusionFrameCache.Add((target.net.ID.Value, net.ID.Value));
				}
				SendEntitySnapshot(target);
			}
			ClientRPC(RpcTarget.Player("SpectateTarget", this), target.net.ID);
		}
		else
		{
			((BaseEntity)this).ClientRPC(RpcTarget.Player("SpectateTarget", this), default(NetworkableId));
		}
		SpectatorSubStrategy spectatorSubStrategy = net.SubStrategy as SpectatorSubStrategy;
		if (spectatorSubStrategy == null)
		{
			net.SubStrategy = Pool.Get<SpectatorSubStrategy>();
			spectatorSubStrategy = net.SubStrategy as SpectatorSubStrategy;
		}
		spectatorSubStrategy.SpectatedPlayer = target;
		if ((Object)(object)target == (Object)null && (Object)(object)spectatingTarget != (Object)null)
		{
			spectatorSubStrategy.LastGroup = spectatingTarget.net.group;
		}
		spectatingTarget = target;
		if ((Object)(object)spectatingTarget != (Object)null && !net.subscriber.IsSubscribed(spectatingTarget.net.group))
		{
			ClearEntityQueue();
			SendEntitySnapshot(this);
			net.InvalidateSubscriptions(2);
		}
		PostSetSpectatePlayer(target);
	}

	private void PostSetSpectatePlayer(BasePlayer player)
	{
		if (!((Object)(object)player == (Object)null) && (Object)(object)player.metabolism != (Object)null)
		{
			player.metabolism.ForceSendChangesToSpectators();
		}
	}

	public void StartSpectating()
	{
		if (!IsSpectating() && Interface.CallHook("OnPlayerSpectate", this, spectateFilter) == null)
		{
			SetPlayerFlag(PlayerFlags.Spectating, b: true);
			UnityEngine.TransformEx.SetLayerRecursive(((Component)this).gameObject, 10);
			CancelInvoke(InventoryUpdate);
			ChatMessage("Becoming Spectator");
			UpdateSpectateTarget(spectateFilter, invalidateIfNone: true);
			Query.Server.RemovePlayer(this);
		}
	}

	public void StopSpectating()
	{
		if (!IsSpectating() || Interface.CallHook("OnPlayerSpectateEnd", this, spectateFilter) != null)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)spectatingTarget))
		{
			SpectatedSubStrategy spectatedSubStrategy = spectatingTarget.net.SubStrategy as SpectatedSubStrategy;
			if (spectatedSubStrategy.RemoveSpectator(this))
			{
				Pool.Free<SpectatedSubStrategy>(ref spectatedSubStrategy);
				spectatingTarget.net.SubStrategy = Network.Server.DefaultSubscriberStrategy;
			}
		}
		spectatingTarget = null;
		SpectatorSubStrategy spectatorSubStrategy = net.SubStrategy as SpectatorSubStrategy;
		Pool.Free<SpectatorSubStrategy>(ref spectatorSubStrategy);
		net.SubStrategy = Network.Server.DefaultSubscriberStrategy;
		SetPlayerFlag(PlayerFlags.Spectating, b: false);
		UnityEngine.TransformEx.SetLayerRecursive(((Component)this).gameObject, 17);
		Query.Server.RemovePlayer(this);
		Query.Server.AddPlayer(this);
	}

	public void Teleport(BasePlayer player)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Teleport(((Component)player).transform.position);
	}

	public void Teleport(string strName, bool playersOnly)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity[] array = Util.FindTargets(strName, playersOnly);
		if (array != null && array.Length != 0)
		{
			BaseEntity baseEntity = array[Random.Range(0, array.Length)];
			Teleport(((Component)baseEntity).transform.position);
		}
	}

	public void TeleportToNearestTargetEntity(string entityName, int index)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity[] array = Util.FindTargets(entityName, onlyPlayers: false);
		if (array == null || array.Length == 0)
		{
			return;
		}
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			((List<BaseEntity>)(object)val).AddRange((IEnumerable<BaseEntity>)array.OrderBy((BaseEntity entity) => Vector3.SqrMagnitude(((Component)entity).transform.position - ((Component)this).transform.position)));
			Vector3 zero = Vector3.zero;
			if (index <= 0)
			{
				zero = ((Component)((List<BaseEntity>)(object)val)[0]).transform.position;
			}
			else if (index >= array.Length)
			{
				zero = ((Component)((List<BaseEntity>)(object)val)[((List<BaseEntity>)(object)val).Count - 1]).transform.position;
			}
			else
			{
				zero = ((Component)((List<BaseEntity>)(object)val)[index]).transform.position;
			}
			Teleport(zero);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void Teleport(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		MovePosition(position);
		ClientRPC(RpcTarget.Player("ForcePositionTo", this), position);
	}

	public void CopyRotation(BasePlayer player)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		viewAngles = player.viewAngles;
		SendNetworkUpdate_Position();
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(10uL)]
	[RPC_Server.InputValidation(new Type[] { typeof(Vector3) })]
	private void UpdateSpectatePositionFromDebugCamera(RPCMessage msg)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (IsSpectating() && Global.updateNetworkPositionWithDebugCameraWhileSpectating)
		{
			Vector3 position = msg.read.Vector3();
			((Component)this).transform.position = position;
			SetParent(null);
		}
	}

	[RPC_Server]
	private void NotifyDebugCameraEnded(RPCMessage msg)
	{
		if (IsSpectating() && Global.updateNetworkPositionWithDebugCameraWhileSpectating)
		{
			UpdateSpectateTarget(spectateFilter);
		}
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		if (IsSleeping())
		{
			return false;
		}
		if (!IsAlive())
		{
			return false;
		}
		if (InSafeZone())
		{
			return false;
		}
		if ((Object)(object)splashType == (Object)null || splashType.shortname == null)
		{
			return false;
		}
		if (!((Object)(object)splashType == (Object)(object)WaterTypes.RadioactiveWaterItemDef) && !((Object)(object)splashType == (Object)(object)WaterTypes.WaterItemDef))
		{
			return (Object)(object)splashType == (Object)(object)WaterTypes.SaltWaterItemDef;
		}
		return true;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		CheckWaterRadiation(splashType, amount);
		CheckWater(splashType, amount);
		return amount;
	}

	public int DoSplashFunWater(ItemDefinition splashType, int amount)
	{
		CheckWaterRadiation(splashType, amount);
		return amount;
	}

	private void CheckWaterRadiation(ItemDefinition splashType, int amount)
	{
		if ((Object)(object)splashType == (Object)(object)WaterTypes.RadioactiveWaterItemDef)
		{
			float num = (float)amount * Radiation.MaterialToRadsRatio;
			num = Mathf.Max(num, 0.5f);
			ApplyRadiation(num);
		}
	}

	private void CheckWater(ItemDefinition splashType, int amount)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)splashType == (Object)(object)WaterTypes.WaterItemDef || (Object)(object)splashType == (Object)(object)WaterTypes.SaltWaterItemDef)
		{
			float num = (float)amount * 0.01f;
			num = Mathf.Max(num, 5f);
			timeSinceLastWaterSplash = TimeSince.op_Implicit(0f);
			if (!(baseProtection.amounts[4] > 0f))
			{
				metabolism.wetness.Add(num);
			}
		}
	}

	public void AddNeabyStash(StashContainer newStash)
	{
		if ((Object)(object)newStash == (Object)null)
		{
			return;
		}
		foreach (NearbyStash nearbyStash in nearbyStashes)
		{
			if ((Object)(object)nearbyStash.Entity == (Object)(object)newStash)
			{
				return;
			}
		}
		if (nearbyStashes.Count == 0)
		{
			InvokeRepeating(CheckStashRevealInvoke, 0f, StashContainer.PlayerDetectionTickRate);
		}
		nearbyStashes.Add(new NearbyStash(newStash));
	}

	public void RemoveNearbyStash(StashContainer stash)
	{
		for (int i = 0; i < nearbyStashes.Count; i++)
		{
			if (!((Object)(object)nearbyStashes[i].Entity != (Object)(object)stash))
			{
				nearbyStashes.RemoveAt(i);
				break;
			}
		}
		if (nearbyStashes.Count == 0)
		{
			CancelInvoke(CheckStashRevealInvoke);
		}
	}

	private void CheckStashRevealInvoke()
	{
		for (int i = 0; i < nearbyStashes.Count; i++)
		{
			NearbyStash nearbyStash = nearbyStashes[i];
			if ((Object)(object)nearbyStash.Entity == (Object)null || nearbyStash.Entity.IsDestroyed)
			{
				nearbyStashes.RemoveAt(i);
			}
			else if (nearbyStash.Entity.IsHidden() && nearbyStash.Entity.PlayerInRange(this))
			{
				nearbyStash.LookingAtTime += StashContainer.PlayerDetectionTickRate;
				if (nearbyStash.LookingAtTime >= nearbyStash.Entity.uncoverTime)
				{
					if (Interface.CallHook("CanSeeStash", this, nearbyStash.Entity) != null)
					{
						break;
					}
					nearbyStash.Entity.SetHidden(isHidden: false);
					Facepunch.Rust.Analytics.Azure.OnStashRevealed(this, nearbyStash.Entity);
					Interface.CallHook("OnStashExposed", nearbyStash.Entity, this);
				}
			}
			else
			{
				nearbyStash.LookingAtTime = 0f;
			}
		}
	}

	public override float GetThreatLevel()
	{
		EnsureUpdated();
		return cachedThreatLevel;
	}

	public void EnsureUpdated()
	{
		if (Time.realtimeSinceStartup - lastUpdateTime < 30f)
		{
			return;
		}
		lastUpdateTime = Time.realtimeSinceStartup;
		cachedThreatLevel = 0f;
		if (IsSleeping() || Interface.CallHook("OnThreatLevelUpdate", this) != null)
		{
			return;
		}
		if (inventory.containerWear.itemList.Count > 2)
		{
			cachedThreatLevel += 1f;
		}
		foreach (Item item in inventory.containerBelt.itemList)
		{
			BaseEntity heldEntity = item.GetHeldEntity();
			if (Object.op_Implicit((Object)(object)heldEntity) && heldEntity is BaseProjectile && !(heldEntity is BowWeapon))
			{
				cachedThreatLevel += 2f;
				break;
			}
		}
	}

	public override bool IsHostile()
	{
		object obj = Interface.CallHook("CanEntityBeHostile", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return State.unHostileTimestamp > TimeEx.currentTimestamp;
	}

	public virtual float GetHostileDuration()
	{
		return Mathf.Clamp((float)(State.unHostileTimestamp - TimeEx.currentTimestamp), 0f, float.PositiveInfinity);
	}

	public void SetHostilePauseTime(float duration = 2f)
	{
		hostilePauseTime = Time.realtimeSinceStartup + duration;
	}

	private bool IsHostilePaused()
	{
		return Time.realtimeSinceStartup < hostilePauseTime;
	}

	public override void MarkHostileFor(float duration = 60f)
	{
		if (Interface.CallHook("OnEntityMarkHostile", this, duration) == null && !IsHostilePaused() && !InSafeCombatZone())
		{
			duration = Mathf.Max(duration, (float)(State.unHostileTimestamp - TimeEx.currentTimestamp));
			SetHostileDuration(duration);
		}
	}

	public void SetHostileDuration(float duration)
	{
		duration = Mathf.Max(duration, 0f);
		State.unHostileTimestamp = TimeEx.currentTimestamp + (double)duration;
		DirtyPlayerState();
		ClientRPC(RpcTarget.Player("SetHostileLength", this), duration);
	}

	public void MarkWeaponDrawnDuration(float newDuration)
	{
		float num = weaponDrawnDuration;
		weaponDrawnDuration = newDuration;
		if (Mathf.FloorToInt(newDuration) != Mathf.FloorToInt(num))
		{
			ClientRPC(RpcTarget.Player("SetWeaponDrawnDuration", this), weaponDrawnDuration);
		}
	}

	public void AddWeaponDrawnDuration(float duration)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (InSafeCombatZone() || HasPlayerFlag(PlayerFlags.CombatZone))
		{
			timeLastInCombatZone = TimeSince.op_Implicit(0f);
			MarkWeaponDrawnDuration(0f);
		}
		else if (!(TimeSince.op_Implicit(timeLastInCombatZone) < 1f))
		{
			MarkWeaponDrawnDuration(weaponDrawnDuration + duration);
		}
	}

	public void OnReceivedTick(NetRead read)
	{
		using (TimeWarning.New("OnReceiveTickFromStream"))
		{
			PlayerTick val;
			using (TimeWarning.New("PlayerTick.Deserialize"))
			{
				val = read.ProtoDelta<PlayerTick>(lastReceivedTick);
			}
			using (TimeWarning.New("RecordPacket"))
			{
				net.connection.RecordPacket(15, (IProto)(object)val);
			}
			using (TimeWarning.New("PlayerTick.Copy"))
			{
				PlayerTick obj = lastReceivedTick;
				if (obj != null)
				{
					obj.Dispose();
				}
				lastReceivedTick = val.Copy();
			}
			using (TimeWarning.New("OnReceiveTick"))
			{
				OnReceiveTick(val, wasStalled);
			}
			lastTickTime = Time.time;
			rawTicksPerSecond.Increment();
			val.Dispose();
		}
	}

	public void OnReceivedVoice(ReadOnlySpan<byte> data)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		NetWrite netWrite = Net.sv.StartWrite();
		netWrite.PacketID(Message.Type.VoiceData);
		netWrite.EntityID(net.ID);
		netWrite.BytesWithSize(data);
		float num = 0f;
		if (HasPlayerFlag(PlayerFlags.VoiceRangeBoost))
		{
			num = Voice.voiceRangeBoostAmount;
		}
		List<Network.Connection> connectionsWithin = BaseNetworkable.GetConnectionsWithin(((Component)this).transform.position, 100f + num, includeInvisPlayers: true);
		ComputerStation.AddRemoteVoiceListeners(connectionsWithin, ((Component)this).transform.position, 100f + num);
		if (PacketProfiler.shouldCaptureDetailedProfiling)
		{
			BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(net.ID) as BaseEntity;
			PacketProfiler.LogDetailedOutbound(Message.Type.VoiceData, net.ID, ((Object)(object)baseEntity != (Object)null) ? baseEntity.PrefabName : null, (int)netWrite.Length, null, Epoch.Current, server: true);
		}
		netWrite.Send(new SendInfo(connectionsWithin)
		{
			priority = Priority.Immediate
		});
		if ((Object)(object)activeTelephone != (Object)null)
		{
			activeTelephone.OnReceivedVoiceFromUser(data);
		}
		if ((Object)(object)SingletonComponent<NpcNoiseManager>.Instance != (Object)null)
		{
			SingletonComponent<NpcNoiseManager>.Instance.OnVoiceChat(this);
		}
	}

	public void ResetInputIdleTime()
	{
		lastInputTime = Time.time;
	}

	internal void EACStateUpdate(in CachedState cachedState, in EACTickState tickState)
	{
		if ((cachedState.PlayerFlags & PlayerFlags.ReceivingSnapshot) == 0)
		{
			EACServer.LogPlayerTick(net, tickState);
		}
	}

	public void AddReceiveTickListener(IReceivePlayerTickListener listener)
	{
		if (receiveTickListeners != null && !receiveTickListeners.Contains(listener))
		{
			receiveTickListeners.Add(listener);
		}
	}

	public void RemoveReceiveTickListener(IReceivePlayerTickListener listener)
	{
		receiveTickListeners.Remove(listener);
	}

	private void OnReceiveTick(PlayerTick msg, bool wasPlayerStalled)
	{
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		if (msg.inputState != null)
		{
			serverInput.Flip(msg.inputState);
		}
		if (Interface.CallHook("OnPlayerTick", this, msg, wasPlayerStalled) != null)
		{
			return;
		}
		if (serverInput.current.buttons != serverInput.previous.buttons)
		{
			ResetInputIdleTime();
		}
		if (Interface.CallHook("OnPlayerInput", this, serverInput) != null || IsReceivingSnapshot)
		{
			return;
		}
		if (IsSpectating())
		{
			using (TimeWarning.New("Tick_Spectator"))
			{
				Tick_Spectator();
				return;
			}
		}
		if (IsDead())
		{
			return;
		}
		if (IsSleeping())
		{
			if (serverInput.WasJustPressed(BUTTON.FIRE_PRIMARY) || serverInput.WasJustPressed(BUTTON.FIRE_SECONDARY) || serverInput.WasJustPressed(BUTTON.JUMP) || serverInput.WasJustPressed(BUTTON.DUCK))
			{
				EndSleeping();
				SendNetworkUpdateImmediate();
			}
			UpdateActiveItem(default(ItemId));
			return;
		}
		if (IsRestrained && restraintItemId.HasValue && restraintItemId.HasValue)
		{
			UpdateActiveItem(restraintItemId.Value);
		}
		else if (!Belt.CanHoldItem())
		{
			UpdateActiveItem(default(ItemId));
		}
		else
		{
			UpdateActiveItem(msg.activeItem);
		}
		UpdateModelStateFromTick(msg);
		if (float.IsNaN(modelState.ducking) || float.IsInfinity(modelState.ducking))
		{
			Kick("Kicked: invalid modelstate");
			return;
		}
		modelState.ducking = Mathf.Clamp01(modelState.ducking);
		if (IsIncapacitated())
		{
			return;
		}
		ForwardReceiveTickToListeners(msg);
		if (isMounted)
		{
			GetMounted().PlayerServerInput(serverInput, this);
		}
		UpdatePositionFromTick(msg, wasPlayerStalled);
		UpdateRotationFromTick(msg);
		if (TryGetActiveMissionInstance(out var instance) && instance.status == BaseMission.MissionStatus.Active && instance.NeedsPlayerInput())
		{
			ProcessMissionEvent(BaseMission.MissionEventType.PLAYER_TICK, net.ID, 0f);
		}
		if (TutorialIsland.EnforceTrespassChecks && !IsAdmin && !IsNpc && net != null && net.group != null)
		{
			if (net.group.restricted)
			{
				bool flag = false;
				if (!IsInTutorial)
				{
					flag = true;
				}
				else
				{
					TutorialIsland currentTutorialIsland = GetCurrentTutorialIsland();
					if ((Object)(object)currentTutorialIsland == (Object)null || currentTutorialIsland.net.group != net.group)
					{
						flag = true;
					}
				}
				if (flag)
				{
					tutorialKickTime += Time.deltaTime;
					if (tutorialKickTime > 3f)
					{
						Debug.LogWarning((object)$"Killing player {displayName}/{userID.Get()} as they are on a tutorial island that doesn't belong them");
						Hurt(999f);
						tutorialKickTime = 0f;
					}
				}
				else
				{
					tutorialKickTime = 0f;
				}
			}
			else if (IsInTutorial && !net.group.restricted)
			{
				bool flag2 = false;
				TutorialIsland currentTutorialIsland2 = GetCurrentTutorialIsland();
				if ((Object)(object)currentTutorialIsland2 == (Object)null || currentTutorialIsland2.net.group != net.group)
				{
					flag2 = true;
				}
				if (flag2)
				{
					tutorialKickTime += Time.deltaTime;
					if (tutorialKickTime > 3f)
					{
						Debug.LogWarning((object)$"Killing player {displayName}/{userID.Get()} as they are no longer on a tutorial island and are marked as being in a tutorial");
						Hurt(999f);
						tutorialKickTime = 0f;
					}
				}
				else
				{
					tutorialKickTime = 0f;
				}
			}
		}
		if (ActivePlayerInd != -1 && EACServer.CanSendAnalytics)
		{
			CollectEACTick(msg);
		}
	}

	private void CollectEACTick(PlayerTick tick)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = tick.position;
		ModelState val = modelStateTick ?? modelState;
		Vector3 val2 = position + GetOffset(val.ducked);
		Vector3 val3 = eyes.PositionWithOverride(position);
		Quaternion val4 = eyes.parentRotation * Quaternion.Euler(tickViewAngles);
		LogPlayerTickOptions val5 = default(LogPlayerTickOptions);
		((LogPlayerTickOptions)(ref val5)).PlayerHandle = ClientHandles[ActivePlayerInd];
		Vec3f value = default(Vec3f);
		((Vec3f)(ref value)).x = val2.x;
		((Vec3f)(ref value)).y = val2.y;
		((Vec3f)(ref value)).z = val2.z;
		((LogPlayerTickOptions)(ref val5)).PlayerPosition = value;
		value = default(Vec3f);
		((Vec3f)(ref value)).x = val3.x;
		((Vec3f)(ref value)).y = val3.y;
		((Vec3f)(ref value)).z = val3.z;
		((LogPlayerTickOptions)(ref val5)).PlayerViewPosition = value;
		Quat value2 = default(Quat);
		((Quat)(ref value2)).w = val4.w;
		((Quat)(ref value2)).x = val4.x;
		((Quat)(ref value2)).y = val4.y;
		((Quat)(ref value2)).z = val4.z;
		((LogPlayerTickOptions)(ref val5)).PlayerViewRotation = value2;
		((LogPlayerTickOptions)(ref val5)).PlayerHealth = base.health;
		LogPlayerTickOptions tickOptions = val5;
		if (val.ducked)
		{
			((LogPlayerTickOptions)(ref tickOptions)).PlayerMovementState = (AntiCheatCommonPlayerMovementState)(((LogPlayerTickOptions)(ref tickOptions)).PlayerMovementState | 1);
		}
		if (isMounted)
		{
			((LogPlayerTickOptions)(ref tickOptions)).PlayerMovementState = (AntiCheatCommonPlayerMovementState)(((LogPlayerTickOptions)(ref tickOptions)).PlayerMovementState | 3);
		}
		if (val.crawling)
		{
			((LogPlayerTickOptions)(ref tickOptions)).PlayerMovementState = (AntiCheatCommonPlayerMovementState)(((LogPlayerTickOptions)(ref tickOptions)).PlayerMovementState | 2);
		}
		if (val.waterLevel >= 0.75f)
		{
			((LogPlayerTickOptions)(ref tickOptions)).PlayerMovementState = (AntiCheatCommonPlayerMovementState)(((LogPlayerTickOptions)(ref tickOptions)).PlayerMovementState | 4);
		}
		if (!val.onground)
		{
			((LogPlayerTickOptions)(ref tickOptions)).PlayerMovementState = (AntiCheatCommonPlayerMovementState)(((LogPlayerTickOptions)(ref tickOptions)).PlayerMovementState | 5);
		}
		if (val.onLadder)
		{
			((LogPlayerTickOptions)(ref tickOptions)).PlayerMovementState = (AntiCheatCommonPlayerMovementState)(((LogPlayerTickOptions)(ref tickOptions)).PlayerMovementState | 7);
		}
		if (val.flying)
		{
			((LogPlayerTickOptions)(ref tickOptions)).PlayerMovementState = (AntiCheatCommonPlayerMovementState)(((LogPlayerTickOptions)(ref tickOptions)).PlayerMovementState | 6);
		}
		int num = Mathf.Min(lastEACTickIndex++, (int)Player.clientTickRate - 1);
		int num2 = ActivePlayerInd * (int)Player.clientTickRate + num;
		EACTickState eACTickState = new EACTickState
		{
			TickOptions = tickOptions
		};
		DateTime unixEpoch = DateTime.UnixEpoch;
		eACTickState.Timestamp = unixEpoch.Ticks;
		EACTickStates[num2] = eACTickState;
	}

	private void RemoveReceiveTickListenersOnDeath()
	{
		for (int num = receiveTickListeners.Count - 1; num >= 0; num--)
		{
			IReceivePlayerTickListener receivePlayerTickListener = receiveTickListeners[num];
			if (receivePlayerTickListener == null)
			{
				receiveTickListeners.RemoveAt(num);
			}
			else if (receivePlayerTickListener.ShouldRemoveOnPlayerDeath())
			{
				receiveTickListeners.Remove(receivePlayerTickListener);
			}
		}
	}

	private void ForwardReceiveTickToListeners(PlayerTick msg)
	{
		if (receiveTickListeners == null)
		{
			return;
		}
		for (int num = receiveTickListeners.Count - 1; num >= 0; num--)
		{
			IReceivePlayerTickListener receivePlayerTickListener = receiveTickListeners[num];
			if (receivePlayerTickListener == null)
			{
				receiveTickListeners.RemoveAt(num);
			}
			else
			{
				receivePlayerTickListener.OnReceivePlayerTick(this, msg);
			}
		}
	}

	public void ApplyStallProtection(float time)
	{
		stallProtectionTime = Mathf.Max(time, stallProtectionTime);
	}

	public void UpdateActiveItem(ItemId itemID)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		Assert.IsTrue(base.isServer, "Realm should be server!");
		if (svActiveItemID == itemID)
		{
			return;
		}
		if (equippingBlocked)
		{
			itemID = default(ItemId);
		}
		Item item = inventory.containerBelt.FindItemByUID(itemID);
		if (IsItemHoldRestricted(item))
		{
			itemID = default(ItemId);
		}
		Item activeItem = GetActiveItem();
		if (Interface.CallHook("OnActiveItemChange", this, activeItem, itemID) != null)
		{
			return;
		}
		svActiveItemID = default(ItemId);
		if (activeItem != null)
		{
			HeldEntity heldEntity = activeItem.GetHeldEntity() as HeldEntity;
			if ((Object)(object)heldEntity != (Object)null)
			{
				heldEntity.SetHeld(bHeld: false);
			}
		}
		svActiveItemID = itemID;
		SendNetworkUpdate();
		Item activeItem2 = GetActiveItem();
		if (activeItem2 != null)
		{
			HeldEntity heldEntity2 = activeItem2.GetHeldEntity() as HeldEntity;
			if ((Object)(object)heldEntity2 != (Object)null)
			{
				heldEntity2.SetHeld(bHeld: true);
			}
			NotifyGesturesNewItemEquipped();
		}
		inventory.UpdatedVisibleHolsteredItems();
		Interface.CallHook("OnActiveItemChanged", this, activeItem, activeItem2);
	}

	internal void UpdateModelStateFromTick(PlayerTick tick)
	{
		if (tick.modelState != null && !ModelState.Equal(modelStateTick, tick.modelState))
		{
			if (modelStateTick != null)
			{
				modelStateTick.ResetToPool();
			}
			modelStateTick = tick.modelState;
			tick.modelState = null;
			PlayerStates.TickNeedsFinalizing[ActivePlayerInd] = true;
		}
	}

	internal void UpdatePositionFromTick(PlayerTick tick, bool wasPlayerStalled)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3Ex.IsNaNOrInfinity(tick.position) || Vector3Ex.IsNaNOrInfinity(tick.eyePos))
		{
			Kick("Kicked: Invalid Position");
		}
		else
		{
			if (tick.parentID != parentEntity.uid)
			{
				return;
			}
			ref AntiHack.PlayerState reference = ref NativeArray<AntiHack.PlayerState>.op_Implicit(ref AntiHack.PlayerStates)[ActivePlayerInd];
			float num = PlayerStates.TickDeltaTime[ActivePlayerInd];
			reference.TickDistancePausetime = Mathf.Max(0f, reference.TickDistancePausetime - num);
			if (isMounted || (modelState != null && modelState.mounted) || (modelStateTick != null && modelStateTick.mounted) || (IsWounded() && IsRestrained))
			{
				return;
			}
			if (wasPlayerStalled)
			{
				Vector3 endPoint = TickInterpolatorCache.GetEndPoint(PlayerStates.TickCache.ReadOnly, ActivePlayerInd);
				float num2 = Vector3.Distance(tick.position, endPoint);
				if (num2 > 0.01f)
				{
					AntiHack.ResetTimer(this);
				}
				if (num2 > 0.5f)
				{
					ClientRPC(RpcTarget.Player("ForcePositionToParentOffset", this), endPoint, parentEntity.uid);
				}
				return;
			}
			if (!AntiHack.ShouldIgnore(this))
			{
				Vector3 endPoint2 = TickInterpolatorCache.GetEndPoint(PlayerStates.TickCache.ReadOnly, ActivePlayerInd);
				float num3 = Vector3.Distance(tick.position, endPoint2);
				float tick_max_distance = ConVar.AntiHack.tick_max_distance;
				float num4 = ((ConVar.AntiHack.flyhack_protection <= 0 || AntiHack.PlayerFlyhackStates[ActivePlayerInd].IsInAir || RecentlyInAir()) ? ConVar.AntiHack.tick_max_distance_falling : tick_max_distance);
				float num5 = (HasParent() ? ConVar.AntiHack.tick_max_distance_parented : tick_max_distance);
				float num6 = ((AntiHack.PlayerStates[ActivePlayerInd].TickDistancePausetime > 0f) ? ConVar.AntiHack.tick_distance_forgiveness : tick_max_distance);
				float num7 = Mathx.Max(tick_max_distance, num4, num5, num6);
				if (num3 > num7)
				{
					AntiHack.Log(this, AntiHackType.Ticks, $"moved too far between ticks: {num3} units. Max dist: {num7}");
					AntiHack.ResetTimer(this);
					ClientRPC(RpcTarget.Player("ForcePositionToParentOffset", this), endPoint2, parentEntity.uid);
					return;
				}
			}
			PlayerStates.TickCache.AddTick(this, tick.position);
			PlayerStates.TickNeedsFinalizing[ActivePlayerInd] = true;
		}
	}

	internal void UpdateRotationFromTick(PlayerTick tick)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (tick.inputState != null)
		{
			if (Vector3Ex.IsNaNOrInfinity(tick.inputState.aimAngles))
			{
				Kick("Kicked: Invalid Rotation");
				return;
			}
			if (Vector3Ex.IsNaNOrInfinity(tick.inputState.mouseDelta))
			{
				Kick("Kicked: Invalid Rotation");
				return;
			}
			tickMouseDelta = tick.inputState.mouseDelta;
			tickViewAngles = tick.inputState.aimAngles;
			PlayerStates.TickNeedsFinalizing[ActivePlayerInd] = true;
		}
	}

	public void UpdateEstimatedVelocity(Vector3 lastPos, Vector3 currentPos, float deltaTime)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		estimatedVelocity = (currentPos - lastPos) / deltaTime;
		Vector3 val = estimatedVelocity;
		estimatedSpeed = ((Vector3)(ref val)).magnitude;
		estimatedSpeed2D = Vector3Ex.Magnitude2D(estimatedVelocity);
		if (estimatedSpeed < 0.01f)
		{
			estimatedSpeed = 0f;
		}
		if (estimatedSpeed2D < 0.01f)
		{
			estimatedSpeed2D = 0f;
		}
	}

	private void CheckModelState(in PlayerServerStates playerStates)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ModelState"))
		{
			if (modelStateTick == null)
			{
				return;
			}
			if (modelStateTick.inheritedVelocity != Vector3.zero && (Object)(object)FindTrigger<TriggerForce>() == (Object)null)
			{
				modelStateTick.inheritedVelocity = Vector3.zero;
			}
			if (modelState != null)
			{
				if (ConVar.AntiHack.modelstate && TriggeredAntiHack())
				{
					modelStateTick.ducked = modelState.ducked;
				}
				modelState.ResetToPool();
				modelState = null;
			}
			modelState = modelStateTick;
			modelStateTick = null;
			UpdateModelState(modelState);
			NativeArray<Flag>.op_Implicit(ref playerStates.PlayerModelStateFlags)[ActivePlayerInd] = (Flag)modelState.flags;
			NativeArray<float>.op_Implicit(ref playerStates.PlayerModelStateDucking)[ActivePlayerInd] = modelState.ducking;
		}
	}

	public static void InitInternalState(int initCap = 32)
	{
		DisposeInternalState();
		PlayerStates.Init(initCap);
		WaterLevel.InitInternalState(initCap);
		AntiHack.InitInternalState(initCap);
	}

	public static void DisposeInternalState()
	{
		PlayerStates.SafeDispose();
		NativeArrayEx.SafeDispose(ref EACTickStates);
		NativeArrayEx.SafeDispose(ref ClientHandles);
		WaterLevel.DisposeInternalState();
		AntiHack.DisposeInternalState();
	}

	private static void FinalizeTickParallel(in PlayerServerStates playerStates, float deltaTime, NativeList<int> toUpdate)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("FinalizeTickParallel"))
		{
			StableObjectArray<BasePlayer> playerCache = playerStates.PlayerCache;
			NativeList<int> indices = new NativeList<int>(playerCache.Count, AllocatorHandle.op_Implicit((Allocator)3));
			GatherPlayersToFinalize(in playerStates, deltaTime, indices);
			ReadOnlySpan<BasePlayer> objects = playerCache.Objects;
			_ = playerStates.TickCache.ReadOnly;
			ServerPreFinalize(in playerStates, indices.AsReadOnly());
			ServerCachePlayerInfo(in playerStates, indices.AsReadOnly(), recachePosDependentOnly: false);
			NativeArray<PositionChange> val = new NativeArray<PositionChange>(objects.Length, (Allocator)3, (NativeArrayOptions)0);
			NativeList<int> toValidate = new NativeList<int>(indices.Length, AllocatorHandle.op_Implicit((Allocator)3));
			BasePlayerJobs.GatherPosToValidateJob gatherPosToValidateJob = new BasePlayerJobs.GatherPosToValidateJob
			{
				Changes = val,
				ToValidate = toValidate,
				TickCache = playerStates.TickCache.ReadOnly,
				Indices = indices.AsReadOnly()
			};
			IJobExtensions.RunByRef<BasePlayerJobs.GatherPosToValidateJob>(ref gatherPosToValidateJob);
			AntiHack.ValidateMoves(playerStates.AsReadOnly(), toValidate.AsReadOnly(), val);
			NativeList<int> indicesToGather = new NativeList<int>(toValidate.Length, AllocatorHandle.op_Implicit((Allocator)3));
			GatherPlayersPosChanged(playerStates.AsReadOnly(), toValidate.AsReadOnly(), val.AsReadOnly(), indicesToGather);
			toValidate.Dispose();
			if (!indicesToGather.IsEmpty)
			{
				using (TimeWarning.New("RecachingPlayerState"))
				{
					CachePlayerTransforms(in playerStates);
					ServerCachePlayerInfo(in playerStates, indicesToGather.AsReadOnly(), recachePosDependentOnly: true);
				}
			}
			indicesToGather.Dispose();
			NativeList<int> toBroadcastIndices = new NativeList<int>(indices.Length, AllocatorHandle.op_Implicit((Allocator)2));
			NativeList<int> validIndices = new NativeList<int>(indices.Length, AllocatorHandle.op_Implicit((Allocator)2));
			ServerFinalizePlayers(in playerStates, val.AsReadOnly(), indices.AsReadOnly(), toBroadcastIndices, validIndices);
			indices.Dispose();
			GatherPlayersToUpdate(in playerStates, deltaTime, toUpdate);
			UpdateSubscriptions(playerStates.AsReadOnly(), toUpdate.AsReadOnly(), Time.realtimeSinceStartup);
			float time = Time.time;
			PooledList<UniTask> val2 = Pool.Get<PooledList<UniTask>>();
			try
			{
				if (EACServer.CanSendAnalytics)
				{
					((List<UniTask>)(object)val2).Add(UpdateEAC(playerCache, validIndices.AsReadOnly(), playerStates.CachedStates.AsReadOnly(), EACTickStates.AsReadOnly(), val.AsReadOnly()));
				}
				if (Facepunch.Rust.Analytics.GameplayTickAnalyticsConVar)
				{
					((List<UniTask>)(object)val2).Add(UpdateAnalytics(playerCache, validIndices.AsReadOnly(), playerStates.CachedStates.AsReadOnly(), playerStates.PlayerPos.AsReadOnly(), playerStates.IsMounted.AsReadOnly()));
				}
				if (ServerOcclusion.OcclusionEnabled)
				{
					ServerUpdateOcclusionParallel(playerStates.AsReadOnly(), time);
				}
				NetworkPositionTick(playerStates.AsReadOnly(), toBroadcastIndices.AsReadOnly(), time);
				WaitForTasks((List<UniTask>)(object)val2);
				toBroadcastIndices.Dispose();
				validIndices.Dispose();
				val.Dispose();
				if (EACServer.CanSendAnalytics)
				{
					FillJobUnsafe<EACTickState> fillJobUnsafe = new FillJobUnsafe<EACTickState>
					{
						Value = default(EACTickState),
						Values = EACTickStates
					};
					IJobExtensions.RunByRef<FillJobUnsafe<EACTickState>>(ref fillJobUnsafe);
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		[AsyncStateMachine(typeof(_003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed))]
		static UniTask UpdateAnalytics(StableObjectArray<BasePlayer> playerCache2, ReadOnly<int> toBroadcast, ReadOnly<CachedState> cachedStates, ReadOnly<Vector3> playerPos, ReadOnly<bool> isMounted)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			_003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed _003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed2 = default(_003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed);
			_003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed2._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
			_003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed2.playerCache = playerCache2;
			_003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed2.toBroadcast = toBroadcast;
			_003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed2.cachedStates = cachedStates;
			_003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed2.playerPos = playerPos;
			_003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed2.isMounted = isMounted;
			_003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed2._003C_003E1__state = -1;
			((AsyncUniTaskMethodBuilder)(ref _003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed2._003C_003Et__builder)).Start<_003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed>(ref _003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed2);
			return ((AsyncUniTaskMethodBuilder)(ref _003C_003CFinalizeTickParallel_003Eg__UpdateAnalytics_007C971_1_003Ed2._003C_003Et__builder)).Task;
		}
		[AsyncStateMachine(typeof(_003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed))]
		static UniTask UpdateEAC(StableObjectArray<BasePlayer> playerCache2, ReadOnly<int> validPlayers, ReadOnly<CachedState> cachedStates, ReadOnly<EACTickState> tickStates, ReadOnly<PositionChange> positionChanges)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			_003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed _003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed2 = default(_003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed);
			_003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed2._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
			_003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed2.playerCache = playerCache2;
			_003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed2.validPlayers = validPlayers;
			_003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed2.cachedStates = cachedStates;
			_003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed2.tickStates = tickStates;
			_003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed2.positionChanges = positionChanges;
			_003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed2._003C_003E1__state = -1;
			((AsyncUniTaskMethodBuilder)(ref _003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed2._003C_003Et__builder)).Start<_003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed>(ref _003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed2);
			return ((AsyncUniTaskMethodBuilder)(ref _003C_003CFinalizeTickParallel_003Eg__UpdateEAC_007C971_0_003Ed2._003C_003Et__builder)).Task;
		}
	}

	private static void GatherPlayersToFinalize(in PlayerServerStates playerStates, float deltaTime, NativeList<int> indices)
	{
		using (TimeWarning.New("GatherPlayersToFinalize"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			Span<float> span = NativeArray<float>.op_Implicit(ref playerStates.TickDeltaTime);
			Span<bool> span2 = NativeArray<bool>.op_Implicit(ref playerStates.TickNeedsFinalizing);
			ReadOnlySpan<BasePlayer> readOnlySpan = objects;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				BasePlayer basePlayer = readOnlySpan[i];
				span[basePlayer.ActivePlayerInd] += deltaTime;
				if (!basePlayer.IsReceivingSnapshot && span2[basePlayer.ActivePlayerInd])
				{
					indices.AddNoResize(basePlayer.ActivePlayerInd);
					span2[basePlayer.ActivePlayerInd] = false;
				}
			}
		}
	}

	private static void GatherPlayersPosChanged(in PlayerServerStates.ReadOnly playerStates, ReadOnly<int> indicesToCheck, ReadOnly<PositionChange> posChanges, NativeList<int> indicesToGather)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GatherPlayersPosChanged"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			TickInterpolatorCache.ReadOnlyState tickCache = playerStates.TickCache;
			Enumerator<int> enumerator = indicesToCheck.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					if (posChanges[current] == PositionChange.Valid)
					{
						indicesToGather.AddNoResize(current);
						BasePlayer basePlayer = objects[current];
						Vector3 endPoint = TickInterpolatorCache.GetEndPoint(tickCache, current);
						((Component)basePlayer).transform.localPosition = endPoint;
						basePlayer.ticksPerSecond.Increment();
						basePlayer.tickHistory.AddPoint(endPoint, basePlayer.tickHistoryCapacity);
						basePlayer.RecordParentPosition(basePlayer.tickHistoryCapacity);
						AntiHack.FadeViolations(basePlayer, playerStates.TickDeltaTime[current]);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static void ServerCachePlayerInfo(in PlayerServerStates playerStates, ReadOnly<int> indices, bool recachePosDependentOnly)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Invalid comparison between Unknown and I4
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ServerCachePlayerInfo"))
		{
			NativeList<int> foundDiff = new NativeList<int>(indices.Length, AllocatorHandle.op_Implicit((Allocator)3));
			try
			{
				ReadOnly<Vector3> val = playerStates.PlayerPos.AsReadOnly();
				ReadOnly<Quaternion> val2 = playerStates.PlayerRots.AsReadOnly();
				DiffVec3Indirect diffVec3Indirect = new DiffVec3Indirect
				{
					FoundDiff = foundDiff,
					A = playerStates.LastFramePlayerPos.AsReadOnly(),
					B = val,
					Indices = indices
				};
				IJobExtensions.RunByRef<DiffVec3Indirect>(ref diffVec3Indirect);
				CopyIndirect<Vector3> copyIndirect = new CopyIndirect<Vector3>
				{
					From = val,
					To = playerStates.LastFramePlayerPos,
					Indices = foundDiff.AsReadOnly()
				};
				IJobExtensions.RunByRef<CopyIndirect<Vector3>>(ref copyIndirect);
				GetWaterFactors(in playerStates, foundDiff.AsReadOnly());
				BasePlayerJobs.UpdateWaterCache updateWaterCache = new BasePlayerJobs.UpdateWaterCache
				{
					States = playerStates.CachedStates,
					Factors = playerStates.WaterFactors.AsReadOnly(),
					Infos = playerStates.WaterInfos.AsReadOnly(),
					Indices = foundDiff.AsReadOnly()
				};
				IJobExtensions.RunByRef<BasePlayerJobs.UpdateWaterCache>(ref updateWaterCache);
				ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
				Span<CachedState> span = NativeArray<CachedState>.op_Implicit(ref playerStates.CachedStates);
				ReadOnly<Flag> val3 = playerStates.PlayerModelStateFlags.AsReadOnly();
				ReadOnly<bool> val4 = playerStates.IsMounted.AsReadOnly();
				ReadOnlySpan<BaseMountable> readOnlySpan = playerStates.Mountables.Buffer;
				Enumerator<int> enumerator = foundDiff.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						int current = enumerator.Current;
						BasePlayer basePlayer = objects[current];
						ref CachedState reference = ref span[current];
						reference.EyePos = basePlayer.eyes.GetPos(val[current], val2[current], val4[current], readOnlySpan[current]);
						bool ducked = (val3[current] & 1) > 0;
						reference.Center = basePlayer.GetCenter(ducked, val[current]);
						reference.MovementModify = basePlayer.GetMovementModify();
						reference.IsOnLadder = basePlayer.onLadderCount > 0;
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
				}
				if (recachePosDependentOnly)
				{
					return;
				}
				Enumerator<int> enumerator2 = indices.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current2 = enumerator2.Current;
						BasePlayer basePlayer2 = objects[current2];
						ref CachedState reference2 = ref span[current2];
						reference2.EyeRot = basePlayer2.eyes.rotation;
						reference2.PlayerFlags = basePlayer2.playerFlags;
						reference2.ModifiersMovementMultiplier = basePlayer2.GetModifiersMovementMultiplier();
						reference2.ClothingMoveSpeedReduction = basePlayer2.clothingMoveSpeedReduction;
						reference2.ClothingWaterSpeedBonus = basePlayer2.clothingWaterSpeedBonus;
						reference2.WeaponMoveSpeedScale = basePlayer2.weaponMoveSpeedScale;
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
			}
			finally
			{
				((IDisposable)foundDiff/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static void ServerPreFinalize(in PlayerServerStates playerStates, ReadOnly<int> indices)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ServerPreFinalize"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			TickInterpolatorCache.ReadOnlyState readOnly = playerStates.TickCache.ReadOnly;
			Enumerator<int> enumerator = indices.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					BasePlayer obj = objects[current];
					obj.rawTickCount = obj.rawTicksPerSecond.Calculate();
					obj.CheckModelState(in playerStates);
					obj.UpdateEstimatedVelocity(TickInterpolatorCache.GetStartPoint(readOnly, current), TickInterpolatorCache.GetEndPoint(readOnly, current), playerStates.TickDeltaTime[current]);
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static void ServerFinalizePlayers(in PlayerServerStates playerStates, ReadOnly<PositionChange> posChanges, ReadOnly<int> finalizeIndices, NativeList<int> toBroadcastIndices, NativeList<int> validIndices)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ServerFinalizePlayers"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			Span<CachedState> span = NativeArray<CachedState>.op_Implicit(ref playerStates.CachedStates);
			TickInterpolatorCache tickCache = playerStates.TickCache;
			ReadOnly<Vector3> val = playerStates.PlayerLocalPos.AsReadOnly();
			Span<float> span2 = NativeArray<float>.op_Implicit(ref playerStates.TickDeltaTime);
			Enumerator<int> enumerator = finalizeIndices.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					BasePlayer basePlayer = objects[current];
					if (basePlayer.IsRealNull())
					{
						continue;
					}
					ref CachedState reference = ref span[current];
					Vector3 val2 = val[current];
					PositionChange num = posChanges[current];
					bool flag = num == PositionChange.Valid;
					if (num == PositionChange.Invalid && ConVar.AntiHack.forceposition)
					{
						basePlayer.ClientRPC(RpcTarget.Player("ForcePositionToParentOffset", basePlayer), val2, basePlayer.parentEntity.uid);
					}
					tickCache.Reset(basePlayer, val2);
					if (basePlayer.tickViewAngles != basePlayer.viewAngles)
					{
						basePlayer.viewAngles = basePlayer.tickViewAngles;
						if (!basePlayer.isMounted || !basePlayer.GetMounted().isMobile)
						{
							((Component)basePlayer).transform.rotation = Quaternion.identity;
						}
						((Component)basePlayer).transform.hasChanged = true;
						flag = true;
					}
					if (basePlayer.modelState != null)
					{
						basePlayer.modelState.waterLevel = reference.WaterFactor;
					}
					span2[current] = 0f;
					using (TimeWarning.New("AntiHack.EnforceViolations"))
					{
						AntiHack.ValidateEyeHistory(basePlayer);
					}
					if (flag)
					{
						basePlayer.eyes.NetworkUpdate(Quaternion.Euler(basePlayer.viewAngles));
						reference.EyePos = basePlayer.eyes.position;
						reference.EyeRot = basePlayer.eyes.rotation;
						reference.Center = basePlayer.GetCenter();
						toBroadcastIndices.AddNoResize(current);
						basePlayer.InvalidateNetworkCache();
					}
					validIndices.AddNoResize(current);
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static void NetworkPositionTick(in PlayerServerStates.ReadOnly playerStates, ReadOnly<int> toUpdate, float networkTime)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("NetworkPositionTick"))
		{
			NativeList<int> val = new NativeList<int>(toUpdate.Length, AllocatorHandle.op_Implicit((Allocator)2));
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			Enumerator<int> enumerator = toUpdate.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					BasePlayer basePlayer = objects[current];
					((Component)basePlayer).transform.hasChanged = false;
					if (Query.Server != null)
					{
						Query.Server.Move(basePlayer);
					}
					SingletonComponent<NpcFireManager>.Instance.Move(basePlayer);
					if (basePlayer.net != null)
					{
						if (!basePlayer.globalBroadcast && !ValidBounds.Test(basePlayer, playerStates.PlayerPos[current]))
						{
							basePlayer.OnInvalidPosition();
							continue;
						}
						basePlayer.TryScheduleUpdateNetworkGroup();
						val.AddNoResize(current);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			SendNetworkPositions(in playerStates, val.AsReadOnly(), networkTime);
			val.Dispose();
		}
	}

	private static void SendNetworkPositions(in PlayerServerStates.ReadOnly playerStates, ReadOnly<int> indices, float networkTime)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isLoading || Application.isLoadingSave)
		{
			return;
		}
		using (TimeWarning.New("SendNetworkPositions"))
		{
			List<Network.Connection> list = Pool.Get<List<Network.Connection>>();
			List<Network.Connection> list2 = Pool.Get<List<Network.Connection>>();
			Network.Connection activeFakeConnection = RustRelay.ActiveFakeConnection;
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			Enumerator<int> enumerator = indices.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					BasePlayer basePlayer = objects[current];
					if (basePlayer.IsDestroyed || !basePlayer.isSpawned)
					{
						continue;
					}
					List<Network.Connection> list3;
					if (ServerOcclusion.OcclusionEnabled)
					{
						list3 = basePlayer.unoccludedSubscribers;
					}
					else
					{
						list3 = basePlayer.GetSubscribers();
						if (list3 == null)
						{
							continue;
						}
					}
					if (list3.Count > 0 && ConVar.AntiHack.stall_position_restrictions)
					{
						list2.Clear();
						foreach (Network.Connection item in list3)
						{
							if (!(item.player as BasePlayer).isStalled)
							{
								list2.Add(item);
							}
						}
						list3 = list2;
					}
					if (activeFakeConnection != null)
					{
						if (list3.Count == 0)
						{
							list3.Add(activeFakeConnection);
						}
						else if (list3[0] != activeFakeConnection)
						{
							list3.Add(list3[0]);
							list3[0] = activeFakeConnection;
						}
					}
					if (list3.Count > 0)
					{
						SendPos(basePlayer, playerStates.PlayerLocalPos[current], basePlayer.viewAngles, networkTime, list3);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			Pool.FreeUnmanaged<Network.Connection>(ref list);
			Pool.FreeUnmanaged<Network.Connection>(ref list2);
		}
		static void SendPos(BasePlayer player, Vector3 networkPos, Vector3 networkRotEuler, float val, List<Network.Connection> dest)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			player.LogEntry(RustLog.EntryType.Network, 3, "SendNetworkPositions");
			NetWrite netWrite = Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.EntityPosition);
			netWrite.EntityID(player.net.ID);
			netWrite.Vector3(in networkPos);
			netWrite.Vector3(in networkRotEuler);
			netWrite.Float(val);
			NetworkableId uid = player.parentEntity.uid;
			if (((NetworkableId)(ref uid)).IsValid)
			{
				netWrite.EntityID(uid);
			}
			SendInfo info = new SendInfo(dest)
			{
				method = SendMethod.ReliableUnordered,
				priority = Priority.Immediate
			};
			if (PacketProfiler.shouldCaptureDetailedProfiling)
			{
				PacketProfiler.LogDetailedOutbound(Message.Type.EntityPosition, player.net.ID, ((Object)(object)player != (Object)null) ? player.PrefabName : null, (int)netWrite.Length, null, Epoch.Current, server: true);
			}
			netWrite.Send(info);
		}
	}

	public bool IsCraftingTutorialBlocked(ItemDefinition def, out bool forceUnlock)
	{
		forceUnlock = false;
		if (!IsInTutorial)
		{
			return false;
		}
		if (def.tutorialAllowance == TutorialItemAllowance.None)
		{
			return true;
		}
		bool num = CurrentTutorialAllowance >= def.tutorialAllowance;
		if (num && (Object)(object)def.Blueprint != (Object)null && !def.Blueprint.defaultBlueprint)
		{
			forceUnlock = true;
		}
		return !num;
	}

	public bool CanModifyCraftAmountDuringTutorial()
	{
		if (IsInTutorial)
		{
			return CurrentTutorialAllowance >= TutorialItemAllowance.Level4_Spear_Fire;
		}
		return false;
	}

	public TutorialIsland GetCurrentTutorialIsland()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!IsInTutorial)
		{
			return null;
		}
		Enumerator<TutorialIsland> enumerator = TutorialIsland.GetTutorialList(base.isServer).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				TutorialIsland current = enumerator.Current;
				if ((Object)(object)current.ForPlayer.Get(base.isServer) == (Object)(object)this)
				{
					return current;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return null;
	}

	public void ClearTutorial()
	{
		SetPlayerFlag(PlayerFlags.IsInTutorial, b: false);
		SleepingBag.ClearTutorialBagsForPlayer(userID);
	}

	public void ClearTutorial_PostDeath()
	{
		ClearAllPings();
		ClearDeathMarker();
		PrepareMissionsForTutorial();
		SendPingsToClient();
		SendMarkersToClient();
	}

	public void OnStartedTutorial()
	{
		ClearAllPings();
		PrepareMissionsForTutorial();
	}

	public void SetTutorialAllowance(TutorialItemAllowance newAllowance)
	{
		if (newAllowance >= CurrentTutorialAllowance)
		{
			CurrentTutorialAllowance = newAllowance;
			SendNetworkUpdate();
		}
	}

	public void Server_FailActiveTutorialMission()
	{
		if (IsInTutorial && TryGetActiveMissionInstance(out var instance) && instance.GetMission() is TutorialMission)
		{
			AbandonActiveMission();
		}
	}

	[RPC_Server]
	private void StartTutorial(RPCMessage msg)
	{
		if (!((Object)(object)msg.player != (Object)(object)this))
		{
			StartTutorial(triggerAnalytics: true);
		}
	}

	public void StartTutorial(bool triggerAnalytics)
	{
		if (ConVar.Server.tutorialEnabled)
		{
			if (!TutorialIsland.HasAvailableTutorialIsland)
			{
				ShowToast(GameTip.Styles.Red_Normal, TutorialIsland.NoTutorialIslandsAvailablePhrase, false);
			}
			else if (startTutorialCooldown > Time.realtimeSinceStartup)
			{
				int num = Mathf.CeilToInt(startTutorialCooldown - Time.realtimeSinceStartup);
				ShowToast(GameTip.Styles.Red_Normal, TutorialIsland.TutorialIslandStartCooldown, false, num.ToString());
			}
			else
			{
				startTutorialCooldown = Time.realtimeSinceStartup + (float)Debugging.tutorial_start_cooldown;
				Hurt(99999f);
				Respawn();
				TutorialIsland.RestoreOrCreateIslandForPlayer(this, triggerAnalytics);
			}
		}
	}

	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server]
	private void PlayerRequestedTutorialStart(RPCMessage msg)
	{
		if (ConVar.Server.tutorialEnabled)
		{
			if (!TutorialIsland.HasAvailableTutorialIsland)
			{
				ShowToast(GameTip.Styles.Red_Normal, TutorialIsland.NoTutorialIslandsAvailablePhrase, false);
			}
			else
			{
				ClientRPC(RpcTarget.Player("PromptToStartTutorial", this));
			}
		}
	}

	public uint GetUnderwearSkin(float time)
	{
		uint infoInt = (uint)GetInfoInt("client.underwearskin", 0);
		if (infoInt != lastValidUnderwearSkin && time > nextUnderwearValidationTime)
		{
			UnderwearManifest underwearManifest = UnderwearManifest.Get();
			nextUnderwearValidationTime = time + 0.2f;
			Underwear underwear = underwearManifest.GetUnderwear(infoInt);
			if ((Object)(object)underwear == (Object)null)
			{
				lastValidUnderwearSkin = 0u;
			}
			else if (Underwear.Validate(underwear, this))
			{
				lastValidUnderwearSkin = infoInt;
			}
		}
		return lastValidUnderwearSkin;
	}

	[RPC_Server]
	public void ServerRPC_UnderwearChange(RPCMessage msg)
	{
		if (!((Object)(object)msg.player != (Object)(object)this))
		{
			uint num = lastValidUnderwearSkin;
			uint underwearSkin = GetUnderwearSkin(Time.time);
			if (num != underwearSkin)
			{
				SendNetworkUpdate();
			}
		}
	}

	public static int CompareByDisplayName(BasePlayer a, BasePlayer b)
	{
		return string.Compare(a.displayName, b.displayName, StringComparison.Ordinal);
	}

	public static void Server_SendWorldNotificationToAllActivePlayers(WorldNotificationConfig.NotificationType notificationType, Vector3 worldPosition)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (!WorldNotificationConfig.instance.TryGetDataForMonumentType(notificationType, out var data))
		{
			Debug.LogError((object)$"Failed to find notification data for monument type {notificationType}");
			return;
		}
		bool flag = (Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null;
		bool isEventInDeepSea = flag && DeepSeaManager.IsInsideDeepSea(worldPosition);
		for (int i = 0; i < activePlayerList.Count; i++)
		{
			activePlayerList[i].Server_SendWorldNotification(notificationType, worldPosition, data, flag, isEventInDeepSea);
		}
	}

	private void Server_SendWorldNotification(WorldNotificationConfig.NotificationType notificationType, Vector3 worldPosition, WorldNotificationConfig.Data notificationData, bool isDeepSeaManagerValid, bool isEventInDeepSea)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (IsPlayerValidForNotification() && (isDeepSeaManagerValid && DeepSeaManager.IsInsideDeepSea(((Component)this).transform.position)) == isEventInDeepSea)
		{
			ClientRPC(RpcTarget.Player("Client_DoWorldNotification", this), (int)notificationType, worldPosition);
		}
	}

	public void Server_SendWorldNotification(WorldNotificationConfig.NotificationType notificationType, Vector3 worldPosition)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (!IsPlayerValidForNotification())
		{
			return;
		}
		if (!WorldNotificationConfig.instance.TryGetDataForMonumentType(notificationType, out var _))
		{
			Debug.LogError((object)$"Failed to find notification data for monument type {notificationType}");
			return;
		}
		bool num = (Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null;
		bool flag = num && DeepSeaManager.IsInsideDeepSea(((Component)this).transform.position);
		bool flag2 = num && DeepSeaManager.IsInsideDeepSea(worldPosition);
		if (flag == flag2)
		{
			ClientRPC(RpcTarget.Player("Client_DoWorldNotification", this), (int)notificationType, worldPosition);
		}
	}

	private bool IsPlayerValidForNotification()
	{
		if (!IsNpc && IsConnected && !IsSleeping())
		{
			return !IsInTutorial;
		}
		return false;
	}

	public bool IsWounded()
	{
		return HasPlayerFlag(PlayerFlags.Wounded);
	}

	public bool IsCrawling()
	{
		return IsCrawling(playerFlags);
	}

	public static bool IsCrawling(PlayerFlags flags)
	{
		if (HasPlayerFlag(flags, PlayerFlags.Wounded))
		{
			return !HasPlayerFlag(flags, PlayerFlags.Incapacitated);
		}
		return false;
	}

	public bool IsIncapacitated()
	{
		return HasPlayerFlag(PlayerFlags.Incapacitated);
	}

	public bool WoundInsteadOfDying(HitInfo info)
	{
		if (!EligibleForWounding(info))
		{
			return false;
		}
		BecomeWounded(info);
		return true;
	}

	public void ResetWoundingVars()
	{
		CancelInvoke(WoundingTick);
		woundedDuration = 0f;
		lastWoundedStartTime = float.NegativeInfinity;
		healingWhileCrawling = 0f;
		woundedByFallDamage = false;
	}

	public virtual bool EligibleForWounding(HitInfo info)
	{
		object obj = Interface.CallHook("CanBeWounded", this, info);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (!ConVar.Server.woundingenabled)
		{
			return false;
		}
		if (IsWounded())
		{
			return false;
		}
		if (IsSleeping())
		{
			return false;
		}
		if (isMounted)
		{
			return false;
		}
		if (info == null)
		{
			return false;
		}
		if (!IsWounded() && Time.realtimeSinceStartup - lastWoundedStartTime < ConVar.Server.rewounddelay)
		{
			return false;
		}
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (Object.op_Implicit((Object)(object)activeGameMode) && !activeGameMode.allowWounding)
		{
			return false;
		}
		if (triggers != null)
		{
			for (int i = 0; i < triggers.Count; i++)
			{
				if (triggers[i] is IHurtTrigger)
				{
					return false;
				}
			}
		}
		if (info.WeaponPrefab is BaseMelee)
		{
			return true;
		}
		if (info.WeaponPrefab is BaseProjectile)
		{
			return !info.isHeadshot;
		}
		switch (info.damageTypes.GetMajorityDamageType())
		{
		case DamageType.Suicide:
			return false;
		case DamageType.Fall:
			return true;
		case DamageType.Bite:
			return true;
		case DamageType.Bleeding:
			return true;
		case DamageType.Hunger:
			return true;
		case DamageType.Thirst:
			return true;
		case DamageType.Poison:
			return true;
		default:
		{
			if (BaseNetworkableEx.Is<BaseNPC2>((Object)(object)info.Initiator, out BaseNPC2 castedUnityObject) && !castedUnityObject.IsAnimal)
			{
				return true;
			}
			return false;
		}
		}
	}

	public void BecomeWounded(HitInfo info)
	{
		if (IsWounded() || Interface.CallHook("OnPlayerWound", this, info) != null)
		{
			return;
		}
		bool flag = info != null && info.damageTypes.GetMajorityDamageType() == DamageType.Fall;
		if (IsCrawling())
		{
			woundedByFallDamage |= flag;
			GoToIncapacitated(info);
			return;
		}
		woundedByFallDamage = flag;
		if (flag || !ConVar.Server.crawlingenabled)
		{
			GoToIncapacitated(info);
		}
		else
		{
			GoToCrawling(info);
		}
	}

	public void StopWounded(BasePlayer source = null)
	{
		if (IsWounded())
		{
			RecoverFromWounded();
			CancelInvoke(WoundingTick);
			EACServer.LogPlayerRevive(source, this);
			PlayerInjureState = GetInjureState();
		}
	}

	public void ProlongWounding(float delay)
	{
		if (!IsRestrained)
		{
			woundedDuration = Mathf.Max(woundedDuration, Mathf.Min(TimeSinceWoundedStarted + delay, woundedDuration + delay));
			SendWoundedInformation(woundedDuration);
		}
	}

	public void SendWoundedInformation(float timeLeft)
	{
		float recoveryChance = GetRecoveryChance();
		ClientRPC(RpcTarget.Player("CLIENT_GetWoundedInformation", this), recoveryChance, timeLeft, woundedDuration);
	}

	public float GetRecoveryChance()
	{
		float num = (IsIncapacitated() ? ConVar.Server.incapacitatedrecoverchance : ConVar.Server.woundedrecoverchance);
		float num2 = (metabolism.hydration.Fraction() + metabolism.calories.Fraction()) / 2f;
		float num3 = Mathf.Lerp(0f, ConVar.Server.woundedmaxfoodandwaterbonus, num2);
		float result = Mathf.Clamp01(num + num3);
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition("largemedkit");
		if (inventory.containerBelt.FindItemByItemID(itemDefinition.itemid) != null && !woundedByFallDamage)
		{
			return 1f;
		}
		return result;
	}

	public void WoundingTick()
	{
		using (TimeWarning.New("WoundingTick"))
		{
			if (IsDead())
			{
				return;
			}
			if (!Player.woundforever && TimeSinceWoundedStarted >= woundedDuration)
			{
				float num = (IsIncapacitated() ? ConVar.Server.incapacitatedrecoverchance : ConVar.Server.woundedrecoverchance);
				float num2 = (metabolism.hydration.Fraction() + metabolism.calories.Fraction()) / 2f;
				float num3 = Mathf.Lerp(0f, ConVar.Server.woundedmaxfoodandwaterbonus, num2);
				float num4 = Mathf.Clamp01(num + num3);
				if (Random.value < num4)
				{
					RecoverFromWounded();
					return;
				}
				if (woundedByFallDamage)
				{
					Die();
					return;
				}
				ItemDefinition itemDefinition = ItemManager.FindItemDefinition("largemedkit");
				Item item = inventory.containerBelt.FindItemByItemID(itemDefinition.itemid);
				if (item != null)
				{
					item.UseItem();
					RecoverFromWounded();
				}
				else
				{
					Die();
				}
			}
			else
			{
				if (IsSwimming() && IsCrawling())
				{
					GoToIncapacitated(null);
				}
				Invoke(WoundingTick, 1f);
			}
		}
	}

	public void GoToCrawling(HitInfo info)
	{
		base.health = Random.Range(ConVar.Server.crawlingminimumhealth, ConVar.Server.crawlingmaximumhealth);
		metabolism.bleeding.value = 0f;
		healingWhileCrawling = 0f;
		WoundedStartSharedCode(info);
		StartWoundedTick(40, 50);
		SendWoundedInformation(woundedDuration);
		SendNetworkUpdateImmediate();
		PlayerInjureState = GetInjureState();
		RefreshColliderSize(forced: true);
	}

	public void GoToIncapacitated(HitInfo info)
	{
		if (!IsWounded())
		{
			WoundedStartSharedCode(info);
		}
		base.health = Random.Range(2f, 6f);
		metabolism.bleeding.value = 0f;
		healingWhileCrawling = 0f;
		SetPlayerFlag(PlayerFlags.Incapacitated, b: true);
		SetServerFall(wantsOn: true);
		StartWoundedTick(10, 25);
		SendWoundedInformation(woundedDuration);
		SendNetworkUpdateImmediate();
		PlayerInjureState = GetInjureState();
		RefreshColliderSize(forced: true);
	}

	public void WoundedStartSharedCode(HitInfo info)
	{
		stats.Add("wounded", 1, (Stats)5);
		SetPlayerFlag(PlayerFlags.Wounded, b: true);
		if (Object.op_Implicit((Object)(object)BaseGameMode.GetActiveGameMode(base.isServer)))
		{
			BaseGameMode.GetActiveGameMode(base.isServer).OnPlayerWounded(info.InitiatorPlayer, this, info);
		}
		inventory.DropBackpackOnDeath(wounded: true);
	}

	public void StartWoundedTick(int minTime, int maxTime)
	{
		woundedDuration = Random.Range(minTime, maxTime + 1);
		ApplyWoundedStartTime();
		Invoke(WoundingTick, 1f);
	}

	public void ApplyWoundedStartTime()
	{
		lastWoundedStartTime = Time.realtimeSinceStartup;
	}

	public void RecoverFromWounded()
	{
		if (Interface.CallHook("OnPlayerRecover", this) == null)
		{
			if (IsCrawling())
			{
				base.health = Random.Range(2f, 6f) + healingWhileCrawling;
			}
			healingWhileCrawling = 0f;
			SetPlayerFlag(PlayerFlags.Wounded, b: false);
			SetPlayerFlag(PlayerFlags.Incapacitated, b: false);
			if (Object.op_Implicit((Object)(object)BaseGameMode.GetActiveGameMode(base.isServer)))
			{
				BaseGameMode.GetActiveGameMode(base.isServer).OnPlayerRevived(null, this);
			}
			Interface.CallHook("OnPlayerRecovered", this);
			RefreshColliderSize(forced: true);
		}
	}

	public bool WoundingCausingImmortality(HitInfo info)
	{
		if (!IsWounded())
		{
			return false;
		}
		if (TimeSinceWoundedStarted > 0.25f)
		{
			return false;
		}
		if (info != null && info.damageTypes.GetMajorityDamageType() == DamageType.Fall)
		{
			return false;
		}
		return true;
	}

	public InjureState GetInjureState()
	{
		if (IsDead())
		{
			return InjureState.Dead;
		}
		if (IsIncapacitated())
		{
			return InjureState.Incapacitated;
		}
		if (IsCrawling())
		{
			return InjureState.Crawling;
		}
		return InjureState.Normal;
	}

	public virtual void OnMedicalToolApplied(BasePlayer fromPlayer, ItemDefinition itemDef, ItemModConsumable consumable, MedicalTool medicalToolEntity, bool canRevive)
	{
		if ((Object)(object)fromPlayer != (Object)(object)this && IsWounded() && canRevive)
		{
			if (Interface.CallHook("OnPlayerRevive", fromPlayer, this) != null)
			{
				return;
			}
			StopWounded(fromPlayer);
		}
		foreach (ItemModConsumable.ConsumableEffect effect in consumable.effects)
		{
			if (effect.type == MetabolismAttribute.Type.Health)
			{
				base.health += effect.amount;
				ProcessMissionEvent(BaseMission.MissionEventType.HEAL, medicalToolEntity.prefabID, effect.amount);
			}
			else
			{
				metabolism.ApplyChange(effect.type, effect.amount, effect.time);
			}
		}
	}

	public override BasePlayer ToPlayer()
	{
		return this;
	}

	public static string SanitizePlayerNameString(string playerName, ulong userId)
	{
		playerName = StringEx.EscapeRichText(StringEx.ToPrintable(playerName, 32), false).Trim();
		if (string.IsNullOrWhiteSpace(playerName))
		{
			playerName = userId.ToString();
		}
		return playerName;
	}

	public bool IsGod()
	{
		if (base.isServer && (IsAdmin || IsDeveloper) && IsConnected && net.connection != null && net.connection.info.GetBool("global.god"))
		{
			return true;
		}
		return false;
	}

	public override Quaternion GetNetworkRotation()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			return Quaternion.Euler(viewAngles);
		}
		return Quaternion.identity;
	}

	public bool CanInteract()
	{
		return CanInteract(usableWhileCrawling: false);
	}

	public bool CanInteract(bool usableWhileCrawling)
	{
		bool flag = CurrentGestureIsSurrendering;
		if (!flag && IsRestrained)
		{
			Handcuffs restraintItem = Belt.GetRestraintItem();
			flag = (Object)(object)restraintItem != (Object)null && restraintItem.BlockUse;
		}
		if (!IsDead() && !IsSleeping() && !IsSpectating() && (usableWhileCrawling ? (!IsIncapacitated()) : (!IsWounded())) && !HasActiveTelephone)
		{
			return !flag;
		}
		return false;
	}

	public override float StartHealth()
	{
		return Random.Range(50f, 60f);
	}

	public override float StartMaxHealth()
	{
		return 100f;
	}

	public override float MaxHealth()
	{
		if (maxHealthOverride > 0f)
		{
			return maxHealthOverride;
		}
		return _maxHealth * (1f + (((Object)(object)modifiers != (Object)null) ? modifiers.GetValue(Modifier.ModifierType.Max_Health) : 0f));
	}

	public override float AntiHackVelocity()
	{
		if (IsSleeping())
		{
			return 0f;
		}
		if (isMounted)
		{
			return GetMounted().AntiHackVelocity();
		}
		return GetMaxSpeed();
	}

	public override float AntiHackPadding()
	{
		if (isMounted)
		{
			return GetMounted().AntiHackPadding();
		}
		if (IsSleeping())
		{
			return 0.6f;
		}
		return base.AntiHackPadding();
	}

	public override OBB WorldSpaceBounds()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (IsSleeping())
		{
			Vector3 center = ((Bounds)(ref bounds)).center;
			Vector3 size = ((Bounds)(ref bounds)).size;
			center.y /= 2f;
			size.y /= 2f;
			return new OBB(((Component)this).transform.position, ((Component)this).transform.lossyScale, ((Component)this).transform.rotation, new Bounds(center, size));
		}
		return base.WorldSpaceBounds();
	}

	public Vector3 GetMountVelocity()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		BaseMountable baseMountable = GetMounted();
		if (!((Object)(object)baseMountable != (Object)null))
		{
			return Vector3.zero;
		}
		return baseMountable.GetWorldVelocity();
	}

	public override Vector3 GetInheritedProjectileVelocity(Vector3 direction)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		BaseMountable baseMountable = GetMounted();
		if (!Object.op_Implicit((Object)(object)baseMountable))
		{
			return base.GetInheritedProjectileVelocity(direction);
		}
		return baseMountable.GetInheritedProjectileVelocity(direction);
	}

	public override Vector3 GetInheritedThrowVelocity(Vector3 direction)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		BaseMountable baseMountable = GetMounted();
		if (!Object.op_Implicit((Object)(object)baseMountable))
		{
			return base.GetInheritedThrowVelocity(direction);
		}
		return baseMountable.GetInheritedThrowVelocity(direction);
	}

	public override Vector3 GetInheritedDropVelocity()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		BaseMountable baseMountable = GetMounted();
		if (!Object.op_Implicit((Object)(object)baseMountable))
		{
			return base.GetInheritedDropVelocity();
		}
		return baseMountable.GetInheritedDropVelocity();
	}

	public override void PreInitShared()
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		base.PreInitShared();
		cachedProtection = ScriptableObject.CreateInstance<ProtectionProperties>();
		baseProtection = ScriptableObject.CreateInstance<ProtectionProperties>();
		protectionAgainstNPCs = ScriptableObject.CreateInstance<ProtectionProperties>();
		inventoryValue.Set(((Component)this).GetComponent<PlayerInventory>());
		blueprints = ((Component)this).GetComponent<PlayerBlueprints>();
		metabolism = ((Component)this).GetComponent<PlayerMetabolism>();
		modifiers = ((Component)this).GetComponent<PlayerModifiers>();
		colliderValue.Set(((Component)this).GetComponent<CapsuleCollider>());
		eyesValue.Set(((Component)this).GetComponent<PlayerEyes>());
		playerColliderStanding = new CapsuleColliderInfo(playerCollider.height, playerCollider.radius, playerCollider.center);
		playerColliderDucked = new CapsuleColliderInfo(1.5f, playerCollider.radius, Vector3.up * 0.75f);
		playerColliderCrawling = new CapsuleColliderInfo(playerCollider.radius, playerCollider.radius, Vector3.up * playerCollider.radius);
		playerColliderLyingDown = new CapsuleColliderInfo(0f, playerCollider.radius - 0.1f, Vector3.up * (playerCollider.radius - 0.1f));
		Belt = new PlayerBelt(this);
	}

	public override void DestroyShared()
	{
		RustNavigation.RemoveDrawViewer(this);
		Object.Destroy((Object)(object)cachedProtection);
		Object.Destroy((Object)(object)baseProtection);
		base.DestroyShared();
	}

	public override void ResetState()
	{
		base.ResetState();
		if (eyesValue != null)
		{
			eyesValue.Dispose();
			eyesValue = null;
		}
		if (inventoryValue != null)
		{
			inventoryValue.Dispose();
			inventoryValue = null;
		}
		if (colliderValue != null)
		{
			colliderValue.Dispose();
			colliderValue = null;
		}
	}

	public override bool InSafeZone()
	{
		if (base.isServer)
		{
			return base.InSafeZone();
		}
		return false;
	}

	public bool IsInNoRespawnZone()
	{
		if (base.isServer)
		{
			return InNoRespawnZone();
		}
		return false;
	}

	public bool IsOnATugboat()
	{
		if (GetMountedVehicle() is Tugboat)
		{
			return true;
		}
		if (GetParentEntity() is Tugboat)
		{
			return true;
		}
		return false;
	}

	public bool IsInAHelicopter()
	{
		if (GetMountedVehicle() is BaseHelicopter)
		{
			return true;
		}
		if (GetParentEntity() is BaseHelicopter)
		{
			return true;
		}
		return false;
	}

	public static void ServerCycle(float deltaTime)
	{
		CleanNulls(activePlayerList, ref PlayerStates);
		ServerUpdateParallel(deltaTime, in PlayerStates);
		try
		{
			using (TimeWarning.New("BasePlayer.BotColliderWorkQueue"))
			{
				((PersistentObjectWorkQueue<BasePlayer>)botColliderWorkQueue).RunList((double)botColliderFrameBudgetMs);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)"Server Exception: BasePlayer.BotColliderWorkQueue");
			Debug.LogException(ex);
		}
		static void CleanNulls(ListHashSet<BasePlayer> players, ref PlayerServerStates playerStates)
		{
			using (TimeWarning.New("CleanNulls"))
			{
				for (int i = 0; i < activePlayerList.Values.Count; i++)
				{
					BasePlayer basePlayer = activePlayerList[i];
					if ((Object)(object)basePlayer == (Object)null)
					{
						activePlayerList.RemoveAt(i--);
						RemoveFromPlayerCache(basePlayer, ref playerStates);
					}
				}
			}
		}
	}

	private bool ManuallyCheckSafezone()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!base.isServer)
		{
			return false;
		}
		if (BaseGameMode.TryGetActiveGameMode(base.isServer, out var gameMode) && !gameMode.safeZone)
		{
			return false;
		}
		List<Collider> list = Pool.Get<List<Collider>>();
		Vis.Colliders<Collider>(((Component)this).transform.position, 0f, list, -1, (QueryTriggerInteraction)2);
		bool result = false;
		foreach (Collider item in list)
		{
			if ((Object)(object)((Component)item).GetComponent<TriggerSafeZone>() != (Object)null)
			{
				result = true;
				continue;
			}
			TriggerSafeZoneOverride component = ((Component)item).GetComponent<TriggerSafeZoneOverride>();
			if (!((Object)(object)component != (Object)null) || !component.IsCombatActive)
			{
				continue;
			}
			result = false;
			break;
		}
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	public override bool OnStartBeingLooted(BasePlayer baseEntity)
	{
		if (InSafeCombatZone())
		{
			if (!ApartmentRoom.ArePlayersInsideSameHostileRoom(baseEntity, this))
			{
				return false;
			}
		}
		else if ((!Player.adminsafezonelooting || !baseEntity.IsAdmin) && (baseEntity.InSafeZone() || InSafeZone() || ManuallyCheckSafezone()) && (ulong)baseEntity.userID != (ulong)userID)
		{
			return false;
		}
		if ((Object)(object)RelationshipManager.ServerInstance != (Object)null)
		{
			if ((IsSleeping() || IsIncapacitated()) && !RelationshipManager.ServerInstance.HasRelations(baseEntity.userID, userID))
			{
				RelationshipManager.ServerInstance.SetRelationship(baseEntity, this, RelationshipManager.RelationshipType.Acquaintance);
			}
			RelationshipManager.ServerInstance.SetSeen(baseEntity, this);
		}
		if (IsCrawling())
		{
			GoToIncapacitated(null);
		}
		if ((Object)(object)inventory.crafting != (Object)null)
		{
			inventory.crafting.CancelAll();
		}
		return base.OnStartBeingLooted(baseEntity);
	}

	public Bounds GetBounds(bool ducked)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return new Bounds(((Component)this).transform.position + GetOffset(ducked), GetSize(ducked));
	}

	public Bounds GetBounds()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return GetBounds(modelState.ducked);
	}

	public Vector3 GetCenter(bool ducked)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return GetCenter(ducked, ((Component)this).transform.position);
	}

	public Vector3 GetCenter(bool ducked, Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return pos + GetOffset(ducked);
	}

	public Vector3 GetOcclusionOffset()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + PlayerEyes.EyeOffset;
	}

	public Vector3 GetCenter()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return GetCenter(modelState.ducked);
	}

	public static Vector3 GetOffset(bool ducked)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (ducked)
		{
			return new Vector3(0f, 0.55f, 0f);
		}
		return new Vector3(0f, 0.9f, 0f);
	}

	public Vector3 GetOffset()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return GetOffset(modelState.ducked);
	}

	public static Vector3 GetSize(bool ducked)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (ducked)
		{
			return new Vector3(1f, 1.1f, 1f);
		}
		return new Vector3(1f, 1.8f, 1f);
	}

	public Vector3 GetSize()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return GetSize(modelState.ducked);
	}

	public static float GetHeight(bool ducked)
	{
		if (ducked)
		{
			return 1.1f;
		}
		return 1.8f;
	}

	public float GetHeight()
	{
		return GetHeight(modelState.ducked);
	}

	public static float GetRadius()
	{
		return 0.5f;
	}

	public static float GetJumpHeight()
	{
		return 1.5f;
	}

	public override Vector3 TriggerPoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + NoClipOffset();
	}

	public static Vector3 NoClipOffset()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(0f, GetHeight(ducked: true) - GetRadius(), 0f);
	}

	public static float NoClipRadius(float margin)
	{
		return GetRadius() - margin;
	}

	public float MaxDeployDistance(Item item)
	{
		return 8f;
	}

	public float GetMinSpeed()
	{
		return GetSpeed(0f, 0f, 1f);
	}

	public float GetMaxSpeed()
	{
		return GetSpeed(1f, 0f, 0f);
	}

	public float GetSpeed(float running, float ducking, float crawling)
	{
		return GetSpeed(running, ducking, crawling, IsSwimming());
	}

	public float GetSpeed(bool includeMovementModify, float running, float ducking, float crawling)
	{
		return GetSpeed(running, ducking, crawling, IsSwimming(), includeMovementModify);
	}

	public float GetSpeed(float running, float ducking, float crawling, bool isSwimming, bool includeMovementModify = true)
	{
		float num = 1f;
		MovementModify movementModify = GetMovementModify();
		num -= clothingMoveSpeedReduction;
		if (isSwimming)
		{
			num += clothingWaterSpeedBonus;
		}
		if (crawling > 0f)
		{
			return Mathf.Lerp(2.8f, 0.72f, crawling) * num * GetModifiersMovementMultiplier();
		}
		float num2 = Mathf.Lerp(Mathf.Lerp(2.8f, 5.5f, running), 1.7f, ducking) * num * weaponMoveSpeedScale * GetModifiersMovementMultiplier();
		if (!includeMovementModify)
		{
			return num2;
		}
		if (!isSwimming)
		{
			return Mathf.Lerp(num2, 0f, Mathf.Max(movementModify.drag, clothingMoveSpeedReduction));
		}
		return num2;
	}

	private float GetModifiersMovementMultiplier()
	{
		float num = (((Object)(object)modifiers != (Object)null) ? modifiers.GetValue(Modifier.ModifierType.MoveSpeed) : 0f);
		return 1f + num;
	}

	public override void OnAttacked(HitInfo info)
	{
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("IOnBasePlayerAttacked", this, info) != null)
		{
			return;
		}
		float oldHealth = base.health;
		if (base.isServer)
		{
			if (InSafeCombatZone())
			{
				if (!ApartmentRoom.ArePlayersInsideSameHostileRoom(info.InitiatorPlayer, this) && (Object)(object)info.Initiator != (Object)null)
				{
					info.damageTypes.ScaleAll(0f);
				}
			}
			else if (InSafeZone() && !IsHostile() && (Object)(object)info.Initiator != (Object)null && (Object)(object)info.Initiator != (Object)(object)this)
			{
				info.damageTypes.ScaleAll(0f);
			}
		}
		if (base.isServer)
		{
			HitArea boneArea = info.boneArea;
			if (boneArea != (HitArea)(-1))
			{
				List<Item> list = Pool.Get<List<Item>>();
				list.AddRange(inventory.containerWear.itemList);
				for (int i = 0; i < list.Count; i++)
				{
					Item item = list[i];
					if (item != null)
					{
						ItemModWearable component = ((Component)item.info).GetComponent<ItemModWearable>();
						if (!((Object)(object)component == (Object)null) && component.ProtectsArea(boneArea))
						{
							item.OnAttacked(info);
						}
					}
				}
				Pool.Free<Item>(ref list, false);
				inventory.ServerUpdate(0f);
			}
		}
		base.OnAttacked(info);
		if (base.isServer && base.isServer && info.hasDamage)
		{
			if (!info.damageTypes.Has(DamageType.Bleeding) && info.damageTypes.IsBleedCausing() && !IsWounded() && !IsImmortalTo(info) && !info.damageTypes.Has(DamageType.BeeSting))
			{
				float num = (((Object)(object)modifiers != (Object)null) ? Mathf.Clamp01(1f - modifiers.GetValue(Modifier.ModifierType.Clotting)) : 1f);
				metabolism.bleeding.Add(info.damageTypes.Total() * 0.2f * num);
			}
			if (isMounted)
			{
				GetMounted().MounteeTookDamage(this, info);
			}
			CheckDeathCondition(info);
			if (net != null && net.connection != null)
			{
				ClientRPC(RpcTarget.Player("TakeDamageHit", this));
			}
			string text = StringPool.Get(info.HitBone);
			Vector3 val = info.PointEnd - info.PointStart;
			bool flag = Vector3.Dot(((Vector3)(ref val)).normalized, eyes.BodyForward()) > 0.4f;
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (Object.op_Implicit((Object)(object)initiatorPlayer) && !info.damageTypes.IsMeleeType())
			{
				initiatorPlayer.LifeStoryShotHit(info.Weapon);
			}
			if (info.isHeadshot)
			{
				if (flag)
				{
					SignalBroadcast(Signal.Flinch_RearHead, string.Empty);
				}
				else
				{
					SignalBroadcast(Signal.Flinch_Head, string.Empty);
				}
				if (!Object.op_Implicit((Object)(object)initiatorPlayer) || !initiatorPlayer.limitNetworking)
				{
					Effect.server.Run("assets/bundled/prefabs/fx/headshot.prefab", this, 0u, new Vector3(0f, 2f, 0f), Vector3.zero, ((Object)(object)initiatorPlayer != (Object)null) ? initiatorPlayer.net.connection : null);
				}
				if (Object.op_Implicit((Object)(object)initiatorPlayer))
				{
					initiatorPlayer.stats.Add("headshot", 1, (Stats)5);
					if (initiatorPlayer.IsBeingSpectated)
					{
						ReadOnlySpan<BasePlayer> spectators = initiatorPlayer.GetSpectators();
						for (int j = 0; j < spectators.Length; j++)
						{
							BasePlayer basePlayer = spectators[j];
							basePlayer.ClientRPC(RpcTarget.Player("SpectatedPlayerHeadshot", basePlayer));
						}
					}
				}
			}
			else if (flag)
			{
				SignalBroadcast(Signal.Flinch_RearTorso, string.Empty);
			}
			else if (text == "spine" || text == "spine2")
			{
				SignalBroadcast(Signal.Flinch_Stomach, string.Empty);
			}
			else
			{
				SignalBroadcast(Signal.Flinch_Chest, string.Empty);
			}
		}
		if (stats != null)
		{
			if (IsWounded())
			{
				stats.combat.LogAttack(info, "wounded", oldHealth);
			}
			else if (IsDead())
			{
				stats.combat.LogAttack(info, "killed", oldHealth);
			}
			else
			{
				stats.combat.LogAttack(info, "", oldHealth);
			}
		}
		if (Global.cinematicGingerbreadCorpses)
		{
			info.HitMaterial = Global.GingerbreadMaterialID();
		}
	}

	public void EnablePlayerCollider()
	{
		if (!((Collider)playerCollider).enabled && Interface.CallHook("OnPlayerColliderEnable", this, playerCollider) == null && !(base.isServer & isInvisible))
		{
			RefreshColliderSize(forced: true);
			((Collider)playerCollider).enabled = true;
		}
	}

	public void DisablePlayerCollider()
	{
		if (((Collider)playerCollider).enabled)
		{
			RemoveFromTriggers();
			((Collider)playerCollider).enabled = false;
		}
	}

	public Bounds GetColliderBounds()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)playerCollider == (Object)null)
		{
			return default(Bounds);
		}
		return ((Collider)playerCollider).bounds;
	}

	private void RefreshColliderSize(bool forced, bool? isSwimmingCached = null)
	{
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)playerCollider == (Object)null) && (forced || (((Collider)playerCollider).enabled && !(Time.time < nextColliderRefreshTime))))
		{
			nextColliderRefreshTime = Time.time + 0.25f + Random.Range(-0.05f, 0.05f);
			BaseMountable baseMountable = GetMounted();
			CapsuleColliderInfo capsuleColliderInfo = (((Object)(object)baseMountable != (Object)null && baseMountable.IsValid()) ? ((!baseMountable.modifiesPlayerCollider) ? playerColliderStanding : baseMountable.customPlayerCollider) : ((!IsIncapacitated() && !IsSleeping()) ? (IsCrawling() ? playerColliderCrawling : ((!modelState.ducked && !(isSwimmingCached.HasValue ? isSwimmingCached.Value : IsSwimming())) ? playerColliderStanding : playerColliderDucked)) : playerColliderLyingDown));
			if (playerCollider.height != capsuleColliderInfo.height || playerCollider.radius != capsuleColliderInfo.radius || playerCollider.center != capsuleColliderInfo.center)
			{
				playerCollider.height = capsuleColliderInfo.height;
				playerCollider.radius = capsuleColliderInfo.radius;
				playerCollider.center = capsuleColliderInfo.center;
			}
		}
	}

	private void SetPlayerRigidbodyState(bool isEnabled)
	{
		if (isEnabled)
		{
			AddPlayerRigidbody();
		}
		else
		{
			RemovePlayerRigidbody();
		}
	}

	public void AddPlayerRigidbody()
	{
		if ((Object)(object)playerRigidbody == (Object)null)
		{
			playerRigidbody = ((Component)this).gameObject.GetComponent<Rigidbody>();
		}
		if ((Object)(object)playerRigidbody == (Object)null)
		{
			playerRigidbody = ((Component)this).gameObject.AddComponent<Rigidbody>();
			playerRigidbody.useGravity = false;
			playerRigidbody.isKinematic = true;
			playerRigidbody.mass = 1f;
			playerRigidbody.interpolation = (RigidbodyInterpolation)0;
			playerRigidbody.collisionDetectionMode = (CollisionDetectionMode)0;
		}
	}

	public void RemovePlayerRigidbody()
	{
		if ((Object)(object)playerRigidbody == (Object)null)
		{
			playerRigidbody = ((Component)this).gameObject.GetComponent<Rigidbody>();
		}
		if ((Object)(object)playerRigidbody != (Object)null)
		{
			RemoveFromTriggers();
			Object.DestroyImmediate((Object)(object)playerRigidbody);
			playerRigidbody = null;
		}
	}

	public bool IsEnsnared()
	{
		if (triggers == null)
		{
			return false;
		}
		for (int i = 0; i < triggers.Count; i++)
		{
			if (triggers[i] is TriggerEnsnare)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAttacking()
	{
		HeldEntity heldEntity = GetHeldEntity();
		if ((Object)(object)heldEntity == (Object)null)
		{
			return false;
		}
		AttackEntity attackEntity = heldEntity as AttackEntity;
		if ((Object)(object)attackEntity == (Object)null)
		{
			return false;
		}
		return attackEntity.NextAttackTime - Time.time > attackEntity.repeatDelay - 1f;
	}

	public bool CanAttack()
	{
		HeldEntity heldEntity = GetHeldEntity();
		if ((Object)(object)heldEntity == (Object)null)
		{
			return false;
		}
		bool flag = IsSwimming();
		bool flag2 = heldEntity.CanBeUsedInWater();
		if (modelState.onLadder)
		{
			return false;
		}
		if (modelState.blocking)
		{
			return false;
		}
		if (!flag && !modelState.onground)
		{
			return false;
		}
		if (flag && !flag2)
		{
			return false;
		}
		if (IsEnsnared())
		{
			return false;
		}
		return true;
	}

	public bool OnLadder()
	{
		if (modelState.onLadder && !IsWounded())
		{
			return Object.op_Implicit((Object)(object)FindTrigger<TriggerLadder>());
		}
		return false;
	}

	public bool IsSwimming()
	{
		return IsSwimming(WaterFactor());
	}

	public static bool IsSwimming(float waterFactor)
	{
		return waterFactor >= 0.65f;
	}

	public bool IsHeadUnderwater()
	{
		return WaterFactor() > 0.75f;
	}

	public virtual bool IsOnGround()
	{
		return modelState.onground;
	}

	public bool IsRunning()
	{
		if (modelState != null)
		{
			return modelState.sprinting;
		}
		return false;
	}

	public bool IsDucked()
	{
		if (modelState != null)
		{
			return IsDucked(modelState.ducking);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsDucked(float factor)
	{
		return factor > 0.5f;
	}

	public void ShowToast(GameTip.Styles style, Phrase phrase, bool overlay = false, params string[] arguments)
	{
		if (base.isServer)
		{
			SendConsoleCommand("gametip.showtoast_translated", (int)style, phrase.token, phrase.english, overlay, arguments);
		}
	}

	public void ShowBlockedByEntityToast(BaseEntity ent, Phrase fallbackError = null)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ent == (Object)null))
		{
			ClientRPC(RpcTarget.Player("CLIENT_ShowBlockedByToast", this), ent.net.ID, fallbackError.token, fallbackError.english);
		}
	}

	public void ChatMessage(string msg)
	{
		if (base.isServer && Interface.CallHook("OnMessagePlayer", msg, this) == null)
		{
			SendConsoleCommand("chat.add", 2, 0, msg);
		}
	}

	public void ConsoleMessage(string msg)
	{
		if (base.isServer)
		{
			SendConsoleCommand("echo " + msg);
		}
	}

	public override float PenetrationResistance(HitInfo info)
	{
		return 100f;
	}

	public override void ScaleDamage(HitInfo info)
	{
		if (isMounted)
		{
			GetMounted().ScaleDamageForPlayer(this, info);
		}
		if (info.UseProtection || info.UseProtectionForNPCs)
		{
			HitArea boneArea = info.boneArea;
			if (info.UseProtectionForNPCs)
			{
				info.damageTypes.Total();
				protectionAgainstNPCs.Scale(info.damageTypes);
			}
			else if (boneArea != (HitArea)(-1))
			{
				cachedProtection.Clear();
				cachedProtection.Add(inventory.containerWear.itemList, boneArea);
				cachedProtection.Multiply(DamageType.Arrow, ConVar.Server.arrowarmor);
				cachedProtection.Multiply(DamageType.Bullet, ConVar.Server.bulletarmor);
				cachedProtection.Multiply(DamageType.Slash, ConVar.Server.meleearmor);
				cachedProtection.Multiply(DamageType.Blunt, ConVar.Server.meleearmor);
				cachedProtection.Multiply(DamageType.Stab, ConVar.Server.meleearmor);
				cachedProtection.Multiply(DamageType.Bleeding, ConVar.Server.bleedingarmor);
				cachedProtection.Scale(info.damageTypes);
			}
			else
			{
				baseProtection.Scale(info.damageTypes);
			}
		}
		if (Object.op_Implicit((Object)(object)info.damageProperties))
		{
			info.damageProperties.ScaleDamage(info);
		}
		if (!IsNpc && (Object)(object)info.InitiatorPlayer != (Object)null && !info.InitiatorPlayer.IsNpc)
		{
			info.damageTypes.Scale(DamageType.Bullet, ConVar.Server.pvpBulletDamageMultiplier);
		}
		if (IsNpc && (Object)(object)info.InitiatorPlayer != (Object)null && !info.InitiatorPlayer.IsNpc)
		{
			info.damageTypes.Total();
			info.damageTypes.Scale(DamageType.Bullet, ConVar.Server.pveBulletDamageMultiplier);
		}
	}

	public void ResetWeaponMoveSpeedScale()
	{
		weaponMoveSpeedScale = 1f;
	}

	private void UpdateMoveSpeedFromClothing()
	{
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		bool flag = false;
		bool flag2 = false;
		float num4 = 0f;
		eggVision = 0f;
		base.Weight = 0f;
		ItemModContainerArmorSlot itemModContainerArmorSlot = default(ItemModContainerArmorSlot);
		foreach (Item item in inventory.containerWear.itemList)
		{
			ItemModWearable component = ((Component)item.info).GetComponent<ItemModWearable>();
			if (Object.op_Implicit((Object)(object)component))
			{
				if (component.blocksAiming)
				{
					flag = true;
				}
				if (component.blocksEquipping)
				{
					flag2 = true;
				}
				num4 += component.accuracyBonus;
				eggVision += component.eggVision;
				base.Weight += component.weight;
				float num5 = 0f;
				float num6 = 0f;
				if (((Component)item.info).TryGetComponent<ItemModContainerArmorSlot>(ref itemModContainerArmorSlot))
				{
					num6 = itemModContainerArmorSlot.TotalSpeedReduction(item);
				}
				if ((Object)(object)component.movementProperties != (Object)null)
				{
					num5 = component.movementProperties.speedReduction;
					num3 += component.movementProperties.waterSpeedBonus;
				}
				float num7 = num5 + num6;
				num = Mathf.Max(num, num7);
				num2 += num7;
			}
		}
		clothingAccuracyBonus = num4;
		clothingMoveSpeedReduction = Mathf.Max(num2, num);
		clothingBlocksAiming = flag;
		clothingWaterSpeedBonus = num3;
		equippingBlocked = flag2;
		if (base.isServer && equippingBlocked)
		{
			UpdateActiveItem(default(ItemId));
		}
		if (base.isServer && isMounted)
		{
			BaseVehicle mountedVehicle = GetMountedVehicle();
			if ((Object)(object)mountedVehicle != (Object)null)
			{
				mountedVehicle.OnMountedPlayerWeightChanged(this);
			}
		}
	}

	public virtual void UpdateProtectionFromClothing()
	{
		baseProtection.Clear();
		baseProtection.Add(inventory.containerWear.itemList);
		float num = 1f / 6f;
		for (int i = 0; i < baseProtection.amounts.Length; i++)
		{
			switch (i)
			{
			case 22:
				baseProtection.amounts[i] = 1f;
				break;
			default:
				baseProtection.amounts[i] *= num;
				break;
			case 17:
			case 25:
				break;
			}
		}
		float num2 = baseProtection.amounts[17];
		baseProtection.amounts[17] = Mathf.Clamp(num2, -1f, Radiation.MaxExposureProtection);
		if (!IsNpc)
		{
			baseProtection.amounts[16] = Mathf.Clamp(baseProtection.amounts[16], 0f, ConVar.Server.max_explosive_protection);
		}
		protectionAgainstNPCs.Clear();
		protectionAgainstNPCs.Add(inventory.containerWear.itemList, HitArea.Head);
		protectionAgainstNPCs.Add(inventory.containerWear.itemList, HitArea.Chest, 1.5f);
		protectionAgainstNPCs.Add(inventory.containerWear.itemList, HitArea.Leg, 0.5f);
		for (int j = 0; j < protectionAgainstNPCs.amounts.Length; j++)
		{
			protectionAgainstNPCs.amounts[j] /= 3f;
		}
	}

	public override string Categorize()
	{
		return "player";
	}

	public override string ToString()
	{
		if (_name == null)
		{
			if (base.isServer)
			{
				_name = $"{displayName}[{userID.Get()}]";
			}
			else
			{
				_name = base.ShortPrefabName;
			}
		}
		return _name;
	}

	public string GetDebugStatus()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("Entity: {0}\n", ((object)this).ToString());
		stringBuilder.AppendFormat("Name: {0}\n", displayName);
		stringBuilder.AppendFormat("SteamID: {0}\n", userID.Get());
		foreach (PlayerFlags value in Enum.GetValues(typeof(PlayerFlags)))
		{
			stringBuilder.AppendFormat("{1}: {0}\n", HasPlayerFlag(value), value);
		}
		return stringBuilder.ToString();
	}

	public override Item GetItem(ItemId itemId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)inventory == (Object)null)
		{
			return null;
		}
		return inventory.FindItemByUID(itemId);
	}

	public override float WaterFactor()
	{
		WaterLevel.WaterInfo info;
		return WaterFactor(out info);
	}

	public float WaterFactor(out WaterLevel.WaterInfo info)
	{
		if (GetMounted().IsValid())
		{
			return GetMounted().WaterFactorForPlayer(this, out info);
		}
		return GetUnmountedWaterFactor(out info);
	}

	public float GetUnmountedWaterFactor(out WaterLevel.WaterInfo info)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)GetParentEntity() != (Object)null && GetParentEntity().BlocksWaterFor(this))
		{
			info = default(WaterLevel.WaterInfo);
			return 0f;
		}
		Vector3 val = ((Component)playerCollider).transform.TransformPoint(playerCollider.center);
		float radius = playerCollider.radius;
		float num = ((playerCollider.height <= 2f * radius || IsSleeping()) ? 0f : (playerCollider.height * 0.5f - radius));
		Vector3 start = val - ((Component)playerCollider).transform.up * num;
		Vector3 end = val + ((Component)playerCollider).transform.up * num;
		info = WaterLevel.GetWaterInfo(start, end, radius, waves: true, volumes: true, this);
		return WaterLevel.Factor(in info, start, end, radius);
	}

	public static void GetWaterFactors(in PlayerServerStates playerStates, ReadOnly<int> indices)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		GetWaterFactors(playerStates.PlayerCache.UnsafeObjects, playerStates.PlayerPos.AsReadOnly(), playerStates.PlayerRots.AsReadOnly(), playerStates.IsMounted.AsReadOnly(), playerStates.Mountables.Buffer, indices, playerStates.WaterInfos, playerStates.WaterFactors);
	}

	public static void GetWaterFactors(BasePlayer[] playerCache, ReadOnly<Vector3> posi, ReadOnly<Quaternion> rots, ReadOnly<int> indices, NativeArray<WaterLevel.WaterInfo> infos, NativeArray<float> factors)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		NativeArray<bool> val = default(NativeArray<bool>);
		val._002Ector(playerCache.Length, (Allocator)2, (NativeArrayOptions)0);
		try
		{
			BufferList<BaseMountable> val2 = Pool.Get<BufferList<BaseMountable>>();
			if (val2.Capacity < playerCache.Length)
			{
				val2.Resize(playerCache.Length);
			}
			Span<bool> span = NativeArray<bool>.op_Implicit(ref val);
			Enumerator<int> enumerator = indices.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					BaseMountable baseMountable = playerCache[current].GetMounted();
					span[current] = baseMountable != null;
					val2[current] = baseMountable;
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			GetWaterFactors(playerCache, posi, rots, val.AsReadOnly(), val2.Buffer, indices, infos, factors);
			Pool.FreeUnmanaged<BaseMountable>(ref val2);
		}
		finally
		{
			((IDisposable)val/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void GetWaterFactors(BasePlayer[] playerCache, ReadOnly<Vector3> posi, ReadOnly<Quaternion> rots, ReadOnly<bool> isMounted, ReadOnlySpan<BaseMountable> mountables, ReadOnly<int> indices, NativeArray<WaterLevel.WaterInfo> infos, NativeArray<float> factors)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GetWaterFactors"))
		{
			ReadOnlySpan<BasePlayer> readOnlySpan = playerCache;
			NativeList<int> val = new NativeList<int>(indices.Length, AllocatorHandle.op_Implicit((Allocator)3));
			Enumerator<int> enumerator = indices.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					BasePlayer basePlayer = readOnlySpan[current];
					if (isMounted[current])
					{
						factors[current] = mountables[current].WaterFactorForPlayer(basePlayer, out var info);
						infos[current] = info;
						continue;
					}
					BaseEntity baseEntity = basePlayer.GetParentEntity();
					if (baseEntity != null && baseEntity.BlocksWaterFor(basePlayer))
					{
						infos[current] = default(WaterLevel.WaterInfo);
						factors[current] = 0f;
					}
					else
					{
						val.AddNoResize(current);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			if (!val.IsEmpty)
			{
				NativeArray<Vector3> starts = default(NativeArray<Vector3>);
				starts._002Ector(readOnlySpan.Length, (Allocator)3, (NativeArrayOptions)0);
				NativeArray<Vector3> ends = default(NativeArray<Vector3>);
				ends._002Ector(readOnlySpan.Length, (Allocator)3, (NativeArrayOptions)0);
				NativeArray<float> radii = default(NativeArray<float>);
				radii._002Ector(readOnlySpan.Length, (Allocator)3, (NativeArrayOptions)0);
				ReadOnly<int> indices2 = val.AsReadOnly();
				enumerator = indices2.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						int current2 = enumerator.Current;
						CapsuleCollider val2 = readOnlySpan[current2].playerCollider;
						starts[current2] = val2.center;
						ends[current2] = Vector2.op_Implicit(new Vector2(val2.radius, val2.height));
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
				}
				GetWaterFactorsParamsJobIndirect getWaterFactorsParamsJobIndirect = new GetWaterFactorsParamsJobIndirect
				{
					Starts = starts,
					Ends = ends,
					Radii = radii,
					Pos = posi,
					Rots = rots,
					Indices = indices2
				};
				IJobExtensions.RunByRef<GetWaterFactorsParamsJobIndirect>(ref getWaterFactorsParamsJobIndirect);
				WaterLevel.GetWaterInfos(starts.AsReadOnly(), ends.AsReadOnly(), radii.AsReadOnly(), new ReadOnlySpan<BaseEntity>(playerCache), indices2, waves: true, volumes: true, infos);
				CalcWaterFactorsJobIndirect calcWaterFactorsJobIndirect = new CalcWaterFactorsJobIndirect
				{
					Factors = factors,
					Indices = indices2,
					Infos = infos.AsReadOnly(),
					Starts = starts.AsReadOnly(),
					Ends = ends.AsReadOnly(),
					Radii = radii.AsReadOnly()
				};
				IJobExtensions.RunByRef<CalcWaterFactorsJobIndirect>(ref calcWaterFactorsJobIndirect);
				starts.Dispose();
				ends.Dispose();
				radii.Dispose();
			}
			val.Dispose();
		}
	}

	public override float AirFactor()
	{
		float num = ((WaterFactor() >= 1f) ? 0f : 1f);
		BaseMountable baseMountable = GetMounted();
		if (baseMountable.IsValid() && baseMountable.BlocksWaterFor(this))
		{
			float num2 = baseMountable.AirFactor();
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	public float GetOxygenTime(out ItemModGiveOxygen.AirSupplyType airSupplyType)
	{
		BaseVehicle mountedVehicle = GetMountedVehicle();
		if (mountedVehicle.IsValid() && mountedVehicle is IAirSupply airSupply)
		{
			float airTimeRemaining = airSupply.GetAirTimeRemaining(null);
			if (airTimeRemaining > 0f)
			{
				airSupplyType = airSupply.AirType;
				return airTimeRemaining;
			}
		}
		foreach (Item item in inventory.containerWear.itemList)
		{
			IAirSupply componentInChildren = ((Component)item.info).GetComponentInChildren<IAirSupply>();
			if (componentInChildren != null)
			{
				float airTimeRemaining2 = componentInChildren.GetAirTimeRemaining(item);
				if (airTimeRemaining2 > 0f)
				{
					airSupplyType = componentInChildren.AirType;
					return airTimeRemaining2;
				}
			}
		}
		airSupplyType = ItemModGiveOxygen.AirSupplyType.Lungs;
		if (metabolism.oxygen.value > 0.5f)
		{
			float num = Mathf.InverseLerp(0.5f, 1f, metabolism.oxygen.value);
			return 5f * num;
		}
		return 0f;
	}

	public override bool ShouldInheritNetworkGroup()
	{
		return false;
	}

	public static bool AnyPlayersVisibleToEntity(Vector3 pos, float radius, BaseEntity source, Vector3 entityEyePos, bool ignorePlayersWithPriv = false)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
		List<BasePlayer> list2 = Pool.Get<List<BasePlayer>>();
		Vis.Entities(pos, radius, list2, 131072, (QueryTriggerInteraction)2);
		bool flag = false;
		foreach (BasePlayer item in list2)
		{
			if (item.IsSleeping() || !item.IsAlive() || (item.IsBuildingAuthed() && ignorePlayersWithPriv))
			{
				continue;
			}
			list.Clear();
			Vector3 position = item.eyes.position;
			Vector3 val = entityEyePos - item.eyes.position;
			GamePhysics.TraceAll(new Ray(position, ((Vector3)(ref val)).normalized), 0f, list, 9f, 1218519297, (QueryTriggerInteraction)0);
			for (int i = 0; i < list.Count; i++)
			{
				BaseEntity entity = RaycastHitEx.GetEntity(list[i]);
				if ((Object)(object)entity != (Object)null && ((Object)(object)entity == (Object)(object)source || entity.EqualNetID((BaseNetworkable)source)))
				{
					flag = true;
					break;
				}
				if (!((Object)(object)entity != (Object)null) || entity.ShouldBlockProjectiles())
				{
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		Pool.FreeUnmanaged<RaycastHit>(ref list);
		Pool.FreeUnmanaged<BasePlayer>(ref list2);
		return flag;
	}

	public bool IsStandingOnEntity(BaseEntity standingOn, int layerMask)
	{
		BaseEntity standingOnEntity = GetStandingOnEntity(layerMask);
		if ((Object)(object)standingOnEntity == (Object)null)
		{
			return false;
		}
		if (standingOnEntity.EqualNetID((BaseNetworkable)standingOn))
		{
			return true;
		}
		BaseEntity baseEntity = standingOnEntity.GetParentEntity();
		if ((Object)(object)baseEntity != (Object)null && baseEntity.EqualNetID((BaseNetworkable)standingOn))
		{
			return true;
		}
		return false;
	}

	public BaseEntity GetStandingOnEntity(int layerMask)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (!IsOnGround())
		{
			return null;
		}
		RaycastHit hit = default(RaycastHit);
		if (Physics.SphereCast(((Component)this).transform.position + Vector3.up * (0.25f + GetRadius()), GetRadius() * 0.95f, Vector3.down, ref hit, 4f, layerMask))
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hit);
			if ((Object)(object)entity != (Object)null)
			{
				return entity;
			}
		}
		return null;
	}

	public void SetActiveTelephone(PhoneController t)
	{
		activeTelephone = t;
		Interface.CallHook("OnActiveTelephoneUpdated", this, t);
	}

	public void ClearDesigningAIEntity()
	{
		if (IsDesigningAI)
		{
			((Component)designingAIEntity).GetComponent<global::IAIDesign>()?.StopDesigning();
		}
		designingAIEntity = null;
	}

	public static bool IsBotId(ulong id)
	{
		return id < 10000000;
	}

	public static void ReserveBotIds(List<ulong> usedIds)
	{
		usedIds.Sort();
		freeBotIds.Clear();
		ulong num = 1uL;
		foreach (ulong usedId in usedIds)
		{
			for (; num != usedId; num++)
			{
				freeBotIds.Add(num);
			}
			num = usedId + 1;
		}
		botIdCounter = num;
	}

	[CompilerGenerated]
	internal static void _003CSendEntityUpdates_003Eg__ProcessPlayerBatch_007C70_2(ReadOnlySpan<BasePlayer> players, BufferList<int> indices, in ThreadSafeTime time)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ProcessPlayerBatch"))
		{
			HashSet<BaseEntity> hashSet = Pool.Get<HashSet<BaseEntity>>();
			BufferList<(BaseEntity, BasePlayer)> val = Pool.Get<BufferList<(BaseEntity, BasePlayer)>>();
			bool errorLogged = false;
			Enumerator<int> enumerator = indices.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					BasePlayer basePlayer = players[current];
					int batchSize = (basePlayer.IsReceivingSnapshot ? ConVar.Server.updatebatchspawn : ConVar.Server.updatebatch);
					Network.Connection connection = basePlayer.net.connection;
					NetworkQueueList snapshotQueue = basePlayer.SnapshotQueue;
					_003CSendEntityUpdates_003Eg__GatherFromQueue_007C70_0(basePlayer, snapshotQueue, batchSize, hashSet, val);
					_003CSendEntityUpdates_003Eg__SendQueue_007C70_3(basePlayer, connection, val, in time, ref errorLogged);
					val.Clear();
					for (int i = 0; i < 2; i++)
					{
						snapshotQueue = basePlayer.networkQueue[i];
						_003CSendEntityUpdates_003Eg__GatherFromQueue_007C70_0(basePlayer, snapshotQueue, batchSize, hashSet, val);
						_003CSendEntityUpdates_003Eg__SendQueue_007C70_3(basePlayer, connection, val, in time, ref errorLogged);
						val.Clear();
					}
					hashSet.Clear();
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			Pool.FreeUnmanaged<(BaseEntity, BasePlayer)>(ref val);
			Pool.FreeUnmanaged<BaseEntity>(ref hashSet);
			Pool.FreeUnmanaged<int>(ref indices);
		}
	}

	[CompilerGenerated]
	internal static void _003CSendEntityUpdates_003Eg__SendQueue_007C70_3(BasePlayer player, Network.Connection conn, BufferList<(BaseEntity from, BasePlayer to)> pairs, in ThreadSafeTime time, ref bool errorLogged)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<(BaseEntity, BasePlayer)> enumerator = pairs.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BaseEntity item = enumerator.Current.Item1;
				try
				{
					if (item.ShouldNetworkTo(player))
					{
						NetWrite write = Net.sv.StartWrite();
						item.SendAsSnapshot(conn, write, in time, ordered: false);
					}
				}
				catch (Exception arg)
				{
					if (!errorLogged)
					{
						Debug.LogError((object)$"ProcessPlayerBatch: {arg}");
						errorLogged = true;
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[CompilerGenerated]
	internal static void _003CSendEntityUpdates_003Eg__GatherFromQueue_007C70_0(BasePlayer player, NetworkQueueList queue, int batchSize, HashSet<BaseEntity> alreadyScheduledPairs, BufferList<(BaseEntity from, BasePlayer to)> shouldNetworkToPairs)
	{
		if (CollectionEx.IsEmpty(queue.queueInternal))
		{
			return;
		}
		using (TimeWarning.New("GatherFromQueue"))
		{
			PooledList<BaseNetworkable> val = Pool.Get<PooledList<BaseNetworkable>>();
			try
			{
				int num = 0;
				foreach (BaseNetworkable item in queue.queueInternal)
				{
					((List<BaseNetworkable>)(object)val).Add(item);
					if ((Object)(object)item == (Object)null || item.net == null)
					{
						continue;
					}
					BaseEntity baseEntity = item as BaseEntity;
					if (!alreadyScheduledPairs.Contains(baseEntity))
					{
						alreadyScheduledPairs.Add(baseEntity);
						shouldNetworkToPairs.Add((baseEntity, player));
						if (++num > batchSize)
						{
							break;
						}
					}
				}
				if (((List<BaseNetworkable>)(object)val).Count == queue.queueInternal.Count)
				{
					queue.queueInternal.Clear();
					if (queue.MaxLength > 2048)
					{
						queue.queueInternal = new HashSet<BaseNetworkable>();
						queue.MaxLength = 0;
					}
					return;
				}
				foreach (BaseNetworkable item2 in (List<BaseNetworkable>)(object)val)
				{
					queue.queueInternal.Remove(item2);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}
}
