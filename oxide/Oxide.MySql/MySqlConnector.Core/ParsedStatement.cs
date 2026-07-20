using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MySqlConnector.Core;

internal sealed class ParsedStatement
{
	public ArraySegment<byte> StatementBytes { get; set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })]
	public List<string> ParameterNames
	{
		[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })]
		get;
	} = new List<string>();

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })]
	public List<string> NormalizedParameterNames
	{
		[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })]
		get;
	} = new List<string>();

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public List<int> ParameterIndexes
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get;
	} = new List<int>();
}
