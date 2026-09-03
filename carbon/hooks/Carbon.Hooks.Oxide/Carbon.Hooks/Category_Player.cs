using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Core;
using Carbon.Extensions;
using HarmonyLib;
using ProtoBuf;
using UnityEngine;

namespace Carbon.Hooks;

public class Category_Player
{
	public class Player_ServerMgr
	{
		[Patch("OnPlayerDisconnected", "OnPlayerDisconnected", "ServerMgr", "OnDisconnected", new string[] { "System.String", "Network.Connection" })]
		[Identifier("4a693a1e393c4a808aa806b7b1f89bb1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("strReason", "System.String", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_4a693a1e393c4a808aa806b7b1f89bb1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 37)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)72085565), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnClientAuth", "OnClientAuth", "ServerMgr", "OnGiveUserInformation", new string[] { "Network.Message" })]
		[Identifier("0f2894e9904c4900bc68dc5100e889ae")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("connection", "Network.Connection", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_0f2894e9904c4900bc68dc5100e889ae : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 118)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2031294194)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Network.Message"), "get_connection", (Type[])null, (Type[])null));
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

		[Patch("OnPlayerSetInfo", "OnPlayerSetInfo [server]", "ServerMgr", "ClientReady", new string[] { "Network.Message" })]
		[Identifier("fca320b7a69543e5866c4ca33515426c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("connection", "Network.Connection", false)]
		[Parameter("name", "System.String", false)]
		[Parameter("value", "System.String", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_fca320b7a69543e5866c4ca33515426c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 30)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2011944267)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Network.Message"), "get_connection", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("ProtoBuf.ClientReady+ClientInfo"), "name"));
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("ProtoBuf.ClientReady+ClientInfo"), "value"));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("IOnPlayerBanned", "IOnPlayerBanned [Publisher/VAC]", "ServerMgr", "OnValidateAuthTicketResponse", new string[] { "System.UInt64", "System.UInt64", "AuthResponse" })]
		[Identifier("db811a96dc004d908f3cd49d55d83239")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "Network.Connection", false)]
		[Parameter("status", "AuthResponse", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_db811a96dc004d908f3cd49d55d83239 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 56)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldloc_1, (object)null), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnPlayerBanned", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnFindSpawnPoint", "OnFindSpawnPoint", "ServerMgr", "FindSpawnPoint", new string[] { "BasePlayer", "System.UInt64" })]
		[Identifier("1aaed3f6bbd1419692fa273d2c7e2d57")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(SpawnPoint))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_1aaed3f6bbd1419692fa273d2c7e2d57 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)619699665), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(SpawnPoint));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(SpawnPoint));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnClientDisconnect", "OnClientDisconnect", "ServerMgr", "ReadDisconnectReason", new string[] { "Network.Message" })]
		[Identifier("a128d3aa48ea438786264cc251cd2db5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("connection", "Network.Connection", false)]
		[Parameter("local0", "System.String", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_a128d3aa48ea438786264cc251cd2db5 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)834943051), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Network.Message"), "get_connection", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnPlayerVoice", "OnPlayerVoice", "ServerMgr", "OnPlayerVoice", new string[] { "Network.Message" })]
		[Identifier("3b1d25f5a1e046c7ac6788593f2fd9a7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.ArraySegment`1[System.Byte]", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_3b1d25f5a1e046c7ac6788593f2fd9a7 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1259305119)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.ArraySegment`1").MakeGenericType(AccessTools.TypeByName("System.Byte")));
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

		[Patch("OnPlayerSpawn", "OnPlayerSpawn", "ServerMgr", "SpawnNewPlayer", new string[] { "Network.Connection" })]
		[Identifier("1b229de475354442a0d232171528bd61")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_1b229de475354442a0d232171528bd61 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Expected O, but got Unknown
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Expected O, but got Unknown
				//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c2: Expected O, but got Unknown
				//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00eb: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnPlayerSpawn"));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 2, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[25];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 2, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[25]), list2[25]);
				}
				list2.InsertRange(25, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Player_CodeLock
	{
		[Patch("CanUseLockedEntity", "CanUseLockedEntity [CodeLock, open]", "CodeLock", "OnTryToOpen", new string[] { "BasePlayer" })]
		[Identifier("c3f868fe82544cf7a0dff4473aea2ca8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CodeLock", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_CodeLock_c3f868fe82544cf7a0dff4473aea2ca8 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-679812965)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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

		[Patch("CanUseLockedEntity", "CanUseLockedEntity [CodeLock, close]", "CodeLock", "OnTryToClose", new string[] { "BasePlayer" })]
		[Identifier("9602aaa8b5054176aea7639356186b8a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CodeLock", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_CodeLock_9602aaa8b5054176aea7639356186b8a : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-679812965)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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

		[Patch("CanUnlock", "CanUnlock [CodeLock]", "CodeLock", "TryUnlock", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("7eb5dd94aba247e5bc063fd0b5e7bdfd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CodeLock", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_CodeLock_7eb5dd94aba247e5bc063fd0b5e7bdfd : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 9)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2118405101), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("CanLock", "CanLock [code]", "CodeLock", "TryLock", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("42bda9813c504e0fbf210b4be4128d2f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CodeLock", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_CodeLock_42bda9813c504e0fbf210b4be4128d2f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 15)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1531266972), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("CanChangeCode", "CanChangeCode", "CodeLock", "RPC_ChangeCode", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("3e9fae883c984976b245b41e03b13086")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CodeLock", false)]
		[Parameter("local0", "System.String", false)]
		[Parameter("local1", "System.Boolean", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_CodeLock_3e9fae883c984976b245b41e03b13086 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 36)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2119330727), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Boolean"));
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

	public class Player_KeyLock
	{
		[Patch("CanUseLockedEntity", "CanUseLockedEntity [KeyLock, close]", "KeyLock", "OnTryToClose", new string[] { "BasePlayer" })]
		[Identifier("f9c7eb9400a341cdad5133d5d6c53830")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "KeyLock", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_KeyLock_f9c7eb9400a341cdad5133d5d6c53830 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-679812965)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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

		[Patch("CanUseLockedEntity", "CanUseLockedEntity [KeyLock, open]", "KeyLock", "OnTryToOpen", new string[] { "BasePlayer" })]
		[Identifier("99423010a26144bb8b1fbaaba755816a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "KeyLock", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_KeyLock_99423010a26144bb8b1fbaaba755816a : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-679812965)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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

		[Patch("CanUnlock", "CanUnlock [KeyLock]", "KeyLock", "RPC_Unlock", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("58b1bbad234b462ca3b5fecae63dcd5c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "KeyLock", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_KeyLock_58b1bbad234b462ca3b5fecae63dcd5c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 9)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2118405101), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("CanLock", "CanLock [key]", "KeyLock", "Lock", new string[] { "BasePlayer" })]
		[Identifier("5695da9641f646f7a5ca80fbd6d62e1e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "KeyLock", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_KeyLock_5695da9641f646f7a5ca80fbd6d62e1e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 13)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1531266972), instruction), instruction);
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

	public class Player_PlayerLoot
	{
		[Patch("OnLootEntity", "OnLootEntity", "PlayerLoot", "StartLootingEntity", new string[] { "BaseEntity", "System.Boolean" })]
		[Identifier("dbd6951e868641f3bc9f165f6da2091c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerLoot", false)]
		[Parameter("targetEntity", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerLoot_dbd6951e868641f3bc9f165f6da2091c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 27)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)576899103), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnLootItem", "OnLootItem", "PlayerLoot", "StartLootingItem", new string[] { "Item" })]
		[Identifier("481bb05858054f62a18c779e7e4aab15")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerLoot", false)]
		[Parameter("item", "Item", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerLoot_481bb05858054f62a18c779e7e4aab15 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 37)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2011244315)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnPlayerLootEnd", "OnPlayerLootEnd", "PlayerLoot", "Clear", new string[] { })]
		[Identifier("6fea33cf4444475eb7a69e18c456b1b8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerLoot", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerLoot_6fea33cf4444475eb7a69e18c456b1b8 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)78733418), instruction);
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

		[Patch("OnLootNetworkUpdate", "OnLootNetworkUpdate", "PlayerLoot", "SendUpdate", new string[] { })]
		[Identifier("0a82c56210344c70a7302308a85e2a1c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerLoot", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerLoot_0a82c56210344c70a7302308a85e2a1c : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1899681783), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

	public class Player_BaseMelee
	{
		[Patch("OnPlayerAttack", "OnPlayerAttack [Melee]", "BaseMelee", "DoAttackShared", new string[] { "HitInfo" })]
		[Identifier("b4fe5de22ab946a2b455dfbd7035bb14")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMelee", false)]
		[Parameter("info", "HitInfo", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMelee_b4fe5de22ab946a2b455dfbd7035bb14 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1437762689), instruction);
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

		[Patch("OnMeleeAttack", "OnMeleeAttack", "BaseMelee", "PlayerAttack", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("605615843a8a4d62922a9621f2642cad")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local3", "HitInfo", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMelee_605615843a8a4d62922a9621f2642cad : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 47)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)853308222), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
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
	}

	public class Player_BasePlayer
	{
		[Patch("OnPlayerAttack", "OnPlayerAttack [Projectile]", "BasePlayer", "OnProjectileAttack", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("cd35a547a409440c9a1adb3b0f2880a9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("hitInfo", "BasePlayer+<>c__DisplayClass466_0", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_cd35a547a409440c9a1adb3b0f2880a9 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 1859)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1437762689), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer+<>c__DisplayClass466_0"), "hitInfo"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("HitInfo"));
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

		[Patch("OnPlayerSleepEnd", "OnPlayerSleepEnd", "BasePlayer", "EndSleeping", new string[] { })]
		[Identifier("b8091f82087746ddbcbcb14f83e56a41")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_b8091f82087746ddbcbcb14f83e56a41 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1550249805), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnPlayerTick", "OnPlayerTick", "BasePlayer", "OnReceiveTick", new string[] { "PlayerTick", "System.Boolean" })]
		[Identifier("f083bc8fb7134b8cb88c396717113a72")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_f083bc8fb7134b8cb88c396717113a72 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)291725147), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}

		[Patch("IOnBasePlayerAttacked", "IOnBasePlayerAttacked", "BasePlayer", "OnAttacked", new string[] { "HitInfo" })]
		[Identifier("1297da42e233439a9b1304d167bef0a6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_1297da42e233439a9b1304d167bef0a6 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldarg_0, (object)null), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnBasePlayerAttacked", (Type[])null, (Type[])null));
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("IOnBasePlayerHurt", "IOnBasePlayerHurt", "BasePlayer", "Hurt", new string[] { "HitInfo" })]
		[Identifier("72a05582778341ad8f186436151c50a1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_72a05582778341ad8f186436151c50a1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 18)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldarg_0, (object)null), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnBasePlayerHurt", (Type[])null, (Type[])null));
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("CanLootPlayer", "CanLootPlayer", "BasePlayer", "CanBeLooted", new string[] { "BasePlayer" })]
		[Identifier("18be51d1716244a0a19c45786a126b62")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_18be51d1716244a0a19c45786a126b62 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1341651690)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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

		[Patch("CanBeWounded", "CanBeWounded", "BasePlayer", "EligibleForWounding", new string[] { "HitInfo" })]
		[Identifier("758831ce8764449d811fe584f18b727a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_758831ce8764449d811fe584f18b727a : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)578388980), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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

		[Patch("OnPlayerRespawned", "OnPlayerRespawned", "BasePlayer", "RespawnAt", new string[] { "UnityEngine.Vector3", "UnityEngine.Quaternion", "BaseEntity" })]
		[Identifier("dd084e59fb814aa7a673d3186734d587")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_dd084e59fb814aa7a673d3186734d587 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 202)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)458523914), instruction), instruction);
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

		[Patch("OnPlayerSpectate", "OnPlayerSpectate", "BasePlayer", "StartSpectating", new string[] { })]
		[Identifier("1761f86be26f4794a4df9a292eccb1bb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_1761f86be26f4794a4df9a292eccb1bb : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1578450530), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer"), "spectateFilter"));
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

		[Patch("OnPlayerSpectateEnd", "OnPlayerSpectateEnd", "BasePlayer", "StopSpectating", new string[] { })]
		[Identifier("94233db7f7334b0188817caf030d613c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_94233db7f7334b0188817caf030d613c : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1309639414), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer"), "spectateFilter"));
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

		[Patch("OnPlayerHealthChange", "OnPlayerHealthChange", "BasePlayer", "OnHealthChanged", new string[] { "System.Single", "System.Single" })]
		[Identifier("17cf139270204c5dbffa4499322b615c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("oldvalue", "System.Single", false)]
		[Parameter("newvalue", "System.Single", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_17cf139270204c5dbffa4499322b615c : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1718534982)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
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

		[Patch("OnPlayerSleep", "OnPlayerSleep", "BasePlayer", "StartSleeping", new string[] { })]
		[Identifier("617952449cb142fe816f4e943e889380")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_617952449cb142fe816f4e943e889380 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-236552164)), instruction), instruction);
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

		[Patch("OnPlayerDeath", "OnPlayerDeath", "BasePlayer", "Die", new string[] { "HitInfo" })]
		[Identifier("e10d2476f46541fa9c40c4d34f730e3b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("info", "HitInfo", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_e10d2476f46541fa9c40c4d34f730e3b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 58)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-733984534)), instruction);
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

		[Patch("OnPlayerRespawn", "OnPlayerRespawn", "BasePlayer", "Respawn", new string[] { })]
		[Identifier("7fbe8ad207304b55b0969f50f3cee96c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "BasePlayer+SpawnPoint", false)]
		[Return(typeof(SpawnPoint), Continues = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_7fbe8ad207304b55b0969f50f3cee96c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 15)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1546340674), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					instruction.labels.Add(label1);
					object retvar = Generator.DeclareLocal(typeof(object));
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(SpawnPoint));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(SpawnPoint));
					yield return __GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 0, typeof(SpawnPoint));
					yield return instruction;
				}
			}
		}

		[Patch("OnPlayerKicked", "OnPlayerKicked", "BasePlayer", "Kick", new string[] { "System.String", "System.Boolean" })]
		[Identifier("0f6963fca79c42099f3b66f9f11caa81")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_0f6963fca79c42099f3b66f9f11caa81 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1321158727), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("CanDropActiveItem", "CanDropActiveItem", "BasePlayer", "ShouldDropActiveItem", new string[] { })]
		[Identifier("d8ddc4c49e5f4111b6c88fbe95130496")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_d8ddc4c49e5f4111b6c88fbe95130496 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-462541570)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnActiveItemChange", "OnActiveItemChange", "BasePlayer", "UpdateActiveItem", new string[] { "ItemId" })]
		[Identifier("26a77433d0d040bcab90b889cc52016d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local1", "Item", false)]
		[Parameter("itemID", "ItemId", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_26a77433d0d040bcab90b889cc52016d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 30)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1428168745)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ItemId));
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

		[Patch("OnPlayerInput", "OnPlayerInput", "BasePlayer", "OnReceiveTick", new string[] { "PlayerTick", "System.Boolean" })]
		[Identifier("63ce61c2408d48fc825cd83cce2c8bca")]
		[Dependencies(new string[] { "OnPlayerTick" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_63ce61c2408d48fc825cd83cce2c8bca : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 28)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-883355335)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BasePlayer"), "get_serverInput", (Type[])null, (Type[])null));
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

		[Patch("OnLootPlayer", "OnLootPlayer", "BasePlayer", "RPC_LootPlayer", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("a66918fc217c471daf181de6d39e690b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_a66918fc217c471daf181de6d39e690b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 43)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-418546259)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnPlayerLand", "OnPlayerLand", "BasePlayer", "ApplyFallDamageFromVelocity", new string[] { "System.Single" })]
		[Identifier("75aa831ba28e4a478aec81bcfcc66424")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "System.Single", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_75aa831ba28e4a478aec81bcfcc66424 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 13)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1397235125)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Single"));
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

		[Patch("OnPlayerLanded", "OnPlayerLanded", "BasePlayer", "ApplyFallDamageFromVelocity", new string[] { "System.Single" })]
		[Identifier("7e0c1a20f58c403197e121d150c5b8d3")]
		[Dependencies(new string[] { "OnPlayerLand" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "System.Single", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_7e0c1a20f58c403197e121d150c5b8d3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 83)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)41951618), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Single"));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("CanSpectateTarget", "CanSpectateTarget", "BasePlayer", "UpdateSpectateTarget", new string[] { "System.String", "System.Boolean", "System.Boolean" })]
		[Identifier("35690114144144c7b0397e2e8b72c0a0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("strName", "System.String", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_35690114144144c7b0397e2e8b72c0a0 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)583626537), instruction);
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

		[Patch("OnPlayerSetInfo", "OnPlayerSetInfo", "BasePlayer", "SetInfo", new string[] { "System.String", "System.String" })]
		[Identifier("39161e68564c49b8978187baabb9fb72")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("key", "System.String", false)]
		[Parameter("val", "System.String", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_39161e68564c49b8978187baabb9fb72 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2011944267)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer"), "net"));
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Network.Networkable"), "get_connection", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnPlayerAssist", "OnPlayerAssist", "BasePlayer", "RPC_Assist", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("f97a388ad6f24b8a8250eb0e91d08e7a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_f97a388ad6f24b8a8250eb0e91d08e7a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 15)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-99153677)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnPlayerKeepAlive", "OnPlayerKeepAlive", "BasePlayer", "RPC_KeepAlive", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("3e20eb2986764f3b8f08de62f3d46209")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_3e20eb2986764f3b8f08de62f3d46209 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 15)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)316982530), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnActiveItemChanged", "OnActiveItemChanged", "BasePlayer", "UpdateActiveItem", new string[] { "ItemId" })]
		[Identifier("0ddaeeb86ebd41f2b554976985a6bdda")]
		[Dependencies(new string[] { "OnActiveItemChange" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local1", "Item", false)]
		[Parameter("local2", "Item", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_0ddaeeb86ebd41f2b554976985a6bdda : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 116)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2026929315)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnMapMarkersClear", "OnMapMarkersClear", "BasePlayer", "Server_ClearMapMarkers", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("8bdf86ee01c0491b926b10e21ccfda6f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_8bdf86ee01c0491b926b10e21ccfda6f : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)555961858), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BasePlayer"), "get_State", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("ProtoBuf.PlayerState"), "pointsOfInterest"));
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

		[Patch("OnMapMarkersCleared", "OnMapMarkersCleared", "BasePlayer", "Server_ClearMapMarkers", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("6ba0dfac358e4588900ab0ec2cc0ba92")]
		[Dependencies(new string[] { "OnMapMarkersClear" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_6ba0dfac358e4588900ab0ec2cc0ba92 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 52)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1358655847), instruction);
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

		[Patch("CanNetworkTo", "CanNetworkTo [BasePlayer]", "BasePlayer", "ShouldNetworkTo", new string[] { "BasePlayer" })]
		[Identifier("ff15007c0dc841acb5496bd132e4085b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_ff15007c0dc841acb5496bd132e4085b : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1622751857), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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

		[Patch("IOnPlayerConnected", "IOnPlayerConnected", "BasePlayer", "PlayerInit", new string[] { "Network.Connection" })]
		[Identifier("569cc4c1efd5495fb904679946918466")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_569cc4c1efd5495fb904679946918466 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 225)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldarg_0, (object)null), instruction);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnPlayerConnected", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnMapMarkerRemove", "OnMapMarkerRemove", "BasePlayer", "Server_RemovePointOfInterest", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("1e7d079e41124d74b3a43b8a8988b2bb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Parameter("local0", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_1e7d079e41124d74b3a43b8a8988b2bb : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 17)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)137635766), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BasePlayer"), "get_State", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("ProtoBuf.PlayerState"), "pointsOfInterest"));
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
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

		[Patch("OnMapMarkerAdded", "OnMapMarkerAdded", "BasePlayer", "Server_AddMarker", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("973ae3b0e7bc4e35a4b06294a2d9b93d")]
		[Dependencies(new string[] { "OnMapMarkerAdd [patch]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "ProtoBuf.MapNote", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_973ae3b0e7bc4e35a4b06294a2d9b93d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 81)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1405948638), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnPlayerCorpseSpawned", "OnPlayerCorpseSpawned", "BasePlayer", "CreateCorpse", new string[] { "BasePlayer/PlayerFlags", "UnityEngine.Vector3", "UnityEngine.Quaternion", "System.Collections.Generic.List`1<TriggerBase>", "System.Boolean" })]
		[Identifier("861e32fe666d4acd94bdc390f4679cd4")]
		[Dependencies(new string[] { "OnPlayerCorpseSpawn" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local3", "PlayerCorpse", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_861e32fe666d4acd94bdc390f4679cd4 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 186)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)21048961), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnDemoRecordingStart", "OnDemoRecordingStart", "BasePlayer", "StartServerDemoRecording", new string[] { })]
		[Identifier("2f423d935bfc4e77b75d257d9b5fd29b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "System.String", false)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_2f423d935bfc4e77b75d257d9b5fd29b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 21)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1246450253), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnDemoRecordingStarted", "OnDemoRecordingStarted", "BasePlayer", "StartServerDemoRecording", new string[] { })]
		[Identifier("4d13075b8ed649cbbeba5b2767df5984")]
		[Dependencies(new string[] { "OnDemoRecordingStart" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "System.String", false)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_4d13075b8ed649cbbeba5b2767df5984 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 91)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-559719077)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnDemoRecordingStop", "OnDemoRecordingStop", "BasePlayer", "StopServerDemoRecording", new string[] { })]
		[Identifier("e98479a907a1413d93fde42020a078bd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_e98479a907a1413d93fde42020a078bd : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-502608069)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer"), "net"));
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Network.Networkable"), "get_connection", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Network.Connection"), "get_RecordFilename", (Type[])null, (Type[])null));
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

		[Patch("OnDemoRecordingStopped", "OnDemoRecordingStopped", "BasePlayer", "StopServerDemoRecording", new string[] { })]
		[Identifier("2699d8bb7cfc4ee2a658db7b40107e71")]
		[Dependencies(new string[] { "OnDemoRecordingStop" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_2699d8bb7cfc4ee2a658db7b40107e71 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 41)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1350840123), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer"), "net"));
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Network.Networkable"), "get_connection", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Network.Connection"), "get_RecordFilename", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnThreatLevelUpdate", "OnThreatLevelUpdate", "BasePlayer", "EnsureUpdated", new string[] { })]
		[Identifier("7ad482a255f2489183caecfa350ee91d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_7ad482a255f2489183caecfa350ee91d : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1022289592)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnRespawnInformationGiven", "OnRespawnInformationGiven", "BasePlayer", "SendRespawnOptions", new string[] { })]
		[Identifier("5e710d719bdf4ef1912a1ef77a1ab5ae")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "System.Collections.Generic.List`1[ProtoBuf.RespawnInformation+SpawnOptions]", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_5e710d719bdf4ef1912a1ef77a1ab5ae : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-448826001)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnPlayerRecover", "OnPlayerRecover", "BasePlayer", "RecoverFromWounded", new string[] { })]
		[Identifier("434f13a230ad4a25b65078e66ef7b752")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_434f13a230ad4a25b65078e66ef7b752 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1249471098)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnPlayerWound", "OnPlayerWound", "BasePlayer", "BecomeWounded", new string[] { "HitInfo" })]
		[Identifier("e0d1012e3cbc4f328675f39ce16c946f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("info", "HitInfo", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_e0d1012e3cbc4f328675f39ce16c946f : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1875605816), instruction), instruction);
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

		[Patch("OnPlayerRecovered", "OnPlayerRecovered", "BasePlayer", "RecoverFromWounded", new string[] { })]
		[Identifier("81f0562002c5483f8aff59bd43d93cc3")]
		[Dependencies(new string[] { "OnPlayerRecover" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_81f0562002c5483f8aff59bd43d93cc3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 39)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1682271133), instruction), instruction);
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

		[Patch("OnPlayerColliderEnable", "OnPlayerColliderEnable", "BasePlayer", "EnablePlayerCollider", new string[] { })]
		[Identifier("62965fdd13df47298bb42bfd10a19579")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_62965fdd13df47298bb42bfd10a19579 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-400535245)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BasePlayer"), "get_playerCollider", (Type[])null, (Type[])null));
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

		[Patch("OnPlayerSleepEnded", "OnPlayerSleepEnded", "BasePlayer", "EndSleeping", new string[] { })]
		[Identifier("47d7dafc71af4fe299071a7d0e5e4e5d")]
		[Dependencies(new string[] { "OnPlayerSleepEnd" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_47d7dafc71af4fe299071a7d0e5e4e5d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 127)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1269498168)), instruction), instruction);
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

		[Patch("OnPlayerMarkersSend", "OnPlayerMarkersSend", "BasePlayer", "SendMarkersToClient", new string[] { })]
		[Identifier("02fb5b80c80f4c4a8e501b80b4e25480")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "ProtoBuf.MapNoteList", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_02fb5b80c80f4c4a8e501b80b4e25480 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 23)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1152063698)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnPlayerPingsSend", "OnPlayerPingsSend", "BasePlayer", "SendPingsToClient", new string[] { })]
		[Identifier("526769967ffa427894289cd2776dda3d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "ProtoBuf.MapNoteList", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_526769967ffa427894289cd2776dda3d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 11)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)372024025), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnActiveTelephoneUpdated", "OnActiveTelephoneUpdated [BasePlayer]", "BasePlayer", "SetActiveTelephone", new string[] { "PhoneController" })]
		[Identifier("b637d1b18d8840128cba11a40f487a9b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_b637d1b18d8840128cba11a40f487a9b : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1021592397)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnPlayerRevive", "OnPlayerRevive", "BasePlayer", "OnMedicalToolApplied", new string[] { "BasePlayer", "ItemDefinition", "ItemModConsumable", "MedicalTool", "System.Boolean" })]
		[Identifier("2f380f38d18041c2b5582a22b92ed633")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("fromPlayer", "BasePlayer", false)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_2f380f38d18041c2b5582a22b92ed633 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 11)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1335676105), instruction);
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}

		[Patch("OnFogOfWarStale", "OnFogOfWarStale", "BasePlayer", "OnFogOfWarStale", new string[] { })]
		[Identifier("b02526c489fa46629ee96cd8a3a74522")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_b02526c489fa46629ee96cd8a3a74522 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)851821006), instruction), instruction);
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

		[Patch("OnFogOfWarCleared", "OnFogOfWarCleared", "BasePlayer", "ServerClearFog", new string[] { "System.Boolean", "System.Boolean" })]
		[Identifier("65fd885d08e642d894850c74af15242d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("mainland", "System.Boolean", false)]
		[Parameter("deepSea", "System.Boolean", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_65fd885d08e642d894850c74af15242d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-790526269)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnFogOfWarImageUpdate", "OnFogOfWarImageUpdate", "BasePlayer", "FogImageUpdate", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("7a0998de90944d8b846969180347441f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "System.Byte", false)]
		[Parameter("local1", "System.Byte", false)]
		[Parameter("local2", "System.UInt32", false)]
		[Parameter("local3", "System.UInt32", false)]
		[Parameter("local5", "System.Byte[]", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_7a0998de90944d8b846969180347441f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 59)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)148009165), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Byte"));
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Byte"));
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt32"));
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt32"));
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[7]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
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

		[Patch("OnMapMarkerAdd", "OnMapMarkerAdd", "BasePlayer", "Server_AddMarker", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("522c5d942b564bc0b8b55084c037f565")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_522c5d942b564bc0b8b55084c037f565 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Expected O, but got Unknown
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Expected O, but got Unknown
				//IL_0081: Unknown result type (might be due to invalid IL or missing references)
				//IL_008b: Expected O, but got Unknown
				//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b8: Expected O, but got Unknown
				//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c9: Expected O, but got Unknown
				//IL_0129: Unknown result type (might be due to invalid IL or missing references)
				//IL_0133: Expected O, but got Unknown
				//IL_014d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0157: Expected O, but got Unknown
				//IL_015e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0168: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity/RPCMessage"), "read")));
				list.Add(new CodeInstruction(OpCodes.Ldnull, (object)null));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Network.NetRead"), "Proto", (Type[])null, new Type[1] { typeof(MapNote) })));
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 0, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnMapMarkerAdd"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[0];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[0]), list2[0]);
				}
				list2.InsertRange(0, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnMapMarkerAdd [patch]", "OnMapMarkerAdd [patch]", "BasePlayer", "Server_AddMarker", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("f5b73a46c62d4b19b786a94992b009d6")]
		[Dependencies(new string[] { "OnMapMarkerAdd" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_f5b73a46c62d4b19b786a94992b009d6 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[43]), list2[43]);
				}
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[44].labels);
				}
				else
				{
					list2[48].labels.AddRange(list2[44].labels);
				}
				list2[44].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[45].labels);
				}
				else
				{
					list2[48].labels.AddRange(list2[45].labels);
				}
				list2[45].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[46].labels);
				}
				else
				{
					list2[48].labels.AddRange(list2[46].labels);
				}
				list2[46].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[47].labels);
				}
				else
				{
					list2[48].labels.AddRange(list2[47].labels);
				}
				list2[47].labels.Clear();
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[48], list2[43]), list2[43]);
				}
				list2.RemoveRange(43, 5);
				list2.InsertRange(43, list);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnPlayerCorpseSpawn", "OnPlayerCorpseSpawn", "BasePlayer", "CreateCorpse", new string[] { "BasePlayer/PlayerFlags", "UnityEngine.Vector3", "UnityEngine.Quaternion", "System.Collections.Generic.List`1<TriggerBase>", "System.Boolean" })]
		[Identifier("2ddca443291f492ca6a2fe1918c5907a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_2ddca443291f492ca6a2fe1918c5907a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_006e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0078: Expected O, but got Unknown
				//IL_0092: Unknown result type (might be due to invalid IL or missing references)
				//IL_009c: Expected O, but got Unknown
				//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ad: Expected O, but got Unknown
				//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00be: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnPlayerCorpseSpawn"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[2]
				{
					typeof(string),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[0];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldnull, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[0]), list2[0]);
				}
				list2.InsertRange(0, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnSendModelState", "OnSendModelState", "BasePlayer", "SendModelState", new string[] { "System.Boolean" })]
		[Identifier("9c58c6ae4d9245eaa3a803685fe92d4f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_9c58c6ae4d9245eaa3a803685fe92d4f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected O, but got Unknown
				//IL_0057: Unknown result type (might be due to invalid IL or missing references)
				//IL_0061: Expected O, but got Unknown
				//IL_006c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0076: Expected O, but got Unknown
				//IL_007d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0087: Expected O, but got Unknown
				//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cc: Expected O, but got Unknown
				//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00dd: Expected O, but got Unknown
				//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
				//IL_0102: Expected O, but got Unknown
				//IL_0109: Unknown result type (might be due to invalid IL or missing references)
				//IL_0113: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseNetworkable"), "get_limitNetworking", (Type[])null, (Type[])null)));
				Label label = Generator.DefineLabel();
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnSendModelState"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[2]
				{
					typeof(string),
					typeof(object)
				}, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Ldnull, (object)null));
				Label label2 = Generator.DefineLabel();
				CodeInstruction obj = list2[23];
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label2));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				list[8].labels.Add(label);
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[23]), list2[23]);
				}
				list2.InsertRange(23, list);
				obj.labels.Add(label2);
				return list2.AsEnumerable();
			}
		}
	}

	public class Player_PlayerMetabolism
	{
		[Patch("OnRunPlayerMetabolism", "OnRunPlayerMetabolism", "PlayerMetabolism", "RunMetabolism", new string[] { "BaseCombatEntity", "System.Single" })]
		[Identifier("e182c67c5da347e4be25065c62edb009")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerMetabolism", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerMetabolism_e182c67c5da347e4be25065c62edb009 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1948488445), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
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

		[Patch("OnPlayerMetabolize", "OnPlayerMetabolize", "PlayerMetabolism", "ServerUpdate", new string[] { "BaseCombatEntity", "System.Single" })]
		[Identifier("1c5e2315b87a42a5825ab180c1315bf6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerMetabolism", false)]
		[Parameter("ownerEntity", "BaseCombatEntity", false)]
		[Parameter("delta", "System.Single", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerMetabolism_1c5e2315b87a42a5825ab180c1315bf6 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)367386711), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Player_ConnectionAuth
	{
		[Patch("IOnUserApprove", "IOnUserApprove", "ConnectionAuth", "OnNewConnection", new string[] { "Network.Connection" })]
		[Identifier("39a4fd48d0344045a96beaa4115fffb2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("connection", "Network.Connection", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConnectionAuth_39a4fd48d0344045a96beaa4115fffb2 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 153)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldarg_1, (object)null), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnUserApprove", (Type[])null, (Type[])null));
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

	public class Player_Signage
	{
		[Patch("CanUpdateSign", "CanUpdateSign [Signage]", "Signage", "CanUpdateSign", new string[] { "BasePlayer" })]
		[Identifier("6e1af65f106b402dabfe488b33e91023")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "Signage", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Signage_6e1af65f106b402dabfe488b33e91023 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1024438622), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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
	}

	public class Player_ResearchTable
	{
		[Patch("CanResearchItem", "CanResearchItem", "ResearchTable", "DoResearch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("f408115a618f41af94bf85ed7cdf6515")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "Item", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ResearchTable_f408115a618f41af94bf85ed7cdf6515 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 13)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-741507662)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

	public class Player_AutoTurret
	{
		[Patch("CanBeTargeted", "CanBeTargeted [AutoTurret]", "AutoTurret", "ObjectVisible", new string[] { "BaseCombatEntity" })]
		[Identifier("4990e237353a48cc8d78fc294c55c01b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("obj", "BaseCombatEntity", false)]
		[Parameter("self", "AutoTurret", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_AutoTurret_4990e237353a48cc8d78fc294c55c01b : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1065566406), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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
	}

	public class Player_HelicopterTurret
	{
		[Patch("CanBeTargeted", "CanBeTargeted [HelicopterTurret]", "HelicopterTurret", "InFiringArc", new string[] { "BaseCombatEntity" })]
		[Identifier("a3f87313ab184ff897043d7de99ae039")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("potentialtarget", "BaseCombatEntity", false)]
		[Parameter("self", "HelicopterTurret", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_HelicopterTurret_a3f87313ab184ff897043d7de99ae039 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1065566406), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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
	}

	public class Player_LootableCorpse
	{
		[Patch("OnLootEntityEnd", "OnLootEntityEnd [LootableCorpse]", "LootableCorpse", "PlayerStoppedLooting", new string[] { "BasePlayer" })]
		[Identifier("35b8f72acb754925954821b5c3a18f68")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "LootableCorpse", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_LootableCorpse_35b8f72acb754925954821b5c3a18f68 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-902474312)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("CanLootEntity", "CanLootEntity [LootableCorpse]", "LootableCorpse", "RPC_LootCorpse", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("b4e029a568cc4f5e8e172ab7b634a923")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("self", "LootableCorpse", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_LootableCorpse_b4e029a568cc4f5e8e172ab7b634a923 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1627232611), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}
	}

	public class Player_StorageContainer
	{
		[Patch("OnLootEntityEnd", "OnLootEntityEnd [StorageContainer]", "StorageContainer", "PlayerStoppedLooting", new string[] { "BasePlayer" })]
		[Identifier("cfb567a0a5f54c7494b73603e4aeaf3f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "StorageContainer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_StorageContainer_cfb567a0a5f54c7494b73603e4aeaf3f : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-902474312)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("CanLootEntity", "CanLootEntity [StorageContainer]", "StorageContainer", "PlayerOpenLoot", new string[] { "BasePlayer", "System.String", "System.Boolean" })]
		[Identifier("935a939e2b3b4cd6a308b670df936e3c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_StorageContainer_935a939e2b3b4cd6a308b670df936e3c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected O, but got Unknown
				//IL_008c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0096: Expected O, but got Unknown
				//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ba: Expected O, but got Unknown
				//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cb: Expected O, but got Unknown
				//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00dc: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"CanLootEntity"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[0];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldc_I4_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[0]), list2[0]);
				}
				list2.InsertRange(0, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Player_ConnectionQueue
	{
		[Patch("CanBypassQueue", "CanBypassQueue", "ConnectionQueue", "CanJumpQueue", new string[] { "Network.Connection" })]
		[Identifier("bd7013aa660e40c6b7aa8cd78c252dbd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConnectionQueue_bd7013aa660e40c6b7aa8cd78c252dbd : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-847415929)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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
	}

	public class Player_FlameTurret
	{
		[Patch("CanBeTargeted", "CanBeTargeted [FlameTurret]", "FlameTurret", "CheckTrigger", new string[] { })]
		[Identifier("79d5a11e44644e7985e3b1f503131703")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local7", "BasePlayer", false)]
		[Parameter("self", "FlameTurret", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_FlameTurret_79d5a11e44644e7985e3b1f503131703 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 62)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1065566406), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)7);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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

		[Patch("CanBeTargeted [patch]", "CanBeTargeted [FlameTurret] [cleanup]", "FlameTurret", "CheckTrigger", new string[] { })]
		[Identifier("ba0aa544d1874b12a15fedb255592cd4")]
		[Dependencies(new string[] { "CanBeTargeted [FlameTurret]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_FlameTurret_ba0aa544d1874b12a15fedb255592cd4 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0040: Unknown result type (might be due to invalid IL or missing references)
				//IL_004a: Expected O, but got Unknown
				//IL_007e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0088: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[71];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(__GeneratorRuntime.CreateLoadLocalAddressInstruction(Generator, Method, 0, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Carbon.Pooling.PoolEx"), "FreeRaycastHitList", (Type[])null, (Type[])null)));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[71]), list2[71]);
				}
				list2.InsertRange(71, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Player_BaseCombatEntity
	{
		[Patch("CanPickupEntity", "CanPickupEntity", "BaseCombatEntity", "CanCompletePickup", new string[] { "BasePlayer" })]
		[Identifier("ec58da2be5ba4c4c9b75416e8b34d231")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BaseCombatEntity", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseCombatEntity_ec58da2be5ba4c4c9b75416e8b34d231 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)861710679), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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
	}

	public class Player_SleepingBag
	{
		[Patch("CanAssignBed", "CanAssignBed", "SleepingBag", "AssignToFriend", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("b7dc9e419ae041fd97fd59c3392ec6fa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "SleepingBag", false)]
		[Parameter("local0", "System.UInt64", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_SleepingBag_b7dc9e419ae041fd97fd59c3392ec6fa : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 24)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1589203649), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt64"));
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

		[Patch("CanSetBedPublic", "CanSetBedPublic", "SleepingBag", "RPC_MakePublic", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("d8d18500c7d947b5ab60469deb773c37")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "SleepingBag", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_SleepingBag_d8d18500c7d947b5ab60469deb773c37 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 31)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1894874021), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("CanRenameBed", "CanRenameBed", "SleepingBag", "Rename", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("942f2ffa720540a3bda9c459a256b718")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "SleepingBag", false)]
		[Parameter("local0", "System.String", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_SleepingBag_942f2ffa720540a3bda9c459a256b718 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 12)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)933436111), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnPlayerRespawn", "OnPlayerRespawn [SleepingBag]", "SleepingBag", "SpawnPlayer", new string[] { "BasePlayer", "NetworkableId" })]
		[Identifier("870a29057fa1461da741be6365416e68")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local1", "SleepingBag", false)]
		[Return(typeof(SleepingBag), Continues = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_SleepingBag_870a29057fa1461da741be6365416e68 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 53)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1546340674), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					instruction.labels.Add(label1);
					object retvar = Generator.DeclareLocal(typeof(object));
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(SleepingBag));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(SleepingBag));
					yield return __GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 1, typeof(SleepingBag));
					yield return instruction;
				}
			}
		}
	}

	public class Player_StashContainer
	{
		[Patch("CanHideStash", "CanHideStash", "StashContainer", "RPC_HideStash", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("3ae5f6f8da2842acbe5a5a3897d4ef78")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "StashContainer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_StashContainer_3ae5f6f8da2842acbe5a5a3897d4ef78 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)425492667), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

	public class Player_AntiHack
	{
		[Patch("OnPlayerViolation", "OnPlayerViolation", "AntiHack", "AddViolation", new string[] { "BasePlayer", "AntiHackType", "System.Single", "UnityEngine.GameObject" })]
		[Identifier("a07db06885d541a9ae92a140adc04e3c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_AntiHack_a07db06885d541a9ae92a140adc04e3c : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1356028081), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(AntiHackType));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
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

	public class Player_Mailbox
	{
		[Patch("CanUseMailbox", "CanUseMailbox", "Mailbox", "PlayerIsOwner", new string[] { "BasePlayer" })]
		[Identifier("cee21d50e8f94aa6b64b7e314fa8c725")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "Mailbox", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Mailbox_cee21d50e8f94aa6b64b7e314fa8c725 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1628781272)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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
	}

	public class Player_SpinnerWheel
	{
		[Patch("OnSpinWheel", "OnSpinWheel", "SpinnerWheel", "RPC_Spin", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("124d5e11cb18452aaf4f849c74167d31")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "SpinnerWheel", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_SpinnerWheel_124d5e11cb18452aaf4f849c74167d31 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1253444423)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

	public class Player_GunTrap
	{
		[Patch("CanBeTargeted", "CanBeTargeted [GunTrap]", "GunTrap", "CheckTrigger", new string[] { })]
		[Identifier("438ab93581934b9ea7a665b8e07feaea")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local7", "BasePlayer", false)]
		[Parameter("self", "GunTrap", false)]
		[Return(typeof(bool), Continues = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_GunTrap_438ab93581934b9ea7a665b8e07feaea : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 44)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1065566406), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)7);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					instruction.labels.Add(label1);
					object retvar = Generator.DeclareLocal(typeof(object));
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(bool));
					yield return __GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 2, typeof(bool));
					yield return instruction;
				}
			}
		}

		[Patch("CanBeTargeted", "CanBeTargeted [GunTrap] [patch]", "GunTrap", "CheckTrigger", new string[] { })]
		[Identifier("aabc92bdc98b4979a37aa4e732f4c6df")]
		[Dependencies(new string[] { "CanBeTargeted [GunTrap]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_GunTrap_aabc92bdc98b4979a37aa4e732f4c6df : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0036: Unknown result type (might be due to invalid IL or missing references)
				//IL_0040: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				list2[145].labels.Add(label);
				list.Add(new CodeInstruction(OpCodes.Leave, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[56]), list2[56]);
				}
				list2.InsertRange(56, list);
				return list2.AsEnumerable();
			}
		}

		[Patch("CanBeTargeted", "CanBeTargeted [GunTrap] [patch2]", "GunTrap", "CheckTrigger", new string[] { })]
		[Identifier("fcfad55a707d46fd8fef0978015b44c5")]
		[Dependencies(new string[] { "CanBeTargeted [GunTrap] [patch]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_GunTrap_fcfad55a707d46fd8fef0978015b44c5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[57];
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[52]), list2[52]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[53], list2[52]), list2[52]);
				}
				list2.RemoveRange(52, 1);
				list2.InsertRange(52, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Player_BaseLock
	{
		[Patch("CanPickupLock", "CanPickupLock", "BaseLock", "RPC_TakeLock", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("ae67a69c699249768b3b8fe4fcbfc7bb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BaseLock", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseLock_ae67a69c699249768b3b8fe4fcbfc7bb : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 9)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1699477002)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

	public class Player_BaseMountable
	{
		[Patch("CanDismountEntity", "CanDismountEntity", "BaseMountable", "DismountPlayer", new string[] { "BasePlayer", "System.Boolean" })]
		[Identifier("a8d1745e47a349639f8d1a0b3bfa0d74")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BaseMountable", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMountable_a8d1745e47a349639f8d1a0b3bfa0d74 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 12)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1801686644), instruction), instruction);
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

		[Patch("CanMountEntity", "CanMountEntity", "BaseMountable", "MountPlayer", new string[] { "BasePlayer" })]
		[Identifier("5a5defdc3abe4d39bfb2b96d75977eb3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BaseMountable", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMountable_5a5defdc3abe4d39bfb2b96d75977eb3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 12)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1731456645), instruction), instruction);
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

		[Patch("OnEntityMounted", "OnEntityMounted", "BaseMountable", "MountPlayer", new string[] { "BasePlayer" })]
		[Identifier("2fa815a565db4cb79c51a1f755ba692e")]
		[Dependencies(new string[] { "CanMountEntity" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMountable", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMountable_2fa815a565db4cb79c51a1f755ba692e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 99)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)715700557), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnPlayerWantsDismount", "OnPlayerWantsDismount", "BaseMountable", "RPC_WantsDismount", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("843c7044deb247f1ba6cf525493313a2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("self", "BaseMountable", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMountable_843c7044deb247f1ba6cf525493313a2 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1447743721)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnPlayerWantsMount", "OnPlayerWantsMount", "BaseMountable", "WantsMount", new string[] { "BasePlayer" })]
		[Identifier("257d70f35737494380700d440d568ce2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BaseMountable", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMountable_257d70f35737494380700d440d568ce2 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 7)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-458380270)), instruction), instruction);
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

		[Patch("CanSwapToSeat", "CanSwapToSeat [BaseMountable]", "BaseMountable", "CanSwapToThis", new string[] { "BasePlayer" })]
		[Identifier("c0dac44b6bb14bac9f49df58eb404992")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BaseMountable", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMountable_c0dac44b6bb14bac9f49df58eb404992 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-219653404)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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

		[Patch("OnPlayerDismountFailed", "OnPlayerDismountFailed", "BaseMountable", "RPC_WantsDismount", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("e1ba4d62494e4013a83aa31e9ea3ba7f")]
		[Dependencies(new string[] { "OnPlayerWantsDismount" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("self", "BaseMountable", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMountable_e1ba4d62494e4013a83aa31e9ea3ba7f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 7)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1686891064), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Player_PlayerBelt
	{
		[Patch("OnPlayerActiveShieldDrop", "OnPlayerActiveShieldDrop", "PlayerBelt", "DropActive", new string[] { "UnityEngine.Vector3", "UnityEngine.Vector3" })]
		[Identifier("f434142ef0504d3898e76c7db290669a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerBelt", false)]
		[Parameter("local0", "Shield", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerBelt_f434142ef0504d3898e76c7db290669a : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1000662839)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("PlayerBelt"), "player"));
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnPlayerDropActiveItem", "OnPlayerDropActiveItem", "PlayerBelt", "DropActive", new string[] { "UnityEngine.Vector3", "UnityEngine.Vector3" })]
		[Identifier("170dc4978172423c8b01e1a47a8d882c")]
		[Dependencies(new string[] { "OnPlayerActiveShieldDrop" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerBelt", false)]
		[Parameter("local1", "Item", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerBelt_170dc4978172423c8b01e1a47a8d882c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 51)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1929499452)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("PlayerBelt"), "player"));
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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
	}

	public class Player_ResourceContainer
	{
		[Patch("CanLootEntity", "CanLootEntity [ResourceContainer]", "ResourceContainer", "StartLootingContainer", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("0345f0545a75436f899da1aa697ccefa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("self", "ResourceContainer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ResourceContainer_0345f0545a75436f899da1aa697ccefa : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1627232611), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

	public class Player_DroppedItemContainer
	{
		[Patch("CanLootEntity", "CanLootEntity [DroppedItemContainer]", "DroppedItemContainer", "PlayerOpenLoot", new string[] { "BasePlayer" })]
		[Identifier("70668bf27738411cb3fd0be847db8c5d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "DroppedItemContainer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_DroppedItemContainer_70668bf27738411cb3fd0be847db8c5d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 7)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1627232611), instruction), instruction);
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

		[Patch("OnLootEntityEnd", "OnLootEntityEnd [DroppedItemContainer]", "DroppedItemContainer", "PlayerStoppedLooting", new string[] { "BasePlayer" })]
		[Identifier("0acd2f30abb44cbbbae504efd8b9563b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "DroppedItemContainer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_DroppedItemContainer_0acd2f30abb44cbbbae504efd8b9563b : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-902474312)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Player_HackableLockedCrate
	{
		[Patch("CanHackCrate", "CanHackCrate", "HackableLockedCrate", "RPC_Hack", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("11fcc74e1daf421db1bc77a733d70f12")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "HackableLockedCrate", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_HackableLockedCrate_11fcc74e1daf421db1bc77a733d70f12 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1229062350), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

	public class Player_Workbench
	{
		[Patch("OnExperimentStart", "OnExperimentStart", "Workbench", "RPC_BeginExperiment", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("8c09ef5933234edcaa67785de6dec8dc")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Workbench", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Workbench_8c09ef5933234edcaa67785de6dec8dc : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 110)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-60206750)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnExperimentStarted", "OnExperimentStarted", "Workbench", "RPC_BeginExperiment", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("0809435e07be436ea3ee7b52013a79c0")]
		[Dependencies(new string[] { "OnExperimentStart" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Workbench", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Workbench_0809435e07be436ea3ee7b52013a79c0 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 190)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1442929596), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnExperimentEnd", "OnExperimentEnd", "Workbench", "ExperimentComplete", new string[] { })]
		[Identifier("2217a5b6c27a431499e37892621b1571")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Workbench", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Workbench_2217a5b6c27a431499e37892621b1571 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 13)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)737761198), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnExperimentEnded", "OnExperimentEnded", "Workbench", "ExperimentComplete", new string[] { })]
		[Identifier("c4f87934c31743fe9e93465e5808dbb8")]
		[Dependencies(new string[] { "OnExperimentEnd" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Workbench", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Workbench_c4f87934c31743fe9e93465e5808dbb8 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 106)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)864580963), instruction), instruction);
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
	}

	public class Player_DoorCloser
	{
		[Patch("ICanPickupEntity", "ICanPickupEntity [DoorCloser]", "DoorCloser", "RPC_Take", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("8f18f5669f09462eb391a75a2fad03f2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "DoorCloser", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_DoorCloser_8f18f5669f09462eb391a75a2fad03f2 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldarg_1, (object)null), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "ICanPickupEntity", (Type[])null, (Type[])null));
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

	public class Player_GrowableEntity
	{
		[Patch("CanTakeCutting", "CanTakeCutting", "GrowableEntity", "TakeClones", new string[] { "BasePlayer" })]
		[Identifier("5dbe4c50344f43909f2c8e4356722af8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "GrowableEntity", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_GrowableEntity_5dbe4c50344f43909f2c8e4356722af8 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 10)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1498549656), instruction), instruction);
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

	public class Player_BuildingBlock
	{
		[Patch("OnPayForUpgrade", "OnPayForUpgrade", "BuildingBlock", "PayForUpgrade", new string[] { "ConstructionGrade", "BasePlayer" })]
		[Identifier("9587df54145d4d2f877b8e9809684da9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BuildingBlock", false)]
		[Parameter("g", "ConstructionGrade", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BuildingBlock_9587df54145d4d2f877b8e9809684da9 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-147615515)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}
	}

	public class Player_Planner
	{
		[Patch("OnPayForPlacement", "OnPayForPlacement [Planner]", "Planner", "PayForPlacement", new string[] { "BasePlayer", "Construction" })]
		[Identifier("40f119dc5229467c902d432d5d430e00")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "Planner", false)]
		[Parameter("component", "Construction", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Planner_40f119dc5229467c902d432d5d430e00 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1437978728)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}
	}

	public class Player_WallpaperPlanner
	{
		[Patch("OnPayForPlacement", "OnPayForPlacement [WallpaperPlanner]", "WallpaperPlanner", "PayForPlacement", new string[] { "BasePlayer", "Construction" })]
		[Identifier("bceb6b9559af442394c9ba9561647b9e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WallpaperPlanner", false)]
		[Parameter("component", "Construction", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_WallpaperPlanner_bceb6b9559af442394c9ba9561647b9e : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1437978728)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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
	}

	public class Player_WireTool
	{
		[Patch("OnWireConnect", "OnWireConnect", "WireTool", "RPC_MakeConnection", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("e798c86e975c41c08b690abac9c79205")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local5", "IOEntity", false)]
		[Parameter("local3", "System.Int32", false)]
		[Parameter("local6", "IOEntity", false)]
		[Parameter("local4", "System.Int32", false)]
		[Parameter("linePoints", "System.Collections.Generic.List`1[UnityEngine.Vector3]", false)]
		[Parameter("local8", "System.Collections.Generic.List`1[System.Single]", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_WireTool_e798c86e975c41c08b690abac9c79205 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 279)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-940506168)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)6);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)4);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("ProtoBuf.WireConnectionMessage"), "linePoints"));
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)8);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[8]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object),
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

		[Patch("CanUseWires", "CanUseWires", "WireTool", "CanPlayerUseWires", new string[] { "BasePlayer", "System.Boolean", "System.Single", "IOEntity" })]
		[Identifier("4f55844d32984e89a5d9e6067de84470")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_WireTool_4f55844d32984e89a5d9e6067de84470 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1762378939)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object),
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
	}

	public class Player_ItemModStudyBlueprint
	{
		[Patch("OnPlayerStudyBlueprint", "OnPlayerStudyBlueprint", "ItemModStudyBlueprint", "ServerCommand", new string[] { "Item", "System.String", "BasePlayer" })]
		[Identifier("bbffcef9dba045f2b1e760d52bf1a7e6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("item", "Item", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ItemModStudyBlueprint_bbffcef9dba045f2b1e760d52bf1a7e6 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 63)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-160872990)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
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
	}

	public class Player_RidableHorse
	{
		[Patch("CanLootEntity", "CanLootEntity [RidableHorse]", "RidableHorse", "SERVER_OpenLoot", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("1d27db95caa24098804050cfa8d2c0f8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("self", "RidableHorse", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_RidableHorse_1d27db95caa24098804050cfa8d2c0f8 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 33)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1627232611), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

	public class Player_PlayerInventory
	{
		[Patch("OnClothingItemChanged", "OnClothingItemChanged", "PlayerInventory", "OnClothingChanged", new string[] { "Item", "System.Boolean" })]
		[Identifier("e4ff894b97d14d78ab79476ef65ced77")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Parameter("item", "Item", false)]
		[Parameter("bAdded", "System.Boolean", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerInventory_e4ff894b97d14d78ab79476ef65ced77 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 63)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2060069440), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnDefaultItemsReceive", "OnDefaultItemsReceive", "PlayerInventory", "GiveDefaultItems", new string[] { })]
		[Identifier("1cc70ee19c7844888de56b706ecb75c0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerInventory_1cc70ee19c7844888de56b706ecb75c0 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-851097159)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnDefaultItemsReceived", "OnDefaultItemsReceived", "PlayerInventory", "GiveDefaultItems", new string[] { })]
		[Identifier("a270cba70d3a440286099d5ff152895b")]
		[Dependencies(new string[] { "OnDefaultItemsReceive" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerInventory_a270cba70d3a440286099d5ff152895b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 84)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1157321387)), instruction), instruction);
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

		[Patch("OnInventoryNetworkUpdate", "OnInventoryNetworkUpdate", "PlayerInventory", "SendUpdatedInventoryInternal", new string[] { "PlayerInventory/Type", "ItemContainer", "PlayerInventory/NetworkInventoryMode" })]
		[Identifier("299825908d2a4e69ae9b324c655aae72")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Parameter("container", "ItemContainer", false)]
		[Parameter("local0", "ProtoBuf.UpdateItemContainer", false)]
		[Parameter("type", "PlayerInventory+Type", false)]
		[Parameter("mode", "PlayerInventory+NetworkInventoryMode", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerInventory_299825908d2a4e69ae9b324c655aae72 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 47)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1378383296)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(Type));
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(NetworkInventoryMode));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[6]
					{
						typeof(uint),
						typeof(object),
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

	public class Player_BaseRagdoll
	{
		[Patch("CanRagdollDismount", "CanRagdollDismount", "BaseRagdoll", "AllowPlayerInstigatedDismount", new string[] { "BasePlayer" })]
		[Identifier("4153ecb5691a4907aa684affc22b93dd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseRagdoll", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseRagdoll_4153ecb5691a4907aa684affc22b93dd : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1601410662), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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
	}

	public class Player_PhotoFrame
	{
		[Patch("CanUpdateSign", "CanUpdateSign [PhotoFrame]", "PhotoFrame", "CanUpdateSign", new string[] { "BasePlayer" })]
		[Identifier("50881a1f04584f6599876740a0afeb1a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "PhotoFrame", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PhotoFrame_50881a1f04584f6599876740a0afeb1a : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1024438622), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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
	}

	public class Player_ContainerIOEntity
	{
		[Patch("OnLootEntityEnd", "OnLootEntityEnd [ContainerIOEntity]", "ContainerIOEntity", "PlayerStoppedLooting", new string[] { "BasePlayer" })]
		[Identifier("f41e7859806243f895741bdef2b04ef4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ContainerIOEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ContainerIOEntity_f41e7859806243f895741bdef2b04ef4 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-902474312)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("CanLootEntity", "CanLootEntity [ContainerIOEntity]", "ContainerIOEntity", "PlayerOpenLoot", new string[] { "BasePlayer", "System.String", "System.Boolean" })]
		[Identifier("48f7c23d795f4bc68917a5139bedceac")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ContainerIOEntity_48f7c23d795f4bc68917a5139bedceac : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected O, but got Unknown
				//IL_008c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0096: Expected O, but got Unknown
				//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ba: Expected O, but got Unknown
				//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cb: Expected O, but got Unknown
				//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00dc: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"CanLootEntity"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[0];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldc_I4_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[0]), list2[0]);
				}
				list2.InsertRange(0, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Player_ModularCarSeat
	{
		[Patch("CanSwapToSeat", "CanSwapToSeat [ModularCarSeat]", "ModularCarSeat", "CanSwapToThis", new string[] { "BasePlayer" })]
		[Identifier("4d6a65e5a2a64391ab530613aed16f5c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ModularCarSeat", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ModularCarSeat_4d6a65e5a2a64391ab530613aed16f5c : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-219653404)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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
	}

	public class Player_GestureConfig
	{
		[Patch("CanUseGesture", "CanUseGesture", "GestureConfig", "IsOwnedBy", new string[] { "BasePlayer", "System.Boolean" })]
		[Identifier("9005f5b4efef4afc83d49593f10f94a7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "GestureConfig", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_GestureConfig_9005f5b4efef4afc83d49593f10f94a7 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)67342617), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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
	}

	public class Player_ConsoleNetwork
	{
		[Patch("OnClientCommand", "OnClientCommand", "ConsoleNetwork", "OnClientCommand", new string[] { "Network.Message" })]
		[Identifier("14d6c06fd6174aaf890af1c0a923ab67")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("connection", "Network.Connection", false)]
		[Parameter("local0", "System.String", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConsoleNetwork_14d6c06fd6174aaf890af1c0a923ab67 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 28)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1268192620)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Network.Message"), "get_connection", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

	public class Player_RelationshipManager
	{
		[Patch("CanSetRelationship", "CanSetRelationship", "RelationshipManager", "SetRelationship", new string[] { "BasePlayer", "BasePlayer", "RelationshipManager/RelationshipType", "System.Int32", "System.Boolean" })]
		[Identifier("271342b4b5f542bc9bd2e70a52f96c9d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("otherPlayer", "BasePlayer", false)]
		[Parameter("type", "RelationshipManager+RelationshipType", false)]
		[Parameter("weight", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_RelationshipManager_271342b4b5f542bc9bd2e70a52f96c9d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 33)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1286611062)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(RelationshipType));
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)4);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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
	}

	public class Player_LiquidContainer
	{
		[Patch("OnPlayerDrink", "OnPlayerDrink", "LiquidContainer", "SVDrink", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("d854894bb19d477382be39c4382b1c21")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "LiquidContainer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_LiquidContainer_d854894bb19d477382be39c4382b1c21 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 6)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)837351664), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}
	}

	public class Player_ItemBasedFlowRestrictor
	{
		[Patch("OnLootEntityEnd", "OnLootEntityEnd [FuseBox]", "ItemBasedFlowRestrictor", "PlayerStoppedLooting", new string[] { "BasePlayer" })]
		[Identifier("1660665dcc364604b7f30de232e721ee")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ItemBasedFlowRestrictor", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ItemBasedFlowRestrictor_1660665dcc364604b7f30de232e721ee : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-902474312)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Player_CarvablePumpkin
	{
		[Patch("CanUpdateSign", "CanUpdateSign [CarvablePumpkin]", "CarvablePumpkin", "CanUpdateSign", new string[] { "BasePlayer" })]
		[Identifier("cb0c7642b20a4068a01e6f6d591a5f25")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CarvablePumpkin", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_CarvablePumpkin_cb0c7642b20a4068a01e6f6d591a5f25 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1024438622), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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
	}

	public class Player_AttackEntity
	{
		[Patch("OnEyePosValidate", "OnEyePosValidate", "AttackEntity", "ValidateEyePos", new string[] { "BasePlayer", "UnityEngine.Vector3", "System.Boolean" })]
		[Identifier("f5a30c2516424958b8f3739a25388be1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AttackEntity", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("eyePos", "UnityEngine.Vector3", false)]
		[Parameter("checkLineOfSight", "System.Boolean", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_AttackEntity_f5a30c2516424958b8f3739a25388be1 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1230915907), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(Vector3));
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object),
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
	}

	public class Player_BaseProjectile
	{
		[Patch("OnClientProjectileEffectCreate", "OnClientProjectileEffectCreate", "BaseProjectile", "CreateProjectileEffectClientside", new string[] { "System.String", "UnityEngine.Vector3", "UnityEngine.Vector3", "System.Int32", "Network.Connection", "System.Boolean", "System.Boolean", "System.Collections.Generic.List`1<Network.Connection>", "System.Single" })]
		[Identifier("b19f91a0e38b40f38934563b64d4062b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("sourceConnection", "Network.Connection", false)]
		[Parameter("self", "BaseProjectile", false)]
		[Parameter("prefabName", "System.String", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseProjectile_b19f91a0e38b40f38934563b64d4062b : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1903222785)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)5);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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
	}

	public class Player_EACServer
	{
		[Patch("OnPlayerBanned", "OnPlayerBanned [EAC]", "EACServer", "OnClientActionRequired", new string[] { "Epic.OnlineServices.AntiCheatCommon.OnClientActionRequiredCallbackInfo&" })]
		[Identifier("37156842fdf94ccabcedbc5d12b3001f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local2", "Network.Connection", false)]
		[Parameter("toString()", "System.String", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_EACServer_37156842fdf94ccabcedbc5d12b3001f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 89)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)140408349), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Epic.OnlineServices.Utf8String"), "ToString", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnPlayerKicked", "OnPlayerKicked [EAC]", "EACServer", "OnClientActionRequired", new string[] { "Epic.OnlineServices.AntiCheatCommon.OnClientActionRequiredCallbackInfo&" })]
		[Identifier("fdb1f3b68b73408aabc2122156c369ef")]
		[Dependencies(new string[] { "OnPlayerBanned [EAC]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local2", "Network.Connection", false)]
		[Parameter("toString()", "System.String", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_EACServer_fdb1f3b68b73408aabc2122156c369ef : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 56)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1321158727), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Epic.OnlineServices.Utf8String"), "ToString", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Player_BasePortal
	{
		[Patch("OnPortalUse", "OnPortalUse", "BasePortal", "UsePortal", new string[] { "BasePlayer" })]
		[Identifier("02806eed74d2422db1b053f1c65357e3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BasePortal", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePortal_02806eed74d2422db1b053f1c65357e3 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1234683421)), instruction), instruction);
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

		[Patch("OnPortalUsed", "OnPortalUsed", "BasePortal", "UsePortal", new string[] { "BasePlayer" })]
		[Identifier("4613711e012c4f30a54e2367814aaba9")]
		[Dependencies(new string[] { "OnPortalUse" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BasePortal", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePortal_4613711e012c4f30a54e2367814aaba9 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 148)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1651465834), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Player_ModularCarCodeLock
	{
		[Patch("CanUnlock", "CanUnlock [ModularCarCodeLock]", "ModularCarCodeLock", "TryOpenWithCode", new string[] { "BasePlayer", "System.String" })]
		[Identifier("630a7da3a7f34c79aa13e9a27cbdf467")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ModularCarCodeLock", false)]
		[Parameter("codeEntered", "System.String", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ModularCarCodeLock_630a7da3a7f34c79aa13e9a27cbdf467 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2118405101), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
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
	}

	public class Player_BaseDiggableEntity
	{
		[Patch("OnPlayerDig", "OnPlayerDig", "BaseDiggableEntity", "Dig", new string[] { "BasePlayer" })]
		[Identifier("b2b3bb660a4b49c2b5e8341248f1bcc6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BaseDiggableEntity", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseDiggableEntity_b2b3bb660a4b49c2b5e8341248f1bcc6 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1089357290), instruction), instruction);
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

		[Patch("OnPlayerDigComplete", "OnPlayerDigComplete", "BaseDiggableEntity", "OnFullyDug", new string[] { "BasePlayer" })]
		[Identifier("28e6477690dd446e89a694513018ef28")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BaseDiggableEntity", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseDiggableEntity_28e6477690dd446e89a694513018ef28 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2006241324)), instruction), instruction);
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

	public class Player_MetalDetectorFlag
	{
		[Patch("OnPlayerDigComplete", "OnPlayerDigComplete [MetalDetectorFlag]", "MetalDetectorFlag", "OnFullyDug", new string[] { "BasePlayer" })]
		[Identifier("12c3bb2e526b44ebb8bb510b7b947b4f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "MetalDetectorFlag", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_MetalDetectorFlag_12c3bb2e526b44ebb8bb510b7b947b4f : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2006241324)), instruction), instruction);
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

	public class Player_BaseMetalDetector
	{
		[Patch("OnMetalDetectorFlagRequest", "OnMetalDetectorFlagRequest", "BaseMetalDetector", "RPC_RequestFlag", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("7fa6d226b1094a1484e39ea8468a4fd2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMetalDetector", false)]
		[Parameter("local1", "UnityEngine.Vector3", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMetalDetector_7fa6d226b1094a1484e39ea8468a4fd2 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 20)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1065397392), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("UnityEngine.Vector3"));
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Player_HBHFSensor
	{
		[Patch("CanUseHBHFSensor", "CanUseHBHFSensor", "HBHFSensor", "CanUse", new string[] { "BasePlayer" })]
		[Identifier("de68b968629a4def8978698a57e6ca06")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "HBHFSensor", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_HBHFSensor_de68b968629a4def8978698a57e6ca06 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1634954413), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
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
	}

	public class Player_Handcuffs
	{
		[Patch("OnPlayerHandcuff", "OnPlayerHandcuff", "Handcuffs", "SV_HandcuffVictim", new string[] { "BasePlayer", "BasePlayer" })]
		[Identifier("d130b9ee02b14665a4cab7db51590308")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Handcuffs_d130b9ee02b14665a4cab7db51590308 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 37)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1370523975), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
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

		[Patch("OnPlayerHandcuffed", "OnPlayerHandcuffed", "Handcuffs", "SV_HandcuffVictim", new string[] { "BasePlayer", "BasePlayer" })]
		[Identifier("8a5c1855d1aa4aaaa7c0ff6ec3a9e684")]
		[Dependencies(new string[] { "OnPlayerHandcuff" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Handcuffs_8a5c1855d1aa4aaaa7c0ff6ec3a9e684 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 164)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)799421953), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Player_ConVarDebugging
	{
		[Patch("OnPlayerVanish", "OnPlayerVanish", "ConVar.Debugging", "invis", new string[] { "ConsoleSystem/Arg" })]
		[Identifier("47231fddc1894a93ac808b758d8bae78")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConVarDebugging_47231fddc1894a93ac808b758d8bae78 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 21)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-580625165)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnPlayerVanished", "OnPlayerVanished", "ConVar.Debugging", "invis", new string[] { "ConsoleSystem/Arg" })]
		[Identifier("3ff98584a4f24d76acabb9574103df7c")]
		[Dependencies(new string[] { "OnPlayerVanish" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConVarDebugging_3ff98584a4f24d76acabb9574103df7c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 81)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2084189832), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnPlayerUnvanish", "OnPlayerUnvanish", "ConVar.Debugging", "invis", new string[] { "ConsoleSystem/Arg" })]
		[Identifier("5084cfe593964b79a53b3b6950cc10c7")]
		[Dependencies(new string[] { "OnPlayerVanished" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConVarDebugging_5084cfe593964b79a53b3b6950cc10c7 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 91)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1836626967), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnPlayerUnvanished", "OnPlayerUnvanished", "ConVar.Debugging", "invis", new string[] { "ConsoleSystem/Arg" })]
		[Identifier("2b1ae8ecf429401f88ae0e1c77b0e0ab")]
		[Dependencies(new string[] { "OnPlayerUnvanish" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConVarDebugging_2b1ae8ecf429401f88ae0e1c77b0e0ab : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 144)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1457859582), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnPlayerUnvanished", "OnPlayerUnvanished [Patch]", "ConVar.Debugging", "invis", new string[] { "ConsoleSystem/Arg" })]
		[Identifier("097afa1562be4db58a8fabb81fcf1583")]
		[Dependencies(new string[] { "OnPlayerUnvanished" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConVarDebugging_097afa1562be4db58a8fabb81fcf1583 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Expected O, but got Unknown
				//IL_0059: Unknown result type (might be due to invalid IL or missing references)
				//IL_0063: Expected O, but got Unknown
				//IL_0095: Unknown result type (might be due to invalid IL or missing references)
				//IL_009f: Expected O, but got Unknown
				//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b5: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[148];
				list.Add(new CodeInstruction(OpCodes.Br, (object)label));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Brtrue, (object)label));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer"), "isInvisible")));
				list.Add(new CodeInstruction(OpCodes.Brfalse, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[85]), list2[85]);
				}
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[86].labels);
				}
				else
				{
					list2[91].labels.AddRange(list2[86].labels);
				}
				list2[86].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[87].labels);
				}
				else
				{
					list2[91].labels.AddRange(list2[87].labels);
				}
				list2[87].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[88].labels);
				}
				else
				{
					list2[91].labels.AddRange(list2[88].labels);
				}
				list2[88].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[89].labels);
				}
				else
				{
					list2[91].labels.AddRange(list2[89].labels);
				}
				list2[89].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[90].labels);
				}
				else
				{
					list2[91].labels.AddRange(list2[90].labels);
				}
				list2[90].labels.Clear();
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[91], list2[85]), list2[85]);
				}
				list2.RemoveRange(85, 6);
				list2.InsertRange(85, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Player_RFTimedExplosive
	{
		[Patch("ICanPickupEntity", "ICanPickupEntity [RFTimedExplosive]", "RFTimedExplosive", "Pickup", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("862c977500234e1cbb712ed0e3ca88ad")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "RFTimedExplosive", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_RFTimedExplosive_862c977500234e1cbb712ed0e3ca88ad : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldarg_1, (object)null), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "ICanPickupEntity", (Type[])null, (Type[])null));
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

	public class Player_BasePlayerOnFeedbackReportd772
	{
		[Patch("OnFeedbackReported", "OnFeedbackReported", "BasePlayer/<OnFeedbackReport>d__772", "MoveNext", new string[] { })]
		[Identifier("2cb69d48ea7343db810eec43a9fd548e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "BasePlayer", false)]
		[Parameter("local2", "System.String", false)]
		[Parameter("local3", "System.String", false)]
		[Parameter("local5", "Facepunch.Models.ReportType", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayerOnFeedbackReportd772_2cb69d48ea7343db810eec43a9fd548e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 87)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1371113424), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("Facepunch.Models.ReportType"));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
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
	}

	public class Player_BasePlayerOnPlayerReportedd771
	{
		[Patch("OnPlayerReported", "OnPlayerReported", "BasePlayer/<OnPlayerReported>d__771", "MoveNext", new string[] { })]
		[Identifier("4f2f9653585a47279c71b7d7bab3ab37")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "BasePlayer", false)]
		[Parameter("local6", "System.String", false)]
		[Parameter("self", "BasePlayer+<OnPlayerReported>d__771", false)]
		[Parameter("local2", "System.String", false)]
		[Parameter("local3", "System.String", false)]
		[Parameter("local5", "System.String", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayerOnPlayerReportedd771_4f2f9653585a47279c71b7d7bab3ab37 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 96)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1491190051), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)6);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer+<OnPlayerReported>d__771"), "<targetId>5__2"));
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[7]
					{
						typeof(uint),
						typeof(object),
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
	}

	public class Player_SteamInventoryUpdateSteamInventoryd9
	{
		[Patch("OnSteamInventoryUpdated", "OnSteamInventoryUpdated", "SteamInventory/<UpdateSteamInventory>d__9", "MoveNext", new string[] { })]
		[Identifier("62657f16d2a54a7e851c9ac43546ba23")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "SteamInventory", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_SteamInventoryUpdateSteamInventoryd9_62657f16d2a54a7e851c9ac43546ba23 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 95)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-614735655)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

	public class Player_BaseEntity
	{
		[Patch("OnSignalBroadcast", "OnSignalBroadcast", "BaseEntity", "SignalBroadcast", new string[] { "BaseEntity/Signal", "System.String", "Network.Connection" })]
		[Identifier("5c813aa1fc6741cfaee380e75e2a88f7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseEntity", false)]
		[Parameter("sourceConnection", "Network.Connection", false)]
		[Parameter("signal", "BaseEntity+Signal", false)]
		[Parameter("arg", "System.String", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseEntity_5c813aa1fc6741cfaee380e75e2a88f7 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 9)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-736926220)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(Signal));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
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
	}

	public class Player_ItemModConsume
	{
		[Patch("OnPlayerAddModifiers", "OnPlayerAddModifiers", "ItemModConsume", "DoAction", new string[] { "Item", "BasePlayer" })]
		[Identifier("7810361ab8254ee2a67565c2cef5c3b5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ItemModConsume_7810361ab8254ee2a67565c2cef5c3b5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected O, but got Unknown
				//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bb: Expected O, but got Unknown
				//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cc: Expected O, but got Unknown
				//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f4: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnPlayerAddModifiers"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_2, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[4]
				{
					typeof(string),
					typeof(object),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Ldnull, (object)null));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[199];
				list.Add(new CodeInstruction(OpCodes.Bne_Un_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[192]), list2[192]);
				}
				list2.InsertRange(192, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Player_AuthCentralizedBansRund0
	{
		[Patch("OnCentralizedBanCheck", "OnCentralizedBanCheck", "Auth_CentralizedBans/<Run>d__0", "MoveNext", new string[] { })]
		[Identifier("92159297da554200b5be75eb296d0431")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_AuthCentralizedBansRund0_92159297da554200b5be75eb296d0431 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0057: Expected O, but got Unknown
				//IL_0092: Unknown result type (might be due to invalid IL or missing references)
				//IL_009c: Expected O, but got Unknown
				//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00dd: Expected O, but got Unknown
				//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
				//IL_0101: Expected O, but got Unknown
				//IL_0108: Unknown result type (might be due to invalid IL or missing references)
				//IL_0112: Expected O, but got Unknown
				//IL_0119: Unknown result type (might be due to invalid IL or missing references)
				//IL_0123: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnCentralizedBanCheck"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("Auth_CentralizedBans/<Run>d__0"), "connection")));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[2]
				{
					typeof(string),
					typeof(object)
				}, (Type[])null)));
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 3, typeof(object)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 3, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldnull, (object)null));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[0];
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldc_I4_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[0]), list2[0]);
				}
				list2.InsertRange(0, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Player_IndustrialCrafter
	{
		[Patch("CanLootEntity", "CanLootEntity [IndustrialCrafter]", "IndustrialCrafter", "PlayerOpenLoot", new string[] { "BasePlayer", "System.String", "System.Boolean" })]
		[Identifier("8a382887beb840d08a705a02993198d1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_IndustrialCrafter_8a382887beb840d08a705a02993198d1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected O, but got Unknown
				//IL_008c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0096: Expected O, but got Unknown
				//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ba: Expected O, but got Unknown
				//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cb: Expected O, but got Unknown
				//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00dc: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"CanLootEntity"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[0];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldc_I4_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[0]), list2[0]);
				}
				list2.InsertRange(0, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Player_ConVarChatsayAsd22
	{
		[Patch("IOnPlayerChat[patch2]", "IOnPlayerChat[patch2]", "ConVar.Chat/<sayAs>d__22", "MoveNext", new string[] { })]
		[Identifier("3030746b8c4e4c6e9739d9bf5d4474ed")]
		[Dependencies(new string[] { "IOnPlayerChat[patch]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConVarChatsayAsd22_3030746b8c4e4c6e9739d9bf5d4474ed : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[108];
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[103]), list2[103]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[104], list2[103]), list2[103]);
				}
				list2.RemoveRange(103, 1);
				list2.InsertRange(103, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Player_NetworkServer
	{
		[Patch("OnClientDisconnected", "OnClientDisconnected", "Network.Server", "OnDisconnected", new string[] { "System.String", "Network.Connection" })]
		[Identifier("d5f13b7b0e1c40b99d1f2d5925fc5b82")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("cn", "Network.Connection", false)]
		[Parameter("strReason", "System.String", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Player")]
		[Assembly("Facepunch.Network.dll")]
		public class Player_NetworkServer_d5f13b7b0e1c40b99d1f2d5925fc5b82 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 17)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1071622507)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}
}
