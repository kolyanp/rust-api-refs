using System;
using API.Hooks;
using Carbon.Core;

namespace Carbon.Hooks;

public class Category_Server
{
	public class Server_Hooks
	{
		[Patch("OnServerCommand", "OnServerCommand", typeof(CorePlugin), "IOnServerCommand")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Server")]
		[Info("Gets called when executing a native command.")]
		[Parameter("arg", typeof(Arg), false)]
		[Return(typeof(void))]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnServerCommand : Patch
		{
		}

		[Patch("OnServerInitialized", "OnServerInitialized", typeof(ServerMgr), "OpenConnection", new Type[] { typeof(bool) })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Server")]
		[Info("Called after the server startup has been completed and is awaiting connections.")]
		[Info("Also called for plugins that are hotloaded while the server is already started running.")]
		[Parameter("initialized", typeof(bool), true)]
		[Return(typeof(void), Discarded = true)]
		[OxideCompatible]
		public class OnServerInitialized : Patch
		{
		}

		[Patch("OnServerShutdown", "OnServerShutdown", typeof(CorePlugin), "IOnServerShutdown")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Server")]
		[Info("Called on server shutdown.")]
		[Return(typeof(void), Discarded = true)]
		[OxideCompatible]
		public class OnServerShutdown : Patch
		{
		}
	}
}
