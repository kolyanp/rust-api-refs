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
		[Identifier("1b88366627844fdbb3201bb646095d2b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMission", false)]
		[Parameter("instance", "BaseMission+MissionInstance", false)]
		[Parameter("assignee", "BasePlayer", false)]
		[Parameter("failReason", "BaseMission+MissionFailReason", false)]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_1b88366627844fdbb3201bb646095d2b : Patch
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
		[Identifier("5322f3b385194480bdcdf725cf1d0f7e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMission", false)]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_5322f3b385194480bdcdf725cf1d0f7e : Patch
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
		[Identifier("1fa6bf8f8ded42fab7d8668c535f373c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMission", false)]
		[Return(typeof(void))]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_1fa6bf8f8ded42fab7d8668c535f373c : Patch
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
		[Identifier("46f3e3e647834dae888566a95fcdb2b0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("assignee", "BasePlayer", false)]
		[Parameter("mission", "BaseMission", false)]
		[Parameter("provider", "IMissionProvider", false)]
		[Return(typeof(bool))]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_46f3e3e647834dae888566a95fcdb2b0 : Patch
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
		[Identifier("a5730a224dc342bd8f3906d885c1c23b")]
		[Dependencies(new string[] { "CanAssignMission" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("mission", "BaseMission", false)]
		[Parameter("provider", "IMissionProvider", false)]
		[Parameter("assignee", "BasePlayer", false)]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_a5730a224dc342bd8f3906d885c1c23b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 200)
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
		[Identifier("1d379d658dd045afa8272ccb6e9dfcc9")]
		[Dependencies(new string[] { "OnMissionStart" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMission", false)]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_1d379d658dd045afa8272ccb6e9dfcc9 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 67)
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
