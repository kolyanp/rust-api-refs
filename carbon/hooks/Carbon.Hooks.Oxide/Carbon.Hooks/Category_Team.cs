using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Team
{
	public class Team_RelationshipManager
	{
		[Patch("OnTeamCreate", "OnTeamCreate", "RelationshipManager", "TryCreateTeam", new string[] { "BasePlayer" })]
		[Identifier("3ec8ddae035b4fce9d343dd70b041537")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_3ec8ddae035b4fce9d343dd70b041537 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1572978909)), instruction), instruction);
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

		[Patch("OnTeamRejectInvite", "OnTeamRejectInvite", "RelationshipManager", "rejectinvite", new string[] { "ConsoleSystem/Arg" })]
		[Identifier("44eb009038874d1ab51278292d6f92e2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "RelationshipManager+PlayerTeam", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_44eb009038874d1ab51278292d6f92e2 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1844985686), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
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

		[Patch("OnTeamLeave", "OnTeamLeave", "RelationshipManager", "leaveteam", new string[] { "ConsoleSystem/Arg" })]
		[Identifier("520ee0f2ad884e13922c76dc4156a03c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "RelationshipManager+PlayerTeam", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_520ee0f2ad884e13922c76dc4156a03c : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1381333618)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

		[Patch("OnTeamKick", "OnTeamKick", "RelationshipManager", "kickmember", new string[] { "ConsoleSystem/Arg" })]
		[Identifier("cb709e993d354362a8f9da5a1461e952")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "RelationshipManager+PlayerTeam", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "System.UInt64", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_cb709e993d354362a8f9da5a1461e952 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 34)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1029550040)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
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

		[Patch("OnTeamAcceptInvite", "OnTeamAcceptInvite", "RelationshipManager", "acceptinvite", new string[] { "ConsoleSystem/Arg" })]
		[Identifier("62182bce50de4137a6bdb5c955ada92d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local2", "RelationshipManager+PlayerTeam", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_62182bce50de4137a6bdb5c955ada92d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1571014963)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
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

		[Patch("OnTeamDisband", "OnTeamDisband", "RelationshipManager", "DisbandTeam", new string[] { "RelationshipManager/PlayerTeam" })]
		[Identifier("e8fa964a55df433782159ce5a09ceb1e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_e8fa964a55df433782159ce5a09ceb1e : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1766427982)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("OnTeamCreated", "OnTeamCreated", "RelationshipManager", "TryCreateTeam", new string[] { "BasePlayer" })]
		[Identifier("24ddeb98a6f840e8ac459b4e9ab2ab4c")]
		[Dependencies(new string[] { "OnTeamCreate" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "RelationshipManager+PlayerTeam", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_24ddeb98a6f840e8ac459b4e9ab2ab4c : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-73805916)), instruction), instruction);
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

		[Patch("OnTeamDisbanded", "OnTeamDisbanded", "RelationshipManager", "DisbandTeam", new string[] { "RelationshipManager/PlayerTeam" })]
		[Identifier("0ed7c518222f41429ba4255a40a395e5")]
		[Dependencies(new string[] { "OnTeamDisband" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_0ed7c518222f41429ba4255a40a395e5 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)197406368), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("OnTeamMemberInvite", "OnTeamMemberInvite [sendofflineinvite]", "RelationshipManager", "sendofflineinvite", new string[] { "ConsoleSystem/Arg" })]
		[Identifier("54a838f8a3004fd0bdd94a3b48da0708")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "RelationshipManager+PlayerTeam", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "System.UInt64", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_54a838f8a3004fd0bdd94a3b48da0708 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 61)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)844539354), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt64"));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
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

		[Patch("OnTeamMemberInvite", "OnTeamMemberInvite [sendinvite]", "RelationshipManager", "sendinvite", new string[] { "ConsoleSystem/Arg" })]
		[Identifier("f228f9b7318046e3a67ea15b9973ab82")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_f228f9b7318046e3a67ea15b9973ab82 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bf: Expected Obj, but got Unknown
				//IL_0121: Unknown result type (might be due to invalid IL or missing references)
				//IL_012b: Expected Obj, but got Unknown
				//IL_0147: Unknown result type (might be due to invalid IL or missing references)
				//IL_0151: Expected Obj, but got Unknown
				//IL_0161: Unknown result type (might be due to invalid IL or missing references)
				//IL_016b: Expected Obj, but got Unknown
				//IL_0172: Unknown result type (might be due to invalid IL or missing references)
				//IL_017c: Expected Obj, but got Unknown
				//IL_018c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0196: Expected Obj, but got Unknown
				//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
				//IL_0202: Expected Obj, but got Unknown
				//IL_021e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0228: Expected Obj, but got Unknown
				//IL_022f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0239: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				MethodInfo objB = AccessTools.Method(AccessToolsEx.TypeByName("RelationshipManager+PlayerTeam"), "SendInvite", new Type[1] { AccessToolsEx.TypeByName("BasePlayer") }, (Type[])null);
				int num = -1;
				for (int i = 0; i < list2.Count - 2; i++)
				{
					if (!(list2[i].opcode != OpCodes.Ldloc_1) && !(list2[i + 1].opcode != OpCodes.Ldloc_3) && object.Equals(list2[i + 2].operand, objB))
					{
						num = i;
						break;
					}
				}
				if (num < 0)
				{
					return list2.AsEnumerable();
				}
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnTeamMemberInvite"));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 3, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldflda, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer"), "userID")));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("EncryptedValue`1[System.UInt64]"), "Get", (Type[])null, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Box, (object)typeof(ulong)));
				list.Add(new CodeInstruction(OpCodes.Ldc_I4_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Box, (object)typeof(bool)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[5]
				{
					typeof(string),
					typeof(object),
					typeof(object),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[num];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[num]), list2[num]);
				}
				list2.InsertRange(num, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Team_BasePlayer
	{
		[Patch("OnTeamUpdate", "OnTeamUpdate", "BasePlayer", "UpdateTeam", new string[] { "System.UInt64" })]
		[Identifier("3b95b619ff264768b07963a2faeca241")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("currentTeam", "System.UInt64", false)]
		[Parameter("newTeam", "System.UInt64", false)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_BasePlayer_3b95b619ff264768b07963a2faeca241 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1613227947), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer"), "currentTeam"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt64"));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnTeamUpdated", "OnTeamUpdated", "BasePlayer", "TeamUpdate", new string[] { "System.Boolean" })]
		[Identifier("5aae1c17e4d4424485260fba2e504bc0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("currentTeam", "System.UInt64", false)]
		[Parameter("local3", "ProtoBuf.PlayerTeam", false)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_BasePlayer_5aae1c17e4d4424485260fba2e504bc0 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 277)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1173491625), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer"), "currentTeam"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt64"));
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

	public class Team_RelationshipManagerPlayerTeam
	{
		[Patch("OnTeamMemberPromote", "OnTeamMemberPromote", "RelationshipManager/PlayerTeam", "SetTeamLeader", new string[] { "System.UInt64" })]
		[Identifier("26c883683bf44461a4f70d3f4ca27edd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RelationshipManager+PlayerTeam", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManagerPlayerTeam_26c883683bf44461a4f70d3f4ca27edd : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1658239813), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
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
}
