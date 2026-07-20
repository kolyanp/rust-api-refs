using System;
using System.Runtime.InteropServices;

namespace Carbon.Components;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct TimeMeasure : IDisposable
{
	public static TimeMeasure New(string name, int miliseconds = 100, string warn = null)
	{
		return default(TimeMeasure);
	}

	public void Dispose()
	{
	}
}
