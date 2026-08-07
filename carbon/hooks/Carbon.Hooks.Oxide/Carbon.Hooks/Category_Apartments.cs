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
		[Identifier("9230a9277efe4954926c7ce5d93d0d27")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_9230a9277efe4954926c7ce5d93d0d27 : Patch
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
		[Identifier("caac440c7e9f4b45a6c52c6540cd71b9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_caac440c7e9f4b45a6c52c6540cd71b9 : Patch
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
		[Identifier("85058b6d886644c3b1b6666150e6b7d3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_85058b6d886644c3b1b6666150e6b7d3 : Patch
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
		[Identifier("c2274199b24242d284036212d87f426b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_c2274199b24242d284036212d87f426b : Patch
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
		[Identifier("fe9da35f42fb48e3b047dcf162d42bbc")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_fe9da35f42fb48e3b047dcf162d42bbc : Patch
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
		[Identifier("941846d1ca90487884b7eb1766c642b1")]
		[Dependencies(new string[] { "OnRentableShopBreakInComplete" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RentableShop", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_RentableShop_941846d1ca90487884b7eb1766c642b1 : Patch
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
		[Identifier("ed79570f19914e9094ad9617ac869b30")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ApartmentDoor", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentDoor_ed79570f19914e9094ad9617ac869b30 : Patch
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
		[Identifier("3eb1b627bb5543fbadf0769a50df4272")]
		[Dependencies(new string[] { "OnApartmentRoomBreakInComplete" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ApartmentDoor", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentDoor_3eb1b627bb5543fbadf0769a50df4272 : Patch
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
		[Identifier("049d11ced4bc470d8f4c7dae082c5ae0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_049d11ced4bc470d8f4c7dae082c5ae0 : Patch
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
		[Identifier("c9b4e383a4f84fe1b93bf2eb624004cc")]
		[Dependencies(new string[] { "OnApartmentRoomCheckout" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_c9b4e383a4f84fe1b93bf2eb624004cc : Patch
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
		[Identifier("78870b79e85b49b9a189dcdbd8a553de")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_78870b79e85b49b9a189dcdbd8a553de : Patch
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
		[Identifier("5e4c14fde96c4ea9b9d50aa014e21288")]
		[Dependencies(new string[] { "OnApartmentRoomUpgrade" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_5e4c14fde96c4ea9b9d50aa014e21288 : Patch
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
		[Identifier("91074000620643048ce2dbf9cad38590")]
		[Dependencies(new string[] { "OnApartmentRoomPurchase" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "ApartmentRoom", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("size", "ApartmentSize", false)]
		[Parameter("self", "ApartmentBuilding", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_91074000620643048ce2dbf9cad38590 : Patch
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
		[Identifier("30f06f233eda439aac673e6b393775f4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_ApartmentBuilding_30f06f233eda439aac673e6b393775f4 : Patch
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
		[Identifier("42569ecdd92d4b708ae15e69d70f0f09")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_42569ecdd92d4b708ae15e69d70f0f09 : Patch
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
		[Identifier("f5486a6336f9438cb49c299edbc6bbfe")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "NPCApartmentSecurity", false)]
		[Return(typeof(bool))]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_f5486a6336f9438cb49c299edbc6bbfe : Patch
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
		[Identifier("bf65603275414e23866cc0d637af7731")]
		[Dependencies(new string[] { "OnApartmentMasterKeyPurchase" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local3", "Item", false)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_bf65603275414e23866cc0d637af7731 : Patch
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
		[Identifier("b3edd1ee937e4b4c8ab0573bac8d1424")]
		[Dependencies(new string[] { "OnApartmentMasterKeyPurchased" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Apartments")]
		[Assembly("Assembly-CSharp.dll")]
		public class Apartments_NPCApartmentSecurity_b3edd1ee937e4b4c8ab0573bac8d1424 : Patch
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
