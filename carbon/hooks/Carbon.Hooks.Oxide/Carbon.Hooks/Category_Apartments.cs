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
		[Identifier("30952b0f4e714c069b0acac74571f4b1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_30952b0f4e714c069b0acac74571f4b1 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)801743095), instruction), instruction);
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
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnRentableShopClosed", "OnRentableShopClosed", "RentableShop", "OnShopClosed", new string[] { "System.Boolean" })]
		[Identifier("66c6803a8ce24b7ca9ea54e93f861233")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_66c6803a8ce24b7ca9ea54e93f861233 : Patch
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
		[Identifier("b768a69dc95b4a278e77da43cb8f7d95")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_b768a69dc95b4a278e77da43cb8f7d95 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 150)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1762235317)), instruction), instruction);
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
		[Identifier("553d8cf0941f493e81e9baa20380dff1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_553d8cf0941f493e81e9baa20380dff1 : Patch
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
		[Identifier("ca58ec57273f470c9e8ce0d2033568a5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_ca58ec57273f470c9e8ce0d2033568a5 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-602182365)), instruction), instruction);
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

		[Patch("OnRentableShopBreakInCompleted", "OnRentableShopBreakInCompleted", "RentableShop", "CompleteBreakIn", new string[] { "BasePlayer" })]
		[Identifier("8b263d6ebd5447a3aa100d3c27e13b65")]
		[Dependencies(new string[] { "OnRentableShopBreakInComplete" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_8b263d6ebd5447a3aa100d3c27e13b65 : Patch
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
		[Identifier("295b17bb09c9480ebae54b387c2243c5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ApartmentDoor", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentDoor_295b17bb09c9480ebae54b387c2243c5 : Patch
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
		[Identifier("8dc0c2040f214fc6aeba8c0cfe4f8259")]
		[Dependencies(new string[] { "OnApartmentRoomBreakInComplete" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ApartmentDoor", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentDoor_8dc0c2040f214fc6aeba8c0cfe4f8259 : Patch
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
		[Identifier("69f6b1e046bc4afb94fe2c2bffd60d51")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_69f6b1e046bc4afb94fe2c2bffd60d51 : Patch
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
		[Identifier("c90b3ed8b0434e9ab789fb32cc8ec468")]
		[Dependencies(new string[] { "OnApartmentRoomCheckout" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_c90b3ed8b0434e9ab789fb32cc8ec468 : Patch
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
		[Identifier("32c6fbedb6a246b8b631a49c8214e421")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_32c6fbedb6a246b8b631a49c8214e421 : Patch
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
		[Identifier("946703bdbeb1470a9fe21058403058e4")]
		[Dependencies(new string[] { "OnApartmentRoomUpgrade" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_946703bdbeb1470a9fe21058403058e4 : Patch
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
		[Identifier("edf7f7ff42754ae485df380ebbc3e366")]
		[Dependencies(new string[] { "OnApartmentRoomPurchase" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_edf7f7ff42754ae485df380ebbc3e366 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 45)
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
		[Identifier("febfb6b890f048709d05d1bff4c0a0af")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_febfb6b890f048709d05d1bff4c0a0af : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_0052: Unknown result type (might be due to invalid IL or missing references)
				//IL_005c: Expected O, but got Unknown
				//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bb: Expected O, but got Unknown
				//IL_0106: Unknown result type (might be due to invalid IL or missing references)
				//IL_0110: Expected O, but got Unknown
				//IL_0117: Unknown result type (might be due to invalid IL or missing references)
				//IL_0121: Expected O, but got Unknown
				//IL_0128: Unknown result type (might be due to invalid IL or missing references)
				//IL_0132: Expected O, but got Unknown
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
				CodeInstruction obj = list2[10];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldc_I4_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[10]), list2[10]);
				}
				list2.InsertRange(10, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Apartments_NPCApartmentSecurity
	{
		[Patch("OnApartmentMasterKeyPurchase", "OnApartmentMasterKeyPurchase", "NPCApartmentSecurity", "OnPurchaseKey", new string[] { "BasePlayer", "UnityEngine.Vector3" })]
		[Identifier("5c2b072c33d34b3cb328c68c866cd012")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_5c2b072c33d34b3cb328c68c866cd012 : Patch
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
		[Identifier("307997a3270849b5949c6e64d413ef5e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "NPCApartmentSecurity", false)]
		[Return(typeof(bool))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_307997a3270849b5949c6e64d413ef5e : Patch
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
		[Identifier("d015d0e94e2f49c2a355013696c91339")]
		[Dependencies(new string[] { "OnApartmentMasterKeyPurchase" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local3", "Item", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_d015d0e94e2f49c2a355013696c91339 : Patch
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
		[Identifier("3011a8aa3f164eb7a29eb3a4e20e8606")]
		[Dependencies(new string[] { "OnApartmentMasterKeyPurchased" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_3011a8aa3f164eb7a29eb3a4e20e8606 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[59];
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
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
