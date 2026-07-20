using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

public static class ImageProcessing
{
	private static byte[] signaturePNG = new byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 };

	private static byte[] signatureIHDR = new byte[8] { 0, 0, 0, 13, 73, 72, 68, 82 };

	public static void GaussianBlur2D(float[] data, int len1, int len2, int iterations = 1)
	{
		float[] array = data;
		float[] array2 = new float[len1 * len2];
		for (int i = 0; i < iterations; i++)
		{
			for (int j = 0; j < len1; j++)
			{
				int num = Mathf.Max(0, j - 1);
				int num2 = Mathf.Min(len1 - 1, j + 1);
				for (int k = 0; k < len2; k++)
				{
					int num3 = Mathf.Max(0, k - 1);
					int num4 = Mathf.Min(len2 - 1, k + 1);
					float num5 = array[j * len2 + k] * 4f + array[j * len2 + num3] + array[j * len2 + num4] + array[num * len2 + k] + array[num2 * len2 + k];
					array2[j * len2 + k] = num5 * 0.125f;
				}
			}
			GenericsUtil.Swap<float[]>(ref array, ref array2);
		}
		if (array != data)
		{
			Buffer.BlockCopy(array, 0, data, 0, data.Length * 4);
		}
	}

	public static void GaussianBlur2D(float[] data, int len1, int len2, int len3, int iterations = 1)
	{
		float[] src = data;
		float[] dst = new float[len1 * len2 * len3];
		for (int i = 0; i < iterations; i++)
		{
			Parallel.For(0, len1, delegate(int x)
			{
				int num = Mathf.Max(0, x - 1);
				int num2 = Mathf.Min(len1 - 1, x + 1);
				for (int j = 0; j < len2; j++)
				{
					int num3 = Mathf.Max(0, j - 1);
					int num4 = Mathf.Min(len2 - 1, j + 1);
					for (int k = 0; k < len3; k++)
					{
						float num5 = src[(x * len2 + j) * len3 + k] * 4f + src[(x * len2 + num3) * len3 + k] + src[(x * len2 + num4) * len3 + k] + src[(num * len2 + j) * len3 + k] + src[(num2 * len2 + j) * len3 + k];
						dst[(x * len2 + j) * len3 + k] = num5 * 0.125f;
					}
				}
			});
			GenericsUtil.Swap<float[]>(ref src, ref dst);
		}
		if (src != data)
		{
			Buffer.BlockCopy(src, 0, data, 0, data.Length * 4);
		}
	}

	public static void Average2D(float[] data, int len1, int len2, int iterations = 1)
	{
		float[] src = data;
		float[] dst = new float[len1 * len2];
		for (int i = 0; i < iterations; i++)
		{
			Parallel.For(0, len1, delegate(int x)
			{
				int num = Mathf.Max(0, x - 1);
				int num2 = Mathf.Min(len1 - 1, x + 1);
				for (int j = 0; j < len2; j++)
				{
					int num3 = Mathf.Max(0, j - 1);
					int num4 = Mathf.Min(len2 - 1, j + 1);
					float num5 = src[x * len2 + j] + src[x * len2 + num3] + src[x * len2 + num4] + src[num * len2 + j] + src[num2 * len2 + j];
					dst[x * len2 + j] = num5 * 0.2f;
				}
			});
			GenericsUtil.Swap<float[]>(ref src, ref dst);
		}
		if (src != data)
		{
			Buffer.BlockCopy(src, 0, data, 0, data.Length * 4);
		}
	}

	public static void Average2D(float[] data, int len1, int len2, int len3, int iterations = 1)
	{
		float[] src = data;
		float[] dst = new float[len1 * len2 * len3];
		for (int i = 0; i < iterations; i++)
		{
			Parallel.For(0, len1, delegate(int x)
			{
				int num = Mathf.Max(0, x - 1);
				int num2 = Mathf.Min(len1 - 1, x + 1);
				for (int j = 0; j < len2; j++)
				{
					int num3 = Mathf.Max(0, j - 1);
					int num4 = Mathf.Min(len2 - 1, j + 1);
					for (int k = 0; k < len3; k++)
					{
						float num5 = src[(x * len2 + j) * len3 + k] + src[(x * len2 + num3) * len3 + k] + src[(x * len2 + num4) * len3 + k] + src[(num * len2 + j) * len3 + k] + src[(num2 * len2 + j) * len3 + k];
						dst[(x * len2 + j) * len3 + k] = num5 * 0.2f;
					}
				}
			});
			GenericsUtil.Swap<float[]>(ref src, ref dst);
		}
		if (src != data)
		{
			Buffer.BlockCopy(src, 0, data, 0, data.Length * 4);
		}
	}

	public static void Upsample2D(float[] src, int srclen1, int srclen2, float[] dst, int dstlen1, int dstlen2)
	{
		if (2 * srclen1 != dstlen1 || 2 * srclen2 != dstlen2)
		{
			return;
		}
		Parallel.For(0, srclen1, delegate(int x)
		{
			int num = Mathf.Max(0, x - 1);
			int num2 = Mathf.Min(srclen1 - 1, x + 1);
			for (int i = 0; i < srclen2; i++)
			{
				int num3 = Mathf.Max(0, i - 1);
				int num4 = Mathf.Min(srclen2 - 1, i + 1);
				float num5 = src[x * srclen2 + i] * 6f;
				float num6 = num5 + src[num * srclen2 + i] + src[x * srclen2 + num3];
				dst[2 * x * dstlen2 + 2 * i] = num6 * 0.125f;
				float num7 = num5 + src[num2 * srclen2 + i] + src[x * srclen2 + num3];
				dst[(2 * x + 1) * dstlen2 + 2 * i] = num7 * 0.125f;
				float num8 = num5 + src[num * srclen2 + i] + src[x * srclen2 + num4];
				dst[2 * x * dstlen2 + (2 * i + 1)] = num8 * 0.125f;
				float num9 = num5 + src[num2 * srclen2 + i] + src[x * srclen2 + num4];
				dst[(2 * x + 1) * dstlen2 + (2 * i + 1)] = num9 * 0.125f;
			}
		});
	}

	public static void Upsample2D(float[] src, int srclen1, int srclen2, int srclen3, float[] dst, int dstlen1, int dstlen2, int dstlen3)
	{
		if (2 * srclen1 != dstlen1 || 2 * srclen2 != dstlen2 || srclen3 != dstlen3)
		{
			return;
		}
		Parallel.For(0, srclen1, delegate(int x)
		{
			int num = Mathf.Max(0, x - 1);
			int num2 = Mathf.Min(srclen1 - 1, x + 1);
			for (int i = 0; i < srclen2; i++)
			{
				int num3 = Mathf.Max(0, i - 1);
				int num4 = Mathf.Min(srclen2 - 1, i + 1);
				for (int j = 0; j < srclen3; j++)
				{
					float num5 = src[(x * srclen2 + i) * srclen3 + j] * 6f;
					float num6 = num5 + src[(num * srclen2 + i) * srclen3 + j] + src[(x * srclen2 + num3) * srclen3 + j];
					dst[(2 * x * dstlen2 + 2 * i) * dstlen3 + j] = num6 * 0.125f;
					float num7 = num5 + src[(num2 * srclen2 + i) * srclen3 + j] + src[(x * srclen2 + num3) * srclen3 + j];
					dst[((2 * x + 1) * dstlen2 + 2 * i) * dstlen3 + j] = num7 * 0.125f;
					float num8 = num5 + src[(num * srclen2 + i) * srclen3 + j] + src[(x * srclen2 + num4) * srclen3 + j];
					dst[(2 * x * dstlen2 + (2 * i + 1)) * dstlen3 + j] = num8 * 0.125f;
					float num9 = num5 + src[(num2 * srclen2 + i) * srclen3 + j] + src[(x * srclen2 + num4) * srclen3 + j];
					dst[((2 * x + 1) * dstlen2 + (2 * i + 1)) * dstlen3 + j] = num9 * 0.125f;
				}
			}
		});
	}

	public static void Dilate2D(NativeArray<int> src, int len1, int len2, int srcmask, int radius, Action<int, int> action)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Parallel.For(0, len1, delegate(int x)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected O, but got Unknown
			MaxQueue val = new MaxQueue(radius * 2 + 1);
			for (int i = 0; i < radius; i++)
			{
				val.Push(src[x * len2 + i] & srcmask);
			}
			for (int j = 0; j < len2; j++)
			{
				if (j > radius)
				{
					val.Pop();
				}
				if (j < len2 - radius)
				{
					val.Push(src[x * len2 + j + radius] & srcmask);
				}
				if (val.Max != 0)
				{
					action(x, j);
				}
			}
		});
		Parallel.For(0, len2, delegate(int y)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected O, but got Unknown
			MaxQueue val = new MaxQueue(radius * 2 + 1);
			for (int i = 0; i < radius; i++)
			{
				val.Push(src[i * len2 + y] & srcmask);
			}
			for (int j = 0; j < len1; j++)
			{
				if (j > radius)
				{
					val.Pop();
				}
				if (j < len1 - radius)
				{
					val.Push(src[(j + radius) * len2 + y] & srcmask);
				}
				if (val.Max != 0)
				{
					action(j, y);
				}
			}
		});
	}

	public static void Dilate2D(int[] src, int len1, int len2, int srcmask, int radius, Action<int, int> action)
	{
		Parallel.For(0, len1, delegate(int x)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected O, but got Unknown
			MaxQueue val = new MaxQueue(radius * 2 + 1);
			for (int i = 0; i < radius; i++)
			{
				val.Push(src[x * len2 + i] & srcmask);
			}
			for (int j = 0; j < len2; j++)
			{
				if (j > radius)
				{
					val.Pop();
				}
				if (j < len2 - radius)
				{
					val.Push(src[x * len2 + j + radius] & srcmask);
				}
				if (val.Max != 0)
				{
					action(x, j);
				}
			}
		});
		Parallel.For(0, len2, delegate(int y)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected O, but got Unknown
			MaxQueue val = new MaxQueue(radius * 2 + 1);
			for (int i = 0; i < radius; i++)
			{
				val.Push(src[i * len2 + y] & srcmask);
			}
			for (int j = 0; j < len1; j++)
			{
				if (j > radius)
				{
					val.Pop();
				}
				if (j < len1 - radius)
				{
					val.Push(src[(j + radius) * len2 + y] & srcmask);
				}
				if (val.Max != 0)
				{
					action(j, y);
				}
			}
		});
	}

	public static void FloodFill2D(int x, int y, int len1, int len2, Func<int, int, bool> check, Action<int, int> action)
	{
		Stack<KeyValuePair<int, int>> stack = new Stack<KeyValuePair<int, int>>();
		stack.Push(new KeyValuePair<int, int>(x, y));
		while (stack.Count > 0)
		{
			KeyValuePair<int, int> keyValuePair = stack.Pop();
			x = keyValuePair.Key;
			y = keyValuePair.Value;
			int num = x;
			while (num >= 0 && check(num, y))
			{
				num--;
			}
			num++;
			bool flag2;
			bool flag = (flag2 = false);
			for (; num < len1 && check(num, y); num++)
			{
				action(num, y);
				if (y > 0)
				{
					bool flag3 = check(num, y - 1);
					if (!flag && flag3)
					{
						stack.Push(new KeyValuePair<int, int>(num, y - 1));
						flag = true;
					}
					else if (flag && !flag3)
					{
						flag = false;
					}
				}
				if (y < len1 - 1)
				{
					bool flag4 = check(num, y + 1);
					if (!flag2 && flag4)
					{
						stack.Push(new KeyValuePair<int, int>(num, y + 1));
						flag2 = true;
					}
					else if (flag2 && !flag4)
					{
						flag2 = false;
					}
				}
			}
		}
	}

	public static bool IsValidPNG(byte[] data, int maxSizeSquare)
	{
		return IsValidPNG(data, maxSizeSquare, maxSizeSquare);
	}

	public static bool IsValidPNG(byte[] data, int maxWidth, int maxHeight)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		if (data == null || data.Length < 29)
		{
			return false;
		}
		if (data.Length > 29 + maxWidth * maxHeight * 4)
		{
			return false;
		}
		for (int i = 0; i < signaturePNG.Length; i++)
		{
			if (data[i] != signaturePNG[i])
			{
				return false;
			}
		}
		for (int j = 0; j < signatureIHDR.Length; j++)
		{
			if (data[8 + j] != signatureIHDR[j])
			{
				return false;
			}
		}
		Union32 val = new Union32
		{
			b4 = data[16],
			b3 = data[17],
			b2 = data[18],
			b1 = data[19]
		};
		if (val.i < 1 || val.i > maxWidth)
		{
			return false;
		}
		Union32 val2 = new Union32
		{
			b4 = data[20],
			b3 = data[21],
			b2 = data[22],
			b1 = data[23]
		};
		if (val2.i < 1 || val2.i > maxHeight)
		{
			return false;
		}
		byte b = data[24];
		if (b != 8 && b != 16)
		{
			return false;
		}
		byte b2 = data[25];
		if (b2 != 2 && b2 != 6)
		{
			return false;
		}
		if (data[26] != 0)
		{
			return false;
		}
		if (data[27] != 0)
		{
			return false;
		}
		if (data[28] != 0)
		{
			return false;
		}
		return true;
	}

	public static bool IsValidJPG(byte[] data, int maxSizeSquare)
	{
		return IsValidJPG(data, maxSizeSquare, maxSizeSquare);
	}

	public static bool IsValidJPG(byte[] data, int maxWidth, int maxHeight)
	{
		if (data.Length < 30)
		{
			return false;
		}
		if (data.Length > 30 + maxWidth * maxHeight)
		{
			return false;
		}
		try
		{
			if (data[0] != byte.MaxValue || data[1] != 216)
			{
				return false;
			}
			if (data[2] != byte.MaxValue || data[3] != 224)
			{
				return false;
			}
			if (data[6] != 74 || data[7] != 70 || data[8] != 73 || data[9] != 70 || data[10] != 0)
			{
				return false;
			}
			if (data[13] != 0)
			{
				return false;
			}
			if (data[14] != data[16] || data[15] != data[17])
			{
				return false;
			}
			int num = 4;
			int num2 = (data[num] << 8) | data[num + 1];
			while (num < data.Length)
			{
				num += num2;
				if (num >= data.Length)
				{
					return false;
				}
				if (data[num] != byte.MaxValue)
				{
					return false;
				}
				if (data[num + 1] == 192 || data[num + 1] == 193 || data[num + 1] == 194)
				{
					int num3 = (data[num + 5] << 8) | data[num + 6];
					return ((data[num + 7] << 8) | data[num + 8]) <= maxWidth && num3 <= maxHeight;
				}
				num += 2;
				num2 = (data[num] << 8) | data[num + 1];
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	public static bool IsClear(Color32[] data)
	{
		for (int i = 0; i < data.Length; i++)
		{
			if (data[i].a > 5)
			{
				return false;
			}
		}
		return true;
	}
}
