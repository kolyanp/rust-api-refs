using System;
using System.IO;
using Unity.Profiling.Memory;
using UnityEngine;

namespace ConVar;

[Factory("memsnap")]
public class MemSnap : ConsoleSystem
{
	private static string NeedProfileFolder()
	{
		string path = "profile/memory";
		if (!Directory.Exists(path))
		{
			return Directory.CreateDirectory(path).FullName;
		}
		return new DirectoryInfo(path).FullName;
	}

	private static bool IsDebugBuild(Arg arg)
	{
		if (Debug.isDebugBuild)
		{
			return true;
		}
		arg.ReplyWith("Memory snapshots are only supported in development builds!");
		return false;
	}

	private static void OnSnapshotFinished(string path, bool success)
	{
		if (!success)
		{
			Debug.LogWarning((object)("Memory snapshot failed to save to " + path));
		}
	}

	[ClientVar(Help = "(Generated) Takes a Unity Memory Profiler snapshot capturing managed (C#) heap allocations and saves it as a timestamped .snap file in the profile/memory/ folder")]
	[ServerVar(Help = "(Generated) Takes a Unity Memory Profiler snapshot capturing managed (C#) heap allocations and saves it as a timestamped .snap file in the profile/memory/ folder")]
	public static void managed(Arg arg)
	{
		if (IsDebugBuild(arg))
		{
			string text = NeedProfileFolder() + "/memdump-" + DateTime.Now.ToString("MM-dd-yyyy-h-mm-ss") + ".snap";
			MemoryProfiler.TakeSnapshot(text, (Action<string, bool>)OnSnapshotFinished, (CaptureFlags)1);
			arg.ReplyWith("Saving memory snapshot to " + text);
		}
	}

	[ClientVar(Help = "(Generated) Takes a Unity Memory Profiler snapshot capturing native (C++) heap allocations and saves it as a timestamped .snap file in the profile/memory/ folder")]
	[ServerVar(Help = "(Generated) Takes a Unity Memory Profiler snapshot capturing native (C++) heap allocations and saves it as a timestamped .snap file in the profile/memory/ folder")]
	public static void native(Arg arg)
	{
		if (IsDebugBuild(arg))
		{
			string text = NeedProfileFolder() + "/memdump-" + DateTime.Now.ToString("MM-dd-yyyy-h-mm-ss") + ".snap";
			MemoryProfiler.TakeSnapshot(text, (Action<string, bool>)OnSnapshotFinished, (CaptureFlags)2);
			arg.ReplyWith("Saving memory snapshot to " + text);
		}
	}

	[ClientVar(Help = "(Generated) Takes a full Unity Memory Profiler snapshot capturing all managed, native, and graphics memory and saves it as a timestamped .snap file in profile/memory/")]
	[ServerVar(Help = "(Generated) Takes a full Unity Memory Profiler snapshot capturing all managed, native, and graphics memory and saves it as a timestamped .snap file in profile/memory/")]
	public static void full(Arg arg)
	{
		if (IsDebugBuild(arg))
		{
			string text = NeedProfileFolder() + "/memdump-" + DateTime.Now.ToString("MM-dd-yyyy-h-mm-ss") + ".snap";
			MemoryProfiler.TakeSnapshot(text, (Action<string, bool>)OnSnapshotFinished, (CaptureFlags)31);
			arg.ReplyWith("Saving memory snapshot to " + text);
		}
	}
}
