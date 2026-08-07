using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Core.RemoteConsole;
using Oxide.Game.Rust.Libraries;
using Oxide.Game.Rust.Libraries.Covalence;
using Rust.Ai.Gen2;
using Steamworks;
using UnityEngine;

namespace Oxide.Game.Rust;

public class RustCore : CSPlugin
{
	internal readonly Command cmdlib = Interface.Oxide.GetLibrary<Command>();

	internal readonly Lang lang = Interface.Oxide.GetLibrary<Lang>();

	internal readonly Permission permission = Interface.Oxide.GetLibrary<Permission>();

	internal readonly Oxide.Game.Rust.Libraries.Player Player = Interface.Oxide.GetLibrary<Oxide.Game.Rust.Libraries.Player>();

	internal static readonly RustCovalenceProvider Covalence = RustCovalenceProvider.Instance;

	internal readonly PluginManager pluginManager = Interface.Oxide.RootPluginManager;

	internal readonly IServer Server = Covalence.CreateServer();

	internal readonly RustExtension Extension;

	internal bool serverInitialized;

	internal bool isPlayerTakingDamage;

	internal static string ipPattern = ":{1}[0-9]{1}\\d*";

	private static readonly DateTime Eoy = new DateTime(2026, 12, 31);

	internal static IEnumerable<string> RestrictedCommands => new string[4] { "ownerid", "moderatorid", "removeowner", "removemoderator" };

	[HookMethod("GrantCommand")]
	private void GrantCommand(IPlayer player, string command, string[] args)
	{
		if (!PermissionsLoaded(player))
		{
			return;
		}
		if (args.Length < 3)
		{
			player.Reply(lang.GetMessage("CommandUsageGrant", this, player.Id));
			return;
		}
		string text = args[0];
		string text2 = args[1].Sanitize();
		string text3 = args[2];
		if (!permission.PermissionExists(text3))
		{
			player.Reply(string.Format(lang.GetMessage("PermissionNotFound", this, player.Id), text3));
		}
		else if (text.Equals("group"))
		{
			if (!permission.GroupExists(text2))
			{
				player.Reply(string.Format(lang.GetMessage("GroupNotFound", this, player.Id), text2));
				return;
			}
			if (permission.GroupHasPermission(text2, text3))
			{
				player.Reply(string.Format(lang.GetMessage("GroupAlreadyHasPermission", this, player.Id), text2, text3));
				return;
			}
			permission.GrantGroupPermission(text2, text3, null);
			player.Reply(string.Format(lang.GetMessage("GroupPermissionGranted", this, player.Id), text2, text3));
		}
		else if (text.Equals("user"))
		{
			IPlayer[] array = Covalence.PlayerManager.FindPlayers(text2).ToArray();
			if (array.Length > 1)
			{
				player.Reply(string.Format(lang.GetMessage("PlayersFound", this, player.Id), string.Join(", ", array.Select((IPlayer p) => p.Name).ToArray())));
				return;
			}
			IPlayer player2 = ((array.Length == 1) ? array[0] : null);
			if (player2 == null && !permission.UserIdValid(text2))
			{
				player.Reply(string.Format(lang.GetMessage("PlayerNotFound", this, player.Id), text2));
				return;
			}
			string text4 = text2;
			if (player2 != null)
			{
				text4 = player2.Id;
				text2 = player2.Name;
				permission.UpdateNickname(text4, text2);
			}
			if (permission.UserHasPermission(text2, text3))
			{
				player.Reply(string.Format(lang.GetMessage("PlayerAlreadyHasPermission", this, player.Id), text4, text3));
				return;
			}
			permission.GrantUserPermission(text4, text3, null);
			player.Reply(string.Format(lang.GetMessage("PlayerPermissionGranted", this, player.Id), text2 + " (" + text4 + ")", text3));
		}
		else
		{
			player.Reply(lang.GetMessage("CommandUsageGrant", this, player.Id));
		}
	}

	[HookMethod("GroupCommand")]
	private void GroupCommand(IPlayer player, string command, string[] args)
	{
		if (!PermissionsLoaded(player))
		{
			return;
		}
		if (args.Length < 2)
		{
			player.Reply(lang.GetMessage("CommandUsageGroup", this, player.Id));
			player.Reply(lang.GetMessage("CommandUsageGroupParent", this, player.Id));
			player.Reply(lang.GetMessage("CommandUsageGroupRemove", this, player.Id));
			return;
		}
		string text = args[0];
		string text2 = args[1];
		string groupTitle = ((args.Length >= 3) ? args[2] : "");
		int groupRank = ((args.Length == 4) ? int.Parse(args[3]) : 0);
		if (text.Equals("add"))
		{
			if (permission.GroupExists(text2))
			{
				player.Reply(string.Format(lang.GetMessage("GroupAlreadyExists", this, player.Id), text2));
				return;
			}
			permission.CreateGroup(text2, groupTitle, groupRank);
			player.Reply(string.Format(lang.GetMessage("GroupCreated", this, player.Id), text2));
		}
		else if (text.Equals("remove"))
		{
			if (!permission.GroupExists(text2))
			{
				player.Reply(string.Format(lang.GetMessage("GroupNotFound", this, player.Id), text2));
				return;
			}
			permission.RemoveGroup(text2);
			player.Reply(string.Format(lang.GetMessage("GroupDeleted", this, player.Id), text2));
		}
		else if (text.Equals("set"))
		{
			if (!permission.GroupExists(text2))
			{
				player.Reply(string.Format(lang.GetMessage("GroupNotFound", this, player.Id), text2));
				return;
			}
			permission.SetGroupTitle(text2, groupTitle);
			permission.SetGroupRank(text2, groupRank);
			player.Reply(string.Format(lang.GetMessage("GroupChanged", this, player.Id), text2));
		}
		else if (text.Equals("parent"))
		{
			if (args.Length <= 2)
			{
				player.Reply(lang.GetMessage("CommandUsageGroupParent", this, player.Id));
				return;
			}
			if (!permission.GroupExists(text2))
			{
				player.Reply(string.Format(lang.GetMessage("GroupNotFound", this, player.Id), text2));
				return;
			}
			string text3 = args[2];
			if (!string.IsNullOrEmpty(text3) && !permission.GroupExists(text3))
			{
				player.Reply(string.Format(lang.GetMessage("GroupParentNotFound", this, player.Id), text3));
			}
			else if (permission.SetGroupParent(text2, text3))
			{
				player.Reply(string.Format(lang.GetMessage("GroupParentChanged", this, player.Id), text2, text3));
			}
			else
			{
				player.Reply(string.Format(lang.GetMessage("GroupParentNotChanged", this, player.Id), text2));
			}
		}
		else
		{
			player.Reply(lang.GetMessage("CommandUsageGroup", this, player.Id));
			player.Reply(lang.GetMessage("CommandUsageGroupParent", this, player.Id));
			player.Reply(lang.GetMessage("CommandUsageGroupRemove", this, player.Id));
		}
	}

	[HookMethod("LangCommand")]
	private void LangCommand(IPlayer player, string command, string[] args)
	{
		if (args.Length < 1)
		{
			player.Reply(lang.GetMessage("CommandUsageLang", this, player.Id));
			return;
		}
		string text = args[0];
		try
		{
			text = new CultureInfo(text)?.TwoLetterISOLanguageName;
		}
		catch (CultureNotFoundException)
		{
			player.Reply(lang.GetMessage("InvalidLanguageName", this, player.Id), text);
			return;
		}
		if (player.IsServer)
		{
			lang.SetServerLanguage(text);
			player.Reply(string.Format(lang.GetMessage("ServerLanguage", this, player.Id), lang.GetServerLanguage()));
		}
		else
		{
			lang.SetLanguage(text, player.Id);
			player.Reply(string.Format(lang.GetMessage("PlayerLanguage", this, player.Id), text));
		}
	}

	[HookMethod("LoadCommand")]
	private void LoadCommand(IPlayer player, string command, string[] args)
	{
		if (args.Length < 1)
		{
			player.Reply(lang.GetMessage("CommandUsageLoad", this, player.Id));
			return;
		}
		if (args[0].Equals("*") || args[0].Equals("all"))
		{
			Interface.Oxide.LoadAllPlugins();
			return;
		}
		foreach (string value in args)
		{
			if (!string.IsNullOrEmpty(value))
			{
				Interface.Oxide.LoadPlugin(value);
				pluginManager.GetPlugin(value);
			}
		}
	}

	[HookMethod("PluginsCommand")]
	private void PluginsCommand(IPlayer player)
	{
		Plugin[] array = (from pl in pluginManager.GetPlugins()
			where !pl.IsCorePlugin
			select pl).ToArray();
		HashSet<string> second = new HashSet<string>(array.Select((Plugin pl) => pl.Name));
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (PluginLoader pluginLoader in Interface.Oxide.GetPluginLoaders())
		{
			foreach (string item in pluginLoader.ScanDirectory(Interface.Oxide.PluginDirectory).Except(second))
			{
				dictionary[item] = (pluginLoader.PluginErrors.TryGetValue(item, out var value) ? value : "Unloaded");
			}
		}
		int num = array.Length + dictionary.Count;
		if (num < 1)
		{
			player.Reply(lang.GetMessage("NoPluginsFound", this, player.Id));
			return;
		}
		string text = $"Listing {array.Length + dictionary.Count} plugins:";
		int num2 = 1;
		foreach (Plugin item2 in array.Where((Plugin p) => p.Filename != null))
		{
			text += string.Format("\n  {0:00} \"{1}\" ({2}) by {3} ({4:0.00}s / {5}) - {6}", new object[7]
			{
				num2++,
				item2.Title,
				item2.Version,
				item2.Author,
				item2.TotalHookTime,
				FormatBytes(item2.TotalHookMemory),
				item2.Filename.Basename()
			});
		}
		foreach (string key in dictionary.Keys)
		{
			text += $"\n  {num2++:00} {key} - {dictionary[key]}";
		}
		player.Reply(text);
	}

	private static string FormatBytes(long bytes)
	{
		if (bytes < 1024)
		{
			return $"{bytes:0} B";
		}
		if (bytes < 1048576)
		{
			return $"{bytes / 1024:0} KB";
		}
		if (bytes < 1073741824)
		{
			return $"{bytes / 1048576:0} MB";
		}
		return $"{bytes / 1073741824:0} GB";
	}

	[HookMethod("ReloadCommand")]
	private void ReloadCommand(IPlayer player, string command, string[] args)
	{
		if (args.Length < 1)
		{
			player.Reply(lang.GetMessage("CommandUsageReload", this, player.Id));
			return;
		}
		if (args[0].Equals("*") || args[0].Equals("all"))
		{
			Interface.Oxide.ReloadAllPlugins();
			return;
		}
		foreach (string value in args)
		{
			if (!string.IsNullOrEmpty(value))
			{
				Interface.Oxide.ReloadPlugin(value);
			}
		}
	}

	[HookMethod("RevokeCommand")]
	private void RevokeCommand(IPlayer player, string command, string[] args)
	{
		if (!PermissionsLoaded(player))
		{
			return;
		}
		if (args.Length < 3)
		{
			player.Reply(lang.GetMessage("CommandUsageRevoke", this, player.Id));
			return;
		}
		string text = args[0];
		string text2 = args[1].Sanitize();
		string text3 = args[2];
		if (text.Equals("group"))
		{
			if (!permission.GroupExists(text2))
			{
				player.Reply(string.Format(lang.GetMessage("GroupNotFound", this, player.Id), text2));
				return;
			}
			if (!text3.EndsWith("*") && !permission.GroupHasPermission(text2, text3))
			{
				player.Reply(string.Format(lang.GetMessage("GroupDoesNotHavePermission", this, player.Id), text2, text3));
				return;
			}
			permission.RevokeGroupPermission(text2, text3);
			player.Reply(string.Format(lang.GetMessage("GroupPermissionRevoked", this, player.Id), text2, text3));
		}
		else if (text.Equals("user"))
		{
			IPlayer[] array = Covalence.PlayerManager.FindPlayers(text2).ToArray();
			if (array.Length > 1)
			{
				player.Reply(string.Format(lang.GetMessage("PlayersFound", this, player.Id), string.Join(", ", array.Select((IPlayer p) => p.Name).ToArray())));
				return;
			}
			IPlayer player2 = ((array.Length == 1) ? array[0] : null);
			if (player2 == null && !permission.UserIdValid(text2))
			{
				player.Reply(string.Format(lang.GetMessage("PlayerNotFound", this, player.Id), text2));
				return;
			}
			string text4 = text2;
			if (player2 != null)
			{
				text4 = player2.Id;
				text2 = player2.Name;
				permission.UpdateNickname(text4, text2);
			}
			if (!text3.EndsWith("*") && !permission.UserHasPermission(text4, text3))
			{
				player.Reply(string.Format(lang.GetMessage("PlayerDoesNotHavePermission", this, player.Id), text2, text3));
				return;
			}
			permission.RevokeUserPermission(text4, text3);
			player.Reply(string.Format(lang.GetMessage("PlayerPermissionRevoked", this, player.Id), text2 + " (" + text4 + ")", text3));
		}
		else
		{
			player.Reply(lang.GetMessage("CommandUsageRevoke", this, player.Id));
		}
	}

	[HookMethod("ShowCommand")]
	private void ShowCommand(IPlayer player, string command, string[] args)
	{
		if (!PermissionsLoaded(player))
		{
			return;
		}
		if (args.Length < 1)
		{
			player.Reply(lang.GetMessage("CommandUsageShow", this, player.Id));
			player.Reply(lang.GetMessage("CommandUsageShowName", this, player.Id));
			return;
		}
		string text = args[0];
		string text2 = ((args.Length == 2) ? args[1].Sanitize() : string.Empty);
		if (text.Equals("perms"))
		{
			player.Reply(string.Format(lang.GetMessage("Permissions", this, player.Id) + ":\n" + string.Join(", ", permission.GetPermissions()), Array.Empty<object>()));
		}
		else if (text.Equals("perm"))
		{
			if (args.Length < 2 || string.IsNullOrEmpty(text2))
			{
				player.Reply(lang.GetMessage("CommandUsageShow", this, player.Id));
				player.Reply(lang.GetMessage("CommandUsageShowName", this, player.Id));
				return;
			}
			string[] permissionUsers = permission.GetPermissionUsers(text2);
			string[] permissionGroups = permission.GetPermissionGroups(text2);
			string text3 = string.Format(lang.GetMessage("PermissionPlayers", this, player.Id), text2) + ":\n";
			text3 += ((permissionUsers.Length != 0) ? string.Join(", ", permissionUsers) : lang.GetMessage("NoPermissionPlayers", this, player.Id));
			text3 = text3 + "\n\n" + string.Format(lang.GetMessage("PermissionGroups", this, player.Id), text2) + ":\n";
			text3 += ((permissionGroups.Length != 0) ? string.Join(", ", permissionGroups) : lang.GetMessage("NoPermissionGroups", this, player.Id));
			player.Reply(text3);
		}
		else if (text.Equals("user"))
		{
			if (args.Length < 2 || string.IsNullOrEmpty(text2))
			{
				player.Reply(lang.GetMessage("CommandUsageShow", this, player.Id));
				player.Reply(lang.GetMessage("CommandUsageShowName", this, player.Id));
				return;
			}
			IPlayer[] array = Covalence.PlayerManager.FindPlayers(text2).ToArray();
			if (array.Length > 1)
			{
				player.Reply(string.Format(lang.GetMessage("PlayersFound", this, player.Id), string.Join(", ", array.Select((IPlayer p) => p.Name).ToArray())));
				return;
			}
			IPlayer player2 = ((array.Length == 1) ? array[0] : null);
			if (player2 == null && !permission.UserIdValid(text2))
			{
				player.Reply(string.Format(lang.GetMessage("PlayerNotFound", this, player.Id), text2));
				return;
			}
			string text4 = text2;
			if (player2 != null)
			{
				text4 = player2.Id;
				text2 = player2.Name;
				permission.UpdateNickname(text4, text2);
				text2 = text2 + " (" + text4 + ")";
			}
			string[] userPermissions = permission.GetUserPermissions(text4);
			string[] userGroups = permission.GetUserGroups(text4);
			string text5 = string.Format(lang.GetMessage("PlayerPermissions", this, player.Id), text2) + ":\n";
			text5 += ((userPermissions.Length != 0) ? string.Join(", ", userPermissions) : lang.GetMessage("NoPlayerPermissions", this, player.Id));
			text5 = text5 + "\n\n" + string.Format(lang.GetMessage("PlayerGroups", this, player.Id), text2) + ":\n";
			text5 += ((userGroups.Length != 0) ? string.Join(", ", userGroups) : lang.GetMessage("NoPlayerGroups", this, player.Id));
			player.Reply(text5);
		}
		else if (text.Equals("group"))
		{
			if (args.Length < 2 || string.IsNullOrEmpty(text2))
			{
				player.Reply(lang.GetMessage("CommandUsageShow", this, player.Id));
				player.Reply(lang.GetMessage("CommandUsageShowName", this, player.Id));
				return;
			}
			if (!permission.GroupExists(text2))
			{
				player.Reply(string.Format(lang.GetMessage("GroupNotFound", this, player.Id), text2));
				return;
			}
			string[] usersInGroup = permission.GetUsersInGroup(text2);
			string[] groupPermissions = permission.GetGroupPermissions(text2);
			string text6 = string.Format(lang.GetMessage("GroupPlayers", this, player.Id), text2) + ":\n";
			text6 += ((usersInGroup.Length != 0) ? string.Join(", ", usersInGroup) : lang.GetMessage("NoPlayersInGroup", this, player.Id));
			text6 = text6 + "\n\n" + string.Format(lang.GetMessage("GroupPermissions", this, player.Id), text2) + ":\n";
			text6 += ((groupPermissions.Length != 0) ? string.Join(", ", groupPermissions) : lang.GetMessage("NoGroupPermissions", this, player.Id));
			string groupParent = permission.GetGroupParent(text2);
			while (permission.GroupExists(groupParent))
			{
				text6 = text6 + "\n" + string.Format(lang.GetMessage("ParentGroupPermissions", this, player.Id), groupParent) + ":\n";
				text6 += string.Join(", ", permission.GetGroupPermissions(groupParent));
				groupParent = permission.GetGroupParent(groupParent);
			}
			player.Reply(text6);
		}
		else if (text.Equals("groups"))
		{
			player.Reply(string.Format(lang.GetMessage("Groups", this, player.Id) + ":\n" + string.Join(", ", permission.GetGroups()), Array.Empty<object>()));
		}
		else
		{
			player.Reply(lang.GetMessage("CommandUsageShow", this, player.Id));
			player.Reply(lang.GetMessage("CommandUsageShowName", this, player.Id));
		}
	}

	[HookMethod("UnloadCommand")]
	private void UnloadCommand(IPlayer player, string command, string[] args)
	{
		if (args.Length < 1)
		{
			player.Reply(lang.GetMessage("CommandUsageUnload", this, player.Id));
			return;
		}
		if (args[0].Equals("*") || args[0].Equals("all"))
		{
			Interface.Oxide.UnloadAllPlugins();
			return;
		}
		foreach (string value in args)
		{
			if (!string.IsNullOrEmpty(value))
			{
				Interface.Oxide.UnloadPlugin(value);
			}
		}
	}

	[HookMethod("UserGroupCommand")]
	private void UserGroupCommand(IPlayer player, string command, string[] args)
	{
		if (!PermissionsLoaded(player))
		{
			return;
		}
		if (args.Length < 3)
		{
			player.Reply(lang.GetMessage("CommandUsageUserGroup", this, player.Id));
			return;
		}
		string text = args[0];
		string text2 = args[1].Sanitize();
		string text3 = args[2];
		IPlayer[] array = Covalence.PlayerManager.FindPlayers(text2).ToArray();
		if (array.Length > 1)
		{
			player.Reply(string.Format(lang.GetMessage("PlayersFound", this, player.Id), string.Join(", ", array.Select((IPlayer p) => p.Name).ToArray())));
			return;
		}
		IPlayer player2 = ((array.Length == 1) ? array[0] : null);
		if (player2 == null && !permission.UserIdValid(text2))
		{
			player.Reply(string.Format(lang.GetMessage("PlayerNotFound", this, player.Id), text2));
			return;
		}
		string text4 = text2;
		if (player2 != null)
		{
			text4 = player2.Id;
			text2 = player2.Name;
			permission.UpdateNickname(text4, text2);
			text2 = text2 + "(" + text4 + ")";
		}
		if (!permission.GroupExists(text3))
		{
			player.Reply(string.Format(lang.GetMessage("GroupNotFound", this, player.Id), text3));
		}
		else if (text.Equals("add"))
		{
			permission.AddUserGroup(text4, text3);
			player.Reply(string.Format(lang.GetMessage("PlayerAddedToGroup", this, player.Id), text2, text3));
		}
		else if (text.Equals("remove"))
		{
			permission.RemoveUserGroup(text4, text3);
			player.Reply(string.Format(lang.GetMessage("PlayerRemovedFromGroup", this, player.Id), text2, text3));
		}
		else
		{
			player.Reply(lang.GetMessage("CommandUsageUserGroup", this, player.Id));
		}
	}

	[HookMethod("VersionCommand")]
	private void VersionCommand(IPlayer player)
	{
		if (player.IsServer)
		{
			string format = "Oxide.Rust Version: {0}\nOxide.Rust Branch: {1}";
			player.Reply(string.Format(format, RustExtension.AssemblyVersion, Extension.Branch));
			return;
		}
		string format2 = Covalence.FormatText(lang.GetMessage("Version", this, player.Id));
		player.Reply(string.Format(format2, new object[4]
		{
			RustExtension.AssemblyVersion,
			Covalence.GameName,
			Server.Version,
			Server.Protocol
		}));
	}

	[HookMethod("SaveCommand")]
	private void SaveCommand(IPlayer player)
	{
		if (PermissionsLoaded(player) && player.IsAdmin)
		{
			Interface.Oxide.OnSave();
			Covalence.PlayerManager.SavePlayerData();
			player.Reply(lang.GetMessage("DataSaved", this, player.Id));
		}
	}

	public RustCore()
	{
		Extension = Interface.Oxide.GetExtension<RustExtension>();
		base.Title = "Rust";
		base.Author = Extension.Author;
		base.Version = Extension.Version;
	}

	private bool PermissionsLoaded(IPlayer player)
	{
		if (!permission.IsLoaded)
		{
			player.Reply(string.Format(lang.GetMessage("PermissionsNotLoaded", this, player.Id), permission.LastException.Message));
			return false;
		}
		return true;
	}

	[HookMethod("Init")]
	private void Init()
	{
		RemoteLogger.SetTag("game", base.Title.ToLower());
		RemoteLogger.SetTag("game version", Server.Version);
		AddCovalenceCommand(new string[3] { "oxide.plugins", "o.plugins", "plugins" }, "PluginsCommand", "oxide.plugins");
		AddCovalenceCommand(new string[3] { "oxide.load", "o.load", "plugin.load" }, "LoadCommand", "oxide.load");
		AddCovalenceCommand(new string[3] { "oxide.reload", "o.reload", "plugin.reload" }, "ReloadCommand", "oxide.reload");
		AddCovalenceCommand(new string[3] { "oxide.unload", "o.unload", "plugin.unload" }, "UnloadCommand", "oxide.unload");
		AddCovalenceCommand(new string[3] { "oxide.grant", "o.grant", "perm.grant" }, "GrantCommand", "oxide.grant");
		AddCovalenceCommand(new string[3] { "oxide.group", "o.group", "perm.group" }, "GroupCommand", "oxide.group");
		AddCovalenceCommand(new string[3] { "oxide.revoke", "o.revoke", "perm.revoke" }, "RevokeCommand", "oxide.revoke");
		AddCovalenceCommand(new string[3] { "oxide.show", "o.show", "perm.show" }, "ShowCommand", "oxide.show");
		AddCovalenceCommand(new string[3] { "oxide.usergroup", "o.usergroup", "perm.usergroup" }, "UserGroupCommand", "oxide.usergroup");
		AddCovalenceCommand(new string[3] { "oxide.lang", "o.lang", "lang" }, "LangCommand");
		AddCovalenceCommand(new string[2] { "oxide.save", "o.save" }, "SaveCommand");
		AddCovalenceCommand(new string[2] { "oxide.version", "o.version" }, "VersionCommand");
		foreach (KeyValuePair<string, Dictionary<string, string>> language in Localization.languages)
		{
			lang.RegisterMessages(language.Value, this, language.Key);
		}
		if (!permission.IsLoaded)
		{
			return;
		}
		int num = 0;
		foreach (string defaultGroup in Interface.Oxide.Config.Options.DefaultGroups)
		{
			if (!permission.GroupExists(defaultGroup))
			{
				permission.CreateGroup(defaultGroup, defaultGroup, num++);
			}
		}
		permission.RegisterValidate(delegate(string s)
		{
			if (ulong.TryParse(s, out var result))
			{
				int num2 = ((result == 0L) ? 1 : ((int)Math.Floor(Math.Log10(result) + 1.0)));
				return num2 >= 17;
			}
			return false;
		});
		permission.CleanUp();
	}

	[HookMethod("OnPluginLoaded")]
	private void OnPluginLoaded(Plugin plugin)
	{
		if (serverInitialized)
		{
			plugin.CallHook("OnServerInitialized", false);
		}
	}

	[HookMethod("IOnServerInitialized")]
	private void IOnServerInitialized()
	{
		if (!serverInitialized)
		{
			Analytics.Collect();
			if (!Interface.Oxide.Config.Options.Modded)
			{
				Interface.Oxide.LogWarning("The server is currently listed under Community. Please be aware that Facepunch only allows admin tools (that do not affect gameplay) under the Community section");
			}
			serverInitialized = true;
			Interface.CallHook("OnServerInitialized", serverInitialized);
		}
	}

	[HookMethod("OnServerSave")]
	private void OnServerSave()
	{
		Interface.Oxide.OnSave();
		Covalence.PlayerManager.SavePlayerData();
	}

	[HookMethod("IOnServerShutdown")]
	private void IOnServerShutdown()
	{
		Interface.Oxide.CallHook("OnServerShutdown");
		Interface.Oxide.OnShutdown();
		Covalence.PlayerManager.SavePlayerData();
	}

	private void ParseCommand(string argstr, out string command, out string[] args)
	{
		List<string> list = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		foreach (char c in argstr)
		{
			if (c == '"')
			{
				if (flag)
				{
					string text = stringBuilder.ToString().Trim();
					if (!string.IsNullOrEmpty(text))
					{
						list.Add(text);
					}
					stringBuilder.Clear();
					flag = false;
				}
				else
				{
					flag = true;
				}
			}
			else if (char.IsWhiteSpace(c) && !flag)
			{
				string text2 = stringBuilder.ToString().Trim();
				if (!string.IsNullOrEmpty(text2))
				{
					list.Add(text2);
				}
				stringBuilder.Clear();
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		if (stringBuilder.Length > 0)
		{
			string text3 = stringBuilder.ToString().Trim();
			if (!string.IsNullOrEmpty(text3))
			{
				list.Add(text3);
			}
		}
		if (list.Count == 0)
		{
			command = null;
			args = null;
		}
		else
		{
			command = list[0];
			list.RemoveAt(0);
			args = list.ToArray();
		}
	}

	public static BasePlayer FindPlayer(string nameOrIdOrIp)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer result = null;
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (string.IsNullOrEmpty(current.UserIDString))
				{
					continue;
				}
				if (current.UserIDString.Equals(nameOrIdOrIp))
				{
					return current;
				}
				if (!string.IsNullOrEmpty(current.displayName))
				{
					if (current.displayName.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase))
					{
						return current;
					}
					if (StringEx.Contains(current.displayName, nameOrIdOrIp, CompareOptions.OrdinalIgnoreCase))
					{
						result = current;
					}
					if (current.net?.connection != null && current.net.connection.ipaddress.Equals(nameOrIdOrIp))
					{
						return current;
					}
					if (current.net?.connection != null && ((object)Unsafe.As<NetworkableId, NetworkableId>(ref current.net.ID)/*cast due to constrained. prefix*/).Equals((object?)nameOrIdOrIp))
					{
						return current;
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Enumerator<BasePlayer> enumerator2 = BasePlayer.sleepingPlayerList.GetEnumerator();
		try
		{
			while (enumerator2.MoveNext())
			{
				BasePlayer current2 = enumerator2.Current;
				if (string.IsNullOrEmpty(current2.UserIDString))
				{
					continue;
				}
				if (current2.UserIDString.Equals(nameOrIdOrIp))
				{
					return current2;
				}
				if (!string.IsNullOrEmpty(current2.displayName))
				{
					if (current2.displayName.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase))
					{
						return current2;
					}
					if (StringEx.Contains(current2.displayName, nameOrIdOrIp, CompareOptions.OrdinalIgnoreCase))
					{
						result = current2;
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
		}
		return result;
	}

	public static BasePlayer FindPlayerByName(string name)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer result = null;
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (!string.IsNullOrEmpty(current.displayName))
				{
					if (current.displayName.Equals(name, StringComparison.OrdinalIgnoreCase))
					{
						return current;
					}
					if (StringEx.Contains(current.displayName, name, CompareOptions.OrdinalIgnoreCase))
					{
						result = current;
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Enumerator<BasePlayer> enumerator2 = BasePlayer.sleepingPlayerList.GetEnumerator();
		try
		{
			while (enumerator2.MoveNext())
			{
				BasePlayer current2 = enumerator2.Current;
				if (!string.IsNullOrEmpty(current2.displayName))
				{
					if (current2.displayName.Equals(name, StringComparison.OrdinalIgnoreCase))
					{
						return current2;
					}
					if (StringEx.Contains(current2.displayName, name, CompareOptions.OrdinalIgnoreCase))
					{
						result = current2;
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
		}
		return result;
	}

	public static BasePlayer FindPlayerById(ulong id)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if ((ulong)current.userID == id)
				{
					return current;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Enumerator<BasePlayer> enumerator2 = BasePlayer.sleepingPlayerList.GetEnumerator();
		try
		{
			while (enumerator2.MoveNext())
			{
				BasePlayer current2 = enumerator2.Current;
				if ((ulong)current2.userID == id)
				{
					return current2;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
		}
		return null;
	}

	public static BasePlayer FindPlayerByIdString(string id)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (string.IsNullOrEmpty(current.UserIDString) || !current.UserIDString.Equals(id))
				{
					continue;
				}
				return current;
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Enumerator<BasePlayer> enumerator2 = BasePlayer.sleepingPlayerList.GetEnumerator();
		try
		{
			while (enumerator2.MoveNext())
			{
				BasePlayer current2 = enumerator2.Current;
				if (string.IsNullOrEmpty(current2.UserIDString) || !current2.UserIDString.Equals(id))
				{
					continue;
				}
				return current2;
			}
		}
		finally
		{
			((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
		}
		return null;
	}

	[HookMethod("IOnBaseCombatEntityHurt")]
	private object IOnBaseCombatEntityHurt(BaseCombatEntity entity, HitInfo hitInfo)
	{
		return (entity is BasePlayer) ? null : Interface.CallHook("OnEntityTakeDamage", entity, hitInfo);
	}

	[HookMethod("IOnNpcTarget")]
	private object IOnNpcTarget(BaseNpc npc, BaseEntity target)
	{
		if (Interface.CallHook("OnNpcTarget", npc, target) != null)
		{
			npc.SetFact(BaseNpc.Facts.HasEnemy, 0);
			npc.SetFact(BaseNpc.Facts.EnemyRange, 3);
			npc.SetFact(BaseNpc.Facts.AfraidRange, 1);
			npc.playerTargetDecisionStartTime = 0f;
			return 0f;
		}
		return null;
	}

	[HookMethod("IOnNpcTarget")]
	private object IOnNpcTarget(SenseComponent sense, BaseEntity target)
	{
		if (!Object.op_Implicit((Object)(object)sense) || !Object.op_Implicit((Object)(object)target))
		{
			return null;
		}
		BaseEntity baseEntity = sense.baseEntity;
		if (!Object.op_Implicit((Object)(object)baseEntity))
		{
			return null;
		}
		if (Interface.CallHook("OnNpcTarget", baseEntity, target) != null)
		{
			return false;
		}
		return null;
	}

	[HookMethod("IOnEntitySaved")]
	private void IOnEntitySaved(BaseNetworkable baseNetworkable, BaseNetworkable.SaveInfo saveInfo)
	{
		if (serverInitialized && saveInfo.forConnection != null)
		{
			Interface.CallHook("OnEntitySaved", baseNetworkable, saveInfo);
		}
	}

	[HookMethod("IOnLoseCondition")]
	private object IOnLoseCondition(Item item, float amount)
	{
		object[] array = new object[2] { item, amount };
		Interface.CallHook("OnLoseCondition", array);
		amount = (float)array[1];
		float condition = item.condition;
		item.condition -= amount;
		if (item.condition <= 0f && item.condition < condition)
		{
			item.OnBroken();
		}
		return true;
	}

	[HookMethod("ICanPickupEntity")]
	private object ICanPickupEntity(BasePlayer basePlayer, BaseEntity entity)
	{
		object obj = Interface.CallHook("CanPickupEntity", basePlayer, entity);
		return (obj is bool && !(bool)obj) ? ((object)true) : null;
	}

	[HookMethod("IOnBasePlayerAttacked")]
	private object IOnBasePlayerAttacked(BasePlayer basePlayer, HitInfo hitInfo)
	{
		if (!serverInitialized || (Object)(object)basePlayer == (Object)null || hitInfo == null || basePlayer.IsDead() || isPlayerTakingDamage || basePlayer is NPCPlayer)
		{
			return null;
		}
		if (Interface.CallHook("OnEntityTakeDamage", basePlayer, hitInfo) != null)
		{
			return true;
		}
		isPlayerTakingDamage = true;
		try
		{
			basePlayer.OnAttacked(hitInfo);
		}
		finally
		{
			isPlayerTakingDamage = false;
		}
		return true;
	}

	[HookMethod("IOnBasePlayerHurt")]
	private object IOnBasePlayerHurt(BasePlayer basePlayer, HitInfo hitInfo)
	{
		return isPlayerTakingDamage ? null : Interface.CallHook("OnEntityTakeDamage", basePlayer, hitInfo);
	}

	[HookMethod("OnServerUserSet")]
	private void OnServerUserSet(ulong steamId, ServerUsers.UserGroup group, string playerName, string reason, long expiry)
	{
		if (serverInitialized && group == ServerUsers.UserGroup.Banned)
		{
			string text = steamId.ToString();
			IPlayer player = Covalence.PlayerManager.FindPlayerById(text);
			Interface.CallHook("OnPlayerBanned", playerName, steamId, player?.Address ?? "0", reason, expiry);
			Interface.CallHook("OnUserBanned", playerName, text, player?.Address ?? "0", reason, expiry);
		}
	}

	[HookMethod("OnServerUserRemove")]
	private void OnServerUserRemove(ulong steamId)
	{
		if (serverInitialized && ServerUsers.users.ContainsKey(steamId) && ServerUsers.users[steamId].group == ServerUsers.UserGroup.Banned)
		{
			string text = steamId.ToString();
			IPlayer player = Covalence.PlayerManager.FindPlayerById(text);
			Interface.CallHook("OnPlayerUnbanned", player?.Name ?? "Unnamed", steamId, player?.Address ?? "0");
			Interface.CallHook("OnUserUnbanned", player?.Name ?? "Unnamed", text, player?.Address ?? "0");
		}
	}

	[HookMethod("IOnUserApprove")]
	private object IOnUserApprove(Connection connection)
	{
		string username = connection.username;
		string text = connection.userid.ToString();
		string obj = Regex.Replace(connection.ipaddress, ipPattern, "");
		uint authLevel = connection.authLevel;
		if (permission.IsLoaded)
		{
			permission.UpdateNickname(text, username);
			OxideConfig.DefaultGroups defaultGroups = Interface.Oxide.Config.Options.DefaultGroups;
			if (!permission.UserHasGroup(text, defaultGroups.Players))
			{
				permission.AddUserGroup(text, defaultGroups.Players);
			}
			if (authLevel >= 2 && !permission.UserHasGroup(text, defaultGroups.Administrators))
			{
				permission.AddUserGroup(text, defaultGroups.Administrators);
			}
		}
		Covalence.PlayerManager.PlayerJoin(connection.userid, username);
		object obj2 = Interface.CallHook("CanClientLogin", connection);
		object obj3 = Interface.CallHook("CanUserLogin", username, text, obj);
		object obj4 = ((obj2 == null) ? obj3 : obj2);
		if (obj4 is string || (obj4 is bool flag && !flag))
		{
			ConnectionAuth.Reject(connection, (obj4 is string) ? obj4.ToString() : lang.GetMessage("ConnectionRejected", this, text));
			return true;
		}
		object obj5 = Interface.CallHook("OnUserApprove", connection);
		object obj6 = Interface.CallHook("OnUserApproved", username, text, obj);
		return (obj5 == null) ? obj6 : obj5;
	}

	[HookMethod("IOnPlayerBanned")]
	private unsafe void IOnPlayerBanned(Connection connection, AuthResponse status)
	{
		Interface.CallHook("OnPlayerBanned", connection, ((object)(*(AuthResponse*)(&status))/*cast due to constrained. prefix*/).ToString());
	}

	[HookMethod("IOnPlayerChat")]
	private object IOnPlayerChat(ulong playerId, string playerName, string message, Chat.ChatChannel channel, BasePlayer basePlayer)
	{
		if (string.IsNullOrEmpty(message) || message.Equals("text"))
		{
			return true;
		}
		string chatCommandPrefix = CommandHandler.GetChatCommandPrefix(message);
		if (chatCommandPrefix != null)
		{
			TryRunPlayerCommand(basePlayer, message, chatCommandPrefix);
			return false;
		}
		message = StringEx.EscapeRichText(message, false);
		if ((Object)(object)basePlayer == (Object)null || !basePlayer.IsConnected)
		{
			return Interface.CallHook("OnPlayerOfflineChat", playerId, playerName, message, channel);
		}
		object obj = Interface.CallHook("OnPlayerChat", basePlayer, message, channel);
		object obj2 = Interface.CallHook("OnUserChat", basePlayer.IPlayer, message);
		return (obj == null) ? obj2 : obj;
	}

	private void TryRunPlayerCommand(BasePlayer basePlayer, string message, string commandPrefix)
	{
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		string text = message.Replace("\n", "").Replace("\r", "").Trim();
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		ParseCommand(text.Substring(commandPrefix.Length), out var command, out var args);
		if (command == null)
		{
			return;
		}
		if (!basePlayer.IsConnected)
		{
			Interface.CallHook("OnApplicationCommand", basePlayer, command, args);
			Interface.CallHook("OnApplicationCommand", basePlayer.IPlayer, command, args);
			return;
		}
		object obj = Interface.CallHook("OnPlayerCommand", basePlayer, command, args);
		object obj2 = Interface.CallHook("OnUserCommand", basePlayer.IPlayer, command, args);
		object obj3 = ((obj == null) ? obj2 : obj);
		if (obj3 != null)
		{
			return;
		}
		try
		{
			if (!Covalence.CommandSystem.HandleChatMessage(basePlayer.IPlayer, text) && !cmdlib.HandleChatCommand(basePlayer, command, args) && Interface.Oxide.Config.Options.Modded)
			{
				basePlayer.IPlayer.Reply(string.Format(lang.GetMessage("UnknownCommand", this, basePlayer.IPlayer.Id), command));
			}
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			string text2 = string.Empty;
			string empty = string.Empty;
			StringBuilder stringBuilder = new StringBuilder();
			while (ex2 != null)
			{
				string text3 = ex2.GetType().Name;
				text2 = (text3 + ": " + ex2.Message).TrimEnd(new char[2] { ' ', ':' });
				stringBuilder.AppendLine(ex2.StackTrace);
				if (ex2.InnerException != null)
				{
					stringBuilder.AppendLine("Rethrow as " + text3);
				}
				ex2 = ex2.InnerException;
			}
			StackTrace stackTrace = new StackTrace(ex, 0, fNeedFileInfo: true);
			for (int i = 0; i < stackTrace.FrameCount; i++)
			{
				StackFrame frame = stackTrace.GetFrame(i);
				MethodBase method = frame.GetMethod();
				if ((object)method != null && (object)method.DeclaringType != null && method.DeclaringType.Namespace == "Oxide.Plugins")
				{
					empty = method.DeclaringType.Name;
				}
			}
			Interface.Oxide.LogError(string.Format("Failed to run command '/{0}' on plugin '{1}'. ({2}){3}{4}", new object[5]
			{
				command,
				empty,
				text2.Replace(Environment.NewLine, " "),
				Environment.NewLine,
				stackTrace
			}));
		}
	}

	[HookMethod("OnClientAuth")]
	private void OnClientAuth(Connection connection)
	{
		connection.username = Regex.Replace(connection.username, "<[^>]*>", string.Empty);
	}

	[HookMethod("IOnPlayerConnected")]
	private void IOnPlayerConnected(BasePlayer basePlayer)
	{
		lang.SetLanguage(basePlayer.net.connection.info.GetString("global.language", "en"), basePlayer.UserIDString);
		basePlayer.SendEntitySnapshot(CommunityEntity.ServerInstance);
		Covalence.PlayerManager.PlayerConnected(basePlayer);
		IPlayer player = Covalence.PlayerManager.FindPlayerById(basePlayer.UserIDString);
		if (player != null)
		{
			basePlayer.IPlayer = player;
			Interface.CallHook("OnUserConnected", player);
		}
		Interface.Oxide.CallHook("OnPlayerConnected", basePlayer);
	}

	[HookMethod("OnPlayerDisconnected")]
	private void OnPlayerDisconnected(BasePlayer basePlayer, string reason)
	{
		IPlayer iPlayer = basePlayer.IPlayer;
		if (iPlayer != null)
		{
			Interface.CallHook("OnUserDisconnected", iPlayer, reason);
		}
		Covalence.PlayerManager.PlayerDisconnected(basePlayer);
	}

	[HookMethod("OnPlayerSetInfo")]
	private void OnPlayerSetInfo(Connection connection, string key, string val)
	{
		if (!(key == "global.language"))
		{
			return;
		}
		lang.SetLanguage(val, connection.userid.ToString());
		BasePlayer basePlayer = connection.player as BasePlayer;
		if ((Object)(object)basePlayer != (Object)null)
		{
			Interface.CallHook("OnPlayerLanguageChanged", basePlayer, val);
			if (basePlayer.IPlayer != null)
			{
				Interface.CallHook("OnPlayerLanguageChanged", basePlayer.IPlayer, val);
			}
		}
	}

	[HookMethod("OnPlayerKicked")]
	private void OnPlayerKicked(BasePlayer basePlayer, string reason)
	{
		IPlayer iPlayer = basePlayer.IPlayer;
		if (iPlayer != null)
		{
			Interface.CallHook("OnUserKicked", basePlayer.IPlayer, reason);
		}
	}

	[HookMethod("OnPlayerRespawn")]
	private object OnPlayerRespawn(BasePlayer basePlayer)
	{
		IPlayer iPlayer = basePlayer.IPlayer;
		return (iPlayer != null) ? Interface.CallHook("OnUserRespawn", iPlayer) : null;
	}

	[HookMethod("OnPlayerRespawned")]
	private void OnPlayerRespawned(BasePlayer basePlayer)
	{
		IPlayer iPlayer = basePlayer.IPlayer;
		if (iPlayer != null)
		{
			Interface.CallHook("OnUserRespawned", iPlayer);
		}
	}

	[HookMethod("IOnRconMessage")]
	private object IOnRconMessage(IPAddress ipAddress, string command)
	{
		if (ipAddress != null && !string.IsNullOrEmpty(command))
		{
			RemoteMessage message = RemoteMessage.GetMessage(command);
			if (string.IsNullOrEmpty(message?.Message))
			{
				return null;
			}
			if (Interface.CallHook("OnRconMessage", ipAddress, message) != null)
			{
				return true;
			}
			string[] array = CommandLine.Split(message.Message);
			if (array.Length >= 1)
			{
				string obj = array[0].ToLower();
				string[] obj2 = array.Skip(1).ToArray();
				if (Interface.CallHook("OnRconCommand", ipAddress, obj, obj2) != null)
				{
					return true;
				}
			}
		}
		return null;
	}

	[HookMethod("IOnRconInitialize")]
	private object IOnRconInitialize()
	{
		return Interface.Oxide.Config.Rcon.Enabled ? ((object)true) : null;
	}

	[HookMethod("IOnRunCommandLine")]
	private object IOnRunCommandLine()
	{
		foreach (KeyValuePair<string, string> @switch in CommandLine.GetSwitches())
		{
			string text = @switch.Value;
			if (text == "")
			{
				text = "1";
			}
			string strCommand = @switch.Key.Substring(1);
			ConsoleSystem.Option unrestricted = ConsoleSystem.Option.Unrestricted;
			unrestricted.PrintOutput = false;
			ConsoleSystem.Run(unrestricted, strCommand, text);
		}
		return false;
	}

	[HookMethod("IOnServerCommand")]
	private object IOnServerCommand(ConsoleSystem.Arg arg)
	{
		if (arg == null || (arg.Connection != null && (Object)(object)arg.Player() == (Object)null))
		{
			return true;
		}
		if (arg.cmd.FullName == "chat.say" || arg.cmd.FullName == "chat.teamsay" || arg.cmd.FullName == "chat.localsay")
		{
			return null;
		}
		object obj = Interface.CallHook("OnServerCommand", arg);
		object obj2 = Interface.CallHook("OnServerCommand", arg.cmd.FullName, RustCommandSystem.ExtractArgs(arg));
		object obj3 = ((obj == null) ? obj2 : obj);
		if (obj3 != null)
		{
			return true;
		}
		return null;
	}

	[HookMethod("OnServerInformationUpdated")]
	private void OnServerInformationUpdated()
	{
		SteamServer.GameTags += ",^o";
		if (Interface.Oxide.Config.Options.Modded)
		{
			SteamServer.GameTags += "^z";
		}
	}

	[HookMethod("IOnCupboardAuthorize")]
	private object IOnCupboardAuthorize(ulong userID, BasePlayer player, BuildingPrivlidge privlidge)
	{
		if (userID == (ulong)player.userID)
		{
			if (Interface.CallHook("OnCupboardAuthorize", privlidge, player) != null)
			{
				return true;
			}
		}
		else if (Interface.CallHook("OnCupboardAssign", privlidge, userID, player) != null)
		{
			return true;
		}
		return null;
	}
}
