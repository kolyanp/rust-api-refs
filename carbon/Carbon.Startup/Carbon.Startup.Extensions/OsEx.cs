using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Carbon.Startup.Extensions;

public static class OsEx
{
	public static void Create(string folder, bool recreate = false)
	{
		if (!string.IsNullOrEmpty(folder))
		{
			if (recreate && Directory.Exists(folder))
			{
				Directory.Delete(folder, recursive: true);
			}
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}
		}
	}

	public static void Copy(string folder, string destination, bool overwrite = true)
	{
		if (!string.IsNullOrEmpty(folder) && !string.IsNullOrEmpty(destination) && Directory.Exists(folder))
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(folder);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			Create(destination);
			dictionary.Add(directoryInfo.FullName, destination);
			FileInfo[] files = directoryInfo.GetFiles();
			FileInfo[] array = files;
			foreach (FileInfo fileInfo in array)
			{
				string text = Path.Combine(destination, fileInfo.Name);
				fileInfo.CopyTo(text, overwrite);
				dictionary.Add(fileInfo.FullName, text);
			}
			DirectoryInfo[] array2 = directories;
			foreach (DirectoryInfo directoryInfo2 in array2)
			{
				string text2 = Path.Combine(destination, directoryInfo2.Name);
				Copy(directoryInfo2.FullName, text2, overwrite);
				dictionary.Add(directoryInfo2.FullName, text2);
			}
		}
	}

	public static void Delete(string folder)
	{
		if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder) && !string.IsNullOrEmpty(folder))
		{
			Directory.Delete(folder, recursive: true);
		}
	}

	public static List<string> Move(string folder, string destination, bool subdirectories = true, bool overwrite = true, SearchOption option = SearchOption.AllDirectories)
	{
		if (!string.IsNullOrEmpty(folder) && !string.IsNullOrEmpty(destination) && Directory.Exists(folder))
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(folder);
			List<string> list = new List<string>();
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			Create(destination);
			list.Add(destination);
			FileInfo[] files = directoryInfo.GetFiles();
			FileInfo[] array = files;
			foreach (FileInfo fileInfo in array)
			{
				string text = Path.Combine(destination, fileInfo.Name);
				fileInfo.CopyTo(text, overwrite);
				list.Add(text);
				fileInfo.Delete();
			}
			DirectoryInfo[] array2 = directories;
			foreach (DirectoryInfo directoryInfo2 in array2)
			{
				string text2 = Path.Combine(destination, directoryInfo2.Name);
				Copy(directoryInfo2.FullName, text2, overwrite);
				list.Add(text2);
				directoryInfo2.Delete(subdirectories);
			}
			Delete(folder);
			return list;
		}
		return null;
	}

	public static bool ExecuteProcess(string applicationPath, string arguments, ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden)
	{
		ProcessStartInfo startInfo = new ProcessStartInfo
		{
			FileName = applicationPath,
			Arguments = arguments,
			WindowStyle = ProcessWindowStyle.Hidden
		};
		Process process = Process.Start(startInfo);
		if (process == null)
		{
			return false;
		}
		process.WaitForExit();
		return true;
	}
}
