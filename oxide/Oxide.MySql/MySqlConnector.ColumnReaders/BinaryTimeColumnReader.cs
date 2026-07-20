using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MySqlConnector.Protocol.Payloads;

namespace MySqlConnector.ColumnReaders;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal sealed class BinaryTimeColumnReader : ColumnReader
{
	public static BinaryTimeColumnReader Instance { get; } = new BinaryTimeColumnReader();

	public override object ReadValue([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] ReadOnlySpan<byte> data, ColumnDefinitionPayload columnDefinition)
	{
		if (data.Length == 0)
		{
			return TimeSpan.Zero;
		}
		byte num = data[0];
		ref ReadOnlySpan<byte> reference = ref data;
		int num2 = MemoryMarshal.Read<int>(reference.Slice(1, reference.Length - 1));
		int num3 = data[5];
		int num4 = data[6];
		int num5 = data[7];
		int num6;
		if (data.Length != 8)
		{
			reference = ref data;
			num6 = MemoryMarshal.Read<int>(reference.Slice(8, reference.Length - 8));
		}
		else
		{
			num6 = 0;
		}
		int num7 = num6;
		if (num != 0)
		{
			num2 = -num2;
			num3 = -num3;
			num4 = -num4;
			num5 = -num5;
			num7 = -num7;
		}
		return new TimeSpan(num2, num3, num4, num5) + TimeSpan.FromTicks(num7 * 10);
	}
}
