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
		[Identifier("9625206bca374ba5b2a36279cc9c44a5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_9625206bca374ba5b2a36279cc9c44a5 : Patch
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
		[Identifier("4ece33f17d414a70b1c25ba32fe766c7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_4ece33f17d414a70b1c25ba32fe766c7 : Patch
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
		[Identifier("eb6171eed02a450081b4c9b125462c65")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_eb6171eed02a450081b4c9b125462c65 : Patch
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
		[Identifier("14b17fe3ba684679aa1e716afe1b8edd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_14b17fe3ba684679aa1e716afe1b8edd : Patch
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
		[Identifier("b3784a26a9814606a577f17f88f14e6c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_b3784a26a9814606a577f17f88f14e6c : Patch
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
		[Identifier("0c18c67d46fe48a4b9f4f7228eb0ebc1")]
		[Dependencies(new string[] { "OnRentableShopBreakInComplete" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_0c18c67d46fe48a4b9f4f7228eb0ebc1 : Patch
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
		[Identifier("f6cadb5107da4e75b5cbc38ec3b5eb56")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ApartmentDoor", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentDoor_f6cadb5107da4e75b5cbc38ec3b5eb56 : Patch
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
		[Identifier("e7feb94c7fc74745a19644a352ff8fd3")]
		[Dependencies(new string[] { "OnApartmentRoomBreakInComplete" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ApartmentDoor", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentDoor_e7feb94c7fc74745a19644a352ff8fd3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 112)
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
		[Identifier("aa798dc0e3ec4755b07b7da091eb5070")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_aa798dc0e3ec4755b07b7da091eb5070 : Patch
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
		[Identifier("b7f8ce97246b43a1953930c5674788ff")]
		[Dependencies(new string[] { "OnApartmentRoomCheckout" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_b7f8ce97246b43a1953930c5674788ff : Patch
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
		[Identifier("767268395a6449f298cf38d5fd87e841")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_767268395a6449f298cf38d5fd87e841 : Patch
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
		[Identifier("cce9c69a9b24493a9b62ccf99355ea26")]
		[Dependencies(new string[] { "OnApartmentRoomUpgrade" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_cce9c69a9b24493a9b62ccf99355ea26 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 94)
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
		[Identifier("84cd66b893da4377ad9b34d99151be70")]
		[Dependencies(new string[] { "OnApartmentRoomPurchase" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_84cd66b893da4377ad9b34d99151be70 : Patch
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
		[Identifier("856d2fc2716f482181f2689d91d6249e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_856d2fc2716f482181f2689d91d6249e : Patch
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
		[Identifier("62004ff102b540de80be2a1ab45b3cad")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_62004ff102b540de80be2a1ab45b3cad : Patch
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
		[Identifier("e38bf47ad73048c79c5698beee280b14")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "NPCApartmentSecurity", false)]
		[Return(typeof(bool))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_e38bf47ad73048c79c5698beee280b14 : Patch
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
		[Identifier("d0551c16f1524cdaa1cf3589c4ffdb58")]
		[Dependencies(new string[] { "OnApartmentMasterKeyPurchase" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local3", "Item", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_d0551c16f1524cdaa1cf3589c4ffdb58 : Patch
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
		[Identifier("23fb010a5b5d49428d76270bd16c224c")]
		[Dependencies(new string[] { "OnApartmentMasterKeyPurchased" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_23fb010a5b5d49428d76270bd16c224c : Patch
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
