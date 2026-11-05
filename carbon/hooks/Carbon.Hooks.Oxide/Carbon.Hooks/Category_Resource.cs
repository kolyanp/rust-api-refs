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
		[Identifier("4a546e13a54846129461360ffcc16549")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("entity", "BasePlayer", false)]
		[Parameter("local7", "Item", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_4a546e13a54846129461360ffcc16549 : Patch
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
		[Identifier("9720e328084649acb3225ecd4d2850c7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local5", "Item", false)]
		[Return(typeof(Item), Continues = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_9720e328084649acb3225ecd4d2850c7 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 57)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1895285994)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
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
					yield return __GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 5, typeof(Item));
					yield return instruction;
				}
			}
		}

		[Patch("OnDispenserGathered", "OnDispenserGathered", "ResourceDispenser", "GiveResourceFromItem", new string[] { "BasePlayer", "ItemAmount", "System.Single", "System.Single", "AttackEntity" })]
		[Identifier("648815991aaa4faabd2379d2524b6ac0")]
		[Dependencies(new string[] { "OnDispenserGather" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("entity", "BasePlayer", false)]
		[Parameter("local7", "Item", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_648815991aaa4faabd2379d2524b6ac0 : Patch
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
		[Identifier("c40f9844face404b9c5a7c1c7c4e082f")]
		[Dependencies(new string[] { "OnDispenserBonus" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local5", "Item", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_c40f9844face404b9c5a7c1c7c4e082f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 84)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1432568158)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
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
		[Identifier("7b70211c764b467c8d5943659fb6da2a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SurveyCharge", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_SurveyCharge_7b70211c764b467c8d5943659fb6da2a : Patch
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
		[Identifier("2932ae84c9294a4cb6401cf47ad7578d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ResourceDepositManager+ResourceDeposit", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDepositManager_2932ae84c9294a4cb6401cf47ad7578d : Patch
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
		[Identifier("d64f57d74f7f4d05993f17f3ea8f03dd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "LootContainer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_LootContainer_d64f57d74f7f4d05993f17f3ea8f03dd : Patch
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
		[Identifier("7591ba4111594746bb1f5cfdcd0b9a51")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CollectibleEntity", false)]
		[Parameter("reciever", "BasePlayer", false)]
		[Parameter("eat", "System.Boolean", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CollectibleEntity_7591ba4111594746bb1f5cfdcd0b9a51 : Patch
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
		[Identifier("fe247f3c9c75452e99cdc352b85ab762")]
		[Dependencies(new string[] { "OnCollectiblePickup" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CollectibleEntity", false)]
		[Parameter("reciever", "BasePlayer", false)]
		[Parameter("local8", "Item", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CollectibleEntity_fe247f3c9c75452e99cdc352b85ab762 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 128)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)93268119), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)8);
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
		[Identifier("0266c2ca336f45a5a1089cb787d4e602")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Parameter("local8", "Item", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_0266c2ca336f45a5a1089cb787d4e602 : Patch
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
		[Identifier("bb0691a2d7724a61968e9567b4754fb7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_bb0691a2d7724a61968e9567b4754fb7 : Patch
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
		[Identifier("4938d85619bc4c99a1f11395983a2ca4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_4938d85619bc4c99a1f11395983a2ca4 : Patch
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
		[Identifier("562d550d862243fb9956bc69f5f08b81")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Parameter("local0", "System.String", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_562d550d862243fb9956bc69f5f08b81 : Patch
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
		[Identifier("eeefcb2379744d13943651bf68e9cf2c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Parameter("local0", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_eeefcb2379744d13943651bf68e9cf2c : Patch
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
		[Identifier("62a751f4176f4e2799966c07cfaa970e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_62a751f4176f4e2799966c07cfaa970e : Patch
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
		[Identifier("9b9a11d160ff41728083071cc9fb0bb8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Parameter("receiver", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_9b9a11d160ff41728083071cc9fb0bb8 : Patch
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
		[Identifier("a2394c2a6e2b45b8a0d04f71a9949d88")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Parameter("state", "PlantProperties+State", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_a2394c2a6e2b45b8a0d04f71a9949d88 : Patch
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
		[Identifier("a3ad9b6e51534e2490015ce2a0faf3e0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MiningQuarry", false)]
		[Parameter("local0", "Item", false)]
		[Return(typeof(Item), Continues = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_MiningQuarry_a3ad9b6e51534e2490015ce2a0faf3e0 : Patch
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
		[Identifier("3b7f3dfce5494b279aa551b04ae0898a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_MiningQuarry_3b7f3dfce5494b279aa551b04ae0898a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Expected Obj, but got Unknown
				//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c3: Expected Obj, but got Unknown
				//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f5: Expected Obj, but got Unknown
				//IL_0123: Unknown result type (might be due to invalid IL or missing references)
				//IL_012d: Expected Obj, but got Unknown
				//IL_014a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0154: Expected Obj, but got Unknown
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
				CodeInstruction val = list2[107];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 7, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldc_R4, (object)0f));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Item"), "Remove", new Type[1] { typeof(float) }, (Type[])null)));
				Label label2 = Generator.DefineLabel();
				CodeInstruction val2 = list2[126];
				list.Add(new CodeInstruction(OpCodes.Br_S, (object)label2));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[107]), list2[107]);
				}
				list2.InsertRange(107, list);
				val.labels.Add(label);
				val2.labels.Add(label2);
				return list2.AsEnumerable();
			}
		}
	}

	public class Resource_CoalingTower
	{
		[Patch("OnCoalingTowerStart", "OnCoalingTowerStart", "CoalingTower", "RPC_Unload", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("518d8d233da443d293baf80bd06c5e30")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CoalingTower", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CoalingTower_518d8d233da443d293baf80bd06c5e30 : Patch
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
		[Identifier("54a2143f67d245d896a787cc6b51caf6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CoalingTower_54a2143f67d245d896a787cc6b51caf6 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_0094: Unknown result type (might be due to invalid IL or missing references)
				//IL_009e: Expected Obj, but got Unknown
				//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c4: Expected Obj, but got Unknown
				//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f7: Expected Obj, but got Unknown
				//IL_0125: Unknown result type (might be due to invalid IL or missing references)
				//IL_012f: Expected Obj, but got Unknown
				//IL_014f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0159: Expected Obj, but got Unknown
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
				CodeInstruction val2 = list2[148];
				list.Add(new CodeInstruction(OpCodes.Br_S, (object)label2));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[123]), list2[123]);
				}
				list2.InsertRange(123, list);
				val.labels.Add(label);
				val2.labels.Add(label2);
				return list2.AsEnumerable();
			}
		}
	}

	public class Resource_EngineSwitch
	{
		[Patch("OnQuarryToggle", "OnQuarryToggle [on]", "EngineSwitch", "StartEngine", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("0d2a1f9714674c6a9f14f2e61a0791ce")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_0d2a1f9714674c6a9f14f2e61a0791ce : Patch
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
		[Identifier("1c01e988e4e84c6f96720a40bd96f728")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_1c01e988e4e84c6f96720a40bd96f728 : Patch
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
		[Identifier("a655fb2ed1664f67bdf78b8ba44b87db")]
		[Dependencies(new string[] { "OnQuarryToggle [off]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_a655fb2ed1664f67bdf78b8ba44b87db : Patch
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
		[Identifier("d70377de81b04607b96fe45065d741b6")]
		[Dependencies(new string[] { "OnQuarryToggle [on]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_d70377de81b04607b96fe45065d741b6 : Patch
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
		[Identifier("9719336266594daab4c58bc292fe4fb7")]
		[Dependencies(new string[] { "OnQuarryToggled [off]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_9719336266594daab4c58bc292fe4fb7 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[14];
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
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnQuarryToggled", "OnQuarryToggled [on] [patch]", "EngineSwitch", "StartEngine", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("d6421f515e244a1e89f8d1485773b6af")]
		[Dependencies(new string[] { "OnQuarryToggled [on]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_d6421f515e244a1e89f8d1485773b6af : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[14];
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
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Resource_LootFill
	{
		[Patch("OnLootSpawn", "OnLootSpawn [LootFill]", "LootFill", "DelayFill", new string[] { })]
		[Identifier("3351ea29090846288144f56bced2ccd7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "LootFill", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_LootFill_3351ea29090846288144f56bced2ccd7 : Patch
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
		[Identifier("f4259bfa0a174a6095d822d147e2f305")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_RandomItemDispenser_f4259bfa0a174a6095d822d147e2f305 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected Obj, but got Unknown
				//IL_0054: Unknown result type (might be due to invalid IL or missing references)
				//IL_005e: Expected Obj, but got Unknown
				//IL_0065: Unknown result type (might be due to invalid IL or missing references)
				//IL_006f: Expected Obj, but got Unknown
				//IL_0076: Unknown result type (might be due to invalid IL or missing references)
				//IL_0080: Expected Obj, but got Unknown
				//IL_0090: Unknown result type (might be due to invalid IL or missing references)
				//IL_009a: Expected Obj, but got Unknown
				//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
				//IL_0106: Expected Obj, but got Unknown
				//IL_0120: Unknown result type (might be due to invalid IL or missing references)
				//IL_012a: Expected Obj, but got Unknown
				//IL_0131: Unknown result type (might be due to invalid IL or missing references)
				//IL_013b: Expected Obj, but got Unknown
				//IL_0142: Unknown result type (might be due to invalid IL or missing references)
				//IL_014c: Expected Obj, but got Unknown
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
				CodeInstruction val = list2[0];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldc_I4_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[0]), list2[0]);
				}
				list2.InsertRange(0, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
