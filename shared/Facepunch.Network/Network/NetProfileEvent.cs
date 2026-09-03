namespace Network;

public struct NetProfileEvent
{
	public double Time;

	public ulong EntityId;

	public ulong ConnectionId;

	public uint PrefabId;

	public uint RpcId;

	public uint Aux;

	public int Bytes;

	public ushort Fanout;

	public byte Type;

	public NetProfileEventFlags Flags;

	public bool IsOutbound => (Flags & NetProfileEventFlags.Outbound) != 0;

	public bool IsServerRealm => (Flags & NetProfileEventFlags.ServerRealm) != 0;
}
