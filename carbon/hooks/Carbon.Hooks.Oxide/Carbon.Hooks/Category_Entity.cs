using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Core;
using Carbon.Extensions;
using HarmonyLib;
using UnityEngine;

namespace Carbon.Hooks;

public class Category_Entity
{
	public class Entity_BaseNetworkable
	{
		[Patch("OnEntitySpawned", "OnEntitySpawned", "BaseNetworkable", "Spawn", new string[] { })]
		[Identifier("8ff3002624fb4692bd22f2e0ca33ad58")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNetworkable", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseNetworkable_8ff3002624fb4692bd22f2e0ca33ad58 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1345128879)), instruction), instruction);
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

		[Patch("OnEntityKill", "OnEntityKill", "BaseNetworkable", "Kill", new string[] { "BaseNetworkable/DestroyMode", "System.Boolean" })]
		[Identifier("b163cd3ea0954ca788c05cfa1fe3805a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNetworkable", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseNetworkable_b163cd3ea0954ca788c05cfa1fe3805a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 14)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)304634108), instruction), instruction);
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

		[Patch("IOnEntitySaved", "IOnEntitySaved", "BaseNetworkable", "ToStream", new string[] { "System.IO.Stream", "BaseNetworkable/SaveInfo" })]
		[Identifier("4b2e2c369ed0486c9d7ed47db0320be2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNetworkable", false)]
		[Parameter("saveInfo", "BaseNetworkable+SaveInfo", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseNetworkable_4b2e2c369ed0486c9d7ed47db0320be2 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldarg_0, (object)null), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnEntitySaved", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnEntitySnapshot", "OnEntitySnapshot", "BaseNetworkable", "SendAsSnapshot", new string[] { "Network.Connection", "System.Boolean" })]
		[Identifier("cfddad1d5d2843a097b6915b400c5d53")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNetworkable", false)]
		[Parameter("connection", "Network.Connection", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseNetworkable_cfddad1d5d2843a097b6915b400c5d53 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1024129379), instruction), instruction);
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

		[Patch("OnEntityLoaded", "OnEntityLoaded", "BaseNetworkable", "Load", new string[] { "BaseNetworkable/LoadInfo" })]
		[Identifier("b1cb67e9933c4c779b4c71c19c547694")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNetworkable", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseNetworkable_b1cb67e9933c4c779b4c71c19c547694 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)752002944), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(LoadInfo));
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

		[Patch("OnEntitySnapshot", "OnEntitySnapshot [BaseNetworkable NetWrite]", "BaseNetworkable", "SendAsSnapshot", new string[] { "Network.Connection", "Network.NetWrite", "BaseNetworkable/ThreadSafeTime&", "System.Boolean" })]
		[Identifier("d18ceca6606847b09196c6abcce36308")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNetworkable", false)]
		[Parameter("connection", "Network.Connection", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseNetworkable_d18ceca6606847b09196c6abcce36308 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1024129379), instruction), instruction);
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

	public class Entity_TriggerBase
	{
		[Patch("OnEntityEnter", "OnEntityEnter", "TriggerBase", "OnEntityEnter", new string[] { "BaseEntity" })]
		[Identifier("9173dfa28e224091b834ab682634fe83")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TriggerBase", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_TriggerBase_9173dfa28e224091b834ab682634fe83 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1472985181), instruction), instruction);
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

		[Patch("OnEntityLeave", "OnEntityLeave", "TriggerBase", "OnEntityLeave", new string[] { "BaseEntity" })]
		[Identifier("75557784717a4b1cb8fd83239b236ced")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TriggerBase", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_TriggerBase_75557784717a4b1cb8fd83239b236ced : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-835509410)), instruction), instruction);
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

	public class Entity_BaseCombatEntity
	{
		[Patch("IOnBaseCombatEntityHurt", "IOnBaseCombatEntityHurt", "BaseCombatEntity", "Hurt", new string[] { "HitInfo" })]
		[Identifier("94dfa45da6d34407ba82d86e04b875e0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseCombatEntity", false)]
		[Parameter("info", "HitInfo", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseCombatEntity_94dfa45da6d34407ba82d86e04b875e0 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 225)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldarg_0, (object)null), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnBaseCombatEntityHurt", (Type[])null, (Type[])null));
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

		[Patch("OnEntityMarkHostile", "OnEntityMarkHostile", "BaseCombatEntity", "MarkHostileFor", new string[] { "System.Single" })]
		[Identifier("218341ed100041cdbd1227a34af807cc")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseCombatEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseCombatEntity_218341ed100041cdbd1227a34af807cc : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-612103326)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
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

		[Patch("CanEntityBeHostile", "CanEntityBeHostile", "BaseCombatEntity", "IsHostile", new string[] { })]
		[Identifier("13a382c0772241ba92840d9dfc15a4ea")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseCombatEntity", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseCombatEntity_13a382c0772241ba92840d9dfc15a4ea : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2008184701)), instruction), instruction);
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

		[Patch("OnEntityDeath", "OnEntityDeath [BaseCombatEntity]", "BaseCombatEntity", "Die", new string[] { "HitInfo" })]
		[Identifier("5417267dc82742e3813bccb07c449467")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseCombatEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseCombatEntity_5417267dc82742e3813bccb07c449467 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 15)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1779071345), instruction);
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

		[Patch("OnEntityPickedUp", "OnEntityPickedUp", "BaseCombatEntity", "OnPickedUp", new string[] { "Item", "BasePlayer" })]
		[Identifier("8d2aca665a4d445fa186cfcdc8002f50")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseCombatEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseCombatEntity_8d2aca665a4d445fa186cfcdc8002f50 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1524387679), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
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

	public class Entity_DestroyOnGroundMissing
	{
		[Patch("OnEntityGroundMissing", "OnEntityGroundMissing", "DestroyOnGroundMissing", "OnGroundMissing", new string[] { })]
		[Identifier("d71686568c3e4ca5ad3d8fdbed3eb116")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BaseEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DestroyOnGroundMissing_d71686568c3e4ca5ad3d8fdbed3eb116 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)883461), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

	public class Entity_CargoPlane
	{
		[Patch("OnAirdrop", "OnAirdrop", "CargoPlane", "UpdateDropPosition", new string[] { "UnityEngine.Vector3" })]
		[Identifier("b9e4355d9d21441da6b80c3235d82f03")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CargoPlane", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CargoPlane_b9e4355d9d21441da6b80c3235d82f03 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2124327688), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(Vector3));
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

		[Patch("OnSupplyDropDropped", "OnSupplyDropDropped", "CargoPlane", "Update", new string[] { })]
		[Identifier("ca708976608e4c4d8aee696cf15d3a57")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "BaseEntity", false)]
		[Parameter("self", "CargoPlane", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CargoPlane_ca708976608e4c4d8aee696cf15d3a57 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 47)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2011096229), instruction), instruction);
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

	public class Entity_BaseOven
	{
		[Patch("OnOvenToggle", "OnOvenToggle", "BaseOven", "SVSwitch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("484ba3bef91c4cf5a8a46e882f25dee3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseOven", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseOven_484ba3bef91c4cf5a8a46e882f25dee3 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1133742332)), instruction), instruction);
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

		[Patch("OnOvenCook", "OnOvenCook", "BaseOven", "Cook", new string[] { "System.Single" })]
		[Identifier("25e21169fe534f5d91d75bbadec2b766")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseOven", false)]
		[Parameter("local0", "Item", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseOven_25e21169fe534f5d91d75bbadec2b766 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1341536445)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

		[Patch("OnOvenCooked", "OnOvenCooked", "BaseOven", "Cook", new string[] { "System.Single" })]
		[Identifier("597fa581beca42da9f7e438c61bc925f")]
		[Dependencies(new string[] { "OnOvenCook" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseOven", false)]
		[Parameter("local0", "Item", false)]
		[Parameter("local1", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseOven_597fa581beca42da9f7e438c61bc925f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 119)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2008347206)), instruction);
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
					yield return new CodeInstruction(OpCodes.Pop, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnOvenStart", "OnOvenStart", "BaseOven", "StartCooking", new string[] { })]
		[Identifier("0deb7fa47c73477d9ad6d36283bc45e4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseOven", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseOven_0deb7fa47c73477d9ad6d36283bc45e4 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1236136137)), instruction);
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

		[Patch("OnOvenStarted", "OnOvenStarted", "BaseOven", "StartCooking", new string[] { })]
		[Identifier("772a280ec1df4a03927ca3584d3bbdfc")]
		[Dependencies(new string[] { "OnOvenStart" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseOven", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseOven_772a280ec1df4a03927ca3584d3bbdfc : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 45)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)743527400), instruction);
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

		[Patch("OnOvenTemperature", "OnOvenTemperature", "BaseOven", "GetTemperature", new string[] { "System.Int32" })]
		[Identifier("273566703cdf462c88a317090ff463bb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseOven", false)]
		[Return(typeof(float))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseOven_273566703cdf462c88a317090ff463bb : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)365560906), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(float));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(float));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class Entity_Recycler
	{
		[Patch("OnRecyclerToggle", "OnRecyclerToggle", "Recycler", "SVSwitch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("aeafbf14ca4743a9a8d0f67d0899c83f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Recycler", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_Recycler_aeafbf14ca4743a9a8d0f67d0899c83f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 15)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1114916409)), instruction);
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

	public class Entity_DropUtil
	{
		[Patch("OnContainerDropItems", "OnContainerDropItems", "DropUtil", "DropItems", new string[] { "ItemContainer", "UnityEngine.Vector3", "System.Boolean" })]
		[Identifier("ef55e87effff441ea14d8acc441059cb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("container", "ItemContainer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DropUtil_ef55e87effff441ea14d8acc441059cb : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-370729442)), instruction), instruction);
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

	public class Entity_BaseMountable
	{
		[Patch("OnEntityDismounted", "OnEntityDismounted", "BaseMountable", "DismountPlayer", new string[] { "BasePlayer", "System.Boolean" })]
		[Identifier("a0f0dc5c66344b04a82fb77b1acc3bcc")]
		[Dependencies(new string[] { "CanDismountEntity" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMountable", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseMountable_a0f0dc5c66344b04a82fb77b1acc3bcc : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 284)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2026747374), instruction), instruction);
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

		[Patch("OnEntityDismounted", "OnEntityDismounted [lite]", "BaseMountable", "DismountPlayer", new string[] { "BasePlayer", "System.Boolean" })]
		[Identifier("47cd0adaa30a4ecd9197c0d013dbff1f")]
		[Dependencies(new string[] { "OnEntityDismounted" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMountable", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseMountable_47cd0adaa30a4ecd9197c0d013dbff1f : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2026747374), instruction), instruction);
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

	public class Entity_HackableLockedCrate
	{
		[Patch("OnCrateHack", "OnCrateHack", "HackableLockedCrate", "StartHacking", new string[] { })]
		[Identifier("e5cfbff655b145fea20a95106048eb01")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HackableLockedCrate", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HackableLockedCrate_e5cfbff655b145fea20a95106048eb01 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1392780491), instruction);
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

		[Patch("OnCrateHackEnd", "OnCrateHackEnd", "HackableLockedCrate", "HackProgress", new string[] { })]
		[Identifier("759434af8e0748de85ffbfa082a3716f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HackableLockedCrate", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HackableLockedCrate_759434af8e0748de85ffbfa082a3716f : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1418106200), instruction);
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

		[Patch("OnCrateLanded", "OnCrateLanded", "HackableLockedCrate", "LandCheck", new string[] { })]
		[Identifier("aa09253dd8414a4ab0474455de7071aa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HackableLockedCrate", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HackableLockedCrate_aa09253dd8414a4ab0474455de7071aa : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-227433108)), instruction), instruction);
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

		[Patch("OnCrateDropped", "OnCrateDropped", "HackableLockedCrate", "SetWasDropped", new string[] { })]
		[Identifier("440e4f529abc44a2bcb0cf41c4c384c3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HackableLockedCrate", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HackableLockedCrate_440e4f529abc44a2bcb0cf41c4c384c3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 3)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-651175011)), instruction), instruction);
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

		[Patch("OnCrateLaptopAttack", "OnCrateLaptopAttack", "HackableLockedCrate", "OnAttacked", new string[] { "HitInfo" })]
		[Identifier("07051f7129734f078d95b442dd4b8d5f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HackableLockedCrate", false)]
		[Parameter("info", "HitInfo", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HackableLockedCrate_07051f7129734f078d95b442dd4b8d5f : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1169007882)), instruction), instruction);
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

	public class Entity_CH47HelicopterAIController
	{
		[Patch("OnEntityDestroy", "OnEntityDestroy [CH47Helicopter]", "CH47HelicopterAIController", "OnDied", new string[] { "HitInfo" })]
		[Identifier("6043b67bf5de4a6791977a1fe1f5e5de")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CH47HelicopterAIController", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CH47HelicopterAIController_6043b67bf5de4a6791977a1fe1f5e5de : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)430051754), instruction), instruction);
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

	public class Entity_BaseArcadeMachine
	{
		[Patch("OnArcadeScoreAdded", "OnArcadeScoreAdded", "BaseArcadeMachine", "AddScore", new string[] { "BasePlayer", "System.Int32" })]
		[Identifier("9bdc614798124fd1afe35a58e2132bfe")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseArcadeMachine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("score", "System.Int32", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseArcadeMachine_9bdc614798124fd1afe35a58e2132bfe : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 64)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2037464427)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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

	public class Entity_BasePlayer
	{
		[Patch("CanEntityBeHostile", "CanEntityBeHostile [BasePlayer]", "BasePlayer", "IsHostile", new string[] { })]
		[Identifier("2fa23932c82b441d85fe3f99a45d13e3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BasePlayer_2fa23932c82b441d85fe3f99a45d13e3 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2008184701)), instruction), instruction);
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

		[Patch("OnEntityMarkHostile", "OnEntityMarkHostile [BasePlayer]", "BasePlayer", "MarkHostileFor", new string[] { "System.Single" })]
		[Identifier("ae87181e2ed3450b955ca00bbfc47eb8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BasePlayer_ae87181e2ed3450b955ca00bbfc47eb8 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-612103326)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
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

		[Patch("OnEntitySnapshot", "OnEntitySnapshot [BasePlayer]", "BasePlayer", "SendEntitySnapshot", new string[] { "BaseNetworkable" })]
		[Identifier("f72a30e29b5541d4b8f2dc4395b68003")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("ent", "BaseNetworkable", false)]
		[Parameter("connection", "Network.Connection", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BasePlayer_f72a30e29b5541d4b8f2dc4395b68003 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1024129379), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer"), "net"));
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("Network.Networkable"), "get_connection", (Type[])null, (Type[])null));
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

		[Patch("CanSeeStash", "CanSeeStash", "BasePlayer", "CheckStashRevealInvoke", new string[] { })]
		[Identifier("d5230f8bcabb4bb6bf379f4dea035daf")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("entity", "StashContainer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BasePlayer_d5230f8bcabb4bb6bf379f4dea035daf : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 43)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)35618031), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer+NearbyStash"), "Entity"));
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

		[Patch("OnStashExposed", "OnStashExposed", "BasePlayer", "CheckStashRevealInvoke", new string[] { })]
		[Identifier("f12c1546939b47b2a498096bd75e00db")]
		[Dependencies(new string[] { "CanSeeStash" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("entity", "StashContainer", false)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BasePlayer_f12c1546939b47b2a498096bd75e00db : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 59)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1506495919), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BasePlayer+NearbyStash"), "Entity"));
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

	public class Entity_SamSite
	{
		[Patch("CanSamSiteShoot", "CanSamSiteShoot", "SamSite", "WeaponTick", new string[] { })]
		[Identifier("09225a71a5f94be98b80d54dd70e047a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SamSite", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SamSite_09225a71a5f94be98b80d54dd70e047a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 39)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1088682450), instruction), instruction);
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

		[Patch("OnSamSiteModeToggle", "OnSamSiteModeToggle", "SamSite", "ToggleDefenderMode", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("2dbd9ced0a2e4ae9a2057620c96a85bb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SamSite", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.Boolean", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SamSite_2dbd9ced0a2e4ae9a2057620c96a85bb : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1306874856)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Boolean"));
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

		[Patch("OnSamSiteTarget", "OnSamSiteTarget", "SamSite", "TargetScan", new string[] { })]
		[Identifier("c9f7e0b25bbf47caa50ad89af5343593")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SamSite_c9f7e0b25bbf47caa50ad89af5343593 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Expected Obj, but got Unknown
				//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c2: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnSamSiteTarget"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 6, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[120];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[117]), list2[117]);
				}
				list2.InsertRange(117, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnSamSiteTargetScan", "OnSamSiteTargetScan", "SamSite", "TargetScan", new string[] { })]
		[Identifier("1804c259d9cf485cb1526b4be174dc01")]
		[Dependencies(new string[] { "OnSamSiteTarget" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SamSite_1804c259d9cf485cb1526b4be174dc01 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Expected Obj, but got Unknown
				//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c2: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnSamSiteTargetScan"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[79];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[66]), list2[66]);
				}
				list2.InsertRange(66, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Entity_ElectricSwitch
	{
		[Patch("OnSwitchToggle", "OnSwitchToggle [ElectricSwitch]", "ElectricSwitch", "RPC_Switch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("45c301430a384b6485965cff61cb08ab")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ElectricSwitch", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ElectricSwitch_45c301430a384b6485965cff61cb08ab : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-254646694)), instruction), instruction);
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

		[Patch("OnSwitchToggled", "OnSwitchToggled [ElectricSwitch]", "ElectricSwitch", "RPC_Switch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("179c923e85f44b3b89c8f05efb91c972")]
		[Dependencies(new string[] { "OnSwitchToggle [ElectricSwitch]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ElectricSwitch", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ElectricSwitch_179c923e85f44b3b89c8f05efb91c972 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 15)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)588890708), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

	public class Entity_ResourceEntity
	{
		[Patch("OnEntityTakeDamage", "OnEntityTakeDamage [ResourceEntity]", "ResourceEntity", "OnAttacked", new string[] { "HitInfo" })]
		[Identifier("089e78fa6d2f445ea3792dbf827edf55")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ResourceEntity_089e78fa6d2f445ea3792dbf827edf55 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 6)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)952055589), instruction), instruction);
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

		[Patch("OnEntityDeath", "OnEntityDeath [ResourceEntity]", "ResourceEntity", "OnDied", new string[] { "HitInfo" })]
		[Identifier("b0cc1f86b9bb4b9681b902a7a0c2520c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ResourceEntity_b0cc1f86b9bb4b9681b902a7a0c2520c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 3)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1779071345), instruction), instruction);
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

	public class Entity_SupplyDrop
	{
		[Patch("OnSupplyDropLanded", "OnSupplyDropLanded", "SupplyDrop", "OnCollisionEnter", new string[] { "UnityEngine.Collision" })]
		[Identifier("e9ec8985189242029d57e21164bf4ea8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SupplyDrop", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SupplyDrop_e9ec8985189242029d57e21164bf4ea8 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)164052317), instruction), instruction);
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

	public class Entity_TriggerComfort
	{
		[Patch("OnEntityEnter", "OnEntityEnter [TriggerComfort]", "TriggerComfort", "OnEntityEnter", new string[] { "BaseEntity" })]
		[Identifier("e5ff23de6b20436cb84d7e3d8bed4070")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TriggerComfort", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_TriggerComfort_e5ff23de6b20436cb84d7e3d8bed4070 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1472985181), instruction), instruction);
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

		[Patch("OnEntityLeave", "OnEntityLeave [TriggerComfort]", "TriggerComfort", "OnEntityLeave", new string[] { "BaseEntity" })]
		[Identifier("5e9de220e47e4b92bdfa69566f2bf2f6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TriggerComfort", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_TriggerComfort_5e9de220e47e4b92bdfa69566f2bf2f6 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-835509410)), instruction), instruction);
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

	public class Entity_StabilityEntity
	{
		[Patch("OnEntityStabilityCheck", "OnEntityStabilityCheck", "StabilityEntity", "StabilityCheck", new string[] { })]
		[Identifier("b6d0e9497b6043eab605aa8144ee943f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "StabilityEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_StabilityEntity_b6d0e9497b6043eab605aa8144ee943f : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1437379155)), instruction), instruction);
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

	public class Entity_MonumentBlocker
	{
		[Patch("OnEntityTakeDamage", "OnEntityTakeDamage [MonumentBlocker]", "MonumentBlocker", "OnAttacked", new string[] { "HitInfo" })]
		[Identifier("ca42a532f363497cb5cb02381c2ca0fd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MonumentBlocker", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_MonumentBlocker_ca42a532f363497cb5cb02381c2ca0fd : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)952055589), instruction), instruction);
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

		[Patch("OnEntityDeath", "OnEntityDeath [MonumentBlocker]", "MonumentBlocker", "OnDied", new string[] { "HitInfo" })]
		[Identifier("5c540481c165414183c0df83db960f23")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MonumentBlocker", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_MonumentBlocker_5c540481c165414183c0df83db960f23 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 3)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1779071345), instruction), instruction);
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

	public class Entity_DieselEngine
	{
		[Patch("OnDieselEngineToggled", "OnDieselEngineToggled [off]", "DieselEngine", "EngineOff", new string[] { })]
		[Identifier("6c1cfa6d2ec24cfd992dfec3f0d42cea")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DieselEngine", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DieselEngine_6c1cfa6d2ec24cfd992dfec3f0d42cea : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1223867373), instruction);
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

		[Patch("OnDieselEngineToggled", "OnDieselEngineToggled [on]", "DieselEngine", "EngineOn", new string[] { })]
		[Identifier("32e554882c63454aad77af3c098e9429")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DieselEngine", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DieselEngine_32e554882c63454aad77af3c098e9429 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1223867373), instruction);
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

		[Patch("OnDieselEngineToggle", "OnDieselEngineToggle", "DieselEngine", "EngineSwitch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("5a7ee7c798e54835b171c722abe779c9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DieselEngine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DieselEngine_5a7ee7c798e54835b171c722abe779c9 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-591897112)), instruction), instruction);
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

	public class Entity_BaseEntity
	{
		[Patch("OnBuildingPrivilege", "OnBuildingPrivilege", "BaseEntity", "GetBuildingPrivilege", new string[] { "OBB", "System.Boolean", "System.Single", "BuildingPrivlidge", "System.Boolean" })]
		[Identifier("a15a01eff5cd4ed2a37a8a4fc8f181e3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseEntity", false)]
		[Return(typeof(BuildingPrivlidge))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseEntity_a15a01eff5cd4ed2a37a8a4fc8f181e3 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1138411791)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(OBB));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)4);
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)5);
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
					Label label1 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(BuildingPrivlidge));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(BuildingPrivlidge));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnEntityFlagsNetworkUpdate", "OnEntityFlagsNetworkUpdate", "BaseEntity", "SendNetworkUpdate_Flags", new string[] { })]
		[Identifier("9aaae337856b4eb5a5fb5028e493849a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseEntity_9aaae337856b4eb5a5fb5028e493849a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 27)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1568260765), instruction);
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

	public class Entity_CargoShip
	{
		[Patch("OnCargoShipEgress", "OnCargoShipEgress", "CargoShip", "StartEgress", new string[] { })]
		[Identifier("1ae78b0eecd84056b1695e91e0879327")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CargoShip", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CargoShip_1ae78b0eecd84056b1695e91e0879327 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-945323082)), instruction);
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

		[Patch("OnCargoShipSpawnCrate", "OnCargoShipSpawnCrate", "CargoShip", "RespawnLoot", new string[] { })]
		[Identifier("c1b1ac17c63141078bc8f69cab9dce19")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CargoShip", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CargoShip_c1b1ac17c63141078bc8f69cab9dce19 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1757939003), instruction), instruction);
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

		[Patch("OnCargoShipHarborApproach", "OnCargoShipHarborApproach", "CargoShip", "StartHarborApproach", new string[] { "CargoNotifier" })]
		[Identifier("3265494ba3c940fcad7f396871331a85")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CargoShip", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CargoShip_3265494ba3c940fcad7f396871331a85 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-138767071)), instruction);
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

		[Patch("OnCargoShipHarborArrived", "OnCargoShipHarborArrived", "CargoShip", "OnArrivedAtHarbor", new string[] { })]
		[Identifier("caa9d0679a164ac092fcd6390158e650")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CargoShip", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CargoShip_caa9d0679a164ac092fcd6390158e650 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 130)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-751899300)), instruction);
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

		[Patch("OnCargoShipHarborLeave", "OnCargoShipHarborLeave", "CargoShip", "LeaveHarbor", new string[] { })]
		[Identifier("d18c726f0c414dcfa2cc5b8bb3756a3e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CargoShip", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CargoShip_d18c726f0c414dcfa2cc5b8bb3756a3e : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1425064063)), instruction);
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

	public class Entity_BradleyAPC
	{
		[Patch("OnEntityDestroy", "OnEntityDestroy [BradleyAPC]", "BradleyAPC", "OnDied", new string[] { "HitInfo" })]
		[Identifier("159da90c80394c3a89e2ac6703ef29fc")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BradleyAPC_159da90c80394c3a89e2ac6703ef29fc : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)430051754), instruction);
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

		[Patch("OnCrateSpawned", "OnCrateSpawned [BradleyAPC]", "BradleyAPC", "OnDied", new string[] { "HitInfo" })]
		[Identifier("7b9396b47d5f422eb0ec5adb76bcfe83")]
		[Dependencies(new string[] { "OnEntityDestroy [BradleyAPC]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Parameter("local14", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BradleyAPC_7b9396b47d5f422eb0ec5adb76bcfe83 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 283)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2131038016), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)14);
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

	public class Entity_FuelGenerator
	{
		[Patch("OnSwitchToggle", "OnSwitchToggle [FuelGenerator]", "FuelGenerator", "RPC_EngineSwitch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("0f17ca55efae4893853b6b40620d0d6d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FuelGenerator", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_FuelGenerator_0f17ca55efae4893853b6b40620d0d6d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-254646694)), instruction), instruction);
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

		[Patch("OnSwitchToggled", "OnSwitchToggled [FuelGenerator]", "FuelGenerator", "RPC_EngineSwitch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("fc621d42db944898b0562fb42b7007a3")]
		[Dependencies(new string[] { "OnSwitchToggle [FuelGenerator]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FuelGenerator", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_FuelGenerator_fc621d42db944898b0562fb42b7007a3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 15)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)588890708), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

	public class Entity_BaseEntityRPCServerIsActiveItem
	{
		[Patch("OnEntityActiveCheck", "OnEntityActiveCheck", "BaseEntity/RPC_Server/IsActiveItem", "Test", new string[] { "System.UInt32", "System.String", "BaseEntity", "BasePlayer" })]
		[Identifier("0939efccbfee447e899ef3ffc9678fe7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("ent", "BaseEntity", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("id", "System.UInt32", false)]
		[Parameter("debugName", "System.String", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseEntityRPCServerIsActiveItem_0939efccbfee447e899ef3ffc9678fe7 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1561104099), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(uint));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
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

	public class Entity_BaseEntityRPCServerFromOwner
	{
		[Patch("OnEntityFromOwnerCheck", "OnEntityFromOwnerCheck", "BaseEntity/RPC_Server/FromOwner", "Test", new string[] { "System.UInt32", "System.String", "BaseEntity", "BasePlayer" })]
		[Identifier("5502ca3fd6db4abcb40e35f3688cd65a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("ent", "BaseEntity", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("id", "System.UInt32", false)]
		[Parameter("debugName", "System.String", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseEntityRPCServerFromOwner_5502ca3fd6db4abcb40e35f3688cd65a : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-296815805)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(uint));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[5]
					{
						typeof(uint),
						typeof(object),
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

	public class Entity_BaseEntityRPCServerIsVisible
	{
		[Patch("OnEntityVisibilityCheck", "OnEntityVisibilityCheck", "BaseEntity/RPC_Server/IsVisible", "Test", new string[] { "System.UInt32", "System.String", "BaseEntity", "BasePlayer", "System.Single" })]
		[Identifier("5d782c7254f24e7faa31f3a940535e89")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("ent", "BaseEntity", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("id", "System.UInt32", false)]
		[Parameter("debugName", "System.String", false)]
		[Parameter("maximumDistance", "System.Single", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseEntityRPCServerIsVisible_5d782c7254f24e7faa31f3a940535e89 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1153778787)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(uint));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)4);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[6]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
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

	public class Entity_BaseEntityRPCServerMaxDistance
	{
		[Patch("OnEntityDistanceCheck", "OnEntityDistanceCheck", "BaseEntity/RPC_Server/MaxDistance", "Test", new string[] { "System.UInt32", "System.String", "BaseEntity", "BasePlayer", "System.Single", "System.Boolean" })]
		[Identifier("43a010ef289e49ad86292e0784eb3680")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("ent", "BaseEntity", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("id", "System.UInt32", false)]
		[Parameter("debugName", "System.String", false)]
		[Parameter("maximumDistance", "System.Single", false)]
		[Parameter("checkParent", "System.Boolean", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseEntityRPCServerMaxDistance_43a010ef289e49ad86292e0784eb3680 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1582967250), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(uint));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)4);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)5);
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

	public class Entity_StashContainer
	{
		[Patch("OnStashHidden", "OnStashHidden", "StashContainer", "RPC_HideStash", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("e3cffcc19fd84ed1bcfbff6e0fa68e17")]
		[Dependencies(new string[] { "CanHideStash" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "StashContainer", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_StashContainer_e3cffcc19fd84ed1bcfbff6e0fa68e17 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 15)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1147855574), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnStashOcclude", "OnStashOcclude", "StashContainer", "DoOccludedCheck", new string[] { })]
		[Identifier("79ad3154d11b471982442fad70bfe5fa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "StashContainer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_StashContainer_79ad3154d11b471982442fad70bfe5fa : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 14)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-64820960)), instruction), instruction);
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

	public class Entity_MixingTable
	{
		[Patch("OnMixingTableToggle", "OnMixingTableToggle", "MixingTable", "SVSwitch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("8d254ffd110f4fdfa70c814f60d8b246")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MixingTable", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_MixingTable_8d254ffd110f4fdfa70c814f60d8b246 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1519034006), instruction), instruction);
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

		[Patch("OnMixingTableFinished", "OnMixingTableFinished", "MixingTable", "ProduceItem", new string[] { "Recipe", "System.Int32" })]
		[Identifier("2be00d0b56124e119e97b8731a364c15")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MixingTable", false)]
		[Parameter("mixStartingPlayer", "BasePlayer", false)]
		[Parameter("recipe", "Recipe", false)]
		[Parameter("quantity", "System.Int32", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_MixingTable_2be00d0b56124e119e97b8731a364c15 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 15)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)224409329), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("MixingTable"), "get_MixStartingPlayer", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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
	}

	public class Entity_SleepingBag
	{
		[Patch("OnSleepingBagDestroyed", "OnSleepingBagDestroyed", "SleepingBag", "DestroyBag", new string[] { "System.UInt64", "NetworkableId" })]
		[Identifier("4d99d15ff5a84da0af83566c4bf13695")]
		[Dependencies(new string[] { "OnSleepingBagDestroy" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "SleepingBag", false)]
		[Parameter("userID", "System.UInt64", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SleepingBag_4d99d15ff5a84da0af83566c4bf13695 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 75)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1768944892), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
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

		[Patch("OnSleepingBagValidCheck", "OnSleepingBagValidCheck", "SleepingBag", "ValidForPlayer", new string[] { "System.UInt64", "System.Boolean" })]
		[Identifier("ff7f4831ba1b48829920472826219fb8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SleepingBag", false)]
		[Parameter("playerID", "System.UInt64", false)]
		[Parameter("ignoreTimers", "System.Boolean", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SleepingBag_ff7f4831ba1b48829920472826219fb8 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-812564304)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
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

		[Patch("OnBedMade", "OnBedMade", "SleepingBag", "RPC_MakeBed", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("4cd618c690184d36b4aa1b2ce3e5854a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SleepingBag", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SleepingBag_4cd618c690184d36b4aa1b2ce3e5854a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 82)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1566638766)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnSleepingBagDestroy", "OnSleepingBagDestroy", "SleepingBag", "DestroyBag", new string[] { "System.UInt64", "NetworkableId" })]
		[Identifier("7874d3c990f34abda7c795905756f5ef")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SleepingBag_7874d3c990f34abda7c795905756f5ef : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Expected Obj, but got Unknown
				//IL_005b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0065: Expected Obj, but got Unknown
				//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b7: Expected Obj, but got Unknown
				//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00dc: Expected Obj, but got Unknown
				//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ed: Expected Obj, but got Unknown
				//IL_012b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0135: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnSleepingBagDestroy"));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Box, (object)typeof(ulong)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[37];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldc_I4_0, (object)null));
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 5, typeof(object)));
				Label label2 = Generator.DefineLabel();
				list2[76].labels.Add(label2);
				list.Add(new CodeInstruction(OpCodes.Leave_S, (object)label2));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[37]), list2[37]);
				}
				list2.InsertRange(37, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Entity_SurveyCrater
	{
		[Patch("OnAnalysisComplete", "OnAnalysisComplete", "SurveyCrater", "AnalysisComplete", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("37fec8ca8caf48c09bae8336ee7dc660")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SurveyCrater", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SurveyCrater_37fec8ca8caf48c09bae8336ee7dc660 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)283407006), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

	public class Entity_HotAirBalloon
	{
		[Patch("OnHotAirBalloonToggle", "OnHotAirBalloonToggle", "HotAirBalloon", "EngineSwitch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("6f472978ca044eb2b517d82b250a6137")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HotAirBalloon", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HotAirBalloon_6f472978ca044eb2b517d82b250a6137 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-991450486)), instruction);
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

		[Patch("OnHotAirBalloonToggled", "OnHotAirBalloonToggled [on]", "HotAirBalloon", "EngineSwitch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("71c5df20388b43dba984eb550b61172a")]
		[Dependencies(new string[] { "OnHotAirBalloonToggle" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HotAirBalloon", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HotAirBalloon_71c5df20388b43dba984eb550b61172a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 52)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1950810279)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

		[Patch("OnHotAirBalloonToggled", "OnHotAirBalloonToggled [off]", "HotAirBalloon", "EngineSwitch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("b5894bf7cafc4b38b8a440735d0b8c29")]
		[Dependencies(new string[] { "OnHotAirBalloonToggled [on]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HotAirBalloon", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HotAirBalloon_b5894bf7cafc4b38b8a440735d0b8c29 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 64)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1950810279)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

	public class Entity_ReactiveTarget
	{
		[Patch("OnReactiveTargetReset", "OnReactiveTargetReset", "ReactiveTarget", "ResetTarget", new string[] { })]
		[Identifier("662423bb921943b88cce1b2f2960f7fb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ReactiveTarget", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ReactiveTarget_662423bb921943b88cce1b2f2960f7fb : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 39)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1804871904)), instruction);
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

	public class Entity_SupplySignal
	{
		[Patch("OnCargoPlaneSignaled", "OnCargoPlaneSignaled", "SupplySignal", "Explode", new string[] { })]
		[Identifier("9925dc1ee74545578d8307508d7974e5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BaseEntity", false)]
		[Parameter("self", "SupplySignal", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SupplySignal_9925dc1ee74545578d8307508d7974e5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 37)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1350371272), instruction), instruction);
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

	public class Entity_WaterPurifier
	{
		[Patch("OnWaterPurify", "OnWaterPurify", "WaterPurifier", "ConvertWater", new string[] { "System.Single" })]
		[Identifier("fe65af3ad06a45a68a14f1946756a63b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "WaterPurifier", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_WaterPurifier_fe65af3ad06a45a68a14f1946756a63b : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2010102072), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
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

		[Patch("OnWaterPurified", "OnWaterPurified", "WaterPurifier", "ConvertWater", new string[] { "System.Single" })]
		[Identifier("3d057e0aad174387be0369851d58b241")]
		[Dependencies(new string[] { "OnWaterPurify" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "WaterPurifier", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_WaterPurifier_3d057e0aad174387be0369851d58b241 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 178)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1939746975)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
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

	public class Entity_WaterCatcher
	{
		[Patch("OnWaterCollect", "OnWaterCollect [WaterCatcher]", "WaterCatcher", "CollectWater", new string[] { })]
		[Identifier("5a23a48855d34ff08355526e89a05125")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "WaterCatcher", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_WaterCatcher_5a23a48855d34ff08355526e89a05125 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 72)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)318355959), instruction);
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

	public class Entity_BaseLiquidVessel
	{
		[Patch("OnLiquidVesselFill", "OnLiquidVesselFill", "BaseLiquidVessel", "FillCheck", new string[] { })]
		[Identifier("2cfcea4127a64fafb3e729009784e6aa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseLiquidVessel", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local3", "LiquidContainer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseLiquidVessel_2cfcea4127a64fafb3e729009784e6aa : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1365929204)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
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
	}

	public class Entity_DecayEntity
	{
		[Patch("OnDecayHeal", "OnDecayHeal", "DecayEntity", "OnDecay", new string[] { "Decay", "System.Single" })]
		[Identifier("2ba2809192c04cd4a0e7806ae76edddb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DecayEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DecayEntity_2ba2809192c04cd4a0e7806ae76edddb : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 84)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1830760464), instruction);
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

		[Patch("OnDecayDamage", "OnDecayDamage", "DecayEntity", "OnDecay", new string[] { "Decay", "System.Single" })]
		[Identifier("b510c5985fa64e709f5c9c56a170c73c")]
		[Dependencies(new string[] { "OnDecayHeal" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DecayEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DecayEntity_b510c5985fa64e709f5c9c56a170c73c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 166)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1821956534), instruction);
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

		[Patch("OnDebrisSpawn", "OnDebrisSpawn", "DecayEntity", "SpawnDebris", new string[] { "UnityEngine.Vector3", "UnityEngine.Quaternion", "System.Boolean" })]
		[Identifier("6a54ef8a16f04368960499dae01d8788")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DecayEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DecayEntity_6a54ef8a16f04368960499dae01d8788 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-317967272)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(Vector3));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(Quaternion));
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
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
	}

	public class Entity_ElectricWindmill
	{
		[Patch("OnWindmillUpdate", "OnWindmillUpdate", "ElectricWindmill", "WindUpdate", new string[] { })]
		[Identifier("ed0949f4414b41dcb77489beefc0b8b7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ElectricWindmill", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ElectricWindmill_ed0949f4414b41dcb77489beefc0b8b7 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)410886036), instruction), instruction);
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

		[Patch("OnWindmillUpdated", "OnWindmillUpdated", "ElectricWindmill", "WindUpdate", new string[] { })]
		[Identifier("7ed8a55ced8a461f852377c2852ed460")]
		[Dependencies(new string[] { "OnWindmillUpdate" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ElectricWindmill", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ElectricWindmill_7ed8a55ced8a461f852377c2852ed460 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1523674490)), instruction), instruction);
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

	public class Entity_Mannequin
	{
		[Patch("CanMannequinChangePose", "CanMannequinChangePose", "Mannequin", "Server_ChangePose", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("c7eb80c222674fdbab694e1cdcd867af")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Mannequin", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_Mannequin_c7eb80c222674fdbab694e1cdcd867af : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-915365740)), instruction), instruction);
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

		[Patch("CanMannequinSwap", "CanMannequinSwap", "Mannequin", "Server_RequestSwap", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("43897d0f2f8e4a1cbe76e7c004b23a04")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Mannequin", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_Mannequin_43897d0f2f8e4a1cbe76e7c004b23a04 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 15)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1406341262)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

	public class Entity_WaterPump
	{
		[Patch("OnWaterCollect", "OnWaterCollect [WaterPump]", "WaterPump", "CreateWater", new string[] { })]
		[Identifier("01cf6b8624d44ff4b56ab9a967fe698d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "WaterPump", false)]
		[Parameter("local0", "ItemDefinition", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_WaterPump_01cf6b8624d44ff4b56ab9a967fe698d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)318355959), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

	public class Entity_Sprinkler
	{
		[Patch("OnSprinklerSplashed", "OnSprinklerSplashed", "Sprinkler", "DoSplash", new string[] { })]
		[Identifier("9dc937c7f2c14e268926d0a91e012917")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Sprinkler", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_Sprinkler_9dc937c7f2c14e268926d0a91e012917 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 363)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)106249974), instruction);
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

	public class Entity_WaterBall
	{
		[Patch("CanWaterBallSplash", "CanWaterBallSplash", "WaterBall", "DoSplash", new string[] { "UnityEngine.Vector3", "System.Single", "ItemDefinition", "System.Int32", "System.Boolean" })]
		[Identifier("7e4713785dc74e8185fc509bab15f6b7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("liquidDef", "ItemDefinition", false)]
		[Parameter("position", "UnityEngine.Vector3", false)]
		[Parameter("radius", "System.Single", false)]
		[Parameter("amount", "System.Int32", false)]
		[Parameter("funWater", "System.Boolean", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_WaterBall_7e4713785dc74e8185fc509bab15f6b7 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1337570747), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(Vector3));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)4);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[6]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
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

	public class Entity_InstantCameraTool
	{
		[Patch("OnPhotoCapture", "OnPhotoCapture", "InstantCameraTool", "TakePhoto", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("1f2b32dc6e894d9b892d791d809b0a57")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local5", "PhotoEntity", false)]
		[Parameter("local1", "Item", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "System.Byte[]", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_InstantCameraTool_1f2b32dc6e894d9b892d791d809b0a57 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 96)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2092469614)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}

		[Patch("OnPhotoCaptured", "OnPhotoCaptured", "InstantCameraTool", "TakePhoto", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("c2fab15eed18493d88bdf5552ca449e3")]
		[Dependencies(new string[] { "OnPhotoCapture" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local5", "PhotoEntity", false)]
		[Parameter("local1", "Item", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "System.Byte[]", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_InstantCameraTool_c2fab15eed18493d88bdf5552ca449e3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 250)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1706180494), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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
	}

	public class Entity_TreeEntity
	{
		[Patch("OnTreeMarkerHit", "OnTreeMarkerHit", "TreeEntity", "DidHitMarker", new string[] { "HitInfo" })]
		[Identifier("8c98b6a57d994ffcb8226e8cedfbf2ef")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TreeEntity", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_TreeEntity_8c98b6a57d994ffcb8226e8cedfbf2ef : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1531249294)), instruction), instruction);
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

	public class Entity_SprayCanSpray
	{
		[Patch("OnSprayRemove", "OnSprayRemove", "SprayCanSpray", "Server_RequestWaterClear", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("095398bb6b0c402e8bd88b53bca19e61")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SprayCanSpray", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SprayCanSpray_095398bb6b0c402e8bd88b53bca19e61 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1484587940)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

	public class Entity_PoweredRemoteControlEntity
	{
		[Patch("OnRemoteIdentifierUpdate", "OnRemoteIdentifierUpdate", "PoweredRemoteControlEntity", "UpdateIdentifier", new string[] { "System.String", "System.Boolean" })]
		[Identifier("6af29d61e70043b2b9edc18ec8f521fc")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PoweredRemoteControlEntity", false)]
		[Parameter("newID", "System.String", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_PoweredRemoteControlEntity_6af29d61e70043b2b9edc18ec8f521fc : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2122349615), instruction), instruction);
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

	public class Entity_IndustrialConveyor
	{
		[Patch("OnSwitchToggle", "OnSwitchToggle [IndustrialConveyor]", "IndustrialConveyor", "SvSwitch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("7002d1d4d1d2487980e4cb6e502cf739")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "IndustrialConveyor", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_IndustrialConveyor_7002d1d4d1d2487980e4cb6e502cf739 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-254646694)), instruction), instruction);
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

		[Patch("OnSwitchToggled", "OnSwitchToggled [IndustrialConveyor]", "IndustrialConveyor", "SvSwitch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("f7f995a7e53c42c1836e335dffc71fd1")]
		[Dependencies(new string[] { "OnSwitchToggle [IndustrialConveyor]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "IndustrialConveyor", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_IndustrialConveyor_f7f995a7e53c42c1836e335dffc71fd1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 14)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)588890708), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

	public class Entity_TimedExplosive
	{
		[Patch("CanExplosiveStick", "CanExplosiveStick", "TimedExplosive", "CanStickTo", new string[] { "BaseEntity" })]
		[Identifier("042a17bf63a243a1a75f959813db3a27")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TimedExplosive", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_TimedExplosive_042a17bf63a243a1a75f959813db3a27 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2031840135), instruction), instruction);
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

	public class Entity_PatrolHelicopter
	{
		[Patch("OnPatrolHelicopterTakeDamage", "OnPatrolHelicopterTakeDamage", "PatrolHelicopter", "Hurt", new string[] { "HitInfo" })]
		[Identifier("36f2cdef0a7749bfacc9393805ff2399")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopter", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_PatrolHelicopter_36f2cdef0a7749bfacc9393805ff2399 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2146588479)), instruction), instruction);
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

		[Patch("OnPatrolHelicopterKill", "OnPatrolHelicopterKill", "PatrolHelicopter", "Hurt", new string[] { "HitInfo" })]
		[Identifier("255b1502bcb24935ae684ed16e1efc04")]
		[Dependencies(new string[] { "OnPatrolHelicopterTakeDamage" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopter", false)]
		[Parameter("info", "HitInfo", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_PatrolHelicopter_255b1502bcb24935ae684ed16e1efc04 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 15)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1857089938), instruction), instruction);
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

		[Patch("OnCrateSpawned", "OnCrateSpawned [PatrolHelicopter]", "PatrolHelicopter", "OnDied", new string[] { "HitInfo" })]
		[Identifier("9133d603b2b14f9aac36ca516949bd59")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopter", false)]
		[Parameter("local14", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_PatrolHelicopter_9133d603b2b14f9aac36ca516949bd59 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 284)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)2131038016), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)14);
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

	public class Entity_PlanterBox
	{
		[Patch("OnPlanterBoxFertilize", "OnPlanterBoxFertilize", "PlanterBox", "FertilizeGrowables", new string[] { })]
		[Identifier("45004c00db6946bfa4fe8d7cfc57b76a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlanterBox", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_PlanterBox_45004c00db6946bfa4fe8d7cfc57b76a : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1359265040), instruction);
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

	public class Entity_PatrolHelicopterAI
	{
		[Patch("OnNoGoZoneAdded", "OnNoGoZoneAdded", "PatrolHelicopterAI", "NoGoZoneAdded", new string[] { "PatrolHelicopterAI/DangerZone" })]
		[Identifier("cfb8c932703c438a90cb3330a092012d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopterAI", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_PatrolHelicopterAI_cfb8c932703c438a90cb3330a092012d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 3)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1052347918)), instruction), instruction);
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

	public class Entity_TriggeredEventPrefab
	{
		[Patch("OnEventTrigger", "OnEventTrigger", "TriggeredEventPrefab", "RunEvent", new string[] { })]
		[Identifier("a199e82332ad4f46a5a0f48999aa67c4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TriggeredEventPrefab", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_TriggeredEventPrefab_a199e82332ad4f46a5a0f48999aa67c4 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1321592237)), instruction), instruction);
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

	public class Entity_WorldItem
	{
		[Patch("CanLootEntity", "CanLootEntity", "WorldItem", "RPC_OpenLoot", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("2182fe01e9fe46249e1d82fd1ab4f940")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "WorldItem", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_WorldItem_2182fe01e9fe46249e1d82fd1ab4f940 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1627232611), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
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

	public class Entity_FreeableLootContainer
	{
		[Patch("OnFreeableContainerRelease", "OnFreeableContainerRelease", "FreeableLootContainer", "Release", new string[] { "BasePlayer" })]
		[Identifier("700e4d50b7b34c6cb48c9112bc23e979")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FreeableLootContainer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_FreeableLootContainer_700e4d50b7b34c6cb48c9112bc23e979 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1623556006)), instruction);
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

		[Patch("OnFreeableContainerReleased", "OnFreeableContainerReleased", "FreeableLootContainer", "Release", new string[] { "BasePlayer" })]
		[Identifier("02947cfc322d422bab9921cc99e354ec")]
		[Dependencies(new string[] { "OnFreeableContainerRelease" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FreeableLootContainer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_FreeableLootContainer_02947cfc322d422bab9921cc99e354ec : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 76)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1264704693)), instruction);
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

		[Patch("OnFreeableContainerReleaseStarted", "OnFreeableContainerReleaseStarted", "FreeableLootContainer", "RPC_FreeCrateTimer", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("dfa75602ac0e462aa5ba4fddb9bf6352")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FreeableLootContainer", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_FreeableLootContainer_dfa75602ac0e462aa5ba4fddb9bf6352 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1361485205), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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

	public class Entity_ServerGib
	{
		[Patch("OnGibsSpawned", "OnGibsSpawned", "ServerGib", "CreateGibs", new string[] { "System.String", "UnityEngine.GameObject", "UnityEngine.GameObject", "UnityEngine.Vector3", "System.Single" })]
		[Identifier("42193d3ea5bb4e52b3e0e2fe74b5bc47")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "System.Collections.Generic.List`1[ServerGib]", false)]
		[Parameter("creator", "UnityEngine.GameObject", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ServerGib_42193d3ea5bb4e52b3e0e2fe74b5bc47 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 169)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-886458376)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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

	public class Entity_RustAiGen2LivestockAnimal
	{
		[Patch("OnEntityTakeDamage", "OnEntityTakeDamage [LivestockAnimal]", "Rust.Ai.Gen2.LivestockAnimal", "Hurt", new string[] { "HitInfo" })]
		[Identifier("0e2aa5e6ff9544f3801cfe41dcd2807d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.LivestockAnimal", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_RustAiGen2LivestockAnimal_0e2aa5e6ff9544f3801cfe41dcd2807d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)952055589), instruction), instruction);
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

	public class Entity_BigWheelGame
	{
		[Patch("OnBigWheelLoss", "OnBigWheelLoss", "BigWheelGame", "Payout", new string[] { })]
		[Identifier("d82a31f7c6ed437987551e051496b0c9")]
		[Dependencies(new string[] { "OnBigWheelWin" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BigWheelGame_d82a31f7c6ed437987551e051496b0c9 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c3: Expected Obj, but got Unknown
				//IL_00de: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e8: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnBigWheelLoss"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 10, typeof(object)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 3, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[4]
				{
					typeof(string),
					typeof(object),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[108];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[94]), list2[94]);
				}
				list2.InsertRange(94, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnBigWheelWin", "OnBigWheelWin", "BigWheelGame", "Payout", new string[] { })]
		[Identifier("5c153cb6fade4d499ad6002cb832b0ae")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BigWheelGame_5c153cb6fade4d499ad6002cb832b0ae : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_008b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0095: Expected Obj, but got Unknown
				//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
				//IL_0101: Expected Obj, but got Unknown
				//IL_011c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0126: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnBigWheelWin"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 6, typeof(object)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 3, typeof(object)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 7, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Box, (object)typeof(int)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[5]
				{
					typeof(string),
					typeof(object),
					typeof(object),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[76];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[43]), list2[43]);
				}
				list2.InsertRange(43, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
