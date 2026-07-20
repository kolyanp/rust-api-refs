using System;
using System.IO;

namespace Carbon.Extensions;

public static class PathEx
{
	public const StringComparison PathComparison = StringComparison.OrdinalIgnoreCase;

	public static string NormalizePath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return string.Empty;
		}
		string fullPath = Path.GetFullPath(path);
		if (!IsRootDirectory(fullPath))
		{
			return fullPath.TrimEnd(new char[2]
			{
				Path.DirectorySeparatorChar,
				Path.AltDirectorySeparatorChar
			});
		}
		return fullPath;
	}

	public static bool HasExtension(string path, string extension)
	{
		if (!string.IsNullOrEmpty(path))
		{
			return path.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public static bool Equals(string a, string b)
	{
		return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsRootDirectory(string path)
	{
		if (path.Length == 1)
		{
			return IsSeparator(path[0]);
		}
		if (path.Length == 3)
		{
			if (path[1] == ':')
			{
				return IsSeparator(path[2]);
			}
			return false;
		}
		return false;
	}

	private static bool IsSeparator(char c)
	{
		if (c != Path.DirectorySeparatorChar)
		{
			return c == Path.AltDirectorySeparatorChar;
		}
		return true;
	}
}
