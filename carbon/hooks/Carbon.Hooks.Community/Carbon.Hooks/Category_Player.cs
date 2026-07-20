using System;
using API.Hooks;
using Carbon.Core;
using Carbon.Modules;
using ConVar;
using Network;
using Oxide.Core.Libraries.Covalence;

namespace Carbon.Hooks;

public class Category_Player
{
	public class Player_Hooks
	{
		[Patch("CanClientLogin", "CanClientLogin", typeof(CorePlugin), "IOnUserApprove")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a client should or not should join the server.")]
		[Parameter("connection", typeof(Connection), false)]
		[Return(typeof(bool))]
		[OxideCompatible]
		public class CanClientLogin : Patch
		{
		}

		[Patch("CanUserLogin", "CanUserLogin", typeof(CorePlugin), "IOnUserApprove")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a client should or not should join the server.")]
		[Parameter("username", typeof(string), false)]
		[Parameter("userid", typeof(string), false)]
		[Parameter("ip", typeof(string), false)]
		[Return(typeof(bool))]
		[OxideCompatible]
		public class CanUserLogin : Patch
		{
		}

		[Patch("OnCarbonBanPlayer", "OnCarbonBanPlayer", typeof(AdminModule), "BanPlayer")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Called when a player becomes banned.")]
		[Parameter("invoker", typeof(BasePlayer), false)]
		[Parameter("target", typeof(BasePlayer), false)]
		[Parameter("reason", typeof(string), false)]
		[Parameter("expiry", typeof(long), false)]
		public class OnCarbonBanPlayer : Patch
		{
		}

		[Patch("OnCarbonBlinded", "OnCarbonBlinded", typeof(AdminModule), "BlindPlayer")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Called when a player becomes blind.")]
		[Info("Their screen and input will become obscured and blocked.")]
		[Parameter("invoker", typeof(BasePlayer), false)]
		[Parameter("target", typeof(BasePlayer), false)]
		public class OnCarbonBlinded : Patch
		{
		}

		[Patch("OnCarbonEmpowerPlayerStats", "OnCarbonEmpowerPlayerStats", typeof(AdminModule), "EmpowerPlayerStats")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Called when a player's health, metabolism, and stats are maxed out.")]
		[Parameter("invoker", typeof(BasePlayer), false)]
		[Parameter("target", typeof(BasePlayer), false)]
		public class OnCarbonEmpowerPlayerStats : Patch
		{
		}

		[Patch("OnCarbonKickPlayer", "OnCarbonKickPlayer", typeof(AdminModule), "KickPlayer")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Called when a player gets kicked.")]
		[Parameter("invoker", typeof(BasePlayer), false)]
		[Parameter("target", typeof(BasePlayer), false)]
		[Parameter("reason", typeof(string), false)]
		public class OnCarbonKickPlayer : Patch
		{
		}

		[Patch("OnCarbonLockPlayerContainer", "OnCarbonLockPlayerContainer", typeof(AdminModule), "LockPlayerContainer")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Called when a player's inventory container becomes locked or unlocked.")]
		[Parameter("invoker", typeof(BasePlayer), false)]
		[Parameter("target", typeof(BasePlayer), false)]
		[Parameter("container", typeof(ItemContainer), false)]
		[Parameter("locked", typeof(bool), false)]
		public class OnCarbonLockPlayerContainer : Patch
		{
		}

		[Patch("OnCarbonMutePlayer", "OnCarbonMutePlayer", typeof(AdminModule), "MutePlayer")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Called when a player becomes mute.")]
		[Parameter("invoker", typeof(BasePlayer), false)]
		[Parameter("target", typeof(BasePlayer), false)]
		[Parameter("wants", typeof(bool), false)]
		[Parameter("reason", typeof(string), false)]
		public class OnCarbonMutePlayer : Patch
		{
		}

		[Patch("OnCarbonPrivateMessage", "OnCarbonPrivateMessage", typeof(AdminModule), "PrivateMessagePlayer")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Called when a user sends a private message to another player via Carbon.")]
		[Parameter("player", typeof(BasePlayer), false)]
		[Parameter("target", typeof(BasePlayer), false)]
		[Parameter("message", typeof(string), false)]
		public class OnCarbonPrivateMessage : Patch
		{
		}

		[Patch("OnCarbonSpectateEnd", "OnCarbonSpectateEnd", typeof(AdminModule), "StopSpectating")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Called when a player stops spectating another player.")]
		[Parameter("player", typeof(BasePlayer), false)]
		[Parameter("target", typeof(BasePlayer), false)]
		public class OnCarbonSpectateEnd : Patch
		{
		}

		[Patch("OnCarbonSpectateStart", "OnCarbonSpectateStart", typeof(AdminModule), "StartSpectating")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Called when a player starts spectating another player.")]
		[Parameter("player", typeof(BasePlayer), false)]
		[Parameter("target", typeof(BasePlayer), false)]
		public class OnCarbonSpectateStart : Patch
		{
		}

		[Patch("OnCarbonUnbanPlayer", "OnCarbonUnbanPlayer", typeof(AdminModule), "UnbanPlayer")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Called when a player becomes banned.")]
		[Parameter("invoker", typeof(BasePlayer), false)]
		[Parameter("target", typeof(BasePlayer), false)]
		public class OnCarbonUnbanPlayer : Patch
		{
		}

		[Patch("OnCarbonUnblinded", "OnCarbonUnblinded", typeof(AdminModule), "UnblindPlayer")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Called when a player becomes unblinded.")]
		[Parameter("invoker", typeof(BasePlayer), false)]
		[Parameter("target", typeof(BasePlayer), false)]
		public class OnCarbonUnblinded : Patch
		{
		}

		[Patch("OnPlayerBanned", "OnPlayerBanned", typeof(CorePlugin), "IOnPlayerBanned")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a connection gets banned.")]
		[Parameter("connection", typeof(Connection), false)]
		[Parameter("reason", typeof(string), false)]
		[OxideCompatible]
		public class OnPlayerBanned : Patch
		{
		}

		[Patch("OnPlayerChat", "OnPlayerChat", typeof(CorePlugin), "IOnPlayerChat")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a player sends a chat message.")]
		[Parameter("player", typeof(BasePlayer), false)]
		[Parameter("message", typeof(string), false)]
		[Parameter("channel", typeof(ChatChannel), false)]
		[OxideCompatible]
		public class OnPlayerChat : Patch
		{
		}

		[Patch("OnPlayerCommand", "OnPlayerCommand", typeof(CorePlugin), "IOnPlayerCommand")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a player executes command.")]
		[Parameter("player", typeof(BasePlayer), false)]
		[Parameter("command", typeof(string), false)]
		[Parameter("args", typeof(string[]), false)]
		[Return(typeof(object))]
		[OxideCompatible]
		public class OnPlayerCommand : Patch
		{
		}

		[Patch("OnPlayerConnected", "OnPlayerConnected", typeof(CorePlugin), "IOnPlayerConnected")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Parameter("player", typeof(BasePlayer), false)]
		[OxideCompatible]
		public class OnPlayerConnected : Patch
		{
		}

		[Patch("OnPlayerLanguageChanged", "OnPlayerLanguageChanged [BasePlayer]", typeof(CorePlugin), "OnPlayerSetInfo")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a player's language gets changed.")]
		[Parameter("player", typeof(BasePlayer), false)]
		[Parameter("var", typeof(string), false)]
		[OxideCompatible]
		public class OnPlayerLanguageChanged_BasePlayer : Patch
		{
		}

		[Patch("OnPlayerLanguageChanged", "OnPlayerLanguageChanged [IPlayer]", typeof(CorePlugin), "OnPlayerSetInfo")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a player's language gets changed.")]
		[Parameter("player", typeof(IPlayer), false)]
		[Parameter("var", typeof(string), false)]
		[OxideCompatible]
		public class OnPlayerLanguageChanged_IPlayer : Patch
		{
		}

		[Patch("OnPlayerOfflineChat", "OnPlayerOfflineChat", typeof(CorePlugin), "OnPlayerOfflineChat")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a player sends an offline chat message.")]
		[Parameter("playerid", typeof(ulong), false)]
		[Parameter("username", typeof(string), false)]
		[Parameter("message", typeof(string), false)]
		[Parameter("channel", typeof(ChatChannel), false)]
		[OxideCompatible]
		public class OnPlayerOfflineChat : Patch
		{
		}

		[Patch("OnPlayerUnbanned", "OnPlayerUnbanned", typeof(CorePlugin), "OnServerUserRemove")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a player gets unbanned.")]
		[Parameter("playerName", typeof(string), false)]
		[Parameter("steamId", typeof(ulong), false)]
		[Parameter("address", typeof(string), false)]
		[OxideCompatible]
		public class OnPlayerUnbanned : Patch
		{
		}

		[Patch("OnUserApprove", "OnUserApprove", typeof(CorePlugin), "IOnUserApprove")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a connection is or not approved to join the server.")]
		[Parameter("connection", typeof(Connection), false)]
		[OxideCompatible]
		public class OnUserApprove : Patch
		{
		}

		[Patch("OnUserApproved", "OnUserApproved", typeof(CorePlugin), "IOnUserApprove")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a connection is approved to join the server.")]
		[Parameter("username", typeof(string), false)]
		[Parameter("userid", typeof(string), false)]
		[Parameter("ip", typeof(string), false)]
		[OxideCompatible]
		public class OnUserApproved : Patch
		{
		}

		[Patch("OnUserBanned", "OnUserBanned", typeof(CorePlugin), "OnServerUserSet")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a player gets banned.")]
		[Parameter("playerName", typeof(string), false)]
		[Parameter("playerId", typeof(string), false)]
		[Parameter("address", typeof(string), false)]
		[Parameter("reason", typeof(string), false)]
		[Parameter("expiry", typeof(long), false)]
		[OxideCompatible]
		public class OnUserBanned : Patch
		{
		}

		[Patch("OnUserChat", "OnUserChat", typeof(CorePlugin), "IOnPlayerChat")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a player sends a chat message.")]
		[Parameter("player", typeof(IPlayer), false)]
		[Parameter("message", typeof(string), false)]
		[OxideCompatible]
		public class OnUserChat : Patch
		{
		}

		[Patch("OnUserCommand", "OnUserCommand", typeof(CorePlugin), "IOnPlayerCommand")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a player executes command.")]
		[Parameter("player", typeof(BasePlayer), false)]
		[Parameter("command", typeof(string), false)]
		[Parameter("args", typeof(string[]), false)]
		[Return(typeof(object))]
		[OxideCompatible]
		public class OnUserCommand_BasePlayer : Patch
		{
		}

		[Patch("OnUserCommand", "OnUserCommand", typeof(CorePlugin), "IOnPlayerCommand")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a player executes command.")]
		[Parameter("player", typeof(IPlayer), false)]
		[Parameter("command", typeof(string), false)]
		[Parameter("args", typeof(string[]), false)]
		[Return(typeof(object))]
		[OxideCompatible]
		public class OnUserCommand_IPlayer : Patch
		{
		}

		[Patch("OnUserConnected", "OnUserConnected", typeof(CorePlugin), "IOnPlayerConnected")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Parameter("player", typeof(IPlayer), false)]
		[OxideCompatible]
		public class OnUserConnected : Patch
		{
		}

		[Patch("OnUserDisconnected", "OnUserDisconnected", typeof(CorePlugin), "OnPlayerDisconnected")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Parameter("player", typeof(IPlayer), false)]
		[Parameter("reason", typeof(string), false)]
		[OxideCompatible]
		public class OnUserDisconnected : Patch
		{
		}

		[Patch("OnUserKicked", "OnUserKicked", typeof(CorePlugin), "OnPlayerKicked")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a covalence player gets kicked.")]
		[Parameter("player", typeof(IPlayer), false)]
		[Parameter("reason", typeof(string), false)]
		[OxideCompatible]
		public class OnUserKicked : Patch
		{
		}

		[Patch("OnUserRespawn", "OnUserRespawn", typeof(CorePlugin), "OnPlayerRespawn")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a covalence player respawns.")]
		[Parameter("player", typeof(IPlayer), false)]
		[OxideCompatible]
		public class OnUserRespawn : Patch
		{
		}

		[Patch("OnUserRespawned", "OnUserRespawned", typeof(CorePlugin), "OnPlayerRespawned")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a covalence player fully respawned.")]
		[Parameter("player", typeof(IPlayer), false)]
		[OxideCompatible]
		public class OnUserRespawned : Patch
		{
		}

		[Patch("OnUserUnbanned", "OnUserUnbanned", typeof(CorePlugin), "OnServerUserRemove")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Info("Gets called when a player gets unbanned.")]
		[Parameter("playerName", typeof(string), false)]
		[Parameter("playerId", typeof(string), false)]
		[Parameter("address", typeof(string), false)]
		[OxideCompatible]
		public class OnUserUnbanned : Patch
		{
		}

		[Patch("OnWireClear", "OnWireClear", "WireTool", "AttemptClearSlot", new string[] { "BaseNetworkable", "BasePlayer", "System.Int32", "System.Boolean" })]
		[Category("Player")]
		[Info("Gets called when a player attempts to clear an IO slot.")]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("ioA", "IOEntity", false)]
		[Parameter("clearIndex", typeof(int), false)]
		[Parameter("ioB", "IOEntity", false)]
		[Parameter("isInput", typeof(bool), false)]
		[Return(typeof(bool))]
		[OxideCompatible]
		public class OnWireClear : Patch
		{
			public static bool Prefix(ref bool __result, BaseNetworkable clearEnt, BasePlayer ply, int clearIndex, bool isInput)
			{
				IOEntity val = (IOEntity)(object)((clearEnt is IOEntity) ? clearEnt : null);
				if (val == null)
				{
					return true;
				}
				IOEntity val2 = (isInput ? val.inputs[clearIndex] : val.outputs[clearIndex]).connectedTo.Get(true);
				if (!BaseNetworkableEx.IsValid((BaseNetworkable)(object)val2))
				{
					return true;
				}
				if (HookCaller.CallStaticHook(1879512085u, (object)ply, (object)val, (object)clearIndex, (object)val2, (object)isInput) is bool flag)
				{
					__result = flag;
					return false;
				}
				return true;
			}
		}
	}

	public class BasePlayer_Player
	{
		[Patch("CanPlayerInheritNetworkGroup", "CanPlayerInheritNetworkGroup", typeof(BasePlayer), "ShouldInheritNetworkGroup", new Type[] { })]
		[Parameter("player", typeof(BasePlayer), false)]
		[Info("Overrides the IsSpectating check, overriding the result.")]
		[Return(typeof(bool))]
		public class CanPlayerInheritNetworkGroup : Patch
		{
			public static bool Prefix(ref BasePlayer __instance, ref bool __result)
			{
				if (!(HookCaller.CallStaticHook(617273774u, (object)__instance) is bool flag))
				{
					return true;
				}
				__result = flag;
				return false;
			}
		}
	}
}
