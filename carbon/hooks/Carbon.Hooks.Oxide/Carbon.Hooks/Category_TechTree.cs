using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_TechTree
{
	public class TechTree_Workbench
	{
		[Patch("OnTechTreeNodeUnlock", "OnTechTreeNodeUnlock", "Workbench", "RPC_TechTreeUnlock", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("7e7ab3672f2c40b9b6cfb2ef5836cf19")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Workbench", false)]
		[Parameter("local5", "TechTreeData+NodeInstance", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("TechTree")]
		[Assembly("Assembly-CSharp.dll")]
		public class TechTree_Workbench_7e7ab3672f2c40b9b6cfb2ef5836cf19 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2046968180)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}

		[Patch("OnTechTreeNodeUnlocked", "OnTechTreeNodeUnlocked", "Workbench", "RPC_TechTreeUnlock", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("89dae90988994953ab602daa8b9b6862")]
		[Dependencies(new string[] { "OnTechTreeNodeUnlock" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Workbench", false)]
		[Parameter("local5", "TechTreeData+NodeInstance", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local12", "PooledList`1[ItemDefinition]", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("TechTree")]
		[Assembly("Assembly-CSharp.dll")]
		public class TechTree_Workbench_89dae90988994953ab602daa8b9b6862 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 296)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1612905148), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)12);
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
}
