using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using Cysharp.Text;
using UnityEngine;

namespace Facepunch.Rust;

public struct EventRecordField
{
	[StructLayout(LayoutKind.Explicit)]
	public struct ValueUnion
	{
		[FieldOffset(0)]
		public long Number;

		[FieldOffset(0)]
		public double Float;

		[FieldOffset(0)]
		public Vector3 Vector;

		[FieldOffset(0)]
		public Guid Guid;

		[FieldOffset(0)]
		public DateTime DateTime;
	}

	public string Key1;

	public string Key2;

	public string String;

	public ReadOnlyMemory<char> Chars;

	public MemoryStream Bytes;

	public ValueUnion Value;

	public FieldType Type;

	public bool IsObject;

	public EventRecordField(string key1)
	{
		Key1 = key1;
		Key2 = null;
		Type = FieldType.None;
		IsObject = false;
		String = null;
		Bytes = null;
		Chars = default(ReadOnlyMemory<char>);
		Value = default(ValueUnion);
	}

	public EventRecordField(string key1, string key2)
	{
		Key1 = key1;
		Key2 = key2;
		Type = FieldType.None;
		IsObject = false;
		String = null;
		Bytes = null;
		Chars = default(ReadOnlyMemory<char>);
		Value = default(ValueUnion);
	}

	public void Serialize(ref Utf8ValueStringBuilder writer, AnalyticsDocumentMode format)
	{
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		switch (Type)
		{
		case FieldType.String:
		{
			if (String == null)
			{
				break;
			}
			if (IsObject)
			{
				((Utf8ValueStringBuilder)(ref writer)).Append(String);
				break;
			}
			string text = String;
			int length3 = String.Length;
			for (int k = 0; k < length3; k++)
			{
				char c2 = text[k];
				WriteChar(ref writer, c2);
			}
			break;
		}
		case FieldType.Float:
		{
			Span<char> destination2 = stackalloc char[128];
			Value.Float.TryFormat(destination2, out var charsWritten3);
			((Utf8ValueStringBuilder)(ref writer)).Append((ReadOnlySpan<char>)destination2.Slice(0, charsWritten3));
			break;
		}
		case FieldType.Number:
			((Utf8ValueStringBuilder)(ref writer)).Append(Value.Number);
			break;
		case FieldType.Guid:
		{
			StandardFormat standardFormat = new StandardFormat('N');
			((Utf8ValueStringBuilder)(ref writer)).Append(Value.Guid, standardFormat);
			break;
		}
		case FieldType.Vector:
		{
			Span<char> destination = stackalloc char[128];
			Vector3 vector = Value.Vector;
			vector.x.TryFormat(destination, out var charsWritten);
			((Utf8ValueStringBuilder)(ref writer)).Append((ReadOnlySpan<char>)destination.Slice(0, charsWritten));
			((Utf8ValueStringBuilder)(ref writer)).Append(',');
			vector.y.TryFormat(destination, out charsWritten);
			((Utf8ValueStringBuilder)(ref writer)).Append((ReadOnlySpan<char>)destination.Slice(0, charsWritten));
			((Utf8ValueStringBuilder)(ref writer)).Append(',');
			vector.z.TryFormat(destination, out charsWritten);
			((Utf8ValueStringBuilder)(ref writer)).Append((ReadOnlySpan<char>)destination.Slice(0, charsWritten));
			break;
		}
		case FieldType.DateTime:
			((Utf8ValueStringBuilder)(ref writer)).Append(Value.DateTime, StandardFormats.DateTime_ISO);
			break;
		case FieldType.Bytes:
			if (Bytes != null)
			{
				Span<char> chars = stackalloc char[128];
				int num = 96;
				byte[] buffer = Bytes.GetBuffer();
				for (int j = 0; j < Bytes.Length; j += num)
				{
					int num2 = (int)Bytes.Length - j;
					int length2 = ((num2 > num) ? num : num2);
					Convert.TryToBase64Chars(new Span<byte>(buffer, j, length2), chars, out var charsWritten2);
					Span<char> span2 = chars.Slice(0, charsWritten2);
					((Utf8ValueStringBuilder)(ref writer)).Append((ReadOnlySpan<char>)span2);
				}
			}
			break;
		case FieldType.Chars:
		{
			if (IsObject)
			{
				((Utf8ValueStringBuilder)(ref writer)).Append(Chars.Span);
				break;
			}
			int length = Chars.Length;
			ReadOnlySpan<char> span = Chars.Span;
			for (int i = 0; i < length; i++)
			{
				char c = span[i];
				WriteChar(ref writer, c);
			}
			break;
		}
		default:
			Debug.LogWarning((object)"Unhandled field type attempted to be serialized");
			break;
		}
		void WriteChar(ref Utf8ValueStringBuilder reference, char c3)
		{
			if (c3 == '\\' && format == AnalyticsDocumentMode.JSON)
			{
				((Utf8ValueStringBuilder)(ref reference)).Append("\\\\");
			}
			else
			{
				switch (c3)
				{
				case '"':
					if (format == AnalyticsDocumentMode.JSON)
					{
						((Utf8ValueStringBuilder)(ref reference)).Append("\\\"");
					}
					else
					{
						((Utf8ValueStringBuilder)(ref reference)).Append("\"\"");
					}
					break;
				case '\n':
					((Utf8ValueStringBuilder)(ref reference)).Append("\\n");
					break;
				case '\r':
					((Utf8ValueStringBuilder)(ref reference)).Append("\\r");
					break;
				case '\t':
					((Utf8ValueStringBuilder)(ref reference)).Append("\\t");
					break;
				default:
					((Utf8ValueStringBuilder)(ref reference)).Append(c3);
					break;
				}
			}
		}
	}
}
