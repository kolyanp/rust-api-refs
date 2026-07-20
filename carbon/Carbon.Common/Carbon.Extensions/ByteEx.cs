using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Carbon.Extensions;

public static class ByteEx
{
	public enum ByteTypes
	{
		Auto,
		Byte,
		Kilobyte,
		Megabyte,
		Gigabyte,
		Terabyte,
		Petabyte,
		Exabyte
	}

	public static string Format<T>(this T value, ByteTypes type = ByteTypes.Auto, bool shortName = true, string valueFormat = "0.0", string stringFormat = "{0} {1}") where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
	{
		long num = (long)Convert.ChangeType(value, typeof(long));
		double num2 = 0.0;
		string arg = "";
		if ((num >= 1152921504606846976L && type == ByteTypes.Auto) || type == ByteTypes.Exabyte)
		{
			arg = (shortName ? "Eb" : "Exabytes");
			num2 = num >> 50;
		}
		else if ((num >= 1125899906842624L && type == ByteTypes.Auto) || type == ByteTypes.Petabyte)
		{
			arg = (shortName ? "Pb" : "Petabytes");
			num2 = num >> 40;
		}
		else if ((num >= 1099511627776L && type == ByteTypes.Auto) || type == ByteTypes.Terabyte)
		{
			arg = (shortName ? "Tb" : "Terabytes");
			num2 = num >> 30;
		}
		else if ((num >= 1073741824 && type == ByteTypes.Auto) || type == ByteTypes.Gigabyte)
		{
			arg = (shortName ? "Gb" : "Gigabytes");
			num2 = num >> 20;
		}
		else if ((num >= 1048576 && type == ByteTypes.Auto) || type == ByteTypes.Megabyte)
		{
			arg = (shortName ? "Mb" : "Megabytes");
			num2 = num >> 10;
		}
		else
		{
			if ((num < 1024 && type == ByteTypes.Auto) || type == ByteTypes.Byte)
			{
				arg = (shortName ? "B" : "Bytes");
				return string.Format(stringFormat, num.ToString(valueFormat), arg);
			}
			if (type == ByteTypes.Auto || type == ByteTypes.Kilobyte)
			{
				arg = (shortName ? "Kb" : "Kilobytes");
				num2 = num;
			}
		}
		return string.Format(stringFormat, (num2 / 1024.0).ToString(valueFormat), arg);
	}

	public static T MakeCopy<T>(this T source)
	{
		if (!typeof(T).IsSerializable)
		{
			throw new ArgumentException("The type must be serializable.", "source");
		}
		if (source == null)
		{
			return default(T);
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, source);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return (T)binaryFormatter.Deserialize(memoryStream);
	}
}
