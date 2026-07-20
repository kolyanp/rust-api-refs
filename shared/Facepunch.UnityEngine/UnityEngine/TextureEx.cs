namespace UnityEngine;

public static class TextureEx
{
	private static Color32[] buffer = (Color32[])(object)new Color32[8192];

	public static void Clear(this Texture2D tex, Color32 color)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)tex == (Object)null)
		{
			return;
		}
		if (((Texture)tex).width > buffer.Length)
		{
			Debug.LogError((object)("Trying to clear texture that is too big: " + ((Texture)tex).width));
			return;
		}
		for (int i = 0; i < ((Texture)tex).width; i++)
		{
			buffer[i] = color;
		}
		for (int j = 0; j < ((Texture)tex).height; j++)
		{
			tex.SetPixels32(0, j, ((Texture)tex).width, 1, buffer);
		}
		tex.Apply();
	}

	public static int GetSizeInBytes(this Texture texture)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		int num = texture.width;
		int num2 = texture.height;
		if (texture is Texture2D)
		{
			Texture obj = ((texture is Texture2D) ? texture : null);
			int bitsPerPixel = GetBitsPerPixel(((Texture2D)obj).format);
			int mipmapCount = obj.mipmapCount;
			int i = 1;
			int num3 = 0;
			int loadedMipmapLevel = ((Texture2D)obj).loadedMipmapLevel;
			for (; i <= mipmapCount; i++)
			{
				if (i >= loadedMipmapLevel)
				{
					num3 += num * num2 * bitsPerPixel / 8;
				}
				num /= 2;
				num2 /= 2;
			}
			return num3;
		}
		if (texture is Texture2DArray)
		{
			Texture obj2 = ((texture is Texture2DArray) ? texture : null);
			int bitsPerPixel2 = GetBitsPerPixel(((Texture2DArray)obj2).format);
			int num4 = 10;
			int j = 1;
			int num5 = 0;
			int depth = ((Texture2DArray)obj2).depth;
			for (; j <= num4; j++)
			{
				num5 += num * num2 * bitsPerPixel2 / 8;
				num /= 2;
				num2 /= 2;
			}
			return num5 * depth;
		}
		if (texture is Cubemap)
		{
			int bitsPerPixel3 = GetBitsPerPixel(((Cubemap)((texture is Cubemap) ? texture : null)).format);
			int num6 = num * num2 * bitsPerPixel3 / 8;
			int num7 = 6;
			return num6 * num7;
		}
		if (texture is RenderTexture)
		{
			RenderTexture val = (RenderTexture)(object)((texture is RenderTexture) ? texture : null);
			int bitsPerPixel4 = GetBitsPerPixel(val.format, val.depth);
			int mipmapCount2 = ((Texture)val).mipmapCount;
			int k = 1;
			int num8 = 0;
			for (; k <= mipmapCount2; k++)
			{
				num8 += num * num2 * bitsPerPixel4 / 8;
				num /= 2;
				num2 /= 2;
			}
			return num8;
		}
		return 0;
	}

	public static int GetBitsPerPixel(TextureFormat format)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected I4, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Invalid comparison between Unknown and I4
		switch (format - 1)
		{
		default:
			if ((int)format != 47)
			{
				break;
			}
			return 8;
		case 0:
			return 8;
		case 1:
			return 16;
		case 12:
			return 16;
		case 2:
			return 24;
		case 3:
			return 32;
		case 4:
			return 32;
		case 6:
			return 16;
		case 9:
		case 27:
			return 4;
		case 11:
		case 24:
		case 28:
			return 8;
		case 29:
			return 2;
		case 30:
			return 2;
		case 31:
			return 4;
		case 32:
			return 4;
		case 33:
			return 4;
		case 13:
			return 32;
		case 5:
		case 7:
		case 8:
		case 10:
		case 14:
		case 15:
		case 16:
		case 17:
		case 18:
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
		case 25:
		case 26:
			break;
		}
		return 0;
	}

	public static int GetBitsPerPixel(RenderTextureFormat format, int depthBits)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected I4, but got Unknown
		switch ((int)format)
		{
		case 11:
		case 17:
			return 128;
		case 2:
		case 9:
		case 10:
		case 12:
		case 18:
		case 24:
		case 26:
			return 64;
		default:
			return 32;
		case 4:
		case 5:
		case 6:
		case 15:
		case 25:
		case 28:
			return 16;
		case 16:
			return 8;
		case 1:
		case 3:
			return depthBits;
		}
	}
}
