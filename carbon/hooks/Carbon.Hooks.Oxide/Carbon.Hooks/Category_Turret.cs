using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Turret
{
	public class Turret_AutoTurret
	{
		[Patch("OnTurretTarget", "OnTurretTarget", "AutoTurret", "SetTarget", new string[] { "BaseCombatEntity" })]
		[Identifier("c6b81657ad8c468bbca03f251cef5394")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Return(typeof(void))]
		[Category("Turret")]
		[Assembly("Assembly-CSharp.dll")]
		public class Turret_AutoTurret_c6b81657ad8c468bbca03f251cef5394 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-603875700)), instruction), instruction);
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

		[Patch("OnTurretStartup", "OnTurretStartup", "AutoTurret", "InitiateStartup", new string[] { })]
		[Identifier("a1a354150a584b28994611a3f6534058")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Return(typeof(void))]
		[Category("Turret")]
		[Assembly("Assembly-CSharp.dll")]
		public class Turret_AutoTurret_a1a354150a584b28994611a3f6534058 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 7)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-576967812)), instruction), instruction);
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

		[Patch("OnTurretShutdown", "OnTurretShutdown", "AutoTurret", "InitiateShutdown", new string[] { })]
		[Identifier("77b7805959c94a6bb0473f27670e4f9d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Return(typeof(void))]
		[Category("Turret")]
		[Assembly("Assembly-CSharp.dll")]
		public class Turret_AutoTurret_77b7805959c94a6bb0473f27670e4f9d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 7)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1020909229), instruction), instruction);
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

		[Patch("OnTurretToggle", "OnTurretToggle", "AutoTurret", "SetIsOnline", new string[] { "System.Boolean" })]
		[Identifier("3938bd00566749748b954e12cd1a1969")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Return(typeof(void))]
		[Category("Turret")]
		[Assembly("Assembly-CSharp.dll")]
		public class Turret_AutoTurret_3938bd00566749748b954e12cd1a1969 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)168477335), instruction);
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

		[Patch("OnTurretDeauthorize", "OnTurretDeauthorize", "AutoTurret", "RemoveSelfAuthorize", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("412814020e6c497b86d3a10a67cb3f4e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Turret")]
		[Assembly("Assembly-CSharp.dll")]
		public class Turret_AutoTurret_412814020e6c497b86d3a10a67cb3f4e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 12)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-911357897)), instruction), instruction);
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

		[Patch("OnTurretClearList", "OnTurretClearList", "AutoTurret", "ClearList", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("f2a7f8fbf5c746da8dd32dd3ce6e384d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Turret")]
		[Assembly("Assembly-CSharp.dll")]
		public class Turret_AutoTurret_f2a7f8fbf5c746da8dd32dd3ce6e384d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 18)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-530757151)), instruction), instruction);
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

		[Patch("OnTurretModeToggle", "OnTurretModeToggle [Peacekeeper]", "AutoTurret", "SERVER_Peacekeeper", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("d14ae3e72e9e47aaa9c27abd822d0dbe")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Turret")]
		[Assembly("Assembly-CSharp.dll")]
		public class Turret_AutoTurret_d14ae3e72e9e47aaa9c27abd822d0dbe : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 5)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)273264000), instruction), instruction);
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

		[Patch("OnTurretRotate", "OnTurretRotate", "AutoTurret", "FlipAim", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("5c414ad113724211b80aaed82c6cbf04")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Turret")]
		[Assembly("Assembly-CSharp.dll")]
		public class Turret_AutoTurret_5c414ad113724211b80aaed82c6cbf04 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 12)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1562791322), instruction), instruction);
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

		[Patch("OnTurretAuthorize", "OnTurretAuthorize", "AutoTurret", "AddSelfAuthorize", new string[] { "BasePlayer" })]
		[Identifier("7055833f185a4cdc9c0a9f2180d3617d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Turret")]
		[Assembly("Assembly-CSharp.dll")]
		public class Turret_AutoTurret_7055833f185a4cdc9c0a9f2180d3617d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1791975019), instruction), instruction);
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

		[Patch("OnTurretAssign", "OnTurretAssign", "AutoTurret", "AssignToFriend", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("a6e45fef30e34938ad0143879fcc8747")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Parameter("local0", "System.UInt64", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Turret")]
		[Assembly("Assembly-CSharp.dll")]
		public class Turret_AutoTurret_a6e45fef30e34938ad0143879fcc8747 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)554719856), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt64"));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnTurretAssigned", "OnTurretAssigned", "AutoTurret", "AssignToFriend", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("7c8f0bb1b99f48dfa4b8be30a32cc1b0")]
		[Dependencies(new string[] { "OnTurretAssign" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Parameter("local0", "System.UInt64", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Turret")]
		[Assembly("Assembly-CSharp.dll")]
		public class Turret_AutoTurret_7c8f0bb1b99f48dfa4b8be30a32cc1b0 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 60)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)498675742), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt64"));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnInterferenceUpdate", "OnInterferenceUpdate", "AutoTurret", "RecalculateInterference", new string[] { })]
		[Identifier("194cc24ec60e478da184e5335d101579")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Return(typeof(bool))]
		[Category("Turret")]
		[Assembly("Assembly-CSharp.dll")]
		public class Turret_AutoTurret_194cc24ec60e478da184e5335d101579 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2075584415), instruction);
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

		[Patch("OnTurretModeToggle", "OnTurretModeToggle [AttackAll]", "AutoTurret", "SERVER_AttackAll", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("dc11fd5fd79245a6a9d24df4ac81759e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Turret")]
		[Assembly("Assembly-CSharp.dll")]
		public class Turret_AutoTurret_dc11fd5fd79245a6a9d24df4ac81759e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 5)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)273264000), instruction), instruction);
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

		[Patch("OnTurretIdentifierSet", "OnTurretIdentifierSet", "AutoTurret", "SetID", new string[] { "BasePlayer", "System.String", "System.String", "System.Boolean" })]
		[Identifier("88001527c4c74ddf80453b8e5233c788")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "AutoTurret", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("newID", "System.String", false)]
		[Return(typeof(void))]
		[Category("Turret")]
		[Assembly("Assembly-CSharp.dll")]
		public class Turret_AutoTurret_88001527c4c74ddf80453b8e5233c788 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)931035950), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
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

		[Patch("OnNearbyTurretsScan", "OnNearbyTurretsScan", "AutoTurret", "AddNearbyTurrets", new string[] { })]
		[Identifier("1cb8fee034514cd28f458a2043b43e1c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Turret")]
		[Assembly("Assembly-CSharp.dll")]
		public class Turret_AutoTurret_1cb8fee034514cd28f458a2043b43e1c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Expected O, but got Unknown
				//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c2: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnNearbyTurretsScan"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[13];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[5]), list2[5]);
				}
				list2.InsertRange(5, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
