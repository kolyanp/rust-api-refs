using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Naval
{
	public class Naval_SmallEngine
	{
		[Patch("OnEngineReverse", "OnEngineReverse", "SmallEngine", "SV_ToggleReverse", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("2a52f654f2a8417ea5ce695d2de78532")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SmallEngine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_SmallEngine_2a52f654f2a8417ea5ce695d2de78532 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1496309690)), instruction);
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

		[Patch("OnEngineStart", "OnEngineStart [SmallEngine]", "SmallEngine", "TurnOn", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("a39226aec73c4956b98da4d5a00bef7c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SmallEngine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_SmallEngine_a39226aec73c4956b98da4d5a00bef7c : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1113127637), instruction), instruction);
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

		[Patch("OnEngineStop", "OnEngineStop [SmallEngine]", "SmallEngine", "TurnOff", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("91e506beea73459eaf4cd5ba92c3f40f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SmallEngine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_SmallEngine_91e506beea73459eaf4cd5ba92c3f40f : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1240927891)), instruction), instruction);
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

	public class Naval_Sail
	{
		[Patch("CanRotateSail", "CanRotateSail", "Sail", "CanRotate", new string[] { "BasePlayer" })]
		[Identifier("79b6ea2d690d4ffba03b350843417ce6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Sail", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Sail_79b6ea2d690d4ffba03b350843417ce6 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-474993931)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("CanRaiseSail", "CanRaiseSail", "Sail", "CanBeRaised", new string[] { "BasePlayer" })]
		[Identifier("346cf052996e4903af2c86c951cb8b31")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Sail", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Sail_346cf052996e4903af2c86c951cb8b31 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)439109945), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("CanLowerSail", "CanLowerSail", "Sail", "CanBeLowered", new string[] { "BasePlayer" })]
		[Identifier("c8ab34b5e3b94d998927440cfb26ff5f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Sail", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Sail_c8ab34b5e3b94d998927440cfb26ff5f : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1493169743), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

	public class Naval_BoatGroupSpawner
	{
		[Patch("OnBoatGroupSpawn", "OnBoatGroupSpawn", "BoatGroupSpawner", "SpawnBoatGroup", new string[] { "System.Collections.Generic.HashSet`1<RHIB>", "BoatAI/AILoadMode", "System.Boolean", "ScientistBoatOilrigManager" })]
		[Identifier("80b0f36cc02246d7a8a3b7aaf24ea1e0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BoatGroupSpawner", false)]
		[Parameter("local1", "UnityEngine.Vector2", false)]
		[Parameter("local4", "UnityEngine.Quaternion", false)]
		[Parameter("list", "System.Collections.Generic.HashSet`1[RHIB]", false)]
		[Parameter("local3", "System.Boolean", false)]
		[Parameter("spawnsPT", "System.Boolean", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_BoatGroupSpawner_80b0f36cc02246d7a8a3b7aaf24ea1e0 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)909607648), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("UnityEngine.Vector2"));
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)4);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("UnityEngine.Quaternion"));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Boolean"));
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[7]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object),
						typeof(object),
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

		[Patch("OnBoatGroupSpawned", "OnBoatGroupSpawned", "BoatGroupSpawner", "SpawnBoatGroup", new string[] { "System.Collections.Generic.HashSet`1<RHIB>", "BoatAI/AILoadMode", "System.Boolean", "ScientistBoatOilrigManager" })]
		[Identifier("16c0e69a9d674975a037e1db75f786f2")]
		[Dependencies(new string[] { "OnBoatGroupSpawn" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BoatGroupSpawner", false)]
		[Parameter("local1", "UnityEngine.Vector2", false)]
		[Parameter("local4", "UnityEngine.Quaternion", false)]
		[Parameter("list", "System.Collections.Generic.HashSet`1[RHIB]", false)]
		[Parameter("local3", "System.Boolean", false)]
		[Parameter("spawnsPT", "System.Boolean", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_BoatGroupSpawner_16c0e69a9d674975a037e1db75f786f2 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 95)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1870904906)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("UnityEngine.Vector2"));
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)4);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("UnityEngine.Quaternion"));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Boolean"));
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[7]
					{
						typeof(uint),
						typeof(object),
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

		[Patch("OnBoatGroupSpawned", "OnBoatGroupSpawned [Patch]", "BoatGroupSpawner", "SpawnBoatGroup", new string[] { "System.Collections.Generic.HashSet`1<RHIB>", "BoatAI/AILoadMode", "System.Boolean", "ScientistBoatOilrigManager" })]
		[Identifier("378ae60282ba49d5aba1fd9f6dd95f70")]
		[Dependencies(new string[] { "OnBoatGroupSpawned" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_BoatGroupSpawner_378ae60282ba49d5aba1fd9f6dd95f70 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[108];
				list.Add(new CodeInstruction(OpCodes.Brfalse, (object)label));
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

	public class Naval_TriggerDeepSeaPortal
	{
		[Patch("OnDeepSeaTeleport", "OnDeepSeaTeleport", "TriggerDeepSeaPortal", "OnEntityEnter", new string[] { "BaseEntity" })]
		[Identifier("c827e33df7ee48c996ecc8fd1c1b43fe")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TriggerDeepSeaPortal", false)]
		[Parameter("ent", "BaseEntity", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_TriggerDeepSeaPortal_c827e33df7ee48c996ecc8fd1c1b43fe : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 121)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1355776107), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("TriggerDeepSeaPortal+<>c__DisplayClass3_0"), "ent"));
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

		[Patch("CanTeleportDeepSea", "CanTeleportDeepSea", "TriggerDeepSeaPortal", "CanEntityTeleport", new string[] { "BaseEntity" })]
		[Identifier("ad533f96de194ec6a63f33bd6943ac06")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("entity", "BaseEntity", false)]
		[Parameter("self", "TriggerDeepSeaPortal", false)]
		[Return(typeof(ValueTuple<bool, Phrase>))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_TriggerDeepSeaPortal_ad533f96de194ec6a63f33bd6943ac06 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-6289695)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("TriggerDeepSeaPortal"), "Portal"));
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
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof((bool, Phrase)));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof((bool, Phrase)));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Naval_Cannon
	{
		[Patch("CanLightCannonFuse", "CanLightCannonFuse", "Cannon", "CanLightFuse", new string[] { })]
		[Identifier("c9c37e68e9874f84ac7aff4b88a0deb0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Cannon", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Cannon_c9c37e68e9874f84ac7aff4b88a0deb0 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1935391534), instruction), instruction);
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
	}

	public class Naval_PlayerBoat
	{
		[Patch("OnPlayerBoatCollide", "OnPlayerBoatCollide", "PlayerBoat", "ProcessCollision", new string[] { "UnityEngine.Collision" })]
		[Identifier("c12c609b9c9443afb614d0d5640fc7a6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerBoat", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Parameter("collision", "UnityEngine.Collision", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_PlayerBoat_c12c609b9c9443afb614d0d5640fc7a6 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 21)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1845938367)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("CanEditPlayerBoat", "CanEditPlayerBoat", "PlayerBoat", "CanStartEditing", new string[] { "BasePlayer", "System.Boolean" })]
		[Identifier("ea1a1176981444b79bdd4079c530ca2c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerBoat", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_PlayerBoat_ea1a1176981444b79bdd4079c530ca2c : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1618730577), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

	public class Naval_DeepSeaManagerCloseDeepSeaAsyncd75
	{
		[Patch("OnDeepSeaClosed", "OnDeepSeaClosed", "DeepSeaManager/<CloseDeepSeaAsync>d__75", "MoveNext", new string[] { })]
		[Identifier("8c9d9080a39d4c9e93efb70b19f22fd8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerCloseDeepSeaAsyncd75_8c9d9080a39d4c9e93efb70b19f22fd8 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 154)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)565264246), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

		[Patch("OnDeepSeaClose", "OnDeepSeaClose", "DeepSeaManager/<CloseDeepSeaAsync>d__75", "MoveNext", new string[] { })]
		[Identifier("ad93b8227dce471096abc946c5aaa119")]
		[Dependencies(new string[] { "OnDeepSeaClosed" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerCloseDeepSeaAsyncd75_ad93b8227dce471096abc946c5aaa119 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 16)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1501492042), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

	public class Naval_DeepSeaManagerOpenDeepSeaAsyncd73
	{
		[Patch("OnDeepSeaOpened", "OnDeepSeaOpened", "DeepSeaManager/<OpenDeepSeaAsync>d__73", "MoveNext", new string[] { })]
		[Identifier("e085a1a641424499a3c4c0da462c0251")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerOpenDeepSeaAsyncd73_e085a1a641424499a3c4c0da462c0251 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 104)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)631225454), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

		[Patch("OnDeepSeaOpen", "OnDeepSeaOpen", "DeepSeaManager/<OpenDeepSeaAsync>d__73", "MoveNext", new string[] { })]
		[Identifier("e39d8707457a4dadbc8183d0aae95001")]
		[Dependencies(new string[] { "OnDeepSeaOpened" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerOpenDeepSeaAsyncd73_e39d8707457a4dadbc8183d0aae95001 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 13)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-218920844)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

	public class Naval_BoatBuildingStation
	{
		[Patch("OnPlayerBoatEditStarted", "OnPlayerBoatEditStarted", "BoatBuildingStation", "ConvertPlayerBoatToConstruction", new string[] { })]
		[Identifier("9e0ea3365749441a9c948ffbdf7e829f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "PlayerBoat", false)]
		[Parameter("self", "BoatBuildingStation", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_BoatBuildingStation_9e0ea3365749441a9c948ffbdf7e829f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 70)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2064514509)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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
}
