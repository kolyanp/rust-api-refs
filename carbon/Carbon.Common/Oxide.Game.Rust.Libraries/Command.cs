using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using API.Commands;
using Carbon;
using Carbon.Base;
using Carbon.Components;
using Carbon.Extensions;
using Carbon.Plugins;
using Facepunch;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Oxide.Game.Rust.Libraries.Covalence;
using UnityEngine;

namespace Oxide.Game.Rust.Libraries;

public class Command : Library
{
	public static bool FromRcon { get; set; }

	private Func<API.Commands.Command, API.Commands.Command.Args, bool> OnPlayerExecute(bool isChat)
	{
		return delegate(API.Commands.Command cmd, API.Commands.Command.Args args)
		{
			if (args is PlayerArgs playerArgs && playerArgs != null)
			{
				object player = playerArgs.Player;
				BasePlayer val = (BasePlayer)((player is BasePlayer) ? player : null);
				AuthenticatedCommand authenticatedCommand = cmd as AuthenticatedCommand;
				if ((Object)(object)val != (Object)null && authenticatedCommand != null)
				{
					if (authenticatedCommand.Auth.Permissions != null)
					{
						bool flag = authenticatedCommand.Auth.Permissions.Count((string x) => !string.IsNullOrEmpty(x)) == 0;
						string[] permissions = authenticatedCommand.Auth.Permissions;
						foreach (string perm in permissions)
						{
							if (Community.Runtime.Core.permission.UserHasPermission(val.UserIDString, perm))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							if (isChat)
							{
								val.ChatMessage(Localisation.Get("no_perm", val.UserIDString));
							}
							else
							{
								val.ConsoleMessage(Localisation.Get("no_perm", val.UserIDString));
							}
							return false;
						}
					}
					if (authenticatedCommand.Auth.Groups != null)
					{
						bool flag2 = authenticatedCommand.Auth.Groups.Count((string x) => !string.IsNullOrEmpty(x)) == 0;
						string[] groups = authenticatedCommand.Auth.Groups;
						foreach (string name in groups)
						{
							if (Community.Runtime.Core.permission.UserHasGroup(val.UserIDString, name))
							{
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							if (isChat)
							{
								val.ChatMessage(Localisation.Get("no_group", val.UserIDString));
							}
							else
							{
								val.ConsoleMessage(Localisation.Get("no_group", val.UserIDString));
							}
							return false;
						}
					}
					if (authenticatedCommand.Auth.AuthLevel != -1 && val.Connection.authLevel < authenticatedCommand.Auth.AuthLevel)
					{
						if (isChat)
						{
							val.ChatMessage(Localisation.Get("no_auth", val.UserIDString, authenticatedCommand.Auth.AuthLevel, val.Connection.authLevel));
						}
						else
						{
							val.ConsoleMessage(Localisation.Get("no_auth", val.UserIDString, authenticatedCommand.Auth.AuthLevel, val.Connection.authLevel));
						}
						return false;
					}
					if ((!Community.Runtime.Config.Permissions.BypassAdminCooldowns || val.Connection.authLevel <= 1) && CarbonPlugin.IsCommandCooledDown(val, cmd.Name, authenticatedCommand.Auth.Cooldown, out var timeLeft, doCooldownIfNot: true, 0.5f, authenticatedCommand.Auth.DoCooldownPenalty))
					{
						if (timeLeft < 2f)
						{
							return false;
						}
						if (isChat)
						{
							val.ChatMessage(Localisation.Get("cooldown_player", val.UserIDString, TimeEx.Format(timeLeft).ToLower()));
						}
						else
						{
							val.ConsoleMessage(Localisation.Get("cooldown_player", val.UserIDString, TimeEx.Format(timeLeft).ToLower()));
						}
						return false;
					}
				}
			}
			return true;
		};
	}

	public void AddChatCommand(string command, BaseHookable plugin, Action<BasePlayer, string, string[]> callback, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1, int cooldown = 0, bool isHidden = false, bool @protected = false, bool silent = false, bool doCooldownPenalty = false)
	{
		API.Commands.Command.Chat chat = new API.Commands.Command.Chat
		{
			Name = command,
			Reference = plugin,
			Callback = delegate(API.Commands.Command.Args arg)
			{
				if (arg is PlayerArgs playerArgs)
				{
					try
					{
						Action<BasePlayer, string, string[]> action = callback;
						if (action != null)
						{
							object player = playerArgs.Player;
							action((BasePlayer)((player is BasePlayer) ? player : null), command, arg.Arguments.ToStringArray());
						}
					}
					catch (Exception ex)
					{
						Logger.Error("Failed executing chat command '" + command + "' in '" + plugin.ToPrettyString() + "' [callback]", ex.InnerException ?? ex);
					}
				}
			},
			Help = help,
			Token = reference,
			Auth = new API.Commands.Command.Authentication
			{
				AuthLevel = authLevel,
				Permissions = permissions,
				Groups = groups,
				Cooldown = cooldown,
				DoCooldownPenalty = doCooldownPenalty
			},
			CanExecute = OnPlayerExecute(isChat: true)
		};
		chat.SetFlag(CommandFlags.Hidden, isHidden);
		chat.SetFlag(CommandFlags.Protected, @protected);
		if (!Community.Runtime.CommandManager.RegisterCommand(chat, out var reason) && !silent)
		{
			Logger.Warn(reason);
		}
	}

	public void AddChatCommand(string command, BaseHookable plugin, string method, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1, int cooldown = 0, bool isHidden = false, bool @protected = false, bool silent = false, bool doCooldownPenalty = false)
	{
		MethodInfo[] methods = plugin.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		MethodInfo methodInfo = methods.FirstOrDefault((MethodInfo x) => x.Name == method && (!x.GetParameters().Any() || x.GetParameters().Any((ParameterInfo y) => y.ParameterType == typeof(IPlayer))));
		MethodInfo methodInfo2 = methods.FirstOrDefault((MethodInfo x) => x.Name == method && (!x.GetParameters().Any() || x.GetParameters().Any((ParameterInfo y) => y.ParameterType != typeof(IPlayer))));
		MethodInfo methodInfo3 = methodInfo ?? methodInfo2;
		AddChatCommand(command, plugin, methodInfo3, help, reference, permissions, groups, authLevel, cooldown, isHidden, @protected, silent, doCooldownPenalty);
	}

	public void AddChatCommand(string command, BaseHookable plugin, MethodInfo methodInfo, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1, int cooldown = 0, bool isHidden = false, bool @protected = false, bool silent = false, bool doCooldownPenalty = false)
	{
		AddChatCommand(command, plugin, delegate(BasePlayer player, string cmd, string[] args)
		{
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			List<object> list = Pool.Get<List<object>>();
			object[] array = null;
			Arg val = null;
			try
			{
				ParameterInfo[] parameters = methodInfo.GetParameters();
				bool flag = parameters.Length != 0 && parameters.Any((ParameterInfo y) => y.ParameterType == typeof(IPlayer));
				if (parameters.Length != 0)
				{
					if (flag)
					{
						RustPlayer rustPlayer = player.AsIPlayer();
						rustPlayer.IsServer = (Object)(object)player == (Object)null;
						list.Add(rustPlayer);
					}
					else if (parameters[0].ParameterType == typeof(Arg))
					{
						string text = ((args == null || args.Length == 0) ? string.Empty : string.Join(" ", args));
						Option option = (((Object)(object)player == (Object)null) ? Option.Unrestricted : Option.Client);
						object uninitializedObject = FormatterServices.GetUninitializedObject(typeof(Arg));
						val = (Arg)((uninitializedObject is Arg) ? uninitializedObject : null);
						if ((Object)(object)player != (Object)null)
						{
							option = ((Option)(ref option)).FromConnection(((BaseNetworkable)player).net.connection);
						}
						((Option)(ref option)).FromRcon = FromRcon;
						val.Option = option;
						val.FullString = StringView.op_Implicit(text);
						val.Args = args.Select(delegate(string x)
						{
							//IL_0001: Unknown result type (might be due to invalid IL or missing references)
							return StringView.op_Implicit(x);
						}).ToArray();
						list.Add(val);
					}
					else
					{
						list.Add(player);
					}
					switch (parameters.Length)
					{
					case 2:
						list.Add(cmd);
						break;
					case 3:
						list.Add(cmd);
						list.Add(args);
						break;
					}
					int count = list.Count;
					if (parameters.Length > count)
					{
						for (int num = 0; num < parameters.Length - count; num++)
						{
							list.Add(null);
						}
					}
					array = list.ToArray();
				}
				methodInfo?.Invoke(plugin, array);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed executing chat command '" + command + "' in '" + plugin.ToPrettyString() + "' [callback]", ex.InnerException ?? ex);
			}
			if (list != null)
			{
				Pool.FreeUnmanaged<object>(ref list);
			}
			if (array != null)
			{
				Array.Clear(array, 0, array.Length);
			}
			val = null;
		}, help, reference, permissions, groups, authLevel, cooldown, isHidden, @protected, silent, doCooldownPenalty);
	}

	public void AddConsoleCommand(string command, BaseHookable plugin, Action<BasePlayer, string, string[]> callback, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1, int cooldown = 0, bool isHidden = false, bool @protected = false, bool silent = false, bool doCooldownPenalty = false)
	{
		API.Commands.Command.ClientConsole clientConsole = new API.Commands.Command.ClientConsole
		{
			Name = command,
			Reference = plugin,
			Callback = delegate(API.Commands.Command.Args arg)
			{
				if (arg is PlayerArgs playerArgs)
				{
					Action<BasePlayer, string, string[]> action = callback;
					if (action != null)
					{
						object player = playerArgs.Player;
						action((BasePlayer)((player is BasePlayer) ? player : null), command, arg.Arguments.ToStringArray());
					}
				}
			},
			Help = help,
			Token = reference,
			Auth = new API.Commands.Command.Authentication
			{
				AuthLevel = authLevel,
				Permissions = permissions,
				Groups = groups,
				Cooldown = cooldown,
				DoCooldownPenalty = doCooldownPenalty
			},
			CanExecute = OnPlayerExecute(isChat: false)
		};
		clientConsole.SetFlag(CommandFlags.Hidden, isHidden);
		clientConsole.SetFlag(CommandFlags.Protected, @protected);
		if (!Community.Runtime.CommandManager.RegisterCommand(clientConsole, out var reason) && !silent)
		{
			Logger.Warn(reason);
		}
		API.Commands.Command.RCon rCon = new API.Commands.Command.RCon
		{
			Name = command,
			Reference = plugin,
			Callback = delegate(API.Commands.Command.Args arg)
			{
				callback?.Invoke(null, command, arg.Arguments.ToStringArray());
			},
			Help = help,
			Token = reference,
			CanExecute = OnPlayerExecute(isChat: false)
		};
		rCon.SetFlag(CommandFlags.Hidden, isHidden);
		rCon.SetFlag(CommandFlags.Protected, @protected);
		if (!Community.Runtime.CommandManager.RegisterCommand(rCon, out var reason2) && !silent)
		{
			Logger.Warn(reason2);
		}
	}

	public void AddConsoleCommand(string command, BaseHookable plugin, string method, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1, int cooldown = 0, bool isHidden = false, bool @protected = false, bool silent = false, bool doCooldownPenalty = false)
	{
		MethodInfo[] methods = plugin.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		MethodInfo methodInfo = methods.FirstOrDefault((MethodInfo x) => x.Name == method && (!x.GetParameters().Any() || x.GetParameters().Any((ParameterInfo y) => y.ParameterType == typeof(IPlayer))));
		MethodInfo methodInfo2 = methods.FirstOrDefault((MethodInfo x) => x.Name == method && (!x.GetParameters().Any() || x.GetParameters().Any((ParameterInfo y) => y.ParameterType != typeof(IPlayer))));
		MethodInfo methodInfo3 = methodInfo ?? methodInfo2;
		AddConsoleCommand(command, plugin, methodInfo3, help, reference, permissions, groups, authLevel, cooldown, isHidden, @protected, silent, doCooldownPenalty);
	}

	public void AddConsoleCommand(string command, BaseHookable plugin, MethodInfo methodInfo, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1, int cooldown = 0, bool isHidden = false, bool @protected = false, bool silent = false, bool doCooldownPenalty = false)
	{
		AddConsoleCommand(command, plugin, delegate(BasePlayer player, string cmd, string[] args)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			List<object> list = Pool.Get<List<object>>();
			object[] array = null;
			try
			{
				string text = ((args == null || args.Length == 0) ? string.Empty : string.Join(" ", args));
				Option option = (((Object)(object)player == (Object)null) ? Option.Server : Option.Client);
				object uninitializedObject = FormatterServices.GetUninitializedObject(typeof(Arg));
				Arg val = (Arg)((uninitializedObject is Arg) ? uninitializedObject : null);
				if ((Object)(object)player != (Object)null)
				{
					option = ((Option)(ref option)).FromConnection(((BaseNetworkable)player).net.connection);
				}
				val.Option = option;
				val.FullString = StringView.op_Implicit(text);
				val.Args = args.ToStringViewArray();
				val.cmd = Community.Runtime.CommandManager.Find(command)?.RustCommand;
				try
				{
					ParameterInfo[] parameters = methodInfo.GetParameters();
					bool flag = parameters.Length != 0 && parameters.Any((ParameterInfo y) => y.ParameterType == typeof(IPlayer));
					if (parameters.Length != 0)
					{
						if (flag)
						{
							if ((Object)(object)player == (Object)null)
							{
								list.Add(new RustPlayer
								{
									IsServer = true
								});
							}
							else
							{
								RustPlayer rustPlayer = player.AsIPlayer();
								rustPlayer.IsServer = (Object)(object)player == (Object)null;
								list.Add(rustPlayer);
							}
							switch (parameters.Length)
							{
							case 2:
								list.Add(cmd);
								break;
							case 3:
								list.Add(cmd);
								list.Add(args);
								break;
							}
						}
						else
						{
							Type parameterType = parameters[0].ParameterType;
							list.Add((parameterType == typeof(BasePlayer)) ? ((object)player) : ((object)val));
							if (parameterType == typeof(BasePlayer))
							{
								switch (parameters.Length)
								{
								case 2:
									list.Add(cmd);
									break;
								case 3:
									list.Add(cmd);
									list.Add(args);
									break;
								}
							}
							else
							{
								for (int num = 1; num < parameters.Length; num++)
								{
									list.Add(null);
								}
							}
						}
					}
					array = list.ToArray();
					if (HookCaller.CallStaticHook(39952195u, val) == null && HookCaller.CallStaticHook(2535152661u, val) == null)
					{
						methodInfo?.Invoke(plugin, array);
						if (!string.IsNullOrEmpty(val.Reply) && ((Option)(ref option)).PrintOutput)
						{
							if ((Object)(object)player != (Object)null)
							{
								player.ConsoleMessage(val.Reply);
							}
							else if (FromRcon)
							{
								RCon.OnMessage(val.Reply, string.Empty, (LogType)3);
							}
							else
							{
								Logger.Log(val.Reply);
							}
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Error("Failed executing console command '" + command + "' in '" + plugin.ToPrettyString() + "' [callback]", ex.InnerException ?? ex);
				}
			}
			catch (TargetParameterCountException)
			{
			}
			catch (Exception ex3)
			{
				Logger.Error("Failed executing console command '" + command + "' in '" + plugin.ToPrettyString() + "' [internal]", ex3.InnerException ?? ex3);
			}
			Pool.FreeUnmanaged<object>(ref list);
			if (array != null)
			{
				Array.Clear(array, 0, array.Length);
			}
		}, help, reference, permissions, groups, authLevel, cooldown, isHidden, @protected, silent, doCooldownPenalty);
	}

	public void AddConsoleCommand(string command, BaseHookable plugin, Func<Arg, bool> callback, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1, int cooldown = 0, bool isHidden = false, bool @protected = false, bool silent = false, bool doCooldownPenalty = false)
	{
		API.Commands.Command.ClientConsole clientConsole = new API.Commands.Command.ClientConsole
		{
			Name = command,
			Reference = plugin,
			Callback = delegate(API.Commands.Command.Args args)
			{
				if (args.Tokenize<Arg>(out var value))
				{
					callback?.Invoke(value);
					args.Reply = value.Reply;
					args.PrintOutput = ((Option)(ref value.Option)).PrintOutput;
				}
			},
			Help = help,
			Token = reference,
			Auth = new API.Commands.Command.Authentication
			{
				AuthLevel = authLevel,
				Permissions = permissions,
				Groups = groups,
				Cooldown = cooldown,
				DoCooldownPenalty = doCooldownPenalty
			},
			CanExecute = OnPlayerExecute(isChat: false)
		};
		clientConsole.SetFlag(CommandFlags.Hidden, isHidden);
		clientConsole.SetFlag(CommandFlags.Protected, @protected);
		if (!Community.Runtime.CommandManager.RegisterCommand(clientConsole, out var reason) && !silent)
		{
			Logger.Warn(reason);
		}
		API.Commands.Command.RCon rCon = new API.Commands.Command.RCon
		{
			Name = command,
			Reference = plugin,
			Callback = delegate(API.Commands.Command.Args args)
			{
				if (args.Tokenize<Arg>(out var value))
				{
					args.PrintOutput = ((Option)(ref value.Option)).PrintOutput;
					callback?.Invoke(value);
					args.Reply = value.Reply;
				}
			},
			Help = help,
			Token = reference,
			CanExecute = OnPlayerExecute(isChat: false)
		};
		rCon.SetFlag(CommandFlags.Hidden, isHidden);
		rCon.SetFlag(CommandFlags.Protected, @protected);
		if (!Community.Runtime.CommandManager.RegisterCommand(rCon, out var reason2) && !silent)
		{
			Logger.Warn(reason2);
		}
	}

	public void AddCovalenceCommand(string command, BaseHookable plugin, string method, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1, int cooldown = 0, bool isHidden = false, bool @protected = false, bool silent = true, bool doCooldownPenalty = false)
	{
		AddChatCommand(command, plugin, method, help, reference, permissions, groups, authLevel, cooldown, isHidden, @protected, silent, doCooldownPenalty);
		AddConsoleCommand(command, plugin, method, help, reference, permissions, groups, authLevel, cooldown, isHidden, @protected, silent, doCooldownPenalty);
	}

	public void AddCovalenceCommand(string command, BaseHookable plugin, Action<BasePlayer, string, string[]> callback, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1, int cooldown = 0, bool isHidden = false, bool @protected = false, bool silent = true, bool doCooldownPenalty = false)
	{
		AddChatCommand(command, plugin, callback, help, reference, permissions, groups, authLevel, cooldown, isHidden, @protected, silent, doCooldownPenalty);
		AddConsoleCommand(command, plugin, callback, help, reference, permissions, groups, authLevel, cooldown, isHidden, @protected, silent, doCooldownPenalty);
	}

	public void RemoveChatCommand(string command, BaseHookable plugin = null)
	{
		Community.Runtime.CommandManager.ClearCommands((API.Commands.Command cmd) => cmd.Name == command && (plugin == null || cmd.Reference == plugin));
	}

	public void RemoveConsoleCommand(string command, BaseHookable plugin = null)
	{
		Community.Runtime.CommandManager.ClearCommands((API.Commands.Command cmd) => cmd.Name == command && (plugin == null || cmd.Reference == plugin));
	}
}
