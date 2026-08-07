using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using API.Abstracts;
using API.Commands;
using API.Events;
using API.Hooks;
using Carbon.Base;
using Carbon.Components;
using Carbon.Core;
using Carbon.Extensions;
using ConVar;
using Facepunch;
using Facepunch.Math;
using HarmonyLib;
using JetBrains.Annotations;
using Network;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Libraries;
using Oxide.Plugins;
using Steamworks;
using UnityEngine;

namespace Carbon.Hooks;

public class Category_Static
{
	public class Static_Debug
	{
		[Patch("IBroadcastOverride", "IBroadcastOverride", typeof(Chat), "Broadcast", new Type[]
		{
			typeof(string),
			typeof(string),
			typeof(string),
			typeof(ulong)
		})]
		[Options(/*Could not decode attribute arguments.*/)]
		public class IBroadcastOverride : Patch
		{
			public static bool Prefix(string message, ref string username, ref string color, ref ulong userid)
			{
				//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
				//IL_013c: Unknown result type (might be due to invalid IL or missing references)
				if (HookCaller.CallStaticHook(3155060134u, (object)message, (object)username, (object)color, (object)userid) != null)
				{
					return false;
				}
				if (userid == 0L)
				{
					if (Community.Runtime.Core.DefaultServerChatName != "-1")
					{
						username = Community.Runtime.Core.DefaultServerChatName;
					}
					if (Community.Runtime.Core.DefaultServerChatColor != "-1")
					{
						color = Community.Runtime.Core.DefaultServerChatColor;
					}
					if (Community.Runtime.Core.DefaultServerChatId != -1)
					{
						userid = (ulong)Community.Runtime.Core.DefaultServerChatId;
					}
				}
				string text = StringEx.EscapeRichText(username, false);
				ConsoleNetwork.BroadcastToAllClients("chat.add", new object[3]
				{
					2,
					userid,
					"<color=" + color + ">" + text + "</color> " + message
				});
				ChatEntry val = default(ChatEntry);
				((ChatEntry)(ref val)).Channel = (ChatChannel)2;
				((ChatEntry)(ref val)).Message = message;
				((ChatEntry)(ref val)).UserId = userid.ToString();
				((ChatEntry)(ref val)).Username = username;
				((ChatEntry)(ref val)).Color = color;
				((ChatEntry)(ref val)).Time = Epoch.Current;
				Chat.Record(val);
				return false;
			}
		}

		[Patch("INoGiveNotices", "INoGiveNotices", typeof(Chat), "BroadcastPlayerAction", new Type[]
		{
			typeof(BasePlayer),
			typeof(string)
		})]
		[Options(/*Could not decode attribute arguments.*/)]
		public class INoGiveNotices : Patch
		{
			public static bool Prefix(BasePlayer subject, string action)
			{
				if (Community.Runtime.Core.NoGiveNoticesCache && (action.Contains("give") || action.Contains("gave")))
				{
					return false;
				}
				return true;
			}
		}

		[Patch("INoGiveNotices2", "INoGiveNotices2", typeof(Chat), "BroadcastPlayerAction", new Type[]
		{
			typeof(BasePlayer),
			typeof(string),
			typeof(BasePlayer),
			typeof(string)
		})]
		[Options(/*Could not decode attribute arguments.*/)]
		public class INoGiveNotices2 : Patch
		{
			public static bool Prefix(BasePlayer subjectA, string middle, BasePlayer subjectB, string suffix)
			{
				if (Community.Runtime.Core.NoGiveNoticesCache && (middle.Contains("give") || middle.Contains("gave")))
				{
					return false;
				}
				return true;
			}
		}
	}

	public class Static_Chat
	{
		[Patch("IGetNameColor", "IGetNameColor", typeof(Chat), "GetNameColor", new Type[]
		{
			typeof(ulong),
			typeof(BasePlayer)
		})]
		[Options(/*Could not decode attribute arguments.*/)]
		public class IGetNameColor : Patch
		{
			private const string UserColor = "#5af";

			private const string AdminColor = "#af5";

			private const string DevColor = "#fa5";

			[UsedImplicitly]
			[HarmonyPostfix]
			public static void Postfix(ref string __result)
			{
				if (!string.IsNullOrEmpty(__result))
				{
					if (Community.Runtime.Core.NoAdminChatColorCache && __result.ToLower() == "#af5")
					{
						__result = "#5af";
					}
					else if (Community.Runtime.Core.NoDevChatColorCache && __result.ToLower() == "#fa5")
					{
						__result = "#5af";
					}
				}
			}
		}
	}

	public class Static_ConsoleSystem
	{
		[Patch("OnConsoleCommand", "OnConsoleCommand", typeof(ConsoleSystem), "RunWithResult", new Type[]
		{
			typeof(Option),
			typeof(string),
			typeof(object[])
		})]
		[Options(/*Could not decode attribute arguments.*/)]
		[Info("Called whenever a Carbon server command is called.")]
		public class IOnConsoleCommand : Patch
		{
			private static string Space = " ";

			private static readonly string[] Filters = new string[1] { "no_input" };

			public static bool Prefix(ref CommandResult __result, Option options, ref string strCommand, object[] args)
			{
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_002f: Unknown result type (might be due to invalid IL or missing references)
				//IL_012c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0136: Unknown result type (might be due to invalid IL or missing references)
				//IL_013d: Expected O, but got Unknown
				//IL_0167: Unknown result type (might be due to invalid IL or missing references)
				//IL_0219: Unknown result type (might be due to invalid IL or missing references)
				//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
				//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
				if (Community.Runtime == null || Enumerable.Contains(Filters, strCommand))
				{
					return true;
				}
				try
				{
					string text = default(string);
					object[] array = default(object[]);
					if (!ConsoleArgEx.TryParseCommand(strCommand, args, ref text, ref array))
					{
						__result = new CommandResult((CommandResultType)3, (string)null, (Command)null);
						return false;
					}
					if (Command.FromRcon)
					{
						return true;
					}
					MonoBehaviour obj = ((Option)(ref options)).Connection?.player;
					BasePlayer val = (BasePlayer)(object)((obj is BasePlayer) ? obj : null);
					List<Command> list = (((Object)(object)val == (Object)null) ? Community.Runtime.CommandManager.RCon : Community.Runtime.CommandManager.ClientConsole);
					if (Community.Runtime.Config.Aliases.TryGetValue(text, out var value))
					{
						text = value;
						strCommand = ((array.Length == 0) ? value : (value + " " + string.Join(Space, array)));
					}
					Command val2 = default(Command);
					if (Community.Runtime.CommandManager.Contains((IList<Command>)list, text, ref val2))
					{
						string text2 = " ";
						if (args != null && args.Length != 0)
						{
							for (int i = 0; i < args.Length; i++)
							{
								text2 += args[i].ToString();
								if (i < args.Length - 1)
								{
									text2 += " ";
								}
							}
						}
						Arg val3 = new Arg(options, strCommand + text2);
						val3.cmd = val2.RustCommand;
						val3.Invalid = false;
						PlayerArgs val4 = Pool.Get<PlayerArgs>();
						((Args)val4).Token = val3;
						((Args)val4).Type = val2.Type;
						((Args)val4).Arguments = array;
						val4.Player = val;
						((Args)val4).IsServer = (Object)(object)val == (Object)null;
						((Args)val4).PrintOutput = ((Option)(ref options)).PrintOutput || (Object)(object)val != (Object)null;
						Command.FromRcon = false;
						Community.Runtime.CommandManager.Execute(val2, (Args)(object)val4);
						__result = new CommandResult((CommandResultType)1, val3.Reply, val3.cmd);
						Pool.Free<PlayerArgs>(ref val4);
						return false;
					}
					if (Community.Runtime.Config.Logging.CommandSuggestions)
					{
						if ((Object)(object)val != (Object)null && !val.IsAdmin)
						{
							return true;
						}
						if (Server.Find(StringView.op_Implicit(text)) != null)
						{
							return true;
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Error((object)("Failed ConsoleSystem.Run [" + strCommand + "] [" + string.Join(" ", args) + "]"), ex);
				}
				return true;
			}
		}
	}

	public class Static_RCon
	{
		[Patch("OnRconCommand", "OnRconCommand", typeof(RCon), "OnCommand", new Type[] { typeof(Command) })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Info("Called when an RCON command is run.")]
		[Parameter("ip", typeof(IPAddress), false)]
		[Parameter("command", typeof(string), false)]
		[Parameter("arguments", typeof(string[]), false)]
		[OxideCompatible]
		public class IOnRconCommand : Patch
		{
			internal static string Space = " ";

			private static readonly Action _resetFromRconAction = delegate
			{
				Command.FromRcon = (Command.FromRcon = false);
			};

			public static bool Prefix(Command cmd)
			{
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_0089: Unknown result type (might be due to invalid IL or missing references)
				//IL_008e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0092: Unknown result type (might be due to invalid IL or missing references)
				//IL_0097: Unknown result type (might be due to invalid IL or missing references)
				//IL_009b: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c4: Expected O, but got Unknown
				//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
				//IL_0178: Unknown result type (might be due to invalid IL or missing references)
				//IL_0136: Unknown result type (might be due to invalid IL or missing references)
				//IL_013b: Unknown result type (might be due to invalid IL or missing references)
				if (Community.Runtime == null)
				{
					return true;
				}
				RCon.responseIdentifier = cmd.Identifier;
				RCon.responseConnection = cmd.ConnectionId;
				RCon.isInput = false;
				try
				{
					string text = default(string);
					object[] array = default(object[]);
					ConsoleArgEx.TryParseCommand(cmd.Message, ref text, ref array);
					object[] array2 = array as string[];
					object[] array3 = array2 ?? array.ToArray();
					if (Community.Runtime.Config.Aliases.TryGetValue(text, out var value))
					{
						text = value;
						cmd.Message = ((array3.Length == 0) ? value : (value + " " + string.Join(Space, array3)));
					}
					Option val = Option.Server;
					val = ((Option)(ref val)).Quiet();
					Arg val2 = new Arg(((Option)(ref val)).FromRconConnection(cmd.ConnectionId, cmd.Ip.ToString(), cmd.Name), cmd.Message);
					if (HookCaller.CallStaticHook(3740958730u, (object)cmd.Ip, (object)text, (object)array3) != null)
					{
						return false;
					}
					try
					{
						Command val3 = default(Command);
						if (Community.Runtime.CommandManager.Contains((IList<Command>)Community.Runtime.CommandManager.RCon, text, ref val3))
						{
							Command.FromRcon = (Command.FromRcon = true);
							StringView[] array4 = (StringView[])(object)new StringView[array3.Length];
							for (int i = 0; i < array3.Length; i++)
							{
								array4[i] = StringView.op_Implicit(array3[i]?.ToString());
							}
							val2.Args = array4;
							val2.cmd = val3.RustCommand;
							Args val4 = Pool.Get<Args>();
							val4.Token = val2;
							val4.Type = val3.Type;
							val4.Arguments = array3;
							val4.IsRCon = true;
							val4.IsServer = true;
							val4.PrintOutput = ((Option)(ref val2.Option)).PrintOutput;
							Community.Runtime.CommandManager.Execute(val3, val4);
							Pool.Free<Args>(ref val4);
							((Plugin)Community.Runtime.Core).NextFrame(_resetFromRconAction);
							return false;
						}
					}
					catch (Exception ex)
					{
						Logger.Error((object)"RconCommand_OnCommand", ex);
					}
				}
				finally
				{
					RCon.responseIdentifier = 0;
					RCon.responseConnection = -1;
				}
				return true;
			}
		}
	}

	public class Static_ServerMgr
	{
		[Patch("OnServerInitialized", "OnServerInitialized", typeof(ServerMgr), "OpenConnection", new Type[] { typeof(bool) })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Info("Called after the server startup has been completed and is awaiting connections.")]
		[Info("Also called for plugins that are hotloaded while the server is already started running.")]
		[Parameter("initialized", typeof(bool), true)]
		[OxideCompatible]
		public class IOnServerInitialized : Patch
		{
			public static void Postfix()
			{
				Community.Runtime.MarkServerInitialized(true);
				Patch.Events.Trigger((CarbonEvent)33, EventArgs.Empty);
			}
		}

		[Patch("IServerAsyncShutdown", "IServerAsyncShutdown", typeof(Global), "quit", new Type[] { typeof(Arg) })]
		[Options(/*Could not decode attribute arguments.*/)]
		public class IServerAsyncShutdown : Patch
		{
			internal static bool _isQuitting;

			internal static bool _allowNative;

			public static bool Prefix(Arg args)
			{
				if (_isQuitting)
				{
					return _allowNative;
				}
				Shutdown();
				return false;
			}

			public static async ValueTask Shutdown()
			{
				_isQuitting = true;
				foreach (BaseHookable module in Community.Runtime.ModuleProcessor.Modules)
				{
					BaseModule val = (BaseModule)(object)((module is BaseModule) ? module : null);
					if (val != null && val.IsEnabled())
					{
						await HandleHookable(module);
					}
				}
				foreach (RustPlugin item in ((IEnumerable<Package>)ModLoader.Packages).SelectMany((Package package) => package.Plugins))
				{
					await HandleHookable((BaseHookable)(object)item);
				}
				_allowNative = true;
				ConsoleSystem.Run(Option.Server, "quit", Array.Empty<object>());
				static async ValueTask HandleHookable(BaseHookable hookable)
				{
					try
					{
						await hookable.OnAsyncServerShutdown();
					}
					catch (Exception ex)
					{
						Logger.Error((object)("[" + hookable.Name + "] Failed asynchronous shutdown"), ex);
					}
				}
			}
		}

		[Patch("IServerInfoUpdate", "IServerInfoUpdate", typeof(ServerMgr), "UpdateServerInformation", new Type[] { })]
		[Options(/*Could not decode attribute arguments.*/)]
		public class IServerInfoUpdate : Patch
		{
			public static bool ForceModded
			{
				get
				{
					if (!CarbonAuto.Singleton.IsForceModded())
					{
						return Community.Runtime.ModuleProcessor.Modules.Any(delegate(BaseHookable x)
						{
							BaseModule val = (BaseModule)(object)((x is BaseModule) ? x : null);
							return val != null && val.IsEnabled() && val.ForceModded;
						});
					}
					return true;
				}
			}

			public static void Postfix()
			{
				if (!SteamServer.IsValid || Community.Runtime == null || Community.Runtime.Config == null || Community.Runtime.Core == null)
				{
					return;
				}
				try
				{
					ServerTagEx.SetRequiredTag("^y", true);
					if (Community.Runtime.Config.IsModded || ForceModded)
					{
						ServerTagEx.SetRequiredTag("^z", true);
					}
					else
					{
						ServerTagEx.UnsetRequiredTag("^z", true);
					}
					if (!string.IsNullOrEmpty(Community.Runtime.Core.CustomMapName) && !Community.Runtime.Core.CustomMapName.Equals("-1"))
					{
						SteamServer.MapName = Community.Runtime.Core.CustomMapName;
					}
				}
				catch (Exception ex)
				{
					Logger.Error((object)"Couldn't patch UpdateServerInformation.", ex);
				}
			}
		}

		[Patch("IServerMgrOnRPCMessage", "IServerMgrOnRPCMessage", typeof(ServerMgr), "OnRPCMessage", new Type[] { typeof(Message) })]
		[Options(/*Could not decode attribute arguments.*/)]
		public class IServerMgrOnRPCMessage : Patch
		{
			public static MethodInfo Method = AccessTools.Method(typeof(ClientEntity), "ServerRPCUnknown", (Type[])null, (Type[])null);

			private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				//IL_005b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0065: Expected O, but got Unknown
				//IL_006c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0076: Expected O, but got Unknown
				//IL_007d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0087: Expected O, but got Unknown
				//IL_0092: Unknown result type (might be due to invalid IL or missing references)
				//IL_009c: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>(instructions);
				int num = -1;
				CodeInstruction val = null;
				for (int num2 = list.Count - 1; num2 >= 0; num2--)
				{
					val = list[num2];
					if (val.opcode == OpCodes.Brfalse_S)
					{
						num = num2;
						break;
					}
				}
				if (num == -1)
				{
					throw new NullReferenceException("IServerMgrOnRPCMessage failure.");
				}
				list.InsertRange(num + 1, new List<CodeInstruction>
				{
					new CodeInstruction(OpCodes.Ldloc_0, (object)null),
					new CodeInstruction(OpCodes.Ldloc_1, (object)null),
					new CodeInstruction(OpCodes.Ldarg_1, (object)null),
					new CodeInstruction(OpCodes.Call, (object)Method)
				});
				return list;
			}
		}
	}
}
