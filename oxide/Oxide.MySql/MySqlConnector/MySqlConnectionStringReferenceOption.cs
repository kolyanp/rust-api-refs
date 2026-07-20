using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MySqlConnector;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal sealed class MySqlConnectionStringReferenceOption<T> : MySqlConnectionStringOption where T : class
{
	private readonly T m_defaultValue;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 2, 1 })]
	private readonly Func<T, T> m_coerce;

	public MySqlConnectionStringReferenceOption(IReadOnlyList<string> keys, T defaultValue, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 2, 1 })] Func<T, T> coerce = null)
		: base(keys)
	{
		m_defaultValue = defaultValue;
		m_coerce = coerce;
	}

	public T GetValue(MySqlConnectionStringBuilder builder)
	{
		if (!builder.TryGetValue(base.Key, out var value))
		{
			return m_defaultValue;
		}
		return ChangeType(value);
	}

	public void SetValue(MySqlConnectionStringBuilder builder, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] T value)
	{
		builder.DoSetValue(base.Key, (m_coerce == null) ? value : m_coerce(value));
	}

	public override object GetObject(MySqlConnectionStringBuilder builder)
	{
		return GetValue(builder);
	}

	public override void SetObject(MySqlConnectionStringBuilder builder, object value)
	{
		SetValue(builder, ChangeType(value));
	}

	private static T ChangeType(object objectValue)
	{
		return (T)Convert.ChangeType(objectValue, typeof(T), CultureInfo.InvariantCulture);
	}
}
