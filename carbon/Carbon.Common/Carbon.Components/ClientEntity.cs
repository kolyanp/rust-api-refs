using System;
using System.Collections.Generic;
using System.IO;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace Carbon.Components;

public class ClientEntity : IDisposable
{
	internal List<Connection> watchers;

	internal NetworkableId _parentId;

	internal static bool _isPatched { get; set; }

	public static Dictionary<ulong, ClientEntity> entities { get; private set; } = new Dictionary<ulong, ClientEntity>();

	public Entity Proto { get; set; }

	public NetworkableId NetID
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return Proto.baseNetworkable.uid;
		}
		private set
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			Proto.baseNetworkable.uid = value;
		}
	}

	public uint Prefab
	{
		get
		{
			return Proto.baseNetworkable.prefabID;
		}
		set
		{
			Proto.baseNetworkable.prefabID = value;
			List<Connection> list = Pool.Get<List<Connection>>();
			list.AddRange(watchers);
			KillAll((DestroyMode)0);
			SpawnAll(list);
			Pool.FreeUnmanaged<Connection>(ref list);
		}
	}

	public NetworkableId ParentID
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _parentId;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_parentId = value;
			SendNetworkUpdate();
		}
	}

	public Flags Flags
	{
		get
		{
			return (Flags)Proto.baseEntity.flags;
		}
		set
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected I4, but got Unknown
			Proto.baseEntity.flags = (int)value;
			SendNetworkUpdate_Flags();
		}
	}

	public Vector3 Position
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return Proto.baseEntity.pos;
		}
		set
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			Proto.baseEntity.pos = value;
		}
	}

	public Vector3 Rotation
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return Proto.baseEntity.rot;
		}
		set
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			Proto.baseEntity.rot = value;
		}
	}

	public static ClientEntity Create(string prefabName, Vector3 position, Quaternion rotation, Entity proto = null, ulong netId = 0uL, uint group = 0u)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		uint num = 0u;
		if (StringPool.toNumber.TryGetValue(prefabName, out var value))
		{
			num = value;
		}
		if (num == 0)
		{
			EntitySpawnRequest spawnEntityFromName = Entity.GetSpawnEntityFromName(prefabName);
			if (string.IsNullOrEmpty(spawnEntityFromName.PrefabName))
			{
				Logger.Warn("ClientEntity creation failed: '" + prefabName + "' does not exist.");
				return null;
			}
			num = StringPool.Get(spawnEntityFromName.PrefabName);
		}
		return new ClientEntity(proto, num, netId, group)
		{
			Position = position,
			Rotation = ((Quaternion)(ref rotation)).eulerAngles
		};
	}

	internal static void ServerRPCUnknown(NetworkableId netID, uint rpcID, Message packet)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		if (entities.TryGetValue(netID.Value, out var value) && value.watchers.Contains(packet.connection))
		{
			value.OnRpc(StringPool.Get(rpcID), packet);
		}
	}

	internal static void HookCheck()
	{
		if (!_isPatched && entities.Count > 0)
		{
			_isPatched = true;
			Community.Runtime.HookManager.Subscribe("IServerMgrOnRPCMessage", "ClientEntity.HookCheck");
		}
	}

	public ClientEntity(Entity proto = null, uint prefabId = 0u, ulong netId = 0uL, uint group = 0u)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		watchers = new List<Connection>();
		base._002Ector();
		Proto = (Entity)(((object)proto) ?? ((object)new Entity()));
		Entity proto2 = Proto;
		if (proto2.baseNetworkable == null)
		{
			proto2.baseNetworkable = new BaseNetworkable();
		}
		proto2 = Proto;
		if (proto2.baseEntity == null)
		{
			proto2.baseEntity = new BaseEntity();
		}
		NetID = new NetworkableId((netId == 0L) ? Net.sv.TakeUID() : netId);
		Proto.baseNetworkable.group = group;
		if (prefabId != 0)
		{
			Proto.baseNetworkable.prefabID = prefabId;
		}
		entities[NetID.Value] = this;
		HookCheck();
	}

	public virtual void SpawnFor(Connection connection)
	{
		_sendNetworkUpdateImmediate(connection);
	}

	public virtual void SpawnAll(IList<Connection> connections)
	{
		_sendNetworkUpdateImmediate(connections);
	}

	public virtual void KillFor(Connection connection, DestroyMode mode = (DestroyMode)0)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected I4, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (watchers.Contains(connection))
		{
			watchers.Remove(connection);
		}
		NetWrite val = ((BaseNetwork)Net.sv).StartWrite();
		try
		{
			val.PacketID((Type)6);
			val.EntityID(NetID);
			val.UInt8((byte)(int)mode);
			val.Send(new SendInfo(connection));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public virtual void KillAll(DestroyMode mode = (DestroyMode)0)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected I4, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		NetWrite val = ((BaseNetwork)Net.sv).StartWrite();
		try
		{
			val.PacketID((Type)6);
			val.EntityID(NetID);
			val.UInt8((byte)(int)mode);
			val.Send(new SendInfo(watchers));
			watchers.Clear();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool HasFlag(Flags flag)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return (Flags)(Flags & flag) == flag;
	}

	public void SetFlag(Flags flag, bool wants, bool update = true)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (wants)
		{
			if (HasFlag(flag))
			{
				return;
			}
			Flags |= flag;
		}
		else
		{
			if (!HasFlag(flag))
			{
				return;
			}
			Flags &= ~flag;
		}
		if (update)
		{
			SendNetworkUpdate_Flags();
		}
	}

	public virtual void SendNetworkUpdate()
	{
		List<Connection> list = Pool.Get<List<Connection>>();
		foreach (Connection watcher in watchers)
		{
			list.Add(watcher);
		}
		_sendNetworkUpdateImmediate((IList<Connection>)list);
		Pool.FreeUnmanaged<Connection>(ref list);
	}

	public virtual void SendNetworkUpdate_Flags()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected I4, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (watchers.Count == 0)
		{
			return;
		}
		NetWrite val = ((BaseNetwork)Net.sv).StartWrite();
		try
		{
			val.PacketID((Type)23);
			val.EntityID(NetID);
			val.Int32((int)Flags);
			val.Send(new SendInfo(watchers));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public virtual void SendNetworkUpdate_Position()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (watchers.Count == 0)
		{
			return;
		}
		NetWrite val = ((BaseNetwork)Net.sv).StartWrite();
		try
		{
			val.PacketID((Type)10);
			val.EntityID(NetID);
			val.Vector3(ref Proto.baseEntity.pos);
			val.Vector3(ref Proto.baseEntity.rot);
			val.Float(Time.time);
			NetworkableId parentID = ParentID;
			if (((NetworkableId)(ref parentID)).IsValid)
			{
				val.EntityID(ParentID);
			}
			SendInfo val2 = new SendInfo(watchers);
			val2.method = (SendMethod)1;
			val2.priority = (Priority)0;
			SendInfo val3 = val2;
			val.Send(val3);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public virtual void OnRpc(string rpc, Message message)
	{
	}

	internal void _sendNetworkUpdateImmediate(Connection connection)
	{
		byte[] data = ProtoStreamExtensions.ToProtoBytes((IProto)(object)Proto);
		if (connection.player is BasePlayer)
		{
			_sendSnapshot(connection, data);
		}
	}

	internal void _sendNetworkUpdateImmediate(IList<Connection> connections)
	{
		byte[] data = ProtoStreamExtensions.ToProtoBytes((IProto)(object)Proto);
		foreach (Connection connection in connections)
		{
			if (connection.player is BasePlayer)
			{
				_sendSnapshot(connection, data);
			}
		}
	}

	internal void _sendSnapshot(Connection connection, byte[] data)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		NetWrite val = ((BaseNetwork)Net.sv).StartWrite();
		try
		{
			val.PacketID((Type)5);
			connection.validate.entityUpdates++;
			val.UInt32(connection.validate.entityUpdates);
			((Stream)(object)val).Write(data, 0, data.Length);
			val.Send(new SendInfo(connection));
			if (!watchers.Contains(connection))
			{
				watchers.Add(connection);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	internal NetWrite RPCWriteStart(Connection sourceConnection, string funcName)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		NetWrite val = ((BaseNetwork)Net.sv).StartWrite();
		val.PacketID((Type)9);
		val.EntityID(NetID);
		val.UInt32(StringPool.Get(funcName));
		val.UInt64(sourceConnection?.userid ?? 0);
		return val;
	}

	public void ClientRPC(Connection sourceConnection, string funcName)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		NetWrite val = RPCWriteStart(sourceConnection, funcName);
		try
		{
			val.Send(new SendInfo(watchers));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void ClientRPC<T1>(Connection sourceConnection, string funcName, T1 arg1)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		NetWrite val = RPCWriteStart(sourceConnection, funcName);
		try
		{
			NetworkWriteEx.WriteObject<T1>(val, arg1);
			val.Send(new SendInfo(watchers));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void ClientRPC<T1, T2>(Connection sourceConnection, string funcName, T1 arg1, T2 arg2)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		NetWrite val = RPCWriteStart(sourceConnection, funcName);
		try
		{
			NetworkWriteEx.WriteObject<T1>(val, arg1);
			NetworkWriteEx.WriteObject<T2>(val, arg2);
			val.Send(new SendInfo(watchers));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void ClientRPC<T1, T2, T3>(Connection sourceConnection, string funcName, T1 arg1, T2 arg2, T3 arg3)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		NetWrite val = RPCWriteStart(sourceConnection, funcName);
		try
		{
			NetworkWriteEx.WriteObject<T1>(val, arg1);
			NetworkWriteEx.WriteObject<T2>(val, arg2);
			NetworkWriteEx.WriteObject<T3>(val, arg3);
			val.Send(new SendInfo(watchers));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void ClientRPC<T1, T2, T3, T4>(Connection sourceConnection, string funcName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		NetWrite val = RPCWriteStart(sourceConnection, funcName);
		try
		{
			NetworkWriteEx.WriteObject<T1>(val, arg1);
			NetworkWriteEx.WriteObject<T2>(val, arg2);
			NetworkWriteEx.WriteObject<T3>(val, arg3);
			NetworkWriteEx.WriteObject<T4>(val, arg4);
			val.Send(new SendInfo(watchers));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void ClientRPC<T1, T2, T3, T4, T5>(Connection sourceConnection, string funcName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		NetWrite val = RPCWriteStart(sourceConnection, funcName);
		try
		{
			NetworkWriteEx.WriteObject<T1>(val, arg1);
			NetworkWriteEx.WriteObject<T2>(val, arg2);
			NetworkWriteEx.WriteObject<T3>(val, arg3);
			NetworkWriteEx.WriteObject<T4>(val, arg4);
			NetworkWriteEx.WriteObject<T5>(val, arg5);
			val.Send(new SendInfo(watchers));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public virtual void Dispose()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (Proto != null)
		{
			entities.Remove(NetID.Value);
			KillAll((DestroyMode)0);
			Entity proto = Proto;
			if (proto != null)
			{
				proto.Dispose();
			}
			Proto = null;
			watchers?.Clear();
			watchers = null;
			HookCheck();
		}
	}
}
