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
		[Identifier("975efda79b3d48adadc04bcab2ed28fa")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNpc", false)]
		[Return(typeof(bool))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BaseNpc_975efda79b3d48adadc04bcab2ed28fa : Patch
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
		[Identifier("17e179c6eb734630b419cd9de9330a2c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNpc", false)]
		[Parameter("self1", "BaseNpc", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BaseNpc_17e179c6eb734630b419cd9de9330a2c : Patch
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
		[Identifier("e097c5172cc24b12aaffb89fce22d077")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseNpc", false)]
		[Return(typeof(float))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BaseNpc_e097c5172cc24b12aaffb89fce22d077 : Patch
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
		[Identifier("80b9bdb8b4054fe5988027beb850a24d")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NPCVendingMachine", false)]
		[Parameter("soldItem", "Item", false)]
		[Parameter("buyer", "BasePlayer", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_NPCVendingMachine_80b9bdb8b4054fe5988027beb850a24d : Patch
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
		[Identifier("a7bbeab3c4c74e6eaaa6a62d8a005074")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ScientistNPC", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_ScientistNPC_a7bbeab3c4c74e6eaaa6a62d8a005074 : Patch
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
		[Identifier("8991ab3d3f8c4085b5251f8a50456a7e")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ScientistNPC", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_ScientistNPC_8991ab3d3f8c4085b5251f8a50456a7e : Patch
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
		[Identifier("63d5f12ed14e4b56a8f530f07762b44c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "ScientistNPC", false)]
		[Parameter("local1", "Item", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_ScientistNPC_63d5f12ed14e4b56a8f530f07762b44c : Patch
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
		[Identifier("844f212764a04a46a231d61354e03e03")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NPCPlayer", false)]
		[Parameter("local0", "Item", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_NPCPlayer_844f212764a04a46a231d61354e03e03 : Patch
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
		[Identifier("caaaa20ba89845bf8ac59c0af973080f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NPCPlayer", false)]
		[Parameter("local1", "NPCPlayerCorpse", false)]
		[Return(typeof(BaseCorpse))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_NPCPlayer_caaaa20ba89845bf8ac59c0af973080f : Patch
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
		[Identifier("3f4b479a2c5443beb1a83b39e827dd2b")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "HumanNPC", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_HumanNPC_3f4b479a2c5443beb1a83b39e827dd2b : Patch
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
		[Identifier("920e17f347db478a9f607753a9688af7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_HumanNPC_920e17f347db478a9f607753a9688af7 : Patch
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
				CodeInstruction obj = list2[85];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[22]), list2[22]);
				}
				list2.InsertRange(22, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class NPC_NPCTalking
	{
		[Patch("OnNpcConversationRespond", "OnNpcConversationRespond", "NPCTalking", "Server_ResponsePressed", new string[] { "BaseEntity/RPCMessage" })]
		[Identifier("3a10807714dc47908ba3eb3ccc77ffd2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NPCTalking", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local5", "ConversationData", false)]
		[Parameter("local17", "ConversationData+ResponseNode", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_NPCTalking_3a10807714dc47908ba3eb3ccc77ffd2 : Patch
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
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1580074841), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)17);
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
		[Identifier("5e8bc60c893749fa830fd716a962704d")]
		[Dependencies(new string[] { "OnNpcConversationRespond" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NPCTalking", false)]
		[Parameter("local0", "BasePlayer", false)]
		[Parameter("local5", "ConversationData", false)]
		[Parameter("local17", "ConversationData+ResponseNode", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_NPCTalking_5e8bc60c893749fa830fd716a962704d : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 190)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1571158728)), instruction), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)5);
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)17);
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
		[Identifier("acf2c2fbbe1346cbbaf5e4ea24c003fe")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NPCTalking", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_NPCTalking_acf2c2fbbe1346cbbaf5e4ea24c003fe : Patch
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
		[Identifier("678ba12db4de454bb10c86ca3b4690a8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "NPCTalking", false)]
		[Parameter("ply", "BasePlayer", false)]
		[Parameter("local2", "ConversationData", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_NPCTalking_678ba12db4de454bb10c86ca3b4690a8 : Patch
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
		[Identifier("f6d2188cc1f24588854f11f8d7f440ff")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("owner", "BaseEntity", false)]
		[Parameter("ent", "BaseEntity", false)]
		[Parameter("brainSenses", "AIBrainSenses", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_RustAiSimpleAIMemory_f6d2188cc1f24588854f11f8d7f440ff : Patch
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
		[Identifier("b36b99260242456abe5eabd3b2591706")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseAIBrain", false)]
		[Parameter("self1", "BaseAIBrain", false)]
		[Parameter("newState", "BaseAIBrain+BasicAIState", false)]
		[Return(typeof(bool))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BaseAIBrain_b36b99260242456abe5eabd3b2591706 : Patch
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
		[Identifier("fe8b9fd3f28b4ec1a2857ebf1f240696")]
		[Dependencies(new string[] { "OnAIBrainStateSwitch" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BaseAIBrain", false)]
		[Parameter("self1", "BaseAIBrain", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BaseAIBrain_fe8b9fd3f28b4ec1a2857ebf1f240696 : Patch
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
		[Identifier("7df782091001429d84696606f80caa72")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(bool))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BradleyAPC_7df782091001429d84696606f80caa72 : Patch
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
		[Identifier("9d99dd5a0f214e249046b1edf229294c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Parameter("scientist", "ScientistNPC", false)]
		[Parameter("spawnPos", "UnityEngine.Vector3", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BradleyAPC_9d99dd5a0f214e249046b1edf229294c : Patch
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
		[Identifier("49eeb77c537940e9b410f681e81310cd")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "BradleyAPC", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_BradleyAPC_49eeb77c537940e9b410f681e81310cd : Patch
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
		[Identifier("b864ad10d4904402bc2b47fee8e5b91f")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.SenseComponent", false)]
		[Return(typeof(bool))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_RustAiGen2SenseComponent_b864ad10d4904402bc2b47fee8e5b91f : Patch
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
		[Identifier("48db24ca66f3413d8de7872a71a06cd7")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("self", "Rust.Ai.Gen2.State_Dead", false)]
		[Parameter("local1", "LootableCorpse", false)]
		[Return(typeof(void))]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_RustAiGen2StateDead_48db24ca66f3413d8de7872a71a06cd7 : Patch
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
		[Identifier("33c48ee34bc24aad8e6793174e802d6c")]
		[Dependencies(new string[] { "OnCorpsePopulate [Rust.Ai.Gen2.State_Dead]" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_RustAiGen2StateDead_33c48ee34bc24aad8e6793174e802d6c : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				Label label = Generator.DefineLabel();
				CodeInstruction obj = list2[121];
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
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}

	public class NPC_AIBrainSenses
	{
		[Patch("OnNpcTarget", "OnNpcTarget [AIBrainSenses]", "AIBrainSenses", "GetNearest", new string[] { "System.Collections.Generic.List`1<BaseEntity>", "System.Single" })]
		[Identifier("0457381c314641f1a8bfa674eb0f52b8")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("NPC")]
		[Assembly("Assembly-CSharp.dll")]
		public class NPC_AIBrainSenses_0457381c314641f1a8bfa674eb0f52b8 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0057: Expected O, but got Unknown
				//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c1: Expected O, but got Unknown
				//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e6: Expected O, but got Unknown
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
				CodeInstruction obj = list2[46];
				list.Add(new CodeInstruction(OpCodes.Brtrue_S, (object)label));
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[26]), list2[26]);
				}
				list2.InsertRange(26, list);
				obj.labels.Add(label);
				return list2.AsEnumerable();
			}
		}
	}
}
