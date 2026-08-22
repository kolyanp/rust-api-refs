using API.Hooks;

namespace Carbon.Hooks;

public class Category_Entity
{
	public class Entity_Outdated
	{
		[Patch("OnEntitySaved", "OnEntitySaved", typeof(BaseNetworkable), "ToStream", null)]
		[Options(/*Could not decode attribute arguments.*/)]
		[Info("Gets called whenever the entity is about to be saved for streaming.")]
		[Parameter("networkable", typeof(BaseNetworkable), false)]
		[Parameter("info", typeof(SaveInfo), false)]
		[Return(typeof(void), Discarded = true)]
		[OxideCompatible]
		public class OnEntitySaved : Patch
		{
		}
	}
}
