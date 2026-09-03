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
		[Identifier("e7e17c3f77d34bc0ab292fc8d63a251e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BallistaGun", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Primitive")]
		[Assembly("Assembly-CSharp.dll")]
		public class Primitive_BallistaGun_e7e17c3f77d34bc0ab292fc8d63a251e : Patch
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
		[Identifier("1e582d852baa4a498fb85b5ec1d228aa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Catapult", false)]
		[Parameter("shooter", "BasePlayer", false)]
		[Parameter("local1", "System.Single", false)]
		[Return(typeof(float), Continues = true)]
		[Category("Primitive")]
		[Assembly("Assembly-CSharp.dll")]
		public class Primitive_Catapult_1e582d852baa4a498fb85b5ec1d228aa : Patch
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
		[Identifier("b7755c8c7aed49d3920d63a71345635b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Catapult", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Primitive")]
		[Assembly("Assembly-CSharp.dll")]
		public class Primitive_Catapult_b7755c8c7aed49d3920d63a71345635b : Patch
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
		[Identifier("4faadcaedf3042e58c82cd00326258a2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BatteringRam", false)]
		[Parameter("local1", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Primitive")]
		[Assembly("Assembly-CSharp.dll")]
		public class Primitive_BatteringRam_4faadcaedf3042e58c82cd00326258a2 : Patch
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
		[Identifier("d1d524d4f3db43a3b674a878d0a1194f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BatteringRam", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Primitive")]
		[Assembly("Assembly-CSharp.dll")]
		public class Primitive_BatteringRam_d1d524d4f3db43a3b674a878d0a1194f : Patch
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
		[Identifier("fa5c3c4e9266458eb01f4143a332a68e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BatteringRam", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Primitive")]
		[Assembly("Assembly-CSharp.dll")]
		public class Primitive_BatteringRam_fa5c3c4e9266458eb01f4143a332a68e : Patch
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
		[Identifier("895fdb77b7344c9a843c46c06f7d42cd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseSiegeWeapon", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Primitive")]
		[Assembly("Assembly-CSharp.dll")]
		public class Primitive_BaseSiegeWeapon_895fdb77b7344c9a843c46c06f7d42cd : Patch
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
