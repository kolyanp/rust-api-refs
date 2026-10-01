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
		[Identifier("49694c4ad5a9433b9eccf4ea61cd1a11")]
		[Dependencies(new string[] { "OnRconConnection [exp]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_FacepunchRConRConListener_49694c4ad5a9433b9eccf4ea61cd1a11 : Patch
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
		[Identifier("6f1fba947fe24a9082967ee960faa758")]
		[Dependencies(new string[] { "OnLootEntity" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_PlayerLoot_6f1fba947fe24a9082967ee960faa758 : Patch
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
		[Identifier("c64268f4c1df447983465c7d8c62969a")]
		[Dependencies(new string[] { "OnLootItem" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_PlayerLoot_c64268f4c1df447983465c7d8c62969a : Patch
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
		[Identifier("b2c0d5305ae24881a023791453945a96")]
		[Dependencies(new string[] { "OnPlayerAttack [Melee]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BaseMelee_b2c0d5305ae24881a023791453945a96 : Patch
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
		[Identifier("3fa8b08925dd4e838abfa9c493ad44c2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BaseEntity_3fa8b08925dd4e838abfa9c493ad44c2 : Patch
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
		[Identifier("d6079bb3367f4699bfe605d45e96ff5a")]
		[Dependencies(new string[] { "OnSignalBroadcast" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BaseEntity_d6079bb3367f4699bfe605d45e96ff5a : Patch
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
		[Identifier("23d02bcf68704775800db90a49924d74")]
		[Dependencies(new string[] { "OnItemCraft" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_ItemCrafter_23d02bcf68704775800db90a49924d74 : Patch
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
		[Identifier("140d63f5eb6a42c293b8cd5e217987a3")]
		[Dependencies(new string[] { "OnCargoPlaneSignaled" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_SupplySignal_140d63f5eb6a42c293b8cd5e217987a3 : Patch
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
		[Identifier("9f5f93cc33ac446a8f948f34122f29db")]
		[Dependencies(new string[] { "OnSupplyDropDropped" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_CargoPlane_9f5f93cc33ac446a8f948f34122f29db : Patch
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
		[Identifier("7461fcb47fb241f3b1c7df28d197aeef")]
		[Dependencies(new string[] { "OnSupplyDropDropped [patch 1]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_CargoPlane_7461fcb47fb241f3b1c7df28d197aeef : Patch
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
		[Identifier("f6dd7aba3cdf459ca3187377e41a370c")]
		[Dependencies(new string[] { "OnSupplyDropDropped [patch 2]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_CargoPlane_f6dd7aba3cdf459ca3187377e41a370c : Patch
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

	public class Patches_Effectserver
	{
		[Patch("LimitNetworkingNoEffect [patch 1]", "LimitNetworkingNoEffect [patch 1]", "Effect/server", "ImpactEffect", new string[] { "HitInfo", "System.String" })]
		[Identifier("a029d454769f40debb5f9ed4a7b9f522")]
		[Dependencies(new string[] { "OnImpactEffectCreate" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_Effectserver_a029d454769f40debb5f9ed4a7b9f522 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected Obj, but got Unknown
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Expected Obj, but got Unknown
				//IL_0060: Unknown result type (might be due to invalid IL or missing references)
				//IL_006a: Expected Obj, but got Unknown
				//IL_0084: Unknown result type (might be due to invalid IL or missing references)
				//IL_008e: Expected Obj, but got Unknown
				//IL_0095: Unknown result type (might be due to invalid IL or missing references)
				//IL_009f: Expected Obj, but got Unknown
				//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c5: Expected Obj, but got Unknown
				//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00eb: Expected Obj, but got Unknown
				//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
				//IL_0101: Expected Obj, but got Unknown
				//IL_0108: Unknown result type (might be due to invalid IL or missing references)
				//IL_0112: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("HitInfo"), "get_InitiatorPlayer", (Type[])null, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("UnityEngine.Object"), "op_Implicit", (Type[])null, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[7];
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
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_BaseProjectile
	{
		[Patch("LimitNetworkingNoEffect [patch 2]", "LimitNetworkingNoEffect [patch 2]", "BaseProjectile", "CLProject", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("889db688268f4d79b64ef59af1e4937f")]
		[Dependencies(new string[] { "OnWeaponFired" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BaseProjectile_889db688268f4d79b64ef59af1e4937f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Expected Obj, but got Unknown
				//IL_0069: Unknown result type (might be due to invalid IL or missing references)
				//IL_0073: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 0, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseNetworkable"), "get_limitNetworking", (Type[])null, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[273];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[254]), list2[254]);
				}
				list2.InsertRange(254, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_BasePlayer
	{
		[Patch("LimitNetworkingNoEffect [patch 3]", "LimitNetworkingNoEffect [patch 3]", "BasePlayer", "OnAttacked", new string[] { "HitInfo" })]
		[Identifier("8f24249984444a81bfbcc6e78f8c38ba")]
		[Dependencies(new string[] { "IOnBasePlayerAttacked" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BasePlayer_8f24249984444a81bfbcc6e78f8c38ba : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Expected Obj, but got Unknown
				//IL_006a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0074: Expected Obj, but got Unknown
				//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b2: Expected Obj, but got Unknown
				//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00dc: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 8, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("UnityEngine.Object"), "op_Implicit", (Type[])null, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[248];
				list.Add(new CodeInstruction(OpCodes.Brfalse_S, (object)label));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 8, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseNetworkable"), "get_limitNetworking", (Type[])null, (Type[])null)));
				Label label2 = Generator.DefineLabel();
				CodeInstruction val2 = list2[270];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label2));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[248]), list2[248]);
				}
				list2.InsertRange(248, list);
				val.labels.Add(label);
				val2.labels.Add(label2);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_AutoTurret
	{
		[Patch("ContinueTargetScan [patch]", "ContinueTargetScan [patch]", "AutoTurret", "TargetScan", new string[] { })]
		[Identifier("0a9f4b068d6d4f669b28ae2083b1b02b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_AutoTurret_0a9f4b068d6d4f669b28ae2083b1b02b : Patch
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

	public class Patches_RelationshipManager
	{
		[Patch("LimitNetworkingAcquaintances [patch]", "LimitNetworkingAcquaintances [patch]", "RelationshipManager", "UpdateAcquaintancesFor", new string[] { "BasePlayer", "System.Single" })]
		[Identifier("3898489dc9c74af3b1d57e86fa87c302")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_RelationshipManager_3898489dc9c74af3b1d57e86fa87c302 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Expected Obj, but got Unknown
				//IL_0066: Unknown result type (might be due to invalid IL or missing references)
				//IL_0070: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 3, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseNetworkable"), "get_limitNetworking", (Type[])null, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[111];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[35]), list2[35]);
				}
				list2.InsertRange(35, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class Patches_CH47HelicopterAIController
	{
		[Patch("AllowNpcNonAdminHeliUse [patch]", "AllowNpcNonAdminHeliUse [patch]", "CH47HelicopterAIController", "AttemptMount", new string[] { "BasePlayer", "System.Boolean" })]
		[Identifier("ef960833763440ca9d7ebc58a388ce0b")]
		[Dependencies(new string[] { "CanUseHelicopter" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_CH47HelicopterAIController_ef960833763440ca9d7ebc58a388ce0b : Patch
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

	public class Patches_BasePlayerOnFeedbackReportd777
	{
		[Patch("OnFeedbackReported", "OnFeedbackReported [patch]", "BasePlayer/<OnFeedbackReport>d__777", "MoveNext", new string[] { })]
		[Identifier("1f769719e37c439cb128adee524f7990")]
		[Dependencies(new string[] { "OnFeedbackReported" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Assembly-CSharp.dll")]
		public class Patches_BasePlayerOnFeedbackReportd777_1f769719e37c439cb128adee524f7990 : Patch
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
		[Identifier("2944df0f802a413c87a50fed216ca093")]
		[Dependencies(new string[] { "OnRconConnection [web]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Facepunch.Rcon.dll")]
		public class Patches_FacepunchRconListener_2944df0f802a413c87a50fed216ca093 : Patch
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
		[Identifier("0d8434c393384723b9b7d88879c1f3de")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("_Patches")]
		[Assembly("Facepunch.Sqlite.dll")]
		public class Patches_FacepunchSqliteDatabase_0d8434c393384723b9b7d88879c1f3de : Patch
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
