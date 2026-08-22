using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Industrial
{
	public class Industrial_IndustrialConveyor
	{
		[Patch("OnConveyorFiltersChange", "OnConveyorFiltersChange", "IndustrialConveyor", "RPC_ChangeFilters", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("abf3244fd1b149f8816b26a10c5f1d03")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "IndustrialConveyor", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "ProtoBuf.IndustrialConveyor+ItemFilterList", false)]
		[Return(typeof(void))]
		[Category("Industrial")]
		[Assembly("Assembly-CSharp.dll")]
		public class Industrial_IndustrialConveyor_abf3244fd1b149f8816b26a10c5f1d03 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)874753198), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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
	}

	public class Industrial_IndustrialCrafter
	{
		[Patch("OnItemCraft", "OnItemCraft [IndustrialCrafter]", "IndustrialCrafter", "RunJob", new string[] { })]
		[Identifier("e4bf02ac5a7e47c7b1894b054be6a523")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "IndustrialCrafter", false)]
		[Parameter("local2", "ItemBlueprint", false)]
		[Return(typeof(void))]
		[Category("Industrial")]
		[Assembly("Assembly-CSharp.dll")]
		public class Industrial_IndustrialCrafter_e4bf02ac5a7e47c7b1894b054be6a523 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 42)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)276522030), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}
	}
}
