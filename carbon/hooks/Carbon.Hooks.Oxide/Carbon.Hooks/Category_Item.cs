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
		[Patch("OnItemRemovedFromContainer", "OnItemRemovedFromContainer", "ItemContainer", "Remove", new string[] { "Item" })]
		[Identifier("8f3cdf8e474e4c7d8ad8d02d00aa855f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ItemContainer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemContainer_8f3cdf8e474e4c7d8ad8d02d00aa855f : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)470300595), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("OnItemAddedToContainer", "OnItemAddedToContainer", "ItemContainer", "Insert", new string[] { "Item" })]
		[Identifier("fc4d0f58bce64e4b8e245bcfe7e910ae")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ItemContainer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemContainer_fc4d0f58bce64e4b8e245bcfe7e910ae : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 68)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)199161889), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("CanAcceptItem", "CanAcceptItem", "ItemContainer", "CanAcceptItem", new string[] { "BasePlayer", "Item", "System.Int32" })]
		[Identifier("bf62eca63b1d4c62b4816f767a78817d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ItemContainer", false)]
		[Parameter("item", "Item", false)]
		[Parameter("targetPos", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(CanAcceptResult))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemContainer_bf62eca63b1d4c62b4816f767a78817d : Patch
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
		[Identifier("fdb607ad74c141ffb0cc8d677c7fbbe5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "ItemCraftTask", false)]
		[Parameter("owner", "BasePlayer", false)]
		[Parameter("fromTempBlueprint", "Item", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemCrafter_fdb607ad74c141ffb0cc8d677c7fbbe5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 83)
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
		[Identifier("f3d945bded14406eac61b7d9e1322c97")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("task", "ItemCraftTask", false)]
		[Parameter("local1", "Item", false)]
		[Parameter("self", "ItemCrafter", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemCrafter_f3d945bded14406eac61b7d9e1322c97 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 215)
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
		[Identifier("0ecf44165e484c7b8e8db273803140a1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ItemCraftTask", false)]
		[Parameter("self", "ItemCrafter", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemCrafter_0ecf44165e484c7b8e8db273803140a1 : Patch
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
		[Identifier("51a3eab07eed41df95eb75c6a81a07c0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ItemCrafter", false)]
		[Parameter("local2", "ItemCraftTask", false)]
		[Parameter("taskID", "System.Int32", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemCrafter_51a3eab07eed41df95eb75c6a81a07c0 : Patch
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
		[Identifier("757be109b9f64482a1a383a0a6e3910c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Deployer", false)]
		[Parameter("local5", "ItemModDeployable", false)]
		[Parameter("local6", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Deployer_757be109b9f64482a1a383a0a6e3910c : Patch
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
		[Identifier("329210f48e694542b8d103db6105aafa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Deployer", false)]
		[Parameter("local1", "BaseEntity", false)]
		[Parameter("local4", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Deployer_329210f48e694542b8d103db6105aafa : Patch
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
		[Identifier("cedb5ed41a6e4f6aa9126038055c5e73")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "Deployer", false)]
		[Parameter("local2", "NetworkableId", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Deployer_cedb5ed41a6e4f6aa9126038055c5e73 : Patch
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
		[Identifier("0b3cf4478c4a46e9831788d02830c288")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Item", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_0b3cf4478c4a46e9831788d02830c288 : Patch
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
		[Identifier("bbef1aa33bdb4026a1d8c9004e3e2c84")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Item", false)]
		[Parameter("amountToConsume", "System.Int32", false)]
		[Return(typeof(int), Continues = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_bbef1aa33bdb4026a1d8c9004e3e2c84 : Patch
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
		[Identifier("2c9b090d87f64aa39c9d95fe60b2efbe")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Item", false)]
		[Return(typeof(Item))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_2c9b090d87f64aa39c9d95fe60b2efbe : Patch
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
		[Identifier("21f7cf71102347cfb2d19a16525c2eee")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Item", false)]
		[Parameter("item", "Item", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_21f7cf71102347cfb2d19a16525c2eee : Patch
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
		[Identifier("3e4dfc575ec64f1381ccf4da09f75924")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Item", false)]
		[Parameter("local1", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_3e4dfc575ec64f1381ccf4da09f75924 : Patch
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
		[Identifier("6e68552c74984df78c35bffdb5eaa2ec")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Item", false)]
		[Return(typeof(int))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_6e68552c74984df78c35bffdb5eaa2ec : Patch
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
		[Identifier("e2da54e23b1f4f68877b18e1aef1ef6b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Item", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_e2da54e23b1f4f68877b18e1aef1ef6b : Patch
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
		[Identifier("a08aa335982d46e5877ee4c70085eed4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local13", "Item", false)]
		[Parameter("self", "Item", false)]
		[Parameter("newcontainer", "ItemContainer", false)]
		[Parameter("local15", "System.Int32", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_a08aa335982d46e5877ee4c70085eed4 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 293)
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
		[Identifier("286caf2e078c4511a0c9049dcf20918f")]
		[Dependencies(new string[] { "OnItemStacked [1]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local23", "Item", false)]
		[Parameter("self", "Item", false)]
		[Parameter("newcontainer", "ItemContainer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_286caf2e078c4511a0c9049dcf20918f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 576)
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
		[Identifier("af918181b7944019b0961901ca4c6c50")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_af918181b7944019b0961901ca4c6c50 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_0043: Expected O, but got Unknown
				//IL_004e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0058: Expected O, but got Unknown
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0069: Expected O, but got Unknown
				//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ae: Expected O, but got Unknown
				//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bf: Expected O, but got Unknown
				//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d5: Expected O, but got Unknown
				//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e6: Expected O, but got Unknown
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
		[Identifier("471160224c2b4bd7a1559634b580b877")]
		[Dependencies(new string[] { "OnItemLock" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Item_471160224c2b4bd7a1559634b580b877 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected O, but got Unknown
				//IL_004f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0059: Expected O, but got Unknown
				//IL_0060: Unknown result type (might be due to invalid IL or missing references)
				//IL_006a: Expected O, but got Unknown
				//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00af: Expected O, but got Unknown
				//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c0: Expected O, but got Unknown
				//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d6: Expected O, but got Unknown
				//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e7: Expected O, but got Unknown
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
		[Identifier("6bc633a9f4564df791960ef681a9ae30")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MedicalTool", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_MedicalTool_6bc633a9f4564df791960ef681a9ae30 : Patch
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
		[Identifier("22e10ae0ebb642cca19dd597931a7437")]
		[Dependencies(new string[] { "CanResearchItem" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResearchTable", false)]
		[Parameter("local1", "Item", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ResearchTable_22e10ae0ebb642cca19dd597931a7437 : Patch
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
		[Identifier("2d089543fba44966b68e0edae17ad245")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResearchTable", false)]
		[Parameter("local2", "System.Int32", false)]
		[Return(typeof(int), Continues = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ResearchTable_2d089543fba44966b68e0edae17ad245 : Patch
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
		[Identifier("bc377d55e9f54383bb0b0fcc728708b4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Return(typeof(int))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ResearchTable_bc377d55e9f54383bb0b0fcc728708b4 : Patch
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
		[Identifier("5918c8776b134e94a000acfb1f10be23")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("info", "ItemDefinition", false)]
		[Return(typeof(int))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ResearchTable_5918c8776b134e94a000acfb1f10be23 : Patch
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
		[Identifier("fca2b820329c40baa26cebd4a1aa7d15")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("itemToRepair", "Item", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_RepairBench_fca2b820329c40baa26cebd4a1aa7d15 : Patch
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
		[Identifier("5492eb462b744421afcedf980a0bf0e2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("inventoryId", "System.Int32", false)]
		[Parameter("local5", "Item", false)]
		[Parameter("self", "RepairBench", false)]
		[Parameter("local1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_RepairBench_5492eb462b744421afcedf980a0bf0e2 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 44)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-201006377)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("RepairBench+<>c__DisplayClass12_0"), "inventoryId"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
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
		[Identifier("e0336a9c807943dfbb4b658c7ea9a599")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_MapEntity_e0336a9c807943dfbb4b658c7ea9a599 : Patch
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
		[Identifier("6865f287732d437a87819e6a3a815fba")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Parameter("local0", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemModUpgrade_6865f287732d437a87819e6a3a815fba : Patch
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
		[Identifier("2ec48862793043a8baac28f5a48e5795")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Parameter("item", "Item", false)]
		[Parameter("targetSlot", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_2ec48862793043a8baac28f5a48e5795 : Patch
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
		[Identifier("a67b81e23cda42e7bb6f9232533c13a4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Parameter("item", "Item", false)]
		[Parameter("targetSlot", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_a67b81e23cda42e7bb6f9232533c13a4 : Patch
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
		[Identifier("c88c6abf0c884ab082bfa750557f92d5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local2", "Item", false)]
		[Parameter("local1", "System.String", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_c88c6abf0c884ab082bfa750557f92d5 : Patch
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
		[Identifier("1b9a30d8840845578c6fabd7e25ec312")]
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
		public class Item_PlayerInventory_1b9a30d8840845578c6fabd7e25ec312 : Patch
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
		[Identifier("c100e8123c274cdeb4a9a29d35479cb7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Return(typeof(int))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_c100e8123c274cdeb4a9a29d35479cb7 : Patch
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
		[Identifier("aa667e921daa4ff6ae7c6c38acbb21fa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Return(typeof(int))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_aa667e921daa4ff6ae7c6c38acbb21fa : Patch
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
		[Identifier("87e36de9aa36488f843f05da5ff39c35")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Parameter("id", "System.Int32", false)]
		[Parameter("list", "System.Collections.Generic.List`1[Item]", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_87e36de9aa36488f843f05da5ff39c35 : Patch
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
		[Identifier("ad59353feae944a5b534bf0e06ece1d9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_ad59353feae944a5b534bf0e06ece1d9 : Patch
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
		[Identifier("ab81636dfb384225bc7fdaf6696abf42")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "Item", false)]
		[Parameter("self", "PlayerInventory", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_ab81636dfb384225bc7fdaf6696abf42 : Patch
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
		[Identifier("5fa9f27997dc47288f6df5d714919fba")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Return(typeof(Item))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_5fa9f27997dc47288f6df5d714919fba : Patch
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
		[Identifier("5cb84c78c85545b3ae147a48ae7d84b1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerInventory", false)]
		[Return(typeof(Item))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PlayerInventory_5cb84c78c85545b3ae147a48ae7d84b1 : Patch
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
		[Identifier("f76d37c9d3534ad1b896d1538e87f8d7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "WorldItem", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self1", "WorldItem", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WorldItem_f76d37c9d3534ad1b896d1538e87f8d7 : Patch
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
		[Identifier("f191ca2531c24b19a08a8939f66cbcc8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseOven", false)]
		[Return(typeof(Item))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_BaseOven_f191ca2531c24b19a08a8939f66cbcc8 : Patch
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
		[Identifier("4a58809c612b44e9af300d4dcdde6bc6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local3", "Item", false)]
		[Parameter("self", "Recycler", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Recycler_4a58809c612b44e9af300d4dcdde6bc6 : Patch
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
		[Identifier("d0db4f8581294dd9912f6f0c489776b6")]
		[Dependencies(new string[] { "OnItemRecycle [2]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local3", "Item", false)]
		[Parameter("local4", "System.Int32", false)]
		[Parameter("self", "Recycler", false)]
		[Return(typeof(int), Continues = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Recycler_d0db4f8581294dd9912f6f0c489776b6 : Patch
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
		[Identifier("b170e012b9414f5a86b5083c4b83491a")]
		[Dependencies(new string[] { "OnItemRecycle" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Recycler_b170e012b9414f5a86b5083c4b83491a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected O, but got Unknown
				//IL_0062: Unknown result type (might be due to invalid IL or missing references)
				//IL_006c: Expected O, but got Unknown
				//IL_0073: Unknown result type (might be due to invalid IL or missing references)
				//IL_007d: Expected O, but got Unknown
				//IL_0099: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a3: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Recycler"), "HasRecyclable", (Type[])null, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[374];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Recycler"), "StopRecycling", (Type[])null, (Type[])null)));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[23]), list2[23]);
				}
				list2.InsertRange(23, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Item_DroppedItem
	{
		[Patch("CanCombineDroppedItem", "CanCombineDroppedItem", "DroppedItem", "OnDroppedOn", new string[] { "DroppedItem" })]
		[Identifier("c60ad699393b4b66bb51aa704adf9682")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DroppedItem", false)]
		[Parameter("di", "DroppedItem", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_DroppedItem_c60ad699393b4b66bb51aa704adf9682 : Patch
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
		[Identifier("04dcebbca3a547bb92481ed01ac84e86")]
		[Dependencies(new string[] { "CanCombineDroppedItem" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DroppedItem", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_DroppedItem_04dcebbca3a547bb92481ed01ac84e86 : Patch
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
		[Identifier("74f4f41908df4e7e8d6c347a60587074")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DroppedItem", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_DroppedItem_74f4f41908df4e7e8d6c347a60587074 : Patch
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
		[Identifier("01f6e86f00614c5bbfdbf6f204ce0714")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local5", "Item", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("container", "ItemContainer", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_LootContainer_01f6e86f00614c5bbfdbf6f204ce0714 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 99)
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
		[Identifier("c33faab032b6427dacc49a2f3a685995")]
		[Dependencies(new string[] { "OnBonusItemDrop" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local5", "Item", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("container", "ItemContainer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_LootContainer_c33faab032b6427dacc49a2f3a685995 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 124)
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
		[Identifier("7105204988864fb3b84e0031e026f5fa")]
		[Dependencies(new string[] { "OnBonusItemDropped" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_LootContainer_7105204988864fb3b84e0031e026f5fa : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[130];
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
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnBonusItemDropped [patch 2]", "OnBonusItemDropped [patch 2]", "LootContainer", "DropBonusItems", new string[] { "BaseEntity", "ItemContainer" })]
		[Identifier("8c61088bab1d4376b8f1f4d3555d2e15")]
		[Dependencies(new string[] { "OnBonusItemDropped [patch 1]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_LootContainer_8c61088bab1d4376b8f1f4d3555d2e15 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[130];
				list.Add(new CodeInstruction(OpCodes.Brfalse, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[26]), list2[26]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[27], list2[26]), list2[26]);
				}
				list2.RemoveRange(26, 1);
				list2.InsertRange(26, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnBonusItemDropped [patch 3]", "OnBonusItemDropped [patch 3]", "LootContainer", "DropBonusItems", new string[] { "BaseEntity", "ItemContainer" })]
		[Identifier("d8adcbe9c1ef4047ad99eb9cdbb94bad")]
		[Dependencies(new string[] { "OnBonusItemDropped [patch 2]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_LootContainer_d8adcbe9c1ef4047ad99eb9cdbb94bad : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[130];
				list.Add(new CodeInstruction(OpCodes.Ble_Un, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[44]), list2[44]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[45], list2[44]), list2[44]);
				}
				list2.RemoveRange(44, 1);
				list2.InsertRange(44, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnBonusItemDropped [patch 4]", "OnBonusItemDropped [patch 4]", "LootContainer", "DropBonusItems", new string[] { "BaseEntity", "ItemContainer" })]
		[Identifier("25daa0ad567b4d7c8a0a539d87ba6742")]
		[Dependencies(new string[] { "OnBonusItemDropped [patch 3]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_LootContainer_25daa0ad567b4d7c8a0a539d87ba6742 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[130];
				list.Add(new CodeInstruction(OpCodes.Ble_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[87]), list2[87]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[88], list2[87]), list2[87]);
				}
				list2.RemoveRange(87, 1);
				list2.InsertRange(87, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnBonusItemDropped [patch 5]", "OnBonusItemDropped [patch 5]", "LootContainer", "DropBonusItems", new string[] { "BaseEntity", "ItemContainer" })]
		[Identifier("b06ddd629ccc437080200ec03272134a")]
		[Dependencies(new string[] { "OnBonusItemDropped [patch 4]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_LootContainer_b06ddd629ccc437080200ec03272134a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[130];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[98]), list2[98]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[99], list2[98]), list2[98]);
				}
				list2.RemoveRange(98, 1);
				list2.InsertRange(98, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Item_ItemModRepair
	{
		[Patch("OnItemRefill", "OnItemRefill", "ItemModRepair", "ServerCommand", new string[] { "Item", "System.String", "BasePlayer" })]
		[Identifier("5bc8dd2504fb4f68ae1ce6f9f145a60e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemModRepair_5bc8dd2504fb4f68ae1ce6f9f145a60e : Patch
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
		[Identifier("75bfc1e1746942cda829bd3ad3d300c8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "Item", false)]
		[Parameter("self", "Mailbox", false)]
		[Parameter("fromPlayer", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Mailbox_75bfc1e1746942cda829bd3ad3d300c8 : Patch
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
		[Identifier("725db6c62fdf46dcbd4c95b4cf0ece4b")]
		[Dependencies(new string[] { "OnItemSubmit" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Mailbox_725db6c62fdf46dcbd4c95b4cf0ece4b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_0050: Unknown result type (might be due to invalid IL or missing references)
				//IL_005a: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[18];
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label));
				Label label2 = Generator.DefineLabel();
				CodeInstruction obj = list2[64];
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
				obj.labels.Add(label2);
				return list2.AsEnumerable();
			}
		}
	}

	public class Item_ItemModUnwrap
	{
		[Patch("OnItemUnwrap", "OnItemUnwrap", "ItemModUnwrap", "ServerCommand", new string[] { "Item", "System.String", "BasePlayer" })]
		[Identifier("10d70bd776094f1799ee9168efd0d6e9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ItemModUnwrap", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_ItemModUnwrap_10d70bd776094f1799ee9168efd0d6e9 : Patch
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
		[Identifier("70423d846e59470e9a770f5f9779b813")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PaintedItemStorageEntity", false)]
		[Parameter("local0", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local1", "System.Byte[]", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_PaintedItemStorageEntity_70423d846e59470e9a770f5f9779b813 : Patch
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
		[Identifier("f27de624db7a4e4bb086285eb1ebbc69")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("inventory", "PlayerInventory", false)]
		[Parameter("self", "Chainsaw", false)]
		[Return(typeof(Item))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Chainsaw_f27de624db7a4e4bb086285eb1ebbc69 : Patch
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
		[Identifier("0db9381b796d486087be3755e5dc312f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("inventory", "PlayerInventory", false)]
		[Parameter("self", "FlameThrower", false)]
		[Return(typeof(Item))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_FlameThrower_0db9381b796d486087be3755e5dc312f : Patch
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
		[Identifier("cde351e88b9d43b68b1ea6368280601b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_cde351e88b9d43b68b1ea6368280601b : Patch
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
		[Identifier("1a1e4b565a0845e88cabb3b360b533bb")]
		[Dependencies(new string[] { "OnRackedWeaponMount" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_1a1e4b565a0845e88cabb3b360b533bb : Patch
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
		[Identifier("c40eac749c434e1e85f3e740b6efda90")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "Item", false)]
		[Parameter("local2", "WeaponRackSlot", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_c40eac749c434e1e85f3e740b6efda90 : Patch
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
		[Identifier("8c0467f2184b414d894d071926d1fa19")]
		[Dependencies(new string[] { "OnRackedWeaponSwap" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "Item", false)]
		[Parameter("local2", "WeaponRackSlot", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_8c0467f2184b414d894d071926d1fa19 : Patch
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
		[Identifier("e73898b659a3408c90c9f428ff0828c9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_e73898b659a3408c90c9f428ff0828c9 : Patch
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
		[Identifier("7a49d200c5ad4401aea6d17b099dc3a8")]
		[Dependencies(new string[] { "OnRackedWeaponTake" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_7a49d200c5ad4401aea6d17b099dc3a8 : Patch
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
		[Identifier("29a976ab2049422aabd21f38a79b69c8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_29a976ab2049422aabd21f38a79b69c8 : Patch
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
		[Identifier("f2bc603ecf7647adaf174df3e1afdacf")]
		[Dependencies(new string[] { "OnRackedWeaponUnload" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_f2bc603ecf7647adaf174df3e1afdacf : Patch
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
		[Identifier("13fea703d8be4528a339fc843f0d7e6f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local4", "Item", false)]
		[Parameter("local7", "ItemDefinition", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_13fea703d8be4528a339fc843f0d7e6f : Patch
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
		[Identifier("3b0c8ab6dde1493b9000c8d2a6ae17dc")]
		[Dependencies(new string[] { "OnRackedWeaponLoad" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local4", "Item", false)]
		[Parameter("local7", "ItemDefinition", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("self", "WeaponRack", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_WeaponRack_3b0c8ab6dde1493b9000c8d2a6ae17dc : Patch
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
		[Identifier("19ea30700e794a468cc45b2e4ad52c3e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Locker", false)]
		[Parameter("item", "Item", false)]
		[Parameter("targetSlot", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_Locker_19ea30700e794a468cc45b2e4ad52c3e : Patch
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
		[Identifier("a3aecd7a48904497abc89c15309af7f7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("item", "Item", false)]
		[Parameter("self", "StorageContainer", false)]
		[Parameter("targetSlot", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Item")]
		[Assembly("Assembly-CSharp.dll")]
		public class Item_StorageContainer_a3aecd7a48904497abc89c15309af7f7 : Patch
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
