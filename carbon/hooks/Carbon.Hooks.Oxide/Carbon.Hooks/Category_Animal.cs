using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Animal
{
	public class Animal_RidableHorse
	{
		[Patch("OnHorseLead", "OnHorseLead [RidableHorse]", "RidableHorse", "SERVER_Lead", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("5d1e9184b48145cf9396e0a75aed445d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_5d1e9184b48145cf9396e0a75aed445d : Patch
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
		[Identifier("fd31e5d86ed84970b6b1025a8103961c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "Item", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_fd31e5d86ed84970b6b1025a8103961c : Patch
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
		[Identifier("ebf464ddc02b46fea8d1f9fcf6167d81")]
		[Dependencies(new string[] { "OnRidableAnimalClaim [RidableHorse]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_ebf464ddc02b46fea8d1f9fcf6167d81 : Patch
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
		[Identifier("bed19e5c76da4631becf37fe83b2c8b9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_bed19e5c76da4631becf37fe83b2c8b9 : Patch
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
		[Identifier("6c5d161f90b14c7d9fe07f8528a3313f")]
		[Dependencies(new string[] { "OnAnimalDungProduced [RidableHorse] [Variable]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_6c5d161f90b14c7d9fe07f8528a3313f : Patch
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
		[Identifier("7522607c23a54121960a3ee28a6483fa")]
		[Dependencies(new string[] { "OnAnimalDungProduce [RidableHorse]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_7522607c23a54121960a3ee28a6483fa : Patch
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
		[Identifier("6fb52c5209cc4a6896b2126b454ca3eb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(bool))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_HitchTrough_6fb52c5209cc4a6896b2126b454ca3eb : Patch
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
		[Identifier("b447787aff0847d1b16d9df68ab1dbb3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("hitchable", "HitchTrough+IHitchable", false)]
		[Parameter("local2", "HitchTrough+HitchSpot", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_HitchTrough_b447787aff0847d1b16d9df68ab1dbb3 : Patch
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

	public class Animal_RustAiGen2LivestockAnimal
	{
		[Patch("OnLivestockAnimalFamiliarityAdd", "OnLivestockAnimalFamiliarityAdd", "Rust.Ai.Gen2.LivestockAnimal", "AddFamiliarity", new string[] { "System.UInt64", "System.Single" })]
		[Identifier("1c42699957544c13bb9ca0e05fa80449")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.LivestockAnimal", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2LivestockAnimal_1c42699957544c13bb9ca0e05fa80449 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 7)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2105188281)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
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

		[Patch("OnLivestockAnimalFamiliarityAdded", "OnLivestockAnimalFamiliarityAdded", "Rust.Ai.Gen2.LivestockAnimal", "AddFamiliarity", new string[] { "System.UInt64", "System.Single" })]
		[Identifier("494774b5771e49bbac6d50c607be7ad1")]
		[Dependencies(new string[] { "OnLivestockAnimalFamiliarityAdd" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.LivestockAnimal", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2LivestockAnimal_494774b5771e49bbac6d50c607be7ad1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 55)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-457800592)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
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

		[Patch("OnLivestockAnimalFamiliarityAdded", "OnLivestockAnimalFamiliarityAdded [new]", "Rust.Ai.Gen2.LivestockAnimal", "AddFamiliarity", new string[] { "System.UInt64", "System.Single" })]
		[Identifier("ef070cacbfa649f2824b30db4d54ace6")]
		[Dependencies(new string[] { "OnLivestockAnimalFamiliarityAdded" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.LivestockAnimal", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2LivestockAnimal_ef070cacbfa649f2824b30db4d54ace6 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 121)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-457800592)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
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

		[Patch("OnLivestockAnimalFamiliarityAdded", "OnLivestockAnimalFamiliarityAdded [probation]", "Rust.Ai.Gen2.LivestockAnimal", "AddFamiliarity", new string[] { "System.UInt64", "System.Single" })]
		[Identifier("4b155dab693b43949478eab20630f333")]
		[Dependencies(new string[] { "OnLivestockAnimalFamiliarityAdded [new]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.LivestockAnimal", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2LivestockAnimal_4b155dab693b43949478eab20630f333 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 189)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-457800592)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
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

		[Patch("CanLivestockAnimalRefuseTarget", "CanLivestockAnimalRefuseTarget", "Rust.Ai.Gen2.LivestockAnimal", "RefusesToTarget", new string[] { "BaseEntity" })]
		[Identifier("c716fe1bfb0e4a8e8f7851dda3dd6fbb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.LivestockAnimal", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2LivestockAnimal_c716fe1bfb0e4a8e8f7851dda3dd6fbb : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1325230980), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnLivestockAnimalClaim", "OnLivestockAnimalClaim", "Rust.Ai.Gen2.LivestockAnimal", "ClaimByTrough", new string[] { "HitchTrough" })]
		[Identifier("bdb2e7d1af7e4bfeae62973edab3c670")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.LivestockAnimal", false)]
		[Parameter("trough", "HitchTrough", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2LivestockAnimal_bdb2e7d1af7e4bfeae62973edab3c670 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 8)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)664716446), instruction), instruction);
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
	}

	public class Animal_RustAiGen2Cow
	{
		[Patch("OnCowMilk", "OnCowMilk", "Rust.Ai.Gen2.Cow", "TryMilk", new string[] { "BasePlayer" })]
		[Identifier("7b3b7263e5a745628059dc8bf1154acc")]
		[Dependencies(new string[] { "OnCowMilk [patch 1]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.Cow", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2Cow_7b3b7263e5a745628059dc8bf1154acc : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 65)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-325448841)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return __GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					Label label2 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Brfalse_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Brtrue_S, (object)label2);
					yield return new CodeInstruction(OpCodes.Ldc_I4_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return CodeInstructionExtensions.WithLabels(new CodeInstruction(OpCodes.Ldloc, retvar), new Label[1] { label2 });
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnCowMilked", "OnCowMilked", "Rust.Ai.Gen2.Cow", "TryMilk", new string[] { "BasePlayer" })]
		[Identifier("e2e4458801824e42a13d2013397762a1")]
		[Dependencies(new string[] { "OnCowMilk [patch 2]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.Cow", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2Cow_e2e4458801824e42a13d2013397762a1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 109)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1061738027)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return __GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object));
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

		[Patch("OnCowMilk", "OnCowMilk [patch 1]", "Rust.Ai.Gen2.Cow", "TryMilk", new string[] { "BasePlayer" })]
		[Identifier("7ef1c889ae0243b3870fef4c621c069d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2Cow_7ef1c889ae0243b3870fef4c621c069d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0036: Expected Obj, but got Unknown
				//IL_003d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0047: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Pop, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[63]), list2[63]);
				}
				list2.InsertRange(63, list);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnCowMilk", "OnCowMilk [patch 2]", "Rust.Ai.Gen2.Cow", "TryMilk", new string[] { "BasePlayer" })]
		[Identifier("5633aee3e7ea4080bec41f7e3177bfba")]
		[Dependencies(new string[] { "OnCowMilk" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2Cow_5633aee3e7ea4080bec41f7e3177bfba : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_003f: Expected Obj, but got Unknown
				//IL_006d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0077: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldc_R4, (object)0f));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Item"), "Remove", new Type[1] { typeof(float) }, (Type[])null)));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[73]), list2[73]);
				}
				list2.InsertRange(73, list);
				return list2.AsEnumerable();
			}
		}
	}
}
