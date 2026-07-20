using System;
using API.Hooks;

namespace Carbon.Hooks;

public class Category_Resources
{
	public class Resources_GrowableEntity
	{
		[Patch("OnGrowableUpdate", "OnGrowableUpdate", typeof(GrowableEntity), "RunUpdate", new Type[] { })]
		[Parameter("growable", typeof(GrowableEntity), false)]
		[Info("Called right before the growable entity is updated.")]
		public class OnGrowableUpdate : Patch
		{
			public static void Prefix(ref GrowableEntity __instance)
			{
				HookCaller.CallStaticHook(719742115u, (object)__instance);
			}
		}
	}
}
