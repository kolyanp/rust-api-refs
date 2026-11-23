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
		[Identifier("9c5fb36e6f94419383f0a1690caf2832")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_9c5fb36e6f94419383f0a1690caf2832 : Patch
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
		[Identifier("8883417b367a44db988c8939fdbb92c5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "Item", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_8883417b367a44db988c8939fdbb92c5 : Patch
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
		[Identifier("577abfe406a1493dab50db70203fbe88")]
		[Dependencies(new string[] { "OnRidableAnimalClaim [RidableHorse]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_577abfe406a1493dab50db70203fbe88 : Patch
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
		[Identifier("07b92bde65744221bb007e1d94a017e1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_07b92bde65744221bb007e1d94a017e1 : Patch
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
		[Identifier("4c831aa2b33f437094895f342e59ff20")]
		[Dependencies(new string[] { "OnAnimalDungProduced [RidableHorse] [Variable]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RidableHorse", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_4c831aa2b33f437094895f342e59ff20 : Patch
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
		[Identifier("9e11a0f0a5c3430a87b676e427034387")]
		[Dependencies(new string[] { "OnAnimalDungProduce [RidableHorse]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RidableHorse_9e11a0f0a5c3430a87b676e427034387 : Patch
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
		[Identifier("ff46029ebcc44affa2423c7183100955")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(bool))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_HitchTrough_ff46029ebcc44affa2423c7183100955 : Patch
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
		[Identifier("8488d9d34cc84150a01e64cded69bf32")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("hitchable", "HitchTrough+IHitchable", false)]
		[Parameter("local2", "HitchTrough+HitchSpot", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_HitchTrough_8488d9d34cc84150a01e64cded69bf32 : Patch
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
		[Identifier("7c2fb29306cc44b98dde395b4beebebb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.LivestockAnimal", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2LivestockAnimal_7c2fb29306cc44b98dde395b4beebebb : Patch
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
		[Identifier("a059ffe73c1d42cf9646eada61ed3f42")]
		[Dependencies(new string[] { "OnLivestockAnimalFamiliarityAdd" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.LivestockAnimal", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2LivestockAnimal_a059ffe73c1d42cf9646eada61ed3f42 : Patch
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
		[Identifier("e071aa91c8de4690b4b6ae20925145a0")]
		[Dependencies(new string[] { "OnLivestockAnimalFamiliarityAdded" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.LivestockAnimal", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2LivestockAnimal_e071aa91c8de4690b4b6ae20925145a0 : Patch
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
		[Identifier("d478237904214db9996196ab2bd3818f")]
		[Dependencies(new string[] { "OnLivestockAnimalFamiliarityAdded [new]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.LivestockAnimal", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2LivestockAnimal_d478237904214db9996196ab2bd3818f : Patch
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
		[Identifier("1a89292b6d7b4487867008596f0de972")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.LivestockAnimal", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2LivestockAnimal_1a89292b6d7b4487867008596f0de972 : Patch
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
		[Identifier("942c587f03124f7f889a8d15bd95b49c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.LivestockAnimal", false)]
		[Parameter("trough", "HitchTrough", false)]
		[Return(typeof(void))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2LivestockAnimal_942c587f03124f7f889a8d15bd95b49c : Patch
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
		[Identifier("812642a3f0b247948873a8cd28c1ca8c")]
		[Dependencies(new string[] { "OnCowMilk [patch 1]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.Cow", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2Cow_812642a3f0b247948873a8cd28c1ca8c : Patch
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
		[Identifier("38420c22746c4d5aaa0a1bbaf69179e3")]
		[Dependencies(new string[] { "OnCowMilk [patch 2]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.Cow", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2Cow_38420c22746c4d5aaa0a1bbaf69179e3 : Patch
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
		[Identifier("76f452c330c842cb8bec2877b6809c40")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2Cow_76f452c330c842cb8bec2877b6809c40 : Patch
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
		[Identifier("5554d1745fa44ab9820dba946e9dc3aa")]
		[Dependencies(new string[] { "OnCowMilk" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Animal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Animal_RustAiGen2Cow_5554d1745fa44ab9820dba946e9dc3aa : Patch
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
