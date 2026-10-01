using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rust;
using TerrainTexturingJobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UtilityJobs;

[ExecuteInEditMode]
public class TerrainTexturing : TerrainExtension
{
	public struct ShoreData : IDisposable
	{
		public int ShoreMapSize;

		public float ShoreDistanceScale;

		public Vector3 Position;

		public Vector3 Size;

		public Vector3 OneOverSize;

		[ReadOnly]
		public NativeArray<float> ShoreDistances;

		[ReadOnly]
		public NativeArray<Vector4> ShoreVectors;

		public Vector4 DefaultVector;

		public float DefaultDistance;

		public int Len => ShoreMapSize * ShoreMapSize;

		public void FillWithDefault()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			IJobExtensions.Run<FillJob<float>>(new FillJob<float>
			{
				Value = DefaultDistance,
				Values = ShoreDistances
			});
			IJobExtensions.Run<FillJob<Vector4>>(new FillJob<Vector4>
			{
				Value = DefaultVector,
				Values = ShoreVectors
			});
		}

		[BurstDiscard]
		public Texture2D CreateTexture(string name)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Expected Obj, but got Unknown
			Texture2D val = new Texture2D(ShoreMapSize, ShoreMapSize, (TextureFormat)17, false, true, true)
			{
				name = name,
				filterMode = (FilterMode)1,
				wrapMode = (TextureWrapMode)1
			};
			NativeArray<half4> val2 = new NativeArray<half4>(Len, (Allocator)3, (NativeArrayOptions)1);
			PopulateTextureDataJob jobData = new PopulateTextureDataJob
			{
				colors = val2,
				vectors = ShoreVectors.AsReadOnly(),
				distances = ShoreDistances.AsReadOnly()
			};
			int length = val2.Length;
			JobHandle val3 = default;
			val3 = ParallelJobEx.ScheduleParallel<PopulateTextureDataJob>(ref jobData, length, val3);
			val3.Complete();
			val.SetPixelData<half4>(val2, 0, 0);
			val.Apply(false, true);
			val3 = default;
			val2.Dispose(val3);
			return val;
		}

		public float GetCoarseDistanceToShore(Vector3 pos)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			Vector2 uv = default;
			uv.x = (pos.x - Position.x) * OneOverSize.x;
			uv.y = (pos.z - Position.z) * OneOverSize.z;
			return GetCoarseDistanceToShore(uv);
		}

		public (Vector3 shoreDir, float shoreDist) GetCoarseVectorToShore(Vector3 pos)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			Vector2 uv = default;
			uv.x = (pos.x - Position.x) * OneOverSize.x;
			uv.y = (pos.z - Position.z) * OneOverSize.z;
			return GetCoarseVectorToShore(uv);
		}

		public (Vector3 shoreDir, float shoreDist) GetCoarseVectorToShore(Vector2 uv)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			int shoreMapSize = ShoreMapSize;
			int num = shoreMapSize - 1;
			float num2 = uv.x * (float)num;
			float num3 = uv.y * (float)num;
			int num4 = (int)num2;
			int num5 = (int)num3;
			float num6 = num2 - (float)num4;
			float num7 = num3 - (float)num5;
			num4 = ((num4 >= 0) ? num4 : 0);
			num5 = ((num5 >= 0) ? num5 : 0);
			num4 = ((num4 <= num) ? num4 : num);
			num5 = ((num5 <= num) ? num5 : num);
			int num8 = ((num2 < (float)num) ? 1 : 0);
			int num9 = ((num3 < (float)num) ? shoreMapSize : 0);
			int num10 = num5 * shoreMapSize + num4;
			int num11 = num10 + num8;
			int num12 = num10 + num9;
			int num13 = num12 + num8;
			Vector3 val = Vector4.op_Implicit(ShoreVectors[num10]);
			Vector3 val2 = Vector4.op_Implicit(ShoreVectors[num11]);
			Vector3 val3 = Vector4.op_Implicit(ShoreVectors[num12]);
			Vector3 val4 = Vector4.op_Implicit(ShoreVectors[num13]);
			Vector3 val5 = default;
			val5.x = (val2.x - val.x) * num6 + val.x;
			val5.y = (val2.y - val.y) * num6 + val.y;
			val5.z = (val2.z - val.z) * num6 + val.z;
			Vector3 val6 = default;
			val6.x = (val4.x - val3.x) * num6 + val3.x;
			val6.y = (val4.y - val3.y) * num6 + val3.y;
			val6.z = (val4.z - val3.z) * num6 + val3.z;
			float num14 = (val6.x - val5.x) * num7 + val5.x;
			float num15 = (val6.y - val5.y) * num7 + val5.y;
			return new ValueTuple<Vector3, float>(item2: ((val6.z - val5.z) * num7 + val5.z) * ShoreDistanceScale, item1: new Vector3(num14, 0f, num15));
		}

		public (Vector3 shoreDir, float shoreDist) GetCoarseVectorToShore(float normX, float normY)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return GetCoarseVectorToShore(new Vector2(normX, normY));
		}

		public Vector4 GetRawShoreVector(Vector3 pos)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			Vector2 uv = default;
			uv.x = (pos.x - Position.x) * OneOverSize.x;
			uv.y = (pos.z - Position.z) * OneOverSize.z;
			return GetRawShoreVector(uv);
		}

		public Vector4 GetRawShoreVector(Vector2 uv)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			int shoreMapSize = ShoreMapSize;
			int num = shoreMapSize - 1;
			float num2 = uv.x * (float)num;
			float num3 = uv.y * (float)num;
			int num4 = (int)num2;
			int num5 = (int)num3;
			num4 = ((num4 >= 0) ? num4 : 0);
			num5 = ((num5 >= 0) ? num5 : 0);
			num4 = ((num4 <= num) ? num4 : num);
			num5 = ((num5 <= num) ? num5 : num);
			return ShoreVectors[num5 * shoreMapSize + num4];
		}

		public readonly float GetCoarseDistanceToShore(Vector2 uv)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			int shoreMapSize = ShoreMapSize;
			int num = shoreMapSize - 1;
			float num2 = uv.x * (float)num;
			float num3 = uv.y * (float)num;
			int num4 = (int)num2;
			int num5 = (int)num3;
			float num6 = num2 - (float)num4;
			float num7 = num3 - (float)num5;
			num4 = ((num4 >= 0) ? num4 : 0);
			num5 = ((num5 >= 0) ? num5 : 0);
			num4 = ((num4 <= num) ? num4 : num);
			num5 = ((num5 <= num) ? num5 : num);
			int num8 = ((num2 < (float)num) ? 1 : 0);
			int num9 = ((num3 < (float)num) ? shoreMapSize : 0);
			int num10 = num5 * shoreMapSize + num4;
			int num11 = num10 + num8;
			int num12 = num10 + num9;
			int num13 = num12 + num8;
			float num14 = ShoreDistances[num10];
			float num15 = ShoreDistances[num11];
			float num16 = ShoreDistances[num12];
			float num17 = ShoreDistances[num13];
			float num18 = (num15 - num14) * num6 + num14;
			return (((num17 - num16) * num6 + num16 - num18) * num7 + num18) * ShoreDistanceScale;
		}

		public void Dispose()
		{
			NativeArrayEx.SafeDispose(ref ShoreDistances);
			ShoreVectors.SafeDispose<Vector4>();
		}
	}

	public struct ShoreVectorQueryStructure
	{
		private ShoreData mainlandData;

		private ShoreData deepSeaData;

		private Bounds deepSeaBounds;

		internal ShoreVectorQueryStructure(ShoreData mainlandData, ShoreData deepSeaData, Bounds deepSeaBounds)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			this.mainlandData = mainlandData;
			this.deepSeaData = deepSeaData;
			this.deepSeaBounds = deepSeaBounds;
		}

		public float GetCoarseDistanceToShore(Vector3 pos)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			return (deepSeaBounds.Contains(pos) ? deepSeaData : mainlandData).GetCoarseDistanceToShore(pos);
		}
	}

	public const int ShoreVectorDownscale = 1;

	private const int ShoreVectorBlurPasses = 1;

	private float terrainSize;

	private float deepSeaSize;

	private ShoreData MainlandShoreData;

	private ShoreData DeepSeaShoreData;

	private bool deepSeaShoreDataDirty;

	private List<(BakedShoreVectors data, Transform t)> deepSeaPostGenApplication;

	public bool debugFoliageDisplacement;

	private bool initialized;

	private static TerrainTexturing instance;

	private int afCached;

	private int globalTextureMipmapLimitCached;

	private int anisotropicFilteringCached;

	private bool streamingMipmapsActiveCached;

	private bool billboardsFaceCameraPositionCached;

	public static TerrainTexturing Instance => instance;

	public bool TexturesInitialized => initialized;

	private void ReleaseBasePyramid()
	{
	}

	private void UpdateBasePyramid()
	{
	}

	private void InitializeCoarseHeightSlope()
	{
	}

	private void ReleaseCoarseHeightSlope()
	{
	}

	private void UpdateCoarseHeightSlope()
	{
	}

	internal ShoreData GetMap(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!DeepSeaManager.IsInsideDeepSea(position))
		{
			return MainlandShoreData;
		}
		return DeepSeaShoreData;
	}

	internal ShoreData GetMap(bool isDeepSea)
	{
		if (!isDeepSea)
		{
			return MainlandShoreData;
		}
		return DeepSeaShoreData;
	}

	internal ref ShoreData GetMapByRef(bool isDeepSea)
	{
		if (!isDeepSea)
		{
			return ref MainlandShoreData;
		}
		return ref DeepSeaShoreData;
	}

	private void InitializeShoreVector()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.ClosestPowerOfTwo(terrainData.heightmapResolution) >> 1;
		terrainSize = Mathf.Max(terrainData.size.x, terrainData.size.z);
		deepSeaSize = Vector3Ex.Max(Vector3Ex.XZ2D(DeepSeaManager.DeepSeaBounds.size));
		MainlandShoreData = new ShoreData
		{
			ShoreMapSize = num,
			ShoreDistanceScale = terrainSize / (float)num,
			ShoreDistances = new NativeArray<float>(num * num, (Allocator)4, (NativeArrayOptions)0),
			ShoreVectors = new NativeArray<Vector4>(num * num, (Allocator)4, (NativeArrayOptions)0),
			Position = TerrainMeta.Position,
			Size = TerrainMeta.Size,
			OneOverSize = TerrainMeta.OneOverSize,
			DefaultDistance = 10000f,
			DefaultVector = new Vector4(1f, 1f, 1f, 0f)
		};
		MainlandShoreData.FillWithDefault();
		int num2 = MainlandShoreData.ShoreMapSize >> 1;
		DeepSeaShoreData = new ShoreData
		{
			ShoreMapSize = num2,
			ShoreDistanceScale = deepSeaSize / (float)num2,
			ShoreDistances = new NativeArray<float>(num2 * num2, (Allocator)4, (NativeArrayOptions)0),
			ShoreVectors = new NativeArray<Vector4>(num2 * num2, (Allocator)4, (NativeArrayOptions)0),
			Position = DeepSeaManager.DeepSeaBounds.min,
			Size = DeepSeaManager.DeepSeaBounds.size,
			OneOverSize = Vector3Ex.Inverse(DeepSeaManager.DeepSeaBounds.size),
			DefaultDistance = 10000f,
			DefaultVector = new Vector4(1f, 1f, 1f, 1f)
		};
		DeepSeaShoreData.FillWithDefault();
		deepSeaShoreDataDirty = true;
		deepSeaPostGenApplication = new List<(BakedShoreVectors, Transform)>();
	}

	private void GenerateShoreVector()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GenerateShoreVector", 500))
		{
			GenerateShoreVector(out var distances, out var vectors);
			MainlandShoreData.ShoreDistances = distances;
			MainlandShoreData.ShoreVectors = vectors;
			if (!DeepSeaShoreData.ShoreDistances.IsCreated)
			{
				DeepSeaShoreData.ShoreDistances = new NativeArray<float>(DeepSeaShoreData.Len, (Allocator)4, (NativeArrayOptions)1);
			}
			if (!DeepSeaShoreData.ShoreVectors.IsCreated)
			{
				DeepSeaShoreData.ShoreVectors = new NativeArray<Vector4>(DeepSeaShoreData.Len, (Allocator)4, (NativeArrayOptions)1);
			}
			DeepSeaShoreData.FillWithDefault();
		}
	}

	private void UpdateDeepSeaShoreVectorTexture()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("UpdateDeepSeaShoreVectorTexture"))
		{
			if (!deepSeaShoreDataDirty)
			{
				return;
			}
			deepSeaShoreDataDirty = false;
			GenerateShoreVector(out var distances, out var vectors, genDeepSea: true);
			NativeArrayEx.SafeDispose(ref DeepSeaShoreData.ShoreDistances);
			DeepSeaShoreData.ShoreDistances = distances;
			DeepSeaShoreData.ShoreVectors.SafeDispose<Vector4>();
			DeepSeaShoreData.ShoreVectors = vectors;
			Bounds deepSeaBounds = DeepSeaManager.DeepSeaBounds;
			Vector3 min = deepSeaBounds.min;
			Vector3 val = Vector3Ex.Inverse(deepSeaBounds.size);
			NativeArray<float> deepSeaShoreDistances = DeepSeaShoreData.ShoreDistances;
			NativeArray<Vector4> deepSeaShoreVectors = DeepSeaShoreData.ShoreVectors;
			Vector3 val2 = default;
			Quaternion val3 = default;
			foreach (var item3 in deepSeaPostGenApplication)
			{
				BakedShoreVectors item = item3.data;
				Transform item2 = item3.t;
				if (!item || !Object.op_Implicit((Object)(object)item2))
				{
					continue;
				}
				ShoreVectorData shoreVectorData = item.ShoreVectorData;
				item2.GetPositionAndRotation(ref val2, ref val3);
				float y = val3.eulerAngles.y;
				float normX = (val2.x - min.x) * val.x;
				float normZ = (val2.z - min.z) * val.z;
				float worldSize = shoreVectorData.WorldSize;
				int shoreMapSize = DeepSeaShoreData.ShoreMapSize;
				float[] srcDistances = shoreVectorData.Distances;
				Vector4[] srcVectors = shoreVectorData.Vectors;
				Quaternion quat = Quaternion.Euler(0f, y, 0f);
				BlitBakedData(worldSize, shoreVectorData.ShoreVectorDimension, deepSeaBounds, shoreMapSize, normX, normZ, y, (int si, int di) =>
				{
					//IL_003d: Unknown result type (might be due to invalid IL or missing references)
					//IL_0042: Unknown result type (might be due to invalid IL or missing references)
					//IL_0045: Unknown result type (might be due to invalid IL or missing references)
					//IL_0050: Unknown result type (might be due to invalid IL or missing references)
					//IL_0056: Unknown result type (might be due to invalid IL or missing references)
					//IL_005c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0061: Unknown result type (might be due to invalid IL or missing references)
					//IL_0062: Unknown result type (might be due to invalid IL or missing references)
					//IL_0067: Unknown result type (might be due to invalid IL or missing references)
					//IL_006a: Unknown result type (might be due to invalid IL or missing references)
					//IL_0070: Unknown result type (might be due to invalid IL or missing references)
					//IL_0076: Unknown result type (might be due to invalid IL or missing references)
					//IL_007c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0082: Unknown result type (might be due to invalid IL or missing references)
					//IL_0093: Unknown result type (might be due to invalid IL or missing references)
					float num = srcDistances[si];
					float num2 = deepSeaShoreDistances[di];
					if (num < num2)
					{
						deepSeaShoreDistances[di] = srcDistances[si];
						Vector4 val4 = srcVectors[si];
						Vector3 val5 = new Vector3(val4.x, 0f, val4.y);
						val5 = quat * val5;
						val4 = new Vector4(val5.x, val5.z, val4.z, val4.w);
						deepSeaShoreVectors[di] = val4;
					}
				});
			}
		}
	}

	private void OnDestroy()
	{
		ReleaseShoreVector();
	}

	private void ReleaseShoreVector()
	{
		MainlandShoreData.Dispose();
		DeepSeaShoreData.Dispose();
	}

	public void GenerateShoreVector(out NativeArray<float> distances, out NativeArray<Vector4> vectors, bool genDeepSea = false)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0433: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_044f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_0469: Unknown result type (might be due to invalid IL or missing references)
		//IL_0471: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GenerateShoreVector"))
		{
			int size;
			float shoreDistanceScale;
			Vector3 position;
			if (genDeepSea)
			{
				size = DeepSeaShoreData.ShoreMapSize;
				shoreDistanceScale = DeepSeaShoreData.ShoreDistanceScale;
				position = DeepSeaShoreData.Position;
			}
			else
			{
				size = MainlandShoreData.ShoreMapSize;
				shoreDistanceScale = MainlandShoreData.ShoreDistanceScale;
				position = MainlandShoreData.Position;
			}
			NativeArray<Vector3> positions = new NativeArray<Vector3>(size * size, (Allocator)3, (NativeArrayOptions)1);
			NativeArray<float> val = new NativeArray<float>(size * size, (Allocator)3, (NativeArrayOptions)1);
			NativeArray<byte> bitmap = new NativeArray<byte>(size * size, (Allocator)3, (NativeArrayOptions)1);
			distances = new NativeArray<float>(size * size, (Allocator)4, (NativeArrayOptions)1);
			vectors = new NativeArray<Vector4>(size * size, (Allocator)4, (NativeArrayOptions)1);
			JobHandle val3;
			JobHandle val4;
			using (TimeWarning.New("WaterDepth"))
			{
				NativeArray<int> indices = new NativeArray<int>(size * size, (Allocator)3, (NativeArrayOptions)1);
				NativeArray<float> heights = new NativeArray<float>(size * size, (Allocator)3, (NativeArrayOptions)1);
				GenSamplePoints genSamplePoints = new GenSamplePoints
				{
					indices = indices,
					positions = positions,
					shoreMapSize = size,
					terrainPosition = position,
					upscaleCoords = shoreDistanceScale
				};
				IJobExtensions.RunByRef<GenSamplePoints>(ref genSamplePoints);
				JobHandle val2 = default;
				if (Object.op_Implicit((Object)(object)TerrainMeta.HeightMap) && TerrainMeta.HeightMap.isInitialized)
				{
					TerrainHeightMap heightMap = TerrainMeta.HeightMap;
					ReadOnly<Vector3> worldPos = positions.AsReadOnly();
					NativeArray<float> results = val;
					val3 = default;
					val2 = heightMap.GetHeights(worldPos, results, val3);
				}
				else
				{
					val2 = IJobExtensions.Schedule<FillJob<float>>(new FillJob<float>
					{
						Value = 0f,
						Values = val
					}, val2);
				}
				WaterLevel.GetWaterLevels(positions.AsReadOnly(), indices.AsReadOnly(), waves: false, heights);
				GenShoreVecBitMapJob jobData = new GenShoreVecBitMapJob
				{
					bitmap = bitmap,
					terrainHeights = val.AsReadOnly(),
					waterHeights = heights.AsReadOnly()
				};
				val4 = ParallelJobEx.ScheduleParallel<GenShoreVecBitMapJob>(ref jobData, bitmap.Length, val2);
				indices.Dispose(val4);
				heights.Dispose(val4);
			}
			using (TimeWarning.New("DistanceField.XXX"))
			{
				NativeReference<bool> hasLand = new NativeReference<bool>(AllocatorHandle.op_Implicit((Allocator)3), (NativeArrayOptions)1);
				val3 = IJobExtensions.Schedule<HasLandJob>(new HasLandJob
				{
					bitmap = bitmap.AsReadOnly(),
					hasLand = hasLand
				}, val4);
				val3.Complete();
				if (hasLand.Value)
				{
					JobHandle val5 = DistanceField.GenerateNative(in size, (byte)127, bitmap.AsReadOnly(), in distances, val4);
					val5 = DistanceField.ApplyGaussianBlurNative(size, distances, 1, val5);
					val5 = DistanceField.GenerateVectorsNative(in size, distances.AsReadOnly(), vectors, val5);
					bitmap.Dispose(val5);
					val5.Complete();
				}
				else
				{
					ShoreData shoreData = (genDeepSea ? DeepSeaShoreData : MainlandShoreData);
					IJobExtensions.Run<FillJob<float>>(new FillJob<float>
					{
						Value = shoreData.DefaultDistance,
						Values = distances
					});
					IJobExtensions.Run<FillJob<Vector4>>(new FillJob<Vector4>
					{
						Value = shoreData.DefaultVector,
						Values = vectors
					});
					bitmap.Dispose();
				}
				hasLand.Dispose();
			}
			using (TimeWarning.New("Topology Mask"))
			{
				if (!((Object)(object)TerrainMeta.TopologyMap != (Object)null) || !TerrainMeta.TopologyMap.isInitialized || !((Object)(object)TerrainMeta.HeightMap != (Object)null) || !TerrainMeta.HeightMap.isInitialized)
				{
					for (int i = 0; i < vectors.Length; i++)
					{
						Vector4 val6 = vectors[i];
						val6.w = -1f;
						vectors[i] = val6;
					}
					val3 = default;
					positions.Dispose(val3);
					val3 = default;
					val.Dispose(val3);
					return;
				}
				JobHandle val7 = default;
				if (genDeepSea)
				{
					FillAsOceanTopologyJob jobData2 = new FillAsOceanTopologyJob
					{
						vectors = vectors
					};
					val7 = ParallelJobEx.ScheduleParallel<FillAsOceanTopologyJob>(ref jobData2, vectors.Length, val7);
				}
				else
				{
					NativeArray<float> radii = new NativeArray<float>(size * size, (Allocator)3, (NativeArrayOptions)1);
					NativeArray<int> results2 = new NativeArray<int>(size * size, (Allocator)3, (NativeArrayOptions)1);
					GenTopologyRadiiJob jobData3 = new GenTopologyRadiiJob
					{
						heights = val.AsReadOnly(),
						radii = radii
					};
					val7 = ParallelJobEx.ScheduleParallel<GenTopologyRadiiJob>(ref jobData3, radii.Length, val7);
					val7 = TerrainMeta.TopologyMap.GetTopologiesIndirect(positions.AsReadOnly(), radii.AsReadOnly(), results2, val7);
					ProcessTopologyJob jobData4 = new ProcessTopologyJob
					{
						topologies = results2.AsReadOnly(),
						vectors = vectors
					};
					val7 = ParallelJobEx.ScheduleParallel<ProcessTopologyJob>(ref jobData4, vectors.Length, val7);
					radii.Dispose(val7);
					results2.Dispose(val7);
				}
				positions.Dispose(val7);
				val.Dispose(val7);
				val7.Complete();
			}
		}
	}

	public float GetCoarseDistanceToShore(Vector3 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return GetMap(pos).GetCoarseDistanceToShore(pos);
	}

	public (Vector3 shoreDir, float shoreDist) GetCoarseVectorToShore(Vector3 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return GetMap(pos).GetCoarseVectorToShore(pos);
	}

	public (Vector3 shoreDir, float shoreDist) GetMainlandCoarseVectorToShore(float normX, float normY)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return MainlandShoreData.GetCoarseVectorToShore(new Vector2(normX, normY));
	}

	public Vector4 GetRawShoreVector(Vector3 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return GetMap(pos).GetRawShoreVector(pos);
	}

	public ShoreVectorQueryStructure GetShoreVectorQueryStructure()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new ShoreVectorQueryStructure(MainlandShoreData, DeepSeaShoreData, DeepSeaManager.DeepSeaBounds);
	}

	public void GetCoarseDistancesToShoreIndirect(ReadOnly<Vector3> positions, ReadOnly<int> indices, NativeArray<float> results)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		GetCoarseDistsToShoreJobIndirect getCoarseDistsToShoreJobIndirect = new GetCoarseDistsToShoreJobIndirect
		{
			Dists = results,
			Positions = positions,
			Indices = indices,
			QueryStructure = GetShoreVectorQueryStructure()
		};
		IJobExtensions.RunByRef<GetCoarseDistsToShoreJobIndirect>(ref getCoarseDistsToShoreJobIndirect);
	}

	public void ApplyBakedDeepSeaVectors(BakedShoreVectors bakedShoreVectors, Transform t)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ApplyBakedDeepSeaVectors"))
		{
			if (bakedShoreVectors.ShoreVectorData == null || bakedShoreVectors.ShoreVectorData.Distances == null)
			{
				return;
			}
			ShoreVectorData shoreVectorData = bakedShoreVectors.ShoreVectorData;
			Vector3 val = default;
			Quaternion val2 = default;
			t.GetPositionAndRotation(ref val, ref val2);
			if (bakedShoreVectors.OnlyBakeShoreVectors)
			{
				deepSeaPostGenApplication.Add((bakedShoreVectors, t));
			}
			float y = val2.eulerAngles.y;
			Bounds deepSeaBounds = DeepSeaManager.DeepSeaBounds;
			Vector3 min = deepSeaBounds.min;
			Vector3 val3 = Vector3Ex.Inverse(deepSeaBounds.size);
			float normX = (val.x - min.x) * val3.x;
			float normZ = (val.z - min.z) * val3.z;
			float worldSize = shoreVectorData.WorldSize;
			short[] srcHeightData = shoreVectorData.HeightData;
			short[] array = srcHeightData;
			if (array != null && array.Length != 0)
			{
				float srcPositionY = shoreVectorData.HeightInfo.x;
				float srcSizeY = shoreVectorData.HeightInfo.y;
				BlitBakedData(worldSize, shoreVectorData.HeightDimension, deepSeaBounds, TerrainMeta.HeightMap.res, normX, normZ, y, (int si, int di) =>
				{
					float num = BitUtility.Short2Float((int)srcHeightData[si]);
					short num2 = BitUtility.Float2Short(TerrainMeta.NormalizeY(srcPositionY + num * srcSizeY));
					short num3 = TerrainMeta.HeightMap.deepSeaHeights[di];
					if (num2 > num3)
					{
						TerrainMeta.HeightMap.deepSeaHeights[di] = num2;
					}
				});
			}
			deepSeaShoreDataDirty = true;
		}
	}

	private static void BlitBakedData(float worldSize, int dimension, Bounds deepSeaBounds, int dstMapSize, float normX, float normZ, float yaw, Action<int, int> action)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BlitBakedData"))
		{
			float scaleMod = worldSize / (float)dimension / (Vector3Ex.Max(Vector3Ex.XZ2D(deepSeaBounds.size)) / (float)dstMapSize);
			Vector2 destCenterPx = new Vector2(normX * (float)dstMapSize, normZ * (float)dstMapSize);
			Vector2 val = new Vector2((float)dimension * scaleMod, (float)dimension * scaleMod);
			float num = yaw * (MathF.PI / 180f);
			float cosA = Mathf.Cos(num);
			float sinA = Mathf.Sin(num);
			Vector2 val2 = val * 0.5f;
			float num2 = Mathf.Abs(cosA);
			float num3 = Mathf.Abs(sinA);
			float num4 = num2 * val2.x + num3 * val2.y;
			float num5 = num3 * val2.x + num2 * val2.y;
			int left = Mathf.FloorToInt(destCenterPx.x - num4);
			int right = Mathf.CeilToInt(destCenterPx.x + num4);
			int num6 = Mathf.FloorToInt(destCenterPx.y - num5);
			int num7 = Mathf.CeilToInt(destCenterPx.y + num5);
			if (right < 0 || left >= dstMapSize || num7 < 0 || num6 >= dstMapSize)
			{
				return;
			}
			left = Mathf.Clamp(left, 0, dstMapSize - 1);
			right = Mathf.Clamp(right, 0, dstMapSize - 1);
			num6 = Mathf.Clamp(num6, 0, dstMapSize - 1);
			num7 = Mathf.Clamp(num7, 0, dstMapSize - 1);
			Vector2 srcPivotPx = new Vector2((float)dimension * 0.5f, (float)dimension * 0.5f);
			Parallel.For(num6, num7 + 1, (int z) =>
			{
				for (int i = left; i <= right; i++)
				{
					float num8 = (float)i + 0.5f - destCenterPx.x;
					float num9 = (float)z + 0.5f - destCenterPx.y;
					float num10 = (cosA * num8 - sinA * num9) / scaleMod;
					float num11 = (sinA * num8 + cosA * num9) / scaleMod;
					int num12 = (int)(srcPivotPx.x + num10);
					int num13 = (int)(srcPivotPx.y + num11);
					if (num12 >= 0 && num12 <= dimension - 1 && num13 >= 0 && num13 <= dimension - 1)
					{
						int arg = z * dstMapSize + i;
						int arg2 = num13 * dimension + num12;
						action(arg2, arg);
					}
				}
			});
		}
	}

	public void ClearDeepSeaData()
	{
		DeepSeaShoreData.FillWithDefault();
		deepSeaShoreDataDirty = true;
	}

	private void InitializeWaterHeight()
	{
	}

	private void ReleaseWaterHeight()
	{
	}

	private void UpdateWaterHeight()
	{
	}

	private void CheckInstance()
	{
		instance = (((Object)(object)instance != (Object)null) ? instance : this);
	}

	private void Awake()
	{
		CheckInstance();
	}

	public override void Setup()
	{
		CheckInstance();
		InitializeShoreVector();
	}

	public override void PostSetup()
	{
		TerrainMeta component = ((Component)this).GetComponent<TerrainMeta>();
		if ((Object)(object)component == (Object)null || component.config == null)
		{
			Debug.LogError((object)"[TerrainTexturing] Missing TerrainMeta or TerrainConfig not assigned.");
			return;
		}
		Shutdown();
		InitializeCoarseHeightSlope();
		GenerateShoreVector();
		InitializeWaterHeight();
		initialized = true;
	}

	private void Shutdown()
	{
		ReleaseBasePyramid();
		ReleaseCoarseHeightSlope();
		ReleaseShoreVector();
		ReleaseWaterHeight();
		initialized = false;
	}

	public void OnEnable()
	{
		CheckInstance();
	}

	private void OnDisable()
	{
		if (!Application.isQuitting)
		{
			Shutdown();
		}
	}

	private void Update()
	{
		if (initialized)
		{
			UpdateBasePyramid();
			UpdateCoarseHeightSlope();
			UpdateWaterHeight();
			UpdateDeepSeaShoreVectorTexture();
		}
	}
}
