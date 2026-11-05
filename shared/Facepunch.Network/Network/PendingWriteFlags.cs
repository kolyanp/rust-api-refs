using System;

namespace Network;

[Flags]
internal enum PendingWriteFlags : byte
{
	None = 0,
	StartBatching = 1,
	StopBatching = 2
}
