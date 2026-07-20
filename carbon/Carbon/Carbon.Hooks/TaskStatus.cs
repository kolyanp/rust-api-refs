using System.Collections.Generic;

namespace Carbon.Hooks;

public struct TaskStatus
{
	public int Static;

	public int Patch;

	public int Dynamic;

	public int Metadata;

	public List<HookEx> Hooks;

	public readonly int Total => Static + Patch + Dynamic + Metadata;
}
