using System;
using System.Runtime.CompilerServices;

namespace MySqlConnector;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
public sealed class MySqlAttribute(string attributeName, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] object value) : ICloneable
{
	public string AttributeName { get; set; } = attributeName ?? "";

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public object Value
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		set;
	} = value;

	public MySqlAttribute()
		: this("", null)
	{
	}

	public MySqlAttribute Clone()
	{
		return new MySqlAttribute(AttributeName, Value);
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	internal MySqlParameter ToParameter()
	{
		if (string.IsNullOrEmpty(AttributeName))
		{
			throw new InvalidOperationException("AttributeName must not be null or empty");
		}
		return new MySqlParameter(AttributeName, Value);
	}
}
