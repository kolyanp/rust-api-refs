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
		[Identifier("533209e62b4146f6ba4cdae8e581d11f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("activeCallTo", "PhoneController", false)]
		[Return(typeof(void))]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_533209e62b4146f6ba4cdae8e581d11f : Patch
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
		[Identifier("7e2006105a8547d1bc7c0af281dde60e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("activeCallTo", "PhoneController", false)]
		[Parameter("currentPlayer", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_7e2006105a8547d1bc7c0af281dde60e : Patch
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
		[Identifier("a0cad950c7d74aacb3315f05d78d8404")]
		[Dependencies(new string[] { "OnPhoneCallStart" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("activeCallTo", "PhoneController", false)]
		[Parameter("currentPlayer", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_a0cad950c7d74aacb3315f05d78d8404 : Patch
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
		[Identifier("38108cce823a4b4e8775dbf526a71e76")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Return(typeof(bool))]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_38108cce823a4b4e8775dbf526a71e76 : Patch
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
		[Identifier("4ab5cfb2e7c54bec91297dcfaa345edd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("local0", "PhoneController", false)]
		[Parameter("currentPlayer", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_4ab5cfb2e7c54bec91297dcfaa345edd : Patch
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
		[Identifier("aa752c7346b34d54af53112c84e95325")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("reason", "Telephone+DialFailReason", false)]
		[Parameter("currentPlayer", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_aa752c7346b34d54af53112c84e95325 : Patch
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
		[Identifier("5b4d4603e153422b993ad6297224da78")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("activeCallTo", "PhoneController", false)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("currentPlayer", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_5b4d4603e153422b993ad6297224da78 : Patch
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
		[Identifier("e9ba4e6960064a19af1dfc8b2aecc0dd")]
		[Dependencies(new string[] { "OnPhoneDialFail" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("reason", "Telephone+DialFailReason", false)]
		[Parameter("currentPlayer", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_e9ba4e6960064a19af1dfc8b2aecc0dd : Patch
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
		[Identifier("ddfbc754f5384ca4bed610cd9c7c8ab1")]
		[Dependencies(new string[] { "OnPhoneDialTimeout" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("activeCallTo", "PhoneController", false)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("currentPlayer", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_ddfbc754f5384ca4bed610cd9c7c8ab1 : Patch
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
		[Identifier("46e60d4e75e64624b0f8ba111200d24a")]
		[Dependencies(new string[] { "OnPhoneAnswer" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("activeCallTo", "PhoneController", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Phone")]
		[Assembly("Assembly-CSharp.dll")]
		public class Phone_PhoneController_46e60d4e75e64624b0f8ba111200d24a : Patch
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
