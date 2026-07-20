using System;
using System.Threading.Tasks;
using TerrainHeightMapJobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Jobs;

public class TerrainHeightMap : TerrainMap<short>
{
	public struct RG16(ushort r, ushort g)
	{
		public ushort r = r;

		public ushort g = g;
	}

	public struct HeightMapQueryStructure
	{
		public ReadOnly<short> Data;

		public Vector3 TerrainPosition;

		public Vector3 TerrainOneOverSize;

		public int Res;

		public float NormY;

		public float HeightOffset;

		public float HeightScale;

		public readonly float GetHeightFromUV(Vector2 uv)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			float height = HeightMapData.GetHeight01(uv, Data, Res);
			return HeightOffset + height * HeightScale;
		}

		public readonly Vector3 GetNormalFromUV(Vector2 uv)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			return HeightMapData.GetNormal(uv, NormY, Data, Res);
		}
	}

	public Texture2D HeightTexture;

	public Texture2D NormalTexture;

	[Header("Collider Sampling")]
	[Min(1f)]
	public int ColliderSamplesPerAxis = 1;

	[Range(0f, 1f)]
	public float ColliderSampleSpread = 1f;

	public float normY;

	private bool _generatedHeightTexture;

	private bool _generatedNormalTexture;

	public NativeArray<short> deepSeaHeights;

	public ReadOnly<short> DeepSeaData => deepSeaHeights.AsReadOnly();

	public override void Setup()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Invalid comparison between Unknown and I4
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Invalid comparison between Unknown and I4
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		res = terrainData.heightmapResolution;
		InitArrays(res * res);
		deepSeaHeights = new NativeArray<short>(res * res, (Allocator)4, (NativeArrayOptions)1);
		ResetDeepSeaToFloor();
		normY = TerrainMeta.Size.x / TerrainMeta.Size.y / (float)res;
		if (!((Object)(object)HeightTexture != (Object)null))
		{
			return;
		}
		if (((Texture)HeightTexture).width == ((Texture)HeightTexture).height && ((Texture)HeightTexture).width == res)
		{
			if ((int)((Texture)HeightTexture).graphicsFormat != 22)
			{
				Color32[] pixels = HeightTexture.GetPixels32();
				int i = 0;
				int num = 0;
				for (; i < res; i++)
				{
					int num2 = 0;
					while (num2 < res)
					{
						Color32 val = pixels[num];
						dst[i * res + num2] = BitUtility.DecodeShort(val);
						num2++;
						num++;
					}
				}
			}
			else
			{
				NativeArray<RG16> pixelData = HeightTexture.GetPixelData<RG16>(0);
				int j = 0;
				int num3 = 0;
				for (; j < res; j++)
				{
					int num4 = 0;
					while (num4 < res)
					{
						RG16 rG = pixelData[num3];
						dst[j * res + num4] = BitUtility.Float2Short(BitUtility.UShort2Float(rG.r));
						num4++;
						num3++;
					}
				}
			}
			if ((int)((Texture)HeightTexture).graphicsFormat != 22)
			{
				ConvertHeightMapTexture();
			}
		}
		else
		{
			Debug.LogError((object)("Invalid height texture: " + ((Object)HeightTexture).name));
		}
	}

	public void SetupEmpty(int newRes)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		res = newRes;
		InitArrays(res * res);
		deepSeaHeights = new NativeArray<short>(res * res, (Allocator)4, (NativeArrayOptions)1);
		ResetDeepSeaToFloor();
		normY = TerrainMeta.Size.x / TerrainMeta.Size.y / (float)res;
	}

	public void SetupFrom(int newRes, ReadOnly<float> heights, ReadOnly<float> dsHeights)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		res = newRes;
		InitArrays(res * res);
		for (int i = 0; i < heights.Length; i++)
		{
			src[i] = BitUtility.Float2Short(heights[i]);
		}
		deepSeaHeights = new NativeArray<short>(res * res, (Allocator)4, (NativeArrayOptions)1);
		for (int j = 0; j < dsHeights.Length; j++)
		{
			deepSeaHeights[j] = BitUtility.Float2Short(dsHeights[j]);
		}
		normY = TerrainMeta.Size.x / TerrainMeta.Size.y / (float)res;
	}

	public override void Dispose()
	{
		base.Dispose();
		NativeArrayEx.SafeDispose(ref deepSeaHeights);
		if (_generatedHeightTexture && (Object)(object)HeightTexture != (Object)null)
		{
			Object.Destroy((Object)(object)HeightTexture);
			HeightTexture = null;
		}
		if (_generatedNormalTexture && (Object)(object)NormalTexture != (Object)null)
		{
			Object.Destroy((Object)(object)NormalTexture);
			NormalTexture = null;
		}
	}

	public void ResetDeepSeaToFloor()
	{
		short num = BitUtility.Float2Short(TerrainMeta.NormalizeY(DeepSeaManager.SeaFloorDepth));
		for (int i = 0; i < res * res; i++)
		{
			deepSeaHeights[i] = num;
		}
	}

	public void ApplyToTerrain()
	{
		float[,] heights = terrainData.GetHeights(0, 0, res, res);
		Parallel.For(0, res, delegate(int z)
		{
			for (int i = 0; i < res; i++)
			{
				heights[z, i] = GetHeight01(i, z);
			}
		});
		terrainData.SetHeights(0, 0, heights);
		TerrainCollider component = terrainRenderer.gameObject.GetComponent<TerrainCollider>();
		if (Object.op_Implicit((Object)(object)component))
		{
			((Collider)component).enabled = false;
			((Collider)component).enabled = true;
		}
	}

	public void ApplyToTerrainDelay()
	{
		float[,] heights = terrainData.GetHeights(0, 0, res, res);
		Parallel.For(0, res, delegate(int z)
		{
			for (int i = 0; i < res; i++)
			{
				heights[z, i] = GetHeight01(i, z);
			}
		});
		terrainData.SetHeightsDelayLOD(0, 0, heights);
		TerrainCollider component = terrainRenderer.gameObject.GetComponent<TerrainCollider>();
		terrainData.SyncHeightmap();
		if (Object.op_Implicit((Object)(object)component))
		{
			((Collider)component).enabled = false;
			((Collider)component).enabled = true;
		}
	}

	public void ConvertHeightMapTexture()
	{
		res = terrainData.heightmapResolution;
		GenerateTextures(heightTexture: true, normalTexture: false);
		HeightTexture.Apply(false, false);
		HeightTexture.Apply(false, true);
	}

	public bool TrySampleColliderHeight01(int layerMask, Vector3 terrainPos, Vector3 terrainSize, float normX, float normZ, out float height01, Collider filterCollider = null)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Max(1, ColliderSamplesPerAxis);
		float num2 = ((num > 1) ? (ColliderSampleSpread / (float)(res - 1) / (float)num) : 0f);
		float num3 = 0.5f * (float)(num - 1) * num2;
		bool flag = false;
		float num4 = 1f;
		RaycastHit val = default(RaycastHit);
		for (int i = 0; i < num; i++)
		{
			float num5 = (float)i * num2 - num3;
			for (int j = 0; j < num; j++)
			{
				float num6 = (float)j * num2 - num3;
				float num7 = Mathf.Clamp01(normX + num6);
				float num8 = Mathf.Clamp01(normZ + num5);
				if (Physics.Raycast(new Vector3(terrainPos.x + terrainSize.x * num7, terrainPos.y + terrainSize.y + 1f, terrainPos.z + terrainSize.z * num8), Vector3.down, ref val, terrainSize.y + 2f, layerMask) && (!((Object)(object)filterCollider != (Object)null) || !((Object)(object)((RaycastHit)(ref val)).collider != (Object)(object)filterCollider)))
				{
					float num9 = Mathf.Clamp01((((RaycastHit)(ref val)).point.y - terrainPos.y) / terrainSize.y);
					if (!flag || num9 < num4)
					{
						num4 = num9;
					}
					flag = true;
				}
			}
		}
		height01 = num4;
		return flag;
	}

	public void GenerateTextures(bool heightTexture = true, bool normalTexture = true, bool useRGBA32 = false)
	{
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Expected O, but got Unknown
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (heightTexture)
		{
			if (useRGBA32)
			{
				HeightTexture = new Texture2D(res, res, (TextureFormat)4, false, true);
				((Object)HeightTexture).name = "HeightTexture";
				((Texture)HeightTexture).wrapMode = (TextureWrapMode)1;
				NativeArray<Color32> heights = HeightTexture.GetPixelData<Color32>(0);
				Parallel.For(0, res, delegate(int z)
				{
					//IL_0038: Unknown result type (might be due to invalid IL or missing references)
					for (int i = 0; i < res; i++)
					{
						heights[z * res + i] = BitUtility.EncodeShort(src[z * res + i]);
					}
				});
				HeightTexture.ignoreMipmapLimit = true;
				_generatedHeightTexture = Application.isPlaying;
			}
			else
			{
				HeightTexture = new Texture2D(res, res, (GraphicsFormat)22, 0, (TextureCreationFlags)0);
				((Object)HeightTexture).name = "HeightTexture";
				((Texture)HeightTexture).wrapMode = (TextureWrapMode)1;
				NativeArray<RG16> heights2 = HeightTexture.GetPixelData<RG16>(0);
				float[,] terrainHeights = terrainData.GetHeights(0, 0, res, res);
				Parallel.For(0, res, delegate(int z)
				{
					for (int i = 0; i < res; i++)
					{
						heights2[z * res + i] = new RG16(BitUtility.Float2UShort(BitUtility.Short2Float((int)src[z * res + i])), (ushort)(terrainHeights[z, i] * 65535f + 0.5f));
					}
				});
				HeightTexture.ignoreMipmapLimit = true;
				_generatedHeightTexture = Application.isPlaying;
			}
		}
		if (!normalTexture)
		{
			return;
		}
		int normalres = (res - 1) / 2;
		NormalTexture = new Texture2D(normalres, normalres, (TextureFormat)4, false, true);
		((Object)NormalTexture).name = "NormalTexture";
		((Texture)NormalTexture).wrapMode = (TextureWrapMode)1;
		NativeArray<Color32> normals = NormalTexture.GetPixelData<Color32>(0);
		Parallel.For(0, normalres, delegate(int z)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			float normZ = ((float)z + 0.5f) / (float)normalres;
			for (int i = 0; i < normalres; i++)
			{
				float normX = ((float)i + 0.5f) / (float)normalres;
				Vector3 normal = GetNormal(normX, normZ);
				float num = Vector3.Angle(Vector3.up, normal);
				float num2 = Mathf.InverseLerp(50f, 70f, num);
				normal = Vector3.Slerp(normal, Vector3.up, num2);
				normals[z * normalres + i] = Color32.op_Implicit(BitUtility.EncodeNormal(normal));
			}
		});
		_generatedNormalTexture = Application.isPlaying;
	}

	public void ApplyTextures()
	{
		HeightTexture.Apply(false, false);
		NormalTexture.Apply(true, false);
		NormalTexture.Compress(false);
		HeightTexture.Apply(false, true);
		NormalTexture.Apply(false, true);
	}

	public float GetHeight(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (DeepSeaManager.IsInsideDeepSea(worldPos))
		{
			float normX = DeepSeaManager.NormalizeX(worldPos.x);
			float normZ = DeepSeaManager.NormalizeZ(worldPos.z);
			return GetHeight(normX, normZ, deepSeaHeights.AsReadOnly());
		}
		float normX2 = TerrainMeta.NormalizeX(worldPos.x);
		float normZ2 = TerrainMeta.NormalizeZ(worldPos.z);
		return GetHeight(normX2, normZ2, src.AsReadOnly());
	}

	public JobHandle GetHeights(ReadOnly<Vector3> worldPos, NativeArray<float> results, JobHandle inputDeps = default(JobHandle))
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		GetHeightsJob jobData = new GetHeightsJob
		{
			Heights = results,
			Pos = worldPos,
			HeightMapData = new HeightMapData
			{
				Data = src.AsReadOnly(),
				DeepSeaData = deepSeaHeights.AsReadOnly(),
				DeepSeaBounds = DeepSeaManager.DeepSeaBounds,
				Res = res,
				TerrainPos = TerrainMeta.Position,
				TerrainScale = TerrainMeta.Size.y,
				TerrainOneOverSize = Vector3Ex.XZ2D(TerrainMeta.OneOverSize),
				NormY = normY
			}
		};
		return ParallelJobEx.ScheduleParallel<GetHeightsJob>(ref jobData, worldPos.Length, inputDeps);
	}

	public void GetHeightsIndirect(ReadOnly<Vector3> worldPos, ReadOnly<int> indices, NativeArray<float> results)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		GetHeightsJobIndirect getHeightsJobIndirect = new GetHeightsJobIndirect
		{
			Heights = results,
			Pos = worldPos,
			Indices = indices,
			HeightMapData = new HeightMapData
			{
				Data = src.AsReadOnly(),
				DeepSeaData = deepSeaHeights.AsReadOnly(),
				DeepSeaBounds = DeepSeaManager.DeepSeaBounds,
				Res = res,
				TerrainPos = TerrainMeta.Position,
				TerrainScale = TerrainMeta.Size.y,
				TerrainOneOverSize = Vector3Ex.XZ2D(TerrainMeta.OneOverSize),
				NormY = normY
			}
		};
		IJobExtensions.RunByRef<GetHeightsJobIndirect>(ref getHeightsJobIndirect);
	}

	public float GetHeight(float normX, float normZ)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return GetHeight(normX, normZ, src.AsReadOnly());
	}

	public float GetHeight(float normX, float normZ, ReadOnly<short> data)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return TerrainMeta.Position.y + GetHeight01(normX, normZ, data) * TerrainMeta.Size.y;
	}

	public float GetHeight(Vector2 uv)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return TerrainMeta.Position.y + GetHeight01(uv, src.AsReadOnly()) * TerrainMeta.Size.y;
	}

	public float GetHeight(Vector2 uv, ReadOnly<short> data)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return TerrainMeta.Position.y + GetHeight01(uv, data) * TerrainMeta.Size.y;
	}

	public HeightMapQueryStructure GetQueryStructure(bool isForDeepSea)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		ReadOnly<short> data = (isForDeepSea ? deepSeaHeights.AsReadOnly() : src.AsReadOnly());
		Bounds deepSeaBounds = DeepSeaManager.DeepSeaBounds;
		Vector3 min = ((Bounds)(ref deepSeaBounds)).min;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(1f / ((Bounds)(ref deepSeaBounds)).size.x, 1f / ((Bounds)(ref deepSeaBounds)).size.z);
		Vector3 terrainPosition = (isForDeepSea ? min : TerrainMeta.Position);
		Vector2 val2 = (isForDeepSea ? val : Vector3Ex.XZ2D(TerrainMeta.OneOverSize));
		return new HeightMapQueryStructure
		{
			Data = data,
			TerrainPosition = terrainPosition,
			TerrainOneOverSize = Vector2.op_Implicit(val2),
			Res = res,
			NormY = normY,
			HeightOffset = TerrainMeta.Position.y,
			HeightScale = TerrainMeta.Size.y
		};
	}

	public float GetHeight(int x, int z)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return TerrainMeta.Position.y + GetHeight01(x, z) * TerrainMeta.Size.y;
	}

	public float GetHeight(int x, int z, ReadOnly<short> data)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return TerrainMeta.Position.y + GetHeight01(x, z, data) * TerrainMeta.Size.y;
	}

	public void GetHeightsIndirect(ReadOnly<Vector2> uvs, ReadOnly<int> indices, NativeArray<float> results)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		GetHeightsIndirect(uvs, src.AsReadOnly(), indices, results);
	}

	public void GetHeightsIndirect(ReadOnly<Vector2> uvs, ReadOnly<short> data, ReadOnly<int> indices, NativeArray<float> results)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		GetHeightsByUVJobIndirect getHeightsByUVJobIndirect = new GetHeightsByUVJobIndirect
		{
			Heights = results,
			UVs = uvs,
			Indices = indices,
			HeightMapData = new HeightMapData
			{
				Data = src.AsReadOnly(),
				DeepSeaData = deepSeaHeights.AsReadOnly(),
				DeepSeaBounds = DeepSeaManager.DeepSeaBounds,
				Res = res,
				TerrainPos = TerrainMeta.Position,
				TerrainScale = TerrainMeta.Size.y,
				TerrainOneOverSize = Vector3Ex.XZ2D(TerrainMeta.OneOverSize),
				NormY = normY
			},
			Data = data
		};
		IJobExtensions.RunByRef<GetHeightsByUVJobIndirect>(ref getHeightsByUVJobIndirect);
	}

	public float GetHeight01(float normX, float normZ)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return GetHeight01(normX, normZ, src.AsReadOnly());
	}

	public float GetHeight01(float normX, float normZ, ReadOnly<short> data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return HeightMapData.GetHeight01(new Vector2(normX, normZ), data, res);
	}

	public float GetHeight01(Vector2 uv, ReadOnly<short> data)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return HeightMapData.GetHeight01(uv, data, res);
	}

	public float GetHeight01(int x, int z)
	{
		return BitUtility.Short2Float((int)src[z * res + x]);
	}

	public float GetHeight01(int x, int z, ReadOnly<short> data)
	{
		return BitUtility.Short2Float((int)data[z * res + x]);
	}

	private float GetSrcHeight01(int x, int z)
	{
		return BitUtility.Short2Float((int)src[z * res + x]);
	}

	private float GetDstHeight01(int x, int z)
	{
		return BitUtility.Short2Float((int)dst[z * res + x]);
	}

	public Vector3 GetNormal(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (DeepSeaManager.IsInsideDeepSea(worldPos))
		{
			float normX = DeepSeaManager.NormalizeX(worldPos.x);
			float normZ = DeepSeaManager.NormalizeZ(worldPos.z);
			return GetNormal(normX, normZ, deepSeaHeights.AsReadOnly());
		}
		float normX2 = TerrainMeta.NormalizeX(worldPos.x);
		float normZ2 = TerrainMeta.NormalizeZ(worldPos.z);
		return GetNormal(normX2, normZ2, src.AsReadOnly());
	}

	public JobHandle GetNormalsIndirect(ReadOnly<Vector3> worldPos, NativeArray<Vector3> results, ReadOnly<int> indices, JobHandle dependsOn = default(JobHandle))
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		GetNormalsJobIndirect getNormalsJobIndirect = new GetNormalsJobIndirect
		{
			Normals = results,
			Pos = worldPos,
			Indices = indices,
			HeightMapData = new HeightMapData
			{
				Data = src.AsReadOnly(),
				DeepSeaData = deepSeaHeights.AsReadOnly(),
				DeepSeaBounds = DeepSeaManager.DeepSeaBounds,
				Res = res,
				TerrainPos = TerrainMeta.Position,
				TerrainScale = TerrainMeta.Size.y,
				TerrainOneOverSize = Vector3Ex.XZ2D(TerrainMeta.OneOverSize),
				NormY = normY
			}
		};
		return IJobExtensions.ScheduleByRef<GetNormalsJobIndirect>(ref getNormalsJobIndirect, dependsOn);
	}

	public JobHandle GetNormalsIndirect(ReadOnly<Vector3> worldPos, NativeArray<Vector3> results, NativeArray<int> deferredIndices, JobHandle dependsOn = default(JobHandle))
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		GetNormalsJobIndirectDeferred getNormalsJobIndirectDeferred = new GetNormalsJobIndirectDeferred
		{
			Normals = results,
			Pos = worldPos,
			Indices = deferredIndices,
			HeightMapData = new HeightMapData
			{
				Data = src.AsReadOnly(),
				DeepSeaData = deepSeaHeights.AsReadOnly(),
				DeepSeaBounds = DeepSeaManager.DeepSeaBounds,
				Res = res,
				TerrainPos = TerrainMeta.Position,
				TerrainScale = TerrainMeta.Size.y,
				TerrainOneOverSize = Vector3Ex.XZ2D(TerrainMeta.OneOverSize),
				NormY = normY
			}
		};
		return IJobExtensions.ScheduleByRef<GetNormalsJobIndirectDeferred>(ref getNormalsJobIndirectDeferred, dependsOn);
	}

	public Vector3 GetNormal(float normX, float normZ)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return GetNormal(normX, normZ, src.AsReadOnly());
	}

	public Vector3 GetNormal(float normX, float normZ, ReadOnly<short> data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return HeightMapData.GetNormal(new Vector2(normX, normZ), normY, data, res);
	}

	public Vector3 GetNormal(int x, int z)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return GetNormal(x, z, src.AsReadOnly());
	}

	public Vector3 GetNormal(int x, int z, ReadOnly<short> data)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		return HeightMapData.GetNormal(x, z, normY, data, res);
	}

	public float GetSlope(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Angle(Vector3.up, GetNormal(worldPos));
	}

	public float GetSlope(float normX, float normZ)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Angle(Vector3.up, GetNormal(normX, normZ));
	}

	public float GetSlope(int x, int z)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Angle(Vector3.up, GetNormal(x, z));
	}

	public float GetSlope01(Vector3 worldPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GetSlope(worldPos) * (1f / 90f);
	}

	public float GetSlope01(float normX, float normZ)
	{
		return GetSlope(normX, normZ) * (1f / 90f);
	}

	public float GetSlope01(int x, int z)
	{
		return GetSlope(x, z) * (1f / 90f);
	}

	public void SetHeight(Vector3 worldPos, float height)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		SetHeight(normX, normZ, height);
	}

	public void SetHeight(float normX, float normZ, float height)
	{
		int x = Index(normX);
		int z = Index(normZ);
		SetHeight(x, z, height);
	}

	public void SetHeight(int x, int z, float height)
	{
		dst[z * res + x] = BitUtility.Float2Short(height);
	}

	public void SetHeight(Vector3 worldPos, float height, float opacity)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		SetHeight(normX, normZ, height, opacity);
	}

	public void SetHeight(float normX, float normZ, float height, float opacity)
	{
		int x = Index(normX);
		int z = Index(normZ);
		SetHeight(x, z, height, opacity);
	}

	public void SetHeight(int x, int z, float height, float opacity)
	{
		float height2 = Mathf.SmoothStep(GetSrcHeight01(x, z), height, opacity);
		SetHeight(x, z, height2);
	}

	public void AddHeight(Vector3 worldPos, float delta)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		AddHeight(normX, normZ, delta);
	}

	public void AddHeight(float normX, float normZ, float delta)
	{
		int x = Index(normX);
		int z = Index(normZ);
		AddHeight(x, z, delta);
	}

	public void AddHeight(int x, int z, float delta)
	{
		float height = Mathf.Clamp01(GetDstHeight01(x, z) + delta);
		SetHeight(x, z, height);
	}

	public float GetAddHeight(int x, int z, float delta)
	{
		float num = Mathf.Clamp01(GetDstHeight01(x, z) + delta);
		SetHeight(x, z, num);
		return num;
	}

	public void LowerHeight(Vector3 worldPos, float height, float opacity)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		LowerHeight(normX, normZ, height, opacity);
	}

	public void LowerHeight(float normX, float normZ, float height, float opacity)
	{
		int x = Index(normX);
		int z = Index(normZ);
		LowerHeight(x, z, height, opacity);
	}

	public void LowerHeight(int x, int z, float height, float opacity)
	{
		float height2 = Mathf.Min(GetDstHeight01(x, z), Mathf.SmoothStep(GetSrcHeight01(x, z), height, opacity));
		SetHeight(x, z, height2);
	}

	public void RaiseHeight(Vector3 worldPos, float height, float opacity)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		RaiseHeight(normX, normZ, height, opacity);
	}

	public void RaiseHeight(float normX, float normZ, float height, float opacity)
	{
		int x = Index(normX);
		int z = Index(normZ);
		RaiseHeight(x, z, height, opacity);
	}

	public void RaiseHeight(int x, int z, float height, float opacity)
	{
		float height2 = Mathf.Max(GetDstHeight01(x, z), Mathf.SmoothStep(GetSrcHeight01(x, z), height, opacity));
		SetHeight(x, z, height2);
	}

	public void SetHeight(Vector3 worldPos, float opacity, float radius, float fade = 0f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		float height = TerrainMeta.NormalizeY(worldPos.y);
		SetHeight(normX, normZ, height, opacity, radius, fade);
	}

	public void SetHeight(float normX, float normZ, float height, float opacity, float radius, float fade = 0f)
	{
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if (lerp > 0f)
			{
				SetHeight(x, z, height, lerp * opacity);
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}

	public void LowerHeight(Vector3 worldPos, float opacity, float radius, float fade = 0f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		float height = TerrainMeta.NormalizeY(worldPos.y);
		LowerHeight(normX, normZ, height, opacity, radius, fade);
	}

	public void LowerHeight(float normX, float normZ, float height, float opacity, float radius, float fade = 0f)
	{
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if (lerp > 0f)
			{
				LowerHeight(x, z, height, lerp * opacity);
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}

	public void RaiseHeight(Vector3 worldPos, float opacity, float radius, float fade = 0f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		float height = TerrainMeta.NormalizeY(worldPos.y);
		RaiseHeight(normX, normZ, height, opacity, radius, fade);
	}

	public void RaiseHeight(float normX, float normZ, float height, float opacity, float radius, float fade = 0f)
	{
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if (lerp > 0f)
			{
				RaiseHeight(x, z, height, lerp * opacity);
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}

	public void AddHeight(Vector3 worldPos, float delta, float radius, float fade = 0f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		AddHeight(normX, normZ, delta, radius, fade);
	}

	public void AddHeight(float normX, float normZ, float delta, float radius, float fade = 0f)
	{
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if (lerp > 0f)
			{
				AddHeight(x, z, lerp * delta);
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}

	public void AddHeightArea(float[,] subHeights, int subDimensions, Vector3 worldPos, float delta, float radius, float fade = 0f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		ApplyFilterSubHeights(subHeights, subDimensions, normX, normZ, radius, fade, delta);
	}

	public void ApplyFilterSubHeights(float[,] subHeights, int subDimensions, float normX, float normZ, float radius, float fade, float delta)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		float num = TerrainMeta.OneOverSize.x * radius;
		float num2 = TerrainMeta.OneOverSize.x * fade;
		float num3 = (float)res * (num - num2);
		float num4 = (float)res * num;
		float num5 = normX * (float)res;
		float num6 = normZ * (float)res;
		int num7 = Index(normX - num);
		int num8 = Index(normX + num);
		int num9 = Index(normZ - num);
		int num10 = Index(normZ + num);
		Debug.Assert(num8 - num7 <= subDimensions);
		Debug.Assert(num10 - num9 <= subDimensions);
		int num11 = (num8 - num7) / 2 + num7;
		int num12 = (num10 - num9) / 2 + num9;
		int num13 = subDimensions / 2;
		for (int i = 0; i < subDimensions; i++)
		{
			for (int j = 0; j < subDimensions; j++)
			{
				subHeights[i, j] = GetHeight01(i + num7, j + num9);
				MonoBehaviour.print((object)$"Sub Height {i},{j} = {subHeights[i, j]}");
			}
		}
		Vector2 val;
		if (num3 != num4)
		{
			for (int k = num9; k <= num10; k++)
			{
				for (int l = num7; l <= num8; l++)
				{
					val = new Vector2((float)l + 0.5f - num5, (float)k + 0.5f - num6);
					float magnitude = ((Vector2)(ref val)).magnitude;
					float num14 = Mathf.InverseLerp(num4, num3, magnitude);
					if (num14 > 0f)
					{
						subHeights[l - num11 + num13, k - num12 + num13] = GetAddHeight(l, k, num14 * delta);
					}
					else
					{
						subHeights[l - num11 + num13, k - num12 + num13] = GetHeight01(l, k);
					}
				}
			}
			return;
		}
		for (int m = num9; m <= num10; m++)
		{
			for (int n = num7; n <= num8; n++)
			{
				val = new Vector2((float)n + 0.5f - num5, (float)m + 0.5f - num6);
				float num15 = ((((Vector2)(ref val)).magnitude < num4) ? 1 : 0);
				if (num15 > 0f)
				{
					subHeights[n - num11 + num13, m - num12 + num13] = GetAddHeight(n, m, num15 * delta);
				}
				else
				{
					subHeights[n - num11 + num13, m - num12 + num13] = GetHeight(n, m);
				}
			}
		}
	}
}
