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
		[Identifier("5e359f5aaf814ce8808a2ec014e2b45f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HelicopterTurret", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_HelicopterTurret_5e359f5aaf814ce8808a2ec014e2b45f : Patch
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
		[Identifier("32efa1dd7ee845fb85cdd5f5fe217c6b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopterAI", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_PatrolHelicopterAI_32efa1dd7ee845fb85cdd5f5fe217c6b : Patch
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
		[Identifier("0f6b82419eba43888ed8c77ca95e4c70")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopterAI", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_PatrolHelicopterAI_0f6b82419eba43888ed8c77ca95e4c70 : Patch
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
		[Identifier("338a95e0494143e9b7b440775c7ce476")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopterAI", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_PatrolHelicopterAI_338a95e0494143e9b7b440775c7ce476 : Patch
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
		[Identifier("d3d04cbf877840118106e20dde1ff706")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopterAI", false)]
		[Parameter("ply", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_PatrolHelicopterAI_d3d04cbf877840118106e20dde1ff706 : Patch
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
		[Identifier("bf00be38422444fdab1e9677e860c12e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopterAI", false)]
		[Parameter("position", "UnityEngine.Vector3", false)]
		[Parameter("strafeTarget", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_PatrolHelicopterAI_bf00be38422444fdab1e9677e860c12e : Patch
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
		[Identifier("dcc664d432a84e5db2e3c05c65fcc4d0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopterAI", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_PatrolHelicopterAI_dcc664d432a84e5db2e3c05c65fcc4d0 : Patch
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
		[Identifier("e36af1ad8ee644a9a463d9991482fdd1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BradleyAPC_e36af1ad8ee644a9a463d9991482fdd1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 107)
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
		[Identifier("28a66095c955445b9cdf0fe6678807cb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BradleyAPC_28a66095c955445b9cdf0fe6678807cb : Patch
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
		[Identifier("c9538ea19df04c4aabe1f2fa23fe65e3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BradleyAPC_c9538ea19df04c4aabe1f2fa23fe65e3 : Patch
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
		[Identifier("bd2900f603e84c9a9fa0b0ad43a62bd1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BradleyAPC_bd2900f603e84c9a9fa0b0ad43a62bd1 : Patch
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
		[Identifier("f2ac4ec61fbc46bf9476e9475224b247")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BradleyAPC_f2ac4ec61fbc46bf9476e9475224b247 : Patch
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
		[Identifier("b965d59fa44c42189dac8460d65e8e46")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CH47HelicopterAIController", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_CH47HelicopterAIController_b965d59fa44c42189dac8460d65e8e46 : Patch
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
		[Identifier("ebe74d2e32aa4c32ae1405030875c25d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CH47HelicopterAIController", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_CH47HelicopterAIController_ebe74d2e32aa4c32ae1405030875c25d : Patch
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
		[Identifier("4bc2dc94f9894606b81e97baffd9b771")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CH47HelicopterAIController", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_CH47HelicopterAIController_4bc2dc94f9894606b81e97baffd9b771 : Patch
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
		[Identifier("176eb8fa19f645318c95b38b8bf6f85f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CH47HelicopterAIController", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_CH47HelicopterAIController_176eb8fa19f645318c95b38b8bf6f85f : Patch
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
		[Identifier("2f0b11e1acf94ee09283586932558e7b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CH47HelicopterAIController", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_CH47HelicopterAIController_2f0b11e1acf94ee09283586932558e7b : Patch
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
		[Identifier("abcd9ede78014400bd3cbd3616cfb751")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CH47HelicopterAIController", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_CH47HelicopterAIController_abcd9ede78014400bd3cbd3616cfb751 : Patch
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
		[Identifier("d629b8bce8fc4fb6975e8b2fb3875458")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(List<Vector3>))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BaseBoat_d629b8bce8fc4fb6975e8b2fb3875458 : Patch
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
		[Identifier("9dfa029201854e168527da84b0750888")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseVehicle", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BaseVehicle_9dfa029201854e168527da84b0750888 : Patch
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
		[Identifier("65dfbc9880904f51b6deabce4a98b8e0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VehicleModuleEngine", false)]
		[Parameter("engineStorage", "Rust.Modular.EngineStorage", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleModuleEngine_65dfbc9880904f51b6deabce4a98b8e0 : Patch
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
		[Identifier("450160fb51b445f4a9e26f22bcf48099")]
		[Dependencies(new string[] { "OnEngineStatsRefresh" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VehicleModuleEngine", false)]
		[Parameter("engineStorage", "Rust.Modular.EngineStorage", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleModuleEngine_450160fb51b445f4a9e26f22bcf48099 : Patch
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
		[Identifier("6ba9efa7612c45b4abb92c86c0d6bb37")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ModularCar", false)]
		[Parameter("socketItemDefs", "Rust.Modular.ItemModVehicleModule[]", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCar_6ba9efa7612c45b4abb92c86c0d6bb37 : Patch
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
		[Identifier("7b347045edd441aa96c5c1d2c5e773fa")]
		[Dependencies(new string[] { "OnVehicleModulesAssign" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ModularCar", false)]
		[Parameter("socketItemDefs", "Rust.Modular.ItemModVehicleModule[]", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCar_7b347045edd441aa96c5c1d2c5e773fa : Patch
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
		[Identifier("0b992fcd63c94dbd93701acb28d7aca4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "ModularCar", false)]
		[Parameter("viaModule", "BaseVehicleModule", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCar_0b992fcd63c94dbd93701acb28d7aca4 : Patch
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
		[Identifier("53de4f3f38494901a6a9c3ba3e1605b4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local2", "Item", false)]
		[Parameter("self", "ModularCarGarage", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarGarage_53de4f3f38494901a6a9c3ba3e1605b4 : Patch
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
		[Identifier("67fa2939de8a430d8ad7be2af8457df5")]
		[Dependencies(new string[] { "OnVehicleModuleSelect" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local2", "Item", false)]
		[Parameter("self", "ModularCarGarage", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarGarage_67fa2939de8a430d8ad7be2af8457df5 : Patch
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
		[Identifier("6816b30471dd47c6b41e6c2d205ffaca")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ModularCarGarage", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarGarage_6816b30471dd47c6b41e6c2d205ffaca : Patch
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
		[Identifier("70726438617349bfbf5ffd82cfb2fbf6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ModularCarGarage", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarGarage_70726438617349bfbf5ffd82cfb2fbf6 : Patch
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
		[Identifier("2184a9bf603d4cc9921f167e17844f4f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("carOccupant", "ModularCar", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarGarage_2184a9bf603d4cc9921f167e17844f4f : Patch
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
		[Identifier("c15b4fc479be4f2285b720943f4ec93d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("carOccupant", "ModularCar", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.String", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarGarage_c15b4fc479be4f2285b720943f4ec93d : Patch
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
		[Identifier("43faa1d170cf4112b2158ca0fc24de09")]
		[Dependencies(new string[] { "OnVehicleModuleSelected" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarGarage_43faa1d170cf4112b2158ca0fc24de09 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[105];
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
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Vehicle_ModularCarCodeLock
	{
		[Patch("OnVehicleLockableCheck", "OnVehicleLockableCheck", "ModularCarCodeLock", "CanHaveALock", new string[] { })]
		[Identifier("54a1799b21b6415aaba2db824d1f298b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ModularCarCodeLock", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarCodeLock_54a1799b21b6415aaba2db824d1f298b : Patch
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
		[Identifier("deef2da835f64aef81a21232d167d75d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("owner", "ModularCar", false)]
		[Parameter("self", "ModularCarCodeLock", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_ModularCarCodeLock_deef2da835f64aef81a21232d167d75d : Patch
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
		[Identifier("6330e752496545ad885590c122d89945")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Modular.EngineStorage", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_RustModularEngineStorage_6330e752496545ad885590c122d89945 : Patch
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
		[Identifier("e547eeb3a293495483a12f295cb8136b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BaseVehicleModule", false)]
		[Parameter("self", "BaseModularVehicle", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(CanMoveFromResponse))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_BaseModularVehicle_e547eeb3a293495483a12f295cb8136b : Patch
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
		[Identifier("458d3a5c56444dec9bfacae8dae1ebc1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MLRS", false)]
		[Parameter("owner", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MLRS_458d3a5c56444dec9bfacae8dae1ebc1 : Patch
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
		[Identifier("72b1b6e96fc1410e8302a8b5d5728324")]
		[Dependencies(new string[] { "OnMlrsFire" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MLRS", false)]
		[Parameter("owner", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MLRS_72b1b6e96fc1410e8302a8b5d5728324 : Patch
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
		[Identifier("054c04743cba41f6b3c5b57b486cd428")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MLRS", false)]
		[Parameter("local7", "ServerProjectile", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MLRS_054c04743cba41f6b3c5b57b486cd428 : Patch
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
		[Identifier("86b2656bb0b4442fa74f7cb1ce932b1d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MLRS", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MLRS_86b2656bb0b4442fa74f7cb1ce932b1d : Patch
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
		[Identifier("f88c3446636b4ff5a152350536172d12")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MLRS", false)]
		[Parameter("worldPos", "UnityEngine.Vector3", false)]
		[Parameter("_mounted", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MLRS_f88c3446636b4ff5a152350536172d12 : Patch
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
		[Identifier("c00df22499ab45989329beadaa0144f9")]
		[Dependencies(new string[] { "OnMlrsTarget" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MLRS", false)]
		[Parameter("trueTargetHitPos", "UnityEngine.Vector3", false)]
		[Parameter("_mounted", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MLRS_c00df22499ab45989329beadaa0144f9 : Patch
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
		[Identifier("d15c52e5c5574f8daff8bccdcc3e9d01")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TrainCar", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_TrainCar_d15c52e5c5574f8daff8bccdcc3e9d01 : Patch
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
		[Identifier("00fa39606eca4fcfb7447bacb3714479")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("owner", "TrainCar", false)]
		[Parameter("owner1", "TrainCar", false)]
		[Return(typeof(bool))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_TrainCoupling_00fa39606eca4fcfb7447bacb3714479 : Patch
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
		[Identifier("1ef8cecde6d24ea488c5867ced0d716c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VehicleModuleSeating", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleModuleSeating_1ef8cecde6d24ea488c5867ced0d716c : Patch
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
		[Identifier("2c6ef6a550844663962970192b726069")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VehiclePrivilege", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehiclePrivilege_2c6ef6a550844663962970192b726069 : Patch
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
		[Identifier("fce03bae9b3c4d458bd07e25cdd1ea8d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VehiclePrivilege", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehiclePrivilege_fce03bae9b3c4d458bd07e25cdd1ea8d : Patch
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
		[Identifier("de979a4fe043458ba93e9e6049746ba3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "VehiclePrivilege", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehiclePrivilege_de979a4fe043458ba93e9e6049746ba3 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1797143416), instruction);
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
	}

	public class Vehicle_MotorRowboat
	{
		[Patch("OnEngineStarted", "OnEngineStarted [MotorRowboat]", "MotorRowboat", "EngineToggle", new string[] { "System.Boolean" })]
		[Identifier("3f348d46cd014eb3abf0056c7d20a6e5")]
		[Dependencies(new string[] { "OnEngineStart [MotorRowboat]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MotorRowboat", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MotorRowboat_3f348d46cd014eb3abf0056c7d20a6e5 : Patch
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
		[Identifier("88305d2498144c9988b00b7e5f6a9482")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_MotorRowboat_88305d2498144c9988b00b7e5f6a9482 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected Obj, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected Obj, but got Unknown
				//IL_0063: Unknown result type (might be due to invalid IL or missing references)
				//IL_006d: Expected Obj, but got Unknown
				//IL_0088: Unknown result type (might be due to invalid IL or missing references)
				//IL_0092: Expected Obj, but got Unknown
				//IL_009d: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a7: Expected Obj, but got Unknown
				//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b8: Expected Obj, but got Unknown
				//IL_0118: Unknown result type (might be due to invalid IL or missing references)
				//IL_0122: Expected Obj, but got Unknown
				//IL_012e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0138: Expected Obj, but got Unknown
				//IL_013f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0149: Expected Obj, but got Unknown
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
		[Identifier("74c8dcf24bbe4549880a16477bd9fc55")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleEngineController1_74c8dcf24bbe4549880a16477bd9fc55 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Expected Obj, but got Unknown
				//IL_005a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0064: Expected Obj, but got Unknown
				//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b6: Expected Obj, but got Unknown
				//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c7: Expected Obj, but got Unknown
				//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ec: Expected Obj, but got Unknown
				//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00fd: Expected Obj, but got Unknown
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
				CodeInstruction val = list2[26];
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[26]), list2[26]);
				}
				list2.InsertRange(26, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnEngineStarted", "OnEngineStarted", "VehicleEngineController`1", "TryStartEngine", new string[] { "BasePlayer" })]
		[Identifier("99d6b73c2b984b76abb2e141d194f7bd")]
		[Dependencies(new string[] { "OnEngineStart" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleEngineController1_99d6b73c2b984b76abb2e141d194f7bd : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Expected Obj, but got Unknown
				//IL_005a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0064: Expected Obj, but got Unknown
				//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b6: Expected Obj, but got Unknown
				//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c7: Expected Obj, but got Unknown
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
		[Identifier("dd6f3f7cbfdf47d998daf4eb0b778ef3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleEngineController1_dd6f3f7cbfdf47d998daf4eb0b778ef3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Expected Obj, but got Unknown
				//IL_008e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0098: Expected Obj, but got Unknown
				//IL_009f: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a9: Expected Obj, but got Unknown
				//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cd: Expected Obj, but got Unknown
				//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00de: Expected Obj, but got Unknown
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
				CodeInstruction val = list2[8];
				list.Add(new CodeInstruction(OpCodes.Beq_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[8]), list2[8]);
				}
				list2.InsertRange(8, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnEngineStopped", "OnEngineStopped", "VehicleEngineController`1", "StopEngine", new string[] { })]
		[Identifier("5d1d845a364c4ed3b65bd916a0d4609b")]
		[Dependencies(new string[] { "OnEngineStop" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleEngineController1_5d1d845a364c4ed3b65bd916a0d4609b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Expected Obj, but got Unknown
				//IL_008e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0098: Expected Obj, but got Unknown
				//IL_009f: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a9: Expected Obj, but got Unknown
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
		[Identifier("22af601eb1574dbca2c9deacbdc4d4bb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Vehicle")]
		[Assembly("Assembly-CSharp.dll")]
		public class Vehicle_VehicleEngineController1_22af601eb1574dbca2c9deacbdc4d4bb : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Expected Obj, but got Unknown
				//IL_008e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0098: Expected Obj, but got Unknown
				//IL_009f: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a9: Expected Obj, but got Unknown
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
