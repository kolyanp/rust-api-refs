using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ConVar;

namespace Terminal;

public class ConsoleInput
{
	public string inputString = "";

	private int caretPosition;

	private int scrollOffset;

	public string[] statusText = new string[3] { "", "", "" };

	private LinkedList<string> history = new LinkedList<string>();

	private int historyCount;

	private LinkedListNode<string> lastSelected;

	private string pendingInput = "";

	public const string ResetColor = "\u001b[0m";

	private const string HideCursor = "\u001b[?25l";

	private const string ShowCursor = "\u001b[?25h";

	private const string EraseLine = "\u001b[2K";

	private const string EraseScreen = "\u001b[2J";

	private const string ControlPrefix = "\u001b[";

	private const string InputColor = "\u001b[0;92m";

	private const string StatusColor = "\u001b[0;97m";

	private const string StatusBusyColor = "\u001b[0;93m";

	private int vtLastHeight = -1;

	private int vtLastWidth = -1;

	private readonly StringBuilder draw = new StringBuilder(256);

	private char[] drawBuffer = new char[256];

	private const int EscapeTimeoutMs = 50;

	private string escapeSequence = string.Empty;

	private int escapeBodyStart;

	private readonly Stopwatch escapeTimer = new Stopwatch();

	private readonly Decoder utf8 = Encoding.UTF8.GetDecoder();

	private readonly byte[] inputByte = new byte[1];

	private readonly char[] inputChars = new char[2];

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

	public bool valid
	{
		get
		{
			if (!ConsoleWindow.StdoutCarriesLog && lineWidth > 0)
			{
				return TerminalRows >= StatusLineCount + 3;
			}
			return false;
		}
	}

	public int lineWidth
	{
		get
		{
			ConsoleWindow.GetSize(out var columns, out var _);
			return columns;
		}
	}

	private int TerminalRows
	{
		get
		{
			ConsoleWindow.GetSize(out var _, out var rows);
			return rows;
		}
	}

	private int VTWindowHeight => Math.Max(TerminalRows, StatusLineCount + 3);

	private int VTLogBottomRow => VTWindowHeight - StatusLineCount - 2;

	private int VTInputRow => VTLogBottomRow + 1;

	private int VTStatusRow => VTInputRow + 1;

	private int VTCaretColumn => Math.Min(Math.Max(caretPosition - scrollOffset, 0) + 1, Math.Max(lineWidth, 1));

	public event Action<string> OnInputText;

	public static string Foreground(ConsoleColor color)
	{
		return color switch
		{
			ConsoleColor.Black => "\u001b[30m", 
			ConsoleColor.DarkBlue => "\u001b[34m", 
			ConsoleColor.DarkGreen => "\u001b[32m", 
			ConsoleColor.DarkCyan => "\u001b[36m", 
			ConsoleColor.DarkRed => "\u001b[31m", 
			ConsoleColor.DarkMagenta => "\u001b[35m", 
			ConsoleColor.DarkYellow => "\u001b[33m", 
			ConsoleColor.Gray => "\u001b[37m", 
			ConsoleColor.DarkGray => "\u001b[90m", 
			ConsoleColor.Blue => "\u001b[94m", 
			ConsoleColor.Green => "\u001b[92m", 
			ConsoleColor.Cyan => "\u001b[96m", 
			ConsoleColor.Red => "\u001b[91m", 
			ConsoleColor.Magenta => "\u001b[95m", 
			ConsoleColor.Yellow => "\u001b[93m", 
			ConsoleColor.White => "\u001b[97m", 
			_ => "\u001b[37m", 
		};
	}

	private void AppendMoveTo(int row, int column)
	{
		draw.Append("\u001b[").Append(row).Append(';')
			.Append(column)
			.Append('H');
	}

	private void WriteDraw()
	{
		if (drawBuffer.Length < draw.Length)
		{
			drawBuffer = new char[Math.Max(draw.Length, drawBuffer.Length * 2)];
		}
		draw.CopyTo(0, drawBuffer, 0, draw.Length);
		System.Console.Write(drawBuffer, 0, draw.Length);
	}

	public void Initialize()
	{
		try
		{
			vtLastHeight = -1;
			vtLastWidth = -1;
			EnsureVTMargins(parkInLogArea: true);
		}
		catch (Exception)
		{
		}
	}

	public void Shutdown()
	{
		try
		{
			System.Console.Write("\u001b[r\u001b[?25h");
		}
		catch (Exception)
		{
		}
	}

	private bool EnsureVTMargins(bool parkInLogArea)
	{
		if (!valid)
		{
			if (vtLastHeight != -1)
			{
				vtLastHeight = -1;
				vtLastWidth = -1;
				draw.Clear();
				draw.Append("\u001b[").Append('r');
				WriteDraw();
			}
			return false;
		}
		int vTWindowHeight = VTWindowHeight;
		int num = lineWidth;
		int num2;
		if (vTWindowHeight == vtLastHeight)
		{
			num2 = ((num != vtLastWidth) ? 1 : 0);
			if (num2 == 0)
			{
				goto IL_00cb;
			}
		}
		else
		{
			num2 = 1;
		}
		vtLastHeight = vTWindowHeight;
		vtLastWidth = num;
		draw.Clear();
		draw.Append("\u001b[2J").Append("\u001b[").Append("1;")
			.Append(VTLogBottomRow)
			.Append('r');
		WriteDraw();
		goto IL_00cb;
		IL_00cb:
		if (parkInLogArea)
		{
			draw.Clear();
			AppendMoveTo(VTLogBottomRow, 1);
			WriteDraw();
		}
		return (byte)num2 != 0;
	}

	public string GetNext()
	{
		if (historyCount == 0)
		{
			return string.Empty;
		}
		if (lastSelected == null)
		{
			pendingInput = inputString;
			lastSelected = history.First;
		}
		else if (lastSelected.Next != null)
		{
			lastSelected = lastSelected.Next;
		}
		return lastSelected.Value;
	}

	public string GetPrevious()
	{
		if (historyCount == 0 || lastSelected == null)
		{
			return string.Empty;
		}
		if (lastSelected.Previous == null)
		{
			lastSelected = null;
			return pendingInput;
		}
		lastSelected = lastSelected.Previous;
		return lastSelected.Value;
	}

	public void AddToHistory(string value)
	{
		if (value.Length == 0)
		{
			return;
		}
		if (history.First?.Value == value)
		{
			lastSelected = null;
			return;
		}
		if (historyCount >= ConVar.Console.consolehistorysize)
		{
			history.AddFirst(value);
			history.RemoveLast();
		}
		else
		{
			history.AddFirst(value);
			historyCount++;
		}
		lastSelected = null;
	}

	public void TrimHistory(int maxSize)
	{
		if (maxSize < 0)
		{
			maxSize = 0;
		}
		while (historyCount > maxSize && history.Last != null)
		{
			history.RemoveLast();
			historyCount--;
		}
		lastSelected = null;
	}

	public void PrepareForLogOutput()
	{
		try
		{
			EnsureVTMargins(parkInLogArea: true);
		}
		catch (Exception)
		{
		}
	}

	public void RedrawInputLine()
	{
		try
		{
			bool flag = EnsureVTMargins(parkInLogArea: false);
			if (!valid)
			{
				return;
			}
			int num = lineWidth - 2;
			if (inputString.Length <= num)
			{
				scrollOffset = 0;
			}
			else
			{
				if (caretPosition < scrollOffset)
				{
					scrollOffset = caretPosition;
				}
				else if (caretPosition > scrollOffset + num)
				{
					scrollOffset = caretPosition - num;
				}
				int num2 = inputString.Length - num;
				if (scrollOffset > num2)
				{
					scrollOffset = num2;
				}
				if (scrollOffset < 0)
				{
					scrollOffset = 0;
				}
			}
			int count = ((inputString.Length != 0) ? Math.Min(num, inputString.Length - scrollOffset) : 0);
			int vTInputRow = VTInputRow;
			draw.Clear();
			draw.Append("\u001b[?25l");
			AppendMoveTo(vTInputRow, 1);
			draw.Append("\u001b[2K");
			draw.Append("\u001b[0;92m");
			draw.Append(inputString, scrollOffset, count);
			draw.Append("\u001b[0m");
			AppendMoveTo(vTInputRow, VTCaretColumn);
			draw.Append("\u001b[?25h");
			WriteDraw();
			if (flag)
			{
				RedrawStatusText();
			}
		}
		catch (Exception)
		{
		}
	}

	public void RedrawStatusText()
	{
		try
		{
			bool flag = EnsureVTMargins(parkInLogArea: false);
			if (valid)
			{
				int vTStatusRow = VTStatusRow;
				int val = lineWidth;
				draw.Clear();
				draw.Append("\u001b[?25l");
				draw.Append("\u001b[0;97m");
				for (int i = 0; i < statusText.Length; i++)
				{
					string text = statusText[i] ?? string.Empty;
					AppendMoveTo(vTStatusRow + i, 1);
					draw.Append("\u001b[2K");
					draw.Append(text, 0, Math.Min(text.Length, val));
				}
				AppendMoveTo(vTStatusRow + StatusLineCount, 1);
				draw.Append("\u001b[2K");
				draw.Append("\u001b[0m");
				AppendMoveTo(VTInputRow, VTCaretColumn);
				draw.Append("\u001b[?25h");
				WriteDraw();
				if (flag)
				{
					RedrawInputLine();
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private int PrevWordBoundary(int from)
	{
		int num = from;
		while (num > 0 && char.IsWhiteSpace(inputString[num - 1]))
		{
			num--;
		}
		while (num > 0 && !char.IsWhiteSpace(inputString[num - 1]))
		{
			num--;
		}
		return num;
	}

	private int NextWordBoundary(int from)
	{
		int i = from;
		int length;
		for (length = inputString.Length; i < length && !char.IsWhiteSpace(inputString[i]); i++)
		{
		}
		for (; i < length && char.IsWhiteSpace(inputString[i]); i++)
		{
		}
		return i;
	}

	internal void OnBackspace()
	{
		if (caretPosition >= 1)
		{
			inputString = inputString.Remove(caretPosition - 1, 1);
			caretPosition--;
			RedrawInputLine();
		}
	}

	internal void OnDelete()
	{
		if (caretPosition < inputString.Length)
		{
			inputString = inputString.Remove(caretPosition, 1);
			RedrawInputLine();
		}
	}

	internal void OnEscape()
	{
		inputString = "";
		caretPosition = 0;
		RedrawInputLine();
	}

	internal void OnEnter()
	{
		AddToHistory(inputString);
		PrepareForLogOutput();
		System.Console.WriteLine(Foreground(ConsoleColor.Green) + "> " + inputString + "\u001b[0m");
		string obj = inputString;
		inputString = "";
		caretPosition = 0;
		if (OnInputText != null)
		{
			OnInputText(obj);
		}
		RedrawInputLine();
	}

	private void ClearEscapeSequence()
	{
		escapeSequence = string.Empty;
		escapeBodyStart = 0;
		escapeTimer.Reset();
	}

	private static bool ShouldStartEscapeSequence(ConsoleKeyInfo keyInfo, out int bodyStart)
	{
		if (keyInfo.Key == ConsoleKey.Escape || keyInfo.KeyChar == '\u001b')
		{
			bodyStart = 2;
			return true;
		}
		if ((keyInfo.Modifiers & ConsoleModifiers.Alt) != 0 && (keyInfo.Modifiers & ConsoleModifiers.Control) == 0)
		{
			bodyStart = 1;
			return true;
		}
		bodyStart = 0;
		return false;
	}

	private void ExpireEscapeSequence()
	{
		if (escapeSequence.Length != 0 && escapeTimer.ElapsedMilliseconds >= 50)
		{
			bool num = escapeSequence.Length == 1 && escapeSequence[0] == '\u001b';
			ClearEscapeSequence();
			if (num)
			{
				OnEscape();
			}
		}
	}

	private bool FeedEscapeSequence(char character, out ConsoleKey key, out bool ctrlHeld)
	{
		key = ConsoleKey.None;
		ctrlHeld = false;
		escapeSequence += character;
		escapeTimer.Restart();
		if (escapeSequence.Length < escapeBodyStart)
		{
			return false;
		}
		if (escapeSequence.Length == escapeBodyStart)
		{
			if (escapeBodyStart == 1 || character == '[' || character == 'O')
			{
				return false;
			}
			ClearEscapeSequence();
			OnEscape();
			return false;
		}
		if (char.IsDigit(character) || character == ';')
		{
			if (escapeSequence.Length > 16)
			{
				ClearEscapeSequence();
			}
			return false;
		}
		string parameters = escapeSequence.Substring(escapeBodyStart, escapeSequence.Length - escapeBodyStart - 1);
		ClearEscapeSequence();
		return TryDecodeSequence(parameters, character, out key, out ctrlHeld);
	}

	private static bool TryDecodeSequence(string parameters, char final, out ConsoleKey key, out bool ctrlHeld)
	{
		key = ConsoleKey.None;
		ctrlHeld = false;
		string[] array = parameters.Split(';');
		if (array.Length > 1 && int.TryParse(array[1], out var result))
		{
			ctrlHeld = ((result - 1) & 4) != 0;
		}
		switch (final)
		{
		case 'A':
			key = ConsoleKey.UpArrow;
			return true;
		case 'B':
			key = ConsoleKey.DownArrow;
			return true;
		case 'C':
			key = ConsoleKey.RightArrow;
			return true;
		case 'D':
			key = ConsoleKey.LeftArrow;
			return true;
		case 'H':
			key = ConsoleKey.Home;
			return true;
		case 'F':
			key = ConsoleKey.End;
			return true;
		case '~':
		{
			if (int.TryParse(array[0], out var result2))
			{
				switch (result2)
				{
				case 1:
				case 7:
					key = ConsoleKey.Home;
					return true;
				case 3:
					key = ConsoleKey.Delete;
					return true;
				case 4:
				case 8:
					key = ConsoleKey.End;
					return true;
				}
			}
			return false;
		}
		default:
			return false;
		}
	}

	private bool TryReadKey(out ConsoleKeyInfo keyInfo)
	{
		keyInfo = default;
		if (ConsoleWindow.HasRawInput)
		{
			if (!ConsoleWindow.TryReadByte(out inputByte[0]))
			{
				return false;
			}
			if (utf8.GetChars(inputByte, 0, 1, inputChars, 0) != 1)
			{
				return false;
			}
			keyInfo = KeyFromChar(inputChars[0]);
			return true;
		}
		try
		{
			if (!System.Console.KeyAvailable)
			{
				return false;
			}
		}
		catch (Exception)
		{
			return false;
		}
		keyInfo = System.Console.ReadKey(intercept: true);
		return true;
	}

	private static ConsoleKeyInfo KeyFromChar(char character)
	{
		switch (character)
		{
		case '\n':
		case '\r':
			return new ConsoleKeyInfo(character, ConsoleKey.Enter, shift: false, alt: false, control: false);
		case '\b':
		case '\u007f':
			return new ConsoleKeyInfo(character, ConsoleKey.Backspace, shift: false, alt: false, control: false);
		case '\u001b':
			return new ConsoleKeyInfo(character, ConsoleKey.Escape, shift: false, alt: false, control: false);
		default:
			if (character < ' ')
			{
				return new ConsoleKeyInfo('\0', ConsoleKey.None, shift: false, alt: false, control: false);
			}
			return new ConsoleKeyInfo(character, ConsoleKey.None, shift: false, alt: false, control: false);
		}
	}

	public void Update()
	{
		ExpireEscapeSequence();
		if (!TryReadKey(out var keyInfo))
		{
			return;
		}
		ConsoleKey key = keyInfo.Key;
		bool ctrlHeld = (keyInfo.Modifiers & ConsoleModifiers.Control) != 0;
		if ((escapeSequence.Length > 0 || ShouldStartEscapeSequence(keyInfo, out escapeBodyStart)) && !FeedEscapeSequence(keyInfo.KeyChar, out key, out ctrlHeld))
		{
			return;
		}
		switch (key)
		{
		case ConsoleKey.UpArrow:
		{
			string next = GetNext();
			if (!string.IsNullOrEmpty(next))
			{
				inputString = next;
				caretPosition = inputString.Length;
				RedrawInputLine();
			}
			break;
		}
		case ConsoleKey.DownArrow:
			if (lastSelected != null)
			{
				inputString = GetPrevious();
				caretPosition = inputString.Length;
				RedrawInputLine();
			}
			break;
		case ConsoleKey.LeftArrow:
			if (caretPosition > 0)
			{
				caretPosition = (ctrlHeld ? PrevWordBoundary(caretPosition) : (caretPosition - 1));
				RedrawInputLine();
			}
			break;
		case ConsoleKey.RightArrow:
			if (caretPosition < inputString.Length)
			{
				caretPosition = (ctrlHeld ? NextWordBoundary(caretPosition) : (caretPosition + 1));
				RedrawInputLine();
			}
			break;
		case ConsoleKey.Home:
			caretPosition = 0;
			RedrawInputLine();
			break;
		case ConsoleKey.End:
			caretPosition = inputString.Length;
			RedrawInputLine();
			break;
		case ConsoleKey.Enter:
			OnEnter();
			break;
		case ConsoleKey.Backspace:
			OnBackspace();
			break;
		case ConsoleKey.Delete:
			OnDelete();
			break;
		case ConsoleKey.Escape:
			OnEscape();
			break;
		default:
			if (keyInfo.KeyChar != '\0')
			{
				inputString = inputString.Insert(caretPosition, keyInfo.KeyChar.ToString());
				caretPosition++;
				RedrawInputLine();
			}
			break;
		}
	}
}
