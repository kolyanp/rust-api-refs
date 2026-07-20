using System;

namespace API.Commands;

[Flags]
public enum CommandFlags
{
	None = 0,
	Hidden = 1,
	Protected = 2
}
