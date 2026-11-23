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

public class Category_Weapon
{
	public class Weapon_ThrownWeapon
	{
		[Patch("OnExplosiveThrown", "OnExplosiveThrown", "ThrownWeapon", "DoThrow", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("3c20d906f8d5483a9360021e639a48c6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local4", "BaseEntity", false)]
		[Parameter("self", "ThrownWeapon", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_ThrownWeapon_3c20d906f8d5483a9360021e639a48c6 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 55)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1930466752), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)4);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnExplosiveDropped", "OnExplosiveDropped", "ThrownWeapon", "DoDrop", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("7d771f7a86f742beaff0f954d28553a5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local3", "BaseEntity", false)]
		[Parameter("self", "ThrownWeapon", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_ThrownWeapon_7d771f7a86f742beaff0f954d28553a5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 138)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)565209634), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

	public class Weapon_BaseMelee
	{
		[Patch("OnMeleeThrown", "OnMeleeThrown", "BaseMelee", "CLProject", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("3af4ce84ac0d475fa2559a6b3d3849d7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "Item", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseMelee_3af4ce84ac0d475fa2559a6b3d3849d7 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 330)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2046277233)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

	public class Weapon_BaseLauncher
	{
		[Patch("OnRocketLaunched", "OnRocketLaunched", "BaseLauncher", "SV_Launch", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("e8009d62449e43c4871b7f885e6283fd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local8", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseLauncher_e8009d62449e43c4871b7f885e6283fd : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 236)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)658881068), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)8);
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

	public class Weapon_BaseProjectile
	{
		[Patch("OnWeaponFired", "OnWeaponFired", "BaseProjectile", "ProcessCLProject", new string[] { "BaseEntity/RPCMessage", "System.Boolean", "BaseMountable" })]
		[Identifier("d9ac1a75a28c43bf9ba8c4ec6347667e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseProjectile", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local3", "ItemModProjectile", false)]
		[Parameter("local2", "ProtoBuf.ProjectileShoot", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseProjectile_d9ac1a75a28c43bf9ba8c4ec6347667e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 146)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1841607624), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
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

		[Patch("OnWeaponReload", "OnWeaponReload", "BaseProjectile", "ProcessStartReload", new string[] { "BaseEntity/RPCMessage", "System.Boolean" })]
		[Identifier("2a142ab8ff7b43108b8466e8de306097")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseProjectile", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseProjectile_2a142ab8ff7b43108b8466e8de306097 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1223329598)), instruction), instruction);
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

		[Patch("OnAmmoSwitch", "OnAmmoSwitch", "BaseProjectile", "SwitchAmmoTo", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("6339f4dd193a49ec9a389189af7c8d0a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseProjectile", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "ItemDefinition", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseProjectile_6339f4dd193a49ec9a389189af7c8d0a : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-536677785)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
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

		[Patch("OnAmmoUnload", "OnAmmoUnload", "BaseProjectile", "UnloadAmmo", new string[] { "Item", "BasePlayer" })]
		[Identifier("757df3b446a14e088c8c097af012cb4d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BaseProjectile", false)]
		[Parameter("item", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseProjectile_757df3b446a14e088c8c097af012cb4d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-271174007)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
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

		[Patch("OnWeaponModChange", "OnWeaponModChange", "BaseProjectile", "DelayedModsChanged", new string[] { })]
		[Identifier("16cb9ed2b7164272a7b0478ba95e6f6d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseProjectile", false)]
		[Parameter("getOwnerPlayer", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseProjectile_16cb9ed2b7164272a7b0478ba95e6f6d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-768644827)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseProjectile"), "GetOwnerPlayer", (Type[])null, (Type[])null));
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

		[Patch("OnMagazineReload", "OnMagazineReload", "BaseProjectile", "TryReloadMagazine", new string[] { "IAmmoContainer", "System.Int32", "System.Boolean" })]
		[Identifier("f7747ee9b5094279aa9461b10f0db51c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseProjectile", false)]
		[Parameter("ammoSource", "IAmmoContainer", false)]
		[Parameter("getOwnerPlayer", "BasePlayer", false)]
		[Return(typeof(bool))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseProjectile_f7747ee9b5094279aa9461b10f0db51c : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1616594073)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseProjectile"), "GetOwnerPlayer", (Type[])null, (Type[])null));
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

	public class Weapon_BasePlayer
	{
		[Patch("CanCreateWorldProjectile", "CanCreateWorldProjectile", "BasePlayer", "CreateWorldProjectile", new string[] { "HitInfo", "ItemDefinition", "ItemModProjectile", "Projectile", "Item" })]
		[Identifier("70c2baece61d4383bef69b5342d978fa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("info", "HitInfo", false)]
		[Parameter("itemDef", "ItemDefinition", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BasePlayer_70c2baece61d4383bef69b5342d978fa : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1010290496)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
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

		[Patch("OnWorldProjectileCreate", "OnWorldProjectileCreate", "BasePlayer", "CreateWorldProjectile", new string[] { "HitInfo", "ItemDefinition", "ItemModProjectile", "Projectile", "Item" })]
		[Identifier("4c27744598da4d8db75b6fef952e04c2")]
		[Dependencies(new string[] { "CanCreateWorldProjectile" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("info", "HitInfo", false)]
		[Parameter("local1", "Item", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BasePlayer_4c27744598da4d8db75b6fef952e04c2 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-943530363)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

		[Patch("OnProjectileRicochet", "OnProjectileRicochet", "BasePlayer", "OnProjectileRicochet", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("478f62f6e5384486b795c2a11352ac5c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "ProtoBuf.PlayerProjectileRicochet", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BasePlayer_478f62f6e5384486b795c2a11352ac5c : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)533818454), instruction);
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

	public class Weapon_FlameThrower
	{
		[Patch("OnFlameThrowerBurn", "OnFlameThrowerBurn", "FlameThrower", "FlameTick", new string[] { })]
		[Identifier("2c2d4a2519a640b990cf40c6c39205d8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FlameThrower", false)]
		[Parameter("local13", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_FlameThrower_2c2d4a2519a640b990cf40c6c39205d8 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 240)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)637528194), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)13);
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

	public class Weapon_FireBall
	{
		[Patch("OnFireBallDamage", "OnFireBallDamage", "FireBall", "DoRadialDamage", new string[] { })]
		[Identifier("5c4f407709c34dd28e271209f90f9954")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FireBall", false)]
		[Parameter("local4", "BaseCombatEntity", false)]
		[Parameter("local2", "HitInfo", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_FireBall_5c4f407709c34dd28e271209f90f9954 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 138)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1371330456)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)4);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
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

		[Patch("OnFireBallSpread", "OnFireBallSpread", "FireBall", "TryToSpread", new string[] { })]
		[Identifier("462ecfbc89f943cbb47b598755d3629d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FireBall", false)]
		[Parameter("local1", "BaseEntity", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_FireBall_462ecfbc89f943cbb47b598755d3629d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 62)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1271344414)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

	public class Weapon_TimedExplosive
	{
		[Patch("OnExplosiveFuseSet", "OnExplosiveFuseSet", "TimedExplosive", "SetFuse", new string[] { "System.Single" })]
		[Identifier("81c8e6c7d7794d5f84129a0bed603556")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TimedExplosive", false)]
		[Parameter("fuseLength", "System.Single", false)]
		[Return(typeof(float), Continues = true)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_TimedExplosive_81c8e6c7d7794d5f84129a0bed603556 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1897039526), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
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
					yield return new CodeInstruction(OpCodes.Brfalse_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(float));
					yield return new CodeInstruction(OpCodes.Starg, (object)1);
					yield return instruction;
				}
			}
		}

		[Patch("OnTimedExplosiveExplode", "OnTimedExplosiveExplode", "TimedExplosive", "Explode", new string[] { "UnityEngine.Vector3" })]
		[Identifier("8f96f0ea0c6840aca0a866e903c5c118")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TimedExplosive", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_TimedExplosive_8f96f0ea0c6840aca0a866e903c5c118 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 127)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-164749751)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(Vector3));
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

	public class Weapon_FlameExplosive
	{
		[Patch("OnFlameExplosion", "OnFlameExplosion", "FlameExplosive", "FlameExplode", new string[] { "UnityEngine.Vector3" })]
		[Identifier("d06177f75009434ba96292ecae940ec3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FlameExplosive", false)]
		[Parameter("local1", "UnityEngine.Collider", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_FlameExplosive_d06177f75009434ba96292ecae940ec3 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)514808608), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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

	public class Weapon_Effectserver
	{
		[Patch("OnImpactEffectCreate", "OnImpactEffectCreate", "Effect/server", "ImpactEffect", new string[] { "HitInfo", "System.String" })]
		[Identifier("457e3a6ddf52447b8097ab01d60c02b5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("info", "HitInfo", false)]
		[Parameter("customEffect", "System.String", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_Effectserver_457e3a6ddf52447b8097ab01d60c02b5 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)880112741), instruction), instruction);
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

	public class Weapon_LiquidWeapon
	{
		[Patch("CanFireLiquidWeapon", "CanFireLiquidWeapon", "LiquidWeapon", "CanFire", new string[] { "BasePlayer" })]
		[Identifier("e25bd40043fb4c54aad995f1bc5941fc")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "LiquidWeapon", false)]
		[Return(typeof(bool))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_LiquidWeapon_e25bd40043fb4c54aad995f1bc5941fc : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1607663432)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnLiquidWeaponFired", "OnLiquidWeaponFired", "LiquidWeapon", "StartFiring", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("23bbc286f8824c0ba4ba252cfc1efbb0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "LiquidWeapon", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_LiquidWeapon_23bbc286f8824c0ba4ba252cfc1efbb0 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 65)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)266948780), instruction);
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

		[Patch("OnLiquidWeaponFiringStopped", "OnLiquidWeaponFiringStopped", "LiquidWeapon", "StopFiring", new string[] { })]
		[Identifier("aa77ac7ff88f49e68e3f7295202d0fd1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "LiquidWeapon", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_LiquidWeapon_aa77ac7ff88f49e68e3f7295202d0fd1 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1131649475)), instruction);
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

	public class Weapon_BaseHelicopter
	{
		[Patch("CanBeHomingTargeted", "CanBeHomingTargeted [BaseHelicopter]", "BaseHelicopter", "IsValidHomingTarget", new string[] { })]
		[Identifier("012cfac215dd435a853f78469b9a0797")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseHelicopter", false)]
		[Return(typeof(bool))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseHelicopter_012cfac215dd435a853f78469b9a0797 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1900309404)), instruction), instruction);
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

	public class Weapon_CH47Helicopter
	{
		[Patch("CanBeHomingTargeted", "CanBeHomingTargeted [CH47Helicopter]", "CH47Helicopter", "IsValidHomingTarget", new string[] { })]
		[Identifier("d09cd2bb192f4a75a84939c14437b105")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CH47Helicopter", false)]
		[Return(typeof(bool))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_CH47Helicopter_d09cd2bb192f4a75a84939c14437b105 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1900309404)), instruction), instruction);
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

	public class Weapon_PlayerHelicopter
	{
		[Patch("CanBeHomingTargeted", "CanBeHomingTargeted [PlayerHelicopter]", "PlayerHelicopter", "IsValidHomingTarget", new string[] { })]
		[Identifier("4a9b330c5e8348439534d5780e85d263")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerHelicopter", false)]
		[Return(typeof(bool))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_PlayerHelicopter_4a9b330c5e8348439534d5780e85d263 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1900309404)), instruction), instruction);
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

	public class Weapon_PatrolHelicopter
	{
		[Patch("CanBeHomingTargeted", "CanBeHomingTargeted [PatrolHelicopter]", "PatrolHelicopter", "IsValidHomingTarget", new string[] { })]
		[Identifier("3a8128e8cf81455d8fd6f7fe41112b72")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopter", false)]
		[Return(typeof(bool))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_PatrolHelicopter_3a8128e8cf81455d8fd6f7fe41112b72 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1900309404)), instruction), instruction);
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

	public class Weapon_HeliPilotFlare
	{
		[Patch("CanBeHomingTargeted", "CanBeHomingTargeted [HeliPilotFlare]", "HeliPilotFlare", "IsValidHomingTarget", new string[] { })]
		[Identifier("7f1a6f65c8a24be8991541f8d368992d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HeliPilotFlare", false)]
		[Return(typeof(bool))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_HeliPilotFlare_7f1a6f65c8a24be8991541f8d368992d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1900309404)), instruction), instruction);
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

	public class Weapon_DudTimedExplosive
	{
		[Patch("OnExplosiveDud", "OnExplosiveDud", "DudTimedExplosive", "Explode", new string[] { })]
		[Identifier("00a494cd15c64abcb28f2f7dd61711d4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_DudTimedExplosive_00a494cd15c64abcb28f2f7dd61711d4 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_006e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0078: Expected Obj, but got Unknown
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnExplosiveDud"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[2]
				{
					typeof(string),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[21];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[18]), list2[18]);
				}
				list2.InsertRange(18, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
