using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Carbon.Extensions;
using Facepunch;
using Network;

namespace Carbon.Components;

public class DevDump : IPooled
{
	public bool includeServerLog;

	public MonoProfiler.Sample sample;

	public void Init(bool includeServerLog)
	{
		this.includeServerLog = includeServerLog;
	}

	public void Export(float duration, string path, Action onComplete = null)
	{
		MonoProfiler.ToggleProfilingTimed(duration, MonoProfiler.ProfilerArgs.CallMemory | MonoProfiler.ProfilerArgs.AdvancedMemory | MonoProfiler.ProfilerArgs.Timings | MonoProfiler.ProfilerArgs.Calls | MonoProfiler.ProfilerArgs.GCEvents, delegate
		{
			sample.Resample();
			Export(path);
			onComplete?.Invoke();
		});
	}

	public void Export(string path)
	{
		using FileStream stream = new FileStream(path, FileMode.Create);
		using ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create);
		if (includeServerLog)
		{
			string text = "-logFile".GetArgumentResult() ?? "-logfile".GetArgumentResult();
			if (!string.IsNullOrEmpty(text) && OsEx.File.Exists(text))
			{
				using FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				byte[] array = BaseNetwork.ArrayPool.Rent((int)fileStream.Length * 2);
				fileStream.Read(array, 0, (int)fileStream.Length);
				AddFile(archive, "output.log", array);
				BaseNetwork.ArrayPool.Return(array);
			}
			else if (!string.IsNullOrEmpty(text))
			{
				Logger.Log("Couldn't find log file '" + Path.GetFullPath(text) + "'");
			}
		}
		if (!sample.IsCleared)
		{
			AddFile(archive, "profile.cprf", sample.ToProto());
			AddFile(archive, "profile.csv", Encoding.UTF8.GetBytes(sample.ToCSV()));
		}
	}

	private static void AddFile(ZipArchive archive, string fileName, byte[] content)
	{
		ZipArchiveEntry zipArchiveEntry = archive.CreateEntry(fileName);
		using Stream stream = zipArchiveEntry.Open();
		stream.Write(content, 0, content.Length);
	}

	public void EnterPool()
	{
		includeServerLog = false;
		sample.Clear();
	}

	public void LeavePool()
	{
	}
}
