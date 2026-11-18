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
		[Identifier("cac3c2d630c64af7bc016261a6c46b0f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Parameter("local2", "IRemoteControllable", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_cac3c2d630c64af7bc016261a6c46b0f : Patch
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
		[Identifier("22a7445b1c2d433ab60c1cbb1464bb71")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_22a7445b1c2d433ab60c1cbb1464bb71 : Patch
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
		[Identifier("35e5fe53a01b46c69b5fd3951b980e62")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "System.String", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_35e5fe53a01b46c69b5fd3951b980e62 : Patch
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
		[Identifier("4cce509763c34113a33e7333237e6170")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("ply", "BasePlayer", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_4cce509763c34113a33e7333237e6170 : Patch
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
		[Identifier("df483cd4bec8477280b981ad1d193559")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("inputState", "InputState", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_df483cd4bec8477280b981ad1d193559 : Patch
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
		[Identifier("bb376310baf4468091f70fbe4f9b3313")]
		[Dependencies(new string[] { "OnBookmarkControl" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Parameter("local2", "IRemoteControllable", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_bb376310baf4468091f70fbe4f9b3313 : Patch
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
		[Identifier("b6172b682ddd426886d893c494558974")]
		[Dependencies(new string[] { "OnBookmarkControlEnd" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("ply", "BasePlayer", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_b6172b682ddd426886d893c494558974 : Patch
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
		[Identifier("dc3862e5adca4682a6b4201aa574660a")]
		[Dependencies(new string[] { "OnBookmarkControlStarted" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local6", "IRemoteControllable", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_dc3862e5adca4682a6b4201aa574660a : Patch
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
		[Identifier("7654fcbd95dd4d69aeb29eea3a72786d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("mountedPlayer", "BasePlayer", false)]
		[Parameter("identifier", "System.String", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_7654fcbd95dd4d69aeb29eea3a72786d : Patch
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
		[Identifier("8319564d3d744814ace91eed80d8ce82")]
		[Dependencies(new string[] { "OnBookmarkControlEnded [2]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_8319564d3d744814ace91eed80d8ce82 : Patch
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
