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
		[Identifier("03aa548d77b84dba80ce7e9da6bb7131")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SmallEngine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_SmallEngine_03aa548d77b84dba80ce7e9da6bb7131 : Patch
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
		[Identifier("99561f99a1d54508a0e5ed877c5514ec")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SmallEngine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_SmallEngine_99561f99a1d54508a0e5ed877c5514ec : Patch
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
		[Identifier("5c8ba8f15ecf4f959f13c321d7038ab9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SmallEngine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_SmallEngine_5c8ba8f15ecf4f959f13c321d7038ab9 : Patch
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
		[Identifier("cc358a3ec16d4971be22f40c724a5459")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Sail", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Sail_cc358a3ec16d4971be22f40c724a5459 : Patch
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
		[Identifier("70150916acc64c1bb2d72fdcef3e6384")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Sail", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Sail_70150916acc64c1bb2d72fdcef3e6384 : Patch
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
		[Identifier("5d3cfd05ed934976906bfb2484a17411")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Sail", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Sail_5d3cfd05ed934976906bfb2484a17411 : Patch
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
		[Identifier("37512fd3f5114b0c9703c39bb475e853")]
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
		public class Naval_BoatGroupSpawner_37512fd3f5114b0c9703c39bb475e853 : Patch
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
		[Identifier("5ba08da4d3cb41449f5f7c4b78754a7b")]
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
		public class Naval_BoatGroupSpawner_5ba08da4d3cb41449f5f7c4b78754a7b : Patch
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
		[Identifier("f4005b49e6404e018e436eee3e58b3e4")]
		[Dependencies(new string[] { "OnBoatGroupSpawned" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_BoatGroupSpawner_f4005b49e6404e018e436eee3e58b3e4 : Patch
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
		[Identifier("ee3d09dc992f4bcfbfc0285316d12e85")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TriggerDeepSeaPortal", false)]
		[Parameter("ent", "BaseEntity", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_TriggerDeepSeaPortal_ee3d09dc992f4bcfbfc0285316d12e85 : Patch
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
		[Identifier("9a36e55a1d774d7a8710434fddc88b37")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("entity", "BaseEntity", false)]
		[Parameter("portal", "DeepSeaPortal", false)]
		[Return(typeof(ValueTuple<bool, Phrase>))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_TriggerDeepSeaPortal_9a36e55a1d774d7a8710434fddc88b37 : Patch
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
		[Identifier("e893fa0e48fb48a18d2f959026fc9f31")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Cannon", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_Cannon_e893fa0e48fb48a18d2f959026fc9f31 : Patch
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
		[Identifier("c91a12627e09452f9ae8d6328fcd7bac")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerBoat", false)]
		[Parameter("local0", "BaseEntity", false)]
		[Parameter("collision", "UnityEngine.Collision", false)]
		[Return(typeof(void))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_PlayerBoat_c91a12627e09452f9ae8d6328fcd7bac : Patch
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
		[Identifier("8c3f76ab48a34e1f89ea7c9a5f810458")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerBoat", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_PlayerBoat_8c3f76ab48a34e1f89ea7c9a5f810458 : Patch
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
		[Identifier("df8c20eafba14baba7340e779f689d73")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerCloseDeepSeaAsyncd80_df8c20eafba14baba7340e779f689d73 : Patch
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
		[Identifier("bdc3eb16f41d4b0e944e6a5b667e5c6b")]
		[Dependencies(new string[] { "OnDeepSeaClosed" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerCloseDeepSeaAsyncd80_bdc3eb16f41d4b0e944e6a5b667e5c6b : Patch
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
		[Identifier("c3e5323540cf4ef780d22aefa0abce1b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerOpenDeepSeaAsyncd78_c3e5323540cf4ef780d22aefa0abce1b : Patch
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
		[Identifier("0bcc6b9e253d4e9db8cd58f3078495ea")]
		[Dependencies(new string[] { "OnDeepSeaOpened" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "DeepSeaManager", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_DeepSeaManagerOpenDeepSeaAsyncd78_0bcc6b9e253d4e9db8cd58f3078495ea : Patch
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
		[Identifier("38d5475a2d70424e9a8f65af77e9a501")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "PlayerBoat", false)]
		[Parameter("self", "BoatBuildingStation", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Naval")]
		[Assembly("Assembly-CSharp.dll")]
		public class Naval_BoatBuildingStation_38d5475a2d70424e9a8f65af77e9a501 : Patch
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
