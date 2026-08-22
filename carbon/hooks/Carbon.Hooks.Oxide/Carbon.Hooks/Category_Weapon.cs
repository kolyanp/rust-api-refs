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
		[Identifier("10ece424346c4528a2f1a3316ccc6f05")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local4", "BaseEntity", false)]
		[Parameter("self", "ThrownWeapon", false)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_ThrownWeapon_10ece424346c4528a2f1a3316ccc6f05 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 44)
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
		[Identifier("6498b157f330406eabbeb94ac22be1c4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local2", "BaseEntity", false)]
		[Parameter("self", "ThrownWeapon", false)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_ThrownWeapon_6498b157f330406eabbeb94ac22be1c4 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 151)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)565209634), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
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
		[Identifier("972f0b3ff8bb43e89d833b5518070655")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "Item", false)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseMelee_972f0b3ff8bb43e89d833b5518070655 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 263)
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
		[Identifier("84e10b67483a4158b5e598651023fa28")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local8", "BaseEntity", false)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseLauncher_84e10b67483a4158b5e598651023fa28 : Patch
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
		[Patch("OnWeaponFired", "OnWeaponFired", "BaseProjectile", "CLProject", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("d0ae7e1d1b1743e18c7b9126e3da20b0")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseProjectile", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local3", "ItemModProjectile", false)]
		[Parameter("local2", "ProtoBuf.ProjectileShoot", false)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseProjectile_d0ae7e1d1b1743e18c7b9126e3da20b0 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 148)
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

		[Patch("OnWeaponReload", "OnWeaponReload", "BaseProjectile", "StartReload", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("5fcbd13fb0fe4b728fa4910b4823356d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseProjectile", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseProjectile_5fcbd13fb0fe4b728fa4910b4823356d : Patch
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
		[Identifier("1c073596f03746fcb77056415db93267")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseProjectile", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local2", "ItemDefinition", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseProjectile_1c073596f03746fcb77056415db93267 : Patch
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
		[Identifier("8cf254904e7440f7a22213a5ed3d507b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BaseProjectile", false)]
		[Parameter("item", "Item", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseProjectile_8cf254904e7440f7a22213a5ed3d507b : Patch
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
		[Identifier("0407889f4e494a6d87702e24456b32b9")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseProjectile", false)]
		[Parameter("self1", "BaseProjectile", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseProjectile_0407889f4e494a6d87702e24456b32b9 : Patch
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
		[Identifier("7215e5c8e6754d25964783f74c68039e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseProjectile", false)]
		[Parameter("ammoSource", "IAmmoContainer", false)]
		[Parameter("self1", "BaseProjectile", false)]
		[Return(typeof(bool))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseProjectile_7215e5c8e6754d25964783f74c68039e : Patch
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
		[Identifier("93535986095845769addf2452b42999e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("info", "HitInfo", false)]
		[Parameter("itemDef", "ItemDefinition", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BasePlayer_93535986095845769addf2452b42999e : Patch
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
		[Identifier("bfe7fc5feda947d6a1b327cf5ce224e3")]
		[Dependencies(new string[] { "CanCreateWorldProjectile" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("info", "HitInfo", false)]
		[Parameter("local1", "Item", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BasePlayer_bfe7fc5feda947d6a1b327cf5ce224e3 : Patch
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
		[Identifier("5de67df7eb284661bacdd7ef288c36c8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BasePlayer", false)]
		[Parameter("local0", "ProtoBuf.PlayerProjectileRicochet", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BasePlayer_5de67df7eb284661bacdd7ef288c36c8 : Patch
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
		[Identifier("2044001429d5423887b16c98c8d472a7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FlameThrower", false)]
		[Parameter("local13", "BaseEntity", false)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_FlameThrower_2044001429d5423887b16c98c8d472a7 : Patch
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
		[Identifier("6586f762c8bd40ffb059afee0c335e48")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FireBall", false)]
		[Parameter("local4", "BaseCombatEntity", false)]
		[Parameter("local2", "HitInfo", false)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_FireBall_6586f762c8bd40ffb059afee0c335e48 : Patch
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
		[Identifier("cfa5891ef34d40b19f3a9c997541c7f5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FireBall", false)]
		[Parameter("local1", "BaseEntity", false)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_FireBall_cfa5891ef34d40b19f3a9c997541c7f5 : Patch
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
		[Identifier("b6870b36baaf40009f323111f904c7a2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TimedExplosive", false)]
		[Parameter("fuseLength", "System.Single", false)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_TimedExplosive_b6870b36baaf40009f323111f904c7a2 : Patch
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
		[Identifier("865cdaf9639f4f52af0d62faab64b1bd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "TimedExplosive", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_TimedExplosive_865cdaf9639f4f52af0d62faab64b1bd : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 131)
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
		[Identifier("99cc76e5ba474d7d9fb936eb6171a99c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "FlameExplosive", false)]
		[Parameter("local1", "UnityEngine.Collider", false)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_FlameExplosive_99cc76e5ba474d7d9fb936eb6171a99c : Patch
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
		[Identifier("9b578e59f7654db8acc6922600fb6bc4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("info", "HitInfo", false)]
		[Parameter("customEffect", "System.String", false)]
		[Return(typeof(void))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_Effectserver_9b578e59f7654db8acc6922600fb6bc4 : Patch
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
		[Identifier("ff53378ad0834d92acd7b2c0fe202b57")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "LiquidWeapon", false)]
		[Return(typeof(bool))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_LiquidWeapon_ff53378ad0834d92acd7b2c0fe202b57 : Patch
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
		[Identifier("0fc29e236dcc4ea09592e1862ad95524")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "LiquidWeapon", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_LiquidWeapon_0fc29e236dcc4ea09592e1862ad95524 : Patch
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
		[Identifier("80ec1ac1779644f194225f1e73fa3740")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "LiquidWeapon", false)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_LiquidWeapon_80ec1ac1779644f194225f1e73fa3740 : Patch
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
		[Identifier("94451bd7d36f4ba49c4f435174ec2b33")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseHelicopter", false)]
		[Return(typeof(bool))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_BaseHelicopter_94451bd7d36f4ba49c4f435174ec2b33 : Patch
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
		[Identifier("a05db94421da4ab39efffa13816b552a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CH47Helicopter", false)]
		[Return(typeof(bool))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_CH47Helicopter_a05db94421da4ab39efffa13816b552a : Patch
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
		[Identifier("a37d5879c0184ebbb39ea13645c418c2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PlayerHelicopter", false)]
		[Return(typeof(bool))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_PlayerHelicopter_a37d5879c0184ebbb39ea13645c418c2 : Patch
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
		[Identifier("1df3269ecaae43bf9c9d42bc9cfb1cad")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PatrolHelicopter", false)]
		[Return(typeof(bool))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_PatrolHelicopter_1df3269ecaae43bf9c9d42bc9cfb1cad : Patch
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
		[Identifier("82f678e4bc904f08a58d0944205df41a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HeliPilotFlare", false)]
		[Return(typeof(bool))]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_HeliPilotFlare_82f678e4bc904f08a58d0944205df41a : Patch
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
		[Identifier("008a565a97e14003a0dbb11c49634eac")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Weapon")]
		[Assembly("Assembly-CSharp.dll")]
		public class Weapon_DudTimedExplosive_008a565a97e14003a0dbb11c49634eac : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_006e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0078: Expected O, but got Unknown
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Expected O, but got Unknown
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
				CodeInstruction obj = list2[21];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[18]), list2[18]);
				}
				list2.InsertRange(18, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
