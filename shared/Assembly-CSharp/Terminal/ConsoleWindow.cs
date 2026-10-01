using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.Win32.SafeHandles;
using UnityEngine;

namespace Terminal;

[SuppressUnmanagedCodeSecurity]
public class ConsoleWindow
{
	private TextWriter oldOutput;

	private const int STD_INPUT_HANDLE = -10;

	private const int STD_OUTPUT_HANDLE = -11;

	private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4u;

	public static bool HasRawInput => false;

	public static bool StdoutCarriesLog => false;

	public bool Initialize()
	{
		FreeConsole();
		if (!AttachConsole(uint.MaxValue))
		{
			AllocConsole();
		}
		EnableVirtualTerminalProcessing();
		oldOutput = Console.Out;
		try
		{
			Console.OutputEncoding = Encoding.UTF8;
			Console.SetOut(new StreamWriter(new FileStream(new SafeFileHandle(GetStdHandle(-11), ownsHandle: true), FileAccess.Write), Encoding.UTF8)
			{
				AutoFlush = true
			});
		}
		catch (Exception ex)
		{
			Debug.Log((object)("Couldn't redirect output: " + ex.Message));
		}
		return true;
	}

	public static void GetSize(out int columns, out int rows)
	{
		columns = Console.BufferWidth;
		rows = Console.WindowHeight;
	}

	public static bool TryReadByte(out byte value)
	{
		value = 0;
		return false;
	}

	public void Shutdown()
	{
		if (oldOutput != null)
		{
			Console.SetOut(oldOutput);
		}
		FreeConsole();
	}

	public void SetTitle(string strName)
	{
		SetConsoleTitleA(strName);
	}

	private static void EnableVirtualTerminalProcessing()
	{
		try
		{
			IntPtr stdHandle = GetStdHandle(-11);
			if (GetConsoleMode(stdHandle, out var lpMode) && (lpMode & 4) == 0)
			{
				SetConsoleMode(stdHandle, lpMode | 4);
			}
		}
		catch (Exception)
		{
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool AttachConsole(uint dwProcessId);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool AllocConsole();

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool FreeConsole();

	[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetStdHandle(int nStdHandle);

	[DllImport("kernel32.dll")]
	private static extern bool SetConsoleTitleA(string lpConsoleTitle);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
}
