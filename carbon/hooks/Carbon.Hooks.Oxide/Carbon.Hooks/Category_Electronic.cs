using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Electronic
{
	public class Electronic_IOEntity
	{
		[Patch("OnOutputUpdate", "OnOutputUpdate", "IOEntity", "UpdateOutputs", new string[] { })]
		[Identifier("b7c28fbff2224900b452b31e06d64fe8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "IOEntity", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_IOEntity_b7c28fbff2224900b452b31e06d64fe8 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-745018695)), instruction);
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

		[Patch("OnInputUpdate", "OnInputUpdate", "IOEntity", "UpdateFromInput", new string[] { "System.Int32", "System.Int32" })]
		[Identifier("573cf5b902c94a9e97cc8a5d718e966e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "IOEntity", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_IOEntity_573cf5b902c94a9e97cc8a5d718e966e : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1169047808), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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
	}

	public class Electronic_CardReader
	{
		[Patch("OnCardSwipe", "OnCardSwipe", "CardReader", "ServerCardSwiped", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("38cb1ed2a6114fce8e17affc2c76601c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CardReader", false)]
		[Parameter("local2", "Keycard", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_CardReader_38cb1ed2a6114fce8e17affc2c76601c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 64)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-761113184)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
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
	}

	public class Electronic_DigitalClock
	{
		[Patch("OnDigitalClockRing", "OnDigitalClockRing", "DigitalClock", "Ring", new string[] { })]
		[Identifier("e2331bc6301f442686841cac2be29584")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DigitalClock", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_DigitalClock_e2331bc6301f442686841cac2be29584 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)762478235), instruction), instruction);
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

		[Patch("OnDigitalClockRingStop", "OnDigitalClockRingStop", "DigitalClock", "StopRinging", new string[] { })]
		[Identifier("8f81c97ad731482295b3901dc9b4cf99")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DigitalClock", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_DigitalClock_8f81c97ad731482295b3901dc9b4cf99 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1462957189), instruction), instruction);
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

		[Patch("OnDigitalClockAlarmsSet", "OnDigitalClockAlarmsSet", "DigitalClock", "RPC_SetAlarms", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("b2baff809b1140d9bdb8a5d4a5aba853")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DigitalClock", false)]
		[Parameter("local0", "ProtoBuf.DigitalClockMessage", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_DigitalClock_b2baff809b1140d9bdb8a5d4a5aba853 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 11)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1813865950), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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
	}

	public class Electronic_PressButton
	{
		[Patch("OnButtonPress", "OnButtonPress", "PressButton", "RPC_Press", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("210dae32196a420e8337d82e5b510323")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PressButton", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PressButton_210dae32196a420e8337d82e5b510323 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1678863929)), instruction), instruction);
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
	}

	public class Electronic_PhoneController
	{
		[Patch("OnPhoneNameUpdate", "OnPhoneNameUpdate", "PhoneController", "UpdatePhoneName", new string[] { "System.String" })]
		[Identifier("24313cf435684dc78f0234590cd4e4ae")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("newName", "System.String", false)]
		[Parameter("currentPlayer", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PhoneController_24313cf435684dc78f0234590cd4e4ae : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1255605595)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("OnPhoneNameUpdated", "OnPhoneNameUpdated", "PhoneController", "UpdatePhoneName", new string[] { "System.String" })]
		[Identifier("20979874177c430fb09e9e65492c67df")]
		[Dependencies(new string[] { "OnPhoneNameUpdate" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("phoneName", "System.String", false)]
		[Parameter("currentPlayer", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PhoneController_20979874177c430fb09e9e65492c67df : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)587926495), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("PhoneController"), "PhoneName"));
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
	}

	public class Electronic_SolarPanel
	{
		[Patch("OnSolarPanelSunUpdate", "OnSolarPanelSunUpdate", "SolarPanel", "SunUpdate", new string[] { })]
		[Identifier("116670cc31734bf2995fe19d3775474b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SolarPanel", false)]
		[Parameter("local0", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_SolarPanel_116670cc31734bf2995fe19d3775474b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 58)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1805755454), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
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

	public class Electronic_AutoTurret
	{
		[Patch("OnEntityControl", "OnEntityControl [AutoTurret]", "AutoTurret", "CanControl", new string[] { "System.UInt64" })]
		[Identifier("432d80d4a9f74469969c0983b600efe7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Return(typeof(bool))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_AutoTurret_432d80d4a9f74469969c0983b600efe7 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1276273354), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
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

	public class Electronic_PoweredRemoteControlEntity
	{
		[Patch("OnEntityControl", "OnEntityControl [PoweredRemoteControl]", "PoweredRemoteControlEntity", "CanControl", new string[] { "System.UInt64" })]
		[Identifier("41ae724377fe40d6967553e30778fa41")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PoweredRemoteControlEntity", false)]
		[Return(typeof(bool))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PoweredRemoteControlEntity_41ae724377fe40d6967553e30778fa41 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1276273354), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
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

	public class Electronic_RemoteControlEntity
	{
		[Patch("OnEntityControl", "OnEntityControl [RemoteControlEntity]", "RemoteControlEntity", "CanControl", new string[] { "System.UInt64" })]
		[Identifier("2faf1951c4844176b6e67448d2281ff5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RemoteControlEntity", false)]
		[Return(typeof(bool))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_RemoteControlEntity_2faf1951c4844176b6e67448d2281ff5 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1276273354), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
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

	public class Electronic_IOEntityIORef
	{
		[Patch("OnIORefCleared", "OnIORefCleared", "IOEntity/IORef", "Clear", new string[] { })]
		[Identifier("9d303b0d259e48a98cfa864a4a8b0c80")]
		[Dependencies(new string[] { "OnIORefCleared [patch]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "IOEntity+IORef", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_IOEntityIORef_9d303b0d259e48a98cfa864a4a8b0c80 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 10)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)92897215), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return __GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object));
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

		[Patch("OnIORefCleared [patch]", "OnIORefCleared [patch]", "IOEntity/IORef", "Clear", new string[] { })]
		[Identifier("61becc0a89b741819c1052fc077b58f0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_IOEntityIORef_61becc0a89b741819c1052fc077b58f0 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected Obj, but got Unknown
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("IOEntity/IORef"), "ioEnt")));
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 0, typeof(object)));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[0]), list2[0]);
				}
				list2.InsertRange(0, list);
				return list2.AsEnumerable();
			}
		}
	}

	public class Electronic_CCTVRC
	{
		[Patch("OnCCTVDirectionChange", "OnCCTVDirectionChange", "CCTV_RC", "Server_SetDir", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("81cb52a4e4484cb18b87a7e303dff0ef")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CCTV_RC", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_CCTVRC_81cb52a4e4484cb18b87a7e303dff0ef : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 14)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1209570562), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

	public class Electronic_ExcavatorSignalComputer
	{
		[Patch("OnExcavatorSuppliesRequest", "OnExcavatorSuppliesRequest", "ExcavatorSignalComputer", "RequestSupplies", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("871f5545923a47a39ad87da2daf47877")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorSignalComputer", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_ExcavatorSignalComputer_871f5545923a47a39ad87da2daf47877 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 15)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)134449885), instruction);
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}

		[Patch("OnExcavatorSuppliesRequested", "OnExcavatorSuppliesRequested", "ExcavatorSignalComputer", "RequestSupplies", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("10106a11bfad4d8fa3e11ef0b09a8745")]
		[Dependencies(new string[] { "OnExcavatorSuppliesRequest" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorSignalComputer", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_ExcavatorSignalComputer_10106a11bfad4d8fa3e11ef0b09a8745 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)49012084), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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
	}

	public class Electronic_PowergridManager
	{
		[Patch("OnPowergridStageChange", "OnPowergridStageChange", "PowergridManager", "ServerTick", new string[] { })]
		[Identifier("2d249aa6fd7e40a79faa4236074eda27")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PowergridManager", false)]
		[Parameter("local2", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowergridManager_2d249aa6fd7e40a79faa4236074eda27 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 33)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-750891661)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
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

		[Patch("OnPowergridStageChanged", "OnPowergridStageChanged", "PowergridManager", "ServerTick", new string[] { })]
		[Identifier("fb8182efa69f4bd9a397e1d5f8579a52")]
		[Dependencies(new string[] { "OnPowergridStageChange [Patch]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PowergridManager", false)]
		[Parameter("local2", "System.Int32", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowergridManager_fb8182efa69f4bd9a397e1d5f8579a52 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)586404566), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
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

		[Patch("OnPowergridStageChange", "OnPowergridStageChange [Patch]", "PowergridManager", "ServerTick", new string[] { })]
		[Identifier("4b32a54a5ef7478daee79cca52e4daaf")]
		[Dependencies(new string[] { "OnPowergridStageChange" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowergridManager_4b32a54a5ef7478daee79cca52e4daaf : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected Obj, but got Unknown
				//IL_0058: Unknown result type (might be due to invalid IL or missing references)
				//IL_0062: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[41];
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label));
				Label label2 = Generator.DefineLabel();
				list2[94].labels.Add(label2);
				list.Add(new CodeInstruction(OpCodes.Leave, (object)label2));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[39]), list2[39]);
				}
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[40].labels);
				}
				else
				{
					list2[41].labels.AddRange(list2[40].labels);
				}
				list2[40].labels.Clear();
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[41], list2[39]), list2[39]);
				}
				list2.RemoveRange(39, 2);
				list2.InsertRange(39, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Electronic_PowerCounter
	{
		[Patch("OnCounterTargetChange", "OnCounterTargetChange", "PowerCounter", "SERVER_SetTarget", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("c38b7c6f73bb49548a7a3a715b16322b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowerCounter_c38b7c6f73bb49548a7a3a715b16322b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected Obj, but got Unknown
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Expected Obj, but got Unknown
				//IL_005e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Expected Obj, but got Unknown
				//IL_008b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0095: Expected Obj, but got Unknown
				//IL_009c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a6: Expected Obj, but got Unknown
				//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b7: Expected Obj, but got Unknown
				//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00db: Expected Obj, but got Unknown
				//IL_0103: Unknown result type (might be due to invalid IL or missing references)
				//IL_010d: Expected Obj, but got Unknown
				//IL_0162: Unknown result type (might be due to invalid IL or missing references)
				//IL_016c: Expected Obj, but got Unknown
				//IL_0186: Unknown result type (might be due to invalid IL or missing references)
				//IL_0190: Expected Obj, but got Unknown
				//IL_0197: Unknown result type (might be due to invalid IL or missing references)
				//IL_01a1: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity/RPCMessage"), "read")));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Network.NetRead"), "Int32", (Type[])null, (Type[])null)));
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 0, typeof(int)));
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnCounterTargetChange"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity/RPCMessage"), "player")));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(int)));
				list.Add(new CodeInstruction(OpCodes.Box, (object)typeof(int)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[4]
				{
					typeof(string),
					typeof(object),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[0];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
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

		[Patch("OnCounterTargetChange [patch]", "OnCounterTargetChange [patch]", "PowerCounter", "SERVER_SetTarget", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("8c680aa78edb4ff1b5ca830e436803b8")]
		[Dependencies(new string[] { "OnCounterTargetChange" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowerCounter_8c680aa78edb4ff1b5ca830e436803b8 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object)));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[20]), list2[20]);
				}
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[21].labels);
				}
				else
				{
					list2[23].labels.AddRange(list2[21].labels);
				}
				list2[21].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[22].labels);
				}
				else
				{
					list2[23].labels.AddRange(list2[22].labels);
				}
				list2[22].labels.Clear();
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[23], list2[20]), list2[20]);
				}
				list2.RemoveRange(20, 3);
				list2.InsertRange(20, list);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnCounterModeToggle", "OnCounterModeToggle", "PowerCounter", "ToggleDisplayMode", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("c380747a36ad48f18a6fe057f2cc431a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowerCounter_c380747a36ad48f18a6fe057f2cc431a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected Obj, but got Unknown
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Expected Obj, but got Unknown
				//IL_005e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Expected Obj, but got Unknown
				//IL_008b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0095: Expected Obj, but got Unknown
				//IL_009c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a6: Expected Obj, but got Unknown
				//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b7: Expected Obj, but got Unknown
				//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00db: Expected Obj, but got Unknown
				//IL_0103: Unknown result type (might be due to invalid IL or missing references)
				//IL_010d: Expected Obj, but got Unknown
				//IL_0162: Unknown result type (might be due to invalid IL or missing references)
				//IL_016c: Expected Obj, but got Unknown
				//IL_0186: Unknown result type (might be due to invalid IL or missing references)
				//IL_0190: Expected Obj, but got Unknown
				//IL_0197: Unknown result type (might be due to invalid IL or missing references)
				//IL_01a1: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity/RPCMessage"), "read")));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Network.NetRead"), "Bit", (Type[])null, (Type[])null)));
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 0, typeof(bool)));
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnCounterModeToggle"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity/RPCMessage"), "player")));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(bool)));
				list.Add(new CodeInstruction(OpCodes.Box, (object)typeof(bool)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[4]
				{
					typeof(string),
					typeof(object),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[0];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
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

		[Patch("OnCounterModeToggle [patch]", "OnCounterModeToggle [patch]", "PowerCounter", "ToggleDisplayMode", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("c2fea44ae4fc46d981c2e8a15e186729")]
		[Dependencies(new string[] { "OnCounterModeToggle" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowerCounter_c2fea44ae4fc46d981c2e8a15e186729 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object)));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[20]), list2[20]);
				}
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[21].labels);
				}
				else
				{
					list2[23].labels.AddRange(list2[21].labels);
				}
				list2[21].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[22].labels);
				}
				else
				{
					list2[23].labels.AddRange(list2[22].labels);
				}
				list2[22].labels.Clear();
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[23], list2[20]), list2[20]);
				}
				list2.RemoveRange(20, 3);
				list2.InsertRange(20, list);
				return list2.AsEnumerable();
			}
		}
	}

	public class Electronic_HBHFSensor
	{
		[Patch("OnSensorDetect", "OnSensorDetect", "HBHFSensor", "CountDetectedPlayers", new string[] { })]
		[Identifier("650eedafa15a4c149a4819f50a051db6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_HBHFSensor_650eedafa15a4c149a4819f50a051db6 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Expected Obj, but got Unknown
				//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c5: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnSensorDetect"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 5, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[135];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[44]), list2[44]);
				}
				list2.InsertRange(44, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
