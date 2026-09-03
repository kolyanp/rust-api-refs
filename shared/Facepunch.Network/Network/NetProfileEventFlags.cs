using System;

namespace Network;

[Flags]
public enum NetProfileEventFlags : byte
{
	None = 0,
	Outbound = 1,
	ServerRealm = 2,
	Annotated = 4,
	Immediate = 8,
	AuxIsStringId = 0x10,
	Demo = 0x20,
	AuxIsInfoId = 0x40
}
