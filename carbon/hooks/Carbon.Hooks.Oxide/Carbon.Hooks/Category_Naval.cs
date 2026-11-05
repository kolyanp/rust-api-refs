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
		[Identifier("8b2b41666a8444aaa0c60a830a9aea9a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SmallEngine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_SmallEngine_8b2b41666a8444aaa0c60a830a9aea9a : Patch
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
		[Identifier("d240877e8adb44d3a576522f4191873a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SmallEngine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_SmallEngine_d240877e8adb44d3a576522f4191873a : Patch
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
		[Identifier("352ebf97680a407780a535ad6543e1de")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SmallEngine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_SmallEngine_352ebf97680a407780a535ad6543e1de : Patch
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
		[Identifier("d04c20d977534ca89533a42823df1607")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Sail", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Sail_d04c20d977534ca89533a42823df1607 : Patch
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
		[Identifier("d554ff3befbe41cfbdd0c78104af4cb0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Sail", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Sail_d554ff3befbe41cfbdd0c78104af4cb0 : Patch
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
		[Identifier("45a2bc6e168847078ae66a5bffc107e0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Sail", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Sail_45a2bc6e168847078ae66a5bffc107e0 : Patch
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
		[Patch("OnBoatGroupSpawn", "OnBoatGroupSpawn", "BoatGroupSpawner", "SpawnBoatGroup", new string[] { "System.Collections.Generic.HashSet`1<RHIB>", "BoatAI/AILoadMode", "System.Boolean" })]
		[Identifier("02df075c868945b2a4a4eadaaaa96f6f")]
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
		public class Naval_BoatGroupSpawner_02df075c868945b2a4a4eadaaaa96f6f : Patch
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

		[Patch("OnBoatGroupSpawned", "OnBoatGroupSpawned", "BoatGroupSpawner", "SpawnBoatGroup", new string[] { "System.Collections.Generic.HashSet`1<RHIB>", "BoatAI/AILoadMode", "System.Boolean" })]
		[Identifier("c9a767a17c1947479348793b4d3a39ef")]
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
		public class Naval_BoatGroupSpawner_c9a767a17c1947479348793b4d3a39ef : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 92)
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

		[Patch("OnBoatGroupSpawned", "OnBoatGroupSpawned [Patch]", "BoatGroupSpawner", "SpawnBoatGroup", new string[] { "System.Collections.Generic.HashSet`1<RHIB>", "BoatAI/AILoadMode", "System.Boolean" })]
		[Identifier("424ec5b4a1d440409ade0abe1e66da3d")]
		[Dependencies(new string[] { "OnBoatGroupSpawned" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_BoatGroupSpawner_424ec5b4a1d440409ade0abe1e66da3d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[105];
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
		[Identifier("b602809136ff4d2ebc437c7c4f34b70f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TriggerDeepSeaPortal", false)]
		[Parameter("ent", "BaseEntity", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_TriggerDeepSeaPortal_b602809136ff4d2ebc437c7c4f34b70f : Patch
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
		[Identifier("5fc46e998e5e4323bf0209d48f06e8c1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("entity", "BaseEntity", false)]
		[Parameter("portal", "DeepSeaPortal", false)]
		[Return(typeof(ValueTuple<bool, Phrase>))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_TriggerDeepSeaPortal_5fc46e998e5e4323bf0209d48f06e8c1 : Patch
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
		[Identifier("dccdb4b4b6d6491781b99b230c70cca6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Cannon", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Cannon_dccdb4b4b6d6491781b99b230c70cca6 : Patch
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
		[Identifier("a33925462b794f0eb0aa8471c817e415")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerBoat", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Parameter("collision", "UnityEngine.Collision", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_PlayerBoat_a33925462b794f0eb0aa8471c817e415 : Patch
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
		[Identifier("edfbfe910f6c4b5f8ce3d56e0c4eebf8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerBoat", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_PlayerBoat_edfbfe910f6c4b5f8ce3d56e0c4eebf8 : Patch
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

	public class Naval_DeepSeaManagerCloseDeepSeaAsyncd80
	{
		[Patch("OnDeepSeaClosed", "OnDeepSeaClosed", "DeepSeaManager/<CloseDeepSeaAsync>d__80", "MoveNext", new string[] { })]
		[Identifier("4ac15e0d34344f63b1c2ad247c990e46")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerCloseDeepSeaAsyncd80_4ac15e0d34344f63b1c2ad247c990e46 : Patch
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

		[Patch("OnDeepSeaClose", "OnDeepSeaClose", "DeepSeaManager/<CloseDeepSeaAsync>d__80", "MoveNext", new string[] { })]
		[Identifier("c00220008d494a8e885c6462694fe356")]
		[Dependencies(new string[] { "OnDeepSeaClosed" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerCloseDeepSeaAsyncd80_c00220008d494a8e885c6462694fe356 : Patch
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

	public class Naval_DeepSeaManagerOpenDeepSeaAsyncd78
	{
		[Patch("OnDeepSeaOpened", "OnDeepSeaOpened", "DeepSeaManager/<OpenDeepSeaAsync>d__78", "MoveNext", new string[] { })]
		[Identifier("b609c5d6bc6d4e089c317949abd9ae6f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerOpenDeepSeaAsyncd78_b609c5d6bc6d4e089c317949abd9ae6f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 105)
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

		[Patch("OnDeepSeaOpen", "OnDeepSeaOpen", "DeepSeaManager/<OpenDeepSeaAsync>d__78", "MoveNext", new string[] { })]
		[Identifier("93aa313a56fb4ff2b0560fe58eedcd11")]
		[Dependencies(new string[] { "OnDeepSeaOpened" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerOpenDeepSeaAsyncd78_93aa313a56fb4ff2b0560fe58eedcd11 : Patch
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
		[Identifier("021a3ab3ad1f4db5894d47ac3ba994ff")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "PlayerBoat", false)]
		[Parameter("self", "BoatBuildingStation", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_BoatBuildingStation_021a3ab3ad1f4db5894d47ac3ba994ff : Patch
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
