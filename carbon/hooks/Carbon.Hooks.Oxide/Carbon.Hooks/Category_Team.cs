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
		[Identifier("6ccdaa3cea6a4989bc101f93ae10cd8f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_6ccdaa3cea6a4989bc101f93ae10cd8f : Patch
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
		[Identifier("c317ea9737f34694be9b4701ad211e81")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "RelationshipManager+PlayerTeam", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_c317ea9737f34694be9b4701ad211e81 : Patch
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
		[Identifier("22ec0c54ab6f4044af3ed570bbe6c4d7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "RelationshipManager+PlayerTeam", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_22ec0c54ab6f4044af3ed570bbe6c4d7 : Patch
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
		[Identifier("f4ee2e50cb1647d9b8e8cbdf6c72fe33")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "RelationshipManager+PlayerTeam", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "System.UInt64", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_f4ee2e50cb1647d9b8e8cbdf6c72fe33 : Patch
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
		[Identifier("0e0c95b51df148cfa2ed55b9272791c2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local2", "RelationshipManager+PlayerTeam", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_0e0c95b51df148cfa2ed55b9272791c2 : Patch
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
		[Identifier("20439e5d0542446e99e4aa1b4558f4ee")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_20439e5d0542446e99e4aa1b4558f4ee : Patch
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
		[Identifier("70b413e8f1e0413dafc2b5c5e22198ba")]
		[Dependencies(new string[] { "OnTeamCreate" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "RelationshipManager+PlayerTeam", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_70b413e8f1e0413dafc2b5c5e22198ba : Patch
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
		[Identifier("d050363ea1144f78b7ccdba518e886cf")]
		[Dependencies(new string[] { "OnTeamDisband" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_d050363ea1144f78b7ccdba518e886cf : Patch
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
		[Identifier("8980bb89703540cc8fc668f08934019a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "RelationshipManager+PlayerTeam", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "System.UInt64", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_8980bb89703540cc8fc668f08934019a : Patch
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
		[Identifier("1e6457971ed941a893e3f744d143c5de")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManager_1e6457971ed941a893e3f744d143c5de : Patch
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
		[Identifier("e6293607039e4ac8b75e15e97c1956c0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("currentTeam", "System.UInt64", false)]
		[Parameter("newTeam", "System.UInt64", false)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_BasePlayer_e6293607039e4ac8b75e15e97c1956c0 : Patch
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
		[Identifier("bb1b757538404aa4b9b5c70cb27433e7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("currentTeam", "System.UInt64", false)]
		[Parameter("local3", "ProtoBuf.PlayerTeam", false)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_BasePlayer_bb1b757538404aa4b9b5c70cb27433e7 : Patch
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
		[Identifier("4476088286c64717a18c5c4fa639db6c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RelationshipManager+PlayerTeam", false)]
		[Return(typeof(void))]
		[Category("Team")]
		[Assembly("Assembly-CSharp.dll")]
		public class Team_RelationshipManagerPlayerTeam_4476088286c64717a18c5c4fa639db6c : Patch
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
