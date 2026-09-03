using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using HarmonyLib;
using UnityEngine;

namespace Carbon.Hooks;

public class Category_CommunityUI
{
	public class CommunityUI_CommunityEntity
	{
		[Patch("OnCuiDraggableDrag", "OnCuiDraggableDrag", "CommunityEntity", "Hook_DragRPC", new string[] { "BasePlayer", "System.String", "UnityEngine.Vector3", "CommunityEntity/DraggablePositionSendType" })]
		[Identifier("f2213e3258a84621a5e8623a6c7e6339")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("CommunityUI")]
		[Assembly("Assembly-CSharp.dll")]
		public class CommunityUI_CommunityEntity_f2213e3258a84621a5e8623a6c7e6339 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1614693435), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(Vector3));
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)4);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(DraggablePositionSendType));
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

		[Patch("OnCuiDraggableDrop", "OnCuiDraggableDrop", "CommunityEntity", "Hook_DropRPC", new string[] { "BasePlayer", "System.String", "System.String", "System.String", "System.String" })]
		[Identifier("c353df7c01e9489fb33b63cb3550a0b8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("CommunityUI")]
		[Assembly("Assembly-CSharp.dll")]
		public class CommunityUI_CommunityEntity_c353df7c01e9489fb33b63cb3550a0b8 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1652197354)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)4);
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)5);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[6]
					{
						typeof(uint),
						typeof(object),
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
	}
}
