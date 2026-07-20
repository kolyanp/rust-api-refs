using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace ConVar;

[ConsoleSystem.Factory("system")]
public static class SystemCommands
{
	public static bool appliedManualCpuAffinity;

	[ClientVar(Help = "(Generated) Sets the CPU core affinity mask for the process using comma-separated core indices or dash-separated ranges (e.g. 0,2-5)")]
	[ServerVar(Help = "(Generated) Sets the CPU core affinity mask for the process using comma-separated core indices or dash-separated ranges (e.g. 0,2-5)")]
	public static void cpu_affinity(ConsoleSystem.Arg arg)
	{
		ulong num = 0uL;
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Format is 'cpu_affinity {core,core1-core2,etc}'");
			return;
		}
		string[] array = arg.GetString(0).Split(',');
		HashSet<int> hashSet = new HashSet<int>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (int.TryParse(text, out var result))
			{
				hashSet.Add(result);
			}
			else
			{
				if (!text.Contains('-'))
				{
					continue;
				}
				string[] array3 = text.Split('-');
				if (array3.Length != 2)
				{
					arg.ReplyWith("Failed to parse section " + text + ", format should be '0-15'");
					continue;
				}
				if (!int.TryParse(array3[0], out var result2) || !int.TryParse(array3[1], out var result3))
				{
					arg.ReplyWith("Core range in section " + text + " are not valid numbers, format should be '0-15'");
					continue;
				}
				if (result2 > result3)
				{
					arg.ReplyWith("Core range in section " + text + " are not ordered from least to greatest, format should be '0-15'");
					continue;
				}
				if (result3 - result2 > 64)
				{
					arg.ReplyWith("Core range in section " + text + " are too big of a range, must be <64");
					return;
				}
				for (int j = result2; j <= result3; j++)
				{
					hashSet.Add(j);
				}
			}
		}
		if (hashSet.Any((int x) => x < 0 || x > 63))
		{
			arg.ReplyWith("Cores provided out of range! Must be in between 0 and 63");
			return;
		}
		for (int num2 = 0; num2 < 64; num2++)
		{
			if (hashSet.Contains(num2))
			{
				num |= (ulong)(1L << num2);
			}
		}
		if (num == 0L)
		{
			arg.ReplyWith("No cores provided (bitmask empty)! Format is 'cpu_affinity {core,core1-core2,etc}'");
		}
		else if (SetCpuAffinity(num))
		{
			appliedManualCpuAffinity = true;
			arg.ReplyWith("Successfully changed CPU affinity");
		}
	}

	public static bool SetCpuAffinity(ulong affinityMask)
	{
		try
		{
			WindowsAffinityShim.SetProcessAffinityMask(Process.GetCurrentProcess().Handle, (IntPtr)(long)affinityMask);
			return true;
		}
		catch (Exception arg)
		{
			Debug.LogWarning((object)$"Unable to set CPU affinity: {arg}");
			return false;
		}
	}

	[ServerVar(Help = "(Generated) Sets the OS process priority class (belownormal, normal, abovenormal, high); Idle and Realtime are blocked; not supported on OSX")]
	[ClientVar(Help = "(Generated) Sets the OS process priority class (belownormal, normal, abovenormal, high); Idle and Realtime are blocked; not supported on OSX")]
	public static void cpu_priority(ConsoleSystem.Arg arg)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)Application.platform == 1)
		{
			arg.ReplyWith("OSX is not a supported platform");
			return;
		}
		string text = arg.GetString(0);
		ProcessPriorityClass mask;
		switch (text.Replace("-", "").Replace("_", ""))
		{
		case "belownormal":
			mask = ProcessPriorityClass.BelowNormal;
			break;
		case "normal":
			mask = ProcessPriorityClass.Normal;
			break;
		case "abovenormal":
			mask = ProcessPriorityClass.AboveNormal;
			break;
		case "high":
			mask = ProcessPriorityClass.High;
			break;
		default:
			arg.ReplyWith("Unknown priority '" + text + "', possible values: below_normal, normal, above_normal, high");
			return;
		}
		try
		{
			WindowsAffinityShim.SetPriorityClass(Process.GetCurrentProcess().Handle, (uint)mask);
		}
		catch (Exception arg2)
		{
			Debug.LogWarning((object)$"Unable to set cpu priority: {arg2}");
			return;
		}
		arg.ReplyWith("Successfully changed cpu priority to " + mask);
	}
}
