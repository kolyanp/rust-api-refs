using System;
using System.IO;

public static class OggVorbisLength
{
	private class BitReader
	{
		private readonly byte[] data;

		private readonly long end;

		private long position;

		public bool Failed { get; private set; }

		public BitReader(byte[] data, int length, int startByte)
		{
			this.data = data;
			end = (long)length * 8L;
			position = (long)startByte * 8L;
		}

		public int Read(int count)
		{
			if (!Advance(count))
			{
				return 0;
			}
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				long num2 = position - count + i;
				num |= ((data[num2 >> 3] >> (int)(num2 & 7)) & 1) << i;
			}
			return num;
		}

		public void Skip(long count)
		{
			Advance(count);
		}

		private bool Advance(long count)
		{
			if (Failed || count > end - position)
			{
				Failed = true;
				return false;
			}
			position += count;
			return true;
		}
	}

	private const int MaxHeaderBytes = 65536;

	private const int MaxCodebookSize = 32768;

	private static readonly uint[] CrcTable = BuildCrcTable();

	public static bool TryGetSampleCount(byte[] data, out int channels, out int sampleRate, out long samples)
	{
		channels = (sampleRate = 0);
		samples = 0L;
		MemoryStream memoryStream = new MemoryStream();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		bool[] array = null;
		int num6 = 0;
		int num7 = 0;
		bool flag = false;
		bool flag2 = false;
		uint num8 = 0u;
		uint num9 = 0u;
		while (num6 < data.Length)
		{
			if (flag2 || !TryReadPage(data, num6, out var segments, out var pageEnd))
			{
				return false;
			}
			byte b = data[num6 + 5];
			uint num10 = BitConverter.ToUInt32(data, num6 + 14);
			uint num11 = BitConverter.ToUInt32(data, num6 + 18);
			bool flag3 = num6 == 0;
			if ((b & 2) != 0 != flag3 || (b & 1) != 0 != flag)
			{
				return false;
			}
			if (!flag3 && (num10 != num8 || num11 != num9 + 1))
			{
				return false;
			}
			num8 = num10;
			num9 = num11;
			flag2 = (b & 4) != 0;
			int num12 = num6 + 27 + segments;
			for (int i = 0; i < segments; i++)
			{
				byte b2 = data[num6 + 27 + i];
				if (!flag)
				{
					num7 = ((b2 <= 0) ? 1 : data[num12]);
				}
				if (num < 3)
				{
					if (memoryStream.Length + b2 > 65536)
					{
						return false;
					}
					memoryStream.Write(data, num12, b2);
				}
				num12 += b2;
				flag = b2 == byte.MaxValue;
				if (flag)
				{
					continue;
				}
				if (num < 3)
				{
					byte[] buffer = memoryStream.GetBuffer();
					int num13 = (int)memoryStream.Length;
					if (num13 < 7 || buffer[0] != 1 + 2 * num || !HasVorbisTag(buffer))
					{
						return false;
					}
					switch (num)
					{
					case 0:
						if (num13 < 30 || BitConverter.ToUInt32(buffer, 7) != 0 || (buffer[29] & 1) == 0)
						{
							return false;
						}
						channels = buffer[11];
						sampleRate = BitConverter.ToInt32(buffer, 12);
						num2 = 1 << (buffer[28] & 0xF);
						num3 = 1 << (buffer[28] >> 4);
						if (channels < 1 || sampleRate < 1 || num2 < 256 || num3 < num2 || num3 > 8192)
						{
							return false;
						}
						break;
					case 2:
						array = ReadLongModes(buffer, num13, channels);
						if (array == null)
						{
							return false;
						}
						num4 = ILog(array.Length - 1);
						break;
					}
					memoryStream.SetLength(0L);
					num++;
				}
				else
				{
					int num14 = (num7 >> 1) & ((1 << num4) - 1);
					if ((num7 & 1) != 0 || num14 >= array.Length)
					{
						return false;
					}
					int num15 = (array[num14] ? num3 : num2);
					if (num5 > 0)
					{
						samples += (num5 + num15) / 4;
					}
					num5 = num15;
				}
			}
			num6 = pageEnd;
		}
		if (flag2 && !flag && num == 3)
		{
			return samples > 0;
		}
		return false;
	}

	private static bool TryReadPage(byte[] data, int position, out int segments, out int pageEnd)
	{
		segments = (pageEnd = 0);
		if (data.Length - position < 27 || data[position] != 79 || data[position + 1] != 103 || data[position + 2] != 103 || data[position + 3] != 83 || data[position + 4] != 0)
		{
			return false;
		}
		segments = data[position + 26];
		pageEnd = position + 27 + segments;
		if (pageEnd > data.Length)
		{
			return false;
		}
		for (int i = 0; i < segments; i++)
		{
			pageEnd += data[position + 27 + i];
		}
		if (pageEnd > data.Length)
		{
			return false;
		}
		uint num = 0u;
		for (int j = position; j < pageEnd; j++)
		{
			num = (num << 8) ^ CrcTable[(num >> 24) ^ (uint)((j < position + 22 || j >= position + 26) ? data[j] : 0)];
		}
		return num == BitConverter.ToUInt32(data, position + 22);
	}

	private static bool HasVorbisTag(byte[] packet)
	{
		if (packet[1] == 118 && packet[2] == 111 && packet[3] == 114 && packet[4] == 98 && packet[5] == 105)
		{
			return packet[6] == 115;
		}
		return false;
	}

	private static bool[] ReadLongModes(byte[] packet, int length, int channels)
	{
		BitReader bitReader = new BitReader(packet, length, 7);
		int num = bitReader.Read(8) + 1;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (bitReader.Read(24) != 5653314)
			{
				return null;
			}
			int num3 = bitReader.Read(16);
			int num4 = bitReader.Read(24);
			if (ILog(num3) + ILog(num4) > 24)
			{
				return null;
			}
			num2 += num4 * Math.Max(num3, 1);
			if (num2 > 32768)
			{
				return null;
			}
			if (bitReader.Read(1) == 0)
			{
				bool flag = bitReader.Read(1) == 1;
				for (int j = 0; j < num4; j++)
				{
					if (bitReader.Failed)
					{
						break;
					}
					if (!flag || bitReader.Read(1) == 1)
					{
						bitReader.Skip(5L);
					}
				}
			}
			else
			{
				int num5 = bitReader.Read(5) + 1;
				int num6 = 0;
				while (num6 < num4 && !bitReader.Failed)
				{
					int num7 = bitReader.Read(ILog(num4 - num6));
					if (num5 > 32 || num7 > num4 - num6)
					{
						return null;
					}
					num6 += num7;
					num5++;
				}
			}
			int num8 = bitReader.Read(4);
			switch (num8)
			{
			case 1:
			case 2:
			{
				bitReader.Skip(64L);
				int num9 = bitReader.Read(4) + 1;
				bitReader.Skip(1L);
				bitReader.Skip(((num8 == 1) ? Lookup1Values(num4, num3) : ((long)num4 * (long)num3)) * num9);
				break;
			}
			default:
				return null;
			case 0:
				break;
			}
		}
		int num10 = bitReader.Read(6) + 1;
		for (int k = 0; k < num10; k++)
		{
			if (bitReader.Read(16) != 0)
			{
				return null;
			}
		}
		int num11 = bitReader.Read(6) + 1;
		for (int l = 0; l < num11; l++)
		{
			switch (bitReader.Read(16))
			{
			case 0:
				bitReader.Skip(54L);
				bitReader.Skip((bitReader.Read(4) + 1) * 8);
				break;
			case 1:
			{
				int[] array = new int[bitReader.Read(5)];
				int[] array2 = new int[16];
				int num12 = 0;
				for (int m = 0; m < array.Length; m++)
				{
					array[m] = bitReader.Read(4);
					num12 = Math.Max(num12, array[m] + 1);
				}
				for (int n = 0; n < num12; n++)
				{
					array2[n] = bitReader.Read(3) + 1;
					int num13 = bitReader.Read(2);
					bitReader.Skip(((num13 > 0) ? 8 : 0) + (8 << num13));
				}
				bitReader.Skip(2L);
				int num14 = bitReader.Read(4);
				int[] array3 = array;
				foreach (int num16 in array3)
				{
					bitReader.Skip(array2[num16] * num14);
				}
				break;
			}
			default:
				return null;
			}
		}
		int num17 = bitReader.Read(6) + 1;
		for (int num18 = 0; num18 < num17; num18++)
		{
			if (bitReader.Read(16) > 2)
			{
				return null;
			}
			bitReader.Skip(72L);
			int num19 = bitReader.Read(6) + 1;
			bitReader.Skip(8L);
			int num20 = 0;
			for (int num21 = 0; num21 < num19; num21++)
			{
				int num22 = bitReader.Read(3);
				if (bitReader.Read(1) == 1)
				{
					num22 |= bitReader.Read(5) << 3;
				}
				while (num22 != 0)
				{
					num20++;
					num22 &= num22 - 1;
				}
			}
			bitReader.Skip(num20 * 8);
		}
		int num23 = bitReader.Read(6) + 1;
		for (int num24 = 0; num24 < num23; num24++)
		{
			if (bitReader.Read(16) != 0)
			{
				return null;
			}
			int num25 = ((bitReader.Read(1) != 1) ? 1 : (bitReader.Read(4) + 1));
			if (bitReader.Read(1) == 1)
			{
				bitReader.Skip((bitReader.Read(8) + 1) * 2 * ILog(channels - 1));
			}
			if (bitReader.Read(2) != 0)
			{
				return null;
			}
			if (num25 > 1)
			{
				bitReader.Skip(channels * 4);
			}
			bitReader.Skip(num25 * 24);
		}
		bool[] array4 = new bool[bitReader.Read(6) + 1];
		for (int num26 = 0; num26 < array4.Length; num26++)
		{
			array4[num26] = bitReader.Read(1) == 1;
			if (bitReader.Read(16) != 0 || bitReader.Read(16) != 0 || bitReader.Read(8) >= num23)
			{
				return null;
			}
		}
		if (bitReader.Read(1) != 1 || bitReader.Failed)
		{
			return null;
		}
		return array4;
	}

	private static long Lookup1Values(long entries, long dimensions)
	{
		if (dimensions == 0L)
		{
			return 0L;
		}
		long num = (long)Math.Pow(entries, 1.0 / (double)dimensions);
		while (num > 0 && !PowerFits(num, dimensions, entries))
		{
			num--;
		}
		for (; PowerFits(num + 1, dimensions, entries); num++)
		{
		}
		return num;
	}

	private static bool PowerFits(long value, long exponent, long limit)
	{
		if (value <= 1)
		{
			return value <= limit;
		}
		long num = 1L;
		for (int i = 0; i < exponent; i++)
		{
			if (num > limit)
			{
				break;
			}
			num *= value;
		}
		return num <= limit;
	}

	private static int ILog(long value)
	{
		int num = 0;
		while (value > 0)
		{
			num++;
			value >>= 1;
		}
		return num;
	}

	private static uint[] BuildCrcTable()
	{
		uint[] array = new uint[256];
		for (uint num = 0u; num < 256; num++)
		{
			uint num2 = num << 24;
			for (int i = 0; i < 8; i++)
			{
				num2 = (((num2 & 0x80000000u) != 0) ? ((num2 << 1) ^ 0x4C11DB7) : (num2 << 1));
			}
			array[num] = num2;
		}
		return array;
	}
}
