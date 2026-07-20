using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Carbon.Extensions;

public class OsEx
{
	public static class File
	{
		public static readonly string EMPTY_STRING = string.Empty;

		public static readonly string[] EMPTY_STRARRAY = new string[0];

		public static readonly byte[] EMPTY_BYTEARRAY = new byte[0];

		public static bool Exists(string file)
		{
			if (string.IsNullOrEmpty(file))
			{
				return false;
			}
			return System.IO.File.Exists(file);
		}

		public static void Create(string file, string content)
		{
			if (!string.IsNullOrEmpty(file))
			{
				System.IO.File.WriteAllText(file, content);
			}
		}

		public static void Create(string file, string[] contents)
		{
			if (!string.IsNullOrEmpty(file))
			{
				System.IO.File.WriteAllLines(file, contents);
			}
		}

		public static void Create(string file, byte[] contents)
		{
			if (!string.IsNullOrEmpty(file))
			{
				System.IO.File.WriteAllBytes(file, contents);
			}
		}

		public static void Delete(string file)
		{
			if (!string.IsNullOrEmpty(file) && System.IO.File.Exists(file))
			{
				System.IO.File.Delete(file);
			}
		}

		public static void Append(string file, string content)
		{
			if (!string.IsNullOrEmpty(file))
			{
				System.IO.File.AppendAllText(file, content);
			}
		}

		public static string Copy(string file, string destination, bool overwrite = true)
		{
			if (!string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(destination) && System.IO.File.Exists(file))
			{
				string directoryName = Path.GetDirectoryName(destination);
				if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				System.IO.File.Copy(file, destination, overwrite);
			}
			return destination;
		}

		public static string Move(string file, string destination, bool overwrite = true)
		{
			if (!string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(destination) && System.IO.File.Exists(file))
			{
				Copy(file, destination);
				Delete(file);
			}
			return destination;
		}

		public static string Find(string filter, string folder, SearchOption option = SearchOption.AllDirectories)
		{
			string[] filesWithExtension = Folder.GetFilesWithExtension(folder, "*", option);
			foreach (string text in filesWithExtension)
			{
				if (text.Contains(filter))
				{
					return text;
				}
			}
			return null;
		}

		public static string ReadText(string file)
		{
			if (!string.IsNullOrEmpty(file) && System.IO.File.Exists(file))
			{
				return System.IO.File.ReadAllText(file);
			}
			return EMPTY_STRING;
		}

		public static string[] ReadTextLines(string file)
		{
			if (!string.IsNullOrEmpty(file) && System.IO.File.Exists(file))
			{
				return System.IO.File.ReadAllLines(file);
			}
			return EMPTY_STRARRAY;
		}

		public static byte[] ReadBytes(string file)
		{
			if (!string.IsNullOrEmpty(file) && System.IO.File.Exists(file))
			{
				return System.IO.File.ReadAllBytes(file);
			}
			return EMPTY_BYTEARRAY;
		}
	}

	public static class Folder
	{
		internal const string Dot = ".";

		public static bool Exists(string folder)
		{
			if (string.IsNullOrEmpty(folder))
			{
				return false;
			}
			return Directory.Exists(folder);
		}

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

		public static void Delete(string folder)
		{
			if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder) && !string.IsNullOrEmpty(folder))
			{
				Directory.Delete(folder, recursive: true);
			}
		}

		public static void DeleteFilesWithExtension(string folder, string extension, SearchOption option = SearchOption.AllDirectories)
		{
			if (!string.IsNullOrEmpty(folder) && Exists(folder))
			{
				string[] files = Directory.GetFiles(folder, extension.Contains(".") ? ("*" + extension) : ("*." + extension), option);
				foreach (string path in files)
				{
					System.IO.File.Delete(path);
				}
			}
		}

		public static void DeleteFilesWithExtension(string folder, string extension, string[] exceptions, SearchOption option = SearchOption.AllDirectories)
		{
			if (!Directory.Exists(folder) || string.IsNullOrEmpty(folder))
			{
				return;
			}
			string[] files = Directory.GetFiles(folder, extension.Contains(".") ? ("*" + extension) : ("*." + extension), option);
			foreach (string file in files)
			{
				if (exceptions.Any((string x) => x != Path.GetFileName(file)))
				{
					System.IO.File.Delete(file);
				}
			}
		}

		public static void DeleteContents(string folder, string folderPattern = "*", string filePattern = "*", SearchOption option = SearchOption.AllDirectories)
		{
			if (!Directory.Exists(folder) || string.IsNullOrEmpty(folder))
			{
				return;
			}
			string[] files = Directory.GetFiles(folder, filePattern, option);
			foreach (string path in files)
			{
				if (System.IO.File.Exists(path))
				{
					System.IO.File.Delete(path);
				}
			}
			string[] directories = Directory.GetDirectories(folder, folderPattern, option);
			foreach (string path2 in directories)
			{
				if (Directory.Exists(path2))
				{
					Directory.Delete(path2, recursive: true);
				}
			}
		}

		public static Dictionary<string, string> Copy(string folder, string destination, bool subdirectories = true, bool overwrite = true, SearchOption option = SearchOption.AllDirectories)
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
					Copy(directoryInfo2.FullName, text2, subdirectories, overwrite, option);
					dictionary.Add(directoryInfo2.FullName, text2);
				}
				return dictionary;
			}
			return null;
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
					Copy(directoryInfo2.FullName, text2, subdirectories, overwrite, option);
					list.Add(text2);
					directoryInfo2.Delete(subdirectories);
				}
				Delete(folder);
				return list;
			}
			return null;
		}

		public static string[] GetFilesWithExtension(string folder, string extension, SearchOption option = SearchOption.AllDirectories)
		{
			List<string> list = new List<string>();
			if (Directory.Exists(folder) && !string.IsNullOrEmpty(folder))
			{
				string[] files = Directory.GetFiles(folder, extension.Contains(".") ? ("*" + extension) : $"*.{extension}", option);
				foreach (string item in files)
				{
					list.Add(item);
				}
			}
			return list.ToArray();
		}

		public static string[] GetFilesWithExtension(string folder, string extension, string[] exceptions, SearchOption option = SearchOption.AllDirectories)
		{
			List<string> list = new List<string>();
			if (Directory.Exists(folder) && !string.IsNullOrEmpty(folder))
			{
				string[] files = Directory.GetFiles(folder, extension.Contains(".") ? ("*" + extension) : $"*.{extension}", option);
				foreach (string file in files)
				{
					if (exceptions.Any((string x) => x != Path.GetFileName(file)))
					{
						list.Add(file);
					}
				}
			}
			return list.ToArray();
		}

		public static string GetPreviousFolder(string folder)
		{
			if (!string.IsNullOrEmpty(folder))
			{
				return Path.GetDirectoryName(folder.TrimEnd(new char[2]
				{
					Path.DirectorySeparatorChar,
					Path.AltDirectorySeparatorChar
				}));
			}
			return string.Empty;
		}
	}

	public static class Utils
	{
		public static string Copy(string fileOrFolder, string destination, bool subdirectories = true, bool overwrite = true)
		{
			if (!string.IsNullOrEmpty(fileOrFolder) && !string.IsNullOrEmpty(destination))
			{
				if (File.Exists(fileOrFolder))
				{
					File.Copy(fileOrFolder, destination, overwrite);
				}
				else if (Folder.Exists(fileOrFolder))
				{
					Folder.Copy(fileOrFolder, destination, subdirectories, overwrite);
				}
				return destination;
			}
			return fileOrFolder;
		}

		public static string Move(string fileOrFolder, string destination, bool subdirectories = true, bool overwrite = true)
		{
			if (!string.IsNullOrEmpty(fileOrFolder) && !string.IsNullOrEmpty(destination))
			{
				if (File.Exists(fileOrFolder))
				{
					File.Move(fileOrFolder, destination, overwrite);
				}
				else if (Folder.Exists(fileOrFolder))
				{
					Folder.Move(fileOrFolder, destination, subdirectories, overwrite);
				}
				return destination;
			}
			return fileOrFolder;
		}

		public static void Delete(string fileOrFolder)
		{
			if (!string.IsNullOrEmpty(fileOrFolder))
			{
				if (System.IO.File.Exists(fileOrFolder))
				{
					File.Delete(fileOrFolder);
				}
				else if (Directory.Exists(fileOrFolder))
				{
					Folder.Delete(fileOrFolder);
				}
			}
		}
	}
}
