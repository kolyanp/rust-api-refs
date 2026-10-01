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
		[Identifier("ba574c3aab7c4692a042ff8dfbd46a73")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "XMasRefill", false)]
		[Return(typeof(void))]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_XMasRefill_ba574c3aab7c4692a042ff8dfbd46a73 : Patch
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
		[Identifier("29a7c4ce55d04e35a04e17a1a74e72cb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_XMasRefill_29a7c4ce55d04e35a04e17a1a74e72cb : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected Obj, but got Unknown
				//IL_008c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0096: Expected Obj, but got Unknown
				//IL_009d: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a7: Expected Obj, but got Unknown
				//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cb: Expected Obj, but got Unknown
				//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00dc: Expected Obj, but got Unknown
				//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ed: Expected Obj, but got Unknown
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
				CodeInstruction val = list2[0];
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label));
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

	public class Seasonal_Stocking
	{
		[Patch("OnXmasStockingFill", "OnXmasStockingFill", "Stocking", "SpawnLoot", new string[] { })]
		[Identifier("4c4ed9fa8dc44bcd9fcbe8d6fbeeedf5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Stocking", false)]
		[Return(typeof(void))]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_Stocking_4c4ed9fa8dc44bcd9fcbe8d6fbeeedf5 : Patch
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
		[Identifier("c358c7336c2746fa893e8847745b89dc")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AdventCalendar", false)]
		[Return(typeof(void))]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_AdventCalendar_c358c7336c2746fa893e8847745b89dc : Patch
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
		[Identifier("66e211c604fb4a93a6941f302aa39bf9")]
		[Dependencies(new string[] { "OnAdventGiftAward" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AdventCalendar", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_AdventCalendar_66e211c604fb4a93a6941f302aa39bf9 : Patch
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
		[Identifier("43e14d18d81543f28ea810663560e62a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_AdventCalendar_43e14d18d81543f28ea810663560e62a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected Obj, but got Unknown
				//IL_008c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0096: Expected Obj, but got Unknown
				//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e0: Expected Obj, but got Unknown
				//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
				//IL_0104: Expected Obj, but got Unknown
				//IL_012c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0136: Expected Obj, but got Unknown
				//IL_013d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0147: Expected Obj, but got Unknown
				//IL_014e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0158: Expected Obj, but got Unknown
				//IL_015f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0169: Expected Obj, but got Unknown
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
				CodeInstruction val = list2[0];
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
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Seasonal_CollectableEasterEgg
	{
		[Patch("OnEventCollectablePickup", "OnEventCollectablePickup", "CollectableEasterEgg", "RPC_PickUp", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("1fb1d4cab98d4c99a622cf89f3bb625b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CollectableEasterEgg", false)]
		[Return(typeof(void))]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_CollectableEasterEgg_1fb1d4cab98d4c99a622cf89f3bb625b : Patch
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
		[Identifier("a8ed4916722d4b37a1daa1ee793f92e4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "EggHuntEvent", false)]
		[Return(typeof(void))]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_EggHuntEvent_a8ed4916722d4b37a1daa1ee793f92e4 : Patch
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
		[Identifier("5dddc7af77e84153b493c24dffaff20b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "EggHuntEvent", false)]
		[Return(typeof(void))]
		[Category("Seasonal")]
		[Assembly("Assembly-CSharp.dll")]
		public class Seasonal_EggHuntEvent_5dddc7af77e84153b493c24dffaff20b : Patch
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
