using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Apartments
{
	public class Apartments_RentableShop
	{
		[Patch("OnRentableShopClose", "OnRentableShopClose", "RentableShop", "CloseStore", new string[] { "System.Boolean" })]
		[Identifier("e599b5b1e88d455a99927ec1262df679")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_e599b5b1e88d455a99927ec1262df679 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)801743095), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
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

		[Patch("OnRentableShopClosed", "OnRentableShopClosed", "RentableShop", "OnShopClosed", new string[] { "System.Boolean" })]
		[Identifier("2273e086798b46f6a30fad818e435d4e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_2273e086798b46f6a30fad818e435d4e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 96)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-367058589)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
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

		[Patch("OnRentableShopOpened", "OnRentableShopOpened", "RentableShop", "OnShopOpened", new string[] { "BasePlayer" })]
		[Identifier("f112cb3fc7c2483dab1d98691fc83ff9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_f112cb3fc7c2483dab1d98691fc83ff9 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 147)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1762235317)), instruction);
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

		[Patch("OnRentableShopBreakInComplete", "OnRentableShopBreakInComplete", "RentableShop", "CompleteBreakIn", new string[] { "BasePlayer" })]
		[Identifier("8b222131c3294c49adc6a74946daaa5e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_8b222131c3294c49adc6a74946daaa5e : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1748499922)), instruction), instruction);
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

		[Patch("OnRentableShopOpen", "OnRentableShopOpen", "RentableShop", "Server_OpenStore", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("d75d99ca88694431bf68a2cceb538912")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_d75d99ca88694431bf68a2cceb538912 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-602182365)), instruction);
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}

		[Patch("OnRentableShopBreakInCompleted", "OnRentableShopBreakInCompleted", "RentableShop", "CompleteBreakIn", new string[] { "BasePlayer" })]
		[Identifier("1c4ea2b56c7b4f6485ac059138154f7a")]
		[Dependencies(new string[] { "OnRentableShopBreakInComplete" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_1c4ea2b56c7b4f6485ac059138154f7a : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1271525427), instruction), instruction);
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
	}

	public class Apartments_ApartmentDoor
	{
		[Patch("OnApartmentRoomBreakInComplete", "OnApartmentRoomBreakInComplete", "ApartmentDoor", "CompleteBreakIn", new string[] { "BasePlayer" })]
		[Identifier("ee480087932e4d3998d367fcdb257725")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ApartmentDoor", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentDoor_ee480087932e4d3998d367fcdb257725 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 33)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1836095990), instruction), instruction);
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

		[Patch("OnApartmentRoomBreakInCompleted", "OnApartmentRoomBreakInCompleted", "ApartmentDoor", "CompleteBreakIn", new string[] { "BasePlayer" })]
		[Identifier("37ba39e3008f427385bdf7a6dfe8cdda")]
		[Dependencies(new string[] { "OnApartmentRoomBreakInComplete" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ApartmentDoor", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentDoor_37ba39e3008f427385bdf7a6dfe8cdda : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 100)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1873533329), instruction), instruction);
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
	}

	public class Apartments_ApartmentBuilding
	{
		[Patch("OnApartmentRoomUpgrade", "OnApartmentRoomUpgrade", "ApartmentBuilding", "TryUpgradeRoom", new string[] { "BasePlayer", "ApartmentSize" })]
		[Identifier("a5be8e3734514a878d6123c7edb0330f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_a5be8e3734514a878d6123c7edb0330f : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1621009615), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ApartmentSize));
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

		[Patch("OnApartmentRoomCheckedout", "OnApartmentRoomCheckedout", "ApartmentBuilding", "TryCheckout", new string[] { "BasePlayer" })]
		[Identifier("44ef148da93d4b209affa3345ef3a718")]
		[Dependencies(new string[] { "OnApartmentRoomCheckout" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_44ef148da93d4b209affa3345ef3a718 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 26)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2113759096), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnApartmentRoomPurchase", "OnApartmentRoomPurchase", "ApartmentBuilding", "PurchaseRoom", new string[] { "BasePlayer", "ApartmentSize" })]
		[Identifier("1ffeaae927bf45feb2de0dac7facbb83")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_1ffeaae927bf45feb2de0dac7facbb83 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-550128166)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ApartmentSize));
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

		[Patch("OnApartmentRoomUpgraded", "OnApartmentRoomUpgraded", "ApartmentBuilding", "TryUpgradeRoom", new string[] { "BasePlayer", "ApartmentSize" })]
		[Identifier("55662584e992481cab2acd5aa0c0126c")]
		[Dependencies(new string[] { "OnApartmentRoomUpgrade" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_55662584e992481cab2acd5aa0c0126c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 93)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)657070166), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ApartmentSize));
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

		[Patch("OnApartmentRoomPurchased", "OnApartmentRoomPurchased", "ApartmentBuilding", "PurchaseRoom", new string[] { "BasePlayer", "ApartmentSize" })]
		[Identifier("9a4db04780244a748a2c106e59ddb8e2")]
		[Dependencies(new string[] { "OnApartmentRoomPurchase" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_9a4db04780244a748a2c106e59ddb8e2 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 50)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2108957277)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ApartmentSize));
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

		[Patch("OnApartmentRoomCheckout", "OnApartmentRoomCheckout", "ApartmentBuilding", "TryCheckout", new string[] { "BasePlayer" })]
		[Identifier("15f0dcb6d734464c8329e95f2a44f26f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_15f0dcb6d734464c8329e95f2a44f26f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_0052: Unknown result type (might be due to invalid IL or missing references)
				//IL_005c: Expected Obj, but got Unknown
				//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bb: Expected Obj, but got Unknown
				//IL_0106: Unknown result type (might be due to invalid IL or missing references)
				//IL_0110: Expected Obj, but got Unknown
				//IL_0117: Unknown result type (might be due to invalid IL or missing references)
				//IL_0121: Expected Obj, but got Unknown
				//IL_0128: Unknown result type (might be due to invalid IL or missing references)
				//IL_0132: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnApartmentRoomCheckout"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[4]
				{
					typeof(string),
					typeof(object),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[10];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldc_I4_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[10]), list2[10]);
				}
				list2.InsertRange(10, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Apartments_NPCApartmentSecurity
	{
		[Patch("OnApartmentMasterKeyPurchase", "OnApartmentMasterKeyPurchase", "NPCApartmentSecurity", "OnPurchaseKey", new string[] { "BasePlayer", "UnityEngine.Vector3" })]
		[Identifier("dbadd904984b47ebaa5f84a6ee165c51")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_dbadd904984b47ebaa5f84a6ee165c51 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1896999757)), instruction), instruction);
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

		[Patch("CanAffordApartmentMasterKey", "CanAffordApartmentMasterKey", "NPCApartmentSecurity", "Conversation_CanAffordMasterKey", new string[] { "BasePlayer" })]
		[Identifier("ce98e02ec1184fb38d8b0de282f82807")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "NPCApartmentSecurity", false)]
		[Return(typeof(bool))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_ce98e02ec1184fb38d8b0de282f82807 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1109259668)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnApartmentMasterKeyPurchased", "OnApartmentMasterKeyPurchased", "NPCApartmentSecurity", "OnPurchaseKey", new string[] { "BasePlayer", "UnityEngine.Vector3" })]
		[Identifier("4618264ec35d4807b343f61c118db5ad")]
		[Dependencies(new string[] { "OnApartmentMasterKeyPurchase" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local3", "Item", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_4618264ec35d4807b343f61c118db5ad : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 54)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)395022365), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
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

		[Patch("OnApartmentMasterKeyPurchased [Patch]", "OnApartmentMasterKeyPurchased [Patch]", "NPCApartmentSecurity", "OnPurchaseKey", new string[] { "BasePlayer", "UnityEngine.Vector3" })]
		[Identifier("7fb58b4a9ac14836982f63c66585a59e")]
		[Dependencies(new string[] { "OnApartmentMasterKeyPurchased" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_7fb58b4a9ac14836982f63c66585a59e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[59];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
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
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
