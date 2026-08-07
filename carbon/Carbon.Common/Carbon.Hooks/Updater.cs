using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Carbon.Core;

namespace Carbon.Hooks;

public sealed class Updater
{
	private static string BuildUrl(string file, string protocol = null)
	{
		string text = ((Community.Runtime.Analytics.Platform == "linux") ? "unix" : null);
		string text2 = ((Community.Runtime.Analytics.Branch == "Release") ? "release" : "debug");
		return "https://cdn.carbonmod.gg/hooks/server/" + text2 + text + "/" + ((protocol == null) ? (file ?? "") : (protocol + "/" + file));
	}

	public static void DoUpdate(Action<bool> callback = null)
	{
		FireAndForget(DoUpdateInternalAsync(callback));
	}

	private static async Task DoUpdateInternalAsync(Action<bool> callback = null)
	{
		bool success = false;
		try
		{
			IReadOnlyList<string> readOnlyList = new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "carbon/managed/hooks/Carbon.Hooks.Community.dll", "carbon/managed/hooks/Carbon.Hooks.Oxide.dll" });
			List<Task<bool>> list = new List<Task<bool>>();
			foreach (string item in readOnlyList)
			{
				list.Add(UpdateFileAsync(item));
			}
			int failed = 0;
			bool[] array = await Task.WhenAll(list);
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i])
				{
					failed++;
				}
			}
			success = failed == 0;
		}
		catch (Exception ex)
		{
			Logger.Error("Hook update failed", ex);
			success = false;
		}
		finally
		{
			if (callback != null)
			{
				try
				{
					callback(success);
				}
				catch (Exception ex2)
				{
					Logger.Error("Hook update callback failed", ex2);
				}
			}
		}
	}

	private static void FireAndForget(Task task)
	{
		task.ContinueWith(delegate(Task t)
		{
			Logger.Error("Hook update task failed", t.Exception);
		}, TaskContinuationOptions.OnlyOnFaulted);
	}

	private static async Task<bool> UpdateFileAsync(string file)
	{
		string fileName = Path.GetFileName(file);
		try
		{
			if (!Community.Runtime.Config.Logging.ReducedLogging)
			{
				Logger.Warn("Updating component '" + fileName + "@" + Community.Runtime.Analytics.Protocol + "' on " + Community.Runtime.Analytics.Platform + " [" + Community.Runtime.Analytics.Branch + "]");
			}
			byte[] array = await DownloadFile(file, Community.Runtime.Analytics.Protocol);
			if (array == null || array.Length < 1)
			{
				Logger.Warn("Retrying component update '" + fileName + "' on " + Community.Runtime.Analytics.Platform + " [" + Community.Runtime.Analytics.Branch + "]...");
				array = await DownloadFile(file);
			}
			if (array == null || array.Length < 1)
			{
				Logger.Warn("Unable to update component '" + fileName + "', please try again later");
				return false;
			}
			try
			{
				File.WriteAllBytes(Path.Combine(Defines.GetHooksFolder(), fileName), array);
				return true;
			}
			catch (Exception ex)
			{
				Logger.Error("Error while updating component '" + fileName + "'", ex);
				return false;
			}
		}
		catch (Exception ex2)
		{
			Logger.Error("Unexpected error while updating component '" + fileName + "'", ex2);
			return false;
		}
	}

	private static async Task<byte[]> DownloadFile(string file, string protocol = null)
	{
		TimeSpan timeoutSpan = TimeSpan.FromSeconds(60.0);
		using CancellationTokenSource timeoutCts = new CancellationTokenSource(timeoutSpan);
		string url = BuildUrl(file, protocol);
		byte[] result = await Community.Runtime.Downloader.Download(url, timeoutCts.Token);
		if (timeoutCts.IsCancellationRequested)
		{
			Logger.Warn($"Timed out downloading '{file}' after {timeoutSpan.Seconds}s");
		}
		return result;
	}
}
