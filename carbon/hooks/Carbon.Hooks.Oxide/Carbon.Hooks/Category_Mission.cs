using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Mission
{
	public class Mission_BaseMission
	{
		[Patch("OnMissionFailed", "OnMissionFailed", "BaseMission", "MissionFailed", new string[] { "BaseMission/MissionInstance", "BasePlayer", "BaseMission/MissionFailReason", "System.Boolean" })]
		[Identifier("821b735f1b0c401b98e3fd7fb7d95ee3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMission", false)]
		[Parameter("instance", "BaseMission+MissionInstance", false)]
		[Parameter("assignee", "BasePlayer", false)]
		[Parameter("failReason", "BaseMission+MissionFailReason", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_821b735f1b0c401b98e3fd7fb7d95ee3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 29)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)63503158), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(MissionFailReason));
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

		[Patch("OnMissionSucceeded", "OnMissionSucceeded", "BaseMission", "MissionSuccess", new string[] { "BaseMission/MissionInstance", "BasePlayer" })]
		[Identifier("61b30f4398dc4291b600a38a65f095de")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMission", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_61b30f4398dc4291b600a38a65f095de : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2044371482), instruction), instruction);
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
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnMissionStart", "OnMissionStart", "BaseMission", "MissionStart", new string[] { "BaseMission/MissionInstance", "BasePlayer" })]
		[Identifier("bd3a3d08f1ea4522a29a8c98808f4984")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMission", false)]
		[Return(typeof(void))]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_bd3a3d08f1ea4522a29a8c98808f4984 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)711033722), instruction), instruction);
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

		[Patch("CanAssignMission", "CanAssignMission", "BaseMission", "AssignMission", new string[] { "BasePlayer", "IMissionProvider", "BaseMission" })]
		[Identifier("1fbee73704354e4f8f794eb25cfcd368")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("assignee", "BasePlayer", false)]
		[Parameter("mission", "BaseMission", false)]
		[Parameter("provider", "IMissionProvider", false)]
		[Return(typeof(bool))]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_1fbee73704354e4f8f794eb25cfcd368 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1070103224), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("OnMissionAssigned", "OnMissionAssigned", "BaseMission", "AssignMission", new string[] { "BasePlayer", "IMissionProvider", "BaseMission" })]
		[Identifier("3a348cbc26914dfa955200635fb01513")]
		[Dependencies(new string[] { "CanAssignMission" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("mission", "BaseMission", false)]
		[Parameter("provider", "IMissionProvider", false)]
		[Parameter("assignee", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_3a348cbc26914dfa955200635fb01513 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 207)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2072569864), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnMissionStarted", "OnMissionStarted", "BaseMission", "MissionStart", new string[] { "BaseMission/MissionInstance", "BasePlayer" })]
		[Identifier("ff569647d55642419c611a08f3c1fb46")]
		[Dependencies(new string[] { "OnMissionStart" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMission", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_ff569647d55642419c611a08f3c1fb46 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 71)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)616743836), instruction), instruction);
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
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}
}
