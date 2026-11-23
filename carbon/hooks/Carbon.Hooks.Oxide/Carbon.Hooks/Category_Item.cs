using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Core;
using Carbon.Extensions;
using HarmonyLib;
using Rust;

namespace Carbon.Hooks;

public class Category_Item
{
	public class Item_ItemContainer
	{
		[Patch("OnItemRemovedFromContainer", "OnItemRemovedFromContainer", "ItemContainer", "Remove", new string[] { "Item", "BasePlayer" })]
		[Identifier("9ceb6c8fb6034180a2ba0538ea38ae71")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ItemContainer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemContainer_9ceb6c8fb6034180a2ba0538ea38ae71 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 81)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)470300595), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
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

		[Patch("OnItemAddedToContainer", "OnItemAddedToContainer", "ItemContainer", "Insert", new string[] { "Item", "BasePlayer" })]
		[Identifier("93e80ce55b324490a9a81fa275bbaf55")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ItemContainer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemContainer_93e80ce55b324490a9a81fa275bbaf55 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 70)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)199161889), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
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

		[Patch("CanAcceptItem", "CanAcceptItem", "ItemContainer", "CanAcceptItem", new string[] { "BasePlayer", "Item", "System.Int32" })]
		[Identifier("5729c28f8a41413e9025e6c98741dc06")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ItemContainer", false)]
		[Parameter("item", "Item", false)]
		[Parameter("targetPos", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(CanAcceptResult))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemContainer_5729c28f8a41413e9025e6c98741dc06 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 117)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1360889797), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(CanAcceptResult));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(CanAcceptResult));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Item_ItemCrafter
	{
		[Patch("OnItemCraft", "OnItemCraft", "ItemCrafter", "CraftItem", new string[] { "ItemBlueprint", "BasePlayer", "ProtoBuf.Item/InstanceData", "System.Int32", "System.Int32", "Item", "System.Boolean", "System.Int32" })]
		[Identifier("c405d80053a04720b7f032fb534bd050")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "ItemCraftTask", false)]
		[Parameter("owner", "BasePlayer", false)]
		[Parameter("fromTempBlueprint", "Item", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemCrafter_c405d80053a04720b7f032fb534bd050 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)276522030), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)6);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
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

		[Patch("OnItemCraftFinished", "OnItemCraftFinished", "ItemCrafter", "FinishCrafting", new string[] { "ItemCraftTask" })]
		[Identifier("05b372e3a0d74be4a199920cb63cb650")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("task", "ItemCraftTask", false)]
		[Parameter("local1", "Item", false)]
		[Parameter("self", "ItemCrafter", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemCrafter_05b372e3a0d74be4a199920cb63cb650 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 216)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)659159968), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnItemCraftCancelled", "OnItemCraftCancelled", "ItemCrafter", "CancelTask", new string[] { "System.Int32" })]
		[Identifier("42e3e34050d94456bd51f3d0c02ab592")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ItemCraftTask", false)]
		[Parameter("self", "ItemCrafter", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemCrafter_42e3e34050d94456bd51f3d0c02ab592 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 38)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)195516419), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("CanFastTrackCraftTask", "CanFastTrackCraftTask", "ItemCrafter", "FastTrackTask", new string[] { "System.Int32" })]
		[Identifier("83bfd3a306af41669d0481ce3c97dcb3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ItemCrafter", false)]
		[Parameter("local2", "ItemCraftTask", false)]
		[Parameter("taskID", "System.Int32", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemCrafter_83bfd3a306af41669d0481ce3c97dcb3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 47)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1393276715)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
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
	}

	public class Item_Deployer
	{
		[Patch("OnItemDeployed", "OnItemDeployed [Regular]", "Deployer", "DoDeploy_Regular", new string[] { "Deployable", "UnityEngine.Ray" })]
		[Identifier("9c9b08f3fa844847b286137c6f806442")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Deployer", false)]
		[Parameter("local5", "ItemModDeployable", false)]
		[Parameter("local6", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Deployer_9c9b08f3fa844847b286137c6f806442 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 142)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)470287711), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)6);
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

		[Patch("OnItemDeployed", "OnItemDeployed [Slot]", "Deployer", "DoDeploy_Slot", new string[] { "Deployable", "UnityEngine.Ray", "NetworkableId" })]
		[Identifier("46ca24f99c5444ea8fcccc5668cfb89d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Deployer", false)]
		[Parameter("local1", "BaseEntity", false)]
		[Parameter("local4", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Deployer_46ca24f99c5444ea8fcccc5668cfb89d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 207)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)470287711), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

		[Patch("CanDeployItem", "CanDeployItem", "Deployer", "DoDeploy", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("c2649f3bfb404c6390404684801e074a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "Deployer", false)]
		[Parameter("local2", "NetworkableId", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Deployer_c2649f3bfb404c6390404684801e074a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 21)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)208822718), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("NetworkableId"));
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

	public class Item_Item
	{
		[Patch("IOnLoseCondition", "IOnLoseCondition", "Item", "LoseCondition", new string[] { "System.Single" })]
		[Identifier("8552f1b43d824dec9e15da5bb5e6180e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Item", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_8552f1b43d824dec9e15da5bb5e6180e : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldarg_0, (object)null), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnLoseCondition", (Type[])null, (Type[])null));
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnItemUse", "OnItemUse", "Item", "UseItem", new string[] { "System.Int32" })]
		[Identifier("0a9b1cad2d1c4944a470c55d1c996d77")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Item", false)]
		[Parameter("amountToConsume", "System.Int32", false)]
		[Return(typeof(int), Continues = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_0a9b1cad2d1c4944a470c55d1c996d77 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1397860695)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Brfalse_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Starg, (object)1);
					yield return instruction;
				}
			}
		}

		[Patch("OnItemSplit", "OnItemSplit", "Item", "SplitItem", new string[] { "System.Int32" })]
		[Identifier("de56d9c77707408e8bac1d15ebf9afae")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Item", false)]
		[Return(typeof(Item))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_de56d9c77707408e8bac1d15ebf9afae : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)983035860), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(Item));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(Item));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("CanStackItem", "CanStackItem", "Item", "CanStack", new string[] { "Item" })]
		[Identifier("60bec9e11bb74af29cc17c09d4b7a8fa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Item", false)]
		[Parameter("item", "Item", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_60bec9e11bb74af29cc17c09d4b7a8fa : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1700188150)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("OnItemDropped", "OnItemDropped", "Item", "Drop", new string[] { "UnityEngine.Vector3", "UnityEngine.Vector3", "UnityEngine.Quaternion" })]
		[Identifier("3c7f917ade1e41a38f7093efcd545c3b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Item", false)]
		[Parameter("local1", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_3c7f917ade1e41a38f7093efcd545c3b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 102)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)226172740), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

		[Patch("OnMaxStackable", "OnMaxStackable", "Item", "MaxStackable", new string[] { })]
		[Identifier("5971c470394f41698200369650f85874")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Item", false)]
		[Return(typeof(int))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_5971c470394f41698200369650f85874 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 29)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)418610024), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnItemRemove", "OnItemRemove", "Item", "Remove", new string[] { "System.Single" })]
		[Identifier("e20983a3e45f435db1f3111633bc847d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Item", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_e20983a3e45f435db1f3111633bc847d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-276075129)), instruction), instruction);
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

		[Patch("OnItemStacked", "OnItemStacked [1]", "Item", "MoveToContainer", new string[] { "ItemContainer", "System.Int32", "System.Boolean", "System.Boolean", "BasePlayer", "System.Boolean" })]
		[Identifier("a265c59c19344945b8a42443796ab10c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local13", "Item", false)]
		[Parameter("self", "Item", false)]
		[Parameter("newcontainer", "ItemContainer", false)]
		[Parameter("local15", "System.Int32", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_a265c59c19344945b8a42443796ab10c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 304)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)746311991), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)13);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)15);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnItemStacked", "OnItemStacked [2]", "Item", "MoveToContainer", new string[] { "ItemContainer", "System.Int32", "System.Boolean", "System.Boolean", "BasePlayer", "System.Boolean" })]
		[Identifier("6fc91b6ee42c477d997635a655fd4555")]
		[Dependencies(new string[] { "OnItemStacked [1]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local23", "Item", false)]
		[Parameter("self", "Item", false)]
		[Parameter("newcontainer", "ItemContainer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_6fc91b6ee42c477d997635a655fd4555 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 623)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)746311991), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)23);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnItemLock", "OnItemLock", "Item", "LockUnlock", new string[] { "System.Boolean" })]
		[Identifier("f98bcbe24f5440819b1cec9a0db887e0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_f98bcbe24f5440819b1cec9a0db887e0 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected Obj, but got Unknown
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_0043: Expected Obj, but got Unknown
				//IL_004e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0058: Expected Obj, but got Unknown
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0069: Expected Obj, but got Unknown
				//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ae: Expected Obj, but got Unknown
				//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bf: Expected Obj, but got Unknown
				//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d5: Expected Obj, but got Unknown
				//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e6: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[6];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnItemLock"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[2]
				{
					typeof(string),
					typeof(object)
				}, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Ldnull, (object)null));
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[6]), list2[6]);
				}
				list2.InsertRange(6, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnItemUnlock", "OnItemUnlock", "Item", "LockUnlock", new string[] { "System.Boolean" })]
		[Identifier("624cff5760584711987c49480893cfd8")]
		[Dependencies(new string[] { "OnItemLock" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_624cff5760584711987c49480893cfd8 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected Obj, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected Obj, but got Unknown
				//IL_004f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0059: Expected Obj, but got Unknown
				//IL_0060: Unknown result type (might be due to invalid IL or missing references)
				//IL_006a: Expected Obj, but got Unknown
				//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00af: Expected Obj, but got Unknown
				//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c0: Expected Obj, but got Unknown
				//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d6: Expected Obj, but got Unknown
				//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e7: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[14];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnItemUnlock"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[2]
				{
					typeof(string),
					typeof(object)
				}, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Ldnull, (object)null));
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[14]), list2[14]);
				}
				list2.InsertRange(14, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Item_MedicalTool
	{
		[Patch("OnHealingItemUse", "OnHealingItemUse", "MedicalTool", "GiveEffectsTo", new string[] { "BasePlayer", "IMedicalToolTarget" })]
		[Identifier("bffc18bc1263468fab05a4f612e532b5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MedicalTool", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_MedicalTool_bffc18bc1263468fab05a4f612e532b5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 31)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1562894683)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
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

	public class Item_ResearchTable
	{
		[Patch("OnItemResearch", "OnItemResearch", "ResearchTable", "DoResearch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("6add02aef08d4264bb20c8fb91edadbd")]
		[Dependencies(new string[] { "CanResearchItem" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResearchTable", false)]
		[Parameter("local1", "Item", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ResearchTable_6add02aef08d4264bb20c8fb91edadbd : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-982336151)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnItemResearched", "OnItemResearched", "ResearchTable", "ResearchAttemptFinished", new string[] { })]
		[Identifier("29a8249095f5423a9b915b3cb50aa9f1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResearchTable", false)]
		[Parameter("local2", "System.Int32", false)]
		[Return(typeof(int), Continues = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ResearchTable_29a8249095f5423a9b915b3cb50aa9f1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 13)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2094586654), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
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
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(int));
					yield return __GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 2, typeof(int));
					yield return instruction;
				}
			}
		}

		[Patch("OnResearchCostDetermine", "OnResearchCostDetermine [Item]", "ResearchTable", "ScrapForResearch", new string[] { "Item" })]
		[Identifier("b2bf53f7d8e8484087c49ae4972678ff")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Return(typeof(int))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ResearchTable_b2bf53f7d8e8484087c49ae4972678ff : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1250819912)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnResearchCostDetermine", "OnResearchCostDetermine [ItemDef]", "ResearchTable", "ScrapForResearch", new string[] { "ItemDefinition" })]
		[Identifier("4a4b42d901344ede850ffc74f58c677b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("info", "ItemDefinition", false)]
		[Return(typeof(int))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ResearchTable_4a4b42d901344ede850ffc74f58c677b : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1250819912)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Item_RepairBench
	{
		[Patch("OnItemRepair", "OnItemRepair", "RepairBench", "RepairAnItem", new string[] { "Item", "BasePlayer", "BaseEntity", "System.Single", "System.Boolean" })]
		[Identifier("104f8eed73854764a58a806e8dc13b87")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("itemToRepair", "Item", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_RepairBench_104f8eed73854764a58a806e8dc13b87 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 78)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)768721788), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnItemSkinChange", "OnItemSkinChange", "RepairBench", "ChangeSkin", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("2d5b9cd75c574c7980313797c914785b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local2", "System.Int32", false)]
		[Parameter("local6", "Item", false)]
		[Parameter("self", "RepairBench", false)]
		[Parameter("local1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_RepairBench_2d5b9cd75c574c7980313797c914785b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 43)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-201006377)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)6);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
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

	public class Item_MapEntity
	{
		[Patch("OnMapImageUpdated", "OnMapImageUpdated", "MapEntity", "ImageUpdate", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("e28854ccef7d420e9c8fd04d4a0b6f40")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_MapEntity_e28854ccef7d420e9c8fd04d4a0b6f40 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 75)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)322168974), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[1] { typeof(uint) }, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Item_ItemModUpgrade
	{
		[Patch("OnItemUpgrade", "OnItemUpgrade", "ItemModUpgrade", "ServerCommand", new string[] { "Item", "System.String", "BasePlayer" })]
		[Identifier("8faba9be2d8949d0b12e47a8f0842eef")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Parameter("local0", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemModUpgrade_8faba9be2d8949d0b12e47a8f0842eef : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 31)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-651643052)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
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

	public class Item_PlayerInventory
	{
		[Patch("CanEquipItem", "CanEquipItem", "PlayerInventory", "CanEquipItem", new string[] { "BasePlayer", "Item", "System.Int32" })]
		[Identifier("1b992ff7bdd94a8fb6326cbc94b69502")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Parameter("item", "Item", false)]
		[Parameter("targetSlot", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_1b992ff7bdd94a8fb6326cbc94b69502 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-823701398)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
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

		[Patch("CanWearItem", "CanWearItem", "PlayerInventory", "CanWearItem", new string[] { "BasePlayer", "Item", "System.Int32" })]
		[Identifier("2d5dafb6e7e448e38f78a72df13b8ff1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Parameter("item", "Item", false)]
		[Parameter("targetSlot", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_2d5dafb6e7e448e38f78a72df13b8ff1 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-246435549)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
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

		[Patch("OnItemAction", "OnItemAction", "PlayerInventory", "ItemCmd", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("47ab981b97304a94801b574e1eadc5c3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local2", "Item", false)]
		[Parameter("local1", "System.String", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_47ab981b97304a94801b574e1eadc5c3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 32)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-891444323)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

		[Patch("CanMoveItem", "CanMoveItem", "PlayerInventory", "MoveItem", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("bd0322a9cfae4299897c2ba48575da0e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local5", "Item", false)]
		[Parameter("self", "PlayerInventory", false)]
		[Parameter("local1", "ItemContainerId", false)]
		[Parameter("local2", "System.Int32", false)]
		[Parameter("local3", "System.Int32", false)]
		[Parameter("local4", "ItemMoveModifier", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_bd0322a9cfae4299897c2ba48575da0e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 49)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-354983102)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("ItemContainerId"));
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)4);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("ItemMoveModifier"));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[7]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object),
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

		[Patch("OnInventoryItemsCount", "OnInventoryItemsCount", "PlayerInventory", "GetAmount", new string[] { "System.Int32", "System.Boolean", "System.Boolean" })]
		[Identifier("b38fa2d7dff44ea5adebdb62fd69fc22")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Return(typeof(int))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_b38fa2d7dff44ea5adebdb62fd69fc22 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1800965447), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnInventoryItemsTake", "OnInventoryItemsTake", "PlayerInventory", "Take", new string[] { "System.Collections.Generic.List`1<Item>", "System.Int32", "System.Int32" })]
		[Identifier("b712b8f54b5c4820b9f070ae62ef7728")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Return(typeof(int))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_b712b8f54b5c4820b9f070ae62ef7728 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)565506075), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnInventoryItemsFind", "OnInventoryItemsFind", "PlayerInventory", "FindItemsByItemID", new string[] { "System.Collections.Generic.List`1<Item>", "System.Int32" })]
		[Identifier("8b3505ce79114b1f89573409c9ca4f55")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Parameter("id", "System.Int32", false)]
		[Parameter("list", "System.Collections.Generic.List`1[Item]", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_8b3505ce79114b1f89573409c9ca4f55 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1280772680)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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

		[Patch("OnInventoryAmmoFind", "OnInventoryAmmoFind", "PlayerInventory", "FindAmmo", new string[] { "System.Collections.Generic.List`1<Item>", "Rust.AmmoTypes" })]
		[Identifier("409ad3d2e3fb48a2aa50040f46eaebfc")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_409ad3d2e3fb48a2aa50040f46eaebfc : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1920243134)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(AmmoTypes));
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

		[Patch("OnBackpackDrop", "OnBackpackDrop", "PlayerInventory", "TryDropBackpack", new string[] { })]
		[Identifier("9e9df486430440218232908599356804")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "Item", false)]
		[Parameter("self", "PlayerInventory", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_9e9df486430440218232908599356804 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 9)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1554715023), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnInventoryItemFind", "OnInventoryItemFind", "PlayerInventory", "FindItemByItemID", new string[] { "System.Int32" })]
		[Identifier("910bb86603f04f5587d0106022f9e228")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Return(typeof(Item))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_910bb86603f04f5587d0106022f9e228 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1207445540)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(Item));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(Item));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnInventoryAmmoItemFind", "OnInventoryAmmoItemFind [PlayerInventory]", "PlayerInventory", "FindAmmo", new string[] { "Rust.AmmoTypes" })]
		[Identifier("f54d7e9056a6434c807b32b548cf7422")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Return(typeof(Item))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_f54d7e9056a6434c807b32b548cf7422 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-564758871)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(AmmoTypes));
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
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(Item));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(Item));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Item_WorldItem
	{
		[Patch("OnItemPickup", "OnItemPickup", "WorldItem", "Pickup", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("3f79174e069946a7bb94f4d445e2c3a0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WorldItem", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WorldItem_3f79174e069946a7bb94f4d445e2c3a0 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 13)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1833117670), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("WorldItem"), "item"));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

	public class Item_BaseOven
	{
		[Patch("OnFindBurnable", "OnFindBurnable", "BaseOven", "FindBurnable", new string[] { })]
		[Identifier("4a68d1cbd47144d5a87e2b2a21a06ffd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseOven", false)]
		[Return(typeof(Item))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_BaseOven_4a68d1cbd47144d5a87e2b2a21a06ffd : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1862688437)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(Item));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(Item));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Item_Recycler
	{
		[Patch("OnItemRecycle", "OnItemRecycle", "Recycler", "RecycleThink", new string[] { })]
		[Identifier("4fd8b1abc2434f8ab293c3d0be4d428f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local3", "Item", false)]
		[Parameter("self", "Recycler", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Recycler_4fd8b1abc2434f8ab293c3d0be4d428f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 17)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1576584464)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnItemRecycleAmount", "OnItemRecycleAmount", "Recycler", "RecycleThink", new string[] { })]
		[Identifier("90f1619d424c4e5cb15750a270ed921b")]
		[Dependencies(new string[] { "OnItemRecycle [2]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local3", "Item", false)]
		[Parameter("local4", "System.Int32", false)]
		[Parameter("self", "Recycler", false)]
		[Return(typeof(int), Continues = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Recycler_90f1619d424c4e5cb15750a270ed921b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 61)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-548780362)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)4);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(int));
					yield return __GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 4, typeof(int));
					yield return instruction;
				}
			}
		}

		[Patch("OnItemRecycle", "OnItemRecycle [2]", "Recycler", "RecycleThink", new string[] { })]
		[Identifier("7f231e25c87244d6afabe9bcc63b37fa")]
		[Dependencies(new string[] { "OnItemRecycle" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Recycler_7f231e25c87244d6afabe9bcc63b37fa : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected Obj, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected Obj, but got Unknown
				//IL_0062: Unknown result type (might be due to invalid IL or missing references)
				//IL_006c: Expected Obj, but got Unknown
				//IL_0073: Unknown result type (might be due to invalid IL or missing references)
				//IL_007d: Expected Obj, but got Unknown
				//IL_0099: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a3: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Recycler"), "HasRecyclable", (Type[])null, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[374];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Recycler"), "StopRecycling", (Type[])null, (Type[])null)));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[23]), list2[23]);
				}
				list2.InsertRange(23, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Item_DroppedItem
	{
		[Patch("CanCombineDroppedItem", "CanCombineDroppedItem", "DroppedItem", "OnDroppedOn", new string[] { "DroppedItem" })]
		[Identifier("d1a097a230ba4bc2bdf15eef8b5ae795")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DroppedItem", false)]
		[Parameter("di", "DroppedItem", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_DroppedItem_d1a097a230ba4bc2bdf15eef8b5ae795 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-787975984)), instruction), instruction);
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

		[Patch("OnDroppedItemCombined", "OnDroppedItemCombined", "DroppedItem", "OnDroppedOn", new string[] { "DroppedItem" })]
		[Identifier("71fdef1fb2794f1aa3fc63a43bf6bd75")]
		[Dependencies(new string[] { "CanCombineDroppedItem" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DroppedItem", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_DroppedItem_71fdef1fb2794f1aa3fc63a43bf6bd75 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 25)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)744056691), instruction), instruction);
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

		[Patch("OnItemDespawn", "OnItemDespawn", "DroppedItem", "IdleDestroy", new string[] { })]
		[Identifier("549c2272da794540a1d9b8b9bf8e47a6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_DroppedItem_549c2272da794540a1d9b8b9bf8e47a6 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)214500625), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("DroppedItem"), "item"));
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

	public class Item_LootContainer
	{
		[Patch("OnBonusItemDrop", "OnBonusItemDrop", "LootContainer", "DropBonusItems", new string[] { "BaseEntity", "ItemContainer" })]
		[Identifier("9dae9125a1414439b1d7ba8f0e0a23e5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local5", "Item", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("container", "ItemContainer", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_LootContainer_9dae9125a1414439b1d7ba8f0e0a23e5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 101)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1235377971), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
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

		[Patch("OnBonusItemDropped", "OnBonusItemDropped", "LootContainer", "DropBonusItems", new string[] { "BaseEntity", "ItemContainer" })]
		[Identifier("16e8d6f1278449d3ae5918f74bd30da0")]
		[Dependencies(new string[] { "OnBonusItemDrop" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local5", "Item", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("container", "ItemContainer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_LootContainer_16e8d6f1278449d3ae5918f74bd30da0 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 126)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-257702788)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
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

		[Patch("OnBonusItemDropped [patch 1]", "OnBonusItemDropped [patch 1]", "LootContainer", "DropBonusItems", new string[] { "BaseEntity", "ItemContainer" })]
		[Identifier("fd60e1d667684c1a989720f76875179d")]
		[Dependencies(new string[] { "OnBonusItemDropped" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_LootContainer_fd60e1d667684c1a989720f76875179d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[132];
				list.Add(new CodeInstruction(OpCodes.Ble, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[22]), list2[22]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[23], list2[22]), list2[22]);
				}
				list2.RemoveRange(22, 1);
				list2.InsertRange(22, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnBonusItemDropped [patch 2]", "OnBonusItemDropped [patch 2]", "LootContainer", "DropBonusItems", new string[] { "BaseEntity", "ItemContainer" })]
		[Identifier("bf118ebbb330453aa1c8e1dae7dc032e")]
		[Dependencies(new string[] { "OnBonusItemDropped [patch 1]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_LootContainer_bf118ebbb330453aa1c8e1dae7dc032e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[132];
				list.Add(new CodeInstruction(OpCodes.Brfalse, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[27]), list2[27]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[28], list2[27]), list2[27]);
				}
				list2.RemoveRange(27, 1);
				list2.InsertRange(27, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnBonusItemDropped [patch 3]", "OnBonusItemDropped [patch 3]", "LootContainer", "DropBonusItems", new string[] { "BaseEntity", "ItemContainer" })]
		[Identifier("1964f4a172eb4112b0f2c70a23ab0f64")]
		[Dependencies(new string[] { "OnBonusItemDropped [patch 2]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_LootContainer_1964f4a172eb4112b0f2c70a23ab0f64 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[132];
				list.Add(new CodeInstruction(OpCodes.Ble_Un, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[45]), list2[45]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[46], list2[45]), list2[45]);
				}
				list2.RemoveRange(45, 1);
				list2.InsertRange(45, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnBonusItemDropped [patch 4]", "OnBonusItemDropped [patch 4]", "LootContainer", "DropBonusItems", new string[] { "BaseEntity", "ItemContainer" })]
		[Identifier("fe0a7111a2a145ae83ef2e60982eeeed")]
		[Dependencies(new string[] { "OnBonusItemDropped [patch 3]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_LootContainer_fe0a7111a2a145ae83ef2e60982eeeed : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[132];
				list.Add(new CodeInstruction(OpCodes.Ble_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[88]), list2[88]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[89], list2[88]), list2[88]);
				}
				list2.RemoveRange(88, 1);
				list2.InsertRange(88, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnBonusItemDropped [patch 5]", "OnBonusItemDropped [patch 5]", "LootContainer", "DropBonusItems", new string[] { "BaseEntity", "ItemContainer" })]
		[Identifier("ffbd41a5bc374be88656602750d9ea97")]
		[Dependencies(new string[] { "OnBonusItemDropped [patch 4]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_LootContainer_ffbd41a5bc374be88656602750d9ea97 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[132];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[100]), list2[100]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[101], list2[100]), list2[100]);
				}
				list2.RemoveRange(100, 1);
				list2.InsertRange(100, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Item_ItemModRepair
	{
		[Patch("OnItemRefill", "OnItemRefill", "ItemModRepair", "ServerCommand", new string[] { "Item", "System.String", "BasePlayer" })]
		[Identifier("fd1080d6da124365a3b4807593cb528f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemModRepair_fd1080d6da124365a3b4807593cb528f : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2020188086)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
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

	public class Item_Mailbox
	{
		[Patch("OnItemSubmit", "OnItemSubmit", "Mailbox", "SubmitInputItems", new string[] { "BasePlayer" })]
		[Identifier("bed283118565441b9235db0616bdfd1d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "Item", false)]
		[Parameter("self", "Mailbox", false)]
		[Parameter("fromPlayer", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Mailbox_bed283118565441b9235db0616bdfd1d : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2108135861), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}

		[Patch("OnItemSubmit", "OnItemSubmit [patch]", "Mailbox", "SubmitInputItems", new string[] { "BasePlayer" })]
		[Identifier("bdd0e248c5734c798a79f2323beb0804")]
		[Dependencies(new string[] { "OnItemSubmit" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Mailbox_bdd0e248c5734c798a79f2323beb0804 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_0050: Unknown result type (might be due to invalid IL or missing references)
				//IL_005a: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[18];
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label));
				Label label2 = Generator.DefineLabel();
				CodeInstruction val2 = list2[75];
				list.Add(new CodeInstruction(OpCodes.Br_S, (object)label2));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[16]), list2[16]);
				}
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[17].labels);
				}
				else
				{
					list2[18].labels.AddRange(list2[17].labels);
				}
				list2[17].labels.Clear();
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[18], list2[16]), list2[16]);
				}
				list2.RemoveRange(16, 2);
				list2.InsertRange(16, list);
				val.labels.Add(label);
				val2.labels.Add(label2);
				return list2.AsEnumerable();
			}
		}
	}

	public class Item_ItemModUnwrap
	{
		[Patch("OnItemUnwrap", "OnItemUnwrap", "ItemModUnwrap", "ServerCommand", new string[] { "Item", "System.String", "BasePlayer" })]
		[Identifier("48e314b4378f47b78ffb74d91c6d8bbc")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ItemModUnwrap", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemModUnwrap_48e314b4378f47b78ffb74d91c6d8bbc : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 9)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)283863316), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

	public class Item_PaintedItemStorageEntity
	{
		[Patch("OnItemPainted", "OnItemPainted", "PaintedItemStorageEntity", "Server_UpdateImage", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("387995b63e39400ebc341dac73d527f7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PaintedItemStorageEntity", false)]
		[Parameter("local0", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local1", "System.Byte[]", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PaintedItemStorageEntity_387995b63e39400ebc341dac73d527f7 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2127678047), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
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

	public class Item_Chainsaw
	{
		[Patch("OnInventoryAmmoItemFind", "OnInventoryAmmoItemFind [Chainsaw]", "Chainsaw", "GetAmmo", new string[] { })]
		[Identifier("371ec2682a774a9f826267297e24c05d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("inventory", "PlayerInventory", false)]
		[Parameter("fuelType", "ItemDefinition", false)]
		[Return(typeof(Item))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Chainsaw_371ec2682a774a9f826267297e24c05d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-564758871)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BasePlayer"), "get_inventory", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("Chainsaw"), "fuelType"));
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
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(Item));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(Item));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Item_FlameThrower
	{
		[Patch("OnInventoryAmmoItemFind", "OnInventoryAmmoItemFind [FlameThrower]", "FlameThrower", "GetAmmo", new string[] { })]
		[Identifier("1a764c116cce4613be878de861adc60b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("inventory", "PlayerInventory", false)]
		[Parameter("fuelType", "ItemDefinition", false)]
		[Return(typeof(Item))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_FlameThrower_1a764c116cce4613be878de861adc60b : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-564758871)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BasePlayer"), "get_inventory", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("FlameThrower"), "fuelType"));
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
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(Item));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(Item));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Item_WeaponRack
	{
		[Patch("OnRackedWeaponMount", "OnRackedWeaponMount", "WeaponRack", "MountWeapon", new string[] { "Item", "BasePlayer", "System.Int32", "System.Int32", "System.Boolean" })]
		[Identifier("dd27f659f7da46a188116794031028db")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_dd27f659f7da46a188116794031028db : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1157802010), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnRackedWeaponMounted", "OnRackedWeaponMounted", "WeaponRack", "MountWeapon", new string[] { "Item", "BasePlayer", "System.Int32", "System.Int32", "System.Boolean" })]
		[Identifier("7452d41a83844920a5ff82bf5f0603fa")]
		[Dependencies(new string[] { "OnRackedWeaponMount" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_7452d41a83844920a5ff82bf5f0603fa : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1049908860), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnRackedWeaponSwap", "OnRackedWeaponSwap", "WeaponRack", "SwapPlayerWeapon", new string[] { "BasePlayer", "System.Int32", "System.Int32", "System.Int32" })]
		[Identifier("f8ef9ae7a77b4676a72dd8cc41e3f6e8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "Item", false)]
		[Parameter("local2", "WeaponRackSlot", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_f8ef9ae7a77b4676a72dd8cc41e3f6e8 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-732312571)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
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

		[Patch("OnRackedWeaponSwapped", "OnRackedWeaponSwapped", "WeaponRack", "SwapPlayerWeapon", new string[] { "BasePlayer", "System.Int32", "System.Int32", "System.Int32" })]
		[Identifier("94e7786039a34a3db2e2aeeaa66c3c56")]
		[Dependencies(new string[] { "OnRackedWeaponSwap" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "Item", false)]
		[Parameter("local2", "WeaponRackSlot", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_94e7786039a34a3db2e2aeeaa66c3c56 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 79)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1340704243)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnRackedWeaponTake", "OnRackedWeaponTake", "WeaponRack", "GivePlayerWeapon", new string[] { "BasePlayer", "System.Int32", "System.Int32", "System.Boolean", "System.Boolean" })]
		[Identifier("9c9c4b48072c4f4c88b1fc49cd14d673")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_9c9c4b48072c4f4c88b1fc49cd14d673 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 21)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)411927536), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnRackedWeaponTaken", "OnRackedWeaponTaken", "WeaponRack", "GivePlayerWeapon", new string[] { "BasePlayer", "System.Int32", "System.Int32", "System.Boolean", "System.Boolean" })]
		[Identifier("500197a33ecc42e3bb74a6fb0811817e")]
		[Dependencies(new string[] { "OnRackedWeaponTake" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_500197a33ecc42e3bb74a6fb0811817e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 136)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1261090672)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnRackedWeaponUnload", "OnRackedWeaponUnload", "WeaponRack", "UnloadWeapon", new string[] { "BasePlayer", "System.Int32" })]
		[Identifier("e544156a8a174be1a1baa74a00c257e8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_e544156a8a174be1a1baa74a00c257e8 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 25)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1441237871)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnRackedWeaponUnloaded", "OnRackedWeaponUnloaded", "WeaponRack", "UnloadWeapon", new string[] { "BasePlayer", "System.Int32" })]
		[Identifier("5752f5b134214086a07768689b8a7ab6")]
		[Dependencies(new string[] { "OnRackedWeaponUnload" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_5752f5b134214086a07768689b8a7ab6 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 71)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)18069347), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnRackedWeaponLoad", "OnRackedWeaponLoad", "WeaponRack", "LoadWeaponAmmo", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("2f0a0161e7cc469baec7202fcb8be0e5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local4", "Item", false)]
		[Parameter("local7", "ItemDefinition", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_2f0a0161e7cc469baec7202fcb8be0e5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 59)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1375473290)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)4);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)7);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
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

		[Patch("OnRackedWeaponLoaded", "OnRackedWeaponLoaded", "WeaponRack", "LoadWeaponAmmo", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("69211871b78e43c0a65d2d3932dbde83")]
		[Dependencies(new string[] { "OnRackedWeaponLoad" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local4", "Item", false)]
		[Parameter("local7", "ItemDefinition", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_69211871b78e43c0a65d2d3932dbde83 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 158)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1427596426)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)4);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)7);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
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

	public class Item_Locker
	{
		[Patch("CanLockerAcceptItem", "CanLockerAcceptItem", "Locker", "ItemFilter", new string[] { "BasePlayer", "Item", "System.Int32" })]
		[Identifier("206899080d494866909f663690f4def5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Locker", false)]
		[Parameter("item", "Item", false)]
		[Parameter("targetSlot", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Locker_206899080d494866909f663690f4def5 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1026445911), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
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
	}

	public class Item_StorageContainer
	{
		[Patch("OnItemFilter", "OnItemFilter", "StorageContainer", "ItemFilter", new string[] { "BasePlayer", "Item", "System.Int32" })]
		[Identifier("0c0dd0d3aa644152a763df6fe9936c33")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Parameter("self", "StorageContainer", false)]
		[Parameter("targetSlot", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_StorageContainer_0c0dd0d3aa644152a763df6fe9936c33 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)44080502), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
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
	}
}
