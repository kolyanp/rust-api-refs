using System;
using System.Threading.Tasks;
using UnityEngine;

public static class MapImageRenderer
{
	private readonly struct Array2D<T>(T[] items, int width, int height)
	{
		private readonly T[] _items = items;

		private readonly int _width = width;

		private readonly int _height = height;

		public ref T this[int x, int y]
		{
			get
			{
				int num = Mathf.Clamp(x, 0, _width - 1);
				int num2 = Mathf.Clamp(y, 0, _height - 1);
				return ref _items[num2 * _width + num];
			}
		}
	}

	private static readonly Vector4 StartColor = new Vector4(0.28627452f, 23f / 85f, 0.24705884f, 1f);

	private static readonly Vector4 WaterColor = new Vector4(0.16941601f, 0.31755757f, 0.36200002f, 1f);

	private static readonly Vector4 GravelColor = new Vector4(0.25f, 37f / 152f, 0.22039475f, 1f);

	private static readonly Vector4 DirtColor = new Vector4(0.6f, 0.47959462f, 0.33f, 1f);

	private static readonly Vector4 SandColor = new Vector4(0.7f, 0.65968585f, 0.5277487f, 1f);

	private static readonly Vector4 GrassColor = new Vector4(0.35486364f, 0.37f, 0.2035f, 1f);

	private static readonly Vector4 ForestColor = new Vector4(0.24843751f, 0.3f, 9f / 128f, 1f);

	private static readonly Vector4 RockColor = new Vector4(0.4f, 0.39379844f, 0.37519377f, 1f);

	private static readonly Vector4 SnowColor = new Vector4(0.86274517f, 0.9294118f, 0.94117653f, 1f);

	private static readonly Vector4 PebbleColor = new Vector4(7f / 51f, 0.2784314f, 0.2761563f, 1f);

	private static readonly Vector4 OffShoreColor = new Vector4(0.04090196f, 0.22060032f, 14f / 51f, 1f);

	private static readonly Vector3 SunDirection = Vector3.Normalize(new Vector3(0.95f, 2.87f, 2.37f));

	private const float SunPower = 0.65f;

	private const float Brightness = 1.05f;

	private const float Contrast = 0.94f;

	private const float OceanWaterLevel = 0f;

	private static readonly Vector4 Half = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);

	public static byte[] Render(out int imageWidth, out int imageHeight, out Color background, float scale = 0.5f, bool lossy = true, bool transparent = false, int oceanMargin = 500)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		if (lossy && transparent)
		{
			throw new ArgumentException("Rendering a transparent map is not possible when using lossy compression (JPG)");
		}
		imageWidth = 0;
		imageHeight = 0;
		background = Color.op_Implicit(OffShoreColor);
		TerrainTexturing instance = TerrainTexturing.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return null;
		}
		TerrainMeta component = ((Component)instance).GetComponent<TerrainMeta>();
		TerrainHeightMap terrainHeightMap = ((Component)instance).GetComponent<TerrainHeightMap>();
		TerrainSplatMap terrainSplatMap = ((Component)instance).GetComponent<TerrainSplatMap>();
		TerrainTopologyMap terrainTopologyMap = ((Component)instance).GetComponent<TerrainTopologyMap>();
		if ((Object)(object)component == (Object)null || (Object)(object)terrainHeightMap == (Object)null || (Object)(object)terrainSplatMap == (Object)null || (Object)(object)terrainTopologyMap == (Object)null)
		{
			return null;
		}
		int mapRes = (int)((float)World.Size * Mathf.Clamp(scale, 0.1f, 4f));
		float invMapRes = 1f / (float)mapRes;
		if (mapRes <= 0)
		{
			return null;
		}
		imageWidth = mapRes + oceanMargin * 2;
		imageHeight = mapRes + oceanMargin * 2;
		Color[] array = (Color[])(object)new Color[imageWidth * imageHeight];
		Array2D<Color> output = new Array2D<Color>(array, imageWidth, imageHeight);
		float maxDepth = (transparent ? Mathf.Max(Mathf.Abs(GetHeight(0f, 0f)), 5f) : 50f);
		Vector4 offShoreColor = (transparent ? Vector4.zero : OffShoreColor);
		Vector4 waterColor = (Vector4)(transparent ? new Vector4(WaterColor.x, WaterColor.y, WaterColor.z, 0.5f) : WaterColor);
		Parallel.For(0, imageHeight, delegate(int y)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Unknown result type (might be due to invalid IL or missing references)
			//IL_0243: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_024d: Unknown result type (might be due to invalid IL or missing references)
			//IL_024f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0251: Unknown result type (might be due to invalid IL or missing references)
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_0260: Unknown result type (might be due to invalid IL or missing references)
			//IL_0265: Unknown result type (might be due to invalid IL or missing references)
			//IL_026a: Unknown result type (might be due to invalid IL or missing references)
			//IL_026f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_0208: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0228: Unknown result type (might be due to invalid IL or missing references)
			//IL_022d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0271: Unknown result type (might be due to invalid IL or missing references)
			//IL_0278: Unknown result type (might be due to invalid IL or missing references)
			//IL_027d: Unknown result type (might be due to invalid IL or missing references)
			//IL_02be: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02da: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02df: Unknown result type (might be due to invalid IL or missing references)
			y -= oceanMargin;
			float y2 = (float)y * invMapRes;
			int num = mapRes + oceanMargin;
			for (int i = -oceanMargin; i < num; i++)
			{
				float x = (float)i * invMapRes;
				Vector4 startColor = StartColor;
				float num2 = GetHeight(x, y2);
				Vector3 val = GetNormal(x, y2);
				float num3 = GetShoreDist(x, y2);
				bool flag = (GetTopology(x, y2) & 0x180) != 0;
				float num4 = Math.Max(Vector3.Dot(val, SunDirection), 0f);
				startColor = Vector4.Lerp(startColor, GravelColor, GetSplat(x, y2, 128) * GravelColor.w);
				startColor = Vector4.Lerp(startColor, PebbleColor, GetSplat(x, y2, 64) * PebbleColor.w);
				startColor = Vector4.Lerp(startColor, RockColor, GetSplat(x, y2, 8) * RockColor.w);
				startColor = Vector4.Lerp(startColor, DirtColor, GetSplat(x, y2, 1) * DirtColor.w);
				startColor = Vector4.Lerp(startColor, GrassColor, GetSplat(x, y2, 16) * GrassColor.w);
				startColor = Vector4.Lerp(startColor, ForestColor, GetSplat(x, y2, 32) * ForestColor.w);
				startColor = Vector4.Lerp(startColor, SandColor, GetSplat(x, y2, 4) * SandColor.w);
				startColor = Vector4.Lerp(startColor, SnowColor, GetSplat(x, y2, 2) * SnowColor.w);
				float num5 = 0f;
				if (num3 > 0f)
				{
					num5 = 0f - num2;
					if (num5 <= 0f || !flag)
					{
						num5 = Mathf.Max(num5, 0.1f * num3);
					}
				}
				if (num5 > 0f)
				{
					startColor = Vector4.Lerp(startColor, waterColor, Mathf.Clamp(0.5f + num5 / 5f, 0f, 1f));
					startColor = Vector4.Lerp(startColor, offShoreColor, Mathf.Clamp(num5 / maxDepth, 0f, 1f));
				}
				else
				{
					startColor += (num4 - 0.5f) * 0.65f * startColor;
					startColor = (startColor - Half) * 0.94f + Half;
				}
				startColor *= 1.05f;
				output[i + oceanMargin, y + oceanMargin] = (transparent ? new Color(startColor.x, startColor.y, startColor.z, startColor.w) : new Color(startColor.x, startColor.y, startColor.z));
			}
		});
		background = output[0, 0];
		return EncodeToFile(imageWidth, imageHeight, array, lossy);
		float GetHeight(float x, float y)
		{
			return terrainHeightMap.GetHeight(x, y);
		}
		Vector3 GetNormal(float x, float y)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			return terrainHeightMap.GetNormal(x, y);
		}
		static float GetShoreDist(float x, float y)
		{
			return TerrainTexturing.Instance.GetMainlandCoarseVectorToShore(x, y).shoreDist;
		}
		float GetSplat(float x, float y, int mask)
		{
			return terrainSplatMap.GetSplat(x, y, mask);
		}
		int GetTopology(float x, float y)
		{
			return terrainTopologyMap.GetTopology(x, y, 16f);
		}
	}

	private static byte[] EncodeToFile(int width, int height, Color[] pixels, bool lossy)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		Texture2D val = null;
		try
		{
			val = new Texture2D(width, height, (TextureFormat)4, false);
			val.SetPixels(pixels);
			val.Apply();
			return lossy ? ImageConversion.EncodeToJPG(val, 85) : ImageConversion.EncodeToPNG(val);
		}
		finally
		{
			if ((Object)(object)val != (Object)null)
			{
				Object.Destroy((Object)(object)val);
			}
		}
	}

	private static Vector3 UnpackNormal(Vector4 value)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		value.x *= value.w;
		Vector3 val = new Vector3
		{
			x = value.x * 2f - 1f,
			y = value.y * 2f - 1f
		};
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(val.x, val.y);
		val.z = Mathf.Sqrt(1f - Mathf.Clamp(Vector2.Dot(val2, val2), 0f, 1f));
		return val;
	}
}
