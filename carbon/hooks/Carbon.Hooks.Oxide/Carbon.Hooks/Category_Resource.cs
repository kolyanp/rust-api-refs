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
		[Identifier("7a372f7fa15e44f7bfc462c6d3364114")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("entity", "BasePlayer", false)]
		[Parameter("local7", "Item", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_7a372f7fa15e44f7bfc462c6d3364114 : Patch
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
		[Identifier("fa486f82f5f44aaaad4606b56d3a49b6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local4", "Item", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_fa486f82f5f44aaaad4606b56d3a49b6 : Patch
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
		[Identifier("23875b93e696489fb381f97ea99f6b40")]
		[Dependencies(new string[] { "OnDispenserGather" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("entity", "BasePlayer", false)]
		[Parameter("local7", "Item", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_23875b93e696489fb381f97ea99f6b40 : Patch
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
		[Identifier("c0dfd2fd21454d149bb89538014b8e8e")]
		[Dependencies(new string[] { "OnDispenserBonus" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local4", "Item", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_c0dfd2fd21454d149bb89538014b8e8e : Patch
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
		[Identifier("ae25c314cd4f42a8844710dcba97c972")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SurveyCharge", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_SurveyCharge_ae25c314cd4f42a8844710dcba97c972 : Patch
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
		[Identifier("0745debbe8604d1aabf65bfb2ef898dd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ResourceDepositManager+ResourceDeposit", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDepositManager_0745debbe8604d1aabf65bfb2ef898dd : Patch
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
		[Identifier("af25294be65e404f850be1f98e95b53e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "LootContainer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_LootContainer_af25294be65e404f850be1f98e95b53e : Patch
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
		[Identifier("cbc3b6b385e644ebb63826ee73e80b7c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CollectibleEntity", false)]
		[Parameter("reciever", "BasePlayer", false)]
		[Parameter("eat", "System.Boolean", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CollectibleEntity_cbc3b6b385e644ebb63826ee73e80b7c : Patch
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
		[Identifier("b497ad4fe5c942858e96baa3cba3e09d")]
		[Dependencies(new string[] { "OnCollectiblePickup" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CollectibleEntity", false)]
		[Parameter("reciever", "BasePlayer", false)]
		[Parameter("local9", "Item", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CollectibleEntity_b497ad4fe5c942858e96baa3cba3e09d : Patch
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
		[Identifier("0a478a7581044a44b0992b36b8e7aae3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Parameter("local8", "Item", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_0a478a7581044a44b0992b36b8e7aae3 : Patch
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
		[Identifier("539dc179145241d69095a3325e3ce2ad")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_539dc179145241d69095a3325e3ce2ad : Patch
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
		[Identifier("92c8a6ef39334acb849e975294eb687d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_92c8a6ef39334acb849e975294eb687d : Patch
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
		[Identifier("93ac0c4583ee4df594a81062c530b8be")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Parameter("local0", "System.String", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_93ac0c4583ee4df594a81062c530b8be : Patch
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
		[Identifier("a12c714cd51d48c18908a4d49a17adff")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Parameter("local0", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_a12c714cd51d48c18908a4d49a17adff : Patch
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
		[Identifier("f2b2e5926db740ceb6bc1423b5eb0a3c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_f2b2e5926db740ceb6bc1423b5eb0a3c : Patch
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
		[Identifier("8f48b09124a543139d75b9d2c04dc4a1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Parameter("receiver", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_8f48b09124a543139d75b9d2c04dc4a1 : Patch
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
		[Identifier("c5d8a46dd03f49e6aed9f140a9b16343")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Parameter("state", "PlantProperties+State", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_c5d8a46dd03f49e6aed9f140a9b16343 : Patch
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
		[Identifier("744f13446f13451fb691bcd6be28e0ab")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MiningQuarry", false)]
		[Parameter("local0", "Item", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_MiningQuarry_744f13446f13451fb691bcd6be28e0ab : Patch
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
		[Identifier("aba23828398045a584bbdff0dd6764c1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_MiningQuarry_aba23828398045a584bbdff0dd6764c1 : Patch
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
		[Identifier("530344fd17dc4654bd1ebe808f373058")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CoalingTower", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CoalingTower_530344fd17dc4654bd1ebe808f373058 : Patch
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
		[Identifier("086db7efb83f4c3f84ffa3555789dad3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CoalingTower_086db7efb83f4c3f84ffa3555789dad3 : Patch
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
		[Identifier("859a756967024de997e82b7c3958fff0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_859a756967024de997e82b7c3958fff0 : Patch
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
		[Identifier("4c99504e7598422bbe407744b1b48881")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_4c99504e7598422bbe407744b1b48881 : Patch
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
		[Identifier("9d0337252f884408983709660254030f")]
		[Dependencies(new string[] { "OnQuarryToggle [off]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_9d0337252f884408983709660254030f : Patch
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
		[Identifier("9e6aef5bed6e48b6bd8ca7391825cfc3")]
		[Dependencies(new string[] { "OnQuarryToggle [on]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_9e6aef5bed6e48b6bd8ca7391825cfc3 : Patch
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
		[Identifier("d8aef7082edd4b55a21b36c7b4fb3c27")]
		[Dependencies(new string[] { "OnQuarryToggled [off]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_d8aef7082edd4b55a21b36c7b4fb3c27 : Patch
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
		[Identifier("8dd8807efbed46bd96034482c8a0d1f8")]
		[Dependencies(new string[] { "OnQuarryToggled [on]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_8dd8807efbed46bd96034482c8a0d1f8 : Patch
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
		[Identifier("7308d42f85c04d85bb687af477140d5b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "LootFill", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_LootFill_7308d42f85c04d85bb687af477140d5b : Patch
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
		[Identifier("0410cb6a580b4fb69ab18d8f4767f4a4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_RandomItemDispenser_0410cb6a580b4fb69ab18d8f4767f4a4 : Patch
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
