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

public class Category_NPC
{
	public class NPC_BaseNpc
	{
		[Patch("CanNpcEat", "CanNpcEat [BaseNpc]", "BaseNpc", "WantsToEat", new string[] { "BaseEntity" })]
		[Identifier("0984acd283a145769a97d33177beab80")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNpc", false)]
		[Return(typeof(bool))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BaseNpc_0984acd283a145769a97d33177beab80 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1128140601)), instruction), instruction);
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

		[Patch("OnNpcAttack", "OnNpcAttack [BaseNpc]", "BaseNpc", "StartAttack", new string[] { })]
		[Identifier("6f58cd2c8af446bd907273af843536e7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNpc", false)]
		[Parameter("attackTarget", "BaseEntity", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BaseNpc_6f58cd2c8af446bd907273af843536e7 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1553899915)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseNpc"), "get_AttackTarget", (Type[])null, (Type[])null));
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

		[Patch("IOnNpcTarget", "IOnNpcTarget [BaseNpc]", "BaseNpc", "GetWantsToAttack", new string[] { "BaseEntity" })]
		[Identifier("9bdc524e3fdb4072825ab3297f80510d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNpc", false)]
		[Return(typeof(float))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BaseNpc_9bdc524e3fdb4072825ab3297f80510d : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldarg_0, (object)null), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnNpcTarget", (Type[])null, (Type[])null));
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

	public class NPC_NPCVendingMachine
	{
		[Patch("OnNpcGiveSoldItem", "OnNpcGiveSoldItem", "NPCVendingMachine", "GiveSoldItem", new string[] { "Item", "BasePlayer" })]
		[Identifier("a220fb3d44e7493e87e2121aae6a0902")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NPCVendingMachine", false)]
		[Parameter("soldItem", "Item", false)]
		[Parameter("buyer", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_NPCVendingMachine_a220fb3d44e7493e87e2121aae6a0902 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)650484908), instruction), instruction);
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

	public class NPC_ScientistNPC
	{
		[Patch("OnNpcRadioChatter", "OnNpcRadioChatter [ScientistNPC]", "ScientistNPC", "PlayRadioChatter", new string[] { })]
		[Identifier("f5bc361a680242579ba70ed5eb54ee0c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ScientistNPC", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_ScientistNPC_f5bc361a680242579ba70ed5eb54ee0c : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-2063003559)), instruction), instruction);
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

		[Patch("OnNpcAlert", "OnNpcAlert [ScientistNPC]", "ScientistNPC", "Alert", new string[] { })]
		[Identifier("4d8a41d4a53c4679b83a58b8ff979068")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ScientistNPC", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_ScientistNPC_4d8a41d4a53c4679b83a58b8ff979068 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1858657499)), instruction), instruction);
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

		[Patch("OnNpcEquipWeapon", "OnNpcEquipWeapon [ScientistNPC]", "ScientistNPC", "EquipWeapon", new string[] { "System.Boolean" })]
		[Identifier("22871e3e38934f36b750dfe8ea9ffe47")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ScientistNPC", false)]
		[Parameter("local1", "Item", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_ScientistNPC_22871e3e38934f36b750dfe8ea9ffe47 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)92399643), instruction), instruction);
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
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class NPC_NPCPlayer
	{
		[Patch("OnNpcEquipWeapon", "OnNpcEquipWeapon [NPCPlayer]", "NPCPlayer", "EquipWeapon", new string[] { "System.Boolean" })]
		[Identifier("62ba2eab8de146d9a2ea260f235ab638")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NPCPlayer", false)]
		[Parameter("local0", "Item", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_NPCPlayer_62ba2eab8de146d9a2ea260f235ab638 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)92399643), instruction), instruction);
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

		[Patch("OnCorpsePopulate", "OnCorpsePopulate", "NPCPlayer", "CreateCorpse", new string[] { "BasePlayer/PlayerFlags", "UnityEngine.Vector3", "UnityEngine.Quaternion", "System.Collections.Generic.List`1<TriggerBase>", "System.Boolean" })]
		[Identifier("6bf6f2bca5bf4cf0965d20f3b19d902f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NPCPlayer", false)]
		[Parameter("local1", "NPCPlayerCorpse", false)]
		[Return(typeof(BaseCorpse))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_NPCPlayer_6bf6f2bca5bf4cf0965d20f3b19d902f : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 121)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)338615359), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
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
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(BaseCorpse));
					yield return new CodeInstruction(OpCodes.Ldnull, (object)null);
					yield return new CodeInstruction(OpCodes.Beq_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(BaseCorpse));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}
	}

	public class NPC_HumanNPC
	{
		[Patch("OnNpcDuck", "OnNpcDuck [HumanNPC]", "HumanNPC", "SetDucked", new string[] { "System.Boolean" })]
		[Identifier("d52284d93e3240bfbf2905ea5ba67548")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HumanNPC", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_HumanNPC_d52284d93e3240bfbf2905ea5ba67548 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)120484519), instruction), instruction);
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

		[Patch("OnNpcTarget", "OnNpcTarget [HumanNPC]", "HumanNPC", "GetBestTarget", new string[] { })]
		[Identifier("e39073fef3c542db89f00759c9f9c0f2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_HumanNPC_e39073fef3c542db89f00759c9f9c0f2 : Patch
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
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnNpcTarget"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 3, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[85];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[22]), list2[22]);
				}
				list2.InsertRange(22, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class NPC_NPCTalking
	{
		[Patch("OnNpcConversationRespond", "OnNpcConversationRespond", "NPCTalking", "Server_ResponsePressed", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("92201614ad584753b896342684859fbf")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NPCTalking", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local6", "ConversationData", false)]
		[Parameter("local20", "ConversationData+ResponseNode", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_NPCTalking_92201614ad584753b896342684859fbf : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 187)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1580074841), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)6);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)20);
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

		[Patch("OnNpcConversationResponded", "OnNpcConversationResponded", "NPCTalking", "Server_ResponsePressed", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("dc17295f174b4a5cba54b4e7f9bcd283")]
		[Dependencies(new string[] { "OnNpcConversationRespond" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NPCTalking", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local6", "ConversationData", false)]
		[Parameter("local20", "ConversationData+ResponseNode", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_NPCTalking_dc17295f174b4a5cba54b4e7f9bcd283 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 246)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1571158728)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)6);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)20);
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

		[Patch("OnNpcConversationEnded", "OnNpcConversationEnded", "NPCTalking", "Server_OnConversationEnded", new string[] { "BasePlayer" })]
		[Identifier("8d1809551bba43b9bf7d2355f39a1dca")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NPCTalking", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_NPCTalking_8d1809551bba43b9bf7d2355f39a1dca : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1853740711), instruction), instruction);
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

		[Patch("OnNpcConversationStart", "OnNpcConversationStart", "NPCTalking", "Server_BeginTalking", new string[] { "BasePlayer" })]
		[Identifier("bb7edc41aa8a44f28a70cf857a52276b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NPCTalking", false)]
		[Parameter("ply", "BasePlayer", false)]
		[Parameter("local2", "ConversationData", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_NPCTalking_bb7edc41aa8a44f28a70cf857a52276b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 51)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1100753115)), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
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
					Label label2 = Generator.DefineLabel();
					Instructions.Last().labels.Add(label2);
					yield return new CodeInstruction(OpCodes.Leave, (object)label2);
					yield return instruction;
				}
			}
		}
	}

	public class NPC_RustAiSimpleAIMemory
	{
		[Patch("OnNpcTargetSense", "OnNpcTargetSense", "Rust.Ai.SimpleAIMemory", "SetKnown", new string[] { "BaseEntity", "BaseEntity", "AIBrainSenses" })]
		[Identifier("dd186e85d7db4620b711f0f9b33a3b1c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("owner", "BaseEntity", false)]
		[Parameter("ent", "BaseEntity", false)]
		[Parameter("brainSenses", "AIBrainSenses", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_RustAiSimpleAIMemory_dd186e85d7db4620b711f0f9b33a3b1c : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)978969450), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
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

	public class NPC_BaseAIBrain
	{
		[Patch("OnAIBrainStateSwitch", "OnAIBrainStateSwitch", "BaseAIBrain", "SwitchToState", new string[] { "BaseAIBrain/BasicAIState", "System.Int32" })]
		[Identifier("89f3f46f188f48c29428c4efa4bc1bee")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseAIBrain", false)]
		[Parameter("currentState", "BaseAIBrain+BasicAIState", false)]
		[Parameter("newState", "BaseAIBrain+BasicAIState", false)]
		[Return(typeof(bool))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BaseAIBrain_89f3f46f188f48c29428c4efa4bc1bee : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-633079558)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseAIBrain"), "get_CurrentState", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HookCaller), "CallStaticHook", new Type[4]
					{
						typeof(uint),
						typeof(object),
						typeof(object),
						typeof(object)
					}, (Type[])null));
					Label label1 = Generator.DefineLabel();
					Label label2 = Generator.DefineLabel();
					object retvar = Generator.DeclareLocal(typeof(object));
					instruction.labels.Add(label1);
					yield return new CodeInstruction(OpCodes.Stloc, retvar);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Brfalse_S, (object)label1);
					yield return new CodeInstruction(OpCodes.Ldloc, retvar);
					yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Brtrue_S, (object)label2);
					yield return new CodeInstruction(OpCodes.Ldc_I4_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return CodeInstructionExtensions.WithLabels(new CodeInstruction(OpCodes.Ldloc, retvar), new Label[1] { label2 });
					yield return new CodeInstruction(OpCodes.Unbox_Any, (object)typeof(bool));
					yield return new CodeInstruction(OpCodes.Ret, (object)null);
					yield return instruction;
				}
			}
		}

		[Patch("OnAIBrainStateSwitched", "OnAIBrainStateSwitched", "BaseAIBrain", "SwitchToState", new string[] { "BaseAIBrain/BasicAIState", "System.Int32" })]
		[Identifier("53c9d5f8dbcd4ed1a35f9114d5526922")]
		[Dependencies(new string[] { "OnAIBrainStateSwitch" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseAIBrain", false)]
		[Parameter("currentState", "BaseAIBrain+BasicAIState", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BaseAIBrain_53c9d5f8dbcd4ed1a35f9114d5526922 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 63)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-872738166)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(AccessToolsEx.TypeByName("BaseAIBrain"), "get_CurrentState", (Type[])null, (Type[])null));
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

	public class NPC_BradleyAPC
	{
		[Patch("CanDeployScientists", "CanDeployScientists [BradleyAPC]", "BradleyAPC", "CanDeployScientists", new string[] { "BaseEntity", "System.Collections.Generic.List`1<GameObjectRef>", "System.Collections.Generic.List`1<UnityEngine.Vector3>" })]
		[Identifier("62f82fb032ae42e8b5708354fa831e41")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(bool))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BradleyAPC_62f82fb032ae42e8b5708354fa831e41 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1075290686), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
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

		[Patch("OnScientistInitialized", "OnScientistInitialized [BradleyAPC]", "BradleyAPC", "InitScientist", new string[] { "ScientistNPC", "UnityEngine.Vector3", "BasePlayer", "System.Boolean", "System.Boolean" })]
		[Identifier("56117f297116449bb22ef833aabba73a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Parameter("scientist", "ScientistNPC", false)]
		[Parameter("spawnPos", "UnityEngine.Vector3", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BradleyAPC_56117f297116449bb22ef833aabba73a : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 123)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)321045627), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(Vector3));
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

		[Patch("OnScientistRecalled", "OnScientistRecalled [BradleyAPC]", "BradleyAPC", "OnScientistMounted", new string[] { "ScientistNPC" })]
		[Identifier("79c2c59777f34b39bf9f4c693cc286ae")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BradleyAPC_79c2c59777f34b39bf9f4c693cc286ae : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-674583032)), instruction), instruction);
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

	public class NPC_RustAiGen2SenseComponent
	{
		[Patch("IOnNpcTarget", "IOnNpcTarget [SenseComponent]", "Rust.Ai.Gen2.SenseComponent", "CanTarget", new string[] { "BaseEntity" })]
		[Identifier("0c617a4826b1463da335d45c086d95af")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.SenseComponent", false)]
		[Return(typeof(bool))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_RustAiGen2SenseComponent_0c617a4826b1463da335d45c086d95af : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldarg_0, (object)null), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(CorePlugin), "IOnNpcTarget", (Type[])null, (Type[])null));
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

	public class NPC_RustAiGen2StateDead
	{
		[Patch("OnCorpsePopulate", "OnCorpsePopulate [Rust.Ai.Gen2.State_Dead]", "Rust.Ai.Gen2.State_Dead", "StartRagdoll", new string[] { })]
		[Identifier("ef015cc78a8e4190ad6d5edf2f9391df")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("owner", "BaseEntity", false)]
		[Parameter("local1", "LootableCorpse", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_RustAiGen2StateDead_ef015cc78a8e4190ad6d5edf2f9391df : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 51)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)338615359), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("Rust.Ai.Gen2.State_Dead"), "Owner"));
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

		[Patch("OnCorpsePopulate", "OnCorpsePopulate [Rust.Ai.Gen2.State_Dead] [Patch]", "Rust.Ai.Gen2.State_Dead", "StartRagdoll", new string[] { })]
		[Identifier("e7534490d65f46d39a51afda8ff4a59b")]
		[Dependencies(new string[] { "OnCorpsePopulate [Rust.Ai.Gen2.State_Dead]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_RustAiGen2StateDead_e7534490d65f46d39a51afda8ff4a59b : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[121];
				list.Add(new CodeInstruction(OpCodes.Bne_Un_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[57]), list2[57]);
				}
				if (list.Count > 0)
				{
					list[0].labels.AddRange(list2[58].labels);
				}
				else
				{
					list2[59].labels.AddRange(list2[58].labels);
				}
				list2[58].labels.Clear();
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[59], list2[57]), list2[57]);
				}
				list2.RemoveRange(57, 2);
				list2.InsertRange(57, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class NPC_AIBrainSenses
	{
		[Patch("OnNpcTarget", "OnNpcTarget [AIBrainSenses]", "AIBrainSenses", "GetNearest", new string[] { "System.Collections.Generic.List`1<BaseEntity>", "System.Single" })]
		[Identifier("8812c0bf7c734788a6412b60aa6843b6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_AIBrainSenses_8812c0bf7c734788a6412b60aa6843b6 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected Obj, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected Obj, but got Unknown
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0057: Expected Obj, but got Unknown
				//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c1: Expected Obj, but got Unknown
				//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e6: Expected Obj, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				list.Add(new CodeInstruction(OpCodes.Ldstr, (object)"OnNpcTarget"));
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("AIBrainSenses"), "owner")));
				list.Add(__GeneratorRuntime.CreateLoadLocalInstruction(Generator, Method, 3, typeof(object)));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(AccessToolsEx.TypeByName("Oxide.Core.Interface"), "CallHook", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null)));
				Label label = Generator.DefineLabel();
				CodeInstruction val = list2[46];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[26]), list2[26]);
				}
				list2.InsertRange(26, list);
				val.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
