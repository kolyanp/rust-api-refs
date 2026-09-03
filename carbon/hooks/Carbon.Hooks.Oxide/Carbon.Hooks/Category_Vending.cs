using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Vending
{
	public class Vending_VendingMachine
	{
		[Patch("OnBuyVendingItem", "OnBuyVendingItem", "VendingMachine", "BuyItem", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("689c1cb098ae45ee9d3a5ec34407feb7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VendingMachine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "System.Int32", false)]
		[Parameter("local1", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_689c1cb098ae45ee9d3a5ec34407feb7 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1340282264)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
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

		[Patch("CanUseVending", "CanUseVending", "VendingMachine", "CanOpenLootPanel", new string[] { "BasePlayer", "System.String" })]
		[Identifier("5518833af2d14e52afd04049ebed3cba")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "VendingMachine", false)]
		[Return(typeof(bool))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_5518833af2d14e52afd04049ebed3cba : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1307174715), instruction), instruction);
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

		[Patch("CanAdministerVending", "CanAdministerVending", "VendingMachine", "CanPlayerAdmin", new string[] { "BasePlayer" })]
		[Identifier("f1a0593eb72c4ed9a3b735f0b9c788c6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "VendingMachine", false)]
		[Return(typeof(bool))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_f1a0593eb72c4ed9a3b735f0b9c788c6 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1222882713)), instruction), instruction);
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

		[Patch("OnRefreshVendingStock", "OnRefreshVendingStock", "VendingMachine", "RefreshSellOrderStockLevel", new string[] { "ItemDefinition" })]
		[Identifier("c20793975cbf4e57920d4a7e5267c67c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VendingMachine", false)]
		[Parameter("itemDef", "ItemDefinition", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_c20793975cbf4e57920d4a7e5267c67c : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1089385978), instruction);
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

		[Patch("OnToggleVendingBroadcast", "OnToggleVendingBroadcast", "VendingMachine", "RPC_Broadcast", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("2d8711f5085548849dfc27ac44e9c559")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VendingMachine", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_2d8711f5085548849dfc27ac44e9c559 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1473728487)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnDeleteVendingOffer", "OnDeleteVendingOffer", "VendingMachine", "RPC_DeleteSellOrder", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("0070ef72bfc74a348e4f983413d44ddd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VendingMachine", false)]
		[Parameter("local1", "System.Int32", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_0070ef72bfc74a348e4f983413d44ddd : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1045055657), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
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

		[Patch("OnOpenVendingAdmin", "OnOpenVendingAdmin", "VendingMachine", "RPC_OpenAdmin", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("ea5584767bac4fcdaa15e0b363611fa3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VendingMachine", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_ea5584767bac4fcdaa15e0b363611fa3 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)130032208), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnVendingShopOpen", "OnVendingShopOpen [VendingMachine]", "VendingMachine", "RPC_OpenShop", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("68c5797e8e7144af91c3864010bf26ff")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VendingMachine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_68c5797e8e7144af91c3864010bf26ff : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1029092944), instruction), instruction);
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

		[Patch("OnRotateVendingMachine", "OnRotateVendingMachine", "VendingMachine", "RPC_RotateVM", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("2506e53a5d62462a8ebdc21ab52f2684")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VendingMachine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_2506e53a5d62462a8ebdc21ab52f2684 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1043196312)), instruction), instruction);
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

		[Patch("CanVendingAcceptItem", "CanVendingAcceptItem", "VendingMachine", "CanAcceptItem", new string[] { "BasePlayer", "Item", "System.Int32" })]
		[Identifier("2a0784805c804700b5c3bd70b626a184")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VendingMachine", false)]
		[Parameter("item", "Item", false)]
		[Parameter("targetSlot", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_2a0784805c804700b5c3bd70b626a184 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-236006696)), instruction), instruction);
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

		[Patch("OnAddVendingOffer", "OnAddVendingOffer", "VendingMachine", "AddSellOrder", new string[] { "System.Int32", "System.Int32", "System.Int32", "System.Int32", "System.Byte", "System.UInt64", "System.UInt64" })]
		[Identifier("2a2bb96e7c454c758f6514fd9fb6f5a7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VendingMachine", false)]
		[Parameter("local2", "ProtoBuf.VendingMachine+SellOrder", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_2a2bb96e7c454c758f6514fd9fb6f5a7 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 69)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-75432783)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
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

		[Patch("OnGiveSoldItem", "OnGiveSoldItem", "VendingMachine", "GiveSoldItem", new string[] { "Item", "BasePlayer" })]
		[Identifier("27534eba885b4bc5bb8443428bb1f6dd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VendingMachine", false)]
		[Parameter("soldItem", "Item", false)]
		[Parameter("buyer", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_27534eba885b4bc5bb8443428bb1f6dd : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)42703263), instruction), instruction);
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

		[Patch("OnVendingShopRename", "OnVendingShopRename", "VendingMachine", "RPC_UpdateShopName", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("ca4013ccbec249ddabb15eb017d62a83")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VendingMachine", false)]
		[Parameter("local1", "System.String", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_ca4013ccbec249ddabb15eb017d62a83 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-619471185)), instruction), instruction);
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
					Label label = Generator.DefineLabel();
					instruction.labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnTakeCurrencyItem", "OnTakeCurrencyItem", "VendingMachine", "TakeCurrencyItem", new string[] { "Item" })]
		[Identifier("c9c63a67be2a41188c653e560b3d9d18")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VendingMachine", false)]
		[Parameter("takenCurrencyItem", "Item", false)]
		[Return(typeof(void))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_c9c63a67be2a41188c653e560b3d9d18 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)819770087), instruction), instruction);
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

		[Patch("OnVendingTransaction", "OnVendingTransaction", "VendingMachine", "DoTransaction", new string[] { "BasePlayer", "System.Int32", "System.Int32", "ItemContainer", "System.Action`2<BasePlayer,Item>", "System.Action`2<BasePlayer,Item>", "MarketTerminal" })]
		[Identifier("997043d1a2af4906832800a0bd212f26")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VendingMachine", false)]
		[Parameter("buyer", "BasePlayer", false)]
		[Parameter("sellOrderId", "System.Int32", false)]
		[Parameter("numberOfTransactions", "System.Int32", false)]
		[Parameter("targetContainer", "ItemContainer", false)]
		[Return(typeof(bool))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_997043d1a2af4906832800a0bd212f26 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 24)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)101737999), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)4);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[6]
					{
						typeof(uint),
						typeof(object),
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

		[Patch("CanPurchaseItem", "CanPurchaseItem", "VendingMachine", "DoTransaction", new string[] { "BasePlayer", "System.Int32", "System.Int32", "ItemContainer", "System.Action`2<BasePlayer,Item>", "System.Action`2<BasePlayer,Item>", "MarketTerminal" })]
		[Identifier("5175a5470059452e918b4363531cf6b1")]
		[Dependencies(new string[] { "OnVendingTransaction" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("buyer", "BasePlayer", false)]
		[Parameter("local21", "Item", false)]
		[Parameter("onItemPurchased", "System.Action`2[BasePlayer,Item]", false)]
		[Parameter("self", "VendingMachine", false)]
		[Parameter("targetContainer", "ItemContainer", false)]
		[Return(typeof(bool))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_5175a5470059452e918b4363531cf6b1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 323)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-876277535)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)21);
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)6);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)4);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[6]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
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

		[Patch("OnVendingShopOpened", "OnVendingShopOpened [VendingMachine]", "VendingMachine", "RPC_OpenShop", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("e9bf3d14be5d4f8a8ad26efb960684b3")]
		[Dependencies(new string[] { "OnVendingShopOpen [VendingMachine]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VendingMachine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_VendingMachine_e9bf3d14be5d4f8a8ad26efb960684b3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 20)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)397104876), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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
	}

	public class Vending_NPCVendingMachine
	{
		[Patch("CanAdministerVending", "CanAdministerVending [NPC]", "NPCVendingMachine", "CanPlayerAdmin", new string[] { "BasePlayer" })]
		[Identifier("f423fa5c781f4fa1981039b16f14e471")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "NPCVendingMachine", false)]
		[Return(typeof(bool))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_NPCVendingMachine_f423fa5c781f4fa1981039b16f14e471 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1222882713)), instruction), instruction);
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

		[Patch("OnTakeCurrencyItem", "OnTakeCurrencyItem [NPC]", "NPCVendingMachine", "TakeCurrencyItem", new string[] { "Item" })]
		[Identifier("0aa140caf475408f9ffd710b771c1bca")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NPCVendingMachine", false)]
		[Parameter("takenCurrencyItem", "Item", false)]
		[Return(typeof(void))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_NPCVendingMachine_0aa140caf475408f9ffd710b771c1bca : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)819770087), instruction), instruction);
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
	}

	public class Vending_MarketTerminal
	{
		[Patch("CanAccessVendingMachine", "CanAccessVendingMachine", "MarketTerminal", "<GetDeliveryEligibleVendingMachines>g__IsEligible|27_0", new string[] { "VendingMachine", "UnityEngine.Vector3", "System.Int32" })]
		[Identifier("79c985d42f234bfb87a6d1bcdbddc4b5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MarketTerminal", false)]
		[Parameter("vendingMachine", "VendingMachine", false)]
		[Return(typeof(bool))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_MarketTerminal_79c985d42f234bfb87a6d1bcdbddc4b5 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1949289818), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("MarketTerminal"), "config"));
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
	}

	public class Vending_NPCTalking
	{
		[Patch("OnVendingShopOpen", "OnVendingShopOpen [NPCTalking]", "NPCTalking", "OnConversationAction", new string[] { "BasePlayer", "System.String" })]
		[Identifier("cd14c0c3127448dc8026a0b186787c8f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "InvisibleVendingMachine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_NPCTalking_cd14c0c3127448dc8026a0b186787c8f : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1029092944), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}

		[Patch("OnVendingShopOpened", "OnVendingShopOpened [NPCTalking]", "NPCTalking", "OnConversationAction", new string[] { "BasePlayer", "System.String" })]
		[Identifier("255d6f68152f43cdafabc6c6932a2d10")]
		[Dependencies(new string[] { "OnVendingShopOpen [NPCTalking]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "InvisibleVendingMachine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_NPCTalking_255d6f68152f43cdafabc6c6932a2d10 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 36)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)397104876), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

	public class Vending_TravellingVendor
	{
		[Patch("OnVendingShopOpen", "OnVendingShopOpen [TravellingVendor]", "TravellingVendor", "SV_OpenMenu", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("1030a42fe08e4987b76f872c378babd2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TravellingVendor", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_TravellingVendor_1030a42fe08e4987b76f872c378babd2 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1029092944), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("TravellingVendor"), "vendingMachine"));
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

		[Patch("OnVendingShopOpened", "OnVendingShopOpened [TravellingVendor]", "TravellingVendor", "SV_OpenMenu", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("f0fb0040d1ff4c929065704b3153167b")]
		[Dependencies(new string[] { "OnVendingShopOpen [TravellingVendor]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TravellingVendor", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vending")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vending_TravellingVendor_f0fb0040d1ff4c929065704b3153167b : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)397104876), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("TravellingVendor"), "vendingMachine"));
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
	}
}
