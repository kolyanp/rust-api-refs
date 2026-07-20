using System;
using System.Threading.Tasks;
using API.Commands;
using API.Hooks;
using Carbon.Core;
using ConVar;

namespace Carbon.Hooks;

public class Category_Player
{
	public class BasePlayer_Player
	{
		[Patch("IOnPlayerChat", "IOnPlayerChat", typeof(Chat), "sayAs", new Type[]
		{
			typeof(ChatChannel),
			typeof(ulong),
			typeof(string),
			typeof(string),
			typeof(BasePlayer)
		})]
		[Options(/*Could not decode attribute arguments.*/)]
		[OxideCompatible]
		public class IOnPlayerChat : Patch
		{
			public static bool IsValidCommand(string input, BasePlayer player, out Prefix prefix)
			{
				prefix = null;
				if (string.IsNullOrEmpty(input))
				{
					return false;
				}
				if (Command.Prefixes == null)
				{
					Logger.Error((object)"This is really bad. Let the devs know ASAP, unless some plugin broke this. (Prefixes == null)", (Exception)null);
					return false;
				}
				if (!Command.HasPrefix(input, ref prefix))
				{
					return false;
				}
				if (prefix.PrintToChat)
				{
					player.ChatMessage("<color=orange>Command:</color> " + input);
				}
				if (prefix.PrintToConsole)
				{
					ServerConsole.PrintColoured($"[{player.Connection}]: {input}", ConsoleColor.DarkYellow);
				}
				return true;
			}

			public static bool Prefix(ChatChannel targetChannel, ulong userId, string username, ref string message, BasePlayer player, ValueTask<bool> __result)
			{
				//IL_003f: Unknown result type (might be due to invalid IL or missing references)
				if (string.IsNullOrEmpty(message))
				{
					return true;
				}
				if (IsValidCommand(message, player, out var prefix) && CorePlugin.IOnPlayerCommand(player, message, prefix) is bool result)
				{
					__result = new ValueTask<bool>(result);
					return false;
				}
				if (!(CorePlugin.IOnPlayerChat(userId, username, ref message, targetChannel, player) is bool result2))
				{
					return true;
				}
				__result = new ValueTask<bool>(result2);
				return false;
			}
		}
	}
}
