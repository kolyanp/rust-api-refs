using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;
using UnityEngine;

namespace Carbon.Hooks;

public class Category_Resource
{
	public class Resource_ResourceDispenser
	{
		[Patch("OnDispenserGather", "OnDispenserGather", "ResourceDispenser", "GiveResourceFromItem", new string[] { "BasePlayer", "ItemAmount", "System.Single", "System.Single", "AttackEntity" })]
		[Identifier("a2d14e0b8e6f4ce4840294f8c158c4ab")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("entity", "BasePlayer", false)]
		[Parameter("local7", "Item", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_a2d14e0b8e6f4ce4840294f8c158c4ab : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 122)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1345063687)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)7);
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

		[Patch("OnDispenserBonus", "OnDispenserBonus", "ResourceDispenser", "AssignFinishBonus", new string[] { "BasePlayer", "System.Single", "AttackEntity" })]
		[Identifier("f86c40a2e4254400bc4922905a867f0a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local4", "Item", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_f86c40a2e4254400bc4922905a867f0a : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1895285994)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)4);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					instruction.labels.Add(label1);
					object retvar = Generator.DeclareLocal(typeof(object));
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(Item));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(Item));
					yield return __GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 4, typeof(Item));
					yield return instruction;
				}
			}
		}

		[Patch("OnDispenserGathered", "OnDispenserGathered", "ResourceDispenser", "GiveResourceFromItem", new string[] { "BasePlayer", "ItemAmount", "System.Single", "System.Single", "AttackEntity" })]
		[Identifier("d28176138d8b42d3b1fbebf111af9301")]
		[Dependencies(new string[] { "OnDispenserGather" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("entity", "BasePlayer", false)]
		[Parameter("local7", "Item", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_d28176138d8b42d3b1fbebf111af9301 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 152)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1489879586)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)7);
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

		[Patch("OnDispenserBonusReceived", "OnDispenserBonusReceived", "ResourceDispenser", "AssignFinishBonus", new string[] { "BasePlayer", "System.Single", "AttackEntity" })]
		[Identifier("eb53c2bc70aa44b6adf2ba5cd22dfc68")]
		[Dependencies(new string[] { "OnDispenserBonus" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local4", "Item", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_eb53c2bc70aa44b6adf2ba5cd22dfc68 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 82)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1432568158)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)4);
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

	public class Resource_SurveyCharge
	{
		[Patch("OnSurveyGather", "OnSurveyGather", "SurveyCharge", "Explode", new string[] { })]
		[Identifier("d80c5d3bf12541feb5244f4bed827eb4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SurveyCharge", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_SurveyCharge_d80c5d3bf12541feb5244f4bed827eb4 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Type returnType = ((MethodInfo)list2[117].operand).ReturnType;
				object retvar = Generator.DeclareLocal(returnType);
				list.Add(new CodeInstruction(OpCodes.Stloc_S, retvar));
				list.Add(new CodeInstruction(OpCodes.Ldloc_S, retvar));
				list2.InsertRange(118, list);
				Instructions = list2.AsEnumerable();
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 120)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-347039994)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, retvar);
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
	}

	public class Resource_ResourceDepositManager
	{
		[Patch("OnResourceDepositCreated", "OnResourceDepositCreated", "ResourceDepositManager", "CreateFromPosition", new string[] { "UnityEngine.Vector3" })]
		[Identifier("bce9533056da4caa98b1f0c680ea12c1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ResourceDepositManager+ResourceDeposit", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDepositManager_bce9533056da4caa98b1f0c680ea12c1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 255)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-154859386)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Resource_LootContainer
	{
		[Patch("OnLootSpawn", "OnLootSpawn [LootContainer]", "LootContainer", "SpawnLoot", new string[] { })]
		[Identifier("59bf0ed6c2604600b6739f016acfcc73")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "LootContainer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_LootContainer_59bf0ed6c2604600b6739f016acfcc73 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 15)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)767976070), instruction), instruction);
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
	}

	public class Resource_CollectibleEntity
	{
		[Patch("OnCollectiblePickup", "OnCollectiblePickup", "CollectibleEntity", "DoPickup", new string[] { "BasePlayer", "System.Boolean" })]
		[Identifier("8ab59b5ba6874aeab35a8e19d0e43f8e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CollectibleEntity", false)]
		[Parameter("reciever", "BasePlayer", false)]
		[Parameter("eat", "System.Boolean", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CollectibleEntity_8ab59b5ba6874aeab35a8e19d0e43f8e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 4)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1004023405)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
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

		[Patch("OnCollectiblePickedup", "OnCollectiblePickedup", "CollectibleEntity", "DoPickup", new string[] { "BasePlayer", "System.Boolean" })]
		[Identifier("2ae761547b3d47ed906a67ed65556f65")]
		[Dependencies(new string[] { "OnCollectiblePickup" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CollectibleEntity", false)]
		[Parameter("reciever", "BasePlayer", false)]
		[Parameter("local9", "Item", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CollectibleEntity_2ae761547b3d47ed906a67ed65556f65 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 146)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)93268119), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)9);
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

	public class Resource_ExcavatorArm
	{
		[Patch("OnExcavatorGather", "OnExcavatorGather", "ExcavatorArm", "ProduceResources", new string[] { })]
		[Identifier("b08a578a52354253acb8fb2bb1d8a2b2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Parameter("local8", "Item", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_b08a578a52354253acb8fb2bb1d8a2b2 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 82)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1847906595)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)8);
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

		[Patch("OnExcavatorMiningToggled", "OnExcavatorMiningToggled [start]", "ExcavatorArm", "BeginMining", new string[] { })]
		[Identifier("f23f721d96bf4e76ac2e9e66295571aa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_f23f721d96bf4e76ac2e9e66295571aa : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 48)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-819749505)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnExcavatorMiningToggled", "OnExcavatorMiningToggled [stop]", "ExcavatorArm", "StopMining", new string[] { })]
		[Identifier("8fde8ee28bf548f8a68618c595ad4f0e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_8fde8ee28bf548f8a68618c595ad4f0e : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-819749505)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnExcavatorResourceSet", "OnExcavatorResourceSet", "ExcavatorArm", "RPC_SetResourceTarget", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("08432dcc32f34e50b4190179dd15ea45")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Parameter("local0", "System.String", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_08432dcc32f34e50b4190179dd15ea45 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)972234103), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

	public class Resource_GrowableEntity
	{
		[Patch("OnGrowableGathered", "OnGrowableGathered", "GrowableEntity", "GiveFruit", new string[] { "BasePlayer", "System.Int32", "System.Boolean", "System.Boolean" })]
		[Identifier("70cc83fefd87457497f4b26d45ec6ced")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Parameter("local0", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_70cc83fefd87457497f4b26d45ec6ced : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 52)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1431665116)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("OnGrowableGather", "OnGrowableGather", "GrowableEntity", "PickFruit", new string[] { "BasePlayer", "System.Boolean" })]
		[Identifier("f4642b5422e84c45b5f108554574b663")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_f4642b5422e84c45b5f108554574b663 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 5)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1267491132), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
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

		[Patch("OnRemoveDying", "OnRemoveDying", "GrowableEntity", "RemoveDying", new string[] { "BasePlayer" })]
		[Identifier("88a5bb84c254408aa65fd2d788b6dc3e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Parameter("receiver", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_88a5bb84c254408aa65fd2d788b6dc3e : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1776723751), instruction), instruction);
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

		[Patch("OnGrowableStateChange", "OnGrowableStateChange", "GrowableEntity", "ChangeState", new string[] { "PlantProperties/State", "System.Boolean", "System.Boolean" })]
		[Identifier("5cf411788ee543d4b1401d90f7a35c6b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Parameter("state", "PlantProperties+State", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_5cf411788ee543d4b1401d90f7a35c6b : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-787732709)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(State));
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

	public class Resource_MiningQuarry
	{
		[Patch("OnQuarryConsumeFuel", "OnQuarryConsumeFuel", "MiningQuarry", "FuelCheck", new string[] { })]
		[Identifier("5ec68194eb8949038688c0c158aff3bd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MiningQuarry", false)]
		[Parameter("local0", "Item", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_MiningQuarry_5ec68194eb8949038688c0c158aff3bd : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 14)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1723311060), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[3]
					{
						typeof(uint),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					instruction.labels.Add(label1);
					object retvar = Generator.DeclareLocal(typeof(object));
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(Item));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(Item));
					yield return __GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 0, typeof(Item));
					yield return instruction;
				}
			}
		}

		[Patch("OnQuarryGather", "OnQuarryGather", "MiningQuarry", "ProcessResources", new string[] { })]
		[Identifier("e7e11ad6b00d49bcac31c4e005b0d2fa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_MiningQuarry_e7e11ad6b00d49bcac31c4e005b0d2fa : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Expected O, but got Unknown
				//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c3: Expected O, but got Unknown
				//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f5: Expected O, but got Unknown
				//IL_0123: Unknown result type (might be due to invalid IL or missing references)
				//IL_012d: Expected O, but got Unknown
				//IL_014d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0157: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnQuarryGather"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 7, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[118];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 7, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldc_R4, (object)0f));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Item"), "Remove", new Type[1] { typeof(float) }, (Type[])null)));
				Label label2 = Generator.DefineLabel();
				CodeInstruction obj = list2[137];
				list.Add(new CodeInstruction(OpCodes.Br_S, (object)label2));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[118]), list2[118]);
				}
				list2.InsertRange(118, list);
				val.labels.Add(label);
				obj.labels.Add(label2);
				return list2.AsEnumerable();
			}
		}
	}

	public class Resource_CoalingTower
	{
		[Patch("OnCoalingTowerStart", "OnCoalingTowerStart", "CoalingTower", "RPC_Unload", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("d73f9c84fdea4963a15e38c492b888e7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CoalingTower", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CoalingTower_d73f9c84fdea4963a15e38c492b888e7 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1616988590), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnCoalingTowerGather", "OnCoalingTowerGather", "CoalingTower", "EmptyTenPercent", new string[] { })]
		[Identifier("435ca9ae80fb4bfb8937f553f5189037")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CoalingTower_435ca9ae80fb4bfb8937f553f5189037 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_0094: Unknown result type (might be due to invalid IL or missing references)
				//IL_009e: Expected O, but got Unknown
				//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c4: Expected O, but got Unknown
				//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f7: Expected O, but got Unknown
				//IL_0125: Unknown result type (might be due to invalid IL or missing references)
				//IL_012f: Expected O, but got Unknown
				//IL_014f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0159: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnCoalingTowerGather"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 13, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[123];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 13, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldc_R4, (object)0f));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Item"), "Remove", new Type[1] { typeof(float) }, (Type[])null)));
				Label label2 = Generator.DefineLabel();
				CodeInstruction obj = list2[148];
				list.Add(new CodeInstruction(OpCodes.Br_S, (object)label2));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[123]), list2[123]);
				}
				list2.InsertRange(123, list);
				val.labels.Add(label);
				obj.labels.Add(label2);
				return list2.AsEnumerable();
			}
		}
	}

	public class Resource_EngineSwitch
	{
		[Patch("OnQuarryToggle", "OnQuarryToggle [on]", "EngineSwitch", "StartEngine", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("7970026f2c4f4ccc96ecd110a93c9821")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_7970026f2c4f4ccc96ecd110a93c9821 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1897209243)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnQuarryToggle", "OnQuarryToggle [off]", "EngineSwitch", "StopEngine", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("e4ac7df144564b55acdb216f3dcd1797")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_e4ac7df144564b55acdb216f3dcd1797 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1897209243)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnQuarryToggled", "OnQuarryToggled [off]", "EngineSwitch", "StopEngine", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("ec3ebfc50d454d8bb4e8441a663c1c30")]
		[Dependencies(new string[] { "OnQuarryToggle [off]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_ec3ebfc50d454d8bb4e8441a663c1c30 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 18)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1754663591), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnQuarryToggled", "OnQuarryToggled [on]", "EngineSwitch", "StartEngine", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("716ca34f48864c37a3a9a6ae3b0488f0")]
		[Dependencies(new string[] { "OnQuarryToggle [on]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_716ca34f48864c37a3a9a6ae3b0488f0 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 18)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1754663591), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnQuarryToggled", "OnQuarryToggled [off] [patch]", "EngineSwitch", "StopEngine", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("38587fe6014e466b93c1c35cf7bae059")]
		[Dependencies(new string[] { "OnQuarryToggled [off]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_38587fe6014e466b93c1c35cf7bae059 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[14];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[6]), list2[6]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[7], list2[6]), list2[6]);
				}
				list2.RemoveRange(6, 1);
				list2.InsertRange(6, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnQuarryToggled", "OnQuarryToggled [on] [patch]", "EngineSwitch", "StartEngine", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("336a19b2feb448c8a216eb7c653df69b")]
		[Dependencies(new string[] { "OnQuarryToggled [on]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_336a19b2feb448c8a216eb7c653df69b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[14];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[6]), list2[6]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[7], list2[6]), list2[6]);
				}
				list2.RemoveRange(6, 1);
				list2.InsertRange(6, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Resource_LootFill
	{
		[Patch("OnLootSpawn", "OnLootSpawn [LootFill]", "LootFill", "DelayFill", new string[] { })]
		[Identifier("990d34d646e54158915c93e1199d4759")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "LootFill", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_LootFill_990d34d646e54158915c93e1199d4759 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)767976070), instruction), instruction);
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
	}

	public class Resource_RandomItemDispenser
	{
		[Patch("OnRandomItemAward", "OnRandomItemAward", "RandomItemDispenser", "TryAward", new string[] { "RandomItemDispenser/RandomItemChance", "BasePlayer", "UnityEngine.Vector3" })]
		[Identifier("940f4a206bc64fb7aaae77fea3e55899")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_RandomItemDispenser_940f4a206bc64fb7aaae77fea3e55899 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected O, but got Unknown
				//IL_0054: Unknown result type (might be due to invalid IL or missing references)
				//IL_005e: Expected O, but got Unknown
				//IL_0065: Unknown result type (might be due to invalid IL or missing references)
				//IL_006f: Expected O, but got Unknown
				//IL_0076: Unknown result type (might be due to invalid IL or missing references)
				//IL_0080: Expected O, but got Unknown
				//IL_0090: Unknown result type (might be due to invalid IL or missing references)
				//IL_009a: Expected O, but got Unknown
				//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
				//IL_0106: Expected O, but got Unknown
				//IL_0120: Unknown result type (might be due to invalid IL or missing references)
				//IL_012a: Expected O, but got Unknown
				//IL_0131: Unknown result type (might be due to invalid IL or missing references)
				//IL_013b: Expected O, but got Unknown
				//IL_0142: Unknown result type (might be due to invalid IL or missing references)
				//IL_014c: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnRandomItemAward"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Box, (object)typeof(RandomItemChance)));
				list.Add(new CodeInstruction(OpCodes.Ldarg_2, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldarg_3, (object)null));
				list.Add(new CodeInstruction(OpCodes.Box, (object)typeof(Vector3)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[5]
				{
					typeof(string),
					typeof(object),
					typeof(object),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[0];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldc_I4_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[0]), list2[0]);
				}
				list2.InsertRange(0, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
