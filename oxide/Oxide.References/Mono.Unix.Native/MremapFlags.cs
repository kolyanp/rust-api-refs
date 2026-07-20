using System;

namespace Mono.Unix.Native;

[CLSCompliant(false)]
[Map]
[Flags]
public enum MremapFlags : ulong
{
	MREMAP_MAYMOVE = 1uL
}
