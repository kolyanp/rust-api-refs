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
		[Identifier("4448c64cd2c44511bf9e988b1fba1738")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Parameter("local2", "IRemoteControllable", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_4448c64cd2c44511bf9e988b1fba1738 : Patch
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
		[Identifier("ce0cc017ad764a7684e49db246f949c5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_ce0cc017ad764a7684e49db246f949c5 : Patch
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
		[Identifier("7fec43065d564fc486886804abbece97")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "System.String", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_7fec43065d564fc486886804abbece97 : Patch
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
		[Identifier("14ef6fbfb1b84ea1834a0021ab5fc935")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("ply", "BasePlayer", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_14ef6fbfb1b84ea1834a0021ab5fc935 : Patch
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
		[Identifier("1568c87f895041379b7ec156f1f647a8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("inputState", "InputState", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_1568c87f895041379b7ec156f1f647a8 : Patch
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
		[Identifier("c8f73c61471c440c8fba60fdb41db138")]
		[Dependencies(new string[] { "OnBookmarkControl" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Parameter("local2", "IRemoteControllable", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_c8f73c61471c440c8fba60fdb41db138 : Patch
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
		[Identifier("2be22a5217be4d86821e00bec54aadc0")]
		[Dependencies(new string[] { "OnBookmarkControlEnd" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("ply", "BasePlayer", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_2be22a5217be4d86821e00bec54aadc0 : Patch
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
		[Identifier("1d7bb5d9b60c4a70a0273592d7e868f4")]
		[Dependencies(new string[] { "OnBookmarkControlStarted" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local6", "IRemoteControllable", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_1d7bb5d9b60c4a70a0273592d7e868f4 : Patch
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
		[Identifier("cf442c37250144148482758e277f24e0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("mountedPlayer", "BasePlayer", false)]
		[Parameter("identifier", "System.String", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_cf442c37250144148482758e277f24e0 : Patch
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
		[Identifier("6365b3043b6940ada107c8402b36c1a9")]
		[Dependencies(new string[] { "OnBookmarkControlEnded [2]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_6365b3043b6940ada107c8402b36c1a9 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[96];
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
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
