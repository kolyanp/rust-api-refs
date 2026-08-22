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
		[Identifier("8b0afb824034406fa1c6d4ef6d12bf4f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNetworkable", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseNetworkable_8b0afb824034406fa1c6d4ef6d12bf4f : Patch
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
		[Identifier("70181900a93a4102a7cd1a7c5140ec10")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNetworkable", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseNetworkable_70181900a93a4102a7cd1a7c5140ec10 : Patch
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
		[Identifier("c56939be43734930a11aa40b8c768ff9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNetworkable", false)]
		[Parameter("saveInfo", "BaseNetworkable+SaveInfo", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseNetworkable_c56939be43734930a11aa40b8c768ff9 : Patch
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
		[Identifier("5d43227caa54472f865f9ea0c569886a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNetworkable", false)]
		[Parameter("connection", "Network.Connection", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseNetworkable_5d43227caa54472f865f9ea0c569886a : Patch
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
		[Identifier("1d9041c96a904e7e855bef47932de279")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNetworkable", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseNetworkable_1d9041c96a904e7e855bef47932de279 : Patch
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
		[Identifier("7945de87beea4cc59d280b7ca71cb255")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNetworkable", false)]
		[Parameter("connection", "Network.Connection", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseNetworkable_7945de87beea4cc59d280b7ca71cb255 : Patch
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
		[Identifier("2adbef9569304169963f555205a033da")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TriggerBase", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_TriggerBase_2adbef9569304169963f555205a033da : Patch
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
		[Identifier("35ed750e779e4e06ac6b9eb52f415a78")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TriggerBase", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_TriggerBase_35ed750e779e4e06ac6b9eb52f415a78 : Patch
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
		[Identifier("c77ebf200a4d40a7a60be15186095128")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseCombatEntity", false)]
		[Parameter("info", "HitInfo", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseCombatEntity_c77ebf200a4d40a7a60be15186095128 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 229)
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
		[Identifier("c0a0313b05344038bd5266a1a7564834")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseCombatEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseCombatEntity_c0a0313b05344038bd5266a1a7564834 : Patch
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
		[Identifier("748d36bf5a9a4b86b9c651d4eaeac716")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseCombatEntity", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseCombatEntity_748d36bf5a9a4b86b9c651d4eaeac716 : Patch
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
		[Identifier("5e160f7e1ee04a08a2a9a8ea70110d11")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseCombatEntity", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseCombatEntity_5e160f7e1ee04a08a2a9a8ea70110d11 : Patch
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
		[Identifier("fc19e584f59f47beb9bb3c909fe11cad")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseCombatEntity", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseCombatEntity_fc19e584f59f47beb9bb3c909fe11cad : Patch
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
		[Identifier("6d1fb6021b8f46afaf9725320e5cb896")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BaseEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DestroyOnGroundMissing_6d1fb6021b8f46afaf9725320e5cb896 : Patch
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
		[Identifier("28e51d568dca4bf39f525b2335c61343")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CargoPlane", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CargoPlane_28e51d568dca4bf39f525b2335c61343 : Patch
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
		[Identifier("a50fe1d859c0493d90fbae061b65e6c1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "BaseEntity", false)]
		[Parameter("self", "CargoPlane", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CargoPlane_a50fe1d859c0493d90fbae061b65e6c1 : Patch
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
		[Identifier("af3f3899c2774d5bbfa4ee33c8d63a53")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseOven", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseOven_af3f3899c2774d5bbfa4ee33c8d63a53 : Patch
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
		[Identifier("c6ad7f19a2334b4fa3b7533bcbacb57b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseOven", false)]
		[Parameter("local0", "Item", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseOven_c6ad7f19a2334b4fa3b7533bcbacb57b : Patch
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
		[Identifier("a27276f1230a4c87b295dc224ca49cd0")]
		[Dependencies(new string[] { "OnOvenCook" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseOven", false)]
		[Parameter("local0", "Item", false)]
		[Parameter("local1", "BaseEntity", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseOven_a27276f1230a4c87b295dc224ca49cd0 : Patch
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
		[Identifier("86feaaadab914a4b8a2b4a3d3262ce3a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseOven", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseOven_86feaaadab914a4b8a2b4a3d3262ce3a : Patch
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
		[Identifier("3f4a202056104a20a3647da11724e32d")]
		[Dependencies(new string[] { "OnOvenStart" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseOven", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseOven_3f4a202056104a20a3647da11724e32d : Patch
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
		[Identifier("de91e543a2494b7c8d18454e0468064d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseOven", false)]
		[Return(typeof(float))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseOven_de91e543a2494b7c8d18454e0468064d : Patch
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
		[Identifier("8f4a6dbe4ce54d4dbdfea6f27fc7f45c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Recycler", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_Recycler_8f4a6dbe4ce54d4dbdfea6f27fc7f45c : Patch
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
		[Patch("OnContainerDropItems", "OnContainerDropItems", "DropUtil", "DropItems", new string[] { "ItemContainer", "UnityEngine.Vector3" })]
		[Identifier("34cf97182058408cbb5abcc9b0fadf6b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("container", "ItemContainer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DropUtil_34cf97182058408cbb5abcc9b0fadf6b : Patch
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
		[Identifier("25bcdedc147540b9978273266c9881c2")]
		[Dependencies(new string[] { "CanDismountEntity" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMountable", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseMountable_25bcdedc147540b9978273266c9881c2 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 271)
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
		[Identifier("aae99d134ea3436a9299490577bd1604")]
		[Dependencies(new string[] { "OnEntityDismounted" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseMountable", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseMountable_aae99d134ea3436a9299490577bd1604 : Patch
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
		[Identifier("a2cd2d7a4b1c49beabdc0edabf65c68c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HackableLockedCrate", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HackableLockedCrate_a2cd2d7a4b1c49beabdc0edabf65c68c : Patch
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
		[Identifier("32aae55cec574d22939400a928b51883")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HackableLockedCrate", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HackableLockedCrate_32aae55cec574d22939400a928b51883 : Patch
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
		[Identifier("67ce0243e2ed4e5780d68cc674bb5bf0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HackableLockedCrate", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HackableLockedCrate_67ce0243e2ed4e5780d68cc674bb5bf0 : Patch
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
		[Identifier("02e3c522d51a45c2ab7c26398a418adb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HackableLockedCrate", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HackableLockedCrate_02e3c522d51a45c2ab7c26398a418adb : Patch
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
		[Identifier("f41e767641f347b7a36e5855bfbe7790")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HackableLockedCrate", false)]
		[Parameter("info", "HitInfo", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HackableLockedCrate_f41e767641f347b7a36e5855bfbe7790 : Patch
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
		[Identifier("e81a4bf8acf64549a2e908d869ef9441")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CH47HelicopterAIController", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CH47HelicopterAIController_e81a4bf8acf64549a2e908d869ef9441 : Patch
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
		[Identifier("9e28c69d4e844a37a454d53932dea752")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseArcadeMachine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("score", "System.Int32", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseArcadeMachine_9e28c69d4e844a37a454d53932dea752 : Patch
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
		[Identifier("3bbba9a306c44822b9a559c09413d8b6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BasePlayer_3bbba9a306c44822b9a559c09413d8b6 : Patch
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
		[Identifier("e58e62245e1a40f7b83eb9a62adb816d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BasePlayer_e58e62245e1a40f7b83eb9a62adb816d : Patch
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
		[Identifier("bfee43b80a554f44880fe586a647ef67")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("ent", "BaseNetworkable", false)]
		[Parameter("self", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BasePlayer_bfee43b80a554f44880fe586a647ef67 : Patch
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
		[Identifier("855cf435b80848aaa55f862c7e21ceea")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("entity", "StashContainer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BasePlayer_855cf435b80848aaa55f862c7e21ceea : Patch
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
		[Identifier("a0fe20a45f4b4209b995a8aa09f39b8a")]
		[Dependencies(new string[] { "CanSeeStash" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("entity", "StashContainer", false)]
		[Parameter("self", "BasePlayer", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BasePlayer_a0fe20a45f4b4209b995a8aa09f39b8a : Patch
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
		[Identifier("78532468f0df4d638747102691b3ce22")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SamSite", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SamSite_78532468f0df4d638747102691b3ce22 : Patch
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
		[Identifier("0b3beecc726145f5871b64a284d2eada")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SamSite", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "System.Boolean", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SamSite_0b3beecc726145f5871b64a284d2eada : Patch
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
		[Identifier("47c93d541c7a4db0b49a5f478b5a0907")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SamSite_47c93d541c7a4db0b49a5f478b5a0907 : Patch
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
				CodeInstruction obj = list2[120];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[117]), list2[117]);
				}
				list2.InsertRange(117, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnSamSiteTargetScan", "OnSamSiteTargetScan", "SamSite", "TargetScan", new string[] { })]
		[Identifier("05672540d39c4572b439b0cbeea069eb")]
		[Dependencies(new string[] { "OnSamSiteTarget" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SamSite_05672540d39c4572b439b0cbeea069eb : Patch
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
				CodeInstruction obj = list2[79];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[66]), list2[66]);
				}
				list2.InsertRange(66, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Entity_ElectricSwitch
	{
		[Patch("OnSwitchToggle", "OnSwitchToggle [ElectricSwitch]", "ElectricSwitch", "RPC_Switch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("f5a7a553b5f942ed94a21242ac5481e0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ElectricSwitch", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ElectricSwitch_f5a7a553b5f942ed94a21242ac5481e0 : Patch
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
		[Identifier("02afafb50fa64ceaa79d18ef8564d870")]
		[Dependencies(new string[] { "OnSwitchToggle [ElectricSwitch]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ElectricSwitch", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ElectricSwitch_02afafb50fa64ceaa79d18ef8564d870 : Patch
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
		[Identifier("8def5ce3e59d4f49953425db2011396e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ResourceEntity_8def5ce3e59d4f49953425db2011396e : Patch
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
		[Identifier("968f1c4a0a50466ca7277527cf236215")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ResourceEntity", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ResourceEntity_968f1c4a0a50466ca7277527cf236215 : Patch
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
		[Identifier("691c282dcf8e4d4cbc81837aa9a62c4d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SupplyDrop", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SupplyDrop_691c282dcf8e4d4cbc81837aa9a62c4d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 50)
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
		[Identifier("4666649a51e244d196f30b1eb1f64d89")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TriggerComfort", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_TriggerComfort_4666649a51e244d196f30b1eb1f64d89 : Patch
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
		[Identifier("59a5f7b35085484bb848e6aea6f49066")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TriggerComfort", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_TriggerComfort_59a5f7b35085484bb848e6aea6f49066 : Patch
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
		[Identifier("b61c7afdbe2a4eb7a342985d1c635e05")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "StabilityEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_StabilityEntity_b61c7afdbe2a4eb7a342985d1c635e05 : Patch
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

	public class Entity_DieselEngine
	{
		[Patch("OnDieselEngineToggled", "OnDieselEngineToggled [off]", "DieselEngine", "EngineOff", new string[] { })]
		[Identifier("04a0f730a2d84d058c8c9c246ef06cfc")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DieselEngine", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DieselEngine_04a0f730a2d84d058c8c9c246ef06cfc : Patch
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
		[Identifier("286988865bd34307a667ad8a5c248685")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DieselEngine", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DieselEngine_286988865bd34307a667ad8a5c248685 : Patch
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
		[Identifier("36fd8f1b88bc49cd88644d3b0262117b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DieselEngine", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DieselEngine_36fd8f1b88bc49cd88644d3b0262117b : Patch
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
		[Patch("OnBuildingPrivilege", "OnBuildingPrivilege", "BaseEntity", "GetBuildingPrivilege", new string[] { "OBB", "System.Boolean", "System.Single", "BuildingPrivlidge" })]
		[Identifier("f37b28b4cb004f2bae8cfc98946e53b5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseEntity", false)]
		[Return(typeof(BuildingPrivlidge))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseEntity_f37b28b4cb004f2bae8cfc98946e53b5 : Patch
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
		[Identifier("21d84db383704a18aa1242b94c6f345a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseEntity_21d84db383704a18aa1242b94c6f345a : Patch
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
		[Identifier("9581cfba6d6b4749bdf9007af84fa649")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CargoShip", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CargoShip_9581cfba6d6b4749bdf9007af84fa649 : Patch
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
		[Identifier("e3c9568e5e684c099e4ab7c0fb6c8e71")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CargoShip", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CargoShip_e3c9568e5e684c099e4ab7c0fb6c8e71 : Patch
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
		[Identifier("ec99188db65b4373b5690d5cc5400823")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CargoShip", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CargoShip_ec99188db65b4373b5690d5cc5400823 : Patch
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
		[Identifier("60e87d3e702a4440b50b54698f4ca145")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CargoShip", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CargoShip_60e87d3e702a4440b50b54698f4ca145 : Patch
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
		[Identifier("0bb058d5226b4c4e9306dbc5ad4a1559")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CargoShip", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_CargoShip_0bb058d5226b4c4e9306dbc5ad4a1559 : Patch
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
		[Identifier("641cafde1e684e04b801c0cf048f4853")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BradleyAPC_641cafde1e684e04b801c0cf048f4853 : Patch
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
		[Identifier("7a66bf79b1bb4dbdb92061d9b7a3f26b")]
		[Dependencies(new string[] { "OnEntityDestroy [BradleyAPC]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Parameter("local14", "BaseEntity", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BradleyAPC_7a66bf79b1bb4dbdb92061d9b7a3f26b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 287)
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
		[Identifier("e0e9a49d09524fe7990baaa7b22c59fe")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FuelGenerator", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_FuelGenerator_e0e9a49d09524fe7990baaa7b22c59fe : Patch
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
		[Identifier("c96064a9b7e94bf5b0c0f33589d76653")]
		[Dependencies(new string[] { "OnSwitchToggle [FuelGenerator]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FuelGenerator", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_FuelGenerator_c96064a9b7e94bf5b0c0f33589d76653 : Patch
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
		[Identifier("01cdfed1b22648af8d78bd6532417334")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("ent", "BaseEntity", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("id", "System.UInt32", false)]
		[Parameter("debugName", "System.String", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseEntityRPCServerIsActiveItem_01cdfed1b22648af8d78bd6532417334 : Patch
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
		[Identifier("acdbd5b4fc024823a526dbd4f3b75968")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("ent", "BaseEntity", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("id", "System.UInt32", false)]
		[Parameter("debugName", "System.String", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseEntityRPCServerFromOwner_acdbd5b4fc024823a526dbd4f3b75968 : Patch
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
		[Identifier("8021c7fdf3d7498fb0afe6dd9ac2f222")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("ent", "BaseEntity", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("id", "System.UInt32", false)]
		[Parameter("debugName", "System.String", false)]
		[Parameter("maximumDistance", "System.Single", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseEntityRPCServerIsVisible_8021c7fdf3d7498fb0afe6dd9ac2f222 : Patch
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
		[Identifier("5115bc1edbdc45e89bb966670579c22b")]
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
		public class Entity_BaseEntityRPCServerMaxDistance_5115bc1edbdc45e89bb966670579c22b : Patch
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
		[Identifier("7a75939afdd04d6b80e6af1db588dd4b")]
		[Dependencies(new string[] { "CanHideStash" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "StashContainer", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_StashContainer_7a75939afdd04d6b80e6af1db588dd4b : Patch
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
		[Identifier("c622a93c6fa14d5c98acf97230823bf4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "StashContainer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_StashContainer_c622a93c6fa14d5c98acf97230823bf4 : Patch
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
		[Identifier("bd4e71fd10c14feb9a1d6bbc48f4def3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MixingTable", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_MixingTable_bd4e71fd10c14feb9a1d6bbc48f4def3 : Patch
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
		[Identifier("ebdd2171bb5b4b45b5ab83327b673ff9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "MixingTable", false)]
		[Parameter("self1", "MixingTable", false)]
		[Parameter("recipe", "Recipe", false)]
		[Parameter("quantity", "System.Int32", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_MixingTable_ebdd2171bb5b4b45b5ab83327b673ff9 : Patch
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
		[Identifier("ca7f12555cdd416caa3a2bf54312ce0a")]
		[Dependencies(new string[] { "OnSleepingBagDestroy" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "SleepingBag", false)]
		[Parameter("userID", "System.UInt64", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SleepingBag_ca7f12555cdd416caa3a2bf54312ce0a : Patch
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
		[Identifier("6e981d23b20e4a27b1da47ff89477255")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SleepingBag", false)]
		[Parameter("playerID", "System.UInt64", false)]
		[Parameter("ignoreTimers", "System.Boolean", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SleepingBag_6e981d23b20e4a27b1da47ff89477255 : Patch
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
		[Identifier("d369002d77bd4d829a8de92fd6125f45")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SleepingBag", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SleepingBag_d369002d77bd4d829a8de92fd6125f45 : Patch
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
		[Identifier("77adf393b0934169b4b1926ba864416b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SleepingBag_77adf393b0934169b4b1926ba864416b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Expected O, but got Unknown
				//IL_005b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0065: Expected O, but got Unknown
				//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b7: Expected O, but got Unknown
				//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00dc: Expected O, but got Unknown
				//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ed: Expected O, but got Unknown
				//IL_012b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0135: Expected O, but got Unknown
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
				CodeInstruction obj = list2[37];
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
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Entity_SurveyCrater
	{
		[Patch("OnAnalysisComplete", "OnAnalysisComplete", "SurveyCrater", "AnalysisComplete", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("e4f4570d67174537916dbc5a882d5da4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SurveyCrater", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SurveyCrater_e4f4570d67174537916dbc5a882d5da4 : Patch
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
		[Identifier("870191f563c24d7b95b96e615302566c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HotAirBalloon", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HotAirBalloon_870191f563c24d7b95b96e615302566c : Patch
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
		[Identifier("f8cc6c8e6ccf42848c454b7fb8bb59ea")]
		[Dependencies(new string[] { "OnHotAirBalloonToggle" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HotAirBalloon", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HotAirBalloon_f8cc6c8e6ccf42848c454b7fb8bb59ea : Patch
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
		[Identifier("274e684dbd9b4904a88981c52fc8b703")]
		[Dependencies(new string[] { "OnHotAirBalloonToggled [on]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HotAirBalloon", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_HotAirBalloon_274e684dbd9b4904a88981c52fc8b703 : Patch
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
		[Identifier("329ea5c6451348338e220f6815ee4ff3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ReactiveTarget", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ReactiveTarget_329ea5c6451348338e220f6815ee4ff3 : Patch
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
		[Identifier("50814847784f44d5bf4d4e8bc03c3705")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BaseEntity", false)]
		[Parameter("self", "SupplySignal", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SupplySignal_50814847784f44d5bf4d4e8bc03c3705 : Patch
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
		[Identifier("892744f401f34c46b83ccae78e58a872")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "WaterPurifier", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_WaterPurifier_892744f401f34c46b83ccae78e58a872 : Patch
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
		[Identifier("b1c11be5501b4011a89d667d236810e5")]
		[Dependencies(new string[] { "OnWaterPurify" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "WaterPurifier", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_WaterPurifier_b1c11be5501b4011a89d667d236810e5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 180)
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
		[Identifier("c0b78e5949524a6da587a512a4702a60")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "WaterCatcher", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_WaterCatcher_c0b78e5949524a6da587a512a4702a60 : Patch
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
		[Identifier("2291f0b1b8f845a1ba57247a09e6be02")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseLiquidVessel", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local3", "LiquidContainer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BaseLiquidVessel_2291f0b1b8f845a1ba57247a09e6be02 : Patch
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
		[Identifier("376fc55617c343b89212949b8c018f23")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DecayEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DecayEntity_376fc55617c343b89212949b8c018f23 : Patch
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
		[Identifier("39bee4d0e2084063946b94637bf190e5")]
		[Dependencies(new string[] { "OnDecayHeal" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DecayEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DecayEntity_39bee4d0e2084063946b94637bf190e5 : Patch
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
		[Identifier("d0e87abab6cf4e92bb357fe4ed43612b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DecayEntity", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_DecayEntity_d0e87abab6cf4e92bb357fe4ed43612b : Patch
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
		[Identifier("832dd38cbe614c2787ed4ab35132e7e6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ElectricWindmill", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ElectricWindmill_832dd38cbe614c2787ed4ab35132e7e6 : Patch
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
		[Identifier("d0a5398e499c45dfafce3438c1b7b350")]
		[Dependencies(new string[] { "OnWindmillUpdate" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ElectricWindmill", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ElectricWindmill_d0a5398e499c45dfafce3438c1b7b350 : Patch
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
		[Identifier("6d3dc69002f34010809012718246dfa5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Mannequin", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_Mannequin_6d3dc69002f34010809012718246dfa5 : Patch
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
		[Identifier("cb70517979684c1799380f9f280be5e9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Mannequin", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_Mannequin_cb70517979684c1799380f9f280be5e9 : Patch
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
		[Identifier("3894fb5711f64524a25d81d0cd53ea1d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "WaterPump", false)]
		[Parameter("local0", "ItemDefinition", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_WaterPump_3894fb5711f64524a25d81d0cd53ea1d : Patch
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
		[Identifier("93e8b1e7391c4d298672cb81dd1c3104")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Sprinkler", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_Sprinkler_93e8b1e7391c4d298672cb81dd1c3104 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 335)
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
		[Identifier("1d7df6c399c44079ad07aaf10b822365")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("liquidDef", "ItemDefinition", false)]
		[Parameter("position", "UnityEngine.Vector3", false)]
		[Parameter("radius", "System.Single", false)]
		[Parameter("amount", "System.Int32", false)]
		[Parameter("funWater", "System.Boolean", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_WaterBall_1d7df6c399c44079ad07aaf10b822365 : Patch
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
		[Identifier("c4a3475ae26b4ec49cc56cc25fd0012a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local5", "PhotoEntity", false)]
		[Parameter("local1", "Item", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "System.Byte[]", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_InstantCameraTool_c4a3475ae26b4ec49cc56cc25fd0012a : Patch
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
		[Identifier("a25c683638564b6285183d7b75f74f24")]
		[Dependencies(new string[] { "OnPhotoCapture" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local5", "PhotoEntity", false)]
		[Parameter("local1", "Item", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "System.Byte[]", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_InstantCameraTool_a25c683638564b6285183d7b75f74f24 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 232)
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
		[Identifier("4765064079864bb7af8ead183624d4b3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TreeEntity", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_TreeEntity_4765064079864bb7af8ead183624d4b3 : Patch
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
		[Identifier("e94e71d56b6b4a0b841e4234e80ff9ed")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SprayCanSpray", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_SprayCanSpray_e94e71d56b6b4a0b841e4234e80ff9ed : Patch
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

	public class Entity_Composter
	{
		[Patch("OnComposterUpdate", "OnComposterUpdate", "Composter", "UpdateComposting", new string[] { })]
		[Identifier("82926d7a719f4d028ae1b390bf0dec19")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Composter", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_Composter_82926d7a719f4d028ae1b390bf0dec19 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2008667508)), instruction), instruction);
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

	public class Entity_PoweredRemoteControlEntity
	{
		[Patch("OnRemoteIdentifierUpdate", "OnRemoteIdentifierUpdate", "PoweredRemoteControlEntity", "UpdateIdentifier", new string[] { "System.String", "System.Boolean" })]
		[Identifier("f9850539454c4ce4b1e76a3ff7b3869e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PoweredRemoteControlEntity", false)]
		[Parameter("newID", "System.String", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_PoweredRemoteControlEntity_f9850539454c4ce4b1e76a3ff7b3869e : Patch
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
		[Identifier("a6b9579103df40918ce679160904954e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "IndustrialConveyor", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_IndustrialConveyor_a6b9579103df40918ce679160904954e : Patch
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
		[Identifier("79ba788babec49809044f3469ac1a1c1")]
		[Dependencies(new string[] { "OnSwitchToggle [IndustrialConveyor]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "IndustrialConveyor", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_IndustrialConveyor_79ba788babec49809044f3469ac1a1c1 : Patch
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
		[Identifier("f44bc2615ac1404e8cb084bb7e3794b3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TimedExplosive", false)]
		[Return(typeof(bool))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_TimedExplosive_f44bc2615ac1404e8cb084bb7e3794b3 : Patch
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
		[Identifier("6bae7fd4d7084f16b125f04d48ca677c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopter", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_PatrolHelicopter_6bae7fd4d7084f16b125f04d48ca677c : Patch
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
		[Identifier("a5771099c05740dc999a15fbac1caeb3")]
		[Dependencies(new string[] { "OnPatrolHelicopterTakeDamage" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopter", false)]
		[Parameter("info", "HitInfo", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_PatrolHelicopter_a5771099c05740dc999a15fbac1caeb3 : Patch
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
		[Identifier("f757378ea1c442ca9df532e3fbd45e3c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopter", false)]
		[Parameter("local14", "BaseEntity", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_PatrolHelicopter_f757378ea1c442ca9df532e3fbd45e3c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 288)
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
		[Identifier("f2bc823dce82419c973a3e7e3e289830")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlanterBox", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_PlanterBox_f2bc823dce82419c973a3e7e3e289830 : Patch
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
		[Identifier("86e116bce69e472388e6f7b7cde48aeb")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopterAI", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_PatrolHelicopterAI_86e116bce69e472388e6f7b7cde48aeb : Patch
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
		[Identifier("cede3d125e314af3ac58bffbee816f2f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TriggeredEventPrefab", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_TriggeredEventPrefab_cede3d125e314af3ac58bffbee816f2f : Patch
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
		[Identifier("56cb3a8fd1e2481fb48cdced5de53573")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "WorldItem", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_WorldItem_56cb3a8fd1e2481fb48cdced5de53573 : Patch
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
		[Identifier("c825076df3a541aaab3bdaef79ec43c5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FreeableLootContainer", false)]
		[Return(typeof(void))]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_FreeableLootContainer_c825076df3a541aaab3bdaef79ec43c5 : Patch
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
		[Identifier("e48e13f793774ac7ab222c8a7aaa7eba")]
		[Dependencies(new string[] { "OnFreeableContainerRelease" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FreeableLootContainer", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_FreeableLootContainer_e48e13f793774ac7ab222c8a7aaa7eba : Patch
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
		[Identifier("62e4cc82b8894f3e864eda3bd9d6167f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FreeableLootContainer", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_FreeableLootContainer_62e4cc82b8894f3e864eda3bd9d6167f : Patch
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
		[Identifier("f5b0ddab86654f6aae48fdd30bd7ed41")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "System.Collections.Generic.List`1[ServerGib]", false)]
		[Parameter("creator", "UnityEngine.GameObject", false)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_ServerGib_f5b0ddab86654f6aae48fdd30bd7ed41 : Patch
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

	public class Entity_BigWheelGame
	{
		[Patch("OnBigWheelLoss", "OnBigWheelLoss", "BigWheelGame", "Payout", new string[] { })]
		[Identifier("cc166f6081c04a1dbdaf394b13a3852b")]
		[Dependencies(new string[] { "OnBigWheelWin" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BigWheelGame_cc166f6081c04a1dbdaf394b13a3852b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c3: Expected O, but got Unknown
				//IL_00de: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e8: Expected O, but got Unknown
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
				CodeInstruction obj = list2[107];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[93]), list2[93]);
				}
				list2.InsertRange(93, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnBigWheelWin", "OnBigWheelWin", "BigWheelGame", "Payout", new string[] { })]
		[Identifier("99c4180945fe4ab3a5e368bcdbc900c3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Entity")]
		[Assembly("Assembly-CSharp.dll")]
		public class Entity_BigWheelGame_99c4180945fe4ab3a5e368bcdbc900c3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_008b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0095: Expected O, but got Unknown
				//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
				//IL_0101: Expected O, but got Unknown
				//IL_011c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0126: Expected O, but got Unknown
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
				CodeInstruction obj = list2[75];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[43]), list2[43]);
				}
				list2.InsertRange(43, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
