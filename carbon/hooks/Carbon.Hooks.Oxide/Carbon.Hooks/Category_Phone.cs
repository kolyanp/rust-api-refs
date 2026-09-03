using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Phone
{
	public class Phone_PhoneController
	{
		[Patch("OnPhoneAnswer", "OnPhoneAnswer", "PhoneController", "AnswerPhone", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("eb9dbbe3b9d4452f84741e96899a2115")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("self1", "PhoneController", false)]
		[Return(typeof(void))]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_eb9dbbe3b9d4452f84741e96899a2115 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1818430455), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("PhoneController"), "activeCallTo"));
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

		[Patch("OnPhoneCallStart", "OnPhoneCallStart", "PhoneController", "BeginCall", new string[] { })]
		[Identifier("21df16595a3245cd85c8ed35719fd00d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("self1", "PhoneController", false)]
		[Parameter("self2", "PhoneController", false)]
		[Return(typeof(void))]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_21df16595a3245cd85c8ed35719fd00d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-758892827)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("PhoneController"), "activeCallTo"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("PhoneController"), "get_currentPlayer", (Type[])null, (Type[])null));
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

		[Patch("OnPhoneCallStarted", "OnPhoneCallStarted", "PhoneController", "BeginCall", new string[] { })]
		[Identifier("e1b0af1c4d66475999d77733e6353552")]
		[Dependencies(new string[] { "OnPhoneCallStart" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("self1", "PhoneController", false)]
		[Parameter("self2", "PhoneController", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_e1b0af1c4d66475999d77733e6353552 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 37)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1453968022)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("PhoneController"), "activeCallTo"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("PhoneController"), "get_currentPlayer", (Type[])null, (Type[])null));
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

		[Patch("CanReceiveCall", "CanReceiveCall", "PhoneController", "CanReceiveCall", new string[] { })]
		[Identifier("9761d81a4d9148de9ced8688b80e7fc6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Return(typeof(bool))]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_9761d81a4d9148de9ced8688b80e7fc6 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1680123496), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
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

		[Patch("OnPhoneDial", "OnPhoneDial", "PhoneController", "CallPhone", new string[] { "System.Int32" })]
		[Identifier("b8cccfd9b8a54e7fa6979158249d9550")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("local0", "PhoneController", false)]
		[Parameter("self1", "PhoneController", false)]
		[Return(typeof(void))]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_b8cccfd9b8a54e7fa6979158249d9550 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1241230080), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("PhoneController"), "get_currentPlayer", (Type[])null, (Type[])null));
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

		[Patch("OnPhoneDialFail", "OnPhoneDialFail", "PhoneController", "OnDialFailed", new string[] { "Telephone/DialFailReason" })]
		[Identifier("0c73e6d9adde48619da23e90328649cd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("reason", "Telephone+DialFailReason", false)]
		[Parameter("self1", "PhoneController", false)]
		[Return(typeof(void))]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_0c73e6d9adde48619da23e90328649cd : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1739398446)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(DialFailReason));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("PhoneController"), "get_currentPlayer", (Type[])null, (Type[])null));
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

		[Patch("OnPhoneDialTimeout", "OnPhoneDialTimeout", "PhoneController", "TimeOutDialing", new string[] { })]
		[Identifier("9d5cc7350a7d4e28a3f92d9b646671bf")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("self1", "PhoneController", false)]
		[Parameter("self2", "PhoneController", false)]
		[Return(typeof(void))]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_9d5cc7350a7d4e28a3f92d9b646671bf : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)772238272), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("PhoneController"), "activeCallTo"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("PhoneController"), "activeCallTo"));
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("PhoneController"), "get_currentPlayer", (Type[])null, (Type[])null));
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

		[Patch("OnPhoneDialFailed", "OnPhoneDialFailed", "PhoneController", "OnDialFailed", new string[] { "Telephone/DialFailReason" })]
		[Identifier("1a9558039c2f4f86b34b697b59dcd084")]
		[Dependencies(new string[] { "OnPhoneDialFail" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("reason", "Telephone+DialFailReason", false)]
		[Parameter("self1", "PhoneController", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_1a9558039c2f4f86b34b697b59dcd084 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 55)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1386337056)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(DialFailReason));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("PhoneController"), "get_currentPlayer", (Type[])null, (Type[])null));
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

		[Patch("OnPhoneDialTimedOut", "OnPhoneDialTimedOut", "PhoneController", "TimeOutDialing", new string[] { })]
		[Identifier("647fe42ad22247af84e0e678a4ba50f2")]
		[Dependencies(new string[] { "OnPhoneDialTimeout" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("self1", "PhoneController", false)]
		[Parameter("self2", "PhoneController", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_647fe42ad22247af84e0e678a4ba50f2 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)31481961), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("PhoneController"), "activeCallTo"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("PhoneController"), "activeCallTo"));
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("PhoneController"), "get_currentPlayer", (Type[])null, (Type[])null));
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

		[Patch("OnPhoneAnswered", "OnPhoneAnswered", "PhoneController", "AnswerPhone", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("ac2f0e10111a4e3ca412e010dd4003c7")]
		[Dependencies(new string[] { "OnPhoneAnswer" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("self1", "PhoneController", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_ac2f0e10111a4e3ca412e010dd4003c7 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)459265237), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("PhoneController"), "activeCallTo"));
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
