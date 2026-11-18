using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Patches
{
	public class Patches_FacepunchRConRConListener
	{
		[Patch("OnRconConnection [exp, patch]", "OnRconConnection [exp, patch]", "Facepunch.RCon/RConListener", "ProcessConnections", new string[] { })]
		[Identifier("4ba7291d7cca497f8ba3f57a20780dd3")]
		[Dependencies(new string[] { "OnRconConnection [exp]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_FacepunchRConRConListener_4ba7291d7cca497f8ba3f57a20780dd3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("System.Net.Sockets.Socket"), "Close", (Type[])null, (Type[])null)));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[22]), list2[22]);
				}
				list2.InsertRange(22, list);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_PlayerLoot
	{
		[Patch("OnLootEntity [patch]", "OnLootEntity [patch]", "PlayerLoot", "StartLootingEntity", new string[] { "BaseEntity", "System.Boolean" })]
		[Identifier("b6f7454d817b4f4abe8766c12035c3c0")]
		[Dependencies(new string[] { "OnLootEntity" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_PlayerLoot_b6f7454d817b4f4abe8766c12035c3c0 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("UnityEngine.Component"), "GetComponent", (Type[])null, new Type[1] { typeof(BasePlayer) })));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[29]), list2[29]);
				}
				list2.InsertRange(29, list);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnLootItem [patch]", "OnLootItem [patch]", "PlayerLoot", "StartLootingItem", new string[] { "Item" })]
		[Identifier("b2102cc084984ce782e7d83a67223c9e")]
		[Dependencies(new string[] { "OnLootItem" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_PlayerLoot_b2102cc084984ce782e7d83a67223c9e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("UnityEngine.Component"), "GetComponent", (Type[])null, new Type[1] { typeof(BasePlayer) })));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[39]), list2[39]);
				}
				list2.InsertRange(39, list);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_BaseMelee
	{
		[Patch("OnPlayerAttack [melee, patch]", "OnPlayerAttack [melee, patch]", "BaseMelee", "DoAttackShared", new string[] { "HitInfo" })]
		[Identifier("35dcb56497b24a979ccf39855362c1f6")]
		[Dependencies(new string[] { "OnPlayerAttack [Melee]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BaseMelee_35dcb56497b24a979ccf39855362c1f6 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("HeldEntity"), "GetOwnerPlayer", (Type[])null, (Type[])null)));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[2]), list2[2]);
				}
				list2.InsertRange(2, list);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_BaseEntity
	{
		[Patch("NoLimboGroupForPlayers [patch]", "NoLimboGroupForPlayers [patch]", "BaseEntity", "UpdateNetworkGroup", new string[] { })]
		[Identifier("cccb067b1ce84c4a9c6c024944c57422")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BaseEntity_cccb067b1ce84c4a9c6c024944c57422 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected Obj, but got Unknown
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Expected Obj, but got Unknown
				//IL_0056: Unknown result type (might be due to invalid IL or missing references)
				//IL_0060: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Isinst, (object)typeof(BasePlayer)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[134];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[126]), list2[126]);
				}
				list2.InsertRange(126, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("LimitNetworkingSignalBroadcast [Patch]", "LimitNetworkingSignalBroadcast [Patch]", "BaseEntity", "SignalBroadcast", new string[] { "BaseEntity/Signal", "System.String", "Network.Connection" })]
		[Identifier("993b416ffa43429988ba5172311db15e")]
		[Dependencies(new string[] { "OnSignalBroadcast" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BaseEntity_993b416ffa43429988ba5172311db15e : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected Obj, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected Obj, but got Unknown
				//IL_005e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseNetworkable"), "get_limitNetworking", (Type[])null, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[8];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[9]), list2[9]);
				}
				list2.InsertRange(9, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_ItemCrafter
	{
		[Patch("FixItemKeyId [patch]", "FixItemKeyId [patch]", "ItemCrafter", "CraftItem", new string[] { "ItemBlueprint", "BasePlayer", "ProtoBuf.Item/InstanceData", "System.Int32", "System.Int32", "Item", "System.Boolean", "System.Int32" })]
		[Identifier("0a1870e835954e37bde707c737148b1c")]
		[Dependencies(new string[] { "OnItemCraft" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_ItemCrafter_0a1870e835954e37bde707c737148b1c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_003f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0049: Expected Obj, but got Unknown
				//IL_0066: Unknown result type (might be due to invalid IL or missing references)
				//IL_0070: Expected Obj, but got Unknown
				//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ac: Expected Obj, but got Unknown
				//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c3: Expected Obj, but got Unknown
				//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d9: Expected Obj, but got Unknown
				//IL_010b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0115: Expected Obj, but got Unknown
				//IL_012f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0139: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[97];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldarg_S, (object)(sbyte)6));
				Label label2 = Generator.DefineLabel();
				CodeInstruction val2 = list2[94];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label2));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("ItemCraftTask"), "instanceData")));
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label2));
				list.Add(new CodeInstruction(OpCodes.Ldarg_S, (object)(sbyte)6));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("ItemCraftTask"), "instanceData")));
				list.Add(new CodeInstruction(OpCodes.Stfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("Item"), "instanceData")));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[92]), list2[92]);
				}
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[93].labels);
				}
				else
				{
					list2[94].labels.AddRange(list2[93].labels);
				}
				list2[93].labels.Clear();
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[94], list2[92]), list2[92]);
				}
				list2.RemoveRange(92, 2);
				list2.InsertRange(92, list);
				val.labels.Add(label);
				val2.labels.Add(label2);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_SupplySignal
	{
		[Patch("OnCargoPlaneSignaled [Patch]", "OnCargoPlaneSignaled [Patch]", "SupplySignal", "Explode", new string[] { })]
		[Identifier("029af3fe15af4a6bb190d8efc91f79f0")]
		[Dependencies(new string[] { "OnCargoPlaneSignaled" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_SupplySignal_029af3fe15af4a6bb190d8efc91f79f0 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[42];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[15]), list2[15]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[16], list2[15]), list2[15]);
				}
				list2.RemoveRange(15, 1);
				list2.InsertRange(15, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_CargoPlane
	{
		[Patch("OnSupplyDropDropped [patch 1]", "OnSupplyDropDropped [patch 1]", "CargoPlane", "Update", new string[] { })]
		[Identifier("81a8a1c58240410c9a1c7b6d81714d7b")]
		[Dependencies(new string[] { "OnSupplyDropDropped" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_CargoPlane_81a8a1c58240410c9a1c7b6d81714d7b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[52];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[19]), list2[19]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[20], list2[19]), list2[19]);
				}
				list2.RemoveRange(19, 1);
				list2.InsertRange(19, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnSupplyDropDropped [patch 2]", "OnSupplyDropDropped [patch 2]", "CargoPlane", "Update", new string[] { })]
		[Identifier("1ecb727160a14430a7b6bd223c7c507b")]
		[Dependencies(new string[] { "OnSupplyDropDropped [patch 1]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_CargoPlane_1ecb727160a14430a7b6bd223c7c507b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[52];
				list.Add(new CodeInstruction(OpCodes.Blt_Un_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[22]), list2[22]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[23], list2[22]), list2[22]);
				}
				list2.RemoveRange(22, 1);
				list2.InsertRange(22, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnSupplyDropDropped [patch 3]", "OnSupplyDropDropped [patch 3]", "CargoPlane", "Update", new string[] { })]
		[Identifier("179bc3c0f9eb425aa6a3aa4fd486ede5")]
		[Dependencies(new string[] { "OnSupplyDropDropped [patch 2]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_CargoPlane_179bc3c0f9eb425aa6a3aa4fd486ede5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[52];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[41]), list2[41]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[42], list2[41]), list2[41]);
				}
				list2.RemoveRange(41, 1);
				list2.InsertRange(41, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_AutoTurret
	{
		[Patch("ContinueTargetScan [patch]", "ContinueTargetScan [patch]", "AutoTurret", "TargetScan", new string[] { })]
		[Identifier("b37b5e0772e44c398aefd1d994aa853f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_AutoTurret_b37b5e0772e44c398aefd1d994aa853f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected Obj, but got Unknown
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Expected Obj, but got Unknown
				//IL_0060: Unknown result type (might be due to invalid IL or missing references)
				//IL_006a: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("AutoTurret"), "target")));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[184];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[183]), list2[183]);
				}
				list2.InsertRange(183, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_CH47HelicopterAIController
	{
		[Patch("AllowNpcNonAdminHeliUse [patch]", "AllowNpcNonAdminHeliUse [patch]", "CH47HelicopterAIController", "AttemptMount", new string[] { "BasePlayer", "System.Boolean" })]
		[Identifier("a839156bb1144f34bb6ff288530726ea")]
		[Dependencies(new string[] { "CanUseHelicopter" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_CH47HelicopterAIController_a839156bb1144f34bb6ff288530726ea : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[7]), list2[7]);
				}
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[8].labels);
				}
				else
				{
					list2[14].labels.AddRange(list2[8].labels);
				}
				list2[8].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[9].labels);
				}
				else
				{
					list2[14].labels.AddRange(list2[9].labels);
				}
				list2[9].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[10].labels);
				}
				else
				{
					list2[14].labels.AddRange(list2[10].labels);
				}
				list2[10].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[11].labels);
				}
				else
				{
					list2[14].labels.AddRange(list2[11].labels);
				}
				list2[11].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[12].labels);
				}
				else
				{
					list2[14].labels.AddRange(list2[12].labels);
				}
				list2[12].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[13].labels);
				}
				else
				{
					list2[14].labels.AddRange(list2[13].labels);
				}
				list2[13].labels.Clear();
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[14], list2[7]), list2[7]);
				}
				list2.RemoveRange(7, 7);
				list2.InsertRange(7, list);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_BasePlayerOnFeedbackReportd801
	{
		[Patch("OnFeedbackReported", "OnFeedbackReported [patch]", "BasePlayer/<OnFeedbackReport>d__801", "MoveNext", new string[] { })]
		[Identifier("c6d5d8f4f71549eab8f75840eb123415")]
		[Dependencies(new string[] { "OnFeedbackReported" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BasePlayerOnFeedbackReportd801_c6d5d8f4f71549eab8f75840eb123415 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[11]), list2[11]);
				}
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[12].labels);
				}
				else
				{
					list2[17].labels.AddRange(list2[12].labels);
				}
				list2[12].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[13].labels);
				}
				else
				{
					list2[17].labels.AddRange(list2[13].labels);
				}
				list2[13].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[14].labels);
				}
				else
				{
					list2[17].labels.AddRange(list2[14].labels);
				}
				list2[14].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[15].labels);
				}
				else
				{
					list2[17].labels.AddRange(list2[15].labels);
				}
				list2[15].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[16].labels);
				}
				else
				{
					list2[17].labels.AddRange(list2[16].labels);
				}
				list2[16].labels.Clear();
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[17], list2[11]), list2[11]);
				}
				list2.RemoveRange(11, 6);
				list2.InsertRange(11, list);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_FacepunchRconListener
	{
		[Patch("OnRconConnection", "OnRconConnection [web, patch]", "Facepunch.Rcon.Listener", "OnConnection", new string[] { "Fleck.IWebSocketConnection" })]
		[Identifier("b5a6791221774701bc5d25a46d1845ea")]
		[Dependencies(new string[] { "OnRconConnection [web]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Facepunch.Rcon.dll")]
		public class Patches_FacepunchRconListener_b5a6791221774701bc5d25a46d1845ea : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[71];
				list.Add(new CodeInstruction(OpCodes.Bne_Un_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[55]), list2[55]);
				}
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[56].labels);
				}
				else
				{
					list2[57].labels.AddRange(list2[56].labels);
				}
				list2[56].labels.Clear();
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[57], list2[55]), list2[55]);
				}
				list2.RemoveRange(55, 2);
				list2.InsertRange(55, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_FacepunchSqliteDatabase
	{
		[Patch("NoPragmaColumnExists", "NoPragmaColumnExists [patch]", "Facepunch.Sqlite.Database", "ColumnExists", new string[] { "System.String", "System.String" })]
		[Identifier("6e714b43fa4d4b9f91136ce949025fe5")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Facepunch.Sqlite.dll")]
		public class Patches_FacepunchSqliteDatabase_6e714b43fa4d4b9f91136ce949025fe5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_003e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0048: Expected Obj, but got Unknown
				//IL_004f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0059: Expected Obj, but got Unknown
				//IL_0064: Unknown result type (might be due to invalid IL or missing references)
				//IL_006e: Expected Obj, but got Unknown
				//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c0: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"select count(*) from sqlite_master where tbl_name=? and sql like ?;"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"% "));
				list.Add(new CodeInstruction(OpCodes.Ldarg_2, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)" %"));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("System.String"), "Concat", new Type[3]
				{
					typeof(string),
					typeof(string),
					typeof(string)
				}, (Type[])null)));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[1]), list2[1]);
				}
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[2].labels);
				}
				else
				{
					list2[4].labels.AddRange(list2[2].labels);
				}
				list2[2].labels.Clear();
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[3].labels);
				}
				else
				{
					list2[4].labels.AddRange(list2[3].labels);
				}
				list2[3].labels.Clear();
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[4], list2[1]), list2[1]);
				}
				list2.RemoveRange(1, 3);
				list2.InsertRange(1, list);
				return list2.AsEnumerable();
			}
		}
	}
}
