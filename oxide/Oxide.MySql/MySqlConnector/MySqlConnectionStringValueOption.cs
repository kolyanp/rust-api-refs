using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MySqlConnector;

internal sealed class MySqlConnectionStringValueOption<T> : MySqlConnectionStringOption where T : struct
{
	private readonly T m_defaultValue;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 0, 0 })]
	private readonly Func<T, T> m_coerce;

	public MySqlConnectionStringValueOption([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] IReadOnlyList<string> keys, T defaultValue, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 0, 0 })] Func<T, T> coerce = null)
		: base(keys)
	{
		m_defaultValue = defaultValue;
		m_coerce = coerce;
	}

	public T GetValue([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] MySqlConnectionStringBuilder builder)
	{
		if (!builder.TryGetValue(base.Key, out var value))
		{
			return m_defaultValue;
		}
		return ChangeType(value);
	}

	public void SetValue([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] MySqlConnectionStringBuilder builder, T value)
	{
		builder.DoSetValue(base.Key, (m_coerce == null) ? value : m_coerce(value));
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public override object GetObject(MySqlConnectionStringBuilder builder)
	{
		return GetValue(builder);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public override void SetObject(MySqlConnectionStringBuilder builder, object value)
	{
		SetValue(builder, ChangeType(value));
	}

	private T ChangeType([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] object objectValue)
	{
		if (typeof(T) == typeof(bool) && objectValue is string a)
		{
			if (string.Equals(a, "yes", StringComparison.OrdinalIgnoreCase))
			{
				return (T)(object)true;
			}
			if (string.Equals(a, "no", StringComparison.OrdinalIgnoreCase))
			{
				return (T)(object)false;
			}
		}
		if ((typeof(T) == typeof(MySqlLoadBalance) || typeof(T) == typeof(MySqlSslMode) || typeof(T) == typeof(MySqlServerRedirectionMode) || typeof(T) == typeof(MySqlDateTimeKind) || typeof(T) == typeof(MySqlGuidFormat) || typeof(T) == typeof(MySqlConnectionProtocol) || typeof(T) == typeof(MySqlCertificateStoreLocation)) && objectValue is string value)
		{
			try
			{
				return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
			}
			catch (Exception ex) when (!(ex is ArgumentException))
			{
				throw new ArgumentException(FormattableString.Invariant($"Value '{objectValue}' not supported for option '{typeof(T).Name}'."), ex);
			}
		}
		try
		{
			return (T)Convert.ChangeType(objectValue, typeof(T), CultureInfo.InvariantCulture);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException(FormattableString.Invariant($"Invalid value '{objectValue}' for '{base.Key}' connection string option."), innerException);
		}
	}
}
