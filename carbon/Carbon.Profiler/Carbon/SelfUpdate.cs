using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using Facepunch.Extend;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Carbon;

public class SelfUpdate
{
	public const string Endpoint = "https://api.carbonmod.gg/releases";

	public static readonly string DownloadEndpoint = "https://github.com/CarbonCommunity/Carbon/releases/download/profiler_build/Carbon.Windows.Profiler.zip";

	public static readonly Version CurrentVersion = typeof(SelfUpdate).Assembly.GetName().Version;

	private const string StagedSuffix = ".new";

	private const string ReplacedSuffix = ".old";

	private static Uri endpointUri = new Uri("https://api.carbonmod.gg/releases");

	private static Uri downloadEndpointUri = new Uri(DownloadEndpoint);

	private static WebClient apiClient;

	private static WebClient updateClient;

	private static bool updateInstalled;

	public static void Api(Action<JArray> callback)
	{
		if (apiClient == null)
		{
			apiClient = new WebClient();
			apiClient.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs args) =>
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
		if (updateInstalled)
		{
			Debug.LogWarning((object)" An update is already installed. Restart the server to load it.");
			return;
		}
		if (updateClient != null)
		{
			Debug.LogWarning((object)" Update already in progress..");
			return;
		}
		updateClient = new WebClient();
		updateClient.DownloadDataCompleted += (object sender, DownloadDataCompletedEventArgs args) =>
		{
			if (args.Error == null)
			{
				Debug.Log((object)"Updating Carbon.Profiler..");
				updateInstalled = Install(ReadZip(args.Result));
				if (updateInstalled)
				{
					callback?.Invoke();
				}
			}
			else
			{
				Debug.LogError((object)(" Couldn't download the update: " + args.Error.Message));
			}
			updateClient.Dispose();
			updateClient = null;
		};
		updateClient.DownloadDataAsync(downloadEndpointUri);
	}

	private static bool Install(List<(string Path, byte[] Data)> files)
	{
		List<string> list = new List<string>();
		try
		{
			foreach (var file in files)
			{
				string item = file.Path;
				byte[] item2 = file.Data;
				string text = Path.Combine(HarmonyLoader.modPath, item);
				Directory.CreateDirectory(Path.GetDirectoryName(text));
				File.WriteAllBytes(text + ".new", item2);
				list.Add(text);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)(" Couldn't write the update, nothing was replaced: " + ex.Message));
			list.ForEach((string destination) =>
			{
				File.Delete(destination + ".new");
			});
			return false;
		}
		bool result = true;
		for (int num = 0; num < list.Count; num++)
		{
			string text2 = list[num];
			string text3 = text2 + ".old";
			try
			{
				File.Delete(text3);
				if (File.Exists(text2))
				{
					File.Move(text2, text3);
				}
				File.Move(text2 + ".new", text2);
				Debug.Log((object)(" - " + files[num].Path + " (" + NumberExtensions.FormatBytes<int>(files[num].Data.Length, true) + ")"));
			}
			catch (Exception ex2)
			{
				result = false;
				Debug.LogError((object)(" - " + files[num].Path + " [failed]: " + ex2.Message));
				continue;
			}
			DeleteUnlessLoaded(text3);
		}
		return result;
	}

	private static void DeleteUnlessLoaded(string path)
	{
		try
		{
			File.Delete(path);
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
		}
	}

	private static List<(string Path, byte[] Data)> ReadZip(byte[] archive)
	{
		using ZipArchive zipArchive = new ZipArchive(new MemoryStream(archive), ZipArchiveMode.Read);
		return zipArchive.Entries.Where((ZipArchiveEntry entry) => !string.IsNullOrEmpty(entry.Name)).Select((ZipArchiveEntry entry) =>
		{
			using Stream stream = entry.Open();
			using MemoryStream memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);
			return (FullName: entry.FullName, memoryStream.ToArray());
		}).ToList();
	}

	private static List<(string Path, byte[] Data)> ReadTarGz(byte[] archive)
	{
		List<(string, byte[])> list = new List<(string, byte[])>();
		using GZipStream stream = new GZipStream(new MemoryStream(archive), CompressionMode.Decompress);
		byte[] array = new byte[512];
		while (ReadExactly(stream, array) && array[0] != 0)
		{
			long num = Convert.ToInt64(ReadField(array, 124, 12), 8);
			byte[] array2 = new byte[num];
			ReadExactly(stream, array2);
			ReadExactly(stream, new byte[(512 - num % 512) % 512]);
			char c = (char)array[156];
			if ((c == '\0' || c == '0') ? true : false)
			{
				string text = ReadField(array, 345, 155);
				string text2 = ReadField(array, 0, 100);
				list.Add(((text.Length == 0) ? text2 : (text + "/" + text2), array2));
			}
		}
		return list;
	}

	private static string ReadField(byte[] header, int offset, int length)
	{
		int num = Array.IndexOf(header, (byte)0, offset, length);
		return Encoding.UTF8.GetString(header, offset, ((num < 0) ? (offset + length) : num) - offset).Trim();
	}

	private static bool ReadExactly(Stream stream, byte[] buffer)
	{
		int num;
		for (int i = 0; i < buffer.Length; i += num)
		{
			num = stream.Read(buffer, i, buffer.Length - i);
			if (num == 0)
			{
				return false;
			}
		}
		return true;
	}
}
