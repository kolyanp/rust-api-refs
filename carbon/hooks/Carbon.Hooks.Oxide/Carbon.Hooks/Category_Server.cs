using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Core;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Server
{
	public class Server_Bootstrap
	{
		[Patch("InitLogging", "InitLogging", "Bootstrap", "StartupShared", new string[] { })]
		[Identifier("517e333aadbd482a9a5f3350527534d8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_Bootstrap_517e333aadbd482a9a5f3350527534d8 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 0)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1534983806)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[1] { typeof(uint) }, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Server_ServerMgr
	{
		[Patch("OnTick", "OnTick", "ServerMgr", "DoTick", new string[] { })]
		[Identifier("338814d7ee68481ca93d556af12e5937")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_ServerMgr_338814d7ee68481ca93d556af12e5937 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 0)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1216670645), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[1] { typeof(uint) }, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("IOnServerShutdown", "IOnServerShutdown", "ServerMgr", "Shutdown", new string[] { })]
		[Identifier("cc0898030e9f4d72b50375471f7b1487")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_ServerMgr_cc0898030e9f4d72b50375471f7b1487 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 0)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnServerShutdown", (Type[])null, (Type[])null)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnServerInitialize", "OnServerInitialize", "ServerMgr", "Initialize", new string[] { "System.Boolean", "System.String", "System.Boolean", "System.Boolean" })]
		[Identifier("cd7390a5bd3f434ab46f43cefaeadfb3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_ServerMgr_cd7390a5bd3f434ab46f43cefaeadfb3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 0)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1046244129)), instruction);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[1] { typeof(uint) }, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("IOnServerInitialized", "IOnServerInitialized", "ServerMgr", "OpenConnection", new string[] { "System.Boolean" })]
		[Identifier("6a5044681379441495c92c684d7eeb2d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_ServerMgr_6a5044681379441495c92c684d7eeb2d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 120)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnServerInitialized", (Type[])null, (Type[])null)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnServerRestartInterrupt", "OnServerRestartInterrupt", "ServerMgr", "RestartServer", new string[] { "System.String", "System.Int32" })]
		[Identifier("dce5a3d57dce40da81a107976d46c4a5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void))]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_ServerMgr_dce5a3d57dce40da81a107976d46c4a5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 8)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1183644224), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[1] { typeof(uint) }, (Type[])null));
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnServerRestart", "OnServerRestart", "ServerMgr", "RestartServer", new string[] { "System.String", "System.Int32" })]
		[Identifier("4c15515243094010b74fc83639cd04d8")]
		[Dependencies(new string[] { "OnServerRestartInterrupt" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("strNotice", "System.String", false)]
		[Parameter("iSeconds", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_ServerMgr_4c15515243094010b74fc83639cd04d8 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 25)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-578400763)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnServerInformationUpdated", "OnServerInformationUpdated", "ServerMgr", "UpdateServerInformation", new string[] { })]
		[Identifier("7c322fd22bbc42a9b70e086111a85418")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_ServerMgr_7c322fd22bbc42a9b70e086111a85418 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 425)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-184988060)), instruction);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[1] { typeof(uint) }, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Server_BasePlayer
	{
		[Patch("OnMessagePlayer", "OnMessagePlayer", "BasePlayer", "ChatMessage", new string[] { "System.String" })]
		[Identifier("d4688950126b4612b1c1bf5d92c65baa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("msg", "System.String", false)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_BasePlayer_d4688950126b4612b1c1bf5d92c65baa : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 3)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1279972524), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Server_ConVarChat
	{
		[Patch("OnServerMessage", "OnServerMessage", "ConVar.Chat", "Broadcast", new string[] { "System.String", "System.String", "System.String", "System.UInt64" })]
		[Identifier("262dd7fa5b4249a9ab0a8196b366c7f0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void))]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_ConVarChat_262dd7fa5b4249a9ab0a8196b366c7f0 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 0)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1139907162)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnPlayerActionBroadcast", "OnPlayerActionBroadcast", "ConVar.Chat", "BroadcastPlayerAction", new string[] { "BasePlayer", "System.String" })]
		[Identifier("7a154701abc84f4899773c4611594f6e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void))]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_ConVarChat_7a154701abc84f4899773c4611594f6e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 0)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1546891068)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}

		[Patch("OnPlayerActionBroadcast", "OnPlayerActionBroadcast [2]", "ConVar.Chat", "BroadcastPlayerAction", new string[] { "BasePlayer", "System.String", "BasePlayer", "System.String" })]
		[Identifier("9dd0eeaae920425cb438fba6d5d2c13a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void))]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_ConVarChat_9dd0eeaae920425cb438fba6d5d2c13a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 0)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1546891068)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}
	}

	public class Server_FacepunchRConRConListener
	{
		[Patch("OnRconConnection", "OnRconConnection [exp]", "Facepunch.RCon/RConListener", "ProcessConnections", new string[] { })]
		[Identifier("0d9c8d55c0544a2ca0771d4ec48e46c3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("address", "System.Net.IPAddress", false)]
		[Return(typeof(void))]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_FacepunchRConRConListener_0d9c8d55c0544a2ca0771d4ec48e46c3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 16)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-311078651)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("System.Net.IPEndPoint"), "get_Address", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
						typeof(object)
					}, (Type[])null));
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Server_SaveRestore
	{
		[Patch("OnNewSave", "OnNewSave", "SaveRestore", "Load", new string[] { "System.String", "System.Boolean" })]
		[Identifier("5a796193a52f477a95dbcb875c1db34f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("strFilename", "System.String", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_SaveRestore_5a796193a52f477a95dbcb875c1db34f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 14)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1479806872)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnSaveLoad", "OnSaveLoad", "SaveRestore", "Load", new string[] { "System.String", "System.Boolean" })]
		[Identifier("2b513382859745859d65ce6d07f739f4")]
		[Dependencies(new string[] { "OnNewSave" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "System.Collections.Generic.Dictionary`2[BaseEntity,ProtoBuf.Entity]", false)]
		[Return(typeof(bool))]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_SaveRestore_2b513382859745859d65ce6d07f739f4 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 367)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)106238856), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnServerSave", "OnServerSave", "SaveRestore", "DoAutomatedSave", new string[] { "System.Boolean" })]
		[Identifier("29ea3242823d48fda834eb664d7b5825")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_SaveRestore_29ea3242823d48fda834eb664d7b5825 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 0)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1898008991)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[1] { typeof(uint) }, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Server_ServerUsers
	{
		[Patch("OnServerUserSet", "OnServerUserSet", "ServerUsers", "Set", new string[] { "System.UInt64", "ServerUsers/UserGroup", "System.String", "System.String", "System.Int64" })]
		[Identifier("80536202b3bf44058eaf768fff812791")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_ServerUsers_80536202b3bf44058eaf768fff812791 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 19)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)931424179), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(UserGroup));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)4);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(long));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[6]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnServerUserRemove", "OnServerUserRemove", "ServerUsers", "Remove", new string[] { "System.UInt64" })]
		[Identifier("30bae818b0e84520becaf8c4f35a32ea")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_ServerUsers_30bae818b0e84520becaf8c4f35a32ea : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 0)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2043356880), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Server_FacepunchRCon
	{
		[Patch("IOnRconInitialize", "IOnRconInitialize", "Facepunch.RCon", "Initialize", new string[] { })]
		[Identifier("ca93ff1dd4954da6af009adcf9675620")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void))]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_FacepunchRCon_ca93ff1dd4954da6af009adcf9675620 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 0)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnRconInitialize", (Type[])null, (Type[])null)), instruction), instruction);
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Server_ConsoleNetwork
	{
		[Patch("OnSendCommand", "OnSendCommand", "ConsoleNetwork", "SendClientCommand", new string[] { "Network.Connection", "System.String", "System.Object[]" })]
		[Identifier("11768ee554f34eabb59e931bf8664758")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("cn", "Network.Connection", false)]
		[Parameter("strCommand", "System.String", false)]
		[Parameter("args", "System.Object[]", false)]
		[Return(typeof(void))]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_ConsoleNetwork_11768ee554f34eabb59e931bf8664758 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 4)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)156775275), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnSendCommand", "OnSendCommand [list]", "ConsoleNetwork", "SendClientCommand", new string[] { "System.Collections.Generic.List`1<Network.Connection>", "System.String", "System.Object[]" })]
		[Identifier("b2a359bd0acd4d00b22c72f7c43cfa89")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("cn", "Network.Connection", false)]
		[Parameter("strCommand", "System.String", false)]
		[Parameter("args", "System.Object[]", false)]
		[Return(typeof(void))]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_ConsoleNetwork_b2a359bd0acd4d00b22c72f7c43cfa89 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 4)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)156775275), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnBroadcastCommand", "OnBroadcastCommand", "ConsoleNetwork", "BroadcastToAllClients", new string[] { "System.String", "System.Object[]" })]
		[Identifier("3dc827337bc3460e8bfad1a6fae7c7bb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("strCommand", "System.String", false)]
		[Parameter("args", "System.Object[]", false)]
		[Return(typeof(void))]
		[Category("Server")]
		[Assembly("Assembly-CSharp.dll")]
		public class Server_ConsoleNetwork_3dc827337bc3460e8bfad1a6fae7c7bb : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 4)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-184063675)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Server_ConsoleSystem
	{
		[Patch("IOnServerCommand", "IOnServerCommand", "ConsoleSystem", "Internal", new string[] { "ConsoleSystem/Arg" })]
		[Identifier("caaa0ae8f1ee4ea386a5e89690fbf387")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("arg", "ConsoleSystem+Arg", false)]
		[Return(typeof(bool))]
		[Category("Server")]
		[Assembly("Facepunch.Console.dll")]
		public class Server_ConsoleSystem_caaa0ae8f1ee4ea386a5e89690fbf387 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 5)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldarg_0, (object)null), instruction);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnServerCommand", (Type[])null, (Type[])null));
					Label label1 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("IOnRunCommandLine", "IOnRunCommandLine", "ConsoleSystem", "UpdateValuesFromCommandLine", new string[] { })]
		[Identifier("7ce9fe1f6a7347188011a7a1628851f1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void))]
		[Category("Server")]
		[Assembly("Facepunch.Console.dll")]
		public class Server_ConsoleSystem_7ce9fe1f6a7347188011a7a1628851f1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 0)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnRunCommandLine", (Type[])null, (Type[])null)), instruction);
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}
	}

	public class Server_FacepunchRconListenercDisplayClass270
	{
		[Patch("OnRconConnection", "OnRconConnection [web]", "Facepunch.Rcon.Listener/<>c__DisplayClass27_0", "<Start>b__0", new string[] { "Fleck.IWebSocketConnection" })]
		[Identifier("76a9ba4bc35a49f593ec375dce2ff154")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("clientIpAddress", "System.Net.IPAddress", false)]
		[Return(typeof(void))]
		[Category("Server")]
		[Assembly("Facepunch.Rcon.dll")]
		public class Server_FacepunchRconListenercDisplayClass270_76a9ba4bc35a49f593ec375dce2ff154 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 46)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-311078651)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Fleck.IWebSocketConnection"), "get_ConnectionInfo", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Fleck.IWebSocketConnectionInfo"), "get_ClientIpAddress", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
						typeof(object)
					}, (Type[])null));
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}
	}
}
