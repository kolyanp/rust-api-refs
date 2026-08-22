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
		[Identifier("af55fc5ac7b94c8fa44294406cba03a5")]
		[Dependencies(new string[] { "OnRconConnection [exp]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_FacepunchRConRConListener_af55fc5ac7b94c8fa44294406cba03a5 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Expected O, but got Unknown
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
		[Identifier("4b1cbf2a0ada4ee19537c5c52b979165")]
		[Dependencies(new string[] { "OnLootEntity" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_PlayerLoot_4b1cbf2a0ada4ee19537c5c52b979165 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Expected O, but got Unknown
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
		[Identifier("202972fcd3a040a298afd54844c36f17")]
		[Dependencies(new string[] { "OnLootItem" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_PlayerLoot_202972fcd3a040a298afd54844c36f17 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Expected O, but got Unknown
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
		[Identifier("34f8ac85cf1340b7a5ffb7ef34d5c5a1")]
		[Dependencies(new string[] { "OnPlayerAttack [Melee]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BaseMelee_34f8ac85cf1340b7a5ffb7ef34d5c5a1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
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
		[Identifier("9d560d46373f4706a06dc116137f2d72")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BaseEntity_9d560d46373f4706a06dc116137f2d72 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Expected O, but got Unknown
				//IL_0056: Unknown result type (might be due to invalid IL or missing references)
				//IL_0060: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Isinst, (object)typeof(BasePlayer)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[134];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[126]), list2[126]);
				}
				list2.InsertRange(126, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("LimitNetworkingSignalBroadcast [Patch]", "LimitNetworkingSignalBroadcast [Patch]", "BaseEntity", "SignalBroadcast", new string[] { "BaseEntity/Signal", "System.String", "Network.Connection" })]
		[Identifier("9fd9a6fc17e84060b6a2a05bdb5f6ffc")]
		[Dependencies(new string[] { "OnSignalBroadcast" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BaseEntity_9fd9a6fc17e84060b6a2a05bdb5f6ffc : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected O, but got Unknown
				//IL_005e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseNetworkable"), "get_limitNetworking", (Type[])null, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[8];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[9]), list2[9]);
				}
				list2.InsertRange(9, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_ItemCrafter
	{
		[Patch("FixItemKeyId [patch]", "FixItemKeyId [patch]", "ItemCrafter", "CraftItem", new string[] { "ItemBlueprint", "BasePlayer", "ProtoBuf.Item/InstanceData", "System.Int32", "System.Int32", "Item", "System.Boolean", "System.Int32" })]
		[Identifier("541e25ef4ad146f2a3b9b4fe78c0143d")]
		[Dependencies(new string[] { "OnItemCraft" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_ItemCrafter_541e25ef4ad146f2a3b9b4fe78c0143d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_003f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0049: Expected O, but got Unknown
				//IL_0066: Unknown result type (might be due to invalid IL or missing references)
				//IL_0070: Expected O, but got Unknown
				//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ac: Expected O, but got Unknown
				//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c3: Expected O, but got Unknown
				//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d9: Expected O, but got Unknown
				//IL_010b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0115: Expected O, but got Unknown
				//IL_012f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0139: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[96];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldarg_S, (object)(sbyte)6));
				Label label2 = Generator.DefineLabel();
				CodeInstruction obj = list2[93];
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
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[91]), list2[91]);
				}
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[92].labels);
				}
				else
				{
					list2[93].labels.AddRange(list2[92].labels);
				}
				list2[92].labels.Clear();
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[93], list2[91]), list2[91]);
				}
				list2.RemoveRange(91, 2);
				list2.InsertRange(91, list);
				val.labels.Add(label);
				obj.labels.Add(label2);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_SupplySignal
	{
		[Patch("OnCargoPlaneSignaled [Patch]", "OnCargoPlaneSignaled [Patch]", "SupplySignal", "Explode", new string[] { })]
		[Identifier("7650df5effbd4dc8a3e2c0362e718243")]
		[Dependencies(new string[] { "OnCargoPlaneSignaled" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_SupplySignal_7650df5effbd4dc8a3e2c0362e718243 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[42];
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
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_CargoPlane
	{
		[Patch("OnSupplyDropDropped [patch 1]", "OnSupplyDropDropped [patch 1]", "CargoPlane", "Update", new string[] { })]
		[Identifier("3d6be72d08d94a9eb0ba0ac92f1a63f2")]
		[Dependencies(new string[] { "OnSupplyDropDropped" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_CargoPlane_3d6be72d08d94a9eb0ba0ac92f1a63f2 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[52];
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
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnSupplyDropDropped [patch 2]", "OnSupplyDropDropped [patch 2]", "CargoPlane", "Update", new string[] { })]
		[Identifier("2e576a43eb7444d89e7490d436d0f2e6")]
		[Dependencies(new string[] { "OnSupplyDropDropped [patch 1]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_CargoPlane_2e576a43eb7444d89e7490d436d0f2e6 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[52];
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
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}

		[Patch("OnSupplyDropDropped [patch 3]", "OnSupplyDropDropped [patch 3]", "CargoPlane", "Update", new string[] { })]
		[Identifier("eac5e9600a194a49b7cc56408b4c80f3")]
		[Dependencies(new string[] { "OnSupplyDropDropped [patch 2]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_CargoPlane_eac5e9600a194a49b7cc56408b4c80f3 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[52];
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
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_Effectserver
	{
		[Patch("LimitNetworkingNoEffect [patch 1]", "LimitNetworkingNoEffect [patch 1]", "Effect/server", "ImpactEffect", new string[] { "HitInfo", "System.String" })]
		[Identifier("51b5ba5e273644049756b24a94de364b")]
		[Dependencies(new string[] { "OnImpactEffectCreate" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_Effectserver_51b5ba5e273644049756b24a94de364b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected O, but got Unknown
				//IL_0060: Unknown result type (might be due to invalid IL or missing references)
				//IL_006a: Expected O, but got Unknown
				//IL_0084: Unknown result type (might be due to invalid IL or missing references)
				//IL_008e: Expected O, but got Unknown
				//IL_0095: Unknown result type (might be due to invalid IL or missing references)
				//IL_009f: Expected O, but got Unknown
				//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c5: Expected O, but got Unknown
				//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00eb: Expected O, but got Unknown
				//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
				//IL_0101: Expected O, but got Unknown
				//IL_0108: Unknown result type (might be due to invalid IL or missing references)
				//IL_0112: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("HitInfo"), "get_InitiatorPlayer", (Type[])null, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("UnityEngine.Object"), "op_Implicit", (Type[])null, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[7];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("HitInfo"), "get_InitiatorPlayer", (Type[])null, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseNetworkable"), "get_limitNetworking", (Type[])null, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[7]), list2[7]);
				}
				list2.InsertRange(7, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_BaseProjectile
	{
		[Patch("LimitNetworkingNoEffect [patch 2]", "LimitNetworkingNoEffect [patch 2]", "BaseProjectile", "CLProject", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("b89df7d1f2fc494a8db0b63cc4ec248a")]
		[Dependencies(new string[] { "OnWeaponFired" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BaseProjectile_b89df7d1f2fc494a8db0b63cc4ec248a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Expected O, but got Unknown
				//IL_0069: Unknown result type (might be due to invalid IL or missing references)
				//IL_0073: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseNetworkable"), "get_limitNetworking", (Type[])null, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[273];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[254]), list2[254]);
				}
				list2.InsertRange(254, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_BasePlayer
	{
		[Patch("LimitNetworkingNoEffect [patch 3]", "LimitNetworkingNoEffect [patch 3]", "BasePlayer", "OnAttacked", new string[] { "HitInfo" })]
		[Identifier("01a9b0c64d924fcba0736eed73668a4d")]
		[Dependencies(new string[] { "IOnBasePlayerAttacked" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BasePlayer_01a9b0c64d924fcba0736eed73668a4d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Expected O, but got Unknown
				//IL_006a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0074: Expected O, but got Unknown
				//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b2: Expected O, but got Unknown
				//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00dc: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 8, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("UnityEngine.Object"), "op_Implicit", (Type[])null, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[242];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 8, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseNetworkable"), "get_limitNetworking", (Type[])null, (Type[])null)));
				Label label2 = Generator.DefineLabel();
				CodeInstruction obj = list2[264];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label2));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[242]), list2[242]);
				}
				list2.InsertRange(242, list);
				val.labels.Add(label);
				obj.labels.Add(label2);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_AutoTurret
	{
		[Patch("ContinueTargetScan [patch]", "ContinueTargetScan [patch]", "AutoTurret", "TargetScan", new string[] { })]
		[Identifier("3661809c65cb47d0bc27d9145f727cb1")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_AutoTurret_3661809c65cb47d0bc27d9145f727cb1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Expected O, but got Unknown
				//IL_0060: Unknown result type (might be due to invalid IL or missing references)
				//IL_006a: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("AutoTurret"), "target")));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[184];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[183]), list2[183]);
				}
				list2.InsertRange(183, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_RelationshipManager
	{
		[Patch("LimitNetworkingAcquaintances [patch]", "LimitNetworkingAcquaintances [patch]", "RelationshipManager", "UpdateAcquaintancesFor", new string[] { "BasePlayer", "System.Single" })]
		[Identifier("0d8557d5ef744f0ab3533a04aafb2737")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_RelationshipManager_0d8557d5ef744f0ab3533a04aafb2737 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Expected O, but got Unknown
				//IL_0066: Unknown result type (might be due to invalid IL or missing references)
				//IL_0070: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 3, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseNetworkable"), "get_limitNetworking", (Type[])null, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[111];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[35]), list2[35]);
				}
				list2.InsertRange(35, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_CH47HelicopterAIController
	{
		[Patch("AllowNpcNonAdminHeliUse [patch]", "AllowNpcNonAdminHeliUse [patch]", "CH47HelicopterAIController", "AttemptMount", new string[] { "BasePlayer", "System.Boolean" })]
		[Identifier("5d8fae54e2414f3fbdd433005c129c76")]
		[Dependencies(new string[] { "CanUseHelicopter" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_CH47HelicopterAIController_5d8fae54e2414f3fbdd433005c129c76 : Patch
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

	public class Patches_BasePlayerOnFeedbackReportd764
	{
		[Patch("OnFeedbackReported", "OnFeedbackReported [patch]", "BasePlayer/<OnFeedbackReport>d__764", "MoveNext", new string[] { })]
		[Identifier("c027e9484f8c43e9a180e34f0b4d2553")]
		[Dependencies(new string[] { "OnFeedbackReported" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BasePlayerOnFeedbackReportd764_c027e9484f8c43e9a180e34f0b4d2553 : Patch
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

	public class Patches_FacepunchRconListenercDisplayClass270
	{
		[Patch("OnRconConnection", "OnRconConnection [web, patch]", "Facepunch.Rcon.Listener/<>c__DisplayClass27_0", "<Start>b__0", new string[] { "Fleck.IWebSocketConnection" })]
		[Identifier("c28f004add334c10838a66bbcccd8858")]
		[Dependencies(new string[] { "OnRconConnection [web]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Facepunch.Rcon.dll")]
		public class Patches_FacepunchRconListenercDisplayClass270_c28f004add334c10838a66bbcccd8858 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[69];
				list.Add(new CodeInstruction(OpCodes.Bne_Un_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[52]), list2[52]);
				}
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[53].labels);
				}
				else
				{
					list2[54].labels.AddRange(list2[53].labels);
				}
				list2[53].labels.Clear();
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[54], list2[52]), list2[52]);
				}
				list2.RemoveRange(52, 2);
				list2.InsertRange(52, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_FacepunchSqliteDatabase
	{
		[Patch("NoPragmaColumnExists", "NoPragmaColumnExists [patch]", "Facepunch.Sqlite.Database", "ColumnExists", new string[] { "System.String", "System.String" })]
		[Identifier("6bf5443093f04c5ab33bf3f6acebf216")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Facepunch.Sqlite.dll")]
		public class Patches_FacepunchSqliteDatabase_6bf5443093f04c5ab33bf3f6acebf216 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_003e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0048: Expected O, but got Unknown
				//IL_004f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0059: Expected O, but got Unknown
				//IL_0064: Unknown result type (might be due to invalid IL or missing references)
				//IL_006e: Expected O, but got Unknown
				//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c0: Expected O, but got Unknown
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
