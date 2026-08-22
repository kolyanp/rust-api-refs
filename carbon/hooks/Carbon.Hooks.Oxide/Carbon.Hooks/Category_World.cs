using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_World
{
	public class World_TerrainMeta
	{
		[Patch("OnTerrainInitialized", "OnTerrainInitialized", "TerrainMeta", "PostSetupComponents", new string[] { })]
		[Identifier("894c5cfbe769430489a62aa305f22d8d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("World")]
		[Assembly("Assembly-CSharp.dll")]
		public class World_TerrainMeta_894c5cfbe769430489a62aa305f22d8d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-983586360)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[1] { typeof(uint) }, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class World_World
	{
		[Patch("OnWorldPrefabSpawned", "OnWorldPrefabSpawned", "World", "SpawnPrefab", new string[] { "System.String", "Prefab", "UnityEngine.Vector3", "UnityEngine.Quaternion", "UnityEngine.Vector3" })]
		[Identifier("eb114696dd884a178bcd0099a1a9e1f3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "UnityEngine.GameObject", false)]
		[Parameter("category", "System.String", false)]
		[Category("World")]
		[Assembly("Assembly-CSharp.dll")]
		public class World_World_eb114696dd884a178bcd0099a1a9e1f3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 31)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-256753986)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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
	}

	public class World_TerrainGenerator
	{
		[Patch("OnTerrainCreate", "OnTerrainCreate", "TerrainGenerator", "CreateTerrain", new string[] { "System.Int32", "System.Int32" })]
		[Identifier("315553876c35410688f67e6fe4eabadb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TerrainGenerator", false)]
		[Category("World")]
		[Assembly("Assembly-CSharp.dll")]
		public class World_TerrainGenerator_315553876c35410688f67e6fe4eabadb : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1463841364)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
						typeof(object)
					}, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}
	}
}
