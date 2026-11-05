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
		[Identifier("e5fd2a5e74bb4dee98de043ad146ce66")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "IOEntity", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_IOEntity_e5fd2a5e74bb4dee98de043ad146ce66 : Patch
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
		[Identifier("30df435153f049f5bd6d41ebf0788349")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "IOEntity", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_IOEntity_30df435153f049f5bd6d41ebf0788349 : Patch
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
		[Identifier("04624b46be73409b8cfa469620886ec4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CardReader", false)]
		[Parameter("local2", "Keycard", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_CardReader_04624b46be73409b8cfa469620886ec4 : Patch
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
		[Identifier("4d365bdfd44841169c0f52d6601bcc0c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DigitalClock", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_DigitalClock_4d365bdfd44841169c0f52d6601bcc0c : Patch
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
		[Identifier("bf8463d7de55421bab2e8379edc9b331")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DigitalClock", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_DigitalClock_bf8463d7de55421bab2e8379edc9b331 : Patch
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
		[Identifier("cee8d8a9e8d748809b5427c00cb68b1b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DigitalClock", false)]
		[Parameter("local0", "ProtoBuf.DigitalClockMessage", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_DigitalClock_cee8d8a9e8d748809b5427c00cb68b1b : Patch
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
		[Identifier("dd73a09fc935439eb461837bbdf1ae96")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PressButton", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PressButton_dd73a09fc935439eb461837bbdf1ae96 : Patch
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
		[Identifier("74c5db09f279428284d36b691b487434")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("newName", "System.String", false)]
		[Parameter("currentPlayer", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PhoneController_74c5db09f279428284d36b691b487434 : Patch
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
		[Identifier("09ddd4e58fed47c78a55bf017e56b4fa")]
		[Dependencies(new string[] { "OnPhoneNameUpdate" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhoneController", false)]
		[Parameter("phoneName", "System.String", false)]
		[Parameter("currentPlayer", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PhoneController_09ddd4e58fed47c78a55bf017e56b4fa : Patch
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
		[Identifier("e9f1a786028443578dcd8454a9aff13b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SolarPanel", false)]
		[Parameter("local0", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_SolarPanel_e9f1a786028443578dcd8454a9aff13b : Patch
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
		[Identifier("5269a3cf0f7a421ebbe08179aa680b44")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Return(typeof(bool))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_AutoTurret_5269a3cf0f7a421ebbe08179aa680b44 : Patch
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
		[Identifier("384c6c7362834bd8bd24108713d17184")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PoweredRemoteControlEntity", false)]
		[Return(typeof(bool))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PoweredRemoteControlEntity_384c6c7362834bd8bd24108713d17184 : Patch
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
		[Identifier("792ea466157b4a85b8de22da7ff8a20d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RemoteControlEntity", false)]
		[Return(typeof(bool))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_RemoteControlEntity_792ea466157b4a85b8de22da7ff8a20d : Patch
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
		[Identifier("f813bfcb27184ab8b0fc2b7d43a8c1e0")]
		[Dependencies(new string[] { "OnIORefCleared [patch]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "IOEntity+IORef", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_IOEntityIORef_f813bfcb27184ab8b0fc2b7d43a8c1e0 : Patch
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
		[Identifier("3028adf0a78e404dbea8832b2cd4d2e1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_IOEntityIORef_3028adf0a78e404dbea8832b2cd4d2e1 : Patch
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
		[Identifier("51ca91c8190547b983c95cc0b0690f89")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CCTV_RC", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_CCTVRC_51ca91c8190547b983c95cc0b0690f89 : Patch
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
		[Identifier("61566be400374b39a21ec2c2a137d4ea")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorSignalComputer", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_ExcavatorSignalComputer_61566be400374b39a21ec2c2a137d4ea : Patch
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
		[Identifier("7a745e2c889d437b87d6841b40b14446")]
		[Dependencies(new string[] { "OnExcavatorSuppliesRequest" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ExcavatorSignalComputer", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_ExcavatorSignalComputer_7a745e2c889d437b87d6841b40b14446 : Patch
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
		[Identifier("af82090b9640483f96deeae3bba40f3a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PowergridManager", false)]
		[Parameter("local2", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowergridManager_af82090b9640483f96deeae3bba40f3a : Patch
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
		[Identifier("ed7e07195b0d4cb6a1c03935d877e6af")]
		[Dependencies(new string[] { "OnPowergridStageChange [Patch]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PowergridManager", false)]
		[Parameter("local2", "System.Int32", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowergridManager_ed7e07195b0d4cb6a1c03935d877e6af : Patch
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
		[Identifier("c77ab1d95b8044ec8a648e9931db2666")]
		[Dependencies(new string[] { "OnPowergridStageChange" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowergridManager_c77ab1d95b8044ec8a648e9931db2666 : Patch
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
		[Identifier("1d9b2d3453624eb88d2d5b0ab3c25b69")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowerCounter_1d9b2d3453624eb88d2d5b0ab3c25b69 : Patch
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
		[Identifier("0658ef1fd9494f6f8b58cc782596a6fc")]
		[Dependencies(new string[] { "OnCounterTargetChange" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowerCounter_0658ef1fd9494f6f8b58cc782596a6fc : Patch
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
		[Identifier("c19064af0a85479595e4a5aec84b4350")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowerCounter_c19064af0a85479595e4a5aec84b4350 : Patch
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
		[Identifier("3b806bf4b5b64c1990408c4b59cfee95")]
		[Dependencies(new string[] { "OnCounterModeToggle" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_PowerCounter_3b806bf4b5b64c1990408c4b59cfee95 : Patch
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
		[Identifier("7ae72c5c64e842758bf1cd14541eaafb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Electronic")]
		[Assembly("Assembly-CSharp.dll")]
		public class Electronic_HBHFSensor_7ae72c5c64e842758bf1cd14541eaafb : Patch
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
