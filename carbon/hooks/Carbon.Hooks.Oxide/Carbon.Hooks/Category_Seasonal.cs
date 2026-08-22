using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Seasonal
{
	public class Seasonal_XMasRefill
	{
		[Patch("OnXmasLootDistribute", "OnXmasLootDistribute", "XMasRefill", "ServerInit", new string[] { })]
		[Identifier("efcfcb9674d145ae98306b1cf35d548d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "XMasRefill", false)]
		[Return(typeof(void))]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_XMasRefill_efcfcb9674d145ae98306b1cf35d548d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 34)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1609625545)), instruction), instruction);
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

		[Patch("OnXmasGiftsDistribute", "OnXmasGiftsDistribute", "XMasRefill", "DistributeGiftsForPlayer", new string[] { "BasePlayer" })]
		[Identifier("6911434c75664f0987d0bf661a74382c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_XMasRefill_6911434c75664f0987d0bf661a74382c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected O, but got Unknown
				//IL_008c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0096: Expected O, but got Unknown
				//IL_009d: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a7: Expected O, but got Unknown
				//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cb: Expected O, but got Unknown
				//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00dc: Expected O, but got Unknown
				//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ed: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnXmasGiftsDistribute"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Ldnull, (object)null));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[0];
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label));
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

	public class Seasonal_Stocking
	{
		[Patch("OnXmasStockingFill", "OnXmasStockingFill", "Stocking", "SpawnLoot", new string[] { })]
		[Identifier("1890fcf5c34e4c22b2a8967dccace43e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Stocking", false)]
		[Return(typeof(void))]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_Stocking_1890fcf5c34e4c22b2a8967dccace43e : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1849224565)), instruction);
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}
	}

	public class Seasonal_AdventCalendar
	{
		[Patch("OnAdventGiftAward", "OnAdventGiftAward", "AdventCalendar", "AwardGift", new string[] { "BasePlayer" })]
		[Identifier("ebaf81de85f74733b48bbe7eea897326")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AdventCalendar", false)]
		[Return(typeof(void))]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_AdventCalendar_ebaf81de85f74733b48bbe7eea897326 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1722451932)), instruction), instruction);
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

		[Patch("OnAdventGiftAwarded", "OnAdventGiftAwarded", "AdventCalendar", "AwardGift", new string[] { "BasePlayer" })]
		[Identifier("0c89a998c9d148059aa15d161a3ae493")]
		[Dependencies(new string[] { "OnAdventGiftAward" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AdventCalendar", false)]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_AdventCalendar_0c89a998c9d148059aa15d161a3ae493 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 172)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1624765317), instruction), instruction);
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

		[Patch("CanBeAwardedAdventGift", "CanBeAwardedAdventGift", "AdventCalendar", "WasAwardedTodaysGift", new string[] { "BasePlayer" })]
		[Identifier("728d67ba3f6e45459df4751d328ab2a9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_AdventCalendar_728d67ba3f6e45459df4751d328ab2a9 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected O, but got Unknown
				//IL_008c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0096: Expected O, but got Unknown
				//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e0: Expected O, but got Unknown
				//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
				//IL_0104: Expected O, but got Unknown
				//IL_012c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0136: Expected O, but got Unknown
				//IL_013d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0147: Expected O, but got Unknown
				//IL_014e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0158: Expected O, but got Unknown
				//IL_015f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0169: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"CanBeAwardedAdventGift"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 2, typeof(object)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 2, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Isinst, (object)typeof(bool)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[0];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 2, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(bool)));
				list.Add(new CodeInstruction(OpCodes.Ldc_I4_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ceq, (object)null));
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

	public class Seasonal_CollectableEasterEgg
	{
		[Patch("OnEventCollectablePickup", "OnEventCollectablePickup", "CollectableEasterEgg", "RPC_PickUp", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("0bd1726260d440888933ed3616b0acdb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CollectableEasterEgg", false)]
		[Return(typeof(void))]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_CollectableEasterEgg_0bd1726260d440888933ed3616b0acdb : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-6620460)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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
	}

	public class Seasonal_EggHuntEvent
	{
		[Patch("OnHuntEventStart", "OnHuntEventStart", "EggHuntEvent", "StartEvent", new string[] { })]
		[Identifier("89a19ea3c09c4a1997d38d96c88ef4bd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "EggHuntEvent", false)]
		[Return(typeof(void))]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_EggHuntEvent_89a19ea3c09c4a1997d38d96c88ef4bd : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)240697297), instruction), instruction);
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

		[Patch("OnHuntEventEnd", "OnHuntEventEnd", "EggHuntEvent", "Update", new string[] { })]
		[Identifier("2a9bfea6695e420093aa3a210ad99e42")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "EggHuntEvent", false)]
		[Return(typeof(void))]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_EggHuntEvent_2a9bfea6695e420093aa3a210ad99e42 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)261230988), instruction);
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}
	}
}
