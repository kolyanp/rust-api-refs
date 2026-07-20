using System;

namespace Windows;

public class ConsoleInput
{
	public string inputString = "";

	public string[] statusText = new string[3] { "", "", "" };

	public int StatusLineCount
	{
		get
		{
			if (statusText != null)
			{
				return statusText.Length;
			}
			return 0;
		}
	}

	public int InputCursorTop => StatusCursorTop - 1;

	public int StatusCursorTop => Console.BufferHeight - StatusLineCount - 1;

	public bool valid => Console.BufferWidth > 0;

	public int lineWidth => Console.BufferWidth;

	public event Action<string> OnInputText;

	public void ClearLine(int numLines)
	{
		Console.CursorLeft = 0;
		Console.Write(new string(' ', lineWidth * numLines));
		Console.CursorTop -= numLines;
		Console.CursorLeft = 0;
	}

	public void RedrawInputLine(bool clear = true)
	{
		ConsoleColor backgroundColor = Console.BackgroundColor;
		ConsoleColor foregroundColor = Console.ForegroundColor;
		Console.CursorVisible = false;
		try
		{
			if (clear)
			{
				ClearLine(1);
			}
			Console.ForegroundColor = ConsoleColor.White;
			Console.CursorLeft = 0;
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.Green;
			if (inputString.Length == 0)
			{
				Console.BackgroundColor = backgroundColor;
				Console.ForegroundColor = foregroundColor;
				return;
			}
			if (inputString.Length < lineWidth - 2)
			{
				Console.Write(inputString);
			}
			else
			{
				Console.Write(inputString.Substring(inputString.Length - (lineWidth - 2)));
			}
		}
		catch (Exception)
		{
		}
		Console.BackgroundColor = backgroundColor;
		Console.ForegroundColor = foregroundColor;
		Console.CursorVisible = true;
	}

	public void RedrawStatusText()
	{
		ConsoleColor backgroundColor = Console.BackgroundColor;
		ConsoleColor foregroundColor = Console.ForegroundColor;
		int cursorTop = Console.CursorTop;
		int cursorLeft = Console.CursorLeft;
		try
		{
			Console.CursorTop++;
			Console.ForegroundColor = ConsoleColor.White;
			for (int i = 0; i < statusText.Length; i++)
			{
				Console.CursorLeft = 0;
				Console.Write(statusText[i].PadRight(lineWidth));
			}
		}
		catch
		{
		}
		Console.BackgroundColor = backgroundColor;
		Console.ForegroundColor = foregroundColor;
		try
		{
			Console.CursorTop = cursorTop;
			Console.CursorLeft = cursorLeft;
		}
		catch
		{
		}
	}

	public void FixBottomOfBuffer()
	{
		try
		{
			Console.CursorTop = InputCursorTop;
		}
		catch
		{
		}
	}

	internal void OnBackspace()
	{
		if (inputString.Length >= 1)
		{
			inputString = inputString.Substring(0, inputString.Length - 1);
			RedrawInputLine();
		}
	}

	internal void OnEscape()
	{
		inputString = "";
		RedrawInputLine();
	}

	internal void OnEnter()
	{
		ClearLine(statusText.Length);
		ConsoleColor foregroundColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("> " + inputString);
		Console.ForegroundColor = foregroundColor;
		string obj = inputString;
		inputString = "";
		if (this.OnInputText != null)
		{
			this.OnInputText(obj);
		}
		RedrawInputLine();
	}

	public void Update()
	{
		if (!valid)
		{
			return;
		}
		try
		{
			if (!Console.KeyAvailable)
			{
				return;
			}
		}
		catch (Exception)
		{
			return;
		}
		ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
		if (consoleKeyInfo.Key == ConsoleKey.Enter)
		{
			OnEnter();
		}
		else if (consoleKeyInfo.Key == ConsoleKey.Backspace)
		{
			OnBackspace();
		}
		else if (consoleKeyInfo.Key == ConsoleKey.Escape)
		{
			OnEscape();
		}
		else if (consoleKeyInfo.KeyChar != 0)
		{
			inputString += consoleKeyInfo.KeyChar;
			RedrawInputLine(clear: false);
		}
	}
}
