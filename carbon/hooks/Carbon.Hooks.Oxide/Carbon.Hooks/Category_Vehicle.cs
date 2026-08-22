using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;
using UnityEngine;

namespace Carbon.Hooks;

public class Category_Vehicle
{
	public class Vehicle_HelicopterTurret
	{
		[Patch("OnHelicopterTarget", "OnHelicopterTarget", "HelicopterTurret", "SetTarget", new string[] { "BaseCombatEntity" })]
		[Identifier("e3766279cca7490a9a9aa804a0c72315")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HelicopterTurret", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_HelicopterTurret_e3766279cca7490a9a9aa804a0c72315 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1860966052), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

	public class Vehicle_PatrolHelicopterAI
	{
		[Patch("CanHelicopterStrafeTarget", "CanHelicopterStrafeTarget", "PatrolHelicopterAI", "ValidRocketTarget", new string[] { "BasePlayer" })]
		[Identifier("2f8b5d4b31e24fcbb0183211d19e5aa3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopterAI", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_PatrolHelicopterAI_2f8b5d4b31e24fcbb0183211d19e5aa3 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1126161881)), instruction), instruction);
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

		[Patch("CanHelicopterUseNapalm", "CanHelicopterUseNapalm", "PatrolHelicopterAI", "CanUseNapalm", new string[] { })]
		[Identifier("1ae122c523574595bea4962af3ca711a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopterAI", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_PatrolHelicopterAI_1ae122c523574595bea4962af3ca711a : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)723973224), instruction), instruction);
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

		[Patch("CanHelicopterStrafe", "CanHelicopterStrafe", "PatrolHelicopterAI", "CanStrafe", new string[] { })]
		[Identifier("c26c3e8b5285458284c85ff9b6bad21d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopterAI", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_PatrolHelicopterAI_c26c3e8b5285458284c85ff9b6bad21d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1865778960)), instruction), instruction);
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

		[Patch("CanHelicopterTarget", "CanHelicopterTarget", "PatrolHelicopterAI", "PlayerVisible", new string[] { "BasePlayer" })]
		[Identifier("d5509594e5f04915a7d22864fc133740")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopterAI", false)]
		[Parameter("ply", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_PatrolHelicopterAI_d5509594e5f04915a7d22864fc133740 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-100690872)), instruction);
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

		[Patch("OnHelicopterStrafeEnter", "OnHelicopterStrafeEnter", "PatrolHelicopterAI", "StartStrafe", new string[] { "BasePlayer", "System.Boolean" })]
		[Identifier("8755fb46f19f4442a249eae1d61bd1c1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopterAI", false)]
		[Parameter("position", "UnityEngine.Vector3", false)]
		[Parameter("strafeTarget", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_PatrolHelicopterAI_8755fb46f19f4442a249eae1d61bd1c1 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1260142114)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BasePlayer"), "get_transform", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("UnityEngine.Transform"), "get_position", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("UnityEngine.Vector3"));
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(BasePlayer));
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

		[Patch("OnHelicopterRetire", "OnHelicopterRetire", "PatrolHelicopterAI", "Retire", new string[] { })]
		[Identifier("aab2466f985142bea81c862a96b25265")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopterAI", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_PatrolHelicopterAI_aab2466f985142bea81c862a96b25265 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 4)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)811131292), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
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

	public class Vehicle_BradleyAPC
	{
		[Patch("CanBradleyApcTarget", "CanBradleyApcTarget", "BradleyAPC", "VisibilityTest", new string[] { "BaseEntity" })]
		[Identifier("7858bf3466854b7390191a8dba62d4e3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BradleyAPC_7858bf3466854b7390191a8dba62d4e3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 109)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1133714366)), instruction), instruction);
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

		[Patch("OnBradleyApcInitialize", "OnBradleyApcInitialize", "BradleyAPC", "Initialize", new string[] { })]
		[Identifier("50c5ae1828a747f7b63350d2b553c65d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BradleyAPC_50c5ae1828a747f7b63350d2b553c65d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)99036976), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
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

		[Patch("OnBradleyApcHunt", "OnBradleyApcHunt", "BradleyAPC", "UpdateMovement_Hunt", new string[] { })]
		[Identifier("b90e3846d6884b5093977228b1a79bf0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BradleyAPC_b90e3846d6884b5093977228b1a79bf0 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2009571478)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
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

		[Patch("OnBradleyApcPatrol", "OnBradleyApcPatrol", "BradleyAPC", "UpdateMovement_Patrol", new string[] { })]
		[Identifier("b08f4b23a2274e15a9201fd75f0fbb85")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BradleyAPC_b08f4b23a2274e15a9201fd75f0fbb85 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-494741600)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
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

		[Patch("OnBradleyApcThink", "OnBradleyApcThink", "BradleyAPC", "DoSimpleAI", new string[] { })]
		[Identifier("9fd5c5d1ad2d46cb8971d915985ee7f1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BradleyAPC_9fd5c5d1ad2d46cb8971d915985ee7f1 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1814075788), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
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
	}

	public class Vehicle_CH47HelicopterAIController
	{
		[Patch("CanHelicopterDropCrate", "CanHelicopterDropCrate", "CH47HelicopterAIController", "CanDropCrate", new string[] { })]
		[Identifier("d9cb1891189e4d3d9e1e094787c53340")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CH47HelicopterAIController", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_CH47HelicopterAIController_d9cb1891189e4d3d9e1e094787c53340 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)769742881), instruction), instruction);
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

		[Patch("OnHelicopterDropCrate", "OnHelicopterDropCrate", "CH47HelicopterAIController", "DropCrate", new string[] { })]
		[Identifier("d6d6f1e17b9b4d8d90c5f9200eccc309")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CH47HelicopterAIController", false)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_CH47HelicopterAIController_d6d6f1e17b9b4d8d90c5f9200eccc309 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 32)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1099756773)), instruction), instruction);
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

		[Patch("OnHelicopterAttack", "OnHelicopterAttack", "CH47HelicopterAIController", "OnAttacked", new string[] { "HitInfo" })]
		[Identifier("edd2f3ab871a41ffbc1955d696ffe0c0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CH47HelicopterAIController", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_CH47HelicopterAIController_edd2f3ab871a41ffbc1955d696ffe0c0 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1821059999), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("OnHelicopterOutOfCrates", "OnHelicopterOutOfCrates", "CH47HelicopterAIController", "OutOfCrates", new string[] { })]
		[Identifier("16482ef63d774fabb3599992ff9651bb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CH47HelicopterAIController", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_CH47HelicopterAIController_16482ef63d774fabb3599992ff9651bb : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)306073545), instruction), instruction);
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

		[Patch("OnHelicopterDropDoorOpen", "OnHelicopterDropDoorOpen", "CH47HelicopterAIController", "SetDropDoorOpen", new string[] { "System.Boolean" })]
		[Identifier("8ba977367f794518a1a74b740b465889")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CH47HelicopterAIController", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_CH47HelicopterAIController_8ba977367f794518a1a74b740b465889 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)394568533), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
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

		[Patch("CanUseHelicopter", "CanUseHelicopter", "CH47HelicopterAIController", "AttemptMount", new string[] { "BasePlayer", "System.Boolean" })]
		[Identifier("617dfbfe9abe43a7a4528fe4bfaa11d0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CH47HelicopterAIController", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_CH47HelicopterAIController_617dfbfe9abe43a7a4528fe4bfaa11d0 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1650330867), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

	public class Vehicle_BaseBoat
	{
		[Patch("OnBoatPathGenerate", "OnBoatPathGenerate", "BaseBoat", "GenerateOceanPatrolPath", new string[] { "System.Single", "System.Single" })]
		[Identifier("094648a7f7984a6ba4570c773a36392e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(List<Vector3>))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BaseBoat_094648a7f7984a6ba4570c773a36392e : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1313592344), instruction);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[1] { typeof(uint) }, (Type[])null));
					Label label1 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(List<Vector3>));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(List<Vector3>));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Vehicle_BaseVehicle
	{
		[Patch("OnVehiclePush", "OnVehiclePush", "BaseVehicle", "RPC_WantsPush", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("f65a30ee4a7f4d218e94986812b547c3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseVehicle", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BaseVehicle_f65a30ee4a7f4d218e94986812b547c3 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1027613504)), instruction), instruction);
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

	public class Vehicle_VehicleModuleEngine
	{
		[Patch("OnEngineStatsRefresh", "OnEngineStatsRefresh", "VehicleModuleEngine", "RefreshPerformanceStats", new string[] { "Rust.Modular.EngineStorage" })]
		[Identifier("f4a7fa98f140412abf24c6e03f453645")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VehicleModuleEngine", false)]
		[Parameter("engineStorage", "Rust.Modular.EngineStorage", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleModuleEngine_f4a7fa98f140412abf24c6e03f453645 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)733130680), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("OnEngineStatsRefreshed", "OnEngineStatsRefreshed", "VehicleModuleEngine", "RefreshPerformanceStats", new string[] { "Rust.Modular.EngineStorage" })]
		[Identifier("9fe61f13f8b547d48f5faaf926936ee2")]
		[Dependencies(new string[] { "OnEngineStatsRefresh" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VehicleModuleEngine", false)]
		[Parameter("engineStorage", "Rust.Modular.EngineStorage", false)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleModuleEngine_9fe61f13f8b547d48f5faaf926936ee2 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 58)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1632415359), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

	public class Vehicle_ModularCar
	{
		[Patch("OnVehicleModulesAssign", "OnVehicleModulesAssign", "ModularCar", "SpawnPreassignedModules", new string[] { })]
		[Identifier("7c3ffab745ec461b9dfb904174e60df2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ModularCar", false)]
		[Parameter("socketItemDefs", "Rust.Modular.ItemModVehicleModule[]", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCar_7c3ffab745ec461b9dfb904174e60df2 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 23)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1211293448), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("ModularCarPresetConfig"), "socketItemDefs"));
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

		[Patch("OnVehicleModulesAssigned", "OnVehicleModulesAssigned", "ModularCar", "SpawnPreassignedModules", new string[] { })]
		[Identifier("316d278196024494bfe315b296f96e01")]
		[Dependencies(new string[] { "OnVehicleModulesAssign" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ModularCar", false)]
		[Parameter("socketItemDefs", "Rust.Modular.ItemModVehicleModule[]", false)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCar_316d278196024494bfe315b296f96e01 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)646458517), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("ModularCarPresetConfig"), "socketItemDefs"));
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

		[Patch("CanDestroyLock", "CanDestroyLock", "ModularCar", "PlayerCanDestroyLock", new string[] { "BasePlayer", "BaseVehicleModule" })]
		[Identifier("bb5434cceb26499c8ea5dc8264da6487")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ModularCar", false)]
		[Parameter("viaModule", "BaseVehicleModule", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCar_bb5434cceb26499c8ea5dc8264da6487 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1017646668), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
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

	public class Vehicle_ModularCarGarage
	{
		[Patch("OnVehicleModuleSelect", "OnVehicleModuleSelect", "ModularCarGarage", "RPC_SelectedLootItem", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("cf80d64633054d63966e2a0da85f0549")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local2", "Item", false)]
		[Parameter("self", "ModularCarGarage", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarGarage_cf80d64633054d63966e2a0da85f0549 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 35)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)169597589), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnVehicleModuleSelected", "OnVehicleModuleSelected", "ModularCarGarage", "RPC_SelectedLootItem", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("38100819a72e441a90a24f05f9523a28")]
		[Dependencies(new string[] { "OnVehicleModuleSelect" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local2", "Item", false)]
		[Parameter("self", "ModularCarGarage", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarGarage_38100819a72e441a90a24f05f9523a28 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 99)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-451867432)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnVehicleModuleDeselected", "OnVehicleModuleDeselected", "ModularCarGarage", "RPC_DeselectedLootItem", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("35137b5fe94849d5b94432f41622c6de")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ModularCarGarage", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarGarage_35137b5fe94849d5b94432f41622c6de : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 26)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1848276154), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnVehicleLockRequest", "OnVehicleLockRequest", "ModularCarGarage", "RPC_RequestAddLock", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("ef8e0c4d055848ada387374f661faf1f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ModularCarGarage", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarGarage_ef8e0c4d055848ada387374f661faf1f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 24)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1717384866)), instruction), instruction);
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

		[Patch("OnLockRemove", "OnLockRemove", "ModularCarGarage", "RPC_RequestRemoveLock", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("29766e5867b44496acd26e8ce723aeb2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ModularCarGarage", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarGarage_29766e5867b44496acd26e8ce723aeb2 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 10)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1003872762), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("ModularCarGarage"), "get_carOccupant", (Type[])null, (Type[])null));
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

		[Patch("OnCodeChange", "OnCodeChange", "ModularCarGarage", "RPC_RequestNewCode", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("91924d27aa0a4c2a932cfbce7500e822")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ModularCarGarage", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarGarage_91924d27aa0a4c2a932cfbce7500e822 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 24)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-119374006)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("ModularCarGarage"), "get_carOccupant", (Type[])null, (Type[])null));
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

		[Patch("OnVehicleModuleSelectedFix [patch]", "OnVehicleModuleSelectedFix [patch]", "ModularCarGarage", "RPC_SelectedLootItem", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("7a183a0db44b4ccb9da93f70f221fb77")]
		[Dependencies(new string[] { "OnVehicleModuleSelected" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarGarage_7a183a0db44b4ccb9da93f70f221fb77 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[105];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[34]), list2[34]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[35], list2[34]), list2[34]);
				}
				list2.RemoveRange(34, 1);
				list2.InsertRange(34, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Vehicle_ModularCarCodeLock
	{
		[Patch("OnVehicleLockableCheck", "OnVehicleLockableCheck", "ModularCarCodeLock", "CanHaveALock", new string[] { })]
		[Identifier("98f558420b2c453b97dd5164712984ab")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ModularCarCodeLock", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarCodeLock_98f558420b2c453b97dd5164712984ab : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1488526457), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					Label label2 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Brfalse_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Brtrue_S, (object)label2);
					yield return new CodeInstruction(OpCodes.Ldc_I4_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return CodeInstructionExtensions.WithLabels(new CodeInstruction(OpCodes.Ldloc, retvar), new Label[1] { label2 });
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("CanLock", "CanLock", "ModularCarCodeLock", "HasLockPermission", new string[] { "BasePlayer" })]
		[Identifier("85c36dd502f54dc887299f6f7fa4eb16")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ModularCarCodeLock", false)]
		[Parameter("self1", "ModularCarCodeLock", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarCodeLock_85c36dd502f54dc887299f6f7fa4eb16 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1531266972), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("ModularCarCodeLock"), "owner"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
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

	public class Vehicle_RustModularEngineStorage
	{
		[Patch("OnEngineLoadoutRefresh", "OnEngineLoadoutRefresh", "Rust.Modular.EngineStorage", "RefreshLoadoutData", new string[] { })]
		[Identifier("4eb24183c2bc42bc8378c15ac9b9766c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Modular.EngineStorage", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_RustModularEngineStorage_4eb24183c2bc42bc8378c15ac9b9766c : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)991351741), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[2]
					{
						typeof(uint),
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

	public class Vehicle_BaseModularVehicle
	{
		[Patch("OnVehicleModuleMove", "OnVehicleModuleMove", "BaseModularVehicle", "CanMoveFrom", new string[] { "BasePlayer", "Item" })]
		[Identifier("9e81919114e2464bbf6c4e4ffb58bd62")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BaseVehicleModule", false)]
		[Parameter("self", "BaseModularVehicle", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(CanMoveFromResponse))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BaseModularVehicle_9e81919114e2464bbf6c4e4ffb58bd62 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 17)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-61909210)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(CanMoveFromResponse));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(CanMoveFromResponse));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Vehicle_MLRS
	{
		[Patch("OnMlrsFire", "OnMlrsFire", "MLRS", "Fire", new string[] { "BasePlayer" })]
		[Identifier("8477df3c63af41a6878a4886af85e1b4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MLRS", false)]
		[Parameter("owner", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MLRS_8477df3c63af41a6878a4886af85e1b4 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 33)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1918182502), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("OnMlrsFired", "OnMlrsFired", "MLRS", "Fire", new string[] { "BasePlayer" })]
		[Identifier("63c5705b1b4043ef845133e76563d213")]
		[Dependencies(new string[] { "OnMlrsFire" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MLRS", false)]
		[Parameter("owner", "BasePlayer", false)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MLRS_63c5705b1b4043ef845133e76563d213 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 81)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-556743022)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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

		[Patch("OnMlrsRocketFired", "OnMlrsRocketFired", "MLRS", "FireNextRocket", new string[] { })]
		[Identifier("69aa150d0ca94830b14c7bb5db73e8e8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MLRS", false)]
		[Parameter("local7", "ServerProjectile", false)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MLRS_69aa150d0ca94830b14c7bb5db73e8e8 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 118)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1771587149), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)7);
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

		[Patch("OnMlrsFiringEnded", "OnMlrsFiringEnded", "MLRS", "EndFiring", new string[] { })]
		[Identifier("948a3d30a1414ae08d3aa76e330c7eb2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MLRS", false)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MLRS_948a3d30a1414ae08d3aa76e330c7eb2 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 41)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-879478946)), instruction);
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

		[Patch("OnMlrsTarget", "OnMlrsTarget", "MLRS", "SetUserTargetHitPos", new string[] { "UnityEngine.Vector3" })]
		[Identifier("282e90a803894c0d9a71650df611eb85")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MLRS", false)]
		[Parameter("worldPos", "UnityEngine.Vector3", false)]
		[Parameter("self1", "MLRS", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MLRS_282e90a803894c0d9a71650df611eb85 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 38)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1914257016)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(Vector3));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("MLRS"), "_mounted"));
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}

		[Patch("OnMlrsTargetSet", "OnMlrsTargetSet", "MLRS", "SetUserTargetHitPos", new string[] { "UnityEngine.Vector3" })]
		[Identifier("fb9f1087e87044c4aca0359ddff645c9")]
		[Dependencies(new string[] { "OnMlrsTarget" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MLRS", false)]
		[Parameter("self1", "MLRS", false)]
		[Parameter("self2", "MLRS", false)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MLRS_fb9f1087e87044c4aca0359ddff645c9 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 124)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1122549224)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("MLRS"), "trueTargetHitPos"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("UnityEngine.Vector3"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("MLRS"), "_mounted"));
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
	}

	public class Vehicle_TrainCar
	{
		[Patch("OnTrainCarUncouple", "OnTrainCarUncouple", "TrainCar", "RPC_WantsUncouple", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("74a65bd673804b3d9f2c54cd08bb9f91")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TrainCar", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_TrainCar_74a65bd673804b3d9f2c54cd08bb9f91 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1462750240)), instruction), instruction);
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

	public class Vehicle_TrainCoupling
	{
		[Patch("CanTrainCarCouple", "CanTrainCarCouple", "TrainCoupling", "TryCouple", new string[] { "TrainCoupling", "System.Boolean" })]
		[Identifier("f3a59f5551554bdfa67bf4b504bd1325")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TrainCoupling", false)]
		[Parameter("owner", "TrainCar", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_TrainCoupling_f3a59f5551554bdfa67bf4b504bd1325 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)409706419), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("TrainCoupling"), "owner"));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("TrainCoupling"), "owner"));
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

	public class Vehicle_VehicleModuleSeating
	{
		[Patch("OnVehicleHornPressed", "OnVehicleHornPressed", "VehicleModuleSeating", "PlayerServerInput", new string[] { "InputState", "BasePlayer" })]
		[Identifier("ae031f47db0f44a7be536f4d13d1883f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VehicleModuleSeating", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleModuleSeating_ae031f47db0f44a7be536f4d13d1883f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 38)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1024380201)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
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

	public class Vehicle_VehiclePrivilege
	{
		[Patch("OnCupboardAuthorize", "OnCupboardAuthorize [VehiclePrivilege]", "VehiclePrivilege", "AddSelfAuthorize", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("40244e20d6b844668c16892e9c11d2ae")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VehiclePrivilege", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehiclePrivilege_40244e20d6b844668c16892e9c11d2ae : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 11)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1460091328), instruction), instruction);
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

		[Patch("OnCupboardDeauthorize", "OnCupboardDeauthorize [VehiclePrivilege]", "VehiclePrivilege", "RemoveSelfAuthorize", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("45d6658d07c449fca4a944863876b0fd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VehiclePrivilege", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehiclePrivilege_45d6658d07c449fca4a944863876b0fd : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 11)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1037905375), instruction), instruction);
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

		[Patch("OnCupboardClearList", "OnCupboardClearList [VehiclePrivilege]", "VehiclePrivilege", "ClearList", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("a0e273d192be4b839d11d5aa7d27d92d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VehiclePrivilege", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehiclePrivilege_a0e273d192be4b839d11d5aa7d27d92d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 11)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1797143416), instruction), instruction);
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

	public class Vehicle_MotorRowboat
	{
		[Patch("OnEngineStarted", "OnEngineStarted [MotorRowboat]", "MotorRowboat", "EngineToggle", new string[] { "System.Boolean" })]
		[Identifier("20ff061d275e48f29b6ac49661851429")]
		[Dependencies(new string[] { "OnEngineStart [MotorRowboat]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MotorRowboat", false)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MotorRowboat_20ff061d275e48f29b6ac49661851429 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 33)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)557013772), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return __GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object));
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

		[Patch("OnEngineStart", "OnEngineStart [MotorRowboat]", "MotorRowboat", "EngineToggle", new string[] { "System.Boolean" })]
		[Identifier("f82c20cb04424b89b7ba45c12d75169e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MotorRowboat_f82c20cb04424b89b7ba45c12d75169e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected O, but got Unknown
				//IL_0063: Unknown result type (might be due to invalid IL or missing references)
				//IL_006d: Expected O, but got Unknown
				//IL_0088: Unknown result type (might be due to invalid IL or missing references)
				//IL_0092: Expected O, but got Unknown
				//IL_009d: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a7: Expected O, but got Unknown
				//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b8: Expected O, but got Unknown
				//IL_0118: Unknown result type (might be due to invalid IL or missing references)
				//IL_0122: Expected O, but got Unknown
				//IL_012e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0138: Expected O, but got Unknown
				//IL_013f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0149: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseVehicle"), "GetDriver", (Type[])null, (Type[])null)));
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[6];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnEngineStart"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[6]), list2[6]);
				}
				list2.InsertRange(6, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Vehicle_VehicleEngineController1
	{
		[Patch("OnEngineStart", "OnEngineStart", "VehicleEngineController`1", "TryStartEngine", new string[] { "BasePlayer" })]
		[Identifier("914152d6a6e146dcbbe6779ba06f6149")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleEngineController1_914152d6a6e146dcbbe6779ba06f6149 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Expected O, but got Unknown
				//IL_005a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0064: Expected O, but got Unknown
				//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b6: Expected O, but got Unknown
				//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c7: Expected O, but got Unknown
				//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ec: Expected O, but got Unknown
				//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00fd: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnEngineStart"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(Method.DeclaringType, "owner")));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Ldnull, (object)null));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[26];
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[26]), list2[26]);
				}
				list2.InsertRange(26, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnEngineStarted", "OnEngineStarted", "VehicleEngineController`1", "TryStartEngine", new string[] { "BasePlayer" })]
		[Identifier("25f4325f6dfa46c5bd5cb0d3860fbd1e")]
		[Dependencies(new string[] { "OnEngineStart" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleEngineController1_25f4325f6dfa46c5bd5cb0d3860fbd1e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Expected O, but got Unknown
				//IL_005a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0064: Expected O, but got Unknown
				//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b6: Expected O, but got Unknown
				//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c7: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnEngineStarted"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(Method.DeclaringType, "owner")));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Pop, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[65]), list2[65]);
				}
				list2.InsertRange(65, list);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnEngineStop", "OnEngineStop", "VehicleEngineController`1", "StopEngine", new string[] { })]
		[Identifier("50a8bb59e72147acb5821b28346165dd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleEngineController1_50a8bb59e72147acb5821b28346165dd : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Expected O, but got Unknown
				//IL_008e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0098: Expected O, but got Unknown
				//IL_009f: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a9: Expected O, but got Unknown
				//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cd: Expected O, but got Unknown
				//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00de: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnEngineStop"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(Method.DeclaringType, "owner")));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[2]
				{
					typeof(string),
					typeof(object)
				}, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Ldnull, (object)null));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[8];
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[8]), list2[8]);
				}
				list2.InsertRange(8, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnEngineStopped", "OnEngineStopped", "VehicleEngineController`1", "StopEngine", new string[] { })]
		[Identifier("8e2bc47aeb6f4bca8395696160485932")]
		[Dependencies(new string[] { "OnEngineStop" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleEngineController1_8e2bc47aeb6f4bca8395696160485932 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Expected O, but got Unknown
				//IL_008e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0098: Expected O, but got Unknown
				//IL_009f: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a9: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnEngineStopped"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(Method.DeclaringType, "owner")));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[2]
				{
					typeof(string),
					typeof(object)
				}, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Pop, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[39]), list2[39]);
				}
				list2.InsertRange(39, list);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnEngineStartFinished", "OnEngineStartFinished", "VehicleEngineController`1", "FinishStartingEngine", new string[] { })]
		[Identifier("72a5350ad4914f4c8c98ea6dd2cf17b6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleEngineController1_72a5350ad4914f4c8c98ea6dd2cf17b6 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Expected O, but got Unknown
				//IL_008e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0098: Expected O, but got Unknown
				//IL_009f: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a9: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnEngineStartFinished"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(Method.DeclaringType, "owner")));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[2]
				{
					typeof(string),
					typeof(object)
				}, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Pop, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[36]), list2[36]);
				}
				list2.InsertRange(36, list);
				return list2.AsEnumerable();
			}
		}
	}
}
