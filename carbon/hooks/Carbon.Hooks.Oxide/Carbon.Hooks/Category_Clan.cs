using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class Category_Clan
{
	public class Clan_LocalClanDisbandd72
	{
		[Patch("OnClanDisbanded", "OnClanDisbanded", "LocalClan/<Disband>d__72", "MoveNext", new string[] { })]
		[Identifier("08b8cac2107346269030e47340153d17")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "LocalClan", false)]
		[Parameter("bySteamId", "System.UInt64", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Clan")]
		[Assembly("Rust.Clans.Local.dll")]
		public class Clan_LocalClanDisbandd72_08b8cac2107346269030e47340153d17 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 74)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-1086123373)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("LocalClan+<Disband>d__72"), "bySteamId"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt64"));
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

	public class Clan_LocalClanBackendCreated11
	{
		[Patch("OnClanCreated", "OnClanCreated", "LocalClanBackend", "Create", new string[] { "System.UInt64", "System.String" })]
		[Identifier("9740425530884ddaa47b119e8a10c2a6")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("clan", "LocalClan", false)]
		[Parameter("leaderSteamId", "System.UInt64", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Clan")]
		[Assembly("Rust.Clans.Local.dll")]
		public class Clan_LocalClanBackendCreated11_9740425530884ddaa47b119e8a10c2a6 : Patch
		{
			public static void Postfix(ulong leaderSteamId, ref ValueTask<ClanValueResult<IClan>> __result)
			{
				__result = AwaitHookResult(__result, leaderSteamId);
			}

			private static async ValueTask<ClanValueResult<IClan>> AwaitHookResult(ValueTask<ClanValueResult<IClan>> original, ulong leaderSteamId)
			{
				ClanValueResult<IClan> result = await original;
				if (result.IsSuccess)
				{
					IClan value = result.Value;
					LocalClan val = (LocalClan)(object)((value is LocalClan) ? value : null);
					if (val != null)
					{
						HookCaller.CallStaticHook(3053309100u, (object)val, (object)leaderSteamId);
					}
				}
				return result;
			}
		}
	}

	public class Clan_LocalClanDatabase
	{
		[Patch("OnClanMemberAdded", "OnClanMemberAdded", "LocalClanDatabase", "AcceptInvite", new string[] { "System.Int64", "System.UInt64" })]
		[Identifier("e1a8fa9f4ccb44c39b3dc6e948973da4")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Return(typeof(void), Discarded = true)]
		[Category("Clan")]
		[Assembly("Rust.Clans.Local.dll")]
		public class Clan_LocalClanDatabase_e1a8fa9f4ccb44c39b3dc6e948973da4 : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)895202397), instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(long));
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Box, (object)typeof(ulong));
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

	public class Clan_LocalClanKickd65
	{
		[Patch("OnClanMemberLeft", "OnClanMemberLeft", "LocalClan/<Kick>d__65", "MoveNext", new string[] { })]
		[Identifier("86202af698e94c2d95070e148d92d30c")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "LocalClan", false)]
		[Parameter("steamId", "System.UInt64", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Clan")]
		[Assembly("Rust.Clans.Local.dll")]
		public class Clan_LocalClanKickd65_86202af698e94c2d95070e148d92d30c : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1270737373), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("LocalClan+<Kick>d__65"), "steamId"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt64"));
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

		[Patch("OnClanMemberKicked", "OnClanMemberKicked", "LocalClan/<Kick>d__65", "MoveNext", new string[] { })]
		[Identifier("b4bef552501c4e0bb2191ffed6f53fc1")]
		[Dependencies(new string[] { "OnClanMemberLeft" })]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local1", "LocalClan", false)]
		[Parameter("steamId", "System.UInt64", false)]
		[Parameter("bySteamId", "System.UInt64", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Clan")]
		[Assembly("Rust.Clans.Local.dll")]
		public class Clan_LocalClanKickd65_b4bef552501c4e0bb2191ffed6f53fc1 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 144)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)151056655), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("LocalClan+<Kick>d__65"), "steamId"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt64"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("LocalClan+<Kick>d__65"), "bySteamId"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt64"));
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

	public class Clan_LocalClanSetColord61
	{
		[Patch("OnClanColorChanged", "OnClanColorChanged", "LocalClan/<SetColor>d__61", "MoveNext", new string[] { })]
		[Identifier("334dd1d083304bb1acb8a8a1e6d11fb2")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "LocalClan", false)]
		[Parameter("newColor", "UnityEngine.Color32", false)]
		[Parameter("bySteamId", "System.UInt64", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Clan")]
		[Assembly("Rust.Clans.Local.dll")]
		public class Clan_LocalClanSetColord61_334dd1d083304bb1acb8a8a1e6d11fb2 : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				int x = 0;
				foreach (CodeInstruction instruction in Instructions)
				{
					if (x++ != 54)
					{
						yield return instruction;
						continue;
					}
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)1049643152), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("LocalClan+<SetColor>d__61"), "newColor"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("UnityEngine.Color32"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("LocalClan+<SetColor>d__61"), "bySteamId"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt64"));
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

	public class Clan_LocalClanSetLogod60
	{
		[Patch("OnClanLogoChanged", "OnClanLogoChanged", "LocalClan/<SetLogo>d__60", "MoveNext", new string[] { })]
		[Identifier("e980cf2c839f47fe98db296f17df105a")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Parameter("local0", "LocalClan", false)]
		[Parameter("newLogo", "System.Byte[]", false)]
		[Parameter("bySteamId", "System.UInt64", false)]
		[Return(typeof(void), Discarded = true)]
		[Category("Clan")]
		[Assembly("Rust.Clans.Local.dll")]
		public class Clan_LocalClanSetLogod60_e980cf2c839f47fe98db296f17df105a : Patch
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
					yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldc_I4, (object)(-539002240)), instruction);
					yield return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("LocalClan+<SetLogo>d__60"), "newLogo"));
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(AccessToolsEx.TypeByName("LocalClan+<SetLogo>d__60"), "bySteamId"));
					yield return new CodeInstruction(OpCodes.Box, (object)AccessToolsEx.TypeByName("System.UInt64"));
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

		[Patch("OnClanLogoChanged [patch]", "OnClanLogoChanged [patch]", "LocalClan/<SetLogo>d__60", "MoveNext", new string[] { })]
		[Identifier("51d36c73b4d94a7ea30e4f7f5ec4a5ce")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Clan")]
		[Assembly("Rust.Clans.Local.dll")]
		public class Clan_LocalClanSetLogod60_51d36c73b4d94a7ea30e4f7f5ec4a5ce : Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator, MethodBase Method)
			{
				List<CodeInstruction> list = new List<CodeInstruction>();
				List<CodeInstruction> list2 = new List<CodeInstruction>(Instructions);
				if (list.Count > 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list[0], list2[66]), list2[66]);
				}
				if (list.Count == 0)
				{
					CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(list2[67], list2[66]), list2[66]);
				}
				list2.RemoveRange(66, 1);
				list2.InsertRange(66, list);
				return list2.AsEnumerable();
			}
		}
	}
}
