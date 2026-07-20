using System;
using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MySqlConnector.Core;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal sealed class DbTypeMapping(Type clrType, DbType[] dbTypes, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 1 })][field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 1 })] Func<object, object> convert = null)
{
	public Type ClrType { get; } = clrType;

	public DbType[] DbTypes { get; } = dbTypes;

	public object DoConversion(object obj)
	{
		if (obj.GetType() == ClrType)
		{
			return obj;
		}
		if (convert != null)
		{
			return convert(obj);
		}
		return Convert.ChangeType(obj, ClrType, CultureInfo.InvariantCulture);
	}
}
