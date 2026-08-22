using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Network
{
	public class Network_BaseNetworkable
	{
		[Patch("CanNetworkTo", "CanNetworkTo", "BaseNetworkable", "ShouldNetworkTo", new string[] { "BasePlayer" })]
		[Identifier("0d8b02ce8ad7476f9c7840bc5c75bc54")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNetworkable", false)]
		[Return(typeof(bool))]
		[Category("Network")]
		[Assembly("Assembly-CSharp.dll")]
		public class Network_BaseNetworkable_0d8b02ce8ad7476f9c7840bc5c75bc54 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1622751857), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnNetworkGroupEntered", "OnNetworkGroupEntered", "BaseNetworkable", "OnNetworkGroupEnter", new string[] { "Network.Visibility.Group" })]
		[Identifier("0dd61fe27e364d7f9c2759b4021a339c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNetworkable", false)]
		[Category("Network")]
		[Assembly("Assembly-CSharp.dll")]
		public class Network_BaseNetworkable_0dd61fe27e364d7f9c2759b4021a339c : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1855185185)), instruction), instruction);
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

		[Patch("OnNetworkGroupLeft", "OnNetworkGroupLeft", "BaseNetworkable", "OnNetworkGroupLeave", new string[] { "Network.Visibility.Group" })]
		[Identifier("6eee07fffabd4d88ae360a90ef54e7f0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNetworkable", false)]
		[Category("Network")]
		[Assembly("Assembly-CSharp.dll")]
		public class Network_BaseNetworkable_6eee07fffabd4d88ae360a90ef54e7f0 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)226983605), instruction), instruction);
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

	public class Network_NetworkVisibilityGrid
	{
		[Patch("OnNetworkSubscriptionsGather", "OnNetworkSubscriptionsGather", "NetworkVisibilityGrid", "GetVisibleFrom", new string[] { "Network.Visibility.Group", "ListHashSet`1<Network.Visibility.Group>", "System.Int32" })]
		[Identifier("1a7ec4f0ce6f45f8baa1a829a90eaf58")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NetworkVisibilityGrid", false)]
		[Return(typeof(void))]
		[Category("Network")]
		[Assembly("Assembly-CSharp.dll")]
		public class Network_NetworkVisibilityGrid_1a7ec4f0ce6f45f8baa1a829a90eaf58 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)26233677), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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
	}

	public class Network_NetworkNetworkable
	{
		[Patch("OnNetworkSubscriptionsUpdate", "OnNetworkSubscriptionsUpdate", "Network.Networkable", "UpdateSubscriptions", new string[] { "System.Int32", "System.Int32" })]
		[Identifier("792508f8469e416b889ed4aa4f9e2e2a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Network")]
		[Assembly("Facepunch.Network.dll")]
		public class Network_NetworkNetworkable_792508f8469e416b889ed4aa4f9e2e2a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c2: Expected O, but got Unknown
				//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e7: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnNetworkSubscriptionsUpdate"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 2, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[4]
				{
					typeof(string),
					typeof(object),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[120];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[31]), list2[31]);
				}
				list2.InsertRange(31, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnNetworkSubscriptionsUpdate", "OnNetworkSubscriptionsUpdate [2]", "Network.Networkable", "UpdateHighPrioritySubscriptions", new string[] { })]
		[Identifier("bd00e97f43db4d1fafc582f2e1b06d29")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Network")]
		[Assembly("Facepunch.Network.dll")]
		public class Network_NetworkNetworkable_bd00e97f43db4d1fafc582f2e1b06d29 : Patch
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
				//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e0: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnNetworkSubscriptionsUpdate"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldnull, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[4]
				{
					typeof(string),
					typeof(object),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[56];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[26]), list2[26]);
				}
				list2.InsertRange(26, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
