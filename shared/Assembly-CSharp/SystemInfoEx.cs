using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class SystemInfoEx
{
	private struct CpuInfo
	{
		public uint eax;

		public uint ebx;

		public uint ecx;

		public uint edx;
	}

	private static bool[] supportedRenderTextureFormats;

	private static string processorFeatures;

	public static int systemMemoryUsed => (int)(System_GetMemoryUsage() / 1024 / 1024);

	[DllImport("RustNative")]
	private static extern ulong System_GetMemoryUsage();

	[DllImport("RustNative")]
	private static extern void System_Cpuid(uint leaf, uint subleaf, out CpuInfo info);

	public static bool SupportsRenderTextureFormat(RenderTextureFormat format)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (supportedRenderTextureFormats == null)
		{
			Array values = Enum.GetValues(typeof(RenderTextureFormat));
			int num = (int)values.GetValue(values.Length - 1);
			supportedRenderTextureFormats = new bool[num + 1];
			for (int i = 0; i <= num; i++)
			{
				bool flag = Enum.IsDefined(typeof(RenderTextureFormat), i);
				supportedRenderTextureFormats[i] = flag && SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat)i);
			}
		}
		return supportedRenderTextureFormats[format];
	}

	public static string GetProcessorFeatures()
	{
		if (processorFeatures != null)
		{
			return processorFeatures;
		}
		System_Cpuid(0u, 0u, out var info);
		System_Cpuid(2147483648u, 0u, out var info2);
		CpuInfo info3 = default;
		CpuInfo info4 = default;
		CpuInfo info5 = default;
		CpuInfo info6 = default;
		if (info.eax >= 1)
		{
			System_Cpuid(1u, 0u, out info3);
		}
		if (info.eax >= 7)
		{
			System_Cpuid(7u, 0u, out info4);
		}
		if (info4.eax >= 1)
		{
			System_Cpuid(7u, 1u, out info5);
		}
		if (info2.eax >= 2147483649u)
		{
			System_Cpuid(2147483649u, 0u, out info6);
		}
		(uint, int)[] array = new (uint, int)[26]
		{
			(info3.ecx, 19),
			(info3.ecx, 20),
			(info4.ebx, 5),
			(info3.ecx, 12),
			(info3.ecx, 29),
			(info4.ebx, 3),
			(info4.ebx, 8),
			(info6.ecx, 5),
			(info4.ebx, 19),
			(info3.ecx, 25),
			(info3.ecx, 1),
			(info4.ebx, 29),
			(info5.eax, 0),
			(info3.ecx, 30),
			(info4.ebx, 18),
			(info4.ecx, 9),
			(info4.ecx, 10),
			(info4.ecx, 8),
			(info5.eax, 4),
			(info4.ebx, 16),
			(info4.ecx, 11),
			(info5.eax, 5),
			(info4.edx, 23),
			(info4.ecx, 3),
			(info4.ecx, 7),
			(info4.edx, 20)
		};
		uint num = 0u;
		for (int i = 0; i < array.Length; i++)
		{
			if ((array[i].Item1 & (uint)(1 << array[i].Item2)) != 0)
			{
				num |= (uint)(1 << i);
			}
		}
		char[] array2 = new char[(array.Length + 3) / 4];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = "0123456789abcdef"[(int)((num >> j * 4) & 0xF)];
		}
		processorFeatures = new string(array2);
		return processorFeatures;
	}
}
