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
		[Identifier("08630086b2bd4bb89a547683e52df07c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("strReason", "System.String", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_08630086b2bd4bb89a547683e52df07c : Patch
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
		[Identifier("5c979dea27d34038ac9e23e0f58a8cb4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("connection", "Network.Connection", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_5c979dea27d34038ac9e23e0f58a8cb4 : Patch
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
		[Identifier("6946c35ffb6c44a0978b588f5acf494f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("connection", "Network.Connection", false)]
		[Parameter("name", "System.String", false)]
		[Parameter("value", "System.String", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_6946c35ffb6c44a0978b588f5acf494f : Patch
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
		[Identifier("a078478a84604179842a536b59447ffa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "Network.Connection", false)]
		[Parameter("status", "AuthResponse", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_a078478a84604179842a536b59447ffa : Patch
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
		[Identifier("bb13aa0fdfce438189a5ce264f8dc800")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(SpawnPoint))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_bb13aa0fdfce438189a5ce264f8dc800 : Patch
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
		[Identifier("f6f831b3885f42389cd28b42f8d76ebe")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("connection", "Network.Connection", false)]
		[Parameter("local0", "System.String", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_f6f831b3885f42389cd28b42f8d76ebe : Patch
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
		[Identifier("1845bed1863b4ac08dc18e861a0af7bc")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.ArraySegment`1[System.Byte]", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_1845bed1863b4ac08dc18e861a0af7bc : Patch
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
		[Identifier("94c5c29229da4cf0a1f0a18260a54c83")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ServerMgr_94c5c29229da4cf0a1f0a18260a54c83 : Patch
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
		[Identifier("f9fce6d74f4f45f7ab13f020ff8112ec")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CodeLock", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_CodeLock_f9fce6d74f4f45f7ab13f020ff8112ec : Patch
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
		[Identifier("1fee12ea7420426084d425c203378716")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CodeLock", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_CodeLock_1fee12ea7420426084d425c203378716 : Patch
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
		[Identifier("1d86d527ab094c7aae6b94d0dcb4279a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CodeLock", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_CodeLock_1d86d527ab094c7aae6b94d0dcb4279a : Patch
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
		[Identifier("ad1cb4016c2f4f05b60779fc6a11ceb2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CodeLock", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_CodeLock_ad1cb4016c2f4f05b60779fc6a11ceb2 : Patch
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
		[Identifier("204451c47b5d431887d355d7b2ec4a29")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CodeLock", false)]
		[Parameter("local0", "System.String", false)]
		[Parameter("local1", "System.Boolean", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_CodeLock_204451c47b5d431887d355d7b2ec4a29 : Patch
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
		[Identifier("ae56285b0e3b4b0091e082d9e10fb50b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "KeyLock", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_KeyLock_ae56285b0e3b4b0091e082d9e10fb50b : Patch
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
		[Identifier("8fa32643c7774f2da6881d3285f8d6d0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "KeyLock", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_KeyLock_8fa32643c7774f2da6881d3285f8d6d0 : Patch
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
		[Identifier("f88d3bb8c80a4be7be86a93777539e9a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "KeyLock", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_KeyLock_f88d3bb8c80a4be7be86a93777539e9a : Patch
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
		[Identifier("0e0a4c01cbe54af294646dd709237230")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "KeyLock", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_KeyLock_0e0a4c01cbe54af294646dd709237230 : Patch
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
		[Identifier("e612fff0da804ef9863fefc4fd9b6f55")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerLoot", false)]
		[Parameter("targetEntity", "BaseEntity", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerLoot_e612fff0da804ef9863fefc4fd9b6f55 : Patch
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
		[Identifier("855485fc639f4093aecab5b9d1bfbb98")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerLoot", false)]
		[Parameter("item", "Item", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerLoot_855485fc639f4093aecab5b9d1bfbb98 : Patch
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
		[Identifier("994abbdd84844ac19e02ccda34eaccd0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerLoot", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerLoot_994abbdd84844ac19e02ccda34eaccd0 : Patch
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
		[Identifier("b185a26736294b4e9ab0e33377b5d2e0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerLoot", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerLoot_b185a26736294b4e9ab0e33377b5d2e0 : Patch
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
		[Identifier("3c6f7d64c0b044a39f507d92f7a634a5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMelee", false)]
		[Parameter("info", "HitInfo", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMelee_3c6f7d64c0b044a39f507d92f7a634a5 : Patch
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
		[Identifier("5b3d3c30f08c496aad97b726eb8d0c16")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local3", "HitInfo", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMelee_5b3d3c30f08c496aad97b726eb8d0c16 : Patch
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
		[Identifier("36bfb825e9134b43896ad12327a878b6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("hitInfo", "BasePlayer+<>c__DisplayClass458_0", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_36bfb825e9134b43896ad12327a878b6 : Patch
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
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer+<>c__DisplayClass458_0"), "hitInfo"));
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
		[Identifier("eb9ac7dbe1de4564bee81a09b5a15699")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_eb9ac7dbe1de4564bee81a09b5a15699 : Patch
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
		[Identifier("e288d354c4874209944f022564f38590")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_e288d354c4874209944f022564f38590 : Patch
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
		[Identifier("b66276f6fe714fccb1e446f25d77e8ef")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_b66276f6fe714fccb1e446f25d77e8ef : Patch
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
		[Identifier("b5db891a62d74b0a918b87048b63c8f4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_b5db891a62d74b0a918b87048b63c8f4 : Patch
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
		[Identifier("5837e021b831485784802ff071308b26")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_5837e021b831485784802ff071308b26 : Patch
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
		[Identifier("be4fef08515e476196237a720a012ea3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_be4fef08515e476196237a720a012ea3 : Patch
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
		[Identifier("ce754e156a244736992b32bcc1ba6e8b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_ce754e156a244736992b32bcc1ba6e8b : Patch
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
		[Identifier("d6c9db59db5e4cf49844192b9c2d4223")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_d6c9db59db5e4cf49844192b9c2d4223 : Patch
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
		[Identifier("33fb3e4740a64e7fa0cdd094a2428d73")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_33fb3e4740a64e7fa0cdd094a2428d73 : Patch
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
		[Identifier("aa163b7dee7b40a7ba71a0d497d1606c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("oldvalue", "System.Single", false)]
		[Parameter("newvalue", "System.Single", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_aa163b7dee7b40a7ba71a0d497d1606c : Patch
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
		[Identifier("4714f3fcdf1c498986a93aedd604b036")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_4714f3fcdf1c498986a93aedd604b036 : Patch
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
		[Identifier("a2b7f3b7f8f441f4a93f7dda584cbf58")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("info", "HitInfo", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_a2b7f3b7f8f441f4a93f7dda584cbf58 : Patch
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
		[Identifier("57612c0f93504dd2901a2388c05d3316")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "BasePlayer+SpawnPoint", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_57612c0f93504dd2901a2388c05d3316 : Patch
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
		[Identifier("9d19e91cddd8410aac2141a0953801c8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_9d19e91cddd8410aac2141a0953801c8 : Patch
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
		[Identifier("097af9e238a34486a4eee2c4881afc34")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_097af9e238a34486a4eee2c4881afc34 : Patch
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
		[Identifier("963d471296a74d2582bf804a66c9d215")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local1", "Item", false)]
		[Parameter("itemID", "ItemId", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_963d471296a74d2582bf804a66c9d215 : Patch
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
		[Identifier("7016d24de277458a98a15744cfd3a14b")]
		[Dependencies(new string[] { "OnPlayerTick" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_7016d24de277458a98a15744cfd3a14b : Patch
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
		[Identifier("ffa981ed4c8a4a38807a9bc03b0f41f2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_ffa981ed4c8a4a38807a9bc03b0f41f2 : Patch
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
		[Identifier("8cbdc7fc31e34d39b2552678bba04a53")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "System.Single", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_8cbdc7fc31e34d39b2552678bba04a53 : Patch
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
		[Identifier("2ba7f506f25d44818cf452658801e66f")]
		[Dependencies(new string[] { "OnPlayerLand" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "System.Single", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_2ba7f506f25d44818cf452658801e66f : Patch
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

		[Patch("CanSpectateTarget", "CanSpectateTarget", "BasePlayer", "UpdateSpectateTarget", new string[] { "System.String", "System.Boolean" })]
		[Identifier("82b85ac399ad4aa7ad74fccb0e5c8f20")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("strName", "System.String", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_82b85ac399ad4aa7ad74fccb0e5c8f20 : Patch
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
		[Identifier("28dd6c78200e471691b38f0436d21200")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("key", "System.String", false)]
		[Parameter("val", "System.String", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_28dd6c78200e471691b38f0436d21200 : Patch
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
		[Identifier("7b3953128a1840e1b8ecd3bad0da1333")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_7b3953128a1840e1b8ecd3bad0da1333 : Patch
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
		[Identifier("fbe5f2b03139414481b58af9414cd4ee")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_fbe5f2b03139414481b58af9414cd4ee : Patch
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
		[Identifier("3cadbcdabf7c4e98a1b7862522b28394")]
		[Dependencies(new string[] { "OnActiveItemChange" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local1", "Item", false)]
		[Parameter("local2", "Item", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_3cadbcdabf7c4e98a1b7862522b28394 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 82)
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
		[Identifier("72e599de7d844658af6ab3aacd639566")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_72e599de7d844658af6ab3aacd639566 : Patch
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
		[Identifier("a46caba030364292b8339ecca3465a26")]
		[Dependencies(new string[] { "OnMapMarkersClear" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_a46caba030364292b8339ecca3465a26 : Patch
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
		[Identifier("79ed10f5dfb14f369f721920dd25940f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_79ed10f5dfb14f369f721920dd25940f : Patch
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
		[Identifier("6bbf882de9f54a3eb3f1acca33828afd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_6bbf882de9f54a3eb3f1acca33828afd : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 223)
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
		[Identifier("6535b773ce954e44aea0264eafe7bd86")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Parameter("local0", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_6535b773ce954e44aea0264eafe7bd86 : Patch
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
		[Identifier("edb7b11440a5470384a4ca89a3e6cc57")]
		[Dependencies(new string[] { "OnMapMarkerAdd [patch]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "ProtoBuf.MapNote", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_edb7b11440a5470384a4ca89a3e6cc57 : Patch
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
		[Identifier("7b38d824153142d2888fa44e0617d504")]
		[Dependencies(new string[] { "OnPlayerCorpseSpawn" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local3", "PlayerCorpse", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_7b38d824153142d2888fa44e0617d504 : Patch
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
		[Identifier("37f8687ddc4d4396b747fdcc8e02099d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "System.String", false)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_37f8687ddc4d4396b747fdcc8e02099d : Patch
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
		[Identifier("2065125406224788a8777df1d2b41cc5")]
		[Dependencies(new string[] { "OnDemoRecordingStart" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "System.String", false)]
		[Parameter("self", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_2065125406224788a8777df1d2b41cc5 : Patch
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
		[Identifier("f6bac91fc55b411d98fe66acad536eda")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_f6bac91fc55b411d98fe66acad536eda : Patch
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
		[Identifier("1ace5243642141deb342119dcfb08ee9")]
		[Dependencies(new string[] { "OnDemoRecordingStop" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_1ace5243642141deb342119dcfb08ee9 : Patch
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
		[Identifier("43d5f008cb3449378ea47c8ad94ea843")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_43d5f008cb3449378ea47c8ad94ea843 : Patch
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
		[Identifier("f2e8d09cbce04a36b7e2db70045c08e3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "System.Collections.Generic.List`1[ProtoBuf.RespawnInformation+SpawnOptions]", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_f2e8d09cbce04a36b7e2db70045c08e3 : Patch
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
		[Identifier("43822ebfbeea4d5c8e5e29272858cfb7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_43822ebfbeea4d5c8e5e29272858cfb7 : Patch
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
		[Identifier("d9b07f74c81046148fd6fbd99a79299b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("info", "HitInfo", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_d9b07f74c81046148fd6fbd99a79299b : Patch
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
		[Identifier("3e7ce898f46f4ae29cf8b44f568a2f99")]
		[Dependencies(new string[] { "OnPlayerRecover" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_3e7ce898f46f4ae29cf8b44f568a2f99 : Patch
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
		[Identifier("a0c931132ea3478bbaa964aedaed0b9b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("self1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_a0c931132ea3478bbaa964aedaed0b9b : Patch
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
		[Identifier("b16f5d60f03f4786a8072411a9db885d")]
		[Dependencies(new string[] { "OnPlayerSleepEnd" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_b16f5d60f03f4786a8072411a9db885d : Patch
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
		[Identifier("9bebcebd68b645e7a0fa2af8192c0cee")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "ProtoBuf.MapNoteList", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_9bebcebd68b645e7a0fa2af8192c0cee : Patch
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
		[Identifier("87e55da737ff4ef0b9349b43dc8d32fc")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "ProtoBuf.MapNoteList", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_87e55da737ff4ef0b9349b43dc8d32fc : Patch
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
		[Identifier("acd46773cf9d43e49d0f7fa84c12f797")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_acd46773cf9d43e49d0f7fa84c12f797 : Patch
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
		[Identifier("cf357abd84b44faa8de04812fe3d1c03")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("fromPlayer", "BasePlayer", false)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_cf357abd84b44faa8de04812fe3d1c03 : Patch
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
		[Identifier("96e8a60ad07f4069b123d542b284b1af")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_96e8a60ad07f4069b123d542b284b1af : Patch
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
		[Identifier("d0bdce44a1ed4276b680005762a81cee")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("mainland", "System.Boolean", false)]
		[Parameter("deepSea", "System.Boolean", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_d0bdce44a1ed4276b680005762a81cee : Patch
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
		[Identifier("92a90a5e0512433e982a1a6f391f3770")]
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
		public class Player_BasePlayer_92a90a5e0512433e982a1a6f391f3770 : Patch
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
		[Identifier("5824d0f7e4b345baa461da847818cb5d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_5824d0f7e4b345baa461da847818cb5d : Patch
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
		[Identifier("89385d31bfc04ef69d15e1ee68a9f244")]
		[Dependencies(new string[] { "OnMapMarkerAdd" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_89385d31bfc04ef69d15e1ee68a9f244 : Patch
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
		[Identifier("b8d685984570492ebabbd3878dbcf162")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_b8d685984570492ebabbd3878dbcf162 : Patch
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
		[Identifier("cdbc64c74ff148bcbf4c64158defd34f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayer_cdbc64c74ff148bcbf4c64158defd34f : Patch
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
		[Identifier("46c375bb40e2435398a2f89e24fe6797")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerMetabolism", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerMetabolism_46c375bb40e2435398a2f89e24fe6797 : Patch
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
		[Identifier("a71c8e813d984f71921a29e4c06c54a2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerMetabolism", false)]
		[Parameter("ownerEntity", "BaseCombatEntity", false)]
		[Parameter("delta", "System.Single", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerMetabolism_a71c8e813d984f71921a29e4c06c54a2 : Patch
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
		[Identifier("afdf12fc80b4454fa2014340aebc0ab7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("connection", "Network.Connection", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConnectionAuth_afdf12fc80b4454fa2014340aebc0ab7 : Patch
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
		[Identifier("064f7c23c69348c39c3e5f757c3cbd32")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "Signage", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Signage_064f7c23c69348c39c3e5f757c3cbd32 : Patch
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
		[Identifier("16b734d914e149a1bd16c61294017547")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "Item", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ResearchTable_16b734d914e149a1bd16c61294017547 : Patch
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
		[Identifier("4fae2a92811d470498cea802c558e5d8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("obj", "BaseCombatEntity", false)]
		[Parameter("self", "AutoTurret", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_AutoTurret_4fae2a92811d470498cea802c558e5d8 : Patch
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
		[Identifier("657fbb51514746d996b3a87342a3df41")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("potentialtarget", "BaseCombatEntity", false)]
		[Parameter("self", "HelicopterTurret", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_HelicopterTurret_657fbb51514746d996b3a87342a3df41 : Patch
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
		[Identifier("0b5a6f7fbdf34135b689609b0a5f8e6f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "LootableCorpse", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_LootableCorpse_0b5a6f7fbdf34135b689609b0a5f8e6f : Patch
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
		[Identifier("7fe2f623c3a94726900d63155e108eb8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("self", "LootableCorpse", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_LootableCorpse_7fe2f623c3a94726900d63155e108eb8 : Patch
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
		[Identifier("d145b8aa004e4c949ff5657150385ace")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "StorageContainer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_StorageContainer_d145b8aa004e4c949ff5657150385ace : Patch
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
		[Identifier("101dfe9d373d4e79bdfd4a478c3f854d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_StorageContainer_101dfe9d373d4e79bdfd4a478c3f854d : Patch
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
		[Identifier("0a076f3ba5b24b27a1c090bc3bc639c6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConnectionQueue_0a076f3ba5b24b27a1c090bc3bc639c6 : Patch
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
		[Identifier("a3db863563404faea5b41d52de89e516")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local7", "BasePlayer", false)]
		[Parameter("self", "FlameTurret", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_FlameTurret_a3db863563404faea5b41d52de89e516 : Patch
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
		[Identifier("6b9cfbb9781f4e7b8e7421568e020771")]
		[Dependencies(new string[] { "CanBeTargeted [FlameTurret]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_FlameTurret_6b9cfbb9781f4e7b8e7421568e020771 : Patch
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
		[Identifier("8654821010a74b7a82a43002df41c739")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BaseCombatEntity", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseCombatEntity_8654821010a74b7a82a43002df41c739 : Patch
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
		[Identifier("147b505069e140aab46e29e85a3e8827")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "SleepingBag", false)]
		[Parameter("local0", "System.UInt64", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_SleepingBag_147b505069e140aab46e29e85a3e8827 : Patch
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
		[Identifier("530aa0d9e3e2479683293872c118acc1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "SleepingBag", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_SleepingBag_530aa0d9e3e2479683293872c118acc1 : Patch
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
		[Identifier("e1784e3096a0476e809b1332bc0b16b9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "SleepingBag", false)]
		[Parameter("local0", "System.String", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_SleepingBag_e1784e3096a0476e809b1332bc0b16b9 : Patch
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
		[Identifier("a3e7394d002f4aa586592b2a7e34204e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local1", "SleepingBag", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_SleepingBag_a3e7394d002f4aa586592b2a7e34204e : Patch
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
		[Identifier("709bb6b10b654146b8fd20500af46f3b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "StashContainer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_StashContainer_709bb6b10b654146b8fd20500af46f3b : Patch
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
		[Identifier("cc7b4b5cf2644c6a8ae2b6f5102036d8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_AntiHack_cc7b4b5cf2644c6a8ae2b6f5102036d8 : Patch
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
		[Identifier("9150ff5cdc434ebf8e94eaf185c1cdfd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "Mailbox", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Mailbox_9150ff5cdc434ebf8e94eaf185c1cdfd : Patch
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
		[Identifier("8fd67ddc1a0f4c20b3df7dc61c91a20a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "SpinnerWheel", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_SpinnerWheel_8fd67ddc1a0f4c20b3df7dc61c91a20a : Patch
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
		[Identifier("eb5ec34e2a004d43822f90ddc0bb64a9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local7", "BasePlayer", false)]
		[Parameter("self", "GunTrap", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_GunTrap_eb5ec34e2a004d43822f90ddc0bb64a9 : Patch
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
		[Identifier("20db3fd57aea4792b733a026cb79b7fe")]
		[Dependencies(new string[] { "CanBeTargeted [GunTrap]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_GunTrap_20db3fd57aea4792b733a026cb79b7fe : Patch
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
		[Identifier("4b13ef0f7dfc4257b1dde38572e0a504")]
		[Dependencies(new string[] { "CanBeTargeted [GunTrap] [patch]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_GunTrap_4b13ef0f7dfc4257b1dde38572e0a504 : Patch
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
		[Identifier("3d5fdee69fd848b594c5c49ad540922e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BaseLock", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseLock_3d5fdee69fd848b594c5c49ad540922e : Patch
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
		[Identifier("b89af43e0ddf4dd99f913797d86a0dbb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BaseMountable", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMountable_b89af43e0ddf4dd99f913797d86a0dbb : Patch
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
		[Identifier("381198ce353c4a5c968989cca24f9a62")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BaseMountable", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMountable_381198ce353c4a5c968989cca24f9a62 : Patch
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
		[Identifier("bd63d8d0bddf458790eca6c9ff17b4cf")]
		[Dependencies(new string[] { "CanMountEntity" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMountable", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMountable_bd63d8d0bddf458790eca6c9ff17b4cf : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 92)
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
		[Identifier("87ca545874ef4deda408ae12054f1496")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("self", "BaseMountable", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMountable_87ca545874ef4deda408ae12054f1496 : Patch
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
		[Identifier("2abc3e9d942c4678ad30cd34836d4206")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BaseMountable", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMountable_2abc3e9d942c4678ad30cd34836d4206 : Patch
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
		[Identifier("daceeb8e25bd4eedb0b2729535b89e36")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BaseMountable", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMountable_daceeb8e25bd4eedb0b2729535b89e36 : Patch
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
		[Identifier("03e08e86409d40628b7d64c9d6165889")]
		[Dependencies(new string[] { "OnPlayerWantsDismount" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("self", "BaseMountable", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMountable_03e08e86409d40628b7d64c9d6165889 : Patch
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
		[Identifier("bb991658a8434148b00c35c76a248dfd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerBelt", false)]
		[Parameter("local0", "Shield", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerBelt_bb991658a8434148b00c35c76a248dfd : Patch
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
		[Identifier("1c1cf700289f493d833242d5d1071ec1")]
		[Dependencies(new string[] { "OnPlayerActiveShieldDrop" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerBelt", false)]
		[Parameter("local1", "Item", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerBelt_1c1cf700289f493d833242d5d1071ec1 : Patch
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
		[Identifier("fc93bfbf621f404fa5e39522c19fcdc5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("self", "ResourceContainer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ResourceContainer_fc93bfbf621f404fa5e39522c19fcdc5 : Patch
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
		[Identifier("e82ac6be2b17406e914106324fb93857")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "DroppedItemContainer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_DroppedItemContainer_e82ac6be2b17406e914106324fb93857 : Patch
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
		[Identifier("1eefb2a775ae40039c522e1917c92a13")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "DroppedItemContainer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_DroppedItemContainer_1eefb2a775ae40039c522e1917c92a13 : Patch
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
		[Identifier("adb51998b1794da1a36329b6931946f8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "HackableLockedCrate", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_HackableLockedCrate_adb51998b1794da1a36329b6931946f8 : Patch
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
		[Identifier("7db48bc3dafc43949c7474fbd60681ce")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Workbench", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Workbench_7db48bc3dafc43949c7474fbd60681ce : Patch
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
		[Identifier("de2a2363172348d79317326a6b30c679")]
		[Dependencies(new string[] { "OnExperimentStart" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Workbench", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Workbench_de2a2363172348d79317326a6b30c679 : Patch
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
		[Identifier("9085dd8ddb64452ea1afe7698c70c14e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Workbench", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Workbench_9085dd8ddb64452ea1afe7698c70c14e : Patch
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
		[Identifier("836b85933b7b4a9badd7e8af413735c8")]
		[Dependencies(new string[] { "OnExperimentEnd" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Workbench", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Workbench_836b85933b7b4a9badd7e8af413735c8 : Patch
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
		[Identifier("a1fe23cc26604067a14f2664cec6abb5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "DoorCloser", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_DoorCloser_a1fe23cc26604067a14f2664cec6abb5 : Patch
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
		[Identifier("fec79ce42102412485b17946f982843c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "GrowableEntity", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_GrowableEntity_fec79ce42102412485b17946f982843c : Patch
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
		[Identifier("8c5ca42adf8e4a11991d4721c30f26b4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BuildingBlock", false)]
		[Parameter("g", "ConstructionGrade", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BuildingBlock_8c5ca42adf8e4a11991d4721c30f26b4 : Patch
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
		[Identifier("094f0a98baeb4311abf2da2d5029a9df")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "Planner", false)]
		[Parameter("component", "Construction", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Planner_094f0a98baeb4311abf2da2d5029a9df : Patch
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
		[Identifier("5dcfd10c32124b0588be33601b57ad53")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WallpaperPlanner", false)]
		[Parameter("component", "Construction", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_WallpaperPlanner_5dcfd10c32124b0588be33601b57ad53 : Patch
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
		[Identifier("efb35392584746cea314138359e84e76")]
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
		public class Player_WireTool_efb35392584746cea314138359e84e76 : Patch
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
		[Identifier("5e9076a2d98b4b499d5eab9b576cf67e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_WireTool_5e9076a2d98b4b499d5eab9b576cf67e : Patch
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
		[Identifier("4e00a2c3afc74e91b10b5a614c0c379c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("item", "Item", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ItemModStudyBlueprint_4e00a2c3afc74e91b10b5a614c0c379c : Patch
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
		[Identifier("4e5cbd1131fe46489b44df95d9213de2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("self", "RidableHorse", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_RidableHorse_4e5cbd1131fe46489b44df95d9213de2 : Patch
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
		[Identifier("30fa7aa143cf4ddaa97b77113b01fd44")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Parameter("item", "Item", false)]
		[Parameter("bAdded", "System.Boolean", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerInventory_30fa7aa143cf4ddaa97b77113b01fd44 : Patch
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
		[Identifier("02ed1851c76e414cb9e183f5dbb66dfe")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerInventory_02ed1851c76e414cb9e183f5dbb66dfe : Patch
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
		[Identifier("563816dd7a6f4614808dc4474d77ff20")]
		[Dependencies(new string[] { "OnDefaultItemsReceive" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerInventory_563816dd7a6f4614808dc4474d77ff20 : Patch
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
		[Identifier("9858d7de0ee5413487c1a680923c8c0e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Parameter("container", "ItemContainer", false)]
		[Parameter("local0", "ProtoBuf.UpdateItemContainer", false)]
		[Parameter("type", "PlayerInventory+Type", false)]
		[Parameter("mode", "PlayerInventory+NetworkInventoryMode", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PlayerInventory_9858d7de0ee5413487c1a680923c8c0e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 35)
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
		[Identifier("3838a2e492df452e9e1996553c146042")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseRagdoll", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseRagdoll_3838a2e492df452e9e1996553c146042 : Patch
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
		[Identifier("a06947871a4743ab854d2fb54b24661d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "PhotoFrame", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_PhotoFrame_a06947871a4743ab854d2fb54b24661d : Patch
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
		[Identifier("c00c0954e0ab4b808906e1123b674b0b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ContainerIOEntity", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ContainerIOEntity_c00c0954e0ab4b808906e1123b674b0b : Patch
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
		[Identifier("688821ce6d8f4fc6b057430fe9988fd6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ContainerIOEntity_688821ce6d8f4fc6b057430fe9988fd6 : Patch
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
		[Identifier("e9e6f4759da04401be1a36b776c8e15b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ModularCarSeat", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ModularCarSeat_e9e6f4759da04401be1a36b776c8e15b : Patch
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
		[Identifier("9a5cec5583434071b6b947776b744e26")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "GestureConfig", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_GestureConfig_9a5cec5583434071b6b947776b744e26 : Patch
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
		[Identifier("35ca1d44283640458cb437fa1c6cdce5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("connection", "Network.Connection", false)]
		[Parameter("local0", "System.String", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConsoleNetwork_35ca1d44283640458cb437fa1c6cdce5 : Patch
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
		[Identifier("53d3049c7aad4cc2901e909c5f830546")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("otherPlayer", "BasePlayer", false)]
		[Parameter("type", "RelationshipManager+RelationshipType", false)]
		[Parameter("weight", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_RelationshipManager_53d3049c7aad4cc2901e909c5f830546 : Patch
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
		[Identifier("88f0a3a485de40a18b12c4b9cf638df6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "LiquidContainer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_LiquidContainer_88f0a3a485de40a18b12c4b9cf638df6 : Patch
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
		[Identifier("fac9161a900c46a28113e5ece1a2ca8b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ItemBasedFlowRestrictor", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ItemBasedFlowRestrictor_fac9161a900c46a28113e5ece1a2ca8b : Patch
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
		[Identifier("7591e537a93646dc946ad578ce3a8853")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CarvablePumpkin", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_CarvablePumpkin_7591e537a93646dc946ad578ce3a8853 : Patch
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
		[Identifier("52b44f7e804f48d09aa27e4b09c3fa21")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AttackEntity", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("eyePos", "UnityEngine.Vector3", false)]
		[Parameter("checkLineOfSight", "System.Boolean", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_AttackEntity_52b44f7e804f48d09aa27e4b09c3fa21 : Patch
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
		[Identifier("ce173ecfad5945978776549891ba6876")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("sourceConnection", "Network.Connection", false)]
		[Parameter("self", "BaseProjectile", false)]
		[Parameter("prefabName", "System.String", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseProjectile_ce173ecfad5945978776549891ba6876 : Patch
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
		[Identifier("8968eaecd19446b6a2611e9e0dc5e37d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local2", "Network.Connection", false)]
		[Parameter("toString()", "System.String", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_EACServer_8968eaecd19446b6a2611e9e0dc5e37d : Patch
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
		[Identifier("7d8598eecdff4023a9d16cfd8d7f153b")]
		[Dependencies(new string[] { "OnPlayerBanned [EAC]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local2", "Network.Connection", false)]
		[Parameter("toString()", "System.String", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_EACServer_7d8598eecdff4023a9d16cfd8d7f153b : Patch
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
		[Identifier("f7654323978147b4b37be13e1935d5d9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BasePortal", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePortal_f7654323978147b4b37be13e1935d5d9 : Patch
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
		[Identifier("a2b8e3a98a29411595ef8000aabaec42")]
		[Dependencies(new string[] { "OnPortalUse" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BasePortal", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePortal_a2b8e3a98a29411595ef8000aabaec42 : Patch
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
		[Identifier("2e7d8248fe4b41a18db413332bb6efa3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ModularCarCodeLock", false)]
		[Parameter("codeEntered", "System.String", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ModularCarCodeLock_2e7d8248fe4b41a18db413332bb6efa3 : Patch
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
		[Identifier("f85f29aef04441dc99b0cf9efa35c711")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BaseDiggableEntity", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseDiggableEntity_f85f29aef04441dc99b0cf9efa35c711 : Patch
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
	}

	public class Player_BaseMetalDetector
	{
		[Patch("OnMetalDetectorFlagRequest", "OnMetalDetectorFlagRequest", "BaseMetalDetector", "RPC_RequestFlag", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("a4f1291d300f4673810cc4062db699c8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMetalDetector", false)]
		[Parameter("local1", "UnityEngine.Vector3", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseMetalDetector_a4f1291d300f4673810cc4062db699c8 : Patch
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
		[Identifier("eac970cda408451481616fb04d1588ed")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "HBHFSensor", false)]
		[Return(typeof(bool))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_HBHFSensor_eac970cda408451481616fb04d1588ed : Patch
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
		[Identifier("2cf6116628c64cdf82f6dea65c138650")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Handcuffs_2cf6116628c64cdf82f6dea65c138650 : Patch
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
		[Identifier("255d2bdb6a55433c9f621f1249686b74")]
		[Dependencies(new string[] { "OnPlayerHandcuff" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_Handcuffs_255d2bdb6a55433c9f621f1249686b74 : Patch
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
		[Identifier("3f7c92cfacf34a9eb598573998763d35")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConVarDebugging_3f7c92cfacf34a9eb598573998763d35 : Patch
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
		[Identifier("cb1104c5a8be4dac8f130a1fbc3e28d6")]
		[Dependencies(new string[] { "OnPlayerVanish" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConVarDebugging_cb1104c5a8be4dac8f130a1fbc3e28d6 : Patch
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
		[Identifier("b6e48181b0de434abf1b0d7a015f2d6b")]
		[Dependencies(new string[] { "OnPlayerVanished" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConVarDebugging_b6e48181b0de434abf1b0d7a015f2d6b : Patch
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
		[Identifier("2dd014f65ef44043acc10d352b9a8947")]
		[Dependencies(new string[] { "OnPlayerUnvanish" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConVarDebugging_2dd014f65ef44043acc10d352b9a8947 : Patch
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
		[Identifier("56c80af3cdee416a9b919d325d38e6c2")]
		[Dependencies(new string[] { "OnPlayerUnvanished" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConVarDebugging_56c80af3cdee416a9b919d325d38e6c2 : Patch
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
		[Identifier("5dc6aa8df0694edbbf4dafc3e7715bb3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "RFTimedExplosive", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_RFTimedExplosive_5dc6aa8df0694edbbf4dafc3e7715bb3 : Patch
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

	public class Player_BasePlayerOnFeedbackReportd764
	{
		[Patch("OnFeedbackReported", "OnFeedbackReported", "BasePlayer/<OnFeedbackReport>d__764", "MoveNext", new string[] { })]
		[Identifier("a8749e05078343c0b1a218c3bc106b17")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "BasePlayer", false)]
		[Parameter("local2", "System.String", false)]
		[Parameter("local3", "System.String", false)]
		[Parameter("local5", "Facepunch.Models.ReportType", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayerOnFeedbackReportd764_a8749e05078343c0b1a218c3bc106b17 : Patch
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

	public class Player_BasePlayerOnPlayerReportedd763
	{
		[Patch("OnPlayerReported", "OnPlayerReported", "BasePlayer/<OnPlayerReported>d__763", "MoveNext", new string[] { })]
		[Identifier("09067f6ab2384b17aefc0739332dacc1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "BasePlayer", false)]
		[Parameter("local6", "System.String", false)]
		[Parameter("self", "BasePlayer+<OnPlayerReported>d__763", false)]
		[Parameter("local2", "System.String", false)]
		[Parameter("local3", "System.String", false)]
		[Parameter("local5", "System.String", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BasePlayerOnPlayerReportedd763_09067f6ab2384b17aefc0739332dacc1 : Patch
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
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer+<OnPlayerReported>d__763"), "<targetId>5__2"));
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
		[Identifier("a8f108f88dff49b1a3f890cf3bfc838b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "SteamInventory", false)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_SteamInventoryUpdateSteamInventoryd9_a8f108f88dff49b1a3f890cf3bfc838b : Patch
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
		[Identifier("08cda4c4a0c043d9aa2732380cfa30c9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseEntity", false)]
		[Parameter("sourceConnection", "Network.Connection", false)]
		[Parameter("signal", "BaseEntity+Signal", false)]
		[Parameter("arg", "System.String", false)]
		[Return(typeof(void))]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_BaseEntity_08cda4c4a0c043d9aa2732380cfa30c9 : Patch
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
		[Identifier("c53d5a0f848c447dac0335523e2368bd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ItemModConsume_c53d5a0f848c447dac0335523e2368bd : Patch
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
				CodeInstruction obj = list2[198];
				list.Add(new CodeInstruction(OpCodes.Bne_Un_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[191]), list2[191]);
				}
				list2.InsertRange(191, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Player_AuthCentralizedBansRund0
	{
		[Patch("OnCentralizedBanCheck", "OnCentralizedBanCheck", "Auth_CentralizedBans/<Run>d__0", "MoveNext", new string[] { })]
		[Identifier("75075a3256c14214bd291787ce9dd5e3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_AuthCentralizedBansRund0_75075a3256c14214bd291787ce9dd5e3 : Patch
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
		[Identifier("2fdfee6d934540f5ab9ca788f0f6d262")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_IndustrialCrafter_2fdfee6d934540f5ab9ca788f0f6d262 : Patch
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
		[Identifier("02539f94ad6645b39e9c323592a3975d")]
		[Dependencies(new string[] { "IOnPlayerChat[patch]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Player")]
		[Assembly("Assembly-CSharp.dll")]
		public class Player_ConVarChatsayAsd22_02539f94ad6645b39e9c323592a3975d : Patch
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
		[Identifier("2f14e49bd8a14bdc9fdd414551f2b1f7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("cn", "Network.Connection", false)]
		[Parameter("strReason", "System.String", false)]
		[Category("Player")]
		[Assembly("Facepunch.Network.dll")]
		public class Player_NetworkServer_2f14e49bd8a14bdc9fdd414551f2b1f7 : Patch
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
