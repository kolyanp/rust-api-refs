using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Core;
using ConVar;
using Facepunch;
using HarmonyLib;
using UnityEngine;

namespace Carbon.Hooks;

public class Category_Fixes
{
	public class Fixes_FixDel
	{
		[Patch("IDelFix", "IDelFix", typeof(Hierarchy), "del", new Type[] { typeof(Arg) })]
		[Options(/*Could not decode attribute arguments.*/)]
		public class IDelFix : Patch
		{
			public static bool Prefix(Arg args)
			{
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_002f: Unknown result type (might be due to invalid IL or missing references)
				if (!args.HasArgs(1))
				{
					return false;
				}
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				string value = args.GetString(0, "");
				PooledList<BaseNetworkable> val = Pool.Get<PooledList<BaseNetworkable>>();
				try
				{
					Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							BaseNetworkable current = enumerator.Current;
							try
							{
								if ((Object)(object)current == (Object)null || (!current.PrefabName.Contains(value, StringComparison.InvariantCultureIgnoreCase) && !((object)current).GetType().Name.Equals(value, StringComparison.InvariantCultureIgnoreCase)))
								{
									continue;
								}
								if (BaseNetworkableEx.IsValid(current))
								{
									BasePlayer val2 = (BasePlayer)(object)((current is BasePlayer) ? current : null);
									if (val2 == null || !val2.IsConnected)
									{
										num++;
										((List<BaseNetworkable>)(object)val).Add(current);
									}
								}
								else
								{
									num2++;
								}
							}
							catch (Exception ex)
							{
								Logger.Error((object)$"Failed destroying '{current}'", ex);
								num3++;
							}
						}
					}
					finally
					{
						((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
					}
					for (int i = 0; i < ((List<BaseNetworkable>)(object)val).Count; i++)
					{
						((List<BaseNetworkable>)(object)val)[i].Kill((DestroyMode)0, true);
					}
					args.ReplyWith($"Deleted {num:n0} entities (found {num2:n0} invalid, {num3:n0} failed).");
					return false;
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
	}

	public class Fixes_ItemCrafter
	{
		[Patch("ICraftDurationMultiplier", "ICraftDurationMultiplier", typeof(ItemCrafter), "GetScaledDuration", new Type[]
		{
			typeof(ItemBlueprint),
			typeof(float),
			typeof(bool)
		})]
		[Options(/*Could not decode attribute arguments.*/)]
		public class ICraftDurationMultiplier : Patch
		{
			public static void Postfix(ItemBlueprint bp, float workbenchLevel, bool isInTutorial, ref float __result)
			{
				if (Community.Runtime.Core.ICraftDurationMultiplier(bp, workbenchLevel, isInTutorial) is float num)
				{
					__result *= num;
				}
			}
		}

		[Patch("IOvenSmeltSpeedMultiplier", "IOvenSmeltSpeedMultiplier", typeof(BaseOvenWorkQueue), "RunJob", new Type[] { typeof(BaseOven) })]
		[Options(/*Could not decode attribute arguments.*/)]
		public class IOvenSmeltSpeedMultiplier : Patch
		{
			public static bool Prefix(BaseOven oven)
			{
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_0051: Unknown result type (might be due to invalid IL or missing references)
				//IL_0065: Unknown result type (might be due to invalid IL or missing references)
				//IL_006a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0085: Unknown result type (might be due to invalid IL or missing references)
				//IL_009c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
				if (!(Community.Runtime.Core.IOvenSmeltSpeedMultiplier(oven) is float num))
				{
					return true;
				}
				if (TimeSince.op_Implicit(oven.lastCookUpdate) > 0.5f * num)
				{
					float num2 = Mathf.Floor(TimeSince.op_Implicit(oven.lastCookUpdate) / 0.5f);
					oven.lastCookUpdate = TimeSince.op_Implicit(TimeSince.op_Implicit(oven.lastCookUpdate) - num2 * (0.5f * num));
					oven.Cook(0.5f * num2);
				}
				if (oven.visualFood && TimeSince.op_Implicit(oven.lastCookVisualsUpdate) > 0.05f)
				{
					oven.lastCookVisualsUpdate = TimeSince.op_Implicit(0f);
					oven.CookVisuals();
				}
				return false;
			}
		}
	}

	public class Fixes_MixingTable
	{
		[Patch("IMixingSpeedMultiplier", "IMixingSpeedMultiplier", typeof(MixingTable), "StartMixing", new Type[] { typeof(BasePlayer) })]
		[Options(/*Could not decode attribute arguments.*/)]
		public class IMixingSpeedMultiplier : Patch
		{
			public static void Postfix(BasePlayer player, ref MixingTable __instance)
			{
				float remainingMixTime = __instance.RemainingMixTime;
				if (remainingMixTime > 0f && Community.Runtime.Core.IMixingSpeedMultiplier(__instance, remainingMixTime) is float num)
				{
					MixingTable obj = __instance;
					MixingTable obj2 = __instance;
					float remainingMixTime2 = (obj2.TotalMixTime /= num);
					obj.RemainingMixTime = remainingMixTime2;
					((BaseNetworkable)__instance).SendNetworkUpdateImmediate();
				}
			}
		}
	}

	public class Fixes_Excavator
	{
		[Patch("IOnExcavatorInit", "IOnExcavatorInit", typeof(ExcavatorArm), "Init", new Type[] { })]
		[Options(/*Could not decode attribute arguments.*/)]
		public class IOnExcavatorInit : Patch
		{
			private static void Postfix(ExcavatorArm __instance)
			{
				Community.Runtime.Core.IOnExcavatorInit(__instance);
			}
		}
	}

	public class Fixes_Recycler
	{
		[Patch("IRecyclerThinkSpeed", "IRecyclerThinkSpeed", typeof(Recycler), "GetRecycleThinkDuration", new Type[] { })]
		[Options(/*Could not decode attribute arguments.*/)]
		public class IRecyclerThinkSpeed : Patch
		{
			private static void Postfix(Recycler __instance, ref float __result)
			{
				if (Community.Runtime.Core.IRecyclerThinkSpeed(__instance) is float num)
				{
					__result *= num;
				}
			}
		}

		[Patch("IResearchDuration", "IResearchDuration", typeof(ResearchTable), "DoResearch", new Type[] { typeof(RPCMessage) })]
		[Options(/*Could not decode attribute arguments.*/)]
		public class IResearchDuration : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				MethodInfo runtime = AccessTools.PropertyGetter(typeof(Community), "Runtime");
				MethodInfo core = AccessTools.PropertyGetter(typeof(Community), "Core");
				MethodInfo multiplier = AccessTools.PropertyGetter(typeof(CorePlugin), "RuntimeResearchDurationMultiplier");
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					switch (x)
					{
					case 30:
						yield return new CodeInstruction(OpCodes.Call, (object)runtime);
						yield return new CodeInstruction(OpCodes.Callvirt, (object)core);
						yield return new CodeInstruction(OpCodes.Callvirt, (object)multiplier);
						yield return new CodeInstruction(OpCodes.Mul, (object)null);
						break;
					case 38:
						yield return new CodeInstruction(OpCodes.Call, (object)runtime);
						yield return new CodeInstruction(OpCodes.Callvirt, (object)core);
						yield return new CodeInstruction(OpCodes.Callvirt, (object)multiplier);
						yield return new CodeInstruction(OpCodes.Mul, (object)null);
						break;
					}
					yield return instruction;
					x++;
				}
			}
		}
	}

	public class Fixes_VendingMachine
	{
		[Patch("IVendingBuyDuration", "IVendingBuyDuration", typeof(VendingMachine), "GetBuyDuration", new Type[] { })]
		[Options(/*Could not decode attribute arguments.*/)]
		public class IVendingBuyDuration1 : Patch
		{
			public static void Postfix(VendingMachine __instance, ref float __result)
			{
				if (Community.Runtime.Core.IVendingBuyDuration() is float num)
				{
					__result *= num;
				}
			}
		}

		[Patch("IVendingBuyDuration", "IVendingBuyDuration", typeof(InvisibleVendingMachine), "GetBuyDuration", new Type[] { })]
		[Options(/*Could not decode attribute arguments.*/)]
		public class IVendingBuyDuration2 : Patch
		{
			public static void Postfix(InvisibleVendingMachine __instance, ref float __result)
			{
				if (Community.Runtime.Core.IVendingBuyDuration() is float num)
				{
					__result *= num;
				}
			}
		}
	}
}
