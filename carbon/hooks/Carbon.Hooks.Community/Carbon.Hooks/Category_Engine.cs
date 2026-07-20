using System;
using API.Hooks;
using Carbon.Core;
using Carbon.Managers;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Oxide.Plugins;

namespace Carbon.Hooks;

public class Category_Engine
{
	public class Engine_Hooks
	{
		[Patch("CanUseUI", "CanUseUI", typeof(CuiHelper), "AddUi")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("CUI")]
		[Info("Gets called when an UI is about to be sent to a player.")]
		[Parameter("player", typeof(BasePlayer), false)]
		[Parameter("json", typeof(string), false)]
		[Return(typeof(bool))]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class CanUseUI : Patch
		{
		}

		[Patch("OnDestroyUI", "OnDestroyUI", typeof(CuiHelper), "DestroyUi")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("CUI")]
		[Info("Gets called when an UI is being destroyed on a client.")]
		[Info("`name` is the name of the client panel.")]
		[Parameter("player", typeof(BasePlayer), false)]
		[Parameter("name", typeof(string), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnDestroyUI : Patch
		{
		}

		[Patch("LoadDefaultConfig", "LoadDefaultConfig", typeof(Plugin), "LoadConfig")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Engine")]
		[Info("Gets called when a plugin should initialize default config.")]
		[Info("You should not use this. Override `LoadDefaultConfig` virtual method instead.")]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class LoadDefaultConfig : Patch
		{
		}

		[Patch("OnCompilationFail", "OnCompilationFail", typeof(ScriptLoader), "Compile")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Engine")]
		[Info("Gets called when a plugin fails compiling.")]
		[Parameter("file", typeof(string), false)]
		[Parameter("result", typeof(CompilationResult), false)]
		[Assembly("Carbon.dll")]
		public class OnCompilationFail : Patch
		{
		}

		[Patch("OnConstructorFail", "OnConstructorFail", typeof(ModLoader), "InitializePlugin")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Engine")]
		[Info("Gets called when a plugin's constructor throws an exception.")]
		[Info("Fatal error which forcefully unloads the plugin right after.")]
		[Parameter("plugin", typeof(RustPlugin), false)]
		[Parameter("exception", typeof(Exception), false)]
		[Assembly("Carbon.Common.dll")]
		public class OnConstructorFail : Patch
		{
		}

		[Patch("OnPluginLoaded", "OnPluginLoaded", typeof(ScriptLoader), "Compile")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Engine")]
		[Info("Gets called when a plugin is loaded.")]
		[Parameter("plugin", typeof(RustPlugin), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnPluginLoaded : Patch
		{
		}

		[Patch("OnPluginUnloaded", "OnPluginUnloaded", typeof(ModLoader), "UninitializePlugin")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Engine")]
		[Info("Gets called when a plugin is unloaded.")]
		[Parameter("plugin", typeof(RustPlugin), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnPluginUnloaded : Patch
		{
		}

		[Patch("OnGroupCreated", "OnGroupCreated", typeof(Permission), "CreateGroup")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Permissions")]
		[Info("Gets called when group got created.")]
		[Parameter("group", typeof(string), false)]
		[Parameter("title", typeof(string), false)]
		[Parameter("rank", typeof(int), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnGroupCreated : Patch
		{
		}

		[Patch("OnGroupDeleted", "OnGroupDeleted", typeof(Permission), "RemoveGroup")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Permissions")]
		[Info("Gets called when group got obliterated.")]
		[Parameter("group", typeof(string), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnGroupDeleted : Patch
		{
		}

		[Patch("OnGroupParentSet", "OnGroupParentSet", typeof(Permission), "SetGroupParent")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Permissions")]
		[Info("Gets called when group parent is set.")]
		[Parameter("group", typeof(string), false)]
		[Parameter("parentGroup", typeof(string), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnGroupParentSet : Patch
		{
		}

		[Patch("OnGroupPermissionGranted", "OnGroupPermissionGranted", typeof(Permission), "GrantGroupPermission")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Permissions")]
		[Info("Gets called when a permission has been assigned to a group.")]
		[Parameter("group", typeof(string), false)]
		[Parameter("permission", typeof(string), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnGroupPermissionGranted : Patch
		{
		}

		[Patch("OnGroupPermissionRevoked", "OnGroupPermissionRevoked", typeof(Permission), "RevokeGroupPermission")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Permissions")]
		[Info("Gets called when a permission has been revoked from a group.")]
		[Parameter("group", typeof(string), false)]
		[Parameter("permission", typeof(string), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnGroupPermissionRevoked : Patch
		{
		}

		[Patch("OnGroupRankSet", "OnGroupRankSet", typeof(Permission), "SetGroupRank")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Permissions")]
		[Info("Gets called when group rank is set.")]
		[Parameter("group", typeof(string), false)]
		[Parameter("rank", typeof(int), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnGroupRankSet : Patch
		{
		}

		[Patch("OnGroupTitleSet", "OnGroupTitleSet", typeof(Permission), "SetGroupTitle")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Permissions")]
		[Info("Gets called when group title is set.")]
		[Parameter("group", typeof(string), false)]
		[Parameter("title", typeof(string), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnGroupTitleSet : Patch
		{
		}

		[Patch("OnPermissionRegistered", "OnPermissionRegistered", typeof(Permission), "RegisterPermission")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Permissions")]
		[Info("Gets called when a permission has been registered for a plugin.")]
		[Parameter("permission", typeof(string), false)]
		[Parameter("plugin", typeof(Plugin), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnPermissionRegistered : Patch
		{
		}

		[Patch("OnPermissionsUnregistered", "OnPermissionsUnregistered", typeof(Permission), "UnregisterPermissions")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Permissions")]
		[Info("Gets called when all permission of a plugin have been unregistered.")]
		[Parameter("plugin", typeof(Plugin), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnPermissionsUnregistered : Patch
		{
		}

		[Patch("OnUserGroupAdded", "OnUserGroupAdded", typeof(Permission), "AddUserGroup")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Permissions")]
		[Info("Whenever an user is added to a group.")]
		[Parameter("playerId", typeof(string), false)]
		[Parameter("group", typeof(string), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnUserGroupAdded : Patch
		{
		}

		[Patch("OnUserGroupRemoved", "OnUserGroupRemoved", typeof(Permission), "RemoveUserGroup")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Permissions")]
		[Info("Whenever an user is removed from a group.")]
		[Parameter("playerId", typeof(string), false)]
		[Parameter("group", typeof(string), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnUserGroupRemoved : Patch
		{
		}

		[Patch("OnUserNameUpdated", "OnUserNameUpdated", typeof(Permission), "UpdateNickname")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Permissions")]
		[Info("Gets called the nickname of a player gets updated.")]
		[Parameter("id", typeof(string), false)]
		[Parameter("oldName", typeof(string), false)]
		[Parameter("newName", typeof(string), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnUserNameUpdated : Patch
		{
		}

		[Patch("OnUserPermissionGranted", "OnUserPermissionGranted", typeof(Permission), "GrantUserPermission")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Permissions")]
		[Info("Gets called when a permission has been granted to a user.")]
		[Parameter("playerId", typeof(string), false)]
		[Parameter("permission", typeof(string), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnUserPermissionGranted : Patch
		{
		}

		[Patch("OnUserPermissionRevoked", "OnUserPermissionRevoked", typeof(Permission), "RevokeUserPermission")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Permissions")]
		[Info("Gets called when a permission has been revoked from a user.")]
		[Parameter("playerId", typeof(string), false)]
		[Parameter("permission", typeof(string), false)]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnUserPermissionRevoked : Patch
		{
		}
	}
}
