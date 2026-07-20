using System;
using System.Collections.Generic;
using System.Linq;
using Carbon.Components;
using Oxide.Core;
using Oxide.Plugins;
using UnityEngine;

namespace Carbon.Plugins;

public class CarbonPlugin : RustPlugin
{
	internal sealed class CooldownInstance
	{
		public string Command;

		public DateTime LastCall;
	}

	internal static Dictionary<BasePlayer, List<CooldownInstance>> CommandCooldownBuffer = new Dictionary<BasePlayer, List<CooldownInstance>>();

	public CUI.Handler CuiHandler { get; set; }

	public override void Setup(string name, string author, VersionNumber version, string description)
	{
		base.Setup(name, author, version, description);
		CuiHandler = new CUI.Handler();
	}

	public CUI CreateCUI()
	{
		return new CUI(CuiHandler);
	}

	public static bool IsCommandCooledDown(BasePlayer player, string command, int time, out float timeLeft, bool doCooldownIfNot = true, float appendMultiplier = 0.5f, bool doCooldownPenalty = false)
	{
		timeLeft = -1f;
		if (time == 0 || (Object)(object)player == (Object)null)
		{
			return false;
		}
		if (!CommandCooldownBuffer.TryGetValue(player, out var value))
		{
			CommandCooldownBuffer.Add(player, value = new List<CooldownInstance>());
		}
		CooldownInstance cooldownInstance = value.FirstOrDefault((CooldownInstance x) => x.Command == command);
		if (cooldownInstance == null)
		{
			List<CooldownInstance> list = value;
			CooldownInstance obj = new CooldownInstance
			{
				Command = command
			};
			cooldownInstance = obj;
			list.Add(obj);
		}
		TimeSpan timeSpan = DateTime.Now - cooldownInstance.LastCall;
		if (timeSpan.TotalMilliseconds >= (double)time)
		{
			if (doCooldownIfNot)
			{
				cooldownInstance.LastCall = DateTime.Now;
			}
			return false;
		}
		timeLeft = (float)(((double)time - timeSpan.TotalMilliseconds) * 0.0010000000474974513);
		if (doCooldownPenalty)
		{
			cooldownInstance.LastCall = cooldownInstance.LastCall.AddMilliseconds((float)time * appendMultiplier);
		}
		return true;
	}
}
