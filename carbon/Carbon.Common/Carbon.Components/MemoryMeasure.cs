using System;
using System.Runtime.InteropServices;

namespace Carbon.Components;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MemoryMeasure : IDisposable
{
	public const string Format = "{0}{1}";

	public static MemoryMeasure New(string name, long threshold = 1024L, string warn = null, bool formatted = true)
	{
		return default(MemoryMeasure);
	}

	public void Dispose()
	{
	}
}
