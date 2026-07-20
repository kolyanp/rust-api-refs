using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Facepunch;

namespace Carbon.Components;

public struct StringTable : IDisposable
{
	public class ConsoleTableOptions
	{
		public string[] Columns { get; set; } = new string[0];

		public bool EnableCount { get; set; } = true;
	}

	public enum FormatTypes
	{
		None,
		Default,
		MarkDown,
		Alternative,
		Minimal
	}

	public IList<object> Columns { get; set; }

	public IList<object[]> Rows { get; private set; }

	public ConsoleTableOptions Options { get; private set; }

	public StringTable(params string[] columns)
		: this(new ConsoleTableOptions
		{
			Columns = columns
		})
	{
	}

	public StringTable(ConsoleTableOptions options)
	{
		Options = options ?? throw new ArgumentNullException("options");
		Rows = new List<object[]>();
		Columns = new List<object>(options.Columns);
	}

	public StringTable AddColumn(params string[] names)
	{
		foreach (string item in names)
		{
			Columns.Add(item);
		}
		return this;
	}

	public StringTable AddRow(params object[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (!Columns.Any())
		{
			throw new Exception("Please set the columns first");
		}
		if (Columns.Count != values.Length)
		{
			throw new Exception($"The number columns in the row ({Columns.Count}) does not match the values ({values.Length}");
		}
		Rows.Add(values);
		return this;
	}

	public static StringTable From<T>(params T[] values)
	{
		StringTable result = default(StringTable);
		string[] columns = GetColumns<T>();
		result.AddColumn(columns);
		foreach (IEnumerable<object> item in values.Select((T value) => columns.Select((string column) => GetColumnValue<T>(value, column))))
		{
			result.AddRow(item.ToArray());
		}
		return result;
	}

	public override string ToString()
	{
		return ToStringDefault();
	}

	private string ToStringNone()
	{
		using StringBody stringBody = default(StringBody);
		IEnumerable<int> columnLengths = ColumnLengths();
		string format = Format(columnLengths, '\0');
		object[] array = Columns.ToArray();
		string data = string.Format(format, array);
		IEnumerable<string> enumerable = Rows.Select((object[] row) => string.Format(format, row));
		stringBody.Add(data);
		foreach (string item in enumerable)
		{
			stringBody.Add(item);
		}
		Array.Clear(array, 0, array.Length);
		array = null;
		enumerable = null;
		columnLengths = null;
		return stringBody.ToNewLine();
	}

	private string ToStringDefault()
	{
		StringBuilder stringBuilder = Pool.Get<StringBuilder>();
		IEnumerable<int> columnLengths = ColumnLengths();
		string format = (from i in Enumerable.Range(0, Columns.Count)
			select " | {" + i + ",-" + columnLengths.ElementAt(i) + "}").Aggregate((string s, string a) => s + a) + " |";
		object[] array = Columns.ToArray();
		int val = Math.Max(0, Rows.Any() ? Rows.Max((object[] row) => string.Format(format, row).Length) : 0);
		string text = string.Format(format, array);
		int num = Math.Max(val, text.Length);
		IEnumerable<string> enumerable = Rows.Select((object[] row) => string.Format(format, row));
		string value = " " + string.Join("", Enumerable.Repeat("-", num - 1)) + " ";
		Array.Clear(array, 0, array.Length);
		array = null;
		columnLengths = null;
		stringBuilder.AppendLine(value);
		stringBuilder.AppendLine(text);
		foreach (string item in enumerable)
		{
			stringBuilder.AppendLine(value);
			stringBuilder.AppendLine(item);
		}
		stringBuilder.AppendLine(value);
		if (Options.EnableCount)
		{
			stringBuilder.AppendLine("");
			stringBuilder.AppendFormat(" Count: {0}", Rows.Count);
		}
		string result = stringBuilder.ToString();
		Pool.FreeUnmanaged(ref stringBuilder);
		return result;
	}

	private string ToStringMarkDown()
	{
		return ToStringMarkDown('|');
	}

	private string ToStringMarkDown(char delimiter)
	{
		StringBuilder stringBuilder = Pool.Get<StringBuilder>();
		IEnumerable<int> columnLengths = ColumnLengths();
		string format = Format(columnLengths, delimiter);
		string text = string.Format(format, Columns.ToArray());
		IEnumerable<string> enumerable = Rows.Select((object[] row) => string.Format(format, row));
		string value = Regex.Replace(text, "[^|]", "-");
		columnLengths = null;
		stringBuilder.AppendLine(text);
		stringBuilder.AppendLine(value);
		foreach (string item in enumerable)
		{
			stringBuilder.AppendLine(item);
		}
		string result = stringBuilder.ToString();
		Pool.FreeUnmanaged(ref stringBuilder);
		return result;
	}

	public string ToStringMinimal()
	{
		return ToStringMarkDown('\0');
	}

	public string ToStringAlternative()
	{
		StringBuilder stringBuilder = Pool.Get<StringBuilder>();
		object[] array = Columns.ToArray();
		IEnumerable<int> columnLengths = ColumnLengths();
		string format = Format(columnLengths);
		string text = string.Format(format, array);
		IEnumerable<string> enumerable = Rows.Select((object[] row) => string.Format(format, row));
		string text2 = Regex.Replace(text, "[^|]", "-");
		string value = text2.Replace("|", "+");
		Array.Clear(array, 0, array.Length);
		array = null;
		columnLengths = null;
		stringBuilder.AppendLine(value);
		stringBuilder.AppendLine(text);
		foreach (string item in enumerable)
		{
			stringBuilder.AppendLine(value);
			stringBuilder.AppendLine(item);
		}
		stringBuilder.AppendLine(value);
		string result = stringBuilder.ToString();
		Pool.FreeUnmanaged(ref stringBuilder);
		return result;
	}

	private string Format(IEnumerable<int> columnLengths, char delimiter = '|')
	{
		string delimiterStr = ((delimiter == '\0') ? string.Empty : delimiter.ToString());
		return ((from i in Enumerable.Range(0, Columns.Count)
			select " " + delimiterStr + " {" + i + ",-" + columnLengths.ElementAt(i) + "}").Aggregate((string s, string a) => s + a) + " " + delimiterStr).Trim();
	}

	private IEnumerable<int> ColumnLengths()
	{
		IList<object[]> rows = Rows;
		IList<object> columns = Columns;
		return Columns.Select((object t, int i) => (from x in rows.Select((object[] x) => x[i]).Union(new object[1] { columns[i] })
			where x != null
			select x.ToString().Length).Max());
	}

	public string Write(FormatTypes format = FormatTypes.Default)
	{
		return format switch
		{
			FormatTypes.None => ToStringNone(), 
			FormatTypes.Default => ToStringDefault(), 
			FormatTypes.MarkDown => ToStringMarkDown(), 
			FormatTypes.Alternative => ToStringAlternative(), 
			FormatTypes.Minimal => ToStringMinimal(), 
			_ => throw new ArgumentOutOfRangeException("format", format, null), 
		};
	}

	private static string[] GetColumns<T>()
	{
		return (from x in typeof(T).GetProperties()
			select x.Name).ToArray();
	}

	private static object GetColumnValue<T>(object target, string column)
	{
		return typeof(T).GetProperty(column).GetValue(target, null);
	}

	public void Dispose()
	{
		Options = null;
		Rows.Clear();
		Columns.Clear();
		Rows = null;
		Columns = null;
	}
}
