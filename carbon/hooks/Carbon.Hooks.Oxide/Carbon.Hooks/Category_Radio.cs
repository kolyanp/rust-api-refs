using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Radio
{
	public class Radio_RFManager
	{
		[Patch("OnRfListenerAdd", "OnRfListenerAdd", "RFManager", "AddListener", new string[] { "System.Int32", "IRFObject" })]
		[Identifier("dfee2340aba84bac99b049828aeadc6e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("obj", "IRFObject", false)]
		[Parameter("frequency", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_RFManager_dfee2340aba84bac99b049828aeadc6e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 3)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1710662532), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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

		[Patch("OnRfListenerRemove", "OnRfListenerRemove", "RFManager", "RemoveListener", new string[] { "System.Int32", "IRFObject" })]
		[Identifier("019c668607c04c9aa8136d2c198775b9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("obj", "IRFObject", false)]
		[Parameter("frequency", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_RFManager_019c668607c04c9aa8136d2c198775b9 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 3)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1199277452), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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

		[Patch("OnRfBroadcasterAdd", "OnRfBroadcasterAdd", "RFManager", "AddBroadcaster", new string[] { "System.Int32", "IRFObject" })]
		[Identifier("60f50a0dec114182b3de9c5621555cd3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("obj", "IRFObject", false)]
		[Parameter("frequency", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_RFManager_60f50a0dec114182b3de9c5621555cd3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 3)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1737418697)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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

		[Patch("OnRfBroadcasterRemove", "OnRfBroadcasterRemove", "RFManager", "RemoveBroadcaster", new string[] { "System.Int32", "IRFObject" })]
		[Identifier("a9069846a42a48289eae0a2cb3e750c9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("obj", "IRFObject", false)]
		[Parameter("frequency", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_RFManager_a9069846a42a48289eae0a2cb3e750c9 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 3)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-960527201)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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

		[Patch("OnRfBroadcasterAdded", "OnRfBroadcasterAdded", "RFManager", "AddBroadcaster", new string[] { "System.Int32", "IRFObject" })]
		[Identifier("306d64ca6a204e44a010c2db9af518e5")]
		[Dependencies(new string[] { "OnRfBroadcasterAdd" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("obj", "IRFObject", false)]
		[Parameter("frequency", "System.Int32", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_RFManager_306d64ca6a204e44a010c2db9af518e5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 34)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)153067018), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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

		[Patch("OnRfListenerRemoved", "OnRfListenerRemoved", "RFManager", "RemoveListener", new string[] { "System.Int32", "IRFObject" })]
		[Identifier("363458b1cf4c478c85934aa0c303cdee")]
		[Dependencies(new string[] { "OnRfListenerRemove" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("obj", "IRFObject", false)]
		[Parameter("frequency", "System.Int32", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_RFManager_363458b1cf4c478c85934aa0c303cdee : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 19)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1188761815)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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

		[Patch("OnRfListenerAdded", "OnRfListenerAdded", "RFManager", "AddListener", new string[] { "System.Int32", "IRFObject" })]
		[Identifier("1626b8825f174dd1a2201e87da00e71d")]
		[Dependencies(new string[] { "OnRfListenerAdd" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("obj", "IRFObject", false)]
		[Parameter("frequency", "System.Int32", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_RFManager_1626b8825f174dd1a2201e87da00e71d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 27)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1595251141), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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

		[Patch("OnRfBroadcasterRemoved", "OnRfBroadcasterRemoved", "RFManager", "RemoveBroadcaster", new string[] { "System.Int32", "IRFObject" })]
		[Identifier("4c9f2526fb4543b3ae54c88f827db6b0")]
		[Dependencies(new string[] { "OnRfBroadcasterRemove" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("obj", "IRFObject", false)]
		[Parameter("frequency", "System.Int32", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_RFManager_4c9f2526fb4543b3ae54c88f827db6b0 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1010277064)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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

		[Patch("OnRfListenerRemoved [patch]", "OnRfListenerRemoved [patch]", "RFManager", "RemoveListener", new string[] { "System.Int32", "IRFObject" })]
		[Identifier("d0ea708a60f74b37ac9bea18396d6724")]
		[Dependencies(new string[] { "OnRfListenerRemoved" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_RFManager_d0ea708a60f74b37ac9bea18396d6724 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[25];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[15]), list2[15]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[16], list2[15]), list2[15]);
				}
				list2.RemoveRange(15, 1);
				list2.InsertRange(15, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Radio_RFBroadcaster
	{
		[Patch("OnRfFrequencyChange", "OnRfFrequencyChange [Broadcaster]", "RFBroadcaster", "ServerSetFrequency", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("e867a0f6f9c744e7904f9c764226f80a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RFBroadcaster", false)]
		[Parameter("local0", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_RFBroadcaster_e867a0f6f9c744e7904f9c764226f80a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 28)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-415055395)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnRfFrequencyChanged", "OnRfFrequencyChanged [Broadcaster]", "RFBroadcaster", "ServerSetFrequency", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("cbaeb4272f9d44e398d27a97e8edfd05")]
		[Dependencies(new string[] { "OnRfFrequencyChange [Broadcaster]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RFBroadcaster", false)]
		[Parameter("local0", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_RFBroadcaster_cbaeb4272f9d44e398d27a97e8edfd05 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 50)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)784684429), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

	public class Radio_RFReceiver
	{
		[Patch("OnRfFrequencyChange", "OnRfFrequencyChange [Receiver]", "RFReceiver", "ServerSetFrequency", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("9c7a82225217443c91b80f14d84b0664")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RFReceiver", false)]
		[Parameter("local0", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_RFReceiver_9c7a82225217443c91b80f14d84b0664 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-415055395)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnRfFrequencyChanged", "OnRfFrequencyChanged [Receiver]", "RFReceiver", "ServerSetFrequency", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("efca5f5df23d4435809fc67175a2287d")]
		[Dependencies(new string[] { "OnRfFrequencyChange [Receiver]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "RFReceiver", false)]
		[Parameter("local0", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_RFReceiver_efca5f5df23d4435809fc67175a2287d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 28)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)784684429), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

	public class Radio_Detonator
	{
		[Patch("OnRfFrequencyChange", "OnRfFrequencyChange [Detonator]", "Detonator", "ServerSetFrequency", new string[] { "BasePlayer", "System.Int32" })]
		[Identifier("feaf8a606dfc46c482667d780fc542a7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Detonator", false)]
		[Parameter("freq", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_Detonator_feaf8a606dfc46c482667d780fc542a7 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 27)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-415055395)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("OnRfFrequencyChanged", "OnRfFrequencyChanged [Detonator]", "Detonator", "ServerSetFrequency", new string[] { "BasePlayer", "System.Int32" })]
		[Identifier("c3e415d1232a40508c336e8230f9c9a6")]
		[Dependencies(new string[] { "OnRfFrequencyChange [Detonator]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Detonator", false)]
		[Parameter("freq", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_Detonator_c3e415d1232a40508c336e8230f9c9a6 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 83)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)784684429), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

	public class Radio_PagerEntity
	{
		[Patch("OnRfFrequencyChange", "OnRfFrequencyChange [PagerEntity]", "PagerEntity", "ServerSetFrequency", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("dffdc6c6fba94ec584692c79b27694b7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PagerEntity", false)]
		[Parameter("local0", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_PagerEntity_dffdc6c6fba94ec584692c79b27694b7 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-415055395)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnRfFrequencyChanged", "OnRfFrequencyChanged [PagerEntity]", "PagerEntity", "ServerSetFrequency", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("023b2bea716548fbb97966edf3c45707")]
		[Dependencies(new string[] { "OnRfFrequencyChange [PagerEntity]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PagerEntity", false)]
		[Parameter("local0", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_PagerEntity_023b2bea716548fbb97966edf3c45707 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 46)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)784684429), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

	public class Radio_BoomBox
	{
		[Patch("OnBoomboxStationValidate", "OnBoomboxStationValidate", "BoomBox", "IsStationValid", new string[] { "System.String" })]
		[Identifier("f00bc321322c4ffeb33198dedf140d01")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("url", "System.String", false)]
		[Return(typeof(bool))]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_BoomBox_f00bc321322c4ffeb33198dedf140d01 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 1)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)725502942), instruction), instruction);
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

		[Patch("OnBoomboxToggle", "OnBoomboxToggle", "BoomBox", "ServerTogglePlay", new string[] { "BaseEntity/RPCMessage", "System.Boolean" })]
		[Identifier("7b55d141efcf40f69a5b180596d07408")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BoomBox", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local1", "System.Boolean", false)]
		[Return(typeof(void))]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_BoomBox_7b55d141efcf40f69a5b180596d07408 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1268541485)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Boolean"));
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

		[Patch("OnBoomboxStationUpdate", "OnBoomboxStationUpdate", "BoomBox", "Server_UpdateRadioIP", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("05c5cde287b3464db65fa2c029589b98")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BoomBox", false)]
		[Parameter("local0", "System.String", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_BoomBox_05c5cde287b3464db65fa2c029589b98 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 6)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1454692131)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnBoomboxStationUpdated", "OnBoomboxStationUpdated", "BoomBox", "Server_UpdateRadioIP", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("fe8fa74a5777491fb8f213cb69878310")]
		[Dependencies(new string[] { "OnBoomboxStationUpdate" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BoomBox", false)]
		[Parameter("local0", "System.String", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Radio")]
		[Assembly("Assembly-CSharp.dll")]
		public class Radio_BoomBox_fe8fa74a5777491fb8f213cb69878310 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 41)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1238374081), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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
}
