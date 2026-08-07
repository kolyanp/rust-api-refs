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
		[Identifier("683841ab224c4942ad0a01d784e397ca")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Workbench", false)]
		[Parameter("local5", "TechTreeData+NodeInstance", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("TechTree")]
		[Assembly("Assembly-CSharp.dll")]
		public class TechTree_Workbench_683841ab224c4942ad0a01d784e397ca : Patch
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
		[Identifier("12e12974180b4c6fbd0892f2d1a26042")]
		[Dependencies(new string[] { "OnTechTreeNodeUnlock" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Workbench", false)]
		[Parameter("local5", "TechTreeData+NodeInstance", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local12", "PooledList`1[ItemDefinition]", false)]
		[Category("TechTree")]
		[Assembly("Assembly-CSharp.dll")]
		public class TechTree_Workbench_12e12974180b4c6fbd0892f2d1a26042 : Patch
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
