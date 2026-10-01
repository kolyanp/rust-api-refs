using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;
using UnityEngine;

namespace Carbon.Hooks;

public class Category_Fishing
{
	public class Fishing_BaseFishingRod
	{
		[Patch("OnFishingStopped", "OnFishingStopped", "BaseFishingRod", "Server_Cancel", new string[] { "BaseFishingRod/FailReason" })]
		[Identifier("2d34042f8c414b1491dcebcbf9c10f52")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseFishingRod", false)]
		[Parameter("reason", "BaseFishingRod+FailReason", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Fishing")]
		[Assembly("Assembly-CSharp.dll")]
		public class Fishing_BaseFishingRod_2d34042f8c414b1491dcebcbf9c10f52 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 78)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)197690385), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(FailReason));
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

		[Patch("OnFishingRodCast", "OnFishingRodCast", "BaseFishingRod", "Server_RequestCast", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("6874f896d5314f9b92a4f392327d0c90")]
		[Dependencies(new string[] { "CanCastFishingRod" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseFishingRod", false)]
		[Parameter("local1", "BasePlayer", false)]
		[Parameter("local2", "Item", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Fishing")]
		[Assembly("Assembly-CSharp.dll")]
		public class Fishing_BaseFishingRod_6874f896d5314f9b92a4f392327d0c90 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 234)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)264708708), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
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

		[Patch("OnFishCaught", "OnFishCaught", "BaseFishingRod", "CatchProcessBudgeted", new string[] { })]
		[Identifier("40fbf900813a465484d1dced8277e930")]
		[Dependencies(new string[] { "OnFishCatch" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("currentFishTarget", "ItemDefinition", false)]
		[Parameter("self", "BaseFishingRod", false)]
		[Parameter("local2", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Fishing")]
		[Assembly("Assembly-CSharp.dll")]
		public class Fishing_BaseFishingRod_40fbf900813a465484d1dced8277e930 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 641)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1488593482)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseFishingRod"), "currentFishTarget"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
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

		[Patch("CanCastFishingRod", "CanCastFishingRod", "BaseFishingRod", "Server_RequestCast", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("f288ff1d06d94985b0fb70fce667e4b7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Fishing")]
		[Assembly("Assembly-CSharp.dll")]
		public class Fishing_BaseFishingRod_f288ff1d06d94985b0fb70fce667e4b7 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Expected Obj, but got Unknown
				//IL_008b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0095: Expected Obj, but got Unknown
				//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
				//IL_0101: Expected Obj, but got Unknown
				//IL_0143: Unknown result type (might be due to invalid IL or missing references)
				//IL_014d: Expected Obj, but got Unknown
				//IL_0168: Unknown result type (might be due to invalid IL or missing references)
				//IL_0172: Expected Obj, but got Unknown
				//IL_019b: Unknown result type (might be due to invalid IL or missing references)
				//IL_01a5: Expected Obj, but got Unknown
				//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
				//IL_01bb: Expected Obj, but got Unknown
				//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
				//IL_01cc: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"CanCastFishingRod"));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 2, typeof(object)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(Vector3)));
				list.Add(new CodeInstruction(OpCodes.Box, (object)typeof(Vector3)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[5]
				{
					typeof(string),
					typeof(object),
					typeof(object),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 11, typeof(object)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 11, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Isinst, (object)typeof(bool)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[28];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 11, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(bool)));
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[28]), list2[28]);
				}
				list2.InsertRange(28, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("CanCatchFish", "CanCatchFish", "BaseFishingRod", "CatchProcessBudgeted", new string[] { })]
		[Identifier("755e316f57a643c8ac961ded25ca0ac3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Fishing")]
		[Assembly("Assembly-CSharp.dll")]
		public class Fishing_BaseFishingRod_755e316f57a643c8ac961ded25ca0ac3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Expected Obj, but got Unknown
				//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c3: Expected Obj, but got Unknown
				//IL_0105: Unknown result type (might be due to invalid IL or missing references)
				//IL_010f: Expected Obj, but got Unknown
				//IL_012d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0137: Expected Obj, but got Unknown
				//IL_0160: Unknown result type (might be due to invalid IL or missing references)
				//IL_016a: Expected Obj, but got Unknown
				//IL_0176: Unknown result type (might be due to invalid IL or missing references)
				//IL_0180: Expected Obj, but got Unknown
				//IL_0187: Unknown result type (might be due to invalid IL or missing references)
				//IL_0191: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"CanCatchFish"));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 2, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 16, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[4]
				{
					typeof(string),
					typeof(object),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 18, typeof(object)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 18, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Isinst, (object)typeof(bool)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[527];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 18, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(bool)));
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[527]), list2[527]);
				}
				list2.InsertRange(527, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnFishCatch", "OnFishCatch", "BaseFishingRod", "CatchProcessBudgeted", new string[] { })]
		[Identifier("a664aeedb3c24214b06819d584664fb3")]
		[Dependencies(new string[] { "CanCatchFish" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Fishing")]
		[Assembly("Assembly-CSharp.dll")]
		public class Fishing_BaseFishingRod_a664aeedb3c24214b06819d584664fb3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0042: Unknown result type (might be due to invalid IL or missing references)
				//IL_004c: Expected Obj, but got Unknown
				//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c3: Expected Obj, but got Unknown
				//IL_0105: Unknown result type (might be due to invalid IL or missing references)
				//IL_010f: Expected Obj, but got Unknown
				//IL_012e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0138: Expected Obj, but got Unknown
				//IL_0161: Unknown result type (might be due to invalid IL or missing references)
				//IL_016b: Expected Obj, but got Unknown
				//IL_0190: Unknown result type (might be due to invalid IL or missing references)
				//IL_019a: Expected Obj, but got Unknown
				//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
				//IL_01cd: Expected Obj, but got Unknown
				//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
				//IL_0205: Expected Obj, but got Unknown
				//IL_022e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0238: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnFishCatch"));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 16, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 2, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[4]
				{
					typeof(string),
					typeof(object),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 19, typeof(object)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 19, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Isinst, (object)typeof(Item)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[540];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 19, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Isinst, (object)typeof(Item)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 16, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 16, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldc_R4, (object)0f));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Item"), "Remove", new Type[1] { typeof(float) }, (Type[])null)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 19, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Castclass, (object)typeof(Item)));
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 16, typeof(object)));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[540]), list2[540]);
				}
				list2.InsertRange(540, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
