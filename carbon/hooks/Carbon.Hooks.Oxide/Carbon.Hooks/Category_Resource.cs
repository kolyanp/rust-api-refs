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
		[Identifier("729f18011a6547c4ae81588b0c7f4325")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("entity", "BasePlayer", false)]
		[Parameter("local7", "Item", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_729f18011a6547c4ae81588b0c7f4325 : Patch
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
		[Identifier("1bf4f2828d264a4bb31e0a1113b96854")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local5", "Item", false)]
		[Return(typeof(Item), Continues = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_1bf4f2828d264a4bb31e0a1113b96854 : Patch
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
		[Identifier("2b0199d95cc2401087676af1d265030c")]
		[Dependencies(new string[] { "OnDispenserGather" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("entity", "BasePlayer", false)]
		[Parameter("local7", "Item", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_2b0199d95cc2401087676af1d265030c : Patch
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
		[Identifier("f5935a29309846cc8d27267cef2ce231")]
		[Dependencies(new string[] { "OnDispenserBonus" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceDispenser", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local5", "Item", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDispenser_f5935a29309846cc8d27267cef2ce231 : Patch
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
		[Identifier("4c17466905954c1591f92977afa45494")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SurveyCharge", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_SurveyCharge_4c17466905954c1591f92977afa45494 : Patch
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
		[Identifier("a157de39f6914a348ab550b50cc1eecd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ResourceDepositManager+ResourceDeposit", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ResourceDepositManager_a157de39f6914a348ab550b50cc1eecd : Patch
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
		[Identifier("479022ff0e1e49f4b1082ba1f42cbcf8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "LootContainer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_LootContainer_479022ff0e1e49f4b1082ba1f42cbcf8 : Patch
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
		[Identifier("18ef7cabe1b44181bc073bcd57053e4a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CollectibleEntity", false)]
		[Parameter("reciever", "BasePlayer", false)]
		[Parameter("eat", "System.Boolean", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CollectibleEntity_18ef7cabe1b44181bc073bcd57053e4a : Patch
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
		[Identifier("28935a7a5f904af59d734d03bfd072e0")]
		[Dependencies(new string[] { "OnCollectiblePickup" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CollectibleEntity", false)]
		[Parameter("reciever", "BasePlayer", false)]
		[Parameter("local8", "Item", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CollectibleEntity_28935a7a5f904af59d734d03bfd072e0 : Patch
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
		[Identifier("12478658ccc74306830c0845c7158376")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Parameter("local8", "Item", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_12478658ccc74306830c0845c7158376 : Patch
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
		[Identifier("5dfb80c0a0d0436b8f5e1732feb46629")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_5dfb80c0a0d0436b8f5e1732feb46629 : Patch
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
		[Identifier("eef4ddb0a653421bb705c88792316f9f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_eef4ddb0a653421bb705c88792316f9f : Patch
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
		[Identifier("808d55de6d4f4240bba8b97d712222a0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorArm", false)]
		[Parameter("local0", "System.String", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_ExcavatorArm_808d55de6d4f4240bba8b97d712222a0 : Patch
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
		[Identifier("0799180ad1914404b3027fa95406f5d5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Parameter("local0", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_0799180ad1914404b3027fa95406f5d5 : Patch
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
		[Identifier("e99a9f958b3849c583def01c34239dc5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_e99a9f958b3849c583def01c34239dc5 : Patch
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
		[Identifier("7fd1a59048a44a818f543bb8f5cc4b28")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Parameter("receiver", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_7fd1a59048a44a818f543bb8f5cc4b28 : Patch
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
		[Identifier("653098771d6f472a97c966d61cb57c35")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "GrowableEntity", false)]
		[Parameter("state", "PlantProperties+State", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_GrowableEntity_653098771d6f472a97c966d61cb57c35 : Patch
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
		[Identifier("8f6ac905ea1d444ba1e19130e616d9e7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MiningQuarry", false)]
		[Parameter("local0", "Item", false)]
		[Return(typeof(Item), Continues = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_MiningQuarry_8f6ac905ea1d444ba1e19130e616d9e7 : Patch
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
		[Identifier("3a2b11471c00444094221a72771d6a36")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_MiningQuarry_3a2b11471c00444094221a72771d6a36 : Patch
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
		[Identifier("e5612bf47ff348638bbc7eaa9de088cd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CoalingTower", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CoalingTower_e5612bf47ff348638bbc7eaa9de088cd : Patch
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
		[Identifier("507bb61e8ea84e1dac1e0a6a30909d1d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_CoalingTower_507bb61e8ea84e1dac1e0a6a30909d1d : Patch
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
		[Identifier("62af2087dde746db9a38aa067b491c7b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_62af2087dde746db9a38aa067b491c7b : Patch
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
		[Identifier("fe6d9b850f344b5e948459722846747e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_fe6d9b850f344b5e948459722846747e : Patch
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
		[Identifier("d3fc3935899f4be59d4bd727f7100a3a")]
		[Dependencies(new string[] { "OnQuarryToggle [off]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_d3fc3935899f4be59d4bd727f7100a3a : Patch
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
		[Identifier("6d41f76632e14fbd9770e7e5af819216")]
		[Dependencies(new string[] { "OnQuarryToggle [on]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "MiningQuarry", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_6d41f76632e14fbd9770e7e5af819216 : Patch
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
		[Identifier("7f35af316be64a39bdc64b3edb83a6af")]
		[Dependencies(new string[] { "OnQuarryToggled [off]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_7f35af316be64a39bdc64b3edb83a6af : Patch
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
		[Identifier("599f5088606145bfab61bab55c27a719")]
		[Dependencies(new string[] { "OnQuarryToggled [on]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_EngineSwitch_599f5088606145bfab61bab55c27a719 : Patch
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
		[Identifier("2644e7aa430240a996cb388165a1e874")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "LootFill", false)]
		[Return(typeof(void))]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_LootFill_2644e7aa430240a996cb388165a1e874 : Patch
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
		[Identifier("a3bcaca227ad4bd3981e9960cf897ee3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Resource")]
		[Assembly("Assembly-CSharp.dll")]
		public class Resource_RandomItemDispenser_a3bcaca227ad4bd3981e9960cf897ee3 : Patch
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
