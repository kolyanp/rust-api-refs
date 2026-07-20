using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using Facepunch.Extend;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Carbon;

public class SelfUpdate
{
	public const string Endpoint = "https://api.carbonmod.gg/releases";

	public static readonly string DownloadEndpoint = "https://github.com/CarbonCommunity/Carbon/releases/download/profiler_build/Carbon.Windows.Profiler.zip";

	public static readonly Version CurrentVersion = typeof(SelfUpdate).Assembly.GetName().Version;

	private static Uri endpointUri = new Uri("https://api.carbonmod.gg/releases");

	private static Uri downloadEndpointUri = new Uri(DownloadEndpoint);

	private static WebClient apiClient;

	private static WebClient updateClient;

	public static void Api(Action<JArray> callback)
	{
		if (apiClient == null)
		{
			apiClient = new WebClient();
			apiClient.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs args)
			{
				if (args.Error != null)
				{
					Debug.LogException(args.Error);
				}
				else
				{
					callback?.Invoke(JArray.Parse(args.Result));
				}
			};
		}
		apiClient.DownloadStringAsync(endpointUri);
	}

	public static void Update(Action callback)
	{
		if (updateClient != null)
		{
			Debug.LogWarning((object)" Update already in progress..");
			return;
		}
		updateClient = new WebClient();
		updateClient.DownloadDataCompleted += delegate(object sender, DownloadDataCompletedEventArgs args)
		{
			if (args.Error == null)
			{
				using MemoryStream stream = new MemoryStream(args.Result);
				Debug.Log((object)"Updating Carbon.Profiler..");
				string modPath = HarmonyLoader.modPath;
				using ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
				foreach (ZipArchiveEntry entry in zipArchive.Entries)
				{
					if (!string.IsNullOrEmpty(entry.Name))
					{
						string path = Path.Combine(modPath, entry.FullName);
						Directory.CreateDirectory(Path.GetDirectoryName(path));
						using Stream stream2 = entry.Open();
						try
						{
							using FileStream destination = new FileStream(path, FileMode.Create, FileAccess.Write);
							stream2.CopyTo(destination);
							Debug.Log((object)(" - " + entry.FullName + " (" + NumberExtensions.FormatBytes<long>(entry.Length, true) + ")"));
						}
						catch (Exception ex)
						{
							Debug.LogWarning((object)(" - " + entry.FullName + " (" + NumberExtensions.FormatBytes<long>(entry.Length, true) + ") [skipped]: " + ex.Message));
						}
					}
				}
				callback?.Invoke();
			}
			updateClient.Dispose();
			updateClient = null;
		};
		updateClient.DownloadDataAsync(downloadEndpointUri);
	}
}
