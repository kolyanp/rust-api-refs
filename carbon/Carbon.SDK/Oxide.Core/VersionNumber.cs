using System;

namespace Oxide.Core;

public struct VersionNumber
{
	public int Major;

	public int Minor;

	public int Patch;

	public VersionNumber(int major, int minor, int patch)
	{
		Major = major;
		Minor = minor;
		Patch = patch;
	}

	public VersionNumber(string version)
	{
		Major = (Minor = (Patch = 0));
		if (string.IsNullOrEmpty(version))
		{
			return;
		}
		string[] array = version.Split('.');
		for (int i = 0; i < array.Length; i++)
		{
			int num = int.Parse(array[i]);
			switch (i)
			{
			case 0:
				Major = num;
				break;
			case 1:
				Minor = num;
				break;
			case 2:
				Patch = num;
				break;
			}
		}
		Array.Clear(array, 0, array.Length);
		array = null;
	}

	public override string ToString()
	{
		return $"{Major}.{Minor}.{Patch}";
	}

	public bool IsValid()
	{
		if (Major == 0 && Minor == 0)
		{
			return Patch != 0;
		}
		return true;
	}

	public static bool operator ==(VersionNumber a, VersionNumber b)
	{
		if (a.Major == b.Major && a.Minor == b.Minor)
		{
			return a.Patch == b.Patch;
		}
		return false;
	}

	public static bool operator !=(VersionNumber a, VersionNumber b)
	{
		if (a.Major == b.Major && a.Minor == b.Minor)
		{
			return a.Patch != b.Patch;
		}
		return true;
	}

	public static bool operator >(VersionNumber a, VersionNumber b)
	{
		if (a.Major < b.Major)
		{
			return false;
		}
		if (a.Major > b.Major)
		{
			return true;
		}
		if (a.Minor < b.Minor)
		{
			return false;
		}
		if (a.Minor > b.Minor)
		{
			return true;
		}
		return a.Patch > b.Patch;
	}

	public static bool operator >=(VersionNumber a, VersionNumber b)
	{
		if (a.Major < b.Major)
		{
			return false;
		}
		if (a.Major > b.Major)
		{
			return true;
		}
		if (a.Minor < b.Minor)
		{
			return false;
		}
		if (a.Minor > b.Minor)
		{
			return true;
		}
		return a.Patch >= b.Patch;
	}

	public static bool operator <(VersionNumber a, VersionNumber b)
	{
		if (a.Major > b.Major)
		{
			return false;
		}
		if (a.Major < b.Major)
		{
			return true;
		}
		if (a.Minor > b.Minor)
		{
			return false;
		}
		if (a.Minor < b.Minor)
		{
			return true;
		}
		return a.Patch < b.Patch;
	}

	public static bool operator <=(VersionNumber a, VersionNumber b)
	{
		if (a.Major > b.Major)
		{
			return false;
		}
		if (a.Major < b.Major)
		{
			return true;
		}
		if (a.Minor > b.Minor)
		{
			return false;
		}
		if (a.Minor < b.Minor)
		{
			return true;
		}
		return a.Patch <= b.Patch;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is VersionNumber versionNumber))
		{
			return false;
		}
		return this == versionNumber;
	}

	public override int GetHashCode()
	{
		return ((391 + Major.GetHashCode()) * 23 + Minor.GetHashCode()) * 23 + Patch.GetHashCode();
	}
}
