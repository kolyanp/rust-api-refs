using System.Reflection;
using API.Hooks;
using HarmonyLib;

namespace Carbon.Hooks;

public struct HookRuntime
{
	public string LastError;

	public HookState Status;

	public Harmony HarmonyHandler;

	public MethodInfo Prefix;

	public MethodInfo Postfix;

	public MethodInfo Transpiler;
}
