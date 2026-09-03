using API.Hooks;
using Carbon.Core;

namespace Carbon.Hooks;

public class Category_Structure
{
	public class Structure_Hooks
	{
		[Patch("OnCupboardAssign", "OnCupboardAssign", typeof(CorePlugin), "IOnCupboardAuthorize")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Structure")]
		[Info("Called when a player is assigned to a cupboard.")]
		[Parameter("priv", typeof(BuildingPrivlidge), false)]
		[Parameter("targetId", typeof(ulong), false)]
		[Parameter("player", typeof(BasePlayer), false)]
		[Return(typeof(void))]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnCupboardAssign : Patch
		{
		}

		[Patch("OnCupboardAuthorize", "OnCupboardAuthorize", typeof(CorePlugin), "IOnCupboardAuthorize")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Structure")]
		[Info("Called when a cupboard attempts to authorize a player.")]
		[Parameter("priv", typeof(BuildingPrivlidge), false)]
		[Parameter("player", typeof(BasePlayer), false)]
		[Return(typeof(void))]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnCupboardAuthorize : Patch
		{
		}
	}
}
