using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Primitive
{
	public class Primitive_BallistaGun
	{
		[Patch("OnBallistaGunReload", "OnBallistaGunReload", "BallistaGun", "SERVER_ReloadStart", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("68c2f134f77f4ac686f1c1b8a52cd74d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BallistaGun", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Primitive")]
		[Assembly("Assembly-CSharp.dll")]
		public class Primitive_BallistaGun_68c2f134f77f4ac686f1c1b8a52cd74d : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1300664478)), instruction);
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

	public class Primitive_Catapult
	{
		[Patch("OnCatapultFireForce", "OnCatapultFireForce", "Catapult", "Fire", new string[] { "BasePlayer", "System.Single" })]
		[Identifier("bd30298e3e2749618afb9390d049035d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Catapult", false)]
		[Parameter("shooter", "BasePlayer", false)]
		[Parameter("local1", "System.Single", false)]
		[Category("Primitive")]
		[Assembly("Assembly-CSharp.dll")]
		public class Primitive_Catapult_bd30298e3e2749618afb9390d049035d : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1397907876)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Single"));
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					instruction.labels.Add(label1);
					object retvar = Generator.DeclareLocal(typeof(object));
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(float));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(float));
					yield return __GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 1, typeof(float));
					yield return instruction;
				}
			}
		}

		[Patch("OnSiegeWeaponFire", "OnSiegeWeaponFire [Catapult]", "Catapult", "SERVER_WantsFire", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("6761852ab4bf4e5c89b92b67a42d96aa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Catapult", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Primitive")]
		[Assembly("Assembly-CSharp.dll")]
		public class Primitive_Catapult_6761852ab4bf4e5c89b92b67a42d96aa : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-445461974)), instruction), instruction);
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

	public class Primitive_BatteringRam
	{
		[Patch("OnSiegeWeaponFire", "OnSiegeWeaponFire [BatteringRam]", "BatteringRam", "SERVER_WantsAttack", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("ce01dba83d1f4285b5da6608eee19a23")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BatteringRam", false)]
		[Parameter("local1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Primitive")]
		[Assembly("Assembly-CSharp.dll")]
		public class Primitive_BatteringRam_ce01dba83d1f4285b5da6608eee19a23 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-445461974)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}

		[Patch("OnSiegeWeaponDoorOpen", "OnSiegeWeaponDoorOpen [BatteringRam]", "BatteringRam", "RPC_OpenDoor", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("13bf6a74b8614502a3862a1de3067fba")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BatteringRam", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Primitive")]
		[Assembly("Assembly-CSharp.dll")]
		public class Primitive_BatteringRam_13bf6a74b8614502a3862a1de3067fba : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1497098116), instruction), instruction);
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

		[Patch("OnSiegeWeaponDoorClose", "OnSiegeWeaponDoorClose [BatteringRam]", "BatteringRam", "RPC_CloseDoor", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("0c3810ea65474e7b8c33de01a79ce8e6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BatteringRam", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Primitive")]
		[Assembly("Assembly-CSharp.dll")]
		public class Primitive_BatteringRam_0c3810ea65474e7b8c33de01a79ce8e6 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1630694525)), instruction), instruction);
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

	public class Primitive_BaseSiegeWeapon
	{
		[Patch("OnSiegeWeaponPull", "OnSiegeWeaponPull", "BaseSiegeWeapon", "SERVER_StartPulling", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("f47eb8b4eab14cdaa2526816e3bf7791")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseSiegeWeapon", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Primitive")]
		[Assembly("Assembly-CSharp.dll")]
		public class Primitive_BaseSiegeWeapon_f47eb8b4eab14cdaa2526816e3bf7791 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1848731217)), instruction), instruction);
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
}
