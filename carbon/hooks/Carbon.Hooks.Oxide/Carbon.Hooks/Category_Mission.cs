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
		[Identifier("43048536d5924bb7971fff55fbb35c0e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMission", false)]
		[Parameter("instance", "BaseMission+MissionInstance", false)]
		[Parameter("assignee", "BasePlayer", false)]
		[Parameter("failReason", "BaseMission+MissionFailReason", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_43048536d5924bb7971fff55fbb35c0e : Patch
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
		[Identifier("b8d99fb5ff184be19a03561e70a7f974")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMission", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_b8d99fb5ff184be19a03561e70a7f974 : Patch
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
		[Identifier("e3ed661313a341258fb4f468857d6f74")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMission", false)]
		[Return(typeof(void))]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_e3ed661313a341258fb4f468857d6f74 : Patch
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
		[Identifier("8049dc5f36544e8ca758eb553f9bcef7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("assignee", "BasePlayer", false)]
		[Parameter("mission", "BaseMission", false)]
		[Parameter("provider", "IMissionProvider", false)]
		[Return(typeof(bool))]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_8049dc5f36544e8ca758eb553f9bcef7 : Patch
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
		[Identifier("4029ffa9f2bd4e9fb02519a17e757768")]
		[Dependencies(new string[] { "CanAssignMission" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("mission", "BaseMission", false)]
		[Parameter("provider", "IMissionProvider", false)]
		[Parameter("assignee", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_4029ffa9f2bd4e9fb02519a17e757768 : Patch
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
		[Identifier("e650926039fc4b289a1a26461c707a43")]
		[Dependencies(new string[] { "OnMissionStart" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMission", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Mission")]
		[Assembly("Assembly-CSharp.dll")]
		public class Mission_BaseMission_e650926039fc4b289a1a26461c707a43 : Patch
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
