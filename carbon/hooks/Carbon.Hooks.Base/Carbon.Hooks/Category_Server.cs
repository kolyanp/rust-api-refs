using System;
using API.Hooks;

namespace Carbon.Hooks;

public class Category_Server
{
	public class Server_ConsoleSystem
	{
		[Patch("OnNativeCommandHasPermission", "OnNativeCommandHasPermission", typeof(Arg), "HasPermission", new Type[] { })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Info("Overrides Rust's native checks for command execution authorization.")]
		[Info("Example: allow `inventory.give` to be executed by regular players.")]
		[Parameter("arg", typeof(Arg), false)]
		[Return(typeof(bool))]
		public class IOnNativeCommandHasPermission : Patch
		{
			public static bool Prefix(ref Arg __instance, ref bool __result)
			{
				if (HookCaller.CallStaticHook(938254961u, (object)__instance) is bool flag)
				{
					__result = flag;
					return false;
				}
				return true;
			}
		}
	}
}
