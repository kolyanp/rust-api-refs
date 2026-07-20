using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

namespace ConVar;

[Factory("bot")]
public class Bot : ConsoleSystem
{
	[ServerVar(ShowInAdminUI = false, Help = "(Generated) Executes a console command on a specific bot player by name or Steam ID; hidden from admin UI as it is intended for bot scripting only")]
	public static string sv_exec_command(Arg args)
	{
		if (TryGetBotServer(args, out var bot, out var error))
		{
			return error;
		}
		string text = args.GetString(1);
		if (string.IsNullOrEmpty(text))
		{
			return "No command provided";
		}
		bot.Command(text);
		return string.Empty;
	}

	[ServerVar(ShowInAdminUI = false, Help = "(Generated) Executes a console command on all bot players within a given radius of the calling admin; hidden from admin UI")]
	public static string sv_exec_command_sphere(Arg args)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		string text = args.GetString(1);
		if (string.IsNullOrEmpty(text))
		{
			return "invalid command";
		}
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return "no player context";
		}
		PooledList<BasePlayer> val = Pool.Get<PooledList<BasePlayer>>();
		try
		{
			global::Vis.Entities(((Component)basePlayer).transform.position, args.GetFloat(0, 50f), (List<BasePlayer>)(object)val, -1, (QueryTriggerInteraction)2);
			int num = 0;
			foreach (BasePlayer item in (List<BasePlayer>)(object)val)
			{
				if (item.IsBot && item.isServer && !item.IsNpc)
				{
					item.Command(text);
					num++;
				}
			}
			return $"Executed command on {num} bots.";
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(ShowInAdminUI = false, Help = "(Generated) Executes a console command on every bot player currently on the server; hidden from admin UI")]
	public static string sv_exec_command_all(Arg args)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		string text = args.GetString(0);
		if (string.IsNullOrEmpty(text))
		{
			return "invalid command";
		}
		int num = 0;
		Enumerator<BasePlayer> enumerator = BasePlayer.bots.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (current.IsBot && current.isServer && !current.IsNpc)
				{
					num++;
					current.Command(text);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return $"Executed command on {num} bots.";
	}

	[ServerVar(ShowInAdminUI = false, Help = "(Generated) Sets the ducked/crouching model state on a specific bot by name or Steam ID; used to control bot posture in testing scenarios")]
	public static string crouch_server(Arg args)
	{
		if (TryGetBotServer(args, out var bot, out var error))
		{
			return error;
		}
		bot.modelState.ducked = args.GetBool(0, def: true);
		bot.SendNetworkUpdate();
		return "Crouched " + bot.displayName + ".";
	}

	private static bool TryGetBotServer(Arg args, out BasePlayer bot, out string error)
	{
		ulong uLong = args.GetULong(0, 0uL);
		if (uLong == 0L)
		{
			bot = null;
			error = "No user id";
			return true;
		}
		bot = BasePlayer.FindBot(uLong);
		if ((Object)(object)bot == (Object)null || bot.IsNpc)
		{
			error = $"No bot found with id{uLong}";
			return true;
		}
		error = string.Empty;
		return false;
	}
}
