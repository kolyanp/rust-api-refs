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
		[Identifier("039a996823ad47069bad7000fc9f3e52")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Parameter("local2", "IRemoteControllable", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_039a996823ad47069bad7000fc9f3e52 : Patch
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
		[Identifier("1ae5cfee8b81400fbffa6e5aa9fdd409")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_1ae5cfee8b81400fbffa6e5aa9fdd409 : Patch
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
		[Identifier("58fed8a74b8444e8b909593c0827ac87")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "System.String", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_58fed8a74b8444e8b909593c0827ac87 : Patch
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
		[Identifier("e9ed9eb2e8b84f12ab7990614df2ce97")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("ply", "BasePlayer", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_e9ed9eb2e8b84f12ab7990614df2ce97 : Patch
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
		[Identifier("ad1748b29fad434185e2808b34566ee8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("inputState", "InputState", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_ad1748b29fad434185e2808b34566ee8 : Patch
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
		[Identifier("1353cf7fbaf4403c8c1d63ef5163d5c5")]
		[Dependencies(new string[] { "OnBookmarkControl" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Parameter("local2", "IRemoteControllable", false)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_1353cf7fbaf4403c8c1d63ef5163d5c5 : Patch
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
		[Identifier("1b32fbaa6d6d471f90e05e1251af1f15")]
		[Dependencies(new string[] { "OnBookmarkControlEnd" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("ply", "BasePlayer", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_1b32fbaa6d6d471f90e05e1251af1f15 : Patch
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
		[Identifier("5dcf86322b584a1e939f3a6c8c7e6085")]
		[Dependencies(new string[] { "OnBookmarkControlStarted" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local6", "IRemoteControllable", false)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_5dcf86322b584a1e939f3a6c8c7e6085 : Patch
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
		[Identifier("bcd3cc723c944e1988f035c8c8091b4f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ComputerStation", false)]
		[Parameter("mountedPlayer", "BasePlayer", false)]
		[Parameter("identifier", "System.String", false)]
		[Return(typeof(void))]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_bcd3cc723c944e1988f035c8c8091b4f : Patch
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
		[Identifier("2ee4e0faaac34e178bf7bf3efc972459")]
		[Dependencies(new string[] { "OnBookmarkControlEnded [2]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Bookmark")]
		[Assembly("Assembly-CSharp.dll")]
		public class Bookmark_ComputerStation_2ee4e0faaac34e178bf7bf3efc972459 : Patch
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
