using System;
using System.Collections.Generic;
using System.Text;
using Carbon.Base;
using Carbon.Extensions;
using Carbon.Pooling;
using Facepunch;
using Oxide.Core;
using Oxide.Core.Plugins;
using ProtoBuf;
using UnityEngine;

namespace Carbon.Modules;

public class AdminExtensionsModule : CarbonModule<AdminExtensionsConfig, EmptyModuleData>
{
	private readonly HashSet<ulong> _tpmUsers = new HashSet<ulong>();

	private const string NoReason = "No reason given";

	public override string Name => "AdminExtensions";

	public override VersionNumber Version => new VersionNumber(1, 0, 0);

	public override Type Type => typeof(AdminExtensionsModule);

	public override bool ForceModded => false;

	public override void OnServerInit(bool initial)
	{
		base.OnServerInit(initial);
		((CarbonModule<AdminExtensionsConfig, EmptyModuleData>)this).OnEnabled(true);
	}

	public override void OnEnabled(bool initialized)
	{
		base.OnEnabled(initialized);
		if (initialized)
		{
			base.Permissions.RegisterPermission(base.ConfigInstance.NameFilter.BypassPermission, (BaseHookable)(object)this);
			base.Permissions.RegisterPermission(base.ConfigInstance.Blind.Permission, (BaseHookable)(object)this);
			base.Permissions.RegisterPermission(base.ConfigInstance.Empower.Permission, (BaseHookable)(object)this);
			base.Permissions.RegisterPermission(base.ConfigInstance.PrivateMessage.Permission, (BaseHookable)(object)this);
			base.Permissions.RegisterPermission(base.ConfigInstance.Lock.Permission, (BaseHookable)(object)this);
			base.Permissions.RegisterPermission(base.ConfigInstance.TeleportMarker.Permission, (BaseHookable)(object)this);
			base.Permissions.RegisterPermission(base.ConfigInstance.Mute.Permission, (BaseHookable)(object)this);
			base.Permissions.RegisterPermission(base.ConfigInstance.MuteList.Permission, (BaseHookable)(object)this);
			base.Permissions.RegisterPermission(base.ConfigInstance.Ban.Permission, (BaseHookable)(object)this);
			base.Permissions.RegisterPermission(base.ConfigInstance.Unban.Permission, (BaseHookable)(object)this);
			base.Permissions.RegisterPermission(base.ConfigInstance.Kick.Permission, (BaseHookable)(object)this);
			base.Permissions.RegisterPermission(base.ConfigInstance.ToggleCadmin.Permission, (BaseHookable)(object)this);
			((Plugin)Community.Runtime.Core).cmd.AddChatCommand(base.ConfigInstance.Blind.Command, (BaseHookable)(object)this, "CmdBlind", (string)null, (object)null, (string[])null, (string[])null, -1, 0, false, false, false, false);
			((Plugin)Community.Runtime.Core).cmd.AddChatCommand(base.ConfigInstance.Empower.Command, (BaseHookable)(object)this, "CmdEmpower", (string)null, (object)null, (string[])null, (string[])null, -1, 0, false, false, false, false);
			((Plugin)Community.Runtime.Core).cmd.AddChatCommand(base.ConfigInstance.PrivateMessage.Command, (BaseHookable)(object)this, "CmdPrivateMessage", (string)null, (object)null, (string[])null, (string[])null, -1, 0, false, false, false, false);
			((Plugin)Community.Runtime.Core).cmd.AddChatCommand(base.ConfigInstance.Lock.Command, (BaseHookable)(object)this, "CmdLockPlayerInventory", (string)null, (object)null, (string[])null, (string[])null, -1, 0, false, false, false, false);
			((Plugin)Community.Runtime.Core).cmd.AddChatCommand(base.ConfigInstance.TeleportMarker.Command, (BaseHookable)(object)this, "CmdTeleportMarker", (string)null, (object)null, (string[])null, (string[])null, -1, 0, false, false, false, false);
			((Plugin)Community.Runtime.Core).cmd.AddChatCommand(base.ConfigInstance.Mute.Command, (BaseHookable)(object)this, "CmdMute", (string)null, (object)null, (string[])null, (string[])null, -1, 0, false, false, false, false);
			((Plugin)Community.Runtime.Core).cmd.AddChatCommand(base.ConfigInstance.MuteList.Command, (BaseHookable)(object)this, "CmdMuteList", (string)null, (object)null, (string[])null, (string[])null, -1, 0, false, false, false, false);
			((Plugin)Community.Runtime.Core).cmd.AddChatCommand(base.ConfigInstance.Ban.Command, (BaseHookable)(object)this, "CmdBan", (string)null, (object)null, (string[])null, (string[])null, -1, 0, false, false, false, false);
			((Plugin)Community.Runtime.Core).cmd.AddChatCommand(base.ConfigInstance.Unban.Command, (BaseHookable)(object)this, "CmdUnban", (string)null, (object)null, (string[])null, (string[])null, -1, 0, false, false, false, false);
			((Plugin)Community.Runtime.Core).cmd.AddChatCommand(base.ConfigInstance.Kick.Command, (BaseHookable)(object)this, "CmdKick", (string)null, (object)null, (string[])null, (string[])null, -1, 0, false, false, false, false);
			((Plugin)Community.Runtime.Core).cmd.AddChatCommand(base.ConfigInstance.ToggleCadmin.Command, (BaseHookable)(object)this, "CmdToggleCadmin", (string)null, (object)null, (string[])null, (string[])null, -1, 0, false, false, false, false);
		}
	}

	public override void OnDisabled(bool initialized)
	{
		base.OnDisabled(initialized);
		_tpmUsers.Clear();
	}

	[Conditional("!MINIMAL")]
	private void OnPlayerConnected(BasePlayer player)
	{
		if (!base.Permissions.UserHasPermission(player.UserIDString, base.ConfigInstance.NameFilter.BypassPermission) && !base.ConfigInstance.NameFilter.IsValid(player.displayName) && base.ConfigInstance.NameFilter.TryRename(player.displayName, out var correctedName))
		{
			AdminExtensionsConfig.NameFilterSettings.FilterModes mode = base.ConfigInstance.NameFilter.Mode;
			if (mode == AdminExtensionsConfig.NameFilterSettings.FilterModes.Rename)
			{
				string displayName = player.displayName;
				CovalenceEx.AsIPlayer(player).Rename(correctedName);
				base.Puts((object)("Updated " + displayName + "[" + player.UserIDString + "]'s name to " + correctedName));
			}
		}
	}

	[Conditional("!MINIMAL")]
	private object CanUserLogin(string username, string userid)
	{
		if (base.Permissions.UserHasPermission(userid, base.ConfigInstance.NameFilter.BypassPermission))
		{
			return null;
		}
		AdminExtensionsConfig.NameFilterSettings.FilterModes mode = base.ConfigInstance.NameFilter.Mode;
		bool flag = !base.ConfigInstance.NameFilter.IsValid(username);
		if (mode == AdminExtensionsConfig.NameFilterSettings.FilterModes.Kick)
		{
			return base.ConfigInstance.NameFilter.KickMessage;
		}
		return null;
	}

	[Conditional("!MINIMAL")]
	private void OnMapMarkerAdded(BasePlayer player, MapNote note)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (_tpmUsers.Contains(EncryptedValue<ulong>.op_Implicit(player.userID)))
		{
			Vector3 val = note.worldPosition + Vector3.up * (TerrainMeta.HeightMap.GetHeight(note.worldPosition) + 1f);
			note.Dispose();
			player.State.pointsOfInterest.Remove(note);
			player.DirtyPlayerState();
			player.SendMarkersToClient();
			player.Teleport(val);
			((BaseNetworkable)player).UpdateNetworkGroup();
			player.SendCompleteSnapshot();
		}
	}

	[Conditional("!MINIMAL")]
	private void CmdBlind(BasePlayer player, string _, string[] args)
	{
		if (base.Permissions.UserHasPermission(player.UserIDString, base.ConfigInstance.Blind.Permission))
		{
			BasePlayer val = BasePlayer.Find(args[0]);
			if ((Object)(object)val == (Object)null)
			{
				player.ChatMessage("Player '" + args[0] + "' not found.");
			}
			else if (PlayersTab.BlindedPlayers.Contains(val))
			{
				AdminModule.UnblindPlayer(player, val);
				player.ChatMessage("Unblinded " + val.displayName + ".");
			}
			else
			{
				AdminModule.BlindPlayer(player, val);
				player.ChatMessage("Blinded " + val.displayName + ".");
			}
		}
	}

	[Conditional("!MINIMAL")]
	private void CmdEmpower(BasePlayer player, string _, string[] args)
	{
		if (base.Permissions.UserHasPermission(player.UserIDString, base.ConfigInstance.Empower.Permission))
		{
			BasePlayer val = BasePlayer.Find(args[0]);
			if ((Object)(object)val == (Object)null)
			{
				player.ChatMessage("Player '" + args[0] + "' not found.");
				return;
			}
			AdminModule.EmpowerPlayerStats(player, val);
			player.ChatMessage("Empowered " + val.displayName + ".");
		}
	}

	[Conditional("!MINIMAL")]
	private void CmdPrivateMessage(BasePlayer player, string cmd, string[] args)
	{
		if (!base.Permissions.UserHasPermission(player.UserIDString, base.ConfigInstance.PrivateMessage.Permission))
		{
			return;
		}
		if (args == null || args.Length < 2)
		{
			player.ChatMessage("Usage: /" + cmd + " <player> <message>");
			return;
		}
		BasePlayer val = BasePlayer.Find(args[0]);
		if ((Object)(object)val == (Object)null)
		{
			player.ChatMessage("Player '" + args[0] + "' not found.");
			return;
		}
		string text = string.Join(" ", args, 1, args.Length - 1);
		AdminModule.PrivateMessagePlayer(player, val, text);
	}

	[Conditional("!MINIMAL")]
	private void CmdLockPlayerInventory(BasePlayer player, string cmd, string[] args)
	{
		if (!base.Permissions.UserHasPermission(player.UserIDString, base.ConfigInstance.Lock.Permission))
		{
			return;
		}
		if (args == null || args.Length < 2)
		{
			player.ChatMessage("Usage: /" + cmd + " <player> <main|wear|belt|all> [toggle]");
			return;
		}
		BasePlayer val = BasePlayer.Find(args[0]);
		if ((Object)(object)val == (Object)null)
		{
			player.ChatMessage("Player '" + args[0] + "' not found.");
			return;
		}
		bool flag = args.Length <= 2;
		bool flag2 = args.Length > 2 && StringEx.ToBool(args[2], false);
		switch (args[1])
		{
		case "main":
			AdminModule.LockPlayerContainer(player, val, val.inventory.containerMain, flag ? (!val.inventory.containerMain.IsLocked()) : flag2);
			break;
		case "wear":
			AdminModule.LockPlayerContainer(player, val, val.inventory.containerWear, flag ? (!val.inventory.containerWear.IsLocked()) : flag2);
			break;
		case "belt":
			AdminModule.LockPlayerContainer(player, val, val.inventory.containerBelt, flag ? (!val.inventory.containerBelt.IsLocked()) : flag2);
			break;
		case "all":
			AdminModule.LockPlayerContainer(player, val, val.inventory.containerBelt, flag ? (!val.inventory.containerBelt.IsLocked()) : flag2);
			AdminModule.LockPlayerContainer(player, val, val.inventory.containerWear, flag ? (!val.inventory.containerWear.IsLocked()) : flag2);
			AdminModule.LockPlayerContainer(player, val, val.inventory.containerMain, flag ? (!val.inventory.containerMain.IsLocked()) : flag2);
			break;
		default:
			player.ChatMessage("Container '" + args[0] + "' not found.");
			break;
		}
	}

	[Conditional("!MINIMAL")]
	private void CmdTeleportMarker(BasePlayer player, string _, string[] args)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (base.Permissions.UserHasPermission(player.UserIDString, base.ConfigInstance.TeleportMarker.Permission))
		{
			if (_tpmUsers.Contains(EncryptedValue<ulong>.op_Implicit(player.userID)))
			{
				player.ChatMessage("Teleport Marker disabled.");
				_tpmUsers.Remove(EncryptedValue<ulong>.op_Implicit(player.userID));
			}
			else
			{
				player.ChatMessage("Teleport Marker enabled.");
				_tpmUsers.Add(EncryptedValue<ulong>.op_Implicit(player.userID));
			}
		}
	}

	[Conditional("!MINIMAL")]
	private void CmdMute(BasePlayer player, string cmd, string[] args)
	{
		if (!base.Permissions.UserHasPermission(player.UserIDString, base.ConfigInstance.Mute.Permission))
		{
			return;
		}
		if (args == null || args.Length == 0)
		{
			player.ChatMessage("Usage: /" + cmd + " <player> [reason]");
			return;
		}
		BasePlayer val = BasePlayer.Find(args[0]);
		if ((Object)(object)val == (Object)null)
		{
			player.ChatMessage("Player '" + args[0] + "' not found.");
			return;
		}
		string text = string.Join(" ", args, 1, args.Length - 1);
		if (string.IsNullOrEmpty(text))
		{
			text = "No reason given";
		}
		AdminModule.MutePlayer(player, val, !val.State.chatMuted, text);
		player.ChatMessage("You have " + (val.State.chatMuted ? "muted" : "unmuted") + " " + val.displayName + ". Reason: " + text);
	}

	[Conditional("!MINIMAL")]
	private void CmdMuteList(BasePlayer player)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Permissions.UserHasPermission(player.UserIDString, base.ConfigInstance.MuteList.Permission))
		{
			return;
		}
		StringBuilder stringBuilder = Pool.Get<StringBuilder>();
		stringBuilder.Clear();
		stringBuilder.AppendLine("Muted Players:");
		int num = 0;
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (current.State.chatMuted)
				{
					stringBuilder.AppendLine(current.displayName);
					num++;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		stringBuilder.AppendLine((num == 0) ? "No players are currently muted." : $"Total muted players: {num}");
		player.ChatMessage(stringBuilder.ToString());
		Pool.FreeUnmanaged(ref stringBuilder);
	}

	[Conditional("!MINIMAL")]
	private void CmdBan(BasePlayer player, string cmd, string[] args)
	{
		if (!base.Permissions.UserHasPermission(player.UserIDString, base.ConfigInstance.Ban.Permission))
		{
			return;
		}
		if (args == null || args.Length == 0)
		{
			player.ChatMessage("Usage: /" + cmd + " <player> [reason]");
			return;
		}
		BasePlayer val = BasePlayer.FindAwakeOrSleeping(args[0]);
		if ((Object)(object)val == (Object)null)
		{
			player.ChatMessage("Player '" + args[0] + "' not found.");
			return;
		}
		string text = string.Join(" ", args, 1, args.Length - 1);
		if (string.IsNullOrEmpty(text))
		{
			text = "No reason given";
		}
		AdminModule.BanPlayer(player, val, text, "");
		player.ChatMessage("You have banned " + val.displayName + ". Reason: " + text);
	}

	[Conditional("!MINIMAL")]
	private void CmdUnban(BasePlayer player, string cmd, string[] args)
	{
		if (!base.Permissions.UserHasPermission(player.UserIDString, base.ConfigInstance.Unban.Permission))
		{
			return;
		}
		if (args == null || args.Length == 0)
		{
			player.ChatMessage("Usage: /" + cmd + " <player>");
			return;
		}
		BasePlayer val = BasePlayer.FindAwakeOrSleeping(args[0]);
		if ((Object)(object)val == (Object)null)
		{
			player.ChatMessage("Player '" + args[0] + "' not found.");
			return;
		}
		AdminModule.UnbanPlayer(player, val);
		player.ChatMessage("You have unbanned " + val.displayName + ".");
	}

	[Conditional("!MINIMAL")]
	private void CmdKick(BasePlayer player, string cmd, string[] args)
	{
		if (!base.Permissions.UserHasPermission(player.UserIDString, base.ConfigInstance.Kick.Permission))
		{
			return;
		}
		if (args == null || args.Length == 0)
		{
			player.ChatMessage("Usage: /" + cmd + " <player> [reason]");
			return;
		}
		BasePlayer val = BasePlayer.Find(args[0]);
		if ((Object)(object)val == (Object)null)
		{
			player.ChatMessage("Player '" + args[0] + "' not found.");
			return;
		}
		string text = string.Join(" ", args, 1, args.Length - 1);
		if (string.IsNullOrEmpty(text))
		{
			text = "No reason given";
		}
		AdminModule.KickPlayer(player, val, text);
		player.ChatMessage("You have kicked " + val.displayName + ". Reason: " + text);
	}

	[Conditional("!MINIMAL")]
	private void CmdToggleCadmin(BasePlayer player, string _, string[] args)
	{
		if (base.Permissions.UserHasPermission(player.UserIDString, base.ConfigInstance.ToggleCadmin.Permission))
		{
			bool flag = player.HasPlayerFlag((PlayerFlags)128);
			player.SetPlayerFlag((PlayerFlags)128, !flag);
			player.ChatMessage("You've " + ((!flag) ? "enabled" : "disabled") + " <color=orange>cadmin</color> mode.");
		}
	}

	public override object InternalCallHook(uint hook, object[] args)
	{
		//IL_0934: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_051d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0601: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0889: Unknown result type (might be due to invalid IL or missing references)
		//IL_0745: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b4: Unknown result type (might be due to invalid IL or missing references)
		int? num = args?.Length;
		object obj = ((num > 0) ? args[0] : null);
		object obj2 = ((num > 1) ? args[1] : null);
		object obj3 = ((num > 2) ? args[2] : null);
		try
		{
			switch (hook)
			{
			case 1045800646u:
			{
				bool flag = ((obj is string || obj == null) ? true : false);
				bool flag27 = flag;
				string username = (flag27 ? ((string)(obj ?? null)) : null);
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag28 = flag;
				string userid = (flag28 ? ((string)(obj2 ?? null)) : null);
				if (flag27 && flag28)
				{
					return CanUserLogin(username, userid);
				}
				break;
			}
			case 3858352595u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag24 = flag;
				BasePlayer player9 = ((!flag24) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag25 = flag;
				string cmd4 = (flag25 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is string[] || obj3 == null) ? true : false);
				bool flag26 = flag;
				string[] args9 = (flag26 ? ((string[])(obj3 ?? null)) : null);
				if (flag24 && flag25 && flag26)
				{
					CmdBan(player9, cmd4, args9);
					return null;
				}
				break;
			}
			case 3316594012u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag12 = flag;
				BasePlayer player5 = ((!flag12) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag13 = flag;
				string _2 = (flag13 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is string[] || obj3 == null) ? true : false);
				bool flag14 = flag;
				string[] args5 = (flag14 ? ((string[])(obj3 ?? null)) : null);
				if (flag12 && flag13 && flag14)
				{
					CmdBlind(player5, _2, args5);
					return null;
				}
				break;
			}
			case 268148139u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag6 = flag;
				BasePlayer player3 = ((!flag6) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag7 = flag;
				string _ = (flag7 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is string[] || obj3 == null) ? true : false);
				bool flag8 = flag;
				string[] args3 = (flag8 ? ((string[])(obj3 ?? null)) : null);
				if (flag6 && flag7 && flag8)
				{
					CmdEmpower(player3, _, args3);
					return null;
				}
				break;
			}
			case 3356795315u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag29 = flag;
				BasePlayer player10 = ((!flag29) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag30 = flag;
				string cmd5 = (flag30 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is string[] || obj3 == null) ? true : false);
				bool flag31 = flag;
				string[] args10 = (flag31 ? ((string[])(obj3 ?? null)) : null);
				if (flag29 && flag30 && flag31)
				{
					CmdKick(player10, cmd5, args10);
					return null;
				}
				break;
			}
			case 3318094191u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag9 = flag;
				BasePlayer player4 = ((!flag9) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag10 = flag;
				string cmd2 = (flag10 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is string[] || obj3 == null) ? true : false);
				bool flag11 = flag;
				string[] args4 = (flag11 ? ((string[])(obj3 ?? null)) : null);
				if (flag9 && flag10 && flag11)
				{
					CmdLockPlayerInventory(player4, cmd2, args4);
					return null;
				}
				break;
			}
			case 3317529770u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag3 = flag;
				BasePlayer player2 = ((!flag3) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag4 = flag;
				string cmd = (flag4 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is string[] || obj3 == null) ? true : false);
				bool flag5 = flag;
				string[] args2 = (flag5 ? ((string[])(obj3 ?? null)) : null);
				if (flag3 && flag4 && flag5)
				{
					CmdMute(player2, cmd, args2);
					return null;
				}
				break;
			}
			case 4200947581u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag34 = flag;
				BasePlayer player12 = ((!flag34) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				if (flag34)
				{
					CmdMuteList(player12);
					return null;
				}
				break;
			}
			case 2256174198u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag35 = flag;
				BasePlayer player13 = ((!flag35) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag36 = flag;
				string cmd6 = (flag36 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is string[] || obj3 == null) ? true : false);
				bool flag37 = flag;
				string[] args11 = (flag37 ? ((string[])(obj3 ?? null)) : null);
				if (flag35 && flag36 && flag37)
				{
					CmdPrivateMessage(player13, cmd6, args11);
					return null;
				}
				break;
			}
			case 3797902375u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag21 = flag;
				BasePlayer player8 = ((!flag21) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag22 = flag;
				string _4 = (flag22 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is string[] || obj3 == null) ? true : false);
				bool flag23 = flag;
				string[] args8 = (flag23 ? ((string[])(obj3 ?? null)) : null);
				if (flag21 && flag22 && flag23)
				{
					CmdTeleportMarker(player8, _4, args8);
					return null;
				}
				break;
			}
			case 1004786013u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag18 = flag;
				BasePlayer player7 = ((!flag18) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag19 = flag;
				string _3 = (flag19 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is string[] || obj3 == null) ? true : false);
				bool flag20 = flag;
				string[] args7 = (flag20 ? ((string[])(obj3 ?? null)) : null);
				if (flag18 && flag19 && flag20)
				{
					CmdToggleCadmin(player7, _3, args7);
					return null;
				}
				break;
			}
			case 109812738u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag15 = flag;
				BasePlayer player6 = ((!flag15) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag16 = flag;
				string cmd3 = (flag16 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is string[] || obj3 == null) ? true : false);
				bool flag17 = flag;
				string[] args6 = (flag17 ? ((string[])(obj3 ?? null)) : null);
				if (flag15 && flag16 && flag17)
				{
					CmdUnban(player6, cmd3, args6);
					return null;
				}
				break;
			}
			case 1405948638u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag32 = flag;
				BasePlayer player11 = ((!flag32) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is MapNote || obj2 == null) ? true : false);
				bool flag33 = flag;
				MapNote note = ((!flag33) ? ((MapNote)null) : ((MapNote)(obj2 ?? null)));
				if (flag32 && flag33)
				{
					OnMapMarkerAdded(player11, note);
					return null;
				}
				break;
			}
			case 2848347654u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag2 = flag;
				BasePlayer player = ((!flag2) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				if (flag2)
				{
					OnPlayerConnected(player);
					return null;
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			Logger.Error((object)string.Format("Failed to call internal hook '{0}' on module '{1} v{2}' [{3}]", new object[4]
			{
				HookStringPool.GetOrAdd(hook),
				((CarbonModule<AdminExtensionsConfig, EmptyModuleData>)this).Name,
				((BaseHookable)this).Version,
				hook
			}), ex);
			((BaseHookable)this).OnException(hook);
		}
		return null;
	}
}
