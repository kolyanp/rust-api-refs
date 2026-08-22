using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Bookmark
{
	public class Bookmark_ComputerStation
	{
		[Patch("OnBookmarkControl", "OnBookmarkControl", "ComputerStation", "BeginControllingBookmark", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("d9ff60b6741b4a129726ab41ab4af9cc")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Parameter("local2", "IRemoteControllable", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_d9ff60b6741b4a129726ab41ab4af9cc : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 61)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1730078324)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
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

		[Patch("OnBookmarkAdd", "OnBookmarkAdd", "ComputerStation", "AddBookmark", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("41f1f0f78087425c9f1cea2dd6356cd8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_41f1f0f78087425c9f1cea2dd6356cd8 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 40)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1206891691), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

		[Patch("OnBookmarksSendControl", "OnBookmarksSendControl", "ComputerStation", "SendControlBookmarks", new string[] { "BasePlayer" })]
		[Identifier("4223d6fb3ab44f39b1b343b6059a5bb6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "System.String", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_4223d6fb3ab44f39b1b343b6059a5bb6 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 8)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2021417980), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("OnBookmarkControlEnd", "OnBookmarkControlEnd", "ComputerStation", "StopControl", new string[] { "BasePlayer" })]
		[Identifier("098d1b592faf4cae8f39d1ae832a2a55")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("ply", "BasePlayer", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_098d1b592faf4cae8f39d1ae832a2a55 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 8)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-684601644)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("OnBookmarkInput", "OnBookmarkInput", "ComputerStation", "PlayerServerInput", new string[] { "InputState", "BasePlayer" })]
		[Identifier("107236b24252480d8fc9b9e84e556501")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("inputState", "InputState", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_107236b24252480d8fc9b9e84e556501 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1063588778)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
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

		[Patch("OnBookmarkControlStarted", "OnBookmarkControlStarted", "ComputerStation", "BeginControllingBookmark", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("2b2efd1cf3084a0992f501cf1ba01b4c")]
		[Dependencies(new string[] { "OnBookmarkControl" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Parameter("local2", "IRemoteControllable", false)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_2b2efd1cf3084a0992f501cf1ba01b4c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 146)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1976560681)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnBookmarkControlEnded", "OnBookmarkControlEnded", "ComputerStation", "StopControl", new string[] { "BasePlayer" })]
		[Identifier("a3a475728dc649948ac4c53c9f19e955")]
		[Dependencies(new string[] { "OnBookmarkControlEnd" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("ply", "BasePlayer", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_a3a475728dc649948ac4c53c9f19e955 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 62)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)764919103), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("OnBookmarkControlEnded", "OnBookmarkControlEnded [2]", "ComputerStation", "BeginControllingBookmark", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("2efd620b863643e491692f851d397b5a")]
		[Dependencies(new string[] { "OnBookmarkControlStarted" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local6", "IRemoteControllable", false)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_2efd620b863643e491692f851d397b5a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 90)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)764919103), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)6);
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

		[Patch("OnBookmarkDelete", "OnBookmarkDelete", "ComputerStation", "RemoveBookmark", new string[] { "System.String", "BasePlayer" })]
		[Identifier("75c2183effa54737a8f8c68ed770f803")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("mountedPlayer", "BasePlayer", false)]
		[Parameter("identifier", "System.String", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_75c2183effa54737a8f8c68ed770f803 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)373279383), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
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

		[Patch("OnBookmarkControlEnded [2] [patch]", "OnBookmarkControlEnded [2] [patch]", "ComputerStation", "BeginControllingBookmark", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("0d3fbf6aa4ca4fe8a82079cfb52897a5")]
		[Dependencies(new string[] { "OnBookmarkControlEnded [2]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_0d3fbf6aa4ca4fe8a82079cfb52897a5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[96];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[77]), list2[77]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[78], list2[77]), list2[77]);
				}
				list2.RemoveRange(77, 1);
				list2.InsertRange(77, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
