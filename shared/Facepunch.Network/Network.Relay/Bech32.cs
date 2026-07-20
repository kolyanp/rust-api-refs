using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Network.Relay;

public static class Bech32
{
	private const string CHARSET = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";

	private static readonly int[] GENERATOR = new int[5] { 996825010, 642813549, 513874426, 1027748829, 705979059 };

	public static string Encode(string hrp, byte[] data)
	{
		byte[] array = ConvertBits(data, 8, 5, pad: true);
		byte[] second = CreateChecksum(hrp, array);
		byte[] array2 = array.Concat(second).ToArray();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(hrp);
		stringBuilder.Append('1');
		byte[] array3 = array2;
		foreach (byte index in array3)
		{
			stringBuilder.Append("qpzry9x8gf2tvdw0s3jn54khce6mua7l"[index]);
		}
		return stringBuilder.ToString();
	}

	public static byte[] Decode(string bech32, out string hrp)
	{
		if (string.IsNullOrWhiteSpace(bech32))
		{
			throw new ArgumentException("Invalid Bech32 string");
		}
		bech32 = bech32.ToLowerInvariant();
		int num = bech32.LastIndexOf('1');
		if (num < 1)
		{
			throw new Exception("Invalid Bech32 format");
		}
		hrp = bech32.Substring(0, num);
		int[] array = bech32.Substring(num + 1).Select(delegate(char c)
		{
			int num2 = "qpzry9x8gf2tvdw0s3jn54khce6mua7l".IndexOf(c);
			if (num2 == -1)
			{
				throw new Exception($"Invalid character: {c}");
			}
			return num2;
		}).ToArray();
		if (array.Length < 6)
		{
			throw new Exception("Invalid Bech32 length");
		}
		if (!VerifyChecksum(hrp, array))
		{
			throw new Exception("Invalid checksum");
		}
		return ConvertBits((from x in array.Take(array.Length - 6)
			select (byte)x).ToArray(), 5, 8, pad: false);
	}

	private static byte[] ConvertBits(byte[] data, int fromBits, int toBits, bool pad)
	{
		int num = 0;
		int num2 = 0;
		int num3 = (1 << toBits) - 1;
		List<byte> list = new List<byte>();
		foreach (byte b in data)
		{
			if (b < 0 || b >> fromBits != 0)
			{
				throw new Exception("Invalid data range");
			}
			num = (num << fromBits) | b;
			num2 += fromBits;
			while (num2 >= toBits)
			{
				num2 -= toBits;
				list.Add((byte)((num >> num2) & num3));
			}
		}
		if (pad)
		{
			if (num2 > 0)
			{
				list.Add((byte)((num << toBits - num2) & num3));
			}
		}
		else
		{
			if (num2 >= fromBits)
			{
				throw new Exception("Illegal zero padding");
			}
			if (((num << toBits - num2) & num3) != 0)
			{
				throw new Exception("Non-zero padding");
			}
		}
		return list.ToArray();
	}

	private static byte[] CreateChecksum(string hrp, byte[] data)
	{
		uint num = Polymod(HrpExpand(hrp).Concat(data).Concat(new byte[6]).ToArray()) ^ 1;
		byte[] array = new byte[6];
		for (int i = 0; i < 6; i++)
		{
			array[i] = (byte)((num >> 5 * (5 - i)) & 0x1F);
		}
		return array;
	}

	private static bool VerifyChecksum(string hrp, int[] data)
	{
		return Polymod(HrpExpand(hrp).Concat(data.Select((int x) => (byte)x)).ToArray()) == 1;
	}

	private static byte[] HrpExpand(string hrp)
	{
		byte[] array = new byte[hrp.Length * 2 + 1];
		for (int i = 0; i < hrp.Length; i++)
		{
			array[i] = (byte)((int)hrp[i] >> 5);
		}
		array[hrp.Length] = 0;
		for (int j = 0; j < hrp.Length; j++)
		{
			array[j + hrp.Length + 1] = (byte)(hrp[j] & 0x1F);
		}
		return array;
	}

	private static uint Polymod(byte[] values)
	{
		uint num = 1u;
		foreach (byte b in values)
		{
			uint num2 = num >> 25;
			num = ((num & 0x1FFFFFF) << 5) ^ b;
			for (int j = 0; j < 5; j++)
			{
				if (((num2 >> j) & 1) != 0)
				{
					num ^= (uint)GENERATOR[j];
				}
			}
		}
		return num;
	}
}
