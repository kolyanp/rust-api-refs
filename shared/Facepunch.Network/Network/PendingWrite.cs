namespace Network;

internal readonly struct PendingWrite
{
	public static readonly PendingWrite StartBatching = new PendingWrite(null, PendingWriteFlags.StartBatching);

	public static readonly PendingWrite StopBatching = new PendingWrite(null, PendingWriteFlags.StopBatching);

	public readonly NetWrite Write;

	public readonly PendingWriteFlags Flags;

	public bool IsStartBatching => (Flags & PendingWriteFlags.StartBatching) != 0;

	public bool IsStopBatching => (Flags & PendingWriteFlags.StopBatching) != 0;

	public int Length
	{
		get
		{
			if (Write != null)
			{
				return (int)Write.Length;
			}
			return 0;
		}
	}

	public PendingWrite(NetWrite write, PendingWriteFlags flags = PendingWriteFlags.None)
	{
		Write = write;
		Flags = flags;
	}
}
