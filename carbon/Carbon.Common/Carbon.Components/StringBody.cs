using System;
using System.Collections.Generic;
using Carbon.Extensions;

namespace Carbon.Components;

public struct StringBody : IDisposable
{
	public enum ExportTypes
	{
		Append,
		NewLine
	}

	public List<object> Items => _items ?? (_items = new List<object>());

	internal List<object> _items { get; set; }

	public StringBody Add(object data)
	{
		Items.Add(data ?? string.Empty);
		return this;
	}

	public StringBody Add(object[] datas)
	{
		Items.AddRange(datas);
		return this;
	}

	public StringBody Empty()
	{
		Add((object)null);
		return this;
	}

	public StringBody Remove(object data)
	{
		Items.Remove(data);
		return this;
	}

	public StringBody Clear()
	{
		Items.Clear();
		return this;
	}

	public string ToAppended(string inBetweenString = " ")
	{
		string text = string.Empty;
		foreach (object item in Items)
		{
			text += $"{item}{inBetweenString}";
		}
		return text.TrimEnd();
	}

	public string ToNewLine()
	{
		string text = string.Empty;
		foreach (object item in Items)
		{
			text += $"{item}\n";
		}
		return text.TrimEnd();
	}

	public override string ToString()
	{
		return ToAppended(string.Empty);
	}

	public StringBody Export(string filePath, ExportTypes exportType, bool @override = true, string inBetweenString = " ", int padding = 50, char paddingCharacter = ' ', bool spacing = true)
	{
		if (!@override && OsEx.File.Exists(filePath))
		{
			return this;
		}
		try
		{
			switch (exportType)
			{
			case ExportTypes.Append:
				OsEx.File.Create(filePath, ToAppended(inBetweenString));
				break;
			case ExportTypes.NewLine:
				OsEx.File.Create(filePath, ToNewLine());
				break;
			}
		}
		catch
		{
		}
		return this;
	}

	public void Dispose()
	{
		_items.Clear();
		_items = null;
	}
}
