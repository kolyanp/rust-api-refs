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
		[Identifier("1e7c433b9acc4e6586db6954736be9cd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "IOEntity", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_IOEntity_1e7c433b9acc4e6586db6954736be9cd : Patch
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
		[Identifier("167a37741ee0460ca7e5b2eb50057b3f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "IOEntity", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_IOEntity_167a37741ee0460ca7e5b2eb50057b3f : Patch
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
		[Identifier("2677005d5a0246d1a4f0fee0ffdf52af")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CardReader", false)]
		[Parameter("local2", "Keycard", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_CardReader_2677005d5a0246d1a4f0fee0ffdf52af : Patch
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
		[Identifier("03065cf67e2b42178825222e37188cdc")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DigitalClock", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_DigitalClock_03065cf67e2b42178825222e37188cdc : Patch
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
		[Identifier("d27a9662815c41918b648fa8352af62c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DigitalClock", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_DigitalClock_d27a9662815c41918b648fa8352af62c : Patch
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
		[Identifier("0370b2d996df4baf9b529fde32a74218")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DigitalClock", false)]
		[Parameter("local0", "ProtoBuf.DigitalClockMessage", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_DigitalClock_0370b2d996df4baf9b529fde32a74218 : Patch
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
		[Identifier("9f612e18da074e1fac2c900bec88a98a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PressButton", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PressButton_9f612e18da074e1fac2c900bec88a98a : Patch
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
		[Identifier("d095125cf27544819fbeb1638f24b90f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("newName", "System.String", false)]
		[Parameter("self1", "PhoneController", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PhoneController_d095125cf27544819fbeb1638f24b90f : Patch
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
		[Identifier("881989ffd4a6430c9c281dcf63ad584b")]
		[Dependencies(new string[] { "OnPhoneNameUpdate" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("self1", "PhoneController", false)]
		[Parameter("self2", "PhoneController", false)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PhoneController_881989ffd4a6430c9c281dcf63ad584b : Patch
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
		[Identifier("f6910b9870ef44d58abe35ba7f70aff2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SolarPanel", false)]
		[Parameter("local0", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_SolarPanel_f6910b9870ef44d58abe35ba7f70aff2 : Patch
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
		[Identifier("a837cc63b8bc45bbbb588948be9a6b0d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Return(typeof(bool))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_AutoTurret_a837cc63b8bc45bbbb588948be9a6b0d : Patch
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
		[Identifier("c26ebe8c139748158b1c57cbfcb13b87")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PoweredRemoteControlEntity", false)]
		[Return(typeof(bool))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PoweredRemoteControlEntity_c26ebe8c139748158b1c57cbfcb13b87 : Patch
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
		[Identifier("10ebb8daef554a8f9084d237314688ae")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RemoteControlEntity", false)]
		[Return(typeof(bool))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_RemoteControlEntity_10ebb8daef554a8f9084d237314688ae : Patch
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
		[Identifier("5989543463dd4913b258a5f4d4025bba")]
		[Dependencies(new string[] { "OnIORefCleared [patch]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "IOEntity+IORef", false)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_IOEntityIORef_5989543463dd4913b258a5f4d4025bba : Patch
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
		[Identifier("b769a9d898254aa6bea99f20c47834a1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_IOEntityIORef_b769a9d898254aa6bea99f20c47834a1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Expected O, but got Unknown
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
		[Identifier("11df525ac1b54666836564d14375c9f1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CCTV_RC", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_CCTVRC_11df525ac1b54666836564d14375c9f1 : Patch
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
		[Identifier("d8c4c950e7b143ad885602e26f06859d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorSignalComputer", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_ExcavatorSignalComputer_d8c4c950e7b143ad885602e26f06859d : Patch
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
		[Identifier("a4afd02042624587a1b605a5eee82b66")]
		[Dependencies(new string[] { "OnExcavatorSuppliesRequest" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorSignalComputer", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_ExcavatorSignalComputer_a4afd02042624587a1b605a5eee82b66 : Patch
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

	public class Electronic_PowerCounter
	{
		[Patch("OnCounterTargetChange", "OnCounterTargetChange", "PowerCounter", "SERVER_SetTarget", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("f4e9b3bc7c4d4c53bd38c8fca771dda5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowerCounter_f4e9b3bc7c4d4c53bd38c8fca771dda5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Expected O, but got Unknown
				//IL_005e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Expected O, but got Unknown
				//IL_008b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0095: Expected O, but got Unknown
				//IL_009c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a6: Expected O, but got Unknown
				//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b7: Expected O, but got Unknown
				//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00db: Expected O, but got Unknown
				//IL_0103: Unknown result type (might be due to invalid IL or missing references)
				//IL_010d: Expected O, but got Unknown
				//IL_0162: Unknown result type (might be due to invalid IL or missing references)
				//IL_016c: Expected O, but got Unknown
				//IL_0186: Unknown result type (might be due to invalid IL or missing references)
				//IL_0190: Expected O, but got Unknown
				//IL_0197: Unknown result type (might be due to invalid IL or missing references)
				//IL_01a1: Expected O, but got Unknown
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
				CodeInstruction obj = list2[0];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
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

		[Patch("OnCounterTargetChange [patch]", "OnCounterTargetChange [patch]", "PowerCounter", "SERVER_SetTarget", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("068fa6ed9e214f7e9404be637253afd0")]
		[Dependencies(new string[] { "OnCounterTargetChange" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowerCounter_068fa6ed9e214f7e9404be637253afd0 : Patch
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
		[Identifier("d1bcf6a0957145389ed89684dbfef139")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowerCounter_d1bcf6a0957145389ed89684dbfef139 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Expected O, but got Unknown
				//IL_005e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Expected O, but got Unknown
				//IL_008b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0095: Expected O, but got Unknown
				//IL_009c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a6: Expected O, but got Unknown
				//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b7: Expected O, but got Unknown
				//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00db: Expected O, but got Unknown
				//IL_0103: Unknown result type (might be due to invalid IL or missing references)
				//IL_010d: Expected O, but got Unknown
				//IL_0162: Unknown result type (might be due to invalid IL or missing references)
				//IL_016c: Expected O, but got Unknown
				//IL_0186: Unknown result type (might be due to invalid IL or missing references)
				//IL_0190: Expected O, but got Unknown
				//IL_0197: Unknown result type (might be due to invalid IL or missing references)
				//IL_01a1: Expected O, but got Unknown
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
				CodeInstruction obj = list2[0];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
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

		[Patch("OnCounterModeToggle [patch]", "OnCounterModeToggle [patch]", "PowerCounter", "ToggleDisplayMode", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("e2bc8c9a2cd84b3f93e7ccbc787cd0ca")]
		[Dependencies(new string[] { "OnCounterModeToggle" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowerCounter_e2bc8c9a2cd84b3f93e7ccbc787cd0ca : Patch
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
		[Identifier("89f1cdb58ed5416494e1c82634cf7d9c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_HBHFSensor_89f1cdb58ed5416494e1c82634cf7d9c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Expected O, but got Unknown
				//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c5: Expected O, but got Unknown
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
				CodeInstruction obj = list2[135];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[44]), list2[44]);
				}
				list2.InsertRange(44, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
