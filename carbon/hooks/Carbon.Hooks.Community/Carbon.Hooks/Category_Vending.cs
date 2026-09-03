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
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 66)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2145652880), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(typeof(RPCMessage), "player"));
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[6]
					{
						typeof(uint),
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
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}
}
