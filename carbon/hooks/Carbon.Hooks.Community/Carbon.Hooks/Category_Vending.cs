using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Vending
{
	public class Vending_MarketTerminal
	{
		[Patch("OnMarketplaceTerminalPurchase", "OnMarketplaceTerminalPurchase", typeof(MarketTerminal), "Server_Purchase", new Type[] { typeof(RPCMessage) })]
		[Category("Vending")]
		[Parameter("terminal", typeof(MarketTerminal), false)]
		[Parameter("vending", typeof(VendingMachine), false)]
		[Parameter("player", typeof(BasePlayer), false)]
		[Parameter("sellOrderIndex", typeof(int), false)]
		[Parameter("amount", typeof(int), false)]
		[Info("Called before making a purchase at the Marketplace terminal.")]
		[Return(typeof(void))]
		public class OnMarketplaceTerminalPurchase : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0243: Unknown result type (might be due to invalid IL or missing references)
				//IL_024f: Expected Obj, but got Unknown
				//IL_025c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0266: Expected Obj, but got Unknown
				//IL_027d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0287: Expected Obj, but got Unknown
				//IL_028f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0299: Expected Obj, but got Unknown
				//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
				//IL_02c3: Expected Obj, but got Unknown
				//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
				//IL_02ed: Expected Obj, but got Unknown
				//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
				//IL_02ff: Expected Obj, but got Unknown
				//IL_030d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0317: Expected Obj, but got Unknown
				//IL_031f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0329: Expected Obj, but got Unknown
				MethodInfo methodInfo = AccessTools.Method(typeof(VendingMachine), "GetSlotsRequiredForTransaction", new Type[2]
				{
					typeof(int),
					typeof(int)
				}, (Type[])null);
				FieldInfo fieldInfo = AccessTools.Field(typeof(MarketTerminal), "_transactionActive");
				FieldInfo fieldInfo2 = AccessTools.Field(typeof(RPCMessage), "player");
				MethodInfo methodInfo2 = AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[6]
				{
					typeof(uint),
					typeof(object),
					typeof(object),
					typeof(object),
					typeof(object),
					typeof(object)
				}, (Type[])null);
				List<CodeInstruction> list = new List<CodeInstruction>(Instructions);
				if (methodInfo == null || fieldInfo == null || fieldInfo2 == null || methodInfo2 == null)
				{
					return Unpatched(list);
				}
				int num = -1;
				int num2 = -1;
				int num3 = -1;
				int num4 = -1;
				for (int i = 2; i < list.Count; i++)
				{
					CodeInstruction val = list[i];
					if (num == -1 && CodeInstructionExtensions.Calls(val, methodInfo) && i >= 3 && CodeInstructionExtensions.IsLdloc(list[i - 3], (LocalBuilder)null) && CodeInstructionExtensions.IsLdloc(list[i - 2], (LocalBuilder)null) && CodeInstructionExtensions.IsLdloc(list[i - 1], (LocalBuilder)null))
					{
						num = CodeInstructionExtensions.LocalIndex(list[i - 3]);
						num2 = CodeInstructionExtensions.LocalIndex(list[i - 2]);
						num3 = CodeInstructionExtensions.LocalIndex(list[i - 1]);
					}
					else if (num4 == -1 && CodeInstructionExtensions.StoresField(val, fieldInfo) && list[i - 1].opcode == OpCodes.Ldc_I4_1 && list[i - 2].opcode == OpCodes.Ldarg_0)
					{
						num4 = i - 2;
					}
				}
				if (num == -1 || num4 == -1)
				{
					return Unpatched(list);
				}
				CodeInstruction val2 = list[num4];
				Label label = Generator.DefineLabel();
				List<CodeInstruction> list2 = new List<CodeInstruction>();
				list2.Add(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2145652880), val2));
				list2.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list2.Add(CodeInstruction.LoadLocal(num, false));
				list2.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list2.Add(new CodeInstruction(OpCodes.Ldfld, (object)fieldInfo2));
				list2.Add(CodeInstruction.LoadLocal(num2, false));
				list2.Add(new CodeInstruction(OpCodes.Box, (object)typeof(int)));
				list2.Add(CodeInstruction.LoadLocal(num3, false));
				list2.Add(new CodeInstruction(OpCodes.Box, (object)typeof(int)));
				list2.Add(new CodeInstruction(OpCodes.Call, (object)methodInfo2));
				list2.Add(new CodeInstruction(OpCodes.Brfalse, (object)label));
				list2.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				List<CodeInstruction> collection = list2;
				val2.labels.Add(label);
				list.InsertRange(num4, collection);
				return list;
			}

			private static List<CodeInstruction> Unpatched(List<CodeInstruction> instructions)
			{
				Logger.Warn((object)"Failed patching 'OnMarketplaceTerminalPurchase', MarketTerminal.Server_Purchase no longer matches the expected IL");
				return instructions;
			}
		}
	}
}
