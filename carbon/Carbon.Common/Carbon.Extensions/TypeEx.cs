using System;
using System.Threading;

namespace Carbon.Extensions;

public static class TypeEx
{
	internal static Type _object = typeof(object);

	internal static Type _bool = typeof(bool);

	internal static Type _char = typeof(char);

	internal static Type _sbyte = typeof(sbyte);

	internal static Type _byte = typeof(byte);

	internal static Type _short = typeof(short);

	internal static Type _ushort = typeof(ushort);

	internal static Type _int = typeof(int);

	internal static Type _uint = typeof(uint);

	internal static Type _long = typeof(long);

	internal static Type _ulong = typeof(ulong);

	internal static Type _float = typeof(float);

	internal static Type _double = typeof(double);

	internal static Type _decimal = typeof(decimal);

	internal static Type _dateTime = typeof(DateTime);

	internal static Type _string = typeof(string);

	internal static IFormatProvider _provider = Thread.CurrentThread.CurrentCulture;

	public static object ConvertType<T>(object value)
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle == null)
		{
			throw new ArgumentNullException("conversionType");
		}
		if (value == null)
		{
			if (typeFromHandle.IsValueType)
			{
				throw new InvalidCastException("Value is null (value type)");
			}
			return null;
		}
		if (!(value is IConvertible convertible))
		{
			if (!(value.GetType() == typeFromHandle))
			{
				throw new InvalidCastException("Value is not convertible");
			}
			return (T)value;
		}
		if (typeFromHandle == _bool)
		{
			return convertible.ToBoolean(_provider);
		}
		if (typeFromHandle == _string)
		{
			return convertible.ToString(_provider);
		}
		if (typeFromHandle == _int)
		{
			return convertible.ToInt32(_provider);
		}
		if (typeFromHandle == _float)
		{
			return convertible.ToSingle(_provider);
		}
		if (typeFromHandle == _double)
		{
			return convertible.ToDouble(_provider);
		}
		if (typeFromHandle == _decimal)
		{
			return convertible.ToDecimal(_provider);
		}
		if (typeFromHandle == _long)
		{
			return convertible.ToInt64(_provider);
		}
		if (typeFromHandle == _dateTime)
		{
			return convertible.ToDateTime(_provider);
		}
		if (typeFromHandle == _char)
		{
			return convertible.ToChar(_provider);
		}
		if (typeFromHandle == _byte)
		{
			return convertible.ToByte(_provider);
		}
		if (typeFromHandle == _uint)
		{
			return convertible.ToUInt32(_provider);
		}
		if (typeFromHandle == _ulong)
		{
			return convertible.ToUInt64(_provider);
		}
		if (typeFromHandle == _short)
		{
			return convertible.ToInt16(_provider);
		}
		if (typeFromHandle == _ushort)
		{
			return convertible.ToUInt16(_provider);
		}
		if (typeFromHandle == _sbyte)
		{
			return convertible.ToSByte(_provider);
		}
		if (!(typeFromHandle == _object))
		{
			return convertible.ToType(typeFromHandle, _provider);
		}
		return value;
	}
}
