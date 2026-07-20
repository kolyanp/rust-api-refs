using System;

namespace Carbon;

public class Cache
{
	public class CUI
	{
		public static readonly string BlankColor = "0 0 0 0";

		public static readonly string BlackColor = "0 0 0 1";

		public static readonly string WhiteColor = "1 1 1 1";
	}

	public static readonly object False = false;

	public static readonly object True = true;

	public static readonly object EmptyString = string.Empty;

	public static readonly object SpaceString = " ";

	public static readonly object DefaultSByte = (sbyte)0;

	public static readonly object DefaultChar = '\0';

	public static readonly object DefaultInt16 = (short)0;

	public static readonly object DefaultInt64 = 0L;

	public static readonly object DefaultByte = (byte)0;

	public static readonly object DefaultUInt16 = (ushort)0;

	public static readonly object DefaultUInt32 = 0u;

	public static readonly object DefaultUInt64 = 0uL;

	public static readonly object DefaultSingle = 0f;

	public static readonly object DefaultDouble = 0.0;

	public static readonly object DefaultDecimal = 0m;

	public static readonly object DefaultDateTime = default(DateTime);
}
