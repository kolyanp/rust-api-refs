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
		[Identifier("d14e6b4561514c79a067a993ff90b92b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Parameter("local2", "IRemoteControllable", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_d14e6b4561514c79a067a993ff90b92b : Patch
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
		[Identifier("7e1c6012307f4c009137b4abd9bf8f32")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_7e1c6012307f4c009137b4abd9bf8f32 : Patch
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
		[Identifier("9408ac480b0b407785f3aec61cb1b00a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "System.String", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_9408ac480b0b407785f3aec61cb1b00a : Patch
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
		[Identifier("a7a020bc30114775a168cb394f4a17d6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("ply", "BasePlayer", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_a7a020bc30114775a168cb394f4a17d6 : Patch
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
		[Identifier("75638f294811430191d5a80fffb6a2ff")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("inputState", "InputState", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_75638f294811430191d5a80fffb6a2ff : Patch
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
		[Identifier("a505bd938e6045159988624ba0ba4f3c")]
		[Dependencies(new string[] { "OnBookmarkControl" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Parameter("local2", "IRemoteControllable", false)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_a505bd938e6045159988624ba0ba4f3c : Patch
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
		[Identifier("53fcd6b21d104b2796fd9517dca48e1f")]
		[Dependencies(new string[] { "OnBookmarkControlEnd" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("ply", "BasePlayer", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_53fcd6b21d104b2796fd9517dca48e1f : Patch
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
		[Identifier("1a9397f5244640319220cf0a302960fc")]
		[Dependencies(new string[] { "OnBookmarkControlStarted" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local6", "IRemoteControllable", false)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_1a9397f5244640319220cf0a302960fc : Patch
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
		[Identifier("e12cfa70ec384c5cae8479ca9d954b13")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("mountedPlayer", "BasePlayer", false)]
		[Parameter("identifier", "System.String", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_e12cfa70ec384c5cae8479ca9d954b13 : Patch
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
		[Identifier("eb2ee31b628e49398acfb7af31bb4f37")]
		[Dependencies(new string[] { "OnBookmarkControlEnded [2]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_eb2ee31b628e49398acfb7af31bb4f37 : Patch
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
