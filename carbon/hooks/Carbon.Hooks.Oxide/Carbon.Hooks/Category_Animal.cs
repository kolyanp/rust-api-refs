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
		[Identifier("4533ccdf3d66485db41a5b44256dad8f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_4533ccdf3d66485db41a5b44256dad8f : Patch
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
		[Identifier("10fceef8fc48499dad1d951967969403")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "Item", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_10fceef8fc48499dad1d951967969403 : Patch
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
		[Identifier("a9e50d000fb94d23becabb52a0874d4f")]
		[Dependencies(new string[] { "OnRidableAnimalClaim [RidableHorse]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_a9e50d000fb94d23becabb52a0874d4f : Patch
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
		[Identifier("242e0b742b2446058d7a0e59f63debff")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_242e0b742b2446058d7a0e59f63debff : Patch
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
		[Identifier("a034919eaf7d4d7b844170bf64702200")]
		[Dependencies(new string[] { "OnAnimalDungProduced [RidableHorse] [Variable]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_a034919eaf7d4d7b844170bf64702200 : Patch
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
		[Identifier("4f9633be584e4569badf75ddb818f3a4")]
		[Dependencies(new string[] { "OnAnimalDungProduce [RidableHorse]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_4f9633be584e4569badf75ddb818f3a4 : Patch
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
		[Identifier("912bedb4aa574f2db7256d0187184e51")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(bool))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_HitchTrough_912bedb4aa574f2db7256d0187184e51 : Patch
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
		[Identifier("e075c2320e8b4d54bb4942d624e1f9c9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("hitchable", "HitchTrough+IHitchable", false)]
		[Parameter("local2", "HitchTrough+HitchSpot", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_HitchTrough_e075c2320e8b4d54bb4942d624e1f9c9 : Patch
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
