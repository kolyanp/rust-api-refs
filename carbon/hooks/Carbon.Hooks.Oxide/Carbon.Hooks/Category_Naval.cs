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
		[Identifier("c21592498599462581d0754e9efcec5e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SmallEngine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_SmallEngine_c21592498599462581d0754e9efcec5e : Patch
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
		[Identifier("6c03400a01ef4758bf279440e5fdb6f0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SmallEngine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_SmallEngine_6c03400a01ef4758bf279440e5fdb6f0 : Patch
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
		[Identifier("2ec041b598924c14aed6531d34cd2366")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SmallEngine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_SmallEngine_2ec041b598924c14aed6531d34cd2366 : Patch
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
		[Identifier("5aef2352f3fc427ea863f559506eab3a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Sail", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Sail_5aef2352f3fc427ea863f559506eab3a : Patch
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
		[Identifier("5ee1c8657755401ca7ac23b69124860a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Sail", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Sail_5ee1c8657755401ca7ac23b69124860a : Patch
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
		[Identifier("7963eadc91074150b9b1bb57e37561f7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Sail", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Sail_7963eadc91074150b9b1bb57e37561f7 : Patch
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
		[Identifier("e84f1eeb395a490ebbef650d590b17b8")]
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
		public class Naval_BoatGroupSpawner_e84f1eeb395a490ebbef650d590b17b8 : Patch
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
		[Identifier("ceccb7e6577d456eaad49fb415f81898")]
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
		public class Naval_BoatGroupSpawner_ceccb7e6577d456eaad49fb415f81898 : Patch
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
		[Identifier("98bd3b894834488699682aceda3340ce")]
		[Dependencies(new string[] { "OnBoatGroupSpawned" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_BoatGroupSpawner_98bd3b894834488699682aceda3340ce : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[108];
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
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Naval_TriggerDeepSeaPortal
	{
		[Patch("OnDeepSeaTeleport", "OnDeepSeaTeleport", "TriggerDeepSeaPortal", "OnEntityEnter", new string[] { "BaseEntity" })]
		[Identifier("27aebe8a162840349729cf6b0e21d8e2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TriggerDeepSeaPortal", false)]
		[Parameter("ent", "BaseEntity", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_TriggerDeepSeaPortal_27aebe8a162840349729cf6b0e21d8e2 : Patch
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
		[Identifier("7a25602ac25943bf85d8e28be38dddac")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("entity", "BaseEntity", false)]
		[Parameter("portal", "DeepSeaPortal", false)]
		[Return(typeof(ValueTuple<bool, Phrase>))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_TriggerDeepSeaPortal_7a25602ac25943bf85d8e28be38dddac : Patch
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
		[Identifier("b32eb73b42034d178fcddfefc206e91c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Cannon", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Cannon_b32eb73b42034d178fcddfefc206e91c : Patch
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
		[Identifier("9761027c8a114d249daced341af19ab2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerBoat", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Parameter("collision", "UnityEngine.Collision", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_PlayerBoat_9761027c8a114d249daced341af19ab2 : Patch
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
		[Identifier("b8d6950e45134dc98b790a8050225ac6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerBoat", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_PlayerBoat_b8d6950e45134dc98b790a8050225ac6 : Patch
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

	public class Naval_DeepSeaManagerCloseDeepSeaAsyncd78
	{
		[Patch("OnDeepSeaClosed", "OnDeepSeaClosed", "DeepSeaManager/<CloseDeepSeaAsync>d__78", "MoveNext", new string[] { })]
		[Identifier("c412cf1d8a6f4a1c8549b47b72d43d65")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerCloseDeepSeaAsyncd78_c412cf1d8a6f4a1c8549b47b72d43d65 : Patch
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

		[Patch("OnDeepSeaClose", "OnDeepSeaClose", "DeepSeaManager/<CloseDeepSeaAsync>d__78", "MoveNext", new string[] { })]
		[Identifier("70e9961dd7d3485cae59938064639dba")]
		[Dependencies(new string[] { "OnDeepSeaClosed" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerCloseDeepSeaAsyncd78_70e9961dd7d3485cae59938064639dba : Patch
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

	public class Naval_DeepSeaManagerOpenDeepSeaAsyncd76
	{
		[Patch("OnDeepSeaOpened", "OnDeepSeaOpened", "DeepSeaManager/<OpenDeepSeaAsync>d__76", "MoveNext", new string[] { })]
		[Identifier("1a4f7cd07b7f43c6a2fa71fec01ac17a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerOpenDeepSeaAsyncd76_1a4f7cd07b7f43c6a2fa71fec01ac17a : Patch
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

		[Patch("OnDeepSeaOpen", "OnDeepSeaOpen", "DeepSeaManager/<OpenDeepSeaAsync>d__76", "MoveNext", new string[] { })]
		[Identifier("57cc9076050a47f0a35c0f77fe38626b")]
		[Dependencies(new string[] { "OnDeepSeaOpened" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerOpenDeepSeaAsyncd76_57cc9076050a47f0a35c0f77fe38626b : Patch
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
		[Identifier("481ac81555694eb3814c9832c0c8f4e3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "PlayerBoat", false)]
		[Parameter("self", "BoatBuildingStation", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_BoatBuildingStation_481ac81555694eb3814c9832c0c8f4e3 : Patch
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
