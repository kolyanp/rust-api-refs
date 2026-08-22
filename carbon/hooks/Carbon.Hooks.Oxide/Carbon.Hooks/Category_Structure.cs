using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Core;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Structure
{
	public class Structure_BuildingBlock
	{
		[Patch("OnWallpaperSet", "OnWallpaperSet", "BuildingBlock", "SetWallpaper", new string[] { "System.UInt64", "System.Int32", "System.Single" })]
		[Identifier("25d2d0b7ad9a4854aaa8f811356d64fa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BuildingBlock", false)]
		[Parameter("id", "System.UInt64", false)]
		[Parameter("side", "System.Int32", false)]
		[Parameter("rotation", "System.Single", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_BuildingBlock_25d2d0b7ad9a4854aaa8f811356d64fa : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1953276601)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(float));
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

		[Patch("OnWallpaperRemove", "OnWallpaperRemove", "BuildingBlock", "RemoveWallpaper", new string[] { "System.Int32" })]
		[Identifier("dbbc999c7c42451aa2363d82f1a1e667")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BuildingBlock", false)]
		[Parameter("side", "System.Int32", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_BuildingBlock_dbbc999c7c42451aa2363d82f1a1e667 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1352098305)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(int));
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

		[Patch("OnStructureUpgrade", "OnStructureUpgrade", "BuildingBlock", "DoUpgradeToGrade", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("8b1d42d9de694166b57c46f52e6ca031")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BuildingBlock", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("type", "BuildingGrade+Enum", false)]
		[Parameter("skin", "System.UInt64", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_BuildingBlock_8b1d42d9de694166b57c46f52e6ca031 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1205776686), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("ConstructionGrade"), "gradeBase"));
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BuildingGrade"), "type"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("BuildingGrade+Enum"));
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("ConstructionGrade"), "gradeBase"));
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BuildingGrade"), "skin"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt64"));
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

		[Patch("OnStructureRotate", "OnStructureRotate", "BuildingBlock", "DoRotation", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("39cd9d3aef3041c58bb0cddce0b8b06d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BuildingBlock", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_BuildingBlock_39cd9d3aef3041c58bb0cddce0b8b06d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1838405871)), instruction), instruction);
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

		[Patch("CanChangeGrade", "CanChangeGrade", "BuildingBlock", "CanChangeToGrade", new string[] { "BuildingGrade/Enum", "System.UInt64", "BasePlayer" })]
		[Identifier("1445005576924150a05739a60a5d842a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BuildingBlock", false)]
		[Parameter("iGrade", "BuildingGrade+Enum", false)]
		[Parameter("iSkin", "System.UInt64", false)]
		[Return(typeof(bool))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_BuildingBlock_1445005576924150a05739a60a5d842a : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1953317500)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(Enum));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
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

		[Patch("CanAffordUpgrade", "CanAffordUpgrade", "BuildingBlock", "CanAffordUpgrade", new string[] { "BuildingGrade/Enum", "System.UInt64", "BasePlayer" })]
		[Identifier("368110a57c424f608b10698b7c285314")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BuildingBlock", false)]
		[Parameter("iGrade", "BuildingGrade+Enum", false)]
		[Parameter("iSkin", "System.UInt64", false)]
		[Return(typeof(bool))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_BuildingBlock_368110a57c424f608b10698b7c285314 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1599841557)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(Enum));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
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

		[Patch("OnPlayerPveDamage", "OnPlayerPveDamage [BuildingBlock]", "BuildingBlock", "Hurt", new string[] { "HitInfo" })]
		[Identifier("94bb3bdc82304d05b67cb30e4441983b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("initiator", "BaseEntity", false)]
		[Parameter("info", "HitInfo", false)]
		[Parameter("self", "BuildingBlock", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_BuildingBlock_94bb3bdc82304d05b67cb30e4441983b : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1273375130), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("HitInfo"), "Initiator"));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
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

		[Patch("OnStructureUpgraded", "OnStructureUpgraded", "BuildingBlock", "DoUpgradeToGrade", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("be62a0f0de1e48388266f546cbfe7760")]
		[Dependencies(new string[] { "OnStructureUpgrade" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BuildingBlock", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("type", "BuildingGrade+Enum", false)]
		[Parameter("skin", "System.UInt64", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_BuildingBlock_be62a0f0de1e48388266f546cbfe7760 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1926574503), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("ConstructionGrade"), "gradeBase"));
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BuildingGrade"), "type"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("BuildingGrade+Enum"));
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("ConstructionGrade"), "gradeBase"));
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BuildingGrade"), "skin"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt64"));
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

	public class Structure_DecayEntity
	{
		[Patch("OnStructureDemolish", "OnStructureDemolish [immediate = true]", "DecayEntity", "DoImmediateDemolish", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("7dcbc0ba3cd448e0b46e4559d7629e68")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DecayEntity", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_DecayEntity_7dcbc0ba3cd448e0b46e4559d7629e68 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-790381475)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
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

		[Patch("OnStructureDemolish", "OnStructureDemolish [immediate = false]", "DecayEntity", "DoDemolish", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("9028f68191f7456cae3094dc711674da")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DecayEntity", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_DecayEntity_9028f68191f7456cae3094dc711674da : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-790381475)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
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

		[Patch("CanDemolish", "CanDemolish", "DecayEntity", "CanDemolish", new string[] { "BasePlayer" })]
		[Identifier("f66f90d0f62745cf811eeb86335a4799")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "DecayEntity", false)]
		[Return(typeof(bool))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_DecayEntity_f66f90d0f62745cf811eeb86335a4799 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)334281728), instruction), instruction);
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
	}

	public class Structure_Signage
	{
		[Patch("OnSignLocked", "OnSignLocked [Signage]", "Signage", "LockSign", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("7a52108b1915442db130a70b202607ad")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Signage", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_Signage_7a52108b1915442db130a70b202607ad : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 25)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1270763772), instruction), instruction);
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

		[Patch("OnSignUpdated", "OnSignUpdated [Signage]", "Signage", "UpdateSign", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("7db6f1f8f45b4ccfa53b234cb73b75a5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Signage", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "System.Int32", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_Signage_7db6f1f8f45b4ccfa53b234cb73b75a5 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1659571272)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
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

	public class Structure_Door
	{
		[Patch("OnDoorOpened", "OnDoorOpened", "Door", "RPC_OpenDoor", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("2634bc0bc8c3440795803b897ebb2af1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Door", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_Door_2634bc0bc8c3440795803b897ebb2af1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 165)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)449010576), instruction), instruction);
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

		[Patch("OnDoorClosed", "OnDoorClosed", "Door", "RPC_CloseDoor", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("72634aadf3254f38b8282d16b47627ed")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Door", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_Door_72634aadf3254f38b8282d16b47627ed : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1955326364), instruction), instruction);
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

		[Patch("OnDoorKnocked", "OnDoorKnocked [Door]", "Door", "RPC_KnockDoor", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("486176dd20dc458dbe9a1676d7f3bcc3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Door", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_Door_486176dd20dc458dbe9a1676d7f3bcc3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 53)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)640250473), instruction), instruction);
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

	public class Structure_Hammer
	{
		[Patch("OnHammerHit", "OnHammerHit", "Hammer", "DoAttackShared", new string[] { "HitInfo" })]
		[Identifier("f5092eec06264d81a904f6e73e72746c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("info", "HitInfo", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_Hammer_f5092eec06264d81a904f6e73e72746c : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-65001434)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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
	}

	public class Structure_BaseCombatEntity
	{
		[Patch("OnStructureRepair", "OnStructureRepair", "BaseCombatEntity", "DoRepair", new string[] { "BasePlayer" })]
		[Identifier("64616f2248dc4a0485a84e0277192fd1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseCombatEntity", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_BaseCombatEntity_64616f2248dc4a0485a84e0277192fd1 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1586842410), instruction);
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
	}

	public class Structure_BuildingPrivlidge
	{
		[Patch("OnCupboardDeauthorize", "OnCupboardDeauthorize", "BuildingPrivlidge", "RemoveSelfAuthorize", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("27619bb8b30445d69b444ba4a3f3d3dd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BuildingPrivlidge", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_BuildingPrivlidge_27619bb8b30445d69b444ba4a3f3d3dd : Patch
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

		[Patch("OnCupboardClearList", "OnCupboardClearList", "BuildingPrivlidge", "ClearList", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("972f20d5056e48a4a46b4a7c0e0fed06")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BuildingPrivlidge", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_BuildingPrivlidge_972f20d5056e48a4a46b4a7c0e0fed06 : Patch
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

		[Patch("OnCupboardProtectionCalculated", "OnCupboardProtectionCalculated", "BuildingPrivlidge", "GetProtectedMinutes", new string[] { "System.Boolean" })]
		[Identifier("1db13f53b5b34fba8cf97d86cb26714e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BuildingPrivlidge", false)]
		[Parameter("self1", "BuildingPrivlidge", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_BuildingPrivlidge_1db13f53b5b34fba8cf97d86cb26714e : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1200792620), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BuildingPrivlidge"), "cachedProtectedMinutes"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Single"));
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

		[Patch("IOnCupboardAuthorize", "IOnCupboardAuthorize [BuildingPrivlidge]", "BuildingPrivlidge", "AddAuthorize", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("b509f2d2063044e3afd6879baa367aba")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "System.UInt64", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "BuildingPrivlidge", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_BuildingPrivlidge_b509f2d2063044e3afd6879baa367aba : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldloc_0, (object)null), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnCupboardAuthorize", (Type[])null, (Type[])null));
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

	public class Structure_Planner
	{
		[Patch("CanBuild", "CanBuild", "Planner", "DoBuild", new string[] { "ProtoBuf.CreateBuilding" })]
		[Identifier("9614b312adcb481e9b28f76e5877b776")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Planner", false)]
		[Parameter("local1", "Construction", false)]
		[Parameter("local3", "Construction+Target", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_Planner_9614b312adcb481e9b28f76e5877b776 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 293)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)269294084), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("Construction+Target"));
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

		[Patch("OnEntityBuilt", "OnEntityBuilt", "Planner", "DoBuild", new string[] { "Construction/Target", "Construction" })]
		[Identifier("aff4ed16a5f140bc80aba02b721f75f1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Planner", false)]
		[Parameter("local2", "UnityEngine.GameObject", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_Planner_aff4ed16a5f140bc80aba02b721f75f1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 183)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)641201665), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
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

		[Patch("CanAffordToPlace", "CanAffordToPlace", "Planner", "CanAffordToPlace", new string[] { "Construction" })]
		[Identifier("60d5279cf6a04885aa6669a5737c107c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("self", "Planner", false)]
		[Parameter("component", "Construction", false)]
		[Return(typeof(bool))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_Planner_60d5279cf6a04885aa6669a5737c107c : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1186965622), instruction);
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

		[Patch("OnConstructionPlace", "OnConstructionPlace", "Planner", "DoPlacement", new string[] { "Construction/Target", "Construction" })]
		[Identifier("09c2ee4f75084f7aad408bbc2af3d164")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_Planner_09c2ee4f75084f7aad408bbc2af3d164 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Expected O, but got Unknown
				//IL_0052: Unknown result type (might be due to invalid IL or missing references)
				//IL_005c: Expected O, but got Unknown
				//IL_006c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0076: Expected O, but got Unknown
				//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
				//IL_00fa: Expected O, but got Unknown
				//IL_0116: Unknown result type (might be due to invalid IL or missing references)
				//IL_0120: Expected O, but got Unknown
				//IL_0166: Unknown result type (might be due to invalid IL or missing references)
				//IL_0170: Expected O, but got Unknown
				//IL_0185: Unknown result type (might be due to invalid IL or missing references)
				//IL_018f: Expected O, but got Unknown
				//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
				//IL_01cd: Expected O, but got Unknown
				//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
				//IL_01ec: Expected O, but got Unknown
				//IL_0214: Unknown result type (might be due to invalid IL or missing references)
				//IL_021e: Expected O, but got Unknown
				//IL_026a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0274: Expected O, but got Unknown
				//IL_0289: Unknown result type (might be due to invalid IL or missing references)
				//IL_0293: Expected O, but got Unknown
				//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
				//IL_02d1: Expected O, but got Unknown
				//IL_0305: Unknown result type (might be due to invalid IL or missing references)
				//IL_030f: Expected O, but got Unknown
				//IL_0343: Unknown result type (might be due to invalid IL or missing references)
				//IL_034d: Expected O, but got Unknown
				//IL_0354: Unknown result type (might be due to invalid IL or missing references)
				//IL_035e: Expected O, but got Unknown
				//IL_0365: Unknown result type (might be due to invalid IL or missing references)
				//IL_036f: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnConstructionPlace"));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldarg_2, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Box, (object)typeof(Target)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[5]
				{
					typeof(string),
					typeof(object),
					typeof(object),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[94];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseNetworkableEx"), "IsValid", new Type[1] { typeof(BaseNetworkable) }, (Type[])null)));
				Label label2 = Generator.DefineLabel();
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label2));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseNetworkable"), "KillMessage", (Type[])null, (Type[])null)));
				Label label3 = Generator.DefineLabel();
				list.Add(new CodeInstruction(OpCodes.Br_S, (object)label3));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Isinst, (object)typeof(DecayEntity)));
				list.Add(__GeneratorRuntime.CreateStoreLocalInstruction(Generator, Method, 8, typeof(object)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 8, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("UnityEngine.Object"), "op_Implicit", (Type[])null, (Type[])null)));
				Label label4 = Generator.DefineLabel();
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label4));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 8, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("DecayEntity"), "DoServerDestroy", (Type[])null, (Type[])null)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseNetworkable"), "TerminateOnServer", (Type[])null, (Type[])null)));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 1, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseNetworkable"), "EntityDestroy", (Type[])null, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Ldnull, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				list[14].labels.Add(label2);
				list[26].labels.Add(label3);
				list[22].labels.Add(label4);
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[94]), list2[94]);
				}
				list2.InsertRange(94, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Structure_CodeLock
	{
		[Patch("OnCodeEntered", "OnCodeEntered", "CodeLock", "UnlockWithCode", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("57807c24cd0b4cb2ae31ba59e300bd1a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CodeLock", false)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("local0", "System.String", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_CodeLock_57807c24cd0b4cb2ae31ba59e300bd1a : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1418013452)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}

		[Patch("OnCodeChanged", "OnCodeChanged", "CodeLock", "RPC_ChangeCode", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("2b3e419edd194c81b374cec5641afb9b")]
		[Dependencies(new string[] { "CanChangeCode" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("player", "BasePlayer", false)]
		[Parameter("self", "CodeLock", false)]
		[Parameter("local0", "System.String", false)]
		[Parameter("local1", "System.Boolean", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_CodeLock_2b3e419edd194c81b374cec5641afb9b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 122)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1462721731)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("BaseEntity+RPCMessage"), "player"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Boolean"));
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

	public class Structure_ServerBuildingManager
	{
		[Patch("OnBuildingSplit", "OnBuildingSplit", "ServerBuildingManager", "Split", new string[] { "BuildingManager/Building" })]
		[Identifier("5f4ea3db74144804b5839aeb11e75756")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("oldBuilding", "BuildingManager+Building", false)]
		[Parameter("local3", "System.UInt32", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_ServerBuildingManager_5f4ea3db74144804b5839aeb11e75756 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1034394591), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt32"));
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

		[Patch("OnBuildingMerge", "OnBuildingMerge", "ServerBuildingManager", "Merge", new string[] { "BuildingManager/Building", "BuildingManager/Building" })]
		[Identifier("26e8f4661e55460587c93b46acf7c7d8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ServerBuildingManager", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_ServerBuildingManager_26e8f4661e55460587c93b46acf7c7d8 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-810622106)), instruction), instruction);
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

	public class Structure_PhotoFrame
	{
		[Patch("OnSignLocked", "OnSignLocked [PhotoFrame]", "PhotoFrame", "LockSign", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("b0871e92b6b343c0bebd14f30a05f894")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhotoFrame", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_PhotoFrame_b0871e92b6b343c0bebd14f30a05f894 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 25)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1270763772), instruction), instruction);
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

		[Patch("OnSignUpdated", "OnSignUpdated [PhotoFrame]", "PhotoFrame", "UpdateSign", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("4328507509df47a68d2ed2c9d010b7ad")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "PhotoFrame", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_PhotoFrame_4328507509df47a68d2ed2c9d010b7ad : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 49)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1659571272)), instruction), instruction);
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

	public class Structure_ItemModDeployable
	{
		[Patch("OnCupboardAuthorize", "OnCupboardAuthorize [ItemModDeployable]", "ItemModDeployable", "OnDeployed", new string[] { "BaseEntity", "BasePlayer" })]
		[Identifier("6e533bc6b64a467cb0b288342dee08d2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "BuildingPrivlidge", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_ItemModDeployable_6e533bc6b64a467cb0b288342dee08d2 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1460091328), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
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
	}

	public class Structure_CarvablePumpkin
	{
		[Patch("OnSignUpdated", "OnSignUpdated [CarvablePumpkin]", "CarvablePumpkin", "UpdateSign", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("12aaa0b0f93b4870ba714da6acb559e8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "CarvablePumpkin", false)]
		[Parameter("player", "BasePlayer", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_CarvablePumpkin_12aaa0b0f93b4870ba714da6acb559e8 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 120)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1659571272)), instruction), instruction);
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

	public class Structure_DoorKnocker
	{
		[Patch("OnDoorKnocked", "OnDoorKnocked [DoorKnocker]", "DoorKnocker", "Knock", new string[] { "BasePlayer" })]
		[Identifier("afb757dca3aa4118a006ead0fe134156")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "DoorKnocker", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_DoorKnocker_afb757dca3aa4118a006ead0fe134156 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)640250473), instruction), instruction);
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

	public class Structure_Locker
	{
		[Patch("OnLockerSwap", "OnLockerSwap", "Locker", "RPC_Equip", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("40f253c503d34e568c6b976f75c5a5f3")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Locker", false)]
		[Parameter("local0", "System.Int32", false)]
		[Parameter("player", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_Locker_40f253c503d34e568c6b976f75c5a5f3 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1350632731), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.Int32"));
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}
	}

	public class Structure_StringLights
	{
		[Patch("OnPoweredLightsPointAdd", "OnPoweredLightsPointAdd", "StringLights", "SERVER_AddPoint", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("adfe0588e28f4c71b1d09fb947a299e8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "StringLights", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local1", "UnityEngine.Vector3", false)]
		[Parameter("local2", "UnityEngine.Vector3", false)]
		[Return(typeof(void))]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_StringLights_adfe0588e28f4c71b1d09fb947a299e8 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 69)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1554325245), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("UnityEngine.Vector3"));
					yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("UnityEngine.Vector3"));
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

	public class Structure_SignContent
	{
		[Patch("OnSignContentCopied", "OnSignContentCopied", "SignContent", "CopyInfoToSign", new string[] { "ISignage", "IUGCBrowserEntity" })]
		[Identifier("a5b4ee28314d4fabb527c8a2caaddcf2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "SignContent", false)]
		[Parameter("s", "ISignage", false)]
		[Parameter("b", "IUGCBrowserEntity", false)]
		[Category("Structure")]
		[Assembly("Assembly-CSharp.dll")]
		public class Structure_SignContent_a5b4ee28314d4fabb527c8a2caaddcf2 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 34)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-685770198)), instruction);
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
}
