using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Animal
{
	public class Animal_RidableHorse
	{
		[Patch("OnHorseLead", "OnHorseLead [RidableHorse]", "RidableHorse", "SERVER_Lead", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("54074a3035734eba9d7e859e502cf316")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_54074a3035734eba9d7e859e502cf316 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1860309333)), instruction), instruction);
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

		[Patch("OnRidableAnimalClaim", "OnRidableAnimalClaim [RidableHorse]", "RidableHorse", "SERVER_Claim", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("5c6d76e6723b432b95e14af516cf75b8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "Item", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_5c6d76e6723b432b95e14af516cf75b8 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1683370851)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
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

		[Patch("OnRidableAnimalClaimed", "OnRidableAnimalClaimed [RidableHorse]", "RidableHorse", "SERVER_Claim", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("7bcfcf119205470b9158d51a650e819b")]
		[Dependencies(new string[] { "OnRidableAnimalClaim [RidableHorse]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_7bcfcf119205470b9158d51a650e819b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 64)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)617527508), instruction);
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

		[Patch("OnAnimalDungProduce", "OnAnimalDungProduce [RidableHorse]", "RidableHorse", "DoDung", new string[] { })]
		[Identifier("bf20a30a7c6e466b9b80606f0db1897d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_bf20a30a7c6e466b9b80606f0db1897d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)73579294), instruction), instruction);
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

		[Patch("OnAnimalDungProduced", "OnAnimalDungProduced [RidableHorse]", "RidableHorse", "DoDung", new string[] { })]
		[Identifier("6d6272ba4df94009b45b678ece478d19")]
		[Dependencies(new string[] { "OnAnimalDungProduced [RidableHorse] [Variable]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_6d6272ba4df94009b45b678ece478d19 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 64)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)878484420), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return __GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 2, typeof(object));
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

		[Patch("OnAnimalDungProduced", "OnAnimalDungProduced [RidableHorse] [Variable]", "RidableHorse", "DoDung", new string[] { })]
		[Identifier("011a0ab1a73748b7af1464d7c8ce477d")]
		[Dependencies(new string[] { "OnAnimalDungProduce [RidableHorse]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_011a0ab1a73748b7af1464d7c8ce477d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 2, typeof(object)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 2, typeof(object)));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[43]), list2[43]);
				}
				list2.InsertRange(43, list);
				return list2.AsEnumerable();
			}
		}
	}

	public class Animal_HitchTrough
	{
		[Patch("OnHorseHitch", "OnHorseHitch", "HitchTrough", "AttemptToHitch", new string[] { "HitchTrough/IHitchable", "HitchTrough/HitchSpot" })]
		[Identifier("eb34548ffb5b4d72a821eb01cec98c91")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(bool))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_HitchTrough_eb34548ffb5b4d72a821eb01cec98c91 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1232754581)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
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

		[Patch("OnHorseUnhitch", "OnHorseUnhitch", "HitchTrough", "UnHitch", new string[] { "HitchTrough/IHitchable" })]
		[Identifier("99109690f7694ce6bf78dc250489da9e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("hitchable", "HitchTrough+IHitchable", false)]
		[Parameter("local2", "HitchTrough+HitchSpot", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_HitchTrough_99109690f7694ce6bf78dc250489da9e : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1764412199), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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
	}
}
