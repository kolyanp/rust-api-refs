using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ConVar;
using Development.Attributes;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Math;
using Facepunch.Rust.Profiling;
using Network;
using Network.Relay;
using Network.Visibility;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Registry;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class BaseNetworkable : BaseMonoBehaviour, IPrefabPostProcess, IEntity, NetworkHandler
{
	public struct ThreadSafeTime
	{
		public DateTime Now;

		public int FrameCount;

		public float Time;

		public float FixedTime;

		public float RealTimeSinceStartup;

		public static ThreadSafeTime TakeSnapshot()
		{
			return new ThreadSafeTime
			{
				Now = DateTime.Now,
				FrameCount = Time.frameCount,
				Time = Time.time,
				FixedTime = Time.fixedTime,
				RealTimeSinceStartup = Time.realtimeSinceStartup
			};
		}
	}

	public struct SaveInfo
	{
		public Entity msg;

		public bool forDisk;

		public bool forTransfer;

		public Connection forConnection;

		public ThreadSafeTime cachedTime;

		internal bool SendingTo(Connection ownerConnection)
		{
			if (ownerConnection == null)
			{
				return false;
			}
			if (forConnection == null)
			{
				return false;
			}
			return forConnection == ownerConnection;
		}
	}

	public struct LoadInfo
	{
		public Entity msg;

		public bool fromDisk;

		public bool fromCopy;

		public bool fromTransfer;
	}

	public class EntityRealmServer : EntityRealm
	{
		protected override Manager visibilityManager
		{
			get
			{
				if (Net.sv == null)
				{
					return null;
				}
				return Net.sv.visibility;
			}
		}
	}

	public abstract class EntityRealm : IEnumerable<BaseNetworkable>, IEnumerable
	{
		public HiddenValue<ListDictionary<NetworkableId, BaseNetworkable>> entityList = new HiddenValue<ListDictionary<NetworkableId, BaseNetworkable>>(new ListDictionary<NetworkableId, BaseNetworkable>());

		public int Count => entityList.Get().Count;

		protected abstract Manager visibilityManager { get; }

		public bool Contains(NetworkableId uid)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return entityList.Get().Contains(uid);
		}

		public BaseNetworkable Find(NetworkableId uid)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("BaseNetworkable.Find"))
			{
				BaseNetworkable result = null;
				if (!entityList.Get().TryGetValue(uid, ref result))
				{
					return null;
				}
				return result;
			}
		}

		public bool TryGetEntity(NetworkableId uid, out BaseEntity entity)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("BaseNetworkable.TryGetEntity"))
			{
				entity = null;
				if (!(Find(uid) is BaseEntity baseEntity))
				{
					return false;
				}
				entity = baseEntity;
				return true;
			}
		}

		public bool TryGetEntity<T>(NetworkableId uid, out T entity) where T : BaseEntity
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("BaseNetworkable.TryGetEntity<T>"))
			{
				entity = null;
				if (!(Find(uid) is T val))
				{
					return false;
				}
				entity = val;
				return true;
			}
		}

		public void RegisterID(BaseNetworkable ent)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (ent.net != null)
			{
				ListDictionary<NetworkableId, BaseNetworkable> val = entityList.Get();
				if (val.Contains(ent.net.ID))
				{
					val[ent.net.ID] = ent;
				}
				else
				{
					val.Add(ent.net.ID, ent);
				}
			}
		}

		public void UnregisterID(BaseNetworkable ent)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (ent.net != null)
			{
				entityList.Get().Remove(ent.net.ID);
			}
		}

		public Group FindGroup(uint uid)
		{
			return visibilityManager?.Get(uid);
		}

		public Group TryFindGroup(uint uid)
		{
			return visibilityManager?.TryGet(uid);
		}

		public void FindInGroup(uint uid, List<BaseNetworkable> list)
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			Group obj = TryFindGroup(uid);
			if (obj == null || CollectionEx.IsNullOrEmpty((ICollection<Networkable>)obj.networkables))
			{
				return;
			}
			int count = obj.networkables.Values.Count;
			Networkable[] buffer = obj.networkables.Values.Buffer;
			for (int i = 0; i < count; i++)
			{
				Networkable networkable = buffer[i];
				BaseNetworkable baseNetworkable = Find(networkable.ID);
				if (!((Object)(object)baseNetworkable == (Object)null) && baseNetworkable.net != null && baseNetworkable.net.group != null)
				{
					if (baseNetworkable.net.group.ID != uid)
					{
						Debug.LogWarning((object)("Group ID mismatch: " + ((object)baseNetworkable).ToString()));
					}
					else
					{
						list.Add(baseNetworkable);
					}
				}
			}
		}

		public Enumerator<BaseNetworkable> GetEnumerator()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			return entityList.Get().Values.GetEnumerator();
		}

		IEnumerator<BaseNetworkable> IEnumerable<BaseNetworkable>.GetEnumerator()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return (IEnumerator<BaseNetworkable>)(object)GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return (IEnumerator)(object)GetEnumerator();
		}

		public virtual void Clear()
		{
			entityList.Get().Clear();
		}
	}

	public enum DestroyMode : byte
	{
		None,
		Gib
	}

	public List<Component> postNetworkUpdateComponents = new List<Component>();

	public bool _limitedNetworking;

	[NonSerialized]
	public EntityRef parentEntity;

	[NonSerialized]
	public readonly List<BaseEntity> children = new List<BaseEntity>();

	[NonSerialized]
	public bool canTriggerParent = true;

	public int creationFrame;

	public bool isSpawned;

	public MemoryStream _NetworkCache;

	public static ConcurrentQueue<MemoryStream> EntityMemoryStreamPool = new ConcurrentQueue<MemoryStream>();

	private MemoryStream _SaveCache;

	private ServerOcclusion.Group occlusionGroup;

	private ListHashSet<BaseNetworkable> occlusionGroupRefs;

	private const bool UsePlayerOnlyOnMediumLayerShortcut = true;

	[Header("BaseNetworkable")]
	[ReadOnly]
	public uint prefabID;

	[Tooltip("If enabled the entity will send to everyone on the server - regardless of position")]
	public bool globalBroadcast;

	[Tooltip("What region of the server should the entity globally network to")]
	public GlobalNetworkBehavior globalNetworkBehavior;

	[Tooltip("Global broadcast a cut down version of the entity to show buildings across the map")]
	public bool globalBuildingBlock;

	[Tooltip("How far away this entity should network to clients")]
	public EntityNetworkRange networkRange = EntityNetworkRange.Medium;

	[NonSerialized]
	public Networkable net;

	[NonSerialized]
	private BaseEntity _prefab;

	private string _prefabName;

	private string _prefabNameWithoutExtension;

	private TransformHandle _transformHandle;

	public static EntityRealm serverEntities = new EntityRealmServer();

	private const bool isServersideEntity = true;

	public static List<Connection> connectionsInSphereList = new List<Connection>();

	public bool limitNetworking
	{
		get
		{
			return _limitedNetworking;
		}
		set
		{
			if (value != _limitedNetworking)
			{
				_limitedNetworking = value;
				if (_limitedNetworking)
				{
					OnNetworkLimitStart();
				}
				else
				{
					OnNetworkLimitEnd();
				}
				UpdateNetworkGroup();
			}
		}
	}

	public int ChildCount => children.Count;

	public GameManager gameManager
	{
		get
		{
			if (isServer)
			{
				return GameManager.server;
			}
			throw new NotImplementedException("Missing gameManager path");
		}
	}

	public PrefabAttribute.Library prefabAttribute
	{
		get
		{
			if (isServer)
			{
				return PrefabAttribute.server;
			}
			throw new NotImplementedException("Missing prefabAttribute path");
		}
	}

	public static Group GlobalNetworkGroup => Net.sv.visibility.Get(0u);

	public static Group LimboNetworkGroup => Net.sv.visibility.Get(1u);

	public static Group MainIslandGroup => Net.sv.visibility.Get(2u);

	public static Group DeepSeaGroup => Net.sv.visibility.Get(3u);

	public bool HasNetworkCache => _NetworkCache != null;

	public ServerOcclusion.Group OcclusionGroup => occlusionGroup;

	public ListHashSet<BaseNetworkable> OcclusionGroupRefs => occlusionGroupRefs;

	public bool IsDestroyed { get; private set; }

	public string PrefabName
	{
		get
		{
			if (_prefabName == null)
			{
				_prefabName = StringPool.Get(prefabID);
			}
			return _prefabName;
		}
	}

	public string ShortPrefabName
	{
		get
		{
			if (_prefabNameWithoutExtension == null)
			{
				_prefabNameWithoutExtension = Path.GetFileNameWithoutExtension(PrefabName);
			}
			return _prefabNameWithoutExtension;
		}
	}

	public TransformHandle TransformHandle
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _transformHandle;
		}
	}

	public static bool UseParallelSaves => ConVar.Server.UsePlayerUpdateJobs >= 4;

	public bool isServer => true;

	public bool isClient => false;

	public void BroadcastOnPostNetworkUpdate(BaseEntity entity)
	{
		foreach (Component postNetworkUpdateComponent in postNetworkUpdateComponents)
		{
			(postNetworkUpdateComponent as IOnPostNetworkUpdate)?.OnPostNetworkUpdate(entity);
		}
		foreach (BaseEntity child in children)
		{
			child.BroadcastOnPostNetworkUpdate(entity);
		}
	}

	public virtual void PostProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (!serverside)
		{
			postNetworkUpdateComponents = ((Component)this).GetComponentsInChildren<IOnPostNetworkUpdate>(true).Cast<Component>().ToList();
		}
	}

	private void OnNetworkLimitStart()
	{
		LogEntry(RustLog.EntryType.Network, 2, "OnNetworkLimitStart");
		List<Connection> subscribers = GetSubscribers();
		if (subscribers == null || CollectionEx.IsEmpty(subscribers))
		{
			return;
		}
		List<Connection> list = Pool.Get<List<Connection>>();
		foreach (Connection item in subscribers)
		{
			if (!ShouldNetworkTo(item.player as BasePlayer))
			{
				list.Add(item);
			}
		}
		OnNetworkSubscribersLeave(list);
		Pool.FreeUnmanaged<Connection>(ref list);
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			child.OnNetworkLimitStart();
		}
	}

	private void OnNetworkLimitEnd()
	{
		LogEntry(RustLog.EntryType.Network, 2, "OnNetworkLimitEnd");
		List<Connection> subscribers = GetSubscribers();
		if (subscribers == null)
		{
			return;
		}
		OnNetworkSubscribersEnter(subscribers);
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			child.OnNetworkLimitEnd();
		}
	}

	public BaseEntity GetParentEntity()
	{
		return parentEntity.Get(isServer);
	}

	public BaseEntity GetRootParentEntity()
	{
		BaseEntity baseEntity = this as BaseEntity;
		BaseEntity baseEntity2 = GetParentEntity();
		while ((Object)(object)baseEntity2 != (Object)null)
		{
			baseEntity = baseEntity2;
			baseEntity2 = baseEntity.GetParentEntity();
		}
		return baseEntity;
	}

	public bool HasParent()
	{
		return parentEntity.IsValid(isServer);
	}

	public void AddChild(BaseEntity child)
	{
		if (!children.Contains(child))
		{
			children.Add(child);
			OnChildAdded(child);
		}
	}

	protected virtual void OnChildAdded(BaseEntity child)
	{
	}

	public void RemoveChild(BaseEntity child)
	{
		children.Remove(child);
		OnChildRemoved(child);
	}

	protected virtual void OnChildRemoved(BaseEntity child)
	{
	}

	public static Group GetGlobalNetworkGroup(GlobalNetworkBehavior mode)
	{
		return mode switch
		{
			GlobalNetworkBehavior.MainIsland => MainIslandGroup, 
			GlobalNetworkBehavior.DeepSea => DeepSeaGroup, 
			_ => GlobalNetworkGroup, 
		};
	}

	public virtual float GetNetworkTime()
	{
		return Time.time;
	}

	public virtual float GetNetworkTime(in ThreadSafeTime time)
	{
		return time.Time;
	}

	public virtual void Spawn()
	{
		EntityProfiler.spawned++;
		if (EntityProfiler.mode >= 2)
		{
			EntityProfiler.OnSpawned(this);
		}
		SpawnShared();
		if (net == null)
		{
			net = Net.sv.CreateNetworkable();
		}
		creationFrame = Time.frameCount;
		PreInitShared();
		InitShared();
		ServerInit();
		PostInitShared();
		UpdateNetworkGroup();
		ServerInitPostNetworkGroupAssign();
		isSpawned = true;
		Interface.CallHook("OnEntitySpawned", this);
		SendNetworkUpdateImmediate();
		Invoke(SendGlobalNetworkUpdate, 0f);
		if (Application.isLoading && !Application.isLoadingSave)
		{
			((Component)this).gameObject.SendOnSendNetworkUpdate(this as BaseEntity);
		}
	}

	private void SendGlobalNetworkUpdate()
	{
		GlobalNetworkHandler.server?.TrySendNetworkUpdate(this);
	}

	public bool IsFullySpawned()
	{
		return isSpawned;
	}

	public virtual void ServerInit()
	{
		serverEntities.RegisterID(this);
		if (net != null)
		{
			net.handler = this;
		}
	}

	public virtual void ServerInitPostNetworkGroupAssign()
	{
	}

	public List<Connection> GetSubscribers()
	{
		if (net == null)
		{
			return null;
		}
		if (net.group == null)
		{
			return null;
		}
		return net.group.subscribers;
	}

	public void KillMessage()
	{
		Kill();
	}

	public virtual void AdminKill()
	{
		Kill(DestroyMode.Gib);
	}

	public virtual void OnKilled()
	{
	}

	public void Kill(DestroyMode mode = DestroyMode.None, bool callOnKilled = true)
	{
		if (IsDestroyed)
		{
			Debug.LogWarning((object)("Calling kill - but already IsDestroyed!? " + (object)this));
		}
		else if (Interface.CallHook("OnEntityKill", this) == null)
		{
			EntityProfiler.killed++;
			if (EntityProfiler.mode >= 2)
			{
				EntityProfiler.OnKilled(this);
			}
			OnParentDestroyingEx.BroadcastOnParentDestroying(((Component)this).gameObject);
			if (callOnKilled)
			{
				OnKilled();
			}
			DoEntityDestroy();
			TerminateOnClient(mode);
			TerminateOnServer();
			EntityDestroy();
		}
	}

	public void KillAsMapEntity()
	{
		if (IsFullySpawned())
		{
			Kill();
			return;
		}
		IsDestroyed = true;
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void TerminateOnClient(DestroyMode mode)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (net != null && net.group != null && Net.sv.IsConnected())
		{
			LogEntry(RustLog.EntryType.Network, 2, "Term {0}", mode);
			NetWrite netWrite = Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.EntityDestroy);
			netWrite.EntityID(net.ID);
			netWrite.UInt8((byte)mode);
			if (PacketProfiler.shouldCaptureDetailedProfiling)
			{
				BaseEntity baseEntity = serverEntities.Find(net.ID) as BaseEntity;
				PacketProfiler.LogDetailedOutbound(Message.Type.EntityDestroy, net.ID, ((Object)(object)baseEntity != (Object)null) ? baseEntity.PrefabName : null, (int)netWrite.Length, null, Epoch.Current, server: true);
			}
			netWrite.Send(new SendInfo(net.group.subscribers));
			GlobalNetworkHandler.server?.OnEntityKilled(this);
		}
	}

	public void TerminateOnServer()
	{
		if (net != null)
		{
			InvalidateNetworkCache();
			serverEntities.UnregisterID(this);
			Net.sv.DestroyNetworkable(ref net);
			((MonoBehaviour)this).StopAllCoroutines();
			((Component)this).gameObject.SetActive(false);
		}
	}

	internal virtual void DoServerDestroy()
	{
		isSpawned = false;
	}

	public virtual bool ShouldNetworkTo(BasePlayer player)
	{
		object obj = Interface.CallHook("CanNetworkTo", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (net.group == null)
		{
			return true;
		}
		return player.net.subscriber.IsSubscribed(net.group);
	}

	public void SendNetworkGroupChange()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (!isSpawned || !Net.sv.IsConnected())
		{
			return;
		}
		if (net.group == null)
		{
			Debug.LogWarning((object)(((object)this).ToString() + " changed its network group to null"));
			return;
		}
		NetWrite netWrite = Net.sv.StartWrite();
		netWrite.PacketID(Message.Type.GroupChange);
		netWrite.EntityID(net.ID);
		netWrite.GroupID(net.group.ID);
		if (PacketProfiler.shouldCaptureDetailedProfiling)
		{
			BaseEntity baseEntity = serverEntities.Find(net.ID) as BaseEntity;
			PacketProfiler.LogDetailedOutbound(Message.Type.GroupChange, net.ID, ((Object)(object)baseEntity != (Object)null) ? baseEntity.PrefabName : null, (int)netWrite.Length, null, Epoch.Current, server: true, "Group: " + net.group.ID);
		}
		netWrite.Send(new SendInfo(net.group.subscribers));
	}

	public void SendAsSnapshot(Connection connection, bool ordered = true)
	{
		if (Interface.CallHook("OnEntitySnapshot", this, connection) == null)
		{
			NetWrite netWrite = Net.sv.StartWrite();
			uint val = (ordered ? (++connection.validate.entityUpdates) : uint.MaxValue);
			SaveInfo saveInfo = new SaveInfo
			{
				forConnection = connection,
				forDisk = false,
				cachedTime = ThreadSafeTime.TakeSnapshot()
			};
			netWrite.PacketID(Message.Type.Entities);
			netWrite.UInt32(val);
			ToStreamForNetwork(netWrite, saveInfo);
			netWrite.Send(new SendInfo(connection));
		}
	}

	public void SendAsSnapshot(Connection connection, in ThreadSafeTime time, bool ordered = true)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		uint val = (ordered ? (++connection.validate.entityUpdates) : uint.MaxValue);
		SaveInfo saveInfo = new SaveInfo
		{
			forConnection = connection,
			forDisk = false,
			cachedTime = time
		};
		netWrite.PacketID(Message.Type.Entities);
		netWrite.UInt32(val);
		ToStreamForNetwork(netWrite, saveInfo);
		netWrite.Send(new SendInfo(connection));
	}

	public void SendAsSnapshot(Connection connection, NetWrite write, in ThreadSafeTime time, bool ordered = true)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		uint val = (ordered ? (++connection.validate.entityUpdates) : uint.MaxValue);
		if (Interface.CallHook("OnEntitySnapshot", this, connection) == null)
		{
			SaveInfo saveInfo = new SaveInfo
			{
				forConnection = connection,
				forDisk = false,
				cachedTime = time
			};
			write.PacketID(Message.Type.Entities);
			write.UInt32(val);
			ToStreamForNetwork(write, saveInfo);
			if (PacketProfiler.shouldCaptureDetailedProfiling)
			{
				BaseEntity baseEntity = serverEntities.Find(net.ID) as BaseEntity;
				PacketProfiler.LogDetailedOutbound(Message.Type.Entities, net.ID, ((Object)(object)baseEntity != (Object)null) ? baseEntity.PrefabName : null, (int)write.Length, null, Epoch.Current, server: true);
			}
			write.Send(new SendInfo(connection));
		}
	}

	public void SendAsSnapshotWithChildren(BasePlayer player, bool ordered = true)
	{
		Connection connection = player.net.connection;
		SendAsSnapshot(connection, ordered);
		SendChildren(children, player, ordered);
		static void SendChildren(List<BaseEntity> children, BasePlayer basePlayer, bool ordered2)
		{
			Connection connection2 = basePlayer.net.connection;
			foreach (BaseEntity child in children)
			{
				if (child.ShouldNetworkTo(basePlayer))
				{
					child.SendAsSnapshot(connection2, ordered2);
					SendChildren(child.children, basePlayer, ordered2);
				}
			}
		}
	}

	public void SendNetworkUpdate(BasePlayer.NetworkQueue queue = BasePlayer.NetworkQueue.Update)
	{
		if (Application.isLoading || Application.isLoadingSave || IsDestroyed || net == null || !isSpawned)
		{
			return;
		}
		using (TimeWarning.New("SendNetworkUpdate"))
		{
			LogEntry(RustLog.EntryType.Network, 3, "SendNetworkUpdate");
			InvalidateNetworkCache();
			List<Connection> subscribers = GetSubscribers();
			if (subscribers != null && subscribers.Count > 0)
			{
				for (int i = 0; i < subscribers.Count; i++)
				{
					BasePlayer basePlayer = subscribers[i].player as BasePlayer;
					if (!((Object)(object)basePlayer == (Object)null) && ShouldNetworkTo(basePlayer))
					{
						basePlayer.QueueUpdate(queue, this);
					}
				}
			}
		}
		((Component)this).gameObject.SendOnSendNetworkUpdate(this as BaseEntity);
	}

	public void SendNetworkUpdateImmediate()
	{
		if (Application.isLoading || Application.isLoadingSave || IsDestroyed || net == null || !isSpawned)
		{
			return;
		}
		using (TimeWarning.New("SendNetworkUpdateImmediate"))
		{
			LogEntry(RustLog.EntryType.Network, 3, "SendNetworkUpdateImmediate");
			InvalidateNetworkCache();
			List<Connection> subscribers = GetSubscribers();
			if (subscribers != null && subscribers.Count > 0)
			{
				for (int i = 0; i < subscribers.Count; i++)
				{
					Connection connection = subscribers[i];
					BasePlayer basePlayer = connection.player as BasePlayer;
					if (!((Object)(object)basePlayer == (Object)null) && ShouldNetworkTo(basePlayer))
					{
						SendAsSnapshot(connection);
					}
				}
			}
		}
		((Component)this).gameObject.SendOnSendNetworkUpdate(this as BaseEntity);
	}

	public void SendNetworkUpdate_Position()
	{
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isLoading || Application.isLoadingSave || IsDestroyed || net == null || !isSpawned)
		{
			return;
		}
		using (TimeWarning.New("SendNetworkUpdate_Position"))
		{
			LogEntry(RustLog.EntryType.Network, 3, "SendNetworkUpdate_Position");
			List<Connection> subscribers = GetSubscribers();
			List<Connection> list = subscribers;
			if (subscribers == null || subscribers.Count <= 0)
			{
				return;
			}
			bool flag = ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion();
			bool stall_position_restrictions = ConVar.AntiHack.stall_position_restrictions;
			if (flag | stall_position_restrictions)
			{
				List<Connection> list2 = Pool.Get<List<Connection>>();
				foreach (Connection item in list)
				{
					BasePlayer basePlayer = item.player as BasePlayer;
					if (!((Object)(object)basePlayer == (Object)null) && (!flag || ShouldNetworkTo(basePlayer)) && (!stall_position_restrictions || !basePlayer.isStalled))
					{
						list2.Add(item);
					}
				}
				list = list2;
			}
			if (list.Count > 0)
			{
				NetWrite netWrite = Net.sv.StartWrite();
				netWrite.PacketID(Message.Type.EntityPosition);
				netWrite.EntityID(net.ID);
				netWrite.Vector3(GetNetworkPosition());
				Quaternion networkRotation = GetNetworkRotation();
				netWrite.Vector3(((Quaternion)(ref networkRotation)).eulerAngles);
				netWrite.Float(GetNetworkTime());
				NetworkableId uid = parentEntity.uid;
				if (((NetworkableId)(ref uid)).IsValid)
				{
					netWrite.EntityID(uid);
				}
				SendInfo sendInfo = new SendInfo(list);
				sendInfo.method = SendMethod.ReliableUnordered;
				sendInfo.priority = Priority.Immediate;
				SendInfo info = sendInfo;
				netWrite.Send(info);
			}
			if (list != subscribers)
			{
				Pool.FreeUnmanaged<Connection>(ref list);
			}
		}
	}

	public void ToStream(Stream stream, SaveInfo saveInfo)
	{
		Entity val = (saveInfo.msg = Pool.Get<Entity>());
		try
		{
			Save(saveInfo);
			if (saveInfo.msg.baseEntity == null)
			{
				Debug.LogError((object)(((object)this)?.ToString() + ": ToStream - no BaseEntity!?"));
			}
			if (saveInfo.msg.baseNetworkable == null)
			{
				Debug.LogError((object)(((object)this)?.ToString() + ": ToStream - no baseNetworkable!?"));
			}
			Interface.CallHook("IOnEntitySaved", this, saveInfo);
			ProtoStreamExtensions.WriteToStream((IProto)(object)saveInfo.msg, stream, false, 2097152);
			PostSave(saveInfo);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public virtual bool CanUseNetworkCache(Connection connection)
	{
		return ConVar.Server.netcache;
	}

	public void ToStreamForNetwork(Stream stream, SaveInfo saveInfo)
	{
		if (!CanUseNetworkCache(saveInfo.forConnection))
		{
			ToStream(stream, saveInfo);
			return;
		}
		if (_NetworkCache == null)
		{
			if (!EntityMemoryStreamPool.TryDequeue(out var result))
			{
				result = new MemoryStream(8);
			}
			try
			{
				ToStream(result, saveInfo);
			}
			catch
			{
				result.SetLength(0L);
				EntityMemoryStreamPool.Enqueue(result);
				throw;
			}
			if (Interlocked.CompareExchange(ref _NetworkCache, result, null) == null)
			{
				_NetworkCache = result;
				ConVar.Server.netcachesize += (int)result.Length;
			}
			else
			{
				result.SetLength(0L);
				EntityMemoryStreamPool.Enqueue(result);
			}
		}
		_NetworkCache.WriteTo(stream);
	}

	public void InvalidateNetworkCache()
	{
		using (TimeWarning.New("InvalidateNetworkCache"))
		{
			if (_SaveCache != null)
			{
				ConVar.Server.savecachesize -= (int)_SaveCache.Length;
				_SaveCache.SetLength(0L);
				_SaveCache.Position = 0L;
				EntityMemoryStreamPool.Enqueue(_SaveCache);
				_SaveCache = null;
			}
			if (_NetworkCache != null)
			{
				ConVar.Server.netcachesize -= (int)_NetworkCache.Length;
				_NetworkCache.SetLength(0L);
				_NetworkCache.Position = 0L;
				EntityMemoryStreamPool.Enqueue(_NetworkCache);
				_NetworkCache = null;
			}
			LogEntry(RustLog.EntryType.Network, 3, "InvalidateNetworkCache");
		}
	}

	public MemoryStream GetSaveCache()
	{
		if (_SaveCache == null)
		{
			if (!EntityMemoryStreamPool.TryDequeue(out _SaveCache))
			{
				_SaveCache = new MemoryStream(8);
			}
			SaveInfo saveInfo = new SaveInfo
			{
				forDisk = true,
				cachedTime = ThreadSafeTime.TakeSnapshot()
			};
			ToStream(_SaveCache, saveInfo);
			ConVar.Server.savecachesize += (int)_SaveCache.Length;
		}
		return _SaveCache;
	}

	public virtual void UpdateNetworkGroup()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Assert.IsTrue(isServer, "UpdateNetworkGroup called on clientside entity!");
		if (net == null)
		{
			return;
		}
		using (TimeWarning.New("UpdateGroups"))
		{
			if (net.UpdateGroups(((Component)this).transform.position, networkRange))
			{
				SendNetworkGroupChange();
			}
		}
	}

	public virtual bool SupportsServerOcclusion()
	{
		return false;
	}

	protected void OcclusionInitGroup(bool canBeInAGroup)
	{
		if (occlusionGroup != null)
		{
			return;
		}
		occlusionGroup = Pool.Get<ServerOcclusion.Group>();
		((ListHashSet<BaseNetworkable>)occlusionGroup).Add(this);
		OcclusionAddGroupRef(this);
		if (ServerOcclusion.Occludees.TryGetValue(net.group, out var value) && ((ListHashSet<BaseNetworkable>)value).Contains(this))
		{
			return;
		}
		OcclusionEnterGroup(net.group);
		List<Connection> subscribers = net.group.subscribers;
		if (subscribers == null)
		{
			return;
		}
		foreach (Connection item in subscribers)
		{
			if (item != net.connection && !RustRelay.IsFakeConnection(item))
			{
				BaseNetworkable baseNetworkable = item.player as BaseNetworkable;
				((ListHashSet<BaseNetworkable>)baseNetworkable.occlusionGroup).Add(this);
				OcclusionAddGroupRef(baseNetworkable);
			}
		}
	}

	protected void OcclusionTransitionNetGroup(Group oldGroup, Group newGroup)
	{
		if (newGroup == null)
		{
			if (occlusionGroup != null)
			{
				OcclusionOnDestroy(oldGroup);
			}
			return;
		}
		OcclusionLeaveGroup(oldGroup);
		if (oldGroup == null && occlusionGroup == null)
		{
			OcclusionInitGroup(canBeInAGroup: false);
		}
		else
		{
			OcclusionEnterGroup(newGroup);
		}
		List<Connection> list = null;
		List<Connection> list2 = null;
		int num;
		if (oldGroup != null)
		{
			List<Connection> subscribers = oldGroup.subscribers;
			num = ((subscribers != null && !CollectionEx.IsEmpty(subscribers)) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		List<Connection> subscribers2 = newGroup.subscribers;
		bool flag2 = subscribers2 != null && !CollectionEx.IsEmpty(subscribers2);
		if (flag & flag2)
		{
			list = Pool.Get<List<Connection>>();
			list2 = Pool.Get<List<Connection>>();
			List.Compare<Connection>(oldGroup.subscribers, newGroup.subscribers, list, list2, (List<Connection>)null);
		}
		else if (flag)
		{
			list2 = oldGroup.subscribers;
		}
		else if (flag2)
		{
			list = newGroup.subscribers;
		}
		if (list != null)
		{
			foreach (Connection item in list)
			{
				if (item != net.connection && !RustRelay.IsFakeConnection(item))
				{
					BaseNetworkable baseNetworkable = item.player as BaseNetworkable;
					((ListHashSet<BaseNetworkable>)baseNetworkable.occlusionGroup).Add(this);
					OcclusionAddGroupRef(baseNetworkable);
				}
			}
			if (list != newGroup.subscribers)
			{
				Pool.FreeUnmanaged<Connection>(ref list);
			}
		}
		if (list2 == null)
		{
			return;
		}
		foreach (Connection item2 in list2)
		{
			if (item2 != net.connection && !RustRelay.IsFakeConnection(item2))
			{
				(item2.player as BaseNetworkable).OcclusionLeavePlayersGroup(this);
			}
		}
		if (list2 != oldGroup.subscribers)
		{
			Pool.FreeUnmanaged<Connection>(ref list2);
		}
	}

	private void OcclusionEnterGroup(Group newGroup)
	{
		if (!ServerOcclusion.Occludees.TryGetValue(newGroup, out var value))
		{
			value = Pool.Get<ServerOcclusion.Group>();
			ServerOcclusion.Occludees[newGroup] = value;
		}
		((ListHashSet<BaseNetworkable>)value).TryAdd(this);
	}

	private void OcclusionLeaveGroup(Group oldGroup)
	{
		if (oldGroup != null && ServerOcclusion.Occludees.TryGetValue(oldGroup, out var value))
		{
			OcclusionLeaveOcclGroup(value);
			if (CollectionEx.IsEmpty((ICollection<BaseNetworkable>)value))
			{
				ServerOcclusion.Occludees.Remove(oldGroup);
				Pool.Free<ServerOcclusion.Group>(ref value);
			}
		}
	}

	private void OcclusionOnDestroy(Group oldGroup)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		OcclusionLeaveGroup(oldGroup);
		((ListHashSet<BaseNetworkable>)occlusionGroup).Remove(this);
		OcclusionRemoveGroupRef(this);
		Enumerator<BaseNetworkable> enumerator = ((ListHashSet<BaseNetworkable>)occlusionGroup).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.OcclusionRemoveGroupRef(this);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Pool.Free<ServerOcclusion.Group>(ref occlusionGroup);
		while (occlusionGroupRefs != null && !CollectionEx.IsEmpty((ICollection<BaseNetworkable>)occlusionGroupRefs))
		{
			occlusionGroupRefs[0].OcclusionLeavePlayersGroup(this);
		}
	}

	private bool OcclusionLeaveOcclGroup(ServerOcclusion.Group globalGroup)
	{
		return ((ListHashSet<BaseNetworkable>)globalGroup).Remove(this);
	}

	public void OcclusionSubscribedTo(Group group)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (!Net.sv.visibility.IsGroupIdSpecial(group.ID))
		{
			int item = Net.sv.visibility.DeconstructGroupId((int)group.ID).layer;
			if (item == 2 && item == 0)
			{
				return;
			}
		}
		if (!ServerOcclusion.Occludees.TryGetValue(group, out var value))
		{
			return;
		}
		Enumerator<BaseNetworkable> enumerator = ((ListHashSet<BaseNetworkable>)value).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BaseNetworkable current = enumerator.Current;
				if (this != current)
				{
					((ListHashSet<BaseNetworkable>)occlusionGroup).Add(current);
					current.OcclusionAddGroupRef(this);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private void OcclusionUnsubscribedFrom(Group group)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (!Net.sv.visibility.IsGroupIdSpecial(group.ID))
		{
			int item = Net.sv.visibility.DeconstructGroupId((int)group.ID).layer;
			if (item == 2 && item == 0)
			{
				return;
			}
		}
		if (!ServerOcclusion.Occludees.TryGetValue(group, out var value))
		{
			return;
		}
		Enumerator<BaseNetworkable> enumerator = ((ListHashSet<BaseNetworkable>)value).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BaseNetworkable current = enumerator.Current;
				if (this != current)
				{
					OcclusionLeavePlayersGroup(current);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	protected virtual bool OcclusionLeavePlayersGroup(BaseNetworkable other)
	{
		bool num = ((ListHashSet<BaseNetworkable>)occlusionGroup).Remove(other);
		if (num)
		{
			other.OcclusionRemoveGroupRef(this);
		}
		return num;
	}

	private void OcclusionAddGroupRef(BaseNetworkable other)
	{
		if (occlusionGroupRefs == null)
		{
			occlusionGroupRefs = Pool.Get<ListHashSet<BaseNetworkable>>();
		}
		occlusionGroupRefs.TryAdd(other);
	}

	private void OcclusionRemoveGroupRef(BaseNetworkable other)
	{
		occlusionGroupRefs.Remove(other);
		if (CollectionEx.IsEmpty((ICollection<BaseNetworkable>)occlusionGroupRefs))
		{
			Pool.FreeUnmanaged<BaseNetworkable>(ref occlusionGroupRefs);
		}
	}

	protected void OcclusionOldRemoveFromOcclusion()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if (occlusionGroup != null && net.group != null)
		{
			ulong value = net.ID.Value;
			Enumerator<BaseNetworkable> enumerator = ((ListHashSet<BaseNetworkable>)occlusionGroup).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BasePlayer basePlayer = enumerator.Current as BasePlayer;
					if ((Object)(object)basePlayer != (Object)null)
					{
						basePlayer.lastPlayerVisibility.Remove(value);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			occlusionGroup = null;
			ListHashSet<Group> val = Pool.Get<ListHashSet<Group>>();
			net.SubStrategy.GatherSubscriptions(net, val);
			Enumerator<Group> enumerator2 = val.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					Group current = enumerator2.Current;
					if (ServerOcclusion.Occludees.TryGetValue(current, out var value2))
					{
						((ListHashSet<BaseNetworkable>)value2).Remove(this);
						if (((ListHashSet<BaseNetworkable>)value2).Count == 0)
						{
							ServerOcclusion.Occludees.Remove(current);
						}
					}
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
			Pool.FreeUnmanaged<Group>(ref val);
		}
		BasePlayer obj = this as BasePlayer;
		obj.lastPlayerVisibility.Clear();
		obj.FreeUnoccludedSubscribers();
	}

	protected void OcclusionOnDisconnect()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BaseNetworkable> enumerator = ((ListHashSet<BaseNetworkable>)occlusionGroup).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BaseNetworkable current = enumerator.Current;
				if (!((Object)(object)current == (Object)(object)this))
				{
					current.OcclusionRemoveGroupRef(this);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		((ListHashSet<BaseNetworkable>)occlusionGroup).Clear();
		((ListHashSet<BaseNetworkable>)occlusionGroup).Add(this);
	}

	public virtual Vector3 GetNetworkPosition()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (UseParallelSaves)
		{
			return Facepunch.Extend.TransformEx.Unsafe.GetLocalPosMT(in _transformHandle);
		}
		return ((TransformHandle)(ref _transformHandle)).localPosition;
	}

	public virtual Quaternion GetNetworkRotation()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (UseParallelSaves)
		{
			return Facepunch.Extend.TransformEx.Unsafe.GetLocalRotMT(in _transformHandle);
		}
		return ((TransformHandle)(ref _transformHandle)).localRotation;
	}

	public string InvokeString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<InvokeAction> list = Pool.Get<List<InvokeAction>>();
		InvokeHandler.FindInvokes((Behaviour)(object)this, list);
		foreach (InvokeAction item in list)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(item.action.Method.Name);
		}
		Pool.FreeUnmanaged<InvokeAction>(ref list);
		return stringBuilder.ToString();
	}

	public BaseEntity LookupPrefab()
	{
		if ((Object)(object)_prefab == (Object)null)
		{
			_prefab = GameObjectEx.ToBaseEntity(gameManager.FindPrefab(PrefabName));
		}
		return _prefab;
	}

	public T LookupPrefab<T>() where T : BaseEntity
	{
		return LookupPrefab() as T;
	}

	public bool EqualNetID(BaseNetworkable other)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!other.IsRealNull() && other.net != null && net != null)
		{
			return other.net.ID == net.ID;
		}
		return false;
	}

	public bool EqualNetID(NetworkableId otherID)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (net != null)
		{
			return otherID == net.ID;
		}
		return false;
	}

	public virtual void ResetState()
	{
		if (children.Count > 0)
		{
			children.Clear();
		}
		if (this is ILootableEntity lootableEntity)
		{
			lootableEntity.LastLootedBy = 0uL;
		}
	}

	public virtual void InitShared()
	{
	}

	public virtual void PreInitShared()
	{
	}

	public virtual void PostInitShared()
	{
	}

	public virtual void DestroyShared()
	{
	}

	public virtual void OnNetworkGroupEnter(Group group)
	{
		Interface.CallHook("OnNetworkGroupEntered", this, group);
		if (ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion())
		{
			OcclusionSubscribedTo(group);
		}
	}

	public virtual void OnNetworkGroupLeave(Group group)
	{
		Interface.CallHook("OnNetworkGroupLeft", this, group);
		if (ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion())
		{
			OcclusionUnsubscribedFrom(group);
		}
	}

	public void OnNetworkGroupChange(Group oldGroup)
	{
		if (isServer)
		{
			InvalidateNetworkCache();
		}
		if (children != null && net.group != null)
		{
			foreach (BaseEntity child in children)
			{
				if (child.IsRealNull())
				{
					Debug.LogError((object)"Child is null when switching groups", (Object)(object)this);
				}
				else if (child.net != null)
				{
					if (child.ShouldInheritNetworkGroup() && ShouldChildrenInheritNetworkGroup())
					{
						child.net.SwitchGroup(net.group);
					}
					else if (isServer)
					{
						child.UpdateNetworkGroup();
					}
				}
			}
		}
		if (isServer && ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion())
		{
			OcclusionTransitionNetGroup(oldGroup, net.group);
		}
	}

	public virtual bool ShouldChildrenInheritNetworkGroup()
	{
		return true;
	}

	public void OnNetworkSubscribersEnter(List<Connection> connections)
	{
		if (!Net.sv.IsConnected() || !isServer || CollectionEx.IsEmpty(connections))
		{
			return;
		}
		foreach (Connection connection in connections)
		{
			BasePlayer basePlayer = connection.player as BasePlayer;
			if (!((Object)(object)basePlayer == (Object)null))
			{
				basePlayer.QueueUpdate(BasePlayer.NetworkQueue.Update, this as BaseEntity);
			}
		}
	}

	public void OnNetworkSubscribersLeave(List<Connection> connections)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv.IsConnected() && isServer && connections != null && !CollectionEx.IsEmpty(connections))
		{
			LogEntry(RustLog.EntryType.Network, 2, "LeaveVisibility");
			NetWrite netWrite = Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.EntityDestroy);
			netWrite.EntityID(net.ID);
			netWrite.UInt8(0);
			if (PacketProfiler.shouldCaptureDetailedProfiling)
			{
				BaseEntity baseEntity = serverEntities.Find(net.ID) as BaseEntity;
				PacketProfiler.LogDetailedOutbound(Message.Type.EntityDestroy, net.ID, ((Object)(object)baseEntity != (Object)null) ? baseEntity.PrefabName : null, (int)netWrite.Length, null, Epoch.Current, server: true);
			}
			netWrite.Send(new SendInfo(connections));
		}
	}

	public void EntityDestroy()
	{
		if (Object.op_Implicit((Object)(object)((Component)this).gameObject))
		{
			ResetState();
			gameManager.Retire(((Component)this).gameObject);
		}
	}

	private void DoEntityDestroy()
	{
		if (IsDestroyed)
		{
			return;
		}
		IsDestroyed = true;
		if (Application.isQuitting)
		{
			return;
		}
		DestroyShared();
		if (isServer)
		{
			DoServerDestroy();
		}
		using (TimeWarning.New("Registry.Entity.Unregister"))
		{
			Entity.Unregister(((Component)this).gameObject);
		}
	}

	private void SpawnShared()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		IsDestroyed = false;
		_transformHandle = ((Component)this).gameObject.transformHandle;
		using (TimeWarning.New("Registry.Entity.Register"))
		{
			Entity.Register(((Component)this).gameObject, (IEntity)(object)this);
		}
	}

	public virtual void Save(SaveInfo info)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (prefabID == 0)
		{
			Debug.LogError((object)("PrefabID is 0! " + UnityEngine.TransformEx.GetRecursiveName(((Component)this).transform)), (Object)(object)((Component)this).gameObject);
		}
		info.msg.baseNetworkable = Pool.Get<BaseNetworkable>();
		info.msg.baseNetworkable.uid = net.ID;
		info.msg.baseNetworkable.prefabID = prefabID;
		if (net.group != null)
		{
			info.msg.baseNetworkable.group = net.group.ID;
		}
		if (!info.forDisk)
		{
			info.msg.createdThisFrame = creationFrame == info.cachedTime.FrameCount;
		}
	}

	public virtual void PostSave(SaveInfo info)
	{
	}

	public void InitLoad(NetworkableId entityID)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		net = Net.sv.CreateNetworkable(entityID);
		serverEntities.RegisterID(this);
	}

	public virtual void PreServerLoad()
	{
	}

	public virtual void Load(LoadInfo info)
	{
		if (info.msg.baseNetworkable != null)
		{
			Interface.CallHook("OnEntityLoaded", this, info);
			BaseNetworkable baseNetworkable = info.msg.baseNetworkable;
			if (prefabID != baseNetworkable.prefabID && 0 == 0)
			{
				Debug.LogError((object)("Prefab IDs don't match! " + prefabID + "/" + baseNetworkable.prefabID + " -> " + (object)((Component)this).gameObject), (Object)(object)((Component)this).gameObject);
			}
		}
	}

	public virtual void PostServerLoad()
	{
		((Component)this).gameObject.SendOnSendNetworkUpdate(this as BaseEntity);
	}

	public T ToServer<T>() where T : BaseNetworkable
	{
		if (isServer)
		{
			return this as T;
		}
		return null;
	}

	public virtual bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		return false;
	}

	protected virtual bool OnSyncVar(byte syncVar, NetRead reader, bool fromAutoSave = false)
	{
		return false;
	}

	protected virtual bool WriteSyncVar(byte id, NetWrite writer)
	{
		return false;
	}

	protected virtual void WriteAutoSaveSyncVars(NetWrite writer)
	{
	}

	protected virtual void ReadAutoSaveSyncVars(NetRead reader)
	{
	}

	protected virtual bool AutoSaveSyncVars(SaveInfo save)
	{
		return false;
	}

	protected virtual bool AutoLoadSyncVars(LoadInfo load)
	{
		return false;
	}

	protected virtual void ResetSyncVars()
	{
	}

	protected virtual bool ShouldInvalidateCache(byte id)
	{
		return false;
	}

	protected virtual bool IsSyncVarEqual<T>(T oldValue, T newValue)
	{
		return EqualityComparer<T>.Default.Equals(oldValue, newValue);
	}

	public static List<Connection> GetConnectionsWithin(Vector3 position, float distance, bool includeInvisPlayers = false)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		connectionsInSphereList.Clear();
		PooledList<BasePlayer> val = Pool.Get<PooledList<BasePlayer>>();
		try
		{
			BaseEntity.Query.Server.GetPlayersInSphere(position, distance, (List<BasePlayer>)(object)val);
			float num = distance * distance;
			foreach (BasePlayer item in (List<BasePlayer>)(object)val)
			{
				if ((Object)(object)item == (Object)null || item.isClient || item.Connection == null)
				{
					continue;
				}
				if (!connectionsInSphereList.Contains(item.Connection))
				{
					connectionsInSphereList.Add(item.Connection);
				}
				if (!item.IsBeingSpectated)
				{
					continue;
				}
				ReadOnlySpan<BasePlayer> spectators = item.GetSpectators();
				for (int i = 0; i < spectators.Length; i++)
				{
					BasePlayer basePlayer = spectators[i];
					if (!connectionsInSphereList.Contains(basePlayer.Connection))
					{
						connectionsInSphereList.Add(basePlayer.Connection);
					}
				}
			}
			if (includeInvisPlayers)
			{
				Enumerator<BasePlayer> enumerator2 = BasePlayer.invisPlayers.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						BasePlayer current2 = enumerator2.Current;
						Vector3 val2 = ((Component)current2).transform.position - position;
						if (((Vector3)(ref val2)).sqrMagnitude <= num && !connectionsInSphereList.Contains(current2.Connection))
						{
							connectionsInSphereList.Add(current2.Connection);
						}
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
			}
			return connectionsInSphereList;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[PoolAnalyzerNonCaching]
	public static void GetCloseConnections(Vector3 position, float distance, List<Connection> foundConnections)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv != null && Net.sv.visibility != null)
		{
			GetCloseConnections(GetGroupForSubscriberChecks(position, distance), position, distance, foundConnections);
		}
	}

	[PoolAnalyzerNonCaching]
	public static void GetCloseConnections(Group group, Vector3 position, float distance, List<Connection> foundConnections)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv == null || Net.sv.visibility == null || group == null || group.subscribers == null)
		{
			return;
		}
		List<Connection> subscribers = group.subscribers;
		float num = distance * distance;
		for (int i = 0; i < subscribers.Count; i++)
		{
			Connection connection = subscribers[i];
			if (connection.active)
			{
				BasePlayer basePlayer = connection.player as BasePlayer;
				if (!((Object)(object)basePlayer == (Object)null) && !(basePlayer.SqrDistance(position) > num) && !foundConnections.Contains(basePlayer.Connection))
				{
					foundConnections.Add(basePlayer.Connection);
				}
			}
		}
	}

	[PoolAnalyzerNonCaching]
	public static void GetCloseConnections(Vector3 position, float distance, List<BasePlayer> players)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv != null && Net.sv.visibility != null)
		{
			GetCloseConnections(GetGroupForSubscriberChecks(position, distance), position, distance, players);
		}
	}

	[PoolAnalyzerNonCaching]
	public static void GetCloseConnections(Group group, Vector3 position, float distance, List<BasePlayer> players)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv == null || Net.sv.visibility == null || group == null || group.subscribers == null)
		{
			return;
		}
		List<Connection> subscribers = group.subscribers;
		float num = distance * distance;
		for (int i = 0; i < subscribers.Count; i++)
		{
			Connection connection = subscribers[i];
			if (connection.active)
			{
				BasePlayer basePlayer = connection.player as BasePlayer;
				if (!((Object)(object)basePlayer == (Object)null) && !(basePlayer.SqrDistance(position) > num) && !players.Contains(basePlayer))
				{
					players.Add(basePlayer);
				}
			}
		}
	}

	public static bool HasCloseConnections(Vector3 position, float distance)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv == null)
		{
			return false;
		}
		if (Net.sv.visibility == null)
		{
			return false;
		}
		return HasCloseConnections(GetGroupForSubscriberChecks(position, distance), position, distance);
	}

	private static Group GetGroupForSubscriberChecks(Vector3 position, float distance)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		float farDistanceForRange = Net.sv.visibility.GetFarDistanceForRange(EntityNetworkRange.Small);
		if (distance < farDistanceForRange)
		{
			return Net.sv.visibility.GetGroup(position, EntityNetworkRange.Small);
		}
		float farDistanceForRange2 = Net.sv.visibility.GetFarDistanceForRange(EntityNetworkRange.Medium);
		if (distance < farDistanceForRange2)
		{
			return Net.sv.visibility.GetGroup(position, EntityNetworkRange.Medium);
		}
		return Net.sv.visibility.GetGroup(position, EntityNetworkRange.Large);
	}

	public static bool HasCloseConnections(Group group, Vector3 position, float distance)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv == null)
		{
			return false;
		}
		if (Net.sv.visibility == null)
		{
			return false;
		}
		if (group == null || group.subscribers == null)
		{
			return false;
		}
		List<Connection> subscribers = group.subscribers;
		float num = distance * distance;
		for (int i = 0; i < subscribers.Count; i++)
		{
			Connection connection = subscribers[i];
			if (connection.active)
			{
				BasePlayer basePlayer = connection.player as BasePlayer;
				if (!((Object)(object)basePlayer == (Object)null) && !(basePlayer.SqrDistance(position) > num))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool HasConnections(Vector3 position)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv == null)
		{
			return false;
		}
		if (Net.sv.visibility == null)
		{
			return false;
		}
		return HasConnections(Net.sv.visibility.GetGroup(position, EntityNetworkRange.Small), position);
	}

	public static bool HasConnections(Group group, Vector3 position)
	{
		if (Net.sv == null)
		{
			return false;
		}
		if (Net.sv.visibility == null)
		{
			return false;
		}
		if (group == null || group.subscribers == null)
		{
			return false;
		}
		List<Connection> subscribers = group.subscribers;
		for (int i = 0; i < subscribers.Count; i++)
		{
			Connection connection = subscribers[i];
			if (connection.active && !((Object)(object)(connection.player as BasePlayer) == (Object)null))
			{
				return true;
			}
		}
		return false;
	}
}
