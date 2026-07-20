using System;

namespace Network;

[Serializable]
public struct PacketProfilingData
{
	public enum PacketProfilingDirection
	{
		Inbound,
		Outbound
	}

	public enum PacketRealm
	{
		Client,
		Server
	}

	public PacketRealm OriginRealm;

	public PacketProfilingDirection Direction;

	public Message.Type PacketType;

	public NetworkableId AssociatedEntityId;

	public string AssociatedEntityName;

	public int PacketByteLength;

	public string Info;

	[NonSerialized]
	public byte[] PacketData;

	public int Timestamp;
}
