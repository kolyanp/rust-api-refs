using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Pet
{
	public class Pet_FrankensteinTable
	{
		[Patch("OnFrankensteinPetWake", "OnFrankensteinPetWake [FrankensteinTable]", "FrankensteinTable", "WakeFrankenstein", new string[] { "BasePlayer" })]
		[Identifier("fb2ec85e1a6e47789c2542af79bd3f19")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FrankensteinTable", false)]
		[Parameter("owner", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Pet")]
		[Assembly("Assembly-CSharp.dll")]
		public class Pet_FrankensteinTable_fb2ec85e1a6e47789c2542af79bd3f19 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)584448208), instruction), instruction);
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

		[Patch("OnFrankensteinPetSleep", "OnFrankensteinPetSleep [FrankensteinTable]", "FrankensteinTable", "SleepFrankenstein", new string[] { "BasePlayer" })]
		[Identifier("01383b0350ea4acaaf7416becc70be7a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "FrankensteinPet", false)]
		[Parameter("self", "FrankensteinTable", false)]
		[Parameter("owner", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Pet")]
		[Assembly("Assembly-CSharp.dll")]
		public class Pet_FrankensteinTable_01383b0350ea4acaaf7416becc70be7a : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1722860509), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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
}
